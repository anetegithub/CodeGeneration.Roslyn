using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Consumer
{
    public class Example
    {
        public async Task<bool> RunAsync()
        {
            Console.WriteLine("before await 1");

            await TaskVoid;

            Console.WriteLine("after await 1");
            Console.WriteLine("before await 2");

            var intValue = await TaskWithResult;

            Console.WriteLine("after await 2");
            Console.WriteLine("before await 3");

            var i = await ExternalTaskSource.ExternalTask;

            Console.WriteLine("after await 3");
            Console.WriteLine("before await 4");

            await TaskVoid;

            Console.WriteLine("after await 4");
            Console.WriteLine("before return");

            return intValue > 1 + i;
        }

        private Task<int> TaskWithResult => Task.FromResult(1);
        private Task TaskVoid => Task.CompletedTask;

        [CompilerGenerated]
        private sealed class RunAsyncStateMachine : IAsyncStateMachine
        {
            public int state;

            public AsyncTaskMethodBuilder<bool> builder;

            public Example @this;

            private int intValue___closure;

            private int i___closure;

            private int temp_s3;

            private int temp_s4;

            private TaskAwaiter awaiter1;

            private TaskAwaiter<int> awaiter2;

            private void MoveNext()
            {
                int num = state;
                bool result;
                try
                {
                    TaskAwaiter awaiter4;
                    TaskAwaiter<int> awaiter3;
                    TaskAwaiter<int> awaiter2;
                    TaskAwaiter awaiter;
                    switch (num)
                    {
                        default:
                            Console.WriteLine("before await 1");
                            awaiter4 = @this.TaskVoid.GetAwaiter();
                            if (!awaiter4.IsCompleted)
                            {
                                num = (state = 0);
                                awaiter1 = awaiter4;
                                RunAsyncStateMachine stateMachine = this;
                                builder.AwaitUnsafeOnCompleted(ref awaiter4, ref stateMachine);
                                return;
                            }
                            goto IL_0099;
                        case 0:
                            awaiter4 = awaiter1;
                            awaiter1 = default(TaskAwaiter);
                            num = (state = -1);
                            goto IL_0099;
                        case 1:
                            awaiter3 = awaiter2;
                            awaiter2 = default(TaskAwaiter<int>);
                            num = (state = -1);
                            goto IL_0117;
                        case 2:
                            awaiter2 = awaiter2;
                            awaiter2 = default(TaskAwaiter<int>);
                            num = (state = -1);
                            goto IL_01a0;
                        case 3:
                            {
                                awaiter = awaiter1;
                                awaiter1 = default(TaskAwaiter);
                                num = (state = -1);
                                break;
                            }
                            IL_0117:
                            temp_s3 = awaiter3.GetResult();
                            intValue___closure = temp_s3;
                            Console.WriteLine("after await 2");
                            Console.WriteLine("before await 3");
                            awaiter2 = ExternalTaskSource.ExternalTask.GetAwaiter();
                            if (!awaiter2.IsCompleted)
                            {
                                num = (state = 2);
                                awaiter2 = awaiter2;
                                RunAsyncStateMachine stateMachine = this;
                                builder.AwaitUnsafeOnCompleted(ref awaiter2, ref stateMachine);
                                return;
                            }
                            goto IL_01a0;
                            IL_0099:
                            awaiter4.GetResult();
                            Console.WriteLine("after await 1");
                            Console.WriteLine("before await 2");
                            awaiter3 = @this.TaskWithResult.GetAwaiter();
                            if (!awaiter3.IsCompleted)
                            {
                                num = (state = 1);
                                awaiter2 = awaiter3;
                                RunAsyncStateMachine stateMachine = this;
                                builder.AwaitUnsafeOnCompleted(ref awaiter3, ref stateMachine);
                                return;
                            }
                            goto IL_0117;
                            IL_01a0:
                            temp_s4 = awaiter2.GetResult();
                            i___closure = temp_s4;
                            Console.WriteLine("after await 3");
                            Console.WriteLine("before await 4");
                            awaiter = @this.TaskVoid.GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                num = (state = 3);
                                awaiter1 = awaiter;
                                RunAsyncStateMachine stateMachine = this;
                                builder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
                                return;
                            }
                            break;
                    }
                    awaiter.GetResult();
                    Console.WriteLine("after await 4");
                    Console.WriteLine("before return");
                    result = (intValue___closure > 1 + i___closure);
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

            void IAsyncStateMachine.MoveNext()
            {
                //ILSpy generated this explicit interface implementation from .override directive in MoveNext
                this.MoveNext();
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
            }

            void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
                this.SetStateMachine(stateMachine);
            }
        }


    }


    public static class ExternalTaskSource
    {
        public static Task<int> ExternalTask => Task.FromResult(1);
    }
}
