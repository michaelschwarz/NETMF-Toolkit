/* 
 * SsdpServer.cs
 * 
 * Copyright (c) 2009-2024, Michael Schwarz (http://www.schwarz-interactive.de)
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
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using MFToolkit.Net.Web;

namespace MFToolkit.Net.SSDP
{
    public class SsdpServer
    {
        private Socket _listenSocket;
        private Thread _thdListener;
        //private bool _stopThreads;
        private List<SsdpDevice> _deviceList;
        private HttpServer _http;

        private static String[] _broadcastAddresses = new String[] {
            "239.255.255.250"
            //,"255.255.255.255"
        };

        public SsdpServer()
        {
        }

        public event DeviceNotifyEventHandler DeviceNotify;

        public void Start()
        {
            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 1900);
            _listenSocket.Bind(endPoint);

            _listenSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(SsdpServer._broadcastAddresses[0]), IPAddress.Any));


            _thdListener = new Thread(new ThreadStart(ListenerThread));
#if(!MF)
            _thdListener.Name = "Listener Thread";
#endif
            _thdListener.Start();
            
            
            
            _http = new HttpServer(8888, IPAddress.Parse("192.168.178.20"), new HttpHandler(this));
            _http.Start();
        }

        private void ListenerThread()
        {
            while (true)        // !_stopThreads)
            {
                try
                {
                    IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint tempReceivePoint = (EndPoint)receivePoint;

                    // Generiere und empfange das Datagramm
                    byte[] packet = new byte[1024];
                    int length = _listenSocket.ReceiveFrom(packet, 0, packet.Length, SocketFlags.None, ref tempReceivePoint);

                    IPEndPoint endPoint = (IPEndPoint)tempReceivePoint;

                    string httpRequest = Encoding.Default.GetString(packet, 0, length);

                    Console.WriteLine("Received HTTP SOAP request. " + httpRequest.Split('\r')[0]);



                    if (httpRequest.StartsWith("M-SEARCH * HTTP/1.1"))
                    {
                        foreach (String address in _broadcastAddresses)
                        {
                            if (_deviceList == null)
                                continue;

                            foreach (SsdpDevice device in _deviceList)
                            {
                                /*
                                 * HTTP/1.1 200 OK
                                 * LOCATION: http://192.168.178.2:49000/igddesc.xml
                                 * SERVER: InteractivePrint UPnP/1.0 AVM FRITZ!Box Fon WLAN (UI) 08.04.27
                                 * CACHE-CONTROL: max-age=60
                                 * EXT:
                                 * ST: upnp:rootdevice
                                 * USN: uuid:75802409-bccb-40e7-8e6c-fa095ecce13e-00040E4B384A::upnp:rootdevice
                                 */

                                string res = string.Format(@"HTTP/1.1 200 OK
LOCATION: http://{0}:{1}/igddesc.xml/{2}
SERVER: {3} UPnP/1.0 MFToolkit
CACHE-CONTROL: max-age=60
EXT:
ST: upnp:rootdevice
USN: {2}::upnp:rootdevice

", _http.Address.ToString(), _http.Port, device.UDN, device.Name);

                                byte[] bytes = Encoding.ASCII.GetBytes(res);
                               
                                //for (int i = 0; i < 2; i++)
                                {
                                    Console.WriteLine("Sending HTTP SOAP message for " + device.Name + " (" + device.UDN + ")...");

                                    _listenSocket.SendTo(bytes, tempReceivePoint);
                                }
                            }
                        }

                        Thread.Sleep(100);
                    }
                }
                catch (Exception)
                {
                }
            }
            
        }

        public SsdpDevice FindDevice(string udn)
        {
            if (_deviceList == null)
                return null;

            return _deviceList.Find(delegate(SsdpDevice device) { return device != null && device.UDN == udn; });
        }

        public void UnregisterDevice(string udn)
        {
            SsdpDevice device = FindDevice(udn);

            if (device == null)
                return;

            NotifyDevice(device, false);

            _deviceList.Remove(device);
        }

        public void RegisterDevice(SsdpDevice device)
        {
            if (_deviceList == null)
                _deviceList = new List<SsdpDevice>();

            _deviceList.Add(device);

            NotifyDevice(device, true);
        }

        public void NotifyDevices()
        {
            if (_deviceList == null)
                return;

            foreach (SsdpDevice device in _deviceList)
                NotifyDevice(device, true);
        }

        internal void NotifyDevice(SsdpDevice device, bool isAlive)
        {
            /*
             * NOTIFY * HTTP/1.1
             * HOST: 239.255.255.250:1900
             * LOCATION: http://192.168.178.1:49000/igddesc.xml
             * SERVER: Interactive UPnP/1.0 AVM FRITZ!Box Fon WLAN 7270 54.04.70
             * CACHE-CONTROL: max-age=60
             * NT: urn:schemas-upnp-org:service:WANCommonInterfaceConfig:1
             * NTS: ssdp:alive
             * USN: uuid:75802409-bccb-40e7-8e6b-001F3FF667FC::urn:schemas-upnp-org:service:WAN
             */


            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.ReceiveTimeout = 100;

            List<string> res = new List<string>();

            foreach (String address in _broadcastAddresses)
            {
                string req = string.Format(@"NOTIFY * HTTP/1.1
HOST: {0}:1900
LOCATION: http://{1}:{2}/igddesc.xml/{3}
SERVER: {5} UPnP/1.0 MFToolkit
CACHE-CONTROL: max-age=60
NT: upnp:rootdevice
NTS: ssdp:{6}
USN: {3}::upnp:rootdevice

",
                      address,
                      _http.Address.ToString(),
                      _http.Port, 
                      device.UDN, 
                      DeviceTypeHelper.GetDeviceType(device.Type), 
                      device.Name,
                      (isAlive ? "alive" : "byebye"));

                byte[] data = Encoding.ASCII.GetBytes(req);
                byte[] buffer = new byte[1024];

                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        Console.WriteLine("Sending ssdp:" + (isAlive ? "alive" : "byebye") + " for " + device.Name + " (" + device.UDN + ")...");

                        s.SendTo(data, new IPEndPoint(IPAddress.Broadcast, 1900));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Use SSDP to discover UPnP devices in the local network.
        /// </summary>
        public List<string> Discover()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.ReceiveTimeout = 100;

            List<string> res = new List<string>();

            foreach (String address in _broadcastAddresses)
            {
                string req = string.Format(
                      "M-SEARCH * HTTP/1.1\r\n" +
                      "HOST: {0}:1900\r\n" +
                      "ST:upnp:rootdevice\r\n" +
                      "MAN:\"ssdp:discover\"\r\n" +
                      "MX:3\r\n\r\n",
                      address);

                byte[] data = Encoding.ASCII.GetBytes(req);
                byte[] buffer = new byte[1024];

                //for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        Console.WriteLine("Sending M-SEARCH...");

                        s.SendTo(data, new IPEndPoint(IPAddress.Broadcast, 1900));

                        while (true)
                        {
                            int l = s.Receive(buffer);

                            String resp = Encoding.ASCII.GetString(buffer, 0, l);

                            //Console.WriteLine(resp);

                            if (resp.ToLower().Contains("upnp:rootdevice"))
                            {
                                resp = resp.Substring(resp.ToLower().IndexOf("location:") + "Location:".Length);
                                resp = resp.Substring(0, resp.IndexOf("\r")).Trim();

                                if (!res.Contains(resp))
                                    res.Add(resp);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return res;
        }

        #region Events

        private void OnDeviceNotify(/* device */)
        {
            DeviceNotifyEventHandler handler = DeviceNotify;

            if (handler != null)
                handler(this, new DeviceNotifyEventArgs(/* status */));
        }

        #endregion
    }
}
