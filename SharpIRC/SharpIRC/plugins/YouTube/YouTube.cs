using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class YouTube : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("youtube")) {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                string searchString = message.Message.GetMessageWithoutCommand();
                searchString = Regex.Replace(searchString, "%2b", "+", RegexOptions.IgnoreCase);
                searchString = HttpUtility.UrlEncode(searchString);
                string wsite = new WebClient().DownloadString("http://gdata.youtube.com/feeds/api/videos?q=" + searchString + "&prettyprint=true&alt=rss");
                var xDoc = new XmlDocument();
                xDoc.LoadXml(wsite);
                XmlNodeList ytTitle = xDoc.GetElementsByTagName("title");
                XmlNodeList ytDate = xDoc.GetElementsByTagName("pubDate");
                DateTime friendlyTime = DateTime.Parse(ytDate[0].InnerText);
                XmlNodeList ytDescription = xDoc.GetElementsByTagName("description");
                XmlNodeList ytLink = xDoc.GetElementsByTagName("link");
                XmlNodeList ytUploader = xDoc.GetElementsByTagName("author");
                if (ytTitle.Count >= 3) {
                    string desc = ytDescription[1].InnerText;
                    if (desc.Length > 300) desc = desc.BreakToLastWord(300);
                    Commands.SendPrivMsg(message.Connection, message.Channel,
                        String.Format("{0}YouTube{0} {1} ({2}) Uploaded at {3} by {4}", (char) 2, ytTitle[2].InnerText, ytLink[2].InnerText, friendlyTime.ToString("MMMM dd, yyyy"), ytUploader[2].InnerText));
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Description:{0} {1}", (char) 2, desc));
                } else Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}YouTube{0} No videos were found.", (char) 2));
            }
        }
    }
}
