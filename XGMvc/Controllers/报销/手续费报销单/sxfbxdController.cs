﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;

namespace BaothApp.Controllers.手续费报销单
{
    public class sxfbxdController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "sxfbxd"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            return View("sxfbxd");
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
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);

            //拿到任务
            //根据任务去找到具体执行的对象
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
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
            Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);
        }
        
    }
}
