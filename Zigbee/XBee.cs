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
 * BL   09-01-27    fixed MicroZigbee build
 * MS   09-02-06    fixed work item 3636 when first character is not the startbyte
 * 
 */
using System;
using System.Text;
using System.Threading;
using System.IO.Ports;
using System.IO;
using MSchwarz.IO;

namespace MSchwarz.Net.XBee
{
    public class XBee : IDisposable
    {
        private string _port;
        private int _baudRate = 9600;
        private SerialPort _serialPort;
		private MemoryStream _readBuffer = new MemoryStream();
		private ApiType _apiType = ApiType.Unknown;
		
		private byte _frameID = 0x00;
		private bool _waitResponse = false;
		private XBeeResponse _receivedPacket = null;

		public delegate void PacketReceivedHandler(XBee sender, XBeeResponse response);
		public event PacketReceivedHandler OnPacketReceived;

        public XBee(string port)
        {
            _port = port;
        }

        public XBee(string port, int baudRate)
            : this(port)
        {
            _baudRate = baudRate;
        }

		public XBee(string port, ApiType apiType)
			: this(port)
		{
			_apiType = apiType;
		}

		public XBee(string port, int baudRate, ApiType apiType)
			: this(port, baudRate)
		{
			_apiType = apiType;
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
				}
			}
			catch (Exception)
			{
				return false;
			}

			if (_apiType == ApiType.Unknown)
			{
				#region Detecting API or transparent AT mode

				try
				{
					if (EnterCommandMode())
						ExitCommandMode();

					// we need some msecs to wait before calling another EnterCommandMode
					Thread.Sleep(1000);

					_apiType = ApiType.Disabled;
				}
				catch (Exception)
				{
					// seems that we are using API

					Thread thd = new Thread(new ThreadStart(this.ReceiveData));
					thd.Start();

					AtCommandResponse at = SendCommand(new ApiEnable()) as AtCommandResponse;

					_apiType = (at.Data as ApiEnableData).ApiType;
				}

				#endregion
			}
			else if (_apiType == ApiType.Enabled || _apiType == ApiType.EnabledWithEscaped)
			{
				Thread thd = new Thread(new ThreadStart(this.ReceiveData));
#if !MF
                thd.Name = "Receive Data Thread";
                thd.IsBackground = true;
#endif
				thd.Start();
			}

			if (_apiType == ApiType.Unknown)
				throw new NotSupportedException("The API type could not be read or is configured wrong.");

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
					byte[] bytes = new byte[1024];	// TODO: what is the maximum size of Zigbee packets?

					if (_serialPort == null || !_serialPort.IsOpen)
					{
						if (_serialPort == null)
							_serialPort = new SerialPort(_port, _baudRate);

						_serialPort.Open();

						bytesToRead = _serialPort.BytesToRead;
						_readBuffer.SetLength(0);
						continue;
					}

