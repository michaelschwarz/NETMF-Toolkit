/* 
 * GM862Exception.cs
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

namespace MFToolkit.MicroGM862
{
    public class GM862Exception : Exception
    {
        public GM862Exception()
            : base()
        { }

        public GM862Exception(String message)
            : base(message)
        { }

        public GM862Exception(String message, Exception innerException)
            : base(message, innerException)
        { }

        public const string WRONGARGUMENT = "GM862::WRONGARGUMENT";
        public const string DISPOSED = "GM862::DISPOSED";
        public const string WRONG_STATE = "GM862::WRONG_STATE";
        public const string BAD_RESPONSEBODY = "GM862::BAD_RESPONSEBODY";
        public const string FAILED_INITIALIZE = "GM862::FAILED_INITIALIZE";

    }
}
