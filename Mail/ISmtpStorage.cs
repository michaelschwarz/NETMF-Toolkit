/* 
 * ISmtpStorage.cs
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
 * MS   09-06-19    initial ersion
 * MS	12-05-19	changed SpoolMessage method argument
 * 
 */
using System;
using System.Net;

namespace MFToolkit.Net.Mail
{
	/// <summary>
	/// Provides an interface to store incoming messages.
	/// </summary>
	public interface ISmtpStorage
	{
		bool AcceptClient(IPEndPoint client);

		bool AcceptRecipient(MailAddress recipient);

		bool AcceptSender(MailAddress sender);

		/// <summary>
		/// If the method returns true, the messeage will be marked as delivered,
		/// and a message will be send to the remote user.
		/// Return false if message will be rejected or if there is an error while
		/// saving message to the spooler (file, SQL Server,...).
		/// </summary>
		bool SpoolMessage(IPEndPoint client, MailAddressCollection recipients, string message, out string reply);
	}
}
