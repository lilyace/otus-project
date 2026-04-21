using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ParsingData.Generators
{
    [Generator]
    public class BinarySerializerGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            /*if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }*/

            var receiver = context.SyntaxReceiver as BinarySerializerSyntaxReceiver;
            if (receiver == null) return;

            var compilation = context.Compilation;
            foreach(var classDecl in receiver.Candidated)
            {
                var semanticModel = compilation.GetSemanticModel(classDecl.SyntaxTree);
                var declaredSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                if (declaredSymbol != null) {
                    var hasAttribute = declaredSymbol.GetAttributes()
                        .Any(a => a.AttributeClass?.Name == "GenerateBinarySerializerAttribute");

                    if (hasAttribute) {
                        var properties = declaredSymbol.GetMembers().Where(s => s.Kind == SymbolKind.Property)
                        .Cast<IPropertySymbol>();

                        var source = GeneratesCustomSerializer(declaredSymbol.ContainingNamespace.ToString(), declaredSymbol.Name, properties);
                        context.AddSource($"{declaredSymbol.Name}.g.cs", source);
                                            
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BinarySerializerSyntaxReceiver());
        }

        private string GeneratesCustomSerializer(string @namespace, string className, IEnumerable<IPropertySymbol> properties)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine();
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");
            sb.AppendLine("public byte[] SerializeToBinary(MemoryStream stream)");
            sb.AppendLine("{");
            sb.AppendLine("using (BinaryWriter writer = new BinaryWriter(stream))");
            sb.AppendLine("{");
            var names = new List<string>(); //$"$\"\\\"{name}\\\" : \\\"{{name}}\\\"\\\"" -> 
            foreach (var property in properties) {
               // if (property.Type.Name != "DateTime")
                    names.Add($"\\\"{property.Name}\\\" : \\\"{{{property.Name}}}\\\"");
               // else

                //sb.AppendLine($"writer.Write({name});");
            }
            var str = string.Join(",", names);
            sb.AppendLine($"writer.Write($\"{{{{{str}}}}}\");");
            sb.AppendLine("}");
            sb.AppendLine("return stream.ToArray();");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
