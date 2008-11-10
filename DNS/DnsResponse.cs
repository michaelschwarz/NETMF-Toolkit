/* 
 * DnsResponse.cs
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
using System.Collections.Generic;
using System.Text;

namespace MSchwarz.Net.Dns
{
    public class DnsResponse
    {
        private readonly short _messageID;
        private readonly byte _flag1;
        private readonly byte _flag2;
        private readonly short _numQuestions;
        private readonly short _numAnswers;
        private readonly short _numNameServers;
        private readonly short _numAdditionalRecords;

#if(NETMF)
        private readonly Question[] _questions;
        private readonly Answer[] _answers;
        private readonly Authority[] _authorities;
        private readonly Additional[] _additionalRecords;
#else
        private readonly List<Question> _questions = new List<Question>();
        private readonly List<Answer> _answers = new List<Answer>();
        private readonly List<Authority> _authorities = new List<Authority>();
        private readonly List<Additional> _additionalRecords = new List<Additional>();
#endif

        #region Public Properties

        public short MessageID
        {
            get { return _messageID; }
        }

#if(NETMF)
        public Question[] Questions
        {
            get { return _questions; }
        }

        public Answer[] Answers
        {
            get { return _answers; }
        }

        public Authority[] Authorities
        {
            get { return _authorities; }
        }

        public Additional[] AdditionalRecords
        {
            get { return _additionalRecords; }
        }
#else
        public List<Question> Questions
        {
            get { return _questions; }
        }

        public List<Answer> Answers
        {
            get { return _answers; }
        }

        public List<Authority> Authorities
        {
            get { return _authorities; }
        }

        public List<Additional> AdditionalRecords
        {
            get { return _additionalRecords; }
        }
#endif

        #endregion


        internal DnsResponse(DnsReader br)
        {
            // Header (RFC 1035 4.1.1)
            _messageID = br.ReadInt16();

            // TODO: all flags
            _flag1 = br.ReadByte();
            _flag2 = br.ReadByte();

            _numQuestions = br.ReadInt16();
            _numAnswers = br.ReadInt16();
            _numNameServers = br.ReadInt16();
            _numAdditionalRecords = br.ReadInt16();

#if(NETMF)
            _questions = new Question[_numQuestions];
            _answers = new Answer[_numAnswers];
            _authorities = new Authority[_numNameServers];
            _additionalRecords = new Additional[_numAdditionalRecords];
#endif


#if(NETMF)
            for (int i = 0; i < _numQuestions; i++)
                _questions[i] = new Question(br);

            for (int i = 0; i < _numAnswers; i++)
                _answers[i] = new Answer(br);

            for (int i = 0; i < _numNameServers; i++)
                _authorities[i] = new Authority(br);

            for (int i = 0; i < _numAdditionalRecords; i++)
                _additionalRecords[i] = new Additional(br);
#else

            for (int i = 0; i < _numQuestions; i++)
                _questions.Add(new Question(br));

            for (int i = 0; i < _numAnswers; i++)
                _answers.Add(new Answer(br));

            for (int i = 0; i < _numNameServers; i++)
                _authorities.Add(new Authority(br));

            for (int i = 0; i < _numAdditionalRecords; i++)
                _additionalRecords.Add(new Additional(br));
#endif
           
        }
    }
}
