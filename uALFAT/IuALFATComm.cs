/* 
 * IuALFATComm.cs
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

namespace MFToolkit.Devices
{
    /// <summary>
    /// Interface class for uALFAT Communications
    /// </summary>
    public interface IuALFATComm
    {
        /// <summary>
        /// Open communication
        /// </summary>
        void Open();

        /// <summary>
        /// Close communication
        /// </summary>
        void Close();

        /// <summary>
        /// Send byte to uALFAT
        /// </summary>
        /// <param name="b">Byte to send</param>
        /// <returns>True on succes, False on error </returns>
        bool SendByte(byte b);

        /// <summary>
        /// Read byte from uALFAT
        /// </summary>
        /// <param name="b">Read byte</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>True on succes, False on error</returns>
        bool ReadByte(out byte b, long timeout);

    }
}
