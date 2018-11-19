using System;
using System.Threading;
using ZeroMQ;

namespace ProxyTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ZContext.Initialize();
            while (true)
            {
                var key =Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    return;
                Console.WriteLine("Create");
                var socket = new ZSocket(ZSocketType.PUSH);
                socket.Bind("tcp://*:10000");
                var socket2 = new ZSocket(ZSocketType.PULL);
                socket2.Bind("tcp://*:10001");
                DateTime now = DateTime.Now;
                socket.Send(new ZFrame("Test"));
                var msg = socket2.ReceiveMessage();
                
                Console.WriteLine(DateTime.Now - now);
                foreach (var frame in msg)
                {
                    Console.WriteLine(frame.ReadString());
                }

                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Q)
                    return;
                socket.Dispose();

                socket2.Dispose();
                Console.WriteLine("Dispose");
            }
        }
    }
}
