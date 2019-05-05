// // /*****************************************************
// // (c)2016-2016 Copy right www.gboxt.com
// // 作者:
// // 工程:Agebull.DataModel
// // 建立:2016-06-12
// // 修改:2016-06-16
// // *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Data.Common;
using Agebull.EntityModel.Common;

#endregion

namespace Agebull.MicroZero.ZeroApis
{
    /// <summary>
    ///     数据扩展检查器
    /// </summary>
    public interface IDataExtendChecker
    {
        /// <summary>
        /// 新增前检查
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        bool PrepareAddnew<T>(T data);

        /// <summary>
        /// 更新前检查
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        bool PrepareUpdate<T>(T data);

        /// <summary>
        /// 删除前检查
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        bool PrepareDelete<T>(IEnumerable<long> ids);

        /// <summary>
        /// 查询前检查
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="condition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        bool PrepareQuery(IDataTable dataTable, ref string condition, ref DbParameter[] args);
    }

    /// <summary>
    ///     数据扩展检查器
    /// </summary>
    public static class DataExtendChecker
    {
        private static readonly Dictionary<Type, ExtendCheckers> Checker = new Dictionary<Type, ExtendCheckers>();

        class ExtendCheckers
        {
            /// <summary>
            /// 检查器集合
            /// </summary>
            public readonly Dictionary<Type, Func<IDataExtendChecker>> Dictionary =
                new Dictionary<Type, Func<IDataExtendChecker>>();
            /// <summary>
            /// 转换器
            /// </summary>
            public Func<object, object> Convert { get; set; }
        }


        /// <summary>
        /// 注册检查器
        /// </summary>
        /// <typeparam name="TDataExtendChecker">检查器本身</typeparam>
        /// <typeparam name="TTargetType">检查的目标类型</typeparam>
        public static void Regist<TDataExtendChecker, TTargetType>()
            where TDataExtendChecker : class, IDataExtendChecker, new() where TTargetType : class
        {
            var distType = typeof(TTargetType);
            if (!Checker.TryGetValue(distType, out var list))
                Checker.Add(distType, list = new ExtendCheckers
                {
                    Convert = arg => (object)(arg as TTargetType)
                });
            if (list.Dictionary.ContainsKey(typeof(TDataExtendChecker)))
                return;
            list.Dictionary.Add(typeof(TDataExtendChecker), () => new TDataExtendChecker());
        }

        /// <summary>
        /// 新增前检查
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool PrepareAddnew<T>(T data)
        {
            foreach (var creaters in Checker)
            {
                var target = creaters.Value.Convert(data);
                if (target == null)
                    continue;
                foreach (var creater in creaters.Value.Dictionary.Values)
                {
                    var checker = creater();
                    if (!checker.PrepareAddnew(target))
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 更新前检查
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static bool PrepareUpdate<T>(T data)
        {
            foreach (var creaters in Checker)
            {
                var target = creaters.Value.Convert(data);
                if (target == null)
                    continue;
                foreach (var creater in creaters.Value.Dictionary.Values)
                {
                    var checker = creater();
                    if (!checker.PrepareUpdate(target))
                        return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 删除前检查
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        internal static bool PrepareDelete<T>(long[] ids)
        {
            var type = typeof(T);
            foreach (var creaters in Checker)
            {
                if (type != creaters.Key && !type.IsSubclassOf(creaters.Key) && !type.IsSupperInterface(creaters.Key))
                    continue;
                foreach (var creater in creaters.Value.Dictionary.Values)
                {
                    var checker = creater();
                    if (!checker.PrepareDelete<T>(ids))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 查询前检查
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="condition"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static bool PrepareQuery<T>(IDataTable dataTable, ref string condition, ref DbParameter[] args)
        {
            var type = typeof(T);
            foreach (var creaters in Checker)
            {
                if (type != creaters.Key && !type.IsSubclassOf(creaters.Key) && !type.IsSupperInterface(creaters.Key))
                    continue;
                foreach (var creater in creaters.Value.Dictionary.Values)
                {
                    var checker = creater();
                    if (!checker.PrepareQuery(dataTable, ref condition, ref args))
                        return false;
                }
            }
            return true;
        }
    }
}