/* 
 * DigiXBeeSensorHelper.cs
 * 
 * Copyright (c) 2017, Michael Schwarz (http://www.schwarz-interactive.de)
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
 * 
 * MS	17-12-21	initial version
 * 
 */
using System;
using MFToolkit.IO;

namespace MFToolkit.Net.XBee
{
	public class DigiXBeeSensorHelper
	{
		private ZNetRxIoSampleResponse sensorResponse = null;

		#region Public Properties

		public double Temperatur
		{
			get
			{
				double mVanalog = (((float)sensorResponse.AD2) / 1023.0) * 1200.0;
				double temp_C = (mVanalog - 500.0) / 10.0 - 4.0;

				return temp_C;
			}
		}

		public double Light
		{
			get
			{
				return (((float)sensorResponse.AD1) / 1023.0) * 1200.0;
			}
		}

		public double Humidity
		{
			get
			{
				double mVanalog = (((float)sensorResponse.AD3) / 1023.0) * 1200.0;
				double hum = ((mVanalog * (108.2 / 33.2)) - 0.16) / (5 * 0.0062 * 1000.0);

				return hum;
			}
		}

		#endregion

		public DigiXBeeSensorHelper(ZNetRxIoSampleResponse res)
		{
			sensorResponse = res;
		}
	}
}
