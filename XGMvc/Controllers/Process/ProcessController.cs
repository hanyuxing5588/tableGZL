using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using Business.Common;
using Business.CommonModule;
namespace BaothApp.Controllers.Process
{
    public class ProcessController : SpecificController
    {

        public ViewResult FlowSuggest()
        {
            return View("suggest");
        }
        //流程


        /// <summary>
        /// 现金报销单        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            ViewData["ModelUrl"] = "xjbxd";
            return View("process");
        }

        /// <summary>
        /// 差旅报销单        /// </summary>
        /// <returns></returns>
        public ViewResult clbxd()
        {
            ViewData["ModelUrl"] = "clbxd";
            return View("process");
        }

        /// <summary>
        /// 支票申领单        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            ViewData["ModelUrl"] = "zpsld";
            return View("process");
        }

        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            ViewData["ModelUrl"] = "lwflkd";
            return View("process");
        }

        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            ViewData["ModelUrl"] = "lsggzd";
            return View("process");
        }

        /// <summary>
        /// 汇款审批单        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            ViewData["ModelUrl"] = "hkspd";
            return View("process");
        }

        /// <summary>
        /// 手续费报销单        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            ViewData["ModelUrl"] = "sxfbxd";
            return View("process");
        }

        /// <summary>
        /// 其他报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            ViewData["ModelUrl"] = "qtbxd";
            return View("process");
        }

        /// <summary>
        /// 期初报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            ViewData["ModelUrl"] = "qcbxd";
            return View("process");
        }

        /// <summary>
        /// 公务卡报销单        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            ViewData["ModelUrl"] = "gwkbxd";
            return View("process");
        }

        /// <summary>
        /// 公务卡汇总报销单        /// </summary>
        /// <returns></returns>
        public ViewResult gwkhzbxd()
        {
            ViewData["ModelUrl"] = "gwkhzbxd";
            return View("process");
        }

        /// <summary>
        /// 收入凭单
        /// </summary>
        /// <returns></returns>
        public ViewResult srpd()
        {
            ViewData["ModelUrl"] = "srpd";
            return View("process");
        }

        /// <summary>
        /// 财政收入
        /// </summary>
        /// <returns></returns>
        public ViewResult czsr()
        {
            ViewData["ModelUrl"] = "czsr";
            return View("process");
        }

        /// <summary>
        /// 应付单        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            ViewData["ModelUrl"] = "yfd";
            return View("process");
        }

        /// <summary>
        /// 应付单填制        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            ViewData["ModelUrl"] = "yfdtz";
            return View("process");
        }

        /// <summary>
        /// 应收单填制        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            ViewData["ModelUrl"] = "ysdtz";
            return View("process");
        }
        /// <summary>
        /// 预算编制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysbz()
        {
            ViewData["ModelUrl"] = "ysbz";
            return View("process");
        }
        /// <summary>
        /// 预算初始值
        /// </summary>
        /// <returns></returns>
        public ViewResult yscszsz()
        {
            ViewData["ModelUrl"] = "yscszsz";
            return View("process");
        }
        /// <summary>
        /// 专用基金列支单        /// </summary>
        /// <returns></returns>
        public ViewResult zyjjlzd()
        {
            ViewData["ModelUrl"] = "zyjjlzd";
            return View("process");
        }

        /// <summary>
        /// 借款单填制        /// </summary>
        /// <returns></returns>
        public ViewResult jkdtz()
        {
            ViewData["ModelUrl"] = "jkdtz";
            return View("process");
        }
        public JsonResult processGCY() 
        {
            Platform.Flow.Run.ResultMessager message = new Platform.Flow.Run.ResultMessager();
            message.Icon = Platform.Flow.Run.MessagerIconEnum.error;
            message.Title = "提示";
            message.Msg = "操作错误";
            Guid g = new System.Guid();
            var dataId = Request["Guid"];
            if (System.Guid.TryParse(dataId, out g))
            {
                var s = Platform.Flow.Run.WorkFlowAPI.GetProccessDoedContainsBackRecord(g, out message);
                if (message.Msg == null)
                {
                    return Json(s);
                }
                return Json(message);
            }
            return Json(message);
        }
        //流程
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
                try
                {
                    var s = Platform.Flow.Run.WorkFlowAPI.GetProccessDoedContainsBackRecord(g, out message);
                    if (message.Msg == null)
                    {
                        return Json(s);
                    }
                    return Json(message);
                }
                catch (Exception ex)
                {
                    return Json(ex.Message);;
                }
                
            }
            return Json(message);
        }
    }
}
