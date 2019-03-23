#region

using Agebull.EntityModel.Common;
using Agebull.EntityModel.Events;


#endregion

namespace MicroZero.Devops.ZeroTracer.DataAccess
{
    /// <summary>
    /// 本地数据库
    /// </summary>
    public partial class ZeroTracerDb
    {
        /// <summary>
        /// 构造
        /// </summary>
        static ZeroTracerDb()
        {
            DataUpdateHandler.RegisterUpdateHandler(new MySqlDataTrigger());
        }

        /// <summary>
        /// 构造
        /// </summary>
        public ZeroTracerDb()
        {
            Name = @"ZeroTracer";
            Caption = @"数据跟踪";
            Description = @"数据跟踪";
            Initialize();
            //RegistToEntityPool();
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize();
    }
}