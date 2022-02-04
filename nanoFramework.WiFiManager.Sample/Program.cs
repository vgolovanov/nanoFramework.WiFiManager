using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace nanoFramework.WiFiManager.Sample
{
    public class Program
    {
        private static WiFiManager wiFiManager = new WiFiManager();

        public static void Main()
        {                    
            //wiFiManager.Open(); default AP name and with no Authentication
            //wiFiManager.Open("apname","password"); AP name is "apname" and password "password"
            wiFiManager.Open("ESPNet"); // AP name is "ESPNet" and with no Authentication

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
