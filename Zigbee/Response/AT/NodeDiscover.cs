/* 
 * NodeDiscoverData.cs
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
 * 
 */
using MFToolkit.IO;
using System;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a node discover command response structure
    /// </summary>
    public class NodeDiscover : IAtCommandResponseData
    {
        private const byte terminationCharacter = 0x00;

        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private byte _signalStrength;
        private string _ni;

        #region Public Properties

        /// <summary>
        /// Serial Number (SH SL)
        /// </summary>
        public XBeeAddress64 SerialNumber
        {
            get { return _address64; }
        }

        /// <summary>
        /// Short Address (MY)
        /// </summary>
        public XBeeAddress16 ShortAddress
        {
            get { return _address16; }
        }

        /// <summary>
        /// Signal Strength (db)
        /// </summary>
        public byte SignalStrength
        {
            get { return _signalStrength; }
        }

        /// <summary>
        /// Node Identifier (NI)
        /// </summary>
        public string NodeIdentifier
        {
            get { return _ni; }
        }

        #endregion

        public static NodeDiscover Parse(IAtCommandResponse cmd)
        {
            if (cmd.Command != NodeDiscoverCommand.command)
                throw new ArgumentException("This method is only applicable for the '" + NodeDiscoverCommand.command + "' command!", "cmd");

            ByteReader br = new ByteReader(cmd.Value, ByteOrder.BigEndian);

            NodeDiscover nd = new NodeDiscover();
            nd.ReadBytes(br);

            return nd;
        }

        public void ReadBytes(ByteReader br)
        {
            _address16 = XBeeAddress16.ReadBytes(br);
            _address64 = XBeeAddress64.ReadBytes(br);
            _signalStrength = br.ReadByte();
            _ni = br.ReadString(terminationCharacter);
        }

        public override string ToString()
        {
            string s = "";

            s += "SerialNumber = " + SerialNumber + "\r\n";
            s += "ShortAddress = " + ShortAddress + "\r\n";
            s += "SignalStrength = " + SignalStrength + "\r\n";
            s += "NodeIdentifier = " + NodeIdentifier;
            
            return s;
        }
    }
}