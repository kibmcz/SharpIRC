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
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Giveaway : PluginInterface {
        public static string giveaway = "";

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("giveaway") && message.Message.GetMessageWithoutCommand().Length > 0) {
                if (message.Sender.IsOperator()) {
                    giveaway = message.Message.GetMessageWithoutCommand();
                    Commands.SendPrivMsg(message.Connection, message.Channel.Name, String.Format("Giveaway object now set to: {0}", message.Message.GetMessageWithoutCommand()));
                }
            }
            if (!message.Message.IsCommand("winner")) return;
            string winner = "";
            winner = message.Channel.Nicks[new Random().Next(0, message.Channel.Nicks.Count)].Nick;
            Commands.SendPrivMsg(message.Connection, message.Channel.Name, String.Format("The winner of {0} is: {1}!", giveaway, winner));
        }
    }
}
