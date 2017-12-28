// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:34
// 整理时间：2012-08-30 2:58

#region

using log4net.Layout ;

#endregion

namespace Agebull.Common.Server.Logging
{
    /// <summary>
    ///   LOG4的记录辅助
    /// </summary>
    public class MyPatternLayout : PatternLayout
    {
        /// <summary>
        ///   构造
        /// </summary>
        public MyPatternLayout()
        {
            this.AddConverter("property" , typeof(MyMessagePatternConverter)) ;
        }
    }
}
