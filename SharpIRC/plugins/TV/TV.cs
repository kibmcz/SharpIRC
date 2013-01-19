/*
   Copyright 2009-2012 Alex Sørlie Glomsaas, Adonis S. Deliannis, Kevin Crowston

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
                client.Headers.Add("user-agent", String.Format("SharpIRC IRC Bot/{0} http://sharpirc.codeplex.com/", Program.Version));
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
