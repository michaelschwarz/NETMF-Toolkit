/* 
 * XBeeModule.cs
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
 * MS   09-01-29    inital version
 * MS   09-02-06    fixed .NET MF support
 * 
 * 
 */
using System;
using System.IO.Ports;
using System.Threading;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a XBee module (Sample Implementation)
    /// </summary>
    public class XBeeModule : XBee
    {
        #region Constructor

        public XBeeModule(string port)
            : base(port)
        {
        }

        public XBeeModule(string port, int baudRate)
            : base(port, baudRate)
        {
        }

        public XBeeModule(string port, int baudRate, ApiType apiType)
            : base(port, baudRate, apiType)
        {
        }

        #endregion

        /// <summary>
        /// Enter the AT command mode.
        /// </summary>
        /// <returns>Returns true if entered command mode; otherwise false.</returns>
        public bool EnterCommandMode()
        {
            if (ApiType == ApiType.Disabled)
            {
                WriteCommand("+++");
                Thread.Sleep(1025);
                if (ReadResponse() == "OK")
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Reads the node identifier from the module.
        /// </summary>
        /// <returns>A string containing the node identifier; otherwise null.</returns>
        public string GetNodeIdentifier()
        {
            AtCommandResponse res = Execute(new NodeIdentifierCommand()) as AtCommandResponse;

            if (res == null)
                throw new Exception("Could not execute NodeIdentifier command.");

            NodeIdentifier ni = NodeIdentifier.Parse(res);

            if (ni == null)
                throw new Exception("Could not parse response as NodeIdentifier.");

            return ni.Identifier;
        }

        /// <summary>
        /// Writes the node identifier <paramref name="identifier"/>.
        /// </summary>
        /// <param name="identifier">The node identifier string to write.</param>
        /// <returns>Returns true if sucessful; otherwise false.</returns>
        public bool SetNodeIdentifier(string identifier)
        {
            Execute(new NodeIdentifierCommand(identifier));
            return true;
        }

        /// <summary>
        /// Exit command mode in AT mode.
        /// </summary>
        /// <returns>Returns true if sucessful; otherwise false.</returns>
        public bool ExitCommandMode()
        {
            if (ApiType == ApiType.Disabled)
                WriteCommand("ATCN\r");

            return true;
        }
        
        /// <summary>
        /// Writes the configuration / changes to nonvolatile memory.
        /// </summary>
        /// <returns>Returns true if sucessful; otherwise false.</returns>
        public bool WriteStateToMemory()
        {
            Execute(new WriteCommand());
            return true;
        }
    }
}
