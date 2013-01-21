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
using System.Threading;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class LoveHate : PluginInterface {
        public static filter lovefilter = ConfigurationAPI.LoadConfigurationFile<filter>("lovehate-filter");
        public static List<Cache> LoveCache = new List<Cache>();

        public override void ConfigurationChange(ConfigurationFile file) { if (file.Name == "lovehate-filter") lovefilter = ConfigurationAPI.LoadConfigurationFile<filter>("lovehate-filter"); }

        public bool Passfilter(string message) {
            bool passfilter = true;
            foreach (Parser.Wildcard wcard in lovefilter.wildcards.Select(wildcard => new Parser.Wildcard(wildcard, RegexOptions.IgnoreCase)).Where(wcard => wcard.IsMatch(message))) passfilter = false;
            return passfilter;
        }

        public string Matchpreset(string lovenick1, string lovenick2) {
            string passfilter = null;
            foreach (Preset pset in from pset in lovefilter.presets
                let wcard = new Parser.Wildcard(pset.Nick1, RegexOptions.IgnoreCase)
                let wcard1 = new Parser.Wildcard(pset.Nick2, RegexOptions.IgnoreCase)
                where wcard.IsMatch(lovenick1) && wcard1.IsMatch(lovenick2)
                select pset) passfilter = pset.Love.ToString();
            return passfilter;
        }

        public bool MatchLoveCache(string love1, string love2) {
            bool match = false;
            foreach (Cache cache in LoveCache.Where(cache => love1 == cache.Love1 && love2 == cache.Love2)) match = true;
            return match;
        }

        public int CacheResult(string love1, string love2) {
            int match = 0;
            foreach (Cache cache in LoveCache.Where(cache => love1 == cache.Love1 && love2 == cache.Love2)) match = cache.Match;
            return match;
        }

        public void RemoveFromCache(Cache cache) {
            new Thread(new ThreadStart(delegate {
                Thread.Sleep(12800000);
                LoveCache.Remove(cache);
            })).Start();
        }

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("love") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "love")) {
                string cmessage = message.Message.GetMessageWithoutCommand();
                if (cmessage.Split('&').Length == 2) {
                    if (Passfilter(cmessage)) {
                        string lovenick1 = cmessage.Split('&')[0];
                        string lovenick2 = cmessage.Split('&')[1];

                        if (MatchLoveCache(lovenick1, lovenick2)) {
                            int love = CacheResult(lovenick1, lovenick2);
                            Commands.SendPrivMsg(message.Connection, message.Channel, lovenick1 + "and" + lovenick2 + " has " + love + "% on the relationship scale. " + Rlcomment(love));
                        } else if (Matchpreset(lovenick1, lovenick2) == null) {
                            int random = new Random().Next(1, 200);
                            random = random - 100;
                            var newcache = new Cache {Love1 = lovenick1, Love2 = lovenick2, Match = random};
                            LoveCache.Add(newcache);
                            RemoveFromCache(newcache);
                            Commands.SendPrivMsg(message.Connection, message.Channel, lovenick1 + "and" + lovenick2 + " has " + random + "% on the relationship scale. " + Rlcomment(random));
                        } else {
                            Commands.SendPrivMsg(message.Connection, message.Channel, lovenick1 + "and" + lovenick2 + " has " + Matchpreset(lovenick1, lovenick2) + "% on the relationship scale. " + 
                                Rlcomment(Convert.ToInt32(Matchpreset(lovenick1, lovenick2))));
                        }
                    } else {
                        Commands.SendAction(message.Connection, message.Channel, "shoves " + message.Sender.Nick + " into a cryogenic tube and set it for 1000 years.");
                    }
                } else {
                    Commands.SendPrivMsg(message.Connection, message.Channel, "You did not enter a second name. \"~love name1 & name2\" Don't forget the &");
                }
            }
        }

        public string Rlcomment(int random) {
            string message = "";
            if (random < -89) message = "WOW! You are the worst enemies I've ever seen O_O";
            else if (random < -69) message = "You two should really try to get better along, this relationship is ice cold.";
            else if (random < -49) message = "What did you two do to hate each other so bad?";
            else if (random < -29) message = "tsk tsk.. Hate brings no good to the world!";
            else if (random < -9) message = "Just a little misunderstanding? you'll get along sooner or later.";
            else if (random < 9) message = "Do you two even know each other? o.0";
            else if (random < 29) message = "You're barely friends -_-";
            else if (random < 49) message = "Good friends :D";
            else if (random < 69) message = "Awwwww.. Best friends!!";
            else if (random < 89) message = "Puppy love.. isn't that adorable!";
            else if (random < 100) message = "Wow you two really love each other o.o ";
            else if (random == 100) message = "TRUE LOVE! I hear wedding bells :D";
            return message;
        }
    }

    public class Cache {
        public string Love1 { get; set; }
        public string Love2 { get; set; }
        public int Match { get; set; }
    }
}
