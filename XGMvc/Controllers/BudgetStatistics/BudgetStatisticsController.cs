using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using Business.CommonModule;

namespace BaothApp.Controllers.BudgetStatistics
{
    public class BudgetStatisticsController : SpecificController
    {
        /// <summary>
        /// 预算表        /// </summary>
        public override string ModelUrl
        {
            get { return "budgetstatistics"; }
        }
        /// <summary>
        /// 支票申领单        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            ViewData["ModelUrl"] = "zpsld";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 差旅报销单        /// </summary>
        /// <returns></returns>
        public ViewResult clbxd()
        {
            ViewData["ModelUrl"] = "clbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 公务卡报销单        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            ViewData["ModelUrl"] = "gwkbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 汇款审批单        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            ViewData["ModelUrl"] = "hkspd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 其它报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            ViewData["ModelUrl"] = "qtbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 初期报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            ViewData["ModelUrl"] = "qcbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 临时工工资单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            ViewData["ModelUrl"] = "lsggzd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 现金报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            ViewData["ModelUrl"] = "xjbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            ViewData["ModelUrl"] = "lwflkd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 手续费报销单        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            ViewData["ModelUrl"] = "sxfbxd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 应付单
        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            ViewData["ModelUrl"] = "yfd";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 应收单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            ViewData["ModelUrl"] = "ysdtz";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            ViewData["ModelUrl"] = "yfdtz";
            return View("budgetstatistics");
        }
        /// <summary>
        /// 现金提取

        /// </summary>
        /// <returns></returns>
        public ViewResult xjcc()
        {
            ViewData["ModelUrl"] = "xjcc";
            return View("budgetstatistics");
        }

        /// <summary>
        /// 内部调账单

        /// </summary>
        /// <returns></returns>
        public ViewResult nbdzd()
        {
            ViewData["ModelUrl"] = "nbdzd";
            return View("budgetstatistics");
        }

        /// <summary>
        /// 预算
        /// </summary>
        /// <returns></returns>
        public ViewResult BudgetStatisticsView()
        {
            return View("BudgetStatistics");
        }

        /// <summary>
        /// 预算
        /// </summary>
        /// <returns></returns>
        public JsonResult BudgetStatistics()
        {
            //[{BudgetDetail},{}];
            string condition = Request["condition"]+"";//{[n:v,n1:v1]}
            string docDate = Request["docDate"]+"";
            string strDocGuid = Request["docId"] + "";
            string ywKey = Request["ywKey"]+"";
            if (string.IsNullOrEmpty(condition) || string.IsNullOrEmpty(docDate)) { 
                return Json(new {msg="参数信息不正确"},JsonRequestBehavior.AllowGet);
            }
            //todo
            int year =DateTime.Now.Year;
            int.TryParse(docDate.Substring(0, 4),out year);
            Guid docGuid;
            Guid.TryParse(strDocGuid, out docGuid);
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<List<BudgetModel>>(condition);
            var result = this.Actor.BudgetStatistics(CurrentUserInfo.UserGuid, docGuid, conditionModel, year, ywKey);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
