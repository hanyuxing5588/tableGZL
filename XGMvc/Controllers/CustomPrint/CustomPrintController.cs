using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessModel;

namespace BaothApp.Controllers.CustomPrint
{
    public class CustomPrintController : Controller
    {
        //
        // GET: /CustomPrint/

        public ActionResult Print()
        {
            return View("Index");
        }
        public ActionResult dhpz()
        {
            ViewData["Year"] = DateTime.Now.Year;
            ViewData["Month"] = DateTime.Now.Month; ;
            ViewData["Day"] = DateTime.Now.Day;
            ViewData["DW"] = "国家基础地理信息中心";
            if (Request["r"] + "" == "1")
            {
                ViewData["pay"] = "7111010182600296021";
            }
            else if (Request["r"] + "" == "2")
            {
                ViewData["pay"] = "7111010182600296642";
                ViewData["DW"] = "国家基础地理信息中心工会委员会";
            }
            else
            {
                ViewData["pay"] = "7111010173300000990";
            }
            return View("dhpzprint");
        }
        public ActionResult xhpz()
        {
            ViewData["Year"] = DateTime.Now.Year;
            ViewData["Month"] = DateTime.Now.Month; ;
            ViewData["Day"] = DateTime.Now.Day; ;
            ViewData["DW"] = "国家基础地理信息中心";
            if (Request["r"] + "" == "1")
            {
                ViewData["pay"] = "7111010182600296021";
            }
            else if (Request["r"] + "" == "2")
            {
                ViewData["pay"] = "7111010182600296642";
                ViewData["DW"] = "国家基础地理信息中心工会委员会";
            }
            else {
                ViewData["pay"] = "7111010173300000990";
            }
            return View("xhpzprint");
        }

        public ActionResult xjzp() {
            return View("zplqzzprint1");
        }
        public ActionResult JZD()
        {
            ViewData["Year"] = DateTime.Now.Year;
            ViewData["Month"] = DateTime.Now.Month; ;
            ViewData["Day"] = DateTime.Now.Day;
            return View("jzdprint");
        }
        public ActionResult xjzpfm()
        {
            return View("zplqzzfmprint");
        }
        public ActionResult zzzp()
        {
            return View("zplqzzprint");
        }
        public JsonResult GetTreePrintTemp() 
        {
            var urlArr = new List<string>() { "/CustomPrint/dhpz?r=0", "/CustomPrint/dhpz?r=1", "/CustomPrint/xhpz?r=0", "/CustomPrint/xhpz?r=1", 
                 "/CustomPrint/dhpz?r=2", "/CustomPrint/xhpz?r=2", 
                "/CustomPrint/xjzp", "/CustomPrint/xjzpfm" ,"/CustomPrint/zzzp", "/CustomPrint/xjzpfm" , "/CustomPrint/JZD" };
            var titleArr = new List<string>() { "中国电信(电汇凭证-国库)",  "中国电信(电汇凭证-非国库)","中国电信(信汇凭证-国库)", "中国电信(信汇凭证-非国库)",
                
                "中国电信(电汇凭证-工会)", "中国电信(信汇凭证-工会)",
             
                "现金支票-正面","现金支票-反面", "转账支票-正面","转账支票-反面" ,"进账单"};
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            for (int i = 0; i < urlArr.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = Guid.NewGuid().ToString();
                treeModel.@checked = false;
                treeModel.text = titleArr[i];
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["URL"] = urlArr[i];
                treeModel.attributes = dic;
                list.Add(treeModel);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

    }
}
