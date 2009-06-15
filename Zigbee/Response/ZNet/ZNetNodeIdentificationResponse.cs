/* 
 * ZNetNodeIdentificationResponse.cs
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
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a node identification response
    /// </summary>
    public class ZNetNodeIdentificationResponse : XBeeResponse
    {
        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private byte _options;
        private XBeeAddress64 _addressNode64;
        private XBeeAddress16 _addressNode16;
        private string _ni;
        private XBeeAddress16 _parent16;
        private byte _deviceType;
        private byte _status;
        private ushort _profileID;
        private ushort _manufactureID;

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

        public byte Options
        {
            get { return _options; }
        }

        /// <summary>
        /// Node Identifier (NI)
        /// </summary>
        public string NodeIdentifier
        {
            get { return _ni; }
        }

        /// <summary>
        /// Parent Network Address
        /// </summary>
        public XBeeAddress16 ParentAddress
        {
            get { return _parent16; }
        }

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

		public ZNetNodeIdentificationResponse(short length, ByteReader br)
            : base(length, br)
        {
            _address64 = XBeeAddress64.ReadBytes(br);
            _address16 = XBeeAddress16.ReadBytes(br);

            _options = br.ReadByte();

            _addressNode16 = XBeeAddress16.ReadBytes(br);
            _addressNode64 = XBeeAddress64.ReadBytes(br);

            _ni = br.ReadString((byte)0x00);		// TODO: verfiy if this is correct?!

            _parent16 = XBeeAddress16.ReadBytes(br);
            _deviceType = br.ReadByte();
            _status = br.ReadByte();
            _profileID = br.ReadUInt16();
            _manufactureID = br.ReadUInt16();
        }

        public override string ToString()
        {
            string s = base.ToString() + "\r\n";

            s += "SerialNumber  = " + SerialNumber + "\r\n";
            s += "ShortAddress  = " + ShortAddress + "\r\n";
            s += "NodeIdentifier = " + NodeIdentifier + "\r\n";
            s += "Parent        = " + ParentAddress + "\r\n";
            s += "DeviceType    = " + DeviceType + "\r\n";
            s += "SourceAction  = " + SourceAction + "\r\n";
            s += "ProfileID     = " + ProfileID + "\r\n";
            s += "ManufactureID = " + ManufacturerID;

            return s;
        }
    }
}
