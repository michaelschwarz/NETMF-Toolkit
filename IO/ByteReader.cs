﻿/* 
 * ByteReader.cs
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
using System.IO;

namespace MFToolkit.IO
{
    public class ByteReader : IDisposable
    {
        protected byte[] _message;
        protected byte[] _buffer;
        protected long _position;
        protected Encoding _encoding = Encoding.UTF8;
        protected ByteOrder _byteOrder = ByteOrder.Default;
        private static ByteOrder _defaultByteOrder;

        /// <summary>
        /// Creates a new instance of ByteReader with UTF-8 encoding.
        /// </summary>
        /// <param name="message"></param>
        public ByteReader(byte[] message)
        {
            _message = message;
            _position = 0;

            if (_byteOrder == ByteOrder.Default)
                _byteOrder = _defaultByteOrder;
        }

        public ByteReader(byte[] message, ByteOrder byteOrder)
        {
            _message = message;
            _position = 0;
            _byteOrder = byteOrder;
        }

        public ByteReader(byte[] message, ByteOrder byteOrder, Encoding encoding)
            : this(message)
        {
            _byteOrder = byteOrder;
            _encoding = encoding;

            if (_byteOrder == ByteOrder.Default)
                _byteOrder = _defaultByteOrder;
        }

        public ByteReader(byte[] message, ByteOrder byteOrder, Encoding encoding, long position)
            : this(message, byteOrder, encoding)
        {
            _position = position;
        }

        static ByteReader()
        {
#if(MF)
            _defaultByteOrder = ByteOrder.LittleEndian;
#else
            if (BitConverter.IsLittleEndian)
                _defaultByteOrder = ByteOrder.LittleEndian;
            else
                _defaultByteOrder = ByteOrder.BigEndian; 
#endif
        }

        #region Public Properties

        public Encoding Encoding
        {
            get { return _encoding; }
            set { _encoding = value; }
        }

        public long Position
        {
            get { return _position; }
            set
            {
                if (value >= _message.Length || value < 0)
#if(MF)
					throw new Exception("At end of stream.");
#else
                    throw new EndOfStreamException();
#endif

                _position = value;
            }
        }

        #endregion

        protected virtual void FillBuffer(long numBytes)
        {
            byte[] bytes = new byte[numBytes];

            for (long i = 0; i < numBytes; i++)
                bytes[i] = Read();

            _buffer = bytes;
        }

        public long AvailableBytes
        {
            get { return _message.Length - _position; }
        }

        internal virtual ByteReader Copy()
        {
            return new ByteReader(_message, _byteOrder, _encoding, _position);
        }

        public virtual byte Peek()
        {
            if (_position >= _message.Length)
#if(MF)
				throw new Exception("At end of stream.");
#else
				throw new EndOfStreamException();
#endif

            return _message[_position];
        }

        private byte Read()
        {
            if (_position >= _message.Length)
#if(MF)
				throw new Exception("At end of stream.");
#else
				throw new EndOfStreamException();
#endif

            return _message[_position++];
        }

        public virtual byte ReadByte()
        {
            return (byte)Read();
        }

        public virtual byte[] ReadBytes(long length)
        {
            FillBuffer(length);

            return _buffer;
        }

        public bool ReadBoolean()
        {
            return (Read() != 0);
        }

        public virtual short ReadInt16()
        {
            FillBuffer(2);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (short)(_buffer[0] << 8 | _buffer[1]);
                case ByteOrder.LittleEndian:
                    return (short)(_buffer[0] | _buffer[1] << 8);
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual int ReadInt32()
        {
            FillBuffer(4);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (int)(_buffer[0] << 24 | _buffer[1] << 16 | _buffer[2] << 8 | _buffer[3]);
                case ByteOrder.LittleEndian:
                    return (int)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual long ReadInt64()
        {
            FillBuffer(8);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (long)(_buffer[0] << 56 | _buffer[1] << 48 | _buffer[2] << 40 | _buffer[3] << 32 | _buffer[4] << 24 | _buffer[5] << 16 | _buffer[6] << 8 | _buffer[7]);
                case ByteOrder.LittleEndian:
                    return (long)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24 | _buffer[4] << 32 | _buffer[5] << 40 | _buffer[6] << 48 | _buffer[7] << 56);
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual ushort ReadUInt16()
        {
            FillBuffer(2);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (ushort)(_buffer[1] | _buffer[0] << 8);
                case ByteOrder.LittleEndian:
                    return (ushort)(_buffer[0] | _buffer[1] << 8);
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual uint ReadUInt32()
        {
            FillBuffer(4);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (uint)(_buffer[3] | _buffer[2] << 8 | _buffer[1] << 16 | _buffer[0] << 24);
                case ByteOrder.LittleEndian:
                    return (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24);
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual ulong ReadUInt64()
        {
            FillBuffer(8);

            switch (_byteOrder)
            {
                case ByteOrder.BigEndian:
                    return (ulong)(
                        ((ulong)(_buffer[3] | _buffer[2] << 8 | _buffer[1] << 16 | _buffer[0] << 24)) << 32 |
                           (uint)(_buffer[7] | _buffer[6] << 8 | _buffer[5] << 16 | _buffer[4] << 24)
                           );
                case ByteOrder.LittleEndian:
                    return (ulong)(
                        ((ulong)(_buffer[4] | _buffer[5] << 8 | _buffer[6] << 16 | _buffer[7] << 24)) << 32 |
                           (uint)(_buffer[0] | _buffer[1] << 8 | _buffer[2] << 16 | _buffer[3] << 24)
                           );
                default:
                    throw new Exception("Could not handle bytes.");
            }
        }

        public virtual char ReadChar()
        {
            return (char)Read();
        }

        public virtual string ReadString(int length)
        {
            byte[] bytes = ReadBytes(length);

#if(MF)
			string s = "";
			foreach(char c in _encoding.GetChars(bytes))
			{
				s += c;
			}
			return s;
#elif (WindowsCE)
            return _encoding.GetString(bytes, 0, bytes.Length);
#else
            return _encoding.GetString(bytes);
#endif
        }

		public virtual string ReadString(byte terminator)
		{
            string s = "";

            while (true)
            {
                byte b = ReadByte();

                if (b == terminator)
                    return s;

                s += (char)b;
            }
        }

        /// <summary>
        /// Reads a string from the current stream. The length of the
        /// string is encoded from prefixed 32-bit integer.
        /// </summary>
        /// <returns></returns>
        public virtual string ReadString()
        {
            int length = ReadInt32();

#if(MF)
			string sb = "";
			for (int i = 0; i < length; i++)
            {
                sb += ReadChar();
            }
			return sb;
#else
			StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(ReadChar());
            }

            return sb.ToString();
#endif
        }

        public static ByteReader operator +(ByteReader br, long offset)
        {
            return new ByteReader(br._message, br._byteOrder, br._encoding, br._position + offset);
        }

        #region IDisposable Members

        public void Dispose(bool disposing)
        {
            _buffer = null;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}