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
        /*
GET / HTTP/1.1
Accept: *\/*
Accept-Language: en-us
UA-CPU: x86
Accept-Encoding: gzip, deflate
User-Agent: Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727)
Host: localhost:12000
Connection: Keep-Alive
        */
        
        public string[] AcceptTypes;
        public string ContentEncoding;
        public int ContentLength;
        public string ContentType;
        public string Connection = "Close";
        public HttpCookie[] Cookies;
        public HttpHeader[] Headers;
        public HttpParameter[] Params;
        public string HttpMethod;
        public string RawUrl;
        public string HttpVersion;
        public string UserAgent;
        public string UserHostAddress;
        public string UserHostName;
        public string[] UserLanguages;
        public string Referer;
        public byte[] Body = null;

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
