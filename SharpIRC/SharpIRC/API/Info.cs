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
            Type type = Type.GetType("Mono.Runtime");
            return type != null ? new Version(type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null).ToString().Split(' ')[0]) : null;
        }

        /// <summary>
        /// Returns what version of the .NET framework the application is currently running on.
        /// </summary>
        /// <param name="GiveSP">Whether or not to include the current Service Pack version.</param>
        /// <returns>A version object, or NULL if .NET is not installed on this system.</returns>
        public static Version NETFrameworkVersion(bool GiveSP) {
            RegistryKey installedVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
            string[] versionNames = installedVersions.GetSubKeyNames();
            double framework = Convert.ToDouble(versionNames[versionNames.Length - 1].Remove(0, 1), CultureInfo.InvariantCulture);
            RegistryKey openSubKey = installedVersions.OpenSubKey(versionNames[versionNames.Length - 1]);
            int SP = Convert.ToInt32(openSubKey.GetValue("SP", 0));
            return GiveSP ? new Version(framework.ToString(CultureInfo.InvariantCulture) + "." + SP) : new Version(framework.ToString(CultureInfo.InvariantCulture));
        }
    }
}
