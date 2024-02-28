using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace AssemblyAttributesSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class AssemblyAttributesGenerator : IIncrementalGenerator
{
    private static readonly ImmutableArray<AttributeInfo> _attributeNames = [
        new("Company", "System.Reflection.AssemblyCompanyAttribute"),
        new("Configuration", "System.Reflection.AssemblyConfigurationAttribute"),
        new("Copyright", "System.Reflection.AssemblyCopyrightAttribute"),
        new("Description", "System.Reflection.AssemblyDescriptionAttribute"),
        new("FileVersion", "System.Reflection.AssemblyFileVersionAttribute"),
        new("InformationalVersion", "System.Reflection.AssemblyInformationalVersionAttribute"),
        new("Product", "System.Reflection.AssemblyProductAttribute"),
        new("Title", "System.Reflection.AssemblyTitleAttribute"),
        new("Version", "System.Reflection.AssemblyVersionAttribute"),
    ];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var providers = _attributeNames
            .Select(x => CreateProvider(context, x))
            .ToImmutableArray();

        var final = Join(providers[0], providers[1]);
        foreach (var provider in providers.Slice(2, providers.Length - 2))
        {
            final = Join(final, provider);
        }

        context.RegisterSourceOutput(final, (a, b) =>
        {
            string ns = b.RootNamespace;
            StringBuilder sb = new();

            foreach (var item in b.AttributeNames)
            {
                sb
                    .Append("    ")
                    .AppendLine(item.ToSourceEntry())
                    .AppendLine();
            }

            string properties = sb.ToString().Trim();

            a.AddSource($"{ns}.AssemblyAttributes.g", $$"""
                namespace {{ns}};

                /// <summary>
                /// Constants defined by assembly attributes.
                /// </summary>
                /// <remarks>
                /// This provides constants for assembly attributes typically found in the AssemblyInfo.cs file.
                /// </remarks>
                internal static class {{ns}}AssemblyAttributes
                {
                    {{properties}}
                }
                """);
        });
    }

    private static IncrementalValuesProvider<AttributeInfoDetails> Join(IncrementalValuesProvider<AttributeInfo> incrementalValuesProvider, IncrementalValuesProvider<AttributeInfo> toJoin)
        => incrementalValuesProvider.Combine(toJoin.Collect()).Select((a, _) => new AttributeInfoDetails(a.Left.RootNamespace, a.Right.Add(a.Left)));

    private static IncrementalValuesProvider<AttributeInfoDetails> Join(IncrementalValuesProvider<AttributeInfoDetails> incrementalValuesProvider, IncrementalValuesProvider<AttributeInfo> toJoin)
        => incrementalValuesProvider.Combine(toJoin.Collect()).Select((a, _) => a.Left with { AttributeNames = a.Left.AttributeNames.AddRange(a.Right) });

    private static IncrementalValuesProvider<AttributeInfo> CreateProvider(IncrementalGeneratorInitializationContext context, AttributeInfo attributeName)
        => context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                attributeName.FullyQualifiedAttributeName,
                Predicate,
                Transform)
            .Where(a => a.HasValue && !a.Value.IsEmpty)
            .Select((a, _) => a!.Value)
            .SelectMany((attr, _) => attr)
            .Where(a => a.ConstructorArguments.Length == 1)
            .Select((d, _) =>
            {
                string? cst = d.ConstructorArguments.ItemRef(0).Value as string;
                return attributeName with { ConstructorArgument = cst!, RootNamespace = d.RootNamespace };
            });

    private static ImmutableArray<AttributeData>? Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string rootNamespace = context.TargetSymbol.Name;
        if (string.IsNullOrEmpty(rootNamespace))
        {
            return null;
        }

        if (context.Attributes.IsDefaultOrEmpty)
        {
            return null;
        }

        foreach (var attr in context.Attributes)
        {
            if (attr.ConstructorArguments.IsDefaultOrEmpty)
            {
                return null;
            }

            foreach (var arg in attr.ConstructorArguments)
            {
                if (arg.IsNull)
                {
                    return null;
                }

                if (arg.Value is not string x)
                {
                    return null;
                }

                if (string.IsNullOrWhiteSpace(x))
                {
                    return null;
                }
            }
        }

        return context.Attributes.Select(a => new AttributeData(rootNamespace, a.ConstructorArguments)).ToImmutableArray();
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return true;
    }
}
