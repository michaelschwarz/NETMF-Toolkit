/* 
 * Pop3Processor.cs
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
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MFToolkit.Net.Mail;
using System.IO;

#if(!MF)
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
#else
using MFToolkit.Text;
using Microsoft.SPOT.Net.Security;
#endif

namespace MFToolkit.Net.Pop3
{
	public class Pop3Processor
	{
		#region Constants

		public const int COMMAND_USER = 0;
		public const int COMMAND_PASS = 1;
		public const int COMMAND_QUIT = 3;
		public const int COMMAND_STAT = 4;
		public const int COMMAND_LIST = 5;
		public const int COMMAND_RETR = 6;
		public const int COMMAND_DELE = 7;
		public const int COMMAND_UIDL = 8;
		public const int COMMAND_TOP = 9;

		private const string EOL = "\r\n";

		private const string MESSAGE_DEFAULT_WELCOME = "+OK POP3 server ready";
		private const string MESSAGE_USER_OK = "+OK";
		private const string MESSAGE_PASSWORD_OK = "+OK";
		private const string MESSAGE_PASSWORD_INVALID = "-ERR password or username wrong";
		private const string MESSAGE_STAT = "+OK {0} {1}";      // message count, message size
		private const string MESSAGE_LIST = "+OK {0} mails";	// message count
		private const string MESSAGE_RETR = "+OK";
		private const string MESSAGE_DELE = "+OK message {0} deleted";	// message id
		private const string MESSAGE_UIDL = "+OK";
		private const string MESSAGE_UIDL_MESSAGE = "+OK {0} {1}";	// message id, message unique id
		private const string MESSAGE_EOF = ".";
		private const string MESSAGE_OK = "+OK";
		private const string MESSAGE_GOODBYE = "+OK Goodbye.";
		private const string MESSAGE_UNKNOWN_COMMAND = "-ERR command unrecognized.";
		private const string MESSAGE_INVALID_COMMAND_ORDER = "-ERR command not allowed here.";
		private const string MESSAGE_INVALID_ARGUMENT_COUNT = "-ERR incorrect number of arguments.";
		private const string MESSAGE_UNKNOWN_USER = "-ERR user does not exist.";
		private const string MESSAGE_SYSTEM_ERROR = "-ERR transaction failed.";
		
		#endregion
		
		#region Private Variables

		private Pop3Server _server;
		private IPop3Storage _storage;
		private Socket _socket;

		#endregion
		
		#region Constructors

		public Pop3Processor(Pop3Server server, Socket socket)
		{
			_server = server;
			_socket = socket;
		}

		public Pop3Processor(Pop3Server server, Socket socket, IPop3Storage storage)
			: this(server, socket)
		{
			_storage = storage;
		}
		
		#endregion
		
		#region Properties
		
		#endregion
				
		#region Public Methods
		
		public void ProcessConnection()
		{
			Stream stream;

			if (_server.IsSecure && _server.Certificate != null)
			{
				SslStream ssl = null;

				try
				{
#if(!MF)
					ssl = new SslStream(new NetworkStream(_socket));
					ssl.AuthenticateAsServer(_server.Certificate, false, SslProtocols.Default, false);
#else
					ssl = new SslStream(_socket);
					ssl.AuthenticateAsServer(_server.Certificate, SslVerification.NoVerification, SslProtocols.Default);
#endif
					stream = ssl;
				}
				catch (Exception)
				{
					//Close();
					return;
				}
			}
			else
			{
				stream = new NetworkStream(_socket);
			}

			Pop3Context context = new Pop3Context(stream, (IPEndPoint)_socket.LocalEndPoint, (IPEndPoint)_socket.RemoteEndPoint);

			try 
			{
				if (_storage != null && !_storage.AcceptClient(context.RemoteEndPoint))
					throw new Exception("Client not accepted!");

				SendWelcomeMessage(context);

				ProcessCommands(context);
			}
			catch(Exception)
			{
				if(_socket.Connected == true)
				{
					context.WriteLine("-ERR service not available");
				}
			}
			finally
			{
				_socket.Close();

				_socket = null;
				context = null;

				System.GC.Collect();
			}
		}
		
		#endregion		
		
		#region Private Handler Methods
		
		private void SendWelcomeMessage(Pop3Context context)
		{
#if(LOG && !MF && !WindowsCE)
			Console.WriteLine("*** Remote IP: {0} ***", context.RemoteEndPoint);
#endif

			
				context.WriteLine(MESSAGE_DEFAULT_WELCOME);
		}
		
		private void ProcessCommands(Pop3Context context)
		{
			bool isRunning = true;
			String inputLine;
			
			while(isRunning)
			{
				try
				{
					inputLine = context.ReadLine();

					String[] inputs = inputLine.Split(' ');
					
					switch(inputs[0].ToLower())
					{
						case "user":
							User(context, inputs);
							break;

						case "pass":
							Pass(context, inputs);
							break;

						case "rset":
							Rset(context);
							break;

						case "stat":
							Stat(context);
							break;

						case "list":
							List(context, inputs);
							break;

						case "uidl":
							Uidl(context, inputs);
							break;

						case "retr":
							Retr(context, inputs);
							break;

						case "top":
							Top(context, inputs);
							break;

						case "dele":
							Dele(context, inputs);
							break;

						case "quit":
							isRunning = false;

							context.WriteLine(MESSAGE_GOODBYE);
							context.Close();
							break;

						case "capa":
							Capa(context);
							break;

						case "auth":
						default:
							context.WriteLine(MESSAGE_UNKNOWN_COMMAND);
							break;
					}				
				}
				catch(Exception)
				{
					isRunning = false;

					context.WriteLine(MESSAGE_SYSTEM_ERROR);
					context.Close();
				}
			}
		}

		/// <summary>
		/// Handels the CAPA command.
		/// </summary>
		private void Capa(Pop3Context context)
		{
			context.WriteLine(MESSAGE_OK);
			context.WriteLine("TOP");
			context.WriteLine("UIDL");
			context.WriteLine("USER");
			context.WriteLine(MESSAGE_EOF);
		}

		/// <summary>
		/// Handels the USER command.
		/// </summary>
		private void User(Pop3Context context, String[] inputs)
		{
			if(context.LastCommand == -1)
			{
				if(inputs.Length == 2)
				{
					context.Username = inputs[1];
					context.LastCommand = COMMAND_USER;
					context.WriteLine(MESSAGE_USER_OK);
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the PASS command.
		/// </summary>
		private void Pass(Pop3Context context, String[] inputs)
		{
			if(context.LastCommand == COMMAND_USER)
			{
				if(inputs.Length == 2)
				{
					IMailStorage mail = (_storage != null ? _storage as IMailStorage : null);
					
					if(mail == null || mail.Login(context.Username, inputs[1]) == true)
					{
						context.Password = inputs[1];
						context.LastCommand = COMMAND_PASS;
						context.WriteLine(MESSAGE_PASSWORD_OK);
					}
					else
					{
						context.Username = null;
						context.Password = null;
						context.LastCommand = -1;
						context.WriteLine(MESSAGE_PASSWORD_INVALID);
					}
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Reset the connection state.
		/// </summary>
		private void Rset(Pop3Context context)
		{
			if(context.LastCommand != -1)
			{
				if (context != null && !String.IsNullOrEmpty(context.Username))
				{
					IMailStorage mail = (_storage != null ? _storage as IMailStorage : null);

					if (mail != null)
						mail.Logout(context.Username);
				}

				context.Reset();
				context.Username = null;
				context.Password = null;
				context.LastCommand = -1;

				context.WriteLine(MESSAGE_OK);
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the STAT command.
		/// </summary>
		private void Stat(Pop3Context context)
		{
			// TODO: wrong implementation, should only return unread messages
			if(context.LastCommand >= COMMAND_PASS)
			{
				Pop3MessageInfo[] list = new Pop3MessageInfo[0];

				if (_storage != null)
					list = _storage.GetMessageOverview(context.Username);

				long mailboxSize = 0;

				foreach (Pop3MessageInfo l in list)
					mailboxSize += l.Size;

				context.WriteLine(String.Format(MESSAGE_STAT, list.Length, mailboxSize));
				context.LastCommand = COMMAND_STAT;
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the LIST command.
		/// </summary>
		private void List(Pop3Context context, String[] inputs)
		{
			if(context.LastCommand >= COMMAND_PASS)
			{
				if (inputs.Length > 2)
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
				else
				{
					int idx = 0;

					if (inputs.Length == 2)
						idx = Convert.ToInt32(inputs[1]);

					Pop3MessageInfo[] list = new Pop3MessageInfo[0];

					if (_storage != null)
						list = _storage.GetMessageOverview(context.Username);

					if (inputs.Length == 1)
					{
						context.WriteLine(String.Format(MESSAGE_LIST, list.Length));

						int id = 1;
						foreach (Pop3MessageInfo l in list)
						{
							context.WriteLine(id + " " + l.Size);
							id++;
						}

						context.WriteLine(MESSAGE_EOF);
					}
					else
					{
						context.WriteLine(string.Format(MESSAGE_STAT, idx, list[idx - 1].Size));
					}

					context.LastCommand = COMMAND_LIST;
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the UIDL command.
		/// </summary>
		private void Uidl(Pop3Context context, String[] inputs)
		{
			if (context.LastCommand >= COMMAND_PASS)
			{
				if (inputs.Length > 2)
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
				else
				{
					int idx = 0;

					if (inputs.Length == 2)
						idx = Convert.ToInt32(inputs[1]);

					Pop3MessageInfo[] list = new Pop3MessageInfo[0];

					if (_storage != null)
						list = _storage.GetMessageOverview(context.Username);

					if (inputs.Length == 1)
					{
						context.WriteLine(String.Format(MESSAGE_LIST, list.Length));

						int id = 1;
						foreach (Pop3MessageInfo l in list)
						{
							context.WriteLine(id + " " + l.UniqueIdentifier);
							id++;
						}

						context.WriteLine(MESSAGE_EOF);
					}
					else
					{
						context.WriteLine(string.Format(MESSAGE_STAT, idx, list[idx - 1].UniqueIdentifier));
					}

					context.LastCommand = COMMAND_LIST;
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the TOP command.
		/// </summary>
		private void Top(Pop3Context context, String[] inputs)
		{
			if (context.LastCommand >= COMMAND_STAT)
			{
				if (inputs.Length == 3)
				{
					int idx = Convert.ToInt32(inputs[1]);
					int linesToRead = Convert.ToInt32(inputs[2]);

					context.WriteLine(MESSAGE_RETR);

					string message = _storage.ReadMessage(context.Username, idx);

					bool foundBody = false;
					int lines = 0;

					foreach (string line in message.Split(new string[] { "\r\n" }, 100, StringSplitOptions.None))
					{
						if (line.Length == 0)
							foundBody = true;

						if (!foundBody)
							context.WriteLine(line);
						else
						{
							if (lines < linesToRead)
								context.WriteLine(line);
							else
								break;
						}
					}

					context.WriteLine(MESSAGE_EOF);
					context.LastCommand = COMMAND_RETR;
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the RETR command.
		/// </summary>
		private void Retr(Pop3Context context, String[] inputs)
		{
			if(context.LastCommand >= COMMAND_STAT)
			{
				if(inputs.Length == 2)
				{
					int idx = Convert.ToInt32(inputs[1]);

					context.WriteLine(MESSAGE_RETR);
					context.WriteLine(_storage.ReadMessage(context.Username, idx));
					context.WriteLine(MESSAGE_EOF);
					context.LastCommand = COMMAND_RETR;
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		/// <summary>
		/// Handels the DELE command.
		/// </summary>
		private void Dele(Pop3Context context, String[] inputs)
		{
			if(context.LastCommand >= COMMAND_STAT)
			{
				if(inputs.Length == 2)
				{
					int idx = Convert.ToInt32(inputs[1]);

					_storage.DeleteMessage(context.Username, idx);
					
					context.WriteLine(String.Format(MESSAGE_DELE, idx));
					context.LastCommand = COMMAND_DELE;
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}

		#endregion
	}
}
