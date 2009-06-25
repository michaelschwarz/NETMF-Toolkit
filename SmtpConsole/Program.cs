/* 
 * Program.cs		(Demo Application)
 * 
 * Copyright (c) 2009, Michael Schwarz (http://www.schwarz-interactive.de)
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
using MFToolkit.Net.Smtp;
using System.Threading;
using MFToolkit.Net.XBee;
using MFToolkit.Net.Pop3;
using MFToolkit.Net.Mail.Storage;

namespace SmtpConsole
{
    class Program
    {
        static LocalStorage storage = new LocalStorage("c:\\temp", "ajaxpro.info", "control-my-world.com", "mftoolkit.net");
        static bool stop = false;

        static void Main(string[] args)
        {
            Thread thd1 = new Thread(new ThreadStart(RunSmtp));
            thd1.IsBackground = true;
            thd1.Start();

            Thread thd2 = new Thread(new ThreadStart(RunPop3));
            thd2.IsBackground = true;
            thd2.Start();

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

    class XBeeMessageSpool
    {
        #region IMessageSpool Members

        public bool SpoolMessage(MailMessage message, out string reply)
        {
            reply = null;

            string subject = message.Subject;

            if (String.IsNullOrEmpty(subject))
            {
                reply = "Missing subject";
                return false;
            }

            string[] parts = subject.Split('|');

            if (parts.Length == 0)
            {
                reply = "Missing command arguments seperator";
                return false;
            }

            if (parts[0] == "NI")
            {
                using (XBee xbee = new XBee("COM4", ApiType.Enabled))
                {
                    xbee.Open();

                    XBeeResponse res = xbee.Execute(new NodeIdentifierCommand());

                    if (res == null)
                    {
                        reply = "Could not execute NI command";
                        return false;
                    }

                    AtCommandResponse atr = res as AtCommandResponse;

                    if (atr != null)
                    {
                        NodeIdentifier ni = NodeIdentifier.Parse(atr);

                        if (ni != null)
                        {
                            reply = "XBee module response: " + ni.GetType() + " = " + ni.Identifier;
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        #endregion
    }
}
