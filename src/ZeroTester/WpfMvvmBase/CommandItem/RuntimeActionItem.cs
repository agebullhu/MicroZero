using System;
using System.Collections;
using System.Windows;
using Agebull.EntityModel;

namespace Agebull.Common.Mvvm
{
    ///// <summary>
    /////     表示一个命令集合的节点
    ///// </summary>
    //public class RuntimeActionItem : NotificationObject
    //{
    //    /// <summary>
    //    ///     动作结束
    //    /// </summary>
    //    public ActionItem Action { get; set; }

    //    public Func<object, IEnumerator> ToEnumerator { get; set; }

    //    /// <summary>
    //    ///     对应的命令参数
    //    /// </summary>
    //    public object Parameter
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// 执行
    //    /// </summary>
    //    public bool Prepare(object arg)
    //    {
    //        if (!NoConfirm && MessageBox.Show($"确认要执行{Action.Caption}吗?", "对象编辑", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
    //        {
    //            return false;
    //        }
    //        if (Action.Begin == null)
    //            return true;
    //        if (ToEnumerator == null)
    //        {
    //            return Action.Begin.Invoke(arg);
    //        }
    //        bool success = true;
    //        var ie = ToEnumerator(arg);
    //        while (ie.MoveNext())
    //        {
    //            if (!Action.Begin.Invoke(ie.Current))
    //                success = false;
    //        }
    //        return success;
    //    }

    //    /// <summary>
    //    /// 执行
    //    /// </summary>
    //    public bool Run(object arg)
    //    {
    //        if (ToEnumerator == null)
    //        {
    //            Action.Action?.Invoke(this, arg);
    //        }
    //        else
    //        {
    //            var ie = ToEnumerator(arg);
    //            while (ie.MoveNext())
    //                Action.Action?.Invoke(this, ie.Current);
    //        }
    //        return true;
    //    }
    //    /// <summary>
    //    /// 执行
    //    /// </summary>
    //    public void End(CommandStatus status, Exception exception, bool arg)
    //    {
    //        Action.End?.Invoke(status, exception, arg);
    //    }
    //}
}