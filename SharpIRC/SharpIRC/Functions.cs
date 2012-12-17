using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            ModeStatus status = ModeStatus.Set;
            int user = 0;
            foreach (char x in modeArray) {
                if (nicks.Length <= user) break;
                IRCUser newUser = nicks[user].IRCUserFromString(connection);
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
            if (Program.GlobalSettings.LogComments) {
                string makePath = Path.Combine(Program.StartupPath, "Logs");
                if (!Directory.Exists(makePath)) Directory.CreateDirectory(makePath);
                makePath = Path.Combine(makePath, DateTime.Now.ToString("dd-MM-yyyy") + ".txt");
                fs = File.Open(makePath, FileMode.Append, FileAccess.Write);
                sw = new StreamWriter(fs, Encoding.UTF8);
                sw.WriteLine("[" + DateTime.Now.ToString("M HH:mm:ss") + "] ERROR: " + error);
                sw.Close();
                fs.Close();
            }
        }

        public static UserLevel ModeToLevel(string chanmode) {
            UserLevel level = UserLevel.Normal;
            switch (chanmode[0]) {
                case ((char) 43):
                    level = UserLevel.Voice;
                    break;
                case ((char) 37):
                    level = UserLevel.Halfop;
                    break;
                case ((char) 64):
                    level = UserLevel.Operator;
                    break;
                case ((char) 38):
                    level = UserLevel.Admin;
                    break;
                case ((char) 126):
                    level = UserLevel.Owner;
                    break;
            }
            return level;
        }

        public static void LogHistory(string Network, String Name, string Text) {
            if (Program.GlobalSettings.ChatHistory) {
                try {
                    string mPath = String.Format("{0}{1}Chat History{1}{2}{1}{3}", Program.StartupPath, Path.DirectorySeparatorChar, Network, Name);
                    if (!Directory.Exists(mPath)) Directory.CreateDirectory(mPath);
                    FileStream fs = File.Open(Path.Combine(mPath, DateTime.Now.ToString("dd-MM-yyyy") + ".txt"), FileMode.Append, FileAccess.Write);
                    var sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine("[" + DateTime.Now.ToString("M HH:mm:ss") + "] " + Text);
                    sw.Close();
                    fs.Close();
                } catch {
                }
            }
        }

        public static bool UpdateNickList(IRCConnection connection, string channel, string whomessage) {
            try {
                string[] sms = whomessage.Split(' ');
                string nickname = sms[3];
                string hostname = sms[1];
                string chanmode = sms[4].Substring(sms[4].Length - 1, 1);
                Console.WriteLine(chanmode);
                string realname = Connect.JoinString(sms, 6, false);
                foreach (NickList ch in connection.Nicklist.Where(temp => temp.Channel == channel)) {
                    foreach (IRCUser nk in ch.Nicks.Where(nk => nk.Nick == nickname)) {
                        nk.Level = ModeToLevel(chanmode);
                        return true;
                    }
                    ch.Nicks.Add(new IRCUser {Host = hostname, Level = ModeToLevel(chanmode), Nick = nickname, RealName = realname});
                    return true;
                }
                connection.Nicklist.Add(new NickList {Channel = channel, Nicks = new List<IRCUser> {new IRCUser {Host = hostname, Level = ModeToLevel(chanmode), Nick = nickname, RealName = realname}}});
                return true;
            } catch {
                return false;
            }
        }

        internal static void LogError(Exception ex) {
            Connect.PrintError(ex.Message);
            LogError(Convert.ToString(ex.GetBaseException()));
        }
    }
}
