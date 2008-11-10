/* 
 * Program.cs		(Demo Application)
 * 
 * Copyright (c) 2008, Michael Schwarz (http://www.schwarz-interactive.de)
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
using System.Collections.Generic;
using System.Text;
using MSchwarz.Net.Zigbee;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using MSchwarz.IO;

namespace ZigbeeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (XBee xbee = new XBee("COM4", 9600))
            {
				xbee.OnPacketReceived += new XBee.PacketReceivedHandler(xbee_OnPacketReceived);
                xbee.Open();

				// reading node identifier
				xbee.SendPacket(new NodeIdentifier().GetPacket());

				// setting node identifier to XBEECOORD
				//xbee.SendPacket(new NodeIdentifier("XBEECOORD").GetPacket());

                while (true)
                {
					// discovering the network
					//AtCommand at = new AtCommand("ND", new byte[0], 1);
                    xbee.SendPacket(new NodeDiscover().GetPacket());

					Console.WriteLine("Waiting for response...");

					Thread.Sleep(20 * 60 * 1000);
            
					//AtRemoteCommand rat = new AtRemoteCommand(5526146519841232, 11214, 0x02, "IS", new byte[0], 3);
					//xbee.SendPacket(rat.GetPacket());

					//Thread.Sleep(5*1000);
                }
            }
        }

		static void xbee_OnPacketReceived(XBee sender, XBeeResponse response)
		{
			Console.WriteLine("OnPacketReceived " + response);

			AtCommandResponse at = response as AtCommandResponse;

			if (at != null)
			{
				Console.WriteLine(at.Command);

				if (at.Data != null)
					Console.WriteLine(at.Data.ToString());
				else
					Console.WriteLine("no data");

				NodeDiscoverResponseData ni = at.Data as NodeDiscoverResponseData;

				if (ni != null)
				{
					// Set node identifier to something else
					Console.WriteLine("Testing Remote Command...");
					AtCommand cmd = new NodeIdentifier("HELLOCLIENT");
					AtRemoteCommand atr = new AtRemoteCommand(ni.Address16, ni.Address64, 0x02, cmd, 1);
					sender.SendPacket(atr.GetPacket());
				}
			}
		}
    }
}
