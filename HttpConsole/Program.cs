using System;
using System.Collections.Generic;
using System.Text;
using MSchwarz.Net.Web;
using System.IO;

namespace HttpConsole
{
	class Program
	{
		static void Main(string[] args)
		{
            using (HttpServer http = new HttpServer(82, new MyHttpHandler(Path.Combine(Environment.CurrentDirectory, "..\\..\\root"))))
            {
                http.OnLogAccess += new HttpServer.LogAccessHandler(http_OnLogAccess);
                http.Start();

                Console.ReadLine();
                Console.WriteLine("Shutting down http server...");
            }

            Console.WriteLine("Done.");
		}

        static void http_OnLogAccess(LogAccess data)
        {
            Console.WriteLine(data.ClientIP + "\t" + data.RawUri + "\t" + data.Method + "\t" + data.Duration + " msec\t" + data.BytesReceived + " bytes\t" + data.BytesSent + " bytes");
        }
	}

    class MyHttpHandler : IHttpHandler
    {
        private string _rootFolder;

        public MyHttpHandler(string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            if (!String.IsNullOrEmpty(_rootFolder))
            {
                string filename = Path.Combine(_rootFolder, context.Request.RawUrl.Replace("/", "\\").Substring(1));
                if (File.Exists(filename))
                {
                    context.Response.Write(File.ReadAllBytes(filename));
                    return;
                }
            }

            switch(context.Request.Path)
            {
                case "/test":
                    context.Response.Redirect("/test.aspx");
                    break;

                case "/test2.aspx":
                    context.Response.Write("<html><head><title></title></head><body>" + Encoding.UTF8.GetString(context.Request.Body) + "</body></html>");
                    break;

                case "/cookie":

                    context.Response.Write("<html><head><title></title></head><body>");

                    if (context.Request.Cookies.Length > 0)
                    {
                        foreach (HttpCookie c in context.Request.Cookies)
                            context.Response.WriteLine("Cookie " + c.Name + " = " + c.Value + "<br/>");
                    }

                    HttpCookie cookie = new HttpCookie("test", DateTime.Now.ToString());
                    cookie.Expires = DateTime.Now.AddDays(2);
                    context.Response.SetCookie(cookie);
                    context.Response.WriteLine("</body></html>");

                    break;

                case "/test.aspx":
                    context.Response.Write("<html><head><title></title><script type=\"text/javascript\" src=\"/scripts/test.js\"></script></head><body><form action=\"/test2.aspx\" method=\"post\"><input type=\"text\" id=\"txtbox1\" name=\"txtbox1\"/><input type=\"submit\" value=\"Post\"/></form></body></html>");
                    break;

                case "/scripts/test.js":
                    context.Response.Write(@"
var c = 0;
function test() {
    var x = new ActiveXObject(""Microsoft.XMLHTTP"");
    x.onreadystatechange = function() {
        if(x.readyState == 4) {
            document.getElementById('txtbox1').value = x.responseText;
            if(++c < 100)
                setTimeout(test, 1);
        }
    }
    x.open(""POST"", ""/test.ajax?x="" + c, true);
    x.send("""" + c);
}
setTimeout(test, 1);
");
                    break;

                case "/test.ajax":
                    context.Response.Write("ajax = " + Encoding.UTF8.GetString(context.Request.Body));
                    break;

                default:
                    context.Response.Write("<html><head><title></title></head><body>" + DateTime.Now + "<br/><b>RawUrl: " + context.Request.RawUrl + "</b><br/>");

                    if (context.Request.Params != null && context.Request.Params.Length > 0)
                    {
                        foreach (HttpParameter p in context.Request.Params)
                            context.Response.Write(p.Name + " = " + p.Value + "<br/>");
                    }

                    context.Response.Write("<br/><a href=\"index.htm\">Demo</a> <a href=\"test\">Redirect, AJAX and Form Test</a></body></html>");
                    break;
            }
        }

        #endregion
    }

}
