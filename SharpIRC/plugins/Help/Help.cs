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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using SharpIRC.API;
using Mono.Addins;

namespace SharpIRC
{
    [Extension] public class Help : PluginInterface {
        public static List<HelpAddin> Addins = LoadHelpFiles();

        public static List<HelpAddin> LoadHelpFiles() {
            return AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetManifestResourceStream(asm.GetName().Name + ".Resources.Help.xml")).Select(str => DeserializeHelpFile(str)).Where(addin => addin != null).ToList();
        }

        public static HelpAddin DeserializeHelpFile(Stream str) {
            try  {
                var s = new XmlSerializer(typeof(HelpAddin));
                XmlReader r = XmlReader.Create(str, new XmlReaderSettings { CheckCharacters = false });

                var newList = (HelpAddin)s.Deserialize(r);
                r.Close();
                return newList;
            }
            catch {
                return null;
            }
        }
        public override void ChanMsg(ChannelMessage message)  {
            if (message.Message.IsCommand("help")) Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("You may reach my help system via \"/msg {0} HELP", message.Connection.CurrentNick));
        }
        public override void PrivMsg(PrivateMessage message) {
            if (message.Message.IsPMCommand("help")) {
                var subcmd = message.Message.GetMessageWithoutCommand();
                if (subcmd == null) {
                    Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0} provides many different services for managing channels, easily retrieving information, and just having some fun.",message.Connection.CurrentNick));
                    Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("The commands are grouped by what plugin they are from. To see more information about the commands available per plugin; type {1}/msg {0} HELP {2}Group{2}{1}. The groups are available below.",message.Connection.CurrentNick,(char)2,(char)31)); 
                    foreach (var addin in Addins) {
                        Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0} - {1}", addin.Name.ToUpper(), addin.Description));
                    }
                }
                else {
                    foreach (var addin in Addins.Where(addin => addin.Name == subcmd)) {
                        Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Displaying all channel commands in the group \"{1}\".", IRCColor.Blue(), addin.Name));
                        foreach (var cmd in addin.Commands.Where(cmd => cmd.Type == HelpCommandType.Channel)) {
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Syntax: {1}{2}", IRCColor.Blue(), Program.Configuration.CommandPrefix, FormatSyntax(cmd.Syntax)));
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Description: {1}", IRCColor.Blue(), cmd.Description));
                            Commands.SendNotice(message.Connection, message.Sender.Nick, " ");
                        }
                        Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Displaying all private message commands in the group \"{1}\".", IRCColor.Green(), addin.Name));
                        foreach (var cmd in addin.Commands.Where(cmd => cmd.Type == HelpCommandType.Query))
                        {
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Syntax: /msg {1} {2}", IRCColor.Green(), message.Connection.CurrentNick, FormatSyntax(cmd.Syntax)));
                            Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0}Description: {1}", IRCColor.Green(), cmd.Description));
                            Commands.SendNotice(message.Connection, message.Sender.Nick, " ");
                        }
                    }
                }
            }
        }
        public String FormatSyntax(string org) {
            var regex = new Regex(@"\$(?<replaceVar>.*?[^ ]+)\$");
            return regex.Matches(org).Cast<Match>().Aggregate((char)2+org+(char)2, (current, i) => current.Replace("$" + i.Groups["replaceVar"].Value + "$", String.Format("{0}{1}{0}", (char) 31, i.Groups["replaceVar"].Value)));
        }
    }
}
