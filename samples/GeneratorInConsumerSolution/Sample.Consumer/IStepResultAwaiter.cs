using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Consumer
{
    public interface IStepResultAwaiter
    {
        bool IsCompleted { get; }

        StepStatus Data { get; }
    }
}
