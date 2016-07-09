using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using System.Text;
using Business.CommonModule;

namespace BaothApp.Controllers.History
{
    public class HistoryController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "history"; }
        }

        /// <summary>
        /// 工资发放
        /// </summary>
        /// <returns></returns>
        public ViewResult gzdHistory()
        {
            ViewData["ModelUrl"] = "gzd";
            return View("gzdHistory");
        }
        /// <summary>
        /// 会计凭证--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult kjpz()
        {
            ViewData["ModelUrl"] = "kjpz";
            return View("kjpzHistory");
        }


        /// <summary>
        /// 专用基金列支单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult zyjjlzd()
        {
            ViewData["ModelUrl"] = "zyjjlzd";
            return View("zyjjlzdHistory");
        }

        /// <summary>
        /// 借款单填制--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult jkdtz()
        {
            ViewData["ModelUrl"] = "jkdtz";
            return View("jkdtzHistory");
        }

        /// <summary>
        /// 公务卡汇总报销单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult gwkhzbxd()
        {
            ViewData["ModelUrl"] = "gwkhzbxd";
            return View("gwkhzbxdHistory");
        }
        /// <summary>
        /// 现金存储--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult xjcc()
        {
            ViewData["ModelUrl"] = "xjcc";
            return View("xjtqHistory");
        }

        /// <summary>
        /// 现金提取--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult xjtq()
        {
            ViewData["ModelUrl"] = "xjtq";
            return View("xjtqHistory");
        }

        /// <summary>
        /// 财政收入--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult czsr()
        {
            ViewData["ModelUrl"] = "czsr";
            return View("srpdHistory");
        }

        /// <summary>
        /// 收入凭单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult srpd()
        {
            ViewData["ModelUrl"] = "srpd";
            return View("srpdHistory");
        }

        /// <summary>
        /// 出纳付款单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult cnfkd()
        {
            ViewData["ModelUrl"] = "cnfkd";
            return View("cnfkdHistory");
        }
        /// <summary>
        /// 出纳收款单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult cnskd()
        {
            ViewData["ModelUrl"] = "cnskd";
            return View("cnfkdHistory");
        }

        /// <summary>
        /// 收款凭单--历史
        /// </summary>
        /// <returns></returns>
        public ViewResult skpd()
        {
            ViewData["ModelUrl"] = "skpd";
            return View("skpdHistory");
        }
        /// <summary>
        /// 支票申领单
        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            ViewData["ModelUrl"] = "zpsld";
            return View("History");
        }
        /// <summary>
        /// 差旅报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult clbxd()
        {
            ViewData["ModelUrl"] = "clbxd";
            return View("History");
        }
        /// <summary>
        /// 公务卡报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            ViewData["ModelUrl"] = "gwkbxd";
            return View("History");
        }
        /// <summary>
        /// 汇款审批单
        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            ViewData["ModelUrl"] = "hkspd";
            return View("History");
        }
        /// <summary>
        /// 其它报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            ViewData["ModelUrl"] = "qtbxd";
            return View("History");
        }
        /// <summary>
        /// 初期报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            ViewData["ModelUrl"] = "qcbxd";
            return View("History");
        }
        /// <summary>
        /// 临时工工资单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            ViewData["ModelUrl"] = "lsggzd";
            return View("History");
        }
        /// <summary>
        /// 现金报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            ViewData["ModelUrl"] = "xjbxd";
            return View("History");
        }
        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            ViewData["ModelUrl"] = "lwflkd";
            return View("History");
        }
        /// <summary>
        /// 手续费报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            ViewData["ModelUrl"] = "sxfbxd";
            return View("History");
        }

        /// <summary>
        /// 应付单
        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            ViewData["ModelUrl"] = "yfd";
            return View("History");
        }

        /// <summary>
        /// 应收单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            ViewData["ModelUrl"] = "ysdtz";
            return View("History");
        }

        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            ViewData["ModelUrl"] = "yfdtz";
            return View("History");
        }

        /// <summary>
        /// 内部掉账单
        /// </summary>
        /// <returns></returns>
        public ViewResult nbdzd()
        {
            ViewData["ModelUrl"] = "nbdzd";
            return View("History");
        }

        /// <summary>
        /// 预算调整
        /// </summary>
        /// <returns></returns>
        public ViewResult ystz()
        {
            ViewData["ModelUrl"] = "ystz";
            return View("ystzHistory");
        }

        /// <summary>
        /// 预算控制
        /// </summary>
        /// <returns></returns>
        public ViewResult yskz()
        {
            ViewData["ModelUrl"] = "yskz";
            return View("yskzHistory");
        }

        /// <summary>
        /// 预算分配
        /// </summary>
        /// <returns></returns>
        public ViewResult ysfp()
        {
            ViewData["ModelUrl"] = "ysfp";
            return View("ysfpHistory");
        }

        public ViewResult ysfpSearch()
        {
            ViewData["ModelUrl"] = "ysfp";
            return View("ysfpShow");
        }
        /// <summary>
        /// 预算编制历史
        /// </summary>
        /// <returns></returns>
        public ViewResult ysbz()
        {
            ViewData["ModelUrl"] = "ysbz";
            return View("ysbzHistory");
        }

        public ViewResult ysbzReference()
        {
            var dwKey = Request["dwKey"];
            var depKey = Request["depKey"];
            var proKey = Request["proKey"];
            ViewData["ModelUrl"] = "ysbz";
            ViewData["dwKey"] = dwKey;
            ViewData["depKey"] = depKey;
            ViewData["proKey"] = proKey;
            return View("ysbzReference");
        }
        /// <summary>
        /// 预算初始值设置
        /// </summary>
        /// <returns></returns>
        public ViewResult yscszsz()
        {
            ViewData["ModelUrl"] = "yscszsz";
            return View("yscszszHistory");
        }


        /// <summary>
        /// 借款
        /// </summary>
        /// <returns></returns>
        public ViewResult HistoryView()
        {
            return View("History");
        }


        /// <summary>
        /// 历史
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            HistoryCondition conditionModel = new HistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<HistoryCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
           string json=BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson,result.Count);
           return Content(json);           
        }
        
        //预算调整
        public  ContentResult YSTZHistory()
        {
            var ystz = new 历史记录(this.CurrentUserInfo.UserGuid,this.ModelUrl);
            //to do 组织参数
            var year = Request["Year"];
            var bgType = Request["BGType"];
            var bgStep = Request["BGStep"];
            var project = Request["Project"];
            var Department = Request["Department"];
            var result = ystz.GetYSTZHistory(year, bgType, bgStep, project, Department);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
    }
}
