using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Addins;

namespace SharpIRC.API {
    /// <summary>
    /// The Plugin Interface System for the bot.
    /// </summary>
    [TypeExtensionPoint] public class PluginInterface {
        /// <summary>
        /// The event that occurs when a PRIVMSG is sent to a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.ChannelMessage" /></param>
        public virtual void ChanMsg(ChannelMessage message) { }

        /// <summary>
        /// The event that occurs when a PRIVMSG is sent to the bot.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.PrivateMessage" /></param>
        public virtual void PrivMsg(PrivateMessage message) { }

        /// <summary>
        /// The event that occurs when a PRIVMSG ACTION (/ME) is sent to a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.ChannelMessage" /></param>
        public virtual void ChanAction(ChannelMessage message) { }

        /// <summary>
        /// The event that occurs when a PRIVMSG ACTION (/ME) is sent to the bot.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.PrivateMessage" /></param>
        public virtual void PrivAction(PrivateMessage message) { }

        /// <summary>
        /// The event that occurs when a NOTICE is sent to the bot.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.PrivateMessage" /></param>
        public virtual void Notice(PrivateMessage message) { }

        /// <summary>
        /// The event that occurs when a NOTICE is sent to a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.ChannelMessage" /></param>
        public virtual void ChanNotice(ChannelMessage message) { }

        /// <summary>
        /// The event that occurs when a CTCP Request is sent to the bot.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.CTCPMessage" /></param>
        public virtual void CTCP(CTCPMessage message) { }

        /// <summary>
        /// The event that occurs when a CTCP reply is sent to the bot.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.CTCPMessage" /></param>
        public virtual void CTCPREPLY(CTCPMessage message) { }

        /// <summary>
        /// The event that occurs when someone joins a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.JoinMessage" /></param>
        public virtual void Join(JoinMessage message) { }

        /// <summary>
        /// The event that occurs when someone leaves a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.PartMessage" /></param>
        public virtual void Part(PartMessage message) { }

        /// <summary>
        /// The event that occurs when someone disconnects from a server.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.QuitMessage" /></param>
        public virtual void Quit(QuitMessage message) { }

        /// <summary>
        /// The event that occurs when someone changes their nickname.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.TopicChangeMessage" /></param>
        public virtual void NickChange(NickChangeMessage message) { }

        /// <summary>
        /// The event that occurs when someone changes the topic of a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.NickChangeMessage" /></param>
        public virtual void TopicChange(TopicChangeMessage message) { }

        /// <summary>
        /// The event that occurs when someone kicks another user from the channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.KickMessage" /></param>
        public virtual void Kick(KickMessage message) { }

        /// <summary>
        /// The event that occurs when someone changes one or more modes in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.ModeChangeMessage" /></param>
        public virtual void ModeChange(ModeChangeMessage message) { }

        /// <summary>
        /// The event that occurs when someone gains Owner privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void Owner(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone gains Admin privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void Admin(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone gains Operator privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void Chanop(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone gains Halfop privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void Halfop(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone gains Voice privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void Voice(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone loses Owner privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void DeOwner(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone loses Admin privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void DeAdmin(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone loses Operator privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void DeChanop(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone loses Halfop privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void DeHalfop(UserPrivilegiesMessage message) { }

        /// <summary>
        /// The event that occurs when someone loses Voice privilegies in a channel.
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.UserPrivilegies" /></param>
        public virtual void DeVoice(UserPrivilegiesMessage message) { }

        /// <summary>
        /// <see href="Connected" />
        /// </summary>
        /// <param name="message"><see href="APIElementTypes.ConnectedMessage" /></param>
        public virtual void Connected(ConnectedMessage message) { }

        /// <summary>
        /// Triggers on any IRC event.
        /// </summary>
        /// <param name="message">Unformatted IRC Protocol message</param>
        public virtual void IRCMessage(IRCMessage message) { }

        /// <summary>
        /// Triggers when a configuration file change has been detected.
        /// </summary>
        /// <param name="file">Filename of changed file.</param>
        public virtual void ConfigurationChange(ConfigurationFile file) { }
    }
}
