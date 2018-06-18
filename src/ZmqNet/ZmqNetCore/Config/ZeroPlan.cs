namespace Agebull.ZeroNet.Core
{

    /// <summary>
    /// 计划类型
    /// </summary>
    public enum plan_date_type
    {
        /// <summary>
        /// 无计划，立即发送
        /// </summary>
        none,
        /// <summary>
        /// 在指定的时间发送
        /// </summary>
        time,
        /// <summary>
        ///  秒间隔后发送
        /// </summary>
        second,
        /// <summary>
        /// 分钟间隔后发送
        /// </summary>
        minute,
        /// <summary>
        /// 小时间隔后发送
        /// </summary>
        hour,
        /// <summary>
        /// 日间隔后发送
        /// </summary>
        day,
        /// <summary>
        /// 每周几
        /// </summary>
        week,
        /// <summary>
        /// 每月几号
        /// </summary>
        month
    };
    /// <summary>
    /// 计划
    /// </summary>
    public class ZeroPlan
    {
        /// <summary>
        /// 计划类型
        /// </summary>
        public plan_date_type plan_type;

        /// <summary>
        /// 类型值
        /// </summary>
        /// <remarks>
        /// none time 无效
        /// second minute hour day : 延时处理的 指定延时数量(单位为对应的plan_date_type)
        /// week : 周日到周六(0-6),值无效系统自动放弃(无提示)
        /// month: 正数为指定号数(如当月不存在,则使用当月最后一天) 零或负数为月未倒推(0为最后一天,负数为减去的数字,减的结果为0或负数的,则为当前第一天)
        /// </remarks>
        public short plan_value;

        /// <summary>
        /// 重复次数,0不重复 >0重复次数,-1永久重复
        /// </summary>
        /// <remarks>
        /// none time 无效
        /// second minute hour day : 延时处理的,如指定时间太小,
        /// no_keep=true:
        ///     多次计算时间如果小于当前的,将密集执行
        /// no_keep=false:
        ///     每次空跳(系统不执行操作)也算一次,如时间足够小,可能全是空跳而未能执行一次,除非指定不允许空跳()
        /// </remarks>
        public int plan_repet;

        /// <summary>
        /// 计划说明
        /// </summary>
        public string description;


        /// <summary>
        /// 是否空跳
        /// </summary>
        public bool no_keep;

        /// <summary>
        /// 计划时间
        /// </summary>
        /// <remarks>
        /// 使用UNIX时间(1970年1月1日0时0分0秒起的总秒数)
        /// plan_type :
        ///  none 无效,系统自动分配当前时间以便下一次立即执行,plan_repet无效
        ///  time 指定时间,如时间过期,系统自动放弃,plan_repet无效
        ///  second minute hour day : 延时处理的,如指定,则以此时间为基准,否则以系统接收时间为基准(可能产生误差)
        ///  week month:指定日期的,只使用此时间的天内部分(即此时间可以明确指定引发的时间),如不指定,则时间为0:0:0
        /// </remarks>
        public int plan_time;

    }
}