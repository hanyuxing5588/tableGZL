using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;

namespace BaothApp.Controllers.基础.项目设置
{
    public class JCxmszController : SpecificController
    {

        #region 项目分类

        ///Controller：JCxmsz
        ///scope：xmfl
        /// <summary>
        /// 项目分类
        /// </summary>
        /// <returns></returns>
        public ActionResult xmfl() {
            this.ModelUrl = "xmfl";
            return View("xmfl");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savexmfl() {
            this.ModelUrl = "xmfl";
            JsonModel result = this.Actor.Save(this.Status,this.jsonModel);
            return Json(result);
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievexmfl()
        {
            this.ModelUrl = "xmfl";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        //AllRetrieve
        public JsonResult AllRetrievexmfl()
        {
            this.ModelUrl = "xmfl";
            var type = Request["type"];
            int iType = 2;
            if (int.TryParse(type,out iType)==false)
            {
                iType = 2;
            }
            Business.Foundation.项目设置.项目分类 obj = new Business.Foundation.项目设置.项目分类();
            JsonModel result = obj.AllRetrieve(iType);
            return Json(result);
        }
        #endregion


        #region 项目档案

        ///Controller：JCxmsz
        ///scope：xmda
        /// <summary>
        /// 项目档案
        /// </summary>
        /// <returns></returns>
        public ActionResult xmda()
        {
            this.ModelUrl = "xmda";
            return View("xmda");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savexmda()
        {
            this.ModelUrl = "xmda";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievexmda()
        {
            this.ModelUrl = "xmda";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        //AllRetrieve
        public JsonResult AllRetrievexmda()
        {
            this.ModelUrl = "xmda";
            var type = Request["type"];
            int iType = 2;
            if (int.TryParse(type, out iType) == false)
            {
                iType = 2;
            }
            Business.Foundation.项目设置.项目档案 obj = new Business.Foundation.项目设置.项目档案();
            JsonModel result = obj.AllRetrieve(iType);
            return Json(result);
        }
        #endregion

        public ActionResult kjkmsz()
        {
            this.ModelUrl = "kjkmsz";
            return View("kjkmsz");
        }

    }
}