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
                http.Start();

                Console.ReadLine();
                Console.WriteLine("Shutting down http server...");
            }

            Console.WriteLine("Done.");
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
#if(DEBUG)
            Console.WriteLine(context.Request.RawUrl);
#endif

            Console.WriteLine(context.Request.Connection);
            context.Response.Connection = context.Request.Connection;

            if (!String.IsNullOrEmpty(_rootFolder))
            {
                string filename = Path.Combine(_rootFolder, context.Request.RawUrl.Replace("/", "\\").Substring(1));
                if (File.Exists(filename))
                {
                    context.Response.Write(File.ReadAllBytes(filename));
                    return;
                }
            }

            switch(context.Request.RawUrl)
            {
                case "/test":
                    context.Response.Redirect("/test.aspx");
                    break;

                case "/test.aspx":
                    context.Response.Write("<html><head><script type=\"text/javascript\" src=\"/scripts/test.js\"></script></head><body><form action=\"/test.aspx\" method=\"post\"><input type=\"text\" name=\"txtbox1\"/><input type=\"submit\" value=\"Post\"/></form></body></html>");
                    break;

                case "/scripts/test.js":
                    context.Response.Write(@"
var c = 0;
function test() {
    var x = new ActiveXObject(""Microsoft.XMLHTTP"");
    x.onreadystatechange = function() {
        if(x.readyState == 4) {
            window.status = x.responseText;
            if(++c < 100) setTimeout(test, 1);
        }
    }
    x.open(""POST"", ""/test.ajax?"" + c, true);
    x.send("""");
}
setTimeout(test, 1000);
");
                    break;

               

                default:
                    context.Response.Write("<html><body>" + DateTime.Now + "<br/><b>" + context.Request.RawUrl + "</b><br/><br/></body></html>");
                    break;
            }
        }

        #endregion
    }

}
