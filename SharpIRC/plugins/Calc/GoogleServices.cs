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
    [Extension] public class GoogleServices : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            string address;
            var client = new WebClient();
            client.Headers["User-Agent"] = "User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.63 Safari/535.7";
            string wsite;
            string cmessage;
            if (message.Message.IsCommand("calc")) {
                cmessage = message.Message.GetMessageWithoutCommand();
                cmessage = Regex.Replace(cmessage, "%2b", "+", RegexOptions.IgnoreCase);
                cmessage = HttpUtility.UrlEncode(cmessage);
                address = String.Format("http://www.google.com/search?hl=en&q={0}", cmessage);
                wsite = client.DownloadString(address);
                Match gres = Regex.Match(wsite, "<h2 class=r style=\"font-size:138%\" ><b>(.*)</b></h2>");
                Commands.SendPrivMsg(message.Connection, message.Channel, 
                    gres.Success ? String.Format("{0}Google Calc{0} {1}", (char) 2, HtmlDecode(gres.Groups[0].ToString())) : String.Format("{0}Google Calc{0} No Results were found.", (char) 2));
            }
            if (message.Message.IsCommand("define")) {
                cmessage = message.Message.GetMessageWithoutCommand();
                cmessage = Regex.Replace(cmessage, "%2b", "+", RegexOptions.IgnoreCase);
                cmessage = HttpUtility.UrlEncode(cmessage);
                address = String.Format("http://www.google.com/search?hl=en&q=define:{0}", cmessage);
                wsite = client.DownloadString(address);
                string name = HtmlDecode(Regex.Match(wsite, "<span style=\"padding-bottom:14px;padding-right:15px\"><em>(.*)</em></span>").Groups[1].ToString());
                string type = Regex.Match(wsite, 
                    "<td valign=\"top\" width=\"80px\" style=\"padding-bottom:5px;padding-top:5px;color:#666\">(.*):</td><td valign=\"top\" style=\"padding-bottom:5px;padding-top:5px\">"
                    ).Groups[1].ToString().Split(':')[0];

                Group definition = Regex.Match(wsite, "<table class=\"ts\"><tr><td>(.*)</td></tr></table></td></tr><tr height=\"1px\" bgcolor=\"#ddd\">").Groups[1];
                Group synonymes = Regex.Match(wsite, 
                    "<td valign=\"top\" style=\"padding-bottom:5px;padding-top:5px\"><div><span style=\"color:#666\"><i>(.*)</i>.&nbsp;&nbsp;</span>(.*)</div><div><span style=\"color:#666\">").Groups[2];
                if (name.Length > 0) {
                    if (definition.Length == 0) definition = Regex.Match(wsite, "<table class=\"ts\"><tr><td>(.*)</td></tr></table></td></tr></table><div class=gl>").Groups[1];
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Google Dictionairy{0} {1}: {2}", (char) 2, type, name));
                    Commands.SendPrivMsg(message.Connection,
                        message.Channel,
                        synonymes.Length > 0
                            ? String.Format("{0}Definition:{0} {1} {0}Synonymes:{0} {2}", (char) 2, HtmlDecode(definition.ToString()), synonymes)
                            : String.Format("{0}Definition:{0} {1}", (char) 2, HtmlDecode(definition.ToString())));
                } else Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Google Dictionairy{0} Unable to find any results for your query.", (char) 2));
            }
            if (message.Message.IsCommand("market")) {
                cmessage = message.Message.GetMessageWithoutCommand();
                cmessage = Regex.Replace(cmessage, "%2b", "+", RegexOptions.IgnoreCase);
                cmessage = HttpUtility.UrlEncode(cmessage);
                address = String.Format("http://www.google.com/finance?q={0}", cmessage);
                wsite = client.DownloadString(address);
                Match nameRegex = Regex.Match(wsite, "<div class=\"g-unit g-first\"><h3>(.*)&nbsp;&nbsp;</h3>\\((.*)\\)&nbsp;&nbsp;");
                Match valueRegex = Regex.Match(wsite, "<span id=\"ref_([0-9]{1,20})_l\">(.*)</span>");
                Match priceRangeRegex = Regex.Match(wsite, "<span class=\"(chg|chr|chb)\"(.*)id=\"ref_([0-9]{1,20})_c\">(.*)</span>");
                Match percentageRegex = Regex.Match(wsite, "<span class=\"(chg|chr|chb)\"(.*)id=\"ref_([0-9]{1,20})_cp\">\\((.*)\\)</span>");
                Commands.SendPrivMsg(message.Connection, message.Channel, nameRegex.Success
                        ? String.Format("{0}Google Finance{0} {1} {2} - {3} ({4}, {5})", (char) 2, nameRegex.Groups[1], nameRegex.Groups[2], valueRegex.Groups[2], priceRangeRegex.Groups[4], percentageRegex.Groups[4])
                        : String.Format("{0}Google Finance{0} No results found.", ((char) 2)));
            }
            if (!message.Message.IsCommand("weather")) return;
            cmessage = message.Message.GetMessageWithoutCommand();
            cmessage = Regex.Replace(cmessage, "%2b", "+", RegexOptions.IgnoreCase);
            cmessage = HttpUtility.UrlEncode(cmessage);
            address = String.Format("http://www.google.com/search?hl=en&q=weather:{0}", cmessage);
            client.Encoding = System.Text.Encoding.UTF8;
            wsite = client.DownloadString(address);
            wsite = HttpUtility.HtmlDecode(wsite);
            Match locationRegex = Regex.Match(wsite, "<h3 class=r><b>Weather</b> for <b>(.*)</b></h3></div>");
            Match temperatureRegex = Regex.Match(wsite, 
                "<td style=\"font-size:140%;white-space:nowrap;vertical-align:top;padding-right:15px;font-weight:bold\" rowspan=2>(.*)Â°C<td style=\"width:5px;border-left:solid 1px #d8d8d8\" rowspan=5>");
            Match typeRegex = Regex.Match(wsite, "<td style=\"white-space:nowrap;padding-right:15px;color:#666\">(.*)<td><td><td><tr><td style=\"white-space:nowrap;padding-right:15px;color:#666\">Wind");
            Match humidityRegex = Regex.Match(wsite, "#666\">Humidity: (.*)%<td");
            Match windRegex = Regex.Match(wsite, "<td><td><td><tr><td (.*)>Wind: (.*)<td><td><td><tr>");
            Commands.SendPrivMsg(message.Connection, message.Channel, locationRegex.Success ? String.Format("{0}Google Weather{0} for {1}: {2}. Temperature: {3}°C. Wind: {4}. Humidity: {5}",
                        (char) 2,
                        locationRegex.Groups[1],
                        typeRegex.Groups[1],
                        temperatureRegex.Groups[1],
                        windRegex.Groups[2],
                        humidityRegex.Groups[1] + "%")
                    : String.Format("{0}Google Weather{0} Your location was not found.", (char) 2));
        }

        public string HtmlDecode(string htmlstring) {
            htmlstring = HttpUtility.HtmlDecode(htmlstring);
            return Regex.Replace(htmlstring, "<.*?>", string.Empty);
        }
    }
}
