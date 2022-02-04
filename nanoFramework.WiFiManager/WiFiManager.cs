using nanoFramework.Runtime.Native;
using System;
using System.Collections;
using System.Device.WiFi;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;



namespace nanoFramework.WiFiManager
{
    public class WiFiManager
    {
        private NetworkInterface networkInterface;
        private ManualResetEvent netScanDone = new ManualResetEvent(false);
        private ArrayList availableNetworks;
        private WebServer webServer;

        private const string HTTP_HEAD = "<!DOCTYPE html><html lang=\"en\"><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1, user-scalable=no\"/><title>{v}</title>";
       // private const string HTTP_STYLE = "<style>.c{text-align: center;} div,input{padding:5px;font-size:1em;} input{width:95%;} body{text-align: center;font-family:verdana;} button{border:0;border-radius:0.3rem;background-color:#1fa3ec;color:#fff;line-height:2.4rem;font-size:1.2rem;width:100%;} .q{float: right;width: 64px;text-align: right;} .l{background: url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAALVBMVEX///8EBwfBwsLw8PAzNjaCg4NTVVUjJiZDRUUUFxdiZGSho6OSk5Pg4eFydHTCjaf3AAAAZElEQVQ4je2NSw7AIAhEBamKn97/uMXEGBvozkWb9C2Zx4xzWykBhFAeYp9gkLyZE0zIMno9n4g19hmdY39scwqVkOXaxph0ZCXQcqxSpgQpONa59wkRDOL93eAXvimwlbPbwwVAegLS1HGfZAAAAABJRU5ErkJggg==\") no-repeat left center;background-size: 1em;}</style>";
        private const string HTTP_STYLE = @"<style>
.c,body{text-align:center;font-family:verdana}div,input{padding:5px;font-size:1em;margin:5px 0;box-sizing:border-box;}
input,button,.msg{border-radius:.3rem;width: 100%},input[type=radio]{width: auto}
button,input[type='button'],input[type='submit']{cursor:pointer;border:0;background-color:#1fa3ec;color:#fff;line-height:2.4rem;font-size:1.2rem;width:100%}
input[type='file']{border:1px solid #1fa3ec}
.wrap {text-align:left;display:inline-block;min-width:260px;max-width:500px}
// links
a{color:#000;font-weight:700;text-decoration:none}a:hover{color:#1fa3ec;text-decoration:underline}
// quality icons
.q{height:16px;margin:0;padding:0 5px;text-align:right;min-width:38px;float:right}.q.q-0:after{background-position-x:0}.q.q-1:after{background-position-x:-16px}.q.q-2:after{background-position-x:-32px}.q.q-3:after{background-position-x:-48px}.q.q-4:after{background-position-x:-64px}.q.l:before{background-position-x:-80px;padding-right:5px}.ql .q{float:left}.q:after,.q:before{content:'';width:16px;height:16px;display:inline-block;background-repeat:no-repeat;background-position: 16px 0;
background-image:url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAGAAAAAQCAMAAADeZIrLAAAAJFBMVEX///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADHJj5lAAAAC3RSTlMAIjN3iJmqu8zd7vF8pzcAAABsSURBVHja7Y1BCsAwCASNSVo3/v+/BUEiXnIoXkoX5jAQMxTHzK9cVSnvDxwD8bFx8PhZ9q8FmghXBhqA1faxk92PsxvRc2CCCFdhQCbRkLoAQ3q/wWUBqG35ZxtVzW4Ed6LngPyBU2CobdIDQ5oPWI5nCUwAAAAASUVORK5CYII=');}
// icons @2x media query (32px rescaled)
@media (-webkit-min-device-pixel-ratio: 2),(min-resolution: 192dpi){.q:before,.q:after {
background-image:url('data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAALwAAAAgCAMAAACfM+KhAAAALVBMVEX///8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAADAOrOgAAAADnRSTlMAESIzRGZ3iJmqu8zd7gKjCLQAAACmSURBVHgB7dDBCoMwEEXRmKlVY3L//3NLhyzqIqSUggy8uxnhCR5Mo8xLt+14aZ7wwgsvvPA/ofv9+44334UXXngvb6XsFhO/VoC2RsSv9J7x8BnYLW+AjT56ud/uePMdb7IP8Bsc/e7h8Cfk912ghsNXWPpDC4hvN+D1560A1QPORyh84VKLjjdvfPFm++i9EWq0348XXnjhhT+4dIbCW+WjZim9AKk4UZMnnCEuAAAAAElFTkSuQmCC');
background-size: 95px 16px;}}
// msg callouts
.msg{padding:20px;margin:20px 0;border:1px solid #eee;border-left-width:5px;border-left-color:#777}.msg h4{margin-top:0;margin-bottom:5px}.msg.P{border-left-color:#1fa3ec}.msg.P h4{color:#1fa3ec}.msg.D{border-left-color:#dc3630}.msg.D h4{color:#dc3630}.msg.S{border-left-color: #5cb85c}.msg.S h4{color: #5cb85c}
// lists
dt{font-weight:bold}dd{margin:0;padding:0 0 0.5em 0;min-height:12px}
td{vertical-align: top;}
.h{display:none}
button.D{background-color:#dc3630}
// invert
body.invert,body.invert a,body.invert h1 {background-color:#060606;color:#fff;}
body.invert .msg{color:#fff;background-color:#282828;border-top:1px solid #555;border-right:1px solid #555;border-bottom:1px solid #555;}
body.invert .q[role=img]{-webkit-filter:invert(1);filter:invert(1);}
input:disabled {opacity: 0.5;}
</style>";


        private const string HTTP_SCRIPT = "<script>function c(l){document.getElementById('s').value=l.innerText||l.textContent;document.getElementById('p').focus();}</script>";
        private const string HTTP_HEAD_END = "</head><body><div style='text-align:left;display:inline-block;min-width:260px;'>";
        private const string HTTP_PORTAL_OPTIONS = "<form action=\"/wifi\" method=\"get\"><button>Configure WiFi</button></form><br/><form action=\"/0wifi\" method=\"get\"><button>Configure WiFi (No Scan)</button></form><br/>";
        private const string HTTP_END = "</div></body></html>";
        private const string HTTP_SCAN_LINK = "<br/><div class=\"c\"><a href=\"/wifi\">Scan</a></div>";
        private const string HTTP_FORM_START = "<form method='post' action='wifisave'><input id='s' name='s' length=32 placeholder='SSID'><br/><input id='p' name='p' length=64 type='password' placeholder='password'><br/>";
        private const string HTTP_FORM_PARAM = "<br/><input id='{i}' name='{n}' length={l} placeholder='{p}' value='{v}' {c}>";
        private const string HTTP_FORM_END = "<br/><button type='submit'>save</button></form>";
        private const string HTTP_ITEM = "<div><a href='#p' onclick='c(this)'>{v}</a>&nbsp;<span class='q {i}'>{r}%</span></div>";
        private const string HTTP_SAVED = "<div>Credentials Saved<br />Trying to connect the device to a network.<br />If it fails, reconnect to the AP again.</div>";

        //private const string HTTP_HEAD = "<!DOCTYPE html><html lang=\"en\"><head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1, user-scalable=no\"/><title>{v}</title>";
        //private const string HTTP_STYLE = "<style>.c{text-align: center;} div,input{padding:5px;font-size:1em;} input{width:95%;} body{text-align: center;font-family:verdana;} button{border:0;border-radius:0.3rem;background-color:#1fa3ec;color:#fff;line-height:2.4rem;font-size:1.2rem;width:100%;} .q{float: right;width: 64px;text-align: right;} .l{background: url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAALVBMVEX///8EBwfBwsLw8PAzNjaCg4NTVVUjJiZDRUUUFxdiZGSho6OSk5Pg4eFydHTCjaf3AAAAZElEQVQ4je2NSw7AIAhEBamKn97/uMXEGBvozkWb9C2Zx4xzWykBhFAeYp9gkLyZE0zIMno9n4g19hmdY39scwqVkOXaxph0ZCXQcqxSpgQpONa59wkRDOL93eAXvimwlbPbwwVAegLS1HGfZAAAAABJRU5ErkJggg==\") no-repeat left center;background-size: 1em;}</style>";
        //private const string HTTP_SCRIPT = "<script>function c(l){document.getElementById('s').value=l.innerText||l.textContent;document.getElementById('p').focus();}</script>";
        //private const string HTTP_HEAD_END = "</head><body><div style='text-align:left;display:inline-block;min-width:260px;'>";
        //private const string HTTP_PORTAL_OPTIONS = "<form action=\"/wifi\" method=\"get\"><button>Configure WiFi</button></form><br/><form action=\"/0wifi\" method=\"get\"><button>Configure WiFi (No Scan)</button></form><br/>";
        //private const string HTTP_END = "</div></body></html>";
        //private const string HTTP_SCAN_LINK = "<br/><div class=\"c\"><a href=\"/wifi\">Scan</a></div>";
        //private const string HTTP_FORM_START = "<form method='post' action='wifisave'><input id='s' name='s' length=32 placeholder='SSID'><br/><input id='p' name='p' length=64 type='password' placeholder='password'><br/>";
        //private const string HTTP_FORM_PARAM = "<br/><input id='{i}' name='{n}' length={l} placeholder='{p}' value='{v}' {c}>";
        //private const string HTTP_FORM_END = "<br/><button type='submit'>save</button></form>";
        //private const string HTTP_ITEM = "<div><a href='#p' onclick='c(this)'>{v}</a>&nbsp;<span class='q {i}'>{r}%</span></div>";
        //private const string HTTP_SAVED = "<div>Credentials Saved<br />Trying to connect the device to a network.<br />If it fails, reconnect to the AP again.</div>";

        private string customHeadElement = "";
        private string bodyClass = "";
        private int minimumSignalQuality = -1;
        private string apName;
        private string apPassword;        
        private ArrayList _params;

        public WiFiManager()
        {                      
            _params = new ArrayList();
        }


        /// <summary>
        /// 
        /// </summary>
        public void Open()
        {
            Open(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Open(string ApName)
        {
            Open(ApName, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Open(string ApName, string ApPassword)
        {
            apName = ApName;
            apPassword = ApPassword;
            availableNetworks = new ArrayList();

            if (!WirelessAP.IsEnabled())
            {
                Wireless80211.Disable();
                if (WirelessAP.Setup(apName, apPassword) == false)
                {
                    //Better to not reboot 
                    Power.RebootDevice();
                }
            }

            webServer = new WebServer(80, HttpProtocol.Http);
            webServer.CommandReceived += ServerCommandReceived;
            webServer.Start();
        }

        public void AddCustomHeadElement(string CustomHeadElement)
        {
            customHeadElement = CustomHeadElement;
        }

        public void AddBodyClasst(string CustomBodyClass)
        {
            bodyClass = CustomBodyClass;
        }

        private void ScanNetworks()
        {
            // 1. Set device to the STA mode without reboot
            // 2. ScanAsync
            // 3. Create arraylist with info of wifi networks.
            // 4. Set device to the SoftAP mode without reboot

            // At that moment is not possible to change modes without restarting the device. For now, better to comment out.

            ////If STA is not enabled, to be able to scan networks we need to set STA mode and restart the device.
            //if (!Wireless80211.IsEnabled())
            //{
            //    //Without SSID and password is not possible to switch the device to the STA mode
            //    Wireless80211.Configure("fakessid", "fakepassword");
            //    Debug.WriteLine("AP mode. Set fake ssid and password to switch into STA mode and sends device to the reboot.");

            //}

            //WiFiAdapter wifiAdapter = WiFiAdapter.FindAllAdapters()[0];
            //wifiAdapter.AvailableNetworksChanged += Wifi_AvailableNetworksChanged;
            //wifiAdapter.ScanAsync();
            //netScanDone.WaitOne(10000, true);

        }

        /// <summary>
        /// Event handler for when WiFi scan completes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Wifi_AvailableNetworksChanged(WiFiAdapter sender, object e)
        {
            Debug.WriteLine("Wifi_AvailableNetworksChanged - get report");

            // Get Report of all scanned WiFi networks
            WiFiNetworkReport report = sender.NetworkReport;
            availableNetworks.Clear();

            // Enumerate though networks looking for our network
            foreach (WiFiAvailableNetwork net in report.AvailableNetworks)
            {
                // Show all networks found
                Debug.WriteLine($"Net SSID :{net.Ssid},  BSSID : {net.Bsid},  rssi : {net.NetworkRssiInDecibelMilliwatts.ToString()},  signal : {net.SignalBars.ToString()}");

                WiFiNetwork wifiNetwork = new WiFiNetwork();
                wifiNetwork.Ssid = net.Ssid;
                wifiNetwork.Bsid = net.Bsid;
                wifiNetwork.Rssi = net.NetworkRssiInDecibelMilliwatts;
                wifiNetwork.SignalBars = net.SignalBars;

                availableNetworks.Add(wifiNetwork);
                netScanDone.Set();
            }
        }

        private void ServerCommandReceived(object source, WebServerEventArgs e)
        {          
            var request = e.Context.Request;
          
            string url = request.RawUrl.ToLower().Split('?')[0];

            if (url == "/")
            {
                webServer.OutPutStream(e.Context.Response, handleRoot());
                //webServer.OutputStream(e.Context.Response, handleRoot());
                //webServer.ou
            }
            else if (url=="/wifi")
            {                
                webServer.OutPutStream(e.Context.Response, handleWifi());
            }
            else if (url == "/0wifi")
            {
                webServer.OutPutStream(e.Context.Response, handleWifi(false));
            }
            else if (url == "/wifisave")
            {
                Hashtable formParams = new Hashtable();

                if (request.InputStream.Length > 0)
                {
                    byte[] buffer = new byte[request.InputStream.Length];
                    request.InputStream.Read(buffer, 0, (int)request.InputStream.Length);
                    string rawParams = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);                    
                    string[] parPairs = rawParams.Split('&');
                    foreach (string pair in parPairs)
                    {
                        string[] nameValue = pair.Split('=');
                        formParams.Add(nameValue[0], nameValue[1]);
                    }                 
                }

                 webServer.OutPutStream(e.Context.Response, handleWifiSave(formParams));
            }           
            else
            {
                webServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
            }
        }

        private string handleRoot()
        {
            //if (captivePortal())
            //{ // If caprive portal redirect instead of displaying the page.
            //    return;
            //}
            string page = HTTP_HEAD;
            page = page.Replace("{v}", "Options");
            page += HTTP_SCRIPT;
            page += HTTP_STYLE;
            page += customHeadElement;
            page += HTTP_HEAD_END;
            page += "<h1>";
            page += apName;
            page += "</h1>";
            page += "<h3>WiFiManager</h3>";
            page += HTTP_PORTAL_OPTIONS;
            page += HTTP_END;

            return page;
        }

        private string handleWifi(bool Scan = true)
        {
            string page = HTTP_HEAD;
            page = page.Replace("{v}", "Config ESP");
            page += HTTP_SCRIPT;
            page += HTTP_STYLE;
            page += customHeadElement;
            page += HTTP_HEAD_END;

            if (Scan)
            {               
                ScanNetworks();

                if (availableNetworks == null || availableNetworks.Count == 0)
                {
                    page += "No networks found. Refresh to scan again.";
                }
                else
                {
                    foreach (WiFiNetwork net in availableNetworks)
                    {
                        if (minimumSignalQuality == -1 || minimumSignalQuality < net.Rssi)
                        {

                            string item = HTTP_ITEM;
                            item = item.Replace("{v}", net.Ssid);
                            item = item.Replace("{r}", net.Rssi.ToString());
                            item = item.Replace("{i}", "");
                            page += item;
                        }
                        page += "<br/>";
                    }
                }
            }
            page += HTTP_FORM_START;

            foreach (WiFiManagerParameter paramItem in _params)
            {
                String pitem = HTTP_FORM_PARAM;

                if (paramItem.Id != null)
                {
                    pitem = pitem.Replace("{i}", paramItem.Id);
                    pitem = pitem.Replace("{n}", paramItem.Id);
                    pitem = pitem.Replace("{p}", paramItem.Placeholder);
                    pitem = pitem.Replace("{l}", paramItem.Value.Length.ToString());
                    pitem = pitem.Replace("{v}", paramItem.Value);
                    pitem = pitem.Replace("{c}", paramItem.CustomHTML);
                }
                else
                {
                    pitem = paramItem.CustomHTML;
                }
                page += pitem;
            }

            if (_params.Count > 0)
            {
                page += "<br/>";
            }

            //if (_sta_static_ip)
            //{

            //    String item = FPSTR(HTTP_FORM_PARAM);
            //    item.replace("{i}", "ip");
            //    item.replace("{n}", "ip");
            //    item.replace("{p}", "Static IP");
            //    item.replace("{l}", "15");
            //    item.replace("{v}", _sta_static_ip.toString());

            //    page += item;

            //    item = FPSTR(HTTP_FORM_PARAM);
            //    item.replace("{i}", "gw");
            //    item.replace("{n}", "gw");
            //    item.replace("{p}", "Static Gateway");
            //    item.replace("{l}", "15");
            //    item.replace("{v}", _sta_static_gw.toString());

            //    page += item;

            //    item = FPSTR(HTTP_FORM_PARAM);
            //    item.replace("{i}", "sn");
            //    item.replace("{n}", "sn");
            //    item.replace("{p}", "Subnet");
            //    item.replace("{l}", "15");
            //    item.replace("{v}", _sta_static_sn.toString());

            //    page += item;

            //    page += "<br/>";
            //}

            page += HTTP_FORM_END;
            page += HTTP_SCAN_LINK;

            page += HTTP_END;

            return page;
        }

        private string handleWifiSave(Hashtable formParams)
        {
            string ssid = (string)formParams["s"];
            string password = (string)formParams["p"];

            Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

            //if (server->arg("ip") != "")
            //{
            //    DEBUG_WM(F("static ip"));
            //    DEBUG_WM(server->arg("ip"));
            //    //_sta_static_ip.fromString(server->arg("ip"));
            //    String ip = server->arg("ip");
            //    optionalIPFromString(&_sta_static_ip, ip.c_str());
            //}
            //if (server->arg("gw") != "")
            //{
            //    DEBUG_WM(F("static gateway"));
            //    DEBUG_WM(server->arg("gw"));
            //    String gw = server->arg("gw");
            //    optionalIPFromString(&_sta_static_gw, gw.c_str());
            //}
            //if (server->arg("sn") != "")
            //{
            //    DEBUG_WM(F("static netmask"));
            //    DEBUG_WM(server->arg("sn"));
            //    String sn = server->arg("sn");
            //    optionalIPFromString(&_sta_static_sn, sn.c_str());
            //}


            String page = HTTP_HEAD;
            page.Replace("{v}", "Credentials Saved");
            page += HTTP_SCRIPT;
            page += HTTP_STYLE;
            page += customHeadElement;
            page += HTTP_HEAD_END;
            page += HTTP_SAVED;
            page += HTTP_END;

            // Enable the Wireless station interface
            Wireless80211.Configure(ssid, password);

            // Disable the Soft AP
            WirelessAP.Disable();

            return page;          
        }
    }
}
