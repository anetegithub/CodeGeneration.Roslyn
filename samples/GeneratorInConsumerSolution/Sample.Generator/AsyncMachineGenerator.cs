using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeGeneration.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Sample.Generator
{
    public class AsyncMachineGenerator : IFreeCodeGenerator
    {
        public AsyncMachineGenerator(AttributeData attributeData)
        {
        }


        public bool CanProcess(CSharpSyntaxNode syntaxNode)
        {
            if(syntaxNode is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                var isAsync = methodDeclarationSyntax.Modifiers.Any(x => x.ToString() == "async");
                var isStepState = methodDeclarationSyntax.ReturnType.ToString().Contains("StepState<StepStatus>");
                
                var canprocess = isAsync && isStepState;

                if(canprocess)
                {
                    File.AppendAllText($"S:\\logfree.txt", $"{Environment.NewLine}neededNode: {methodDeclarationSyntax}");
                }

                return canprocess;
            }

            File.AppendAllText($"S:\\logfree.txt", $"{Environment.NewLine}wrongnode");

            return false;
        }

        public async Task<SyntaxList<MemberDeclarationSyntax>> GenerateAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            FixPdb(context.Compilation);
            
            if (!(context.ProcessingNode is MethodDeclarationSyntax methodDeclarationSyntax))
                return new SyntaxList<MemberDeclarationSyntax>();

            var awaitNodes = methodDeclarationSyntax.DescendantNodes()
                .Where(n => n is AwaitExpressionSyntax)
                .Cast<AwaitExpressionSyntax>()
                .ToList();

            List<AwaitInfo> awaitInfos = new List<AwaitInfo>();

            string log = string.Empty;

            var methodString = methodDeclarationSyntax.ChildNodes()
                .FirstOrDefault(x => x is BlockSyntax)
                .ToString();
            
            Dictionary<string, int> hashTempVariables = new Dictionary<string, int>();

            foreach (var awaitNode in awaitNodes)
            {

                var awaitExpressionType = context.SemanticModel.GetTypeInfo(awaitNode.Expression);

                var symbolInfo = context.SemanticModel.GetSymbolInfo(awaitNode.Expression);

                var isStatic = symbolInfo.Symbol.IsStatic;

                if (awaitExpressionType.ConvertedType is INamedTypeSymbol namedType)
                {
                    var getAwaiterSymbol = namedType.GetMembers().FirstOrDefault(m => m.Name == "GetAwaiter");
                    if (getAwaiterSymbol is IMethodSymbol awaiterMethodSymbol)
                    {
                        if (awaiterMethodSymbol.ReturnType is INamedTypeSymbol returnNamedSymbol)
                        {
                            string awaiterType = returnNamedSymbol.Name + "<" + string.Join(",", returnNamedSymbol.TypeArguments.Select(t => t.ToString())) + ">";

                            var dontNeedThis = (awaitNode.Expression is MemberAccessExpressionSyntax) || isStatic;

                            var tempVariableName = awaitNode
                                .ToString()
                                .ToLower()
                                .Replace(".", "")
                                .Replace(" ", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace(";", "");

                            if (hashTempVariables.ContainsKey(tempVariableName))
                            {
                                hashTempVariables[tempVariableName]++;
                                tempVariableName += hashTempVariables[tempVariableName];
                            }
                            else
                            {
                                hashTempVariables.Add(tempVariableName, 0);
                                tempVariableName += "0";
                            }

                            //    Guid.NewGuid().ToString().Replace("-", "");
                            //tempVariableName = Regex.Replace(tempVariableName, "[0-9]", "");

                            var beforeBlock = FindBeforeParentBlockSyntax(awaitNode);

                            var needVariable = NeedVariable(awaitNode, beforeBlock);

                            var awaitInfo = new AwaitInfo()
                            {
                                Await = awaitNode.ToString(),
                                AwaiterType = awaiterType,
                                NeedThis = !dontNeedThis,
                                GenericType = returnNamedSymbol.TypeArguments.FirstOrDefault()?.ToString(),
                                AssignmentVariable = needVariable ? tempVariableName : ""
                            };

                            awaitInfos.Add(awaitInfo);

                            if (needVariable)
                            {
                                methodString = methodString.Replace(beforeBlock.ToString(), $"var {tempVariableName};{Environment.NewLine}" + beforeBlock.ToString());
                                methodString = ReplaceFirst(methodString, awaitNode.ToString(), tempVariableName);

                                //var fullNewAwait = $"var {tempVariableName}={awaitNode};{Environment.NewLine}";
                                //methodString = methodString.Replace($"var {tempVariableName};{Environment.NewLine}", fullNewAwait);

                                //awaitInfo.Await = fullNewAwait;
                            }

                            log += Environment.NewLine
                                + returnNamedSymbol.Name + "<" + string.Join(",", returnNamedSymbol.TypeArguments.Select(t => t.ToString())) + ">";
                        }
                    }
                }
            }

            //теперь надо заменить переменные на вызов методов, т.к. теперь мы точно можем идентифицировать нужные места
            foreach (var awaitInfo in awaitInfos)
            {
                if (string.IsNullOrEmpty(awaitInfo.AssignmentVariable))
                {
                    awaitInfo.AwaitNew = awaitInfo.Await;
                }
                else
                {
                    awaitInfo.AwaitNew = $"var {awaitInfo.AssignmentVariable}={awaitInfo.Await};";
                    methodString = methodString.Replace($"var {awaitInfo.AssignmentVariable};", $"var {awaitInfo.AssignmentVariable}={awaitInfo.Await};");
                }
            }

            var clearMethodString = methodString.Trim(new char[] { '{', '}' });

            string TempAwaiters = string.Empty;

            List<string> tempAwaitersHash = new List<string>();

            foreach (var awaitInfo in awaitInfos)
            {
                var tempClassName = awaitInfo.AwaiterType
                    .ToLowerInvariant()
                    .Replace(".", "_")
                    .Replace("<", "_")
                    .Replace(">", "");

                var tempName = $"tempAwaiter__oftype__{tempClassName}";

                var typeAwaiter = $"{awaitInfo.AwaiterType} {tempName};";
                var boolTypeAwaiter = typeAwaiter.Replace(";", "").Replace(awaitInfo.AwaiterType, "bool") + "_completed;";

                awaitInfo.TempAwaiterName = tempName;
                awaitInfo.TempAwaiterNameCompleted = tempName.Replace(";","") + "_completed";

                if (!tempAwaitersHash.Contains(typeAwaiter))
                {
                    TempAwaiters += Environment.NewLine + "// пары переменных временных значений awaiter'ов: первая переменная сам awaiter, вторая информация о том установлен он или нет, т.к. не каждую struct можно проверить на default";
                    TempAwaiters += Environment.NewLine + typeAwaiter;
                    TempAwaiters += Environment.NewLine + boolTypeAwaiter;

                    tempAwaitersHash.Add(typeAwaiter);
                }
            }

            methodString += Environment.NewLine + TempAwaiters;

            string TempVariables = Environment.NewLine + "// переменные в методе выведенные в состояние которое может сохраняться";

            foreach (var awaitInfo in awaitInfos)
            {
                if (!string.IsNullOrEmpty(awaitInfo.AssignmentVariable))
                {
                    TempVariables += $"{Environment.NewLine}{awaitInfo.GenericType} {awaitInfo.AssignmentVariable};";
                }
            }
            TempVariables += Environment.NewLine + "// локальные переменные в оригинальном методе";
            
            var allLocalVariables = methodDeclarationSyntax.DescendantNodes()
                .Where(n => n is VariableDeclarationSyntax)
                .Cast<VariableDeclarationSyntax>();

            List<string> postprocessVars = new List<string>();

            foreach (var localVariable in allLocalVariables)
            {
                var symbolInfo =context.SemanticModel.GetSymbolInfo(localVariable.Type);
                var typeSymbol = symbolInfo.Symbol;

                var t = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

                var variableName = localVariable.DescendantNodes()
                    .Where(n => n is VariableDeclaratorSyntax)
                    .Cast<VariableDeclaratorSyntax>()
                    .FirstOrDefault();

                TempVariables += $"{Environment.NewLine}{t} {variableName.Identifier};";

                postprocessVars.Add(variableName.Identifier.ToString());
            }

            methodString += Environment.NewLine + TempVariables;

            string LocalAwaiterVariables = "// локальные переменные метода";

            int awaitCount = 1;
            foreach (var awaitInfo in awaitInfos)
            {
                var awaiterVariable = $"awaiter{awaitCount}";
                var awaiterName = $"{awaitInfo.AwaiterType} {awaiterVariable}";
                awaitInfo.LocalAwaiter = awaiterVariable;
                LocalAwaiterVariables += Environment.NewLine + awaiterName+";";
                awaitCount++;
            }

            methodString += Environment.NewLine + LocalAwaiterVariables;
            
            string AsyncMethod = "// изменённое тело метода";
            int caseNum = -1;
            foreach (var awaitInfo in awaitInfos)
            {
                var next = awaitInfos.ElementAtOrDefault(awaitInfos.IndexOf(awaitInfo) + 1);
                AsyncMethod += Environment.NewLine + CaseTemplate(clearMethodString, awaitInfo, next, awaitInfos.First() == awaitInfo, awaitInfos.Last() == awaitInfo, caseNum, methodDeclarationSyntax.Identifier.ToString());
                caseNum++;
            }

            methodString += Environment.NewLine + AsyncMethod;

            var stateMachine = AsyncMachineTemplate(
                methodDeclarationSyntax.FirstAncestorOrSelf<NamespaceDeclarationSyntax>().Name.ToString(),
                methodDeclarationSyntax.Identifier.ToString(),
                (methodDeclarationSyntax.Parent as ClassDeclarationSyntax).Identifier.ToString(),
                TempAwaiters,
                TempVariables,
                LocalAwaiterVariables,
                AsyncMethod);

            foreach (var @var in postprocessVars)
            {
                stateMachine = stateMachine.Replace($"var {@var}", @var);
            }

            File.AppendAllText($"S:\\GenerateRichAsync.txt", stateMachine);

            return new SyntaxList<MemberDeclarationSyntax>(SyntaxFactory.ParseMemberDeclaration(stateMachine));
        }

        private void FixPdb(Compilation compilation)
        {

            File.AppendAllText($"S:\\fiulepath.txt", compilation.SyntaxTrees.FirstOrDefault().FilePath);

            using (var asm = new MemoryStream())
            {
                using (var pdb = new MemoryStream())
                {
                    var path = Assembly.GetExecutingAssembly().Location.Replace("dll", "pdb");

                    var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb, pdbFilePath: $"{path}");

                    var compilationResult = compilation.Emit(asm, pdb, options: emitOptions);
                    

                    var fileStream = File.Create("S:\\f.pdb");

                    pdb.Seek(0, SeekOrigin.Begin);
                    pdb.CopyTo(fileStream);

                    fileStream.Close();
                }
            }
        }

        public async Task<RichGenerationResult> GenerateRichAsync(TransformationContext context, IProgress<Diagnostic> progress, CancellationToken cancellationToken)
        {
            var generatedMembers = await GenerateAsync(context, progress, CancellationToken.None);

            // Figure out ancestry for the generated type, including nesting types and namespaces.
            var wrappedMembers = context.ProcessingNode.Ancestors().Aggregate(generatedMembers, WrapInAncestor);
            return new RichGenerationResult { Members = wrappedMembers };
        }

        private static SyntaxList<MemberDeclarationSyntax> WrapInAncestor(SyntaxList<MemberDeclarationSyntax> generatedMembers, SyntaxNode ancestor)
        {
            switch (ancestor)
            {
                case NamespaceDeclarationSyntax ancestorNamespace:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(ancestorNamespace)
                        .WithMembers(generatedMembers));
                    break;
                case ClassDeclarationSyntax nestingClass:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(nestingClass)
                        .WithMembers(generatedMembers));
                    break;
                case StructDeclarationSyntax nestingStruct:
                    generatedMembers = SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        CopyAsAncestor(nestingStruct)
                        .WithMembers(generatedMembers));
                    break;
            }
            return generatedMembers;
        }

        private static NamespaceDeclarationSyntax CopyAsAncestor(NamespaceDeclarationSyntax syntax)
        {
            return SyntaxFactory.NamespaceDeclaration(syntax.Name.WithoutTrivia())
                .WithExterns(SyntaxFactory.List(syntax.Externs.Select(x => x.WithoutTrivia())))
                .WithUsings(SyntaxFactory.List(syntax.Usings.Select(x => x.WithoutTrivia())));
        }

        private static ClassDeclarationSyntax CopyAsAncestor(ClassDeclarationSyntax syntax)
        {
            return SyntaxFactory.ClassDeclaration(syntax.Identifier.WithoutTrivia())
                .WithModifiers(SyntaxFactory.TokenList(syntax.Modifiers.Select(x => x.WithoutTrivia())))
                .WithTypeParameterList(syntax.TypeParameterList);
        }

        private static StructDeclarationSyntax CopyAsAncestor(StructDeclarationSyntax syntax)
        {
            return SyntaxFactory.StructDeclaration(syntax.Identifier.WithoutTrivia())
                .WithModifiers(SyntaxFactory.TokenList(syntax.Modifiers.Select(x => x.WithoutTrivia())))
                .WithTypeParameterList(syntax.TypeParameterList);
        }
        
        public string CaseTemplate(string method, AwaitInfo awaitInfo, AwaitInfo next, bool start, bool end, int caseNum,string methodName)
        {
            var template = @"case [Case]:
                            completed = [TempAwaiterCompleted];
                            if (!completed)
                            {
                                // код вначале метода в первом case
                                [CodeBefore]

                                //получение awaiter
								[AwaiterN] = [AwaiterCode].GetAwaiter();
                                if (![AwaiterN].IsCompleted)
                                {
                                    state++;
                                    [TempAwaiter] = [AwaiterN]; //запоминаем темповую переменную
                                    ___CustomAsyncStateMachine___[MethodName] stateMachine = this; //машине присваиваем себя
                                    builder.Await[Unsafe]OnCompleted(ref [AwaiterN], ref stateMachine);
                                    return;
                                }
                                [TempAwaiter] = [AwaiterN];
                                completed = true;
                            }

                            if (completed)
                            {
                                [AwaiterN] = [TempAwaiter];
                                [TempAwaiter] = default;
                                [TempAwaiterCompletedVariable] = false;

                                //код после await
                                [VariableAwaiterBindResult][AwaiterN].GetResult();
								[CodeAfter]

                                // до следующего await, либо присваиваем результат
                                [EndComplete]
                            }
							[EndCase]";

            awaitInfo.Await = awaitInfo.Await.Replace(";", "");


            template = template.Replace("[Unsafe]", awaitInfo.AwaiterType.Contains("StepResultAwaiter") ? "" : "Unsafe");
            template = template.Replace("[MethodName]", methodName);
            template = template.Replace("[Case]", caseNum.ToString());
            template = template.Replace("[TempAwaiterCompleted]", (start ? "" : "!switched || ") + awaitInfo.TempAwaiterNameCompleted);
            template = template.Replace("[TempAwaiterCompletedVariable]", awaitInfo.TempAwaiterNameCompleted);
            template = template.Replace("[TempAwaiter]", awaitInfo.TempAwaiterName);
            template = template.Replace("[AwaiterN]", awaitInfo.LocalAwaiter);
            template = template.Replace("[AwaiterCode]", (awaitInfo.NeedThis ? "@this." : "") + awaitInfo.Await.Replace("await", "").Trim());
            template = template.Replace("[EndComplete]", !end ? $"switched = true; goto case {caseNum + 1};" : "");
            template = template.Replace("[EndCase]", !end ? $"return;" : "break;");

            // определяем awaiter будет писать данные в переменную или нет
            if (string.IsNullOrEmpty(awaitInfo.AssignmentVariable))
            {
                template = template.Replace("[VariableAwaiterBindResult]", "");
            }
            else
            {
                template = template.Replace("[VariableAwaiterBindResult]", $"{awaitInfo.AssignmentVariable}=");
            }

            var splitted = new string[0];

            if (start)
            {
                splitted = method.Split(new string[] { awaitInfo.AwaitNew }, StringSplitOptions.RemoveEmptyEntries);
                template = template.Replace("[CodeBefore]", splitted[0]);
            }
            else
            {
                template = template.Replace("[CodeBefore]", "");
            }

            if (!end)
            {
                splitted = method.Split(new string[] { awaitInfo.AwaitNew }, StringSplitOptions.RemoveEmptyEntries);
                splitted = splitted[1].Split(new string[] { next.AwaitNew }, StringSplitOptions.RemoveEmptyEntries);
                template = template.Replace("[CodeAfter]", splitted[0]);
            }
            else
            {
                splitted = method.Split(new string[] { awaitInfo.AwaitNew }, StringSplitOptions.RemoveEmptyEntries);
                var endOfMethod = splitted[1];
                var indexOfReturn = endOfMethod.LastIndexOf("return");
                var startOfEndMethod = endOfMethod.Substring(0, indexOfReturn);
                var returnPartFromMethod = endOfMethod.Replace(startOfEndMethod, "").Replace("return", "result = ");

                template = template.Replace("[CodeAfter]", startOfEndMethod + returnPartFromMethod);
            }

            return template;
        }

        public string AsyncMachineTemplate(string @namespace, string methodName, string className, string tempAwaiters, string tempVariables, string localAwaiterVariables, string asyncMethod)
        {
            var template = @"public class ___CustomAsyncStateMachine___[MethodName] : IAsyncStateMachine
        {
            public ___CustomAsyncStateMachine___[MethodName]([ClassName] @this, StepResultMethodBuilder<StepStatus> builder, int state)
            {
                this.@this = @this;
                this.builder = builder;
                this.state = state;
            }

            public [ClassName] @this;

            public StepResultMethodBuilder<StepStatus> builder;

            public int state;
			
			[TempAwaiters]
			
			[TempVariables]

            //результат работы метода
            StepStatus result = default;

            public void MoveNext()
            {
                bool switched = false; //информация о том восстанавливались мы или перешли по метке
				[LocalAwaiterVariables]
				
                bool completed = false;

                try
                {
                    switch (state)
                    {
                        [AsyncMethod]
                    }
                }
                catch (Exception exception)
                {
                    state = -2;
                    builder.SetException(exception);
                    return;
                }
                state = -2;
                builder.SetResult(result);
            }

            public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        }
";
            template = template.Replace("[MethodName]", methodName);
            template = template.Replace("[ClassName]", className);
            template = template.Replace("[TempAwaiters]", tempAwaiters);
            template = template.Replace("[TempVariables]", tempVariables);
            template = template.Replace("[LocalAwaiterVariables]", localAwaiterVariables);
            template = template.Replace("[AsyncMethod]", asyncMethod);

            return template;
        }

        public string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        private bool NeedVariable(SyntaxNode node, SyntaxNode maxTop)
        {
            if (node is ExpressionStatementSyntax)
            {
                return false;
            }
            else if (node.Parent != maxTop)
            {
                return NeedVariable(node.Parent, maxTop);
            }
            else if (node.Parent is ExpressionStatementSyntax)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private SyntaxNode FindBeforeParentBlockSyntax(SyntaxNode node)
        {
            if (node is BlockSyntax)
                return default;

            if (node.Parent == null)
                return default;

            if (node.Parent is BlockSyntax blockSyntax)
            {
                return node;
            }

            return FindBeforeParentBlockSyntax(node.Parent);
        }

        public class AwaitInfo
        {
            public string Await { get; set; }

            public string AwaitNew { get; set; }

            public string AwaiterType { get; set; }

            public string GenericType { get; set; }

            public string AssignmentVariable { get; set; }

            public bool NeedThis { get; set; }

            public string TempAwaiterName { get; set; }

            public string TempAwaiterNameCompleted { get; set; }

            public string LocalAwaiter { get; set; }
        }

    }
}