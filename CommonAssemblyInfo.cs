using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyDescription("NewProjectTemplate")]
[assembly: AssemblyProduct("NewProjectTemplate")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0.0")]

[assembly: ComVisible(false)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Retail")]
#endif
