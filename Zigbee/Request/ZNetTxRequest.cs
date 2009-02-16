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
using MSchwarz.IO;

namespace MSchwarz.Net.XBee
{
    public class ZNetTxRequest : XBeeRequest
    {
        private byte _frameID;
        private ulong _address64;
        private ushort _address16;
        private byte _broadcastRadius = 0;
        private byte _option;
        private byte[] _payload;

        public ZNetTxRequest(ulong address64, ushort address16, byte broadcastRadius, byte option, byte[] payload, byte frameID)
        {
            this.ApiID = XBeeApiType.ZigBeeTransmitRequest;
            _frameID = frameID;
            _address64 = address64;
            _address16 = address16;
            _broadcastRadius = broadcastRadius;
            _option = option;
            _payload = payload;
        }

        public override byte[] GetBytes()
        {
            ByteWriter bw = new ByteWriter(ByteOrder.BigEndian);

            bw.Write((byte)ApiID);
            bw.Write(_frameID);
            bw.Write(_address64);
            bw.Write(_address16);
            bw.Write(_broadcastRadius);
            bw.Write(_option);
            bw.Write(_payload);

            return bw.GetBytes();
        }
    }
}
