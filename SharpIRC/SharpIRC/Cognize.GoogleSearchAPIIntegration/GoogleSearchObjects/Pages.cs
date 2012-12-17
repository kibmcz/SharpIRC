using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cognize.GoogleSearchAPIIntegration {
    [DataContract] public class Pages {
        [DataMember] public int start { get; set; }

        [DataMember] public int label { get; set; }
    }
}
