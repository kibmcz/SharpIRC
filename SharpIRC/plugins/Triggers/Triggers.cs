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
using System.Xml.Serialization;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Triggers : PluginInterface {
        public static TriggerList Triggerlist = ConfigurationAPI.LoadConfigurationFile<TriggerList>("Triggers");

        public override void ConfigurationChange(ConfigurationFile file) {
            if (file.Name == "Triggers") Triggerlist = ConfigurationAPI.LoadConfigurationFile<TriggerList>("Triggers");
        }

        public override void ChanMsg(ChannelMessage message) {
            var thisnet = (from net in Triggerlist.Nets where net.ID == message.Connection.NetworkConfiguration.ID.ToString() select net).FirstOrDefault();
            if (thisnet != null) {
                foreach (Trigger trig in thisnet.Triggers.Where(trig => trig.Command.IsCommand(message.Message) && trig.Channel == message.Channel.Name)) {
                    if (trig.Action == "message") Commands.SendPrivMsg(message.Connection, message.Channel, trig.Message);
                    if (trig.Action == "notice") Commands.SendNotice(message.Connection, message.Sender.Nick, trig.Message);
                }
            }
            if (!message.Message.IsCommand("trigger")) return;
            TriggerNet oldnet;
            if (message.Message.IsSubCommand("add") && message.Sender.IsOperator()) {
                oldnet = (from net in Triggerlist.Nets where net.ID == message.Connection.NetworkConfiguration.ID.ToString() select net).FirstOrDefault();
                if (oldnet == null) {
                    var newnet = new TriggerNet {ID = message.Connection.NetworkConfiguration.ID.ToString()};
                    oldnet = newnet;
                }
                var newdata = new Trigger {
                    Action = message.Message.Split(' ')[2],
                    Command = message.Message.Split(' ')[3],
                    Message = Connect.JoinString(message.Message.Split(' '), 4, false),
                    Channel = message.Channel.Name
                };
                oldnet.Triggers.RemoveAll(x => x.Command == message.Message.Split(' ')[3]);
                Triggerlist.Nets.RemoveAll(x => x.ID == message.Connection.NetworkConfiguration.ID.ToString());
                oldnet.Triggers.Add(newdata);
                Triggerlist.Nets.Add(oldnet);
                ConfigurationAPI.SaveConfigurationFile(Triggerlist, "Triggers");
                Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("The command ~{0} with the {1} response \"{2}\" has been added for {3}",
                    message.Message.Split(' ')[3], message.Message.Split(' ')[2], Connect.JoinString(message.Message.Split(' '), 4, false), message.Channel));
            }
            if (message.Message.IsSubCommand("del") && message.Sender.IsOperator()) {
                oldnet = (from net in Triggerlist.Nets where net.ID == message.Connection.NetworkConfiguration.ID.ToString() select net).FirstOrDefault();
                if (oldnet != null) {
                    bool exists = oldnet.Triggers.Exists(x => x.Command == message.Message.Split(' ')[2]);
                    if (exists) {
                        oldnet.Triggers.RemoveAll(x => x.Command == message.Message.Split(' ')[2]);
                        Triggerlist.Nets.RemoveAll(x => x.ID == message.Connection.NetworkConfiguration.ID.ToString());
                        Triggerlist.Nets.Add(oldnet);
                        ConfigurationAPI.SaveConfigurationFile(Triggerlist, "Triggers");
                        Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("The command {0} has been deleted from {1}", message.Message.Split(' ')[2], message.Channel));
                    } else Commands.SendPrivMsg(message.Connection, message.Channel, "This command does not exsist.");
                } else Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("No commands has been added for {0}", message.Channel));
            }
            if (!message.Message.IsSubCommand("list")) return;
            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("Displaying all custom commands for {0}", message.Channel));
            oldnet = (from net in Triggerlist.Nets where net.ID == message.Connection.NetworkConfiguration.ID.ToString() select net).FirstOrDefault();
            if (oldnet != null) foreach (Trigger trig in oldnet.Triggers) Commands.SendNotice(message.Connection, message.Sender.Nick, 
                String.Format("Command: {0}, Type: {1}, Message: {2}", trig.Command, trig.Action, trig.Message));
            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("Completed custom command listing for {0}", message.Channel));
        }
    }

    public class TriggerList {
        public List<TriggerNet> Nets = new List<TriggerNet>();
    }

    public class TriggerNet {
        public List<Trigger> Triggers = new List<Trigger>();
        [XmlAttribute("ID")] public string ID { get; set; }
    }

    public class Trigger {
        public string Action { get; set; }
        public string Command { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; }
    }
}
