/* 
 * DnsResponse.cs
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
 * 
 * 
 */
using System;
using System.Text;
#if(!MF)
using System.Collections.Generic;
#endif

namespace MSchwarz.Net.Dns
{
    public class DnsResponse
    {
        private readonly DnsHeader _header;
        private readonly short _numQuestions;
        private readonly short _numAnswers;
        private readonly short _numNameServers;
        private readonly short _numAdditionalRecords;

#if(MF)
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

        public DnsHeader Header
        {
            get { return _header; }
        }

#if(MF)
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
            _header = new DnsHeader(br);

            _numQuestions = br.ReadInt16();
            _numAnswers = br.ReadInt16();
            _numNameServers = br.ReadInt16();
            _numAdditionalRecords = br.ReadInt16();

#if(MF)
            _questions = new Question[_numQuestions];
            _answers = new Answer[_numAnswers];
            _authorities = new Authority[_numNameServers];
            _additionalRecords = new Additional[_numAdditionalRecords];
#endif


#if(MF)
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
