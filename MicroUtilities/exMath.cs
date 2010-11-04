/* 
 * exMath.cs
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
 * EK: 2009, Initial Version
 */

using System;

namespace MFToolkit.MicroUtilities
{
    /// <summary>
    /// Math Library for Micro Framework
    /// Compatible with full .NET Framework System.Math
    /// </summary>
    public static class exMath
    {

        #region Internaly used constants

        const double sq2p1 = 2.414213562373095048802e0F;
        const double sq2m1 = .414213562373095048802e0F;
        const double pio2 = 1.570796326794896619231e0F;
        const double pio4 = .785398163397448309615e0F;
        const double log2e = 1.4426950408889634073599247F;
        const double sqrt2 = 1.4142135623730950488016887F;
        const double ln2 = 6.93147180559945286227e-01F;
        const double atan_p4 = .161536412982230228262e2F;
        const double atan_p3 = .26842548195503973794141e3F;
        const double atan_p2 = .11530293515404850115428136e4F;
        const double atan_p1 = .178040631643319697105464587e4F;
        const double atan_p0 = .89678597403663861959987488e3F;
        const double atan_q4 = .5895697050844462222791e2F;
        const double atan_q3 = .536265374031215315104235e3F;
        const double atan_q2 = .16667838148816337184521798e4F;
        const double atan_q1 = .207933497444540981287275926e4F;
        const double atan_q0 = .89678597403663861962481162e3F;

        #endregion

        /// <summary>
        /// PI
        /// </summary>
        public static readonly double PI = 3.14159265358979323846F;

        /// <summary>
        /// Natural base E
        /// </summary>
        public static readonly double E = 2.71828182845904523536F;

        /// <summary>
        /// Returns the absolute value 
        /// </summary>
        /// <param name="x">A number</param>
        /// <returns>absolute value of x</returns>
        public static double Abs(double x)
        {
            if (x >= 0.0F)
                return x;
            else
                return (-x);
        }

        /// <summary>
        /// Returns the angle whose cosine is the specified number
        /// </summary>
        /// <param name="x">A number representing a cosine</param>
        /// <returns>An angle</returns>
        public static double Acos(double x)
        {
            if ((x > 1.0F) || (x < -1.0F))
                throw new System.ArgumentOutOfRangeException("Domain error");

            return (pio2 - Asin(x));
        }

        /// <summary>
        /// Returns the angle whose sine is the specified number
        /// </summary>
        /// <param name="x">A number representing a sine</param>
        /// <returns>An angle</returns>
        public static double Asin(double x)
        {
            double sign, temp;

            sign = 1.0F;

            if (x < 0.0F)
            {
                x = -x;
                sign = -1.0F;
            }

            if (x > 1.0F)
            {
                throw new System.ArgumentOutOfRangeException("Domain error");
            }

            temp = Sqrt(1.0F - (x * x));

            if (x > 0.7)
            {
                temp = pio2 - Atan(temp / x);
            }
            else
            {
                temp = Atan(x / temp);
            }

            return (sign * temp);
        }

        /// <summary>
        /// Returns the angle whose tangent is the specified number
        /// </summary>
        /// <param name="x">A number representing a tangent</param>
        /// <returns>the arctangent of x</returns>
        public static double Atan(double x)
        {
            if (x > 0.0F)
                return (atans(x));
            else
                return (-atans(-x));
        }

