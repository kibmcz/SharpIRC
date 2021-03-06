﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using SharpIRC.API;

namespace SharpIRC {
    /// <summary>
    /// Maintaines a connection to the IRC Servers.
    /// </summary>
    public class Connect {

        /// <summary>
        /// The Uptime Signature
        /// </summary>
        static DateTime dt = new DateTime(1, 1, 1, 0, 0, 0, 0);

        delegate void Time();

        /// <summary>
        /// Is the Core of Uptime, Use the Uptime Plugin to access
        /// </summary>
        public static string Uptime
        {
            get;
            set;
        }

        internal static string HidePassword(string Password)
        {
            char passChar = '*';
            string nPass = string.Empty;

            foreach (char c in Password)
            {
                nPass += passChar;
            }

            return nPass;
        }

        internal static void SendToServer(IRCConnection con, string data)
        {
            foreach (Wildcard wcard1 in con.NetworkConfiguration.Filter.Select(comp => new Wildcard(comp, RegexOptions.IgnoreCase)).Where(wcard1 => wcard1.IsMatch(data)))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Program.Comment("Warning: Intercepted filtered message: " + data);
                Console.ForegroundColor = ConsoleColor.White;
            }
            string pss;
            if (data.StartsWith("PRIVMSG NickServ : IDENTIFY"))
            {
                pss = HidePassword(data.Split(' ')[4]);
                data = data.Replace(data.Split(' ')[4], pss);
                con.writer.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(con.ActiveNetwork + "> " + data);
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                con.writer.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine(con.ActiveNetwork + "> " + data);
                Console.ForegroundColor = ConsoleColor.White;
            }
            con.writer.Flush();
        }

