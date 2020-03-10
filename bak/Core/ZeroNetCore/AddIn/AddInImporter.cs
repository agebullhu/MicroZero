using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Agebull.Common;
using Agebull.Common.Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    /// MEF插件导入器
    /// </summary>
    internal class AddInImporter
    {
        /// <summary>
        /// 实例对象
        /// </summary>
        internal static AddInImporter Instance;

        /// <summary>
        /// 插件对象
        /// </summary>
        [ImportMany(typeof(IAutoRegister))]
        public IEnumerable<IAutoRegister> Registers { get; set; }

        /// <summary>
        /// 导入
        /// </summary>
        public static void Importe()
        {
            if (Instance != null)
                return;
            Instance = new AddInImporter();
            IocHelper.ServiceCollection.AddSingleton(pro => Instance);
            CheckSystemAddIn();
            CheckAddIn();
        }

        static void CheckSystemAddIn()
        {
            // 通过容器对象将宿主和部件组装到一起。 
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(ZeroApplication.Config.BinPath, "*.sys_addin.dll");
            var container = new CompositionContainer(directoryCatalog);
            container.ComposeParts(Instance);
            foreach (var reg in Instance.Registers)
            {
                ZeroTrace.SystemLog("AddIn(System)", reg.GetType().Assembly.FullName);
            }
        }

        static void CheckAddIn()
        {
            if (string.IsNullOrEmpty(ZeroApplication.Config.AddInPath))
                return;

            var path = ZeroApplication.Config.AddInPath[0] == '/'
                ? ZeroApplication.Config.AddInPath
                : IOHelper.CheckPath(ZeroApplication.Config.RootPath, ZeroApplication.Config.AddInPath);
            ZeroTrace.SystemLog("AddIn(Service)", path);
            // 通过容器对象将宿主和部件组装到一起。 
            DirectoryCatalog directoryCatalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(directoryCatalog);
            container.ComposeParts(Instance);
            foreach (var reg in Instance.Registers)
            {
                ZeroTrace.SystemLog("AddIn(Extend)", reg.GetType().Assembly.FullName);
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            foreach (var reg in Registers)
            {
                reg.Initialize();
            }
        }

        /// <summary>
        /// 执行自动注册
        /// </summary>
        public void AutoRegist()
        {
            foreach (var reg in Registers)
                reg.AutoRegist();
        }

    }
}