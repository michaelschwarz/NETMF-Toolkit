/* 
 * DateTimeStamp.cs
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
    public class DateTimeStamp
    {
        private DateFormat _format;
        private DateCorner _corner;
        private DateStyle _style;

        #region Public Properties

        public DateFormat Format
        {
            get { return _format; }
            set { _format = value; }
        }
        
        public DateCorner Corner
        {
            get { return _corner; }
            set { _corner = value; }
        }
        
        public DateStyle Style
        {
            get { return _style; }
            set { _style = value; }
        }

        #endregion

        public DateTimeStamp(DateFormat format, DateCorner corner, DateStyle style)
        {
            Format = format;
            Corner = corner;
            Style = style;
        }
    }
}
