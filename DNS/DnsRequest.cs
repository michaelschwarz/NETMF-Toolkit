/* 
 * DnsRequest.cs
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
 * MS   09-02-16    changed build compiler argument for .NET MF
 * 
 */
using System;
using System.Text;
#if(!MF)
using System.Collections.Generic;
#endif
using MSchwarz.IO;

namespace MSchwarz.Net.Dns
{
    public class DnsRequest
    {
        private short _messageID;
        //private readonly byte _flag1;
        //private readonly byte _flag2;

#if(MF)
        private Question[] _questions;
#else
        private List<Question> _questions = new List<Question>();
#endif


        #region Public Properties

        public short MessageID
        {
            get { return _messageID; }
            internal set { _messageID = value; }
        }

#if(MF)
        public Question[] Questions
#else
        public List<Question> Questions
#endif
        {
            get { return _questions; }
            set { _questions = value; }
        }

        #endregion

        public DnsRequest()
        {
        }

        public byte[] GetMessage()
        {
            DnsWriter bw = new DnsWriter();

            // Header (RFC 1035 4.1.1)

            bw.Write(_messageID);
            
            // TODO: all flags
            bw.Write(new byte[]
            {
                0x1, 0x0        // status field, here it is recursion desired bit
            });


            // Question

#if(MF)
            bw.Write((short)_questions.Length);
#else
            bw.Write((short)_questions.Count);
#endif

            // Answer, Authority, Additional 
            // Those three records are not available in a query.

            bw.Write(new byte[]
            {
                0x0, 0x0,       // how many answers
                0x0, 0x0,       // how many name server records
                0x0, 0x0        // how many additional records
            });

            foreach (Question q in _questions)
            {
                bw.WriteDomain(q.Domain);
                bw.Write((short)q.Type);
                bw.Write((short)q.Class);
            }

            return bw.GetBytes();
        }
    }
}
