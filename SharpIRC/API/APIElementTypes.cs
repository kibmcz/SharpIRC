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

namespace SharpIRC.API {
    /// <summary>
    /// Returns the IRC Protocol type of the message. ACTION CTCP and CTCPREPLY are really subsets of PRIVMSG and NOTICE but have their own type in this API.
    /// </summary>
    public enum MessageTypes {
        /// <summary>
        /// An IRC PRIVMSG message type.
        /// </summary>
        PRIVMSG,

        /// <summary>
        /// An IRC PRIVMSG ACTION message type.
        /// </summary>
        ACTION,

        /// <summary>
        /// An IRC NOTICE message type.
        /// </summary>
        NOTICE,

        /// <summary>
        /// An IRC PRIVMSG CTCP message type.
        /// </summary>
        CTCP,

        /// <summary>
        /// An IRC NOTICE CTCP message type.
        /// </summary>
        CTCPREPLY,

        /// <summary>
        /// An IRC MODE message type.
        /// </summary>
        MODE,

        /// <summary>
        /// An IRC JOIN message type.
        /// </summary>
        JOIN,

        /// <summary>
        /// An IRC PART message type.
        /// </summary>
        PART,

        /// <summary>
        /// An IRC QUIT message type.
        /// </summary>
        QUIT,

        /// <summary>
        /// An IRC NICK message type.
        /// </summary>
        NICK,

        /// <summary>
        /// An IRC TOPIC message type.
        /// </summary>
        TOPIC,

        /// <summary>
        /// An IRC KICK message type.
        /// </summary>
        KICK
    }

    /// <summary>
    /// Required Plugin Information.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)] public class PluginInfoAttribute : Attribute {
        /// <summary>
        /// Plugin information.
        /// </summary>
        /// <param name="name">Plugin name</param>
        /// <param name="description">Plugin description</param>
        /// <param name="author">Plugin author</param>
        /// <param name="version">Plugin version</param>
        public PluginInfoAttribute(string name,string description,string author, string version) { 
            Name = name;
            Description = description;
            Author = author;
            Version = version;
        }

