using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business;

namespace BaothApp.Controllers
{
    public class ceshiController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View("/print/lwflkdgcy1");
        }

        public ActionResult SendMail()
        {
            string uid = Request["id"];

            if (string.IsNullOrEmpty(uid)) return Content("no user id error");

            SADrive sd = new SADrive();
            var result = sd.RetrievePersonSA(Guid.Parse(uid), 2015, 4, "0");
            

            EmailDrive edrive = new EmailDrive("smtp.126.com", "lbmax0211@126.com", "lbmax)@!!");
            edrive.SendMail("国地信工资条测试", "lbmax0211@126.com", "国地信",
                "lbmax@sohu.com", "陈军", result.ConvertToHtml());
            return Content("OK");
        }
    }
}
