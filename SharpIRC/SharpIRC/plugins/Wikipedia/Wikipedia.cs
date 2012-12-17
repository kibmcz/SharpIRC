/*
Copyright 2011 KComputer Zone (http://kcomputerzone.ca)

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
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Wikipedia : PluginInterface {
        public static bool trustedon;

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("wiki")) {
                try {
                    string cmessage = message.Message.GetMessageWithoutCommand();
                    string address = "http://en.wikipedia.org/wiki/" + cmessage.Replace(' ', '_');
                    var client = new WebClient();

                    client.Headers.Add("user-agent", String.Format("SharpIRC IRC Bot/{0} http://nasutek.com/", Program.Version));
                    string wsite = client.DownloadString(address);
                    string desc = "";
                    var regex = new Regex(@"<p>.*?<b>.*?</b>.*?</p>");
                    var regexFallback = new Regex(@"<p>.*?</p>");

                    if (regex.IsMatch(wsite)) {
                        Match info = regex.Match(wsite);
                        desc = info.Value;
                    } else {
                        Match fallback = regexFallback.Match(wsite);
                        desc = fallback.Value;
                    }
                    desc = Regex.Replace(desc, "<.*?>", "");
                    desc = Regex.Replace(desc, @"\[(.*?)\]", "");
                    desc = desc.Replace(" ", " ");
                    desc = desc.Replace(" ,", ",");
                    if (desc.Split('.').Length <= 2) desc = String.Format("{0}.{1}", desc.Split('.')[0], desc.Split('.')[1]);
                    if (desc.Length > 400) desc = desc.BreakToLastWord(400);
                    Match getTitle = Regex.Match(wsite, "<span dir=\"auto\">(.*)</span>");
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Wikipedia{0} {1} ({2})", (char) 2, getTitle.Groups[1], address));
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Description:{0} {1}", (char) 2, desc));
                } catch {
                    Commands.SendPrivMsg(message.Connection, message.Channel, "There were no results matching the query.");
                }
            }
        }
    }
}
