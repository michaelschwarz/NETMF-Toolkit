/* 
 * TextMessaging.cs
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
using MFToolkit.MicroUtilities;

namespace MFToolkit.MicroGM862.Modules
{
    public class TextMessaging
    {

        #region Public classes and enums

        /// <summary>
        /// Representation of a Text Message as it is stored on the GM862
        /// </summary>
        public class TextMessage
        {
            public readonly String Memory;
            public readonly int Location;
            public readonly String Status;
            public readonly String Orginator;
            public readonly String ArrivalTime;
            public readonly String Message;

            /// <summary>
            /// Instantiate a new Text Message 
            /// </summary>
            public TextMessage(String Memory, int Location, String Status, String Orginator, String ArrivalTime, String Message)
            {
                this.Memory = Memory;
                this.Location = Location;
                this.Status = Status;
                this.Orginator = Orginator;
                this.ArrivalTime = ArrivalTime;
                this.Message = Message;
            }
        }

        /// <summary>
        /// Used for specifing the kind of messages listed by ListTextMessages
        /// </summary>
        public enum ListGroups
        {
            /// <summary>
            /// Recieved, Unread Messages
            /// </summary>
            REC_UNREAD,

            /// <summary>
            /// Recieved, Read Messages
            /// </summary>
            REC_READ,

            /// <summary>
            /// Stored, Not yet send messages
            /// </summary>
            STO_UNSEND,

            /// <summary>
            /// Stored, And send messages
            /// </summary>
            STO_SEND,

            /// <summary>
            /// All Messages
            /// </summary>
            ALL
        }

        /// <summary>
        /// Handler for OnRecievedMessage indicating reception of a new Text Message
        /// </summary>
        /// <param name="Memory">Memory of recieved message</param>
        /// <param name="Location">Location of recieved message</param>
        public delegate void RecievedTextMessageHandler(String Memory, int Location);

        #endregion

        #region Private properties and functions

        // Device this module is associated with
        private GM862GPS _device;

        // Indicator if text messaging is initialized
        private bool _textmessage_initialized = false;

        // Event handler for unsolicitated responses, check for text message events
        private void _textmessage_unsolicitated(String Response)
        {
            // Check if text message recieved event
            if (Response.IndexOf("+CMTI: ") == 0)
            {
                // Explode on comma to get message location
                string[] MessageLocation = Response.Substring(7).Split(',');

                // Check if handler is set if so call it
                if (OnRecievedTextMessage != null) OnRecievedTextMessage(MessageLocation[0], NumberParser.StringToInt(MessageLocation[1]));
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// True if Text Messaging is succesfully initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _textmessage_initialized;
            }
        }

        /// <summary>
        /// This function is called when a text message has arrived
        /// </summary>
        public RecievedTextMessageHandler OnRecievedTextMessage;

        #endregion

        #region Constructor / Destructror

        /// <summary>
        /// Create new TextMessaging Module
        /// </summary>
        /// <remarks>The GM862 Driver creates an instance. Don't create a new one!</remarks>
        /// <param name="Device">Device driver this module associated with</param>
        public TextMessaging(GM862GPS Device)
        {
            this._device = Device;
        }

        #endregion

        /// <summary>
        /// Initialize Text Messaging Support
        /// </summary>
        public void Initialize()
        {
            // String used to store response body when executing functions
            string responseBody;

            // Add event handler for unsolicitated responses to check for text message events
            _device.OnUnsolicitatedResponse += new GM862GPS.UnsolicitatedResponseHandler(_textmessage_unsolicitated);

            // Select standard SMS instruction set
            if (_device.ExecuteCommand("AT#SMSMODE=0", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Select Unsolicited SMS code to be buffered and in form +CMTI: <mem>, <id>
            if (_device.ExecuteCommand("AT+CNMI=2,1,0,0,0", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Select Text Message Format
            if (_device.ExecuteCommand("AT+CMGF=1", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Set Show Text Mode Parameters to show extended information
            if (_device.ExecuteCommand("AT+CSDH=1", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // We have initialized the text message functionality succesfull
            _textmessage_initialized = true;
        }

        /// <summary>
        /// Read Text Message from memory
        /// </summary>
        /// <param name="Memory">"ME" for GM862 storage, "SM" for SIM storage</param>
        /// <param name="Location">Location number</param>
        /// <param name="Message">Message</param>
        /// <returns>True when message is read succesfully</returns>
        public bool ReadTextMessage(String Memory, int Location, out TextMessage Message)
        {
            // Check if Text Messaging was initialized correctly
            if (!_textmessage_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check Memory, must be "ME" or "SM"
            if (Memory.ToUpper() == "ME") Memory = "\"ME\"";
            if (Memory.ToUpper() == "SM") Memory = "\"SM\"";

            // String used to store response body when executing functions
            string responseBody;

            // Select memory location
            if (_device.ExecuteCommand("AT+CPMS=" + Memory, out responseBody, 10000) != AT_Interface.ResponseCodes.OK)
            {
                // Error
                Message = null;
                return false;
            }

            // Read Message from Location
            if (_device.ExecuteCommand("AT+CMGR=" + Location.ToString(), out responseBody, 10000) != AT_Interface.ResponseCodes.OK)
            {
                // Error
                Message = null;
                return false;
            }

            // Check response
            if ((responseBody.IndexOf("\r\n+CMGR: ") != 0) | (responseBody.LastIndexOf("\r\n") < 2))
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Variables used to read message text
            int messageStart = 0;

            // Variables used to decode message header
            System.Collections.ArrayList header = new System.Collections.ArrayList();
            String headerPart = "";
            bool withinQuote = false;
            bool ignoreRest = false;
            bool exitLoop = false;

            // Try reading header parts
            foreach (char c in responseBody.Substring(9))
            {
                exitLoop = false;
                switch (c)
                {
                    case '"':
                        if (withinQuote) { withinQuote = false; ignoreRest = true; break; }
                        if (!withinQuote) { headerPart = ""; withinQuote = true; break; }
                        break;
                    case ',':
                        if (!withinQuote) { ignoreRest = false; header.Add(headerPart); headerPart = ""; break; }
                        break;
                    case '\r':
                        header.Add(headerPart); headerPart = "";
                        exitLoop = true;
                        break;
                    default:
                        if (!ignoreRest) headerPart += c;
                        break;
                }
                if (exitLoop) break;
            }

            // Header should now contain:
            // [0] Status
            // [1] Originator
            // [2] 
            // [3] Arrival Time
            // [Length-1] Data length

            // Exit on wrong header
            if (header.Count < 7)
            {
                Message = null;
                return false;
            }

            // Get Message Start
            messageStart = responseBody.IndexOf("\r\n", 9) + 2;

            // Build new TextMessage
            Message = new TextMessage(
                Memory,
                Location,
                (String)header[0],
                (String)header[1],
                (String)header[3],
                responseBody.Substring(messageStart, NumberParser.StringToInt((String)header[header.Count-1]))
                );

            // Return succes
            return true; 

        }

        /// <summary>
        /// Deletes text message from memory
        /// </summary>
        /// <param name="Memory">Memory to delete message from</param>
        /// <param name="Location">Location to delete message for</param>
        /// <returns>True on succes</returns>
        public bool DeleteMessage(String Memory, int Location)
        {
            // Check if Text Messaging was initialized correctly
            if (!_textmessage_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check Memory, must be "ME" or "SM"
            if (Memory.ToUpper() == "ME") Memory = "\"ME\"";
            if (Memory.ToUpper() == "SM") Memory = "\"SM\"";

            // String used to store response body when executing functions
            string responseBody;

            // Select memory location
            if (_device.ExecuteCommand("AT+CPMS=" + Memory, out responseBody, 60000) != AT_Interface.ResponseCodes.OK)
                return false;

            // Delete message
            if (_device.ExecuteCommand("AT+CMGD=" + Location.ToString() + ", 0", out responseBody, 60000) != AT_Interface.ResponseCodes.OK)
                return false;

            // Succes
            return true;
        }

        /// <summary>
        /// Send Text Message
        /// </summary>
        /// <param name="Destination">Destination for SMS</param>
        /// <param name="Message">Message to Send</param>
        public void SendTextMessage(String Destination, String Message)
        {
            // Check if Text Messaging was initialized correctly
            if (!_textmessage_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // String used to store response body when executing functions
            string responseBody;

            // Add format identification to destination
            if (Destination.IndexOf('+') != -1)
                Destination = "\"" + Destination + "\",157"; // Internation format
            else
                Destination = "\"" + Destination + "\",129"; // National format

            // Say we want to send message
            if (_device.ExecuteCommand("AT+CMGS=" + Destination, out responseBody, 15000) != AT_Interface.ResponseCodes.SEND_SMS_DATA)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Wait a little before sending the message
            System.Threading.Thread.Sleep(20);

            // Send Message, End with ^Z
            byte[] OutBuffer = System.Text.Encoding.UTF8.GetBytes(Message + "\x1A");
            _device.SendRawData(OutBuffer, 0, OutBuffer.Length);

            // Wait for response
            if (_device.WaitForResponse(out responseBody, 15000) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);
        }

        /// <summary>
        /// List all text messages from a memory location
        /// </summary>
        /// <param name="Memory">Memory location to read from</param>
        /// <param name="Group">Group of messages to read</param>
        /// <param name="Messages">Array of read messages</param>
        /// <returns>Number of read messages</returns>
        public int ListTextMessages(String Memory, ListGroups Group, out TextMessage[] Messages)
        {
            // Check if Text Messaging was initialized correctly
            if (!_textmessage_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check Memory, must be "ME" or "SM"
            if (Memory.ToUpper() == "ME") Memory = "\"ME\"";
            if (Memory.ToUpper() == "SM") Memory = "\"SM\"";

            // String used to store response body when executing functions
            string responseBody;

            // Get group as string
            string groupString;

            // Get group as string
            switch (Group)
            {
                case ListGroups.ALL:
                    groupString = "ALL";
                    break;

                case ListGroups.REC_READ:
                    groupString = "REC READ";
                    break;

                case ListGroups.REC_UNREAD:
                    groupString = "REC UNREAD";
                    break;

                case ListGroups.STO_SEND:
                    groupString = "STO SEND";
                    break;

                case ListGroups.STO_UNSEND:
                    groupString = "STO UNSEND";
                    break;

                default:
                    groupString = "ALL";
                    break;
            }

            // ArrayList to store read messages 
            System.Collections.ArrayList readMessages = new System.Collections.ArrayList();

            // Select memory location
            if (_device.ExecuteCommand("AT+CPMS=" + Memory, out responseBody, 10000) != AT_Interface.ResponseCodes.OK)
            {
                // Error
                Messages = null;
                return 0;
            }

            // Read Message from Location
            if (_device.ExecuteCommand("AT+CMGL=\"" + groupString + "\"", out responseBody, 10000) != AT_Interface.ResponseCodes.OK)
            {
                // Error
                Messages = null;
                return 0;
            }

            // Variables used to read message text
            int messageStart = 0;

            // Variables used to decode message header
            System.Collections.ArrayList header = new System.Collections.ArrayList();
            String headerPart = "";
            bool withinQuote = false;
            bool ignoreRest = false;
            bool exitLoop = false;

            // Keep reading while we have valid listing entries
            while (responseBody.IndexOf("\r\n+CMGL: ") == 0)
            {
                // Start of clean
                header.Clear();
                headerPart = "";
                withinQuote = false;
                ignoreRest = false;

                // Try reading header parts
                foreach (char c in responseBody.Substring(9)) 
                {
                    exitLoop = false;
                    switch (c) 
                    {
                        case '"':
                            if (withinQuote) { withinQuote = false; ignoreRest = true; break; }
                            if (!withinQuote) { headerPart = ""; withinQuote = true;  break; }
                            break;
                        case ',':
                            if (!withinQuote) { ignoreRest = false; header.Add(headerPart); headerPart = ""; break; }
                            break;
                        case '\r':
                            header.Add(headerPart); headerPart = "";
                            exitLoop = true;
                            break;
                        default:
                            if (!ignoreRest) headerPart += c;
                            break;
                    }
                    if (exitLoop) break;
                }

                // Header should now contain:
                // [0] Index
                // [1] Status
                // [2] Originator
                // [3] Arrival Time
                // [4]
                // [5] Originator Type 
                // [6] Data length

                // Exit on wrong header
                if (header.Count < 7)
                    break;

                // Get Message Start
                messageStart = responseBody.IndexOf("\r\n", 9) + 2;

                // Build new TextMessage
                TextMessage newMessage = new TextMessage(
                    Memory,
                    NumberParser.StringToInt((String)header[0]),
                    (String)header[1],
                    (String)header[2],
                    (String)header[3],
                    responseBody.Substring(messageStart, NumberParser.StringToInt((String)header[6]))
                    );

                // And add it to the array
                readMessages.Add(newMessage);

                // Ok, Next Message
                responseBody = responseBody.Substring(messageStart + NumberParser.StringToInt((String)header[6]));
            }



            // Return read messages
            Messages = (TextMessage[]) readMessages.ToArray(typeof(TextMessage));
            return Messages.Length;
        }
    }
}
