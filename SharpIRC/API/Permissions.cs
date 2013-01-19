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
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SharpIRC.API
{
    /// <summary>
    /// The class that handles command permissions.
    /// </summary>
    public static class Permissions {
        /// <summary>
        /// List of loaded permission files.
        /// </summary>
        public static List<PermissionIndex> PermissionsList;

        /// <summary>
        /// Loads permission data from ressource files.
        /// </summary>
        /// <returns>Permission data</returns>
        public static List<PermissionIndex> LoadPermissionsData() {
            var path = Path.Combine(Program.StartupPath, "Permissions");
            return Directory.GetFiles(path).Select(x => DeserializePermissionsFile(Path.Combine(path, x))).Where(file => file != null).ToList();
        }

        /// <summary>
        /// Reads XML permission files.
        /// </summary>
        /// <param name="path">Path to the permissions file</param>
        /// <returns>Permissions</returns>
        public static PermissionIndex DeserializePermissionsFile(string path) {
            try {
                var s = new XmlSerializer(typeof(PermissionIndex));
                XmlReader r = XmlReader.Create(path, new XmlReaderSettings { CheckCharacters = false });
                var newList = (PermissionIndex)s.Deserialize(r);
                r.Close();
                return newList;
            }
            catch (Exception ex) {
                Program.OutputConsole(ex.GetBaseException().ToString(), ConsoleMessageType.Error);
                return null;
            }
        }

        /// <summary>
        /// Whether or not this command is registered in the permissions system.
        /// </summary>
        /// <param name="mCommand">Command</param>
        /// <returns>Boolean value</returns>
        public static bool isRegisteredCommand(this string mCommand) {
            return PermissionsList.SelectMany(index => index.Commands).Any(command => command.Name == mCommand);
        }

        /// <summary>
        /// Whether or not the user has permission to access this command.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="mCommand">Command name.</param>
        /// <returns>Boolean value.</returns>
        public static bool hasCommandPermission(this IRCUser user, IRCConnection connection, Channel channel, string mCommand) {
            if (FloodControl.Timeouts.ChannelTimeouts.Any(x => x.Network == connection.Configuration.ID && x.Channel == channel.Name))
                return false;
            if (FloodControl.Timeouts.UserTimeouts.Any(x => x.Network == connection.Configuration.ID && x.Nick == user.Nick))
                return false;
            if (Program.Configuration.FloodControl) FloodControl.AddCommand(connection, channel, user);
            foreach (var permission in (from index in PermissionsList from command in index.Commands where command.Name == mCommand select command).SelectMany(command => command.Permissions)) {
                if (permission.Type == "Admin") {
                    if (user.IsBotAdmin()) return true;
                }
                if (permission.Type == "ChannelStatus") {
                    if (permission.Channel.Length == 0) {
                        if (permission.Value == "Owner" && user.IsOwner()) return true;
                        if (permission.Value == "Admin" && user.IsAdmin()) return true;
                        if (permission.Value == "Operator" && user.IsOperator()) return true;
                        if (permission.Value == "Halfop" && user.IsHalfop()) return true;
                        if (permission.Value == "Voice" && user.IsVoice()) return true; 
                    } 
                    else {
                        if (permission.Value == "Owner" && user.IsOwner(new Guid(permission.NetworkUUID).GetConnectionByGUID().GetChannelByName(permission.Channel))) return true;
                        if (permission.Value == "Admin" && user.IsAdmin(new Guid(permission.NetworkUUID).GetConnectionByGUID().GetChannelByName(permission.Channel))) return true;
                        if (permission.Value == "Operator" && user.IsOperator(new Guid(permission.NetworkUUID).GetConnectionByGUID().GetChannelByName(permission.Channel))) return true;
                        if (permission.Value == "Halfop" && user.IsHalfop(new Guid(permission.NetworkUUID).GetConnectionByGUID().GetChannelByName(permission.Channel))) return true;
                        if (permission.Value == "Voice" && user.IsVoice(new Guid(permission.NetworkUUID).GetConnectionByGUID().GetChannelByName(permission.Channel))) return true;
                    }
                }
                if (permission.Type == "Nick") {
                    if (user.Nick == permission.Value) return true;
                }
                if (permission.Type == "Normal") {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Index of all commands from the file.
    /// </summary>
    public class PermissionIndex {
        /// <summary>
        /// Command list.
        /// </summary>
        public List<Command> Commands { get; set; }
    }

    /// <summary>
    /// Specific command object.
    /// </summary>
    public class Command {
        /// <summary>
        /// Name of command.
        /// </summary>
        [XmlAttribute("Name")] public string Name { get; set; }
        /// <summary>
        /// Permissions set for specific command.
        /// </summary>
        [XmlElement]
        public List<Permission> Permissions { get; set; }
    }

    /// <summary>
    /// Permission.
    /// </summary>
    public class Permission {
        /// <summary>
        /// Type of permission.
        /// </summary>
        [XmlAttribute("Type")] public string Type { get; set; }
        /// <summary>
        /// The channel in question (if any)
        /// </summary>
        [XmlAttribute("Channel")] public string Channel { get; set; }
        /// <summary>
        /// The network in question (if any)
        /// </summary>
        [XmlAttribute("NetworkUUID")] public string NetworkUUID { get; set; }
        /// <summary>
        /// The value.
        /// </summary>
        [XmlText] public String Value { get; set; }
    }
}
