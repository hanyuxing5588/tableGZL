using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;

namespace BaothApp.Controllers.会计.工资类款项设置
{   
    public class gzlkxszController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "gzlkxsz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            return View("gzlkxsz");
        }

        public ViewResult gzlkxsz_particulars()
        {
            ViewData["ModelUrl"] = "yfd";
            ViewData["btnControl"] = 0;
            return View("gzlkxsz_particulars");
        }
        
        public override JsonResult New()
        {

            JsonModel result = this.Actor.New();
            return Json(result);
        }

        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }

        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Json(result);
        }

    }

}
