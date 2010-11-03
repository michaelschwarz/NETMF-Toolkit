/* 
 * C6820.cs
 * 
 * Copyright (c) 2009, Freesc Huang (http://www.microframework.cn)
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
 * MS   09-03-13    initial version
 * 
 * 
 * 
 */
using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT;

namespace MFToolkit.MicroC6820
{
    /// <summary>
    /// C6820 camera driver for .NET Micro Framework.
    /// C6820 - recording module Manufacturer: COMedia Ltd. http://www.comedia.com.hk/
    /// 
    /// Driver Author: Freesc Huang http://fox23.cnblogs.com
    /// 
    /// This code was written by Freesc Huang. It is released under the terms of 
    /// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
    /// http://creativecommons.org/licenses/by-nc-sa/2.5/
    /// </summary>
    public class C6820
    {
        #region Commented area
        // /// <summary>
        // /// Size of the data package with image
        // /// </summary>
        //private const int PACKAGE_SIZE = 512;

        // /// <summary>
        // /// MASK for bit-convertion
        // /// </summary>
        //private const byte MASK = 0xff;

        //private bool mode;
        //public bool Mode
        //{
        //    get { return mode; }
        //}
        #endregion

        #region Constructor

        public C6820 (string portName, int baudRate)
        {
            serialPort = new SerialPort(portName, baudRate);
        }

        #endregion

        #region Private Fields
       
        private SerialPort serialPort;

        private byte[] id_cmd = new byte[5];
        private byte[] para_cmd;

        // Commands
        const byte CMD_SYNCPREFIX = 0xaa;       // Synchronization byte
        const byte CMD_RESTORE = 0x00;
        const byte CMD_SHUTDOWN = 0x01;
        const byte CMD_REQUEST_HWREVISIONID = 0x02;
        const byte CMD_SETSYSCLK = 0x03;
        const byte CMD_REQUEST_SYSTIME = 0x04;

        const byte CMD_SNAPSHOT_CONFIG = 0x06;
        const byte CMD_SELECT_OPMODE = 0x1e;
        const byte CMD_REQUEST_OPMODE = 0x1f;
        const byte CMD_JPEG_RESOLUTION = 0x32;
        const byte CMD_REQUEST_LUMINANCE = 0x33;
        const byte CMD_DATETIME_STAMP = 0x35;
        const byte CMD_STRING_STAMP = 0x36;
        const byte CMD_SEQUENCE_CAPTURE = 0x38;
        const byte CMD_AVI_RESOLUTION = 0x51;
        const byte CMD_AVI_START_STOP = 0x54;
        const byte CMD_SELECT_STORAGEMEDIA = 0x64;
        const byte CMD_FORMAT_STORAGEMEDIA = 0x65;
        const byte CMD_REQUEST_STORAGEINFO = 0x66;
        const byte CMD_REQUEST_FILEINFO = 0x78;
        const byte CMD_DOWNLOADFILE = 0x79;
        const byte CMD_DELETEFILE = 0x7a;
        const byte CMD_SELECT_TVSTANDARD = 0x9b;
        const byte CMD_SELECT_BAUDRATE = 0x9f;
        const byte CMD_REQUEST_CONNECTSTATUS = 0xa9;
        const byte CMD_SYNC_SIGNAL = 0xb0;
        const byte CMD_PLAYBACK_CURRENTAVI = 0xc8;
        const byte CMD_SELECT_PRE_NEXT = 0xc9;
        const byte CMD_SELECT_PARTICULAR_FILE = 0xca;
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Creat ID Command
        /// </summary>
        /// <param name="paraLength">Length of parameter</param>
        /// <param name="commandid">ID of Command(see the C6820 spec)</param>
        /// <returns>Command</returns>
        private byte[] CreatCommand(byte paraLength, byte commandId)
        {
            id_cmd[0] = CMD_SYNCPREFIX;
            id_cmd[1] = paraLength;
            id_cmd[2] = commandId;
            id_cmd[4] = CMD_SYNCPREFIX;
            unchecked
            {
                //checksum,lowest 8bit
                id_cmd[3] = (byte)(id_cmd[0] + id_cmd[1] + id_cmd[2] + id_cmd[4]);
            }
            return id_cmd;
        }

