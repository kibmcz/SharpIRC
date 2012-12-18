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

        public override void PrivMsg(PrivateMessage message) {
            if (message.Message.IsPMCommand("autojoin") && message.Sender.IsBotAdmin()) {
                string chantoAdd = message.Message.GetMessageWithoutSubCommand().ToLower();
                if (message.Message.IsSubCommand("add")) {
                    if (autojoin.Networks.Count(net => net.ID == message.Connection.NetworkConfiguration.ID) > 0) 
                        foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) net.Channels.Add(chantoAdd);
                    else {
                        var newnet = new AutoJoinNetwork {ID = message.Connection.NetworkConfiguration.ID};
                        newnet.Channels.Add(chantoAdd);
                        autojoin.Networks.Add(newnet);
                    }
                    Commands.SendPrivMsg(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been added to autojoin.", chantoAdd));
                    ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                }
                if (message.Message.IsSubCommand("del")) {
                    foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) {
                        if (net.Channels.Remove(chantoAdd)) {
                            Commands.SendPrivMsg(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been removed from autojoin.", chantoAdd));
                            ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                        } else Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "Deletion was unsucessful, channel was not found.");
                    }
                }
                if (message.Message.IsSubCommand("list")) {
                    foreach (var net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) {
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "Listing all channels in autojoin for " + message.Connection.ActiveNetwork);
                        int cha = 0;
                        foreach (string chan in net.Channels) {
                            cha++;
                            Commands.SendPrivMsg(message.Connection, message.Sender.Nick, String.Format("{0}. {1}", cha, chan));
                        }
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "Autojoin listing complete.");
                    }
                }
            }
        }

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("autojoin") && message.Sender.IsBotAdmin()) {
                string chantoAdd = message.Message.GetMessageWithoutSubCommand().ToLower();
                if (message.Message.IsSubCommand("add")) {
                    if (autojoin.Networks.Count(net => net.ID == message.Connection.NetworkConfiguration.ID) > 0) foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) net.Channels.Add(chantoAdd);
                    else {
                        var newnet = new AutoJoinNetwork {ID = message.Connection.NetworkConfiguration.ID};
                        newnet.Channels.Add(chantoAdd);
                        autojoin.Networks.Add(newnet);
                    }
                    Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been added to autojoin.", chantoAdd));
                    Commands.SendJoin(message.Connection, chantoAdd);
                    ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                }
                if (message.Message.IsSubCommand("del")) {
                    foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) {
                        if (net.Channels.Remove(chantoAdd)) {
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("The channel {0} has been removed from autojoin.", chantoAdd));
                            ConfigurationAPI.SaveConfigurationFile(autojoin, "Autojoin");
                            Commands.SendPart(message.Connection, chantoAdd, "Removed from autojoin.");
                        } else Commands.SendNotice(message.Connection, message.Sender.Nick, "Deletion was unsucessful, channel was not found.");
                    }
                }
                if (!message.Message.IsSubCommand("list")) return;
                foreach (AutoJoinNetwork net in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID)) {
                    Commands.SendNotice(message.Connection, message.Sender.Nick, "Listing all channels in autojoin for " + message.Connection.ActiveNetwork);
                    foreach (string chan in net.Channels) Commands.SendNotice(message.Connection, message.Sender.Nick, chan);
                    Commands.SendNotice(message.Connection, message.Sender.Nick, "Autojoin listing complete.");
                }
            }
        }

        public override void Notice(PrivateMessage message) {
            if (message.Sender.Nick == message.Connection.NetworkConfiguration.AuthenticationService) {
                if (new Regex("You are now identified for (.*)", RegexOptions.IgnoreCase).Match(message.Message).Success || message.Message == "Password accepted - you are now recognized.") {
                    foreach (string chan in autojoin.Networks.Where(net => net.ID == message.Connection.NetworkConfiguration.ID).SelectMany(net => net.Channels)) Commands.SendJoin(message.Connection, chan);
                }
            }
        }

        public override void LoginTimedOut(IRCConnection connection) {
            foreach (string chan in autojoin.Networks.Where(net => net.ID == connection.NetworkConfiguration.ID).SelectMany(net => net.Channels)) Commands.SendJoin(connection, chan);
            Commands.SendPrivMsg(connection, connection.NetworkConfiguration.SetupChannel, "30 seconds has passed without 9");
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
