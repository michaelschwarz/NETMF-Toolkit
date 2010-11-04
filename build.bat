mkdir Release
mkdir Debug

del Release\*.*
del Debug\*.*

copy DNS\bin\Release\DNS.dll Release\
copy DNS\bin\Debug\DNS.* Debug\

copy HTTP\bin\Release\HTTP.dll Release\
copy HTTP\bin\Debug\HTTP.* Debug\

copy IO\bin\Release\IO.dll Release\
copy IO\bin\Debug\IO.* Debug\

copy Mail\bin\Release\Mail.dll Release\
copy Mail\bin\Debug\Mail.* Debug\

copy MicroC6820\bin\Release\MicroC6820.dll Release\
copy MicroC6820\bin\Debug\MicroC6820.* Debug\

copy MicroDNS\bin\Release\MicroDNS.dll Release\
copy MicroDNS\bin\Debug\MicroDNS.* Debug\

copy MicroGM862\bin\Release\MFToolkit.MicroGM862.dll Release\
copy MicroGM862\bin\Debug\MFToolkit.MicroGM862.* Debug\

copy MicroHTTP\bin\Release\MicroHTTP.dll Release\
copy MicroHTTP\bin\Debug\MicroHTTP.* Debug\

copy MicroIO\bin\Release\MicroIO.dll Release\
copy MicroIO\bin\Debug\MicroIO.* Debug\

copy MicroNTP\bin\Release\MicroNTP.dll Release\
copy MicroNTP\bin\Debug\MicroNTP.* Debug\

copy MicroUtilities\bin\Release\MFToolkit.MicroUtilities.dll Release\
copy MicroUtilities\bin\Debug\MFToolkit.MicroUtilities.* Debug\

copy MicroZigbee\bin\Release\MicroXBee.dll Release\
copy MicroZigbee\bin\Debug\MicroXBee.* Debug\

copy NetBIOS\bin\Release\NetBIOS.dll Release\
copy NetBIOS\bin\Debug\NetBIOS.* Debug\

copy NTP\bin\Release\NTP.dll Release\
copy NTP\bin\Debug\NTP.* Debug\

copy POP3\bin\Release\POP3.dll Release\
copy POP3\bin\Debug\POP3.* Debug\

copy SMTP\bin\Release\SMTP.dll Release\
copy SMTP\bin\Debug\SMTP.* Debug\

copy SSDP\bin\Release\SSDP.dll Release\
copy SSDP\bin\Debug\SSDP.* Debug\

copy uALFAT\bin\Release\uALFAT.dll Release\
copy uALFAT\bin\Debug\uALFAT.* Debug\

copy Zigbee\bin\Release\XBee.dll Release\
copy Zigbee\bin\Debug\XBee.* Debug\
