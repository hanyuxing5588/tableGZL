using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;

namespace BaothApp.Controllers.基础.桌面设置
{
    public class JCzmszController : SpecificController
    {
        #region 桌面设置--通知公告
        ///Controller：JCzmsz
        ///scope：tzgg
        /// <summary>
        /// 通知公告
        /// </summary>
        /// <returns></returns>
        public ActionResult tzgg()
        {
            this.ModelUrl = "tzgg";
            return View("tzgg1");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savetzgg()
        {
            this.ModelUrl = "tzgg";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievetzgg()
        {
            this.ModelUrl = "tzgg";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 桌面设置--政策法规
        ///Controller：JCzmsz
        ///scope：zcfg
        /// <summary>
        /// 政策法规
        /// </summary>
        /// <returns></returns>
        public ActionResult zcfg()
        {
            this.ModelUrl = "zcfg";
            return View("zcfg");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savezcfg()
        {
            this.ModelUrl = "zcfg";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievezcfg()
        {
            this.ModelUrl = "zcfg";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 桌面设置--文件类型
        ///Controller：JCzmsz
        ///scope：wjlx
        /// <summary>
        /// 文件类型
        /// </summary>
        /// <returns></returns>
        public ActionResult wjlx()
        {
            this.ModelUrl = "wjlx";
            return View("wjlx");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savewjlx()
        {
            this.ModelUrl = "wjlx";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievewjlx()
        {
            this.ModelUrl = "wjlx";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion




    }
}
