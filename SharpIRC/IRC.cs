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
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using SharpIRC.API;

namespace SharpIRC {
    /// <summary>
    /// An IRC Connection to a server.
    /// </summary>
    public class IRCConnection {
        /// <summary>
        /// The database containing all users in all channels that the bot is currently in.
        /// </summary>
        public List<Channel> Channels = new List<Channel>();

        internal TcpClient Tcpclient = new TcpClient();
        internal StreamWriter writer { get; set; }
        internal NetworkStream stream { get; set; }
        internal StreamReader reader { get; set; }
        internal X509Certificate serverCertificate;
        internal SslStream sslStream;
        /// <summary>
        /// Login timeout timer.
        /// </summary>
        public Timer loginTimeout = new Timer(30000);

        /// <summary>
        /// The address of the IRC Server the bot is currently connected to.
        /// </summary>
        public string ActiveServer { get; set; }

        /// <summary>
        /// The name of the IRC Network the bot is currently connected to.
        /// </summary>
        public string ActiveNetwork { get; set; }

        /// <summary>
        /// Software information about the IRC Server the bot is currently connected to.
        /// </summary>
        public string ServerSoftware { get; set; }

        /// <summary>
        /// The maximum nick length for this server.
        /// </summary>
        public int NickLength { get; set; }

        /// <summary>
        /// The maximum topic length for this server.
        /// </summary>
        public int TopicLength { get; set; }

        /// <summary>
        /// The maximum channel name length for this server.
        /// </summary>
        public int ChannelNameLength { get; set; }

        /// <summary>
        /// The allowed prefixes on this server.
        /// </summary>
        public string Prefixes { get; set; }

        /// <summary>
        /// The defined IRCD usermodes.
        /// </summary>
        public IRCDUMode Umodes { get; set; }

        /// <summary>
        /// Name modes supported by this server.
        /// </summary>
        public string SupportedChannelModes { get; set; }

        /// <summary>
        /// The maximum amount of channels a user can be in at one time on this server.
        /// </summary>
        public int MaxChannels { get; set; }

        /// <summary>
        /// The current nick of the IRC Bot.
        /// </summary>
        public string CurrentNick { get; set; }

        /// <summary>
        /// Whether or not startup delay is currently active on this connection
        /// </summary>
        public bool DelayActive { get; set; }

        /// <summary>
        /// Whether or not the bot is currently connected.
        /// </summary>
        public bool Connected { get; set; }

        internal PingSender pingSender { get; set; }

        /// <summary>
        /// The network configuration information related to this connection.
        /// </summary>
        public Network NetworkConfiguration { get; set; }

        /// <summary>
        /// Immedantly terminate the connection to the server.
        /// </summary>
        public void Terminate() {
            Tcpclient.Close();
            Connected = false;
        }

        internal void timeout_Elapsed(object sender, ElapsedEventArgs e) {
            new Events().LoginTimedOut(this);
        }
    }
}
