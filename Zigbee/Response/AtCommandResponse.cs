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
		private IAtCommandResponseData _data = null;

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

		public IAtCommandResponseData Data
		{
			get { return _data; }
		}

        public AtCommandResponse(ByteReader br)
            : base(br)
        {
            _frameID = br.ReadByte();

#if(MF)
			_command = ByteUtil.GetString(br.ReadBytes(2));
#else
			_command = Encoding.ASCII.GetString(br.ReadBytes(2));
#endif

            _status = br.ReadByte();

			if (br.AvailableBytes > 0)
			{
				_value = br.ReadBytes(br.AvailableBytes -1);

				switch (_command)
				{
					case "ND": _data = new NodeDiscoverResponseData(); break;
				}

				if (_data != null)
					_data.Fill(_value);
			}
        }

        public override string ToString()
        {
            string s = _command + " = ???";

            return s;
        }
    }
}
