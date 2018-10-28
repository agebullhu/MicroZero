using System.Diagnostics;
using System.IO;
using System.Windows;
using Agebull.Common.Configuration;
using Agebull.ZeroNet.Core;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App
    {
        public App()
        {
            WorkContext.SynchronousContext = new DispatcherSynchronousContext
            {
                Dispatcher = Dispatcher
            };
            Trace.Listeners.Add(new MessageTraceListener());
            ConfigurationManager.BasePath = Path.GetDirectoryName(typeof(App).Assembly.Location);
            ZeroApplication.CheckOption();
            ZeroApplication.Initialize();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ZeroApplication.Shutdown();
            base.OnExit(e);
        }
    }
}