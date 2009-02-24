/* 
 * HttpServer.cs
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
 * MS   09-02-10    added MT support
 * 
 * 
 */
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using Socket = System.Net.Sockets.Socket;
namespace MSchwarz.Net.Web
{
	public class HttpServer : IDisposable
	{
        private IHttpHandler _httpHandler;
        private int _port = 80;
        private IPAddress _address = IPAddress.Any;
        private Socket _listenSocket;
        private ArrayList _workerThreads = new ArrayList();
        private Thread _thdListener;
        private Thread _thdWorker;
        private bool _stopThreads = true;
        private const int _maxWorkers = 256;        // for AJAX enabled web sites we need a higher max worker process count

        public delegate void LogEventHandler(LogEventType ev, string text);
        public delegate void LogAccessHandler(LogAccess data);

        public event LogEventHandler OnLogEvent;
        public event LogAccessHandler OnLogAccess;


        public HttpServer(IHttpHandler Handler)
        {
            _httpHandler = Handler;
        }

        public HttpServer(int Port, IHttpHandler Handler)
            : this(Handler)
        {
            _port = Port;
        }

        public HttpServer(int Port, IPAddress Address, IHttpHandler Handler)
            : this(Port, Handler)
        {
            _address = Address;
        }

#if(!MF)
        public HttpServer(IPAddress Address, IHttpHandler Handler)
            : this(Handler)
        {
            _address = Address;
        }
#endif

        public bool Start()
        {
            if (_stopThreads)
            {
                _stopThreads = false;

                _thdWorker = new Thread(new ThreadStart(RemoveWorkerThreads));
#if(!MF)
                _thdWorker.Name = "Worker Thread";
#endif
                _thdWorker.Start();

                _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // TODO: check if there is already a binding for the IPEndPoint
                _listenSocket.Bind(new IPEndPoint(_address, _port));
                _listenSocket.Listen(1000);         // TODO: check what value is good

                _thdListener = new Thread(new ThreadStart(ListenerThread));
#if(!MF)
                _thdListener.Name = "Listener Thread";
#endif
                _thdListener.Start();
            }

            return true;
        }

        public bool Stop()
        {
            _stopThreads = true;

            int count = 0;

            if (_thdListener != null && _thdListener.ThreadState != ThreadState.Stopped)
            {
                try
                {
                    _listenSocket.Close();
                    _thdListener.Abort();
                }
                finally
                {
                    _thdListener = null;
                }
            }

            while (++count < 30)
            {
                if (_thdWorker.ThreadState == ThreadState.Stopped)
                {
                    _thdWorker = null;

                    return true;
                }

                Thread.Sleep(10);
            }

            return false;
        }

        private void ListenerThread()
        {
            while (!_stopThreads)
            {
                Socket client = _listenSocket.Accept();

                CreateWorkerProcess(ref client);

                Thread.Sleep(10);
            }
        }

        private void CreateWorkerProcess(ref Socket client)
        {
            int workerCount;

            while (!_stopThreads)        // TODO: add timeout
            {
                lock (_workerThreads)
                {
                    workerCount = _workerThreads.Count;
                }

                if (workerCount < _maxWorkers)
                    break;

                Thread.Sleep(10);
            }

            ProcessClientRequest pcr = new ProcessClientRequest(ref client, _httpHandler, this);

            Thread thd = new Thread(new ThreadStart(pcr.ProcessRequest));
#if(!MF)
            thd.Name = "Client Worker Process";
#endif

            thd.Start();

            lock (_workerThreads)
            {
                _workerThreads.Add(thd);
            }
        }

        private void RemoveWorkerThreads()
        {
            while (!_stopThreads)
            {
                lock (_workerThreads)
                {
                    if (_workerThreads.Count > 0)
                    {
                        for (int i = _workerThreads.Count - 1; i >= 0; i--)
                        {
                            if (((Thread)_workerThreads[i]).ThreadState == ThreadState.Stopped)
                            {
                                 _workerThreads.RemoveAt(i);
                            }
                        }
                    }
                }

                Thread.Sleep(300);
            }
        }

        private void RaiseLogEvent(LogEventType ev, string message)
        {
            if (OnLogEvent != null)
            {
                OnLogEvent(ev, message);
            }
        }

        internal void RaiseLogAccess(LogAccess data)
        {
            if (OnLogAccess != null)
            {
                OnLogAccess(data);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
