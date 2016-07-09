using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.CommonModule;
using Business.Approval;
using Business.IBusiness;
namespace BaothApp.Controllers.审批.待办任务

{   
    public class dbrwController : SpecificController
    {

        Business.IBusiness.IAgencyTask IATask = new Business.Approval.待办任务();

        public override string ModelUrl
        {
            get { return "dbrw"; }
        }


        /// <summary>
        /// 主页
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            return View("dbrw");
        }
        //启用
        //提交
        public JsonResult CommitDocFlow() 
        {
            var scope = Request["scope"];
            var docId = Request["DocId"];
            var classId = Request["ClassId"];
            Guid docGuid; int iClassId;
            if (!Guid.TryParse(docId, out docGuid)||!int.TryParse(classId,out iClassId))
            {
                return Json(new { msg = "参数不正确" }, JsonRequestBehavior.AllowGet);
            }
            if (docGuid == Guid.Empty || iClassId == 0) {
                return Json(new { msg = "参数不正确" }, JsonRequestBehavior.AllowGet);
            }
            var isSuceess = IATask.DocSumitWorkFlow(scope, this.CurrentUserInfo.UserGuid, docGuid, iClassId);
            return Json(new { isSuceess =isSuceess.Resulttype!=1, msg =isSuceess.Msg }, JsonRequestBehavior.AllowGet);
        }
        //退回
        public JsonResult SendBackDocFlow()
        {
            var scope = Request["scope"];
            var docId = Request["DocId"];
            var classId = Request["ClassId"];
            Guid docGuid; int iClassId;
            if (!Guid.TryParse(docId, out docGuid) || !int.TryParse(classId, out iClassId))
            {
                return Json(new { msg = "参数不正确" }, JsonRequestBehavior.AllowGet);
            }
            var isSuceess = IATask.DocSendBackWorkFlow(scope, this.CurrentUserInfo.UserGuid, docGuid,iClassId);
            return Json(new { isSuceess = true, msg = isSuceess.Msg }, JsonRequestBehavior.AllowGet);
        }

        //待办任务
        public JsonResult GetDataForTask()
        {
            var docTypeId = string.IsNullOrEmpty(Request["DocTypeId"] + "") ? Guid.Empty.ToString() : Request["DocTypeId"];
            var YWTypeKey =string.IsNullOrEmpty(Request["YWTypeKey"] + "")?"00": Request["YWTypeKey"];
            var docNum = Request["DocNum"];
            Guid docTypeGuid; int num;
            Guid.TryParse(docTypeId, out docTypeGuid) ;
            //if (Guid.TryParse(docTypeId, out docTypeGuid) || !int.TryParse(docNum, out num))
            //{
            //    return Json(new { msg = "参数不正确" }, JsonRequestBehavior.AllowGet);
            //}
            var data = IATask.GetFlowDataModel(YWTypeKey, docTypeGuid, this.CurrentUserInfo.UserGuid, docNum);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //未提交流程的单据
        public JsonResult GetDocDataWithNoFlow()
        {
            var data = IATask.GetDocData(this.CurrentUserInfo.UserGuid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
