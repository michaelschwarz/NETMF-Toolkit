using System;
using System.Collections.Generic;
using System.Text;
using MSchwarz.Net.Web;
using System.IO;
using MSchwarz.IO;

namespace HttpConsole
{
	class Program
	{
		static void Main(string[] args)
		{
            //Char[] chars;
            //Byte[] bytes = Encoding.UTF8.GetBytes("Hello Worlö");

            //Console.WriteLine(ByteUtil.PrintBytes(bytes));

            //Decoder utf7Decoder = Encoding.UTF8.GetDecoder();

            //int charCount = utf7Decoder.GetCharCount(bytes, 0, bytes.Length);
            //chars = new Char[charCount];
            //int charsDecodedCount = utf7Decoder.GetChars(bytes, 0, bytes.Length, chars, 0);

            //Console.WriteLine(
            //    "{0} characters used to decode bytes.", charsDecodedCount
            //);

            //Console.Write("Decoded chars: ");
            //foreach (Char c in chars)
            //{
            //    Console.Write("{0}", c);
            //}
            //Console.WriteLine();


            //Console.WriteLine(HttpServerUtility.UrlEncode("ö"));
            //Console.WriteLine(HttpServerUtility.UrlDecode("%c3%b6"));

            //return;


            using (HttpServer http = new HttpServer(new MyHttpHandler(Path.Combine(Environment.CurrentDirectory, "..\\..\\root"))))
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
                    if (Path.GetExtension(filename) == ".htm")
                        context.Response.ContentType = "text/html; charset=UTF-8";
                    else if (Path.GetExtension(filename) == ".jpg")
                        context.Response.ContentType = "image/jpeg";

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
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title></title></head><body>" + Encoding.UTF8.GetString(context.Request.Body) + "</body></html>");
                    break;

                case "/cookie":
                    context.Response.ContentType = "text/html; charset=UTF-8";
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
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title></title><script type=\"text/javascript\" src=\"/scripts/test.js\"></script></head><body><form action=\"/test2.aspx\" method=\"post\"><input type=\"text\" id=\"txtbox1\" name=\"txtbox1\"/><input type=\"submit\" value=\"Post\"/></form></body></html>");
                    break;

                case "/scripts/test.js":
                    context.Response.ContentType = "text/javascript";
                    context.Response.Write(@"
var c = 0;
function test() {
    var x = window.ActiveXObject ? new ActiveXObject(""Microsoft.XMLHTTP"") : new XMLHttpRequest();
    x.onreadystatechange = function() {
        if(x.readyState == 4) {
            document.getElementById('txtbox1').value = x.responseText;
            if(++c < 5)
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
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title></title></head><body>" + DateTime.Now + "<br/><b>RawUrl: " + context.Request.RawUrl + "</b><br/>");

                    if (context.Request.Params != null && context.Request.Params.Length > 0)
                    {
                        foreach (HttpParameter p in context.Request.Params)
                            context.Response.Write(p.Name + " = " + p.Value + "<br/>");
                    }

                    context.Response.Write(@"<br/><a href=""index.htm"">Demo (files on SD card)</a><br/>
<a href=""test"">Redirect, AJAX and Form Test</a><br/>
<a href=""cookie"">Cookie Test</a><br/><br/><br/>
<hr size=1/>
<b>Any feedback welcome: <a href=""http://weblogs.asp.net/mschwarz/contact.aspx"">contact</a>
<a href=""http://michael-schwarz.blogspot.com/"">My Blog</a> <a href=""http://weblogs.asp.net/mschwarz/"">My Blog (en)</a>
</body></html>");
                    break;
            }
        }

        #endregion
    }

}
