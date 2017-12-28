// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:34
// 整理时间：2012-08-30 2:58

#region

using System.IO ;
using System.Reflection ;

using log4net.Core ;
using log4net.Layout.Pattern ;

#endregion

namespace Agebull.Common.Server.Logging
{
    /// <summary>
    ///   消息转换器
    /// </summary>
    public class MyMessagePatternConverter : PatternLayoutConverter
    {
        /// <summary>
        ///   转换
        /// </summary>
        /// <param name="writer"> </param>
        /// <param name="loggingEvent"> </param>
        protected override void Convert(TextWriter writer , LoggingEvent loggingEvent)
        {
            if(this.Option != null)
            {
                WriteObject(writer , loggingEvent.Repository , LookupProperty(this.Option , loggingEvent)) ;
            }
            else
            {
                WriteDictionary(writer , loggingEvent.Repository , loggingEvent.GetProperties()) ;
            }
        }

        /// <summary>
        ///   通过反射获取传入的日志对象的某个属性的值
        /// </summary>
        /// <param name="property"> </param>
        /// <param name="loggingEvent"> </param>
        /// <returns> </returns>
        private static object LookupProperty(string property , LoggingEvent loggingEvent)
        {
            object propertyValue = string.Empty ;
            PropertyInfo propertyInfo = loggingEvent.MessageObject.GetType().GetProperty(property) ;
            if(propertyInfo != null)
            {
                propertyValue = propertyInfo.GetValue(loggingEvent.MessageObject , null) ;
            }
            return propertyValue ;
        }
    }
}
