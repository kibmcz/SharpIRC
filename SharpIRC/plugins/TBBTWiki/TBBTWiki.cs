using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Fry_2_0;
using PluginArchitecture;
using System.Web;

namespace FuturamaWiki
{
    public class FuturamaWiki : PluginInterface, Plugin
    {
        #region PluginInterface Members

        public string WSDesc = "";
        public string WSLink = "";
        public string PluginName()
        {
            return "The Big Bang Theory Wikipedia Search";
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
            if (string.Equals(Functions.getcommand(message), "tbbt", StringComparison.OrdinalIgnoreCase) && Functions.isTrusted(nick, trustedon))
            {
                var cmessage = message.Substring(Functions.getcommand(message).Length + 2);
                cmessage = HttpUtility.UrlEncode(cmessage);
                var address = "http://bigbangtheory.wikia.com/wiki/Special:Search?search=" + cmessage + "&go=1&x=0&y=0";
                var client = new WebClient();
                var wsite = client.DownloadString(address);
                var lines = Regex.Split(wsite, "\n");
                foreach (string line in lines)
                {
                    if (line.Contains("<meta name=\"description\" content="))
                    {
                        WSDesc = HttpUtility.HtmlDecode(line.Split('"')[3]); 
                    }
                    if (line.Contains("<link rel=\"canonical\" href="))
                    {
                        WSLink = line.Split('"')[3];
                    }
                    if (line.Contains("var wgTitle") && WSDesc.Length > 0)
                    {
                        var WSTitle = line.Split('"')[1];
                        Commands.Send_PrivMsg(connection, channel, IFCommands.bold + "The Big Bang Theory Wikipedia " + IFCommands.bold + WSTitle + " - http://bigbangtheory.wikia.com" + WSLink);
                        Commands.Send_PrivMsg(connection, channel, IFCommands.bold + "Description: " + IFCommands.bold + WSDesc);
                    }
                    if (line.Contains("No search results for that term."))
                    {
                        Commands.Send_PrivMsg(connection, channel, IFCommands.bold + "The Big Bang Theory Wikipedia " + IFCommands.bold + "Your search returned no results.");
                    }
                }
                WSDesc = "";
                WSLink = "";
            }
        }
    }
}