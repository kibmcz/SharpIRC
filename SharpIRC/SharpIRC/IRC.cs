using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using SharpIRC.API;

namespace SharpIRC {
    /// <summary>
    /// An IRC Connection to a server.
    /// </summary>
    public class IRCConnection {
        /// <summary>
        /// The database containing all users in all channels that the bot is currently in.
        /// </summary>
        public List<NickList> Nicklist = new List<NickList>();

        internal TcpClient Tcpclient = new TcpClient();
        internal StreamWriter writer { get; set; }
        internal NetworkStream stream { get; set; }
        internal StreamReader reader { get; set; }
        internal X509Certificate serverCertificate;
        internal SslStream sslStream;

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
        /// Channel modes supported by this server.
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
    }
}
