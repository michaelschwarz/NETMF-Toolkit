/* 
 * ProcessClientRequest.cs
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
 * MS   09-02-10    fixed keep-alive support
 * 
 * 
 * 
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Text;
using Socket = System.Net.Sockets.Socket;
using System.Diagnostics;
using System.IO;

namespace MSchwarz.Net.Web
{
    internal sealed class ProcessClientRequest
    {
        private Socket _client;
        private IHttpHandler _handler;
        private HttpServer _server;
        private int _bufferSize = 256;
        private DateTime _begin;

        private const long MAX_BYTE_PER_REQUEST = 100 * 1024;   // 100 KB
        
        public ProcessClientRequest(ref Socket Client, IHttpHandler Handler, HttpServer Server)
        {
            _client = Client;
            _handler = Handler;
            _server = Server;
        }

		public int Send(Byte[] data)
        {
            if (_client == null || data == null || data.Length <= 0)
                return -1;

            try
            {
                if (_client.Poll(100, SelectMode.SelectWrite))
                {
                    int bytesSent = _client.Send(data);

                    return bytesSent;
                }
            }
            catch (SocketException)
            {
                _client.Close();
                _client = null;

                return -1;
            }
            catch (Exception)
            {
                _client.Close();
                _client = null;
            }
            
            return -1;
        }

        private void RaiseError(HttpStatusCode httpStatusCode)
        {
            HttpResponse res = new HttpResponse();

            res.HttpStatus = httpStatusCode;
            res.RaiseError();

            Send(res.GetResponseHeaderBytes());
            Send(res.GetResponseBytes());

            Close();
        } 

        private void Close()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        internal void ProcessRequest()
        {
            using (_client)
            {
                while(true)
                {
                    try
                    {
                        _begin = DateTime.Now;

                        string requestHeader = "";
                        MemoryStream requestBody = null;

                        int avail = 0;
                        byte[] buffer = new byte[_bufferSize];

                        #region Wait for first byte

                        // wait maxWait unil first byte arrives
                        DateTime maxWait = _begin.AddMilliseconds(3000);
                        do
                        {
                            try
                            {
                                avail = _client.Available;
                            }
                            catch
                            {
                                break;
                            }
                        }
                        while (avail == 0 && DateTime.Now <= maxWait);

                        #endregion

                        #region Reading http request header and body

                        ArrayList header = new ArrayList();
                        bool isHeader = true;

                        try
                        {
                            while (_client.Poll(300, SelectMode.SelectRead))
                            {
                                avail = _client.Available;
                                if (avail == 0)
                                    break;
#if(MF)
                                // set all bytes to null byte (strings are ending with null byte in MF)
                                Array.Clear(buffer, 0, buffer.Length);
#endif

                                int bytesRead = _client.Receive(buffer, avail > buffer.Length ? buffer.Length : avail, SocketFlags.None);

                                int c = requestHeader.Length;

#if(MF)
                                requestHeader += new string(Encoding.UTF8.GetChars(buffer));
#else
                                requestHeader += new string(Encoding.UTF8.GetChars(buffer, 0, bytesRead));
#endif

                                if (!isHeader)
                                {
                                    if (bytesRead + requestBody.Length > MAX_BYTE_PER_REQUEST)
                                    {
                                        RaiseError(HttpStatusCode.RequestEntitiyTooLarge);
                                        return;
                                    }

                                    requestBody.Write(buffer, 0, bytesRead);
                                    continue;
                                }

                                int lineBegin = 0;
                                int lineEnd = 0;

                                while (isHeader)
                                {
                                    lineEnd = requestHeader.IndexOf("\r\n", lineBegin);

                                    if (lineEnd < 0)
                                    {
                                        if (lineBegin + 2 < requestHeader.Length)
                                            requestHeader = requestHeader.Substring(lineBegin);
                                        
                                        break;
                                    }

                                    if (lineEnd - lineBegin > 0)
                                        header.Add(requestHeader.Substring(lineBegin, lineEnd - lineBegin));
                                    else
                                    {
                                        isHeader = false;

                                        lineBegin += 2;

                                        requestBody = new MemoryStream();
                                        requestBody.Write(buffer, lineBegin -c, bytesRead - lineBegin +c );

                                        break;
                                    }

                                    lineBegin = lineEnd + 2;
                                }
                            }
                        }
                        catch
                        {
                            break;
                        }

                        #endregion

                        if (header.Count == 0)
                        {
                            RaiseError(HttpStatusCode.BadRequest);
                            return;
                        }

                        // GET /index.htm HTTP/1.1
                        string[] httpRequest = header[0].ToString().Split(' ');
                        bool isHttpPost = false;

                        if (httpRequest.Length != 3)
                        {
                            RaiseError(HttpStatusCode.BadRequest);
                            return;
                        }

                        if (httpRequest[0] != "GET" && httpRequest[0] != "POST")
                        {
                            RaiseError(HttpStatusCode.MethodNotAllowed);
                            return;
                        }

                        if (httpRequest[0] == "POST")
                        {
                            isHttpPost = true;
                        }
                        else
                        {
                            if (requestBody != null && requestBody.Length > 0)
                            {
                                RaiseError(HttpStatusCode.BadRequest);
                                return;
                            }
                        }

                        if (httpRequest[2] != "HTTP/1.1")        // HTTP/1.0 is not used any more
                        {
                            RaiseError(HttpStatusCode.HttpVersionNotSupported);
                            return;
                        }


        
                        HttpRequest request = new HttpRequest();
                        request.HttpMethod = httpRequest[0];
                        request.RawUrl = httpRequest[1];
                        request.HttpVersion = httpRequest[2];


                        request.Headers = new HttpHeader[header.Count - 2];
                        for(int i=1; i<header.Count -1; i++)
                        {
                            string h = header[i].ToString();
                            int hsep = h.IndexOf(": ");
                            request.Headers[i-1] = new HttpHeader(h.Substring(0, hsep), h.Substring(hsep + 2));

                            switch (request.Headers[i - 1].Name)
                            {
                                case "User-Agent":
                                    request.UserAgent = request.Headers[i - 1].Value;
                                    break;
                                case "Connection":
                                    request.Connection = request.Headers[i - 1].Value;
                                    break;
                            }
                        }

                        if (requestBody != null && requestBody.Length > 0)
                        {
                            request.Body = requestBody.ToArray();

                            // TODO: parse MIME content
                        }
                        else if(httpRequest[1].IndexOf("?") >= 0)
                        {
                            string queryString = httpRequest[1].Substring(httpRequest[1].IndexOf("?") + 1);

                            if (queryString != null && queryString.Length > 0)
                            {
                                ArrayList ps = new ArrayList();

                                foreach (string param in queryString.Split('&'))
                                {
                                    string[] keyvalue = param.Split('=');

                                    HttpParameter p = new HttpParameter();
                                    p.Name = keyvalue[0];

                                    if (keyvalue.Length == 2)
                                        p.Value = HttpServerUtility.UrlDecode(keyvalue[1]);

                                    ps.Add(p);
                                }

                                request.Params = new HttpParameter[ps.Count];

                                for (int i = 0; i < ps.Count; i++)
                                    request.Params[i] = ps[i] as HttpParameter;
                            }

                        }









                        //log.ClientIP = (_client.RemoteEndPoint as IPEndPoint).Address;
                        //log.BytesReceived = bytes.Length;
                        //log.Date = begin;
                        //log.Method = request.HttpMethod;
                        //log.RawUri = request.RawUrl;
                        //log.UserAgent = request.UserAgent;

                        //request.Content = body;

                        HttpContext ctx = new HttpContext();
                        ctx.Request = request;
                        ctx.Response = new HttpResponse();

                        ctx.Response.Connection = request.GetHeaderValue("Connection");

                        _handler.ProcessRequest(ctx);


                        if (ctx != null && ctx.Response != null)
                        {
                            int bytesSent = Send(ctx.Response.GetResponseHeaderBytes()) + Send(ctx.Response.GetResponseBytes());

                            //log.BytesSent = bytesSent;
#if(!MF)
                            //log.Duration = (int)(DateTime.Now - log.Date).TotalMilliseconds;
#endif

                            //_server.RaiseLogAccess(log);

                            if (bytesSent > 0)
                            {
                                //if (ctx.Response.Connection.ToLower() == "keep-alive")
                                //{
                                //    Thread.Sleep(10);
                                //    continue;
                                //}
                            }
                        }
                    }
                    catch(Exception)
                    {

                    }

                    break;
                }

                Close();
            }
        }
    }
}
