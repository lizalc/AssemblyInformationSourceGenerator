using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AssemblyAttributesSourceGenerator;

public readonly record struct AttributeInfo(string Name, string FullyQualifiedAttributeName)
{
    public string ConstructorArgument { get; init; } = "";

    public string RootNamespace { get; init; } = "";

    public string DocSource => $"{FullyQualifiedAttributeName}.{Name}";

    public string ToSourceEntry() => $$"""
        /// <inheritdoc cref="global::{{DocSource}}"/>
            internal static string {{Name}} { get; } = "{{ConstructorArgument}}";
        """;
}

public readonly record struct AttributeData(string RootNamespace, ImmutableArray<TypedConstant> ConstructorArguments);

public readonly record struct AttributeInfoDetails(string RootNamespace, ImmutableArray<AttributeInfo> AttributeNames);
