using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        public static Settings GlobalSettings = new Settings();

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
        public static List<string> Sessions = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public static List<Admin> LoggedIn = new List<Admin>();

        /// <summary>
        /// Writes a "comment" to console.
        /// </summary>
        /// <param name="comment">Comment to write to console.</param>
        public static void Comment(string comment) {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(comment);
            Console.ForegroundColor = ConsoleColor.White;
            if (GlobalSettings.LogComments) {
                try {
                    string makePath = Path.Combine(StartupPath, "Logs");
                    if (!Directory.Exists(makePath)) Directory.CreateDirectory(makePath);
                    makePath = Path.Combine(makePath, DateTime.Now.ToString("dd-MM-yyyy") + ".txt");
                    FileStream fs = File.Open(makePath, FileMode.Append, FileAccess.Write);
                    var sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.WriteLine("[" + DateTime.Now.ToString("M HH:mm:ss") + "] " + comment);
                    sw.Close();
                    fs.Close();
                }
                catch {
                }
            }
        }

        private static void Main(string[] args) {
            try {
                if (Info.IsRunningMono()) {
                    if (Info.MonoFrameworkVersion().CompareTo(new Version(2, 6, 10)) < 0) {
                        Connect.PrintError(String.Format(
                                "You are running an outdated version of the Mono Framework ({0}) and may experience unexpected results or bugs while running this application. Version 2.6.10 or above is prefered.",
                                Info.MonoFrameworkVersion()));
                        Functions.LogError(String.Format(
                                "You are running an outdated version of the Mono Framework ({0}) and may experience unexpected results or bugs while running this application. Version 2.6.10 or above is prefered.",
                                Info.MonoFrameworkVersion()));
                    }
                }
                if (!File.Exists(Path.Combine(StartupPath, "Settings.xml"))) {
                    Connect.PrintError("SharpIRC did not find a configuration file in path " + Path.Combine(StartupPath, "Settings.xml") + " A new file has been generated, please fill in the required information.");
                    File.WriteAllBytes(Path.Combine(StartupPath, "Settings.xml"), Encoding.ASCII.GetBytes(Resources.Settings));
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                GlobalSettings = DeserializeDataFile();
                if (GlobalSettings == null) {
                    Connect.PrintError("SharpIRC encountered an error parsing your configuration file in path: " + Path.Combine(StartupPath, "Settings.xml") + " and cannot proceed.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                ConfigurationAPI.StartAutomaticFileChecker();
                if (!Directory.Exists(Path.Combine(StartupPath, "Database"))) {
                    Connect.PrintError("The database and configuration directory does not exist, generating a new one..");
                    Directory.CreateDirectory(Path.Combine(StartupPath, "Database"));
                }
                if (Directory.Exists(StartupPath + Path.DirectorySeparatorChar + "Addins")) {
                    Connect.PrintError("Found deprecated addins folder, SharpIRC now utilize the \"Plugins\" folder, automatically deleting..");
                    var directory = new DirectoryInfo(StartupPath + Path.DirectorySeparatorChar + "Addins");
                    directory.Empty();
                }
                if (GlobalSettings.Admins.Count(admin => admin.Owner) > 1) {
                    Connect.PrintError("Multiple bot owners are defined in configuration file and SharpIRC cannot proceed.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                Comment("Loading plugins..");
                LoadPlugins();
                Permissions.PermissionsList = Permissions.LoadPermissionsData();

                foreach (var net in GlobalSettings.Networks.Where(net => net.ID == Guid.Empty)) {
                    net.ID = Guid.NewGuid();
                    SerializeDataFile(GlobalSettings);
                }
                foreach (var netw in GlobalSettings.Networks) {
                    var con = new IRCConnection {NetworkConfiguration = netw};
                    new Thread(() => Connect.ConnnectToNetwork(con)).Start();
                }
            } catch {
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
        public static void SerializeDataFile(Settings d) {
            // Serialization
            var s = new XmlSerializer(typeof (Settings));
            string configpath = Path.Combine(StartupPath, "Settings.xml");
            XmlWriter w = XmlWriter.Create(configpath, new XmlWriterSettings {CheckCharacters = false, Indent = true});

            s.Serialize(w, d);
            w.Close();
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
                Comment(String.Format("Loaded Plugin: \"{0}\" by {1}. Version: {2} ", attribute.Name, attribute.Author, attribute.Version));
            }
            if (Plugins.Count == 0) {
                Connect.PrintError("No plugins were loaded, this means vital ressources are missing and the application may not operate properly. Are you sure you want to continue? (Y/N)");
                string ch = "";
                while (!String.Equals((ch = Console.ReadLine()), "y", StringComparison.OrdinalIgnoreCase)) {
                    if (String.Equals(ch, "n", StringComparison.OrdinalIgnoreCase)) {
                        Environment.Exit(0);
                    }
                }
            }
            Comment("All Plugins Loaded");
        }

        /// <summary>
        /// Loads the Global Configuration from the Settings.xml file.
        /// </summary>
        /// <returns></returns>
        public static Settings DeserializeDataFile() {
            try {
                var s = new XmlSerializer(typeof (Settings));
                s.UnknownElement += Serializer_UnknownElement;
                s.UnknownNode += Serializer_UnknownNode;
                s.UnknownAttribute += Serializer_UnknownAttribute;
                char sep = Path.DirectorySeparatorChar;
                string configpath = StartupPath + sep + "Settings.xml";
                XmlReader r = XmlReader.Create(configpath, new XmlReaderSettings {CheckCharacters = false});

                var newList = (Settings) s.Deserialize(r);
                r.Close();
                return newList;
            } catch(Exception ex) {
                Connect.PrintError(ex.Message);
                return null;
            }
        }

        private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e)
        {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}",Path.Combine(StartupPath, "Settings.xml"),e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e)
        {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(StartupPath, "Settings.xml"), e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e)
        {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your configuration file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(StartupPath, "Settings.xml"), e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }

    }
}
