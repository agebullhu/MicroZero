#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

using Agebull.Common;
using Agebull.Common.DataModel;
using Agebull.Common.WebApi;
using Gboxt.Common.DataModel;
using Gboxt.Common.DataModel.MySql;


#endregion

namespace ZeroNet.Devops.ZeroTracer.DataAccess
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