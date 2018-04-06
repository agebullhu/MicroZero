using Agebull.ZeroNet.ZeroApi;
using System;
using System.Diagnostics;
using System.Linq;

namespace RpcTest
{
    public class RegistTester
    {
        /// <summary>
        /// 注册方法
        /// </summary>
        public void RegistAction<TControler>()
            where TControler : class, new()
        {
            Debug.WriteLine($"//{typeof(TControler).FullName}");
            foreach (var method in typeof(TControler).GetMethods())
            {
                if (method == null) continue;
                var attributes = method.GetCustomAttributesData();
                var route = attributes?.FirstOrDefault(p => p.AttributeType == typeof(RouteAttribute));
                if (route == null)
                    continue;
                var name = route.ConstructorArguments.FirstOrDefault().Value?.ToString();
                var optionAtt = attributes.FirstOrDefault(p => p.AttributeType == typeof(ApiAccessOptionFilterAttribute));
                var option = ApiAccessOption.None;
                if (optionAtt != null && optionAtt.ConstructorArguments.Count > 0)
                    option = (ApiAccessOption)optionAtt.ConstructorArguments[0].Value;
                
                var ArgumenType = method.GetParameters().FirstOrDefault()?.ParameterType;
                if (ArgumenType == null)
                {
                    var action = method.CreateDelegate(typeof(Action<IApiResult>), new TControler());
                    //action.DynamicInvoke() as IApiResult;
                }
                else
                {
                    var action = method.CreateDelegate(typeof(ApiDelegate));
                    
                    var result =action.DynamicInvoke(new Argument<string> {Value = "test"}) as IApiResult;
                    //action.DynamicInvoke() as IApiResult;
                    Debug.WriteLine(result.Result);
                }
            }
        }

        void abc()
        {
            
        }
    }
}