        /// <summary>
        /// Returns the angle whose tangent is the quotient of two specified numbers.
        /// </summary>
        /// <param name="y">The y coordinate of a point</param>
        /// <param name="x">The x coordinate of a point</param>
        /// <returns>the arctangent of x/y</returns>
        public static double Atan2(double y, double x)
        {

            if ((x + y) == x)
            {
                if ((x == 0F) & (y == 0F)) return 0F;

                if (x >= 0.0F)
                    return pio2;
                else
                    return (-pio2);
            }
            else if (y < 0.0F)
            {
                if (x >= 0.0F)
                    return ((pio2 * 2) - atans((-x) / y));
                else
                    return (((-pio2) * 2) + atans(x / y));

            }
            else if (x > 0.0F)
            {
                return (atans(x / y));
            }
            else
            {
                return (-atans((-x) / y));
            }
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the specified number
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>the smallest integer greater than or equal to x</returns>
        public static double Ceiling(double x)
        {
            return System.Math.Ceiling(x);
        }


        /// <summary>
        /// Calculate Cosinus
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Cosinus of Value</returns>
        public static double Cos(double x)
        {
            // This function is based on the work described in
            // http://www.ganssle.com/approx/approx.pdf

            // Make X positive if negative
            if (x < 0) { x = 0.0F - x; }

            // Get quadrand

            // Quadrand 0,  >-- Pi/2
            byte quadrand = 0;

            // Quadrand 1, Pi/2 -- Pi
            if ((x > (System.Math.PI / 2F)) & (x < (System.Math.PI)))
            {
                quadrand = 1;
                x = System.Math.PI - x;
            }

            // Quadrand 2, Pi -- 3Pi/2
            if ((x > (System.Math.PI)) & (x < ((3F * System.Math.PI) / 2)))
            {
                quadrand = 2;
                x = System.Math.PI - x;
            }

            // Quadrand 3 - 3Pi/2 -->
            if ((x > ((3F * System.Math.PI) / 2)))
            {
                quadrand = 3;
                x = (2F * System.Math.PI) - x;
            }

            // Constants used for approximation
            const double c1 = 0.99999999999925182;
            const double c2 = -0.49999999997024012;
            const double c3 = 0.041666666473384543;
            const double c4 = -0.001388888418000423;
            const double c5 = 0.0000248010406484558;
            const double c6 = -0.0000002752469638432;
            const double c7 = 0.0000000019907856854;

            // X squared
            double x2 = x * x; ;

            // Check quadrand
            if ((quadrand == 0) | (quadrand == 3))
            {
                // Return positive for quadrand 0, 3
                return (c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2))))));
            }
            else
            {
                // Return negative for quadrand 1, 2
                return 0.0F - (c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2))))));
            }
        }


        /// <summary>
        /// Returns the hyperbolic cosine of the specified angle
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>hyperbolic cosine of x</returns>
        public static double Cosh(double x)
        {

            if (x < 0.0F) x = -x;

            if (x == 0F)
            {
                return 1F;
            }
            else if (x <= (ln2 / 2))
            {
                return (1 + (_power((Exp(x) - 1), 2) / (2 * Exp(x))));
            }
            else if (x <= 22F)
            {
                return ((Exp(x) + (1 / Exp(x))) / 2);
            }
            else
            {
                return (0.5F * (Exp(x) + Exp(-x)));
            }

        }

        /// <summary>
        /// Returns e raised to the specified power
        /// </summary>
        /// <param name="x">A number specifying a power</param>
        /// <returns>e raised to x</returns>
        public static double Exp(double x)
        {
            double c;
            int n = 1;
            double ex = 1F;
            double m = 1F;

            // exp(x+y) = exp(x) * exp(y)
            // http://www.quinapalus.com/efunc.html
            while (x > 10.000F) { m *= 22026.4657948067; x -= 10F; }
            while (x > 01.000F) { m *= E; x -= 1F; }
            while (x > 00.100F) { m *= 1.10517091807565; x -= 0.1F; }
            while (x > 00.010F) { m *= 1.01005016708417; x -= 0.01F; }

            // if (Abs(x) < (double.Epsilon * 2)) return m;

            // Uses Taylor series 
            // http://www.mathreference.com/ca,tfn.html
            for (int y = 1; y <= 4; y++)
            {
                c = _power(x, y);
                ex += c / (double)n;
                n *= (y + 1);
            }

            return ex * m;

        }

        /// <summary>
        /// Returns a specified number raised to the specified power
        /// </summary>
        /// <param name="x">number to be raised to a power</param>
        /// <param name="y">number that specifies a power</param>
        /// <returns>x raised to the power y</returns>
        public static double Pow(double x, double y)
        {
            double temp = 0F;
            long l;

            if (x <= 0.0F)
            {
                if (x == 0.0F)
                {
                    if (y <= 0.0F)
                        throw new ArgumentException();
                }

                l = (long)Floor(y);
                if (l != y)

                    temp = Exp(y * Log(-x));

                if ((l % 2) == 1)
                    temp = -temp;

                return (temp);
            }

            return (Exp(y * Log(x)));
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>the largest integer less than or equal to x</returns>
        public static double Floor(double x)
        {
            return System.Math.Floor(x);
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified number
        /// </summary>
        /// <param name="x">a Number</param>
        /// <returns>Logaritmic of x</returns>
        public static double Log(double x)
        {
            return Log(x, System.Math.E);
        }

        /// <summary>
        /// Calculate logaritmic value from value with given base
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="newBase">Base to use</param>
        /// <returns>Logaritmic of x</returns>
        public static double Log(double x, double newBase)
        {
            // Based on Python sourcecode from:
            // http://en.literateprograms.org/Logarithm_Function_%28Python%29

            double partial = 0.5F;

            double integer = 0F;
            double fractional = 0.0F;

            if (x == 0.0F) return double.NegativeInfinity;
            if ((x < 1.0F) & (newBase < 1.0F)) throw new ArgumentOutOfRangeException("can't compute Log");

            while (x < 1.0F)
            {
                integer -= 1F;
                x *= newBase;
            }

            while (x >= newBase)
            {
                integer += 1F;
                x /= newBase;
            }

            x *= x;

            while (partial >= 1E-10F)
            {
                if (x >= newBase)
                {
                    fractional += partial;
                    x = x / newBase;
                }
                partial *= 0.5F;
                x *= x;
            }

            return (integer + fractional);
        }

        /// <summary>
        /// Returns the base 10 logarithm of a specified number. 
        /// </summary>
        /// <param name="x">a Number </param>
        /// <returns>Logaritmic of x</returns>
        public static double Log10(double x)
        {
            return Log(x, 10F);
        }

        /// <summary>
        /// Returns the larger of two specified numbers
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="y">a Number</param>
        /// <returns>The larger of two specified numbers</returns>
        public static double Max(double x, double y)
        {
            if (x >= y)
                return x;
            else
                return y;
        }

        /// <summary>
        /// Returns the smaller of two specified numbers
        /// </summary>
        /// <param name="x">a Number</param>
        /// <param name="y">a Number</param>
        /// <returns>The smaller of two specified numbers</returns>
        public static double Min(double x, double y)
        {
            if (x <= y)
                return x;
            else
                return y;
        }

        /// <summary>
        /// Returns the hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>The hyperbolic sine of x</returns>
        public static double Sinh(double x)
        {
            if (x < 0F) x = -x;

            if (x <= 22F)
            {
                double Ex_1 = Tanh(x / 2) * (Exp(x) + 1);
                return ((Ex_1 + (Ex_1 / (Ex_1 - 1))) / 2);
            }
            else
            {
                return (Exp(x) / 2);
            }
        }

        /// <summary>
        /// Returns a value indicating the sign 
        /// </summary>
        /// <param name="x">A signed number.</param>
        /// <returns>A number indicating the sign of x</returns>
        public static double Sign(double x)
        {
            if (x < 0F)
                return -1;
            else if (x == 0F)
                return 0;
            else
                return 1;
        }

        /// <summary>
        /// Calculate Sinus
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Sinus of Value</returns>
        public static double Sin(double x)
        {
            return Cos((System.Math.PI / 2.0F) - x);
        }

        /// <summary>
        /// Returns the square root of a specified number
        /// </summary>
        /// <param name="x">A number</param>
        /// <returns>square root of x</returns>
        public static double Sqrt(double x)
        {
            double i = 0;
            double x1 = 0.0F;
            double x2 = 0.0F;

            if (x == 0F) return 0F;

            while ((i * i) <= x)
                i += 0.1F;

            x1 = i;

            for (int j = 0; j < 10; j++)
            {
                x2 = x;
                x2 /= x1;
                x2 += x1;
                x2 /= 2;
                x1 = x2;
            }

            return x2;

        }

        /// <summary>
        /// Calculate Tangens
        /// </summary>
        /// <param name="x">Value</param>
        /// <returns>Tangens of Value</returns>
        public static double Tan(double x)
        {
            return (Sin(x) / Cos(x));
        }

        /// <summary>
        /// Returns the hyperbolic tangent of the specified angle
        /// </summary>
        /// <param name="x">An angle, measured in radians</param>
        /// <returns>The hyperbolic tangent of x</returns>
        public static double Tanh(double x)
        {
            return (expm1(2F * x) / (expm1(2F * x) + 2F));
        }

        /// <summary>
        /// Calculates the integral part of x to the nearest integer towards zero. 
        /// </summary>
        /// <param name="x">A number to truncate</param>
        /// <returns>integral part of x</returns>
        public static double Truncate(double x)
        {
            if (x == 0F)
                return 0F;
            else if (x > 0F)
                return Floor(x);
            else
                return Ceiling(x);
        }

        #region Internaly used functions

        private static double expm1(double x)
        {
            double u = Exp(x);

            if (u == 1.0F)
                return x;

            if (u - 1.0F == -1.0F)
                return -1.0F;

            return (u - 1.0F) * x / Log(u);
        }

        private static double _power(double x, int c)
        {
            if (c == 0) return 1.0F;

            double ret = x;

            if (c >= 0f)
            {
                for (int _c = 1; _c < c; _c++)
                    ret *= ret;
            }
            else
            {
                for (int _c = 1; _c < c; _c++)
                    ret /= ret;
            }

            return ret;
        }

        private static double atans(double x)
        {
            if (x < sq2m1)
                return (atanx(x));
            else if (x > sq2p1)
                return (pio2 - atanx(1.0F / x));
            else
                return (pio4 + atanx((x - 1.0F) / (x + 1.0F)));
        }

        private static double atanx(double x)
        {
            double argsq;
            double value;

            argsq = x * x;
            value = ((((atan_p4 * argsq + atan_p3) * argsq + atan_p2) * argsq + atan_p1) * argsq + atan_p0);
            value = value / (((((argsq + atan_q4) * argsq + atan_q3) * argsq + atan_q2) * argsq + atan_q1) * argsq + atan_q0);
            return (value * x);

        }

        #endregion
    }
}

