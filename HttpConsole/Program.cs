using System;
using System.Collections.Generic;
using System.Text;
using MFToolkit.Net.Web;
using System.IO;
using MFToolkit.IO;
using MFToolkit.Net.Dns;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using MFToolkit.Net.XBee;
using MFToolkit.MicroUtilities;

namespace HttpConsole
{
	class Program
	{
        public static double temperature = 17.235;

		static void Main(string[] args)
		{
            //using(XBeeModule m = new XBeeModule("COM5", 9600, ApiType.Enabled))
            //{
            //  m.Open();
            //  m.EnterCommandMode();
            //  m.SetNodeIdentification("COORDINATOR");
            //  m.WriteStateToMemory();
            //  m.ExitCommandMode();
            //}

            Thread thd = new Thread(new ThreadStart(UpdateTemperature));
            thd.IsBackground = true;
            thd.Start();

            using (HttpServer http = new HttpServer(new MyHttpHandler(Path.Combine(Environment.CurrentDirectory, "..\\..\\root"))))
            {
                http.OnLogAccess += new HttpServer.LogAccessHandler(http_OnLogAccess);
                http.Start();

                Console.ReadLine();
                Console.WriteLine("Shutting down http server...");
            }

            Console.WriteLine("Done.");
		}

        static void UpdateTemperature()
        {
            try
            {
                using (XBee xbee = new XBee("COM5", 9600, ApiType.Enabled))
                {
                    xbee.OnPacketReceived += new XBee.PacketReceivedHandler(xbee_OnPacketReceived);
                    xbee.Open();
                    xbee.ExecuteNonQuery(new NodeIdentifier("COORDINATOR"));

                    while (true)
                    {
                        xbee.Execute(new NodeDiscover());

                        Thread.Sleep(60 * 1000);
                    }
                }
            }
            catch (Exception)
            {
                while (true)
                {
                    temperature = new Random((int)DateTime.Now.Ticks).Next(19, 23);

                    Thread.Sleep(60 * 1000);
                }
            }
        }

        static void xbee_OnPacketReceived(XBee sender, XBeeResponse response)
        {
            if (response != null)
                Console.WriteLine(response.ToString() + "\r\n==============================");

            AtCommandResponse res = response as AtCommandResponse;
            if (res != null)
            {
                if (res.ParseValue() is NodeDiscoverData)
                {
                    NodeDiscoverData nd = res.ParseValue() as NodeDiscoverData;

                    if (nd.NodeIdentifier != "MICHAEL")
                    {
                        sender.Execute(new AtRemoteCommand(nd.SerialNumber, nd.ShortAddress, new ForceSample()));
                        //sender.SendCommand(new AtRemoteCommand(nd.SerialNumber, nd.ShortAddress, new XBeeSensorSample()));
                    }
                    else
                    {
                        ZigBeeTransmitRequest x = new ZigBeeTransmitRequest(nd.SerialNumber, nd.ShortAddress, Encoding.ASCII.GetBytes("Hello"));
                        sender.Execute(x);
                    }

                }
                return;

            }

            AtRemoteCommandResponse res2 = response as AtRemoteCommandResponse;
            if (res2 != null)
            {
                if (res2.ParseValue() is ForceSampleData)
                {
                    ForceSampleData d = res2.ParseValue() as ForceSampleData;

                    double mVanalog = (((float)d.AD2) / 1023.0) * 1200.0;
                    double temp_C = (mVanalog - 500.0) / 10.0 - 4.0;
                    double lux = (((float)d.AD1) / 1023.0) * 1200.0;

                    mVanalog = (((float)d.AD3) / 1023.0) * 1200.0;
                    double hum = ((mVanalog * (108.2 / 33.2)) - 0.16) / (5 * 0.0062 * 1000.0);

                    temperature = temp_C;
                }
            }
        }

