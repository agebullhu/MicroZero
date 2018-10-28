// /*****************************************************
// (c)2008-2013 Copy right www.Gboxt.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-26
// 修改:2014-12-07
// *****************************************************/

#region 引用

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Agebull.EntityModel.Config;

#endregion

namespace Agebull.Common.Mvvm
{
    ///// <summary>
    /////     表示一个命令集合的节点
    ///// </summary>
    //public class ActionItem : CommandConfig, ICommandItemBuilder
    //{

    //    //固定参数
    //    public Dictionary<string, object> FixArguments = new Dictionary<string, object>();

    //    /// <summary>
    //    ///     动作开始
    //    /// </summary>
    //    public Func<object, bool> Begin { get; set; }

    //    /// <summary>
    //    ///     实际操作
    //    /// </summary>
    //    public Action<RuntimeActionItem, object> Action { get; set; }

    //    /// <summary>
    //    ///     动作结束
    //    /// </summary>
    //    public Action<CommandStatus, Exception, bool> End { get; set; }


    //    private ImageSource _image;


    //    /// <summary>
    //    ///     图标
    //    /// </summary>
    //    public virtual ImageSource Image
    //    {
    //        get => _image ?? (IconName == null ? null : Application.Current.Resources[IconName] as BitmapImage);
    //        set => _image = value;
    //    }

    //    CommandItemBase ICommandItemBuilder.ToCommand(object arg, Func<object, IEnumerator> enumerator)
    //    {
    //        var item = new RuntimeActionItem
    //        {
    //            Action = this,
    //            Parameter = arg,
    //            ToEnumerator = enumerator
    //        };
    //        var r2 = new AsyncCommandItem<object, bool>(item.Prepare, item.Run, item.End);
    //        r2.CopyFrom(this);
    //        r2.Image = Image;
    //        r2.Source = arg;
    //        return r2;
    //    }
    //}
}
