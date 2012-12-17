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
using System.Net;
using System.Text.RegularExpressions;
using FrySharp.API;
using System.Web;

namespace FrySharp
{
    public class StargateWiki : PluginInterface, Plugin
    {
        #region PluginInterface Members

        public string WSDesc = "";
        public string WSLink = "";
        public string PluginName()
        {
            return "Stargate Wikipedia Search";
        }

        public string PluginAuthor()
        {
            return "Alex Sørlie";
        }

        public string PluginPurpose()
        {
            return "Searches Wikipedia for the wanted query.";
        }
        #endregion

        #region PluginInterface Members


        #endregion
        public static bool trustedon = false;
        public override void ChanMsg(IRCConnection connection, string host, string nick, string channel, string message)
        {
            if (message.isCommand("stargate"))
            {
                try
                {
                    var cmessage = message.Substring(Functions.getcommand(message).Length + 2);
                    cmessage = HttpUtility.UrlEncode(cmessage);
                    var address = "http://stargate.wikia.com/wiki/Special:Search?search=" + cmessage + "&go=1&x=0&y=0";
                    var client = new WebClient();
                    var wsite = client.DownloadString(address);
                    var getDesc = Regex.Match(wsite, "<meta name=\"description\" content=\"(.*?)\" />");
                    var description = getDesc.Groups[1].Value;
                    var getTitle = Regex.Match(wsite, "wgTitle=\"(.*?)\",");
                    var title = getTitle.Groups[1].Value;
                    var getUrl = Regex.Match(wsite, "wgPageName=\"(.*?)\",");
                    var url = "http://stargate.wikia.com/wiki/" + getUrl.Groups[1].Value;
                    if (description.Split('.').Length <= 2) description = String.Format("{0}.{1}", description.Split('.')[0], description.Split('.')[1]);
                    if (description.Length > 300)
                    {
                        description = description.BreakToLastWord(300);
                    }
                    if (wsite.Contains("Special:Search"))
                    {
                        Commands.Send_PrivMsg(connection, channel, string.Format("{0}Stargate Wikia{0} Chevron 7... will not lock! No results were found for your query."));
                    }
                    else
                    {
                        Commands.Send_PrivMsg(connection, channel,
                                              String.Format("{0}Stargate Wikipedia{0} {1} ({2})", (char) 2, title, url));
                        Commands.Send_PrivMsg(connection, channel,
                                              String.Format("{0}Description:{0} {1}", (char) 2, description));
                    }
                }
                catch
                {
                    
                }
            }
        }
    }
}