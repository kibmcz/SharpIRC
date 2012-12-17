using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class TV : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("tv")) {
                string cmessage = HttpUtility.UrlEncode(message.Message.GetMessageWithoutCommand());
                string address = String.Format("http://tv.com/search?type=11&stype=all&tag=search;frontdoor&q={0}", cmessage);
                var client = new WebClient();
                client.Headers.Add("user-agent", String.Format("SharpIRC IRC Bot/{0} http://nasutek.com/", Program.Version));
                string wsite = client.DownloadString(address);
                Match gres = Regex.Match(wsite, "<a href=\"(.*)\"><img src=\"(.*)\" alt=\"(.*)\"></a>");
                if (gres.Success) {
                    string result = HtmlDecode(gres.Groups[2].Value);
                }
            }
        }

        public string HtmlDecode(string htmlstring) {
            htmlstring = HttpUtility.HtmlDecode(htmlstring);
            return Regex.Replace(htmlstring, "<.*?>", string.Empty);
        }
    }
}
