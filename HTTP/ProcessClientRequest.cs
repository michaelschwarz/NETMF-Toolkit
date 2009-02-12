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

namespace MSchwarz.Net.Web
{
    internal sealed class ProcessClientRequest
    {
        private Socket _client;
        private IHttpHandler _handler;
        private HttpServer _server;
        private bool _receiving = false;
        private int _bufferSize = 256;
        private int _timeOut = 1000;
        private int _sleepTime = 100;
        
        public ProcessClientRequest(ref Socket Client, IHttpHandler Handler, HttpServer Server)
        {
            _client = Client;
            _handler = Handler;
            _server = Server;
        }

		public int Send(Byte[] data)
        {
            if (_client == null || data == null || data.Length <= 0 || _receiving)
                return -1;

            try
            {
                if (_client.Poll(100, SelectMode.SelectWrite))
                {
                    return _client.Send(data);
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

        public byte[] Receive()
        {
            try
            {
                int cnt = 0;
                _receiving = true;

                ArrayList mem = new ArrayList();

                byte[] buffer = new byte[_bufferSize];

                while (cnt < (_timeOut / _sleepTime))
                {
                    while (_client.Available > 0)
                    {
                        int bytesRead = _client.Receive(buffer, (_client.Available > buffer.Length ? buffer.Length : _client.Available), SocketFlags.None);
                        
                        if (bytesRead <= 0)
                            continue;       // TODO: maybe endless loop?

                        for (int i = 0; i < bytesRead; i++)
                            mem.Add(buffer[i]);
                    }

                    Thread.Sleep(_sleepTime);

                    if (mem.Count > 0 && _client.Available == 0)
                    {
                        _receiving = false;

                        byte[] res = new byte[mem.Count];
                        for (int i = 0; i < mem.Count; i++)
                            res[i] = (byte)mem[i];

                        return res;
                    }
                    else
                    {
                        cnt++;
                    }
                }

                _receiving = false;
                return null;
            }
            catch
            {
                _receiving = false;
                return null;
            }
        }

        //internal NameValueCollection ParseQueryString(string queryString)
        //{
        //    NameValueCollection nameValueCollection = new NameValueCollection();
        //    string[] parts = queryString.Split('&');

        //    foreach (string part in parts)
        //    {
        //        string[] nameValue = part.Split('=');
        //        nameValueCollection.Add(nameValue[0], HttpServerUtility.UrlDecode(nameValue[1]));
        //    }

        //    return nameValueCollection;
        //}

        internal void ProcessRequest()
        {
            using (_client)
            {
                while (true)
                {
                    DateTime begin = DateTime.Now;

                    byte[] bytes = Receive();

                    // TODO: check if this is a keep-alive connection
                    if (bytes == null || bytes.Length == 0)
                    {
                        break;
                    }

                    LogAccess log = new LogAccess();

                    #region Fill Request Object

                    HttpRequest request = new HttpRequest();
                    ArrayList a = new ArrayList();
                    string ss = "";
                    byte[] body = null;

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if ((int)bytes[i] == 10)
                        {
                            if (i > 4)
                            {
                                if (bytes[i - 3] == 13 && bytes[i - 2] == 10 && bytes[i - 1] == 13)
                                {
                                    body = new byte[bytes.Length - i - 1];
                                    for (int c = 0; c < body.Length; c++)
                                    {
                                        body[c] = bytes[c + i + 1];
                                    }
#if(!MF && DEBUG)
                                    Console.WriteLine(Encoding.UTF8.GetString(body));
#endif
                                    break;
                                }
                            }

                            if (a.Count == 0)
                            {
                                string[] httpreq = ss.Split(' ');

                                request.HttpMethod = httpreq[0];
                                request.RawUrl = httpreq[1];
                                request.HttpVersion = httpreq[2];
                            }
                            else if (ss != null && ss.Length > 0)
                            {
                                string[] header = ss.Split(':');

                                if (header.Length > 1)
                                {
                                    string headervalue = ss.Substring(header[0].Length + 1).TrimStart();
                                    switch (header[0])
                                    {
                                        case "Accept-Language":
                                            request.UserLanguages = new string[] { headervalue };     // TODO: split in array
                                            break;

                                        case "Connection":
                                            request.Connection = headervalue;
                                            break;

                                        case "User-Agent":
                                            request.UserAgent = headervalue;
                                            break;

                                        case "Host":
                                            request.UserHostName = headervalue;
                                            break;

                                        case "Referer":
                                            request.Referer = headervalue;
                                            break;

                                        case "Cookie":
                                            request.Cookies = HttpCookie.FromHttpHeader(headervalue);
                                            break;

                                        case "Content-Type":
                                            request.ContentType = headervalue;
                                            break;

                                        case "Content-Length":
                                            request.ContentLength = int.Parse(headervalue);
                                            break;
                                    }

                                }
                            }

                            a.Add(ss);
                            ss = "";

                            continue;
                        }
                        else if ((int)bytes[i] == 13)
                        {
                            continue;
                        }
                        else
                        {
                            ss += (char)bytes[i];
                        }
                    }

                    #endregion

                    log.ClientIP = (_client.RemoteEndPoint as IPEndPoint).Address;
                    log.BytesReceived = bytes.Length;
                    log.Date = begin;
                    log.Method = request.HttpMethod;
                    log.RawUri = request.RawUrl;
                    log.UserAgent = request.UserAgent;

                    request.Content = body;

                    HttpContext ctx = new HttpContext();
                    ctx.Request = request;
                    ctx.Response = new HttpResponse();

                    _handler.ProcessRequest(ctx);


                    if (ctx != null && ctx.Response != null)
                    {
                        int bytesSent = Send(ctx.Response.GetResponseHeaderBytes()) + Send(ctx.Response.GetResponseBytes());

                        log.BytesSent = bytesSent;
#if(!MF)
                        log.Duration = (int)(DateTime.Now - log.Date).TotalMilliseconds;
#endif

                        _server.RaiseLogAccess(log);

                        if ( bytesSent > 0)
                        {
                            

                            if (ctx.Response.Connection.ToLower() == "keep-alive")
                            {
                                Thread.Sleep(10);
                                continue;
                            }
                        }
                    }

                    break;
                }
            }
        }
    }
}
