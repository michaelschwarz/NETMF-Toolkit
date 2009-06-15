/* 
 * TxRequest16.cs
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
 * 
 * 
 */
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// A TX Request message will cause the module to send RF Data as an RF Packet.TX Packet Frames.
    /// </summary>
    public class TxRequest16 : XBeeFrameRequest
    {
        private XBeeAddress16 _address16;
        private byte _options = 0x00;
        private byte[] _value;

        #region Public Properties

        /// <summary>
        /// Destination Address
        /// </summary>
        public XBeeAddress16 Destination
        {
            get { return _address16; }
            set 
            { 
                _address16 = value; 
            }
        }

        public byte Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public bool DisableACK
        {
            get { return BitHelper.GetBit(Options, 1); }
            set
            {
                BitHelper.SetBit(ref _options, 1, true);
            }
        }

        public bool EnableBroadcastPanID
        {
            get { return BitHelper.GetBit(Options, 8); }
            set
            {
                BitHelper.SetBit(ref _options, 8, true);
            }
        }

        public byte[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        public TxRequest16(XBeeAddress16 address16, byte[] data)
        {
            this.ApiID = XBeeApiType.TxRequest16;
            _address16 = address16;

            _value = data;
        }

        internal override void WriteApiBytes(ByteWriter bw)
        {
            if (_value != null && _value.Length > 100)
                throw new Exception("Value exceeds maximum of 100 bytes per packet.");

            base.WriteApiBytes(bw);

            _address16.WriteBytes(bw);

            bw.Write(_options);
            bw.Write(_value);
        }
    }
}
