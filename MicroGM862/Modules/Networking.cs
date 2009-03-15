/* 
 * Networking.cs
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
    public class Networking
    {
        #region Public classes and enums

        /// <summary>
        /// Socket Class for GM862 Driver
        /// </summary>
        public class Socket
        {
            #region Public classes and enums

            /// <summary>
            /// Supported Socket States
            /// </summary>
            public enum States
            {
                Closed,
                Open,
                Listening,
                Error
            }

            /// <summary>
            /// Supported Socket Protocols
            /// </summary>
            public enum SocketProtocols
            {
                UDP,
                TCP
            }

            #endregion

            #region Private properties and functions

            // The state this socket is in
            private States _state;

            // Object used to lock state
            private object _statelock = new object();

            // The protocol of this socket
            private SocketProtocols _socketprotocol;

            // Object used to lock socket protocol
            private object _socketprotocollock = new object();

            // Parent object for this socket
            private readonly Networking _parent;

            #endregion

            #region Public properties

            /// <summary>
            /// The current state this socket is in
            /// </summary>
            public States State
            {
                get
                {
                    lock (_statelock)
                    {
                        return _state;
                    }
                }

                set
                {
                    lock (_statelock)
                    {
                        _state = value;
                    }
                }
            }

            /// <summary>
            /// The protocol of this socket
            /// </summary>
            public SocketProtocols Protocol
            {
                get
                {
                    lock (_socketprotocollock)
                    {
                        return _socketprotocol;
                    }
                }

                set
                {
                    // We can't change the type on an open socket
                    if (State != States.Closed)
                        throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                    lock (_socketprotocollock)
                    {
                        _socketprotocol = value;
                    }
                }
            }

            /// <summary>
            /// The socket number used with the GM862 (1-6)
            /// </summary>
            public readonly byte SocketNo;

            #endregion

            #region Constructor/Destructor

            /// <summary>
            /// Create new Socket Instance
            /// Use the pre-created Socket instances of GM862.Sockets instead of creating new ones!
            /// </summary>
            /// <param name="SocketNo">GM862 to associate with</param>
            /// <param name="Parent">Parent GM862 Network Driver Instance</param>
            public Socket(byte SocketNo, Networking Parent)
            {
                if ((SocketNo < 1) || (SocketNo > 6) || (Parent == null))
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Store socket ID
                this.SocketNo = SocketNo;

                // Store parent
                this._parent = Parent;
            }

            #endregion

            /// <summary>
            /// Connect to EndPoint (IP address or DNS Name)
            /// </summary>
            /// <param name="EndPoint">IP address or DNS Name to connect to</param>
            /// <returns>True on succes, False on error</returns>
            public bool Connect(String EndPoint)
            {
                // We can only connect if this socket is closed
                if (State != States.Closed)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Check endpoint
                if ((EndPoint == null) || (EndPoint == String.Empty))
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                throw new NotImplementedException();
            }

            /// <summary>
            /// Close Socket Connection
            /// </summary>
            /// <returns></returns>
            public void Close()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Accept connection on a listening socket
            /// </summary>
            /// <returns></returns>
            public bool Accept()
            {
                // We can only accept on listening sockets
                if (State != States.Listening)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                throw new NotImplementedException();
            }

            /// <summary>
            /// Write data to socket
            /// </summary>
            /// <param name="Data">Data to write</param>
            /// <param name="Offset">Offset to start of data</param>
            /// <param name="Count">Number of bytes to write</param>
            /// <returns>Number of bytes written</returns>
            public int Write(byte[] Data, int Offset, int Count)
            {
                // We can only write to an open socket
                if (State != States.Open)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Check if data is not null
                if (Data == null)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Check if we have reached buffer borders
                if ((Offset + Count) > Data.Length)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                throw new NotImplementedException();
            }

            /// <summary>
            /// Read data from socket
            /// </summary>
            /// <param name="Data">Buffer for read data</param>
            /// <param name="Offset">Offset to start of data</param>
            /// <param name="Count">Number of bytes to read</param>
            /// <returns>Number of bytes read</returns>
            public int Read(byte[] Data, int Offset, int Count)
            {
                // We can only read from an open socket
                if (State != States.Open)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Check if data is not null
                if (Data == null)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                // Check if we have reached buffer borders
                if ((Offset + Count) > Data.Length)
                    throw new GM862Exception(GM862Exception.WRONGARGUMENT);

                throw new NotImplementedException();
            }
        }

        #endregion

        #region Private properties and functions

        // Device that module is associated with
        private readonly GM862GPS _device;

        // Indicator if networking is initialized
        private bool _networking_Initialized = false;

        // Unsolicitated Response handler used for networking
        private void _networking_Unsolicitated(string Response)
        {

        }

        // Find last position of an byte array in an other byte array
        public static int _find_array_last(byte[] needle, byte[] haystack)
        {
            return _find_array_last(needle, haystack, haystack.Length - 1);
        }

        // Find last position of an byte array in an other byte array
        public static int _find_array_last(byte[] needle, byte[] haystack, int start)
        {
            if (start == 0) return -1;

            int _needle_pos = needle.Length - 1;
            int _haystak_pos = start;

            for (_haystak_pos = start; _haystak_pos >= 0; _haystak_pos--)
            {
                if (haystack[_haystak_pos] == needle[_needle_pos])
                    _needle_pos--;
                else
                    _needle_pos = needle.Length - 1;

                if (_needle_pos == -1)
                {
                    return (_haystak_pos - _needle_pos + 1);
                }
            }

            return -1;
        }

        // Find first position of an byte array in an other byte array
        public static int _find_array(byte[] needle, byte[] haystack)
        {
            int _needle_pos = 0;
            int _haystak_pos = 0;

            for (_haystak_pos = 0; _haystak_pos < haystack.Length; _haystak_pos++)
            {
                if (haystack[_haystak_pos] == needle[_needle_pos])
                    _needle_pos++;
                else
                    _needle_pos = 0;

                if (_needle_pos == needle.Length)
                {
                    return (_haystak_pos - _needle_pos + 1);
                }
            }

            return -1;

        }

        #endregion

        #region Public properties

        /// <summary>
        /// Sockets used for networking
        /// </summary>
        public Socket[] Sockets;

        /// <summary>
        /// True if networking was succesfully initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _networking_Initialized;
            }
        }

        #endregion

        #region Constructor / Destructor

        /// <summary>
        /// Create new Networking Module
        /// </summary>
        /// <remarks>The GM862 Driver creates an instance. Don't create a new one!</remarks>
        /// <param name="Device">GM862GPS Driver that this module belongs to</param>
        public Networking(GM862GPS Device)
        {
            this._device = Device;
        }

        #endregion

        /// <summary>
        /// Initialize networking for GM862 Driver
        /// </summary>
        public void Initialize()
        {
            // Create Sockets
            Sockets = new Socket[]
            {
                new Socket(1, this),
                new Socket(2, this),
                new Socket(3, this),
                new Socket(4, this),
                new Socket(5, this),
                new Socket(6, this)
            };

            // Add new Unsolicitated Response handler checking for network related responses
            _device.OnUnsolicitatedResponse += new GM862GPS.UnsolicitatedResponseHandler(_networking_Unsolicitated);

            // We have succesfully initialized networking
            _networking_Initialized = true;
        }

        /// <summary>
        /// Request document trough HTTP
        /// Note: This function is depriciated. Use at own risk.
        /// It's removed when networking stack is finished
        /// </summary>
        /// <param name="SocketID">Socket ID to use</param>
        /// <param name="URL">URL of document to fetch</param>
        /// <param name="Referer">Referer. String.Empty if not used</param>
        /// <param name="Contents">Contents of retrieved document</param>
        /// <param name="RequestMethod">GET or POST depending on method</param>
        /// <param name="POSTContentType">Content type for POST body if used</param>
        /// <param name="POSTBody">POST Body</param>
        /// <returns>True on succes</returns>
        [Obsolete]
        public bool WebRequest(int SocketID, String URL, String Referer, out byte[] Contents, String RequestMethod, String POSTContentType, String POSTBody)
        {
            // Activate first GPRS Context
            _device.GPRS.ActivateContext(1);

            // Check for HTTP and remove it from the URL
            if (URL.IndexOf("http://") != 0) { Contents = null;  return false; }
            URL = URL.Substring(7);

            // Get Server
            String server = URL.Substring(0, URL.IndexOf("/"));

            // Get Path
            String path = URL.Substring(URL.IndexOf("/"));

            // Get Port
            Int32 port = 80;
            if (server.IndexOf(':') != -1)
            {
                String[] sServer = server.Split(new char[] { '|' });
                server = sServer[0];
                port = NumberParser.StringToInt(sServer[1]);
            }

            // Create request header
            String requestHeader = RequestMethod + " " + path + " HTTP/1.0\r\n";                          // Request URI
            requestHeader += "Host: " + server + "\r\n";                                                  // HTTP1.0 Host
            requestHeader += "Connection: Close\r\n";                                                     // HTTP1.0 Close connection after request
            requestHeader += "Pragma:	no-cache\r\n";                                                    // HTTP1.0 No caching support
            requestHeader += "Cache-Control: no-cache\r\n";                                               // HTTP1.0 No caching support
            requestHeader += "User-Agent: GM862 (.NET Micro Framework 3.0)\r\n";                          // HTTP1.0 User Agent

            if (Referer != String.Empty)
            {
                requestHeader += "Referer: " + Referer + "\r\n";                                          // HTTP1.0 Referer
            }

            if ((RequestMethod == "POST") & (POSTContentType != String.Empty))
            {
                requestHeader += "Content-Type: " + POSTContentType + "\r\n";
            }

            if (POSTBody != String.Empty)
            {
                requestHeader += "Content-Length: " + POSTBody.Length + "\r\n";
                requestHeader += "\r\n";
                requestHeader += POSTBody;
            }
            else
            {
                requestHeader += "\r\n";
            }

            // Make byte array from request
            byte[] requestHeaderRAW = System.Text.Encoding.UTF8.GetBytes(requestHeader);

            // Make connection
            if (!_device.GPRS.SocketDail(SocketID, GPRS.SocketProtocols.TCP, server, port, false, true)) { Contents = null; return false; }

            // Open online data mode
            if (!_device.GPRS.SocketRestore(1)) { Contents = null; return false; }

            // Send request
            _device.SendRawData(requestHeaderRAW, 0, requestHeaderRAW.Length);

            // Variables used for parsing response 
            byte[] responseBuffer = new byte[0xffff];
            int responseLength = 0;
            
            byte[] END_OF_HEADER = new byte[] { 13, 10, 13, 10 };
            String responseHeader = String.Empty;
            byte[] responseHeaderRAW;

            DateTime timeoutAt = DateTime.Now.AddSeconds(120);
            int bytesRead;
            int expectedSize = -1;

            // Read data from Socket until timeout
            while ((DateTime.Now < timeoutAt))
            {
                // Read as many bytes as posible from GM862
                bytesRead = _device.ReadRawData(responseBuffer, responseLength, responseBuffer.Length - responseLength);

                // If we have data reset timeout
                if (bytesRead > 0)
                {
                    // Reset Timeout Timer
                    timeoutAt = DateTime.Now.AddMilliseconds(10000);

                    // Increase read position
                    responseLength += bytesRead;
                }
                else
                {
                    System.Threading.Thread.Sleep(10);
                    continue;
                }

                // If our response buffer gets somewhat small increase it
                if ((responseBuffer.Length - responseLength) < 32)
                {
                    byte[] ResponseTemp = new byte[responseBuffer.Length + 0xffff];
                    Array.Copy(responseBuffer, ResponseTemp, responseBuffer.Length);
                    responseBuffer = ResponseTemp;
                }

                // Search for header End
                if (responseHeader == String.Empty)
                {
                    int HeaderEnd = _find_array(END_OF_HEADER, responseBuffer);
                    if (HeaderEnd != -1)
                    {
                        // Copy data from header
                        responseHeaderRAW = new byte[HeaderEnd];
                        Array.Copy(responseBuffer, responseHeaderRAW, responseHeaderRAW.Length);

                        // Try to parse header
                        try
                        {
                            responseHeader = new String(System.Text.Encoding.UTF8.GetChars(responseHeaderRAW)) + "\r\n";
                        }
                        catch
                        {
                            continue;
                        }

                        // Create lower case variant
                        String ResponseHeaderLower = responseHeader.ToLower();

                        // Find an expected length 
                        if (ResponseHeaderLower.IndexOf("content-length:") != -1)
                        {
                            // Get Content Length
                            Int32 SizeStart = ResponseHeaderLower.IndexOf("content-length: ") + 10;
                            Int32 SizeEnd = responseHeader.IndexOf("\r\n", SizeStart);

                            expectedSize = NumberParser.StringToInt(responseHeader.Substring(SizeStart, SizeEnd - SizeStart));
                        }

                        // Check for a location header
                        if (ResponseHeaderLower.IndexOf("location: ") != -1)
                        {
                            // Get Location
                            Int32 LocationStart = ResponseHeaderLower.IndexOf("location: ") + 10;
                            Int32 LocationEnd = responseHeader.IndexOf("\r\n", LocationStart);

                            String newLocation = responseHeader.Substring(LocationStart, LocationEnd - LocationStart);

                            // Close online connection
                            if (_device.EscapeSequence(1500, 1500, 5000) != AT_Interface.ResponseCodes.OK)
                            {
                                Contents = null;
                                return false;
                            }

                            // Close socket
                            if (!_device.GPRS.SocketShutdown(SocketID))
                            {
                                Contents = null;
                                return false;
                            }

                            // Pass on WebRequest
                            return WebRequest(SocketID, newLocation, URL, out Contents, RequestMethod, POSTContentType, POSTBody);
                        }

                        // Strip header from response
                        responseLength -= HeaderEnd + 4;
                        byte[] ResponseTemp = new byte[responseBuffer.Length - HeaderEnd - 4];
                        Array.Copy(responseBuffer, HeaderEnd + 4, ResponseTemp, 0, ResponseTemp.Length);
                        responseBuffer = ResponseTemp;
                    }

                }

                // Close connection when we've reached the expected size
                if ((expectedSize != -1) & (responseLength >= expectedSize))
                    break;
            }

            // Close online connection
            if (_device.EscapeSequence(1500, 1500, 5000) != AT_Interface.ResponseCodes.OK)
            {
                Contents = null;
                return false;
            }

            // Close socket
            if (!_device.GPRS.SocketShutdown(SocketID))
            {
                Contents = null;
                return false;
            }

            // Check header
            if (responseHeader == String.Empty)
            {
                Contents = null;
                return false;
            }

            // Check if we have all data
            if ((expectedSize != -1) & (responseLength < expectedSize))
            {
                Contents = null;
                return false;
            }

            // If we have an expected size strip response
            if ((expectedSize != -1) & (responseLength >= expectedSize))
                responseLength = expectedSize;

            // Strip unused bytes from input buffer
            Contents = new byte[responseLength];
            Array.Copy(responseBuffer, Contents, responseLength);

            // Succes
            return true;
        }
    }
}
