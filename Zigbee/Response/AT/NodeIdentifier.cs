﻿/* 
 * NodeIdentifierData.cs
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
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a node identifier command response structure
    /// </summary>
    public class NodeIdentifier : IAtCommandResponseData
	{
		private string _ni;

        #region Public Properties

        /// <summary>
        /// Node Identifier (NI)
        /// </summary>
        public string Identifier
        {
            get { return _ni; }
        }

        #endregion

        public static NodeIdentifier Parse(IAtCommandResponse cmd)
		{
            if (cmd.Command != NodeIdentifierCommand.command)
                throw new ArgumentException("This method is only applicable for the '" + NodeIdentifierCommand.command + "' command!", "cmd");

            ByteReader br = new ByteReader(cmd.Value, ByteOrder.BigEndian);

            NodeIdentifier ni = new NodeIdentifier();
            ni.ReadBytes(br);
			
            return ni;
		}

        public void ReadBytes(ByteReader br)
        {
            if (br.AvailableBytes > 0)
                _ni = br.ReadString((int)br.AvailableBytes);
        }

		public override string ToString()
		{
            return Identifier;
		}
	}
}