        /// <summary>
        /// Creat parameter command
        /// </summary>
        /// <param name="commandPara">Command parameter to</param>
        /// <returns></returns>
        private byte[] CreatCommand(byte[] commandPara)
        {
            int chksum = 0;
            int cmdLength = commandPara.Length + 3;
            for (int i = 0; i < commandPara.Length; i++)
            {
                chksum += commandPara[i];
            }
            chksum += (2 * CMD_SYNCPREFIX);
            para_cmd = new byte[cmdLength];
            para_cmd[0] = CMD_SYNCPREFIX;
            commandPara.CopyTo(para_cmd, 1);
            para_cmd[cmdLength - 1] = CMD_SYNCPREFIX;
            unchecked
            {
                para_cmd[cmdLength - 2] = (byte)chksum;
            }
            return para_cmd;
        }

        /// <summary>
        /// Send generic command
        /// </summary>
        /// <param name="commandArray">Byte array with command and arguments</param>
        /// <returns>True if succeeded</returns>
        private bool SendCommand(byte[] commandArray)
        {
            int len = commandArray.Length;
            int send = serialPort.Write(commandArray, 0, len);
            Thread.Sleep(10);
            return send == len;
        }

        /// <summary>
        /// Waits for response from camera
        /// </summary>
        /// <param name="readBuffer">Buffer for response</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>False if timeout occured</returns>
        private bool WaitForResponse(ref byte[] readBuffer, int timeout)
        {
            serialPort.ReadTimeout = timeout;

            int recv = serialPort.Read(readBuffer, 0, readBuffer.Length);
            return recv != 0;
        }

