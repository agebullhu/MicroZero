using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskTest
{
    class Program
    {
        private static Thread thread;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var task = Task.Factory.StartNew(Test);
            task.ContinueWith(t =>
            {
                Console.WriteLine("Close");
            });
            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Q)
                {
                    thread.Interrupt();
                    break;
                }
            }

            Console.WriteLine("Bye");
            Console.ReadKey();
        }

        static void Test()
        {
            thread = Thread.CurrentThread;
            for(int i=0;i>=0;i++)
                Console.Write(i);
        }
    }
}
