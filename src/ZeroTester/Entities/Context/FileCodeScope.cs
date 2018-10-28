using System;
using System.Collections.Generic;
using Agebull.Common.Base;

namespace Agebull.EntityModel
{
    /// <summary>
    /// 文件代码生成器的范围
    /// </summary>
    public class FileCodeScope : ScopeBase
    {
        readonly Dictionary<string, string> _codes;
        private readonly bool? _writeToFile;
        FileCodeScope(bool noWrite)
        {
            _codes = WorkContext.codes;
            _writeToFile = WorkContext.writeToFile;
            WorkContext.writeToFile = !noWrite;
            WorkContext.codes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        /// <summary>
        /// 生成范围
        /// </summary>
        /// <returns></returns>
        public static IDisposable CreateScope(bool noWrite)
        {
            return new FileCodeScope(noWrite);
        }

        protected override void OnDispose()
        {
            WorkContext.codes = _codes;
            WorkContext.writeToFile = _writeToFile;
        }
    }
}