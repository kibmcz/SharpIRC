﻿/*
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using SharpIRC.API;
using SharpIRC.Properties;

namespace SharpIRC {
    /// <summary>
    /// The Global Configuration class for the bot.
    /// </summary>
    public class Config {
        /// <summary>
        /// List of admins/superusers for the bot.
        /// </summary>
        public List<Admin> Admins = new List<Admin>();

        /// <summary>
        /// List of IRC Networks to connect to.
        /// </summary>
        public List<Network> Networks = new List<Network>();

        /// <summary>
        /// The ASCII number of the prefix character used to identify commands.
        /// </summary>
        public char CommandPrefix { get; set; }

        /// <summary>
        /// Log all non-server related content to file.
        /// </summary>
        public bool LogComments { get; set; }

        /// <summary>
        /// Whether or not to post IRC communication to console.
        /// </summary>
        public bool PostIRCCommunication { get; set; }

        /// <summary>
        /// Log server traffic to file in a chat-history style format. Seperated by user and channel.
        /// </summary>
        public bool ChatHistory { get; set; }

        /// <summary>
        /// Whether to automatically reload configuration files when they have been altered.
        /// </summary>
        public bool AutomaticConfigurationReload { get; set; }

        /// <summary>
        /// If set to true, the 60 minute timer that automatically signs out administrators will be disabled and they will remain logged in until they quit
        /// </summary>
        public bool DisableSessionTimer { get; set; }

        /// <summary>
        /// Enables a 15 second delay after a successful connection has been acheived where the bot will not pass on ChanMsg ChanAction and ChanNotice events to plugins.
        /// </summary>
        public bool StartupDelay { get; set; }

        /// <summary>
        /// Splits and sends channel messages in bulks of 420 characters to avoid command output from being cut off by server.
        /// </summary>
        public bool SplitCommandOutput { get; set; }

        /// <summary>
        /// Enables the built in flood/spam control system that limits commands per 10 seconds. Exceeding this will trigger a temporary ignore.
        /// </summary>
        public bool FloodControl { get; set; }

        /// <summary>
        /// The number of commands any user should be able to request per 10 seconds.
        /// </summary>
        public int UserLimit { get; set; }

        /// <summary>
        /// The number of commands any channel should be able to request per 10 seconds.
        /// </summary>
        public int ChannelLimit { get; set; }

        /// <summary>
        /// The number of seconds an ignore should last when triggered by a channel or user.
        /// </summary>
        public int IgnoreSeconds { get; set; }
    }

    /// <summary>
    /// An IRC Server Configuration
    /// </summary>
    public class Network {
        /// <summary>
        /// A List of phrases to intercept EVERYTHING from sending to this server.
        /// </summary>
        public List<string> Filter = new List<string>();

        [XmlAttribute("Enabled")] public bool Enabled { get; set; }

        /// <summary>
        /// The IRC IDENTD of the bot.
        /// </summary>
        public string Ident { get; set; }

        /// <summary>
        /// The setup channel for the bot to join. If none leave empty.
        /// </summary>
        public string SetupChannel { get; set; }

        /// <summary>
        /// This network's unique identifier. LEAVE IT ALONE.li
        /// </summary>
        [XmlAttribute("ID")] public Guid ID { get; set; }

        /// <summary>
        /// The nick to use to connect to the server.
        /// </summary>
        public string Nick { get; set; }

        private string _authpassword;
        /// <summary>
        /// The password to use to authenticate with NickServ.
        /// </summary>
        public string AuthenticationPassword {
            get {
                var xy = new StackTrace().GetFrame(1).GetMethod().ReflectedType.ToString();
                Program.OutputConsole(xy + Resources.acceessed_your_authentication_password, ConsoleMessageType.Warning);
                return _authpassword;
            }
            set { _authpassword = value; }
        }

        /// <summary>
        /// Whether or not to use IRCv3 SASL authentication.
        /// </summary>
        public bool SASL { get; set; }

        /// <summary>
        /// Account name (Main Nick) to use with SASL authentication.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// The nick of the authentication service used by this network. i.e: NickServ. Irrelevant if SASL is enabled.
        /// </summary>
        public string AuthenticationService { get; set; }

        /// <summary>
        /// Will send a /NICKSERV command on connect regardless of receiving an authentication prompt or not.
        /// </summary>
        public bool IdentifyOnConnect { get; set; }

        /// <summary>
        /// The "Real Name" the bot should use when connecting to this network.
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// The adress/host to connect to, to reach this server.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The port to use to establish a connection with this server.
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Whether or not to use SSL to establish a connection with the server.
        /// </summary>
        public bool SSL { get; set; }

        /// <summary>
        /// Default message when quitting this server.
        /// </summary>
        public string QuitMessage { get; set; }
    }

    /// <summary>
    /// A bot admin configuration.
    /// </summary>
    public class Admin {
        /// <summary>
        /// Admin username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Admin login password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Whether or not this is the current owner of the bot.
        /// </summary>
        [XmlAttribute("Owner")] public bool Owner { get; set; }
    }
}
