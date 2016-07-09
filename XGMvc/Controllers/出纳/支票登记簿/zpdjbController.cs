using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Casher;
using Business.Reimbursement;
using Business.CommonModule;

namespace BaothApp.Controllers.出纳.支票登记簿
{
    public class zpdjbController : SpecificController
    {
        //zzp-2014-03-20
        public override string ModelUrl
        {
            get { return "zpdjb"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            return View("zpdjb");
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
        public override ContentResult History()
        {
            string condition = Request["condition"];
            if (condition == null)
            {
                condition = "{\"CheckNumber\":\"\",\"CheckType\":\"all\"}";
            }
            CheckHistoryCondition conditionModel = new CheckHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<CheckHistoryCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);           
        }
        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);

        }

    }
}