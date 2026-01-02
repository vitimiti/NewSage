// -----------------------------------------------------------------------
// <copyright file="BitMaskGenerator.cs" company="NewSage">
// A transliteration and update of the CnC Generals (Zero Hour) engine and games with mod-first support.
// Copyright (C) 2025 NewSage Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see https://www.gnu.org/licenses/.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NewSage.Generators.BitMask;

/// <summary>
/// A source generator that creates additional code output for enumerations
/// marked with a specific attribute. This generator processes enumerations
/// with attributes and generates bitmask-related code based on the provided
/// configuration.
/// </summary>
[Generator]
public sealed class BitMaskGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "NewSage.Game.Masks.BitMasksAttribute";

    /// <summary>
    /// Initializes the incremental generator with the specified context.
    /// This method configures syntax providers and registers the source output for code generation.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IncrementalGeneratorInitializationContext"/> used to configure and register the generator.
    /// </param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumDeclarationSyntax?> enumDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is EnumDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(static m => m is not null);

        context.RegisterSourceOutput(enumDeclarations.Collect(), Execute);
    }

    private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;
        foreach (
            AttributeSyntax? attribute in enumDeclaration.AttributeLists.SelectMany(attributeList =>
                attributeList.Attributes
            )
        )
        {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }

            INamedTypeSymbol? attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            if (attributeContainingTypeSymbol.ToDisplayString() == AttributeFullName)
            {
                return enumDeclaration;
            }
        }

        return null;
    }

    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        var nameSpace = string.Empty;
        SyntaxNode? parent = syntax.Parent;
        while (parent is not null and not NamespaceDeclarationSyntax and not FileScopedNamespaceDeclarationSyntax)
        {
            parent = parent.Parent;
        }

        if (parent is BaseNamespaceDeclarationSyntax namespaceDeclaration)
        {
            nameSpace = namespaceDeclaration.Name.ToString();
        }

        return nameSpace;
    }

    [SuppressMessage(
        "csharpsquid",
        "S1192:String literals should not be duplicated",
        Justification = "Makes it less readable."
    )]
    private static void Execute(SourceProductionContext context, ImmutableArray<EnumDeclarationSyntax?> enums)
    {
        if (enums.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (EnumDeclarationSyntax? enumDecl in enums.Distinct())
        {
            if (enumDecl is null)
            {
                continue;
            }

            var sb = new StringBuilder();
            var enumName = enumDecl.Identifier.Text;
            var structName = enumName.EndsWith("Type") ? $"{enumName.Replace("Type", "Mask")}" : $"{enumName}Mask";
            var namespaceName = GetNamespace(enumDecl);
            var accessibility = enumDecl.Modifiers.Any(m => m.ValueText == "public") ? "public" : "internal";

            // Derive count from the number of members (excluding the 'Invalid' member if present).
            var memberCount = enumDecl.Members.Count;
            var hasInvalid = enumDecl.Members.Any(m => m.Identifier.Text == "Invalid");
            var bitCount = hasInvalid ? memberCount - 1 : memberCount;

            _ = sb.AppendLine("// <auto-generated/>")
                .AppendLine("#nullable enable")
                .AppendLine("using System;")
                .AppendLine("using System.Runtime.CompilerServices;")
                .AppendLine("using System.Runtime.InteropServices;")
                .AppendLine("using System.Text;")
                .AppendLine()
                .AppendLine($"namespace {namespaceName};")
                .AppendLine()
                .AppendLine($"/// <summary>Represents a bitmask for the {enumName} enumeration.</summary>")
                .AppendLine("[StructLayout(LayoutKind.Sequential)]")
                .AppendLine($"{accessibility} partial struct {structName} : IEquatable<{structName}>")
                .AppendLine("{")
                .AppendLine($"    private const int BitCount = {bitCount};")
                .AppendLine("    private const int ElementCount = (BitCount + 31) / 32;")
                .AppendLine()
                .AppendLine("    [StructLayout(LayoutKind.Explicit, Size = ElementCount * sizeof(uint))]")
                .AppendLine("    private struct InlineBuffer")
                .AppendLine("    {")
                .AppendLine("        [FieldOffset(0)] public uint Data;")
                .AppendLine("    }")
                .AppendLine()
                .AppendLine("    private InlineBuffer _bits;")
                .AppendLine()
                .AppendLine("    /// <summary>Tests if the specified enum value is set in the bitmask.</summary>")
                .AppendLine($"    public readonly bool Test({enumName} type)")
                .AppendLine("    {")
                .AppendLine("        int index = (int)type;")
                .AppendLine("        if (index < 0 || index >= BitCount) return false;")
                .AppendLine("        return (GetElement(index / 32) & (1u << (index % 32))) != 0;")
                .AppendLine("    }")
                .AppendLine()
                .AppendLine("    /// <summary>Sets the specified enum value in the bitmask.</summary>")
                .AppendLine($"    public void Set({enumName} type, bool value)")
                .AppendLine("    {")
                .AppendLine("        int index = (int)type;")
                .AppendLine("        if (index < 0 || index >= BitCount) return;")
                .AppendLine("        ref uint element = ref GetElement(index / 32);")
                .AppendLine("        if (value) element |= (1u << (index % 32));")
                .AppendLine("        else element &= ~(1u << (index % 32));")
                .AppendLine("    }")
                .AppendLine()
                .AppendLine("    private readonly ref uint GetElement(int index) =>")
                .AppendLine("        ref Unsafe.Add(ref Unsafe.AsRef(in _bits.Data), index);")
                .AppendLine()
                .AppendLine("    /// <summary>Returns the bitmask as a span of bytes.</summary>")
                .AppendLine("    /// <returns>A span of bytes representing the bitmask.</returns>")
                .AppendLine("    public readonly Span<byte> AsBytes() =>")
                .AppendLine(
                    "        MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref Unsafe.AsRef(in _bits.Data), ElementCount));"
                )
                .AppendLine()
                .AppendLine("    /// <inheritdoc/>")
                .AppendLine(
                    $"    public readonly bool Equals({structName} other) => AsBytes().SequenceEqual(other.AsBytes());"
                )
                .AppendLine()
                .AppendLine("    /// <inheritdoc/>")
                .AppendLine(
                    $"    public override readonly bool Equals(object? obj) => obj is {structName} other && Equals(other);"
                )
                .AppendLine()
                .AppendLine("    /// <inheritdoc/>")
                .AppendLine("    public override readonly int GetHashCode() => _bits.Data.GetHashCode();")
                .AppendLine()
                .AppendLine("    /// <inheritdoc/>")
                .AppendLine("    public override readonly string ToString()")
                .AppendLine("    {")
                .AppendLine("        var sb = new StringBuilder(BitCount);")
                .AppendLine("        for (var i = BitCount - 1; i >= 0; i--)")
                .AppendLine("        {")
                .AppendLine("            _ = sb.Append((GetElement(i / 32) & (1u << (i % 32))) != 0 ? '1' : '0');")
                .AppendLine("        }")
                .AppendLine()
                .AppendLine("        return sb.ToString();")
                .AppendLine("    }")
                .AppendLine()
                .AppendLine("    /// <summary>Tests two bitmasks for equality.</summary>")
                .AppendLine("    /// <param name=\"left\">The left bitmask.</param>")
                .AppendLine("    /// <param name=\"right\">The right bitmask.</param>")
                .AppendLine(
                    "    /// <returns><see langword=\"true\"/> if the bitmasks are equal; otherwise, <see langword=\"false\"/>.</returns>"
                )
                .AppendLine(
                    $"    public static bool operator ==({structName} left, {structName} right) => left.Equals(right);"
                )
                .AppendLine()
                .AppendLine("    /// <summary>Tests two bitmasks for inequality.</summary>")
                .AppendLine("    /// <param name=\"left\">The left bitmask.</param>")
                .AppendLine("    /// <param name=\"right\">The right bitmask.</param>")
                .AppendLine(
                    "    /// <returns><see langword=\"true\"/> if the bitmasks are not equal; otherwise, <see langword=\"false\"/>.</returns>"
                )
                .AppendLine(
                    $"    public static bool operator !=({structName} left, {structName} right) => !left.Equals(right);"
                )
                .AppendLine("}");

            context.AddSource($"{structName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }
}
