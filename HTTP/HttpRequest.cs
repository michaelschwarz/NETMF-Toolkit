/* 
 * HttpRequest.cs
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
 * MS	08-03-24	initial version
 * MS   09-02-10    added GetHeaderValue
 * 
 */
using System;

namespace MSchwarz.Net.Web
{
    public class HttpRequest
    {
        private string _httpMethod;
        private string _rawUrl;
        private string _httpVersion;

        private string _path;
        private string _queryString;

        private string _userHostAddress;


        public HttpCookie[] Cookies;
        public HttpHeader[] Headers;
        public HttpParameter[] Params;
        
        public byte[] Body = null;

        #region Public Properties

        public string this[string name]
        {
            get
            {
                if (Params == null || Params.Length == 0)
                    return null;

                for (int i = 0; i < Params.Length; i++)
                {
                    if (Params[i].Name == name)
                        return Params[i].Value;
                }

                return null;
            }
        }

        public string UserAgent
        {
            get
            {
                return this["User-Agent"];
            }
        }

        public string[] AcceptTypes
        {
            get
            {
                string accept = this["Accept"];

                if (accept == null)
                    return null;

                string[] acceptTypes = accept.Split(',');

                for(int i=0; i<acceptTypes.Length; i++)
                {
                    acceptTypes[i].Trim();
                }

                return acceptTypes;
            }
        }

        public int ContentLength
        {
            get
            {
                return int.Parse(this["Content-Length"]);
            }
        }

        public string ContentType
        {
            get
            {
                return this["Content-Type"];
            }
        }

        public string HttpMethod
        {
            get
            {
                return _httpMethod;
            }
            internal set
            {
                _httpMethod = value;
            }
        }

        public string RawUrl
        {
            get
            {
                return _rawUrl;
            }
            internal set
            {
                _rawUrl = value;
            }
        }

        public string HttpVersion
        {
            get
            {
                return _httpVersion;
            }
            internal set
            {
                _httpVersion = value;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            internal set
            {
                _path = value;
            }
        }

        public string QueryString
        {
            get
            {
                return _queryString;
            }
            internal set
            {
                _queryString = value;
            }
        }

        public string UserHostAddress
        {
            get
            {
                return _userHostAddress;
            }
            internal set
            {
                _userHostAddress = value;
            }
        }

        #endregion

        public string GetHeaderValue(string name)
        {
            if (Headers == null)
                return null;

            for (int i = 0; i < Headers.Length; i++)
            {
                if (Headers[i].Name == name)
                    return Headers[i].Value;
            }

            return null;
        }
    }
}
