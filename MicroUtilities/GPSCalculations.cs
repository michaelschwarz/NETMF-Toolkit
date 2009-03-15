/* 
 * GPSCalculations.cs
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
using Microsoft.SPOT;

namespace MFToolkit.MicroUtilities
{
    public static class GPSCalculations
    {
 
        /// <summary>
        /// Returns the distance between two points in kilometers 
        /// </summary>
        /// <param name="Lat1">Latitude of Point 1</param>
        /// <param name="Lon1">Longitude of Point 1</param>
        /// <param name="Lat2">Latitude of Point 2</param>
        /// <param name="Lon2">Longitude of Point 2</param>
        /// <returns>Distance between point 1 and 2 in kilometers</returns>
        public static double DistanceKM(double Lat1, double Lon1, double Lat2, double Lon2)
        {
            // http://franson.com/forum/topic.asp?TOPIC_ID=6097
            double x = 69.1 * (Lat1 - Lat2);
            double y = 69.1 * (Lon1 - Lon2) * exMath.Cos(Lat2 / (double) 57.29758F);

            double distance = exMath.Sqrt((x * x) + (y * y)); // distance in Miles
            distance = distance * 1.609344; // Conversion to meter

            return distance;

            //// Code based on http://www.zipcodeworld.com/samples/distance.cs.html
            //// Copyright  Hexa Software Development 
            //double theta = Lon1 - Lon2;

            //double dist =   exMath.Sin(Lat1 * (exMath.PI / 180.0)) * exMath.Sin(Lat2 * (exMath.PI / 180.0)) + 
            //                exMath.Cos(Lat1 * (exMath.PI / 180.0)) * exMath.Cos(Lat2 * (exMath.PI / 180.0)) * 
            //                exMath.Cos(theta * (exMath.PI / 180.0));

            //dist = exMath.Acos(dist);
            //dist = dist / (exMath.PI / 180.0);
            //dist = dist * 60F * 1.1515F * 1.609344F;
            //return dist;
        }

        /// <summary>
        /// Returns the distance between two points in miles 
        /// </summary>
        /// <param name="Lat1">Latitude of Point 1</param>
        /// <param name="Lon1">Longitude of Point 1</param>
        /// <param name="Lat2">Latitude of Point 2</param>
        /// <param name="Lon2">Longitude of Point 2</param>
        /// <returns>Distance between point 1 and 2 in miles</returns>
        public static double DistanceM(double Lat1, double Lon1, double Lat2, double Lon2)
        {
            // Code based on http://www.zipcodeworld.com/samples/distance.cs.html
            // Copyright  Hexa Software Development 
            double theta = Lon1 - Lon2;
            double dist = exMath.Sin(Lat1 * (exMath.PI / 180.0)) * exMath.Sin(Lat2 * (exMath.PI / 180.0)) + exMath.Cos(Lat1 * (exMath.PI / 180.0)) * exMath.Cos(Lat2 * (exMath.PI / 180.0)) * exMath.Cos(theta * (exMath.PI / 180.0));
            dist = exMath.Acos(dist);
            dist = dist / (exMath.PI / 180.0);
            dist = dist * 60F * 1.1515F;
            return dist;
        }
    }
}
