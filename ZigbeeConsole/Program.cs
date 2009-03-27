using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using MFToolkit.IO;
using MFToolkit.Net.XBee;

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
			using (XBee xbee = new XBee("COM4", 9600, ApiType.Disabled))
			{
                xbee.FrameReceived += new FrameReceivedEventHandler(xbeedevice_OnPacketReceived);
				xbee.Open();

				
				while (true)
				{
					xbee.ExecuteNonQuery(new NodeDiscover());
					Thread.Sleep(100);
				}
			}
		}

		static void RunCoordinator()
		{
			using (XBee xbee = new XBee("COM4", 9600, ApiType.Enabled))
			{
                xbee.FrameReceived += new FrameReceivedEventHandler(xbeecoord_OnPacketReceived);
				xbee.Open();

                while (true)
				{
					// discovering the network
                    AtCommand at = new NodeDiscover();
                    xbee.Execute(at);
                    Thread.Sleep(60 * 1000);
				}

                xbee.StopReceiveData();
                Console.WriteLine("stopped receiving thread");
			}
		}

		static void xbeecoord_OnPacketReceived(object sender, FrameReceivedEventArgs e)
		{
            XBeeResponse response = e.Response;

			if (!Monitor.TryEnter(ConsoleLock, 1000))
				return;

			try
			{
				Console.WriteLine("COORDINATOR OnPacketReceived [" + response.GetType().FullName + "]\r\n" + response);

				AtCommandResponse at = response as AtCommandResponse;

				if (at != null)
				{
					NodeDiscoverData ni = at.ParseValue() as NodeDiscoverData;
					if (ni != null)
					{
						if (ni.NodeIdentifier == "XBEE_SENSOR" || ni.NodeIdentifier == "DEVICE2" || ni.NodeIdentifier == "XBEESENSOR")
						{
                            //XBeeSensorSample sample = new XBeeSensorSample();
                            //ForceSample sample = new ForceSample();
							//NodeIdentifier sample = new NodeIdentifier();
							//SupplyVoltage sample = new SupplyVoltage();
                            
                            //AtRemoteCommand rcmd = new AtRemoteCommand(ni.SerialNumber, sample);
                            //sender.Execute(rcmd);

                            ZigBeeTransmitRequest x = new ZigBeeTransmitRequest(ni.SerialNumber, ni.ShortAddress, Encoding.ASCII.GetBytes("Hallo"));
                            (sender as XBee).Execute(x);

							Console.WriteLine("Sending ForceSample command...");
						}

						if (ni.NodeIdentifier == "XBEEDEVICE")
						{
							ZigBeeTransmitRequest send = new ZigBeeTransmitRequest(ni.SerialNumber, ni.ShortAddress, Encoding.UTF8.GetBytes("" + DateTime.Now.Ticks));
                            (sender as XBee).Execute(send);
							Console.WriteLine("Sending ZigBeeTransmitRequest...");
						}
					}
				}

				ZigBeeReceivePacket zr = response as ZigBeeReceivePacket;

				if (zr != null)
				{
					Console.WriteLine("ZigBee: " + Encoding.UTF8.GetString(zr.Value));
				}

				Console.WriteLine("============================================================");
			}
			finally
			{
				Monitor.Exit(ConsoleLock);
			}
		}

		static void xbeedevice_OnPacketReceived(object sender, FrameReceivedEventArgs e)
		{
            XBeeResponse response = e.Response;

			if (!Monitor.TryEnter(ConsoleLock, 1000))
				return;

			try
			{
				Console.WriteLine("DEVICE OnPacketReceived [" + response.GetType().FullName + "]\r\n" + response);

				AtCommandResponse at = response as AtCommandResponse;

				if (at != null)
				{
					NodeDiscoverData ni = at.ParseValue() as NodeDiscoverData;
					if (ni != null)
					{
                        // ...
					}
				}

				ZigBeeReceivePacket zigp = response as ZigBeeReceivePacket;
				if (zigp != null)
				{
					DateTime now = DateTime.Now;
					string txt = Encoding.UTF8.GetString(zigp.Value);
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
