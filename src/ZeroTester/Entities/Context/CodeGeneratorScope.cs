using System;
using Agebull.Common.Base;

namespace Agebull.EntityModel
{
    /// <summary>
    /// 代码生成基类
    /// </summary>
    public class CodeGeneratorScope : ScopeBase
    {
        readonly IDisposable _innerScope;
        CodeGeneratorScope(NotificationObject config)
        {
            _innerScope = WorkModelScope.CreateScope(WorkModel.Coder);
            if (config == null)
                return;
            GlobalTrigger.OnCodeGeneratorBegin(config);
        }
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <returns></returns>
        public static IDisposable CreateScope(NotificationObject config)
        {
            return new CodeGeneratorScope(config);
        }

        /// <inheritdoc />
        protected override void OnDispose()
        {
            if (_innerScope == null)
                return;
            _innerScope.Dispose();
            GlobalTrigger.OnCodeGeneratorEnd();
        }
    }
}