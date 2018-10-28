using Agebull.Common.Rpc;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.Text.RegularExpressions;

namespace ApiTest
{
    /// <summary>
    /// 登录参数
    /// </summary>
    public class MachineEventArg : IApiArgument
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 机器标识
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public string JsonStr { get; set; }
        
        /// <summary>
        /// 校验
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool IApiArgument.Validate(out string message)
        {
            message = null;
            return true;
        }
    }

    public class OpenDoorArg
    {
        public string CompanyId {get;set;}//公司id
        public string UserType {get;set;}//用户类型
        public string UserId {get;set;}//用户id
        public string DeviceId {get;set;}//设备id
        public string RecordDate{get;set;}//记录时间
        public string RecordUserStatus{get;set;}//状态
        public string InOrOut{get;set;}//进或出
        public string EnterType{get;set;}//进出方式
        public string PhotoUrl{get;set;}//人脸照
        public string IdentityImageUrl{get;set;}//证件照
        public string PanoramaUrl{get;set;}//全景照
        public string Score{get;set;}//识别系数

    }
}
