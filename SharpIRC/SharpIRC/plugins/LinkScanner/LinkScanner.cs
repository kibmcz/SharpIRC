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
            if (IsWhitelisted(message.Connection.NetworkConfiguration.ID, message.Channel) || !config.EnableWhitelist)  {
                Regex regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
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
            if (config.EnableWhitelist) {
                if (message.Message.IsCommand("linkscan") && (message.Sender.IsOperator() || message.Sender.IsBotAdmin())) {
                    if (message.Message.IsSubCommand("enable")) {
                        if (config.Whitelist.Any(chan => chan.Network == message.Connection.NetworkConfiguration.ID && chan.Channel == message.Channel)) {
                            Commands.SendNotice(message.Connection,message.Sender.Nick, "This channel already has linkscanner enabled.");
                            return;
                        }
                        config.Whitelist.Add(new WhitelistChan() { Channel = message.Channel, Network = message.Connection.NetworkConfiguration.ID });
                        ConfigurationAPI.SaveConfigurationFile(config, "LinkScanner");
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "LinkScanner is now enabled in this channel.");
                    }
                    if (message.Message.IsSubCommand("disable")){
                        if (config.Whitelist.Any(chan => chan.Network == message.Connection.NetworkConfiguration.ID && chan.Channel == message.Channel)) {
                            config.Whitelist.RemoveAll(chan => chan.Network == message.Connection.NetworkConfiguration.ID && chan.Channel == message.Channel);
                            ConfigurationAPI.SaveConfigurationFile(config, "LinkScanner");
                            Commands.SendNotice(message.Connection, message.Sender.Nick, "LinkScanner is now disabled.");
                            return;
                        }
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "This channel already has linkscanner disabled.");
                    }
                }
            }
        }
        public static bool IsWhitelisted(Guid Connection, String Channel) {
            return config.Whitelist.Any(chan => chan.Network == Connection && chan.Channel == Channel);
            
        }
    }
    public class LinkScannerConfig {
        public bool EnableWhitelist { get; set; }
        public List<WhitelistChan> Whitelist = new List<WhitelistChan>();
    }
    public class WhitelistChan {
        public Guid Network { get; set; }
        public String Channel { get; set; }
    }
}
