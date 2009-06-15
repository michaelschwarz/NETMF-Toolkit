/* 
 * ZNetTxStatusResponse.cs
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
    /// When a TX Request is completed, the module sends a TX Status message. This message will
    /// indicate if the packet was transmitted successfully or if there was a failure.
    /// </summary>
	public class ZNetTxStatusResponse : XBeeFrameResponse
	{
        private XBeeAddress16 _address16;
		private byte _retryCount;
		private byte _deliveryStatus;
		private byte _discoveryStatus;

		#region Public Properties

        /// <summary>
        /// 16-bit Network Address the packet was delivered to (if success). If not success, this
        /// address matches the Destination Network Address that was provided in the Transmit Request Frame.
        /// </summary>
        public XBeeAddress16 RemoteNetwork
        {
            get { return _address16; }
        }

        /// <summary>
        /// The number of application transmission retries that took place.
        /// </summary>
        public byte RetryCount
        {
            get { return _retryCount; }
        }

		public DeliveryStatusType DeliveryStatus
		{
			get { return (DeliveryStatusType)_deliveryStatus; }
		}

		public DiscoveryStatusType DiscoveryStatus
		{
			get { return (DiscoveryStatusType)_discoveryStatus; }
		}
		
		#endregion

        public ZNetTxStatusResponse(short length, ByteReader br)
			: base(length, br)
		{
            _address16 = XBeeAddress16.ReadBytes(br);
			_retryCount = br.ReadByte();
			_deliveryStatus = br.ReadByte();
			_discoveryStatus = br.ReadByte();
		}

		public override string ToString()
		{
			string s = base.ToString() + "\r\n";

            s += "RemoteNetwork = " + RemoteNetwork + "\r\n";
			s += "Retries       = " + RetryCount + "\r\n";
			s += "Delivery      = " + DeliveryStatus + "\r\n";
			s += "Discovery     = " + DiscoveryStatus;

			return s;
		}
	}
}