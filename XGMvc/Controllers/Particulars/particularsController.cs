using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers.Particulars
{
    public class particularsController : Controller
    {

        /// <summary>
        /// 财政收入--明细
        /// </summary>
        /// <returns></returns>
        public ViewResult zyjjlzd()
        {
            ViewData["ModelUrl"] = "zyjjlzd";
            ViewData["btnControl"] = 1;
            return View("zyjjlzd_particulars");
        }


        /// <summary>
        /// 财政收入--明细
        /// </summary>
        /// <returns></returns>
        public ViewResult czsr()
        {
            ViewData["ModelUrl"] = "czsr";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "收入明细卡片";
            return View("srpd_particulars");
        }

        /// <summary>
        /// 收入凭单--明细
        /// </summary>
        /// <returns></returns>
        public ViewResult srpd()
        {
            ViewData["ModelUrl"] = "srpd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "收入明细卡片";
            return View("srpd_particulars");
        }

        /// <summary>
        /// 出纳付款单--明细
        /// </summary>
        /// <returns></returns>
        public ViewResult cnfkd()
        {
            ViewData["ModelUrl"] = "cnfkd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "出纳明细卡片";
            return View("cnfkd_particulars");
        }
        /// <summary>
        /// 出纳收款单--明细
        /// </summary>
        /// <returns></returns>
        public ViewResult cnskd()
        {
            ViewData["ModelUrl"] = "cnskd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "出纳明细卡片";
            return View("cnfkd_particulars");
        }

        /// <summary>
        /// 应付单        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            ViewData["ModelUrl"] = "yfd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("yfd_particulars");
        }

        /// <summary>
        /// 借款单填制        /// </summary>
        /// <returns></returns>
        public ViewResult jkdtz()
        {
            ViewData["ModelUrl"] = "jkdtz";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "借款明细卡片";
            return View("yfd_particulars");
        }

        /// <summary>
        /// 支票申领单        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            ViewData["ModelUrl"] = "zpsld";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }

        /// <summary>
        /// 差旅报销单        /// </summary>
        /// <returns></returns>
        public ViewResult clbxd()
        {
            ViewData["ModelUrl"] = "clbxd";
            ViewData["btnControl"] = 1;
            ViewData["title"] = "差旅报销明细卡片";
            return View("clbxdparticulars");
        }
        /// <summary>
        /// 公务卡报销单        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            ViewData["ModelUrl"] = "gwkbxd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("gwkbxdparticulars");
        }
        /// <summary>-+
        /// 汇款审批单        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            ViewData["ModelUrl"] = "hkspd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        /// <summary>
        /// 其他报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            ViewData["ModelUrl"] = "qtbxd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        /// <summary>
        /// 期初报销单        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            ViewData["ModelUrl"] = "qcbxd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        /// <summary>
        /// 临时工工资单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            ViewData["ModelUrl"] = "lsggzd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        //
        // GET: /Print/
        /// <summary>
        /// 现金报销单        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            ViewData["ModelUrl"] = "xjbxd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            ViewData["ModelUrl"] = "lwflkd";
            ViewData["btnControl"] = 1;
            ViewData["title"] = "报销明细卡片";
            return View("lwfbxdparticulars");
        }
        /// <summary>
        /// 手续费报销单        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            ViewData["ModelUrl"] = "sxfbxd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
        /// <summary>
        /// 应收单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            ViewData["ModelUrl"] = "ysdtz";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "往来明细卡片";
            return View("yfd_particulars");
        }
        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            ViewData["ModelUrl"] = "yfdtz";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "往来明细卡片";
            return View("yfd_particulars");
        }
        /// <summary>
        /// 现金提取
        /// </summary>
        /// <returns></returns>
        public ViewResult xjtq()
        {
            ViewData["ModelUrl"] = "xjtq";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "出纳明细卡片";
            return View("xjtq_particulars");
        }

        /// <summary>
        /// 现金存储
        /// </summary>
        /// <returns></returns>
        public ViewResult xjcc()
        {
            ViewData["ModelUrl"] = "xjcc";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "出纳明细卡片";
            return View("xjtq_particulars");
        }

        /// <summary>
        /// 内部调账单
        /// </summary>
        /// <returns></returns>
        public ViewResult nbdzd()
        {
            ViewData["ModelUrl"] = "nbdzd";
            ViewData["btnControl"] = 0;
            ViewData["title"] = "报销明细卡片";
            return View("particulars");
        }
    }
}
