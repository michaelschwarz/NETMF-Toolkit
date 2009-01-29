/* 
 * NodeIdentification.cs
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

namespace MSchwarz.Net.XBee
{
    public class NodeIdentification : XBeeResponse
    {
        private ulong _address64;
        private ushort _address16;
        private byte _options;
        private ulong _addressNode64;
        private ushort _addressNode16;
        private string _ni;
        private ushort _parent16;
        private byte _deviceType;
        private byte _sourceAction;
        private ushort _profileID;
        private ushort _manufactureID;

		#region Public Properties

		// ...

		public ZigBeeDeviceType DeviceType
		{
			get { return (ZigBeeDeviceType)_deviceType; }
		}

		// ...

		#endregion

		public NodeIdentification(short length, ByteReader br)
            : base(br)
        {
            _address64 = br.ReadUInt64();
            _address16 = br.ReadUInt16();
            _options = br.ReadByte();

            _addressNode16 = br.ReadUInt16();
            _addressNode64 = br.ReadUInt64();

            _ni = br.ReadString((byte)0x00);		// TODO: verfiy if this is correct?!

            _parent16 = br.ReadUInt16();
            _deviceType = br.ReadByte();
            _sourceAction = br.ReadByte();
            _profileID = br.ReadUInt16();
            _manufactureID = br.ReadUInt16();
        }

        public override string ToString()
        {
            string s = "";

            s += "NodeIdentifier: " + _ni;

            return s;
        }
    }
}
