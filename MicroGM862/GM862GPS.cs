/* 
 * GM862GPS.cs
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


namespace MFToolkit.MicroGM862
{
    public class GM862GPS
    {

        #region Public classes and enums

        /// <summary>
        /// Handle Unsolicitated Response
        /// </summary>
        /// <param name="Response">Response to handle</param>
        public delegate void UnsolicitatedResponseHandler(String Response);

        #endregion

        #region Private properties and functions

        // AT Interface used for communication with the GM862
        private AT_Interface _interface;

        // Object is disposed
        private bool _disposed = false;

        // This list will hold recieved unsolicitated responses until they are handled
        private ArrayList _unsolicitatedResponseQueue = new ArrayList();

        #endregion

        #region Public properties

        /// <summary>
        /// Handle unsoliciated responses. Function is called when HandleUnsolicitatedResponses is invoked
        /// </summary>
        public event UnsolicitatedResponseHandler OnUnsolicitatedResponse;

        /// <summary>
        /// GSM Functions
        /// </summary>
        public Modules.GSM GSM;

        /// <summary>
        /// GPRS Functions
        /// </summary>
        public Modules.GPRS GPRS;

        /// <summary>
        /// Text Messaging Functions
        /// </summary>
        public Modules.TextMessaging TextMessaging;

        /// <summary>
        /// GPS Functions
        /// </summary>
        public Modules.GPS GPS;

        /// <summary>
        /// Networking Functions
        /// </summary>
        public Modules.Networking Networking;


        #endregion

        #region Constructor / Destructor

        /// <summary>
        /// Create new GM862-GPS Driver Instance
        /// </summary>
        /// <param name="Interface">AT Interface used for communication</param>
        public GM862GPS(AT_Interface Interface)
        {
            // Variable used for execution of initializing routine
            String responseBody;

            // Store interface
            this._interface = Interface;

            // Create new UnsolicitatedResponseHandler, this handler puts responses on a queue
            this._interface.OnUnsolicitatedResponse = new AT_Interface.UnsolicitatedResponseHandler(delegate(String Response)
            {
                // Add Response to Queue
                lock (_unsolicitatedResponseQueue.SyncRoot) {
                    _unsolicitatedResponseQueue.Add(Response);
                }
            });

            // Temporarily make the AT Interface less strict
            lock (_interface)
                _interface.Strict = false;

            // Select Extended Instruction Set
            if (ExecuteCommand("AT#SELINT=2", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Reset GM862 to factory defaults
            if (ExecuteCommand("AT&F1", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Disable local echo
            if (ExecuteCommand("ATE0", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Set AT Interface to strict again
            lock (_interface)
                _interface.Strict = true;

            // Set fixed baudrate
            if (ExecuteCommand("AT+IPR=" + Interface.Baudrate.ToString(), out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Setup flow control
            if (ExecuteCommand("AT&K1", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);

            // Clear garbage collected in unsolicitated response queue
            lock (_unsolicitatedResponseQueue.SyncRoot)
                _unsolicitatedResponseQueue.Clear();

            // Load Modules
            GSM = new Modules.GSM(this);
            GPRS = new Modules.GPRS(this);
            TextMessaging = new Modules.TextMessaging(this);
            GPS = new Modules.GPS(this);
            Networking = new Modules.Networking(this);

        }

        /// <summary>
        /// Dispose GM862 device
        /// </summary>
        public void Dispose()
        {
            _interface.Dispose();
            _disposed = true;
        }

        #endregion


        /// <summary>
        /// Handle Queued Unsolicitated Responses
        /// </summary>
        public void HandleUnsolicitatedResponses()
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // This variable will hold the retrieved response
            String responseToHandle = String.Empty;

            // This loop will end on a break
            while (true)
            {
                // Lock Queue
                lock (_unsolicitatedResponseQueue.SyncRoot)
                {
                    // Break when there are no more responses
                    if (_unsolicitatedResponseQueue.Count == 0) break;

                    // Responses in Queue, load first and delete it from Queue
                    responseToHandle = (string)_unsolicitatedResponseQueue[0];
                    _unsolicitatedResponseQueue.RemoveAt(0);
                }

                // Invoke Event handlers
                if (OnUnsolicitatedResponse != null) OnUnsolicitatedResponse(responseToHandle);
            }
        }

         /// <summary>
        /// If response is timed out an ERROR response is returned with TIMEOUT as response body
        /// </summary>
        /// <param name="Command">Command to execute</param>
        /// <param name="ResponseBody">Response body</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public AT_Interface.ResponseCodes ExecuteCommand(String Command, out String ResponseBody, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // Call function trough interface
            lock (_interface)
            {
                return _interface.ExecuteCommand(Command, out ResponseBody, Timeout);
            }
        }

        /// <summary>
        /// Waits until it recieves a valid response code. 
        /// This command automaticly sets the state to IDLE or DATA
        /// If response is timed out an ERROR response is returned with TIMEOUT as response body
        /// </summary>
        /// <param name="ResponseBody">Response body</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public AT_Interface.ResponseCodes WaitForResponse(out String ResponseBody, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            // Call function trough interface
            lock (_interface)
            {
                return _interface.WaitForResponse(out ResponseBody, Timeout);
            }
        }

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

            lock (_interface)
            {
                _interface.SendRawData(Data, Offset, Count);
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

            lock (_interface)
            {
                return _interface.ReadRawData(Data, Offset, Count);
            }
        }

        /// <summary>
        /// Sends Hayes Escape Sequence +++
        /// </summary>
        /// <param name="PauseBefore">Pause before sending the escape sequence</param>
        /// <param name="PauseAfter">Pause after sending the escape sequence</param>
        /// <param name="Timeout">Timeout in mSec to wait for response</param>
        /// <returns>Response type, or ERROR if timed out</returns>
        public AT_Interface.ResponseCodes EscapeSequence(int PauseBefore, int PauseAfter, int Timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new GM862Exception(GM862Exception.DISPOSED);

            return _interface.EscapeSequence(PauseBefore, PauseAfter, Timeout);
        }

    }
}
