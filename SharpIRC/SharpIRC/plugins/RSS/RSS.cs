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
using System.Xml.Serialization;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class RSS : PluginInterface {
        public static RSSList MyRSS = ConfigurationAPI.LoadConfigurationFile<RSSList>("RSS");
        public static Caches Rsscache = ConfigurationAPI.LoadConfigurationFile<Caches>("RSS-Cache");
        public static List<Thread> Killallhumans = new List<Thread>();

        public override void ConfigurationChange(ConfigurationFile file) {
            if (file.Name == "RSS") MyRSS = ConfigurationAPI.LoadConfigurationFile<RSSList>("RSS");
        }

        public static void StartRSSTimers(IRCConnection connection) {
            foreach (RSSFeed feed in MyRSS.RSS) {
                new Thread(delegate(object thefeed) {
                    Killallhumans.Add(Thread.CurrentThread);
                    while (true) {
                        Thread.Sleep(((RSSFeed) thefeed).UpdateRate);
                        RefreshFeed((RSSFeed) thefeed);
                    }
                }).Start(feed);
            }
        }

        public static void StopRSSTimers() {
            foreach (Thread mythread in Killallhumans) mythread.Abort();
        }

        public static void RefreshFeed(RSSFeed feed) {
            try {
                var webClient = new WebClient();
                var xmlDocument = new XmlDocument();
                int limit = 0;
                xmlDocument.LoadXml(webClient.DownloadString(feed.URL));
                foreach (XmlNode i in xmlDocument.SelectNodes("/rss/channel/item")) {
                    limit++;
                    var rssFormatter = new RSSFormatter(feed.Layout, i);
                    if (!MatchesCache(rssFormatter.FormattedString)) {
                        IRCConnection rsscon = feed.ID.GetConnectionByGUID();
                        if (rsscon != null) {
                            foreach (string channel in feed.Channels) {
                                if (limit <= feed.Limit) Commands.SendPrivMsg(rsscon, channel, rssFormatter.FormattedString);
                                AddtoCache(rssFormatter.FormattedString);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                Connect.PrintError(ex.Message);
            }
        }

        public static void AddtoCache(string msg) {
            msg = HttpUtility.UrlEncode(msg);
            Rsscache.RSSCache.Add(msg);
            ConfigurationAPI.SaveConfigurationFile(Rsscache, "RSS-Cache");
        }

        public static bool MatchesCache(string matche) {
            bool returnal = false;
            matche = HttpUtility.UrlEncode(matche);
            foreach (string matcher in Rsscache.RSSCache.Where(matcher => matcher == matche && matche != null)) returnal = true;
            return returnal;
        }

        public override void ChanMsg(ChannelMessage message) {
            if (!message.Message.IsCommand("rss") || !message.Sender.IsBotAdmin()) return;
            string qmsg = message.Message.GetMessageWithoutCommand();
            if (message.Message.IsSubCommand("add")) {
                if (qmsg.Split(' ').Length >= 4) {
                    var newrss = new RSSFeed {
                        URL = qmsg.Split(' ')[1],
                        UpdateRate = (Convert.ToInt32(qmsg.Split(' ')[2])*1000),
                        NetworkName = message.Connection.ActiveNetwork,
                        Limit = Convert.ToInt32(qmsg.Split(' ')[4]),
                        ID = message.Connection.NetworkConfiguration.ID
                    };

                    string xchans = qmsg.Split(' ')[3];
                    string[] tchans = xchans.Split(',');
                    foreach (string chan in tchans) newrss.Channels.Add(chan);
                    if (qmsg.Split(' ').Length > 5) {
                        string rssmsg = Connect.JoinString(qmsg.Split(' '), 5, false);
                        newrss.Layout = rssmsg;
                    } else newrss.Layout = "RSS ~ %title% %link% @ %pubDate% ~";
                    MyRSS.RSS.Add(newrss);
                    ConfigurationAPI.SaveConfigurationFile(MyRSS, "RSS");
                    StopRSSTimers();
                    StartRSSTimers(message.Connection);
                    Commands.SendPrivMsg(message.Connection, message.Channel, "The RSS feed has been sucessfully added!");
                }
            }
            if (message.Message.IsSubCommand("del")) {
                int number = Convert.ToInt32(qmsg.Split(' ')[1]);
                if (MyRSS.RSS.Count >= number) {
                    int theint = 0;
                    foreach (RSSFeed feed in MyRSS.RSS) {
                        theint++;
                        if (theint == number) {
                            MyRSS.RSS.Remove(feed);
                            ConfigurationAPI.SaveConfigurationFile(MyRSS, "RSS");
                            StopRSSTimers();
                            StartRSSTimers(message.Connection);
                            Commands.SendPrivMsg(message.Connection, message.Channel, "Feed number " + number + " has been deleted.");
                            break;
                        }
                    }
                }
            }
            if (message.Message.IsSubCommand("list")) {
                Commands.SendNotice(message.Connection, message.Sender.Nick, "Displaying All RSS Feeds in my database.");
                int rfeed = 0;
                foreach (RSSFeed rssfeed in MyRSS.RSS) {
                    string limit = rssfeed.Limit.ToString();
                    if (limit == "0") limit = "Infinite";
                    rfeed++;
                    Commands.SendNotice(message.Connection, message.Sender.Nick,
                        String.Format("{0}{1}{0} URL: {2}. Update Rate: {3}.. Displayed in channels: {4}. Layout: {5} Limit: {6}", 
                        (char) 2, rfeed, rssfeed.URL, (rssfeed.UpdateRate/1000), RSSChannelList(rfeed), HttpUtility.UrlDecode(rssfeed.Layout), rssfeed.Limit));
                }
                Commands.SendNotice(message.Connection, message.Sender.Nick, "RSS Feed List displayed.");
            }
        }

        public string RSSChannelList(int rfeed) {
            int feed = 0;
            string returnal = "";
            foreach (RSSFeed rssfeed in MyRSS.RSS) {
                feed++;
                if (feed != rfeed) continue;
                foreach (string channel in rssfeed.Channels) {
                    if (returnal.Length == 0) returnal = channel;
                    else returnal = returnal + "," + channel;
                }
            }
            return returnal;
        }

        public override void Connected(ConnectedMessage message) { StartRSSTimers(message.Connection); }
    }

    public class RSSFormatter {
        private readonly string formattedString;

        public RSSFormatter(string formatString, XmlNode itemNode) {
            formattedString = formatString;

            var regex = new Regex(@"\%(?<replaceVar>.*?[^ ]+)\%");

            foreach (Match i in regex.Matches(formatString)) {
                XmlNode selectSingleNode = itemNode.SelectSingleNode(i.Groups["replaceVar"].Value);
                if (selectSingleNode == null) continue;
                string replaceVar = selectSingleNode.InnerText;
                formattedString = formattedString.Replace("%" + i.Groups["replaceVar"].Value + "%", replaceVar);
            }
        }

        public string FormattedString {
            get { return formattedString; }
        }
    }

    public class RSSFeed {
        public List<string> Channels = new List<string>();
        public string URL { get; set; }
        public int UpdateRate { get; set; }
        public string Layout { get; set; }
        [XmlAttribute("ID")] public Guid ID { get; set; }
        public string NetworkName { get; set; }
        public int Limit { get; set; }
    }

    public class RSSList {
        public List<RSSFeed> RSS = new List<RSSFeed>();
    }

    public class Caches {
        public List<string> RSSCache = new List<string>();
    }
}
