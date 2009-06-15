/* 
 * ZNetNodeDiscoverData.cs
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
using System;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a node discover command response structure
    /// </summary>
    public class ZNetNodeDiscover : IAtCommandResponseData
    {
        private const byte terminationCharacter = 0x00;

        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private string _ni;
        private XBeeAddress16 _parent16;
        private byte _deviceType;
        private byte _status;
        private ushort _profileID;
        private ushort _manufactureID;

        #region Public Properties

        /// <summary>
        /// Serial Number (SH SL)
        /// </summary>
        public XBeeAddress64 SerialNumber
        {
            get { return _address64; }
        }

        /// <summary>
        /// Short Address (MY)
        /// </summary>
        public XBeeAddress16 ShortAddress
        {
            get { return _address16; }
        }

        /// <summary>
        /// Node Identifier (NI)
        /// </summary>
        public string NodeIdentifier
        {
            get { return _ni; }
        }

        /// <summary>
        /// Parent Network Address (MP)
        /// </summary>
        public XBeeAddress16 ParentAddress
        {
            get { return _parent16; }
        }

        /// <summary>
        /// Device Type
        /// </summary>
        public ZNetDeviceType DeviceType
        {
            get { return (ZNetDeviceType)_deviceType; }
        }

        /// <summary>
        /// Source Action
        /// </summary>
        public SourceActionType SourceAction
        {
            get { return (SourceActionType)_status; }
        }

        /// <summary>
        /// Profile ID
        /// </summary>
        public ushort ProfileID
        {
            get { return _profileID; }
        }

        /// <summary>
        /// Manufacturer ID
        /// </summary>
        public ushort ManufacturerID
        {
            get { return _manufactureID; }
        }

        #endregion

        public static ZNetNodeDiscover Parse(IAtCommandResponse cmd)
        {
            if (cmd.Command != NodeDiscoverCommand.command)
                throw new ArgumentException("This method is only applicable for the '" + NodeDiscoverCommand.command + "' command!", "cmd");

            ByteReader br = new ByteReader(cmd.Value, ByteOrder.BigEndian);

            ZNetNodeDiscover nd = new ZNetNodeDiscover();
            nd.ReadBytes(br);

            return nd;
        }

        public void ReadBytes(ByteReader br)
        {
            _address16 = XBeeAddress16.ReadBytes(br);
            _address64 = XBeeAddress64.ReadBytes(br);

            _ni = br.ReadString(terminationCharacter);

            _parent16 = XBeeAddress16.ReadBytes(br);
            _deviceType = br.ReadByte();
            _status = br.ReadByte();
            _profileID = br.ReadUInt16();
            _manufactureID = br.ReadUInt16();
        }

        public override string ToString()
        {
            string s = "";

            s += "SerialNumber = " + SerialNumber + "\r\n";
            s += "ShortAddress = " + ShortAddress + "\r\n";
            s += "NodeIdentifier = " + NodeIdentifier + "\r\n";
            s += "Parent       = " + ParentAddress + "\r\n";
            s += "DeviceType   = " + DeviceType + "\r\n";
            s += "SourceAction = " + SourceAction + "\r\n";
            s += "ProfileID    = " + ProfileID + "\r\n";
            s += "ManufactureID = " + ManufacturerID;

            return s;
        }
    }
}