        /// <summary>
        /// Waits for 6 bytes response and parse it for ACK
        /// </summary>
        /// <param name="expectedACKCommand">Command to be ACKnowlegde</param>
        /// <param name="timeout">Timeout in miliseconds</param>
        /// <returns>True if ACK for expected command was received</returns>
        private bool ReceiveACK(byte expectedACKCommand, int timeout)
        {
            byte[] responseBuffer = new byte[6];
            bool stat = WaitForResponse(ref responseBuffer, timeout);

            // If no ACK or ACK for different command received - return false
            if (!stat || responseBuffer[2] != expectedACKCommand)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Waits for 6 bytes response and parse it for ACK
        /// </summary>
        /// <param name="expectedACKCommand">Command to be ACKnowlegde</param>
        /// <param name="timeout">Timeout in miliseconds</param>
        /// <param name="response">the return byte</param>
        /// <returns>True if ACK for expected command was received</returns>
        private bool ReceiveACK(byte expectedACKCommand, int timeout, ref byte returnValue)
        {
            byte[] responseBuffer = new byte[6];
            bool stat = WaitForResponse(ref responseBuffer, timeout);

            // If no ACK or ACK for different command received - return false
            if (!stat || responseBuffer[2] != expectedACKCommand)
            {
                return false;
            }
            
            returnValue = responseBuffer[3];
            return true;
        }

        /// <summary>
        /// cast a string into an ASCII byte array
        /// </summary>
        /// <param name="s">string to cast</param>
        /// <returns>the result byte array</returns>
        private byte[] StringToByteArray(string s)
        {
            byte[] result = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                result[i] = (byte)(s.ToCharArray()[i]);
            }
            return result;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sync with the camera
        /// </summary>
        /// <returns>True if succeed</returns>
        public bool Sync()
        {
            // Create 'Sync' command
            CreatCommand(0, CMD_SYNC_SIGNAL);

            byte[] recvCommand = new byte[6];
            int i = 0;
            bool stat = false;

            while (true)
            {
                i++;
                if (i > 60)
                {
                    stat = false;
                    break;
                }
                stat = SendCommand(id_cmd);

                // Wait for SYNC                
                stat = WaitForResponse(ref recvCommand, 100);
                if (!stat || recvCommand[1] != 0x01 || recvCommand[4] != 0x05 || recvCommand[2] != CMD_SYNC_SIGNAL)
                {
                    continue;
                }
                break;
            }
            return stat;
        }

        /// <summary>
        /// Set communicatin speed that will be used by camera until physically power off.
        /// </summary>
        /// <param name="baudRate">Baudrate</param>
        /// <returns>True if succeeded</returns>
        public bool SetBaudRate(C6820BaudRate baudRate)
        {
            byte[] recvCommand = new byte[6];
            byte bRate = (byte)baudRate;
            CreatCommand(1, CMD_SELECT_BAUDRATE);
            CreatCommand(new byte[] { bRate });
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            // Receive ACK
            if (!ReceiveACK(CMD_SELECT_BAUDRATE, 100))
            {
#if(DEBUG)
                Debug.Print("Failed to set baudrate");
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        /// set camera operation mode
        /// </summary>
        /// <param name="operationMode">camera opertation mode</param>
        /// <returns>Ture if succeed</returns>
        public bool SetOperationMode(OperationMode operationMode)
        {
            byte returnValue = 255;
            byte opMode = (byte)operationMode;
            CreatCommand(1, CMD_SELECT_OPMODE);
            CreatCommand(new byte[] { opMode });
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            // Receive ACK
            if (!ReceiveACK(CMD_SELECT_OPMODE, 500, ref returnValue) || returnValue != 0)
            {
#if(DEBUG)
                Debug.Print("Failed to set op mode");
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        /// Restore the camera to factory configuration
        /// </summary>
        /// <returns>Ture if succeed</returns>
        public bool Reset()
        {
            CreatCommand(0, CMD_RESTORE);
            SendCommand(id_cmd);

            if (!ReceiveACK(CMD_RESTORE, 100))
            {
#if(DEBUG)
                Debug.Print("Failed to reset");
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        /// Shut down camera
        /// </summary>
        /// <returns>True if success</returns>
        public bool ShutDown()
        {
            CreatCommand(0, CMD_SHUTDOWN);
            SendCommand(id_cmd);

            if (!ReceiveACK(CMD_SHUTDOWN, 100))
            {
#if(DEBUG)
                Debug.Print("Failed to shutdown");
#endif
                return false;
            }
            return true;
        }

        /// <summary>
        /// Capture still image in jpeg  
        /// </summary>
        /// <param name="n">number of images , from 1 to 255</param>
        /// <returns>Ture if succeed</returns>
        public bool CaptureJpeg(byte n)
        {
            byte returnValue = 255;
            if (GetOperationMode() != OperationMode.JpegCap)
            {
                Thread.Sleep(500);
                if (!SetOperationMode(OperationMode.JpegCap))
                {
                    CaptureJpeg(n);
                }

#if(DEBUG)
                Debug.Print("Set Op in capturejpeg OK");
#endif
            }

            Thread.Sleep(500);

            CreatCommand(1, CMD_SEQUENCE_CAPTURE);
            CreatCommand(new byte[] { n });

            Thread.Sleep(500);

#if(DEBUG)
            Debug.Print(SendCommand(id_cmd).ToString());
#else
            SendCommand(id_cmd).ToString();
#endif

            /*
             * here breakpoint tells the problem 
             */

#if(DEBUG)
            Debug.Print(SendCommand(para_cmd).ToString());
#else
            SendCommand(para_cmd).ToString();
#endif

            Thread.Sleep(1000);

            if (!ReceiveACK(CMD_SEQUENCE_CAPTURE, 1000, ref returnValue) || (returnValue != 0))
            {
                //String errorMsg = "Failed";
                //switch (returnValue)
                //{
                //    case 2:
                //        errorMsg = "Now in USB mode";
                //        break;
                //    case 3:
                //        errorMsg = "Mode error";
                //        break;
                //    case 4:
                //        errorMsg = "Memory full(RAM)";
                //        break;
                //    case 5:
                //        errorMsg = "Memory full (Flash)";
                //        break;
                //    case 6:
                //        errorMsg = "External memory card write-protect";
                //        break;
                //    default:
                //        break;
                //}

                // throw new Exception("C6820 failed to capture jpeg,error message:" + errorMsg);
                
                Sync();
                CaptureJpeg(n);
            }

            Thread.Sleep(500);

            int i = 0;
            while (!SetOperationMode(OperationMode.Playback))
            {
                i++;
                if (i > 60)
                {
                    throw new Exception("C6820 failed to set playback mode");
                }
            }

            return true;
        }

        /// <summary>
        /// Request hardware revision id of jpeg module 
        /// </summary>
        /// <returns>the HW Revision string</returns>
        /// <example>
        /// 0xY15Y14 Y13Y12 Y11Y10 Y9Y8 Y7Y6 Y5Y4 Y3Y2 Y1Y0
        /// Y15Y14 Y13Y12: Hardware Version
        /// Y11Y10 Y9Y8: Coach Version
        /// Y7Y6: Sensor Version
        /// Y5Y4: xx
        /// Y3Y2 Y1Y0: HCE Version 
        /// </example>
        public HardwareRevision GetRevisionID()
        {
            HardwareRevision hr = new HardwareRevision();
            byte[] hwRevisions = new byte[8];
            byte[] recvBuffer = new byte[13];
            CreatCommand(0, 2);
            SendCommand(id_cmd);

            if (!WaitForResponse(ref recvBuffer, 200) || recvBuffer[2] != CMD_REQUEST_HWREVISIONID)
                throw new Exception("C6820 Error " + recvBuffer.ToString());

            for (int i = 3; i < 11; i++)
            {
                hwRevisions[i - 3] = recvBuffer[i];
            }

            hr.HardwareVersion = (ushort)(hwRevisions[0] << 8 | hwRevisions[1]);
            hr.CoachVersion = (ushort)(hwRevisions[2] << 8 | hwRevisions[3]);
            hr.SensorVersion = hwRevisions[4];
            hr.HCEVersion = (ushort)(hwRevisions[6] << 8 | hwRevisions[7]);

            return hr;
        }

        /// <summary>
        /// Set datetime of camera
        /// </summary>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="day">day</param>
        /// <param name="hour">hour</param>
        /// <param name="minnute">minnute</param>
        /// <param name="seconds">seconds</param>
        /// <returns>True if succeed</returns>
        public bool SetSystemClock(short year, byte month, byte day, byte hour, byte minnute, byte seconds)
        {
            bool tag = false;
            byte returnValue = 255;
            byte[] writeBuffer = new byte[7];

            writeBuffer[0] = (byte)(year >> 8);
            writeBuffer[1] = (byte)(year);
            writeBuffer[2] = month;
            writeBuffer[3] = day;
            writeBuffer[4] = hour;
            writeBuffer[5] = minnute;
            writeBuffer[6] = seconds;

            CreatCommand(7, CMD_SETSYSCLK);
            CreatCommand(writeBuffer);
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (ReceiveACK(CMD_SETSYSCLK, 100, ref returnValue))
            {
                switch (returnValue)
                {
                    case 0:
                        tag = true;
                        break;
                    case 1:
                        throw new Exception("Failed to set system clock");
                    default:
                        break;
                }
            }
            return tag;
        }

        /// <summary>
        /// get system time of C6820 jpeg module
        /// </summary>
        /// <returns>system time value in DateTime type</returns>
        public DateTime GetSystemTime()
        {
            byte[] recvBuffer = new byte[12];
            
            CreatCommand(0, CMD_REQUEST_SYSTIME);
            SendCommand(id_cmd);

            if (!WaitForResponse(ref recvBuffer, 100) || recvBuffer[2] != CMD_REQUEST_SYSTIME)
                throw new Exception("C6820 error:" + recvBuffer.ToString());

            DateTime sysDT = new DateTime(
                (int)(recvBuffer[3] << 8 | recvBuffer[4]),//year
                recvBuffer[5],//month
                recvBuffer[6],//day
                recvBuffer[7],//hour
                recvBuffer[8],//minute
                recvBuffer[9]//second
                );

            return sysDT;
        }

        /// <summary>
        /// config the snapshot
        /// </summary>
        /// <param name="wb">WhiteBalance</param>
        /// <param name="ev">explosure value</param>
        /// <param name="contrast">Contrast</param>
        /// <param name="ce">ColorEffect</param>
        /// <param name="sharp">Sharpness</param>
        /// <returns>True if succeed</returns>
        public bool ConfigSnapshot(WhiteBalance wb, EV ev, Contrast contrast, ColorEffect ce, Sharpness sharp)
        {
            byte[] writeBuffer = new byte[5];

            writeBuffer[0] = (byte)wb;
            writeBuffer[1] = (byte)ev;
            writeBuffer[2] = (byte)contrast;
            writeBuffer[3] = (byte)ce;
            writeBuffer[4] = (byte)sharp;

            CreatCommand(5, CMD_SNAPSHOT_CONFIG);
            CreatCommand(writeBuffer);

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_SNAPSHOT_CONFIG, 100))
                return false;
            
            return true;
        }

        /// <summary>
        /// Get current operation mode 
        /// </summary>
        /// <returns>Operation Mode enum</returns>
        public OperationMode GetOperationMode()
        {
            OperationMode om = 0;
            byte returnValue = 255;
            
            CreatCommand(0, CMD_REQUEST_OPMODE);
            SendCommand(id_cmd);

            if (ReceiveACK(CMD_REQUEST_OPMODE, 100, ref returnValue))
            {
                switch (returnValue)
                {
                    case 1:
                        throw new Exception("Failed to return op mode");
                    case 2:
                        om = OperationMode.USB;
                        break;
                    case 3:
                        om = OperationMode.Idle;
                        break;
                    case 4:
                        om = OperationMode.JpegCap;
                        break;
                    case 5:
                        om = OperationMode.AVICap;
                        break;
                    case 6:
                        om = OperationMode.Playback;
                        break;
                    default:
                        throw new Exception("Failed to return op mode");
                }
                return om;
            }
            else
            {
                //throw new Exception("Failed to get current operation mode");
                GetOperationMode();
                return om;
            }
        }

        /// <summary>
        /// Set the resolution of Jpeg
        /// </summary>
        /// <param name="jr">resolution, can be 640x480 or 1280x960</param>
        /// <param name="compression">compression rate,1x to 45x, default 24x</param>
        /// <returns>True if success</returns>
        public bool SetJpegResolution(JpegResolution jr, byte compression)
        {
            byte returnValue = 255;
            
            if (compression > 45)
            {
                return false;
            }
            
            CreatCommand(2, CMD_JPEG_RESOLUTION);
            CreatCommand(new byte[] { (byte)jr, compression });

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_JPEG_RESOLUTION, 100, ref returnValue) || returnValue != 0)
                throw new Exception("C6820 set jpeg resolution error: " + returnValue.ToString());
            
            return true;
        }

        /// <summary>
        /// Get current luminance
        /// </summary>
        /// <returns>luminance</returns>
        public short GetLuminance()
        {
            byte[] recvBuffer = new byte[7];
            short luminance = 0;
            
            CreatCommand(0, CMD_REQUEST_LUMINANCE);
            SendCommand(id_cmd);

            if (!WaitForResponse(ref recvBuffer, 100) || recvBuffer[2] != CMD_REQUEST_LUMINANCE)
                throw new Exception("C6820 get luminance error:" + recvBuffer.ToString());

            luminance = (short)(recvBuffer[3] << 8 | recvBuffer[4]);
            
            return luminance;
        }

        /// <summary>
        /// Set DateTimeStamp to the still picture
        /// </summary>
        /// <param name="dts">DateTime settings</param>
        /// <param name="enable">indicates whether the DateTime stamp was enabled</param>
        /// <returns>True if succeed</returns>
        public bool SetDateTimeStamp(DateTimeStamp dts, bool enable)
        {
            byte format, corner, style;
            byte writeBuffer = (byte)(enable ? 0 : 1);
            byte returnValue = 1;
            
            CreatCommand(1, CMD_DATETIME_STAMP);

            format = (byte)((byte)(dts.Format) << 6);
            corner = (byte)((byte)(dts.Corner) << 4);
            style = (byte)((byte)(dts.Style) << 2);

            writeBuffer = (byte)(writeBuffer | format | corner | style);

            CreatCommand(new byte[] { writeBuffer });

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_DATETIME_STAMP, 100, ref returnValue) || returnValue != 0)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Set string stamp to the picture
        /// </summary>
        /// <param name="strStamp">stamp</param>
        /// <param name="enable">True if enable stamp</param>
        /// <returns>True if succeess</returns>
        public bool SetStringStamp(StringStamp strStamp, bool enable)
        {
            byte paraLength = (byte)(strStamp.StringLength + 11);
            byte[] writeBuffer = new byte[paraLength];
            byte returnValue = 1;
            
            writeBuffer[0] = (byte)(enable ? 0 : 1);
            
            CreatCommand(paraLength, CMD_STRING_STAMP);
            
            writeBuffer[1] = strStamp.FontW;
            writeBuffer[2] = strStamp.FontH;
            writeBuffer[3] = (byte)(strStamp.X >> 8);
            writeBuffer[4] = (byte)(strStamp.X);
            writeBuffer[5] = (byte)(strStamp.Y >> 8);
            writeBuffer[6] = (byte)(strStamp.Y);
            writeBuffer[7] = (byte)(strStamp.Red);
            writeBuffer[8] = (byte)(strStamp.Green);
            writeBuffer[9] = (byte)(strStamp.Blue);
            writeBuffer[10] = (byte)(strStamp.StringLength);
            
            StringToByteArray(strStamp.Text).CopyTo(writeBuffer, 11);

            CreatCommand(writeBuffer);

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_STRING_STAMP, 100, ref returnValue) || (returnValue != 0))
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Set AVI video resolution
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="compression">1x to 45x,default 0x1d</param>
        /// <returns>True if success</returns>
        public bool SetAVIResolution(AVIResolution resolution, byte compression)
        {
            byte returnValue = 1;
            
            if (compression > 45)
            {
                return false;
            }
            
            byte[] writeBuffer = new byte[2];
            
            CreatCommand(2, CMD_AVI_RESOLUTION);
            writeBuffer[0] = (byte)resolution;
            writeBuffer[1] = compression;

            CreatCommand(writeBuffer);

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_AVI_RESOLUTION, 200, ref returnValue) || returnValue != 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start/Stop record the AVI video clip
        /// </summary>
        /// <param name="start">true if start and false if stop</param>
        /// <returns>True if succeed</returns>
        public bool RecordAVI(bool start)
        {
            // if (GetOperationMode() != OperationMode.AVICap)
            // {
            Thread.Sleep(500);
            SetOperationMode(OperationMode.AVICap);
            // }
            
            byte writeBuffer = (byte)(start ? 0 : 1);
            
            CreatCommand(1, CMD_AVI_START_STOP);
            CreatCommand(new byte[] { writeBuffer });
            
            Thread.Sleep(500);
            
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_AVI_START_STOP, 500))
                return false;
            
