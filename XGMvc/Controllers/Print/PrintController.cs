using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers.Print
{
    public class PrintController : Controller
    {
        public ViewResult index()
        {
            return View("index");
        }
        /// <summary>
        /// 打印单据
        /// </summary>
        /// 
        
        /// <summary>
        /// 现金报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult xjbxd()
        {
            SetData();
            ViewData["ModelUrl"] = "xjbxd";
            return View("xjbxdprint");
        }

        /// <summary>
        /// 差旅报销单
        /// </summary>
        /// <returns></returns>
        public JsonResult clbxdData() {
            var data = Request["data"];
            if (!string.IsNullOrEmpty(data))
            {
                ViewData["data"] = data;
                Session["printData"] = data;
            }
            return Json(new { msg = "ok" },JsonRequestBehavior.AllowGet);
        }
        public ViewResult clbxd()
        {
            //var guid = Request["guid"];
            //if (!string.IsNullOrEmpty(guid))
            //{
            //    ViewData["guid"] = guid;
            //}
            //var moneychinese = Request["moneychinese"];
            //if (!string.IsNullOrEmpty(moneychinese))
            //{
            //    ViewData["moneychinese"] = moneychinese;
            //}
            //var moneyunmber = Request["moneyunmber"];
            //if (!string.IsNullOrEmpty(moneyunmber))
            //{
            //    ViewData["moneyunmber"] = moneyunmber;
            //}
            //var data = Request["data"];
            //if (!string.IsNullOrEmpty(data))
            //{
            //    ViewData["data"] = data;
            //    Session["printData"] = data ;
            //}
            if (Session["printData"] != null)
            {
                ViewData["data"] = Session["printData"];
            }
            ViewData["ModelUrl"] = "clbxd";
            return View("clbxdprint");
        }

        #region 劳务费领款单

        /// <summary>
        /// 劳务费领款单
        /// </summary>
        /// <returns></returns>
        public ViewResult lwflkd()
        {
            SetData(); 
            ViewData["ModelUrl"] = "lwflkd";
            return View("lwflkdprint");
        }
        /// <summary>
        /// 领款人未签字
        /// </summary>
        /// <returns>提交后</returns>
        public ViewResult lwflkdgcy1()
        {
            SetData();
            ViewData["ModelUrl"] = "lwflkd";
            return View("lwflkdgcy21");
        }
        /// <summary>
        /// 领款人已签字
        /// </summary>
        /// <returns>提交后</returns>
        public ViewResult lwflkdgcy2()
        {
            SetData();
            ViewData["ModelUrl"] = "lwflkd";
            return View("lwflkdgcy2");
        }
        /// <summary>
        /// 发放表--有合计
        /// </summary>
        /// <returns>保存后但为提交</returns>
        public ViewResult lwflkdgcy3()
        {
            SetData();
            ViewData["ModelUrl"] = "lwflkd";
            return View("lwflkdgcy3");
        }
        /// <summary>
        /// 发放表--无合计
        /// </summary>
        /// <returns>保存后但为提交</returns>
        public ViewResult lwflkdgcy4()
        {
            SetData();
            ViewData["ModelUrl"] = "lwflkd";
            return View("lwflkdgcy4");
        }

        #endregion

        /// <summary>
        /// 临时工工资单
        /// </summary>
        /// <returns></returns>
        public ViewResult lsggzd()
        {
            SetData();
            ViewData["ModelUrl"] = "lsggzd";
            return View("lsggzdprint");
        }

        /// <summary>
        /// 汇款审批单
        /// </summary>
        /// <returns></returns>
        public ViewResult hkspd()
        {
            SetData();
            ViewData["ModelUrl"] = "hkspd";
            return View("hkspdprint");
        }

        /// <summary>
        /// 手续费报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult sxfbxd()
        {
            SetData();
            ViewData["ModelUrl"] = "sxfbxd";
            return View("sxfbxdprint");
        }

        /// <summary>
        /// 其他报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult qtbxd()
        {
            SetData();
            ViewData["ModelUrl"] = "qtbxd";
            return View("qtbxdprint");
        }

        /// <summary>
        /// 期初报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult qcbxd()
        {
            SetData();
            ViewData["ModelUrl"] = "qcbxd";
            return View("qcbxdprint");
        }

        /// <summary>
        /// 支票申领单
        /// </summary>
        /// <returns></returns>
        public ViewResult zpsld()
        {
            SetData();
            if (ViewData["moneyunmber"] + "" == "￥0.00") {
                ViewData["moneyunmber"] = "￥&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
            }
            ViewData["ModelUrl"] = "zpsld";
            return View("zpsldprint");
        }

        /// <summary>
        /// 公务卡报销单
        /// </summary>
        /// <returns></returns>
        public ViewResult gwkbxd()
        {
            SetData();
            ViewData["ModelUrl"] = "gwkbxd";
            return View("gwkbxdprint");
        }

        /// <summary>
        /// 借款单
        /// </summary>
        /// <returns></returns>
        public ViewResult jkd()
        {
            SetData();
            ViewData["ModelUrl"] = "jkdtz";
            return View("jkdprint");
        }

        /// <summary>
        /// 收款凭单
        /// </summary>
        /// <returns></returns>
        public ViewResult skpd()
        {
            SetData();
            ViewData["ModelUrl"] = "skpd";
            return View("skpdprint");
        }

        /// <summary>
        /// 应付单
        /// </summary>
        /// <returns></returns>
        public ViewResult yfd()
        {
            SetData();
            ViewData["ModelUrl"] = "yfd";
            return View("yfdprint");
        }

        /// <summary>
        /// 现金提取
        /// </summary>
        /// <returns></returns>
        public ViewResult xjtq()
        {
            SetData();
            ViewData["moneychinese"] = Request["moneychinese"];
            ViewData["moneyunmber"] = Request["moneyunmber"];
            ViewData["moneychineseField"] = "xjtq-CN_CashMain-moneychinese";
            ViewData["moneyunmberField"]  = "xjtq-CN_CashMain-moneyunmber";
            //moneychineseField moneyunmberField
            ViewData["ModelUrl"] = "xjtq";
            return View("xjtqprint");
        }

        /// <summary>
        /// 现金存储
        /// </summary>
        /// <returns></returns>
        public ViewResult xjcc()
        {
            SetData();
            ViewData["ModelUrl"] = "xjcc";
            ViewData["moneychinese"] = Request["moneychinese"];
            ViewData["moneyunmber"] = Request["moneyunmber"];
            return View("xjccprint");
        }

        /// <summary>
        /// 应收单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult ysdtz()
        {
            SetData();
            ViewData["ModelUrl"] = "ysdtz";
            return View("ysdtzprint");
        }

        /// <summary>
        /// 应付单填制
        /// </summary>
        /// <returns></returns>
        public ViewResult yfdtz()
        {
            SetData();
            ViewData["ModelUrl"] = "yfdtz";
            return View("yfdtzprint");
        }

        /// <summary>
        /// 内部调账单
        /// </summary>
        /// <returns></returns>
        public ViewResult nbdzd()
        {
            SetData();
            ViewData["ModelUrl"] = "nbdzd";
            return View("nbdzdprint");
        }

        /// <summary>
        /// 支票领取
        /// </summary>
        /// <returns>zzp--2014-03-19</returns>
        public ViewResult zplq()
        {
            SetData();
            ViewData["ModelUrl"] = "zplq";
            return View("zplqprint");
          
        }
   

        public ViewResult zplqFrame()
        {

            //ViewData["guid"] = "4821A9C3-C18B-F247-9132-9A774846E008";
            SetData();
            ViewData["ModelUrl"] = "zplq";
            return View("zpIframePrint");
        }
        public ViewResult xjtqFrame()
        {
            //moneychineseField moneyunmberField
            //ViewData["guid"] = "4821A9C3-C18B-F247-9132-9A774846E008";
            SetData();
          
            ViewData["ModelUrl"] = "xjtq";
            return View("xjzpIframePrint");
        }
        /// <summary>
        /// 现金反面
        /// </summary>
        /// <returns></returns>
        public ViewResult zplqxjfm()
        {
            SetData();
            return View("zplqxjfmprint");
        }
        /// <summary>
        /// 转账反面
        /// </summary>
        /// <returns></returns>
        public ViewResult zplqzzfm()
        {
            SetData();
            return View("zplqzzfmprint");
        }
        /// <summary>
        /// 支票领取 转账
        /// </summary>
        /// <returns></returns>
        public ViewResult zplqzz()
        {
            SetData();
            return View("zplqzzprint");
        }
        public ViewResult zplqzz1()
        {
            SetData();
            return View("zplqzzprint1");
        }
        /// <summary>
        /// 电汇凭证
        /// </summary>
        /// <returns></returns>
        public ViewResult dhpz()
        {
            var doctypekey = Request["doctypekey"];
            if (!string.IsNullOrEmpty(doctypekey))
            {
                ViewData["doctypekey"] = doctypekey;
            }
            var paymentnumber = Request["paymentnumber"];
            if (!string.IsNullOrEmpty(paymentnumber))
            {
                ViewData["paymentnumber"]=paymentnumber;
            }

            SetData();
            return View("dhpzprint");
        }
        /// <summary>
        /// 信汇凭证
        /// </summary>
        public ViewResult xhpz()
        {
            var doctypekey = Request["doctypekey"];
            if (!string.IsNullOrEmpty(doctypekey))
            {
                ViewData["doctypekey"] = doctypekey;
            }
            var paymentnumber = Request["paymentnumber"];
            if (!string.IsNullOrEmpty(paymentnumber))
            {
                ViewData["paymentnumber"] = paymentnumber;
            }
            SetData();
            return View("xhpzprint");
        }
        /// <summary>
        /// 汇款审批单凭证打印
        /// </summary>
        /// <returns></returns>
        public ViewResult hkspdframe()
        {
            var doctypekey = Request["doctypekey"];
            if (!string.IsNullOrEmpty(doctypekey))
            {
                ViewData["doctypekey"] = doctypekey;
            }

            SetData();
            return View("hkspdframePrint");
        }
        /// <summary>
        /// 核销中的收入凭单(进账单)
        /// </summary>
        /// <returns></returns>
        public ViewResult jzd()
        {
            var doctypekey = Request["doctypekey"];
            if (!string.IsNullOrEmpty(doctypekey))
            {
                ViewData["doctypekey"] = doctypekey;
            }
            var paymentnumber = Request["paymentnumber"];
            if (!string.IsNullOrEmpty(paymentnumber))
            {
                ViewData["paymentnumber"] = paymentnumber;
            }
            SetData();
            return View("jzdprint");
        }
        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData()
        {
            var guid = Request["guid"];
            if (!string.IsNullOrEmpty(guid))
            {
                ViewData["guid"] = guid;
            }
            var cName = Request["cn"];
            if (!string.IsNullOrEmpty(cName))
            {
                var moneychineseName = cName.Split('-')[2];
                var moneychinese = Request[moneychineseName];
                if (!string.IsNullOrEmpty(moneychinese))
                {
                    ViewData["moneychinese"] = moneychinese;
                }
                ViewData["moneychineseField"] = cName;
            }
            var numberName = Request["number"];
            if (!string.IsNullOrEmpty(numberName))
            {
                var moneyunmberName = numberName.Split('-')[2];
                var moneyunmber = Request[moneyunmberName];
                if (!string.IsNullOrEmpty(moneyunmber))
                {
                    ViewData["moneyunmber"] = "￥"+moneyunmber;
                }
                ViewData["moneyunmberField"] = numberName;
                //moneychineseField moneyunmberField
            }
        }
    }
}
