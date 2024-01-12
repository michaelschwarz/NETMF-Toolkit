/* 
 * DnsClass.cs
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
    /// The DNS CLASS fields appear in resource records. (RFC 1035 3.2.4)
    /// </summary>
    public enum DnsClass : ushort
    {
        /// <summary>
        ///  The Internet
        /// </summary>
        IN = 1,

        /// <summary>
        /// The CSNET class (Obsolete - used only for examples in 
        /// some obsolete RFCs)
        /// </summary>
        [Obsolete("Used only for examples in some obsolete RFCs.", true)]
        CS = 2,

        /// <summary>
        /// The CHAOS class
        /// </summary>
        CH = 3,

        /// <summary>
        /// Hesiod [Dyer 87]
        /// </summary>
        HS = 4
    }
}
