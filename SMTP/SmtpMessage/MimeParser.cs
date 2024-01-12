#if(!MF)
/* 
 * MimeParser.cs
 * 
 * Copyright (c) 2009-2024, Michael Schwarz (http://www.schwarz-interactive.de)
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
 * 
 * 
 */
using System;
using System.IO;
using System.Text;

namespace MFToolkit.Net.Smtp
{
	public class MimeParser
	{
		public MimeParser()
		{
		}

		/// <summary>
		/// Quoted-Printable Decoder.
		/// </summary>
		public static string QDecode(System.Text.Encoding encoding, string data)
		{
			MemoryStream strm = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(data));
			int b = strm.ReadByte();

			MemoryStream dStrm = new MemoryStream();

			while(b > -1)
			{
				if(b == '=')
				{
					byte[] buf = new byte[2];
					strm.Read(buf,0,2);

					if(!(buf[0] == '\r' && buf[1] == '\n'))
					{
						try
						{
							int val = int.Parse(System.Text.Encoding.ASCII.GetString(buf),System.Globalization.NumberStyles.HexNumber);
							string encodedChar = encoding.GetString(new byte[]{(byte)val});
							byte[] d = System.Text.Encoding.Unicode.GetBytes(encodedChar);
							dStrm.Write(d, 0, d.Length);
						}
						catch
						{
						}
					}
				}
				else if(b == '_')
				{
					string blank = " ";
					byte[] d = System.Text.Encoding.Unicode.GetBytes(blank);
					dStrm.Write(d, 0, d.Length);
				}
				else
				{
					string encodedChar = encoding.GetString(new byte[]{(byte)b});
					byte[] d = System.Text.Encoding.Unicode.GetBytes(encodedChar);
					dStrm.Write(d, 0, d.Length);
				}

				b = strm.ReadByte();
			}

			return System.Text.Encoding.Unicode.GetString(dStrm.ToArray());
		}

		public static string CDecode(string data)
		{			
			if(data.IndexOf("=?") > -1)
			{
				int index = data.IndexOf("=?");

				string[] parts = data.Substring(index+2).Split(new char[]{'?'});
				
				string encoding = parts[0];
				string type     = parts[1];
				string datax    = parts[2];

				System.Text.Encoding enc = System.Text.Encoding.GetEncoding(encoding);
				if(type.ToUpper() == "Q")
				{
					return QDecode(enc, datax);
				}

				if(type.ToUpper() == "B")
				{
					return enc.GetString(Convert.FromBase64String(datax));
				}				
			}

			return data;
		}

		/// <summary>
		/// Parses RFC 2822 datetime.
		/// </summary>
		public static DateTime ParseDateS(string date)
		{
			date = date.Replace("GMT","-0000");
			date = date.Replace("EDT","-0400");
			date = date.Replace("EST","-0500");
			date = date.Replace("CDT","-0500");
			date = date.Replace("CST","-0600");
			date = date.Replace("MDT","-0600");
			date = date.Replace("MST","-0700");
			date = date.Replace("PDT","-0700");
			date = date.Replace("PST","-0800");

			string[] formats = new string[]{
			   "r",
			   "ddd, d MMM yyyy HH':'mm':'ss zzz",
			   "ddd, dd MMM yyyy HH':'mm':'ss zzz",
			   "dd'-'MMM'-'yyyy HH':'mm':'ss zzz",
			   "d'-'MMM'-'yyyy HH':'mm':'ss zzz"
		   };

			return DateTime.ParseExact(date.Trim(),formats,System.Globalization.DateTimeFormatInfo.InvariantInfo,System.Globalization.DateTimeStyles.None); 
		}
	}
}
#endif