using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers.Print
{
    public class PrintZPController : Controller
    {
        //
        // GET: /PrintZP/
        
        public ActionResult CZPZZ() { 
            //打印模版
            return View("czzzp");
        }
        public ActionResult ZPLQZZ()
        {
            //zzzp
            return View("czzzp");
        }
        public ActionResult XJZP()
        {
            return View("xjzp");
        }
        public ActionResult dhpz()
        {
            return View("dhpz");
        }
        public ActionResult xhpz()
        {
            return View("xhpz");
        }
        public ActionResult jzd()
        {
            return View("jzd");
        }
        
    }
}
