using Agebull.MicroZero;
using Agebull.MicroZero.LogService;

namespace ZeroService
{
    class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.RegistZeroObject<RemoteLogStation>();
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
