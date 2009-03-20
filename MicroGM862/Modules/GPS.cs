/* 
 * GPS.cs
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
using MFToolkit.MicroUtilities;

namespace MFToolkit.MicroGM862.Modules
{
    public class GPS
    {

        #region Public classes and enums
        #endregion

        #region Private properties and functions

        // The device this module is attached to
        private GM862GPS _device;

        // Indicator if GPS is initialized
        private bool _gps_initialized;

        // Unsolicitated response handler searches for GPS related responses
        private void _gps_unsolicitated(String Response)
        {
        }

        /// <summary>
        /// Internal function used to convert DDDMM.MMMM string to degrees
        /// </summary>
        /// <param name="S">Latitude/Longitude in DDDMM.MMMM format</param>
        /// <returns>Latitude/Longitude in degrees</returns>
        private static double _decode_dm(String S)
        {
            // Internal values used to store values
            double Result = 0F;
            double Degrees = 0F;
            double Minutes = 0F;

            // Internal values used to convert value to double
            bool negativeR = false;
            bool afterDot = false;
            double m = 0.1F;

            foreach (char c in S)
            {
                // Stop when character is not a valid character
                if ("0123456789.EWNS".IndexOf(c) == -1) break;

                // Check for Dot
                if (c == '.')
                {
                    afterDot = true;
                    continue;
                }
                // If West and South Negative number
                else if ((c == 'W') || (c == 'S'))
                {
                    negativeR = true;
                    continue;
                }
                // If North/East ignore 
                else if ((c == 'N') || (c == 'E'))
                {
                    continue;
                }
                // Number
                else if (!afterDot)
                {
                    Degrees *= 10F;
                    Degrees += (double)(c - '0');
                }
                else
                {
                    Degrees += ((double)(c - '0')) * m;
                    m /= 10F;
                }
            }

            // Now convert from DD.MMSSSS to Degrees
            Minutes = Degrees % 100F;
            Degrees = System.Math.Floor(Degrees / 100F);

            Result = Degrees + (Minutes / 60F);

            // Make it negative when in West or South    
            if (negativeR)
                Result = -Result;

            return Result;
        }


        #endregion

        #region Public properties

        /// <summary>
        /// True if GPS is succesfully initialized
        /// </summary>
        public bool Initialized
        {
            get
            {
                return _gps_initialized;
            }
        }

        #endregion

        #region Constructor/Destructor

        /// <summary>
        /// Instantiate GPS Functions for GM862
        /// </summary>
        /// <remarks>The GM862 Driver creates an instance. Don't create a new one!</remarks>
        /// <param name="Device">Device this driver belongs to</param>
        public GPS(GM862GPS Device)
        {
            this._device = Device;
        }

        #endregion

        /// <summary>
        /// Initialize GPS Functions
        /// </summary>
        public void Initialize()
        {
            // Add handler for unsolicitated responses, search for GPS related responses
            _device.OnUnsolicitatedResponse += new GM862GPS.UnsolicitatedResponseHandler(_gps_unsolicitated);

            // We have succesfully initialized
            _gps_initialized = true;
        }

        /// <summary>
        /// Get GPS Location and Fix Data 
        /// </summary>
        /// <param name="Fix">Dimensions of Fix (0 = No fix, 2 = 2D fix, 3 = 3D fix)</param>
        /// <param name="NoSatelites">Number of tracked satelites (Valid when fix>0)</param>
        /// <param name="Latitude">Latitude in degrees (Valid when fix>0)</param>
        /// <param name="Longitude">Longitude in degrees (Valid when fix>0)</param>
        /// <param name="Speed">Speed in KM/H (Valid when fix>0)</param>
        /// <param name="GPSTime">UTC/GPS Time (Valid when fix>0)</param>
        public void ReadGPSData(out byte Fix, out byte NoSatelites, out Double Latitude, out Double Longitude, out Double Speed, out DateTime GPSTime)
        {
            // Used for returning response body
            string responseBody;

            // Request GPS information
            if (_device.ExecuteCommand("AT$GPSACP", out responseBody, 2500) != AT_Interface.ResponseCodes.OK)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Check response body
            if (responseBody.IndexOf("\r\n$GPSACP: ") != 0)
                throw new GM862Exception(GM862Exception.BAD_RESPONSEBODY);

            // Split message on comma
            String[] gpsInformation = responseBody.Substring(11).Split(new char[] { ',' });

            // Check for Fix
            Fix = (byte)NumberParser.StringToInt(gpsInformation[5]);
            if (Fix > 0)
            {
                // If Fix get Satelites, Lat and Lon
                NoSatelites = (byte)NumberParser.StringToInt(gpsInformation[10]);
                Latitude = _decode_dm(gpsInformation[1]);
                Longitude = _decode_dm(gpsInformation[2]);
                Speed = NumberParser.StringToDouble(gpsInformation[7]);

                // Create DateTime Object
                int year = NumberParser.StringToInt(gpsInformation[9].Substring(4, 2)) + 2000;
                int month = NumberParser.StringToInt(gpsInformation[9].Substring(2, 2));
                int day = NumberParser.StringToInt(gpsInformation[9].Substring(0, 2));

                int hour = NumberParser.StringToInt(gpsInformation[0].Substring(0, 2));
                int minute = NumberParser.StringToInt(gpsInformation[0].Substring(2, 2));
                int second = NumberParser.StringToInt(gpsInformation[0].Substring(4, 2));

                GPSTime = new DateTime(year, month, day, hour, minute, second);

            }
            else
            {
                // If not return 0
                NoSatelites = 0;
                Latitude = 0F;
                Longitude = 0F;
                Speed = 0F;
                GPSTime = new DateTime();
            }
        }


    }
}
