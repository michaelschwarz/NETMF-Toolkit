using System;
using System.Net;

namespace MSchwarz.Net.Web
{
    public struct LogAccess
    {
        /*
         * #Software: Microsoft Internet Information Services 7.0
         * #Version: 1.0
         * #Date: 2008-12-08 09:36:54
         * #Fields: date time s-ip cs-method cs-uri-stem cs-uri-query s-port cs-username c-ip cs(User-Agent) cs(Referer) sc-status sc-substatus sc-win32-status sc-bytes cs-bytes time-taken
         * 2008-12-08 09:36:59 80.190.178.106 GET /livecust.aspx - 80 - 80.190.178.106 Mozilla/4.0+(compatible;+MSIE+7.0;+Windows+NT+6.0;+WOW64;+SLCC1;+.NET+CLR+2.0.50727;+.NET+CLR+3.5.30729;+.NET+CLR+3.0.30618) - 500 0 0 4784 662 63
         */

        /// <summary>
        /// Date and time
        /// </summary>
        public DateTime Date;

        /// <summary>
        /// s-ip
        /// </summary>
        public IPAddress ServerIP;

        /// <summary>
        /// cs-method
        /// </summary>
        public string Method;

        /// <summary>
        /// cs-uri-stem
        /// </summary>
        public string RawUri;

        /// <summary>
        /// cs-uri-query
        /// </summary>
        public string RequestString;

        /// <summary>
        /// s-port
        /// </summary>
        public int Port;

        /// <summary>
        /// cs-username
        /// </summary>
        public string Username;

        /// <summary>
        /// c-ip
        /// </summary>
        public IPAddress ClientIP;

        /// <summary>
        /// cs(User-Agent)
        /// </summary>
        public string UserAgent;

        /// <summary>
        /// cs(Referer)
        /// </summary>
        public string HttpReferer;

        /// <summary>
        /// sc-status
        /// </summary>
        public int Status;

        /// <summary>
        /// sc-substatus
        /// </summary>
        public int SubStatus;

        /// <summary>
        /// sc-win32-status
        /// </summary>
        [Obsolete("Not implemented.", true)]
        public int Win32Status;

        /// <summary>
        /// sc-bytes
        /// </summary>
        public int BytesSent;

        /// <summary>
        /// cs-bytes
        /// </summary>
        public int BytesReceived;

        /// <summary>
        /// time-taken
        /// </summary>
        public int Duration;
    }
}
