using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using PluginArchitecture;

namespace 
{
    public class  : PluginInterface
    {
        #region PluginInterface Members

        public string PluginName()
        {
            return "";
        }

        public string PluginAuthor()
        {
            return "";
        }

        public string PluginPurpose()
        {
            return "";
        }
        #endregion

        #region Events
        public void ChanMsg(string host, string nick, string channel, string message) { }
        public void PrivMsg(string host, string nick, string message) { }
        public void ChanAction(string host, string nick, string channel, string message) { }
        public void PrivAction(string host, string nick, string channel, string message) { }
        public void Notice(string host, string nick, string message) { }
        public void ChanNotice(string host, string nick, string channel, string message) { }
        public void CTCP(string host, string nick, string channel, string message) { }
        public void CTCPREPLY(string host, string nick, string message) { }
        public void Join(string host, string nick, string channel) { }
        public void Part(string host, string nick, string channel, string message) { }
        public void Quit(string host, string nick, string message) { }
        public void NickChange(string host, string nick, string newnick) { }
        public void TopicChange(string host, string nick, string channel, string message) { }
        public void Kick(string host, string nick, string channel, string kicked, string message) { }
        public void ModeChange(string host, string nick, string channel, string message) { }
        public void PluginLoad(PluginInterface Plugin) { }
        public void PluginUnload(PluginInterface Plugin) { }
        public void CustomEvents(string data) { }
        #endregion
    }
}