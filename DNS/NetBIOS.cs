/* 
 * NetBIOS.cs
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
 * MS   09-03-13    inital version
 * 
 * 
 * 
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
#if(MF)
using MFToolkit.Text;
#endif

namespace MFToolkit.Net.Dns
{
    public class NetBIOS
    {
        private readonly int _maxRetryAttemps = 2;
        private Exception _lastException = null;
        private ushort _nextMessageID = 1;

        public NetBIOS()
        {
        }

        #region Public Properties

        public Exception LastException
        {
            get { return _lastException; }
        }

        #endregion

        public IPAddress QueryName(string name)
        {
            DnsRequest request = new DnsRequest(new Question(EncodeName(name), DnsType.NB, DnsClass.IN));
            request.Header.RD = true;
            request.Header.B = true;

            DnsResponse res = Invoke(request);

            if (res == null ||
#if(MF)
                res.Answers.Length == 0
#else
                res.Answers.Count == 0
#endif
)
                return null;

            NBRecord nb = res.Answers[0].Record as NBRecord;

            if (nb == null)
                return null;

            return nb.IPAddress;
        }

        public bool Register(string name, IPAddress address)
        {
            DnsRequest request = new DnsRequest(new Question(EncodeName(name), DnsType.NB, DnsClass.IN));
            request.Header.Opcode = OpcodeType.Registration;
            request.Header.RA = false;
            request.Header.RD = false;
            request.Header.B = false;


            Additional add = new Additional();
            add.Domain = EncodeName(name);
            add.Class = DnsClass.IN;
            add.Type = DnsType.NB;
            add.Ttl = 30000;

            NBRecord nb = new NBRecord(address);
            add.Record = nb;
            
#if(MF)
            request.AdditionalRecords = new Additional[] { add };
#else
            request.AdditionalRecords.Add(add);
#endif

            DnsResponse res = Invoke(request, false);

            return true;
        }

        internal static string EncodeName(string domain)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in domain + "                ".Substring(0, 16 - domain.Length))
            {
                byte b = (byte)c;
                char x = (char)((byte)'A' + (((byte)c & 0xF0) >> 4));

                sb.Append(x);

                x = (char)((byte)'A' + ((byte)c & 0x0F));

                sb.Append(x);
            }

            return sb.ToString();
        }

        internal DnsResponse Invoke(DnsRequest request)
        {
            return Invoke(request, true);
        }

        internal DnsResponse Invoke(DnsRequest request, bool isQuery)
        {
            int attempts = 0;

            if (request.Header.MessageID == 0)
                request.Header.MessageID = _nextMessageID++;

            while (attempts <= _maxRetryAttemps)
            {
                byte[] bytes = request.GetMessage();

                if (bytes.Length > 512)
                    throw new ArgumentException("RFC 1035 2.3.4 states that the maximum size of a UDP datagram is 512 octets (bytes).");

                Socket socket = null;

                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    socket.ReceiveTimeout = 300;
                    //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 300);

#if(DEBUG && MF)
                    Microsoft.SPOT.Debug.Print(MFToolkit.IO.ByteUtil.PrintBytes(bytes));
#endif

                    socket.SendTo(bytes, bytes.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse("192.168.178.255"), 137));

                    if (!isQuery)
                        return null;

                    // Messages carried by UDP are restricted to 512 bytes (not counting the IP
                    // or UDP headers).  Longer messages are truncated and the TC bit is set in
                    // the header. (RFC 1035 4.2.1)
                    byte[] responseMessage = new byte[512];


                    //int numBytes = socket.Receive(responseMessage);

                    EndPoint ep = (EndPoint)new IPEndPoint(new IPAddress(4294967295), 137);
                    int numBytes = socket.ReceiveFrom(responseMessage, ref ep);

                    if (numBytes == 0 || numBytes > 512)
                        throw new Exception("RFC 1035 2.3.4 states that the maximum size of a UDP datagram is 512 octets (bytes).");

                    DnsReader br = new DnsReader(responseMessage);
                    DnsResponse res = new DnsResponse(br);

                    if (request.Header.MessageID == res.Header.MessageID)
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