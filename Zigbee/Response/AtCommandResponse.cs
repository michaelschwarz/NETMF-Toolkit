/* 
 * AtCommandResponse.cs
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
    public class AtCommandResponse : XBeeResponse
    {
        private byte _frameID;
        private string _command;
        private byte _status;
        private byte[] _value;

        public string Command
        {
            get { return _command; }
        }

        public int Status
        {
            get { return (int)_status; }
        }

        public byte[] Value
        {
            get { return _value; }
        }

        public AtCommandResponse(short length, ByteReader br)
            : base(length, br)
        {
            _frameID = br.ReadByte();

#if(MF)
			_command = ByteUtil.GetString(br.ReadBytes(2));
#else
			_command = Encoding.ASCII.GetString(br.ReadBytes(2));
#endif

            _status = br.ReadByte();
            if (length > 6)
            {
                _value = br.ReadBytes(length - 6);

#if(DEBUG && !MF)
                if (_command == "ND")
                {
                    ByteReader nd = new ByteReader(_value, ByteOrder.BigEndian);

                    ushort addr16 = nd.ReadUInt16();
                    Console.WriteLine("MY " + addr16);

                    ulong addr64 = nd.ReadUInt64();
                    Console.WriteLine("SH SL " + addr64);

                    string ni = nd.ReadString(length - 24);

                    Console.WriteLine("NI [" + ni + "]");
                    Console.WriteLine("PARENT " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
                    Console.WriteLine("DEVICE_TYPE " + ByteUtil.PrintByte(nd.ReadByte()));
                    Console.WriteLine("STATUS " + ByteUtil.PrintByte(nd.ReadByte()));
                    Console.WriteLine("PROFILE_ID " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
                    Console.WriteLine("MANUFACTURER_ID " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
                }
#endif
            }
        }

        public override string ToString()
        {
            string s = _command + " = ???";

            return s;
        }
    }
}
