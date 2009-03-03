using System;
using Microsoft.SPOT;
using MSchwarz.Net.Web;
using System.Threading;
using System.Text;
using System.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using MSchwarz.Net.Dns;

namespace MicroHTTPConsole
{
    public class Program
    {
        internal static bool stopThread = false;

        public static void Main()
        {
            foreach (NetworkInterface net in NetworkInterface.GetAllNetworkInterfaces())
            {
                Debug.Print(net.IPAddress.ToString());
            }

            using (HttpServer http = new HttpServer((int)81, IPAddress.Any, new MyHttpHandler()))
            {
                http.Start();

                while (!stopThread)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }

    public class MyHttpHandler : IHttpHandler
    {
        const string htmlhead = "<html><head><title>Demo</title></head><body>";
        const string htmlfoot = "</body></html>";

        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            context.Response.RemoveHeader("Connection");
            context.Response.ContentType = "text/html; charset=UTF-8";

            if (context.Request.RawUrl == "/stop")
            {
                context.Response.WriteLine(htmlhead);
                context.Response.WriteLine("Ok, stopped http server.");
                context.Response.WriteLine(htmlfoot);

                Program.stopThread = true;
            }
            else
            {
                switch (context.Request.Path)
                {
                    default:
                    case "/":
                        context.Response.ContentType = "text/html; charset=UTF-8";


                        context.Response.WriteLine(htmlhead);


                        context.Response.Write("<h1>Welcome to my .NET Micro Framework web server</h1><p>This demo server is running on a Tahoe-II board using XBee modules to communicate with XBee sensors from Digi.</p><p>On my device the current date is " + DateTime.Now + "</b><p><b>RawUrl: " + context.Request.RawUrl + "</b><br/>" + context.Request.GetHeaderValue("User-Agent") + "</p>");

                        context.Response.WriteLine("<img src=\"ms.jpg\"/>");

                        if (context.Request.Params != null && context.Request.Params.Length > 0)
                        {
                            context.Response.Write("<p style=\"color:blue\">");

                            foreach (HttpParameter p in context.Request.Params)
                                context.Response.Write(p.Name + " = " + p.Value + "<br/>");

                            context.Response.Write("</p>");
                        }

                        if (context.Request.Body != null)
                        {
                            context.Response.Write("<h3>Received Bytes:</h3>");
                            context.Response.Write("<p>" + context.Request.Body.Length + " bytes</p>");
                            context.Response.Write("<hr size=1/>");
                        }

                        context.Response.WriteLine("<form action=\"/testget\" method=\"GET\"><input type=\"text\" name=\"txtbox1\"/><input type=\"submit\" value=\"post\"/></form>");
                        context.Response.WriteLine(htmlfoot);
                        break;

                    case "/ms.jpg":
                        context.Response.ContentType = "image/jpeg";
                        context.Response.Write(Resources.GetBytes(Resources.BinaryResources.ms_jpg));
                        break;

                    case "/testget":
                        context.Response.ContentType = "text/html; charset=UTF-8";
                        context.Response.WriteLine(htmlhead);
                        context.Response.WriteLine("<p>Click <a href=\"/\">here</a> to go back to main page.</p>");
                        context.Response.Write(DateTime.Now + "<br/><b>RawUrl: " + context.Request.RawUrl + "</b><br/>");

                        if (context.Request.Params != null && context.Request.Params.Length > 0)
                        {
                            foreach (HttpParameter p in context.Request.Params)
                                context.Response.Write(p.Name + " = " + p.Value + "<br/>");
                        }

                        if (context.Request.Body != null)
                            context.Response.WriteLine(new String(Encoding.UTF8.GetChars(context.Request.Body)));

                        context.Response.WriteLine(htmlfoot);
                        break;

                    case "/network":
                        context.Response.ContentType = "text/html; charset=UTF-8";
                        context.Response.WriteLine(htmlhead);
                        foreach (NetworkInterface net in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            context.Response.WriteLine(net.IPAddress.ToString() + "<br/>");

                            if (net.DnsAddresses.Length > 0)
                            {
                                string dns = net.DnsAddresses[0];

                                DnsResolver resolver = new DnsResolver(IPAddress.Parse(dns));
                                DnsRequest dnsreq = new DnsRequest();
                                dnsreq.Questions = new Question[] {
                                    new Question("microsoft.com", DnsType.A, DnsClass.IN)
                                };

                                DnsResponse dnsres = resolver.Resolve(dnsreq);

                                foreach (Answer a in dnsres.Answers)
                                    context.Response.WriteLine("microsoft.com A record: " + (a.Record as ARecord).IPAddress.ToString() + "<br/>");
                            }


                        }
                        context.Response.WriteLine(htmlfoot);
                        break;
                }
            }
        }

        #endregion
    }
}
