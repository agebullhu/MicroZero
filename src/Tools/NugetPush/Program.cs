using System;
using System.Diagnostics;
using System.IO;
using Agebull.Common;

namespace NugetPush
{
    class Program
    {
        private static string curPath;
        static void Main(string[] args)
        {
            curPath = "C:\\Projects\\Agebull";//Environment.CurrentDirectory;
            var pkgs = IOHelper.GetAllFiles(Path.Combine(curPath, "nuget"), "nupkg");
            foreach (var file in pkgs)
            {
                Push(file);
                File.Move(file, Path.Combine(curPath, "olds", Path.GetFileName(file)));
            }
        }

        static void Push(string file)
        {
            var arg = $"push -Source http://47.98.229.139:8080 -ApiKey ee28314c-f7fe-2550-bd77-e09eda3d0119 {file}";

            var process = Process.Start(Path.Combine(curPath, "Nuget.exe"), arg);
            process.OutputDataReceived += Process_OutputDataReceived;
            process.WaitForExit();
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