        /// <summary>
        /// Establishes a new connection.
        /// </summary>
        /// <param name="connection">Connection object</param>
        public static void ConnnectToNetwork(IRCConnection connection) {
            string ServerData = "";
            (new Thread(new ThreadStart(delegate
            {
                for (; ; )
                {
                    dt = dt.AddMilliseconds(1d);
                    string days = ((dt.Day - 1) < 1 | (dt.Day - 1) > 1 ? "days" : "day");
                    string hours = (dt.Hour < 1 | dt.Hour > 1 ? "hours" : "hour");
                    string minutes = (dt.Minute < 1 | dt.Minute > 1 ? "minutes" : "minute");
                    string seconds = (dt.Second < 1 | dt.Second > 1 ? "seconds" : "second");
                    Uptime = string.Format("Uptime: {0} {4}, {1} {5}, {2} {6}, {3} {7}\t\t", dt.Day - 1, dt.Hour, dt.Minute, dt.Second, days, hours, minutes, seconds);
                    Console.Title = string.Format("SharpIRC - {0}", Uptime);
                    System.Threading.Thread.Sleep(1);
                }
            }))).Start();
            try {
                Program.Comment("Connecting To " + connection.NetworkConfiguration.Address + ":" + connection.NetworkConfiguration.ServerPort);
                connection.Tcpclient = new TcpClient(connection.NetworkConfiguration.Address, connection.NetworkConfiguration.ServerPort);
                if (connection.Tcpclient.Connected) {
                    if (connection.NetworkConfiguration.SSL) {
                        throw new NotImplementedException("SSL is not yet supported by SharpIRC. please disable this for now.");
                    }
                    else {
                        connection.stream = connection.Tcpclient.GetStream();
                        connection.reader = new StreamReader(connection.stream);
                        connection.writer = new StreamWriter(connection.stream);
                        connection.ActiveServer = connection.NetworkConfiguration.Address;
                        connection.ActiveNetwork = connection.NetworkConfiguration.Address;
                    }
                    while (true) {
                        while (connection.Tcpclient.Connected && (ServerData = connection.reader.ReadLine()) != null) {
                            if (!connection.Connected) {
                                connection.Connected = true;
                                SendToServer(connection, "NICK " + connection.NetworkConfiguration.Nick);
                                SendToServer(connection, String.Format("USER {0} {1} * :{2}", connection.NetworkConfiguration.Ident, "8", connection.NetworkConfiguration.RealName));
                                connection.pingSender = new PingSender(connection);
                                connection.pingSender.Start();
                                if (Program.GlobalSettings.StartupDelay) {
                                    connection.DelayActive = true;
                                    new Thread(delegate() {
                                        Thread.Sleep(15000);
                                        connection.DelayActive = false;
                                    }).Start();
                                }
                            }
                            new Events().IRCMessage(connection, ServerData);
                            string[] smsg = ServerData.Split(' ');
                            string[] ims = JoinString(smsg, 0, true).Split(' ');


                            if (smsg[1] != "PONG") Console.WriteLine(connection.ActiveServer + " => " + ServerData);
                            if (smsg[0] == "PING") SendToServer(connection, "PONG " + ServerData.Split(' ')[1]);
                            switch (smsg[1]) {
                                case "PING":
                                    {
                                        SendToServer(connection, "PONG " + ServerData.Split(' ')[1]);
                                        break;
                                    }

                                case "ERROR": {
                                    string errormessage = JoinString(smsg, 2, true);
                                    PrintError("Connection Failed: " + errormessage);
                                    Reconnect(connection);
                                    break;
                                }
                                case "004":
                                    connection.CurrentNick = smsg[2];
                                    connection.ActiveServer = smsg[3];
                                    connection.ServerSoftware = smsg[4];
                                    break;
                                case "005":
                                    string[] fe = JoinString(smsg, 3, false).Split(' ');
                                    foreach (string word in fe) {
                                        switch (word.Split('=')[0]) {
                                            case "MAXCHANNELS":
                                                connection.MaxChannels = Convert.ToInt32(word.Split('=')[1]);
                                                break;
                                            case "NICKLEN":
                                                connection.NickLength = Convert.ToInt32(word.Split('=')[1]);
                                                break;
                                            case "CHANNELLEN":
                                                connection.ChannelNameLength = Convert.ToInt32(word.Split('=')[1]);
                                                break;
                                            case "TOPICLEN":
                                                connection.TopicLength = Convert.ToInt32(word.Split('=')[1]);
                                                break;
                                            case "PREFIX":
                                                connection.Prefixes = word.Split('=')[1];
                                                break;
                                            case "NETWORK":
                                                connection.ActiveNetwork = word.Split('=')[1];
                                                break;
                                            case "CHANMODES":
                                                connection.SupportedChannelModes = word.Split('=')[1];
                                                break;
                                        }
                                    }
                                    break;
                                case "443":
                                    SendToServer(connection, "NICK " + smsg[3] + "_");
                                    break;
                                case "376":
                                case "422":
                                    if (connection.NetworkConfiguration.IdentifyOnConnect) SendToServer(connection,"NICKSERV IDENTIFY " + connection.NetworkConfiguration.NickServPass);
                                    Commands.SendModes(connection, connection.CurrentNick, "+B");
                                    if (connection.NetworkConfiguration.SetupChannel.Length > 0) Commands.SendJoin(connection, connection.NetworkConfiguration.SetupChannel);
                                    new Events().Connected(connection);
                                    break;

                                case "PRIVMSG":
                                    string message = JoinString(smsg, 3, true);
                                    if (smsg[3].Contains(((char) 1).ToString())) {
                                        if (ims[3] == ":" + ((char) 1) + "ACTION") {
                                            if (ims[2].IsChannel()) new Events().ChanAction(connection, message.Substring(8), ims[0], ims[2]);
                                            else new Events().PrivAction(connection, message.Substring(8), ims[0]);
                                        } else new Events().CTCP(connection, message.Substring(1, message.Length - 1), ims[0], ims[2]);
                                    } else {
                                        if (ims[2].IsChannel()) new Events().ChanMsg(connection, message, ims[0], ims[2]);
                                        else new Events().PrivMsg(connection, message, ims[0]);
                                    }
                                    break;
                                case "NOTICE":
                                    message = JoinString(smsg, 3, true);
                                    if (smsg[3].Contains(((char) 1).ToString())) new Events().CTCPREPLY(connection, message, ims[0]);
                                    else {
                                        if (ims[2].IsChannel()) new Events().ChanNotice(connection, message, ims[0], ims[2]);
                                        else new Events().Notice(connection, message, ims[0]);
                                    }
                                    break;
                                case "JOIN":
                                    new Events().Join(connection, ims[0], ims[2].Substring(1));
                                    if (Functions.NickFromHost(ims[0]) == connection.CurrentNick) SendToServer(connection, "WHO " + ims[2]);
                                    break;
                                case "PART":
                                    message = JoinString(smsg, 3, true);
                                    new Events().Part(connection, ims[0], ims[2], message);
                                    break;
                                case "QUIT":
                                    message = JoinString(smsg, 3, true);
                                    new Events().Quit(connection, ims[0], ims[2].Substring(1) + " " + message);
                                    break;
                                case "TOPIC":
                                    message = JoinString(smsg, 3, true);
                                    new Events().TopicChange(connection, ims[0], ims[2], message);
                                    break;
                                case "KICK":
                                    message = JoinString(smsg, 4, true);
                                    new Events().Kick(connection, ims[0], ims[2], ims[3], message);
                                    break;
                                case "MODE":
                                    message = JoinString(smsg, 3, true);
                                    new Events().Modechange(connection, ims[0], ims[2], message);
                                    SendToServer(connection, "WHO " + ims[2]);
                                    break;
                                case "NICK":
                                    if (Functions.NickFromHost(ims[0]) == connection.CurrentNick) connection.CurrentNick = ims[2].Substring(1);
                                    new Events().Nickchange(connection, ims[0], ims[2].Substring(1));
                                    break;
                                case "352":
                                    Functions.UpdateNickList(connection, smsg[3], JoinString(smsg, 4, false));
                                    break;
                            }
                        }
                    }
                }
            } catch (SocketException ex) {
                PrintError("Connection Failed: " + ex.Message.ToString());
                Reconnect(connection);
            } catch (Exception ex) {
                PrintError(ex.Message.ToString());
                Functions.LogError(ex.InnerException.ToString());
            }
        }

