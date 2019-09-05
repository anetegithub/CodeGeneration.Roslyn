using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sample.Generator
{
    public class AsyncMachineGenerator : IRichCodeGenerator
    {
        public AsyncMachineGenerator(AttributeData attributeData)
        {
        }

        public Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            File.WriteAllText("S:\\USUAL.txt", "dsadfsa");

            var partialType = CreatePartialType();
            return Task.FromResult(SyntaxFactory.List(partialType));

            IEnumerable<MemberDeclarationSyntax> CreatePartialType()
            {
                var newPartialType = 
                    context.ProcessingNode is ClassDeclarationSyntax classDeclaration
                        ? SyntaxFactory.ClassDeclaration(classDeclaration.Identifier.ValueText)
                        : context.ProcessingNode is StructDeclarationSyntax structDeclaration
                            ? SyntaxFactory.StructDeclaration(structDeclaration.Identifier.ValueText)
                            : default(TypeDeclarationSyntax);
                if (newPartialType is null)
                    yield break;
                yield return newPartialType
                    ?.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                    .AddMembers(CreateAsyncStateMachine());
            }

            MemberDeclarationSyntax CreateAsyncStateMachine()
            {
                return SyntaxFactory.ParseMemberDeclaration("public System.Guid Id2 { get; } = default;");
            }
        }

        public Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            IEnumerable<MemberDeclarationSyntax> CreatePartialType()
            {
                TypeDeclarationSyntax newPartialType = default;

                if (context.ProcessingNode is MemberDeclarationSyntax)
                {
                    newPartialType = context.ProcessingNode.Parent as ClassDeclarationSyntax;
                    newPartialType.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
                    newPartialType = SyntaxFactory.ClassDeclaration(newPartialType.Identifier.ValueText);
                }

                yield return newPartialType
                    ?.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                    .AddMembers(CreateAsyncStateMachine(context.ProcessingNode as MethodDeclarationSyntax));
            }

            MemberDeclarationSyntax CreateAsyncStateMachine(MethodDeclarationSyntax asyncMethod)
            {
                return SyntaxFactory.ParseMemberDeclaration("public class HelloJopa{}");
            }

            var r = new RichGenerationResult();
            r.Members = new SyntaxList<MemberDeclarationSyntax>(CreatePartialType().ToList());
            return Task.FromResult(r);
        }
    }
}