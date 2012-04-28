/* 
 * XBeeAddress64.cs
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
 * 
 * MS   09-03-24    initial version (replaces ulong Address64 properties)
 * MS   10-11-08    fixed property Value (work item 6894)
 * JS   11-05-19    XBeeAddress64.Value setter masks out bytes (work item 10320)
 * 
 */
using System;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// This class represents a XBee device 64-bit address (serial number).
    /// </summary>
    public class XBeeAddress64
    {
        private uint _msb;
        private uint _lsb;

        #region Constants

        /// <summary>
        /// The broadcast address.
        /// </summary>
        public static readonly XBeeAddress64 BROADCAST = new XBeeAddress64(0xffff);

        /// <summary>
        /// The coordinator shortcut address.
        /// </summary>
        public static readonly XBeeAddress64 ZNET_COORDINATOR = new XBeeAddress64(0x00);

        #endregion

        #region Public Properties

        /// <summary>
        /// The 64-bit address uniquely identifies a note and is permanent.
        /// </summary>
        public ulong Value
        {
            get
            {
                return ((ulong)_msb << 32) + _lsb;
            }
            set 
            {
                _msb = (uint)(value >> 32);
                _lsb = (uint)(value & 0xFFFFFFFF);
            }
        }

        /// <summary>
        /// The serial number high value (MSB).
        /// </summary>
        public uint SH
        {
            get { return _msb; }
            set { _msb = value; }
        }

        /// <summary>
        /// The serial number low value (LSB).
        /// </summary>
        public uint SL
        {
            get { return _lsb; }
            set { _lsb = value; }
        }

        #endregion

        #region Constructor

        public XBeeAddress64(ulong address64)
        {
            Value = address64;
        }

        public XBeeAddress64(uint msb, uint lsb)
        {
            _msb = msb;
            _lsb = lsb;
        }

        #endregion

        #region Internal Methods

        internal static XBeeAddress64 ReadBytes(ByteReader br)
        {
            uint sh = br.ReadUInt32();
            uint sl = br.ReadUInt32();
            
            return new XBeeAddress64(sh, sl);
        }

        internal void WriteBytes(ByteWriter bw)
        {
            bw.Write(_msb);
            bw.Write(_lsb);
        }

        #endregion

        public override string ToString()
        {
            ByteWriter bw = new ByteWriter(ByteOrder.BigEndian);
            bw.Write(SH);

            string s = "";

            s += "SH: " + ByteUtil.PrintBytes(bw.GetBytes()) + ", ";

            bw = new ByteWriter(ByteOrder.BigEndian);
            bw.Write(SL);

            s += "SL: " + ByteUtil.PrintBytes(bw.GetBytes());

            return s;
        }
    }
}
