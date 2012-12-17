using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension] public class Topic : PluginInterface {
        public static TopicList Topics = ConfigurationAPI.LoadConfigurationFile<TopicList>("Topic");

        public static String UTF8ByteArrayToString(Byte[] characters) {
            var encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        public static Byte[] StringToUTF8ByteArray(String pXmlString) {
            var encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        public override void ConfigurationChange(ConfigurationFile file) { if (file.Name == "Topic") Topics = ConfigurationAPI.LoadConfigurationFile<TopicList>("Topic"); }

        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("topic") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 0, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("static") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 1, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("owner") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 2, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("status") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 3, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("divider") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 4, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("decoration") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 5, message.Message.GetMessageWithoutCommand());
            if (message.Message.IsCommand("prefix") && message.Sender.IsHalfop()) SetTopic(message.Connection, message.Channel, 6, message.Message.GetMessageWithoutCommand());
        }

        public static void SetTopic(IRCConnection con, string channel, int tpart, string msg) {
            TopicNetwork oldnet = (from net in Topics.Networks where net.ID == con.NetworkConfiguration.ID.ToString() select net).FirstOrDefault();
            if (oldnet == null) {
                var addnet = new TopicNetwork {ID = con.NetworkConfiguration.ID.ToString()};
                oldnet = addnet;
            }
            TopicData olddata = (from chan in oldnet.Channels where chan.Name == channel select chan).FirstOrDefault();
            if (olddata == null) {
                var addchan = new TopicData {Name = channel};
                olddata = addchan;
            }
            if (tpart == 0) olddata.Topic = msg;
            if (tpart == 1) olddata.Static = msg;
            if (tpart == 2) olddata.Owner = msg;
            if (tpart == 3) olddata.Status = msg;
            if (tpart == 4) olddata.Divider = msg;
            if (tpart == 5) olddata.Decoration = msg;
            if (tpart == 6) olddata.Prefix = msg;
            oldnet.Channels.RemoveAll(x => x.Name == channel);
            oldnet.Channels.Add(olddata);
            Topics.Networks.RemoveAll(x => x.ID == con.NetworkConfiguration.ID.ToString());
            Topics.Networks.Add(oldnet);
            ConfigurationAPI.SaveConfigurationFile(Topics, "Topic");
            Commands.SendTopic(con, channel, String.Format("{0} {1} {2} {3} {4} {5} {3} {6} {0}",
                    olddata.Decoration,
                    olddata.Prefix,
                    olddata.Topic,
                    olddata.Divider,
                    olddata.Owner,
                    olddata.Status,
                    olddata.Static));
        }
    }

    public class TopicData {
        [XmlAttribute("Name")] public string Name { get; set; }
        public string Decoration { get; set; }
        public string Topic { get; set; }
        public string Divider { get; set; }
        public string Status { get; set; }
        public string Static { get; set; }
        public string Prefix { get; set; }
        public string Owner { get; set; }
    }

    public class TopicNetwork {
        public List<TopicData> Channels = new List<TopicData>();
        [XmlAttribute("ID")] public string ID { get; set; }
    }

    public class TopicList {
        public List<TopicNetwork> Networks = new List<TopicNetwork>();
    }
}
