using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace BiliLite.Analyzer
{
    class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // 收集所有类声明
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                // 检查是否有任何我们感兴趣的属性
                if (classDeclaration.AttributeLists.Count > 0)
                {
                    CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
