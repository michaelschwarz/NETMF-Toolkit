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
using MFToolkit.IO;

namespace MFToolkit.Net.Dns
{
    public class DnsRequest
    {
        #region Private Variables

        private DnsHeader _header;

#if(MF)
        private Question[] _questions;
        private Additional[] _additionalRecords;
#else
        private List<Question> _questions = new List<Question>();
        private List<Additional> _additionalRecords = new List<Additional>();
#endif
        #endregion

        #region Public Properties

        public DnsHeader Header
        {
            get { return _header; }
            set { _header = value; }
        }

#if(MF)
        public Question[] Questions
        {
            get { return _questions; }
            set { _questions = value; }
        }

        public Additional[] AdditionalRecords
        {
            get { return _additionalRecords; }
            set { _additionalRecords = value; }
        }
#else
        public List<Question> Questions
        {
            get { return _questions; }
            set { _questions = value; }
        }

        public List<Additional> AdditionalRecords
        {
            get { return _additionalRecords; }
            set { _additionalRecords = value; }
        }
#endif

        #endregion

        public DnsRequest()
        {
            _header = new DnsHeader();

            _header.QR = false;
            _header.Opcode = OpcodeType.Query;
        }

        public DnsRequest(Question question)
            : this()
        {
#if(MF)
            _questions = new Question[] { question };
#else
            _questions.Add(question);
#endif
        }

        public byte[] GetMessage()
        {
            DnsWriter bw = new DnsWriter();

            _header.WriteBytes(bw);


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
                0x0, 0x0       // how many name server records                
            });

            // Additional

#if(MF)
            bw.Write((short)_additionalRecords.Length);
#else
            bw.Write((short)_additionalRecords.Count);
#endif

            foreach (Question q in _questions)
            {
                q.Write(bw);
            }

            foreach (Additional a in _additionalRecords)
            {
                a.Write(bw);
            }

            return bw.GetBytes();
        }
    }
}
