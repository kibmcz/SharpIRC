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
using System.Threading;
using System.Web;
using System.Xml;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Steam : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {

            if (message.Message.IsCommand("steam")) {
                string address = "https://steamcommunity.com/id/" + message.Message.GetMessageWithoutCommand() +
                                 "/?xml=1";
                string wsite = new WebClient().DownloadString(address);
                try {
                    var xDoc = new XmlDocument();
                    xDoc.LoadXml(wsite);
                    XmlNodeList steamError = xDoc.GetElementsByTagName("error");
                    if (steamError.Count > 0) {
                        Commands.SendPrivMsg(message.Connection, message.Channel, steamError[0].InnerText);
                        return;
                    }
                    XmlNodeList steamRealname = xDoc.GetElementsByTagName("realname");
                    XmlNodeList steamSteamId = xDoc.GetElementsByTagName("steamID");
                    XmlNodeList steamVacBanned = xDoc.GetElementsByTagName("vacBanned");
                    XmlNodeList steamMemberSince = xDoc.GetElementsByTagName("memberSince");
                    XmlNodeList steamSteamRating = xDoc.GetElementsByTagName("steamRating");
                    XmlNodeList steamHoursPlayed2Wk = xDoc.GetElementsByTagName("hoursPlayed2Wk");
                    XmlNodeList steamMostPlayed = xDoc.GetElementsByTagName("gameName");
                    XmlNodeList steamHoursPlayed = xDoc.GetElementsByTagName("hoursPlayed");
                    XmlNodeList steamHoursOnRecord = xDoc.GetElementsByTagName("hoursOnRecord");
                    XmlNodeList steamStatus = xDoc.GetElementsByTagName("stateMessage");
                    XmlNodeList steamLocation = xDoc.GetElementsByTagName("location");
                    string vacbanned = "Clean";
                    string identname = steamRealname[0].InnerText;
                    if (identname.Length == 0) identname = steamSteamId[0].InnerText;
                    if (steamVacBanned[0].InnerText == "1") vacbanned = "Banned";
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format(
                                             "Steam stats for {1} ({2}): {0}Status:{0} {3}. {0}Location:{0} {4}. {0}Member Since:{0} {5}. {0}Steam Rating:{0} " +
                                             "{6}/10. {0}Playing Time:{0} {7} hours the last 2 weeks.. {0}VAC Status:{0} {8}. {0}Most Played Game:{0} {9} @ {10} hours the past two weeks ({11} total).",
                                             (char) 2,
                                             identname,
                                             message.Message.GetMessageWithoutCommand(),
                                             HtmlDecode(steamStatus[0].InnerText),
                                             steamLocation[0].InnerText,
                                             steamMemberSince[0].InnerText,
                                             steamSteamRating[0].InnerText,
                                             steamHoursPlayed2Wk[0].InnerText,
                                             vacbanned,
                                             steamMostPlayed[0].InnerText,
                                             steamHoursPlayed[0].InnerText,
                                             steamHoursOnRecord[0].InnerText));
                }
                catch  {
                }
            }
        }

        public string HtmlDecode(string htmlstring) {
            htmlstring = HttpUtility.HtmlDecode(htmlstring);
            return Regex.Replace(htmlstring, "<.*?>", string.Empty);
        }
    }
}
