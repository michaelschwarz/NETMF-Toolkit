/* 
 * NodeDiscoverData.cs
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
 * BL	09-01-28	update for .NET 3.0
 * MS   09-02-07    changed back to non .NET 3.0 compiler options to support old VS
 * 
 * 
 */
using MSchwarz.IO;

namespace MSchwarz.Net.XBee
{
    public class NodeDiscoverData : IAtCommandData
    {
        private const byte terminationCharacter = 0x00;

        private ushort _addr16;
        private ulong _addr64;
        private string _ni;
        private ushort _parent16;
        private byte _deviceType;
        private byte _sourceAction;
        private ushort _profileID;
        private ushort _manufactureID;

        #region Public Properties

        public ushort Address16
        {
            get { return _addr16; }
        }

        public ulong Address64
        {
            get { return _addr64; }
        }

        public string NodeIdentifier
        {
            get { return _ni; }
        }

        public ushort ParentAddress
        {
            get { return _parent16; }
        }

        public ZigBeeDeviceType DeviceType
        {
            get { return (ZigBeeDeviceType)_deviceType; }
        }

        public byte Status
        {
            get { return _sourceAction; }
        }

        public ushort ProfileID
        {
            get { return _profileID; }
        }

        public ushort ManufacturerID
        {
            get { return _manufactureID; }
        }

        #endregion

        public void Fill(byte[] frameData)
        {
            using (ByteReader reader = new ByteReader(frameData, ByteOrder.BigEndian))
            {
                _addr16 = reader.ReadUInt16();
                _addr64 = reader.ReadUInt64();
                _ni = reader.ReadString(terminationCharacter);
                _parent16 = reader.ReadUInt16();
                _deviceType = reader.ReadByte();
                _sourceAction = reader.ReadByte();
                _profileID = reader.ReadUInt16();
                _manufactureID = reader.ReadUInt16();
            }
        }

        public override string ToString()
        {
#if(MF)
            return "Address: " + Address16.ToString() + ", Serial: " + Address64.ToString() + 
                   ", ID: " + NodeIdentifier + ", Type: " + DeviceType.ToString();
#else
            return string.Format("Address: {0:X}, Serial: {1:X}, ID: {2}, Type: {3}",
                Address16, Address64, NodeIdentifier, DeviceType.ToString());
#endif
        }
    }
}