﻿/* 
 * Program.cs		(Demo Application)
 * 
 * Copyright (c) 2008, Michael Schwarz (http://www.schwarz-interactive.de)
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
using System.Collections.Generic;
using System.Text;
using MSchwarz.Net.Zigbee;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using MSchwarz.IO;

namespace ZigbeeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using (XBee xbee = new XBee("COM1", 9600))
            {
                xbee.Open();

                while (true)
                {
                    AtCommand at = new AtCommand("ND", new byte[0], 1);
                    xbee.SendPacket(at.GetPacket());

					Thread.Sleep(10 * 1000);
            
                    AtRemoteCommand rat = new AtRemoteCommand(5526146519841232, 11214, 0x02, "IS", new byte[0], 3);
                    xbee.SendPacket(rat.GetPacket());

                    Thread.Sleep(5*1000);
                }
            }
        }
    }
}
