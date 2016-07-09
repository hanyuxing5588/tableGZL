using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using Business.Reimbursement;
using Business.Common;
using Business.Budget;

namespace BaothApp.Controllers.预算.预算分配
{   
    public class ysfpController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "ysfp"; }
        }


        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
         
        /// <summary>
        /// 主页
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            return View("ysfp");
        }

        /// <summary>
        /// 默认数据
        /// </summary>
        /// <returns></returns>
        public override JsonResult New()
        {
            JsonModel result = this.Actor.New();
            return Json(result);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }

        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);
            iRunWork.SetOperatorId(this.CurrentUserInfo.UserGuid);
            //拿到任务
            //根据任务去找到具体执行的对象
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow("ysfp", this.CurrentUserInfo.UserGuid, this.Guid, 94);
            if (iRunWork.ProcessStatus != 1)
            {
                //审批中
                this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            }
            else
            {
                //审批完成 
                this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approved);
            }
            return Json(message);
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

        public ContentResult GetFlowNode()
        {
            Business.Budget.预算分配 objYSFP = new Business.Budget.预算分配(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            List<object> result = objYSFP.GetFlowNode();
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        // 历史
        public override ContentResult History()
        {
            string condition = Request["condition"];
            ysfpCondition conditionModel = new ysfpCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<ysfpCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }

        public ContentResult SearchHistory()
        {
            string condition = Request["condition"];
            ysfpCondition conditionModel = new ysfpCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<ysfpCondition>(condition);
            Business.Budget.预算分配 objYSFP = new Business.Budget.预算分配(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            List<object> result = objYSFP.SearchHistory(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }

        public ContentResult SearchHistoryEx()
        {
            Business.Budget.预算分配 objYSFP = new Business.Budget.预算分配(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            List<object> result = objYSFP.SearchHistoryEx();
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        public ContentResult WorkFlow()
        {
            Business.Budget.预算分配 objYSFP = new Business.Budget.预算分配(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            List<object> result = objYSFP.GetWorkFlow();
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
