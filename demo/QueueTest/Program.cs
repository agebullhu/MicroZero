using Agebull.MicroZero;
using ApiTest;


namespace QueueTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<QueueDemo>();
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
