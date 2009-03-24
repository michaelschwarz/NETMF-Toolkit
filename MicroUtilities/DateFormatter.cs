/* 
 * DateFormatter.cs
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
 * 
 * MS   09-03-23    changed using culture fields instead of English hard-coded
 * 
 * 
 */
using System;
using Microsoft.SPOT;
using System.Globalization;

namespace MFToolkit.MicroUtilities
{
    /// <summary>
    /// Date Formatter Class
    /// Provides PHP strftime compatible date formatting
    /// http://nl.php.net/strftime
    /// 
    /// ISO 8601:1988 Week numbers and years and timezone names are not supported
    /// </summary>
    public static class DateFormatter
    {
        /// <summary>
        /// Month names to use for conversion
        /// </summary>
        public static String[] Months = new String[]
        {
            "January",
            "February",
            "March",
            "April",
            "May",
            "June",
            "July",
            "August",
            "September",
            "Oktober",
            "November",
            "December"
        };

        /// <summary>
        /// Day of Week names for conversion
        /// </summary>
        public static String[] Days = new String[]
        {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"
        };



        // Return integer string with padding
        private static String _padding(byte length, int value, char padding)
        {
            string ret;
            ret = value.ToString();
            while (ret.Length < length)
                ret = padding + ret;
            return ret;
        }

        // This function returns the week number starting with the first Monday as the first day of the first week 
        private static int _week_first_sunday(DateTime TimeToFormat)
        {
            int CurrentDay = TimeToFormat.DayOfYear;
            int WeekNo = 0;

            // First go to jan 1st
            while (TimeToFormat.DayOfYear != 1)
            {
                TimeToFormat = TimeToFormat.Subtract(new TimeSpan(1, 0, 0, 0));
            }

            // Now go to first sunday
            while (TimeToFormat.DayOfWeek != DayOfWeek.Sunday)
            {
                TimeToFormat = TimeToFormat.AddDays(1);
            }

            // Skip weeks
            while (TimeToFormat.DayOfYear <= CurrentDay)
            {
                WeekNo += 1;
                TimeToFormat = TimeToFormat.AddDays(7);
            }

            return WeekNo;
        }

        // This function returns the week number starting with the first Monday as the first day of the first week 
        private static int _week_first_monday(DateTime TimeToFormat)
        {
            int CurrentDay = TimeToFormat.DayOfYear;
            int WeekNo = 0;

            // First go to jan 1st
            while (TimeToFormat.DayOfYear != 1)
            {
                TimeToFormat = TimeToFormat.Subtract(new TimeSpan(1, 0, 0, 0));
            }

            // Now go to first sunday
            while (TimeToFormat.DayOfWeek != DayOfWeek.Monday)
            {
                TimeToFormat = TimeToFormat.AddDays(1);
            }

            // Skip weeks
            while (TimeToFormat.DayOfYear <= CurrentDay)
            {
                WeekNo += 1;
                TimeToFormat = TimeToFormat.AddDays(7);
            }

            return WeekNo;
        }

        // Return day of week as integer
        // Used this funtion to make sure sunday is 0 and monday = 6 (with offset=0)
        private static int _dayofweek(DayOfWeek d, int offset)
        {
            int ret = 0;
            switch (d)
            {
                case DayOfWeek.Sunday:
                    ret = 0;
                    break;

                case DayOfWeek.Monday:
                    ret = 1;
                    break;

                case DayOfWeek.Tuesday:
                    ret = 2;
                    break;

                case DayOfWeek.Wednesday:
                    ret = 3;
                    break;

                case DayOfWeek.Thursday:
                    ret = 4;
                    break;

                case DayOfWeek.Friday:
                    ret = 5;
                    break;

                case DayOfWeek.Saturday:
                    ret = 6;
                    break;
            }

            ret += offset;
            while (ret > 6) ret -= 6;
            return ret;
        }
    
