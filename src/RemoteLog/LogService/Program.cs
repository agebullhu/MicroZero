using Agebull.ZeroNet.Core;
using System;

namespace Agebull.ZeroNet.LogService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ZeroApplication.Initialize();
            ZeroApplication.Discove();
            ZeroApplication.RunAwaite();
        }
    }
}
