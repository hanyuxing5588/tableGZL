using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;
using Business.CommonModule;

namespace BaothApp.Controllers.报销.借款单填制
{
    public class jkdtzController : SpecificController
    {
        public override string ModelUrl
        {
            get
            {
                return "jkdtz";
            }
        }
        /// <summary>
        /// 页面
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            return View("jkdtz");
        }
        /// <summary>
        /// 打印
        /// </summary>
        /// <returns></returns>
        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
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

        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
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

        public override JsonResult CommitFlow()
        {
            var result = this.Actor.WorkFlowCommitVerify(this.Guid);
            if (result.Resulttype == 1)
            {
                return Json(result);
            }

            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);

            //拿到任务
            //根据任务去找到具体执行的对象
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 30);
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
            Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 30);
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

    }
}