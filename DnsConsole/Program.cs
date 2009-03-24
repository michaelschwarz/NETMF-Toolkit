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
