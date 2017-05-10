# Read a MX record for domain ajaxpro.info

Start a new Visual Studio project (i.e. Console Application) and add the reference DNS.dll (if you're using .NET Micro Framework use MicroDNS.dll instead).

Next you have to create a question. In our sample we want to query the MX record of domain ajaxpro.info:

**Question q = new Question("ajaxpro.info", DnsType.MX, DnsClass.IN);**

The DnsResolver needs a DNS server. If you're not using the library within .NET Micro Framework you can use the .LoadNetworkConfiguration method:

**DnsResolver dns = new DnsResolver();**
**dns.LoadNetworkConfiguration();**

Using .NET Micro Framework you should specifiy the DNS server with the constructor of the DnsResolver:

**DnsResolver dns = new DnsResolver("dns.local");**

To get the MX record IP address you could now read the property from the DnsResponse:

**DnsResponse res = dns.Resolve(q);**
**Console.WriteLine((res.Answers[0](0) as MXRecord).ToString());**