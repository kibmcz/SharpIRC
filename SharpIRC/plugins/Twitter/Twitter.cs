using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fry_2_0;
using PluginArchitecture;
using Twitterizer.Core;
using Twitterizer.Commands;

namespace iTwitter
{
    public class iTwitter : PluginInterface, Plugin
    {
        public string PluginName()
        {
            return "Twitter";
        }

        public string PluginAuthor()
        {
            return "Alex Sørlie";
        }

        public string PluginPurpose()
        {
            return "Kibmcz Personal Twitter Script";
        }
        public override void ChanMsg(string host, string nick, string channel, string message)
        {
            if (Functions.getcommand(message) == "twitter" && Functions.IsAdmin(nick))
            {
                Twitter mytwitter = new Twitter();
                mytwitter.
            }
        }
    }
}
