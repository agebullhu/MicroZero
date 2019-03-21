// /***********************************************************************************************************************
// 工程：Agebull.EntityModel.Designer
// 项目：CodeRefactor
// 文件：DataAccessDesignModel.cs
// 作者：Administrator/
// 建立：2015－07－13 12:26
// ****************************************************文件说明**********************************************************
// 对应文档：
// 说明摘要：
// 作者备注：
// ****************************************************修改记录**********************************************************
// 日期：
// 人员：
// 说明：
// ************************************************************************************************************************
// 日期：
// 人员：
// 说明：
// ************************************************************************************************************************
// 日期：
// 人员：
// 说明：
// ***********************************************************************************************************************/

#region 命名空间引用

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Agebull.Common;
using Agebull.Common.Context;
using Agebull.Common.OAuth;
using Agebull.EntityModel.Designer.DataTemplate;
using Agebull.EntityModel.Common;
using Agebull.MicroZero;
using Agebull.MicroZero.ApiDocuments;
using Agebull.MicroZero.ZeroApi;
using Agebull.MicroZero.ZeroApis;
using Newtonsoft.Json;

#endregion

namespace Agebull.EntityModel.Designer
{
    /// <summary>
    /// 数据模型设计模型
    /// </summary>
    public sealed class DataModelDesignModel : ModelBase
    {
        private UserInfo _user;

        #region 设计对象 

        public TreeModel Tree { get; }


        public static DataModelDesignModel Current { get; set; }

        /// <summary>
        /// 构造
        /// </summary>
        public DataModelDesignModel()
        {
            Current = this;
            LoadUserScreen();
            if (User == null)
                User = new UserInfo();
            User.ContextJson = JsonConvert.SerializeObject(GlobalContext.Current, Formatting.Indented);
            Tree = new TreeModel
            {
                Model = this
            };
            Tree.PropertyChanged += Tree_PropertyChanged;
        }

