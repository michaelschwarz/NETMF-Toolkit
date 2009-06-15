/* 
 * ZNetRxResponse.cs
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
    /// When the module receives an RF packet, it is sent out the UART using this message type.
    /// </summary>
	public class ZNetRxResponse : XBeeResponse
	{
        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
		private byte _options;
		private byte[] _value;

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

        public ReceiveOptionType ReceiveOption
        {
            get { return (ReceiveOptionType)_options; }
        }

        /// <summary>
        /// RF Data
        /// </summary>
		public byte[] Value
		{
			get { return _value; }
		}

		#endregion

        public ZNetRxResponse(short length, ByteReader br)
			: base(length, br)
		{
            _address64 = XBeeAddress64.ReadBytes(br);
            _address16 = XBeeAddress16.ReadBytes(br);
			_options = br.ReadByte();
			_value = br.ReadBytes(length - 12);
		}

		public override string ToString()
		{
			string s = base.ToString() + "\r\n";

			s += "SerialNumber = " + SerialNumber + "\r\n";
            s += "ShortAddress = " + ShortAddress + "\r\n";
			s += "Options      = " + ByteUtil.PrintByte(Options) + "\r\n";
			s += "Value        = " + ByteUtil.PrintBytes(Value);

			return s;
		}
	}
}