using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Foundation;

namespace BaothApp.Controllers.基础.其他设置
{
    public partial class JCqtszController :SpecificController
    {
        //
        // GET: /JCqtsz/
        //

        #region 往来单位档案--基础

        public ActionResult wldwda()
        {
            this.ModelUrl = "wldwda";
            return View("wldwda");
        }
        /// <summary>
        /// 往来单位档案
        /// </summary>
        /// <returns></returns>
        public JsonResult Savewldwda()
        {
            this.ModelUrl = "wldwda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 往来单位添加默认值
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievewldwda()
        {
            this.ModelUrl = "wldwda";
            JsonModel result = this.Actor.Retrieve(Guid.NewGuid());
            return Json(result);
        }

        #endregion

        #region 外聘人员档案
        ///Controller：JCqtsz
        ///scope：wpryda
        /// <summary>
        /// 外聘人员档案
        /// </summary>
        /// <returns></returns>
        public ActionResult wpryda()
        {
            this.ModelUrl = "wpryda";
            return View("wpryda");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savewpryda()
        {
            this.ModelUrl = "wpryda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievewpryda()
        {
            this.ModelUrl = "wpryda";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion


        #region 往来单位--业务

        
        public ActionResult WLDW()
        {
            this.ModelUrl = "wldw";
            //this.ModelUrl = "";
            return View("wldw");
        }
        /// <summary>
        /// 往来单位档案        /// </summary>
        /// <returns></returns>
        public  JsonResult WlDWSave()
        {
            this.ModelUrl = "wldw";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 往来单位添加默认值        /// </summary>
        /// <returns></returns>
        public JsonResult WlDWNew()
        {
            this.ModelUrl = "wldw";
            JsonModel result = this.Actor.New();
            return Json(result);
        }

        #endregion

        #region 单据编号
        public ActionResult djbh()
        {
            this.ModelUrl = "djbh";
            //this.ModelUrl = "";
            return View("djbh");
        }
        public JsonResult GetRule()
        {
            Business.Foundation.其他设置.单据编号 objDjbh = new Business.Foundation.其他设置.单据编号();
            JsonModel result = objDjbh.New();
            if(null== result)
            {
                return Json("{\"success\": false ,\"msg\":'\"获取编号规则失败!\"}");
            }
            return Json(result);
        }
        public JsonResult SaveNumberRule()
        {
            this.ModelUrl = "djbh";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        #endregion

        #region 帐套设置--基础

        public ActionResult ztsz()
        {
            this.ModelUrl = "ztsz";
            return View("ztsz");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveztsz()
        {
            this.ModelUrl = "ztsz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化返回一个JsonModel空类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveztsz()
        {
            this.ModelUrl = "ztsz";
            JsonModel result = this.Actor.Retrieve(Guid.NewGuid());
            return Json(result);
        }

        #endregion

        #region 帐套子表设置--基础

        public ActionResult ztzbsz()
        {
            this.ModelUrl = "ztzbsz";
            return View("ztzbsz");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveztzbsz()
        {
            this.ModelUrl = "ztzbsz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化返回一个JsonModel空类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveztzbsz()
        {
            this.ModelUrl = "ztzbsz";
            JsonModel result = this.Actor.Retrieve(Guid.NewGuid());
            return Json(result);
        }

        #endregion

        
    }
}
