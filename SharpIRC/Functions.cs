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
using System.Text.RegularExpressions;
using System.Threading;
using SharpIRC.API;

namespace SharpIRC {
    internal static class Functions {
        public static string NickFromHost(string host) { return host.Split('!')[0]; }

        public static List<ModeChange> StringtoModeList(string modes) {
            modes = modes.Split(' ')[0];
            var modelist = new List<ModeChange>();
            char[] modeArray = modes.ToCharArray();
            ModeStatus status = ModeStatus.Set;
            foreach (char x in modeArray) {
                switch (x) {
                    case '+':
                        status = ModeStatus.Set;
                        break;
                    case '-':
                        status = ModeStatus.Removed;
                        break;
                }
                var newMode = new ModeChange {Mode = x, Action = status};
                modelist.Add(newMode);
            }
            return modelist;
        }

        public static List<UserModeChange> StringtoUserModeList(string modes, IRCConnection connection, string channel) {
            var modelist = new List<UserModeChange>();
            char[] modeArray = modes.Split(' ')[0].ToCharArray();
            string[] nicks = modes.Split(' ');
            var status = ModeStatus.Set;
            int user = 0;
            foreach (char x in modeArray) {
                if (nicks.Length <= user) break;
                IRCUser newUser = nicks[user].IRCUserFromString(connection.GetChannelByName(channel));
                if (x == '+') status = ModeStatus.Set;
                if (x == '-') status = ModeStatus.Removed;
                var newMode = new UserModeChange {Mode = x, Action = status, Nick = newUser};
                modelist.Add(newMode);
                user++;
            }
            return modelist;
        }
        public static void LogError(string error) {
            FileStream fs = File.Open(Path.Combine(Program.StartupPath, "ErrorLog.txt"), FileMode.Append, FileAccess.Write);
            var sw = new StreamWriter(fs, Encoding.UTF8);
            sw.WriteLine("---------------[" + DateTime.Now.ToString("MM/dd/yy HH:mm:ss zz") + "]---------------");
            sw.WriteLine(error);
            sw.WriteLine();
            sw.Close();
            fs.Close();
            if (Program.Configuration.LogComments) {
                string makePath = Path.Combine(Program.StartupPath, "Logs");
                if (!Directory.Exists(makePath)) Directory.CreateDirectory(makePath);
                makePath = Path.Combine(makePath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                fs = File.Open(makePath, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("[" + DateTime.Now.ToString("M HH:mm:ss") + "] ERROR: " + error);
                sw.Close();
                fs.Close();
            }
        }
        public static void AssertUModes(IRCConnection connection, string umodes) {
            connection.Umodes = new IRCDUMode();
            var match = Regex.Match(umodes, @"\((.*?)\)(.*)");
            var udef = match.Groups[1].Value;
            var uchar = match.Groups[2].Value;
            for (var i = 0; i < udef.Length; i++) {
                if (udef[i] == 'Y') connection.Umodes.IRCOP = uchar[i];
                if (udef[i] == 'q') connection.Umodes.Owner = uchar[i];
                if (udef[i] == 'a') connection.Umodes.Admin = uchar[i];
                if (udef[i] == 'o') connection.Umodes.Operator = uchar[i];
                if (udef[i] == 'h') connection.Umodes.Halfop = uchar[i];
                if (udef[i] == 'v') connection.Umodes.Voice = uchar[i];
            }
        }

        public static UserLevel ModeToLevel(IRCConnection connection, string chanmode) {
            if (chanmode[0] == connection.Umodes.IRCOP) return UserLevel.IRCOP;
            if (chanmode[0] == connection.Umodes.Owner) return UserLevel.Owner;
            if (chanmode[0] == connection.Umodes.Admin) return UserLevel.Admin;
            if (chanmode[0] == connection.Umodes.Operator) return UserLevel.Operator;
            if (chanmode[0] == connection.Umodes.Halfop) return UserLevel.Halfop;
            if (chanmode[0] == connection.Umodes.Voice) return UserLevel.Voice;
            return UserLevel.Normal;
        }

        public static void LogHistory(string Network, String Name, string Text) {
            if (Program.Configuration.ChatHistory) {
                try {
                    string mPath = String.Format("{0}{1}History{1}{2}{1}{3}", Program.StartupPath, Path.DirectorySeparatorChar, Network, Name);
                    if (!Directory.Exists(mPath)) Directory.CreateDirectory(mPath);
                    FileStream fs = File.Open(Path.Combine(mPath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt"), FileMode.Append, FileAccess.Write);
                    var sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine("[" + DateTime.Now.ToString("M HH:mm:ss") + "] " + Text);
                    sw.Close();
                    fs.Close();
                } catch {
                }
            }
        }

        public static void UpdateNickList(IRCConnection connection, string channel, string whomessage) {
            string[] sms = whomessage.Split(' ');
            string nickname = sms[3];
            string hostname = sms[1];
            string chanmode = sms[4].Substring(sms[4].Length - 1, 1);
            string realname = Connect.JoinString(sms, 6, false);
            connection.GetChannelByName(channel).Nicks.RemoveAll(x => x.Nick == nickname);
            connection.GetChannelByName(channel).Nicks.Add(new IRCUser {
                                                                             Host = hostname,
                                                                             Level = ModeToLevel(connection, chanmode),
                                                                             Nick = nickname,
                                                                             RealName = realname
                                                                         });
        }

        internal static void LogError(Exception ex) {
            Program.OutputConsole(ex.GetBaseException().ToString(), ConsoleMessageType.Error);
            LogError(Convert.ToString(ex.GetBaseException()));
        }

        public static void UpdateNickChange(IRCConnection connection, string oldnick, string newnick) {
            foreach (var user in from chan in connection.Channels from user in chan.Nicks where user.Nick == oldnick select user) {
                user.Nick = newnick;
            }
        }

        public static string Base64Encode(string data) {
            try {
                var encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                var encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e) {
                return null;
            }
        }
    }
}
