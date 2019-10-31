using System;
using Agebull.MicroZero;



namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(TestController).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }

}
