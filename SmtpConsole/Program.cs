﻿/* 
 * Program.cs		(Demo Application)
 * 
 * Copyright (c) 2009-2024, Michael Schwarz (http://www.schwarz-interactive.de)
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
using MFToolkit.Net.Mail.Storage;
using MFToolkit.Net.Pop3;
using MFToolkit.Net.Smtp;
using System;
using System.Threading;

namespace SmtpConsole
{
    class Program
    {
        static LocalStorage storage = new LocalStorage("c:\\temp", "mydomain.com");
        static bool stop = false;

        static void Main(string[] args)
        {
            Thread thd1 = new Thread(new ThreadStart(RunSmtp));
            thd1.IsBackground = true;
            thd1.Start();

            Thread thd2 = new Thread(new ThreadStart(RunPop3));
            thd2.IsBackground = true;
            thd2.Start();

            Console.WriteLine("Press any key to stop server...");
            Console.ReadLine();

            Console.WriteLine("Shutting down mail server...");

            stop = true;

            thd1.Join();
            thd2.Join();

            Console.WriteLine("Done.");
        }

        static void RunSmtp()
        {
            SmtpServer smtp = new SmtpServer(storage);
            //smtp.IsSecure = true;
            //smtp.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate("c:\\temp\\michael.cer");
            smtp.Start();

            while (!stop)
            {
                Thread.Sleep(1000);
            }

            smtp.Stop();
        }

        static void RunPop3()
        {
            Pop3Server pop3 = new Pop3Server(storage);
            pop3.Start();

            while (!stop)
            {
                Thread.Sleep(1000);
            }

            pop3.Stop();
        }
    }
}
