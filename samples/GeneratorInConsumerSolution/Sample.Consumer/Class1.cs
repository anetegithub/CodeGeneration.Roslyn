using System;
using System.Runtime.CompilerServices;
 
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

    public interface IStepResultAwaiter
    {
        bool IsCompleted { get; }

        StepStatus Data { get; }
    }

    public class StepResultAwaiter<T> : INotifyCompletion, IStepResultAwaiter where T : StepStatus
    {
        public StepResultAwaiter(StepStatus data) => Data = data;

        public bool IsCompleted => Data.MoveNext;

        public T GetResult() => (T)Data;

        public StepStatus Data { get; }

        public void OnCompleted(Action completion) => completion();
    }

    public class StepStatus
    {
        public bool MoveNext => !Completed;

        internal bool Completed { get; set; }

        public StepStatus()
        {
        }

        public bool Proceed { get; set; }

        public object OutcomeValue { get; set; }

        public StepStatus(object outcome)
        {
            Proceed = true;
            OutcomeValue = outcome;
        }
    }

    public class StepResultMethodBuilder<T> where T : StepStatus
    {
        public static StepResultMethodBuilder<T> Create() => new StepResultMethodBuilder<T>();

        private IAsyncStateMachine _asyncStateMachine;

        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (Task == null || Task.MoveNext)
            {
                stateMachine.MoveNext();
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
            var stateAwaiter = awaiter as IStepResultAwaiter;

            if (stateAwaiter.IsCompleted)
            {
                var foo = stateMachine;
                awaiter.OnCompleted(() => { foo.MoveNext(); });
            }
            else
            {
                Task = new StepState<T>(stateAwaiter.Data);
            }
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
            ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine

        {
            var await = awaiter as INotifyCompletion;
            AwaitOnCompleted(ref await, ref stateMachine);
        }

        public StepState<T> Task { get; private set; }
    }

    public partial class Class2
    {
        public async StepState<StepStatus> RunAsync()
        {
            await WaitEvent();

            return await Next;
        }

        private StepState<StepStatus> WaitEvent()
        {
            return new StepState<StepStatus>(new StepStatus()
            {
            });
        }

        private static StepState<StepStatus> Next => new StepState<StepStatus>(new StepStatus() { OutcomeValue = 5 });

    }
}