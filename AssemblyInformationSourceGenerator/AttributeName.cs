using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace AssemblyInformationSourceGenerator;

public readonly record struct AttributeName(string Name, string FullyQualifiedAttributeName)
{
    public string ConstructorArgument { get; init; } = "";

    public string RootNamespace { get; init; } = "";

    public string ToSourceEntry() => $"public static string {Name} {{ get; }} = \"{ConstructorArgument}\";";
}

public readonly record struct AData(string RootNamespace, ImmutableArray<TypedConstant> ConstructorArguments);

public readonly record struct Asm(string RootNamespace, ImmutableArray<AttributeName> AttributeNames);