/* 
 * AtCommandResponse.cs
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
 * MS   09-02-07    added support for Windows CE
 * 
 * 
 */
using System;
using System.Text;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
    /// <summary>
    /// Represents a AT command response
    /// </summary>
    public class AtCommandResponse : XBeeFrameResponse
    {
        private string _command;
        private byte _status;
        private byte[] _value;
        
        #region Public Properties

        public string Command
        {
            get { return _command; }
        }

        public AtCommandStatus Status
        {
            get { return (AtCommandStatus)_status; }
        }

        public byte[] Value
        {
            get { return _value; }
        }

        #endregion

        public AtCommandResponse(short length, ByteReader br)
            : base(length, br)
        {
#if(MF)
			_command = ByteUtil.GetString(br.ReadBytes(2));
#elif(WindowsCE)
            byte[] tempArr = br.ReadBytes(2);
            _command = Encoding.ASCII.GetString(tempArr, 0, tempArr.Length);
#else
            _command = Encoding.ASCII.GetString(br.ReadBytes(2));
#endif

            _status = br.ReadByte();

            if (br.AvailableBytes > 0)
                _value = br.ReadBytes(length - 5);
        }

        public IAtCommandData ParseValue()
        {
            return ParseValue(Command, Value);
        }

#if(!MF && !WindowsCE)
        public T ParseValue<T>() where T : IAtCommandData
        {
            return (T)ParseValue(Command, Value);
        }
#endif

        internal static IAtCommandData ParseValue(string command, byte[] value)
        {
            if (value == null || value.Length == 0)
                return null;

            IAtCommandData data = null;

            switch (command)
            {
                case "DB": data = new ReceivedSignalStrengthData(); break;
                case "IS": data = new ForceSampleData(); break;
                case "ND": data = new NodeDiscoverData(); break;
                case "NI": data = new NodeIdentifierData(); break;
                case "SM": data = new SleepModeData(); break;
                case "SP": data = new CyclicSleepPeriodData(); break;
                case "ST": data = new TimeBeforeSleepData(); break;
                case "%V": data = new SupplyVoltageData(); break;
                case "AP": data = new ApiEnableData(); break;
            }

            if (data != null)
            {
                using (ByteReader br = new ByteReader(value, ByteOrder.BigEndian))
                {
                    data.ReadBytes(br);
                }
            }

            return data;
        }

        public override string ToString()
        {
            string s = "command " + Command + "\r\n" + "status  " + Status;
            s += "\r\nvalue = \r\n" + ParseValue();

            return s;
        }
    }
}
