using Agebull.ZeroNet.ZeroApi;

namespace ApiTest
{
    partial class Program
    {
        public class Arg : IApiArgument
        {
            string IApiArgument.ToFormString()
            {
                return "";
            }

            bool IApiArgument.Validate(out string message)
            {
                message = null;
                return true;
            }
        }
    }
}