        static void http_OnLogAccess(LogAccess data)
        {
            Console.WriteLine(data.Date + "\t" + data.ClientIP + "\t" + data.Method + "\t" + data.RawUrl);
            Console.WriteLine(data.UserAgent);
            if(data.HttpReferer != null) Console.WriteLine(data.HttpReferer);
            //Console.WriteLine("------------------------------------------------------------");
        }
	}

    class MyHttpHandler : IHttpHandler
    {
        private string _rootFolder;

        public MyHttpHandler(string rootFolder)
        {

            _rootFolder = Path.GetFullPath(rootFolder);
        }

        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            context.Response.RemoveHeader("Connection");

            if (!String.IsNullOrEmpty(_rootFolder) && context.Request.Path != null)
            {
                string filename = Path.Combine(_rootFolder, context.Request.Path.Replace("/", "\\").Substring(1));
                if (filename.IndexOf("..") < 0 && filename.ToLower().StartsWith(_rootFolder.ToLower()))   // ensure that the files are below _rootFolder
                {
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
            }

            switch(context.Request.Path)
            {
                case "/throwerror":
                    throw new HttpException(MFToolkit.Net.Web.HttpStatusCode.InternalServerError);

                case "/filenotfound":
                    throw new HttpException(MFToolkit.Net.Web.HttpStatusCode.NotFound);
                    
                case "/imbot":
                    context.Response.ContentType = "text/html; charset=UTF-8";

                    switch (context.Request.Form["step"])
                    {
                        case "1":
                            context.Response.Write("Hi, what's your name?");
                            break;

                        case "2":
                            context.Response.Write("Hi " + HttpServerUtility.HtmlEncode(context.Request["value1"]) + ", where do you live?");
                            break;

                        case "3":
                            context.Response.WriteLine("Well, welcome to this hello world bot, " + HttpServerUtility.HtmlEncode(context.Request["value1"]) + " from " + HttpServerUtility.HtmlEncode(context.Request["value2"]) + ".");
                            context.Response.WriteLine("<br>");
                            context.Response.Write("Which temperature do you want to read?<br>A : Kitchen<br>B : Simulated<br>Hit A or B and press enter...");
                            break;

                        case "4":
                            if (context.Request["value3"] == "A")
                                context.Response.WriteLine("In the kitchen it is " + Program.temperature + HttpServerUtility.HtmlEncode(" °C."));
                            else if (context.Request["value3"] == "B")
                                context.Response.WriteLine("In the simulated room it is " + new Random().Next(17, 20) + HttpServerUtility.HtmlEncode(" °C."));
                            else
                                context.Response.WriteLine("I don't know this room.");

                            context.Response.WriteLine("<br>");
                            context.Response.WriteLine("<br>");
                            context.Response.WriteLine("Visit my blog at http://netmicroframework.blogspot.com/");
                            context.Response.Write("<reset>");
                            break;


                        default:
                            context.Response.Write("<goto=1>");
                            break;
                    }

                    break;

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

                    context.Response.AddHeader("Cache-Control", "no-cache");
                    context.Response.AddHeader("Pragma", "no-cache");

                    if(context.Request.Body != null && context.Request.Body.Length > 0)
                        context.Response.Write("ajax = " + Encoding.UTF8.GetString(context.Request.Body));
                    else
                        context.Response.Write("ajax = could not read request");
                        
                    break;

                default:
                    context.Response.ContentType = "text/html; charset=UTF-8";
                    context.Response.Write("<html><head><title>Control My World - How to switch lights on and heating off?</title></head><body>");
                    
                    
                    context.Response.Write("<h1>Welcome to my .NET Micro Framework web server</h1><p>This demo server is running on a Tahoe-II board using XBee modules to communicate with XBee sensors from Digi.</p><p>On my device the current date is " + DateTime.Now + "</b><p><b>RawUrl: " + context.Request.RawUrl + "</b><br/>" + context.Request.Headers["User-Agent"] + "</p>");

                    context.Response.Write("<p>Current temperature: " + Program.temperature + "°C</p>");

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
                                context.Response.Write("<pre>" + Encoding.UTF8.GetString(mime.Content) + "</pre>");
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
