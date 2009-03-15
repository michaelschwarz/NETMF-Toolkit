/* 
 * NumberParser.cs
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
    public static class NumberParser
    {
        /// <summary>
        /// Convert a numeric string to an integer
        /// </summary>
        /// <param name="S">String to convert</param>
        /// <returns>Converted integer</returns>
        public static int StringToInt(String S)
        {
            int r = 0;
            bool negative = false;
            foreach (Char c in S)
            {
                if (c == '-')
                {
                    negative = true;
                }
                else if ("0123456789".IndexOf(c) != -1)
                {
                    r *= 10;
                    r += (int)(c - '0');
                }
            }

            if (negative)
                return -r;
            else
                return r;
        }

        /// <summary>
        /// Convert a numeric string to an floating point var
        /// </summary>
        /// <param name="S">String to convert</param>
        /// <returns>Converted value</returns>
        public static double StringToDouble(String S)
        {
            double r = 0F;
            double m = 0.1F;

            bool afterDot = false;
            bool negative = false;

            foreach (char c in S)
            {
                if (c == '-')
                {
                    negative = true;
                    continue;
                }
                else if (c == '.')
                {
                    afterDot = true;
                    continue;
                }

                // Stop when character is not a number
                if ("0123456789".IndexOf(c) == -1) break;

                if (!afterDot)
                {
                    r *= 10F;
                    r += (double)(c - '0');
                }
                else
                {
                    r += ((double)(c - '0')) * m;
                    m /= 10F;
                }
            }

            if (negative)
                return -r;
            else
                return r;
        }
    }
}
