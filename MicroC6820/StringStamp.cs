/* 
 * StringStamp.cs
 * 
 * Copyright (c) 2009, Freesc Huang (http://www.microframework.cn)
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
 * MS   09-03-13    initial version
 * 
 * 
 * 
 */
using System;

namespace MFToolkit.MicroC6820
{
    [Serializable]
    public class StringStamp
    {
        private byte _fontW;

        public byte FontW
        {
            get { return _fontW; }
            set { _fontW = value; }
        }
        private byte _fontH;

        public byte FontH
        {
            get { return _fontH; }
            set { _fontH = value; }
        }
        private ushort _x;

        public ushort X
        {
            get { return _x; }
            set { _x = value; }
        }
        private ushort _y;

        public ushort Y
        {
            get { return _y; }
            set { _y = value; }
        }
        private byte _red;

        public byte Red
        {
            get { return _red; }
            set { _red = value; }
        }
        private byte _green;

        public byte Green
        {
            get { return _green; }
            set { _green = value; }
        }
        private byte _blue;

        public byte Blue
        {
            get { return _blue; }
            set { _blue = value; }
        }
        private byte _stringLength;

        public byte StringLength
        {
            get { return _stringLength; }
            set { _stringLength = value; }
        }
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (value.Length > 12)
                {
                    throw new Exception("OutOfRange Exception,the string length must be no more than 11");
                }
                _text = value;
            }
        }
        public StringStamp(byte fontW, byte fontH, ushort x, ushort y, byte red, byte green, byte blue, byte strLength, string text)
        {
            FontW = fontW;
            FontH = fontH;
            X = x;
            Y = y;
            Red = red;
            Green = green;
            Blue = blue;
            StringLength = strLength;
            Text = text;
        }
    }
}
