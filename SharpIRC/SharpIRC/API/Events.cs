using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpIRC.API {
    internal class Events {
        internal void ChanMsg(IRCConnection connection, string message, string host, string channel) {
            if (host.IsIgnored() || connection.DelayActive) return;
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);
            var newMsg = new ChannelMessage {Channel = channel.ToLower(), Connection = connection, Message = message, Sender = newUser, Type = MessageTypes.PRIVMSG};
            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, channel, String.Format("({0}) <{1}> {2}", newMsg.Sender.Level, newMsg.Sender.Nick, newMsg.Message));

            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.ChanMsg(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void ChanAction(IRCConnection connection, string message, string host, string channel) {
            if (host.IsIgnored() || connection.DelayActive) return;
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new ChannelMessage {Channel = channel.ToLower(), Connection = connection, Message = message, Sender = newUser, Type = MessageTypes.ACTION};

            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, channel, String.Format("({0}) * {1} {2}", newMsg.Sender.Level, newMsg.Sender.Nick, newMsg.Message));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.ChanAction(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void PrivMsg(IRCConnection connection, string message, string host) {
            if (host.IsIgnored()) return;
            var newUser = new IRCUser {Host = host, Level = UserLevel.Normal, Nick = Functions.NickFromHost(host), RealName = ""};

            var newMsg = new PrivateMessage {Connection = connection, Sender = newUser, Message = message, Type = MessageTypes.PRIVMSG};

            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Sender.Nick, String.Format("<{0}> {1}", newMsg.Sender.Nick, newMsg.Message));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.PrivMsg(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void PrivAction(IRCConnection connection, string message, string host) {
            if (host.IsIgnored()) return;
            var newUser = new IRCUser {Host = host, Level = UserLevel.Normal, Nick = Functions.NickFromHost(host), RealName = ""};

            var newMsg = new PrivateMessage {Connection = connection, Sender = newUser, Message = message, Type = MessageTypes.ACTION};
            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Sender.Nick, String.Format("* {0} {1}", newMsg.Sender.Nick, newMsg.Message));
            foreach (PluginInterface Plugin in Program.Plugins) {
                try {
                    Plugin.PrivAction(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void CTCP(IRCConnection connection, string message, string host, string channel) {
            if (host.IsIgnored()) return;
            var newUser = new IRCUser {Host = host, Level = UserLevel.Normal, Nick = Functions.NickFromHost(host), RealName = ""};

            var newMsg = new CTCPMessage
            {Connection = connection, Sender = newUser, Prefix = message.Split(' ')[0].Substring(0, message.Split(' ')[0].Length - 1), Message = Connect.JoinString(message.Split(' '), 1, false), Type = MessageTypes.CTCP};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.CTCP(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Notice(IRCConnection connection, string message, string host) {
            if (host.IsIgnored()) return;
            var newUser = new IRCUser {Host = host, Level = UserLevel.Normal, Nick = Functions.NickFromHost(host), RealName = ""};

            var newMsg = new PrivateMessage {Connection = connection, Sender = newUser, Message = message, Type = MessageTypes.NOTICE};
            
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Notice(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void ChanNotice(IRCConnection connection, string message, string host, string channel) {
            if (host.IsIgnored() || connection.DelayActive) return;
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new ChannelMessage {Channel = channel.ToLower(), Connection = connection, Sender = newUser, Message = message, Type = MessageTypes.NOTICE};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.ChanNotice(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void CTCPREPLY(IRCConnection connection, string message, string host) {
            if (host.IsIgnored()) return;
            var newUser = new IRCUser {Host = host, Level = UserLevel.Normal, Nick = Functions.NickFromHost(host), RealName = ""};

            var newMsg = new CTCPMessage {Connection = connection, Sender = newUser, Prefix = message.Split(' ')[0], Message = Connect.JoinString(message.Split(' '), 1, false), Type = MessageTypes.CTCPREPLY};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.CTCPREPLY(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Join(IRCConnection connection, string host, string channel) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);
            var newMsg = new JoinMessage {Connection = connection, Sender = newUser, Channel = channel.ToLower(), Type = MessageTypes.JOIN};
            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Channel, String.Format("- {0} has joined the channel ({1})", newMsg.Sender.Nick, newMsg.Channel));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Join(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Part(IRCConnection connection, string host, string channel, string message) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new PartMessage {Connection = connection, Sender = newUser, Message = message, Channel = channel.ToLower(), Type = MessageTypes.PART};
            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Channel, String.Format("- {0} has left the channel (Message: )", newMsg.Sender.Nick, newMsg.Message));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Part(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Quit(IRCConnection connection, string host, string message) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new QuitMessage {Connection = connection, Sender = newUser, Message = message, Type = MessageTypes.QUIT};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Quit(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Nickchange(IRCConnection connection, string host, string newnick) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new NickChangeMessage {Connection = connection, Sender = newUser, NewNick = newnick, Type = MessageTypes.NICK};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.NickChange(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void TopicChange(IRCConnection connection, string host, string channel, string message) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new TopicChangeMessage {Connection = connection, Sender = newUser, Content = message, Channel = channel.ToLower(), Type = MessageTypes.TOPIC};

            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Channel, String.Format("- {0} has changed the topic: {1}", newMsg.Sender.Nick, newMsg.Content));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.TopicChange(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Kick(IRCConnection connection, string host, string channel, string kicked, string message) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var recieveUser = new IRCUser {Host = "", Level = UserLevel.Normal, Nick = kicked, RealName = ""};
            var newMsg = new KickMessage {Connection = connection, Sender = newUser, Reciever = recieveUser, Message = message, Channel = channel.ToLower(), Type = MessageTypes.KICK};

            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Channel, String.Format("- {0} has kicked {1} ({2})", newMsg.Sender.Nick, newMsg.Reciever.Nick, newMsg.Message));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Kick(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Modechange(IRCConnection connection, string host, string channel, string modes) {
            IRCUser newUser = Functions.NickFromHost(host).IRCUserFromString(connection);

            var newMsg = new ModeChangeMessage {Connection = connection, Sender = newUser, Modes = Functions.StringtoModeList(modes), Channel = channel.ToLower(), Type = MessageTypes.MODE};
            if (modes.Split(' ').Length > 1) {
                var newuMode = new UserPrivilegiesMessage {Connection = connection, Sender = newUser, Channel = channel.ToLower(), Type = MessageTypes.MODE};
                foreach (UserModeChange mode in Functions.StringtoUserModeList(modes, connection, channel)) {
                    if (mode.Action == ModeStatus.Set) {
                        if (mode.Mode == 'q') {
                            newuMode.Change = LevelChange.Owner;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.Owner(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'a') {
                            newuMode.Change = LevelChange.Admin;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.Admin(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'o') {
                            newuMode.Change = LevelChange.Operator;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.Chanop(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'h') {
                            newuMode.Change = LevelChange.Halfop;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.Halfop(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'v') {
                            newuMode.Change = LevelChange.Voice;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.Voice(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                    }

                    if (mode.Action == ModeStatus.Removed) {
                        if (mode.Mode == 'q') {
                            newuMode.Change = LevelChange.OwnerRevoked;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.DeOwner(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'a') {
                            newuMode.Change = LevelChange.AdminRevoked;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.DeAdmin(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'o') {
                            newuMode.Change = LevelChange.OperatorRevoked;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.DeChanop(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                        if (mode.Mode == 'h') {
                            newuMode.Change = LevelChange.HalfopRevoked;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.DeHalfop(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }

                        }
                        if (mode.Mode == 'v') {
                            newuMode.Change = LevelChange.VoiceRevoked;
                            foreach (PluginInterface plugin in Program.Plugins) {
                                try {
                                    plugin.DeVoice(newuMode);
                                } catch (Exception e) {
                                    Functions.LogError(e);
                                }
                            }
                        }
                    }
                }
            }

            if (newMsg.Sender != null) Functions.LogHistory(connection.ActiveNetwork, newMsg.Channel, String.Format("- {0} has set channel modes: {1}", newMsg.Sender.Nick, modes));
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.ModeChange(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void Connected(IRCConnection connection) {
            var newMsg = new ConnectedMessage {Connection = connection};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.Connected(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }

        internal void IRCMessage(IRCConnection connection, string Message) {
            var newMsg = new IRCMessage {Connection = connection, Message = Message};
            foreach (PluginInterface plugin in Program.Plugins) {
                try {
                    plugin.IRCMessage(newMsg);
                } catch (Exception e) {
                    Functions.LogError(e);
                }
            }
        }
    }
}
