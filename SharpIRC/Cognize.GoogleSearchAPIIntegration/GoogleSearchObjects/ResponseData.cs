using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Cognize.GoogleSearchAPIIntegration {
    [DataContract] public class ResponseData {
        /// <summary>
        /// Contains data relating to the actual search results
        /// </summary>
        [DataMember] public Results[] results { get; set; }

        /// <summary>
        /// Contains information about the search call, such as no. of results returned
        /// </summary>
        [DataMember] public Cursor cursor { get; set; }
    }
}
