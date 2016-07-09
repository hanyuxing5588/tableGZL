using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;

namespace BaothApp.Controllers.基础.其他设置
{
    public partial class JCqtszController : SpecificController
    {
        #region 页面
        public ActionResult zclx()
        {
            this.ModelUrl = "zclx";
            return View("zclx");
        }
        public ActionResult gnfl()
        {
            this.ModelUrl = "gnfl";
            return View("gnfl");
        }
        public ActionResult jtgj()
        {
            this.ModelUrl = "jtgj";
            return View("jtgj");
        }
        public ActionResult ccbzbz()
        {
            this.ModelUrl = "ccbzbz";
            return View("ccbzbz");
        }
        public ActionResult yhzh()
        {
            this.ModelUrl = "yhzh";
            return View("yhzh");
        }
        public ActionResult ysyflx()
        {
            this.ModelUrl = "ysyflx";
            return View("ysyflx");
        }

        public ActionResult jsfssz()
        {
            this.ModelUrl = "jsfssz";
            return View("jsfssz");
        }

        public ActionResult sklxsz()
        {
            this.ModelUrl = "sklxsz";
            return View("sklxsz");
        }
        public ActionResult srlxsz()
        {
            this.ModelUrl = "srlxsz";
            return View("srlxsz");
        }
        public ActionResult yhda()
        {
            this.ModelUrl = "yhda";
            return View("yhda");
        }
        public ActionResult rylbsz()
        {
            this.ModelUrl = "rylbsz";
            return View("rylbsz");
        }
        public ActionResult zjlxsz()
        {
            this.ModelUrl = "zjlxsz";
            return View("zjlxsz");
        }
        #endregion
        #region 公共新建、修改、删除、停用、启用
        /// <summary>
        /// 新建、修改、删除、停用、启用
        /// </summary>
        /// <returns></returns>
        public override JsonResult Save()
        {
            this.ModelUrl = Request["scope"];
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        #endregion
    }
}
