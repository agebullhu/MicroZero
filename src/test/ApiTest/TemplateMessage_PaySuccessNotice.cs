using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;

namespace ApiTest
{
    /*
模版ID
VfV9XCDf_u5bxc0fUD9uJSLH_qgphTuMFNzhPjGeozE
开发者调用模版消息接口时需提供模版ID
标题
预约挂号成功通知
行业
医疗护理 - 医药医疗
详细内容
{{first.DATA}}

姓名：{{patientName.DATA}}
性别：{{patientSex.DATA}}
预约医院：{{hospitalName.DATA}}
预约科室：{{department.DATA}}
预约医生：{{doctor.DATA}}
流水号：{{seq.DATA}}
{{remark.DATA}}
     */
    /// <summary>
    /// “预约挂号成功通知”模板消息数据定义
    /// </summary>
    public class AppointmentTemplateData : TemplateMessageBase
    {
        /// <summary>
        /// 标题
        /// </summary>
        public TemplateDataItem first { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public TemplateDataItem patientName { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public TemplateDataItem patientSex { get; set; }
        /// <summary>
        /// 预约医院
        /// </summary>
        public TemplateDataItem hospitalName { get; set; }
        /// <summary>
        /// 预约科室
        /// </summary>
        public TemplateDataItem department { get; set; }
        /// <summary>
        /// 预约医生
        /// </summary>
        public TemplateDataItem doctor { get; set; }
        /// <summary>
        /// 流水号
        /// </summary>
        public TemplateDataItem seq { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public TemplateDataItem remark { get; set; }

        /// <summary>
        /// “预约挂号成功通知”模板消息数据定义 构造函数
        /// </summary>
        public AppointmentTemplateData()
            : base("VfV9XCDf_u5bxc0fUD9uJSLH_qgphTuMFNzhPjGeozE", "https://mp.weixin.qq.com", "预约挂号成功通知")
        {
        }
    }
}