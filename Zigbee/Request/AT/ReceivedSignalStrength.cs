﻿/* 
 * ReceivedSignalStrength.cs
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

namespace MSchwarz.Net.Zigbee
{
	/// <summary>
	/// Diagnostics: DB parameter is used to read the
	/// received signal strength (in dBm) of the last RF
	/// packet received. Reported values are accurate
	/// between -40 dBm and the RF module's receiver
	/// sensitivity.
	/// Absolute values are reported. For example: 0x58 = -88 dBm (decimal). If no packets have been
	/// received (since last reset, power cycle or sleep event), "0" will be reported.
	/// </summary>
	public class ReceivedSignalStrength : AtCommand
	{
		public ReceivedSignalStrength()
			: base("DB")
		{
		}
	}
}
