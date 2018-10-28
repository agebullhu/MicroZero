using System;
using System.Collections.Generic;

#if CLIENT
namespace Agebull.EntityModel
{
    /// <summary>
    /// 使用的上下文汇总
    /// </summary>
    public static class WorkContext
    {
        /// <summary>
        /// 同步上下文
        /// </summary>
        public static ISynchronousContext SynchronousContext
        {
            get; set;
        }

        /// <summary>
        /// 当前工作模式
        /// </summary>
        [ThreadStatic] internal static WorkModel _workModel;

        /// <summary>
        /// 当前工作模式
        /// </summary>
        public static WorkModel WorkModel => _workModel;


        /// <summary>
        /// 是否不引发修改事件
        /// </summary>
        public static bool IsNoChangedNotify => WorkModel >= WorkModel.Show;

        /// <summary>
        /// 正在载入
        /// </summary>
        public static bool InLoding => WorkModel == WorkModel.Loding;

        /// <summary>
        /// 正在保存
        /// </summary>
        public static bool InSaving => WorkModel == WorkModel.Saving;

        /// <summary>
        /// 正在修复
        /// </summary>
        public static bool InRepair => WorkModel == WorkModel.Repair;


        /// <summary>
        /// 正在显示
        /// </summary>
        public static bool InShow => WorkModel == WorkModel.Show;

        /// <summary>
        /// 正在生成代码
        /// </summary>
        public static bool InCoderGenerating => WorkModel == WorkModel.Coder;

        /// <summary>
        /// 代码显示器
        /// </summary>
        [ThreadStatic] internal static Dictionary<string, string> codes;

        /// <summary>
        /// 代码显示器
        /// </summary>
        public static Dictionary<string, string> FileCodes => codes;


        /// <summary>
        /// 代码显示器
        /// </summary>
        [ThreadStatic] internal static bool? writeToFile;

        /// <summary>
        /// 代码显示器
        /// </summary>
        public static bool WriteToFile => writeToFile == null || writeToFile.Value;

    }
}
#endif