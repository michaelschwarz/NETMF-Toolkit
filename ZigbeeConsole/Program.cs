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
		public static object ConsoleLock = new object();

        static void Main(string[] args)
        {
			Thread thd1 = new Thread(new ThreadStart(RunCoordinator));
			thd1.Start();

			//Thread thd2 = new Thread(new ThreadStart(RunDevice));
			//thd2.Start();

			Console.ReadLine();
        }

		static void RunDevice()
		{
			using (XBee xbee = new XBee("COM5", 9600))
			{
				xbee.OnPacketReceived += new XBee.PacketReceivedHandler(xbeedevice_OnPacketReceived);
				xbee.Open();

				xbee.SendPacket(new NodeIdentifier().GetPacket());
				//xbee.SendPacket(new NodeIdentifier("XBEE_DEVICE").GetPacket());
				//xbee.SendPacket(new SupplyVoltage().GetPacket());
				//xbee.SendPacket(new Channel().GetPacket());
				//xbee.SendPacket(new ReceivedSignalStrength().GetPacket());

				Thread.Sleep(10 * 1000);

				while (true)
				{
					//xbee.SendPacket(new NodeDiscover().GetPacket());
					Thread.Sleep(100);
				}
			}
		}

		static void RunCoordinator()
		{
			using (XBee xbee = new XBee("COM4", 9600))
			{
				xbee.OnPacketReceived += new XBee.PacketReceivedHandler(xbeecoord_OnPacketReceived);
				xbee.Open();

				//xbee.SendPacket(new InterfaceDataRate(115200).GetPacket());
				xbee.SendPacket(new NodeIdentifier().GetPacket());
				//xbee.SendPacket(new NodeIdentifier("XBEE_COORDINATOR").GetPacket());
				//xbee.SendPacket(new SupplyVoltage().GetPacket());
				//xbee.SendPacket(new Channel().GetPacket());
				//xbee.SendPacket(new ReceivedSignalStrength().GetPacket());

				Thread.Sleep(10 * 1000);

				while (true)
				{
					lock (ConsoleLock)
					{
						Console.WriteLine("COORDINATOR Sending NodeDiscover command...");
					}

					// discovering the network
					//AtCommand at = new AtCommand("ND", new byte[0], 1);
					xbee.SendPacket(new NodeDiscover().GetPacket());
					
					Thread.Sleep(20 * 1000);
				}
			}
		}

		static void xbeecoord_OnPacketReceived(XBee sender, XBeeResponse response)
		{
			if (!Monitor.TryEnter(ConsoleLock, 1000))
				return;

			try
			{
				Console.WriteLine("COORDINATOR OnPacketReceived [" + response.GetType().FullName + "]\r\n" + response);

				AtCommandResponse at = response as AtCommandResponse;

				if (at != null)
				{
					NodeDiscoverData ni = at.Data as NodeDiscoverData;
					if (ni != null)
					{
						Console.WriteLine(ni.NodeIdentifier + "#");

						if (ni.NodeIdentifier == "XBEE_SENSOR")
						{
							XBeeSensorSample sample = new XBeeSensorSample();
							AtRemoteCommand rcmd = new AtRemoteCommand(ni.Address16, ni.Address64, 0x00, sample, 0x02);
							sender.SendPacket(rcmd.GetPacket());
							Console.WriteLine("Sending ForceSample command...");
						}

						if (ni.NodeIdentifier == "XBEE_DEVICE")
						{
							ZigBeeTransmitRequest send = new ZigBeeTransmitRequest(0x01, ni.Address16, ni.Address64, Encoding.UTF8.GetBytes("" + DateTime.Now.Ticks));
							sender.SendPacket(send.GetPacket());
							Console.WriteLine("Sending ZigBeeTransmitRequest...");
						}
					}
				}

				Console.WriteLine("============================================================");
			}
			finally
			{
				Monitor.Exit(ConsoleLock);
			}
		}

		static void xbeedevice_OnPacketReceived(XBee sender, XBeeResponse response)
		{
			if (!Monitor.TryEnter(ConsoleLock, 1000))
				return;

			try
			{
				Console.WriteLine("DEVICE OnPacketReceived [" + response.GetType().FullName + "]\r\n" + response);

				AtCommandResponse at = response as AtCommandResponse;

				if (at != null)
				{
					NodeDiscoverData ni = at.Data as NodeDiscoverData;
					if (ni != null)
					{
						// Set node identifier to something else
						//Console.WriteLine("Testing Remote Command...");
						//AtCommand cmd = new NodeIdentifier("HELLOCLIENT");
						//AtRemoteCommand atr = new AtRemoteCommand(ni.Address16, ni.Address64, 0x02, cmd, 1);
						//sender.SendPacket(atr.GetPacket());
					}
				}

				ZigBeeReceivePacket zigp = response as ZigBeeReceivePacket;
				if (zigp != null)
				{
					DateTime now = DateTime.Now;
					string txt = Encoding.UTF8.GetString(zigp.RFData);
					long ticks = long.Parse(txt);
					DateTime sent = new DateTime(ticks);

					Console.WriteLine((now - sent).TotalMilliseconds + " msec");
				}

				Console.WriteLine("============================================================");
			}
			finally
			{
				Monitor.Exit(ConsoleLock);
			}
		}
    }
}
