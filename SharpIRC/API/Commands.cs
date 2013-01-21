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
using System.Text.RegularExpressions;

namespace SharpIRC.API {
    /// <summary>
    /// A number of commands to provide easy communication with the IRC Server.
    /// </summary>
    public class Commands {
        /// <summary>
        /// Sends a standard message to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendPrivMsg(IRCConnection connection, string reciever, string message) {
            if (Program.Configuration.SplitCommandOutput) {
                var result = new List<string>(Regex.Split(message, @"(?<=\G.{420})"));
                foreach (string msg in result) Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever," " + msg));
            }
            else Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever, message));
        }

        /// <summary>
        /// Sends a standard message to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendPrivMsg(IRCConnection connection, Channel reciever, string message) {
            if (Program.Configuration.SplitCommandOutput) {
                var result = new List<string>(Regex.Split(message, @"(?<=\G.{420})"));
                foreach (string msg in result) Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever.Name, " " + msg));
            } else Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever.Name, message));
        }

        /// <summary>
        /// Sends a standard IRC action (/me) to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendAction(IRCConnection connection, string reciever, string message) { Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}ACTION {2}{1}", reciever, (char)1, message)); }
        
        /// <summary>
        /// Sends a standard IRC action (/me) to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendAction(IRCConnection connection, Channel reciever, string message) { Parser.SendToServer(connection, String.Format("PRIVMSG {0} :{1}ACTION {2}{1}", reciever.Name, (char)1, message)); }

        /// <summary>
        /// Sends a CTCP to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendCTCP(IRCConnection connection, string reciever, string message) { Parser.SendToServer(connection, "PRIVMSG " + reciever + " :" + ((char) 1) + message + ((char) 1)); }

        /// <summary>
        /// Sends a CTCP Reply to a user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendCTCPReply(IRCConnection connection, string reciever, string message) { Parser.SendToServer(connection, "NOTICE " + reciever + " :" + ((char) 1) + message + ((char) 1)); }

        /// <summary>
        /// Sends a notice to a user or channel.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendNotice(IRCConnection connection, string reciever, string message) { Parser.SendToServer(connection, "NOTICE " + reciever + " :" + message); }

        /// <summary>
        /// Joins a new channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection to join on.</param>
        /// <param name="channel">The name of the channel to join.</param>
        public static void SendJoin(IRCConnection connection, string channel) { Parser.SendToServer(connection, "JOIN " + channel); }
        
        /// <summary>
        /// Joins a new channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection to join on.</param>
        /// <param name="channel">The name of the channel to join.</param>
        public static void SendJoin(IRCConnection connection, Channel channel) { Parser.SendToServer(connection, "JOIN " + channel.Name); }

        /// <summary>
        /// Leaves a channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="message">Optional message to display in the Part.</param>
        public static void SendPart(IRCConnection connection, string channel, string message) { Parser.SendToServer(connection, "PART " + channel + " :" + message); }

        /// <summary>
        /// Leaves a channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="message">Optional message to display in the Part.</param>
        public static void SendPart(IRCConnection connection, Channel channel, string message) { Parser.SendToServer(connection, "PART " + channel.Name + " :" + message); }

        /// <summary>
        /// Disconnects from the IRC Network
        /// </summary>
        /// <param name="connection">IRC Connection to disconnect from.</param>
        /// <param name="message">Optional message to display in the Quit.</param>
        public static void SendQuit(IRCConnection connection, string message) { Parser.SendToServer(connection, "QUIT :" + message); }

        /// <summary>
        /// Changes the nickname of the bot on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection to change nick on.</param>
        /// <param name="newnick">The new nick to change to.</param>
        public static void SendNick(IRCConnection connection, string newnick) { Parser.SendToServer(connection, "NICK " + newnick); }

        /// <summary>
        /// Changes the channel topic.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="newtopic">The contents of the new topic.</param>
        public static void SendTopic(IRCConnection connection, string channel, string newtopic) { Parser.SendToServer(connection, "TOPIC " + channel + " :" + newtopic); }

        /// <summary>
        /// Changes the channel topic.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="newtopic">The contents of the new topic.</param>
        public static void SendTopic(IRCConnection connection, Channel channel, string newtopic) { Parser.SendToServer(connection, "TOPIC " + channel.Name + " :" + newtopic); }

        /// <summary>
        /// Change one or more modes in the channel.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to modify.</param>
        /// <param name="message">The modes to change. (Standard +mode-mode format)</param>
        public static void SendModes(IRCConnection connection, string channel, string message) { Parser.SendToServer(connection, "MODE " + channel + " " + message); }

        /// <summary>
        /// Change one or more modes in the channel.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to modify.</param>
        /// <param name="message">The modes to change. (Standard +mode-mode format)</param>
        public static void SendModes(IRCConnection connection, Channel channel, string message) { Parser.SendToServer(connection, "MODE " + channel.Name + " " + message); }

        /// <summary>
        /// Sends a standard IRC Protocol message to the server. Useful to send data the API does not provide an option for.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendIRCMessage(IRCConnection connection, string message) { Parser.SendToServer(connection, message); }
    }
}
