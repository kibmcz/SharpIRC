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

[assembly: AssemblyTitle("Clone")]
[assembly: AssemblyDescription("Copies the messages sent by a user for entertainment value.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SharpIRC Developers")]
[assembly: AssemblyProduct("Clone")]
[assembly: AssemblyCopyright("Copyright © 2009-2012 Alex Sørlie, Adonis S. Deliannis, Kevin Crowston")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: Addin]
[assembly: AddinDependency("SharpIRC", "1.0.0.0")]
[assembly: PluginInfo("Clone", "Alex Sørlie", "Copies the messages sent by a user for entertainment value.", "1.0.0.0")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("2f9446bb-d99a-4ed1-98bf-a0801181d806")]

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
