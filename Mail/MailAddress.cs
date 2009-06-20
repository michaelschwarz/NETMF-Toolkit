/* 
 * MailAddress.cs
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
#if(!MF)
using System.Text.RegularExpressions;
#endif

namespace MFToolkit.Net.Mail
{
	/// <summary>
	/// Store a MailAddress and check if it is a valid MailAddress.
	/// </summary>
	public class MailAddress
	{
		#region Constants

#if(!MF)
		private static readonly Regex ILLEGAL_CHARACTERS = new Regex("[][)(@><\\\",;:]");
        private static readonly Regex ADDRESS_REGEX = new Regex("<[a-z0-9-_\\. ]+@[a-z0-9-_\\.]+>", RegexOptions.IgnoreCase);
#endif

        #endregion

        #region Privat Variables

        private string _localPart;
		private string _domain;

		#endregion

		#region Constructors

		public MailAddress(string address)
		{
			this.Address = address;
		}

		public MailAddress(string localPart, string domain)
		{
			this.LocalPart = localPart;
			this.Domain = domain;
		}

		#endregion

		#region Public Properties

        /// <summary>
        /// Gets or sets the local part of the address
        /// </summary>
		public string LocalPart
		{
			get
			{
				return _localPart.ToLower();
			}
			set
			{
				if(value == null || value.Length == 0)
				{
					throw new InvalidMailAddressException("Invalid local part. Local part must be at least one character.");
				}

				VerifySpecialCharacters(value);

				_localPart = value;
			}
		}

        /// <summary>
        /// Gets or sets the domain for the address
        /// </summary>
		public string Domain
		{
			get
			{
				return _domain.ToLower();
			}
			set
			{
				if(value == null || value.Length < 5)
				{
					throw new InvalidMailAddressException("Invalid domain. Domain must be at least five character (f.e. a.com, abc.de).");
				}

				VerifySpecialCharacters(value);

				_domain = value;
			}
		}

        /// <summary>
        /// Gets or sets the address
        /// </summary>
		public string Address
		{
			get
			{
				return LocalPart + "@" + Domain;
			}
			set
			{
				if(value == null || value.Length < 7)
				{
					throw new InvalidMailAddressException("Invalid email address. Email address must be at least seven character (f.e. a@a.com, a@aaa.de).");
				}

				string[] emailParts = value.Split(new Char[]{'@'});
				if(emailParts.Length != 2)
				{
					throw new InvalidMailAddressException("Invalid email address. Email addresses must have the format: username@domain.");
				}

				LocalPart = emailParts[0];
				Domain = emailParts[1];
			}
		}

		#endregion

		#region Object Methods

		public override string ToString()
		{
			return this.Address;
		}

		#endregion

		#region Private Methods

		private void VerifySpecialCharacters(string data)
		{
#if(!MF)
			if(ILLEGAL_CHARACTERS.IsMatch(data))
			{
                throw new InvalidMailAddressException("Illegal characters found.");
	    	}
#endif
		}

		#endregion
	}
}