        /// <summary>
        /// The typical name of the plugin. Should be shorter than 5 words, unique and shortly describe what the plugin does.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of the function of the plugin.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The author of the plugin. Format: John Smith (steve@me.com)
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The current version of the plugin.
        /// </summary>
        public string Version { get; set; }
    }

    /// <summary>
    /// Contains all information related to a PRIVMSG or NOTICE from a channel.
    /// </summary>
    public class ChannelMessage {
        /// <summary>
        /// The IRC Connection the message was sent from.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that sent it.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The contents of the message.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The channel it was sent to.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>Contains all information related to a PRIVMSG or NOTICE from a user.
    /// </summary>
    public class PrivateMessage {
        /// A generic private message element.  /// <summary>
        /// The IRC Connection the message was sent from.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that sent it.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The contents of the message.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to an incoming CTCP request from a user.
    /// </summary>
    public class CTCPMessage {
        /// <summary>
        /// The IRC Connection the message was sent from.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that sent it.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The first word of the message. Which is the CTCP Prefix or Command.
        /// </summary>
        public string Prefix { get; internal set; }

        /// <summary>
        /// The last part of the CTCP Message. This value is empty in most CTCP requests.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a new user entering the channel.
    /// </summary>
    public class JoinMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that joined.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The channel that was joined.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a user leaving the channel.
    /// </summary>
    public class PartMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that left the channel.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The message the user left inside the part message.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The channel that was left.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a user disconnecting from the IRC server.
    /// </summary>
    public class QuitMessage {
        /// <summary>
        /// The IRC connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that disconnected from the server.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The message the user left inside the quit mesage.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a user changing their nickname.
    /// </summary>
    public class NickChangeMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that changed their nick. The old nickname is retained.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The new nickname of the user.
        /// </summary>
        public string NewNick { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a user changing the topic of a channel.
    /// </summary>
    public class TopicChangeMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that changed the topic.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The new topic of the channel.
        /// </summary>
        public string Content { get; internal set; }

        /// <summary>
        /// The channel the topic was changed at.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to a user being kicked from the channel.
    /// </summary>
    public class KickMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user that initated the kick.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The user that was kicked.
        /// </summary>
        public IRCUser Reciever { get; internal set; }

        /// <summary>
        /// The kick message.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// The channel the user was kicked from.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Contains all information related to one or more modes being changed in a channel.
    /// </summary>
    public class ModeChangeMessage {
        /// <summary>
        /// A list of modes that were changed and what type of change it was.
        /// </summary>
        public List<ModeChange> Modes = new List<ModeChange>();

        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The user who changed the modes.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The channel the modes were changed in.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// Information about a mode that has been changed.
    /// </summary>
    public class ModeChange {
        /// <summary>
        /// The ASCII character that represents the changed mode.
        /// </summary>
        public char Mode { get; internal set; }

        /// <summary>
        /// <see href="ModeStatus" />
        /// </summary>
        public ModeStatus Action { get; internal set; }
    }

    /// <summary>
    /// Information about a usermode that has been changed.
    /// </summary>
    public class UserModeChange {
        /// <summary>
        /// The ASCII character that represents the changed mode.
        /// </summary>
        public char Mode { get; internal set; }

        /// <summary>
        /// <see href="ModeStatus" />
        /// </summary>
        public ModeStatus Action { get; internal set; }

        /// <summary>
        /// The IRC User that got their usermode changed.
        /// </summary>
        public IRCUser Nick { get; set; }
    }

    /// <summary>
    /// Whether a mode was set or removed.
    /// </summary>
    public enum ModeStatus {
        /// <summary>
        /// Mode is set (+mode)
        /// </summary>
        Set,

        /// <summary>
        /// Mode is revoked (-mode)
        /// </summary>
        Removed
    }

    /// <summary>
    /// Contains all information related to a being changed in a channel.
    /// </summary>
    public class UserPrivilegiesMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// The channel the modes were changed on.
        /// </summary>
        public Channel Channel { get; internal set; }

        /// <summary>
        /// The user that changed them.
        /// </summary>
        public IRCUser Sender { get; internal set; }

        /// <summary>
        /// The user that got their usermode changed.
        /// </summary>
        public IRCUser Reciever { get; internal set; }

        /// <summary>
        /// <see href="LevelChange" />
        /// </summary>
        public LevelChange Change { get; internal set; }

        /// <summary>
        /// <see href="MessageTypes" />
        /// </summary>
        public MessageTypes Type { get; internal set; }
    }

    /// <summary>
    /// The type of change that was made to a usermode.
    /// </summary>
    public enum LevelChange {
        /// <summary>
        /// Has gained owner privilegies.
        /// </summary>
        Owner,

        /// <summary>
        /// Has lost owner privilegies.
        /// </summary>
        OwnerRevoked,

        /// <summary>
        /// Has gained admin privilegies.
        /// </summary>
        Admin,

        /// <summary>
        /// Has lost admin privilegies.
        /// </summary>
        AdminRevoked,

        /// <summary>
        /// Has gained operator privilegies.
        /// </summary>
        Operator,

        /// <summary>
        /// Has lost operator privilegies.
        /// </summary>
        OperatorRevoked,

        /// <summary>
        /// Has gained halfop privilegies.
        /// </summary>
        Halfop,

        /// <summary>
        /// Has lost halfop privilegies.
        /// </summary>
        HalfopRevoked,

        /// <summary>
        /// Has gained voice privilegies.
        /// </summary>
        Voice,

        /// <summary>
        /// Has lost voice privilegies.
        /// </summary>
        VoiceRevoked
    }

    /// <summary>
    /// An event that occurs when the bot has connected to a new server.
    /// </summary>
    public class ConnectedMessage {
        /// <summary>
        /// The server that was connected to.
        /// </summary>
        public IRCConnection Connection { get; internal set; }
    }

    /// <summary>
    /// Triggers on any IRC event.
    /// </summary>
    public class IRCMessage {
        /// <summary>
        /// The server the message was recieved on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }

        /// <summary>
        /// Unformatted IRC Protocol message.
        /// </summary>
        public string Message { get; internal set; }
    }

    /// <summary>
    /// An event that occurs when the bot recieves the You have Been identified message from the Authentication Service.
    /// </summary>
    public class IdentificationCompleteMessage {
        /// <summary>
        /// The IRC Connection the event occured on.
        /// </summary>
        public IRCConnection Connection { get; internal set; }
    }

    /// <summary>
    /// The users current privilegies in the channel.
    /// </summary>
    public enum UserLevel {
        /// <summary>
        /// Is a channel owner.
        /// </summary>
        Owner,

        /// <summary>
        /// Is a channel admin.
        /// </summary>
        Admin,

        /// <summary>
        /// Is a channel operator.
        /// </summary>
        Operator,

        /// <summary>
        /// Is a channel halfop.
        /// </summary>
        Halfop,

        /// <summary>
        /// Is a voiced channel user.
        /// </summary>
        Voice,

        /// <summary>
        /// Is a normal channel user.
        /// </summary>
        Normal
    }

    /// <summary>
    /// A user in an IRC channel.
    /// </summary>
    public class IRCUser {
        /// <summary>
        /// Their hostname in a ident@host format.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// <see href="APIElementTypes.UserLevel" />
        /// </summary>
        public UserLevel Level { get; set; }

        /// <summary>
        /// The current nickname of the user.
        /// </summary>
        public string Nick { get; set; }

        /// <summary>
        /// Their "Real Name" configuration.
        /// </summary>
        public string RealName { get; set; }
    }

    /// <summary>
    /// A collection of users within a channel.
    /// </summary>
    public class Channel {
        private string _channelname;
        /// <summary>
        /// The users in the channel.
        /// </summary>
        public List<IRCUser> Nicks = new List<IRCUser>();

        /// <summary>
        /// The channel containing these users.
        /// </summary>
        public string Name {
            get { return _channelname; }
            set { _channelname = value.ToLower(); }
        }
    }

    /// <summary>
    /// Logged in admin
    /// </summary>
    public class LoggedInAdmin {
        /// <summary>
        /// Admin nickname.
        /// </summary>
        public string Nick { get; set; }
        /// <summary>
        /// The user.
        /// </summary>
        public Admin User { get; set; }
    }
}