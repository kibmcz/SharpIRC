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
using System.Text.RegularExpressions;
using System.Web;
using Cognize.GoogleSearchAPIIntegration;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Google : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("google") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "google")) {
                var cmessage = message.Message.GetMessageWithoutCommand();
                var searchParam = cmessage;
                var results = GoogleSearch.GetSearchResults(searchParam, 1);
                if (results.Count > 0) {
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Google Search{0} {1} ({2})", (char)2, HtmlDecode(results[0].titleNoFormatting), results[0].unescapedUrl));
                    Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Description:{0} {1}", (char)2, HtmlDecode(results[0].content)));
                }
                else Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0} Google Search{0} No results were found.",(char)2));
            }
        }

        public string HtmlDecode(string htmlstring) {
            htmlstring = HttpUtility.HtmlDecode(htmlstring);
            return Regex.Replace(htmlstring, "<.*?>", string.Empty);
        }
    }
}
