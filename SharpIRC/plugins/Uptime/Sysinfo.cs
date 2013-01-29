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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Linq;
using System.Threading;
using Mono.Addins;
using SharpIRC.API;

namespace SharpIRC {
    [Extension]
    public class System : PluginInterface {
        public override void ChanMsg(ChannelMessage message) {
            if (message.Message.IsCommand("uptime") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "uptime")) {
                var ts = DateTime.Now.Subtract(Parser.startTime);
                Commands.SendPrivMsg(message.Connection, message.Channel.Name, String.Format("I've been up for {0} days, {1} hours, {2} minutes, and {3} seconds.", ts.Days, ts.Hours, ts.Minutes, ts.Seconds));
            }
            if (message.Message.IsCommand("version") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "version")) {
                Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("SharpIRC IRC Bot v.{0} -  Changeset: ~{1} - Branch: {2} http://sharpirc.codeplex.com/", Program.Version, Program.Revision.RevisionID, Program.Revision.Branch));
            }
            if (message.Message.IsCommand("load") && message.Sender.hasCommandPermission(message.Connection, message.Channel, "load")) {
                Commands.SendPrivMsg(message.Connection, message.Channel, String.Format("{0}Application Ressource Usage{0} CPU Usage: {1}. Memory Usage: {2}", IRCStyle.Bold(), Math.Round(Convert.ToDecimal(getCPUCOunter()), 2) + "%", ConvertSize(GC.GetTotalMemory(false))));
            }
        }

        public string ConvertSize(long len) {
            string[] sizes = { "B", "KiB", "MiB", "GiB" };
            var order = 0;
            while (len >= 1024 && order + 1 < sizes.Length) {
                order++;
                len = len / 1024;
            }
            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public object getCPUCOunter() {

            var cpuCounter = new PerformanceCounter
                             {CategoryName = "Processor", CounterName = "% Processor Time", InstanceName = "_Total"};

            // will always start at 0
            dynamic firstValue = cpuCounter.NextValue();
            Thread.Sleep(1000);
            // now matches task manager reading
            dynamic secondValue = cpuCounter.NextValue();

            return secondValue;

        }
    }
}