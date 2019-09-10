using Sample.Consumer;
using System;
using System.Threading.Tasks;

namespace Sample.Run
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = Run();
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }

        static async Task Run()
        {
            var c = new Class2();
            var x = await c.RunAsync();
        }
    }
}
