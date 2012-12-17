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
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using PluginArchitecture;
using System.Web;

namespace FrySharp
{
    public class Weather : PluginInterface, Plugin
    {
        #region PluginInterface Members
        public string PluginName()
        {
            return "Weather";
        }

        public string PluginAuthor()
        {
            return "Alex Sørlie";
        }

        public string PluginPurpose()
        {
            return "Retrive the difference in time between now and a the given location";
        }
        #endregion

        #region PluginInterface Members


        #endregion
        public override void ChanMsg(IRCConnection connection, string host, string nick, string channel, string message)
        {
            if (message.isCommand("weather"))
            {
                var address = "http://weather.mikeable.com/?q=" + message.GetMessageWithoutCommand();
                var wsite = new WebClient().DownloadString(address);
                string[] substrings = Regex.Split(wsite, @"--------------");
                Commands.Send_PrivMsg(connection,channel,String.Format("{0}Weather{0} {1}",(char)2,substrings[0]));
                if (substrings.Length > 1) Commands.Send_PrivMsg(connection,channel,String.Format("{0}04{1}",(char)3,substrings[1]));
            }
        }
    }
}