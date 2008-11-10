/* 
 * SOARecord.cs
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

namespace MSchwarz.Net.Dns
{
    /// <summary>
    /// SOA Resource Record (RFC 1035 3.3.13)
    /// </summary>
    [Serializable]
    public class SOARecord : RecordBase
    {
        private readonly string _mname;
        private readonly string _rname;
        private readonly int _serial;
        private readonly int _refresh;
        private readonly int _retry;
        private readonly int _expire;
        private readonly int _minimumTtl;

        #region Public Properties

        /// <summary>
        /// The &lt;domain-name&gt; of the name server that was the
        /// original or primary source of data for this zone.
        /// </summary>
        public string PrimaryNameServer
        {
            get { return _mname; }
        }

        /// <summary>
        /// A &lt;domain-name&gt; which specifies the mailbox of the
        /// person responsible for this zone.
        /// </summary>
        public string ResponsibleMailAddress
        {
            get { return _rname; }
        }

        /// <summary>
        /// The unsigned 32 bit version number of the original copy
        /// of the zone.  Zone transfers preserve this value.  This
        /// value wraps and should be compared using sequence space
        /// arithmetic.
        /// </summary>
        public int Serial
        {
            get { return _serial; }
        }

        /// <summary>
        /// A 32 bit time interval before the zone should be
        /// refreshed.
        /// </summary>
        public int Refresh
        {
            get { return _refresh; }
        }

        /// <summary>
        /// A 32 bit time interval that should elapse before a
        /// failed refresh should be retried.
        /// </summary>
        public int Retry
        {
            get { return _retry; }
        }

        /// <summary>
        /// A 32 bit time value that specifies the upper limit on
        /// the time interval that can elapse before the zone is no
        /// longer authoritative.
        /// </summary>
        public int Expire
        {
            get { return _expire; }
        }

        /// <summary>
        /// The unsigned 32 bit minimum TTL field that should be
        /// exported with any RR from this zone.
        /// </summary>
        public int DefaultTtl
        {
            get { return _minimumTtl; }
        }

        #endregion

        internal SOARecord(DnsReader br)
        {
            _mname = br.ReadDomain();
            _rname = br.ReadDomain();
            _serial = br.ReadInt32();
            _refresh = br.ReadInt32();
            _retry = br.ReadInt32();
            _expire = br.ReadInt32();
            _minimumTtl = br.ReadInt32();
        }

        internal string GetDuration(int d)
        {
            TimeSpan t = new TimeSpan(0, 0, d);

            string s = "(";

            if (t.TotalDays >= 1.0)
            {
                s += (int)t.TotalDays + " day" + ((int)t.TotalDays > 1 ? "s" : "") + " ";
                t = t.Subtract(new TimeSpan((int)t.TotalDays, 0, 0, 0));
            }

            if (t.TotalHours >= 1.0)
            {
                s += (int)t.TotalHours + " hour" + ((int)t.TotalHours > 1 ? "s" : "") + " ";
                t = t.Subtract(new TimeSpan((int)t.TotalHours, 0, 0));
            }

            if (t.TotalMinutes >= 1.0)
            {
                s += (int)t.TotalMinutes + " min" + ((int)t.TotalMinutes > 1 ? "s" : "") + " ";
                t = t.Subtract(new TimeSpan(0, (int)t.TotalMinutes, 0));
            }

            if (t.TotalSeconds >= 1.0)
            {
                s += (int)t.TotalSeconds + " sec" + ((int)t.TotalSeconds > 1 ? "s" : "") + " ";
                t = t.Subtract(new TimeSpan(0, 0, (int)t.TotalSeconds));
            }

            if (s.EndsWith(" "))
                s = s.Substring(0, s.Length - 1);

            return s + ")";
        }

        public override string ToString()
        {
            return string.Format(@"    primary name server = {0}
    responsible mail addr = {1}
    serial  = {2}
    refresh = {3} {7}
    retry   = {4} {8}
    expire  = {5} {9}
    default TTL = {6} {10}",
                _mname,
                _rname,
                _serial.ToString(),
                _refresh.ToString(),
                _retry.ToString(),
                _expire.ToString(),
                _minimumTtl.ToString(),
                GetDuration(_refresh),
                GetDuration(_retry),
                GetDuration(_expire),
                GetDuration(_minimumTtl)
            );

        }
    }
}