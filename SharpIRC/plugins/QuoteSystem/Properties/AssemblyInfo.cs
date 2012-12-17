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

[assembly: AssemblyTitle("QuoteSystem")]
[assembly: AssemblyDescription("Provides a way to add and view chat quotes from IRC.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SharpIRC Developers")]
[assembly: AssemblyProduct("QuoteSystem")]
[assembly: AssemblyCopyright("Copyright © 2009-2012 Alex Sørlie, Adonis S. Deliannis, Kevin Crowston")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: Addin]
[assembly: AddinDependency("SharpIRC", "1.0.0.0")]
[assembly: PluginInfo("Quote System", "Alex Sørlie", "Provides a way to add and view chat quotes from IRC.", "1.0.0.0")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("7dbeecfa-1d48-44db-9ef6-b693e4c9ba6c")]

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
