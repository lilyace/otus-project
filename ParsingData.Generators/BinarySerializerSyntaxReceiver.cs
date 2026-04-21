using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace ParsingData.Generators
{
    internal class BinarySerializerSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidated { get; } = new List<ClassDeclarationSyntax>();
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0)
            {
                Candidated.Add(classDeclaration);
            }
        }
    }
}
