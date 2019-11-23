using Newtonsoft.Json;
using System;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            int _inCheckTask = 0;

            Console.WriteLine(Interlocked.Increment(ref _inCheckTask));
            Console.WriteLine(Interlocked.Increment(ref _inCheckTask));
            Console.WriteLine(Interlocked.Increment(ref _inCheckTask));
            Console.WriteLine(Interlocked.Decrement(ref _inCheckTask));
            Console.WriteLine(Interlocked.Decrement(ref _inCheckTask));
            Console.WriteLine(Interlocked.Decrement(ref _inCheckTask));
            Console.ReadKey();
        }
    }
}
