using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using Business.Common;
using Business.CommonModule;
namespace BaothApp.Controllers.dj
{
    public class djController : SpecificController
    {
        //--zzp 2014-03-19
        //单据(选单)
        public override string ModelUrl
        {
            get { return "dj"; }
        }

        /// <summary>
        /// 支票领取
        /// </summary>
        /// <returns></returns>
        public ViewResult zplq()
        {
            ViewData["ModelUrl"] = "zplq";
            return View("zplqdj");
        }
        
        //单据
        public JsonResult dj()
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
