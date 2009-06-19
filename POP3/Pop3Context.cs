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

namespace MFToolkit.Net.Pop3
{
	public class Pop3Context
	{
		#region Constants
		
		private const string EOL = "\r\n";
		
		#endregion
		
		#region Private Variables

		private Socket socket;
		private int lastCommand;
		private string username;
		private string password;
		private Encoding encoding;
		private StringBuilder inputBuffer;
			
		#endregion
		
		#region Constructors
		
		public Pop3Context(Socket client)
		{
			lastCommand = -1;
			socket = client;
			encoding = Encoding.ASCII;
			inputBuffer = new StringBuilder();
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
				return lastCommand;
			}
			set
			{
				lastCommand = value;
			}
		}
		
		/// <summary>
		/// The client username, as specified by the USER command.
		/// </summary>
		public string Username
		{
			get
			{
				return username;
			}
			set
			{
				username = value;
			}
		}
		
		/// <summary>
		/// The client password, as specified by the PASS command.
		/// </summary>
		public string Password
		{
			get
			{
				return password;
			}
			set
			{
				password = value;
			}
		}

		
		/// <summary>
		/// The Socket that is connected to the client.
		/// </summary>
		public Socket Socket
		{
			get
			{
				return socket;
			}
		}
		
		#endregion
		
		#region Public Methods
		
		public void WriteLine(string data)
        {
#if(LOG && !MF && !WindowsCE)
			Console.WriteLine(" > " + data);
#endif
            socket.Send(encoding.GetBytes(data + EOL));
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
			int count;

			// TODO: read from Stream instead of the Socket to support SSL

			do
			{
				count = socket.Receive(byteBuffer);
				inputBuffer.Append(encoding.GetString(byteBuffer, 0, count));				
			}

			// TODO: count > 0 is not very good, but there is a problem with one of gmails mail servers
			while(count > 0 && (output = ReadBuffer()) == null);

#if(LOG && !MF && !WindowsCE)
			Console.WriteLine(" < " + output);
#endif
            return output;
		}
		
		public void Reset()
		{
			lastCommand = -1;
		}
		
		/// <summary>
		/// Closes the socket connection to the client and performs any cleanup.
		/// </summary>
		public void Close()
		{
			socket.Shutdown(SocketShutdown.Both);
			socket.Close();

			socket = null;
		}
		
		#endregion
		
		#region Private Methods
		
		private string ReadBuffer()
		{
			if(inputBuffer.Length > 0)				
			{
				string buffer = inputBuffer.ToString();
				int idx = buffer.IndexOf(EOL);
				if(idx != -1)
				{
					string output = buffer.Substring(0, idx);
					inputBuffer = new StringBuilder(buffer.Substring(idx + EOL.Length));
					return output;
				}				
			}
			return null;
		}
		
		#endregion
	}
}
