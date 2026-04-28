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
        private static readonly DiagnosticDescriptor UnsupportedTypeRule =
            new DiagnosticDescriptor(
                "SG0001",
                "Unsupported property type",
                "Property '{0}' in type '{1}' has unsupported type '{2}' for binary serialization",
                "GenerateSerializer",
                DiagnosticSeverity.Error,
                true);

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
            //вариантов было два. Второй находится в другом коммите.
            //остановилась на этом т.к. с точки зрения бенчмарка он чуть-чуть пошустрее
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine($"namespace {@namespace}");
            sb.AppendLine("{");
            sb.AppendLine();
            sb.AppendLine($"public partial class {className}");
            sb.AppendLine("{");
            sb.AppendLine("public byte[] SerializeToBinary(MemoryStream stream)");
            sb.AppendLine("{");
            sb.AppendLine("using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))");
            sb.AppendLine("{");
            var names = new List<string>(); 
            foreach (var property in properties) {
                if (property.Type.Name == "String")
                {
                    sb.AppendLine("var bytes = System.Text.Encoding.UTF8.GetBytes(this." + property.Name + ");");
                    sb.AppendLine("//writer.Write(bytes.Length);");
                    sb.AppendLine("writer.Write(bytes);");
                }
                // names.Add($"\\\"{property.Name}\\\" : \\\"{{{property.Name}}}\\\"");
                else if (property.Type.Name != "DateTime")
                    sb.AppendLine($"writer.Write({property.Name});");
                else
                {
                    sb.AppendLine($"var strDate = {property.Name}.ToString();");
                    sb.AppendLine("var bytes1 = System.Text.Encoding.UTF8.GetBytes(strDate);");
                    sb.AppendLine("writer.Write(bytes1);");
                    sb.AppendLine($"//writer.Write({property.Name}.ToString());");
                }
            }
           // var str = string.Join(",", names);
            //sb.AppendLine($"writer.Write($\"{{{{{str}}}}}\");");
            sb.AppendLine("}");
            sb.AppendLine("return stream.ToArray();");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            return sb.ToString();
        }
    }
}
