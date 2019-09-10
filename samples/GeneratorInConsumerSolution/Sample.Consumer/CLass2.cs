using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sample.Consumer
{
    public static class ExternalClass
    {
        public static async Task<int> ExternalCall()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            return await Task<int>.FromResult(5);
        }

        public static Task<int> ExternalCall2 => Task<int>.FromResult(5);
    }
    public partial class Class2
    {
        public async StepState<StepStatus> RunAsync()
        {
            Console.WriteLine("before await 1");

            await WaitEvent();

            Console.WriteLine("after await 1");

            Console.WriteLine("before await 2");

            var x = await ExternalClass.ExternalCall();

            Console.WriteLine("after await 2");

            Console.WriteLine("before await 3");

            var VARIABLE = await Next;
            VARIABLE.Completed = x == 5;

            var z = await Next;
            await ExternalClass.ExternalCall2;

            Console.WriteLine("after await 3");

            return VARIABLE;
        }

        private StepState<StepStatus> WaitEvent()
        {
            return new StepState<StepStatus>(new StepStatus()
            {
                OutcomeValue="WaitEvent"
            });
        }

        private static StepState<StepStatus> Next => new StepState<StepStatus>(new StepStatus() { OutcomeValue = 5 });
        
        //public class ___CustomGeneratedAsyncStateMachine___RunAsync : IAsyncStateMachine
        //{
        //    public ___CustomGeneratedAsyncStateMachine___RunAsync(Class2 @this, StepResultMethodBuilder<StepStatus> builder, int state)
        //    {
        //        this.__4__this = @this;
        //        __t__builder = builder;
        //        __1__state = state;
        //    }

        //    public int __1__state;

        //    public StepResultMethodBuilder<StepStatus> __t__builder;

        //    public Class2 __4__this;

        //    private StepStatus __s__1;

        //    private object __u__1;

        //    private void MoveNext()
        //    {
        //        int num = __1__state;
        //        StepStatus result;
        //        try
        //        {
        //            StepResultAwaiter<StepStatus> awaiter4;
        //            StepResultAwaiter<StepStatus> awaiter3;
        //            StepResultAwaiter<StepStatus> awaiter2;
        //            StepResultAwaiter<StepStatus> awaiter;
        //            switch (num)
        //            {
        //                default:
        //                    awaiter4 = __4__this.WaitEvent().GetAwaiter();
        //                    if (!awaiter4.IsCompleted)
        //                    {
        //                        num = (__1__state = 0);
        //                        __u__1 = awaiter4;
        //                        ___CustomGeneratedAsyncStateMachine___RunAsync stateMachine = this;
        //                        __t__builder.AwaitOnCompleted(ref awaiter4, ref stateMachine);
        //                        return;
        //                    }
        //                    goto IL_008d;
        //                case 0:
        //                    awaiter4 = (StepResultAwaiter<StepStatus>)__u__1;
        //                    __u__1 = null;
        //                    num = (__1__state = -1);
        //                    goto IL_008d;
        //                case 1:
        //                    awaiter3 = (StepResultAwaiter<StepStatus>)__u__1;
        //                    __u__1 = null;
        //                    num = (__1__state = -1);
        //                    goto IL_00ee;
        //                case 2:
        //                    awaiter2 = (StepResultAwaiter<StepStatus>)__u__1;
        //                    __u__1 = null;
        //                    num = (__1__state = -1);
        //                    goto IL_0150;
        //                case 3:
        //                    {
        //                        awaiter = (StepResultAwaiter<StepStatus>)__u__1;
        //                        __u__1 = null;
        //                        num = (__1__state = -1);
        //                        break;
        //                    }
        //                    IL_00ee:
        //                    awaiter3.GetResult();
        //                    awaiter2 = Next.GetAwaiter();
        //                    if (!awaiter2.IsCompleted)
        //                    {
        //                        num = (__1__state = 2);
        //                        __u__1 = awaiter2;
        //                        ___CustomGeneratedAsyncStateMachine___RunAsync stateMachine = this;
        //                        __t__builder.AwaitOnCompleted(ref awaiter2, ref stateMachine);
        //                        return;
        //                    }
        //                    goto IL_0150;
        //                    IL_008d:
        //                    awaiter4.GetResult();
        //                    awaiter3 = Next.GetAwaiter();
        //                    if (!awaiter3.IsCompleted)
        //                    {
        //                        num = (__1__state = 1);
        //                        __u__1 = awaiter3;
        //                        ___CustomGeneratedAsyncStateMachine___RunAsync stateMachine = this;
        //                        __t__builder.AwaitOnCompleted(ref awaiter3, ref stateMachine);
        //                        return;
        //                    }
        //                    goto IL_00ee;
        //                    IL_0150:
        //                    awaiter2.GetResult();
        //                    awaiter = Next.GetAwaiter();
        //                    if (!awaiter.IsCompleted)
        //                    {
        //                        num = (__1__state = 3);
        //                        __u__1 = awaiter;
        //                        ___CustomGeneratedAsyncStateMachine___RunAsync stateMachine = this;
        //                        __t__builder.AwaitOnCompleted(ref awaiter, ref stateMachine);
        //                        return;
        //                    }
        //                    break;
        //            }
        //            __s__1 = awaiter.GetResult();
        //            result = __s__1;
        //        }
        //        catch (Exception exception)
        //        {
        //            __1__state = -2;
        //            __t__builder.SetException(exception);
        //            return;
        //        }
        //        __1__state = -2;
        //        __t__builder.SetResult(result);
        //    }

        //    void IAsyncStateMachine.MoveNext()
        //    {
        //        //ILSpy generated this explicit interface implementation from .override directive in MoveNext
        //        this.MoveNext();
        //    }

        //    [DebuggerHidden]
        //    private void SetStateMachine(IAsyncStateMachine stateMachine)
        //    {
        //    }

        //    void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
        //    {
        //        //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
        //        this.SetStateMachine(stateMachine);
        //    }
        //}

        //public class ___CustomAsyncStateMachine___ : IAsyncStateMachine
        //{
        //    public ___CustomAsyncStateMachine___(Class2 @this, StepResultMethodBuilder<StepStatus> builder, int state)
        //    {
        //        this.@this = @this;
        //        this.builder = builder;
        //        this.state = state;
        //    }

        //    public Class2 @this;

        //    public StepResultMethodBuilder<StepStatus> builder;

        //    public int state;

        //    //пары переменных временных значений awaiter'ов
        //    // первая переменная сам awaiter, вторая информация о том установлен он или нет, т.к. не каждую struct можно проверить на default
        //    StepResultAwaiter<StepStatus> tempAwaiter__oftype_stepresultawaiter_stepstatus;
        //    bool tempAwaiter__oftype_stepresultawaiter_stepstatus_completed;

        //    //ещё одна пара временных значений awaiter'ов
        //    TaskAwaiter<int> tempAwaiter__oftype__taskawaiter_int;
        //    bool tempAwaiter__oftype__taskawaiter_int_completed;

        //    // переменные в методе выведенные в состояние которое может сохраняться
        //    int x__state = default;
        //    StepStatus VARIABLE__state = default;

        //    //результат работы метода
        //    StepStatus result = default;

        //    public void MoveNext()
        //    {
        //        //локальные переменные метода
        //        StepResultAwaiter<StepStatus> awaiter1;
        //        TaskAwaiter<int> awaiter2;
        //        StepResultAwaiter<StepStatus> awaiter3;
        //        bool completed = false;

        //        try
        //        {
        //            switch (state)
        //            {
        //                case -1:
        //                    completed = tempAwaiter__oftype_stepresultawaiter_stepstatus_completed;
        //                    if (!completed)
        //                    {
        //                        // код вначале метода
        //                        Console.WriteLine("before await 1");

        //                        //первый await
        //                        awaiter1 = @this.WaitEvent().GetAwaiter();
        //                        if (!awaiter1.IsCompleted)
        //                        {
        //                            tempAwaiter__oftype_stepresultawaiter_stepstatus = awaiter1; //запоминаем темповую переменную
        //                            ___CustomAsyncStateMachine___ stateMachine = default; //машине присваиваем себя
        //                            builder.AwaitOnCompleted(ref awaiter1, ref stateMachine);
        //                            return;
        //                        }
        //                        completed = true;
        //                    }

        //                    if (completed)
        //                    {
        //                        awaiter1 = tempAwaiter__oftype_stepresultawaiter_stepstatus;
        //                        tempAwaiter__oftype_stepresultawaiter_stepstatus = default;
        //                        tempAwaiter__oftype_stepresultawaiter_stepstatus_completed = false;

        //                        //код после await
        //                        awaiter1.GetResult();
        //                        Console.WriteLine("after await 1");
        //                        Console.WriteLine("before await 2");

        //                        // до следующего await
        //                        goto case 0;
        //                    }

        //                    return;
        //                case 0:
        //                    completed = tempAwaiter__oftype__taskawaiter_int_completed;
        //                    if (!completed)
        //                    {
        //                        //код перед await уже выполнен, тут только код await'а
        //                        awaiter2 = ExternalClass.ExternalCall.GetAwaiter();
        //                        if (!awaiter2.IsCompleted)
        //                        {
        //                            state++; //перемещаемся по машине состояний до этого лейбла
        //                            tempAwaiter__oftype__taskawaiter_int = awaiter2; //запоминаем темповую переменную
        //                            ___CustomAsyncStateMachine___ stateMachine = default; //машине присваиваем себя
        //                            builder.AwaitUnsafeOnCompleted(ref awaiter2, ref stateMachine);
        //                            return;
        //                        }
        //                        completed = true;
        //                    }

        //                    if (completed)
        //                    {
        //                        awaiter2 = tempAwaiter__oftype__taskawaiter_int;
        //                        tempAwaiter__oftype__taskawaiter_int = default;
        //                        tempAwaiter__oftype__taskawaiter_int_completed = false;

        //                        //код после второго await
        //                        x__state = awaiter2.GetResult();
        //                        Console.WriteLine("after await 2");
        //                        Console.WriteLine("before await 3");

        //                        // до следующего await
        //                        goto case 1;
        //                    }

        //                    return;
        //                case 1:
        //                    completed = tempAwaiter__oftype_stepresultawaiter_stepstatus_completed;
        //                    if (!completed)
        //                    {
        //                        //код перед нашим await уже выполнен, тут только код await'а                        
        //                        awaiter3 = Next.GetAwaiter();
        //                        if (!awaiter3.IsCompleted)
        //                        {
        //                            state++; //перемещаемся по машине состояний до этого лейбла
        //                            tempAwaiter__oftype_stepresultawaiter_stepstatus = awaiter3; //запоминаем темповую переменную
        //                            ___CustomAsyncStateMachine___ stateMachine = default; //машине присваиваем себя
        //                            builder.AwaitOnCompleted(ref awaiter3, ref stateMachine);
        //                            return;
        //                        }
        //                        completed = true;
        //                    }

        //                    if (completed)
        //                    {
        //                        awaiter3 = tempAwaiter__oftype_stepresultawaiter_stepstatus;
        //                        tempAwaiter__oftype_stepresultawaiter_stepstatus = default;
        //                        tempAwaiter__oftype_stepresultawaiter_stepstatus_completed = false;

        //                        //код после await
        //                        VARIABLE__state = awaiter3.GetResult();
        //                        VARIABLE__state.Completed = x__state == 5;
        //                        Console.WriteLine("after await 3");

        //                        // формируем return
        //                        result = VARIABLE__state;
        //                    }
        //                    break; //у последнего формируем break
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            state = -2;
        //            builder.SetException(exception);
        //            return;
        //        }
        //        state = -2;
        //        builder.SetResult(result);
        //    }

        //    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
        //}
    }
}