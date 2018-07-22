## 开发你的第一个ZeroApi

###  一 新建Project
1 控制台程序(注意不是Asp.net core)

2 Core版本 : 不低于于2.0.3

###  二 引用Nuget包

1. ZeroNet.core
2. AgebullExtend.core : 隐式引用
3. Agebull.LogRecorder : 隐式引用
4. Agebull.EntityModel.Core : 隐式引用
5. ... 其它依赖

### 三 编码
####  Program.cs:

``` Csharp
using Agebull.Common.Ioc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    partial class Program
    {
        static void Main(string[] args)
        {
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(AutoRegister).Assembly);
            ZeroApplication.Initialize();
            ZeroApplication.RunAwaite();
        }
    }
}
```
代码说明:

1 ZeroApplication.CheckOption();
 必须是第一句,它会处理你的所有配置文件

2 ZeroApplication.Discove(typeof(AutoRegister).Assembly);
 
 这句的目的是自动发现对应程序集中的 ZeroObject对象(如ApiStation\SubStation\PubStation) ,如果存在API站点,将处理以下几件事:

- 生成动态程序集,注入到API调用链中
- 分析你的接口与对应注释,生成帮助文件.
- ZeroApplication.Initialize();
 初始化发现的ZeroObject对象,以保证能正常运行

3 ZeroApplication.RunAwaite();

 运行站点,并在接收到 ctr+c中止信号时正确关闭站点，其中会处理如下事情：

 - 3.1 与ZeroCenter握手 , 如果失败,则跳过3.2-3.3步,直接执行第4步
 - 3.2 获取所有站点配置
 - 3.3 启动每一个站点
   3.3.1 检查是否存在对应站点配置文件，在配置文件不存在时
   3.3.1.1  如果是ApiStation，会执行自动安装后继续运行(注意会分配一组端口地址)
   3.3.1.2 如果是其它，则不继续运行站点
 - 3.4 运行SystemMonitor,用于接收系统消息
  - 接收心跳事件,以指引站点向ZeroCenter报告状态
  - 接收站点状态事件
  - 接收站点暂停,继续,关闭,卸载事件,并进行相应处理
  - 接收ZeroCenter 关闭与启动事件,并进行相应处理

> 如果站点启动时未连接到Center或是曾经接收到Center关闭事件,收到启动事件后,会自动启动所有站点(执行3.2-3.3步骤)

> 如果接收到Center关闭事件,会关闭所有站点(未来可能取消此行为)

#### ApiControl
``` csharp
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.ComponentModel;

namespace ApiTest
{
    /// <summary>
    /// 登录服务
    /// </summary>
    [Station("Login")]
    public class LoginStation : ZeroApiController
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>登录状态</returns>
        [Route("api/login"), Category("登录")]
       [ApiAccessOptionFilter(ApiAccessOption.Anymouse | ApiAccessOption.Public)]
        public ApiResult Login(LoginArg user)
        {
            return new ApiResult
            {
                Success = true,
                Status = new ApiStatsResult
                {
                    ErrorCode = 0,
                    ClientMessage = $"Wecome {user.MobilePhone}!"
                }
            };
        }
    }
}
``` 
> 与WebApi基本兼容
 
##### 1 继承于ZeroApiController
##### 2 Station特性 
 - 如果此Controler的站点名称不同于配置文件中的名称,可在此独立命名.
 - 如无此特性,将合并到配置文件对应的StationName的站点(我们称为全局站点).
 - 限制,如果用全局站点,每一个Controler中的方法名称不能相同

#####  3 api定义
- 必须为实例方法(非static)
- public (当前未限制,但不保证未来会进行限制,所以请遵行此原则)
- Route特性 : 与WebApi相同,即暴露的Api名称,必须存在
- Category 特性 : 用于在生成文档时进行分类
- ApiAccessOptionFilter特性 : 配置此API的访问权限
```csharp
    /// <summary>
    /// API访问配置
    /// </summary>
    [Flags]
    public enum ApiAccessOption
    {
        /// <summary>
        /// 不可访问
        /// </summary>
        None,
        /// <summary>
        /// 公开访问
        /// </summary>
        Public = 0x1,
        /// <summary>
        /// 内部访问
        /// </summary>
        Internal = 0x2,

        /// <summary>
        /// 游客
        /// </summary>
        Anymouse = 0x4,
        /// <summary>
        /// 客户
        /// </summary>
        Customer = 0x10,
        /// <summary>
        /// 内部员工
        /// </summary>
        Employe = 0x20,
        /// <summary>
        /// 商家用户
        /// </summary>
        Business = 0x40,
        /// <summary>
        /// 扩展用户性质3
        /// </summary>
        User1 = 0x80,
        /// <summary>
        /// 扩展用户性质2
        /// </summary>
        User2 = 0x100,
        /// <summary>
        /// 扩展用户性质3
        /// </summary>
        User3 = 0x200,
        /// <summary>
        /// 扩展用户性质4
        /// </summary>
        User4 = 0x400,
        /// <summary>
        /// 扩展用户性质5
        /// </summary>
        User5 = 0x800,
        /// <summary>
        /// 扩展用户性质6
        /// </summary>
        User6 = 0x1000,
        /// <summary>
        /// 扩展用户性质7
        /// </summary>
        User7 = 0x4000,
        /// <summary>
        /// 扩展用户性质8
        /// </summary>
        User8 = 0x8000,

        /// <summary>
        /// 参数可以为null
        /// </summary>
        ArgumentCanNil = 0x10000
    }
```

