// -----------------------------------------------------------------------
// <copyright file="PoolGenerator.cs" company="NewSage">
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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NewSage.Generators.GameMemory;

/// <summary>
/// Represents a source generator that scans for classes marked with the
/// specified <c>NewSage.Utilities.MemoryPooledAttribute</c> and
/// generates pool management code for them.
/// </summary>
/// <remarks>
/// The generator processes the target classes during compile-time
/// and creates source code for memory pooling functionality based on the
/// attribute configuration.
/// </remarks>
[Generator]
public sealed class PoolGenerator : IIncrementalGenerator
{
    private const string AttributeName = "NewSage.Utilities.MemoryPooledAttribute";

    /// <summary>
    /// Initializes the source generator, specifying the steps required to analyze syntax and
    /// produce source generation outputs.
    /// </summary>
    /// <param name="context">
    /// The incremental generator initialization context, providing methods to register analysis pipelines
    /// and manage the interaction with the compilation process.
    /// </param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Filter classes that have at least one attribute
        IncrementalValuesProvider<ClassDeclarationSyntax?> classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(static m => m is not null);

        // 2. Combine with the compilation to get full symbols
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax?>)> compilationAndClasses =
            context.CompilationProvider.Combine(classDeclarations.Collect());

        // 3. Generate the source
        context.RegisterSourceOutput(
            compilationAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2!, spc)
        );
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        foreach (
            AttributeSyntax? attribute in classDeclaration.AttributeLists.SelectMany(attributeList =>
                attributeList.Attributes
            )
        )
        {
            if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is not IMethodSymbol attributeSymbol)
            {
                continue;
            }

            INamedTypeSymbol? attributeContainingTypeSymbol = attributeSymbol.ContainingType;
            var fullName = attributeContainingTypeSymbol.ToDisplayString();

            if (fullName == AttributeName)
            {
                return classDeclaration;
            }
        }

        return null;
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classes,
        SourceProductionContext context
    )
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        foreach (ClassDeclarationSyntax? classDecl in classes.Distinct())
        {
            SemanticModel semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol symbol)
            {
                continue;
            }

            AttributeData? attributeData = symbol
                .GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == AttributeName);

            if (attributeData == null)
            {
                continue;
            }

            var namespaceName = symbol.ContainingNamespace.ToDisplayString();
            var className = symbol.Name;

            // Extract attribute values
            var initialSize = attributeData.ConstructorArguments[1].Value ?? 1024;
            var overflowSize = attributeData.ConstructorArguments[2].Value ?? 1024;

            var source = GenerateSource(namespaceName, className, initialSize, overflowSize);
            context.AddSource($"{className}_PoolGlue.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateSource(
        string namespaceName,
        string className,
        object initialSize,
        object overflowSize
    ) =>
        $$"""
            // <auto-generated/>
            using NewSage.Utilities;

            namespace {{namespaceName}};

            internal partial class {{className}}
            {
                private static readonly ObjectPool<{{className}}> _pool = new(() => new {{className}}(), {{initialSize}}, {{overflowSize}});

                static {{className}}() => _pool.Initialize();

                /// <summary>
                /// Fetches a reset instance of <see cref="{{className}}"/> from the {{className}} object pool.
                /// </summary>
                /// <returns>A new instance of <see cref="{{className}}"/>.</returns>
                public static {{className}} New() => _pool.Get();

                /// <summary>
                /// Resets this instance and returns it to the object pool.
                /// </summary>
                public void Delete() => _pool.Return(this);
            }
            """;
}
