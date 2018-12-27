using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Agebull.ZeroNet.ZeroApi;

namespace Agebull.ZeroNet.Core
{
    /// <summary>
    /// 站点连接池
    /// </summary>
    public interface IZeroObject : IDisposable
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }
        /// <summary>
        /// 名称
        /// </summary>
        string StationName { get; }

        /// <summary>
        ///     运行状态
        /// </summary>
        int State { get; }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void OnZeroInitialize();

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        bool OnZeroStart();

        /// <summary>
        ///     要求心跳
        /// </summary>
        void OnHeartbeat();

        /// <summary>
        /// 系统关闭时调用
        /// </summary>
        bool OnZeroEnd();

        /// <summary>
        /// 注销时调用
        /// </summary>
        void OnZeroDestory();

        /// <summary>
        /// 站点状态变更时调用
        /// </summary>
        void OnStationStateChanged(StationConfig config);
    }

    /// <summary>
    ///     站点应用
    /// </summary>
    partial class ZeroApplication
    {

        #region IZeroObject

        /// <summary>
        /// 已注册的对象
        /// </summary>
        private static readonly Dictionary<string, IZeroObject> ZeroObjects = new Dictionary<string, IZeroObject>();

        /// <summary>
        ///     注册单例对象
        /// </summary>
        public static bool RegistZeroObject<TZeroObject>() where TZeroObject : class, IZeroObject, new()
        {
            return RegistZeroObject(new TZeroObject());
        }

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IZeroObject> ActiveObjects = new List<IZeroObject>();

        /// <summary>
        /// 活动对象(执行中)
        /// </summary>
        private static readonly List<IZeroObject> FailedObjects = new List<IZeroObject>();

        /// <summary>
        /// 全局执行对象(内部的Task)
        /// </summary>
        private static readonly List<IZeroObject> GlobalObjects = new List<IZeroObject>();

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim ActiveSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     对象活动状态记录器锁定
        /// </summary>
        private static readonly SemaphoreSlim GlobalSemaphore = new SemaphoreSlim(0, short.MaxValue);

        /// <summary>
        ///     重置当前活动数量
        /// </summary>
        public static void ResetObjectActive()
        {
            ActiveObjects.Clear();
            FailedObjects.Clear();
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalStart(IZeroObject obj)
        {
            lock (GlobalObjects)
            {
                GlobalObjects.Add(obj);
                ZeroTrace.SystemLog(obj.Name, "GlobalStart");
            }
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalEnd(IZeroObject obj)
        {
            lock (GlobalObjects)
            {
                GlobalObjects.Remove(obj);
                ZeroTrace.SystemLog(obj.Name, "GlobalEnd");
                if (GlobalObjects.Count == 0)
                    GlobalSemaphore.Release();
            }
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
                ZeroTrace.SystemLog(obj.Name, "Run");
                if (ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
                ZeroTrace.SystemLog(obj.Name, "Closed");
                if (ActiveObjects.Count == 0)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed(IZeroObject obj)
        {
            lock (ActiveObjects)
            {
                FailedObjects.Add(obj);
                ZeroTrace.WriteError(obj.Name, "Failed");
                if (ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count)
                    ActiveSemaphore.Release(); //发出完成信号
            }
        }

        /// <summary>
        ///     是否存在活动对象
        /// </summary>
        public static bool HaseActiveObject => ActiveObjects.Count > 0;

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        public static void WaitAllObjectSemaphore()
        {
            ActiveSemaphore.Wait();
        }

        /// <summary>
        ///     取已注册对象
        /// </summary>
        public static IZeroObject TryGetZeroObject(string name)
        {
            if (ZeroObjects.TryGetValue(name, out var zeroObject))
                return zeroObject;
            return null;
        }

        /// <summary>
        ///     注册对象
        /// </summary>
        public static bool RegistZeroObject(IZeroObject obj)
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                if (ZeroObjects.ContainsKey(obj.Name))
                    return false;
                ZeroTrace.SystemLog("RegistZeroObject", obj.Name);
                ZeroObjects.Add(obj.Name, obj);
                if (ApplicationState >= StationState.Initialized)
                {
                    try
                    {
                        obj.OnZeroInitialize();
                        ZeroTrace.SystemLog(obj.Name, "Initialize");
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "Initialize");
                    }
                }

                if (obj.GetType().IsSubclassOf(typeof(ApiStationBase)))
                {
                    ZeroDiscover discover = new ZeroDiscover();
                    discover.FindApies(obj.GetType());
                    //ZeroDiscover.DiscoverApiDocument(obj.GetType());
                }

                if (!CanDo)
                    return true;
                try
                {
                    ZeroTrace.SystemLog(obj.Name, "Start");
                    obj.OnZeroStart();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.Name, e, "Start");
                }
            }

            return true;
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroInitialize>>");
            using (OnceScope.CreateScope(ZeroObjects))
            {
                Parallel.ForEach(ZeroObjects.Values.ToArray(), obj =>
                {
                    try
                    {
                        obj.OnZeroInitialize();
                        ZeroTrace.SystemLog(obj.Name, "Initialize");
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "*Initialize");
                    }
                });
                ZeroTrace.SystemLog("Application", "<<OnZeroInitialize]");
            }
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroStart()
        {
            if (WorkModel != ZeroWorkModel.Service)
                return;
            Debug.Assert(!HaseActiveObject);
            ZeroTrace.SystemLog("Application", "[OnZeroStart>>");
            using (OnceScope.CreateScope(ZeroObjects, ResetObjectActive))
            {
                foreach (var obj in ZeroObjects.Values.ToArray())
                {
                    try
                    {
                        ZeroTrace.SystemLog(obj.Name, "*Start");
                        obj.OnZeroStart();
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "*Start");
                    }
                }
                WaitAllObjectSemaphore();
            }
            SystemManager.Instance.HeartReady();
            ApplicationState = StationState.Run;
            RaiseEvent(ZeroNetEventType.AppRun);
            ZeroTrace.SystemLog("Application", "<<OnZeroStart]");
        }


        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            using (OnceScope.CreateScope(ZeroObjects))
            {
                var stations = ActiveObjects.Where(p =>
                    string.Equals(config.StationName, p.StationName, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (stations.Length == 0)
                    return;
                ZeroTrace.SystemLog(config.StationName, "[OnStationStateChanged>>");
                Parallel.ForEach(stations, obj =>
                     {
                         try
                         {
                             obj.OnStationStateChanged(config);
                         }
                         catch (Exception e)
                         {
                             ZeroTrace.WriteException(obj.Name, e, "OnStationStateChanged");
                         }
                     });
                ZeroTrace.SystemLog(config.StationName, "<<OnStationStateChanged]");
            }
        }

        /// <summary>
        ///     心跳
        /// </summary>
        internal static void OnHeartbeat()
        {
            if (!CanDo)
                return;
            using (OnceScope.CreateScope(ZeroObjects))
            {
                SystemManager.Instance.Heartbeat();
                foreach (var obj in ActiveObjects.ToArray())
                {
                    try
                    {
                        obj.OnHeartbeat();
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "OnHeartbeat");
                    }
                };
            }
        }

        /// <summary>
        ///     系统关闭时调用
        /// </summary>
        internal static void OnZeroEnd()
        {
            if (WorkModel != ZeroWorkModel.Service)
                return;
            ZeroTrace.SystemLog("Application", "[OnZeroEnd>>");
            RaiseEvent(ZeroNetEventType.AppStop);
            using (OnceScope.CreateScope(ZeroObjects))
            {
                SystemManager.Instance.HeartLeft();
                ApplicationState = StationState.Closing;
                if (HaseActiveObject)
                {
                    Parallel.ForEach(ActiveObjects.ToArray(), obj =>
                    {
                        try
                        {
                            ZeroTrace.SystemLog(obj.Name, "*Close");
                            obj.OnZeroEnd();
                        }
                        catch (Exception e)
                        {
                            ZeroTrace.WriteException(obj.Name, e, "*Close");
                        }
                    });
                    WaitAllObjectSemaphore();
                }
                GC.Collect();
                ApplicationState = StationState.Closed;
                ZeroTrace.SystemLog("Application", "<<OnZeroEnd]");
            }
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroDestory()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroDestory>>");
            using (OnceScope.CreateScope(ZeroObjects))
            {
                RaiseEvent(ZeroNetEventType.AppEnd);
                var array = ZeroObjects.Values.ToArray();
                ZeroObjects.Clear();
                Parallel.ForEach(array, obj =>
                {
                    try
                    {
                        ZeroTrace.SystemLog(obj.Name, "*Destory");
                        obj.OnZeroDestory();
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "*Destory");
                    }
                });

                GC.Collect();
                ZeroTrace.SystemLog("Application", "<<OnZeroDestory]");

                ZeroTrace.SystemLog("Application", "[OnZeroDispose>>");
                Parallel.ForEach(array, obj =>
                {
                    try
                    {
                        ZeroTrace.SystemLog(obj.Name, "*Dispose");
                        obj.Dispose();
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.Name, e, "*Dispose");
                    }
                });
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroDispose]");
        }

        #endregion
    }
}