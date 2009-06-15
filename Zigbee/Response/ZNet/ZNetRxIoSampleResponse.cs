/* 
 * ZNetRxIoSampleResponse.cs
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
/*
 * MS   09-02-06    fixed work item 3711
 * 
 */
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a ZigBee IO data sample response
    /// </summary>
    public class ZNetRxIoSampleResponse : XBeeResponse
    {
        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private byte _options;
        private byte _numSamples;
        private byte _digitalChannelMask1;
        private byte _digitalChannelMask2;
        private byte _analogChannelMask;
        private byte _digital1;
        private byte _digital2;
        private ushort _AD0;
        private ushort _AD1;
        private ushort _AD2;
        private ushort _AD3;
        private ushort _supplyVoltage;

        #region Public Properties

        /// <summary>
        /// Serial Number
        /// </summary>
        public XBeeAddress64 SerialNumber
        {
            get { return _address64; }
        }

        /// <summary>
        /// Short Address
        /// </summary>
        public XBeeAddress16 ShortAddress
        {
            get { return _address16; }
        }
        
        public ReceiveOptionType ReceiveOption
        {
            get { return (ReceiveOptionType)_options; } 
        }

        public byte NumSamples 
        { 
            get { return _numSamples; } 
        }

        public byte DigitalMask1 
        {
            get { return _digitalChannelMask1; } 
        }

        public byte DigitalMask2 
        { 
            get { return _digitalChannelMask2; } 
        }

        public byte AnalogMask 
        {
            get { return _analogChannelMask; }
        }

        public byte Digital1 
        { 
            get { return _digital1; }
        }

        public byte Digital2
        {
            get { return _digital2; } 
        }

        public ushort AD0 
        { 
            get { return _AD0; } 
        }

        public ushort AD1 
        { 
            get { return _AD1; }
        }

        public ushort AD2
        { 
            get { return _AD2; } 
        }

        public ushort AD3 
        { 
            get { return _AD3; } 
        }

        // return value is always Supply Voltage so I scaled it to real value
        public int SupplyVoltage 
        {
            get { return _supplyVoltage * 1200 / 1024; } 
        }

        #endregion

        public ZNetRxIoSampleResponse(short length, ByteReader br)
            : base(length, br)
        {
            _address64 = XBeeAddress64.ReadBytes(br);
            _address16 = XBeeAddress16.ReadBytes(br);

            _options = br.ReadByte();

            _numSamples = br.ReadByte();
            _digitalChannelMask1 = br.ReadByte();
            _digitalChannelMask2 = br.ReadByte();
            _analogChannelMask = br.ReadByte();

            if (_digitalChannelMask1 != 0x00 || _digitalChannelMask2 != 0x00)
            {
                _digital1 = br.ReadByte();
                _digital2 = br.ReadByte();
            }

            if (_analogChannelMask != 0x00)
            {
                if ((_analogChannelMask & 0x01) == 0x01) _AD0 = br.ReadUInt16();
                if ((_analogChannelMask & 0x02) == 0x02) _AD1 = br.ReadUInt16();
                if ((_analogChannelMask & 0x04) == 0x04) _AD2 = br.ReadUInt16();
                if ((_analogChannelMask & 0x08) == 0x08) _AD3 = br.ReadUInt16();
                if ((_analogChannelMask & 0x80) == 0x80) _supplyVoltage = br.ReadUInt16();
            }
        }

        public override string ToString()
        {
            string s = base.ToString() + "\r\n";

            s += "Receive Options = " + ReceiveOption + "\r\n";

            if (DigitalMask1 != 0x00 || DigitalMask2 != 0x00)
            {
                s += "D1  = " + Digital1 + "\r\n";
                s += "D2  = " + Digital2 + "\r\n";
            }

            if ((AnalogMask & 0x01) == 0x01) s += "AD0 = " + AD0 + "\r\n";
            if ((AnalogMask & 0x02) == 0x02) s += "AD1 = " + AD1 + "\r\n";
            if ((AnalogMask & 0x04) == 0x04) s += "AD2 = " + AD2 + "\r\n";
            if ((AnalogMask & 0x08) == 0x08) s += "AD3 = " + AD3 + "\r\n";
            if ((AnalogMask & 0x80) == 0x80) s += "supplyVoltage = " + SupplyVoltage + "mV\r\n";

            return s;
        }
    }
}