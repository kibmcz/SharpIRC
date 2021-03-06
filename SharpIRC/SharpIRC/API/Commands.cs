﻿using System;
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
            if (Program.GlobalSettings.SplitCommandOutput) {
                var result = new List<string>(Regex.Split(message, @"(?<=\G.{420})"));
                foreach (string msg in result) Connect.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever," " + msg));
            }
            else Connect.SendToServer(connection, String.Format("PRIVMSG {0} :{1}", reciever, message));
        }

        /// <summary>
        /// Sends a standard IRC action (/me) to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendAction(IRCConnection connection, string reciever, string message) { Connect.SendToServer(connection, String.Format("PRIVMSG {0} :{1}ACTION {2}{1}", reciever, (char) 1, message)); }

        /// <summary>
        /// Sends a CTCP to a channel or user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendCTCP(IRCConnection connection, string reciever, string message) { Connect.SendToServer(connection, "PRIVMSG " + reciever + " :" + ((char) 1) + message + ((char) 1)); }

        /// <summary>
        /// Sends a CTCP Reply to a user.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendCTCPReply(IRCConnection connection, string reciever, string message) { Connect.SendToServer(connection, "NOTICE " + reciever + " :" + ((char) 1) + message + ((char) 1)); }

        /// <summary>
        /// Sends a notice to a user or channel.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="reciever">The channel or user to send to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendNotice(IRCConnection connection, string reciever, string message) { Connect.SendToServer(connection, "NOTICE " + reciever + " :" + message); }

        /// <summary>
        /// Joins a new channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection to join on.</param>
        /// <param name="channel">The name of the channel to join.</param>
        public static void SendJoin(IRCConnection connection, string channel) { Connect.SendToServer(connection, "JOIN " + channel); }

        /// <summary>
        /// Leaves a channel on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="message">Optional message to display in the Part.</param>
        public static void SendPart(IRCConnection connection, string channel, string message) { Connect.SendToServer(connection, "PART " + channel + " :" + message); }

        /// <summary>
        /// Disconnects from the IRC Network
        /// </summary>
        /// <param name="connection">IRC Connection to disconnect from.</param>
        /// <param name="message">Optional message to display in the Quit.</param>
        public static void SendQuit(IRCConnection connection, string message) { Connect.SendToServer(connection, "QUIT :" + message); }

        /// <summary>
        /// Changes the nickname of the bot on the IRC Network.
        /// </summary>
        /// <param name="connection">IRC Connection to change nick on.</param>
        /// <param name="newnick">The new nick to change to.</param>
        public static void SendNick(IRCConnection connection, string newnick) { Connect.SendToServer(connection, "NICK " + newnick); }

        /// <summary>
        /// Changes the channel topic.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to leave.</param>
        /// <param name="newtopic">The contents of the new topic.</param>
        public static void SendTopic(IRCConnection connection, string channel, string newtopic) { Connect.SendToServer(connection, "TOPIC " + channel + " :" + newtopic); }

        /// <summary>
        /// Change one or more modes in the channel.
        /// </summary>
        /// <param name="connection">IRC Connection the channel is on.</param>
        /// <param name="channel">The channel to modify.</param>
        /// <param name="message">The modes to change. (Standard +mode-mode format)</param>
        public static void SendModes(IRCConnection connection, string channel, string message) { Connect.SendToServer(connection, "MODE " + channel + " " + message); }

        /// <summary>
        /// Sends a standard IRC Protocol message to the server. Useful to send data the API does not provide an option for.
        /// </summary>
        /// <param name="connection">IRC Connection to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendIRCMessage(IRCConnection connection, string message) { Connect.SendToServer(connection, message); }
    }
}
