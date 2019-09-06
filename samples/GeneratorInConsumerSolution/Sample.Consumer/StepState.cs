using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sample.Consumer
{
    [AsyncMethodBuilder(typeof(StepResultMethodBuilder<>))]
    public class StepState<T> where T : StepStatus
    {
        public StepState()
        {
        }

        private readonly StepStatus _data;

        public StepState(T data) => _data = data;

        public StepState(StepStatus executionResult) => _data = executionResult;

        public StepResultAwaiter<T> GetAwaiter() => new StepResultAwaiter<T>(_data);

        public bool MoveNext => _data.MoveNext;
    }
}
