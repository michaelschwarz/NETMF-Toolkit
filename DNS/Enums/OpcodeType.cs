/* 
 * OpcodeType.cs
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
    /// The Query Types (OPCODE) that specifies kind of query in a message.
    /// (RFC 1035 4.1.1 and 1002 4.2.1.1)
    /// </summary>
    public enum OpcodeType
    {
        /// <summary>
        /// A standard query (QUERY); used for NetBIOS, too
        /// </summary>
        Query = 0,

        /// <summary>
        /// An inverse query (IQUERY)
        /// </summary>
        InverseQuery = 1,

        /// <summary>
        /// A server status request (STATUS)
        /// </summary>
        Status = 2,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd3 = 3,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd4 = 4,

        /// <summary>
        /// NetBIOS registration
        /// </summary>
        Registration = 5,

        /// <summary>
        /// NetBIOS release
        /// </summary>
        Release = 6,

        /// <summary>
        /// NetBIOS WACK
        /// </summary>
        WACK = 7,

        /// <summary>
        /// NetBIOS refresh
        /// </summary>
        Refresh = 8,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd9 = 9,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd10 = 10,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd11 = 11,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd12 = 12,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd13 = 13,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd14 = 14,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        [Obsolete("Reserved for future use.")]
        Reserverd15 = 15
    }
}
