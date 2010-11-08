using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MFToolkit.Net.Web;
using System.Threading;

namespace WebDAVConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpServer http = new HttpServer(new MyHttpHandler());
            http.Start();

            while (true)
                Thread.Sleep(1000);
        }
    }

    class MyHttpHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            Console.WriteLine(context.Request.HttpMethod);

            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.AddHeader("Allow", "OPTIONS, GET, HEAD, PROPFIND, PROPPATCH, PUT, DELETE, COPY, MOVE, LOCK, UNLOCK");
                context.Response.AddHeader("Public", "OPTIONS, GET, HEAD, PROPFIND, PROPPATCH, PUT, DELETE, COPY, MOVE, LOCK, UNLOCK");
                context.Response.AddHeader("DAV", "1,2,3");
            }
            else if (context.Request.HttpMethod == "PROPFIND")
            {
                Console.WriteLine("content: " + Encoding.UTF8.GetString(context.Request.Body));

                context.Response.RaiseError(HttpStatusCode.OK);
            }
            else
                context.Response.RaiseError(HttpStatusCode.NotFound);
        }
    }
}
