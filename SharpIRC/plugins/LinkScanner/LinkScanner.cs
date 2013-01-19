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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC
{
    [Extension] public class LinkScanner : PluginInterface {
        public static LinkScannerConfig config = ConfigurationAPI.LoadConfigurationFile<LinkScannerConfig>("LinkScanner");
        public LinkScanner() {
            ConfigurationAPI.SaveConfigurationFile(config,"LinkScanner");
        }

        public string FormatToUTF8(string format)
        {
            byte[] fmt = Encoding.Default.GetBytes(format);
            return Encoding.UTF8.GetString(fmt);
        }

        public override void ChanMsg(ChannelMessage message) {
            if (IsWhitelisted(message.Connection.Configuration.ID, message.Channel.Name))  {
                var regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
                MatchCollection mactches = regx.Matches(message.Message);
                foreach (Match match in mactches) {
                    Match match1 = match;
                    new Thread(delegate()  {
                        try {
                            var client = new WebClient();
                            string wsite = client.DownloadString(match1.Value);
                            var matchTitle = Regex.Match(wsite, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase);
                            if (matchTitle.Success) {
                                var matchLink = match1.Value;
                                if (matchLink.Length > 100) matchLink = matchLink.Substring(0, 100);
                                var matchDesc = Regex.Match(wsite, "<meta name=\"description\" content=\"(.*)\">");
                                if (!matchDesc.Success) {
                                    Commands.SendPrivMsg(message.Connection, message.Channel,
                                        FormatToUTF8(String.Format("{0}Link Detected{0} {1} - {0}Original URL:{0} {2}", (char)2, HttpUtility.HtmlDecode(matchTitle.Groups["Title"].Value).BreakToLastWord(300).Replace(Environment.NewLine, ""), matchLink)));
                                }
                                else {
                                    Commands.SendPrivMsg(message.Connection, message.Channel,
                                        FormatToUTF8(String.Format("{0}Link Detected{0} {1} - {0}Original URL:{0} {2}", (char)2, HttpUtility.HtmlDecode(matchTitle.Groups["Title"].Value).BreakToLastWord(300).Replace(Environment.NewLine, ""), matchLink)));
                                    Commands.SendPrivMsg(message.Connection, message.Channel,
                                        FormatToUTF8(String.Format("{0}Description:{0} {1}", (char)2, HttpUtility.HtmlDecode(matchDesc.Groups[1].ToString()).BreakToLastWord(300).Replace(Environment.NewLine, ""))));
                                }
                            }
                        }
                        catch
                        {
                        }
                    }).Start();
                }
            }
            if (message.Message.IsCommand("linkscan") && (message.Sender.IsOperator() || message.Sender.IsBotAdmin())) {
                if (message.Message.IsSubCommand("enable")) {
                    if (
                        config.Whitelist.Any(
                            chan =>
                            chan.Network == message.Connection.Configuration.ID &&
                            chan.Channel == message.Channel.Name)) {
                        Commands.SendNotice(message.Connection, message.Sender.Nick,
                                            "This channel already has linkscanner enabled.");
                        return;
                    }
                    config.Whitelist.Add(new WhitelistChan() {
                                                                 Channel = message.Channel.Name,
                                                                 Network = message.Connection.Configuration.ID
                                                             });
                    ConfigurationAPI.SaveConfigurationFile(config, "LinkScanner");
                    Commands.SendNotice(message.Connection, message.Sender.Nick,
                                        "LinkScanner is now enabled in this channel.");
                }
                if (message.Message.IsSubCommand("disable")) {
                    if (
                        config.Whitelist.Any(
                            chan =>
                            chan.Network == message.Connection.Configuration.ID &&
                            chan.Channel == message.Channel.Name)) {
                        config.Whitelist.RemoveAll(
                            chan =>
                            chan.Network == message.Connection.Configuration.ID &&
                            chan.Channel == message.Channel.Name);
                        ConfigurationAPI.SaveConfigurationFile(config, "LinkScanner");
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "LinkScanner is now disabled.");
                        return;
                    }
                    Commands.SendNotice(message.Connection, message.Sender.Nick,
                                        "This channel already has linkscanner disabled.");
                }
            }
        }
        public static bool IsWhitelisted(Guid Connection, String Channel) {
            return config.Whitelist.Any(chan => chan.Network == Connection && chan.Channel == Channel);
            
        }
    }
    public class LinkScannerConfig {
        public List<WhitelistChan> Whitelist = new List<WhitelistChan>();
    }
    public class WhitelistChan {
        public Guid Network { get; set; }
        public String Channel { get; set; }
    }
}
