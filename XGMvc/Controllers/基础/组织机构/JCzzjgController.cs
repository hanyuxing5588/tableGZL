using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;

namespace BaothApp.Controllers.基础.组织机构
{
    public class JCzzjgController : SpecificController
    { 

        #region  单位档案
        ///Controller：JCzzjg
        ///scope：dwda
        /// <summary>
        /// 单位档案
        /// </summary>
        /// <returns></returns>
        public ActionResult dwda()
        {
            this.ModelUrl = "dwda";
            return View("dwda");
        }
        /// <summary>
        /// 单位保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savedwda()
        {
            this.ModelUrl = "dwda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievedwda() 
        {
            this.ModelUrl = "dwda";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        /// <summary>
        /// 默认初始值   --目前没有用到--
        /// </summary>
        /// <returns></returns>
        public JsonResult NewDwda()
        {
            this.ModelUrl = "dwda";
            JsonModel result = this.Actor.New();
            return Json(result);
        }
        /// <summary>
        /// 列表记录    --目前没有用到--
        /// </summary>
        /// <returns></returns>
        /// 默认浏览状态时，返回表格数据，data
        public ContentResult Historydwda()
        {
            this.ModelUrl = "dwda";
            List<object> result = this.Actor.History(null);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        #endregion

        #region 部门档案
        ///Controller：JCzzjg
        ///scope：bmda
        /// <summary>
        /// 部门档案
        /// </summary>
        /// <returns></returns>
        public ActionResult bmda()
        {
            this.ModelUrl = "bmda";
            return View("bmda");
        }
        /// <summary>
        /// 部门保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savebmda()
        {
            this.ModelUrl = "bmda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievebmda()
        {
            this.ModelUrl = "bmda";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        

        #endregion

        #region 人员档案
        ///Controller：JCzzjg
        ///scope：ryda
        /// <summary>
        /// 人员档案
        /// </summary>
        /// <returns></returns>
        public ActionResult ryda()
        {
            this.ModelUrl = "ryda";
            return View("ryda");
        }
        /// <summary>
        /// 人员保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveryda() {
            this.ModelUrl = "ryda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveryda() {
            this.ModelUrl = "ryda";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

    }
}
