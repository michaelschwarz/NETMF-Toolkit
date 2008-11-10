/* 
 * ZigBeeIODataSample.cs
 * 
 * Copyright (c) 2008, Michael Schwarz (http://www.schwarz-interactive.de)
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
using System.Text;
using MSchwarz.IO;

namespace MSchwarz.Net.Zigbee
{
    public class ZigBeeIODataSample : XBeeResponse
    {
        private ulong _address64;
        private ushort _address16;
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

        public ZigBeeIODataSample(short length, ByteReader br)
            : base(length, br)
        {
            _address64 = br.ReadUInt64();
            _address16 = br.ReadUInt16();
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
                //light = parseIS(buf)["AI1"]
                //temp = parseIS(buf)["AI2"]
                //hum = parseIS(buf)["AI3"]

                if ((_analogChannelMask & 0x01) == 0x01) _AD0 = br.ReadUInt16();
                if ((_analogChannelMask & 0x02) == 0x02) _AD1 = br.ReadUInt16();
                if ((_analogChannelMask & 0x04) == 0x04) _AD2 = br.ReadUInt16();
                if ((_analogChannelMask & 0x08) == 0x08) _AD3 = br.ReadUInt16();
                if ((_analogChannelMask & 0x10) == 0x10) _supplyVoltage = br.ReadUInt16();
            }
        }

        public override string ToString()
        {
            string s = "";

            s += _digitalChannelMask1.ToString("X2") + "-" + _digitalChannelMask2.ToString("X2");
            s += "\r\n";
            s += "AD0 = " + _AD0 + "\r\n";
            s += "AD1 = " + _AD1 + "\r\n";
            s += "AD2 = " + _AD2 + "\r\n";
            s += "AD3 = " + _AD3 + "\r\n";
            s += "supplyVoltage = " + _supplyVoltage + "\r\n";

            s += "\r\n";

            double mVanalog = (((float)_AD2) / 1023.0) * 1200.0;
            double temp_C = (mVanalog - 500.0)/ 10.0 - 4.0;
            double lux = (((float)_AD1)  / 1023.0) * 1200.0;

	        mVanalog = (((float)_AD3) / 1023.0) * 1200.0;
            double hum = ((mVanalog * (108.2 / 33.2)) - 0.16) / (5 * 0.0062 * 1000.0);

            s += "temperature = " + temp_C + "\r\n";
            s += "light = " + lux + "\r\n";
            s += "humidity = " + hum + "\r\n";

            return s;
        }
    }
}
