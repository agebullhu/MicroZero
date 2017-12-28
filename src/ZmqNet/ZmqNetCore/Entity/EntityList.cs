using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Agebull.Common.DataModel
{
    /// <summary>
    ///     业务实体对象列表
    /// </summary>
    public class EntityList<TEntity> : NotificationObject, IList, IList<TEntity>, INotifyCollectionChanged
        where TEntity : EntityBase
    {
        #region 基础数据

        #region 集合读取

        /// <summary>
        ///     取设下标对象
        /// </summary>
        /// <param name="idx"> </param>
        /// <returns> </returns>
        public TEntity this[int idx]
        {
            get { return This2(idx); }
            set { Insert(idx, value); }
        }

        /// <summary>
        ///     是否包含
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public TEntity Find(TEntity value)
        {
            return Entities.FirstOrDefault(p => p == value);
        }

        /// <summary>
        ///     所在位置
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public int IndexOf(TEntity value)
        {
            return IndexOf2(value);
        }

        /// <summary>
        ///     是否包含
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public bool Contains(TEntity value)
        {
            return Entities.Contains(value);
        }

        /// <summary>
        ///     是否包含
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        public void SyncOrInsert(TEntity value)
        {
            SyncByUpdated(value);
        }

        /// <summary>
        ///     总行数
        /// </summary>
        public int Count
        {
            get { return Entities.Count; }
        }

        /// <summary>
        ///     得到枚举器
        /// </summary>
        public IEnumerator<TEntity> GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        #endregion

        #region 加入

        /// <summary>
        ///     加入一批
        /// </summary>
        /// <param name="entities"> </param>
        public void AddRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                Add(entity);
        }

        /// <summary>
        ///     加入或是同步
        /// </summary>
        /// <param name="entity"> </param>
        public void AddOrSwitch(TEntity entity)
        {
            if (entity == null)
                return;
            var old = this.FirstOrDefault(p => p.Equals(entity));
            if (old == entity)
                return;
            if (old != null)
                old.CopyValue(entity);
            else
                Add(entity);
        }

        /// <summary>
        ///     加入或是同步
        /// </summary>
        public void AddOrSwitch(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                return;
            foreach (var entity in entities)
                AddOrSwitch(entity);
        }

        #endregion

        #region 清除

        /// <summary>
        ///     删除下标
        /// </summary>
        public void RemoveAt(int index)
        {
            RemoveAt2(index);
        }

        #endregion

        #region 同步

        /// <summary>
        ///     从一个集合中复制数据
        /// </summary>
        /// <param name="entities"></param>
        /// <remarks>集合中不存在的数据将被删除,原来存在的数据进行覆盖,原来不存在的加入</remarks>
        public virtual void CopyFrom(IEnumerable<TEntity> entities)
        {
            if (entities == null)
            {
                Clear();
                return;
            }
            var values = Entities.ToList();
            var entityObjectBases = entities as List<TEntity> ?? entities.ToList();
            if (entityObjectBases.Count == 0)
            {
                Clear();
                return;
            }
            //双方都有
            foreach (var entity in entityObjectBases.ToArray())
            {
                var olds = Entities.Where(p => p == entity).ToArray();
                if (olds.Length == 0)
                    Add(entity);
                foreach (var old in olds)
                {
                    old.CopyValue(entity);
                    Add(entity);
                    values.Remove(old);
                }
            }
            //旧有新无
            foreach (var oe in values)
                Remove(oe);
        }


#if CLIENT
        /// <summary>
        ///     清除临时数据
        /// </summary>
        public virtual void OnBeginUpdate()
        {
        }
#endif

        /// <summary>
        ///     同步数据
        /// </summary>
        /// <param name="entity"></param>
        public virtual void SyncMessage(TEntity entity)
        {
            var any = false;
            var olds = Entities.Where(p => p == entity);
            foreach (var old in olds)
            {
                if (entity != old)
                    old.CopyValue(entity);
                any = true;
                break;
            }
            if (!any)
                Add(entity);
        }

        /// <summary>
        ///     同步数据
        /// </summary>
        /// <param name="entity"></param>
        public virtual void SyncByUpdated(TEntity entity)
        {
            var any = false;
            var olds = Entities.Where(p => p == entity);
            foreach (var old in olds)
            {
                if (entity != old)
                    old.CopyValue(entity);
                any = true;
                break;
            }
            if (!any)
                Add(entity);
        }

        #endregion

        #region 接口实现

        /// <summary>
        ///     按索引排序
        /// </summary>
        public void Sort<TKey>(Func<TEntity, TKey> sort)
        {
            var v = Entities.OrderBy(sort).ToArray();
            SetEmpty();
            foreach (var e in v)
                Add(e);
        }

        /// <summary>
        /// </summary>
        /// <param name="value"> </param>
        /// <returns> </returns>
        int IList.IndexOf(object value)
        {
            return IndexOf2(value as TEntity);
        }

        /// <summary>
        /// </summary>
        /// <param name="index"> </param>
        /// <param name="value"> </param>
        void IList.Insert(int index, object value)
        {
            Insert(index, value as TEntity);
        }

        /// <summary>
        ///     得到枚举器
        /// </summary>
        /// <returns> 枚举器 </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Entities.GetEnumerator();
        }

        /// <summary>
        ///     复制到--未实现
        /// </summary>
        /// <param name="array"> 数组 </param>
        /// <param name="arrayIndex"> 位置 </param>
        void ICollection<TEntity>.CopyTo(TEntity[] array, int arrayIndex)
        {
            foreach (var k in Entities)
                array[arrayIndex++] = k;
        }

        bool IList.Contains(object value)
        {
            return Contains(value as TEntity);
        }

        int IList.Add(object value)
        {
            var entity = value as TEntity;
            if (entity == null)
                return -1;
            Add(entity);
            return IndexOf(entity);
        }

        bool ICollection<TEntity>.IsReadOnly {get{return false;}}

        bool IList.IsReadOnly {get{return false;}}

        bool IList.IsFixedSize {get{return false;}}

        void IList.Remove(object value)
        {
            Remove(value as TEntity);
        }

        TEntity IList<TEntity>.this[int idx]
        {
            get { return This2(idx); }
            set { Insert(idx, value); }
        }

        object IList.this[int idx]
        {
            get { return This2(idx); }
            set { Insert(idx, value as TEntity); }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var k in Entities)
                array.SetValue(k, index++);
        }

        /// <summary>获取一个值，该值指示是否同步对 <see cref="T:System.Collections.ICollection" /> 的访问（线程安全）。</summary>
        /// <returns>如果对 <see cref="T:System.Collections.ICollection" /> 的访问是同步的（线程安全），则为 true；否则为 false。</returns>
        /// <filterpriority>2</filterpriority>
        bool ICollection.IsSynchronized {get{return false;}}

        /// <summary>获取可用于同步对 <see cref="T:System.Collections.ICollection" /> 的访问的对象。</summary>
        /// <returns>可用于同步对 <see cref="T:System.Collections.ICollection" /> 的访问的对象。</returns>
        /// <filterpriority>2</filterpriority>
        object ICollection.SyncRoot
        {
            get { return Entities; }
        }

        #endregion

        #region INotifyCollectionChanged

        private event NotifyCollectionChangedEventHandler collectionChanged;

        /// <summary>
        ///     集合内容增删
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                collectionChanged -= value;
                collectionChanged += value;
            }
            remove { collectionChanged -= value; }
        }

        #endregion

        /// <summary>
        ///     发出集合修改事件
        /// </summary>
        private void RaiseCollectionChangedInner(NotifyCollectionChangedEventArgs e)
        {
            collectionChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     结束编辑
        /// </summary>
        protected override void EndEditInner(List<string> changeds)
        {
            if (_haseEditEvent)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _haseEditEvent = false;
        }

        /// <summary>
        ///     是否存在编辑期间的事件发生
        /// </summary>
        private bool _haseEditEvent;

        /// <summary>
        ///     处理集合修改事件
        /// </summary>
        public void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsEditing)
                _haseEditEvent = true;
            else
                InvokeInUiThread(RaiseCollectionChangedInner, e);
        }

        /// <summary>
        ///     集合有没有修改
        /// </summary>
        public bool CollectionIsModify { get; set; }

        /// <summary>
        ///     新对象加入后续的处理
        /// </summary>
        public void OnAdd(TEntity entity)
        {
            if (entity == null)
                return;
            CollectionIsModify = true;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, entity));
        }

        /// <summary>
        ///     对象被清除后续的处理
        /// </summary>
        private void OnRemove(TEntity entity)
        {
            if (entity == null)
                return;
            CollectionIsModify = true;
            //特殊说明:如果使用理论上正确的NotifyCollectionChangedAction.Remove, entity两个参数来构造对象,在之前进行新增时,DataGrid将死去
            //永不停止地发出数组越界的异常,不知道是为什么,但用NotifyCollectionChangedAction.Reset可以解决这个问题,
            //估计是内部在对删除事件的处理会出现内部可重用的对象无法再找到的问题吧(对象析构过但UI对象都是符合DataGrid要重用之前用过的UI对象的原则)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///     集合清空的后续处理
        /// </summary>
        private void OnClear()
        {
            CollectionIsModify = true;
            //特殊说明:如果使用理论上正确的NotifyCollectionChangedAction.Remove, entity两个参数来构造对象,在之前进行新增时,DataGrid将死去
            //永不停止地发出数组越界的异常,不知道是为什么,但用NotifyCollectionChangedAction.Reset可以解决这个问题,
            //估计是内部在对删除事件的处理会出现内部可重用的对象无法再找到的问题吧(对象析构过但UI对象都是符合DataGrid要重用之前用过的UI对象的原则)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region 内部数据存储

        /// <summary>
        ///     内部数据存储
        /// </summary>
        private List<TEntity> _entities;

        /// <summary>
        ///     内部列表
        /// </summary>
        public List<TEntity> Entities
        {
            get { return _entities ?? (_entities = new List<TEntity>()); }
        }

        /// <summary>
        ///     是否为空
        /// </summary>
        public bool IsEmpty
        {
            get { return (_entities == null) || (_entities.Count == 0); }
        }

        /// <summary>
        ///     是否为空
        /// </summary>
        public void SetEmpty()
        {
            if (!IsEmpty)
                _entities.Clear();
        }

        private void RemoveAt2(int idx)
        {
            if (IsEmpty || (idx >= Count) || (idx < 0))
                return;
            var en = _entities[idx];
            _entities.RemoveAt(idx);
            OnRemove(en);
        }

        private int IndexOf2(TEntity entity)
        {
            if (entity == null)
                return -1;
            return _entities.IndexOf(entity);
        }

        private TEntity This2(int idx)
        {
            if (IsEmpty || (idx >= Count) || (idx < 0))
                return null;
            return IsEmpty ? null : _entities[idx];
        }

        #endregion

        #region 基本方法


        /// <summary>
        ///     加入
        /// </summary>
        /// <param name="entity"> </param>
        public void Add(TEntity entity)
        {
            if (entity == null)
                return;
            if (_entities == null)
            {
                _entities = new List<TEntity> { entity };
                return;
            }
            _entities.Add(entity);
            OnAdd(entity);
        }

        /// <summary>
        ///     排序加入
        /// </summary>
        /// <param name="entity"> </param>
        /// <param name="sort"></param>
        public int Add(TEntity entity, Func<TEntity, TEntity, bool> sort)
        {
            if (entity == null)
                return -1;
            if (_entities == null)
            {
                _entities = new List<TEntity> { entity };
                return -1;
            }
            var added = false;
            int idx;
            for (idx = 0; idx < _entities.Count; idx++)
            {
                if (sort(entity, _entities[idx]))
                    continue;
                _entities.Insert(idx, entity);
                added = true;
                break;
            }
            if (!added)
                _entities.Add(entity);
            OnAdd(entity);
            return idx;
        }


        /// <summary>
        ///     插入
        /// </summary>
        public void Insert(int idx, TEntity entity)
        {
            if (entity == null)
                return;
            if (_entities == null)
            {
                _entities = new List<TEntity> { entity };
                return;
            }
            var list = _entities;
            if (idx <= 0)
                list.Insert(0, entity);
            else if (idx >= list.Count)
                list.Add(entity);
            else
                list.Insert(idx, entity);
            OnAdd(entity);
        }

        /// <summary>
        ///     从列表中清除
        /// </summary>
        /// <param name="entity"> </param>
        public bool Remove(TEntity entity)
        {
            if (IsEmpty)
                return false;
            if (!_entities.Remove(entity))
                return false;
            OnRemove(entity);
            return true;
        }

        /// <summary>
        ///     清除所有数据
        /// </summary>
        public void Clear()
        {
            if (IsEmpty)
                return;
            var old = _entities.ToArray();
            foreach (var entity in old)
                Remove(entity);
            OnClear();
        }


        #endregion
    }

    /// <summary>
    /// 实体分组
    /// </summary>
    /// <typeparam name="TEntityBase"></typeparam>
    public class EntityGroup<TEntityBase> : EntityList<TEntityBase>
        where TEntityBase : EntityBase
    {
        private string _title;

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (string.Equals(_title, value, StringComparison.OrdinalIgnoreCase))
                    return;
                _title = value;
                OnPropertyChanged();
            }
        }
    }
}