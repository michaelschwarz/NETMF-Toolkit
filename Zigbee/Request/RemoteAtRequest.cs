/* 
 * ZNetRemoteAtRequest.cs
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
    /// Allows for module parameter registers on a remote device to be queried or set.
    /// </summary>
    public class RemoteAtRequest : AtCommand
    {
        private XBeeAddress64 _address64 = XBeeAddress64.BROADCAST;
        private XBeeAddress16 _address16 = XBeeAddress16.BROADCAST;

        private byte _options = 0x00;           // 0x02 to save remote configuration instead of AC command

        #region Public Properties

        /// <summary>
        /// Serial Number
        /// </summary>
        public XBeeAddress64 SerialNumber
        {
            get { return _address64; }
            set { _address64 = value; }
        }

        /// <summary>
        /// Short Address
        /// </summary>
        public XBeeAddress16 ShortAddress
        {
            get { return _address16; }
            set { _address16 = value; }
        }

        public byte Options
        {
            get { return _options; }
            set { _options = value; }
        }

        #endregion

        public RemoteAtRequest(XBeeAddress64 address64, XBeeAddress16 address16, bool applyChanges, string command, byte[] value)
            : base(command, value)
        {
            this.ApiID = XBeeApiType.RemoteAtCommandRequest;
            _address64 = address64;
            _address16 = address16;

            if (applyChanges)
                _options = 0x02;
        }

        #region Other Constructors

        public RemoteAtRequest(XBeeAddress64 address64, XBeeAddress16 address16, bool applyChanges, AtCommand cmd)
            : this(address64, address16, applyChanges, cmd.Command, cmd.Value)
		{
		}

        public RemoteAtRequest(XBeeAddress64 address64, XBeeAddress16 address16, AtCommand cmd)
            : this(address64, address16, true, cmd)
        {
        }

        public RemoteAtRequest(XBeeAddress64 address64, bool applyChanges, AtCommand cmd)
            : this(address64, XBeeAddress16.BROADCAST, applyChanges, cmd.Command, cmd.Value)
        {
        }

        public RemoteAtRequest(XBeeAddress64 address64, AtCommand cmd)
            : this(address64, true, cmd)
        {
        }

        public RemoteAtRequest(XBeeAddress64 address64, string command)
            : this(address64, XBeeAddress16.BROADCAST, true, command, new byte[0])
        {
        }

        public RemoteAtRequest(XBeeAddress16 address16, bool applyChanges, AtCommand cmd)
            : this(XBeeAddress64.BROADCAST, address16, applyChanges, cmd.Command, cmd.Value)
        {
        }

        public RemoteAtRequest(XBeeAddress16 address16, AtCommand cmd)
            : this(address16, true, cmd)
        {
        }

        public RemoteAtRequest(XBeeAddress16 address16, string command)
            : this(XBeeAddress64.BROADCAST, address16, true, command, new byte[0])
        {
        }

        #endregion

        internal override void WriteBytesCommand(ByteWriter bw)
        {
            _address64.WriteBytes(bw);
            _address16.WriteBytes(bw);
            bw.Write(_options);

            bw.Write(Command);

            if (Value != null)
                bw.Write(Value);
        }

        public override string ToString()
        {
            return base.ToString() + "\r\nSerialNumber = " + SerialNumber + "\r\nShortAddress = " + ShortAddress;
        }
    }
}
