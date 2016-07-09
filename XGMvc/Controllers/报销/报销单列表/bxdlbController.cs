using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;
using Business.CommonModule;

namespace BaothApp.Controllers.报销.报销单列表
{   
    public class bxdlbController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "bxdlb"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            return View("bxdlb");
        }

        public ViewResult filter() 
        {
            ViewData["ModelUrl"] = "filter";
            return View("filter");
        }

        /// <summary>
        /// 保存功能
        /// </summary>
        /// <returns></returns>
        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 报销单数据列表数据
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            BX_BillListCondition conditionModel = new BX_BillListCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BX_BillListCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);  
        }
       

    }
}
