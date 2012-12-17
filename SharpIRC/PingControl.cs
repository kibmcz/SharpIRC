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
