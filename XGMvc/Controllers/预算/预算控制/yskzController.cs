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

namespace BaothApp.Controllers.预算.预算控制
{   
    public class yskzController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "yskz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //主界面
        public override JsonResult New()
        {
            JsonModel result = this.Actor.New();
            return Json(result);
        }
        public override ViewResult Index()
        {
            return View("yskz");
        }
        //选单
        public ViewResult selectDoc()
        {
            return View("selectDoc");
        }
        public JsonResult SaveControlMain()
        {
            var CMYear = Request["CMYear"];
            var Dw = Request["Dw"];
            var Department = Request["Department"];
            var ControlWaykey = Request["ControlWaykey"];
            var Project = Request["Project"];
            var Detail = Request["Detail"];
            var Maker = Request["Maker"];
            var MakeDate = Request["MakeDate"];
            var State = Request["iState"];
            var MoneyUnit = Request["MoneyUnit"];
            Business.Budget.预算控制 objYSKZ = new Business.Budget.预算控制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            VerifyResult vr = new VerifyResult();
            JsonModel json = objYSKZ.SaveControlMain(CMYear, Dw, Department, ControlWaykey, Project, Detail, Maker, MakeDate, State, MoneyUnit);
            return Json(json); ;
        }
        public ContentResult LoadDetail()
        {
            var CMYear = Request["CMYear"];
            var ControlWayKey = Request["ControlWayKey"];
            var GUID_DW = Request["GUID_DW"];
            var GUID_Department = Request["GUID_Department"];
            var GUID_Project = Request["GUID_Project"];
            var GUID_Control_Main = Request["GUID_Control_Main"];

            Business.Budget.预算控制 objYSKZ = new Business.Budget.预算控制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            string strJson = objYSKZ.LoadDetail(CMYear, ControlWayKey, GUID_DW, GUID_Department, GUID_Project, GUID_Control_Main);
            ContentResult result = Content(strJson);
            return Content(strJson);
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
