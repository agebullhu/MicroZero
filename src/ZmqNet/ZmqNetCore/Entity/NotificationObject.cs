// /*****************************************************
// (c)2008-2017 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-12-09
// 修改:2014-12-09
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

#endregion

namespace Agebull.Common.DataModel
{
    public class DictionaryItem
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class DictionaryItem<TValue>
    {
        public string Name { get; set; }
        public TValue Value { get; set; }
    }
}