using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SharpIRC.API;

namespace SharpIRC {
    internal class FloodControl {
        internal static List<CommandCache> Cache = new List<CommandCache>();
        internal static List<ChannelCache> ChCache = new List<ChannelCache>(); 
        public static Timeouts Timeouts = new Timeouts(); 
        public static void AddCommand(IRCConnection connection, Channel channel, IRCUser nick) {
            Cache.Add(new CommandCache {
                                           Network = connection.Configuration.ID,
                                           Nick = nick.Nick,
                                           Time = DateTime.Now
                                       });

            Cache.RemoveAll(x => (DateTime.Now - x.Time).TotalSeconds > 10);
            var c = Cache.Count(x => x.Nick == nick.Nick && x.Network == connection.Configuration.ID);
            if (c > Program.Configuration.UserLimit) TimeOut(connection, nick);


            ChCache.Add(new ChannelCache {
                                             Network = connection.Configuration.ID,
                                             Channel = channel.Name,
                                             Time = DateTime.Now
                                         });

            ChCache.RemoveAll(x => (DateTime.Now - x.Time).TotalSeconds > 10);
            var ch = ChCache.Count(x => x.Channel == channel.Name && x.Network == connection.Configuration.ID);
            if (ch > Program.Configuration.ChannelLimit) TimeOut(connection, channel);
        }
        internal static void TimeOut(IRCConnection connection, IRCUser nick) {
            var mTimeout = new UserTimeout {
                                               Network = connection.Configuration.ID,
                                               Nick = nick.Nick
                                           };
            Timeouts.UserTimeouts.Add(mTimeout);
            Program.OutputConsole(String.Format("Command overload by {0} on {1}. User has been put in timeout.", nick.Nick, connection.ActiveNetwork), ConsoleMessageType.Information);
            TimeoutTimer(mTimeout);
        }

        internal static void TimeOut(IRCConnection connection, Channel channel) {
            var mTimeout = new ChannelTimeout {
                Network = connection.Configuration.ID,
                Channel = channel.Name
            };
            Timeouts.ChannelTimeouts.Add(mTimeout);
            Program.OutputConsole(String.Format("Command overload in {0} on {1}. Channel has been put in timeout.", channel.Name, connection.ActiveNetwork), ConsoleMessageType.Information);
            TimeoutTimer(mTimeout);
        }

        internal static void TimeoutTimer(ChannelTimeout timeout) {
            new Thread(new ThreadStart(delegate {
                Thread.Sleep(Program.Configuration.IgnoreSeconds * 1000);
                                           Timeouts.ChannelTimeouts.Remove(timeout);
                                       })).Start();
        }

        internal static void TimeoutTimer(UserTimeout timeout) {
            new Thread(new ThreadStart(delegate {
                Thread.Sleep(Program.Configuration.IgnoreSeconds * 1000);
                Timeouts.UserTimeouts.Remove(timeout);
            })).Start();
        }
    }

    internal class CommandCache {
        public Guid Network { get; set; }
        public string Nick { get; set; }
        public DateTime Time { get; set; }
    }

    internal class ChannelCache {
        public Guid Network { get; set; }
        public string Channel { get; set; }
        public DateTime Time { get; set; }
    }

    internal class Timeouts {
        public List<ChannelTimeout> ChannelTimeouts = new List<ChannelTimeout>();
        public List<UserTimeout> UserTimeouts = new List<UserTimeout>();
    }

    internal class ChannelTimeout {
        public Guid Network { get; set; }
        public string Channel { get; set; }
    }

    internal class UserTimeout {
        public Guid Network { get; set; }
        public string Nick { get; set; }
    }
}