            return true;
        }

        /// <summary>
        /// Set the storage media of the jpeg module
        /// </summary>
        /// <param name="stMedia">resident or external</param>
        /// <returns>Ture if succeed</returns>
        public bool SetStorageMedia(StorageMedia stMedia)
        {
            byte returnValue = 1;
            byte writeBuffer = (byte)(stMedia == StorageMedia.Resident ? 0 : 1);
            
            CreatCommand(1, CMD_SELECT_STORAGEMEDIA);
            CreatCommand(new byte[] { writeBuffer });

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_SELECT_STORAGEMEDIA, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Format the storage madia of the jpeg module
        /// </summary>
        /// <param name="stMedia">the storage media to format</param>
        /// <returns>True if success</returns>
        public bool FormatStorageMedia(StorageMedia stMedia)
        {
            byte returnValue = 1;
            byte writeBuffer = (byte)(stMedia == StorageMedia.Resident ? 0 : 1);
            
            CreatCommand(1, CMD_FORMAT_STORAGEMEDIA);
            CreatCommand(new byte[] { writeBuffer });

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_FORMAT_STORAGEMEDIA, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Get information of storage media
        /// </summary>
        /// <returns>the storage media infomation</returns>
        public StorageMediaInfo GetStorageMediaInfo()
        {
            byte[] recvBuffer = new byte[15];
            
            CreatCommand(0, CMD_REQUEST_STORAGEINFO);
            SendCommand(id_cmd);

            if (!WaitForResponse(ref recvBuffer, 100) || recvBuffer[2] != CMD_REQUEST_STORAGEINFO)
                throw new Exception("C6820 get storageMediaInfo error:");

            return new StorageMediaInfo(
                (uint)(recvBuffer[3] << 24 | recvBuffer[4] << 16 | recvBuffer[5] << 8 | recvBuffer[6]),
                (ushort)(recvBuffer[7] << 8 | recvBuffer[8]),
                (ushort)(recvBuffer[9] << 8 | recvBuffer[10]),
                (ushort)(recvBuffer[11] << 9 | recvBuffer[12])
                );
        }

        /// <summary>
        /// Request the targeted file information, File name, File size and Video length (For AVI ONLY)
        ///Memory Unit: Byte
        ///Time Unit: Second
        /// </summary>
        /// <param name="fileid">File ID</param>
        /// <returns>an instance of FileInfo</returns>
        public FileInfo GetFileInfo(ushort fileid)
        {
            if (fileid < 1)
            {
                throw new Exception("Invalid File ID");
            }
            
            char[] filename = new char[12];
            byte[] recvBuffer = new byte[25];
            byte[] writeBuffer = new byte[2];
            
            writeBuffer[0] = (byte)(fileid >> 8);
            writeBuffer[1] = (byte)(fileid);
            
            CreatCommand(2, CMD_REQUEST_FILEINFO);
            CreatCommand(writeBuffer);
            
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!WaitForResponse(ref recvBuffer, 300) || recvBuffer[2] != CMD_REQUEST_FILEINFO)
                throw new Exception("C6820 get File Info error:");
            
            for (int i = 0; i < 12; i++)
            {
                filename[i] = (char)recvBuffer[i + 3];
            }

            return new FileInfo(
                new string(filename),
                (uint)(recvBuffer[17] << 24 | recvBuffer[18] << 16 | recvBuffer[19] << 8 | recvBuffer[20]),//filesize
                (ushort)(recvBuffer[21] << 8 | recvBuffer[22]) //video length
            );
        }

        /// <summary>
        /// Delete a file by file id
        /// </summary>
        /// <param name="fileId">the file id of which file to be delete</param>
        /// <returns>True if success</returns>
        public bool DeleteFile(ushort fileId)
        {
            byte returnValue = 1;
            byte[] writeBuffer = new byte[2];
            
            CreatCommand(2, CMD_DELETEFILE);
            
            writeBuffer[0] = (byte)(fileId >> 8);
            writeBuffer[1] = (byte)fileId;
            
            CreatCommand(writeBuffer);

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_DELETEFILE, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Set the TV standard of jpeg module
        /// </summary>
        /// <param name="tvs">standard</param>
        /// <returns>True if success</returns>
        public bool SetTvStandard(TVStandard tvs)
        {
            byte returnBuffer = 1;
            byte writeBuffer = (byte)tvs;
            
            CreatCommand(1, CMD_SELECT_TVSTANDARD);
            CreatCommand(new byte[] { writeBuffer });
            
            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_SELECT_TVSTANDARD, 100, ref returnBuffer) || returnBuffer != 0)
                throw new Exception("C6820 error : Set TV standard error");
            
            return true;
        }

