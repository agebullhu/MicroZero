using Agebull.EntityModel.Config;

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 扩展节点
    /// </summary>
    public class ExtendItem : NotificationObject
    {
        public ConfigBase Config { get; set; }
        
        public string Name { get; set; }

        public string Value
        {
            get => Config.Option[Name];
            set
            {
                if (value == Config.Option[Name])
                {
                    return;
                }
                Config.Option[Name] = value;
                RaisePropertyChanged(nameof(value));
            }
        }
    }
}