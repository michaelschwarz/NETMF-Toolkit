/* 
 * NSRecord.cs
 * 
 * Copyright (c) 2008, Michael Schwarz (http://www.schwarz-interactive.de)
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

namespace MSchwarz.Net.Dns
{
    /// <summary>
    /// NS (Name Server) Resource Record (RFC 1035 3.3.11)
    /// </summary>
    [Serializable]
    public class NSRecord : RecordBase
    {
        private readonly string _domainName;

        #region Public Properties

        /// <summary>
        /// A &lt;domain-name&gt; which specifies a host which should be 
        /// authoritative for the specified class and domain.
        /// </summary>
        public string DomainName
        {
            get { return _domainName; }
        }

        #endregion

        internal NSRecord(DnsReader br)
        {
            _domainName = br.ReadDomain();
        }

        public override string ToString()
        {
            return "    nameserver = " + _domainName;
        }
    }
}