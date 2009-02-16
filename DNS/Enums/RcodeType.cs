/* 
 * RcodeType.cs
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

namespace MSchwarz.Net.Dns
{
    /// <summary>
    /// These are the return codes (RCODE) the server can send back. (RFC 1035 4.1.1)
    /// </summary>
    public enum RcodeType
    {
        /// <summary>
        /// No error condition
        /// </summary>
        Success = 0,

        /// <summary>
        /// The name server was unable to interpret the query.
        /// </summary>
        FormatError = 1,

        /// <summary>
        /// The name server was unable to process this query due to a problem 
        /// with the name server.
        /// </summary>
        ServerFailure = 2,

        /// <summary>
        /// Meaningful only for responses from an authoritative name server, 
        /// this code signifies that the domain name referenced in the query 
        /// does not exist.
        /// </summary>
        NameError = 3,

        /// <summary>
        /// The name server does not support the requested kind of query.
        /// </summary>
        NotImplemented = 4,

        /// <summary>
        /// The name server refuses to perform the specified operation for 
        /// policy reasons.  For example, a name server may not wish to provide 
        /// the information to the particular requester, or a name server may 
        /// not wish to perform a particular operation (e.g., zone transfer) 
        /// for particular data.
        /// </summary>
        Refused = 5,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd6 = 6,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd7 = 7,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd8 = 8,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd9 = 9,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd10 = 10,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd11 = 11,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd12 = 12,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd13 = 13,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd14 = 14,

        /// <summary>
        /// Reserved for future use
        /// </summary>
        Reserverd15 = 15
    }
}
