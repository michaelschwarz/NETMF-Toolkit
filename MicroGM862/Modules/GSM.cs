/* 
 * GSM.cs
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

namespace MFToolkit.MicroGM862.Modules
{
    public class GSM
    {
        #region Public classes and enums

        /// <summary>
        /// Handler for PIN requests
        /// </summary>
        /// <param name="PINType">Type of PIN requested</param>
        /// <returns>Code for PIN Type</returns>
        public delegate string PinRequestHandler(String PINType);

        /// <summary>
        /// Handler for recieving call
        /// </summary>
        public delegate void RecievingCallHandler();

        /// <summary>
        /// Supported Network Bands
        /// </summary>
        public enum NetworkBands
        {
            /// <summary>
            /// GSM 900MHz + DCS 1800MHz
            /// </summary>
            GSM900_DCS1800 = 0,

            /// <summary>
            /// GSM 900MHz + PCS 1900MHz
            /// </summary>
            GSM900_PCS1900 = 1,

            /// <summary>
            /// GMS 850MHz + DCS 1800MHz
            /// </summary>
            GMS850_DCS1800 = 2,

            /// <summary>
            /// GMS 850MHz + PCS 1900MHz
            /// </summary>
            GMS850_PCS1900 = 3
        }

        #endregion

        #region Private properties and functions

        // Device that this module belongs to
        private readonly GM862GPS _device;

        // Indicator if phone function is initialized
        private bool _phone_initialized = false;

        // Event handler for unsolicitated responses, check for phone events
        void _phone_unsolicitated(string Response)
        {
            // Check for phone call
            if (Response == "RING")
            {
                // Call event handler
                if (OnRecievingCall != null) OnRecievingCall();
            }
        }


        #endregion

        #region Public properties

        /// <summary>
        /// True if GSM is succesfully initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _phone_initialized;
            }
        }

        /// <summary>
        /// When checking for GSM Connection allow roaming connections 
        /// to be considered connected
        /// </summary>
        public bool AllowRoaming = false;

        /// <summary>
        /// This function is called when a PIN code is required. 
        /// Function should return valid code for requested PIN type
        /// </summary>
        public PinRequestHandler OnPinRequest;

        /// <summary>
        /// This function is called when a phone call comes in
        /// </summary>
        public RecievingCallHandler OnRecievingCall;

        #endregion

        #region Constructor/Destructor
        
        /// <summary>
        /// Create new GSM module
        /// </summary>
        /// <remarks>The GM862 Driver creates an instance. Don't create a new one!</remarks>
        /// <param name="Device">Device driver this module belongs to</param>
        public GSM(GM862GPS Device)
        {
            this._device = Device;
        }

        #endregion

        /// <summary>
        /// Select Networking Band to use
        /// </summary>
        /// <param name="Band">Band to use</param>
        /// <returns>True on succes</returns>
        public bool SelectNetworkBand(NetworkBands Band)
        {
             // Used for returning response body
            string responseBody;

            // Do a band change
            if (_device.ExecuteCommand("AT#BND=" + ((int) Band).ToString(), out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            return true;
        }

        /// <summary>
        /// Check if there are pending PIN Locks.
        /// If so it will run OnPinRequest for next pending PIN Lock
        /// </summary>
        /// <returns>True if there are pending PIN Locks, False when phone fully unlocked</returns>
        public bool ActivePINLocks()
        {
            // Used for returning response body
            string responseBody;
            
            // Used for sending requested pin
            string pinCode;

            // Do a PIN request
            if (_device.ExecuteCommand("AT+CPIN?", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check response body
            if (responseBody.IndexOf("\r\n+CPIN: ") != 0)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Strip responseBody to only the PIN response
            responseBody = responseBody.Substring(9, responseBody.LastIndexOf('\r') - 9);

            // If ready there are no more pending PIN requests
            if (responseBody == "READY")
                return false;

            // Try unlocking the PIN
            if (OnPinRequest != null)
            {
                // Try to get Pin
                pinCode = OnPinRequest(responseBody);

                // Send PIN code
                if (_device.ExecuteCommand("AT+CPIN=" + pinCode, out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                    throw new GM862Exception(GM862Exception.FAILED_INITIALIZE);
            }

            // There can be more pending PIN locks
            return true;
        }

        /// <summary>
        /// Check if we have a valid GSM registration
        /// </summary>
        /// <returns>True of registrated on GSM Network</returns>
        public bool RegistratedOnGSMNetwork()
        {
            // Used for returning response body
            string responseBody;

            // Do a registration  check
            if (_device.ExecuteCommand("AT+CREG?", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check response body
            if (responseBody.IndexOf("\r\n+CREG: ") != 0)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check registration state
            if ((responseBody[11] == '1') | (AllowRoaming & responseBody[11] == '5'))
                return true;
            else
                return false; 
        }

        /// <summary>
        /// Retrieve current operator name
        /// </summary>
        /// <returns></returns>
        public string OperatorName()
        {
            // String used for executing commands
            string responseBody;

            // Request operator name 
            if (_device.ExecuteCommand("AT+COPS?", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check response body
            if (responseBody.IndexOf("\r\n+COPS: ") != 0)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Split up response
            string[] splitResponse = responseBody.Substring(9, responseBody.Length - 11).Split(',');

            // Check if network name is given
            if (splitResponse.Length < 3)
                return "";

            // Remove quotes
            if (splitResponse[2][0] == '"')
                splitResponse[2] = splitResponse[2].Substring(1, splitResponse[2].Length-2);

            // Return operator name
            return splitResponse[2];
        }


        /// <summary>
        /// Initialize GSM Functionality
        /// </summary>
        public void Initialize()
        {
            // Unlock all Pin codes
            while (ActivePINLocks()) { System.Threading.Thread.Sleep(10); }

            // Add event handler for unsolicitated responses to check for phone events
            _device.OnUnsolicitatedResponse += new GM862GPS.UnsolicitatedResponseHandler(_phone_unsolicitated);

            // We have initialized the phone functionality succesfull
            _phone_initialized = true;
        }

    }
}
