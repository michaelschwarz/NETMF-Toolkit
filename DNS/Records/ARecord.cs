/* 
 * ARecord.cs
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
 */
using System;
using System.Net;

namespace MFToolkit.Net.Dns
{
    /// <summary>
    /// A Resource Record (RFC 1035 3.4.1)
    /// 
    /// A records cause no additional section processing. The RDATA section of
    /// an A line in a master file is an Internet address expressed as four
    /// decimal numbers separated by dots without any imbedded spaces (e.g.,
    /// "10.2.0.52" or "192.0.5.6").
    /// </summary>
    [Serializable]
    public class ARecord : RecordBase
    {
        private readonly IPAddress _ipAddress;

        #region Public Properties

        /// <summary>
        /// A 32 bit Internet address.
        /// </summary>
        public IPAddress IPAddress
        {
            get { return _ipAddress; }
        }

        #endregion

        internal ARecord(DnsReader br)
        {
            _ipAddress = new IPAddress(br.ReadBytes(4));
        }

        public override string ToString()
        {
            return "    " + _ipAddress.ToString();
        }
    }
}