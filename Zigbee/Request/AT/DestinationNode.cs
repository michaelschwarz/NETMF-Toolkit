/* 
 * DestinationNodeCommand.cs
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
	public class DestinationNodeCommand : AtCommand
	{
        internal static string command = "DN";

        public DestinationNodeCommand()
			: base(DestinationNodeCommand.command)
		{
		}

        public DestinationNodeCommand(string NI)
			: this()
		{
			if (NI != null && NI.Length > 0)			// !String.IsNullOrEmpty(NI))
			{
				if (NI.Length > 20)
					throw new ArgumentException("NI argument can only have a maximum of 20 characters.", "NI");

				if (NI[0] == ' ')						// NI.StartsWith(" "))
					throw new ArgumentException("A NI can not start with a space.", "NI");

				// TODO: check if the NI contains only ASCII characters

#if(MF)
				this.Value = Encoding.UTF8.GetBytes(NI);
#else
				this.Value = Encoding.ASCII.GetBytes(NI);
#endif
			}
		}
	}
}
