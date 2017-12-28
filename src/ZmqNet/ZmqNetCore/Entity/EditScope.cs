using Agebull.Common.DataModel;
using Gboxt.Common.DataModel;

namespace System.ComponentModel
{
    /// <summary>
    ///     对象编辑范围
    /// </summary>
    public class EditScope : IDisposable
    {
        /// <summary>
        ///     当前范围对象
        /// </summary>
        private readonly NotificationObject _data;

        /// <summary>
        ///     锁定对象
        /// </summary>
        private readonly object _lockObj;

        /// <summary>
        ///     前一个范围是否编辑中(如果是,事实上不起作用)
        /// </summary>
        private readonly bool _preIsEditing;

        /// <summary>
        ///     构造
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="lockObj">锁定对象</param>
        private EditScope(NotificationObject data, object lockObj)
        {
            _data = data;
            _preIsEditing = data.IsEditing;
            _lockObj = lockObj ?? new object();
            if (!_preIsEditing)
                _data.BeginEdit();
        }

        /// <summary>
        ///     析构
        /// </summary>
        public void Dispose()
        {
            lock (_lockObj)
            {
                if (_preIsEditing)
                    return;
                _data.EndEdit();
            }
        }

        /// <summary>
        ///     生成一个范围
        /// </summary>
        /// <param name="data"></param>
        /// <param name="lockObj"></param>
        /// <returns></returns>
        public static IDisposable CreatScope(NotificationObject data, object lockObj = null)
        {
            return new EditScope(data, lockObj);
        }
    }
}