/* 
 * InterfaceDataRateCommand.cs
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
 */
using System;
using System.Text;

namespace MFToolkit.Net.XBee
{
	public class InterfaceDataRateCommand : AtCommand
	{
        internal static string command = "BD";

		public InterfaceDataRateCommand()
			: base(InterfaceDataRateCommand.command)
		{
		}

        public InterfaceDataRateCommand(int baudRate)
			: this()
		{
			byte baudRateValue = 0x80;

			switch(baudRate)
			{
				case 1200:	baudRateValue = 0x00; break;
				case 2400:	baudRateValue = 0x01; break;
				case 4800:	baudRateValue = 0x02; break;
				case 9600:	baudRateValue = 0x03; break;
				case 19200:	baudRateValue = 0x04; break;
				case 38400:	baudRateValue = 0x05; break;
				case 57600:	baudRateValue = 0x06; break;
				case 115200:baudRateValue = 0x07; break;
			}

			this.Value = new byte[] { baudRateValue };
		}
	}
}