using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpIRC.API
{
    public class Permissions {
        public static bool RequirePermission(Permission permission) { return false; }
        public enum Permission {
            ACCESS_CONFIGURATION_FILE,
            ACCESS_ADMINLIST,
            ACCESS_IGNORELIST,
            ACCESS_CONNECTIONS
        }
    }
}