        /// <summary>
        /// Reconnects a broken connection.
        /// </summary>
        /// <param name="connection">Connection object to use.</param>
        public static void Reconnect(IRCConnection connection) {
            Program.Comment(String.Format("Retrying to Connect to: \"{0}\" in 5 seconds..", connection.NetworkConfiguration.Address));
            Thread.Sleep(5000);
            Program.Connections.Add(new IRCConnection {NetworkConfiguration = connection.NetworkConfiguration});
            connection.Terminate();
            new Thread(() => ConnnectToNetwork(connection)).Start();
        }

        /// <summary>
        /// Prints an error on console.
        /// </summary>
        /// <param name="message">Error message to print.</param>
        public static void PrintError(string message) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Joins together a string array back to a string.
        /// </summary>
        /// <param name="strData">The stringarray to stitch together.</param>
        /// <param name="startIndex">The index point at which to start</param>
        /// <param name="removeColon"><see href="RemoveColon" /></param>
        /// <returns></returns>
        public static string JoinString(string[] strData, int startIndex, bool removeColon) {
            if (startIndex > strData.GetUpperBound(0)) return "";

            string tempString = String.Join(" ", strData, startIndex, strData.GetUpperBound(0) + 1 - startIndex);
            if (removeColon) tempString = RemoveColon(tempString);
            return tempString;
        }

        /// <summary>
        /// Removes colon from the start of the message. If none is there the original message is returned.
        /// </summary>
        /// <param name="data">Message to remove the colon from.</param>
        /// <returns>Original message substringed by one.</returns>
        public static string RemoveColon(string data) {
            return data.StartsWith(":") ? data.Substring(1) : data;
        }

        #region Nested type: Wildcard
        /// <summary>
        /// Lets you perform standard wildcard matches without the hassle of regexes. 
        /// </summary>
        public class Wildcard : Regex {
            /// <summary>
            /// Turns a wildcard expression into a regex.
            /// </summary>
            /// <param name="pattern">Wildcard expression.</param>
            public Wildcard(string pattern)
                : base(WildcardToRegex(pattern)) { }

            /// <summary>
            /// Turns a wildcard expression into a regex.
            /// </summary>
            /// <param name="pattern">Wildcard expression.</param>
            /// <param name="options">Regex Options for the wildcard conversion.</param>
            public Wildcard(string pattern, RegexOptions options)
                : base(WildcardToRegex(pattern), options) { }

            /// <summary>
            /// Turns a wildcard expression into a regex.
            /// </summary>
            /// <param name="pattern">Wildcard expression.</param>
            public static string WildcardToRegex(string pattern) {
                return "^" + Escape(pattern).
                    Replace("\\*", ".*").
                    Replace("\\?", ".") + "$";
            }
        }
        #endregion
    }
}
