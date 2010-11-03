/* 
 * Program.cs
 * 
 * Copyright (c) 2009, Elze Kool (http://www.microframework.nl)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Threading;

using Microsoft.SPOT;

using MFToolkit.MicroGM862;
using MFToolkit.MicroGM862.Modules;

using Microsoft.SPOT.Hardware;

namespace MFToolkit.MicroGM862Demo
{
    public class Program
    {
        public static void Main()
        {

            #region Thread to simulate working application

            // Simulate a thread that does some heavy work with random spikes and dips
            // so it could interfere with the processing of GM862 responses
            Thread HeavyWork = new Thread(new ThreadStart(delegate()
            {
                Random rand = new Random();
                long testOperator = 0;

                while (true)
                {
                    do
                    {
                        for(int x = 0; x < 1000; x++)
                            testOperator = rand.Next(1100);

                    } while (testOperator > 1000);

                    Thread.Sleep(10);
                }
            }));

            HeavyWork.Priority = ThreadPriority.AboveNormal;
            HeavyWork.Start();

            #endregion

            #region Reset procedure for GM862

            Debug.Print("Resetting GM862");

            // Reset GM862 Device
            OutputPort ResetPin = new OutputPort((Cpu.Pin) 44, false);
            System.Threading.Thread.Sleep(100);
            ResetPin.Write(true);

            Debug.Print("Turn On GM862");

            // Wait a while for GM862 to reset
            System.Threading.Thread.Sleep(1000);

            // Turn on GM862
            OutputPort OnOffPin = new OutputPort((Cpu.Pin)45, true);
            System.Threading.Thread.Sleep(500);
            OnOffPin.Write(false);

            // Wait a while for GM862 to turn onn
            System.Threading.Thread.Sleep(1000);

            #endregion

            // Counter to count how hard we have been working
            long largeCounter = 0;            

            // Create new GM862 Device
            GM862GPS GM862 = new GM862GPS(new AT_Interface("COM1", 115200));

            // used for sending commands
            String ResponseBody = "";


            #region GSM Setup

            // Initialize gsm function
            lock (GM862)
            {
                // Select network band
                if (!GM862.GSM.SelectNetworkBand(GSM.NetworkBands.GSM900_DCS1800))
                    throw new Exception("Failed to select GSM Network Band");

                // First create function to activate phone
                GM862.GSM.OnPinRequest = new GSM.PinRequestHandler(delegate(String PINType)
                {
                    if (PINType == "SIM PIN")
                        return "0000";

                    if (PINType == "SIM PUK")
                        return "05969374, 0000";

                    throw new Exception("Unkown PIN Code");
                });

                // Event handler for recieving calls
                GM862.GSM.OnRecievingCall = new GSM.RecievingCallHandler(delegate()
                {
                    Debug.Print("You are beeing called!");
                    Thread.Sleep(500);
                });

                // Allow roaming
                GM862.GSM.AllowRoaming = true;

                // Now try to initialize gsm
                GM862.GSM.Initialize();
            }

            #endregion

            #region GPRS Setup

            // Initialize GPRS function
            lock (GM862)
            {
                // Allow roaming
                GM862.GPRS.AllowRoaming = true;

                // Now try to initialize GPRS
                GM862.GPRS.Initialize();
            }

            #endregion

            #region Text Messaging Setup

            // Initialize text messaging function
            lock (GM862)
            {

                GM862.TextMessaging.OnRecievedTextMessage = new TextMessaging.RecievedTextMessageHandler(delegate(String Memory, int Location)
                {
                    TextMessaging.TextMessage TextMessage;

                    Debug.Print("Recieved Text Message!");

                    // Read text message from indicated storage
                    if (GM862.TextMessaging.ReadTextMessage(Memory, Location, out TextMessage))
                    {
                        Debug.Print("From: " + TextMessage.Orginator);
                        Debug.Print("Message: " + TextMessage.Message);

                        if (Memory.IndexOf("SM") != -1)
                            Debug.Print("Deleting message: " + (GM862.TextMessaging.DeleteMessage("SM", TextMessage.Location) ? "OK" : "ERROR"));

                        //Debug.Print("Sending it back");
                        //GM862.TextMessaging.SendTextMessage(TextMessage.Orginator, TextMessage.Message);
                    }
                    

                });

                // Now try to initialize text messaging
                GM862.TextMessaging.Initialize();

            }

            #endregion

            #region GPS Setup

            // Enable GPS Functions
            lock (GM862)
            {
                // Initialize GPS Functions
                GM862.GPS.Initialize();
            }

            #endregion

            #region Extra functions

            // Enable status output to get some garbage in the output
            lock (GM862)
            {
                if (GM862.ExecuteCommand("AT+CMER=1, 0, 0, 2, 0", out ResponseBody, 1500) != AT_Interface.ResponseCodes.OK)
                {
                    Debug.Print("ERROR +CMER: " + ResponseBody);
                    System.Threading.Thread.Sleep(-1);
                }
            }

            // Also unsolicitated GPS output for even more garbage
            /* lock (GM862)
            {
                if (GM862.ExecuteCommand("AT$GPSNMUN=1, 0, 0, 0, 0, 1, 1", out ResponseBody, 1500) != AT_Interface.ResponseCodes.OK)
                {
                    Debug.Print("ERROR $GPSNMUN: " + ResponseBody);
                    System.Threading.Thread.Sleep(-1);
                }
            } //*/

            #endregion

            // Wait until we have initial GSM/GPRS ready
            Debug.Print("Waiting until GSM/GPRS ready");
            while (true)
            {
                if (!GM862.GSM.RegistratedOnGSMNetwork()) continue;
                if (!GM862.GPRS.RegistratedOnGPRSNetwork()) continue;
                break;
            }
            Debug.Print("Ok");

            // Read list of messages from SIM and delete them to make room 
            Debug.Print("Read list of messages from SIM and delete them to make room");
            TextMessaging.TextMessage[] Messages;
            if (GM862.TextMessaging.ListTextMessages("SM", TextMessaging.ListGroups.ALL, out Messages) > 0)
                foreach (TextMessaging.TextMessage Message in Messages)
                    Debug.Print("Deleting message #" + Message.Location + ": " + (GM862.TextMessaging.DeleteMessage("SM", Message.Location) ? "OK" : "ERROR"));
            Debug.Print("Ok");

            // Setup first GPRS Context
            Debug.Print("Setup first GPRS Context");
            if (!GM862.GPRS.SetContextConfiguration(1, "live.vodafone.com", "vodafone", "vodafone", "0.0.0.0"))
                throw new Exception("Failed to activate GPRS");
            Debug.Print("Ok");

            // Activate first GPRS Context
            Debug.Print("Activate first GPRS Context");
            if (!GM862.GPRS.ActivateContext(1))
                throw new Exception("Failed to activate GPRS");
            Debug.Print("Ok");


            // Setup Socket Configuration
            Debug.Print("Setup Socket Configuration");
            if (!GM862.GPRS.SetSocketConfig(1, 1, 0, 90, 600, 50))
                throw new Exception("Failed to setup socket 1/2");

            if (!GM862.GPRS.SetSocketExtendedConfig(1, 0, 0, 0))
                throw new Exception("Failed to setup socket 2/2");
            Debug.Print("Ok");


            // Parameters used when retrieving GPS info
            byte gpsFix;
            byte gpsNoSat;
            double gpsLat;
            double gpsLon;
            double gpsSpeed;
            DateTime gpsDateTime;


            // Heavy testing ;-)
            while (true)
            {
                lock (GM862)
                {
                    if (!GM862.GSM.RegistratedOnGSMNetwork()) Debug.Print("GSM NOT READY");
                    if (!GM862.GPRS.RegistratedOnGPRSNetwork()) Debug.Print("GPRS NOT READY");

                    GM862.GPS.ReadGPSData(out gpsFix, out gpsNoSat, out gpsLat, out gpsLon, out gpsSpeed, out gpsDateTime);
                    Debug.Print((gpsFix > 0) ? "GPS Fix ;-)" : "NO GPS FIX");
                }

                lock (GM862)
                {
                    if ((largeCounter % 21) == 20)
                    {
                        byte[] response;
                        Debug.Print("Fetching sample page from the Web");
                        if (GM862.Networking.WebRequest(1, "http://joomlacluster.edu-actief.nl/", String.Empty, out response, "GET", String.Empty, String.Empty))
                        {
                            Debug.Print(new string(System.Text.Encoding.UTF8.GetChars(response)));
                        }
                        else
                        {
                            Debug.Print("Failed");
                        }
                    }
                }

                largeCounter++;
                Debug.Print(largeCounter.ToString());

                lock (GM862)
                {
                    if ((largeCounter % 5) == 0)
                        GM862.HandleUnsolicitatedResponses();
                }
            }
        }

    }
}
