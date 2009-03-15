/* 
 * GPRS.cs
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
    public class GPRS
    {

        #region Public classes and enums

        /// <summary>
        /// Supported Protocols for Socket
        /// </summary>
        public enum SocketProtocols
        {
            TCP = 0,
            UDP = 1
        }

        #endregion

        #region Private properties and functions

        // Device this module is associated with
        private readonly GM862GPS _device;

        // Indicator if gprs is initialized
        private bool _gprs_initialized = false;

        #endregion

        #region Public properties

        /// <summary>
        /// When checking for GPRS Connection allow roaming connections 
        /// to be considered connected
        /// </summary>
        public bool AllowRoaming = false;

        /// <summary>
        /// True if GPRS is initialized correctly
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _gprs_initialized;
            }
        }

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Create new GPRS Module
        /// </summary>
        /// <remarks>The GM862 Driver creates an instance. Don't create a new one!</remarks>
        /// <param name="Device">Device driver this module is associated with</param>
        public GPRS(GM862GPS Device)
        {
            this._device = Device;
        }

        #endregion

        /// <summary>
        /// Check if we have a valid GPRS Network registration
        /// </summary>
        /// <returns>True of registrated on GPRS Network</returns>
        public bool RegistratedOnGPRSNetwork()
        {
            // Used for returning response body
            string responseBody;

            // Do a registration  check
            if (_device.ExecuteCommand("AT+CGREG?", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check response body
            if (responseBody.IndexOf("\r\n+CGREG: ") != 0)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check registration state
            if ((responseBody[12] == '1') | (AllowRoaming & responseBody[12] == '5'))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Initialize GPRS Functions
        /// </summary>
        public void Initialize()
        {
            // GPRS is now initialized 
            _gprs_initialized = true;
        }

        /// <summary>
        /// Activate GPRS Context for Data Transfer
        /// </summary>
        /// <param name="ContextID">Context ID to configure</param>
        /// <param name="APN">Access point to use</param>
        /// <param name="Username">Username to use</param>
        /// <param name="Password">Password to use</param>
        /// <param name="StaticIP">IP Adress to use or 0.0.0.0 for DHCP</param>
        /// <returns>True on succes</returns>
        public bool SetContextConfiguration(int ContextID, String APN, String Username, String Password, String StaticIP)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Create new GPRS Context
            if (_device.ExecuteCommand("AT+CGDCONT=" + ContextID.ToString() + ",\"IP\", \"" + APN + "\",\"" + StaticIP + "\",0,0", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            // Set UserID
            if (_device.ExecuteCommand("AT#USERID=\"" + Username + "\"", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            // Set Password
            if (_device.ExecuteCommand("AT#PASSW=\"" + Password + "\"", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false; 
            // Succes
            return true;
        }

        /// <summary>
        /// Activate GPRS Context
        /// </summary>
        /// <param name="ContextID">Context ID to activate</param>
        /// <returns>True on succes</returns>
        public bool ActivateContext(int ContextID)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Check if Context is Active
            if (_device.ExecuteCommand("AT#GPRS?", out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            // Check if context isn't active
            if (responseBody.IndexOf("\r\n#GPRS: 1") == -1)
            {
                // If not, try activating it
                if (_device.ExecuteCommand("AT#GPRS=1", out responseBody, 60000) != AT_Interface.ResponseCodes.OK)
                    return false;
            }

            // Succes
            return true;
        }

        /// <summary>
        /// Setup Socket Configuration
        /// </summary>
        /// <param name="SocketNo">Socket Number (1-6)</param>
        /// <param name="ContextID">Context ID</param>
        /// <param name="PacketSize">Packet size to use for TCP and UDP Data sending (0, 1-1500)</param>
        /// <param name="ExchangeTimeOut">Socket inactivity timeout (0, 1-65535)</param>
        /// <param name="ConnectionTimeout">Timeout to make connection (10-1200 mSec)</param>
        /// <param name="DataTimeOut">Data sending timeout, after this data is also send below package size (0, 1-255)</param>
        /// <returns>True on succes</returns>
        public bool SetSocketConfig(int SocketNo, int ContextID, int PacketSize, int ExchangeTimeOut, int ConnectionTimeout, int DataTimeOut)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Setup socket
            if (_device.ExecuteCommand(
                "AT#SCFG=" + SocketNo.ToString() + "," + ContextID.ToString() + "," + PacketSize.ToString() + "," + ExchangeTimeOut.ToString() + "," + ConnectionTimeout.ToString() + "," + DataTimeOut.ToString()
                , out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            // Succes 
            return true;
        }

        /// <summary>
        /// Setup extended Socket Configuration
        /// </summary>
        /// <param name="SocketNo">Socket Number (1-6)</param>
        /// <param name="RingMode">Ring Mode (0-2)</param>
        /// <param name="DataMode">Data Mode (0 = Text, 1 = Hex)</param>
        /// <param name="KeepAlive">Keep Alive (0, 1-240)</param>
        /// <returns>True on succes</returns>
        public bool SetSocketExtendedConfig(int SocketNo, int RingMode, int DataMode, int KeepAlive)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Setup socket
            if (_device.ExecuteCommand(
                "AT#SCFGEXT=" + SocketNo.ToString() + "," + RingMode.ToString() + "," + DataMode.ToString() + "," + KeepAlive.ToString()
                , out responseBody, 1500) != AT_Interface.ResponseCodes.OK)
                return false;

            // Succes 
            return true;
        }

        /// <summary>
        /// Open connection trough TCP or UDP
        /// </summary>
        /// <param name="SocketNo">Socket Number (1-6)</param>
        /// <param name="Protocol">Protocol (UDP/TCP)</param>
        /// <param name="EndPointAddr">Endpoint address (IP or DNS)</param>
        /// <param name="EndPointPort">Endpoint Port</param>
        /// <param name="CloseOnDisconect">Close On Disconect</param>
        /// <param name="OnlineMode">true to return to IDLE state after connect</param>
        /// <returns>True on succes</returns>
        public bool SocketDail(int SocketNo, SocketProtocols Protocol, string EndPointAddr, int EndPointPort, bool CloseOnDisconect, bool CommandMode)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Select witch response code to expect
            AT_Interface.ResponseCodes expectedResponse = (CommandMode) ? AT_Interface.ResponseCodes.OK : AT_Interface.ResponseCodes.CONNECT;

            // Dail socket
            if (_device.ExecuteCommand(
                "AT#SD=" + SocketNo.ToString() + "," + ((int)Protocol).ToString() + "," + EndPointPort.ToString() + ",\"" + EndPointAddr + "\"," + (CloseOnDisconect ? "0" : "255") + "," + EndPointPort.ToString() + "," + (CommandMode ? "1" : "0"),
                out responseBody,
                60000) != expectedResponse)
                return false;

            // Succes 
            return true;
        }

        /// <summary>
        /// Close opened socket
        /// </summary>
        /// <param name="SocketNo">Socket Number (1-6)</param>
        /// <returns>True on succes</returns>
        public bool SocketShutdown(int SocketNo)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Close socket
            if (_device.ExecuteCommand("AT#SH=" + SocketNo.ToString(), out responseBody, 12000) != AT_Interface.ResponseCodes.OK)
                return false;

            // Succes 
            return true;
        }

        /// <summary>
        /// Restore opened socket and go to Online mode
        /// </summary>
        /// <param name="SocketNo">Socket Number (1-6)</param>
        /// <returns>True on succes</returns>
        public bool SocketRestore(int SocketNo)
        {
            // Check if GPRS functions has been initialized
            if (!_gprs_initialized)
                throw new GM862Exception(GM862Exception.WRONGARGUMENT);

            // Used when executing commands
            string responseBody;

            // Close socket
            if (_device.ExecuteCommand("AT#SO=" + SocketNo.ToString(), out responseBody, 12000) != AT_Interface.ResponseCodes.CONNECT)
                return false;
                
            // Succes 
            return true;
        }

    }
}