        private void Tree_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TreeModel.SelectItem)) return;
            if (SelecTestItem?.IsModify ?? false)
                SaveApiScript();
            switch (Tree.SelectItem?.BindingObject)
            {
                case StationConfig st:
                    Station = st;
                    Document = null;
                    break;
                case ApiDocument doc:
                    Station = Tree.SelectItem.Parent.BindingObject as StationConfig;
                    Document = doc;
                    break;
                default:
                    Station = null;
                    Document = null;
                    break;
            }

            LoadApiScript();
            if (SelecTestItem != null)
                SelecTestItem.IsModify = false;
            RaisePropertyChanged(nameof(Station));
            RaisePropertyChanged(nameof(Document));
            RaisePropertyChanged(nameof(SelecTestItem));
        }

        #endregion

        #region 初始化

        /// <summary>
        ///     初始化
        /// </summary>
        protected override void DoInitialize()
        {
            base.DoInitialize();
            Tree.ViewModel = ViewModel;
            Tree.Dispatcher = Dispatcher;
            Tree.Initialize();
        }

        /// <summary>
        /// 同步解决方案变更
        /// </summary>
        public void OnSolutionChanged()
        {
            Tree.OnSolutionChanged();
            FirstSelect();

        }
        /// <summary>
        /// 保证载入后选择正常
        /// </summary>
        internal void FirstSelect()
        {
            Tree.SetSelect(null);
            //Tree.SetSelect(Tree.TreeRoot.Items[0]);
        }

        #endregion

        #region 测试
        public ApiTestItem SelecTestItem { get; set; }

        public StationConfig Station { get; set; }

        public ApiDocument Document { get; set; }


        internal void TestApi(object arg)
        {
            try
            {
                SelecTestItem.RequestId = RandomOperate.Generate(8);
                GlobalContext.Current.SetRequestContext(ZeroApplication.Config.ServiceKey,"", SelecTestItem.RequestId);
                User.ContextJson = JsonConvert.SerializeObject(GlobalContext.Current, Formatting.Indented);
                var client = new ApiClient
                {
                    Station = SelecTestItem.Station,
                    Commmand = SelecTestItem.Api,
                    Argument = SelecTestItem.Arguments
                };
                client.CallCommand();
                SelecTestItem.GlobalId = client.GlobalId;
                var result = JsonConvert.DeserializeObject<ApiResult>(client.Result);
                Tree.SelectItem.StatusIcon = result?.Success ?? false
                    ? Application.Current.Resources["async_Succeed"] as BitmapImage
                    : Application.Current.Resources["async_Faulted"] as BitmapImage;
                var obj = JsonConvert.DeserializeObject(client.Result);
                SelecTestItem.ResultJson = JsonConvert.SerializeObject(obj, Formatting.Indented);
                SaveApiScript();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        internal void ResetArgument(object arg)
        {
            try
            {
                if (SelecTestItem == null)
                {
                    MessageBox.Show("请选择一个API");
                    return;
                }

                SelecTestItem.Arguments = OutputJson(Document.ArgumentInfo);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        #endregion

        #region 测试脚本

        /// <summary>
        /// 用户操作现场
        /// </summary>
        internal void LoadApiScript()
        {
            if (Document == null)
            {
                SelecTestItem = new ApiTestItem
                {
                    Visibility = Visibility.Collapsed
                };
                return;
            }

            var folder = IOHelper.CheckPath(ZeroApplication.Config.DataFolder, "Scripts", Station.StationName);
            var file = Path.Combine(folder, Document.RouteName.Replace('/', '_') + ".json");
            if (!File.Exists(file))
            {
                CreateDefaultItem();
                return;
            }

            try
            {
                var json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json))
                {
                    CreateDefaultItem();
                    return;
                }

                SelecTestItem = JsonConvert.DeserializeObject<ApiTestItem>(json);
                if (SelecTestItem?.Api == null)
                    CreateDefaultItem();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                CreateDefaultItem();
            }
        }

        /// <summary>
        /// 用户操作现场
        /// </summary>
        internal void CreateDefaultItem()
        {
            SelecTestItem = new ApiTestItem
            {
                Visibility = Visibility.Visible,
                Station = Station.StationName,
                Api = Document.RouteName,
                Arguments = OutputJson(Document.ArgumentInfo),
                IsModify = false
            };
        }

        /// <summary>
        /// 用户操作现场
        /// </summary>
        internal void SaveApiScript()
        {
            if (SelecTestItem == null)
            {
                return;
            }

            var folder = IOHelper.CheckPath(ZeroApplication.Config.DataFolder, "Scripts", Station.StationName);
            var file = Path.Combine(folder, Document.RouteName.Replace('/', '_') + ".json");
            try
            {
                File.WriteAllText(file, JsonConvert.SerializeObject(SelecTestItem));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            SelecTestItem.IsModify = false;
        }

        #endregion

        #region 参数重构


        string OutputJson(TypeDocument field)
        {
            if (field == null)
                return "";
            StringBuilder sb = new StringBuilder("");
            OutputJson(field, sb);
            return sb.ToString();
        }
        void OutputJson(TypeDocument par, StringBuilder sb, int level = 0)
        {
            sb.AppendLine();
            sb.Append(' ', level * 4);
            sb.Append('{');
            bool first = true;
            foreach (var field in par.fields.Values)
            {
                if (first)
                    first = false;
                else
                    sb.Append(",");
                sb.AppendLine();
                sb.Append(' ', level * 4);

                sb.Append($@"""{field.JsonName ?? field.Name}"" : ");
                if (field.fields == null || field.fields.Count == 0)
                {
                    switch (field.TypeName)
                    {
                        case "string":
                            sb.Append($@"""{field.Example ?? field.Caption ?? field.Name}""");
                            break;
                        case "DateTime":
                            sb.Append($@"""{field.Example ?? "2018-01-01T12:00:00:00"}""");
                            break;
                        case "bool":
                            sb.Append(field.Example ?? "false");
                            break;
                        default:
                            sb.Append(field.Example ?? "0");
                            break;
                    }
                    continue;
                }
                if (field.IsEnum)
                {
                    sb.Append(field.fields.Values.FirstOrDefault()?.Value ?? "0");
                    continue;
                }
                OutputJson(field, sb, level + 1);
            }
            sb.AppendLine();
            sb.Append(' ', level * 4);
            sb.Append('}');
        }

        #endregion

        #region 用户上下文

        public UserInfo User
        {
            get => _user;
            set
            {
                _user = value;
                RaisePropertyChanged(nameof(User));
            }
        }


        /// <summary>
        /// 用户操作现场
        /// </summary>
        internal void LoadUserScreen()
        {
            var folder = IOHelper.CheckPath(ZeroApplication.Config.DataFolder, "Context");
            var file = Path.Combine(folder, "user.json");
            if (!File.Exists(file))
            {
                return;
            }
            try
            {
                var json = File.ReadAllText(file);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                User = JsonConvert.DeserializeObject<UserInfo>(json);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                CreateDefaultItem();
            }
        }

        /// <summary>
        /// 用户操作现场
        /// </summary>
        internal void SaveUserScreen()
        {
            if (User == null)
            {
                return;
            }
            var folder = IOHelper.CheckPath(ZeroApplication.Config.DataFolder, "Scripts", "Context");
            var file = Path.Combine(folder, "user.json");
            try
            {
                File.WriteAllText(file, JsonConvert.SerializeObject(User));
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        public void Anymouse(object obj)
        {
            if (User.DeviceId == null)
            {
                MessageBox.Show("请先获取DeviceId");
                return;
            }
            GlobalContext.SetUser(new LoginUserInfo
            {
                UserId = -1,
                Account = "anymouse",
                DeviceId = User.DeviceId,
                App = "*",
                Os = "*",
                LoginType = 0,
                State = UserStateType.None
            });
            GlobalContext.SetRequestContext(new RequestInfo
            {
                Token = GlobalContext.Customer.DeviceId
            });
            User.ContextJson = JsonConvert.SerializeObject(GlobalContext.Current, Formatting.Indented);
        }

        public void Customer(object obj)
        {
            if (User.Customer == null)
            {
                MessageBox.Show("请先登录");
                return;
            }
            GlobalContext.SetUser(User.Customer);
            GlobalContext.SetRequestContext(new RequestInfo
            {
                Token = GlobalContext.Customer.DeviceId
            });
            User.ContextJson = JsonConvert.SerializeObject(GlobalContext.Current, Formatting.Indented);
        }

        public void NoUser(object obj)
        {
            GlobalContext.SetUser(new LoginUserInfo
            {
                UserId = -1,
                Account = "anymouse",
                DeviceId = "*",
                App = "*",
                Os = "*",
                LoginType = 0,
                State = UserStateType.None
            });
            GlobalContext.SetRequestContext(new RequestInfo
            {
                Token = GlobalContext.Customer.DeviceId
            });
            User.ContextJson = JsonConvert.SerializeObject(GlobalContext.Current, Formatting.Indented);
        }

        void VerifyAccessToken()
        {
            try
            {
                SelecTestItem.RequestId = RandomOperate.Generate(8);
                GlobalContext.Current.SetRequestContext(ZeroApplication.Config.ServiceKey, "", SelecTestItem.RequestId);
                var client = new ApiClient
                {
                    Station = "Auth",
                    Commmand = "v1/verify/at",
                    Argument = JsonConvert.SerializeObject(new
                    {
                        Token = User.AccessToken
                    })
                };
                client.CallCommand();
                SelecTestItem.GlobalId = client.GlobalId;
                var result = JsonConvert.DeserializeObject<ApiResult<LoginUserInfo>>(client.Result);
                if (!result.Success)
                {
                    MessageBox.Show(result.Status?.ClientMessage, "校验AT", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                User.Customer = result.ResultData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "校验AT", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        void Login()
        {
            try
            {
                SelecTestItem.RequestId = RandomOperate.Generate(8);
                GlobalContext.Current.SetRequestContext(ZeroApplication.Config.ServiceKey, "", SelecTestItem.RequestId);
                GlobalContext.Customer.DeviceId = User.DeviceId;
                GlobalContext.RequestInfo.Token = User.DeviceId;
                var client = new ApiClient
                {
                    Station = "UserCenter",
                    Commmand = "v1/login/account",
                    Argument = JsonConvert.SerializeObject(new PhoneLoginRequest
                    {
                        MobilePhone = User.UserName,
                        UserPassword = User.PassWord
                    })
                };
                client.CallCommand();
                SelecTestItem.GlobalId = client.GlobalId;
                var result = JsonConvert.DeserializeObject<ApiResult<LoginResponse>>(client.Result);
                if (!result.Success)
                {
                    MessageBox.Show(result.Status?.ClientMessage, "登录", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
                User.AccessToken = result.ResultData.AccessToken;
                User.RefreshToken = result.ResultData.RefreshToken;
                VerifyAccessToken();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "登录", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        string LoadDeviceId()
        {
            try
            {
                SelecTestItem.RequestId = RandomOperate.Generate(8);
                GlobalContext.Current.SetRequestContext(ZeroApplication.Config.ServiceKey, "", SelecTestItem.RequestId);
                var client = new ApiClient
                {
                    Station = "UserCenter",
                    Commmand = "v1/refresh/did"
                };
                client.CallCommand();
                SelecTestItem.GlobalId = client.GlobalId;
                var result = JsonConvert.DeserializeObject<ApiResult<string>>(client.Result);
                if (!result.Success)
                {
                    MessageBox.Show(result.Status?.ClientMessage, "获取DeviceId", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return null;
                }

                return result.ResultData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "获取DeviceId", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        #endregion

        public void Login(object obj)
        {
            Login();
            SaveUserScreen();
        }
        public void VerifyAccessToken(object obj)
        {
            VerifyAccessToken();
            SaveUserScreen();
        }
        public void LoadDeviceId(object obj)
        {
            User.DeviceId = LoadDeviceId();
            SaveUserScreen();
        }
    }
}