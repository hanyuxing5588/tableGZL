using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using Business.Reimbursement;
using Business.Common;
using Business.Budget;
using CAE;
using System.Data;

namespace BaothApp.Controllers.预算.预算调整
{   
    public class ystzController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "ystz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //主界面
        public override ViewResult Index()
        {
            return View("ystz");
        }
        //选单
        public ViewResult selectDoc()
        {
            return View("selectDoc");
        }
        //public JsonResult ChangeDocData()
        //{
        //    string guid = Request["guid"];
        //    string docType = Request["docType"];
        //    var o= this.Actor.ChangeDocData(guid,docType,false);
        //    return Json(o);
        //}
       
        // 历史
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

        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
       
    }
}
