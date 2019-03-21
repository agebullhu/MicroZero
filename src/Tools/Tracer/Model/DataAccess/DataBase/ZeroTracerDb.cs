#region
using Agebull.Common.Configuration;
using Agebull.EntityModel.MySql;

#endregion

namespace MicroZero.Devops.ZeroTracer.DataAccess
{
    /// <summary>
    /// 本地数据库
    /// </summary>
    sealed partial class ZeroTracerDb : MySqlDataBase
    {
        
        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize()
        {
        }
        
        /// <summary>
        /// 生成缺省数据库
        /// </summary>
        public static void CreateDefault()
        {
            Default = new ZeroTracerDb();
        }

        /// <summary>
        /// 缺省强类型数据库
        /// </summary>
        public static ZeroTracerDb Default
        {
            get;
            set;
        }

        /// <summary>
        /// 读取连接字符串
        /// </summary>
        /// <returns></returns>
        protected override string LoadConnectionStringSetting()
        {
            return ConfigurationManager.ConnectionStrings["ZeroTrace"];
        }
    }
}