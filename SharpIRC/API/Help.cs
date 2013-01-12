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
using System.Text;
using System.Xml.Serialization;

namespace SharpIRC.API
{
    /// <summary>
    /// 
    /// </summary>
    public class HelpAddin
    {
        /// <summary>
        /// The name of the the plugin question
        /// </summary>
        [XmlAttribute("Name")]public String Name { get; set; }
        /// <summary>
        /// Whether or not this is an admin command.
        /// </summary>
        [XmlAttribute("AdminOnly")]public bool adminonly { get; set; }
        /// <summary>
        /// A short description of this addons purpose
        /// </summary>
        public String Description { get; set; }
        /// <summary>
        /// List of commands available.
        /// </summary>
        public List<HCMD> Commands { get; set; }
    }
    /// <summary>
    /// Help information about a command.
    /// </summary>
    public class HCMD {
        /// <summary>
        /// The type of command.
        /// </summary>
        [XmlAttribute("Type")] public HelpCommandType Type { get; set;}
        /// <summary>
        /// The command itself.
        /// </summary>
        public String Command { get; set; }
        /// <summary>
        /// Permission settings for the command.
        /// </summary>
        public CommandPermission Permission { get; set; }
        /// <summary>
        /// Short description of the inividual command.
        /// </summary>
        public String Description { get; set; }
        /// <summary>
        /// Full command syntax.
        /// </summary>
        public String Syntax { get; set; }

    }

    /// <summary>
    /// Type of command.
    /// </summary>
    public enum HelpCommandType {
        /// <summary>
        /// IRC Channel based command.
        /// </summary>
        Channel,
        /// <summary>
        /// PM based command.
        /// </summary>
        Query,
        /// <summary>
        /// Notice based command.
        /// </summary>
        Notice
    }
    /// <summary>
    /// Permissions information about the command.
    /// </summary>
    public enum CommandPermission {
        /// <summary>
        /// Command can only be accessed by bot administrators.
        /// </summary>
        Admin,
        /// <summary>
        /// Command can be accessed by everyone.
        /// </summary>
        Normal
    }
}
