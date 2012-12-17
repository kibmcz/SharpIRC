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
        [XmlAttribute("Name")]public String Name { get; set; }
        [XmlAttribute("AdminOnly")]public bool adminonly { get; set; }
        public String Description { get; set; }
        public List<CMD> Commands { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CMD {
        [XmlAttribute("Type")] public HelpCommandType Type { get; set;}
        public String Command { get; set; }
        public CommandPermission Permission { get; set; }
        public String Description { get; set; }
        public String Syntax { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public enum HelpCommandType {
        Channel,
        Query,
        Notice
    }
    /// <summary>
    /// 
    /// </summary>
    public enum CommandPermission {
        Admin,
        Normal
    }
}