        /// <summary>
        /// Get the status of external memory
        /// </summary>
        /// <returns>connection status</returns>
        public ExternMemoConnectStatus GetExMemoConnectStatus()
        {
            ExternMemoConnectStatus emcs = 0;
            byte returnValue = 0;
            
            CreatCommand(0, CMD_REQUEST_CONNECTSTATUS);
            SendCommand(id_cmd);

            if (!ReceiveACK(CMD_REQUEST_CONNECTSTATUS, 100, ref returnValue))
                throw new Exception("C6820 error: Failed to get status of the external memory connection");
            
            switch (returnValue)
            {
                case 6:
                    emcs = ExternMemoConnectStatus.WriteProtected;
                    break;
                case 7:
                    emcs = ExternMemoConnectStatus.Connect;
                    break;
                case 8:
                    emcs = ExternMemoConnectStatus.Disconnect;
                    break;
                default:
                    break;
            }
            
            return emcs;
        }

        /// <summary>
        /// Play the selected AVI file 
        /// </summary>
        /// <param name="playop">playback option</param>
        /// <returns>Ture if success</returns>
        public bool PlayCurrentAVI(PlayBackOperation playop)
        {
            byte returnValue = 255;
            byte writeBuffer = (byte)playop;
            
            CreatCommand(1, CMD_PLAYBACK_CURRENTAVI);
            CreatCommand(new byte[] { writeBuffer });

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_PLAYBACK_CURRENTAVI, 200, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Select previours file in current storage media
        /// </summary>
        /// <returns>True if success</returns>
        public bool SelectPrevioursFile()
        {
            byte returnValue = 255;
            
            CreatCommand(1, CMD_SELECT_PRE_NEXT);
            CreatCommand(new byte[] { 0 });
            
            SendCommand(id_cmd);
            SendCommand(para_cmd);
            
            if (!ReceiveACK(CMD_SELECT_PRE_NEXT, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Select next file in current storage media
        /// </summary>
        /// <returns>True if success</returns>
        public bool SelectNextFile()
        {
            byte returnValue = 255;
            
            CreatCommand(1, CMD_SELECT_PRE_NEXT);
            CreatCommand(new byte[] { 1 });
            
            SendCommand(id_cmd);
            SendCommand(para_cmd);
            
            if (!ReceiveACK(CMD_SELECT_PRE_NEXT, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        /// <summary>
        /// Select particular file in current storage media 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public bool SelectParticularFile(ushort fileId)
        {
            byte returnValue = 255;
            byte[] writeBuffer = new byte[2];
            
            writeBuffer[0] = (byte)(fileId >> 8);
            writeBuffer[1] = (byte)fileId;
            
            CreatCommand(2, CMD_SELECT_PARTICULAR_FILE);
            CreatCommand(writeBuffer);

            SendCommand(id_cmd);
            SendCommand(para_cmd);

            if (!ReceiveACK(CMD_SELECT_PARTICULAR_FILE, 100, ref returnValue) || returnValue != 0)
                return false;
            
            return true;
        }

        #endregion

        #region TODO

        /// <summary>
        /// Download file 
        /// </summary>
        /// <param name="filieid">File ID</param>
        /// <returns>True if success</returns>
        public bool GetFile(ushort filieid)
        {
            SetOperationMode(OperationMode.Idle);
            CreatCommand(2, CMD_DOWNLOADFILE);
            //TODO
            return false;
        }

        #endregion
    }
}
