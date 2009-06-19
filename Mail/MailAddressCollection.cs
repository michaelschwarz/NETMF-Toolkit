/* 
 * MailAddressCollection.cs
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
using System.Collections;

namespace MFToolkit.Net.Mail
{
    /// <summary>
    /// Store several MailAddress. Will be used i.e. for the recipients list of SmtpServer.
    /// </summary>
    public class MailAddressCollection : CollectionBase
    {
        public MailAddress this[int index]
        {
            get
            {
                return ((MailAddress)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        #region Public Methods

        public int Add(MailAddress value)
        {
            return (List.Add(value));
        }

        public int IndexOf(MailAddress value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, MailAddress value)
        {
            List.Insert(index, value);
        }

        public void Remove(MailAddress value)
        {
            List.Remove(value);
        }

        public bool Contains(MailAddress value)
        {
            // If value is not of type MailAddress, this will return false.
            return List.Contains(value);
        }

        #endregion

        protected override void OnInsert(int index, Object value)
        {
            if (value.GetType() != typeof(MailAddress))
                throw new ArgumentException("value must be of type MailAddress.", "value");
        }

        protected override void OnRemove(int index, Object value)
        {
            if (value.GetType() != typeof(MailAddress))
                throw new ArgumentException("value must be of type MailAddress.", "value");
        }

        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            if (newValue.GetType() != typeof(MailAddress))
                throw new ArgumentException("newValue must be of type MailAddress.", "newValue");
        }

        protected override void OnValidate(Object value)
        {
            if (value.GetType() != typeof(MailAddress))
                throw new ArgumentException("value must be of type MailAddress.");
        }

    }
}
