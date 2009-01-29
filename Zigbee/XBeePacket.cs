/* 
 * XBeePacket.cs
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
using System.IO;
using MSchwarz.IO;
#if(!MF)
using System.Collections.Generic;
#endif

namespace MSchwarz.Net.XBee
{
    public class XBeePacket
    {
        public const byte PACKET_STARTBYTE = 0x7E;
        public const byte PACKET_ESCAPE = 0x7D;
        public const byte PACKET_XON = 0x11;
        public const byte PACKET_XOFF = 0x13;

        private byte[] _bytes;

        public XBeePacket(byte[] bytes)
        {
			_bytes = bytes;
		}

		private byte[] GetBytesInternal()
		{
            ushort length = (ushort)_bytes.Length;

            XBeeChecksum checksum = new XBeeChecksum();
            checksum.AddBytes(_bytes);

            ByteWriter bw = new ByteWriter(length + 1 /* start byte */ + 2 /* bytes for length */ + 1 /* checksum byte */, ByteOrder.BigEndian);

            bw.Write(PACKET_STARTBYTE);
            bw.Write(length);
            bw.Write(_bytes);
            bw.Write(checksum.Compute());

			return bw.GetBytes();
        }

		public byte[] GetBytes()
        {
			return GetBytesInternal();
        }

		public byte[] GetEscapedBytes()
		{
			return EscapePacket(GetBytesInternal());
		}

        internal static bool IsSpecialByte(byte b)
        {
            if (b == PACKET_STARTBYTE || b == PACKET_ESCAPE || b == PACKET_XON || b == PACKET_XOFF)
                return true;

            return false;
        }

        private byte[] EscapePacket(byte[] bytes)
        {
			MemoryStream ms = new MemoryStream();

            int c = 0;
            foreach (byte b in bytes)
            {
                if (c++ > 0 && IsSpecialByte(b))
                {
                    ms.WriteByte(PACKET_ESCAPE);
					ms.WriteByte((byte)(0x20 ^ b));
                }
                else
                {
					ms.WriteByte(b);
                }
            }

            return ms.ToArray();
        }
    }
}