        // Process special character 
        private static String _processchar(char c, ref DateTime TimeToFormat)
        {
            switch (c)
            {
                case '%': // literal %
                    return "%";

                case 'n': // newline
                    return "\n";

                case 't': // tab
                    return "\t";

                case 'c': // DateTime in local format
                    return TimeToFormat.ToString();

                case 'd': // Day with padding
                    return _padding(2, TimeToFormat.Day, '0');

                case 'D': // Same as %m/%d/%y
                    return _processchar('m', ref TimeToFormat) + "/" + _processchar('d', ref TimeToFormat) + "/" + _processchar('y', ref TimeToFormat);

                case 'm': // Month with padding
                    return _padding(2, TimeToFormat.Month, '0');

                case 'y': // Year w/o century
                    return _padding(2, TimeToFormat.Year % 100, '0');

                case 'Y': // Year w. century
                    return _padding(4, TimeToFormat.Year, '0');

                case 'H': // Hour w. padding (24h format)
                    return _padding(2, TimeToFormat.Hour, '0');

                case 'I': // Hour w. padding (12h format)
                    return _padding(2, TimeToFormat.Hour % 12, '0');

                case 'M': // Minute w. padding
                    return _padding(2, TimeToFormat.Minute, '0');

                case 'S': // Second w. padding
                    return _padding(2, TimeToFormat.Second, '0');

                case 'p': // AM/PM
                    if (TimeToFormat.Hour >= 12) return "PM"; else return "AM";

                case 'P': // am/pm
                    if (TimeToFormat.Hour >= 12) return "pm"; else return "am";

                case 'r': // time in am/pm notation (without seconds)
                    return _processchar('I', ref TimeToFormat) + ":" + _processchar('M', ref TimeToFormat) + " " + _processchar('P', ref TimeToFormat);

                case 'R': // time in 24h notation (without seconds)
                    return _processchar('H', ref TimeToFormat) + ":" + _processchar('M', ref TimeToFormat);

                case 'T': // Same as %H:%M:%S
                    return _processchar('H', ref TimeToFormat) + ":" + _processchar('M', ref TimeToFormat) + ":" + _processchar('S', ref TimeToFormat);

                case 'u': // Day of week (1-7) with monday beeing 1
                    return (_dayofweek(TimeToFormat.DayOfWeek, 1)+1).ToString();

                case 'w': // Day of week (0-6) with sunday beeing 0
                    return _dayofweek(TimeToFormat.DayOfWeek,0).ToString();

                case 'U': // Week number of the current year where week 1 starts at the first sunday
                    return _padding(2, _week_first_sunday(TimeToFormat), '0');

                case 'W': // Week number of the current year where week 1 starts at the first sunday
                    return _padding(2, _week_first_monday(TimeToFormat), '0');

                case 'a': // Abr. weekday name
                    return DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames[_dayofweek(TimeToFormat.DayOfWeek, 0)];

                case 'A': // Full weekday name
                    return DateTimeFormatInfo.CurrentInfo.DayNames[_dayofweek(TimeToFormat.DayOfWeek, 0)];

                case 'b': // Abr month name
                    return DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[TimeToFormat.Month - 1];

                case 'h': // Abr month name
                    return DateTimeFormatInfo.CurrentInfo.AbbreviatedMonthNames[TimeToFormat.Month - 1];

                case 'B': // Full month name
                    return DateTimeFormatInfo.CurrentInfo.MonthNames[TimeToFormat.Month - 1];

                case 'C': // Century
                    return _padding(2, (int) System.Math.Floor(TimeToFormat.Year / 100F), '0');
            }

            return c.ToString();
        }

        /// <summary>
        /// Format TimeToFormat as String with Formatting of Formatter.
        /// </summary>
        /// <param name="Formatter">Format of returned string</param>
        /// <param name="TimeToFormat">Date/Time to format</param>
        /// <returns>Formatted String</returns>
        public static String Format(String Formatter, DateTime TimeToFormat)
        {
            // return string
            string ret = "";

            // true when next char needs processing
            bool processnextchar = false;

            // go trough string
            foreach (char c in Formatter)
            {
                // If process next char process it and go to next
                if (processnextchar)
                {
                    ret += _processchar(c, ref TimeToFormat);
                    processnextchar = false;
                    continue;
                }

                // If percent char process next char skip this one
                if (c == '%')
                {
                    processnextchar = true;
                    continue;
                }

                // if no percent and no processing add it to the return string
                ret += c;
            }

            return ret;
        }
    }
}
