using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

namespace AssemblyInformationSourceGenerator;

[Generator(LanguageNames.CSharp)]
public class AssemblyInformationGenerator : IIncrementalGenerator
{
    private static readonly AttributeName _company = new("Company", "System.Reflection.AssemblyCompanyAttribute");
    private static readonly AttributeName _configuration = new("Configuration", "System.Reflection.AssemblyConfigurationAttribute");
    private static readonly AttributeName _fileVersion = new("FileVersion", "System.Reflection.AssemblyFileVersionAttribute");
    private static readonly AttributeName _informationalVersion = new("InformationalVersion", "System.Reflection.AssemblyInformationalVersionAttribute");
    private static readonly AttributeName _product = new("Product", "System.Reflection.AssemblyProductAttribute");
    private static readonly AttributeName _title = new("Title", "System.Reflection.AssemblyTitleAttribute");
    private static readonly AttributeName _version = new("Version", "System.Reflection.AssemblyVersionAttribute");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var companyProvider = CreateProvider(context, _company);
        var configurationProvider = CreateProvider(context, _configuration);
        var fileVersionProvider = CreateProvider(context, _fileVersion);
        var informationalVersionProvider = CreateProvider(context, _informationalVersion);
        var productProvider = CreateProvider(context, _product);
        var titleProvider = CreateProvider(context, _title);
        var versionProvider = CreateProvider(context, _version);

        var final = Join(companyProvider, configurationProvider);
        final = Join(final, fileVersionProvider);
        final = Join(final, informationalVersionProvider);
        final = Join(final, productProvider);
        final = Join(final, titleProvider);
        final = Join(final, versionProvider);

        context.RegisterSourceOutput(final, (a, b) =>
        {
            string ns = b.RootNamespace;
            StringBuilder sb = new();

            foreach (var item in b.AttributeNames)
            {
                sb
                    .AppendLine(item.ToSourceEntry())
                    .AppendLine()
                    .Append("    ");
            }

            string properties = sb.ToString().Trim();
            a.AddSource($"{nameof(AssemblyInformationSourceGenerator)}.AssemblyInfo.g.cs", $$"""
				namespace {{ns}};

				public static class AssemblyInfo
				{
					{{properties}}
				}
				""");
        });
    }

    private static IncrementalValuesProvider<Asm> Join(IncrementalValuesProvider<AttributeName> incrementalValuesProvider, IncrementalValuesProvider<AttributeName> toJoin)
        => incrementalValuesProvider.Combine(toJoin.Collect()).Select((a, _) => new Asm(a.Left.RootNamespace, a.Right.Add(a.Left)));

    private static IncrementalValuesProvider<Asm> Join(IncrementalValuesProvider<Asm> incrementalValuesProvider, IncrementalValuesProvider<AttributeName> toJoin)
        => incrementalValuesProvider.Combine(toJoin.Collect()).Select((a, _) => a.Left with { AttributeNames = a.Left.AttributeNames.AddRange(a.Right) });

    private static IncrementalValuesProvider<AttributeName> CreateProvider(IncrementalGeneratorInitializationContext context, AttributeName attributeName)
        => context
            .SyntaxProvider
            .ForAttributeWithMetadataName(
                attributeName.FullyQualifiedAttributeName,
                Predicate,
                Transform)
            .SelectMany((attr, _) => attr)
            .Select((d, _) =>
            {
                string? cst = d.ConstructorArguments.ItemRef(0).Value as string;
                return attributeName with { ConstructorArgument = cst!, RootNamespace = d.RootNamespace };
            });

    private static ImmutableArray<AData> Transform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string rootNamespace = context.TargetSymbol.Name;
        return context.Attributes.Select(a => new AData(rootNamespace, a.ConstructorArguments)).ToImmutableArray();
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return node is ICompilationUnitSyntax;
    }
}
