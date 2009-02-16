/* 
 * HttpServerUtility.cs
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
 * MS	08-03-24	initial version
 * 
 */
using System;
using System.IO;
using MSchwarz.IO;
using System.Text;

namespace MSchwarz.Net.Web
{
    public class HttpServerUtility
    {
        static bool IsSafe(char c)
        {
#if(!MF)
            if (char.IsLetterOrDigit(c))
                return true;
#endif

            switch (c)
            {
                case '\'':
                case '(':
                case ')':
                case '[':
                case ']':
                case '*':
                case '-':
                case '.':
                case '!':
                case '_':
                    return true;
            }

            if (c > 255)
                return true;

            return false;
        }

        public static string UrlEncode(string s)
        {
            if(s == null)
                return null;

            string res = "";

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ')
                    res += '+';
                else if (!IsSafe(s[i]))
                    res += '%' + ByteUtil.BytesToHex(Encoding.UTF8.GetBytes(s[i].ToString()));
                else
                    res += s[i];
            }

            return res;
        }

        public static string UrlDecode(string s)
        {            
            if (s == null) 
                return null;            
            
            if (s.Length < 1) 
                return s;            
            
            char[] chars = s.ToCharArray();            
            byte[] bytes = new byte[chars.Length * 2];
            
            int count = chars.Length;
            int dstIndex = 0;
            int srcIndex = 0;
            
            while (true)
            {                
                if (srcIndex >= count)
                {                    
                    if (dstIndex < srcIndex)
                    {                        
                        byte[] sizedBytes = new byte[dstIndex]; 
                        Array.Copy(bytes, 0, sizedBytes, 0, dstIndex);
                        bytes = sizedBytes;                    
                    }                    
                    
                    return new string(Encoding.UTF8.GetChars(bytes));
                }                 
                
                if (chars[srcIndex] == '+')
                {                    
                    bytes[dstIndex++] = (byte)' ';
                    srcIndex += 1;
                }                
                else if (chars[srcIndex] == '%' && srcIndex < count - 2)
                    if (chars[srcIndex + 1] == 'u' && srcIndex < count - 5)
                    {                        
                        int ch1 = HexToInt(chars[srcIndex + 2]);
                        int ch2 = HexToInt(chars[srcIndex + 3]);
                        int ch3 = HexToInt(chars[srcIndex + 4]);
                        int ch4 = HexToInt(chars[srcIndex + 5]);
                        
                        if (ch1 >= 0 && ch2 >= 0 && ch3 >= 0 && ch4 >= 0)
                        {
                            bytes[dstIndex++] = (byte)((ch1 << 4) | ch2);
                            bytes[dstIndex++] = (byte)((ch3 << 4) | ch4);
                            srcIndex += 6;
                            continue;
                        }
                    }
                    else                    
                    {
                        int ch1 = HexToInt(chars[srcIndex + 1]);
                        int ch2 = HexToInt(chars[srcIndex + 2]);
                        
                        if (ch1 >= 0 && ch2 >= 0)
                        {
                            bytes[dstIndex++] = (byte)((ch1 << 4) | ch2);
                            srcIndex += 3;
                            continue;
                        }
                    }
                else                
                {
                    byte[] charBytes = Encoding.UTF8.GetBytes(chars[srcIndex++].ToString());
                    charBytes.CopyTo(bytes, dstIndex);
                    dstIndex += charBytes.Length;
                }
            }
        }
        
        private static int HexToInt(char ch)
        {
            return (ch >= '0' && ch <= '9') ? ch - '0' :(ch >= 'a' && ch <= 'f') ? ch - 'a' + 10 :(ch >= 'A' && ch <= 'F') ? ch - 'A' + 10 :-1;
        }
    }
}
