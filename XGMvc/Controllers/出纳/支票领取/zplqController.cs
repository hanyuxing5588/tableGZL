using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Casher;
using Business.Reimbursement;
using Business.CommonModule;
using Business.CommonModule.Search;

namespace BaothApp.Controllers.出纳.支票领取
{
    public class zplqController : SpecificController
    {
        //zzp-2014-03-19
        public override string ModelUrl
        {
            get { return "zplq"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            return View("zplq");
        }

        public override JsonResult New()
        {

            JsonModel result = this.Actor.New();
            return Json(result);
        }
        public JsonResult ChangeCheck() 
        {
            string GuidStr = Request["guid"];
            string ywkey = Request["ywkey"];
           // string checkGuidStr = Request["checkguid"];
            Guid docGuid=Guid.Empty, checkGuid = Guid.Empty; ;
            //if (!Guid.TryParse(docGuidStr, out docGuid) ) {
            //    Json(new { msg="参数传递错误"}); 
            //}
            //if (!string.IsNullOrEmpty(checkGuidStr))
            //{
            //    Guid.TryParse(checkGuidStr,out checkGuid);
            //}
            Business.Casher.支票领取 zplq = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            var result = zplq.ChangeCheck(GuidStr,ywkey);
            return Json(result);
        }
        public override JsonResult Save()
        {
            Business.Casher.支票领取 result = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            string ywKey = Request["ywkey"];
            string data=Request["data"];
            ResponseJsonMessage msg = result.Save(ywKey, data);
            if (msg.result == "success")
            {
                /*提交流程*/
                Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
                var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
                iRunWork.SetTasks(listTasks);
                //拿到任务
                //根据任务去找到具体执行的对象
                Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow("zplq", this.CurrentUserInfo.UserGuid,msg.Id, 23, "");
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
            }
           return Json(msg);
        }
        /// <summary>
        /// 根据支票GUID
        /// </summary>
        /// <returns></returns>
        public override JsonResult Retrieve()
        {
            string guid=Request["guid"];
            Business.Casher.支票领取 obj = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
           object result=obj.Retrieve(guid);
            return Json(result);
        }
        /// <summary>
        /// 选择单据后调用此函数
        /// </summary>
        /// <returns></returns>
        public  JsonResult YWRetrieve()
        {
            string guid = Request["guid"];
            Guid g;
            if (!Guid.TryParse(guid, out g)) {
                return null;
            }
            string yekey=Request["ywkey"];
            Business.Casher.支票领取 obj = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            object result = obj.Retrieve(g, yekey);
            return Json(result);
        }

        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);

        }

        /// <returns></returns>
        public ViewResult SelectDoc()
        {
            ViewData["ModelUrl"] = "zplqdj";
            return View("zplqdj");
        }

        public JsonResult GetZPLQ()
        { 
            var guid=Request["guid"];
            Business.Casher.支票领取 obj = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            var list = obj.GetTreeZPLQ(guid);
            return Json(list,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetZPLQXJ()
        {
            var guid = Request["guid"];
            Business.Casher.支票领取 obj = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            var list = obj.GetTreeZPLQ(guid,true);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult RetrieveModelByGuid()
        {
            var guid = Request["guid"];
            Guid g;
            if (Guid.TryParse(guid, out g))
            {
                Business.Casher.支票领取 obj = new Business.Casher.支票领取(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                var list = obj.RetrieveModelByGuid(g);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return null;
        }

        /// <summary>
        /// 选单
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<ZPLQSelectDocCondition>(condition);
            var dls = new ZPLQSearch(conditionModel, this.CurrentUserInfo.UserGuid);
            var list = dls.GetResult();
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }

    }
}