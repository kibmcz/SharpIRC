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
    [Extension] public class Eightball : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("8ball") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "8ball")) {
                if (Validate8Ball(message.Message.GetMessageWithoutCommand())) {
                    var ra = new[] {
                        "As I see it, yes",
                        "It is certain",
                        "It is decidedly so",
                        "Most likely",
                        "Outlook good",
                        "Signs point to yes",
                        "Without a doubt",
                        "Yes",
                        "Yes – definitely",
                        "You may rely on it",
                        "Reply hazy, try again",
                        "Ask again later",
                        "Better not tell you now",
                        "Cannot predict now",
                        "Concentrate and ask again",
                        "Don't count on it",
                        "My reply is no",
                        "My sources say no",
                        "Outlook not so good",
                        "Very doubtful"
                    };
                    int rnd = new Random().Next(0, ra.Length - 1);
                    Commands.SendPrivMsg(message.Connection, message.Channel, ra[rnd]);

                    string[] lol = new String[] { "lol" };
                    Console.WriteLine(lol[904]);
                } else Commands.SendPrivMsg(message.Connection, message.Channel, "How about you ask a real question stupid.");
            }
        }

        public bool Validate8Ball(string q) {
            var ci = new CultureInfo("en-US");
            bool validated = q.StartsWith("am i", true, ci);
            if (q.StartsWith("are", true, ci)) validated = true;
            if (q.StartsWith("Is ", true, ci)) validated = true;
            if (q.StartsWith("does", true, ci)) validated = true;
            if (q.StartsWith("do", true, ci)) validated = true;
            if (q.StartsWith("would", true, ci)) validated = true;
            if (q.StartsWith("will", true, ci)) validated = true;
            if (q.StartsWith("did", true, ci)) validated = true;
            if (q.StartsWith("can", true, ci)) validated = true;
            if (q.StartsWith("was", true, ci)) validated = true;
            if (q.StartsWith("were", true, ci)) validated = true;
            if (q.StartsWith("should", true, ci)) validated = true;
            if (!q.EndsWith(("?"))) validated = false;
            if (q.Split(' ').Length < 2) validated = false;
            return validated;
        }
    }
}
