using Newtonsoft.Json;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = new System.Collections.Generic.Dictionary<string, int>
            {
                {"a",1 },
                {"b",1 }
            };
            Console.WriteLine(JsonConvert.SerializeObject(dir));
            Console.ReadKey();
        }
    }
}
