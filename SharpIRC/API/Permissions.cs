using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SharpIRC.API
{
    public static class Permissions {
        public static List<PermissionIndex> PermissionsList;

        public static List<PermissionIndex> LoadPermissionsData() {
            var x = new PermissionIndex() { Commands = new List<Command>() { new Command() { Name = "TestPermission", Permissions = new List<Permission>
                       {new Permission() {Channel = "#kteck-labs", NetworkUUID = "010010101", Type = "Nick", Value = "lol"}}}}};
            ConfigurationAPI.SaveConfigurationFile(x, "testXMLPErmissions");
            return AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetManifestResourceStream(asm.GetName().Name + ".Resources.Permissions.xml")).Select(DeserializePermissionsFile).Where(addin => addin != null).ToList();
        }

        public static PermissionIndex DeserializePermissionsFile(Stream str) {
            try {
                var s = new XmlSerializer(typeof(HelpAddin));
                var r = XmlReader.Create(str, new XmlReaderSettings { CheckCharacters = false });

                var newList = (PermissionIndex)s.Deserialize(r);
                r.Close();
                return newList;
            }
            catch {
                return null;
            }
        }

        public static bool isRegisteredCommand(this string mCommand) {
            return PermissionsList.SelectMany(index => index.Commands).Any(command => command.Name == mCommand);
        }

        public static bool hasCommandPermission(this IRCUser user, string mCommand) {
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

    public class PermissionIndex {
        public List<Command> Commands { get; set; }
    }

    public class Command {
        [XmlAttribute("Name")] public string Name { get; set; }
        [XmlElement]
        public List<Permission> Permissions { get; set; }
    }

    public class Permission {
        [XmlAttribute("Type")] public string Type { get; set; }
        [XmlAttribute("Name")] public string Channel { get; set; }
        [XmlAttribute("NetworkUUID")] public string NetworkUUID { get; set; }
        [XmlText] public String Value { get; set; }
    }
}
