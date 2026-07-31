using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleStorage.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class BinarySerializerGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor _unsupportedTypeRule =
        new(
            "SG0001",
            "Unsupported property type",
            "Property '{0}' in class '{1}' has unsupported type '{2}' for binary serialization",
            "GenerateSerializer",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

    private static readonly Dictionary<string, (string ReadMethod, string WriteCast)> _supportedTypes = new()
    {
        { "int", ("ReadInt32()", string.Empty) },
        { "long", ("ReadInt64()", string.Empty) },
        { "bool", ("ReadBoolean()", string.Empty) },
        { "double", ("ReadDouble()", string.Empty) },
        { "float", ("ReadSingle()", string.Empty) },
        { "System.DateTime", ("new DateTime(reader.ReadInt64())", ".Ticks") }
    };

    private readonly record struct GenerationResult(INamedTypeSymbol? Symbol, List<Diagnostic>? Diagnostics);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: (ctx, cancellationToken) =>
                {
                    var classDeclaration = (ClassDeclarationSyntax)ctx.Node;
                    var semanticModel = ctx.SemanticModel;
                    if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
                    {
                        return default;
                    }

                    foreach (var attr in classSymbol.GetAttributes())
                    {
                        var name = attr.AttributeClass?.Name;
                        var attrName = attr.AttributeClass?.ToDisplayString();

                        if (name == "GenerateBinarySerializerAttribute" ||
                            attrName?.EndsWith("GenerateBinarySerializerAttribute", StringComparison.InvariantCulture) == true)
                        {
                            return ValidateProperties(classSymbol);
                        }
                    }

                    return default;
                })
            .Where(result => result.Symbol is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses,
            (ctx, combined) =>
            {
                var (_, results) = combined;
                foreach (var result in results)
                {
                    if (result.Diagnostics is not null)
                    {
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            ctx.ReportDiagnostic(diagnostic);
                        }
                    }

                    if (result.Diagnostics?.Any(d => d.Severity == DiagnosticSeverity.Error) == true)
                    {
                        continue;
                    }

                    if (result.Symbol is INamedTypeSymbol classSymbol)
                    {
                        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                        var code = GenerateSerializerCode(classSymbol, namespaceName);
                        ctx.AddSource($"{classSymbol.Name}_BinarySerializer.g.cs", code);
                    }
                }
            });
    }

    private static GenerationResult ValidateProperties(INamedTypeSymbol classSymbol)
    {
        List<Diagnostic>? diagnostics = null;

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic);

        foreach (var prop in properties)
        {
            var typeStr = prop.Type.ToDisplayString();

            var isSupported = _supportedTypes.ContainsKey(typeStr) ||
                typeStr == "string" ||
                typeStr == "byte[]";

            if (!isSupported)
            {
                diagnostics ??= [];

                var location = prop.Locations.FirstOrDefault() ?? Location.None;
                diagnostics.Add(Diagnostic.Create(
                    _unsupportedTypeRule,
                    location,
                    prop.Name,
                    classSymbol.Name,
                    typeStr
                ));
            }
        }

        return new GenerationResult(classSymbol, diagnostics);
    }

    private static string GenerateSerializerCode(INamedTypeSymbol classSymbol, string namespaceName)
    {
        var className = classSymbol.Name;

        var properties = classSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .ToList();

        var stringBuilder = new StringBuilder($@"// <auto-generated />
using System;
using System.IO;
using System.Text;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        public void SerializeToBinary(Stream stream)
        {{
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
");

        foreach (var prop in properties)
        {
            var name = prop.Name;
            var type = prop.Type.ToDisplayString();

            if (_supportedTypes.TryGetValue(type, out var typeInfo))
            {
                stringBuilder.AppendLine($"            writer.Write({name}{typeInfo.WriteCast});");
            }
            else if (type == "string")
            {
                stringBuilder.AppendLine($"            if ({name} is null)");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine("                writer.Write(0);");
                stringBuilder.AppendLine("            }");
                stringBuilder.AppendLine("            else");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine($"                var {name}Bytes = Encoding.UTF8.GetBytes({name});");
                stringBuilder.AppendLine($"                writer.Write({name}Bytes.Length);");
                stringBuilder.AppendLine($"                writer.Write({name}Bytes);");
                stringBuilder.AppendLine("            }");
            }
            else if (type == "byte[]")
            {
                stringBuilder.AppendLine($"            writer.Write({name}?.Length ?? 0);");
                stringBuilder.AppendLine($"            if ({name} is not null)");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine($"                writer.Write({name});");
                stringBuilder.AppendLine("            }");
            }
            else
            {
                stringBuilder.AppendLine($"            // TODO: сериализация {type} для свойства {name}");
            }
        }

        stringBuilder.AppendLine($@"        }}

        public static {className} DeserializeFromBinary(Stream stream)
        {{
            var obj = new {className}();
            using var reader = new BinaryReader(stream);
");

        foreach (var prop in properties)
        {
            var name = prop.Name;
            var type = prop.Type.ToDisplayString();

            if (_supportedTypes.TryGetValue(type, out var typeInfo))
            {
                if (type == "System.DateTime")
                {
                    stringBuilder.AppendLine($"            obj.{name} = {typeInfo.ReadMethod};");
                }
                else
                {
                    stringBuilder.AppendLine($"            obj.{name} = reader.{typeInfo.ReadMethod};");
                }
            }
            else if (type == "string")
            {
                stringBuilder.AppendLine($"            var {name}Length = reader.ReadInt32();");
                stringBuilder.AppendLine($"            if ({name}Length == 0)");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine($"                obj.{name} = string.Empty;");
                stringBuilder.AppendLine("            }");
                stringBuilder.AppendLine("            else");
                stringBuilder.AppendLine("            {");
                stringBuilder.AppendLine($"                var {name}Bytes = reader.ReadBytes({name}Length);");
                stringBuilder.AppendLine($"                obj.{name} = Encoding.UTF8.GetString({name}Bytes);");
                stringBuilder.AppendLine("            }");
            }
            else if (type == "byte[]")
            {
                stringBuilder.AppendLine($"            var {name}Length = reader.ReadInt32();");
                stringBuilder.AppendLine($"            obj.{name} = reader.ReadBytes({name}Length);");
            }
            else
            {
                stringBuilder.AppendLine($"            // TODO: десериализация {type} для свойства {name}");
            }
        }

        stringBuilder.AppendLine($@"
            return obj;
        }}
    }}
}}");
        return stringBuilder.ToString();
    }
}