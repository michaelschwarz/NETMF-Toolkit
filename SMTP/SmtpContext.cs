/* 
 * MimeParser.cs
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
 * 
 * 
 */
using System;
using System.Net.Sockets;
using System.Text;

namespace MFToolkit.Net.Smtp
{
	public class SmtpContext
	{
		#region Constants
		
		private const string EOL = "\r\n";
		
		#endregion
		
		#region Private Variables

        private Socket socket;
		private int lastCommand;
		private string clientDomain;
		private MailMessage message;
		private Encoding encoding;
		private StringBuilder inputBuffer;
			
		#endregion
		
		#region Constructors
		
		public SmtpContext(Socket client)
		{
			lastCommand = -1;
            socket = client;
            message = new MailMessage();
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
		/// The client domain, as specified by the HELO command.
		/// </summary>
		public string ClientDomain
		{
			get
			{
				return clientDomain;
			}
			set
			{
				clientDomain = value;
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
		
		/// <summary>
		/// The MailMessage that is currently being received.
		/// </summary>
		public MailMessage Message
		{
			get
			{
				return message;
			}
			set
			{
				message = value;
			}
		}
		
		#endregion
		
		#region Public Methods

        public void Write(string s)
        {
#if(LOG && !MF && !WindowsCE)
            Console.WriteLine(" > " + s);
#endif
            socket.Send(encoding.GetBytes(s));
        }

		public void WriteLine(string line)
		{
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
			int count;

            // TODO: read from Stream instead of the Socket to support SSL

            do
			{
                socket.ReceiveTimeout = 2000;
                count = socket.Receive(byteBuffer);

				inputBuffer.Append(encoding.GetString(byteBuffer, 0, count));				
			}
			while(count > 0 && (output = ReadBuffer()) == null);

#if(LOG && !MF && !WindowsCE)
			Console.WriteLine(" < " + output);
#endif
            return output;
		}
		
		public void Reset()
		{
			message = new MailMessage();
			lastCommand = SmtpProcessor.COMMAND_HELO;
		}
		
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
				int eolIndex = buffer.IndexOf(EOL);
				if(eolIndex != -1)
				{
					string output = buffer.Substring(0, eolIndex);
					inputBuffer = new StringBuilder(buffer.Substring(eolIndex + EOL.Length));
					return output;
				}				
			}
			return null;
		}
		
		#endregion
	}
}
