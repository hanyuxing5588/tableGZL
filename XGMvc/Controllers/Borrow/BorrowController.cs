using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using Business.Common;
using Business.CommonModule;

namespace BaothApp.Controllers.borrow
{
    //无特殊性  改为通用的 像明细
    public class BorrowController : SpecificController
    {
        /// <summary>
        /// 借款表        /// </summary>
        public override string ModelUrl
        {
            get { return "borrow"; }
        }
        
        /// <summary>
        /// 支票申领单
        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "zpsld";
            return View("Borrow");
        }
         /// <summary>
        /// 差旅报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult clbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "clbxd";
            return View("Borrow");
        }
        /// <summary>
        /// 公务卡报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "gwkbxd";
            return View("Borrow");
        }
        /// <summary>
        /// 汇款审批单
        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "hkspd";
            return View("Borrow");
        }
        /// <summary>
        /// 其他报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "qtbxd";
            return View("Borrow");
        }
        /// <summary>
        /// 初期报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "qcbxd";
            return View("Borrow");
        }
        /// <summary>
        /// 临时工工资单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "lsggzd";
            return View("Borrow");
        }
        /// <summary>
        /// 现金报销单        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "xjbxd";
            return View("Borrow");
        }
        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "lwflkd";
            return View("Borrow");
        }
        /// <summary>
        /// 手续费报销单        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "sxfbxd";
            return View("Borrow");
        }

        /// <summary>
        /// 应付单
        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "yfd";
            return View("Borrow");
        }
        /// <summary>
        /// 应收单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "ysdtz";
            return View("Borrow");
        }
        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "yfdtz";
            return View("Borrow");
        }
        /// <summary>
        /// 现金存储

        /// </summary>
        /// <returns></returns>
        public ViewResult xjcc()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "xjcc";
            return View("Borrow");
        }

        /// <summary>
        /// 内部调账单

        /// </summary>
        /// <returns></returns>
        public ViewResult nbdzd()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["startDate"] = DateTime.Now.Year + "-01-01";
            ViewData["ModelUrl"] = "nbdzd";
            return View("Borrow");
        }

       
        /// <summary>
        /// 借款
        /// </summary>
        /// <returns></returns>
        public ViewResult BorrowView()
        {
            return View("Borrow");
        }
        /// <summary>
        /// 借款
        /// </summary>
        /// <returns></returns>
        public JsonResult BorrowMoney()
        {
            //this.Actor = BaseDocument.CreatInstance(this.ModelUrl, this.CurrentUserInfo.UserGuid);
            string condition = Request["condition"];
            BorrowMoneyCondition conditionModel = new BorrowMoneyCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BorrowMoneyCondition>(condition);
            var result = this.Actor.BorrowMoney(conditionModel);
            return Json(result);
        }
    }
}
