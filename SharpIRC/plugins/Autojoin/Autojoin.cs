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
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Autojoin : PluginInterface {
        public static AutojoinDatabase autojoin = ConfigurationAPI.LoadConfigurationFile<AutojoinDatabase>("Autojoin");


        public override void ConfigurationChange(ConfigurationFile file) { if (file.Name == "Autojoin") autojoin = ConfigurationAPI.LoadConfigurationFile<AutojoinDatabase>("Autojoin"); }
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("autojoin") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "autojoin")) {
                string chantoAdd = message.Message.GetMessageWithoutSubCommand().ToLower();
                if (message.Message.IsSubCommand("add")) {
                    if (autojoin.Networks.Count(net => net.ID == message.Connection.Configuration.ID) == message.Connection.MaxChannels) {
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "Request unsuccessful: You have reached the channel limit for this server (" + message.Connection.MaxChannels + ").");
                    }
                    if (autojoin.Networks.Count(net => net.ID == message.Connection.Configuration.ID) > 0) foreach (var net in autojoin.Networks.Where(net => net.ID == message.Connection.Configuration.ID)) net.Channels.Add(chantoAdd);
                    else {
                        var newnet = new AutoJoinNetwork {ID = message.Connection.Configuration.ID};
                        newnet.Channels.Add(chantoAdd);
                        autojoin.Networks.Add(newnet);
                    }
                    Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been added to autojoin.", chantoAdd));
                    Commands.SendJoin(message.Connection, chantoAdd);
                    ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                }
                if (message.Message.IsSubCommand("del")) {
                    foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.Configuration.ID)) {
                        if (net.Channels.Remove(chantoAdd)) {
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been removed from autojoin.", chantoAdd));
                            ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                            Commands.SendPart(message.Connection, chantoAdd, "Removed from autojoin.");
                        } else Commands.SendNotice(message.Connection, message.Sender.Nick, "Deletion was unsucessful, channel was not found.");
                    }
                }
                if (message.Message.IsSubCommand("list")) {
                    foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.Configuration.ID)) {
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "Listing all channels in autojoin for " + message.Connection.ActiveNetwork);
                        foreach (string chan in net.Channels) Commands.SendNotice(message.Connection, message.Sender.Nick, chan);
                        Commands.SendNotice(message.Connection, message.Sender.Nick, "Autojoin listing complete.");
                    }
                }
            }
        }

        public override void Authorized(IRCConnection connection) {
            foreach (string chan in autojoin.Networks.Where(net => net.ID == connection.Configuration.ID).SelectMany(net => net.Channels)) Commands.SendJoin(connection, chan);
        }

        public override void LoginTimedOut(IRCConnection connection) {
            foreach (string chan in autojoin.Networks.Where(net => net.ID == connection.Configuration.ID).SelectMany(net => net.Channels)) Commands.SendJoin(connection, chan);
            Commands.SendPrivMsg(connection, connection.Configuration.SetupChannel, "Authentication failed. Continuing without.");
        }

        #region Nested type: AutoJoinNetwork
        public class AutoJoinNetwork {
            public List<string> Channels = new List<string>();
            [XmlAttribute("ID")] public Guid ID { get; set; }
        }
        #endregion

        #region Nested type: AutojoinDatabase
        public class AutojoinDatabase {
            public List<AutoJoinNetwork> Networks = new List<AutoJoinNetwork>();
        }
        #endregion
    }
}
