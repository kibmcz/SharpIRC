using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpIRC {
    public class filter {
        public List<Preset> presets = new List<Preset>();
        public List<string> wildcards = new List<string>();
    }

    public class Preset {
        public string Nick1 { get; set; }
        public string Nick2 { get; set; }
        public int Love { get; set; }
    }
}
