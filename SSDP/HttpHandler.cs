/* 
 * HttpHandler.cs
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
using System.Collections.Generic;
using System.Text;
using MFToolkit.Net.Web;

namespace MFToolkit.Net.SSDP
{
    internal class HttpHandler : IHttpHandler
    {
        private SsdpServer ssdp;

        internal HttpHandler(SsdpServer server)
        {
            ssdp = server;
        }

        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            Console.WriteLine(context.Request.RawUrl);

            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!");

            if (context.Request.Path.StartsWith("/igddesc.xml/"))
            {
                SsdpDevice device = ssdp.FindDevice(context.Request.Path.Substring(13));

                if (device == null)
                {
                    Console.WriteLine("Could not find UPnP device.");

                    context.Response.RaiseError("Could not find UPnP device.", HttpStatusCode.NotFound);
                }
                else
                {
                    Console.WriteLine("Sending XML...");

                    context.Response.ContentType = "text/xml";
                    context.Response.Write(device.ToSoapMessage());

                    Console.WriteLine(device.ToSoapMessage());
                }
            }
            else
            {
                context.Response.RaiseError(HttpStatusCode.NotFound);
            }
        }

        #endregion
    }
}
