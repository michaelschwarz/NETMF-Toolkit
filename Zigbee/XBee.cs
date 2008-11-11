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
/*
 * MS	08-11-10	changed how data reading is working
 * 
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

		private bool _isEscapeEnabled = false;

		public delegate void PacketReceivedHandler(XBee sender, XBeeResponse response);
		public event PacketReceivedHandler OnPacketReceived;

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

				Thread thd = new Thread(new ThreadStart(this.ReceiveData));
				thd.Start();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

		void ReceiveData()
		{
			int bytesToRead = _serialPort.BytesToRead;

			while (true)
			{
				if (bytesToRead == 0)
				{
					Thread.Sleep(20);
				}
				else
				{
					byte[] bytes = new byte[1024];		// TODO: what is the maximum size of Zigbee packets?

					if (_serialPort == null || !_serialPort.IsOpen)
						return;		// TODO: what?

					int bytesRead = _serialPort.Read(bytes, 0, bytesToRead);

					for (int i = 0; i < bytesRead; i++)
					{
						if (_isEscapeEnabled && XBeePacket.IsSpecialByte(bytes[i]))
						{
							if (bytes[i] == XBeePacket.PACKET_STARTBYTE)
							{
								_readBuffer.WriteByte(bytes[i]);
							}
							else if (bytes[i] == XBeePacket.PACKET_ESCAPE)
							{
								_readBuffer.WriteByte((byte)(0x20 ^ bytes[++i]));
							}
							else
								throw new Exception("This special byte should not appear.");
						}
						else
							_readBuffer.WriteByte(bytes[i]);
					}

					if (_readBuffer.Length > 4)
					{
						_readBuffer.Position = 0;
						if ((byte)_readBuffer.ReadByte() == XBeePacket.PACKET_STARTBYTE)
						{
							bytes = _readBuffer.ToArray();
							_readBuffer.SetLength(0);

							ByteReader br = new ByteReader(bytes, ByteOrder.BigEndian);

							byte startByte = br.ReadByte();	// start byte
							short length = br.ReadInt16();

							if (br.AvailableBytes > length)
							{
								// verify checksum
								CheckFrame(length, br);

								if (bytes.Length - (1 + 2 + length + 1) > 0)
								{
									_readBuffer.Write(bytes, 1 + 2 + length + 1, bytes.Length - (1 + 2 + length + 1));
								}
							}
							else
							{
								_readBuffer.Write(bytes, 0, bytes.Length);
							}
						}
					}
				}

				if(_serialPort != null && _serialPort.IsOpen)
					bytesToRead = _serialPort.BytesToRead;
			}
		}

		void CheckFrame(short length, ByteReader br)
        {
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
					res = new ZigBeeReceivePacket(length, br);
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
				case XBeeApiType.ZigBeeTransmitStatus:
					res = new ZigBeeTransmitStatus(length, br);
					break;
				default:
#if(!MF)
					Console.WriteLine("Could not handle API message " + apiId + ".");
#endif
					break;
            }

			if (res != null && OnPacketReceived != null)
				OnPacketReceived(this, res);
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
