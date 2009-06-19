/* 
 * SmtpProcessor.cs
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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MSchwarz.Net.Mail;
using MFToolkit.Net.Mail;

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

		private const string MESSAGE_DEFAULT_WELCOME = "220 SMTP server ready {0}";
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

        private static readonly Regex ADDRESS_REGEX = new Regex("<[a-z0-9-_\\. ]+@[a-z0-9-_\\.]+>", RegexOptions.IgnoreCase);
		
		#endregion
		
		#region Private Variables
		
        private ISmtpStorage _storage;
        private string heloResponse;
        private Socket _socket;
		
		#endregion
		
		#region Constructors

        public SmtpProcessor(Socket socket)
		{
            _socket = socket;
		}

		public SmtpProcessor(Socket socket, ISmtpStorage storage)
            : this(socket)
		{
            _storage = storage;
		}
		
		#endregion
		
		#region Public Methods
		
		public void ProcessConnection()
		{
			SmtpContext context = new SmtpContext(_socket);

			try 
			{
				SendWelcomeMessage(context);
				
				// TODO: add this if gmail google is running
                //System.Threading.Thread.Sleep(200);

				ProcessCommands(context);
			}
			catch(Exception)
            {
#if(LOG && !MF && !WindowsCE)
				Console.WriteLine("Processor Exception: " + ex.Message);
#endif
                if (_socket != null && _socket.Connected)
				{
					context.WriteLine("421 Service not available, closing transmission channel");
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
		
		private void SendWelcomeMessage(SmtpContext context)
        {
#if(LOG && !MF && !WindowsCE)
			IPEndPoint clientEndPoint = (IPEndPoint) context.Socket.RemoteEndPoint;
			Console.WriteLine("*** Remote IP: {0} ***", clientEndPoint);
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

						case "quit":
							isRunning = false;

							context.WriteLine(MESSAGE_GOODBYE);
							context.Close();
							break;

						case "mail":
                            // TODO: move the input line to Mail(...)
							if(inputs[1].ToLower().StartsWith("from"))
							{
								Mail(context, inputLine.Substring(inputLine.IndexOf(" ")));
								break;
							}
							context.WriteLine(MESSAGE_UNKNOWN_COMMAND);
							break;

						case "rcpt":
							if(inputs[1].ToLower().StartsWith("to")) 							
							{
								Rcpt(context, inputLine.Substring(inputLine.IndexOf(" ")));
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
				catch(Exception)
				{
                    isRunning = false;

                    context.WriteLine(MESSAGE_SYSTEM_ERROR);
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
		private void Helo(SmtpContext context, String[] inputs)
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
                    MailAddress MailAddress = new MailAddress(argument);

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
                    MailAddress MailAddress = new MailAddress(argument);

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
			
			IPEndPoint clientEndPoint = (IPEndPoint) context.Socket.RemoteEndPoint;
			IPEndPoint localEndPoint = (IPEndPoint) context.Socket.LocalEndPoint;

			StringBuilder header = new StringBuilder();

            header.Append(String.Format("Received: from {0} ({0} [{1}])", context.ClientDomain, clientEndPoint.Address));
			header.Append("\r\n");
			header.Append(String.Format("     by {0}", localEndPoint.Address));			// TODO: can be replaced by hostname or something else
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

            if (message == null || _storage.SpoolMessage(message.ToAddress, message.Message, out spoolError))
                context.WriteLine(MESSAGE_OK);
            else
            {
                if (!String.IsNullOrEmpty(spoolError))
                {
                    context.Write("554");
                    context.WriteLine(spoolError.Replace("\r\n", " "));
                }
                else
                    context.WriteLine(MESSAGE_SYSTEM_ERROR);
            }
			
			context.Reset();
		}

		#endregion
	}
}
