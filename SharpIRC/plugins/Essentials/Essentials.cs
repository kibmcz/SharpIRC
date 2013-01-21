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
using System.Threading;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Essentials : PluginInterface {
        public override void ConfigurationChange(ConfigurationFile file) { if (file.Name == "Ignore") Program.IgnoreList = ConfigurationAPI.LoadConfigurationFile<Ignore>("Ignore"); }

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("raw") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "raw")) Commands.SendIRCMessage(message.Connection, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("say") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "say")) Commands.SendPrivMsg(message.Connection, message.Message.Split(' ')[1], message.Message.GetMessageWithoutSubCommand());
            if (message.Message.IsCommand("act") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "act")) Commands.SendAction(message.Connection, message.Message.Split(' ')[1], message.Message.GetMessageWithoutSubCommand());
            if (message.Message.IsCommand("rejoin") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "rejoin")) {
                if (message.Message.Split(' ').Length > 1) {
                    Commands.SendPart(message.Connection, message.Message.Split(' ')[1], message.Connection.Configuration.QuitMessage);
                    Commands.SendJoin(message.Connection, message.Message.Split(' ')[1]);
                } else {
                    Commands.SendPart(message.Connection, message.Channel, message.Connection.Configuration.QuitMessage);
                    Commands.SendJoin(message.Connection, message.Channel);
                }
            }
            if (message.Message.IsCommand("log") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "log")) {
                var lines = message.Message.GetMessageWithoutCommand() != null ? Convert.ToInt32(message.Message.GetMessageWithoutCommand()) : 10;

            }
            if (message.Message.IsCommand("restart") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "restart")) Program.Restart();
            if (message.Message.IsCommand("nick") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "nick")) Commands.SendNick(message.Connection, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("quit") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "quit")) Environment.Exit(0);
            if (message.Message.IsCommand("reconnect") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "reconnect")) {
                Commands.SendQuit(message.Connection, message.Connection.Configuration.QuitMessage);
                var newcon = new IRCConnection {Configuration = message.Connection.Configuration};
                Program.Connections.Add(newcon);
                new Thread(() => Parser.ConnnectToNetwork(newcon)).Start();
                Program.Connections.Remove(message.Connection);
            }
            if (!message.Message.IsCommand("ignore")) return;
            if (message.Message.IsSubCommand("add") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "ignore add")) {
                string newhost = message.Message.Split(' ')[2];
                if (!newhost.Contains("@")) newhost = "*!*@" + newhost.IRCUserFromString(message.Connection).Host;
                var newIgnore = new IgnoredUser {
                    Added = DateTime.Now,
                    Host = newhost,
                    Reason = Parser.JoinString(message.Message.Split(' '), 3, false)
                };
                Program.IgnoreList.Ignores.Add(newIgnore);
                ConfigurationAPI.SaveConfigurationFile(Program.IgnoreList, "Ignore");
                Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("Now added {0} to ignorelist.", newhost));
            }
            if (message.Message.IsSubCommand("del") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "ignore del")) {
                string newhost = message.Message.Split(' ')[2];
                if (!newhost.Contains("@")) newhost = newhost.IRCUserFromString(message.Connection).Host;
                bool exists = Program.IgnoreList.Ignores.Exists(x => x.Host == newhost);
                if (exists) {
                    Program.IgnoreList.Ignores.RemoveAll(x => x.Host == newhost);
                    ConfigurationAPI.SaveConfigurationFile(Program.IgnoreList, "Ignore");
                    Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("Removed {0} from ignore list.", newhost));
                } else Commands.SendNotice(message.Connection, message.Sender.Nick, String.Format("{0} does not exist in the ignore list.", newhost));
            }
            if (message.Message.IsSubCommand("list") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "ignore list")) {
                Commands.SendNotice(message.Connection, message.Sender.Nick, "Displaying all users that are currently ignored.");
                foreach (IgnoredUser user in Program.IgnoreList.Ignores) {
                    Commands.SendNotice(message.Connection, message.Sender.Nick,
                        String.Format("Host: {0}. Ignored at: {1}. Reason: {2}", user.Host, user.Added, user.Reason));
                }
                Commands.SendNotice(message.Connection, message.Sender.Nick, "Finished ignore listing.");
            }
        }

        public override void PrivMsg(PrivateMessage message) {
            if (message.Message.IsPMCommand("login")) {
                try {
                    if (message.Sender.IsBotAdmin()) {
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "You are already signed in as an administrator.");
                        return;
                    }
                    var username = message.Message.Split(' ')[1];
                    var password = message.Message.Split(' ')[2];
                    foreach (Admin admin in Program.Configuration.Admins.Where(admin => username == admin.Username && password == admin.Password)) {
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "You have authenticated as \"" + admin.Username + "\" and are now signed in as an administrator. . Your session will end in 60 minutes or when you quit.");
                        if (!Program.Configuration.DisableSessionTimer) SessionTimer(message.Connection, message.Sender.Nick);
                        Program.Sessions.Add(new LoggedInAdmin() {Nick = message.Sender.Nick, User = admin});
                        return;
                    }
                    Commands.SendPrivMsg(message.Connection,message.Sender.Nick,"Login failed: Incorrect username or password.");
                }
                catch {
                    
                }
            }
        }
        public override void CTCP(CTCPMessage message)
        {
            switch (message.Prefix) {
                case "VERSION": Commands.SendCTCPReply(message.Connection, message.Sender.Nick, String.Format("VERSION SharpIRC IRC Bot {0} http://nasutek.com/", Program.Version));
                    break;
                case "TIME": Commands.SendCTCPReply(message.Connection, message.Sender.Nick, String.Format("TIME {0}", DateTime.Now));
                    break;
                case "PING": Commands.SendCTCPReply(message.Connection, message.Sender.Nick, String.Format("PING {0}", Convert.ToInt32((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)));
                    break;
            }
        }
        public override void Notice(PrivateMessage message)
        {
            if (message.Sender.Nick == message.Connection.Configuration.AuthenticationService)
            {
                switch (message.Message)
                {
                    case "This nickname is registered. Please choose a different nickname, or identify via \x02/msg NickServ identify <password>\x02.":
                    case "This nickname is registered and protected.  If it is your":
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "IDENTIFY " + message.Connection.Configuration.AuthenticationPassword);
                        message.Connection.loginTimeout.Stop();
                        break;

                    case "This is a registered nick, please identify to NickServ now.":
                        Commands.SendPrivMsg(message.Connection, message.Sender.Nick, "ID " + message.Connection.Configuration.AuthenticationPassword);
                        message.Connection.loginTimeout.Stop();
                        break;
                }
            }
        }
        public static void SessionTimer(IRCConnection connection, string nick)
        {
            new Thread(new ThreadStart(delegate
            {
                Thread.Sleep(3600000);
                var templogin = Program.Sessions.ToList();
                foreach (LoggedInAdmin user in templogin.Where(user => user.Nick == nick))
                {
                    Program.Sessions.Remove(user);
                    Commands.SendPrivMsg(connection, nick, String.Format("Your 60 minute admin session has ended. \"/msg {0} login <Username> <Password>\" to log in again.", connection.CurrentNick));
                }
            })).Start();
        }
    }
}
