using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sample.Consumer
{
    public class CustomAsyncMachineFactory
    {
        IAsyncStateMachine generatedAsyncStateMachine;

        public CustomAsyncMachineFactory(IAsyncStateMachine generatedAsyncStateMachine)
        {
            this.generatedAsyncStateMachine = generatedAsyncStateMachine;
        }

        public IAsyncStateMachine CreateCustomAsyncStateMachine(StepResultMethodBuilder<StepStatus> builder)
        {
            var stateMachineName = ExtractStateMachineName(generatedAsyncStateMachine);
            var classContained = ExtractClassContained(generatedAsyncStateMachine);
            var stateMachineType = FindCustomStateMachineType(classContained, stateMachineName);
            var targetThis = ExtractTarget(generatedAsyncStateMachine);

            return (IAsyncStateMachine)Activator
                .CreateInstance(stateMachineType,
                targetThis, builder, -1);
        }

        private object ExtractTarget(IAsyncStateMachine generatedStateMachine)
            => generatedStateMachine.GetType().GetFields()
                .FirstOrDefault(f => f.Name.Contains("this"))
                ?.GetValue(generatedStateMachine);

        private Type FindCustomStateMachineType(string className, string methodName) => Type.GetType($"{className}+___CustomAsyncStateMachine___{methodName}");

        private string ExtractClassContained(IAsyncStateMachine generatedStateMachine)
            => generatedStateMachine.GetType()
            .FullName
            .Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries)[0];

        private string ExtractStateMachineName(IAsyncStateMachine generatedStateMachine)
            => generatedStateMachine.GetType()
            .Name
            .Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries)[0]
            .Replace("<", "");
    }
}