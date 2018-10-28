using System;
using Agebull.Common.Base;

namespace Agebull.EntityModel
{
    /// <summary>
    /// 工作模式范围
    /// </summary>
    public class WorkModelScope : ScopeBase
    {
        readonly WorkModel _workModel;
        WorkModelScope(WorkModel model)
        {
            _workModel = WorkContext.WorkModel;
            WorkContext._workModel = model;
        }
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static IDisposable CreateScope(WorkModel model)
        {
            return new WorkModelScope(model);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            WorkContext._workModel = _workModel;
        }

    }
}