using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.CommonModule;

namespace BaothApp.Controllers.报销.公务卡汇总报销单
{   
    public class gwkhzbxdController : SpecificController
    {

        public override string ModelUrl
        {
            get { return "gwkhzbxd"; }
        }
        /// <summary>
        /// 页面
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            return View("gwkhzbxd");
        }
        /// <summary>
        /// 参照
        /// </summary>
        /// <returns></returns>
        public ViewResult reference()
        {
            ViewData["ModelUrl"] = "gwkhzbxd";
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            return View("reference");
        }
        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult gwkhzbxd()
        {
            ViewData["ModelUrl"] = "gwkhzbxd";
            return View("reference");
        }
        /// <summary>
        /// 新建
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
        /// <summary>
        /// 明细
        /// </summary>
        /// <returns></returns>
        public override JsonResult Retrieve()
        {
            string strGUID = Request["guid"];
            Guid g;
            JsonModel result;
            if (Guid.TryParse(strGUID, out g))
            {
               result = this.Actor.Retrieve(this.Guid);
            }
            else
            {
                result = this.Actor.Retrieve(strGUID);
            }
            return Json(result);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);

            //拿到任务
            //根据任务去找到具体执行的对象
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 76);
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
        public override JsonResult SendBackFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 76);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);
        }
        /// <summary>
        /// 参照
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            this.ModelUrl = "gwkhzbxd";
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
