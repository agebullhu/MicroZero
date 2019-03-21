/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2019/1/17 17:18:44*/
#region


using Agebull.EntityModel.MySql;


#endregion

namespace MicroZero.Devops.ZeroTracer.DataAccess
{
    /// <summary>
    /// 以流程方式记录日志
    /// </summary>
    sealed partial class FlowLogDataAccess : MySqlTable<FlowLogData,ZeroTracerDb>
    {

    }
}