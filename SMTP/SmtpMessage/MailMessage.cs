/* 
 * MailMessage.cs
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
using System.Text;
using System.Text.RegularExpressions;
using MFToolkit.Net.Mail;

namespace MFToolkit.Net.Smtp
{
	public class MailMessage
	{
		#region Constants

		private static readonly Regex SUBJECT_ENCODING = new Regex("=\\?[a-z0-9-]*\\?[BQ]\\?(.*)\\?=", RegexOptions.IgnoreCase);

		#endregion

		#region Private Variables

		private MailAddress senderAddress;
		private MailAddressCollection recipientAddresses;
		private string subject;
		private DateTime? sendDate;
        private int headerEnd = 0;
		private StringBuilder message;

		#endregion

		#region Constructors

		public MailMessage()
		{
			recipientAddresses = new MailAddressCollection();
			message = new StringBuilder();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// The mail address from the person sending this email.
		/// </summary>
		public MailAddress FromAddress
		{
			get
			{
				return senderAddress;
			}
			set
			{
				senderAddress = value;
			}
		}

		/// <summary>
		/// The mail subject.
		/// </summary>
		public string Subject
		{
			get
			{
				return subject;
			}
			set
			{
				subject = value;
			}
		}

		/// <summary>
		/// The mail sent date.
		/// </summary>
		public DateTime? Date
		{
			get
			{
				return sendDate;
			}
			set
			{
				sendDate = value;
			}
		}

		/// <summary>
		/// The addresses that this message will be delivered to.
		/// </summary>
		public MailAddressCollection ToAddress
		{
			get
			{
				return recipientAddresses;
			}
			set
			{
				recipientAddresses = value;
			}
		}

		/// <summary>
		/// The message that will be sent.
		/// </summary>
		public string Message
		{
			get
			{
				return message.ToString();
			}
		}

		#endregion

        internal void SetEndOfHeader()
        {
            headerEnd = message.Length;
        }

		#region Public Methods

		/// <summary>
		/// Append data to the message.
		/// </summary>
		/// <param name="data">The string to be added.</param>
		public void AddData(string data)
		{
            if (headerEnd <= 0)
            {
                try
                {
                    if (data.ToUpper().StartsWith("SUBJECT: ") == true)
                    {
                        // TODO: remove the parsing to the properties (ParseHeaderValue) because subject sometimes will have a linebreak
                        subject = MimeParser.CDecode(data.Substring(9));

                        //Match m = SUBJECT_ENCODING.Match(subject);
                        //if (m.Success)
                        //{
                        //    subject = m.Groups[1].Captures[0].Value;
                        //    subject = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(System.Text.Encoding.Default.GetBytes(subject));
                        //}
                    }
                    else if (data.ToUpper().StartsWith("CONTENT-TYPE: ") == true)
                    {

                    }
                    else if (data.ToUpper().StartsWith("DATE: ") == true)
                    {
                        sendDate = MimeParser.ParseDateS(data.Substring(6));
                    }
                }
                catch (Exception)
                {
                }
            }

			this.message.Append(data);
		}

		#endregion

		#region Object Methods

		public override string ToString()
		{
			return Message;
		}

		#endregion
	}
}
