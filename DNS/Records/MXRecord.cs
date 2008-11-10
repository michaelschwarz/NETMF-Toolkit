/* 
 * MXRecord.cs
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
using System.Net;

namespace MSchwarz.Net.Dns
{
    /// <summary>
    /// MX (Mail Exchange) Resource Record (RFC 1035 3.3.9)
    /// </summary>
    [Serializable]
    public class MXRecord : RecordBase, IComparable
    {
        private readonly int _preference;
        private readonly string _domainName;

        #region Public Properties

        /// <summary>
        /// A &lt;domain-name&gt; which specifies a host willing to act as
        /// a mail exchange for the owner name.
        /// </summary>
        public int Preference 
        {
            get { return _preference; }
        }

        /// <summary>
        /// A 16 bit integer which specifies the preference given to
        /// this RR among others at the same owner.  Lower values
        /// are preferred.
        /// </summary>
        public string DomainName
        {
            get { return _domainName; }
        }

        #endregion

        internal MXRecord(DnsReader br)
        {
            _preference = br.ReadInt16();
            _domainName = br.ReadDomain();
        }

        public override string ToString()
        {
            return string.Format("    MX preference ={1,3}, mail exchanger = {0}", _domainName, _preference.ToString());
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            MXRecord mxOther = (MXRecord)obj;

            if (mxOther._preference < _preference) return 1;
            if (mxOther._preference > _preference) return -1;

            return -mxOther._domainName.CompareTo(_domainName);
        }

        public static bool operator == (MXRecord record1, MXRecord record2)
        {
            if (record1 == null)
                throw new ArgumentNullException("record1");

            return record1.Equals(record2);
        }

        public static bool operator != (MXRecord record1, MXRecord record2)
        {
            return !(record1 == record2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (this.GetType() != obj.GetType())
                return false;

            MXRecord mxOther = obj as MXRecord;

            if (mxOther == null)
                return false;

            if (mxOther._preference != _preference)
                return false;

            if (mxOther._domainName != _domainName)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return _preference;
        }

        #endregion
    }
}