using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;

namespace BaothApp.Controllers.出纳.出纳工作台
{   
    public class cngztController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "cngzt"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //核销主界面
        public override ViewResult Index()
        {
            return View("cngzt");
        }
        //选单
        public ViewResult selectDoc()
        {
            return View("selectDoc");
        }
        public JsonResult ChangeDocData()
        {
            string guid = Request["guid"];
            string docType = Request["docType"];

            //var o= this.Actor.ChangeDocData(guid,docType,false);
           // return Json(o);
            return null;
        }
       
        // 选单列表查询
        public override ContentResult History()
        {
            string condition = Request["condition"];
            BillHistoryCondition conditionModel = new BillHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BillHistoryCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }

        /// <summary>
        /// 借款选单列表
        /// </summary>
        /// <returns></returns>
        public  ContentResult BorrowList()
        {
            //string condition = Request["condition"];
            //BillHistoryCondition conditionModel = new BillHistoryCondition();
            //conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BillHistoryCondition>(condition);
            //List<object> result = this.Actor.History(conditionModel);
            //string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            //string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            //return Content(json);
            return null;
        }       
       
    }
}
