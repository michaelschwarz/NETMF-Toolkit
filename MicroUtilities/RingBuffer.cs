/* 
 * RingBuffer.cs
 * 
 * Copyright (c) 2009, Elze Kool (http://www.microframework.nl)
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
using Microsoft.SPOT;

namespace MFToolkit.MicroUtilities
{
    public class RingBuffer
    {
        public readonly int size;
        byte[] buffer;
        int readPos = 0;
        int writePos = 0;

        public RingBuffer(int Size)
        {
            this.size = Size;
            buffer = new byte[this.size];
        }

        public bool isEmpty()
        {
            if (readPos == writePos)
                return true;
            else
                return false;
        }

        public int Count
        {
            get
            {
                int len = writePos - readPos;
                if (len < 0)
                    len += size;
                return len;
            }
        }

        public void Clear()
        {
            writePos = 0;
            readPos = 0;
        }

        public byte this[int index]
        {
            get
            {
                return buffer[(readPos + index) % size];
            }
            set
            {
                buffer[(writePos + index) % size] = value;
            }
        }

        public void Write(byte[] data)
        {
            foreach (byte b in data)
            {
                Write(b);
            }
        }

        public void Write(byte data)
        {
            buffer[writePos] = data;
            writePos = (writePos + 1) % size;
            if (readPos == writePos)
                throw new Exception("Buffer full");
        }

        public byte Peek()
        {
            return buffer[readPos];
        }

        public byte Read()
        {
            int pos = readPos;
            readPos = (readPos + 1) % size;
            return buffer[pos];
        }

        public void Read(byte[] buffer, int offset, int len)
        {
            for (int i = 0; i < len; ++i)
                buffer[i + offset] = Read();
        }

        public void Discard(int bytes)
        {
            if (Count < bytes)
                bytes = Count;

            readPos = (readPos + bytes) % size;
        }
    }
}
