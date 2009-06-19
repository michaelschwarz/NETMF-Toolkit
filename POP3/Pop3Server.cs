/* 
 * Pop3Server.cs
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
using MFToolkit.Net.Mail;

namespace MFToolkit.Net.Pop3
{
    public class Pop3Server : IDisposable
    {
        private int _port = 110;
        private IPAddress _address = IPAddress.Any;
        private Socket _listenSocket;
        private ArrayList _workerThreads = new ArrayList();
        private Thread _thdListener;
        private Thread _thdWorker;
        private bool _stopThreads = true;
        private const int _maxWorkers = 256;
        private IPop3Storage _storage;

        #region Events

        //public event LogAccessEventHandler LogAccess;
        //public event ClientConnectedEventHandler ClientConnected;

        #endregion

        #region Constructor

        public Pop3Server()
        {
        }

        public Pop3Server(int Port)
            : this()
        {
            _port = Port;
        }

        public Pop3Server(int Port, IPAddress Address)
            : this(Port)
        {
            _address = Address;
        }

        public Pop3Server(IPAddress Address)
            : this()
        {
            _address = Address;
        }

        public Pop3Server(IPop3Storage storage)
            : this()
        {
            _storage = storage;
        }

        public Pop3Server(IPAddress Address, IPop3Storage storage)
            : this(storage)
        {
            _address = Address;
        }

        public Pop3Server(int Port, IPAddress Address, IPop3Storage storage)
            : this(Address, storage)
        {
            _port = Port;
        }

        #endregion

        #region Public Properties

        public int Port
        {
            get { return _port; }
        }

        public IPAddress Address
        {
            get { return _address; }
        }

        #endregion

        public bool Start()
        {
            try
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
                    _listenSocket.Listen(30);

                    _thdListener = new Thread(new ThreadStart(ListenerThread));
#if(!MF)
                    _thdListener.Name = "Listener Thread";
#endif
                    _thdListener.Start();
                }
            }
            catch (Exception)
            {
                Stop();
                return false;
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
                if (_thdWorker != null && _thdWorker.ThreadState == ThreadState.Stopped)
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
                Socket client = null;

                try
                {
                    client = _listenSocket.Accept();
                }
                catch (Exception)
                {
                    break;
                }

                if (client == null)
                    continue;

                if (!OnClientConnected((client.RemoteEndPoint as IPEndPoint).Address))
                {
#if(MF)
                    client.Close();
#else
                    client.Disconnect(false);
                    client.Close();
#endif
                    continue;
                }

                CreateWorkerProcess(ref client);
            }
        }

        private bool OnClientConnected(IPAddress address)
        {
            //ClientConnectedEventHandler handler = ClientConnected;

            //bool res = true;

            //if (handler != null)
            //    res = handler(this, new ClientConnectedEventArgs(address));

            //return res;

            return true;
        }

        //internal void OnLogAccess(LogAccess data)
        //{
        //    LogAccessEventHandler handler = LogAccess;

        //    if (handler != null)
        //        handler(this, new LogAccessEventArgs(data));
        //}

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

            Pop3Processor proc = new Pop3Processor(client, _storage);

            Thread thd = new Thread(new ThreadStart(proc.ProcessConnection));
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
#if(LOG && !MF && !WindowsCE)
                    if (_workerThreads.Count > 0)
                        Console.WriteLine(_workerThreads.Count + " worker threads (" + Address + ":" + Port + ")");
#endif
                }

                Thread.Sleep(300);
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
