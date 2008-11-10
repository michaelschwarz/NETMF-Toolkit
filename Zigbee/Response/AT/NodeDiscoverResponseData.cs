/* 
 * NodeDiscoverResponseData.cs
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
	public class NodeDiscoverResponseData : IAtCommandResponseData
	{
		private ushort _addr16;
		private ulong _addr64;
		private string _ni;
 
		public void Fill(byte[] value)
		{
			ByteReader nd = new ByteReader(value, ByteOrder.BigEndian);

            _addr16 = nd.ReadUInt16();
            _addr64 = nd.ReadUInt64();
            _ni = nd.ReadString(value.Length - 8);

			//Console.WriteLine("PARENT " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
			//Console.WriteLine("DEVICE_TYPE " + ByteUtil.PrintByte(nd.ReadByte()));
			//Console.WriteLine("STATUS " + ByteUtil.PrintByte(nd.ReadByte()));
			//Console.WriteLine("PROFILE_ID " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
			//Console.WriteLine("MANUFACTURER_ID " + ByteUtil.PrintBytes(nd.ReadBytes(2)));
		}
	}

	
}
