/* 
 * DnsHeader.cs
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
 * MS   09-03-13    initial version
 * 
 * 
 * 
 */
using System;
using MFToolkit.IO;

namespace MFToolkit.Net.Dns
{
    /// <summary>
    /// Header (RFC 1035 4.1.1, 1002 4.2.1)
    /// </summary>
    public class DnsHeader
    {
        private ushort _messageID;
        private ushort _flags;

        #region Public Properties

        /// <summary>
        /// Transaction ID
        /// </summary>
        public ushort MessageID
        {
            get { return _messageID; }
            internal set { _messageID = value; }
        }

        /// <summary>
        /// Response flag
        /// </summary>
        public bool QR
        {
            get { return BitHelper.GetBit(_flags, 15); }
            set { BitHelper.SetBit(ref _flags, 15, value); }
        }

        /// <summary>
        /// Packet type code
        /// </summary>
        public OpcodeType Opcode
        {
            get { return (OpcodeType)BitHelper.GetBits(_flags, 11, 4); }
            set { BitHelper.SetBits(ref _flags, 11, 4, (ushort)value); }
        }

        /// <summary>
        /// Authoritative Answer flag
        /// </summary>
        public bool AA
        {
            get { return BitHelper.GetBit(_flags, 10); }
            set { BitHelper.SetBit(ref _flags, 10, value); }
        }

        /// <summary>
        /// Truncation Flag
        /// </summary>
        public bool TC
        {
            get { return BitHelper.GetBit(_flags, 9); }
            set { BitHelper.SetBit(ref _flags, 9, value); }
        }

        /// <summary>
        /// Recursion Desired Flag
        /// </summary>
        public bool RD
        {
            get { return BitHelper.GetBit(_flags, 8); }
            set { BitHelper.SetBit(ref _flags, 8, value); }
        }

        /// <summary>
        /// Recursion Available Flag
        /// </summary>
        public bool RA
        {
            get { return BitHelper.GetBit(_flags, 7); }
            set { BitHelper.SetBit(ref _flags, 7, value); }
        }

        internal ushort Reserved
        {
            get { return BitHelper.GetBits(_flags, 4, 3); }
            set { BitHelper.SetBits(ref _flags, 4, 3, value); }
        }

        /// <summary>
        /// Broadcast Flag
        /// </summary>
        public bool B
        {
            get { return BitHelper.GetBit(_flags, 4); }
            set { BitHelper.SetBit(ref _flags, 4, value); }
        }

        public RcodeType Rcode
        {
            get { return (RcodeType)BitHelper.GetBits(_flags, 0, 4); }
            set { BitHelper.SetBits(ref _flags, 0, 4, (ushort)value); }
        }

        #endregion

        public DnsHeader()
        {
            this.RD = true;
        }

        internal DnsHeader(ByteReader br)
        {
            _messageID = br.ReadUInt16();
            _flags = br.ReadUInt16();
        }

        internal void WriteBytes(ByteWriter bw)
        {
            bw.Write(_messageID);
            bw.Write(_flags);
        }
    }
}