##### 4 Api 参数

 - 只允许一个参数且继承于IApiArgument
 - IApiArgument.Validate : 如果有参数的校验,应在此处实现
 > 早于进入API时处理,并在校验不通过的情况直接返回错误
 > 返回参数校验错误时,不会进入API处理方法

``` csharp
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using System.Text.RegularExpressions;

namespace ApiTest
{
    /// <summary>
    /// 登录参数
    /// </summary>
    public class LoginArg : IApiArgument
    {
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value>11位手机号,不能为空</value>
        /// <example>15618965007</example>
        [DataRule(CanNull = false, Max = 11, Min = 11, Regex = "1[3-9]\\d{9,9}")]
        public string MobilePhone { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        /// <value>6-16位特殊字符\字母\数字组成,特殊字符\字母\数字都需要一或多个,不能为空</value>
        /// <example>pwd#123</example>
        [DataRule(CanNull = false, Max = 6, Min = 16, Regex = "[\\da-zA-Z~!@#$%^&*]{6,16}")]
        public string UserPassword { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        /// <value>6位字母或数字,不能为空</value>
        /// <example>123ABC</example>
        [DataRule(CanNull = false, Max = 6, Min = 6, Regex = "[a-zA-Z\\d]{6,6}")]
        public string VerificationCode { get; set; }
        
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
}

```
当只有一个参数时,可使用Argument类
- Argument : 非泛型版本,Value为string类型参数
- Argument<> : 泛型版本,Value可为各种值类型参数

###### 注释与Api文档的关系

1. summary : 对应文档的名称
2. remarks : 对应文档的说明
3. example : 对应文档的示例内容
4. value : 对应文档的字段规则文字说明
4. DataRule特性 : 对应文档中的数据规则详细说明
 注意 这与校验无关,无关,无关.只是为了文档

##### 5 Api返回值
> 继承于IApiResult

``` csharp

namespace Gboxt.Common.DataModel
{
    /// <summary>
    /// 表示API返回数据
    /// </summary>
    public interface IApiResultData
    {
    }
    /// <summary>
    /// API状态返回（一般在出错时发生）
    /// </summary>
    public interface IApiStatusResult
    {
        /// <summary>
        /// 错误码（系统定义）
        /// </summary>
        [JsonProperty("code")]
        int ErrorCode { get; set; }

        /// <summary>
        /// 对应HTTP错误码（参考）
        /// </summary>
        [JsonProperty("http")]
        string HttpCode { get; set; }

        /// <summary>
        /// 客户端信息
        /// </summary>
        [JsonProperty("msg")]
        string ClientMessage { get; set; }

        /// <summary>
        /// 内部信息
        /// </summary>
        string InnerMessage { get; set; }
    }
    /// <summary>
    /// API返回基类
    /// </summary>
    public interface IApiResult
    {
        /// <summary>
        /// 成功或失败标记
        /// </summary>
        [JsonProperty("success")]
        bool Success { get; set; }

        /// <summary>
        /// API执行状态（为空表示状态正常）
        /// </summary>
        [JsonProperty("status")]
        IApiStatusResult Status { get; set; }
    }
    /// <summary>
    /// API返回基类
    /// </summary>
    public interface IApiResult<out TData> : IApiResult
        where TData : IApiResultData
    {
        /// <summary>
        /// 返回值
        /// </summary>
        [JsonProperty("data")]
        TData ResultData { get; }
    }
}

```
特殊说明:

- 为保持不同语言之间的风格独立,Json序列化使用的Name与CSharp是不相同的,调试时需要特别注意
- 以上接口,均有相应的各种实现,可直接使用,无需再造车轮
 - ApiResult : 仅返回状态而无数据时使用
 - ApiValueResult : 非泛型类,返回值为string类型
 - ApiValueResult<TData> : 泛型类,返回值为任何对象
 - ApiArrayResult<TData> : 泛型类,返回值为对象数组
 - ApiPageResult<TData> : 泛型类,返回值为分页数据





















