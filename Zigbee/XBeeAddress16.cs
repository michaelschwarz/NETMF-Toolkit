/* 
 * XBeeAddress16.cs
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
 * MS   09-03-24    initial version (replaces ushort Address16 properties)
 * 
 */
using System;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// This class represents a XBee device 16-bit address (short address).
    /// </summary>
    public class XBeeAddress16
    {
        private ushort _address16;

        #region Constants

        /// <summary>
        /// The broadcast address.
        /// </summary>
        public static readonly XBeeAddress16 BROADCAST = new XBeeAddress16(0xffff);

        ///// <summary>
        ///// The ZNet broadcast address.
        ///// </summary>
        //public static readonly XBeeAddress16 ZNET_BROADCAST = new XBeeAddress16(0xfffe);

        #endregion

        #region Public Properties

        /// <summary>
        /// The 16-bit address uniquely identifies a note and is permanent.
        /// </summary>
        public ushort Value
        {
            get { return _address16; }
            set { _address16 = value; }
        }

        #endregion

        public XBeeAddress16(ushort address16)
        {
            _address16 = address16;
        }

        #region Internal Methods

        internal static XBeeAddress16 ReadBytes(ByteReader br)
        {
            ushort addr16 = br.ReadUInt16();
            
            return new XBeeAddress16(addr16);
        }

        internal void WriteBytes(ByteWriter bw)
        {
            bw.Write(_address16);
        }

        #endregion

        public override string ToString()
        {
            ByteWriter bw = new ByteWriter(ByteOrder.BigEndian);
            bw.Write(Value);

            return ByteUtil.PrintBytes(bw.GetBytes());
        }
    }
}
