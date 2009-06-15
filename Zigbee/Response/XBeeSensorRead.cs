/* 
 * XBeeSensorRead.cs
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
	// TODO: not sure if this is the correct implementation

    /// <summary>
    /// Represents a XBee sensor read response
    /// </summary>
    public class XBeeSensorRead : XBeeResponse
    {
        private XBeeAddress64 _address64;
        private XBeeAddress16 _address16;
        private byte _options;
        private byte _sensors;
        private ushort _sensorA;
        private ushort _sensorB;
        private ushort _sensorC;
        private ushort _sensorD;
        private ushort _temperature;

        #region Public Properties

        /// <summary>
        /// Serial Number
        /// </summary>
        public XBeeAddress64 SerialNumber
        {
            get { return _address64; }
        }

        /// <summary>
        /// Short Address
        /// </summary>
        public XBeeAddress16 ShortAddress
        {
            get { return _address16; }
        }

        public ReceiveOptionType ReceiveOption
        {
            get { return (ReceiveOptionType)_options; }
        }

        /// <summary>
        /// XBee Sensors
        /// </summary>
        public byte Sensors
        {
            get { return _sensors; }
        }

        public bool IsAnalogDigitalRead
        {
            get { return BitHelper.GetBit(Sensors, 1); }
        }

        public bool IsTemperaturRead
        {
            get { return BitHelper.GetBit(Sensors, 2); }
        }

        public bool IsWaterPresent
        {
            get { return BitHelper.GetBit(Sensors, 128); }
        }

        public ushort SensorA
        {
            get { return _sensorA; }
        }

        public ushort SensorB
        {
            get { return _sensorB; }
        }

        public ushort SensorC
        {
            get { return _sensorC; }
        }

        public ushort SensorD
        {
            get { return _sensorD; }
        }

        public ushort Temperature
        {
            get { return _temperature; }
        }

        #endregion

        public XBeeSensorRead(short length, ByteReader br)
            : base(length, br)
        {
            _address64 = XBeeAddress64.ReadBytes(br);
            _address16 = XBeeAddress16.ReadBytes(br);

            _options = br.ReadByte();

            _sensors = br.ReadByte();

            _sensorA = br.ReadUInt16();
            _sensorB = br.ReadUInt16();
            _sensorC = br.ReadUInt16();
            _sensorD = br.ReadUInt16();

            _temperature = br.ReadUInt16();
        }

		public override string ToString()
		{
            string s = base.ToString() + "\r\n";
            
            s += "Sensor A = " + SensorA + "\r\n";
			s += "Sensor B = " + SensorB + "\r\n";
			s += "Sensor C = " + SensorC + "\r\n";
            s += "Sensor D = " + SensorD + "\r\n";
            s += "Temperature = " + Temperature;

			return s;
		}
    }
}
