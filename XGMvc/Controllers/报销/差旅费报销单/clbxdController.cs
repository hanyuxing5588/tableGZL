using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;

namespace BaothApp.Controllers.差旅费报销单
{
    public class clbxdController : SpecificController
    {

        public override string ModelUrl 
        {
            get { return "clbxd"; }
        }
        /// <summary>
        /// 页面
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            return View("clbxd");
        }
        /// <summary>
        /// 明细
        /// </summary>
        /// <returns></returns>
        public  ViewResult MX()
        {
            return View("mx");
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
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
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