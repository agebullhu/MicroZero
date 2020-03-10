using System;
using System.Threading.Tasks;
using Agebull.Common.Logging;
using Agebull.MicroZero;
using Agebull.MicroZero.ZeroApis;


namespace ApiTest
{
    partial class Program0
    {
        static async Task Main(string[] args)
        {
            ZeroApplication.TestFunc = Test;
            ZeroApplication.CheckOption();
            ZeroApplication.Discove(typeof(TestController).Assembly);
            ZeroApplication.Initialize();
            await ZeroApplication.RunAwaiteAsync();
            LogRecorder.SystemLog("The end");
        }

        static string Test()
        {
            var result = ApiResult.Succees("IsOk");

            return JsonHelper.SerializeObject(result);
        }
    }


    //static void Main(string[] args)
    //{
    //    IocHelper.AddScoped<HpcBaseMySqlDb, HpcBaseMySqlDb>();
    //    ZeroApplication.CheckOption();
    //    //DataExtendChecker.Regist<SiteDataChecker, ISiteData>();
    //    //DataExtendChecker.Regist<SiteOrgDataChecker, ISiteOrgData>();
    //    ZeroApplication.Discove(typeof(Program).Assembly, "Hpc");
    //    ZeroApplication.Initialize();

    //    //var access = new ProductSkuDataAccess();
    //    //access.First();
    //    //Console.ReadKey();
    //    ZeroApplication.RunAwaite();
    //}

    //static void Test()
    //{
    //    var file = Path.Combine(StorageBillUnitExcel.BasePath, "temp", $"{RandomOperate.Generate(12)}.xlsx"); ;
    //    using (IocScope.CreateScope())
    //    {
    //        using (var bl = new StorageBillUnitExcel())
    //        {
    //            bl.SampleFile = "bill_import.xlsx";
    //            if (bl.Prepare() && bl.ImportBill(6534709894958813184))
    //            {
    //                bl.SaveToFile(file);
    //                Console.WriteLine(file);
    //            }
    //        }

    //        Console.WriteLine(GlobalContext.Current.LastMessage);
    //    }
    //    Console.WriteLine("End");
    //}

}