					try
					{
						int bytesRead = _serialPort.Read(bytes, 0, bytesToRead);

						for (int i = 0; i < bytesRead; i++)
						{
							if (_apiType == ApiType.EnabledWithEscaped && XBeePacket.IsSpecialByte(bytes[i]))
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
                                    //TODO: verify checksum

                                    XBeeChecksum checksum = new XBeeChecksum();
                                    for (int i = 0; i < length; i++)
                                    {
                                        checksum.AddByte(bytes[i + 3]);
                                    }

                                    checksum.Compute();

#if(DEBUG && !MF)
                                    Console.WriteLine("Received " + ByteUtil.PrintBytes(bytes));
#endif
                                    if (checksum.Verify(bytes[length + 3]))
                                        CheckFrame(length, br);

                                    if (bytes.Length - (1 + 2 + length + 1) > 0)
                                        _readBuffer.Write(bytes, 1 + 2 + length + 1, bytes.Length - (1 + 2 + length + 1));
                                }
                                else
                                {
                                    _readBuffer.Write(bytes, 0, bytes.Length);
                                }
                            }
                            else
                            {
                                int discardCount = 0;
                                _readBuffer.Position = 0;

                                while ((byte)_readBuffer.ReadByte() != XBeePacket.PACKET_STARTBYTE 
                                    && (_readBuffer.Position < _readBuffer.Length))
                                {
                                    discardCount++;
                                }

                                if (discardCount > 0)
                                {
                                    bytes = _readBuffer.ToArray();

                                    _readBuffer.SetLength(0);
                                    _readBuffer.Write(bytes, discardCount, bytes.Length - discardCount);
                                }
                            }
						}
					}
					catch (Exception)
					{
						_readBuffer.SetLength(0);

						if (_serialPort != null && _serialPort.IsOpen)
						{
							bytesToRead = _serialPort.BytesToRead;
						}
					}
				}

				if (_serialPort != null && _serialPort.IsOpen)
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

			if (res != null)
			{
				if (_waitResponse && res is AtCommandResponse && (res as AtCommandResponse).FrameID == _frameID)
				{
					_receivedPacket = res;
					_waitResponse = false;
				}

				if (OnPacketReceived != null)
					OnPacketReceived(this, res);
			}
		}

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();
        }

        public bool SendPacket(XBeePacket packet)
        {
            byte[] bytes;

			if (_apiType == ApiType.EnabledWithEscaped)
				bytes = packet.GetEscapedBytes();
			else
				bytes = packet.GetBytes();

#if(DEBUG && !MF)
			Console.WriteLine("Sending " + ByteUtil.PrintBytes(bytes) + "...");
#endif

			_serialPort.Write(bytes, 0, bytes.Length);

            return true;
		}

		public XBeeResponse SendCommand(AtCommand cmd)
		{
			cmd.FrameID = ++_frameID;
			_waitResponse = true;

			SendPacket(cmd.GetPacket());

			int c = 0;

			while (_waitResponse && ++c < 300)
			{
				Thread.Sleep(10);
			}

			if (c >= 300)
			{
#if(MF)
				throw new Exception("Could not receive response.");
#else
				throw new TimeoutException("Could not receive response.");
#endif
			}

			if (!_waitResponse)
			{
				return _receivedPacket;
			}

			return null;
		}

		public void SendCommand(string s)
		{
#if(!MF)
			Console.WriteLine(s);
#endif
			byte[] bytes = Encoding.UTF8.GetBytes(s + "\r");
			_serialPort.Write(bytes, 0, bytes.Length);
		}

		private string ReadTo(string value)
		{
			string textArrived = string.Empty;

			if (value == null)
				throw new ArgumentNullException();

			if (value.Length == 0)
				throw new ArgumentException();

			byte[] byteValue = Encoding.UTF8.GetBytes(value);
			int byteValueLen = byteValue.Length;

			bool flag = false;
			byte[] buffer = new byte[byteValueLen];

			do
			{
				int bytesRead;
				int bufferIndex = 0;
				Array.Clear(buffer, 0, buffer.Length);

				do
				{
					bytesRead = _serialPort.Read(buffer, bufferIndex, 1);
					bufferIndex += bytesRead;

					if (bytesRead <= 0)
					{
						return null;
					}

				}
				while ((bufferIndex < byteValueLen)
						&& (buffer[bufferIndex - 1] != byteValue[byteValueLen - 1]));

				char[] charData = Encoding.UTF8.GetChars(buffer);

				for (int i = 0; i < charData.Length; i++)
					textArrived += charData[i];

				flag = true;

				if (textArrived.Length > 0)
				{
					for (int i = 1; i <= value.Length; i++)
					{
						if (value[value.Length - i] != textArrived[textArrived.Length - i])
						{
							flag = false;
							break;
						}
					}
				}

			} while (!flag);

			if (textArrived.Length >= value.Length)
				textArrived = textArrived.Substring(0, textArrived.Length - value.Length);

			return textArrived;
		}

		public string GetResponse()
		{
			string s = ReadTo("\r");

#if(!MF)
			Console.WriteLine(s);
#endif

			return s;
		}

		private bool StrEndWith(string data, string pattern)
		{
			int dataLen = data.Length;
			int patLen = pattern.Length;

			if (dataLen < patLen || data.Substring(dataLen - patLen) != pattern)
				return false;
			else
				return true;
		}

		#region XBee ZNet 2.5 Commands

		public bool EnterCommandMode()
		{
			if (_apiType != ApiType.Disabled && _apiType != ApiType.Unknown)
				throw new NotSupportedException("While using API mode entering command mode is not available.");

			SendCommand("+++");

			Thread.Sleep(2500);

			return GetResponse() == "OK";
		}

		public bool ExitCommandMode()
		{
			if (_apiType != ApiType.Disabled)
				throw new NotSupportedException("While using API mode entering command mode is not available.");

			SendCommand("ATCN");

			return GetResponse() == "OK";
		}

		public bool NetworkReset()
		{
			if (_apiType == ApiType.Disabled)
			{
				SendCommand("ATNR");
				return GetResponse() == "OK";
			}
			else
			{
				AtCommandResponse res = SendCommand(new NetworkReset()) as AtCommandResponse;

				if (res == null)
					return false;

				return (res.Status == AtCommandStatus.Ok);
			}
		}

		public void SetApiMode(ApiType apiType)
		{
			switch (apiType)
			{
				case ApiType.Disabled:
					if (_apiType == ApiType.Enabled || _apiType == ApiType.EnabledWithEscaped)
					{
						AtCommand at = new ApiEnable(apiType);
						at.FrameID = ++_frameID;

						SendPacket(at.GetPacket());

						while (true)
						{
						}
					}
					break;

				case ApiType.Enabled:
					if (_apiType == ApiType.Disabled)
					{
						

					}
					break;
			}
		}

		public bool SetNodeIdentifier(string identifier)
		{
			switch (_apiType)
			{
				case ApiType.Disabled:

					//if (!EnterCommandMode())
					//    return false;

					SendCommand("ATNI" + identifier);
					bool res = GetResponse() == "OK";

					if (res)
					{
						SendCommand("ATWR");
						res = GetResponse() == "OK";
					}

					//ExitCommandMode();

					return res;

				case ApiType.Enabled:
				case ApiType.EnabledWithEscaped:

					AtCommandResponse atres = SendCommand(new NodeIdentifier(identifier)) as AtCommandResponse;

					if (atres == null)
						return false;

					return (atres.Status == AtCommandStatus.Ok);
			}

			return false;
		}

		#endregion

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
