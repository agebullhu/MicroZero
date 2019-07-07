namespace MicroZero.Http.Gateway
{
    /// <summary>
    ///     Http配置类
    /// </summary>
    public class HttpOption
    {
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否Https
        /// </summary>
        public bool IsHttps { get; set; }

        /// <summary>
        /// 证书文件路径
        /// </summary>
        public string CerFile { get; set; }

        /// <summary>
        /// 证书密码
        /// </summary>
        public string CerPwd { get; set; }
    }
}