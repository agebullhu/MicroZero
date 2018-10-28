using Newtonsoft.Json;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 保存用户的操作现场
    /// </summary>
    [JsonObject]
    public class UserScreen
    {
        /// <summary>
        /// 当前设计页面
        /// </summary>
        [JsonProperty]
        public string NowEditor { get; set; }
        /// <summary>
        /// 工作视图
        /// </summary>
        [JsonProperty]
        public string WorkView { get; set; }

        /// <summary>
        /// 高级视角
        /// </summary>
        public bool AdvancedView { get; set; }

        /// <summary>
        ///最后一次打开的文件
        /// </summary>
        public string LastFile { get; set; }
    }
}