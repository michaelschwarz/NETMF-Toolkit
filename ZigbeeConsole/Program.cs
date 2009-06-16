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

        static void xbee_FrameReceived(object sender, FrameReceivedEventArgs e)
        {
            Console.WriteLine("received a packet: " + e.Response);

            NodeDiscover nd = NodeDiscover.Parse((e.Response as AtCommandResponse));

            if (nd != null && nd.ShortAddress != null)
            {
                Console.WriteLine(nd);

                if (nd.NodeIdentifier == "SLAVE")
                {
                    Console.WriteLine("Sending \"Hallo\" to the SLAVE...");
                    (sender as XBee).ExecuteNonQuery(new TxRequest64(nd.SerialNumber, Encoding.ASCII.GetBytes("Hallo")));
                }
            }

            if (e.Response is RxResponse64)
            {
                Console.WriteLine("Recevied Rx64");
                Console.WriteLine(ByteUtil.PrintBytes((e.Response as RxResponse64).Value));
            }

        }

        static void Main(string[] args)
        {
            Thread thd1 = new Thread(new ThreadStart(RunModule1));
            thd1.IsBackground = true;
            thd1.Start();

            Thread thd2 = new Thread(new ThreadStart(RunModule2));
            thd2.IsBackground = true;
            thd2.Start();

            //thd1.Join();
            //thd2.Join();

            Console.ReadLine();
        }

        static void RunModule1()
        {
            using (XBee xbee = new XBee("COM3", 9600, ApiType.Enabled))
            {
                xbee.FrameReceived += new FrameReceivedEventHandler(xbee_FrameReceived);
                xbee.Open();

                // discovering the network
                AtCommand at = new NodeDiscoverCommand();

                xbee.Execute(at);
                Thread.Sleep(10 * 1000);

                xbee.Execute(at);
                Thread.Sleep(10 * 1000);


                xbee.StopReceiveData();
                Console.WriteLine("stopped master");
            }
        }

        static void RunModule2()
        {
            using (XBee xbee = new XBee("COM4", 9600, ApiType.Enabled))
            {
                xbee.FrameReceived += new FrameReceivedEventHandler(xbee_FrameReceived);
                xbee.Open();

                Thread.Sleep(30 * 1000);

                xbee.StopReceiveData();
                Console.WriteLine("stopped slave");
            }
        }
    }
}
