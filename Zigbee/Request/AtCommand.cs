/* 
 * AtCommand.cs
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
/*
 * MS	08-11-10	changed _value to protected
 * MS   10-11-08    fixed missing Value set in ctors (work item 4218)
 * 
 * 
 */
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Allows for module parameter registers to be queried or set.
    /// </summary>
    public class AtCommand : XBeeFrameRequest
    {
        private string _command;
        private byte[] _value;

        #region Public Properties

        public string Command
        {
            get { return _command; }
            set
            {
				if (value == null || value.Length == 0)
                    throw new NullReferenceException("The command cannot be null or empty.");

                if (value.Length < 2)
                    throw new ArgumentException("The command must have at least 2 characters.", "value");

                _command = value;
            }
        }

        public byte[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #region Constructor

        public AtCommand(string command)
            : this(command, new byte[0])
        {
        }

        public AtCommand(string command, byte value)
            : this(command, new byte[] { value })
        {
        }

        public AtCommand(string command, byte[] value)
        {
            this.ApiID = XBeeApiType.AtCommand;
            this.Command = command;
            this.Value = value;
        }

        #endregion

        internal virtual void WriteBytesCommand(ByteWriter bw)
        {
            bw.Write(Command);

            if (Value != null)
                bw.Write(Value);
        }

        internal override void WriteApiBytes(ByteWriter bw)
        {
            base.WriteApiBytes(bw);

            WriteBytesCommand(bw);
        }

        internal override void WriteAtBytes(ByteWriter bw)
        {
            bw.Write(new byte[] { 0x41, 0x54 });        // "AT" prefix
            bw.Write(Command);

            if (Value != null)
                bw.Write(Value);

            bw.Write("\r");
        }

        public override string ToString()
        {
            return base.ToString() + "\r\nCommand = " + Command;
        }
    }
}
