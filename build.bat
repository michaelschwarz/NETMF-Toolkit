rmdir Release /S /Q
rmdir Debug /S /Q

mkdir Release
mkdir Debug

xcopy DNS\bin\Release\*.* Release\DNS /E /I
xcopy DNS\bin\Debug\*.* Debug\DNS /E /I

xcopy HTTP\bin\Release\*.* Release\HTTP /E /I
xcopy HTTP\bin\Debug\*.* Debug\HTTP /E /I

xcopy IO\bin\Release\*.* Release\IO /E /I
xcopy IO\bin\Debug\*.* Debug\IO /E /I

xcopy Mail\bin\Release\*.* Release\Mail /E /I
xcopy Mail\bin\Debug\*.* Debug\Mail /E /I

xcopy MicroC6820\bin\Release\*.* Release\MicroC6820 /E /I
xcopy MicroC6820\bin\Debug\*.* Debug\MicroC6820 /E /I

xcopy MicroDNS\bin\Release\*.* Release\MicroDNS /E /I
xcopy MicroDNS\bin\Debug\*.* Debug\MicroDNS /E /I

xcopy MicroGM862\bin\Release\*.* Release\MicroGM862 /E /I
xcopy MicroGM862\bin\Debug\*.* Debug\MicroGM862 /E /I

xcopy MicroHTTP\bin\Release\*.* Release\MicroHTTP /E /I
xcopy MicroHTTP\bin\Debug\*.* Debug\MicroHTTP /E /I

xcopy MicroIO\bin\Release\*.* Release\MicroIO /E /I
xcopy MicroIO\bin\Debug\*.* Debug\MicroIO /E /I

xcopy MicroNTP\bin\Release\*.* Release\MicroNTP /E /I
xcopy MicroNTP\bin\Debug\*.* Debug\MicroNTP /E /I

xcopy MicroUtilities\bin\Release\*.* Release\MicroUtilities /E /I
xcopy MicroUtilities\bin\Debug\*.* Debug\MicroUtilities /E /I

xcopy MicroZigbee\bin\Release\*.* Release\MicroXBee /E /I
xcopy MicroZigbee\bin\Debug\*.* Debug\MicroXBee /E /I

xcopy NetBIOS\bin\Release\*.* Release\NetBIOS /E /I
xcopy NetBIOS\bin\Debug\*.* Debug\NetBIOS /E /I

xcopy NTP\bin\Release\*.* Release\NTP /E /I
xcopy NTP\bin\Debug\*.* Debug\NTP /E /I

xcopy POP3\bin\Release\*.* Release\POP3 /E /I
xcopy POP3\bin\Debug\*.* Debug\POP3 /E /I

xcopy SMTP\bin\Release\*.* Release\SMTP /E /I
xcopy SMTP\bin\Debug\*.* Debug\SMTP /E /I

xcopy SSDP\bin\Release\*.* Release\SSDP /E /I
xcopy SSDP\bin\Debug\*.* Debug\SSDP /E /I

xcopy uALFAT\bin\Release\*.* Release\uALFAT /E /I
xcopy uALFAT\bin\Debug\*.* Debug\uALFAT /E /I

xcopy Zigbee\bin\Release\*.* Release\XBee /E /I
xcopy Zigbee\bin\Debug\*.* Debug\XBee /E /I
