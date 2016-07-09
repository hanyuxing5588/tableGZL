using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using Business.Foundation.薪酬设置;
using Infrastructure;

namespace BaothApp.Controllers.基础.薪酬设置
{
    public class JCxcszController : SpecificController
    {
        #region 薪酬设置--工资项目设置
        ///Controller：JCxcsz
        ///scope：gzxmsz
        /// <summary>
        /// 工资项目设置
        /// </summary>
        /// <returns></returns>
        public ActionResult gzxmsz()
        {
            this.ModelUrl = "gzxmsz";
            return View("gzxmsz");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savegzxmsz()
        {
            this.ModelUrl = "gzxmsz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievegzxmsz()
        {
            this.ModelUrl = "gzxmsz";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 薪酬设置--工资计划设置
        ///Controller：JCxcsz
        ///scope：gzjhsz
        /// <summary>
        /// 工资计划设置
        /// </summary>
        /// <returns></returns>
        public ActionResult gzjhsz()
        {
            this.ModelUrl = "gzjhsz";
            return View("gzjhsz");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savegzjhsz()
        {
            this.ModelUrl = "gzjhsz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievegzjhsz()
        {
            this.ModelUrl = "gzjhsz";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        #endregion

        #region 薪酬设置--工资计划人员设置

        ///Controller：JCxcsz
        ///scope：gzjhrysz
        /// <summary>
        /// 工资计划设置
        /// </summary>
        /// <returns></returns>
        public ActionResult gzjhrysz()
        {
            this.ModelUrl = "gzjhrysz";
            return View("gzjhrysz");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savegzjhrysz()
        {
            this.ModelUrl = "gzjhrysz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievegzjhrysz()
        {
            this.ModelUrl = "gzjhrysz";
            var guid = Request["id"];
            Guid g;
            Guid.TryParse(guid, out g);
            JsonModel result = this.Actor.Retrieve(g);
            return Json(result);
        }

        #endregion

        #region 人员工资默认值设置

        public ActionResult rygzmrzsz()
        {
            this.ModelUrl = "rygzmrzsz";
            return View("rygzmrzsz");
        }

        public JsonResult RequestData() 
        {
            var Rguid = Request["guid"];
            var type = Request["type"];
            Guid guid = new Guid();
            JsonModel jm = new JsonModel();
            if (Rguid != null)
            {
                if (Rguid.Length != 0 || Rguid != "")
                {
                    guid = new Guid(Rguid);
                    if (type=="Plan")
                    {
                        this.ModelUrl = "gzxsjjzfssz";    
                    }

                    if (type == "Item" || type == "Person")
                    {
                        this.ModelUrl = "rygzmrzsz";
                    }
                    
                    jm = this.Actor.Retrieve(guid, type); 
                }
            }

            return Json(jm);
            
        }

        public JsonResult SaveDoc()
        {
            var contions = Request["contions"];
            var msg = "请先选中要修改的数据，然后进行保存！";
            if (contions!=null)
            {
                var obj = BaothApp.Utils.JsonHelper.JsonDeserialize<List<Item>>(contions);
                var b = new 人员工资默认值设置().Save(obj);
                if (b) msg = "保存成功！";
                return Json(msg);
            }
            return Json(msg);
        }
        #endregion

        #region 工资项数据加载方式设置
        public ActionResult gzxsjjzfssz()
        {
            this.ModelUrl = "gzxsjjzfssz";
            return View("gzxsjjzfssz");
        }

        public JsonResult SaveDoc1()
        {
            var contions = Request["contions"];
            var msg = "请先选中要修改的数据，然后进行保存！！";
            if (contions != null)
            {
                var obj = BaothApp.Utils.JsonHelper.JsonToObject<List<SA_PlanItemSetupEx>>(contions);
                var b = new 工资项数据加载方式设置().Save(obj);
                
                if (b) msg = "保存成功！";
                return Json(msg);
            }
            return Json(msg);
        }
        #endregion

    }
}
