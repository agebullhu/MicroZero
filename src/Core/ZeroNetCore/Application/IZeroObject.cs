using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Agebull.MicroZero.ZeroManagemant;
using Agebull.MicroZero.ZeroApis;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Agebull.MicroZero
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
        int RealState { get; }

        /// <summary>
        ///     配置状态
        /// </summary>
        StationStateType ConfigState { get; }

        /// <summary>
        /// 系统初始化时调用
        /// </summary>
        void OnZeroInitialize();

        /// <summary>
        /// 系统启动时调用
        /// </summary>
        Task<bool> OnZeroStart();

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
        /// 开启
        /// </summary>
        Task<bool> Start();

        /// <summary>
        /// 关闭
        /// </summary>
        bool Close();

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
        internal static readonly ConcurrentDictionary<string, IZeroObject> ZeroObjects = new ConcurrentDictionary<string, IZeroObject>();

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
        internal static readonly List<IZeroObject> ActiveObjects = new List<IZeroObject>();

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
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalStart(IZeroObject obj)
        {
            lock (GlobalObjects)
            {
                GlobalObjects.Add(obj);
                ZeroTrace.SystemLog(obj.StationName, "GlobalStart");
            }
        }

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnGlobalEnd(IZeroObject obj)
        {
            ZeroTrace.SystemLog(obj.StationName, "GlobalEnd");
            bool can;
            lock (GlobalObjects)
            {
                GlobalObjects.Remove(obj);
                can = GlobalObjects.Count == 0;
            }
            if (can)
                GlobalSemaphore.Release();
        }

        /*// <summary>
        ///     重置当前活动数量
        /// </summary>
        public static void ResetObjectActive()
        {
            lock (ActiveObjects)
            {
                ActiveObjects.Clear();
                FailedObjects.Clear();
            }
            while (ActiveSemaphore.CurrentCount > 0)
                ActiveSemaphore.Wait();
        }

        /// <summary>
        ///     是否存在活动对象
        /// </summary>
        public static bool HaseActiveObject
        {
            get
            {
                lock (ActiveObjects)
                {
                    return ActiveObjects.Count > 0;
                }
            }
        }*/

        /// <summary>
        ///     对象活动时登记
        /// </summary>
        public static void OnObjectActive(IZeroObject obj)
        {
            bool can;
            ZeroTrace.SystemLog(obj.StationName, "OnObjectActive");
            lock (ActiveObjects)
            {
                ActiveObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectFailed(IZeroObject obj)
        {
            ZeroTrace.WriteError(obj.StationName, "OnObjectFailed");
            bool can;
            lock (ActiveObjects)
            {
                FailedObjects.Add(obj);
                can = ActiveObjects.Count + FailedObjects.Count == ZeroObjects.Count;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     对象关闭时登记
        /// </summary>
        public static void OnObjectClose(IZeroObject obj)
        {
            ZeroTrace.SystemLog(obj.StationName, "OnObjectClose");
            bool can;
            lock (ActiveObjects)
            {
                ActiveObjects.Remove(obj);
                can = ActiveObjects.Count == 0;
            }
            if (can)
                ActiveSemaphore.Release(); //发出完成信号
        }

        /// <summary>
        ///     等待所有对象信号(全开或全关)
        /// </summary>
        public static async Task WaitAllObjectSafeClose()
        {
            lock (ActiveObjects)
                if (ActiveSemaphore.CurrentCount == 0 && ActiveObjects.Count == 0)
                    return;
            await ActiveSemaphore.WaitAsync();
        }

        /// <summary>
        ///     取已注册对象
        /// </summary>
        public static IZeroObject TryGetZeroObject(string name)
        {
            return ZeroObjects.TryGetValue(name, out var zeroObject) ? zeroObject : null;
        }

        /// <summary>
        ///     注册对象
        /// </summary>
        public static bool RegistZeroObject(IZeroObject obj)
        {
            if (!ZeroObjects.TryAdd(obj.StationName, obj))
                return false;
            ZeroTrace.SystemLog(obj.StationName, "RegistZeroObject");

            if (ApplicationState >= StationState.Initialized)
            {
                try
                {
                    obj.OnZeroInitialize();
                    ZeroTrace.SystemLog(obj.StationName, "Initialize");
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.StationName, e, "Initialize");
                }
            }

            if (obj.GetType().IsSubclassOf(typeof(ApiStationBase)))
            {
                ZeroDiscover discover = new ZeroDiscover
                {
                    StationName = obj.StationName
                };
                discover.FindApies(obj.GetType());
                //ZeroDiscover.DiscoverApiDocument(obj.GetType());
            }

            if (ApplicationState != StationState.Run || !ZerCenterIsRun)
                return true;
            try
            {
                ZeroTrace.SystemLog(obj.StationName, "Start");
                obj.OnZeroStart();
            }
            catch (Exception e)
            {
                ZeroTrace.WriteException(obj.StationName, e, "Start");
            }
            return true;
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnZeroInitialize()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroInitialize>>");
            //using (OnceScope.CreateScope(ZeroObjects))
            {
                foreach (var obj in ZeroObjects.Values.ToArray())
                {
                    try
                    {
                        obj.OnZeroInitialize();
                        ZeroTrace.SystemLog(obj.StationName, "Initialize");
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.StationName, e, "*Initialize");
                    }
                }
                ZeroTrace.SystemLog("Application", "<<OnZeroInitialize]");
            }
        }

        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static async Task OnZeroStart()
        {
            if (WorkModel != ZeroWorkModel.Service)
                return;
            await Task.Yield();
            //Debug.Assert(!HaseActiveObject);
            ZeroTrace.SystemLog("Application", "[OnZeroStart>>");
            //using (OnceScope.CreateScope(ZeroObjects, ResetObjectActive))
            {
                List<Task> tasks = new List<Task>();
                foreach (var obj in ZeroObjects.Values.ToArray())
                {
                    try
                    {
                        ZeroTrace.SystemLog(obj.StationName, $"Try start by {StationState.Text(obj.RealState)}");
                        tasks.Add(obj.OnZeroStart());
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException(obj.StationName, e, "*Start");
                    }
                }

                Task.WaitAll(tasks.ToArray());
                //等待所有对象信号(全开或全关)
                await ActiveSemaphore.WaitAsync();
            }
            await SystemManager.Instance.HeartReady();
            ApplicationState = StationState.Run;
            RaiseEvent(ZeroNetEventType.AppRun, true);
            ZeroTrace.SystemLog("Application", "<<OnZeroStart]");
        }


        private static readonly LockData RunLock = new LockData();
        /// <summary>
        ///     系统启动时调用
        /// </summary>
        internal static void OnStationStateChanged(StationConfig config)
        {
            foreach (var obj in ZeroObjects.Values.Where(p => p.StationName == config.StationName).ToArray())
            {
                try
                {
                    ZeroTrace.SystemLog(obj.Name, "[OnStationStateChanged>>", config.State);
                    obj.OnStationStateChanged(config);
                    ZeroTrace.SystemLog(obj.Name, "<<OnStationStateChanged]");
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException(obj.StationName, e, "OnStationStateChanged");
                }
            }
        }

        /// <summary>
        ///     系统关闭时调用
        /// </summary>
        internal static async Task OnZeroEnd(bool fromCenter = true)
        {
            if (WorkModel != ZeroWorkModel.Service || ApplicationState >= StationState.Closing || ApplicationState == StationState.Failed)
            {
                return;
            }
            ZeroTrace.SystemLog(StationState.Text(ApplicationState), "[OnZeroEnd>>");
            ApplicationState = StationState.Closing;
            
            RaiseEvent(ZeroNetEventType.AppStop, true);
            //using (OnceScope.CreateScope(ZeroObjects))
            {
                IZeroObject[] array;
                lock (ActiveObjects)
                    array = ActiveObjects.ToArray();
                foreach (var obj in array)
                {
                    try
                    {
                        ZeroTrace.SystemLog("OnZeroEnd", obj.StationName);
                        obj.OnZeroEnd();
                    }
                    catch (Exception e)
                    {
                        ZeroTrace.WriteException("OnZeroEnd", e, obj.StationName);
                    }
                }
                await WaitAllObjectSafeClose();
            }
            ApplicationState = /*fromCenter ? StationState.Failed :*/ StationState.Closed;
            GC.Collect();
            ZeroTrace.SystemLog(StationState.Text(ApplicationState), "<<OnZeroEnd]");
        }

        /// <summary>
        ///     注销时调用
        /// </summary>
        internal static void OnZeroDestory()
        {
            ZeroTrace.SystemLog("Application", "[OnZeroDestory>>");
            //using (OnceScope.CreateScope(ZeroObjects))
            RaiseEvent(ZeroNetEventType.AppEnd, true);
            var array = ZeroObjects.Values.ToArray();
            ZeroObjects.Clear();
            foreach (var obj in array)
            {
                try
                {
                    ZeroTrace.SystemLog("OnZeroDestory", obj.StationName);
                    obj.OnZeroDestory();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("OnZeroDestory", e, obj.StationName);
                }
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroDestory]");

            GC.Collect();

            ZeroTrace.SystemLog("Application", "[OnZeroDispose>>");
            foreach (var obj in array)
            {
                try
                {
                    ZeroTrace.SystemLog("OnZeroDispose", obj.StationName);
                    obj.Dispose();
                }
                catch (Exception e)
                {
                    ZeroTrace.WriteException("OnZeroDispose", e, obj.StationName);
                }
            }
            ZeroTrace.SystemLog("Application", "<<OnZeroDispose]");
        }

        #endregion
    }
}