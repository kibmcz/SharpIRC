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
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using SharpIRC.API;
using SharpIRC.Properties;

namespace SharpIRC {

    /// <summary>
    /// Maintains a connection to the IRC Servers.
    /// </summary>
    public class Parser {

        /// <summary>
        /// The Uptime Signature
        /// </summary>
        public static DateTime startTime = DateTime.Now;

        internal static string HidePassword(string password) {
            const char passChar = '*';
            return password.Aggregate(string.Empty, (current, c) => current + passChar);
        }

        internal static void SendToServer(IRCConnection con, string data) {
            if (con.Configuration.Filter.Select(comp => new Wildcard(comp, RegexOptions.IgnoreCase)).Any(wcard1 => wcard1.IsMatch(data))) {
                Program.OutputConsole("Warning: Intercepted filtered message: " + data, ConsoleMessageType.Warning);
                return;
            }
            if (!Program.Configuration.PostIRCCommunication) return;
            string pss;
            if (data.StartsWith("PRIVMSG NickServ : IDENTIFY")) {
                pss = HidePassword(data.Split(' ')[4]);
                var _data = data.Replace(data.Split(' ')[4], pss);
                con.writer.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(con.ActiveNetwork + Resources.Connect_SendToServer___ + _data);
                Console.ForegroundColor = ConsoleColor.White;

            } else if (data.StartsWith("AUTHENTICATE") && !data.Contains("PLAIN")) {
                pss = HidePassword(data.Split(' ')[1]);
                var _data = data.Replace(data.Split(' ')[1], pss);
                con.writer.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(con.ActiveNetwork + Resources.Connect_SendToServer___ + _data);
                Console.ForegroundColor = ConsoleColor.White;
            } else {
                con.writer.WriteLine(data);
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(con.ActiveNetwork + Resources.Connect_SendToServer___ + data);
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
            (new Thread(new ThreadStart(delegate { }))).Start();
            try {
                Program.OutputConsole("Connecting To " + connection.Configuration.Address + ":" + connection.Configuration.ServerPort, ConsoleMessageType.Normal);
                connection.Tcpclient = new TcpClient(connection.Configuration.Address, connection.Configuration.ServerPort);
                if (connection.Tcpclient.Connected) {
                    if (connection.Configuration.SSL) {
                        connection.sslStream = new SslStream(connection.Tcpclient.GetStream(), false, ValidateServerCertificate, null);
                        connection.sslStream.AuthenticateAsClient(connection.Configuration.Address);
                        connection.reader = new StreamReader(connection.sslStream);
                        connection.writer = new StreamWriter(connection.sslStream);
                    } else {
                        connection.stream = connection.Tcpclient.GetStream();
                        connection.reader = new StreamReader(connection.stream);
                        connection.writer = new StreamWriter(connection.stream);
                    }
                    connection.ActiveServer = connection.Configuration.Address;
                    connection.ActiveNetwork = connection.Configuration.Address;
                    while (true) {
                        while (connection.Tcpclient.Connected && (ServerData = connection.reader.ReadLine()) != null) {
                            if (!connection.Connected) {
                                connection.Connected = true;
                                if (connection.Configuration.SASL) SendToServer(connection, "CAP REQ :sasl");
                                SendToServer(connection, "NICK " + connection.Configuration.Nick);
                                SendToServer(connection, String.Format("USER {0} 8 1 :{1}", connection.Configuration.Ident, connection.Configuration.RealName));
                                connection.pingSender = new PingSender(connection);
                                connection.pingSender.Start();
                                if (Program.Configuration.StartupDelay) {
                                    connection.DelayActive = true;
                                    new Thread(delegate() {
                                        Thread.Sleep(15000);
                                        connection.DelayActive = false;
                                    }).Start();
                                }
                                connection.loginTimeout.Elapsed += connection.timeout_Elapsed;
                                connection.loginTimeout.Start();

                            }
                            new Events().IRCMessage(connection, ServerData);
                            if (fData(ServerData, 1, 1) != "PONG" && Program.Configuration.PostIRCCommunication) Console.WriteLine(connection.ActiveServer + Resources.toNetwork + ServerData);
                            if (fData(ServerData, 0, 0) == "PING") SendToServer(connection, "PONG " + ServerData.Split(' ')[1]);
                            if (ServerData == "AUTHENTICATE +") SendToServer(connection, "AUTHENTICATE " + Functions.Base64Encode(String.Format("{0}\0{1}\0{2}", connection.Configuration.Nick, connection.Configuration.AccountName, connection.Configuration.AuthenticationPassword)));
                            switch (fData(ServerData, 1, 1)) {
                                case "PING": {
                                        SendToServer(connection, "PONG " + ServerData.Split(' ')[1]);
                                        break;
                                    }

                                case "ERROR": {
                                        Program.OutputConsole("Connection Failed: " + fData(ServerData, 2), ConsoleMessageType.Error);
                                        Reconnect(connection);
                                        break;
                                    }
                                case "004":
                                    connection.CurrentNick = fData(ServerData, 2, 2);
                                    connection.ActiveServer = fData(ServerData, 3, 3);
                                    connection.ServerSoftware = fData(ServerData, 4, 4);
                                    break;
                                case "005":
                                    foreach (var word in fData(ServerData, 3).Split(' ')) {
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
                                                Functions.AssertUModes(connection, connection.Prefixes);
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
                                    SendToServer(connection, "NICK " + fData(ServerData, 3) + "_");
                                    break;
                                case "CAP":
                                    if (fData(ServerData, 3) == "ACK") SendToServer(connection, "AUTHENTICATE PLAIN");
                                    break;
                                case "903":
                                    new Events().Authorized(connection);
                                    SendToServer(connection, "CAP END");
                                    connection.loginTimeout.Stop();
                                    break;
                                case "904":
                                case "905":
                                    Program.OutputConsole(connection.ActiveNetwork + " : SASL AUTHENTICATION FAILED", ConsoleMessageType.Error);
                                    SendToServer(connection, "CAP END");
                                    break;
                                case "376":
                                case "422":
                                    if (connection.Configuration.IdentifyOnConnect && !connection.Configuration.SASL) SendToServer(connection, "NICKSERV IDENTIFY " + connection.Configuration.AuthenticationPassword);
                                    Commands.SendModes(connection, connection.CurrentNick, "+B");
                                    if (connection.Configuration.SetupChannel.Length > 0) Commands.SendJoin(connection, connection.Configuration.SetupChannel);
                                    new Events().Connected(connection);
                                    break;

                                case "PRIVMSG":
                                    if (fData(ServerData, 3, 3).isCTCP()) {
                                        if (fData(ServerData, 3, 3).isCTCP("ACTION")) {
                                            if (fData(ServerData, 2, 2).IsChannel()) new Events().ChanAction(connection, fData(ServerData, 3).Substring(8), fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                            else new Events().PrivAction(connection,fData(ServerData, 3).Substring(8), fData(ServerData, 0, 0));
                                        } else new Events().CTCP(connection, fData(ServerData, 3).Substring(1,fData(ServerData, 3).Length - 1), fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                    } else {
                                        if (fData(ServerData, 2, 2).IsChannel()) new Events().ChanMsg(connection, fData(ServerData, 3), fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                        else new Events().PrivMsg(connection, fData(ServerData, 3), fData(ServerData, 0, 0));
                                    }
                                    break;
                                case "NOTICE":
                                    if (fData(ServerData, 3).isCTCP()) new Events().CTCPREPLY(connection, fData(ServerData, 3), fData(ServerData, 0, 0));
                                    else {
                                        if (fData(ServerData, 2, 2).IsChannel()) new Events().ChanNotice(connection, fData(ServerData, 3), fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                        else new Events().Notice(connection, fData(ServerData, 3), fData(ServerData, 0, 0));
                                    }
                                    break;
                                case "JOIN":
                                    new Events().Join(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2).Substring(1));
                                    if (Functions.NickFromHost(fData(ServerData, 0, 0)) == connection.CurrentNick) SendToServer(connection, "WHO " + fData(ServerData, 2, 2));
                                    break;
                                case "PART":
                                    new Events().Part(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2), fData(ServerData, 3));
                                    break;
                                case "QUIT":
                                    new Events().Quit(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2) + " " + fData(ServerData, 3));
                                    break;
                                case "TOPIC":
                                    new Events().TopicChange(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2), fData(ServerData, 3));
                                    break;
                                case "KICK":
                                    new Events().Kick(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2), fData(ServerData, 3, 3), fData(ServerData, 4));
                                    break;
                                case "MODE":
                                    new Events().Modechange(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2), fData(ServerData, 3));
                                    SendToServer(connection, "WHO " + fData(ServerData, 2, 2));
                                    break;
                                case "NICK":
                                    if (Functions.NickFromHost(fData(ServerData, 0, 0)) == connection.CurrentNick) connection.CurrentNick = fData(ServerData, 2);
                                    new Events().Nickchange(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                    Functions.UpdateNickChange(connection, fData(ServerData, 0, 0), fData(ServerData, 2, 2));
                                    break;
                                case "352":
                                    Functions.UpdateNickList(connection, fData(ServerData, 3, 3), fData(ServerData, 4));
                                    break;
                            }
                        }
                    }
                }
                Program.OutputConsole("Unable to establish a connection to " + connection.Configuration.Address + " on port " + connection.Configuration.ServerPort, ConsoleMessageType.Error);
                Reconnect(connection);
            }
            catch (SocketException ex) {
                Program.OutputConsole("Connection Failed: " + ex.Message, ConsoleMessageType.Error);
                Reconnect(connection);
            }
            catch (Exception ex) {
                Program.OutputConsole(ex.Source + ": " + ex.Message, ConsoleMessageType.Error);
                Functions.LogError(ex.GetBaseException().ToString());
            }
        }

        /// <summary>
        /// Reconnects a broken connection.
        /// </summary>
        /// <param name="connection">Connection object to use.</param>
        public static void Reconnect(IRCConnection connection) {
            Program.OutputConsole(String.Format("Retrying to Connect to: \"{0}\" in 5 seconds..", connection.Configuration.Address), ConsoleMessageType.Normal);
            Thread.Sleep(5000);
            Program.Connections.Add(new IRCConnection { Configuration = connection.Configuration });
            connection.Terminate();
            new Thread(() => ConnnectToNetwork(connection)).Start();
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

        public static string fData(string data, int startIndex, int endIndex) {
            var strData = data.Split(' ');
            if (startIndex == endIndex) return RemoveColon(strData[startIndex]);
            if (startIndex > strData.GetUpperBound(0)) return "";
            var builder = new StringBuilder();
            for (var i = startIndex; i <= endIndex; i++) {
                builder.Append(strData[i]);
            }
            return RemoveColon(builder.ToString());
;        }
        public static string fData(string data, int startIndex) {
            var strData = data.Split(' ');
            if (startIndex > strData.GetUpperBound(0)) return "";

            var tempString = String.Join(" ", strData, startIndex, strData.GetUpperBound(0) + 1 - startIndex);
            return RemoveColon(tempString);
        }

        /// <summary>
        /// Removes colon from the start of the message. If none is there the original message is returned.
        /// </summary>
        /// <param name="data">Message to remove the colon from.</param>
        /// <returns>Original message substringed by one.</returns>
        public static string RemoveColon(string data) {
            return data.StartsWith(":") ? data.Substring(1) : data;
        }

        /// <summary>
        /// Validates the SSL certificate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors) {
            if (sslPolicyErrors == SslPolicyErrors.None) {
                Program.OutputConsole(String.Format("SSL Certificate approved.\r\n" +
                                                    "Name: {0}\r\n" +
                                                    "Issued by: {1}\r\n" +
                                                    "Expires: {2}\r\n", certificate.Subject, certificate.Issuer, certificate.GetExpirationDateString()), ConsoleMessageType.Information);
                return true;
            }

            Program.OutputConsole("SSL Certificate Error: " + sslPolicyErrors, ConsoleMessageType.Error);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
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
