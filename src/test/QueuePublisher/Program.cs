using System;
using Agebull.Common.Configuration;
using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;

namespace QueuePublisher
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
