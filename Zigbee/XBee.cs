/* 
 * XBee.cs
 * 
 * Copyright (c) 2008, Michael Schwarz (http://www.schwarz-interactive.de)
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
using System.Threading;
using System.IO.Ports;
using System.IO;
using MSchwarz.IO;

namespace MSchwarz.Net.Zigbee
{
    public class XBee : IDisposable
    {
        private string _port;
        private int _baudRate = 9600;
        private SerialPort _serialPort;
		private MemoryStream _readBuffer = new MemoryStream();
        
        public XBee()
        {

        }

        public XBee(string port)
            : this()
        {
            _port = port;
        }

        public XBee(string port, int baudRate)
            : this(port)
        {
            _baudRate = baudRate;
        }

        public bool Open(string port, int baudRate)
        {
            _port = port;
            _baudRate = baudRate;

            return Open();
        }

        public bool Open()
        {
            try
            {
                if (_serialPort == null)
                    _serialPort = new SerialPort(_port, _baudRate);

                if (!_serialPort.IsOpen)
                {
                    _serialPort.ReadTimeout = 2000;
                    _serialPort.WriteTimeout = 2000;

                    _serialPort.Open();
                    
					//_serialPort.Write("x");     // not sure if this is a bug or not, but if writing any character on init speeds up first command
                }

#if(!MF)
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
#endif
            
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

#if(!MF)
        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = _serialPort.BytesToRead;
            while(count > 0)
            {
                //Console.WriteLine("found " + count + " bytes...");

                byte[] bytes = new byte[count];
                int readBytes = _serialPort.Read(bytes, 0, count);

                for (int i = 0; i < readBytes; i++)
                    _readBuffer.WriteByte(bytes[i]);

                count = _serialPort.BytesToRead;
            }


            System.Collections.Generic.List<byte> x = new System.Collections.Generic.List<byte>();

            for(int i=0; i<_readBuffer.Length; i++)
            {
				byte b = (byte)_readBuffer.ReadByte();

                //if (XBeePacket.IsSpecialByte(b))
                //{
                //    if (b == XBeePacket.PACKET_STARTBYTE)
                //    {
                //        x.Add(b);
                //    }
                //    else if (b == XBeePacket.PACKET_ESCAPE)
                //    {
                //        b = _readBuffer[++i];
                //        x.Add((byte)(0x20 ^ b));
                //    }
                //    //else throw new Exception("");
                //}
                //else
                    x.Add(b);
            }

			_readBuffer = new MemoryStream(x.Count);
			_readBuffer.Write(x.ToArray(), 0, x.Count);

            //Console.WriteLine("Received:\r\n" + ByteUtil.PrintBytes(_readBuffer.ToArray()));

            CheckFrame();
        }

#endif

        void CheckFrame()
        {
            if (_readBuffer.Length < 4) // we don't have the start byte, the length and the checksum
                return;

            if (_readBuffer.ReadByte() != XBeePacket.PACKET_STARTBYTE)
                return;

            ByteReader br = new ByteReader(_readBuffer.ToArray(), ByteOrder.BigEndian);
            br.ReadByte();      // start byte

            short length = br.ReadInt16();
            if (br.AvailableBytes < length +1) // the frame data and checksum
            {
                return;
            }

            // verify checksum
            XBeeChecksum checksum = new XBeeChecksum();
            byte[] bytes = new byte[length +1];
            Array.Copy(_readBuffer.ToArray(), 3, bytes, 0, length +1);
            checksum.AddBytes(bytes);

            XBeeApiType apiId = (XBeeApiType)br.Peek();
            XBeeResponse res = null;

            switch (apiId)
            {
                case XBeeApiType.ATCommandResponse:
                    res = new AtCommandResponse(length, br);
                    break;
                case XBeeApiType.NodeIdentificationIndicator:
                    res = new NodeIdentification(length, br);
                    break;
                case XBeeApiType.ZigBeeReceivePacket:
                    res = new ZigbeeReceivePacket(length, br);
                    break;
                case XBeeApiType.XBeeSensorReadIndicator:
                    res = new XBeeSensorRead(length, br);
                    break;
                case XBeeApiType.RemoteCommandResponse:
                    res = new AtRemoteCommandResponse(length, br);
                    break;
                case XBeeApiType.ZigBeeIODataSampleRxIndicator:
                    res = new ZigBeeIODataSample(length, br);
                    break;
                default:
                    break;
            }

			//_readBuffer.RemoveRange(0, length +1 +2 +1);
		}

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
        }

        public bool SendPacket(XBeePacket packet)
        {
            byte[] bytes = packet.GetBytes();

            _serialPort.Write(bytes, 0, bytes.Length);

            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();

            if(_serialPort != null)
            {
                _serialPort.Dispose();
                _serialPort = null;
            }
        }

        #endregion
    }
}
