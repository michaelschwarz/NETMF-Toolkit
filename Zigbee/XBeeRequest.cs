/* 
 * XBeeRequest.cs
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
    public abstract class XBeeRequest : XBeePacket
    {
        private XBeeApiType _apiId;

        #region Public Properties

        public XBeeApiType ApiID
        {
            get { return _apiId; }
            set { _apiId = value; }
        }

        #endregion

        #region Constructor

        public XBeeRequest()
            : base()
        {
        }

        public XBeeRequest(XBeeApiType apiID)
            : this()
        {
            ApiID = apiID;
        }

        #endregion

        internal override void WriteApiBytes(ByteWriter bw)
        {
            base.WriteApiBytes(bw);

            bw.Write((byte)ApiID);
        }

        public override string ToString()
        {
            return base.ToString() + "\r\nApiID: " + ApiID;
        }
    }
}
