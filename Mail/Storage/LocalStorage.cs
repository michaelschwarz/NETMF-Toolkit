#if(!MF && !WindowsCE)
/* 
 * LocalStorage.cs
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
 * MS   09-06-19    initial ersion
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using MFToolkit.Net.Pop3;

namespace MFToolkit.Net.Mail.Storage
{
    /// <summary>
    /// Represents a local mail storage used for Pop3 and Smtp server
    /// </summary>
    public class LocalStorage : IMailStorage, ISmtpStorage, IPop3Storage
    {
        private string _path;
        private string[] _domains;
        private List<string> _files;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">The directory used to store mails.</param>
        /// <param name="domains">The domains handled by this storage.</param>
        public LocalStorage(string path, params string[] domains)
        {
            _path = path;
            _domains = domains;
        }

        private string GetMailboxFromAddress(MailAddress address)
        {
            return "postmaster";
        }

        private string GetMailboxPath(string mailbox)
        {
            string path = Path.Combine(_path, mailbox);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        #region IMailStorage Members

        public bool Login(string username, string password)
        {
            if(username == "postmaster") {
                return true;
            }

            return false;
        }

        public void Logout(string username)
        {
        }

        #endregion

        #region ISmtpStorage Members

        public bool AcceptRecipient(MailAddress recipient)
        {
            if (new List<string>(_domains).Contains(recipient.Domain.ToLower()) && recipient.LocalPart.ToLower() == "info")
                return true;

            return false;
        }

        public bool AcceptSender(MailAddress sender)
        {
            return true;
        }

        public bool SpoolMessage(MailAddressCollection recipients, string message, out string reply)
        {
            reply = null;

            foreach (MailAddress address in recipients)
            {
                string mailbox = GetMailboxFromAddress(address);
                string path = GetMailboxPath(mailbox);

                File.AppendAllText(Path.Combine(path, Guid.NewGuid() + ".txt"), "Delivered-To: " + address.Address + "\r\n" + message);
            }

            return true;
        }

        #endregion

        #region IPop3Storage Members

        public Pop3MessageInfo[] GetMessageOverview(string mailbox)
        {
            List<Pop3MessageInfo> res = new List<Pop3MessageInfo>();
            _files = new List<string>();

            foreach (string fileName in Directory.GetFiles(GetMailboxPath(mailbox), "*.txt"))
            {
                _files.Add(fileName);
                res.Add(new Pop3MessageInfo { Size = new FileInfo(fileName).Length, UniqueIdentifier = Path.GetFileNameWithoutExtension(fileName) });
            }

            return res.ToArray();
        }

        public string ReadMessage(string mailbox, int idx)
        {
            return File.ReadAllText(_files[idx - 1]);
        }

        public bool DeleteMessage(string mailbox, int idx)
        {
            if(!String.IsNullOrEmpty(_files[idx -1]))
                File.Delete(_files[idx - 1]);

            _files[idx - 1] = null;

            return true;
        }

        #endregion
    }
}
#endif