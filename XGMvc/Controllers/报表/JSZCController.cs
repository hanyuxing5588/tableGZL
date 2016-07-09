using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers.报表
{
    public class JSZCController : SpecificController
    {
        //
        // GET: /JSZC/

        public override ViewResult Index()
        {
            ViewData["CKey"] = CurrentUserInfo.UserKey;
            return View();
        }
        public ActionResult YSCX() {
            return View("YSCX");
        }
    }
}
