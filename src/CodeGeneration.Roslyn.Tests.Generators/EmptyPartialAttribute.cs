﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MS-PL license. See LICENSE.txt file in the project root for full license information.

namespace CodeGeneration.Roslyn.Tests.Generators
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [CodeGenerationAttribute(typeof(EmptyPartialGenerator))]
    [Conditional("CodeGeneration")]
    public class EmptyPartialAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [CodeGenerationAttribute(typeof(AsyncMachineGenerator))]
    public class GenerateAsyncMachineAttribute : Attribute
    {
    }

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
            File.WriteAllText("S:\\RICH BITCH.txt", "dsadfsa");

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
                return SyntaxFactory.ParseMemberDeclaration("public class HelloJopa{}");
            }

            var r = new RichGenerationResult
            {
                Members = new SyntaxList<MemberDeclarationSyntax>(CreatePartialType().ToList()),
            };
            return Task.FromResult(r);
        }
    }
}