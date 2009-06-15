/* 
 * SupplyVoltage.cs
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
    /// <summary>
    /// Represents a supply voltage command response structure
    /// </summary>
    public class SupplyVoltage : IAtCommandResponseData
	{
		private short _supply;
 
		#region Public Properties

		/// <summary>
		/// Gets the voltage in mV on the Vcc pin.
		/// </summary>
		public int Voltage
		{
			get
			{
				return ((int)_supply) * (1200 / 1024);
			}
		}

		#endregion

        public static SupplyVoltage Parse(IAtCommandResponse cmd)
        {
            if (cmd.Command != SupplyVoltageCommand.command)
                throw new ArgumentException("This method is only applicable for the '" + SupplyVoltageCommand.command + "' command!", "cmd");

            ByteReader br = new ByteReader(cmd.Value, ByteOrder.BigEndian);

            SupplyVoltage sv = new SupplyVoltage();
            sv.ReadBytes(br);

            return sv;
        }

        public void ReadBytes(ByteReader br)
		{
    		_supply = br.ReadInt16();
		}

		public override string ToString()
		{
			return this.Voltage + " mV";
		}

		public static implicit operator int(SupplyVoltage d)
		{
			if (d == null)
				throw new ArgumentException("SupplyVoltageData can not be null.", "d");

			return d.Voltage;
		}
	}
}
