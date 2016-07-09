using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Casher;
using Business.Reimbursement;
using Business.CommonModule;

namespace BaothApp.Controllers.出纳.支票管理
{
    public class zpglController : SpecificController
    {
        //zzp-2014-03-20
        public override string ModelUrl
        {
            get { return "zpgl"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            ViewData["status"] = 4;
            return View("zpgl");
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

        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);

        }

    }
}