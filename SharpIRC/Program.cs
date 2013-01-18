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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using SharpIRC.API;
using SharpIRC.Properties;
using Mono.Addins;

namespace SharpIRC {
    /// <summary>
    /// Main Application Runtime Class.
    /// </summary>
    public class Program {
        /// <summary>
        /// The Global Configuration/Settings element in the bot.
        /// </summary>
        public static Config Configuration = new Config();

        /// <summary>
        /// The Global Ignore list for the bot.
        /// </summary>
        public static Ignore IgnoreList = ConfigurationAPI.LoadConfigurationFile<Ignore>("Ignore");

        /// <summary>
        /// The bots current assembly version information.
        /// </summary>
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// A List of plugins currently loaded in the bot.
        /// </summary>
        public static List<PluginInterface> Plugins = new List<PluginInterface>();

        /// <summary>
        /// A list of currently open IRC Connections.
        /// </summary>
        public static List<IRCConnection> Connections = new List<IRCConnection>();

        /// <summary>
        /// The current path of the bot executable.
        /// </summary>
        public static string StartupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// List of admins currently logged in.
        /// </summary>
        public static List<LoggedInAdmin> Sessions = new List<LoggedInAdmin>();

        private static void Main(string[] args) {
            try {
                if (Info.IsRunningMono()) {
                    if (Info.MonoFrameworkVersion().CompareTo(new Version(2, 6, 10)) < 0) {
                        Program.OutputConsole(String.Format(
                                "You are running an outdated version of the Mono Framework ({0}) and may experience unexpected results or bugs while running this application. Version 2.6.10 or above is prefered.",
                                Info.MonoFrameworkVersion()), ConsoleMessageType.Warning);
                    }
                }
                if (!WritePermission()) {
                    Program.OutputConsole("Fatal Error: Lacking write permission to main directory: " + StartupPath, ConsoleMessageType.Error);
                    Console.ReadLine();
                }

                if (!File.Exists(Path.Combine(StartupPath, "app.config"))) {
                    Program.OutputConsole("SharpIRC did not find a configuration file in path " + Path.Combine(StartupPath, "app.config") + " A default file has been generated, please fill in the required information.", ConsoleMessageType.Information);
                    File.WriteAllBytes(Path.Combine(StartupPath, "app.config"), Encoding.ASCII.GetBytes(Resources.sharpirc));
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                Configuration = DeserializeDataFile();
                if (Configuration == null) {
                    Program.OutputConsole("SharpIRC encountered an error parsing your configuration file in path: " + Path.Combine(StartupPath, "app.config") + " and cannot proceed.", ConsoleMessageType.Error);
                    Console.ReadLine();
                    Environment.Exit(0);
                }

               
                ConfigurationAPI.StartAutomaticFileChecker();
                if (!Directory.Exists(Path.Combine(StartupPath, "Database"))) {
                    Program.OutputConsole("The database and configuration directory does not exist, generating a new one..", ConsoleMessageType.Information);
                    Directory.CreateDirectory(Path.Combine(StartupPath, "Database"));
                }
                if (Directory.Exists(StartupPath + Path.DirectorySeparatorChar + "Addins")) {
                    Program.OutputConsole("Found deprecated addins folder, SharpIRC now utilize the \"Plugins\" folder, automatically deleting..", ConsoleMessageType.Error);
                    var directory = new DirectoryInfo(StartupPath + Path.DirectorySeparatorChar + "Addins");
                    directory.Empty();
                }
                if (Configuration.Admins.Count(admin => admin.Owner) > 1) {
                    Program.OutputConsole("Multiple bot owners are defined in configuration file and SharpIRC cannot proceed.", ConsoleMessageType.Error);
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                Program.OutputConsole("Loading plugins..", ConsoleMessageType.Normal);
                LoadPlugins();
                //Permissions.PermissionsList = Permissions.LoadPermissionsData();

                foreach (var net in Configuration.Networks.Where(net => net.ID == Guid.Empty)) {
                    net.ID = Guid.NewGuid();
                    SerializeDataFile(Configuration);
                }
                foreach (var netw in Configuration.Networks) {
                    var con = new IRCConnection {NetworkConfiguration = netw};
                    new Thread(() => Connect.ConnnectToNetwork(con)).Start();
                }
            }
            catch(Exception ex) {
                Program.OutputConsole(ex.GetBaseException().ToString(), ConsoleMessageType.Error);
            }
        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        public static void Restart() {
            if (Info.IsRunningMono()) {
                var process = new Process {StartInfo = {FileName = "mono", Arguments = Path.Combine(StartupPath, Process.GetCurrentProcess().ProcessName + ".exe")}};
                process.Start();
                Environment.Exit(0);
            } else {
                Process.Start(Path.Combine(StartupPath, Process.GetCurrentProcess().ProcessName + ".exe"));
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Saves the changes to the Global Configuration to file.
        /// </summary>
        /// <param name="d"></param>
        public static void SerializeDataFile(Config d) {
            try {
                // Serialization
                var s = new XmlSerializer(typeof(Config));
                string configpath = Path.Combine(StartupPath, "app.config");
                XmlWriter w = XmlWriter.Create(configpath, new XmlWriterSettings { CheckCharacters = false, Indent = true });

                s.Serialize(w, d);
                w.Close();
            }
            catch(Exception ex) {
                Program.OutputConsole(ex.GetBaseException().ToString(), ConsoleMessageType.Error);
            }
        }

        /// <summary>
        /// Loads all plugins in the plugins directory.
        /// </summary>
        public static void LoadPlugins() {
            AddinManager.Initialize(StartupPath, "Plugins");
            AddinManager.Registry.Update();
            Console.WriteLine(AddinManager.Registry.DefaultAddinsFolder);
            foreach (var extensionObject in AddinManager.GetExtensionObjects<PluginInterface>()) {
                Plugins.Add(extensionObject);
            }

            foreach (var attribute in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetCustomAttributes(typeof(PluginInfoAttribute), false).Cast<PluginInfoAttribute>())) {
                Program.OutputConsole(String.Format("Loaded Plugin: \"{0}\" by {1}. Version: {2} ", attribute.Name, attribute.Author, attribute.Version), ConsoleMessageType.Normal);
            }
            if (Plugins.Count == 0) {
                Program.OutputConsole("No plugins were loaded, this means vital ressources are missing and the application may not operate properly. Are you sure you want to continue? (Y/N)", ConsoleMessageType.Warning);
                string ch = "";
                while (!String.Equals((ch = Console.ReadLine()), "y", StringComparison.OrdinalIgnoreCase)) {
                    if (String.Equals(ch, "n", StringComparison.OrdinalIgnoreCase)) {
                        Environment.Exit(0);
                    }
                }
            }
            Program.OutputConsole("All Plugins Loaded", ConsoleMessageType.Normal);
        }

        /// <summary>
        /// Loads the Global Configuration from the Settings.xml file.
        /// </summary>
        /// <returns></returns>
        public static Config DeserializeDataFile() {
            try {
                var s = new XmlSerializer(typeof (Config));
                s.UnknownElement += Serializer_UnknownElement;
                s.UnknownNode += Serializer_UnknownNode;
                s.UnknownAttribute += Serializer_UnknownAttribute;
                char sep = Path.DirectorySeparatorChar;
                string configpath = StartupPath + sep + "app.config";
                XmlReader r = XmlReader.Create(configpath, new XmlReaderSettings {CheckCharacters = false});

                var newList = (Config) s.Deserialize(r);
                r.Close();
                return newList;
            } catch(Exception ex) {
                Program.OutputConsole(ex.GetBaseException().ToString(), ConsoleMessageType.Error);
                return null;
            }
        }

        private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            Program.OutputConsole(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}",Path.Combine(StartupPath, "app.config"),e.LineNumber, e.LinePosition), ConsoleMessageType.Error);
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Program.OutputConsole(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(StartupPath, "app.config"), e.LineNumber, e.LinePosition), ConsoleMessageType.Error);
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Program.OutputConsole(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(StartupPath, "app.config"), e.LineNumber, e.LinePosition), ConsoleMessageType.Error);
            Console.ReadLine();
            Environment.Exit(0);
        }

        private static bool WritePermission() {
            try {
                File.WriteAllText(Path.Combine(StartupPath, "Access.txt"), Resources.Program_WritePermission_Filesystem_access_test_);
                File.Delete(Path.Combine(StartupPath, "Access.txt"));
                return true;
            }
            catch {
                return false;
            }
        }
        /// <summary>
        /// Outputs a message to console. Please use this instead of just Console.Writeline
        /// </summary>
        /// <param name="message">The message. (Remains unchanged)</param>
        /// <param name="type">The type of console message.</param>
        public static void OutputConsole(string message, ConsoleMessageType type) {
            switch (type) {
                case ConsoleMessageType.Normal: Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case ConsoleMessageType.Information: Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case ConsoleMessageType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    System.Media.SystemSounds.Beep.Play();
                    break;
                case ConsoleMessageType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    System.Media.SystemSounds.Beep.Play();
                    Functions.LogError(message);
                    break;
            }
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
