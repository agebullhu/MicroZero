using Agebull.CodeRefactor.CodeRefactor;

namespace Agebull.EntityModel
{
    /// <summary>
    ///     MVVM的Model的基类
    /// </summary>
    public abstract class TraceModelBase : ModelBase
    {

        private TraceModel _trace;

        /// <summary>
        /// 当前选中对象的消息跟踪器
        /// </summary>
        public TraceModel CurrentTrace => _trace ?? (_trace = new TraceModel());
    }
}