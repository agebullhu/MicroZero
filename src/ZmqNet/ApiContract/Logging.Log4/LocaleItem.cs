// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:34
// 整理时间：2012-08-30 2:58

#region

using System ;

#endregion

namespace Agebull.Common.Server.Logging
{
    /// <summary>
    ///   本地信息
    /// </summary>
    [Serializable]
    public class LocaleItem
    {
        /// <summary>
        ///   名称
        /// </summary>
        public string Name { get ; set ; }

        /// <summary>
        ///   说明
        /// </summary>
        public string Description { get ; set ; }

        /// <summary>
        ///   值
        /// </summary>
        public object Value { get ; set ; }
    }
}
