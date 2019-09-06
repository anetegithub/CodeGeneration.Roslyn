using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sample.Consumer
{
    public class StepResultAwaiter<T> : INotifyCompletion, IStepResultAwaiter where T : StepStatus
    {
        public StepResultAwaiter(StepStatus data) => Data = data;

        public bool IsCompleted => Data.MoveNext;

        public T GetResult() => (T)Data;

        public StepStatus Data { get; }

        public void OnCompleted(Action completion) => completion();
    }
}
