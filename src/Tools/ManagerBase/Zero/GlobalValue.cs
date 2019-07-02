using System;
using System.Collections.Generic;
using Agebull.MicroZero.ApiDocuments;

namespace WebMonitor.Models
{
    public class GlobalValue
    {

        /// <summary>
        /// 静态文档
        /// </summary>
        public static readonly Dictionary<string, StationDocument> Documents = new Dictionary<string, StationDocument>(StringComparer.OrdinalIgnoreCase);

    }
}