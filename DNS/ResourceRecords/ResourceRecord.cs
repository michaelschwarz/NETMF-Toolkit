﻿/* 
 * ResourceRecord.cs
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
using System;

namespace MFToolkit.Net.Dns
{
    /// <summary>
    /// Represents a Resource Record (RFC1035 4.1.3)
    /// </summary>
    [Serializable]
    public class ResourceRecord
    {
        private string _domain;    // NAME
        private DnsType _qtype;
        private DnsClass _qclass;
        private int _ttl;
        private RecordBase _record;

        #region Public Properties

        /// <summary>
        /// A domain name to which this resource record pertains.
        /// </summary>
        public string Domain
        {
            get { return _domain; }
            internal set { _domain = value; }
        }
        
        public DnsType Type 
        { 
            get { return _qtype; }
            internal set { _qtype = value; }
        }
        
        public DnsClass Class 
        { 
            get { return _qclass; }
            internal set { _qclass = value; }
        }
        
        /// <summary>
        /// A 32 bit unsigned integer that specifies the time
        /// interval (in seconds) that the resource record may be
        /// cached before it should be discarded.  Zero values are
        /// interpreted to mean that the RR can only be used for the
        /// transaction in progress, and should not be cached.
        /// </summary>
        public int Ttl 
        { 
            get { return _ttl; }
            internal set { _ttl = value; }
        }

        /// <summary>
        /// RDATA
        /// </summary>
        public RecordBase Record 
        { 
            get { return _record; }
            internal set { _record = value; }
        }

        #endregion

        internal ResourceRecord()
        {
        }

        internal ResourceRecord(DnsReader br)
        {
            _domain = br.ReadDomain();
            _qtype = (DnsType)br.ReadInt16();
            _qclass = (DnsClass)br.ReadInt16();
            _ttl = br.ReadInt32();

            int recordLength = br.ReadInt16();
            if (recordLength != 0)
            {
                switch (_qtype)
                {
                    case DnsType.A:     _record = new ARecord(br);      break;
                    case DnsType.CNAME: _record = new CNAMERecord(br);  break;
                    case DnsType.MX:    _record = new MXRecord(br);     break;
                    case DnsType.NS:    _record = new NSRecord(br);     break;
                    case DnsType.SOA:   _record = new SOARecord(br);    break;
                    case DnsType.TXT:   _record = new TXTRecord(br);    break;
					case DnsType.PTR:	_record = new PTRERecord(br);	break;

                    // NetBIOS related records
                    case DnsType.NB:    _record = new NBRecord(br);     break;
                    
                    default:
                        br += recordLength;
                        break;
                }
            }
        }

        public override string ToString()
        {
            return _domain + "\r\n" + _record.ToString();
        }

        internal void Write(DnsWriter bw)
        {
            bw.WriteDomain(Domain);
            bw.Write((short)Type);
            bw.Write((short)Class);
            bw.Write(Ttl);

            byte[] record = Record.GetBytes();

            bw.Write((short)record.Length);
            bw.Write(record);
        }
    }
}