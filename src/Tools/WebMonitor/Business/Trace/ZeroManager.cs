using System;
using System.Collections.Generic;
using System.Linq;
using Agebull.Common.Rpc;
using Agebull.ZeroNet.Core;
using Agebull.ZeroNet.PubSub;
using Agebull.ZeroNet.ZeroApi;
using Gboxt.Common.DataModel;
using Newtonsoft.Json;
using WebMonitor.Models;

namespace WebMonitor.Models
{

    /// <summary>
    ///     管理类
    /// </summary>
    internal class ZeroTraceSub : SubStation
    {
        public override void Handle(PublishItem args)
        {
            throw new NotImplementedException();
        }
    }
}