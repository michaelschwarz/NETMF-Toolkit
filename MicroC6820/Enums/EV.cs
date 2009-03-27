/* 
 * EV.cs
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
    /// <summary>
    /// Explosure value 0-8(low to high), default EV04
    /// </summary>
    public enum EV
    {
        EV00 = 0,
        EV01 = 1,
        EV02 = 2,
        EV03 = 3,
        EV04 = 4,   // default
        EV05 = 5,
        EV06 = 6,
        EV07 = 7,
        EV08 = 8
    }
}
