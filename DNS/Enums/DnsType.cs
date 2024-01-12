/* 
 * DnsType.cs
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

namespace MFToolkit.Net.Dns
{
    /// <summary>
    /// The DNS TYPE fields are used in resource records. (RFC 1035 3.2.2)
    /// </summary>
    public enum DnsType : ushort
    {
        /// <summary>
        /// A host address
        /// </summary>
        A = 1,

        /// <summary>
        /// An authoritative name server
        /// </summary>
        NS = 2,

        /// <summary>
        /// A mail destination (Obsolete - use MX)
        /// </summary>
        [Obsolete("Use DnsType.MX instead.", true)]
        MD = 3,

        /// <summary>
        /// A mail forwarder (Obsolete - use MX)
        /// </summary>
        [Obsolete("Use DnsType.MX instead.", true)]
        MF = 4,

        /// <summary>
        /// The canonical name for an alias
        /// </summary>
        CNAME = 5,

        /// <summary>
        /// Marks the start of a zone of authority
        /// </summary>
        SOA = 6,

        /// <summary>
        /// A mailbox domain name (EXPERIMENTAL)
        /// </summary>
        [Obsolete("DnsType used only experimental.", true)]
        MB = 7,

        /// <summary>
        /// A mail group member (EXPERIMENTAL)
        /// </summary>
        [Obsolete("DnsType used only experimental.", true)]
        MG = 8,

        /// <summary>
        /// a mail rename domain name (EXPERIMENTAL)
        /// </summary>
        [Obsolete("DnsType used only experimental.", true)]
        MR = 9,

        /// <summary>
        /// A null RR (EXPERIMENTAL)
        /// </summary>
        [Obsolete("DnsType used only experimental.", true)]
        NULL = 10,

        /// <summary>
        /// A well known service description
        /// </summary>
        WKS = 11,

        /// <summary>
        /// A domain name pointer
        /// </summary>
        PTR = 12,

        /// <summary>
        /// Host information
        /// </summary>
        HINFO = 13,

        /// <summary>
        /// Mailbox or mail list information
        /// </summary>
        MINFO = 14,

        /// <summary>
        /// Mail exchange
        /// </summary>
        MX = 15,

        /// <summary>
        /// Text strings
        /// </summary>
        TXT = 16,

        /// <summary>
        /// NetBIOS general Name Service Resource Record
        /// </summary>
        NB = 32,

        /// <summary>
        /// NetBIOS NODE STATUS Resource Record
        /// </summary>
        NBSTAT = 33
    }
}
