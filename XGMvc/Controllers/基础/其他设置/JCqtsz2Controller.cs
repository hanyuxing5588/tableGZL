using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Infrastructure;


namespace BaothApp.Controllers.基础.其他设置
{
    public partial class JCqtszController : SpecificController
    {
        private BaseConfigEdmxEntities context = null;
        
        /// <summary>
        /// 工资公式设置
        /// </summary>
        /// <returns></returns>
        public ActionResult gzgssz()
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SA_Plan.OrderBy(e => e.PlanKey).Select(e => new { e.PlanName, e.GUID }).ToList();
            //向ViewData中添加tab标签的名字

            ViewData["TabsGuid"] = list.Select(e => e.GUID).ToList();
            ViewData["TabsName"] = list.Select(e => e.PlanName).ToList();
            return View("gzgssz");
        }
        /// <summary>
        /// 预算公式设置
        /// </summary>
        /// <returns></returns>
        public ActionResult ysgssz()
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.BG_Setup.OrderBy(e => e.BGSetupKey).Select(e => new { e.BGSetupName, e.GUID }).ToList();
            //向ViewData中添加tab标签的名字

            ViewData["TabsGuid"] = list.Select(e => e.GUID).ToList();
            ViewData["TabsName"] = list.Select(e => e.BGSetupName).ToList();
            return View("ysgssz");
        }

        /// <summary>
        /// 预算默认值设置
        /// </summary>
        /// <returns></returns>
        public ActionResult ysmrzsz()
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.BG_Setup.OrderBy(e => e.BGSetupKey).Select(e => new { e.BGSetupName, e.GUID }).ToList();
            //向ViewData中添加tab标签的名字

            ViewData["TabsGuid"] = list.Select(e => e.GUID).ToList();
            ViewData["TabsName"] = list.Select(e => e.BGSetupName).ToList();
            return View("ysmrzsz");
        }


    }
}
