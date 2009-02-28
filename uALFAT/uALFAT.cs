/* 
 * uALFAT.cs
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
using System.Text;
using System.Collections;

using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace MFToolkit.Devices
{
    /// <summary>
    /// Interface class for uALFAT SD/SDHC Card Reader/Writer
    /// 
    /// More information on uALFAT-SD: 
    /// http://www.ghielectronics.com/details.php?id=1&sid=2
    /// </summary>
    public class uALFAT : IDisposable
    {
        /// <summary>
        /// Directory Listing 
        /// </summary>
        public class DirectoryEntry
        {
            /// <summary>
            /// True if Folder
            /// </summary>
            public readonly bool Folder;
            
            /// <summary>
            /// True if Volume ID
            /// </summary>
            public readonly bool VolumeID;

            /// <summary>
            /// True if Read Only
            /// </summary>
            public readonly bool Readonly;

            /// <summary>
            /// Size in bytes
            /// </summary>
            public readonly UInt32 Size;

            /// <summary>
            /// Unicode Name
            /// </summary>
            public readonly String Name;

            /// <summary>
            /// Timestamp
            /// </summary>
            public readonly DateTime Timestamp;

            /// <summary>
            /// Create new DirectoryEntry object with data
            /// </summary>
            public DirectoryEntry(bool Folder, bool VolumeID, bool Readonly, UInt32 Size, String Name, DateTime Timestamp)
            {
                this.Folder = Folder;
                this.VolumeID = VolumeID;
                this.Readonly = Readonly;
                this.Size = Size;
                this.Name = Name;
                this.Timestamp = Timestamp;
            }
        }

        #region Privates

        #region Internal variables

        // Communication class
        private IuALFATComm _comm;

        // Device disposed
        private bool _disposed = false;

        // Will hold the current work directory
        private String _workingdirectory = "\\";

        // Used to decode hex
        const String HEXTONIBBLE = "0123456789ABCDEF";

        #endregion

        #region Hex/Datetime encoding/decoding functions

        // Create DateTime object from Hex
        private DateTime _gettimefromhex(String hex)
        {
            // First convert to number
            UInt32 TimeStamp = _fromhex(hex[1], hex[2]);
            TimeStamp = (TimeStamp << 8) | _fromhex(hex[3], hex[4]);
            TimeStamp = (TimeStamp << 8) | _fromhex(hex[5], hex[6]);
            TimeStamp = (TimeStamp << 8) | _fromhex(hex[7], hex[8]);

            // Decode
            int Seconds = (int)(TimeStamp & 0x0000001F) * 2;
            int Minutes = (int)((TimeStamp & 0x0000BE0) >> 5);
            int Hours = (int)((TimeStamp & 0x0000F800) >> 11);

            int Day = (int)((TimeStamp & 0x001F0000) >> 16);
            int Month = (int)((TimeStamp & 0x01E00000) >> 21);
            int Year = (int)((TimeStamp & 0xFE000000) >> 25) + 1980;

            // Check if valid DateTime
            if ((Seconds >= 0) & (Seconds < 60) & (Minutes >= 0) & (Minutes < 60) & (Hours >= 0) & (Hours < 24) &
                (Day >= 1) & (Day < 31) & (Month >= 1) & (Month <= 12) & (Year > 1980))
            {
                // Return
                return new DateTime(Year, Month, Day, Hours, Minutes, Seconds);
            }
            else
            {
                // Return MinValue,invalid DateTime
                return DateTime.MinValue;
            }
        }

        // Create Hex Timestamp from DateTime object
        private String _makehextime(DateTime d)
        {
            // Bits 0..4 Seconds/2
            UInt32 TimeStamp = ((UInt32)(d.Second / 2) & 0x0000001F);

            // Bits 5..10 Minutes
            TimeStamp |= (UInt32)((d.Minute << 5) & 0x0000BE0L); 

            // Bits 11..15 Hours
            TimeStamp |= (UInt32)((d.Hour << 11) & 0x0000F800L); 

            // Bits 16..20 Day
            TimeStamp |= (UInt32)((d.Day << 16) & 0x001F0000L); 

            // Bits 21..24 Month
            TimeStamp |= (UInt32)((d.Month << 21) & 0x01E00000L); 

            // Bits 25..31 Years since 1980
            TimeStamp |= (UInt32) (((d.Year - 1980) << 25) & 0xFE000000L);

            // Create Hex
            char[] TimeStampStr = new char[8];
            TimeStampStr[7] = HEXTONIBBLE[(int)((TimeStamp & 0x0000000FL))];
            TimeStampStr[6] = HEXTONIBBLE[(int)((TimeStamp & 0x000000F0L) >> 4)];
            TimeStampStr[5] = HEXTONIBBLE[(int)((TimeStamp & 0x00000F00L) >> 8)];
            TimeStampStr[4] = HEXTONIBBLE[(int)((TimeStamp & 0x0000F000L) >> 12)];
            TimeStampStr[3] = HEXTONIBBLE[(int)((TimeStamp & 0x000F0000L) >> 16)];
            TimeStampStr[2] = HEXTONIBBLE[(int)((TimeStamp & 0x00F00000L) >> 20)];
            TimeStampStr[1] = HEXTONIBBLE[(int)((TimeStamp & 0x0F000000L) >> 24)];
            TimeStampStr[0] = HEXTONIBBLE[(int)((TimeStamp & 0xF0000000L) >> 28)];

            // Return String
            return new String(TimeStampStr);
        }

        // Create byte from two nibbles
        private byte _fromhex(char nibble1c, char nibble2c)
        {
            int nibble1 = HEXTONIBBLE.IndexOf(nibble1c);
            int nibble2 = HEXTONIBBLE.IndexOf(nibble2c);
            if ((nibble1 == -1) || (nibble2 == -1)) throw new Exception("False character found in Hex String");
            return (byte)((nibble1 * 16) + nibble2);
        }

        // Convert Hex to Unicode string
        private String _hextounicode(String Hex)
        {
            // Make string uppercase
            Hex = Hex.ToUpper();

            // String must have an even number of characters
            if ((Hex.Length % 4) != 0) throw new Exception("Invalid Hex String!");

            // Calculate number of bytes
            int StrSize = Hex.Length / 4;

            // Create new byte buffer
            byte[] RawBytes = new byte[StrSize];

            // Parse whole string
            for (int pos = 0; pos < StrSize; pos++)
            {
                // Add byte to buffer
                RawBytes[pos] = _fromhex(Hex[(pos * 4) + 2], Hex[(pos * 4) + 3]);
            }

            // Make Unicode string
            return new String(System.Text.UTF8Encoding.UTF8.GetChars(RawBytes));
        }

        #endregion

        #region File routines

        // Open File Handle
        // HandleNo - Handle to open (1-4)
        // Mode - R,W,A
        // Name - Filename
        private bool _openfile(byte HandleNo, char Mode, String Name)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send O<sp>nM>Name<cr>
            if (!_sendbyte((byte)'O')) throw new Exception("O");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            if (!_sendbyte((byte)(HandleNo + ((byte)'0')))) throw new Exception("n");
            if (!_sendbyte((byte)Mode)) throw new Exception(Mode.ToString());
            if (!_sendbyte((byte)'>')) throw new Exception(">");
            foreach (char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes
        }

        // Close File handle
        // HandleNo - Handle to close (1-4)
        private bool _closefile(byte HandleNo)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send C<sp>n<cr>
            if (!_sendbyte((byte)'C')) throw new Exception("C");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            if (!_sendbyte((byte)(HandleNo + ((byte)'0')))) throw new Exception("n");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes
        }

        // Write data to file handle
        // HandleNo - File handle to use (1-4)
        // Filler - Padding
        // Count - Number of bytes to read
        // Data - Data to read
        private bool _readdatafromhandle(byte HandleNo, byte Filler, UInt32 Count, out byte[] Data)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Create size string
            char[] SizeStr = new char[8];
            SizeStr[7] = HEXTONIBBLE[(int)((Count & 0x0000000FL))];
            SizeStr[6] = HEXTONIBBLE[(int)((Count & 0x000000F0L) >> 4)];
            SizeStr[5] = HEXTONIBBLE[(int)((Count & 0x00000F00L) >> 8)];
            SizeStr[4] = HEXTONIBBLE[(int)((Count & 0x0000F000L) >> 12)];
            SizeStr[3] = HEXTONIBBLE[(int)((Count & 0x000F0000L) >> 16)];
            SizeStr[2] = HEXTONIBBLE[(int)((Count & 0x00F00000L) >> 20)];
            SizeStr[1] = HEXTONIBBLE[(int)((Count & 0x0F000000L) >> 24)];
            SizeStr[0] = HEXTONIBBLE[(int)((Count & 0xF0000000L) >> 28)];

            // Send R<sp>nM>ssssssss<cr>
            if (!_sendbyte((byte)'R')) throw new Exception("R");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            if (!_sendbyte((byte)(HandleNo + ((byte)'0')))) throw new Exception("n");
            if (!_sendbyte(Filler)) throw new Exception("M");
            if (!_sendbyte((byte)'>')) throw new Exception(">");
            if (!_sendbyte((byte)SizeStr[0])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[1])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[2])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[3])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[4])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[5])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[6])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[7])) throw new Exception("s");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // These are used for parsing response
            String tmp = "";
            byte b;
            int cnt = 0;

            // First read first 4 bytes for response
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
                cnt++;
                if (cnt == 4) break;
            }

            // Check first response
            if (!_checkresponsecode(tmp))
            {
                Data = null;
                return false; // error
            }

            // Create new byte array
            Data = new byte[Count];
            tmp = "";

            // Read data
            for (cnt = 0; cnt < Count; cnt++)
            {
                // If byte failed an error happened
                if (!_readbyte(out b, 750))
                {
                    Data = null;
                    return false; // error
                }
                Data[cnt] = b;
            }

            // Read remaining response
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
            {
                Data = null;
                return false; // error
            }


            return true;

        }

        // Write data to file handle
        // HandleNo - File handle to use (1-4)
        // Data - Data to write
        private bool _writedatatohandle(byte HandleNo, byte[] Data)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Create size string
            char[] SizeStr = new char[8];
            SizeStr[7] = HEXTONIBBLE[(int)((Data.Length & 0x0000000FL))];
            SizeStr[6] = HEXTONIBBLE[(int)((Data.Length & 0x000000F0L) >> 4)];
            SizeStr[5] = HEXTONIBBLE[(int)((Data.Length & 0x00000F00L) >> 8)];
            SizeStr[4] = HEXTONIBBLE[(int)((Data.Length & 0x0000F000L) >> 12)];
            SizeStr[3] = HEXTONIBBLE[(int)((Data.Length & 0x000F0000L) >> 16)];
            SizeStr[2] = HEXTONIBBLE[(int)((Data.Length & 0x00F00000L) >> 20)];
            SizeStr[1] = HEXTONIBBLE[(int)((Data.Length & 0x0F000000L) >> 24)];
            SizeStr[0] = HEXTONIBBLE[(int)((Data.Length & 0xF0000000L) >> 28)];

            // Send W<sp>n>ssssssss<cr>
            if (!_sendbyte((byte)'W')) throw new Exception("W");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            if (!_sendbyte((byte)(HandleNo + ((byte)'0')))) throw new Exception("n");
            if (!_sendbyte((byte)'>')) throw new Exception(">");
            if (!_sendbyte((byte)SizeStr[0])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[1])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[2])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[3])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[4])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[5])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[6])) throw new Exception("s");
            if (!_sendbyte((byte)SizeStr[7])) throw new Exception("s");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // These are used for parsing response
            String tmp = "";
            byte b;

            // Read response
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check first response
            if (!_checkresponsecode(tmp))
            {
                return false; // error
            }

            // Send data
            foreach (byte d in Data)
            {
                if (!_sendbyte(d)) throw new Exception("d");
            }

            tmp = "";

            // Read response
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check second response
            if (!_checkresponsecode(tmp))
            {
                return false; // error
            }

            // Succes
            return true;

        }


      

        #endregion

        #region Basic communications

        // Send one byte to uALFAT device
        // Returns true on succes, false on error
        private bool _sendbyte(byte b)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Call Sendbyte of communication interface
            return _comm.SendByte(b);
        }

        // Read one byte from  uALFAT device
        // timeout = timeout in msec
        // Returns true on succes, false on error (Or no data available)
        private bool _readbyte(out byte b, long timeout)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Call function on communication interface
            return _comm.ReadByte(out b, timeout);
        }

        // Check response string for !00<cr> indicating ok
        private bool _checkresponsecode(String ResponseString)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            if (ResponseString.Length < 4) return false;

            if (ResponseString.Substring(ResponseString.Length - 4, 4) == "!00\r")
                return true;
            else
                return false;
        }

        #endregion

        #endregion

        /// <summary>
        /// Create new uALFAT Instance
        /// <param name="Communication">Communication interface to use</param>
        /// </summary>
        public uALFAT(IuALFATComm Communication)
        {
            // Store communication interface
            _comm = Communication;

            // Open communication with uALFAT
            _comm.Open();

            // Not disposed
            _disposed = false;
        }

        /// <summary>
        /// Dispose uALFAT
        /// </summary>
        public void Dispose()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Close communication
            _comm.Close();

            // Disposed
            _disposed = true;
        }

        /// <summary>
        /// Return version string of uALFAT device
        /// </summary>
        /// <returns></returns>
        public String GetVersion()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send V<cr>
            if (!_sendbyte((byte)'V')) throw new Exception("V");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 250))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                throw new Exception("Failed to get version!");

            // Return version number
            return tmp.Substring(0, tmp.IndexOf('\r'));
        }

        /// <summary>
        /// Mount and Initalize SD Card
        /// </summary>
        /// <returns>True on succes</returns>
        public bool MountSDCard()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send I<cr>
            if (!_sendbyte((byte)'I')) throw new Exception("I");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false;

            // Reset current directory
            _workingdirectory = "\\";

            // succes
            return true;
        }

        /// <summary>
        /// Set current Date and Time of uALFAT RTC
        /// </summary>
        /// <param name="d">Datetime to set</param>
        /// <returns>True on succes</returns>
        public bool SetDateTime(DateTime d)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String DateTimeHex = _makehextime(d);
            String tmp = "";
            byte b;

            // Send S<sp>hextime<cr>
            if (!_sendbyte((byte)'S')) throw new Exception("S");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            foreach (char c in DateTimeHex)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes

        }

        /// <summary>
        /// Get current Date and Time of uALFAT RTC
        /// </summary>
        /// <param name="d">Current RTC Date Time</param>
        /// <returns>True on succes</returns>
        public bool GetDateTime(out DateTime d)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send G<sp>X<cr>
            if (!_sendbyte((byte)'G')) throw new Exception("G");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            if (!_sendbyte((byte)'X')) throw new Exception("X");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
            {
                d = DateTime.MinValue;
                return false;
            }

            // Get DateTime
            d = _gettimefromhex(tmp.Substring(4, 9));

            if (d != DateTime.MinValue)
                return true; // Succes
            else
                return false; // Error
        }

        /// <summary>
        /// Create new Directory
        /// </summary>
        /// <param name="Name"></param>
        /// <returns>True on succes</returns>
        public bool CreateDirectory(String Name)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send M<sp>Name<cr>
            if (!_sendbyte((byte)'M')) throw new Exception("M");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");          
            foreach(char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes

        }

        /// <summary>
        /// Delete directory 
        /// </summary>
        /// <remarks>Directory must be empty</remarks>
        /// <param name="Name">Directory to delete</param>
        /// <returns>True on succes</returns>
        public bool DeleteDirectory(String Name)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send E<sp>Name<cr>
            if (!_sendbyte((byte)'E')) throw new Exception("E");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            foreach (char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes
        }

        /// <summary>
        /// Go to directory
        /// </summary>
        /// <param name="Name">Directory to move to, .. to go up</param>
        /// <returns>True on succes</returns>
        public bool GotoDirectory(String Name)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send A<sp>Name<cr>
            if (!_sendbyte((byte)'A')) throw new Exception("A");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            foreach (char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
            {
                return false;
            }

            // Add to current direcory
            if (Name != "..")
            {
                _workingdirectory += Name + "\\";
            }
            else
            {
                _workingdirectory = _workingdirectory.Substring(0, _workingdirectory.LastIndexOf('\\'));
                _workingdirectory = _workingdirectory.Substring(0, _workingdirectory.LastIndexOf('\\') + 1);
            }

            return true;
        }

        /// <summary>
        /// Go to path
        /// Move to path, automaticly moves up the file tree and can create subdirectories
        /// </summary>
        /// <remarks>Use with care for the moment!</remarks>
        /// <param name="Path">Absolute(!) path to move to</param>
        /// <param name="allowCreation">May create directory if it doesn't exists</param>
        /// <returns>True on succes</returns>
        public bool GoToAbsolutePath(String Path, bool allowCreation)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();


            // Check path
            if (Path.IndexOf("\\") != 0)
                throw new Exception("File path MUST start with \\ !");

            // Split current directory in parts
            String[] CurrentDirectory = _workingdirectory.Split('\\');

            // Split wanted directory in parts
            String[] WantedDirectory = Path.Split('\\');

            // First search how far we need to go up
            int DirectoryEntryUp;
            for (DirectoryEntryUp = 0; DirectoryEntryUp < CurrentDirectory.Length; DirectoryEntryUp++)
            {
                if (DirectoryEntryUp >= WantedDirectory.Length) break;
                if (!(CurrentDirectory[DirectoryEntryUp] == WantedDirectory[DirectoryEntryUp]))
                {
                    // We've reached the destination
                    break;
                }
            }

            // No go back as needed
            for (int DirectoryEntryUp2 = (CurrentDirectory.Length - 1); DirectoryEntryUp2 > DirectoryEntryUp; DirectoryEntryUp2--)
            {
                if (!GotoDirectory(".."))
                    throw new Exception("Failed to move directory!");
            }


            int DirectoryEntryDown;
            for (DirectoryEntryDown = DirectoryEntryUp; DirectoryEntryDown < WantedDirectory.Length; DirectoryEntryDown++)
            {
                if ((WantedDirectory[DirectoryEntryDown] == "") & (DirectoryEntryDown == (WantedDirectory.Length-1)))
                    return true;

                DirectoryEntry CheckIfExists;
                if (FindEntry(WantedDirectory[DirectoryEntryDown], out CheckIfExists))
                {
                    // Entry exists, check if directory
                    if (!CheckIfExists.Folder)
                        return false; // File is folder

                    // Check if we can move directory
                    if (!GotoDirectory(WantedDirectory[DirectoryEntryDown]))
                        throw new Exception("Failed to change directory");
                }
                else
                {
                    // Check if we are allowed to create directory
                    if (!allowCreation)
                        return false; // Can't create directory 

                    if (!CreateDirectory(WantedDirectory[DirectoryEntryDown]))
                        return false; // Can't create directory

                    // Check if we can move directory
                    if (!GotoDirectory(WantedDirectory[DirectoryEntryDown]))
                        throw new Exception("Failed to change directory");
                }
            }

            return true; // succes
        }

        /// <summary>
        /// Return current working directory
        /// </summary>
        /// <returns>String representing working directory</returns>
        public String CurrentDirectory()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            return _workingdirectory;
        }

        /// <summary>
        /// Read file contents in to a byte[] buffer
        /// </summary>
        /// <param name="Name">File to read</param>
        /// <param name="Data">Contects of file</param>
        /// <param name="FileHandle">File handle to use (1-4)</param>
        /// <returns>True on succes</returns>
        public bool ReadAllBytesFromFile(String Name, out byte[] Data, byte FileHandle)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            // Get file info
            DirectoryEntry d;
            if (!FindEntry(Name, out d))
            {
                Data = null;
                return false;
            }

            // Can't read Folder and Volume ID
            if ((d.Folder) | (d.VolumeID))
            {
                Data = null;
                return false;
            }
            
            // Open file for reading
            if (!_openfile(FileHandle, 'R', Name))
            {
                Data = null;
                return false;
            }

            // Create a new Data array
            Data = new byte[d.Size];

            // Read all contents from file
            if (!_readdatafromhandle(FileHandle, 0, d.Size, out Data))
                return false;

            // Close file again
            if (!_closefile(FileHandle))
                return false;

            // Succes
            return true;
        }

        /// <summary>
        /// Write data to file
        /// </summary>
        /// <param name="Name">File to write</param>
        /// <param name="Append">If file exists appends data instead of rewriting it</param>
        /// <param name="Data">Data to write</param>
        /// <param name="FileHandle">File handle to use (1-4)</param>
        /// <returns>True on succes</returns>
        public bool WriteAllBytesToFile(String Name, bool Append, byte[] Data, byte FileHandle)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            if (Append)
            {
                // Open file for writing
                if (!_openfile(FileHandle, 'R', Name))
                    return false;
            }
            else
            {
                // Open file for appending
                if (!_openfile(FileHandle, 'A', Name))
                    return false;
            }

            // Write data to file
            if (!_writedatatohandle(FileHandle, Data))
                return false;

            // Close file 
            if (!_closefile(FileHandle))
                return false;

            // Succes
            return true;
        }

        /// <summary>
        /// Find Entry in current working directory
        /// </summary>
        /// <param name="Name">Entry to find</param>
        /// <param name="Entry">Returned entry if found</param>
        /// <returns>True on succes</returns>
        public bool FindEntry(String Name, out DirectoryEntry Entry)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            String[] tmp2;

            byte b;

            // Send ?<sp>Name<cr>
            if (!_sendbyte((byte)'?')) throw new Exception("?");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            foreach (char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

             // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
            {
                Entry = null;
                return false;
            }

            // Split response on <cr>
            tmp2 = tmp.Split('\r');

            // Get size
            UInt32 Size = _fromhex(tmp2[1][1], tmp2[1][2]);
            Size = (Size << 8) | _fromhex(tmp2[1][3], tmp2[1][4]);
            Size = (Size << 8) | _fromhex(tmp2[1][5], tmp2[1][6]);
            Size = (Size << 8) | _fromhex(tmp2[1][7], tmp2[1][8]);

            // Get Attributes
            byte Attributes = _fromhex(tmp2[1][11], tmp2[1][12]);
            bool Folder = ((Attributes & 0x10) == 0x10);
            bool VolumeID = ((Attributes & 0x08) == 0x08);
            bool Readonly = ((Attributes & 0x01) == 0x01);

            // Get modified time
            DateTime Modified = _gettimefromhex(tmp2[1].Substring(14, 9));

            // Create Directory Entry
            Entry = new DirectoryEntry(Folder, VolumeID, Readonly, Size, Name, Modified);
            return true;
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="Name">File to delete</param>
        /// <returns>True on succes</returns>
        public bool DeleteFile(String Name)
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send D<sp>Name<cr>
            if (!_sendbyte((byte)'D')) throw new Exception("D");
            if (!_sendbyte((byte)' ')) throw new Exception(" ");
            foreach (char c in Name)
                if (!_sendbyte((byte)c)) throw new Exception(c.ToString());
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 750))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                return false; // error
            else
                return true; // succes
        }

        /// <summary>
        /// Initialize listing
        /// </summary>
        public void InitializeListing()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            byte b;

            // Send I<cr>
            if (!_sendbyte((byte)'@')) throw new Exception("@");
            if (!_sendbyte(0x0D)) throw new Exception("<cr>");

            // Read as many bytes as posible
            while (_readbyte(out b, 500))
            {
                // Add chars to string
                tmp += ((char)b);
            }

            // Check response
            if (!_checkresponsecode(tmp))
                throw new Exception("Failed to initialize listing!");
        }

        /// <summary>
        /// Return directory listing of current directory
        /// Automaticly calls InitializeListing() 
        /// </summary>
        /// <returns></returns>
        public DirectoryEntry[] ListDirectory()
        {
            // Check if disposed
            if (_disposed)
                throw new ObjectDisposedException();

            String tmp = "";
            String[] tmp2;

            byte b;

            // First initialize listing
            InitializeListing();

            // The items will be added to this list
            ArrayList DirectoryEntries = new ArrayList();

            // While no errors keep reading directory entries in unicode format
            while (true)
            {
                tmp = "";

                // Send L<cr>
                if (!_sendbyte((byte)'L')) throw new Exception("L");
                if (!_sendbyte((byte)' ')) throw new Exception(" ");
                if (!_sendbyte((byte)':')) throw new Exception(":");
                if (!_sendbyte(0x0D)) throw new Exception("<cr>");

                // Read as many bytes as posible
                while (_readbyte(out b, 500))
                {
                    // Add chars to string
                    tmp += ((char)b);
                }

                // Check response, break on error
                if (!_checkresponsecode(tmp)) break;

                // Split response on <cr>
                tmp2 = tmp.Split('\r');

                // Get Attributes
                byte Attributes = _fromhex(tmp2[1][1], tmp2[1][2]);
                bool Folder = ((Attributes & 0x10) == 0x10);
                bool VolumeID = ((Attributes & 0x08) == 0x08);
                bool Readonly = ((Attributes & 0x01) == 0x01);

                // Get size
                UInt32 Size = _fromhex(tmp2[1][5], tmp2[1][6]);
                Size = (Size << 8) | _fromhex(tmp2[1][7], tmp2[1][8]);
                Size = (Size << 8) | _fromhex(tmp2[1][9], tmp2[1][10]);
                Size = (Size << 8) | _fromhex(tmp2[1][11], tmp2[1][12]);

                // Decode name
                String UnicodeName = _hextounicode(tmp2[2].Substring(0, tmp2[2].Length - 3));

                // Add to list
                DirectoryEntries.Add(new DirectoryEntry(Folder, VolumeID, Readonly, Size, UnicodeName, DateTime.MinValue));
            }

            // Return list
            return (DirectoryEntry[])DirectoryEntries.ToArray(typeof(DirectoryEntry));
        }

    }
}
