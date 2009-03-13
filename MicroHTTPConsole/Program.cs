using System;
using Microsoft.SPOT;
using MSchwarz.Net.Web;
using System.Threading;
using System.Text;
using System.Net;
using Microsoft.SPOT.Net.NetworkInformation;
using MSchwarz.Net.Ntp;
using System.IO;

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

            Microsoft.SPOT.ExtendedTimeZone.SetTimeZone(TimeZoneId.Berlin);
            Microsoft.SPOT.Hardware.Utility.SetLocalTime(NtpClient.GetNetworkTime());

            using (HttpServer http = new HttpServer(new MyHttpHandler()))
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

            //if (!String.IsNullOrEmpty(_rootFolder) && context.Request.Path != null)
            //{
            //    string filename = Path.Combine(_rootFolder, context.Request.Path.Replace("/", "\\").Substring(1));
            //    if (filename.IndexOf("..") < 0 && filename.ToLower().StartsWith(_rootFolder.ToLower()))   // ensure that the files are below _rootFolder
            //    {
            //        if (File.Exists(filename))
            //        {
            //            if (Path.GetExtension(filename) == ".htm")
            //                context.Response.ContentType = "text/html; charset=UTF-8";
            //            else if (Path.GetExtension(filename) == ".jpg")
            //                context.Response.ContentType = "image/jpeg";

            //            context.Response.Write(File.ReadAllBytes(filename));
            //            return;
            //        }
            //    }
            //}

            switch (context.Request.Path)
            {
                case "/imbot":
                    context.Response.ContentType = "text/html; charset=UTF-8";

                    switch (context.Request.Form["step"])
                    {
                        case "2":
                            context.Response.WriteLine("Hi " + context.Request["value1"] + ", where do you live?");
                            break;

                        case "3":
                            context.Response.WriteLine("Well, welcome to this hello world bot, " + context.Request["value1"] + " from " + context.Request["value2"] + ".");
                            context.Response.WriteLine("<br/>");
                            context.Response.WriteLine("Visit my blog at http://netmicroframework.blogspot.com/");
                            context.Response.WriteLine("<reset>");
                            break;

                        case "1":
                            context.Response.WriteLine("Hi, what's your name?");
                            break;

                        default:
                            context.Response.WriteLine("<goto=1>");
                            break;
                    }

                    break;

                case "/test":
                    context.Response.Redirect("/test.aspx");
                    break;

                case "/test2.aspx":
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title></title></head><body>" + new string(Encoding.UTF8.GetChars(context.Request.Body)) + "</body></html>");
                    break;

                case "/cookie":
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title></title></head><body>");

                    if (context.Request.Cookies.Count > 0)
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
var d = new Date();
function test() {
    var x = window.ActiveXObject ? new ActiveXObject(""Microsoft.XMLHTTP"") : new XMLHttpRequest();
    x.onreadystatechange = function() {
        if(x.readyState == 4) {
            document.getElementById('txtbox1').value = x.responseText;
            if(++c <= 5)
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

                    //if (context.Request.Connection.ToLower() == "keep-alive")
                    //{
                    //    context.Response.AddHeader("Connection", "Keep-Alive");
                    //    context.Response.AddHeader("Keep-Alive", "timeout=15, max=100");
                    //}

                    if (context.Request.Body != null && context.Request.Body.Length > 0)
                        context.Response.Write("ajax = " + new string(Encoding.UTF8.GetChars(context.Request.Body)));
                    else
                        context.Response.Write("ajax = could not read request");

                    break;

                default:
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title>Control My World - How to switch lights on and heating off?</title></head><body>");


                    context.Response.Write("<h1>Welcome to my .NET Micro Framework web server</h1><p>This demo server is running on a Tahoe-II board using XBee modules to communicate with XBee sensors from Digi.</p><p>On my device the current date is " + DateTime.Now + "</b><p><b>RawUrl: " + context.Request.RawUrl + "</b><br/>" + context.Request.Headers["User-Agent"] + "</p>");

                    if (context.Request.Params != null && context.Request.Params.Count > 0)
                    {
                        context.Response.Write("<h3>Params</h3>");
                        context.Response.Write("<p style=\"color:blue\">");

                        foreach (string key in context.Request.Params.AllKeys)
                            context.Response.Write(key + " = " + context.Request.Params[key] + "<br/>");

                        context.Response.Write("</p>");
                    }

                    if (context.Request.Form != null && context.Request.Form.Count > 0)
                    {
                        context.Response.Write("<h3>Form</h3>");
                        context.Response.Write("<p style=\"color:brown\">");

                        foreach (string key in context.Request.Form.AllKeys)
                            context.Response.Write(key + " = " + context.Request.Form[key] + "<br/>");

                        context.Response.Write("</p>");
                    }

                    if (context.Request.MimeContent != null)
                    {
                        context.Response.Write("<h3>MIME Content</h3>");

                        foreach (string key in context.Request.MimeContent.AllKeys)
                        {
                            MimeContent mime = context.Request.MimeContent[key];

                            context.Response.Write("<p style=\"color:blue\">");
                            context.Response.Write(key + " =&gt; " + (mime.Content != null ? mime.Content.Length.ToString() : "0") + " bytes<br/>");

                            foreach (string mkey in context.Request.MimeContent[key].Headers.Keys)
                                context.Response.Write("<i>" + mkey + " : " + context.Request.MimeContent[key].Headers[mkey] + "</i><br/>");

                            context.Response.Write("</p>");

                            if (mime.Headers["Content-Type"] == "text/plain" && mime.Content != null && mime.Content.Length > 0)
                                context.Response.Write("<pre>" + new string(Encoding.UTF8.GetChars(mime.Content)) + "</pre>");
                        }
                    }

                    if (context.Request.Headers != null && context.Request.Headers.Count > 0)
                    {
                        context.Response.Write("<h3>HTTP Header</h3>");
                        context.Response.Write("<p style=\"color:green\">");

                        foreach (string key in context.Request.Headers.AllKeys)
                            context.Response.Write(key + " = " + context.Request.Headers[key] + "<br/>");

                        context.Response.Write("</p>");
                    }

                    if (context.Request.Body != null)
                    {
                        context.Response.Write("<h3>Received Bytes:</h3>");
                        context.Response.Write("<p>" + context.Request.Body.Length + " bytes</p>");
                        context.Response.Write("<hr size=1/>");
                    }

                    context.Response.Write(@"<p><a href=""index.htm"">Demo HTML and JPEG (files on SD card)</a><br/>
<a href=""test.txt"">Demo Plain Text (file on SD card)</a><br/>
<a href=""test"">Redirect Test</a> calls /test and gets redirected to /test.aspx<br/>
<a href=""test.aspx"">AJAX Test</a> requests 5 times a value from webserver<br/>
<a href=""cookie"">Cookie Test</a> sets and displays a cookie<br/></p>
<hr size=1/>
<p>Any feedback welcome: <a href=""http://weblogs.asp.net/mschwarz/contact.aspx"">contact</a>
<a href=""http://michael-schwarz.blogspot.com/"">My Blog</a> <a href=""http://weblogs.asp.net/mschwarz/"">My Blog (en)</a><br/>
<a href=""http://www.control-my-world.com/"">Control My World</a></p>
</body></html>");
                    break;
            } 
        }

        #endregion
    }
}
