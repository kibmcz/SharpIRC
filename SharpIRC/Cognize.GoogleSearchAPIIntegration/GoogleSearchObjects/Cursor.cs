using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cognize.GoogleSearchAPIIntegration {
    [DataContract] public class Cursor {
        [DataMember] public Pages[] pages { get; set; }

        [DataMember] public int estimatedResultCount { get; set; }

        [DataMember] public int currentPageIndex { get; set; }

        [DataMember] public string moreResultsUrl { get; set; }

        [DataMember] public string responseDetails { get; set; }

        [DataMember] public int responseStatus { get; set; }
    }
}
