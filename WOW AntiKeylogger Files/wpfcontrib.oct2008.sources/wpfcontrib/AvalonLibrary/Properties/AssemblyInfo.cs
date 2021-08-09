using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Markup;
using System.Resources;

[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: AllowPartiallyTrustedCallers]
[assembly: SecurityCritical]

[assembly: AssemblyTitle("Avalon Library")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany("The WPF Contrib Project")]
[assembly: AssemblyProduct("The WPF Contrib Project")]
[assembly: AssemblyCopyright("Copyright ©  2008")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]

[assembly: NeutralResourcesLanguage("en-US")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // theme-specific
    ResourceDictionaryLocation.SourceAssembly   // generic
)]

[assembly: XmlnsPrefix("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "av")]
[assembly: XmlnsDefinition("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "Avalon.Windows.Controls")]
[assembly: XmlnsDefinition("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "Avalon.Windows.Converters")]
[assembly: XmlnsDefinition("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "Avalon.Windows.Media.Animation")]
[assembly: XmlnsDefinition("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "Avalon.Windows.Media.Effects")]
[assembly: XmlnsDefinition("http://schemas.codeplex.com/wpfcontrib/xaml/presentation", "Avalon.Windows.Utility")]
