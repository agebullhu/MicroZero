// 所在工程：GBoxtCommonService
// 整理用户：bull2
// 建立时间：2012-08-13 5:34
// 整理时间：2012-08-30 2:58

#region

using System ;
using System.Collections.Generic ;

using Agebull.Common.Reflection ;

#endregion

namespace Agebull.Common.Server.Logging
{
    /// <summary>
    ///   日志记录扩展对象
    /// </summary>
    public class LogContent
    {
        /// <summary>
        ///   现场需要保存的数据
        /// </summary>
        private readonly List<LocaleItem> _localeObject = new List<LocaleItem>() ;

        /// <summary>
        ///   构造一个日志对象
        /// </summary>
        public static LogContent Content
        {
            get
            {
                LogContent lc = new LogContent
                {
                        QueryKey = Guid.NewGuid().ToString()
                } ;
                //if (HttpContext.Current != null)
                //{
                //    if (HttpContext.Current.User != null)
                //        lc.User = HttpContext.Current.User.Identity.Name;
                //    if (HttpContext.Current.Request != null)
                //    {
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.Form",
                //            Description = "Web",
                //            Value = TypeObject.SerializeToSoap(HttpContext.Current.Request.Form)
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.Url",
                //            Description = "Web",
                //            Value = TypeObject.SerializeToSoap(HttpContext.Current.Request.Url)
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.Url",
                //            Description = "Web",
                //            Value = TypeObject.SerializeToSoap(HttpContext.Current.Request.Params)
                //        }
                //        );
                //        //lc.LocaleObject.Add(new LocaleItem()
                //        //{
                //        //    Name = "Request.Cookies",
                //        //    Description = "Web",
                //        //    Value = TypeObject.SerializeToSoap(HttpContext.Current.Request.Cookies)
                //        //}
                //        //);
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.Headers",
                //            Description = "Web",
                //            Value = TypeObject.SerializeToSoap(HttpContext.Current.Request.Headers)
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.RequestType",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.RequestType
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.UrlReferrer",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.UrlReferrer
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.UserAgent",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.UserAgent
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.UserHostAddress",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.UserHostAddress
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.UserLanguages",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.UserLanguages
                //        }
                //        );
                //        //lc.LocaleObject.Add(new LocaleItem()
                //        //{
                //        //    Name = "Request.ServerVariables",
                //        //    Description = "Web",
                //        //    Value = HttpContext.Current.Request.ServerVariables
                //        //}
                //        //);
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.QueryString",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.QueryString
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.PhysicalPath",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.PhysicalPath
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.HttpMethod",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.HttpMethod
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.ContentType",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.ContentType
                //        }
                //        );
                //        lc.LocaleObject.Add(new LocaleItem()
                //        {
                //            Name = "Request.AnonymousID",
                //            Description = "Web",
                //            Value = HttpContext.Current.Request.AnonymousID
                //        }
                //        );
                //    }
                //}
                return lc ;
            }
        }

        /// <summary>
        ///   查询标记
        /// </summary>
        public string QueryKey { get ; set ; }

        /// <summary>
        ///   类型
        /// </summary>
        public string Sort { get ; set ; }

        /// <summary>
        ///   自定义消息
        /// </summary>
        public string Infomation { get ; set ; }

        /// <summary>
        ///   系统定义的序列化值
        /// </summary>
        public string User { get ; set ; }

        /// <summary>
        ///   现场需要保存的数据的序列化值
        /// </summary>
        public string Locale
        {
            get
            {
                return ReflectionHelper.Serialize(this._localeObject) ;
            }
        }

        /// <summary>
        ///   现场需要保存的数据
        /// </summary>
        public List<LocaleItem> LocaleObject
        {
            get
            {
                return this._localeObject ;
            }
        }
    }
}
