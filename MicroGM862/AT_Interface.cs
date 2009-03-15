/* 
 * AT_Interface.cs
 * 
 * Copyright (c) 2009, Elze Kool (http://www.microframework.nl)
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

using System.Collections;
using System.Text;
using System.Threading;

using System.IO;
using System.IO.Ports;

// For Parsing and Ringbuffer
using MFToolkit.MicroUtilities;


namespace MFToolkit.MicroGM862
{
    /// <summary>
    /// This class is used for communicating with the GM862 trough the serial interface trough
    /// Hayes compatible AT commands
    /// </summary>
    public class AT_Interface : IDisposable
    {

        #region Public classes and types

        /// <summary>
        /// VT25 AT Response Codes
        /// </summary>
        public enum ResponseCodes
        {
            OK,
            CONNECT,
            RING,
            NO_CARRIER,
            ERROR,
            NO_DIALTONE,
            BUSY,
            NO_ANSWER,
            SEND_SMS_DATA
        }

        /// <summary>
        /// This enum holds all the states the GM862 can be in
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Command State:
            /// The device is in this state when it has send a command but did not recieve a 
            /// valid response code yet
            /// </summary>
            COMMAND,

            /// <summary>
            /// Data state:
            /// The device is in this state when a connection is established. Data should be read and not parsed for AT responses
            /// </summary>
            DATA,

            /// <summary>
            /// Idle state:
            /// The device is in this state when no connection is established and no command is send. All responses recieved are unsolicitated responses
            /// </summary>
            IDLE
        }
        
        /// <summary>
        /// Event handler for unsolicitated responses
        /// </summary>
        /// <param name="Response"></param>
        public delegate void UnsolicitatedResponseHandler(String Response);

        #endregion

        #region Internal properties and functions

        // UART Object
        private SerialPort _uart;

        // FIFO used for reading
        private RingBuffer _inputFIFO;

        // Array used for reading larger blocks into the FIFO (GC)
        private byte[] _readHelper;

        // object used to lock the current state
        private object _statelock = new object();

        // current state gm862 is in, don't use this var but use the public State propery
        private States _state = States.IDLE;

        // true when disposed
        private bool _disposed = false;

        // used by _uartReadLine to store string data that is recieved but not yet returned (timeout etc.)
        private string _bufferedlinedata = "";

        // Data recieved handler
        void _uart_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int BytesToRead;

            // Only accept char data
            if (e.EventType == SerialData.Eof) return;

            // Get number of bytes to read
            lock (_uart)
            {
                BytesToRead = _uart.BytesToRead;
            }

            // First read large chuncks
            while (BytesToRead >= _readHelper.Length)
            {
                // Read large chunck of data to fifo
                lock (_uart)
                {
                    _uart.Read(_readHelper, 0, _readHelper.Length);
                }

                // Lock the input FIFO
                lock (_inputFIFO)
                {
                    _inputFIFO.Write(_readHelper);
                }

                // Check for new buffer length
                BytesToRead = _uart.BytesToRead;
            }

            // Exit if no more bytes to read
            if (BytesToRead <= 0) return;

            // Create buffer for remaing bytes to read
            byte[] remainingData = new byte[BytesToRead];

            // Read rest of the data from FIFO
            lock (_uart)
            {
                _uart.Read(remainingData, 0, remainingData.Length);
            }

            // Lock the input FIFO
            lock (_inputFIFO)
            {
                _inputFIFO.Write(remainingData);
            }
        }

        // Read line from UART Fifo
        private String _uartReadLine(int Timeout)
        {
            // This whole function may be executed only one at a time
            lock (_bufferedlinedata)
            {
                bool CR = false;
                char r;
                String s = _bufferedlinedata;

                // Make timeout datetime object
                DateTime timeoutAt = DateTime.Now.AddMilliseconds(Timeout);

                // Keep working until we 
                while ((DateTime.Now < timeoutAt) | (Timeout == -1))
                {
                    Thread.Sleep(1);

                    // Lock input FIFO
                    lock (_inputFIFO)
                    {
                        // If empty wait
                        if (_inputFIFO.isEmpty())
                            continue;

                        // Read Char from fifo
                        r = (char)_inputFIFO.Read();
                    }

                    // Add character to string
                    s += r;

                    // After recieving more then two chars check for <cr><lf>
                    // This prevents the first <cr><lf> to trigger end of line
                    if (s.Length > 2)
                    {
                        // Check for <cr>
                        if (r == '\r') CR = true;

                        // Check for <lf>
                        if (r == '\n')
                        {
                            // Check if we have already recieved a <cr>
                            if (CR)
                            {
                                // Check for last line enter
                                if (s == "\r\n\r\n")
                                {
                                    s = "\r\n";
                                    continue;
                                }

                                // Valid line end
                                _bufferedlinedata = "";
                                return s;
                            }
                            else
                            {
                                // False <cr> recieved 
                                CR = false;
                            }
                        }

                        // We seem to have recieved an SEND_SMS_MESSAGE response
                        if (s == "\r\n>")
                            return s;

                    }
                }

                // Invalid line, buffer data
                _bufferedlinedata = (s != "\r\n") ? s : "";
                return "";
            }
        }

        // Send string to UART and wait until send
        private void _uartSendString(String S)
        {
            // Get bytes for String
            byte[] DataToSend = System.Text.UTF8Encoding.UTF8.GetBytes(S);

            int bufferPos = 0;
            int bufferWriteLength = 0;

            // Send blocks of data
            while (true)
            {
                // Get remaining number of bytes to write
                bufferWriteLength = DataToSend.Length - bufferPos;

                // Check size, keep it a maximum of 32 bytes at once
                if (bufferWriteLength > 32) bufferWriteLength = 32;

                // Write bytes to UART
                lock (_uart)
                {
                    _uart.Write(DataToSend, bufferPos, bufferWriteLength);
                }

                // Now wait until bytes are send
                while (true)
                {
                    // Exit loop when no more bytes in ouput buffer
                    lock (_uart)
                    {
                        if (_uart.BytesToWrite == 0)
                            break;
                    }

                    Thread.Sleep(1);
                }

                // Increase buffer position
                bufferPos += bufferWriteLength;

                // Check if we have send all
                if (bufferPos == DataToSend.Length)
                    break;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Used SerialPort
        /// </summary>
        public readonly String SerialPort;

        /// <summary>
        /// Used Baudrate
        /// </summary>
        public readonly int Baudrate;

        /// <summary>
        /// When false the response checker is more loose. 
        /// This should only be used in initialisation phase where local echo may be enabled  
        /// </summary>
        public bool Strict = true;

        /// <summary>
        /// Current state the AT Interface is in
        /// </summary>
        public States State
        {
            get
            {
                lock (_statelock)
                {
                    return _state;
                }
            }

            set
            {
                lock (_statelock)
                {
                    _state = value;
                }
            }
        }

        /// <summary>
        /// This function is triggered when the AT Interface driver gets an unsolicated response
        /// </summary>
        public UnsolicitatedResponseHandler OnUnsolicitatedResponse;

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Create new GM862-GPS Driver Interface class
        /// </summary>
        /// <param name="SerialPort">Serialport where GM862-GPS is connected</param>
        /// <param name="Baudrate">Baudrate to use for communication</param>
        public AT_Interface(String SerialPort, int Baudrate)
        {
            // Store parameters
            this.SerialPort = SerialPort;
            this.Baudrate = Baudrate;

            // Create a 64KB Input buffer
            _inputFIFO = new RingBuffer(64 * 1024);

            // Create a 64 Bytes FIFO helper
            _readHelper = new byte[64];

            // Create new UART with 8N1 and RTS Handshaking
            _uart = new SerialPort(SerialPort, Baudrate, Parity.None, 8, StopBits.One);
            _uart.Handshake = Handshake.RequestToSend;

            // Open UART
            _uart.Open();

            // Add DATA Recieved Handler
            _uart.DataReceived += new SerialDataReceivedEventHandler(_uart_DataReceived);
        }

        /// <summary>
        /// Dispose AT Interface
        /// </summary>
        public void Dispose()
        {
            try
            {
                _uart.Close();
                _uart.Dispose();
            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Send raw data to GM862 in DATA mode
        /// </summary>
        /// <param name="Data">Byte array of data to send</param>
        /// <param name="Offset">Index to start sending from</param>
        /// <param name="Count">Bytes to send</param>
        public void SendRawData(byte[] Data, int Offset, int Count)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // We can only write when in DATA mode
            if (State != States.DATA)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check if data is not null
            if (Data == null)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check if we have reached buffer borders
            if ((Offset + Count) > Data.Length)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Variables used for sending
            byte[] DataToSend = new byte[Count];
            int bufferPos = 0;
            int bufferWriteLength = 0;

            // Copy data to array
            Array.Copy(Data, Offset, DataToSend, 0, Count);

            // Send blocks of data
            while (true)
            {
                // Get remaining number of bytes to write
                bufferWriteLength = DataToSend.Length - bufferPos;

                // Check size, keep it a maximum of 32 bytes at once
                if (bufferWriteLength > 32) bufferWriteLength = 32;

                // Write bytes to UART
                lock (_uart)
                {
                    _uart.Write(DataToSend, bufferPos, bufferWriteLength);
                }

                // Now wait until bytes are send
                while (true)
                {
                    // Exit loop when no more bytes in ouput buffer
                    lock (_uart)
                    {
                        if (_uart.BytesToWrite == 0)
                            break;
                    }

                    Thread.Sleep(1);
                }

                // Increase buffer position
                bufferPos += bufferWriteLength;

                // Check if we have send all
                if (bufferPos == DataToSend.Length)
                    break;
            }
        }


        /// <summary>
        /// Read raw data from GM862
        /// </summary>
        /// <param name="Data">Byte array to put data in</param>
        /// <param name="Offset">Offset to start storing</param>
        /// <param name="Count">Bytes to read</param>
        /// <returns>Actual number of bytes read</returns>
        public int ReadRawData(byte[] Data, int Offset, int Count)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // We can only read when in DATA mode
            if (State != States.DATA)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check if data is not null
            if (Data == null)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check if we have reached buffer borders
            if ((Offset + Count) > Data.Length)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Check how many bytes we can read
            lock (_inputFIFO)
            {
                if (Count > _inputFIFO.Count)
                    Count = _inputFIFO.Count;
            }

            // If no data simply return
            if (Count == 0)
                return 0;

            // Read data
            lock (_inputFIFO)
            {
                _inputFIFO.Read(Data, Offset, Count);
            }

            // Return number of bytes read
            return Count;
        }


        /// <summary>
        /// If response is timed out an ERROR response is returned with TIMEOUT as response body
        /// </summary>
        /// <param name="Command">Command to execute</param>
        /// <param name="ResponseBody">Response body</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public ResponseCodes ExecuteCommand(String Command, out String ResponseBody, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // Check current state
            if (State != States.IDLE)
                throw new GM862Exception(GM862Exception.WRONG_STATE);

            // Send Command, this stops the GM862 from sending unsolicitated responses
            _uartSendString(Command);

            // Now check input buffer for remaining data (e.g. unsolicitated responses)
            while (true)
            {
                // Read line from FIFO
                String Unsolicitated = _uartReadLine(250);
                if (Unsolicitated != "")
                {
                    // If response starts with \r\n strip it
                    if ((Unsolicitated[0] == '\r') & (Unsolicitated[1] == '\n'))
                        Unsolicitated = Unsolicitated.Substring(2);

                    // If response ends with \r\n strip it
                    if ((Unsolicitated[Unsolicitated.Length - 2] == '\r') & (Unsolicitated[Unsolicitated.Length - 1] == '\n'))
                        Unsolicitated = Unsolicitated.Substring(0, Unsolicitated.Length - 2);

                    // Check if there is still a response left, if so trigger handler
                    if ((Unsolicitated != "") && (OnUnsolicitatedResponse != null))
                        OnUnsolicitatedResponse(Unsolicitated);

                    // Wait some time, maybe some more text is comming our way
                    Thread.Sleep(150);
                }
                else
                {
                    // Check if FIFO is empty now
                    lock (_inputFIFO)
                    {
                        if (_inputFIFO.isEmpty())
                            break;
                    }
                }
            }

            // Set GM862 in COMMAND state
            State = States.COMMAND;

            // Send newline to GM862
            _uartSendString("\r\n");

            // Now wait for response
            return WaitForResponse(out ResponseBody, Timeout);
        }

        /// <summary>
        /// Sends Hayes Escape Sequence +++
        /// </summary>
        /// <param name="PauseBefore">Pause before sending the escape sequence</param>
        /// <param name="PauseAfter">Pause after sending the escape sequence</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public ResponseCodes EscapeSequence(int PauseBefore, int PauseAfter, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // Wait before we send first + sign
            Thread.Sleep(PauseBefore);

            // Set GM862 in COMMAND state
            State = States.COMMAND;

            // Send escape sequence
            _uartSendString("+++");

            // Clear input FIFO
            lock (_inputFIFO)
                _inputFIFO.Clear();

            // Clear buffered line data
            lock (_bufferedlinedata)
                _bufferedlinedata = "";

            // Wait after we send last + sign
            Thread.Sleep(PauseAfter);

            // Now wait for response
            string ResponseBody;
            return WaitForResponse(out ResponseBody, Timeout);
        }


        /// <summary>
        /// Waits until it recieves a valid response code. 
        /// This command automaticly sets the state to IDLE or DATA
        /// If response is timed out an ERROR response is returned with TIMEOUT as response body
        /// </summary>
        /// <param name="ResponseBody">Response body</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public ResponseCodes WaitForResponse(out String ResponseBody, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // Check current state
            if (State == States.IDLE)
                throw new GM862Exception(GM862Exception.WRONG_STATE);

            // Start with empty response body
            ResponseBody = "";

            // Used to store recieved line
            String recievedLine;

            // Time out at
            DateTime timeoutAt = DateTime.Now.AddMilliseconds(Timeout);

            // Recieve lines until timed out
            while ((DateTime.Now < timeoutAt) | (Timeout == -1))
            {
                // Read line from GM862
                recievedLine = _uartReadLine(Timeout);

                // OK Response
                if (recievedLine == "\r\nOK\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.OK;
                }
                // ERROR Response
                else if (recievedLine == "\r\nERROR\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.ERROR;
                }
                else if (recievedLine.IndexOf("\r\n+CMS ERROR:") == 0)
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.ERROR;
                }
                else if (recievedLine.IndexOf("\r\n+CME ERROR:") == 0)
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.ERROR;
                }
                // RING Response
                else if (recievedLine == "\r\nRING\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.RING;
                }
                // BUSY Response
                else if (recievedLine == "\r\nBUSY\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.BUSY;
                }
                // NO_ANSWER Response
                else if (recievedLine == "\r\nNO ANSWER\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.NO_ANSWER;
                }
                // NO_CARRIER Response
                else if (recievedLine == "\r\nNO CARRIER\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.NO_CARRIER;
                }
                // NO_DAILTONE Response
                else if (recievedLine == "\r\nNO DAILTONE\r\n")
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.NO_DIALTONE;
                }
                // CONNECT Response
                else if (recievedLine.IndexOf("\r\nCONNECT") == 0)
                {
                    State = States.DATA;
                    return ResponseCodes.CONNECT;
                }
                // SEND_SMS_DATA Response
                else if (recievedLine.IndexOf("\r\n>") == 0)
                {
                    State = States.DATA;
                    return ResponseCodes.SEND_SMS_DATA;
                }
                // OK response when in less strict mode
                else if ((!Strict) & (recievedLine.IndexOf("OK\r\n") != -1))
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.OK;
                }
                // ERROR response when in less strict mode
                else if ((!Strict) & (recievedLine.IndexOf("ERROR\r\n") != -1))
                {
                    State = States.IDLE;
                    Thread.Sleep(20);
                    return ResponseCodes.ERROR;
                }

                // Unkown response line add it to to the response body
                ResponseBody += recievedLine;
            }


            // Timeout, return error response
            State = States.IDLE;
            Thread.Sleep(20);
            ResponseBody = "\r\nTIMEOUT:\r\n" + ResponseBody;
            return ResponseCodes.ERROR;
        }

    }
}
