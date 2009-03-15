/* 
 * NumberFormatter.cs
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

    /// <summary>
    /// Formats a number with the format defined in Format.
    /// 
    /// The function follows the C# Custom Formatting defined at the MSDN document below:
    /// http://msdn.microsoft.com/en-us/library/0c899ak8.aspx
    /// 
    /// The Per mille symbol isn't supported as this is not included in the UTF8 character set.
    /// 
    /// Besides the custom formatting strings the function also supports the X,x,D,d numeric formatting specifiers
    /// </summary>
    /// <param name="Format">Formatting for number</param>
    /// <param name="Number">Number to format</param>
    /// <returns>Formatted number</returns>
    public static class NumberFormatter
    {
        #region Basic culture settings

        private const int CULTURE_GROUPSIZE = 3;
        private const char CULTURE_GROUPSEPERATOR = ',';
        private const char CULTURE_DECIMAL_POINT = '.';

        #endregion

        #region Internal Helper functions

        // Works like IndexOf but checks if the character is escaped
        private static int _IndexOfEscaped(String S, Char C) { return _IndexOfEscaped(S, C, 0); }
        private static int _IndexOfEscaped(String S, Char C, int Offset)
        {
            int pos = -1;
            bool escape = false;

            for (int x = 0; x < S.Length; x++ )
            {
                if (escape)
                {
                    escape = false;
                }
                else if (S[x] == '\\')
                {
                    escape = true;
                }
                else
                {
                    if ((S[x] == C) & (x >= Offset))
                        return x;
                }
            }

            return pos;
        }

        // Works like LastIndexOf but checks if the character is escaped
        private static int _LastIndexOfEscaped(String S, Char C) { return _LastIndexOfEscaped(S, C, 0); }
        private static int _LastIndexOfEscaped(String S, Char C, int Offset)
        {
            int pos = -1;
            bool escape = false;

            for (int x = 0; x < S.Length; x++)
            {
                if (escape)
                {
                    escape = false;
                }
                else if (S[x] == '\\')
                {
                    escape = true;
                }
                else
                {
                    if ((S[x] == C) & (x >= Offset))
                        pos = x;
                }
            }

            return pos;
        }
        
        // Works like _IndexOfEscaped but works on reversed strings where the esacpe 
        // character is after the character to escape
        private static int _ReversedIndexOfEscaped(String S, Char C, int Offset)
        {
            int pos = -1;
            bool escape = false;
            Char C_Next = (char) 0;

            for (int x = 0; x < S.Length; x++)
            {
                if (x < S.Length - 1) { C_Next = S[x + 1]; }
                if (escape)
                {
                    escape = false;
                }
                else if ((x < S.Length - 1) & (C_Next == '\\'))
                {
                    escape = true;
                }
                else
                {
                    if ((S[x] == C) & (x >= Offset))
                        return x;
                }
            }

            return pos;
        }
        
        // Return last digit for integer number
        private static char _SingleDigit(int N)
        {
            N = N % 10;
            return (char)(((char)N) + '0');
        }

        // Reverse String
        private static String _ReverseString(String S)
        {
            String reversedS = "";
            foreach (char c in S)
                reversedS = c + reversedS;
            return reversedS;
        }

        // Convert a numeric string to an integer
        private static int _intval(String S)
        {
            if (S.Length == 0) return 0;
            int r = 0;
            foreach (Char c in S)
            {
                if ("0123456789".IndexOf(c) != -1)
                {
                    r *= 10;
                    r += (int)(c - '0');
                }
            }
            return r;
        }

        #endregion

        #region The formatting functions

        // This routine formats the Fractional part of a number to it's formatting rules
        private static String _formatFractionalPart(String Format, int Number)
        {
            // This will hold the formatted string
            String S = "";

            // Make number positive
            if (Number < 0) Number = -Number;

            // By converting the number to a string we can easily find the MSD (Most Significant Digit)
            String NumberAsString = Number.ToString();

            // Strip it from zero's
            for (int x = NumberAsString.Length-1; x >= 0; x--)
            {
                if (NumberAsString[x] != '0')
                {
                    NumberAsString = NumberAsString.Substring(0, x+1);
                    break;
                }
            }

            int NumberAsStringPos = 0;

            // Check if escaped
            bool escape = false;

            // Go trough format string
            for (int x = 0; x < Format.Length; x++)
            {
                // If last character was an escape character just output it
                if (escape)
                {
                    S += Format[x];
                    escape = false;
                }
                // Check for escape character
                else if (Format[x] == '\\')
                {
                    escape = true;
                }
                else
                {
                    // Check what to do based on current character
                    switch (Format[x])
                    {
                        case '0':
                            if (NumberAsStringPos == NumberAsString.Length)
                            {
                                S += "0";
                            }
                            else
                            {
                                S += NumberAsString[NumberAsStringPos];
                                NumberAsStringPos++;
                            }
                            break;

                        case '#':
                            if (NumberAsStringPos != NumberAsString.Length)
                            {
                                S += NumberAsString[NumberAsStringPos];
                                NumberAsStringPos++;
                            }
                            break;

                        default:
                            S += Format[x];
                            break;
                    }
                }
            }

            return S;
        }

        // This routine formats the Integer part of a number to it's formatting rules
        private static String _formatIntegerPart(String Format, int Number)
        {
            int NumberScalingComma = -1;

            // Search for , (Number Scaling Comma)
            NumberScalingComma = _LastIndexOfEscaped(Format, ',');
            if (NumberScalingComma == (Format.Length - 1))
            {
                // Divide by thousand and continue formatting there
                return _formatIntegerPart(Format.Substring(0, Format.Length - 1), (int)System.Math.Round(Number / 1000F));
            }

            // Reverse format string as we work right to left
            String reversedFormatString = _ReverseString(Format);

            
            // Check if number is negative
            bool NumberNegative = (Number < 0);

            // Internal operator indicating if we should add a +/- for the string start
            bool DisplayNegative = NumberNegative;
            bool DisplayPositive = false;

            // Make number positive
            if (Number < 0) Number = -Number;

            // Storage for formatted string
            String S = "";

            // Check if escaped
            bool escape = false;
            Char C_Next = (char)0;

            // Go trough format string
            for (int x = 0; x < reversedFormatString.Length; x++)
            {
                // Used for escape checking
                if (x < reversedFormatString.Length - 1) { C_Next = reversedFormatString[x + 1]; }

                // Check if previous character was an escape character
                if (escape)
                {
                    // Process next char
                    escape = false;
                }
                else if ((x < reversedFormatString.Length - 1) & (C_Next == '\\'))
                {
                    // Add character to output string and say that next character is the escape character
                    S = reversedFormatString[x] + S;
                    escape = true;
                }
                else
                {
                    // Select what to do based on the current character
                    switch (reversedFormatString[x])
                    {
                        case '0': // Display digit else display 0
                            S = _SingleDigit(Number) + S;
                            Number /= 10;
                            break;

                        case '#': // Display digit if digit not null
                            if (Number == 0) break;
                            S = _SingleDigit(Number) + S;
                            Number /= 10;
                            break;

                        case ',': // Comma indicating grouping
                            break;
   
                        case '-':
                            if (x != reversedFormatString.Length-1) S = reversedFormatString[x] + S;
                            break;

                        case '+': // Display + on positive numbers
                            if (x != reversedFormatString.Length-1)
                                S = reversedFormatString[x] + S;
                            else if (DisplayNegative == false) DisplayPositive = true;
                            break;

                        default: // Just add the character to the string
                            S = reversedFormatString[x] + S;
                            break;
                    }

                    // If we are on the integer side of the number check if this is the last hash or null, if so process the number until
                    // the processing number gets nul
                    if (((reversedFormatString[x] == '#') | (reversedFormatString[x] == '0')) & ((_ReversedIndexOfEscaped(reversedFormatString, '#', x + 1) == -1)) & (_ReversedIndexOfEscaped(reversedFormatString, '0', x + 1) == -1))
                    {
                        while (Number != 0)
                        {
                            S = _SingleDigit(Number) + S;
                            Number /= 10;
                        }
                    }
                }
            }

            // Add number group specificier if requested
            if (NumberScalingComma != -1)
            {
                String S_ = "";
                int GroupSize = 0;
                for (int x = S.Length-1; x >= 0 ; x--)
                {
                    // Add grouping symbol when we got a full block
                    if (GroupSize == CULTURE_GROUPSIZE)
                    {
                        if ((x != 0) & (S[x+1] != '0'))
                        {
                            S_ = CULTURE_GROUPSEPERATOR + S_;
                            GroupSize = 0;
                        }
                        else
                        {
                            // Check if there are more Significant digits
                            bool MoreSignificantDigits = false;
                            foreach (char c in S.Substring(0, x+1))
                            {
                                if ("123456789".IndexOf(c) != -1)
                                {
                                    MoreSignificantDigits = true;
                                    break;
                                }
                            }

                            // Only add comma when there are more significant digits
                            if (MoreSignificantDigits)
                            {
                                S_ = CULTURE_GROUPSEPERATOR + S_;
                                GroupSize = 0;
                            }
                        }
                    }

                    // If number increase current group size
                    if ("0123456789".IndexOf(S[x]) != -1)
                        GroupSize++;

                    // Add character/digit to String
                    S_ = S[x] + S_;
                }

                // Copy temporary S to real S
                S = S_;
            }

            // return formatted string
            if (DisplayNegative)
            {
                return "-" + S;
            }
            else if (DisplayPositive)
            {
                return "+" + S;
            }
            else
            {
                return S;
            }
        }

        // This function formats a section based on its format
        private static String _formatSection(String Format, Double Number)
        {
            // Check for Infinity
            if (Number == Double.NegativeInfinity)
                return "-Infinity";
            else if (Number == Double.PositiveInfinity)
                return "Infinity";

            // String to build formated number
            String S = "";

            // Check for percent Symbol
            int PercentSymbol = _IndexOfEscaped(Format, '%');
            if (PercentSymbol != -1)
            {
                return _formatSection(Format.Substring(0, PercentSymbol), Number * 100) + "%";
            }


            // Check for Exponent Symbol
            int ExponentSymbol = _IndexOfEscaped(Format.ToLower(), 'e');
            if (ExponentSymbol != -1)
            {
                // To be a valid exponent format it must be proceeded by 1 or more zeros
                if (_IndexOfEscaped(Format, '0', ExponentSymbol + 1) != -1)
                {
                    int Exponent = 0;

                    // Number = 1
                    if (Number == 1F)
                    {
                        // 1 has an exponent of 1
                        Exponent = 1;
                    }
                    // Number > 1
                    else if (Number > 1F)
                    {
                        // Numbers above 1 are divided by 10 until it's below 10
                        while ((Number <= -10F) | (Number >= 10F))
                        {
                            Number /= 10F;
                            Exponent++;
                        }
                    }
                    // Number < 1
                    else
                    {
                        // Numbers below 1 are multiplied by 10 until there above 10
                        while ((Number > -1F) & (Number < 1F))
                        {
                            Number *= 10F;
                            Exponent--;
                        }
                    }

                    // Return Exponent
                    return _formatSection(Format.Substring(0, ExponentSymbol), Number) + Format[ExponentSymbol] + _formatIntegerPart(Format.Substring(ExponentSymbol + 1), Exponent);
                }
            }

            if (_IndexOfEscaped(Format, '.') != -1)
            {
                // Get Integer and Fractional part of number
                int IntegerPart = (int)System.Math.Floor(Number);
                int FractionalPart = (int)((Number * 1000000F - (Double)IntegerPart * 1000000F));

                // Get formatted result for Integer Part
                S += _formatIntegerPart(Format.Substring(0, _IndexOfEscaped(Format, '.')), IntegerPart);

                // Get formatted result for Fractional 
                S += CULTURE_DECIMAL_POINT + _formatFractionalPart(Format.Substring(_IndexOfEscaped(Format, '.') + 1), FractionalPart);
            }
            else
            {
                // Round and process as integer
                return _formatIntegerPart(Format, (int)System.Math.Round(Number));
            }

            // Return formatted result
            return S;
        }

        // Used to convert 0-15 to 0-F
        private static readonly char[] ByteToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        // Convert integer to string
        private static String _FormatAsHex(int Number, int Size)
        {
            String S = "";
            bool Negative = (Number < 0);
            if (Negative) Number = -Number;

            // Build string
            while (Number != 0)
            {
                S = ByteToHex[Number % 16] + S;
                Number /= 16;
            }

            // Apply padding
            while (S.Length < Size)
            {
                S = "0" + S;
            }

            if (Negative)
                return "-" + S;
            else
                return S;
        }

        // Convert integer to string
        private static String _FormatAsDecimal(int Number, int Size)
        {
            String S = "";
            bool Negative = (Number < 0);
            if (Negative) Number = -Number;

            // Build string
            while (Number != 0)
            {
                S = ((char)(Number % 10) + '0').ToString();
                Number /= 10;
            }

            // Apply padding
            while (S.Length < Size)
            {
                S = "0" + S;
            }

            if (Negative)
                return "-" + S;
            else
                return S;
        }

        #endregion

        /// <summary>
        /// Formats a number with the format defined in Format.
        /// 
        /// The function follows the C# Custom Formatting defined at the MSDN document below:
        /// http://msdn.microsoft.com/en-us/library/0c899ak8.aspx
        /// 
        /// The Per mille symbol isn't supported as this is not included in the UTF8 character set.
        /// 
        /// Besides the custom formatting strings the function also supports the X,x,D,d numeric formatting specifiers
        /// </summary>
        /// <param name="Format">Formatting for number</param>
        /// <param name="Number">Number to format</param>
        /// <returns>Formatted number</returns>
        public static String FormatNumber(String Format, Double Number)
        {
            if (Format == null)
                throw new NullReferenceException("Format can't be null.");

            if (Format == "")
                throw new ArgumentException("Specify a format.");

            // Check for default formats -> Hex uppercase
            if (Format[0] == 'X')
            {
                if (Format.Length > 1)
                    return _FormatAsHex((int)System.Math.Round(Number), _intval(Format.Substring(1)));
                else
                    return _FormatAsHex((int)System.Math.Round(Number), 0);
            }

            // Check for default formats -> Hex lowercase
            if (Format[0] == 'x')
            {
                if (Format.Length > 1)
                    return _FormatAsHex((int)System.Math.Round(Number), _intval(Format.Substring(1))).ToLower();
                else
                    return _FormatAsHex((int)System.Math.Round(Number), 0).ToLower();
            }

            // Check for default formats -> Decimal
            if ((Format[0] == 'D') | Format[0] == 'd')
            {
                if (Format.Length > 1)
                    return _FormatAsDecimal((int)System.Math.Round(Number), _intval(Format.Substring(1)));
                else
                    return _FormatAsDecimal((int)System.Math.Round(Number), 0);
            }


            // This will hold the found sections
            String[] Sections = new String[] { "", "", "" };

            // Used to find sections
            int SectionCount = 1;
            int SectionSearchPos1 = 0;
            int SectionSearchPos2 = 0;


            SectionSearchPos1 = _IndexOfEscaped(Format, ';');

            // Search for first section splitter
            if (SectionSearchPos1 != -1)
            {
                // Section 1
                Sections[0] = Format.Substring(0, SectionSearchPos1);

                // Search for second section splitter
                SectionCount += 1;
                SectionSearchPos2 = _IndexOfEscaped(Format, ';', SectionSearchPos1 + 1);
                if (SectionSearchPos2 != -1)
                {
                    Sections[1] = Format.Substring(SectionSearchPos1 + 1, SectionSearchPos2 - SectionSearchPos1 - 1);
                    Sections[2] = Format.Substring(SectionSearchPos2 + 1);
                    SectionCount = 3;
                }
                else
                {
                    // Section 2
                    Sections[1] = Format.Substring(SectionSearchPos1+1);
                    SectionCount = 2;
                }

            }
            else
            {
                // Just one section
                Sections[0] = Format;
                SectionCount = 1;
            }

            // One section, The format string applies to all values.
            if (SectionCount == 1)
            {
                return _formatSection(Sections[0], Number);
            }
            // Two Sections, The first section applies to positive values and zeros, and the second section applies to negative values. 
            else if (SectionCount == 2)
            {
                // Check for 0/Positive
                if (Number >= 0F)
                {
                    // Return formatted string formatted as section 1
                    return _formatSection(Sections[0], Number);
                }
                else
                {
                    // If the number to be formatted is negative, but becomes zero after rounding according to the format in the second section, then the resulting zero is formatted according to the first section.
                    if (((_IndexOfEscaped(Sections[1], '.') == -1) & (_IndexOfEscaped(Sections[1].ToLower(), 'e') == -1)) & (System.Math.Round(Number) == 0F))
                    {
                        return _formatSection(Sections[0], 0F);
                    }

                    // Section 2 is for negative numbers, but don't display - sign (Make number positive)
                    return _formatSection(Sections[1], -Number);
                }
            }
            // Three Sections, The first section applies to positive values, the second section applies to negative values, and the third section applies to zeros. 
            else
            {
                // Check for Positive
                if (Number > 0F)
                {
                    // If the number to be formatted is nonzero, but becomes zero after rounding according to the format in the first or second section, then the resulting zero is formatted according to the third section.
                    if (((_IndexOfEscaped(Sections[0], '.') == -1) & (_IndexOfEscaped(Sections[0].ToLower(), 'e') == -1)) & (System.Math.Round(Number) == 0F))
                    {
                        return _formatSection(Sections[2], 0F);
                    }

                    // Return formatted string formatted as section 1
                    return _formatSection(Sections[0], Number);
                }
                // Check for Negative
                else if (Number < 0F)
                {
                    // The second section can be left empty (by having nothing between the semicolons), in which case the first section applies to all nonzero values. 
                    if (Sections[1] == "")
                    {
                        // If the number to be formatted is nonzero, but becomes zero after rounding according to the format in the first or second section, then the resulting zero is formatted according to the third section.
                        if (((_IndexOfEscaped(Sections[0], '.') == -1) & (_IndexOfEscaped(Sections[0].ToLower(), 'e') == -1)) & (System.Math.Round(Number) == 0F))
                        {
                            return _formatSection(Sections[2], 0F);
                        }

                        // Return formatted string formatted as section 1
                        return _formatSection(Sections[0], -Number);
                    }
                    else
                    {
                        // If the number to be formatted is nonzero, but becomes zero after rounding according to the format in the first or second section, then the resulting zero is formatted according to the third section.
                        if (((_IndexOfEscaped(Sections[1], '.') == -1) & (_IndexOfEscaped(Sections[1].ToLower(), 'e') == -1)) & (System.Math.Round(Number) == 0F))
                        {
                            return _formatSection(Sections[2], 0F);
                        }

                        // Return formatted string formatted as section 2
                        return _formatSection(Sections[1], -Number);
                    }
                }
                // Else Zero
                {
                    // Return formatted string formatted as section 3
                    return _formatSection(Sections[2], Number);
                }
            }
        }

    


    }
}
