/* 
 * SmtpProcessor.cs
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
using System.Net;
using System.Net.Sockets;
using System.Text;
using MFToolkit.Net.Mail;
using System.IO;

#if(!MF)
using System.Net.Security;
using System.Security.Authentication;
using System.Text.RegularExpressions;
#else
using MFToolkit.Text;
using Microsoft.SPOT.Net.Security;
#endif

namespace MFToolkit.Net.Smtp
{
	public class SmtpProcessor
	{
		#region Constants

		public const int COMMAND_HELO = 0;
		public const int COMMAND_RSET = 1;
		public const int COMMAND_NOOP = 2;
		public const int COMMAND_QUIT = 3;
		public const int COMMAND_MAIL = 4;
		public const int COMMAND_RCPT = 5;
		public const int COMMAND_DATA = 6;
		public const int COMMAND_VRFY = 7;

		private const string MESSAGE_DEFAULT_WELCOME = "220 SMTP server ready";
		private const string MESSAGE_DEFAULT_HELO_RESPONSE = "250 OK";
		private const string MESSAGE_OK = "250 OK";
		private const string MESSAGE_START_DATA = "354 Start mail input; end with <CRLF>.<CRLF>";
		private const string MESSAGE_GOODBYE = "221 Goodbye";
		private const string MESSAGE_UNKNOWN_COMMAND = "500 Command Unrecognized";
		private const string MESSAGE_INVALID_COMMAND_ORDER = "503 Command not allowed here";
		private const string MESSAGE_INVALID_ARGUMENT_COUNT = "501 Incorrect number of arguments";
		private const string MESSAGE_INVALID_ADDRESS = "451 Address is invalid";
		private const string MESSAGE_UNKNOWN_USER = "550 User does not exist";
		private const string MESSAGE_SENDER_NOT_ALLOWED = "554 Sender address rejected: Access denied";
		private const string MESSAGE_SYSTEM_ERROR = "554 Transaction failed";
		private const string MESSAGE_VRFY_VALID = "250 Address accepted";
		private const string MESSAGE_VRFY_VALIDFORWARD = "251 Address accepted, but forwarded";
		private const string MESSAGE_VRFY_NOTSURE = "252 Address is invalid";

#if(!MF)
		private static readonly Regex ADDRESS_REGEX = new Regex("(<)?[a-z0-9-_\\=\\.]+@[a-z0-9-_\\.]+(>)?", RegexOptions.IgnoreCase);
#endif

		#endregion
		
		#region Private Variables

		private SmtpServer _server;
		private ISmtpStorage _storage;
		private Socket _socket;
		
		#endregion
		
		#region Constructors

		public SmtpProcessor(SmtpServer server, Socket socket)
		{
			_server = server;
			_socket = socket;
		}

		public SmtpProcessor(SmtpServer server, Socket socket, ISmtpStorage storage)
			: this(server, socket)
		{
			_storage = storage;
		}
		
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
					ssl = new SslStream(new NetworkStream(_socket), false);
					ssl.AuthenticateAsServer(_server.Certificate); // , false, SslProtocols.Default, false);
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

			SmtpContext context = new SmtpContext(stream, (IPEndPoint)_socket.LocalEndPoint, (IPEndPoint)_socket.RemoteEndPoint);

			try 
			{
				if (_storage != null && !_storage.AcceptClient(context.RemoteEndPoint))
					throw new Exception("Client not accepted!");

				SendWelcomeMessage(context);

				ProcessCommands(context);
			}
			catch(Exception ex)
			{
#if(LOG && !MF && !WindowsCE)
				Console.WriteLine("Processor Exception: " + ex.Message);
#endif
				if (_socket != null
#if(!MF)
					&& _socket.Connected
#endif
				)
				{
					context.WriteLine("421 Service not available, closing transmission channel");
				}
			}
			finally
			{
				_socket.Close();

				_socket = null;
				context = null;

#if(!MF && !WindowsCE)
				System.GC.Collect();
#endif
			}
		}
		
		#endregion		
		
		#region Private Handler Methods
		
		private void SendWelcomeMessage(SmtpContext context)
		{
#if(LOG && !MF && !WindowsCE)
			Console.WriteLine("*** Remote IP: {0} ***", context.RemoteEndPoint);
#endif
			context.WriteLine(MESSAGE_DEFAULT_WELCOME);
		}
		
		private void ProcessCommands(SmtpContext context)
		{
			bool isRunning = true;
			String inputLine;
			
			while(isRunning)
			{
				try
				{
					inputLine = context.ReadLine();

					// TODO: remove this if gmail google is running
					if(inputLine == null)
					{
						isRunning = false;
						context.WriteLine(MESSAGE_GOODBYE);
						context.Close();
						break;
					}

					String[] inputs = inputLine.Split(' ');
					
					switch(inputs[0].ToLower())
					{
						case "helo":
							Helo(context, inputs);
							break;

						case "ehlo":
							Ehlo(context, inputs);
							break;

						case "rset":
							Rset(context);
							break;

						case "noop":
							context.WriteLine(MESSAGE_OK);
							break;

						case "vrfy":
							Vrfy(context, inputLine.Substring(5));
							break;

						case "quit":
							isRunning = false;

							context.WriteLine(MESSAGE_GOODBYE);
							context.Close();
							break;

						case "mail":
							// TODO: move the input line to Mail(...)
							if(inputs[1].ToLower().StartsWith("from"))
							{
								Mail(context, inputLine.Substring(inputLine.IndexOf(":")));
								break;
							}
							context.WriteLine(MESSAGE_UNKNOWN_COMMAND);
							break;

						case "rcpt":
							if(inputs[1].ToLower().StartsWith("to")) 							
							{
								Rcpt(context, inputLine.Substring(inputLine.IndexOf(":")));
								break;
							}
							context.WriteLine(MESSAGE_UNKNOWN_COMMAND);
							break;

						case "data":
							Data(context);
							break;

						default:
							context.WriteLine(MESSAGE_UNKNOWN_COMMAND);
							break;
					}				
				}
				catch(Exception ex)
				{
					SocketException sx = ex as SocketException;

					if (sx != null && sx.ErrorCode == 10060)
						context.WriteLine(MESSAGE_GOODBYE);
					else
						context.WriteLine(MESSAGE_SYSTEM_ERROR);

					isRunning = false;
					context.Close();
				}
			}
		}

		/// <summary>
		/// Handles the EHLO command.
		/// </summary>
		private void Ehlo(SmtpContext context, string[] inputs)
		{
			if (context.LastCommand == -1)
			{
				if (inputs.Length == 2)
				{
					context.ClientDomain = inputs[1];
					context.LastCommand = COMMAND_HELO;

					// TODO: EhloResponse instead of HeloResponse
					context.WriteLine(MESSAGE_DEFAULT_HELO_RESPONSE);
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
		/// Handles the HELO command.
		/// </summary>
		private void Helo(SmtpContext context, string[] inputs)
		{
			if(context.LastCommand == -1)
			{
				if(inputs.Length == 2)
				{
					context.ClientDomain = inputs[1];
					context.LastCommand = COMMAND_HELO;
					context.WriteLine(MESSAGE_DEFAULT_HELO_RESPONSE);				
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
		/// Verify if the recipient is valid.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="argument"></param>
		private void Vrfy(SmtpContext context, string argument)
		{
			//if (context.LastCommand != -1)
			//{
				if (!String.IsNullOrEmpty(argument))
				{
					MailAddress MailAddress = new MailAddress(ParseAddress(argument));

					if (_storage == null || _storage.AcceptRecipient(MailAddress))
					{
						context.LastCommand = COMMAND_VRFY;
						context.WriteLine(MESSAGE_VRFY_VALID);
					}
					else
					{
						context.WriteLine(MESSAGE_VRFY_NOTSURE);
					}
				}
				else
				{
					context.WriteLine(MESSAGE_INVALID_ARGUMENT_COUNT);
				}
			//}
			//else
			//{
			//    context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			//}
		}
		
		/// <summary>
		/// Reset the connection state.
		/// </summary>
		private void Rset(SmtpContext context)
		{
			if(context.LastCommand != -1)
			{
				context.Reset();
				context.WriteLine(MESSAGE_OK);
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}
		
		/// <summary>
		/// Handle the MAIL FROM command.
		/// </summary>
		private void Mail(SmtpContext context, string argument)
		{
			if(context.LastCommand == COMMAND_HELO)
			{
				try
				{
					MailAddress MailAddress = new MailAddress(ParseAddress(argument));

					if (_storage == null || _storage.AcceptSender(MailAddress))
					{
						context.Message.FromAddress = MailAddress;
						context.LastCommand = COMMAND_MAIL;
						context.WriteLine(MESSAGE_OK);
					}
					else
					{
						context.WriteLine(MESSAGE_SENDER_NOT_ALLOWED);
					}
				}
				catch (InvalidMailAddressException)
				{
					context.WriteLine(MESSAGE_INVALID_ADDRESS);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}
		
		/// <summary>
		/// Handle the RCPT TO command.
		/// </summary>
		private void Rcpt(SmtpContext context, string argument)
		{
			if(context.LastCommand == COMMAND_MAIL || context.LastCommand == COMMAND_RCPT)
			{
				try
				{
					MailAddress MailAddress = new MailAddress(ParseAddress(argument));

					if (_storage == null || _storage.AcceptRecipient(MailAddress))
					{
						context.Message.ToAddress.Add(MailAddress);
						context.LastCommand = COMMAND_RCPT;
						context.WriteLine(MESSAGE_OK);
					}
					else
					{
						context.WriteLine(MESSAGE_UNKNOWN_USER);
					}
				}
				catch (InvalidMailAddressException)
				{
					context.WriteLine(MESSAGE_INVALID_ADDRESS);
				}
			}
			else
			{
				context.WriteLine(MESSAGE_INVALID_COMMAND_ORDER);
			}
		}
		
		private void Data(SmtpContext context)
		{
			context.WriteLine(MESSAGE_START_DATA);
			
			MailMessage message = context.Message;
			
			//IPEndPoint clientEndPoint = (IPEndPoint) context.Socket.RemoteEndPoint;
			//IPEndPoint localEndPoint = (IPEndPoint) context.Socket.LocalEndPoint;

			StringBuilder header = new StringBuilder();

			header.Append("Received: from " + context.ClientDomain + " (" + context.ClientDomain + " [" + context.RemoteEndPoint.Address + "])");
			header.Append("\r\n");
			header.Append("     by " + context.LocalEndPoint.Address);
			header.Append("\r\n");
			header.Append("     " + System.DateTime.Now);
			header.Append("\r\n");
			
			message.AddData(header.ToString());
			
			String line = context.ReadLine();
			while(!line.Equals("."))
			{
				message.AddData(line);
				message.AddData("\r\n");

				if (line.Length == 0)
					message.SetEndOfHeader();

				line = context.ReadLine();
			}

			string spoolError;

			if (message == null || _storage.SpoolMessage(context.RemoteEndPoint, message.ToAddress, message.Message, out spoolError))
				context.WriteLine(MESSAGE_OK);
			else
			{
				if (spoolError != null && spoolError.Length > 0)
				{
					context.Write("554");
					context.WriteLine(spoolError);
				}
				else
					context.WriteLine(MESSAGE_SYSTEM_ERROR);
			}
			
			context.Reset();
		}

		private string ParseAddress(string input)
		{
			if (input == null || input.Length == 0)
				return null;

#if(!MF)
			System.Text.RegularExpressions.Match match = ADDRESS_REGEX.Match(input);

			if (match.Success)
			{
				string matchText = match.Value;
				
				if(matchText.StartsWith("<"))
					matchText = matchText.Remove(0, 1);

				if (matchText.EndsWith(">"))
					matchText = matchText.Remove(matchText.Length - 1, 1);

				return matchText;
			}

			return null;
#else
			string addr = "";
			for(int i=0; i<input.Length; i++)
			{
				if(input[i] == '<' || input[i] == '>')
					continue;
				
				addr += input[i];
			}
			return addr;
#endif
		}

		#endregion
	}
}
