using Agebull.MicroZero;
using System;
using System.Reflection;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(Assembly.GetExecutingAssembly());
            ZeroApplication.Initialize();

            ZeroApplication.RunAwaite();
        }
    }
}
