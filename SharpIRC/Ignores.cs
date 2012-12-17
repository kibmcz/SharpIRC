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

namespace SharpIRC {
    /// <summary>
    /// The public list of people who are added to ignore.
    /// </summary>
    public class Ignore {
        /// <summary>
        /// <see href="Ignore" />
        /// </summary>
        public List<IgnoredUser> Ignores = new List<IgnoredUser>();
    }

    /// <summary>
    /// A user that is currently added to ignore.
    /// </summary>
    public class IgnoredUser {
        /// <summary>
        /// The host that has been ignored, only the domain part is stored. (the section after @)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The reason as to why the user was ignored.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// When the ignore was added.
        /// </summary>
        public DateTime Added { get; set; }
    }
}
