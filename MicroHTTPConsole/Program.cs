using System;
using Microsoft.SPOT;
using MSchwarz.Net.Web;
using System.Threading;
using System.Text;
using System.Net;
using Microsoft.SPOT.Net.NetworkInformation;

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
            if (context.Request.RawUrl == "/stop")
            {
                context.Response.WriteLine(htmlhead);
                context.Response.WriteLine("Ok, stopped http server.");
                context.Response.WriteLine(htmlfoot);

                Program.stopThread = true;
            }
            else
            {
                switch (context.Request.RawUrl)
                {
                    case "/":
                        context.Response.WriteLine(htmlhead);
                        context.Response.WriteLine("<form action=\"/testpost\" method=\"POST\"><input type=\"text\" name=\"txtbox1\"/><input type=\"submit\" value=\"post\"/></form>");
                        context.Response.WriteLine(htmlfoot);
                        break;

                    case "/testpost":
                        context.Response.WriteLine(htmlhead);
                        context.Response.WriteLine(new String(Encoding.UTF8.GetChars(context.Request.Content)));
                        context.Response.WriteLine(htmlfoot);
                        break;

                    case "/network":
                        context.Response.WriteLine(htmlhead);
                        foreach (NetworkInterface net in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            context.Response.WriteLine(net.IPAddress.ToString() + "<br/>");
                        }
                        context.Response.WriteLine(htmlfoot);
                        break;
                }
            }
        }

        #endregion
    }

}
