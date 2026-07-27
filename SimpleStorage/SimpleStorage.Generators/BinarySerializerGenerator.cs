using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text;

namespace SimpleStorage.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class BinarySerializerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax,
                transform: (ctx, token) =>
                {
                    var classSyntax = (ClassDeclarationSyntax)ctx.Node;
                    var semanticModel = ctx.SemanticModel;
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax, token);
                    if (classSymbol == null)
                    {
                        return null;
                    }

                    foreach (var attr in classSymbol.GetAttributes())
                    {
                        var attrName = attr.AttributeClass?.ToDisplayString();

                        if (attrName == "SimpleStorage.Generators.GenerateBinarySerializerAttribute" ||
                            attrName?.EndsWith("GenerateBinarySerializerAttribute", StringComparison.InvariantCulture) == true)
                        {
                            return classSymbol;
                        }
                    }

                    return null;
                })
            .Where(symbol => symbol != null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses,
            (ctx, combined) =>
            {
                var (compilation, symbols) = combined;
                foreach (var symbol in symbols)
                {
                    if (symbol is INamedTypeSymbol classSymbol)
                    {
                        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                        var code = GenerateSerializerCode(classSymbol, namespaceName);
                        ctx.AddSource($"{classSymbol.Name}_BinarySerializer.g.cs", code);
                    }
                }
            });
    }

    private static string GenerateSerializerCode(INamedTypeSymbol classSymbol, string namespaceName)
    {
        var className = classSymbol.Name;

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .ToList();

        var stringBuilder = new StringBuilder($@"
using System;
using System.IO;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        public void SerializeToBinary(Stream stream)
        {{
            using var writer = new BinaryWriter(stream);
");

        foreach (var prop in properties)
        {
            var name = prop.Name;
            var type = prop.Type.ToDisplayString();
            switch (type)
            {
                case "int":
                    stringBuilder.AppendLine($"            writer.Write({name});");
                    break;
                case "string":
                    stringBuilder.AppendLine($"            writer.Write({name});");
                    break;
                case "System.DateTime":
                    stringBuilder.AppendLine($"            writer.Write({name}.Ticks);");
                    break;
                default:
                    stringBuilder.AppendLine($"            // TODO: сериализация {type} для свойства {name}");
                    break;
            }
        }

        stringBuilder.AppendLine($@"
        }}
    }}
}}");
        return stringBuilder.ToString();
    }
}