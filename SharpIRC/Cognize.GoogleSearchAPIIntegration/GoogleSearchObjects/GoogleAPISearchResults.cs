using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cognize.GoogleSearchAPIIntegration {
    [DataContract] internal class GoogleAPISearchResults {
        [DataMember] internal ResponseData responseData { get; set; }
    }
}
