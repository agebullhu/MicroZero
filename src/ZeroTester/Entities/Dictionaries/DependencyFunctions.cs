// /*****************************************************
// (c)2008-2013 Copy right www.Agebull.com
// 作者:bull2
// 工程:CodeRefactor-Agebull.Common.WpfMvvmBase
// 建立:2014-11-29
// 修改:2014-11-30
// *****************************************************/

#region 引用

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel
{
    /// <summary>
    ///     依赖方法字典
    /// </summary>
    [DataContract, JsonObject(MemberSerialization.OptIn)]
    public sealed class DependencyDelegates : IDependencyDelegates
    {
        #region 方法字典

        /// <summary>
        ///     依赖方法字典
        /// </summary>
        [IgnoreDataMember]
        private readonly Dictionary<Type, object> _dictionary = new Dictionary<Type, object>();

        /// <summary>
        ///     附加方法
        /// </summary>
        public void AnnexDelegate<TAction>(TAction action) where TAction : class
        {
            var type = typeof(TAction);
            if (_dictionary.ContainsKey(type))
            {
                if (Equals(action, null))
                {
                    _dictionary.Remove(type);
                }
                else
                {
                    _dictionary[type] = action as Delegate;
                }
            }
            else if (!Equals(action, null))
            {
                _dictionary.Add(type, action as Delegate);
            }
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public TValue GetDelegate<TValue>() where TValue : class
        {
            var type = typeof(TValue);
            if (!_dictionary.TryGetValue(type, out object value))
            {
                return null;
            }
            if (!(value is TValue))
            {
                return null;
            }
            return value as TValue;
        }

        /// <summary>
        ///     是否已附加对象
        /// </summary>
        /// <typeparam name="TAction">对象类型(理论上限定为Action或Func这两种类型的委托)</typeparam>
        /// <returns>如果存在则返回方法,否则返回空</returns>
        public bool HaseDelegate<TAction>() where TAction : class
        {
            return _dictionary.ContainsKey(typeof(TAction));
        }

        #endregion

        #region 附加方法

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexFunc<TResult>(Func<TResult> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexFunc<TResult, T2>(Func<TResult, T2> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexFunc<TResult, T2, T3>(Func<TResult, T2, T3> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexFunc<TResult, T2, T3, T4>(Func<TResult, T2, T3, T4> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexFunc<TResult, T2, T3, T4, T5>(Func<TResult, T2, T3, T4, T5> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexAction(Action value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexAction<T>(Action<T> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexAction<T1, T2>(Action<T1, T2> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexAction<T1, T2, T3>(Action<T1, T2, T3> value)
        {
            AnnexDelegate(value);
        }

        /// <summary>
        ///     附加方法
        /// </summary>
        /// <remarks>
        ///     这种方法只存在一个,即多次附加,只存最后一个对象
        /// </remarks>
        public void AnnexAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> value)
        {
            AnnexDelegate(value);
        }

        #endregion

        #region 取方法

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        /// <returns></returns>
        public Func<TResult> GetFunction<TResult>()
        {
            return GetDelegate<Func<TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Func<TArg1, TResult> GetFunction<TArg1, TResult>()
        {
            return GetDelegate<Func<TArg1, TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Func<TArg1, TArg2, TResult> GetFunction<TArg1, TArg2, TResult>()
        {
            return GetDelegate<Func<TArg1, TArg2, TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Func<TArg1, TArg2, TArg3, TResult> GetFunction<TArg1, TArg2, TArg3, TResult>()
        {
            return GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Func<TArg1, TArg2, TArg3, TArg4, TResult> GetFunction<TArg1, TArg2, TArg3, TArg4, TResult>()
        {
            return GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> GetFunction<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>()
        {
            return GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        /// <returns></returns>
        public Action GetAction()
        {
            return GetDelegate<Action>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Action<TArg1> GetAction<TArg1>()
        {
            return GetDelegate<Action<TArg1>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Action<TArg1, TArg2> GetAction<TArg1, TArg2>()
        {
            return GetDelegate<Action<TArg1, TArg2>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Action<TArg1, TArg2, TArg3> GetAction<TArg1, TArg2, TArg3>()
        {
            return GetDelegate<Action<TArg1, TArg2, TArg3>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Action<TArg1, TArg2, TArg3, TArg4> GetAction<TArg1, TArg2, TArg3, TArg4>()
        {
            return GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
        }

        /// <summary>
        ///     取得内部附加对象
        /// </summary>
        public Action<TArg1, TArg2, TArg3, TArg4, TArg5> GetAction<TArg1, TArg2, TArg3, TArg4, TArg5>()
        {
            return GetDelegate<Action<TArg1, TArg2, TArg3, TArg4, TArg5>>();
        }

        #endregion

        #region 有返回值

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult TryExecute<TResult>()
        {
            var func = GetDelegate<Func<TResult>>();
            return func != null ? func() : default(TResult);
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult TryExecute<TArg1, TResult>(TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            return func != null ? func(arg1) : default(TResult);
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult TryExecute<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            return func != null ? func(arg1, arg2) : default(TResult);
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult TryExecute<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            return func != null ? func(arg1, arg2, arg3) : default(TResult);
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult TryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();
            return func != null ? func(arg1, arg2, arg3, arg4) : default(TResult);
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult Execute<TResult>()
        {
            var func = GetDelegate<Func<TResult>>();
            if (func != null)
            {
                return func();
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult Execute<TArg1, TResult>(TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            if (func != null)
            {
                return func(arg1);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult Execute<TArg1, TArg2, TResult>(TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            if (func != null)
            {
                return func(arg1, arg2);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult Execute<TArg1, TArg2, TArg3, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            if (func != null)
            {
                return func(arg1, arg2, arg3);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public TResult Execute<TArg1, TArg2, TArg3, TArg4, TResult>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();
            if (func != null)
            {
                return func(arg1, arg2, arg3, arg4);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        #endregion

        #region 无返回值

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void TryRun()
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                action();
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void TryRun<TArg1>(TArg1 arg1)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                action(arg1);
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void TryRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                action(arg1, arg2);
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void TryRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                action(arg1, arg2, arg3);
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void TryRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                action(arg1, arg2, arg3, arg4);
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void Run()
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                action();
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void Run<TArg1>(TArg1 arg1)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                action(arg1);
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void Run<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                action(arg1, arg2);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void Run<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                action(arg1, arg2, arg3);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void Run<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                action(arg1, arg2, arg3, arg4);
            }
            throw new ArgumentException("不存在对应的方法");
        }

        #endregion


        #region 异步


        #region 执行

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncExecute<TResult>(Action<TResult> asyncAction)
        {
            var func = GetDelegate<Func<TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func();
                    asyncAction(result);
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncExecute<TArg1, TResult>(Action<TResult> asyncAction, TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1);
                    asyncAction(result);
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncExecute<TArg1, TArg2, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2);
                    asyncAction(result);
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncExecute<TArg1, TArg2, TArg3, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2, arg3);
                    asyncAction(result);
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步以默认值为参数调用asyncAction
        ///  </remarks>
        public void AsyncExecute<TArg1, TArg2, TArg3, TArg4, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();

            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2, arg3, arg4);
                    asyncAction(result);
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncRun(Action asyncAction = null)
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action();
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncRun<TArg1>(TArg1 arg1, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                Task.Factory.StartNew(() => action(arg1, arg2));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1, arg2, arg3);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步以默认值为参数调用asyncAction
        ///  </remarks>
        public void AsyncRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1, arg2, arg3, arg4);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        #endregion

        #region 有则执行

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryExecute<TResult>(Action<TResult> asyncAction)
        {
            var func = GetDelegate<Func<TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func();
                    asyncAction(result);
                });
            }
            else
            {
                asyncAction(default(TResult));
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryExecute<TArg1, TResult>(Action<TResult> asyncAction, TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1);
                    asyncAction(result);
                });
            }
            else
            {
                asyncAction(default(TResult));
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryExecute<TArg1, TArg2, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2);
                    asyncAction(result);
                });
            }
            else
            {
                asyncAction(default(TResult));
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Func),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryExecute<TArg1, TArg2, TArg3, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2, arg3);
                    asyncAction(result);
                });
            }
            else
            {
                asyncAction(default(TResult));
            }
        }

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步以默认值为参数调用asyncAction
        ///  </remarks>
        public void AsyncTryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(Action<TResult> asyncAction, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();

            if (func != null)
            {
                Task.Factory.StartNew(() =>
                {
                    var result = func(arg1, arg2, arg3, arg4);
                    asyncAction(result);
                });
            }
            else
            {
                asyncAction(default(TResult));
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryRun(Action asyncAction = null)
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action();
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            else if (asyncAction != null)
            {
                asyncAction();
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryRun<TArg1>(TArg1 arg1, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            else if (asyncAction != null)
            {
                asyncAction();
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryRun<TArg1, TArg2>(TArg1 arg1, TArg2 arg2, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                Task.Factory.StartNew(() => action(arg1, arg2));
            }
            else if (asyncAction != null)
            {
                asyncAction();
            }
        }

        /// <summary>
        ///     如果有对应类型的方法(Action),执行它并返回数据
        /// </summary>
        /// <returns>
        ///     如果方法存在并成功执行,返回对应的值,否则返回空
        /// </returns>
        public void AsyncTryRun<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1, arg2, arg3);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            else if (asyncAction != null)
            {
                asyncAction();
            }
        }

        /// <summary>
        ///     异步执行对应类型的方法(Func)
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步以默认值为参数调用asyncAction
        ///  </remarks>
        public void AsyncTryRun<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, Action asyncAction = null)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                Task.Factory.StartNew(() =>
                {
                    action(arg1, arg2, arg3, arg4);
                    if (asyncAction != null)
                        asyncAction();
                });
            }
            else if (asyncAction != null)
            {
                asyncAction();
            }
        }

        #endregion
        #endregion


        #region 通过异步代理执行


        #region 执行

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Execute<TResult>(ITackProxy<TResult> proxy)
        {
            var func = GetDelegate<Func<TResult>>();
            if (func != null)
            {
                proxy.Run(func);
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Execute<TArg1, TResult>(ITackProxy<TResult> proxy, TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Execute<TArg1, TArg2, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Execute<TArg1, TArg2, TArg3, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2, arg3));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则抛出异常
        ///  </remarks>
        public void Execute<TArg1, TArg2, TArg3, TArg4, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();

            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2, arg3, arg4));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Run(ITackProxy proxy)
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                proxy.Run(action);
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Run<TArg1>(ITackProxy proxy, TArg1 arg1)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Run<TArg1, TArg2>(ITackProxy proxy, TArg1 arg1, TArg2 arg2)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则抛出异常
        /// </returns>
        public void Run<TArg1, TArg2, TArg3>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2, arg3));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则抛出异常
        ///  </remarks>
        public void Run<TArg1, TArg2, TArg3, TArg4>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2, arg3, arg4));
            }
            throw new ArgumentException("不存在对应名称的方法");
        }

        #endregion

        #region 有则执行

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryExecute<TResult>(ITackProxy<TResult> proxy)
        {
            var func = GetDelegate<Func<TResult>>();
            if (func != null)
            {
                proxy.Run(func);
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryExecute<TArg1, TResult>(ITackProxy<TResult> proxy, TArg1 arg1)
        {
            var func = GetDelegate<Func<TArg1, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryExecute<TArg1, TArg2, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryExecute<TArg1, TArg2, TArg3, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TResult>>();
            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2, arg3));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步调用proxy.Exist
        ///  </remarks>
        public void TryExecute<TArg1, TArg2, TArg3, TArg4, TResult>(ITackProxy<TResult> proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var func = GetDelegate<Func<TArg1, TArg2, TArg3, TArg4, TResult>>();

            if (func != null)
            {
                proxy.Run(() => func(arg1, arg2, arg3, arg4));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryRun(ITackProxy proxy)
        {
            var action = GetDelegate<Action>();
            if (action != null)
            {
                proxy.Run(action);
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryRun<TArg1>(ITackProxy proxy, TArg1 arg1)
        {
            var action = GetDelegate<Action<TArg1>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryRun<TArg1, TArg2>(ITackProxy proxy, TArg1 arg1, TArg2 arg2)
        {
            var action = GetDelegate<Action<TArg1, TArg2>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Action
        /// </summary>
        /// <returns>
        ///     如果方法存在异步调用proxy.Run,否则同步调用proxy.Exist
        /// </returns>
        public void TryRun<TArg1, TArg2, TArg3>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2, arg3));
            }
            else
            {
                proxy.Exist();
            }
        }

        /// <summary>
        ///     通过异步代理执行Func
        /// </summary>
        /// <remarks>
        ///     如果方法存在,就异步调用方法,以返回值为参数调用asyncAction,
        ///     否则同步调用proxy.Exist
        ///  </remarks>
        public void TryRun<TArg1, TArg2, TArg3, TArg4>(ITackProxy proxy, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            var action = GetDelegate<Action<TArg1, TArg2, TArg3, TArg4>>();
            if (action != null)
            {
                proxy.Run(() => action(arg1, arg2, arg3, arg4));
            }
            else
            {
                proxy.Exist();
            }
        }

        #endregion

        #endregion
    }
}
