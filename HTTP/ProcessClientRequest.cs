/* 
 * ProcessClientRequest.cs
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
 * MS	08-03-24	initial version
 * MS   09-02-10    fixed keep-alive support
 * MS   09-03-09    changed how the request and response is handled
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

namespace MFToolkit.Net.Web
{
    internal sealed class ProcessClientRequest
    {
        private Socket _client;
        private IHttpHandler _handler;
        private HttpServer _server;

        public ProcessClientRequest(ref Socket Client, IHttpHandler Handler, HttpServer Server)
        {
            _client = Client;
            _handler = Handler;
            _server = Server;
        }

        private void Close()
        {
            if (_client != null)
            {
#if(!MF)
                _client.Shutdown(SocketShutdown.Both);
#endif
                _client.Close();
                _client = null;
            }
        }

        internal void ProcessRequest()
        {
            using (_client)
            {
                while (true)
                {
#if(DEBUG && !MF && !WindowsCE)
                    Console.WriteLine((_client.RemoteEndPoint as IPEndPoint).ToString());
#endif

                    #region Wait for first byte (used for keep-alive, too)

                    int avail = 0;

                    DateTime maxWait = DateTime.Now.AddMilliseconds(2000);
                    do
                    {
                        try
                        {
                            avail = _client.Available;

                            if (avail == 0)
                                Thread.Sleep(10);
                        }
                        catch
                        {
                            break;
                        }
                    }
                    while (avail == 0 && DateTime.Now <= maxWait);

                    #endregion

                    if (avail == 0)
                        break;

                    DateTime begin = DateTime.Now;

                    HttpRequest httpRequest = new HttpRequest();
                    HttpResponse httpResponse = null;

                    try
                    {
                        httpRequest.ReadSocket(_client);
                    }
                    catch (HttpException ex)
                    {
                        httpResponse = new HttpResponse();
                        httpResponse.RaiseError(ex.Message, ex.Code);
                        httpResponse.AddHeader("Connection", "close");
                    }
                    catch (Exception)
                    {
                        httpResponse = new HttpResponse();
                        httpResponse.RaiseError();
                        httpResponse.AddHeader("Connection", "close");
                    }

                    if (httpResponse == null)
                    {
                        httpResponse = new HttpResponse();

                        HttpContext ctx = new HttpContext();
                        ctx.Request = httpRequest;
                        ctx.Response = httpResponse;

                        try
                        {
                            _handler.ProcessRequest(ctx);
                        }
                        catch (HttpException ex)
                        {
                            httpResponse = new HttpResponse();
                            httpResponse.RaiseError(ex.Message, ex.Code);
                            httpResponse.AddHeader("Connection", "close");
                        }
                        catch (Exception)
                        {
                            httpResponse = new HttpResponse();
                            httpResponse.RaiseError();
                            httpResponse.AddHeader("Connection", "close");
                        }
                    }

                    httpResponse.WriteSocket(_client);

                    LogAccess log = new LogAccess();
                    log.ClientIP = httpRequest.UserHostAddress;
                    log.BytesReceived = httpRequest.totalBytes;
                    log.BytesSent = httpResponse.totalBytes;
                    log.Date = begin;
                    log.Method = httpRequest.HttpMethod;
                    log.RawUrl = httpRequest.RawUrl;
                    log.UserAgent = httpRequest.UserAgent;
                    log.HttpReferer = httpRequest.Referer;

#if(MF)
                    log.Duration = (DateTime.Now.Ticks - begin.Ticks) / TimeSpan.TicksPerMillisecond;
#else
                    log.Duration = (long)(DateTime.Now - begin).TotalMilliseconds;
#endif

                    _server.RaiseLogAccess(log);

                    if (httpResponse.Connection == null || httpResponse.Connection != "Keep-Alive")
                        break;

                    Thread.Sleep(15);
                }

                Close();
            }
        }
    }
}
