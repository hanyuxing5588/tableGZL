using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers.PrintReport
{
    public class PrintReportController : SpecificController
    {
        /// <summary>
        /// 报表打印
        /// </summary>
        /// 
        
        /// <summary>
        /// 管理费用执行进度表
        /// </summary>
        /// <returns></returns>
        public ViewResult glfyzxcx()
        {
            ViewData["ModelUrl"] = "glfyzxcx";
            return View("glfyzxcxprint");
        }


        public ViewResult PrintReport()
        {
            var template = Request["pturl"];
            return View(template);
        }

        
    }
}
