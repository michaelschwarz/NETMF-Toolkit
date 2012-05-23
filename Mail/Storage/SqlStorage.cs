#if(!MF && !WindowsCE)
/* 
 * SqlStorage.cs
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
 * MS   09-06-19    initial ersion
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using MFToolkit.Net.Pop3;
using System.Net;

namespace MFToolkit.Net.Mail.Storage
{
	/// <summary>
	/// Represents a SQL Server storage used for Pop3 and Smtp server
	/// </summary>
	public class SqlStorage : IMailStorage, ISmtpStorage, IPop3Storage
	{
		private string _connectionString;

		public SqlStorage()
		{
		}

		public SqlStorage(string connectionString)
		{
			_connectionString = connectionString;
		}

		#region IMailStorage Members

		public bool Login(string username, string password)
		{
			throw new NotImplementedException();
		}

		public void Logout(string username)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region ISmtpStorage Members

		public bool AcceptRecipient(MailAddress recipient)
		{
			throw new NotImplementedException();
		}

		public bool AcceptSender(MailAddress sender)
		{
			throw new NotImplementedException();
		}

		public bool SpoolMessage(IPEndPoint client, MailAddressCollection recipients, string message, out string reply)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IPop3Storage Members

		public bool AcceptClient(IPEndPoint client)
		{
			return true;
		}

		public Pop3MessageInfo[] GetMessageOverview(string mailbox)
		{
			throw new NotImplementedException();
		}

		public string ReadMessage(string mailbox, int idx)
		{
			throw new NotImplementedException();
		}

		public bool DeleteMessage(string mailbox, int idx)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
#endif