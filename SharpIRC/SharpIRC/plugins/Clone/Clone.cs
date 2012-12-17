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
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Clone : PluginInterface {
        public List<ClonedUser> Clones = new List<ClonedUser>();

        public override void ChanMsg(ChannelMessage message) {
            foreach (ClonedUser user in Clones.Where(user => user.ID == message.Connection.NetworkConfiguration.ID && user.channel == message.Channel && user.nick == message.Sender.Nick)) {
                Commands.SendPrivMsg(message.Connection, message.Channel, message.Message);
            }
            if (!message.Message.IsCommand("clone") || message.Message.GetMessageWithoutCommand().Length <= 0 || !message.Sender.IsBotAdmin()) return;
            string cmessage = message.Message.GetMessageWithoutCommand();
            bool exists = false;
            if (message.Message.IsSubCommand("add")) {
                foreach (ClonedUser user in Clones.Where(user => user.nick == cmessage.Split(' ')[1])) exists = true;
                if (!exists) {
                    var newclone = new ClonedUser {
                        nick = cmessage.Split(' ')[1],
                        ID = message.Connection.NetworkConfiguration.ID,
                        channel = message.Channel
                    };
                    Clones.Add(newclone);
                    Commands.SendPrivMsg(message.Connection, message.Channel, "Now cloning " + cmessage.Split(' ')[1]);
                } else Commands.SendPrivMsg(message.Connection, message.Channel, cmessage.Split(' ')[0] + " is already being cloned.");
            }
            if (message.Message.IsSubCommand("del")) {
                ClonedUser tuser = null;
                foreach (ClonedUser user in Clones.Where(user => user.nick == cmessage.Split(' ')[1])) tuser = user;
                if (tuser != null) {
                    Clones.Remove(tuser);
                    Commands.SendPrivMsg(message.Connection, message.Channel, "I am now no longer cloning " + cmessage.Split(' ')[1]);
                } else Commands.SendPrivMsg(message.Connection, message.Channel, cmessage.Split(' ')[1] + " is not being cloned.");
            }
            if (!message.Message.IsSubCommand("list")) return;
            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("List of users currently cloned."));
            foreach (ClonedUser user in Clones) Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format(String.Format("Channel: {0}. Nickname: {1}", user.channel, user.nick)));
            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("End of List."));
        }

        public override void ChanAction(ChannelMessage message) {
            foreach (ClonedUser user in Clones.Where(user => user.ID == message.Connection.NetworkConfiguration.ID && user.channel == message.Channel && user.nick == message.Sender.Nick)) {
                Commands.SendAction(message.Connection, message.Channel, message.Message);
            }
        }

        public override void NickChange(NickChangeMessage message) {
            ClonedUser nickdata = null;
            foreach (ClonedUser user in Clones.Where(user => user.nick == message.Sender.Nick)) nickdata = user;
            if (nickdata == null) return;
            Commands.SendPrivMsg(message.Connection, nickdata.channel, "Detected nickchange of cloned user. Updating database.");
            Clones.Remove(nickdata);
            nickdata.nick = message.NewNick;
            Clones.Add(nickdata);
        }
    }

    public class ClonedUser {
        public string nick { get; set; }
        public string channel { get; set; }
        public Guid ID { get; set; }
    }
}
