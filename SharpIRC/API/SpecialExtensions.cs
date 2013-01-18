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
using System.Text.RegularExpressions;

namespace SharpIRC.API {
    /// <summary>
    /// LINQ Extensions provided by the Fry API to make plugin management easier.
    /// </summary>
    public static class SpecialExtentions {
        /// <summary>
        /// Returns true of the channel message starts with the prefix character set in the configuration file and matches the parameter.
        /// </summary>
        /// <param name="msg">Original message.</param>
        /// <param name="cmd">Command to match with.</param>
        /// <returns></returns>
        public static bool IsCommand(this string msg, string cmd) {
            if (msg.StartsWith(Program.Configuration.CommandPrefix.ToString())) {
                return (String.Equals(msg.Split(' ')[0].Substring(1), cmd, StringComparison.OrdinalIgnoreCase));
            }
            var myMatch = Regex.Match(msg, @"\(.*?\) " + Program.Configuration.CommandPrefix + @"(.*)");
            return myMatch.Success && (String.Equals(myMatch.Groups[1].Value.Split(' ')[0], cmd, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns true of the private message starts with the parameter.
        /// </summary>
        /// <param name="msg">Original message.</param>
        /// <param name="cmd">Command to match with.</param>
        /// <returns></returns>
        public static bool IsPMCommand(this string msg, string cmd) { return (String.Equals(msg.Split(' ')[0], cmd, StringComparison.OrdinalIgnoreCase)) ? true : false; }

        /// <summary>
        /// Searches for open IRC Connections to networks where the name matches the paramater. 
        /// </summary>
        /// <param name="name">IRC Network name.</param>
        /// <returns>An IRC Connection object.</returns>
        public static IRCConnection GetConnectionByName(this string name) {
            return Program.Connections.FirstOrDefault(con => con.ActiveNetwork == name);
        }

        /// <summary>
        /// Searches for open IRC Connections to find a network with the matching unique ID.
        /// </summary>
        /// <param name="ID">Unique ID (GUID)</param>
        /// <returns>An IRC Connection object.</returns>
        public static IRCConnection GetConnectionByGUID(this Guid ID) {
            return Program.Connections.FirstOrDefault(con => con.NetworkConfiguration.ID == ID);
        }

        /// <summary>
        /// Searches for open IRC channels to find a channel with the matching name.
        /// </summary>
        /// <param name="conn">IRC Connection</param>
        /// <param name="mChannel">Channel name</param>
        /// <returns>IRC Channel object.</returns>
        public static Channel GetChannelByName(this IRCConnection conn, string mChannel) {
            var x = conn.Channels.FirstOrDefault(channel => channel.Name == mChannel.ToLower());
            if (x != null) {
                return x;
            }
            x = new Channel { Name = mChannel };
            conn.Channels.Add(x);
            return x;

        }

        /// <summary>
        /// Gets the domain section (the part after @) in a hostmask.
        /// </summary>
        /// <param name="host">Full original hostmask.</param>
        /// <returns>Hostmask domain.</returns>
        public static string ToHostDomain(this string host) { return new Regex("@(?<host>.*?[^ ]+)?").Match("input").Groups["host"].Value; }

        /// <summary>
        /// Returns true if the specified user is currently logged in as admin.
        /// </summary>
        /// <param name="nick">An IRCUser object.</param>
        /// <returns>Whether or not the user is an admin.</returns>
        public static bool IsBotAdmin(this IRCUser nick) { return Program.Sessions.Any(user => user.Nick == nick.Nick); }

        /// <summary>
        /// Returns true if the specified user is currently specified as the bot owner.
        /// </summary>
        /// <param name="nick">An IRCUser object.</param>
        /// <returns></returns>
        public static bool IsBotOwner(this IRCUser nick) {
            return Program.Sessions.Any(user => user.Nick == nick.Nick && user.User.Owner);
        }

        /// <summary>
        /// Searches the nicklist for users in the channel matching the string.
        /// </summary>
        /// <param name="nick">Nickname.</param>
        /// <param name="channel">The channel to search on.</param>
        /// <returns>The IRCUser object in the nicklist.</returns>
        public static IRCUser IRCUserFromString(this string nick, Channel channel) {
            return channel.Nicks.FirstOrDefault(x => x.Nick == nick);
        }

        /// <summary>
        /// Searches the nicklist for users in the network matching the string.
        /// </summary>
        /// <param name="nick">Nickname.</param>
        /// <param name="connection">IRC connection to search on.</param>
        /// <returns>First result or null.</returns>
        public static IRCUser IRCUserFromString(this string nick, IRCConnection connection) {
            return connection.Channels.SelectMany(x => x.Nicks).FirstOrDefault(y => y.Nick == nick);
        }

        /// <summary>
        /// returns whether or not the second word in a channel message matches the parameter. If the message does not start with the command char, it will return null.
        /// </summary>
        /// <param name="msg">The original message.</param>
        /// <param name="cmd">Sub command to check against.</param>
        /// <returns>True if it is a positive match.</returns>
        public static bool IsSubCommand(this string msg, string cmd) {
            Match myMatch = Regex.Match(msg, @"\(.*?\) " + Program.Configuration.CommandPrefix + @"(.*)");
            return myMatch.Success ? (String.Equals(msg.Split(' ')[2], cmd, StringComparison.OrdinalIgnoreCase)) : (String.Equals(msg.Split(' ')[1], cmd, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns the original message without the command. 
        /// </summary>
        /// <param name="msg">The original message.</param>
        /// <returns>Message without command.</returns>
        public static string GetMessageWithoutCommand(this string msg) {
            return (Connect.JoinString(msg.Split(' '), 1, false).Length > 0) ? Connect.JoinString(msg.Split(' '), 1, false) : null;
        }

        /// <summary>
        /// Returns the original message without the command and subcommand. If it is not a valid command it will return the origianl message.
        /// </summary>
        /// <param name="msg">The original message.</param>
        /// <returns>Message without command and subcommand.</returns>
        public static string GetMessageWithoutSubCommand(this string msg) { 
            return Connect.JoinString(msg.Split(' '), 2, false).Length > 0 ? Connect.JoinString(msg.Split(' '), 2, false) : null;
        }

        /// <summary>
        /// Appropriately cuts off messages that are too long to send to IRC.
        /// 
        /// </summary>
        /// <param name="sent">The original message.</param>
        /// <param name="limit">Limit in number of characters.</param>
        /// <returns>Message cut down to the given limit with 3 periods to indicate its cut off.</returns>
        public static string BreakToLastWord(this string sent, int limit) {
            if (sent.Length > limit)
            {
                string strSubstringed = sent.Substring(0, limit);
                string[] splitedString = strSubstringed.Split(' ');
                splitedString[splitedString.Length - 1] = "";
                return String.Join(" ", splitedString);
            }
            return sent;
        }

        /// <summary>
        /// Whether or not the user is owner (~) in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is owner.</returns>
        public static bool IsOwner(this IRCUser user) {
            switch (user.Level) {
                case UserLevel.IRCOP:
                    return true;
                case UserLevel.Owner:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user is owner (~) in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is owner.</returns>
        public static bool IsOwner(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
                    case UserLevel.IRCOP:
                        return true;
                    case UserLevel.Owner:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether or not the user has admin or higher privilegies in the channel.
        /// </summary>just 
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is admin or above</returns>
        public static bool IsAdmin(this IRCUser user) {
            if (user == null) return false;
            switch (user.Level) {
                case UserLevel.IRCOP:
                    return true;
                case UserLevel.Owner:
                    return true;
                case UserLevel.Admin:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user has admin or higher privileges in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsAdmin(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
                    case UserLevel.IRCOP:
                        return true;
                    case UserLevel.Owner:
                        return true;
                    case UserLevel.Admin:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether or not the user has operator (@) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is operator or above</returns>
        public static bool IsOperator(this IRCUser user) {
            if (user == null) return false;
            switch (user.Level) {
                case UserLevel.IRCOP:
                    return true;
                case UserLevel.Owner:
                    return true;
                case UserLevel.Admin:
                    return true;
                case UserLevel.Operator:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user has operator (@) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsOperator(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
                    case UserLevel.IRCOP:
                        return true;
                    case UserLevel.Owner:
                        return true;
                    case UserLevel.Admin:
                        return true;
                    case UserLevel.Operator:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether or not the user has halfop (%) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is halfop or above</returns>
        public static bool IsHalfop(this IRCUser user) {
            if (user == null) return false;
            switch (user.Level) {
                case UserLevel.IRCOP:
                    return true;
                case UserLevel.Owner:
                    return true;
                case UserLevel.Admin:
                    return true;
                case UserLevel.Operator:
                    return true;
                case UserLevel.Halfop:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user has halfop (%) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsHalfop(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
                    case UserLevel.IRCOP:
                        return true;
                    case UserLevel.Owner:
                        return true;
                    case UserLevel.Admin:
                        return true;
                    case UserLevel.Operator:
                        return true;
                    case UserLevel.Halfop:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether or not the user has voice (+) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsVoice(this IRCUser user) {
            if (user == null) return false;
            switch (user.Level) {
                case UserLevel.IRCOP:
                    return true;
                case UserLevel.Owner:
                    return true;
                case UserLevel.Admin:
                    return true;
                case UserLevel.Operator:
                    return true;
                case UserLevel.Halfop:
                    return true;
                case UserLevel.Voice:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user has voice (+) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsVoice(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
                    case UserLevel.IRCOP:
                        return true;
                    case UserLevel.Owner:
                        return true;
                    case UserLevel.Admin:
                        return true;
                    case UserLevel.Operator:
                        return true;
                    case UserLevel.Halfop:
                        return true;
                    case UserLevel.Voice:
                        return true;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether or not this host is in the ignore list of the bot.
        /// </summary>
        /// <param name="host">The full host (nick!ident@domain)</param>
        /// <returns>True if host is ignored</returns>
        public static bool IsIgnored(this string host) {
            if (host.Contains("@")) host = host.Split('@')[1];
            var exists = false;
            foreach (var user in Program.IgnoreList.Ignores.Where(user => user.Host == host)) exists = true;
            return exists;
        }

        /// <summary>
        /// Whether or not the input is a standard IRC channel.
        /// </summary>
        /// <param name="s">Input WORD (no spaces) to validate.</param>
        /// <returns>Whether or not the input is a valid IRC protocol irc channel.</returns>
        public static bool IsChannel(this string s) {
            return Regex.Match(s, @"([#&][^\x07\x2C\s]{0,200})").Success;
        }

        /// <summary>
        /// Deletes all the contents of a directory
        /// </summary>
        /// <param name="directory">Directory</param>
        public static void Empty(this DirectoryInfo directory) {
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        
    }

    /// <summary>
    /// IRC Color representation.
    /// </summary>
    public static class IRCColor {
        /// <summary>
        /// White
        /// </summary>
        /// <returns>Color code for white</returns>
        public static String White() { return (char)3 + "0"; }
        /// <summary>
        /// Black
        /// </summary>
        /// <returns>Color code for black</returns>
        public static String Black() { return (char)3 + "1"; }
        /// <summary>
        /// Dark Blue
        /// </summary>
        /// <returns>Color code for dark blue</returns>
        public static String DarkBlue() { return (char)3 + "2"; }
        /// <summary>
        /// Green
        /// </summary>
        /// <returns>Color code for green</returns>
        public static String Green() { return (char)3 + "3"; }
        /// <summary>
        /// Red
        /// </summary>
        /// <returns>Color code for red</returns>
        public static String Red() { return (char)3 + "4"; }
        /// <summary>
        /// Maroon
        /// </summary>
        /// <returns>Color code for maroon</returns>
        public static String Maroon() { return (char)3 + "5"; }
        /// <summary>
        /// Purple
        /// </summary>
        /// <returns>Color code for purple/violet</returns>
        public static String Purple() { return (char)3 + "6"; }
        /// <summary>
        /// Orange
        /// </summary>
        /// <returns>Color code for orange</returns>
        public static String Orange() { return (char)3 + "7"; }
        /// <summary>
        /// Yellow
        /// </summary>
        /// <returns>Color code for yellow</returns>
        public static String Yellow() { return (char)3 + "8"; }
        /// <summary>
        /// Lime Green
        /// </summary>
        /// <returns>Color code for lime green</returns>
        public static String LimeGreen() { return (char)3 + "9"; }
        /// <summary>
        /// Teal
        /// </summary>
        /// <returns>Color code for teal</returns>
        public static String Teal() { return (char)3 + "10"; }
        /// <summary>
        /// Cyan
        /// </summary>
        /// <returns>Color code for cyan</returns>
        public static String Cyan() { return (char)3 + "11"; }
        /// <summary>
        /// Blue
        /// </summary>
        /// <returns>Color code for blue</returns>
        public static String Blue() { return (char)3 + "12"; }
        /// <summary>
        /// Pink
        /// </summary>
        /// <returns>Color code for pink</returns>
        public static String Pink() { return (char)3 + "13"; }
        /// <summary>
        /// Gray
        /// </summary>
        /// <returns>Color code for gray/dark gray</returns>
        public static String Gray() { return (char)3 + "14"; }
        /// <summary>
        /// Light Gray
        /// </summary>
        /// <returns>Color code for light gray</returns>
        public static String LightGray() { return (char)3 + "15"; }
    }
    /// <summary>
    /// IRC Formatting Style characters.
    /// </summary>
    public static class IRCStyle
    {
        /// <summary>
        /// Returns a character that will instruct the server to bold the following text.
        /// </summary>
        public static char Bold() { return (char)2; }
        /// <summary>
        /// Returns a character that will instruct the server to remove all previous formatting settings in the following text.
        /// </summary>
        public static char Normal() { return (char)15; }
        /// <summary>
        /// Returns a character that will instruct the server to reverse the formatting settings in the following text.
        /// </summary>
        public static char Reversed() { return (char)22; }
        /// <summary>
        /// Returns a character that will instruct the server to underline the following text.
        /// </summary>
        public static char Underline() { return (char)31; }
    }


}
