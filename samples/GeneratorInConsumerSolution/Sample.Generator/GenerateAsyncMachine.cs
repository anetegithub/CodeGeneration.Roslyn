using CodeGeneration.Roslyn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Generator
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    [CodeGenerationAttribute(typeof(AsyncMachineGenerator))]
    public class GenerateAsyncMachineAttribute : Attribute
    {
    }
}
