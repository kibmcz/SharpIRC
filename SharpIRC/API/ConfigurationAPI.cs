using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace SharpIRC.API
{
    /// <summary>
    /// An API provided to make storing configurations and databases easy.
    /// </summary>
    public class ConfigurationAPI
    {
        internal static List<ConfigurationFile> ConfigurationFiles = new List<ConfigurationFile>();
        private static Object filesLock = new Object();

        /// <summary>
        /// Creates or modifies a custom Serializable XML Configuration file.
        /// </summary>
        /// <param name="d">Serializable class to write to the XML file.</param>
        /// <param name="fileName">Filename for the XML file without extension.</param>
        public static void SaveConfigurationFile(Object d, string fileName) {
            // Serialization
            var s = new XmlSerializer(d.GetType());
            string configpath = String.Format("{0}{1}Database{1}{2}.xml", Program.StartupPath, Path.DirectorySeparatorChar, fileName);
            XmlWriter w = XmlWriter.Create(configpath, new XmlWriterSettings { CheckCharacters = false, Indent = true });
            s.Serialize(w, d);
            w.Close();
        }

        /// <summary>
        /// Loads a custom Serializable XML Configuration file. If it does not exist a new object is created.
        /// </summary>
        /// <typeparam name="TObj">Class type.</typeparam>
        /// <param name="fileName">Filename for the XML file without extension.</param>
        /// <returns>Object containing the serialized information of the file.</returns>
        public static TObj LoadConfigurationFile<TObj>(string fileName)
        {
            lock (filesLock)
            {
                try
                {
                    // Deserialization
                    var s = new XmlSerializer(typeof(TObj));
                    s.UnknownElement += Serializer_UnknownElement;
                    s.UnknownNode += Serializer_UnknownNode;
                    s.UnknownAttribute += Serializer_UnknownAttribute;
                    ConfigurationFiles.Add(new ConfigurationFile {
                        Name = fileName, LastModifed = File.GetLastWriteTime(String.Format("{0}{1}Database{1}{2}.xml", Program.StartupPath, Path.DirectorySeparatorChar, fileName))
                    });
                    var configpath = String.Format("{0}{1}Database{1}{2}.xml", Program.StartupPath, Path.DirectorySeparatorChar, fileName);
                    var r = XmlReader.Create(configpath, new XmlReaderSettings { CheckCharacters = false });
                    var newList = s.Deserialize(r);
                    r.Close();
                    return (TObj)newList;
                }
                catch
                {
                    return (TObj)typeof(TObj).GetConstructor(Type.EmptyTypes).Invoke(null);
                }
            }
        }

        internal static void StartAutomaticFileChecker()
        {
            if (Program.GlobalSettings.AutomaticConfigurationReload)
            {
                new Thread(delegate(object files)
                {
                    while (true)
                    {
                        Thread.Sleep(10000);
                        lock (filesLock)
                        {
                            foreach (var file in (List<ConfigurationFile>)files)
                            {
                                var lastmodified = File.GetLastWriteTime(String.Format("{0}{1}Database{1}{2}.xml", Program.StartupPath, Path.DirectorySeparatorChar, file));
                                if (lastmodified != file.LastModifed)
                                {
                                    file.LastModifed = lastmodified;
                                    foreach (var plugin in Program.Plugins)
                                    {
                                        plugin.ConfigurationChange(file);
                                    }
                                }
                            }
                        }
                    }
                }).Start(ConfigurationFiles);
            }
        }

        private static void Serializer_UnknownElement(object sender, XmlElementEventArgs e) {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your database file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(Program.StartupPath, "/Database/" + (sender as string)), e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your database file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(Program.StartupPath, "/Database/" + (sender as string)), e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }
        private static void Serializer_UnknownNode(object sender, XmlNodeEventArgs e) {
            Connect.PrintError(String.Format("SharpIRC encountered an error parsing your database file in path: \"{0}\" and cannot proceed. Invalid Element. Line: {1}. Position: {2}. Error: {3}", Path.Combine(Program.StartupPath, "/Database/" + (sender as string)), e.LineNumber, e.LinePosition));
            Console.ReadLine();
            Environment.Exit(0);
        }
    }

    /// <summary>
    /// An XML serialized Configuration or database file.
    /// </summary>
    public class ConfigurationFile
    {
        /// <summary>
        /// The filename of the configuration file without extension.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The last point at which the file was modified.
        /// </summary>
        public DateTime LastModifed { get; set; }
    }
}