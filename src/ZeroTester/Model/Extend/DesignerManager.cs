using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Agebull.EntityModel.Config;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 扩展设计器管理类
    /// </summary>
    public static class DesignerManager
    {
        internal static readonly Dictionary<Type, Dictionary<string, ExtendViewOption>> ExtendDictionary = new Dictionary<Type, Dictionary<string, ExtendViewOption>>();

        /// <summary>
        /// 注册扩展
        /// </summary>
        /// <typeparam name="TEditor">扩展类型</typeparam>
        /// <typeparam name="TConfig">对应的类型</typeparam>
        /// <param name="name">扩展名称</param>
        /// <param name="filter">关联的工作模式</param>
        public static void Registe<TConfig, TEditor>(string name, params string[] filter)
            where TEditor : UserControl, new()
            where TConfig : ConfigBase
        {
            Registe<TConfig, TEditor>(name, -1, filter);
        }

        /// <summary>
        /// 注册扩展
        /// </summary>
        /// <typeparam name="TEditor">扩展类型</typeparam>
        /// <typeparam name="TConfig">对应的类型</typeparam>
        /// <param name="name">扩展名称</param>
        /// <param name="index">显示顺序</param>
        /// <param name="filter">关联的工作模式</param>
        public static void Registe<TConfig, TEditor>(string name, int index, params string[] filter)
            where TEditor : UserControl, new()
            where TConfig : ConfigBase
        {
            if (!ExtendDictionary.TryGetValue(typeof(TConfig), out var exts))
                ExtendDictionary.Add(typeof(TConfig), exts = new Dictionary<string, ExtendViewOption>(StringComparer.OrdinalIgnoreCase));

            if (exts.ContainsKey(name))
                exts[name] = new ExtendViewOption
                {
                    Index = index,
                    Name = name,
                    Caption = name,
                    Create = () => new TEditor()
                };
            else
                exts.Add(name, new ExtendViewOption
                {
                    Index = index,
                    Name = name,
                    Caption = name,
                    Create = () => new TEditor()
                });
            if (filter.Length > 0)
                exts[name].Filter.AddRange(filter.Select(p => p.ToLower()));
        }
    }

    public class ExtendViewOption : SimpleConfig
    {
        /// <summary>
        /// 显示顺序
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 关联的工作模式
        /// </summary>
        public List<string> Filter { get; } = new List<string>();

        /// <summary>
        /// 构造器
        /// </summary>
        public Func<UserControl> Create { get; set; }
    }
}