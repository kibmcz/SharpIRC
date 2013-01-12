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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace SharpIRC.API {
    /// <summary>
    /// Contains an easy way to access information that might be useful to your plugin.
    /// </summary>
    public class Info {
        /// <summary>
        /// Whether or not the application is currently running on the Mono framework.
        /// </summary>
        /// <returns>True if the application is currently running on Mono.</returns>
        public static bool IsRunningMono() {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Returns what version of the Mono framework the application is currently running on.
        /// </summary>
        /// <returns>A Version object, or NULL if the aplication is not on Mono.</returns>
        public static Version MonoFrameworkVersion() {
            var type = Type.GetType("Mono.Runtime");
            return type != null ? new Version(type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null).ToString().Split(' ')[0]) : null;
        }

        /// <summary>
        /// Returns what version of the .NET framework the application is currently running on.
        /// </summary>
        /// <param name="GiveSP">Whether or not to include the current Service Pack version.</param>
        /// <returns>A version object, or NULL if .NET is not installed on this system.</returns>
        public static Version NETFrameworkVersion(bool GiveSP) {
            var installedVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            if (installedVersions != null) {
                var versionNames = installedVersions.GetSubKeyNames();
                var framework = Convert.ToDouble(versionNames[versionNames.Length - 1].Remove(0, 1), CultureInfo.InvariantCulture);
                var openSubKey = installedVersions.OpenSubKey(versionNames[versionNames.Length - 1]);
                if (openSubKey != null) {
                    var sp = Convert.ToInt32(openSubKey.GetValue("SP", 0));
                    return GiveSP ? new Version(framework.ToString(CultureInfo.InvariantCulture) + "." + sp) : new Version(framework.ToString(CultureInfo.InvariantCulture));
                }
            }
            return null;
        }
    }
}
