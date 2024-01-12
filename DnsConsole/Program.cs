/* 
 * Program.cs		(Demo Application)
 * 
 * Copyright (c) 2009-2024, Michael Schwarz (http://www.schwarz-interactive.de)
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
 */
using System;
using System.Collections.Generic;
using System.Text;
using MFToolkit.Net.Dns;
using System.Net;

namespace DnsConsole
{
	class Program
	{
		static void Main(string[] args)
		{
            //NetBIOS nbt = new NetBIOS();
            //nbt.Register("HANS", IPAddress.Parse("192.168.177.1"));

            //try
            //{
            //    IPAddress ip = nbt.QueryName("HANS");
            //    Console.WriteLine("NetBIOS IP address for HANS: " + ip);
            //}
            //catch(Exception)
            //{
            //    Console.WriteLine("Could not get IP address for HANS.");
            //}



			// Retreiving the mail record (MX) of a domain

			Console.WriteLine("Retreiving MX record for domain google.com...");

			DnsRequest r = new DnsRequest();
			Question q = new Question("google.com", DnsType.MX, DnsClass.IN);
			r.Questions.Add(q);

			DnsResolver dns = new DnsResolver();
			dns.LoadNetworkConfiguration();			// using local IP configuration
			
			DnsResponse res = dns.Resolve(r);

			foreach (Answer a in res.Answers)
				Console.WriteLine(a);

			Console.WriteLine();
			Console.WriteLine();

			// Retreiving the IP address of an host

			Console.WriteLine("Retreiving A record for domain www.microsoft.com...");

			r = new DnsRequest();
			r.Questions.Add(new Question("www.microsoft.com", DnsType.A, DnsClass.IN));

			res = dns.Resolve(r);

			foreach (Answer a in res.Answers)
				Console.WriteLine(a);

			Console.WriteLine();
			Console.WriteLine();

			// Retreiving the IP address of an host

			Console.WriteLine("Retreiving TXT record (i.e. SPF) for domain microsoft.com...");

			r = new DnsRequest();
			r.Questions.Add(new Question("microsoft.com", DnsType.TXT, DnsClass.IN));

			res = dns.Resolve(r);

			foreach (Answer a in res.Answers)
				Console.WriteLine(a);

		}
	}
}
