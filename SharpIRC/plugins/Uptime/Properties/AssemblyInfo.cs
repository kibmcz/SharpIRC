using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Addins;
using SharpIRC.API;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Uptime")]
[assembly: AssemblyDescription("Provides the Uptime of the Connection.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SharpIRC Developers")]
[assembly: AssemblyProduct("Uptime")]
[assembly: AssemblyCopyright("Copyright © 2009-2012 Alex Sørlie, Adonis S. Deliannis, Kevin Crowston")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: Addin]
[assembly: AddinDependency("SharpIRC", "1.0.0.0")]
[assembly: PluginInfo("Uptime", "Adonis S. Deliannis", "Provides the Uptime of the Connection.","1.0.0.0")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e2211ee6-1457-48ce-817b-0303c3b30655")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
