/* 
 * HttpResponse.cs
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
 *
 * MS	08-03-24	initial version
 * 
 */
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace MSchwarz.Net.Web
{
    public class HttpResponse
    {
        private MemoryStream _content;

        public HttpResponse()
        {
            _content = new MemoryStream();
        }

        /*
HTTP/1.1 200 OK
Cache-Control: no-cache
Pragma: no-cache,no-cache
Content-Length: 37514
Content-Type: text/html; charset=utf-8
Expires: -1
Server: Microsoft-IIS/7.0
X-AspNet-Version: 2.0.50727
Set-Cookie: LastPage=/multimachineterminal.aspx; path=/
X-Powered-By: ASP.NET
Date: Wed, 05 Mar 2008 11:14:43 GMT
         */

        public string HttpVersion = "HTTP/1.1";
        public HttpStatusCode HttpStatus = HttpStatusCode.OK;

        public string ContentType = "text/html; charset=utf-8";
        public string Expires = "-1";
        public string Server = "HttpServer";
        public List<HttpCookie> SetCookie = null;
        public List<HttpHeader> Headers = null;     // TODO: implement HttpHeaders
        public DateTime Date = DateTime.Now;    // "Date: Wed, 05 Mar 2008 11:14:43 GMT";
        public string Connection = "Close";

        public void Clear()
        {
            _content = new MemoryStream();
        }

        public void Write(string s)
        {
            byte[] b = Encoding.UTF8.GetBytes(s);
            _content.Write(b, 0, b.Length);
        }

        public void WriteLine(string line)
        {
            Write(line);
            Write(Environment.NewLine);
        }

        internal string GetResponseHeader()
        {
            string response = HttpVersion + " " + (int)HttpStatus + HttpStatusHelper.GetHttpStatusFromCode(HttpStatus) + "\r\nContent-Type: " + ContentType + "\r\n"
                + "Expires: " + Expires + "\r\nServer: " + Server + "\r\n";

            if (SetCookie != null)
            {
                for (int i = 0; i < SetCookie.Count; i++)
                {
                    response += "Set-Cookie: " + SetCookie[i] + "\r\n";
                }
            }

            response += "Date: " + Date + "\r\n";

            response += "Content-Length: " + _content.Length + "\r\n";
            response += "Connection: " + Connection + "\r\n";
            response += "\r\n";

            return response;
        }

        internal byte[] GetResponseHeaderBytes()
        {
            return Encoding.UTF8.GetBytes(this.GetResponseHeader());
        }

        internal byte[] GetResponseBytes()
        {
            return _content.ToArray();   
        }
    }
}
