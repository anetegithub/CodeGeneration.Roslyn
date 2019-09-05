// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MS-PL license. See LICENSE.txt file in the project root for full license information.

namespace CodeGeneration.Roslyn
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Describes a code generator that responds to attributes on members to generate code,
    /// and returns compilation unit members.
    /// </summary>
    public interface IFreeCodeGenerator : IRichCodeGenerator
    {
        /// <summary>
        /// Determine generator can process this node
        /// </summary>
        /// <param name="syntaxNode">node for processing</param>
        /// <returns>true if can process</returns>
        bool CanProcess(CSharpSyntaxNode syntaxNode);
    }
}
