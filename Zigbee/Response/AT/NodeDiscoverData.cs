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
 * 
 * 
 * 
 */
using MSchwarz.IO;

namespace MSchwarz.Net.Zigbee
{
    public class NodeDiscoverData : IAtCommandData
    {
        private const byte terminationCharacter = 0x00;

        public ushort Address16 { get; private set; }
        public ulong Address64 { get; private set; }
        public string NodeIdentifier { get; private set; }
        public ushort ParentAddress { get; private set; }
        public ZigBeeDeviceType DeviceType { get; private set; }
        public byte Status { get; private set; }
        public ushort ProfileID { get; private set; }
        public ushort ManufacturerID { get; private set; }

        public void Fill(byte[] frameData)
        {
            using (ByteReader reader = new ByteReader(frameData, ByteOrder.BigEndian))
            {
                Address16 = reader.ReadUInt16();
                Address64 = reader.ReadUInt64();
                NodeIdentifier = reader.ReadString(terminationCharacter);
                ParentAddress = reader.ReadUInt16();
                DeviceType = (ZigBeeDeviceType)reader.ReadByte();
                Status = reader.ReadByte();
                ProfileID = reader.ReadUInt16();
                ManufacturerID = reader.ReadUInt16();
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