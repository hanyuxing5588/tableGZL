using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Casher;
using Business.Reimbursement;
using Business.CommonModule;
using BaothApp.Utils;

namespace BaothApp.Controllers.报销.现金提取
{
    public class xjtqController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "xjtq"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            ViewData["ModelUrl"] = "xjtq";
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            return View("xjtq");
            //return View("GCYDemand");
        }
        public  ViewResult XJTQ()
        {
            var dataGuid = Request["datagrid"];
            if (dataGuid != null)
            {
                ViewData["dataguid"] = dataGuid;
            }
            var dataType = Request["datatype"];
            if (dataType != null)
            {
                ViewData["datatype"] = dataType;
            }
            return View("xjtq");
        }
        /// <summary>
        /// 需求
        /// </summary>
        public ViewResult Demand()
        {
            ViewData["ModelUrl"] = "xjtq";
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"]=DateTime.Now.Year+"-01-01";
            return View("Demand");
        }
        public override JsonResult New()
        {

            JsonModel result = this.Actor.New();
            return Json(result);
        }

        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }

        public override JsonResult Retrieve()
        {            
           JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        public JsonResult RetrieveMoneyDoc()
        
        {
            var strGuid = Request["guid"];
            List<YWTypeDataModel> list = JsonHelper.JsonToObject<List<YWTypeDataModel>>(strGuid);           
            var obj = new Business.Casher.现金提取(this.CurrentUserInfo.UserGuid,this.ModelUrl);
            JsonModel result = obj.RetrieveMoneyDocData(list);
            
            //foreach (DocTypeDataModel item in list)
            //{                
            //    //this.ModelUrl = item.DocTypeUrl;
            //    var actor = BaseDocument.CreatInstance(item.DocTypeUrl, this.CurrentUserInfo.UserGuid);
            //    JsonModel result = actor.Retrieve(item.GUID);
            //    if (result.d != null && result.d.Count > 0)
            //    {
            //        jsonModel.d.AddRange(result.d);
            //    }
            //}
            return Json(result);
        }

        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);

        }

        public ViewResult viewProcess()
        {
            return View("process");
        }
        public JsonResult process()
        {
            Platform.Flow.Run.ResultMessager message = new Platform.Flow.Run.ResultMessager();
            message.Icon = Platform.Flow.Run.MessagerIconEnum.error;
            message.Title = "提示";
            message.Msg = "操作错误";
            Guid g = new System.Guid();
            var dataId = Request["Guid"];
            if (System.Guid.TryParse(dataId, out g))
            {
                var s = Platform.Flow.Run.WorkFlowAPI.GetProccessDoed(g, out message);
                if (message.Msg == null)
                {
                    return Json(s);
                }
                return Json(message);
            }
            return Json(message);
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            HistoryCondition conditionModel = new HistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<HistoryCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
       
    }
}