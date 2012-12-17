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
using System.Globalization;
using System.Linq;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension]
    public class Uptime : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("uptime")) {
                var ts = DateTime.Now.Subtract(Connect.startTime);
                Commands.SendPrivMsg(message.Connection, message.Channel.Name, String.Format("I've been up for {0} days, {1} hours, {2} minutes, and {3} seconds.", ts.Days, ts.Hours, ts.Minutes, ts.Seconds));
            }
        }
    }
}