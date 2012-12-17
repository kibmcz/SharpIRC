using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SharpIRC {
    internal class PingSender {
        private readonly Thread pingSender;
        public PingSender(IRCConnection con) { pingSender = new Thread(() => Run(con)); }
        public void Start() { pingSender.Start(); }
        public void End() { pingSender.Abort(); }

        public static void Run(IRCConnection con) {
            try {
                while (true) {
                    con.writer.WriteLine("PING " + con.NetworkConfiguration.Address);
                    con.writer.Flush();
                    Thread.Sleep(60000);
                }
            } catch {
            }
        }
    }
}
