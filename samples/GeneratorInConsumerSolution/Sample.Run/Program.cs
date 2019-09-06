using Sample.Consumer;
using System;
using System.Threading.Tasks;

namespace Sample.Run
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        static void Run()
        {
            var c = new Class2();
            _ = c.RunAsync().GetAwaiter().GetResult();
        }
    }
}
