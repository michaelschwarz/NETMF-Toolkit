/* 
 * ZNetTxRequest.cs
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
 * MS   09-02-07    included ZNetTxRequest in this project
 * 
 */
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    [Obsolete("Use ZigBeeTransmitRequest instead.", true)]
    public class ZNetTxRequest : XBeeFrameRequest
    {
        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private byte _broadcastRadius = 0x00;
        private byte _options = 0x00;
        private byte[] _value;

        #region Public Properties

        public byte BroadcastRadios
        {
            get { return _broadcastRadius; }
            set
            {
                if (value > 10)
                    throw new ArgumentOutOfRangeException("The maximum hop value is 10.");

                _broadcastRadius = value;
            }
        }

        public byte Options
        {
            get { return _options; }
            set { _options = value; }
        }

        public byte[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        public ZNetTxRequest(XBeeAddress64 address64, XBeeAddress16 address16, byte broadcastRadius, byte options, byte[] value)
        {
            this.ApiID = XBeeApiType.ZigBeeTransmitRequest;
            _address64 = address64;
            _address16 = address16;
            _broadcastRadius = broadcastRadius;
            _options = options;
            _value = value;
        }

        internal override void WriteApiBytes(ByteWriter bw)
        {
            base.WriteApiBytes(bw);

            _address64.WriteBytes(bw);
            _address16.WriteBytes(bw);
            bw.Write(_broadcastRadius);
            bw.Write(_options);
            bw.Write(_value);
        }
    }
}
