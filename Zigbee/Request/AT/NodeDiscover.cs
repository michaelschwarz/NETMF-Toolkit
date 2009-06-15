/* 
 * NodeDiscoverCommand.cs
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
	/// <summary>
	/// Networking Identification: The ND command
	/// is used to discover and report all modules on its
	/// current operating channel (CH parameter) and
	/// PAN ID (ID parameter). ND also accepts an NI
	/// (Node Identifier) value as a parameter. In this
	/// case, only a module matching the supplied identifier
	/// will respond.
	/// </summary>
	public class NodeDiscoverCommand : AtCommand
	{
        internal static string command = "ND";

		public NodeDiscoverCommand()
			: base(NodeDiscoverCommand.command)
		{
		}

		public NodeDiscoverCommand(string NI)
			: this()
		{
			if (NI != null && NI.Length > 0)			// !String.IsNullOrEmpty(NI))
			{
				if (NI.Length > 20)
					throw new ArgumentException("NI argument can only have a maximum of 20 characters.", "NI");

#if(MF)
				this.Value = Encoding.UTF8.GetBytes(NI);
#else
				this.Value = Encoding.ASCII.GetBytes(NI);
#endif
			}
		}
	}
}
