using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cognize.GoogleSearchAPIIntegration {
    [DataContract] public class Results {
        [DataMember] public string GsearchResultClass { get; set; }

        [DataMember] public string unescapedUrl { get; set; }

        [DataMember] public string url { get; set; }

        [DataMember] public string visibleUrl { get; set; }

        [DataMember] public string cacheUrl { get; set; }

        [DataMember] public string title { get; set; }

        [DataMember] public string titleNoFormatting { get; set; }

        [DataMember] public string content { get; set; }
    }
}
