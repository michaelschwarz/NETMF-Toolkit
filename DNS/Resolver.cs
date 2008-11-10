/* 
 * Resolver.cs
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
using System.Net;
using System.Net.Sockets;
using System.Management;

namespace MSchwarz.Net.Dns
{
    public class DnsResolver
    {
        private IPAddress _server = null;
        private IPEndPoint _endPoint;
        private readonly int _maxRetryAttemps = 4;
        private Exception _lastException = null;

        public DnsResolver()
        {
        }

        public DnsResolver(IPAddress server)
        {
            _server = server;

            // Messages sent using UDP user server port 53.
            // (RFC 1035 4.2)
            _endPoint = new IPEndPoint(_server, 53);
        }

        public DnsResolver(string hostName)
        {
			LoadFromHostname (hostName);   
        }

        #region Public Properties

        public Exception LastException
        {
            get { return _lastException; }
        }

        #endregion

		private void LoadFromHostname(string hostName)
		{
			IPAddress[] addresses = System.Net.Dns.GetHostAddresses (hostName);

			if (addresses.Length == 0)
				throw new ArgumentException ("Could not retrieve host IP address.");

			_server = addresses[0];

			// Messages sent using UDP user server port 53.
			// (RFC 1035 4.2)
			_endPoint = new IPEndPoint (_server, 53);
		}

		public void LoadNetworkConfiguration()
		{
			// http://msdn.microsoft.com/en-us/library/aa394217(VS.85).aspx


			//create out management class object using the
			//Win32_NetworkAdapterConfiguration class to get the attributes
			//of the network adapter
			ManagementClass mgmt = new ManagementClass ("Win32_NetworkAdapterConfiguration");

			//create our ManagementObjectCollection to get the attributes with
			ManagementObjectCollection objCol = mgmt.GetInstances ();

			//loop through all the objects we find
			foreach (ManagementObject obj in objCol)
			{
				if (_server == null)
				{
					if ((bool)obj["IPEnabled"] == true)
					{
						string[] dns = (string[])obj["DNSServerSearchOrder"];

						if (dns != null && dns.Length > 0)
						{
							LoadFromHostname (dns[0]);
						}
					}
				}
				//dispose of our object
				obj.Dispose ();
			}
		}

        public DnsResponse Resolve(DnsRequest request)
        {
            int attempts = 0;

            while (attempts <= _maxRetryAttemps)
            {
                byte[] bytes = request.GetMessage();

                if (bytes.Length > 512)
                    throw new ArgumentException("RFC 1035 2.3.4 states that the maximum size of a UDP datagram is 512 octets (bytes).");

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                socket.SendTo(bytes, bytes.Length, SocketFlags.None, _endPoint);

                // Messages carried by UDP are restricted to 512 bytes (not counting the IP
                // or UDP headers).  Longer messages are truncated and the TC bit is set in
                // the header. (RFC 1035 4.2.1)
                byte[] responseMessage = new byte[512];

                try
                {
                    int numBytes = socket.Receive(responseMessage);

                    if (numBytes == 0 || numBytes > 512)
                        throw new Exception("RFC 1035 2.3.4 states that the maximum size of a UDP datagram is 512 octets (bytes).");

                    DnsReader br = new DnsReader(responseMessage);
                    DnsResponse res = new DnsResponse(br);

                    if (request.MessageID == res.MessageID)
                        return res;

                    _lastException = new Exception("Not the answer for the current query.");
                    attempts++;
                }
                catch (SocketException se)
                {
                    _lastException = se;
                    attempts++;
                }
                finally
                {
                    socket.Close();
                    socket = null;
                }
            }

            throw new Exception("Could not resolve the query (" + attempts + " attempts).");
        }
    }
}
