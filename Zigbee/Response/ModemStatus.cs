/* 
 * ModemStatus.cs
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
 * 
 * PH   09-01-28    inital version
 * 
 */
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a XBee modem status response
    /// </summary>
	public class ModemStatusResponse : XBeeResponse
	{
		private byte _modemStatus;

		#region Public Properties

        public ModemStatusType ModemStatus
		{
            get { return (ModemStatusType)_modemStatus; }
		}
		
		#endregion

        public ModemStatusResponse(short length, ByteReader br)
			: base(length, br)
		{
            _modemStatus = br.ReadByte();
		}

		public override string ToString()
		{
            return base.ToString() + "\r\n" + ModemStatus.ToString();
		}
	}
}