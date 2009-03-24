/* 
 * Question.cs
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
 * MS   09-02-16    changed build compiler argument for .NET MF
 * 
 * 
 */
using System;
using System.Text;
using System.Net;
using MFToolkit.IO;
#if(MF)
using MFToolkit.Text;
#endif

namespace MFToolkit.Net.Dns
{
    /// <summary>
    /// A Question (RFC 1035 4.1.2)
    /// </summary>
    public class Question
    {
        private string _domain;     // QNAME
        private DnsType _qtype;
        private DnsClass _qclass;

        #region Public Properties

        /// <summary>
        /// The domain name to ask for.
        /// </summary>
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        /// <summary>
        /// The type of the query.
        /// </summary>
        public DnsType Type
        {
            get { return _qtype; }
            set { _qtype = value; }
        }

        /// <summary>
        /// The class of the query.
        /// </summary>
        public DnsClass Class
        {
            get { return _qclass; }
            set { _qclass = value; }
        }

        #endregion

        internal Question(DnsReader br)
        {
            _domain = br.ReadDomain();
            _qtype = (DnsType)br.ReadInt16();
            _qclass = (DnsClass)br.ReadInt16();
        }

        public Question()
        {
        }

        public Question(string domain, DnsType qtype, DnsClass qclass)
        {
			if (qtype == DnsType.PTR)
			{
				IPAddress addr = IPAddress.Parse (domain);

				StringBuilder sb = new StringBuilder();

				byte[] addrBytes = addr.GetAddressBytes();
				for(int i=addrBytes.Length -1; i>=0; i--)
					sb.Append((int)addrBytes[i] + ".");

				sb.Append ("in-addr.arpa");

				_domain = sb.ToString ();
			}
            else
                _domain = domain;

            _qtype = qtype;
            _qclass = qclass;
        }

        internal void Write(DnsWriter bw)
        {
            bw.WriteDomain(Domain);
            bw.Write((short)Type);
            bw.Write((short)Class);
        }
    }
}
