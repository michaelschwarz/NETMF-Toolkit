/* 
 * NBRecord.cs
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
 * MS   09-03-13    inital version
 * 
 */
using System;
using System.Net;
using MFToolkit.IO;

namespace MFToolkit.Net.Dns
{
    [Serializable]
    public class NBRecord : RecordBase
    {
        private readonly ushort _flags;
        private readonly IPAddress _ipAddress;

        #region Public Properties

        public bool G
        {
            get { return BitHelper.GetBit(_flags, 15); }
        }

        public ushort ONT
        {
            get { return BitHelper.GetBits(_flags, 13, 2); }
        }

        /// <summary>
        /// A 32 bit Internet address.
        /// </summary>
        public IPAddress IPAddress
        {
            get { return _ipAddress; }
        }

        #endregion

        internal NBRecord(IPAddress address)
        {
            _ipAddress = address;
        }

        internal NBRecord(DnsReader br)
        {
            _flags = br.ReadUInt16();
            _ipAddress = new IPAddress(br.ReadBytes(4));
        }

        public override string ToString()
        {
            return "    " + _ipAddress.ToString();
        }

        internal override byte[] GetBytes()
        {
            DnsWriter bw = new DnsWriter();

            bw.Write(_flags);
            bw.Write(_ipAddress.GetAddressBytes());

            return bw.GetBytes();
        }
    }
}