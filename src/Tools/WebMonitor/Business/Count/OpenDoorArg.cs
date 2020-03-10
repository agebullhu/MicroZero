namespace ZeroNet.Http.Route
{
    public class OpenDoorArg
    {
        public string CompanyId { get; set; }//公司id
        public string UserType { get; set; }//用户类型
        public long UserId { get; set; }//用户id
        public string DeviceId { get; set; }//设备id
        public string RecordDate { get; set; }//记录时间
        public string RecordUserStatus { get; set; }//状态
        public string InOrOut { get; set; }//进或出
        public string EnterType { get; set; }//进出方式
        public string PhotoUrl { get; set; }//人脸照
        public string IdentityImageUrl { get; set; }//证件照
        public string PanoramaUrl { get; set; }//全景照
        public string Score { get; set; }//识别系数

    }
}