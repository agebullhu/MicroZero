using System.IO;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 版本控制节点
    /// </summary>
    public class VersionControlItem
    {
        #region 构造
        /// <summary>
        /// 当前解决方案文件
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 当前解决方案目录
        /// </summary>
        public string Directory => string.IsNullOrEmpty(FilePath) ? null : Path.GetDirectoryName(FilePath);
        /// <summary>
        /// 单例
        /// </summary>
        private static VersionControlItem _current;
        /// <summary>
        /// 单例
        /// </summary>
        public static VersionControlItem Current => _current ?? (_current = new VersionControlItem());
        /// <summary>
        /// 构造
        /// </summary>
        private VersionControlItem()
        {
        }
        /// <summary>
        /// 更新源代码
        /// </summary>
        public void UpdateSourseCode()
        {
            TfsCheckOut();
        }
        /// <summary>
        /// 提交源代码
        /// </summary>
        public void CommitChanged()
        {
            TfsCheckIn();
        }
        #endregion

        #region TFS源代码管理

        public void TfsUpdate()
        {
            //using (TfsSourceCodeHelper helper = new TfsSourceCodeHelper(this.FilePath))
            //{
            //    helper.Update();
            //}
        }

        public void TfsCheckOut()
        {
            //using (TfsSourceCodeHelper helper = new TfsSourceCodeHelper(this.FilePath))
            //{
            //    helper.CheckOut();
            //}
        }

        public void TfsCheckIn()
        {
            //using (TfsSourceCodeHelper helper = new TfsSourceCodeHelper(this.FilePath))
            //{
            //    helper.CheckIn();
            //}
        }

        #endregion
    }
}