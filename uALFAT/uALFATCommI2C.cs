/* 
 * uALFATCommI2C.cs
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
 */

using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MFToolkit.Devices
{
    /// <summary>
    /// I2C Interface for uALFAT device
    /// </summary>
    public class uALFATCommI2C : IuALFATComm
    {
        /// <summary>
        /// Pin to use for reset
        /// </summary>
        private const Cpu.Pin _resetpin = DeviceSolutions.SPOT.Hardware.Meridian.Pins.GPIO3;

        /// <summary>
        /// Pin to use for SSEL 
        /// </summary>
        private const Cpu.Pin _sselpin = DeviceSolutions.SPOT.Hardware.Meridian.Pins.GPIO9;

        /// <summary>
        /// Pin to use for SCK
        /// </summary>
        private const Cpu.Pin _sckpin = DeviceSolutions.SPOT.Hardware.Meridian.Pins.GPIO5;

        /// <summary>
        /// Reset output port
        /// </summary>
        private OutputPort _resetport;

        /// <summary>
        /// I2CDevice used to connect to uALFAT
        /// </summary>
        private I2CDevice _i2cconnection;

        /// <summary>
        /// Open communication
        /// </summary>
        public void Open()
        {
            try
            {
                // Reset uALFAT with I2C enabled
                OutputPort SSELPort = new OutputPort(_sselpin, true);
                OutputPort SCKPort = new OutputPort(_sckpin, false);
                _resetport = new OutputPort(_resetpin, false);
                Thread.Sleep(100);
                _resetport.Write(true);
                Thread.Sleep(100);
                SSELPort.Dispose();
                SCKPort.Dispose();
            }
            catch
            {
                throw new Exception("Failed to reset uALFAT");
            }
            // Create new I2C Connection
            try { _i2cconnection = new I2CDevice(new I2CDevice.Configuration(0x52, 400)); }
            catch { throw new Exception("Failed to initialize uALFAT I2c Interface!"); }

            // Read as many bytes as posible to clear any cached output
            byte b;
            while (ReadByte(out b, 250))
            {
            }

        }

        /// <summary>
        /// Close communication
        /// </summary>
        public void Close()
        {

            // Dispose Reset Port
            try { _resetport.Dispose(); }
            catch { }

            // Dispose I2C Connection
            try { _i2cconnection.Dispose(); }
            catch { }
        }

        /// <summary>
        /// Send byte to uALFAT
        /// </summary>
        /// <param name="b">Byte to send</param>
        /// <returns>True on succes, False on error </returns>
        public bool SendByte(byte b)
        {
            // Create write transaction
            I2CDevice.I2CTransaction[] WriteByte = new I2CDevice.I2CTransaction[] { _i2cconnection.CreateWriteTransaction(new byte[] { b }) };

            // Execute
            int bytessend = _i2cconnection.Execute(WriteByte, 100);

            // Check bytes send
            if (bytessend == 1)
                return true; // succes
            else
                return false; // error
        }

        /// <summary>
        /// Read byte from uALFAT
        /// </summary>
        /// <param name="b">Read byte</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>True on succes, False on error</returns>
        public bool ReadByte(out byte b, long timeout)
        {
            // Store start time
            long start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // Buffer used for reading
            byte[] buf = new byte[1];

            // Start with 0xff;
            b = 0xff;

            do
            {
                // Create read transaction
                I2CDevice.I2CTransaction[] ReadByte = new I2CDevice.I2CTransaction[] { _i2cconnection.CreateReadTransaction(buf) };

                // Execute and check if byte is read succesfull
                int bytesread = _i2cconnection.Execute(ReadByte, 100);
                if (bytesread != 1) return false;

                // Get read byte
                b = buf[0];

                // Check if timed otu
                if ((start + timeout) < (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)) return false;

            } while (b == 0xff);

            // Check for half-data token
            if (b == 0xfe)
            {
                // Create read transaction
                I2CDevice.I2CTransaction[] ReadByte = new I2CDevice.I2CTransaction[] { _i2cconnection.CreateReadTransaction(buf) };

                // Execute and check if byte is read succesfull
                int bytesread = _i2cconnection.Execute(ReadByte, 100);
                if (bytesread != 1) return false;

                // Get read byte
                b = buf[0];

                // Check for second half data token, if not send 0xff
                if (b != 0xfe) b = 0xff;
                return true;
            }

            // Succes
            return true;
        }

    }
}
