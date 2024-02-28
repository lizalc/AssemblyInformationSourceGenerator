# AssemblyAttributesSourceGenerator

Converts assembly level attributes to constants.

## Output example

Given the following `AssemblyInfo.cs` file:

```csharp
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("Example Company")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyCopyrightAttribute("Copyright © Example Company 2024")]
[assembly: System.Reflection.AssemblyDescriptionAttribute("Example description.")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("0.1.2.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("0.1.2-beta+g1234567.c0cb5e2bae6340db93d94367b3e0b95b38504ca5")]
[assembly: System.Reflection.AssemblyProductAttribute("Example Product")]
[assembly: System.Reflection.AssemblyTitleAttribute("ExampleProject")]
[assembly: System.Reflection.AssemblyVersionAttribute("0.1.2.0")]

// Generated by the MSBuild WriteCodeFragment class.
```

The source generator will generate the following code:

```csharp
namespace ExampleProject;

/// <summary>
/// Constants defined by assembly attributes.
/// </summary>
/// <remarks>
/// This provides constants for assembly attributes typically found in the AssemblyInfo.cs file.
/// </remarks>
internal static class ExampleProjectAssemblyAttributes
{
    /// <inheritdoc cref="global::System.Reflection.AssemblyConfigurationAttribute.Configuration"/>
    internal static string Configuration { get; } = "Debug";

    /// <inheritdoc cref="global::System.Reflection.AssemblyCompanyAttribute.Company"/>
    internal static string Company { get; } = "Example Company";

    /// <inheritdoc cref="global::System.Reflection.AssemblyCopyrightAttribute.Copyright"/>
    internal static string Copyright { get; } = "Copyright © Example Company 2024";

    /// <inheritdoc cref="global::System.Reflection.AssemblyDescriptionAttribute.Description"/>
    internal static string Description { get; } = "Example description.";

    /// <inheritdoc cref="global::System.Reflection.AssemblyFileVersionAttribute.FileVersion"/>
    internal static string FileVersion { get; } = "0.1.2.0";

    /// <inheritdoc cref="global::System.Reflection.AssemblyInformationalVersionAttribute.InformationalVersion"/>
    internal static string InformationalVersion { get; } = "0.1.2-beta+g1234567.c0cb5e2bae6340db93d94367b3e0b95b38504ca5";

    /// <inheritdoc cref="global::System.Reflection.AssemblyProductAttribute.Product"/>
    internal static string Product { get; } = "Example Product";

    /// <inheritdoc cref="global::System.Reflection.AssemblyTitleAttribute.Title"/>
    internal static string Title { get; } = "ExampleProject";

    /// <inheritdoc cref="global::System.Reflection.AssemblyVersionAttribute.Version"/>
    internal static string Version { get; } = "0.1.2.0";
}
```