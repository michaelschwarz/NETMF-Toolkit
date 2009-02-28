/* 
 * Program.cs
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
using System.Threading;
using Microsoft.SPOT;
using MFToolkit.Devices;

namespace MFToolkit.uALFAT_DemoApplication
{
    /// <summary>
    /// Example program to demonstrate the usage of the uALFAT Driver
    /// This example program expects a file called Configuration.xml in the root directory 
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Debug.Print("Initialize uALFAT..");
            using (uALFAT uALFATDevice = new uALFAT(new uALFATCommI2C()))
            {
                // Variables used in this demo
                byte[] FileContents;
                DateTime d;
                uALFAT.DirectoryEntry TestDirectory;

                Debug.Print("Get version:");
                Debug.Print(uALFATDevice.GetVersion());

                Debug.Print("Mounting SD Card");
                if (!uALFATDevice.MountSDCard())
                    throw new Exception("Failed to mount SD");

                Debug.Print("Updating RTC");
                if (!uALFATDevice.SetDateTime(DateTime.Now))
                    throw new Exception("Failed to set RTC");

                Debug.Print("Checking RTC");
                if (!uALFATDevice.GetDateTime(out d))
                    throw new Exception("Failed to read RTC");

                Debug.Print("Check if directory 'Test' exists");
                if (uALFATDevice.FindEntry("Test", out TestDirectory))
                {
                    Debug.Print("Deleting directory 'Test'");
                    if (!uALFATDevice.DeleteDirectory("Test"))
                        throw new Exception("Failed to delete directory 'Test'");
                }

                Debug.Print("Creating new Directory");
                if (!uALFATDevice.CreateDirectory("Test"))
                    throw new Exception("Failed to create directory");

                Debug.Print("Moving to directory Test");
                if (!uALFATDevice.GoToAbsolutePath("\\Test", false))
                    throw new Exception("Failed to move to directory");
                
                Debug.Print("And back to the root");
                if (!uALFATDevice.GoToAbsolutePath("\\", false))
                    throw new Exception("Failed to move to directory");

                Debug.Print("Read contents from Configuration.xml");
                if (!uALFATDevice.ReadAllBytesFromFile("Configuration.xml", out FileContents, 1))
                    throw new Exception("Error reading configuration.xml");

                Debug.Print("Write contents to Configuration2.xml");
                if (!uALFATDevice.WriteAllBytesToFile("Configuration2.xml", false, FileContents, 1))
                    throw new Exception("Error writing to configuration2.xml");

                Debug.Print("Deleting Configuration2.xml");
                if (!uALFATDevice.DeleteFile("Configuration2.xml"))
                    throw new Exception("Failed to delete file");

                Debug.Print("---");
            }

            // Sleep well ;-)
            Thread.Sleep(Timeout.Infinite);
        }
            
    }
}
