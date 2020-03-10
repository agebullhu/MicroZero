using Agebull.MicroZero;

namespace ZeroService
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
