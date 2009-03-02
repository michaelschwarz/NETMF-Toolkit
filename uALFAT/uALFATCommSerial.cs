/* 
 * uALFATCommSerial.cs
 * 
 * Copyright (c) 2009, Elze Kool (http://www.microframework.nl)
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
 * EK 1 March 2009: Initial version.
 * 
 */

using System;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MFToolkit.Devices
{
    /// <summary>
    /// Serial Interface for uALFAT device
    /// </summary>
    public class uALFATCommSerial : IuALFATComm
    {
        /// <summary>
        /// Pin to use for reset
        /// </summary>
        private Cpu.Pin _resetpin; 

        /// <summary>
        /// Pin to use for SSEL 
        /// </summary>
        private Cpu.Pin _sselpin;

        /// <summary>
        /// Pin to use for SCK
        /// </summary>
        private Cpu.Pin _sckpin;

        /// <summary>
        /// Reset output port
        /// </summary>
        private OutputPort _resetport;


        // Comport to use
        private readonly String _comport;

        // UART Object
        private SerialPort _uart;


        /// <summary>
        /// Create new uALFATCommSerial Instance
        /// </summary>
        /// <param name="UART">UART to use</param>
        /// <param name="Reset">Pin connected to uALFAT Reset</param>
        /// <param name="SSEL">Pin connected to uALFAT SSEL#</param>
        /// <param name="SCK">Pin connected to uALFAT SCK</param>
        public uALFATCommSerial(String UART, Cpu.Pin Reset, Cpu.Pin SSEL, Cpu.Pin SCK)
        {
            this._comport = UART;
            this._resetpin = Reset;
            this._sselpin = SSEL;
            this._sckpin = SCK;
        }

        /// <summary>
        /// Open communication
        /// </summary>
        public void Open()
        {
            try
            {
                // Reset uALFAT with I2C enabled
                OutputPort SSELPort = new OutputPort(_sselpin, false);
                OutputPort SCKPort = new OutputPort(_sckpin, false);
                _resetport = new OutputPort(_resetpin, false);
                System.Threading.Thread.Sleep(100);
                _resetport.Write(true);
                System.Threading.Thread.Sleep(100);
                SSELPort.Dispose();
                SCKPort.Dispose();
            }
            catch
            {
                throw new Exception("Failed to reset uALFAT");
            }

            try
            {
                _uart = new SerialPort(_comport, 9600, Parity.None, 8, StopBits.One);
                _uart.Handshake = Handshake.RequestToSend;
                _uart.Open();

                byte[] SwitchBaudRate = System.Text.Encoding.UTF8.GetBytes("B 1EF4\r");
                _uart.Write(SwitchBaudRate, 0, SwitchBaudRate.Length);
                _uart.Flush();
                while (_uart.BytesToWrite != 0) { }

                _uart.Close();

                _uart = new SerialPort(_comport, 115200, Parity.None, 8, StopBits.One);
                _uart.Handshake = Handshake.RequestToSend;
                _uart.Open();

                byte[] cleanbuffer = new byte[1];
                DateTime Wait = DateTime.Now.AddMilliseconds(1000);
                while (true)
                {
                    if (_uart.BytesToRead > 0)
                    {
                        _uart.Read(cleanbuffer, 0, 1);
                        Wait = DateTime.Now.AddMilliseconds(200);
                    }
                    if (DateTime.Now > Wait) break;
                }
                
            }
            catch
            {
                throw new Exception("Failed to open uALFAT Serial Port");
            }
        }


        /// <summary>
        /// Close communication
        /// </summary>
        public void Close()
        {
            try
            {
                _uart.Close();
                _resetport.Dispose();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Send byte to uALFAT
        /// </summary>
        /// <param name="b">Byte to send</param>
        /// <returns>True on succes, False on error </returns>
        public bool SendByte(byte b)
        {
            try
            {
                _uart.Write(new byte[] { b }, 0, 1);
                _uart.Flush();
                while (_uart.BytesToWrite != 0) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read byte from uALFAT
        /// </summary>
        /// <param name="b">Read byte</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>True on succes, False on error</returns>
        public bool ReadByte(out byte b, long timeout)
        {
            DateTime TimeOutAt = DateTime.Now.AddMilliseconds(timeout);

            while (_uart.BytesToRead == 0)
            {
                System.Threading.Thread.Sleep(5);
                b = 0;
                if (DateTime.Now > TimeOutAt) return false;
            }

            byte[] buf = new byte[1];
            _uart.Read(buf, 0, 1);

            b = buf[0];
            return true;
        }


    }
}
