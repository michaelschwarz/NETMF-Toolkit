/* 
 * SsdpDevice.cs
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
 * 
 * 
 */
using System;
using System.Xml;

namespace MFToolkit.Net.SSDP
{
    public class SsdpDevice
    {
        public SsdpDevice()
        {
        }

        #region Public Properties

        public DeviceType Type { get; set; }
        public string FriendlyName { get; set; }
        public string Manufacturer { get; set; }
        public Uri ManufacturerURL { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ModelNumber { get; set; }
        public Uri ModelURL { get; set; }
        public string UDN { get; set; }
        public Uri PresentationURL { get; set; }

        #endregion

        internal string ToSoapMessage()
        {
            string xml = string.Format(@"<?xml version=""1.0""?>
<root xmlns=""urn:schemas-upnp-org:device-1-0"">
    <specVersion>
		<major>1</major>
		<minor>0</minor>
	</specVersion>
    <device>
        <deviceType>{0}</deviceType>
        <friendlyName>{1}</friendlyName>
        <manufacturer/>
        <manufacturerURL/>
        <modelDescription/>
        <modelName/>
        <modelNumber/>
        <modelURL/>
        <UDN>{2}</UDN>
        <iconList/>
        <serviceList>
            <service>
				<serviceType>urn:schemas-any-com:service:Any:1</serviceType>
				<serviceId>urn:any-com:serviceId:any1</serviceId>
				<controlURL>/upnp/control/any</controlURL>
				<eventSubURL>/upnp/control/any</eventSubURL>
				<SCPDURL>/any.xml</SCPDURL>
			</service>
        </serviceList>
        <deviceList/>
        <presentationURL>{3}</presentationURL>
    </device>
</root>", DeviceTypeHelper.GetDeviceType(Type), Name, UDN, PresentationURL.ToString());

            return xml;
        }
    }
}
