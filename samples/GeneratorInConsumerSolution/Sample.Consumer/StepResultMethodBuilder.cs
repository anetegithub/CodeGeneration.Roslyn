using System;
using System.Runtime.CompilerServices;

namespace Sample.Consumer
{
    public class StepResultMethodBuilder<T> where T: StepStatus
    {
        public static StepResultMethodBuilder<T> Create() => new StepResultMethodBuilder<T>();

        private IAsyncStateMachine _asyncStateMachine;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            var factory = new CustomAsyncMachineFactory(stateMachine);
            var customStateMachine = factory.CreateCustomAsyncStateMachine(this as StepResultMethodBuilder<StepStatus>);
            
            if (Task == null || Task.MoveNext)
            {
                customStateMachine.MoveNext();
            }
        }        

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            _asyncStateMachine = stateMachine;
        }

        public void SetException(Exception exception)
        {
            throw exception;
        }

        public void SetResult(T result) => Task = new StepState<T>(result);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            var foo = stateMachine;

            if (awaiter is IStepResultAwaiter stateAwaiter)
            {
                if (stateAwaiter.IsCompleted)
                {
                    awaiter.OnCompleted(() => { foo.MoveNext(); });
                }
                else
                {
                    Task = new StepState<T>(stateAwaiter.Data);
                }
            }
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine

        {
            var foo = stateMachine;
            awaiter.OnCompleted(() => { foo.MoveNext(); });
        }

        public StepState<T> Task { get; private set; }
    }
}
