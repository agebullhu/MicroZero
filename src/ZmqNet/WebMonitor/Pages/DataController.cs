using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace WebMonitor.Pages
{
    [Produces("application/json")]
    [Route("api/Data")]
    public class DataController : Controller
    {
        [HttpGet]
        public JsonResult from()
        {
            var value = new test
            {
                name = "resetForm",
                region = "shanghai",
                date1 = DateTime.Today,
                date2 = DateTime.Now,
                type = new List<string>
                {
                    "地推活动",
                    "线下主题活动"
                },
                delivery = false,
                resource = "",
                desc = ""
            };
            return new JsonResult(value);
        }

        public class test
        {
            public string name;
            public string region;
            public DateTime date1;
            public DateTime date2;
            public bool delivery = false;
            public string resource;
            public string desc;
            public List<string> type;
        }
    }
}