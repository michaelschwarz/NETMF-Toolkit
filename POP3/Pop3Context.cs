/* 
 * Pop3Context.cs
 * 
 * Copyright (c) 2009, Michael Schwarz (http://www.schwarz-interactive.de)
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
using System.Net.Sockets;
using System.Text;
using System.Data;
using System.Threading;
using System.IO;
using System.Net;

namespace MFToolkit.Net.Pop3
{
	public class Pop3Context
	{
		#region Constants
		
		private const string EOL = "\r\n";
		
		#endregion
		
		#region Private Variables

		private Stream _stream;
        private IPEndPoint _localEndPoint;
        private IPEndPoint _remoteEndPoint;
		private int _lastCommand;
		private string _username;
		private string _password;
		private Encoding _encoding;
		private StringBuilder _inputBuffer;
			
		#endregion
		
		#region Constructors
		
		public Pop3Context(Stream client, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
		{
			_lastCommand = -1;
			_stream = client;
            _localEndPoint = localEndPoint;
            _remoteEndPoint = remoteEndPoint;
			_encoding = Encoding.ASCII;
			_inputBuffer = new StringBuilder();
		}
		
		#endregion
		
		#region Public Properties
		
		/// <summary>
		/// Last successful command received.
		/// </summary>
		public int LastCommand
		{
			get
			{
				return _lastCommand;
			}
			set
			{
				_lastCommand = value;
			}
		}
		
		/// <summary>
		/// The client username, as specified by the USER command.
		/// </summary>
		public string Username
		{
			get
			{
				return _username;
			}
			set
			{
				_username = value;
			}
		}
		
		/// <summary>
		/// The client password, as specified by the PASS command.
		/// </summary>
		public string Password
		{
			get
			{
				return _password;
			}
			set
			{
				_password = value;
			}
		}

        public IPEndPoint LocalEndPoint
        {
            get
            {
                return _localEndPoint;
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return _remoteEndPoint;
            }
        }

		#endregion
		
		#region Public Methods

        public void Write(string s)
        {
            byte[] bytes = _encoding.GetBytes(s);
            _stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteLine(string line)
        {
#if(LOG && !MF && !WindowsCE)
            Console.WriteLine(" > " + line);
#endif

            Write(line + EOL);
        }
		
		public String ReadLine()
		{
			string output = ReadBuffer();
			if(output != null)
            {
#if(LOG && !MF && !WindowsCE)
				Console.WriteLine(" < " + output);
#endif
                return output;
			}
						
			byte[] byteBuffer = new byte[80];
            int count = 0;

            DateTime begin = DateTime.Now;
            DateTime lastByteReceived = begin;

            do
            {
                _stream.ReadTimeout = 2000;

                try
                {
                    count = _stream.Read(byteBuffer, 0, byteBuffer.Length);

                    if (count > 0)
                        lastByteReceived = DateTime.Now;
                }
                catch (IOException)
                {
                    continue;
                }
                catch (Exception)
                {
                    DateTime nd = DateTime.Now;
#if(MF)
                    if((nd.Ticks - lastByteReceived.Ticks) / TimeSpan.TicksPerMillisecond < 10 * 1000)
                        continue;
#else
                    if ((nd - lastByteReceived).TotalMilliseconds < 10 * 1000)
                        continue;
#endif
                }

#if(MF)
                string s = "";
                foreach (char c in encoding.GetChars(byteBuffer))
                {
                    s += c;
                }
                inputBuffer.Append(s);
#else
                _inputBuffer.Append(_encoding.GetString(byteBuffer, 0, count));
#endif

			}
			while(count > 0 && (output = ReadBuffer()) == null);

#if(LOG && !MF && !WindowsCE)
			Console.WriteLine(" < " + output);
#endif
            return output;
		}
		
		public void Reset()
		{
			_lastCommand = -1;
		}
		
		/// <summary>
		/// Closes the socket connection to the client and performs any cleanup.
		/// </summary>
		public void Close()
		{
			_stream.Close();
            _stream.Dispose();

			_stream = null;
		}
		
		#endregion
		
		#region Private Methods
		
		private string ReadBuffer()
		{
			if(_inputBuffer.Length > 0)				
			{
				string buffer = _inputBuffer.ToString();

				int idx = buffer.IndexOf(EOL);
				if(idx != -1)
				{
					string output = buffer.Substring(0, idx);
					_inputBuffer = new StringBuilder(buffer.Substring(idx + EOL.Length));
					return output;
				}				
			}
			return null;
		}
		
		#endregion
	}
}
