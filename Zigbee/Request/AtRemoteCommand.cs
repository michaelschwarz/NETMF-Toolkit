/* 
 * AtRemoteCommand.cs
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
    public class AtRemoteCommand : XBeeRequest
    {
        private byte _frameID;
        private ulong _address64;
        private ushort _address16;
        private byte _options = 0x00;           // 0x02 to save remote configuration instead of AC command
        private string _command;
        private byte[] _value;

        public string Command
        {
            get { return _command; }
            set
            {
				if (value == null || value.Length == 0)		// String.IsNullOrEmpty(value))
                    throw new NullReferenceException("The command cannot be null or empty.");

                if (value.Length < 2)
                    throw new ArgumentException("The command must have at least 2 characters.", "value");

                _command = value;
            }
        }

        public byte[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public AtRemoteCommand(ulong address64, ushort address16, byte options, string command, byte[] value, byte frameID)
        {
            this.ApiID = XBeeApiType.RemoteCommandRequest;
            _frameID = frameID;
            _address64 = address64;
            _address16 = address16;
            _options = options;
            this.Command = command;
            this.Value = value;
        }

        public override byte[] GetBytes()
        {
            ByteWriter bw = new ByteWriter(ByteOrder.BigEndian);

            bw.Write((byte)ApiID);
            bw.Write(_frameID);
            bw.Write(_address64);
            bw.Write(_address16);
            bw.Write(_options);
            bw.Write(Command);

            if (_value != null)
                bw.Write(_value);

            return bw.GetBytes();
        }
    }
}
