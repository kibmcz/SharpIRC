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
            if (msg.StartsWith(Program.GlobalSettings.CommandPrefix.ToString())) {
                return (String.Equals(msg.Split(' ')[0].Substring(1), cmd, StringComparison.OrdinalIgnoreCase));
            }
            var myMatch = Regex.Match(msg, @"\(.*?\) " + Program.GlobalSettings.CommandPrefix + @"(.*)");
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
            return conn.Channels.FirstOrDefault(channel => channel.Name == mChannel);
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
        public static bool IsBotAdmin(this IRCUser nick) { return Program.LoggedIn.Any(user => user.Username == nick.Nick); }

        /// <summary>
        /// Returns true if the specified user is currently specified as the bot owner.
        /// </summary>
        /// <param name="nick">An IRCUser object.</param>
        /// <returns></returns>
        public static bool IsBotOwner(this IRCUser nick) {
            return Program.LoggedIn.Any(user => user.Username == nick.Nick && user.Owner);
        }

        /// <summary>
        /// Searches the nicklist for users on the network matching the string.
        /// </summary>
        /// <param name="nick">Nickname.</param>
        /// <param name="connection">The IRCConnection to search on.</param>
        /// <returns>The IRCUser object in the nicklist.</returns>
        public static IRCUser IRCUserFromString(this string nick, IRCConnection connection) { 
            IRCUser getUser = null;
            foreach (IRCUser user in from list in connection.Channels from user in list.Nicks where user.Nick == nick select user) {
                getUser = user;
            }
            return getUser;
        }

        /// <summary>
        /// returns whether or not the second word in a channel message matches the parameter. If the message does not start with the command char, it will return null.
        /// </summary>
        /// <param name="msg">The original message.</param>
        /// <param name="cmd">Sub command to check against.</param>
        /// <returns>True if it is a positive match.</returns>
        public static bool IsSubCommand(this string msg, string cmd) {
            Match myMatch = Regex.Match(msg, @"\(.*?\) " + Program.GlobalSettings.CommandPrefix + @"(.*)");
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
            if (user == null) return false;
            return user.Level == UserLevel.Owner;
        }

        /// <summary>
        /// Whether or not the user is owner (~) in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is owner.</returns>
        public static bool IsOwner(this IRCUser user, Channel channel) {
            return channel.Nicks.Any(mUser => mUser.Nick == user.Nick && mUser.IsOwner());
        }

        /// <summary>
        /// Whether or not the user has admin or higher privilegies in the channel.
        /// </summary>just 
        /// <param name="user">The user to check.</param>
        /// <returns>Returns true if the user is admin or above</returns>
        public static bool IsAdmin(this IRCUser user) {
            if (user == null) return false;
            switch (user.Level) {
                case UserLevel.Owner:
                    return true;
                case UserLevel.Admin:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the user has admin (&) or higher privilegies in the channel.
        /// </summary>
        /// <param name="user">The user to check.</param>
        /// <param name="channel">The channel to check</param>
        /// <returns>Returns true if the user is voice or above</returns>
        public static bool IsAdmin(this IRCUser user, Channel channel) {
            foreach (var mUser in channel.Nicks.Where(mUser => mUser.Nick == user.Nick)) {
                switch (mUser.Level) {
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
        /// Whether or not the input is a standard #& IRC channel.
        /// </summary>
        /// <param name="s">Input WORD (no spaces) to validate.</param>
        /// <returns>Whether or not the input is a valid IRC protocol irc channel.</returns>
        public static bool IsChannel(this string s) {
            return Regex.Match(s, @"([#&][^\x07\x2C\s]{0,200})").Success;
        }


        public static void Empty(this DirectoryInfo directory) {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class IRCColor {
        public static String White() { return (char)3 + "0"; }
        public static String Black() { return (char)3 + "1"; }
        public static String DarkBlue() { return (char)3 + "2"; }
        public static String Green() { return (char)3 + "3"; }
        public static String Red() { return (char)3 + "4"; }
        public static String Maroon() { return (char)3 + "5"; }
        public static String Purple() { return (char)3 + "6"; }
        public static String Orange() { return (char)3 + "7"; }
        public static String Yellow() { return (char)3 + "8"; }
        public static String LimeGreen() { return (char)3 + "9"; }
        public static String Teal() { return (char)3 + "10"; }
        public static String Cyan() { return (char)3 + "11"; }
        public static String Blue() { return (char)3 + "12"; }
        public static String Pink() { return (char)3 + "13"; }
        public static String Gray() { return (char)3 + "14"; }
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
