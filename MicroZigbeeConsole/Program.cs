using System;
using Microsoft.SPOT;

namespace MicroZigbeeConsole
{
	public class Program
	{
		public static void Main()
		{
			Debug.Print(
				Resources.GetString(Resources.StringResources.String1));

			using (MSchwarz.Net.Zigbee.XBee xbee = new MSchwarz.Net.Zigbee.XBee("COM1", 9600))
			{

			}
		}

	}
}
