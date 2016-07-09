using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.IBusiness;
using BaothApp.Utils;
using Business.Common;
using Infrastructure;
namespace BaothApp.Controllers.基础.权限设置
{
    public class AuthSetController : SpecificController
    {
         IAuthSet authSet ;
         public AuthSetController(IAuthSet auth) {
             authSet = auth;
         }
        //单位权限设置
        public ActionResult DWAuthSet()
        {
            return View("dwAuth");
        }
     
        public JsonResult GetDWAuth()
        {
            var strGuid = Request["guid"];
            var isRole =(Request["isRole"] + "").Trim()=="1" ? true : false;
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetDWAuth(g,isRole).ToList();
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }


        //部门权限设置
        public ActionResult BMAuthSet()
        {
            return View("bmAuth");
        }
        public JsonResult GetBMAuth()
        {
            var strGuid = Request["guid"];
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetBMAuth(g,isRole);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //人员权限设置
        public ActionResult RYAuthSet()
        {
            return View("ryAuth");
        }
        public JsonResult GetPersonAuth() 
        {
            var strGuid = Request["guid"];
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetPersonAuth(g,isRole);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //项目权限设置
        public ActionResult XMAuthSet()
        {
            return View("xmAuth");
        }
        public JsonResult GetProjectAuth()
        {
            var strGuid = Request["guid"];
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetProjectAuth(g,isRole);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //科目权限设置
        public ActionResult KMAuthSet()
        {
            return View("kmAuth");
        }
        public JsonResult GetBGCodeAuth()
        {
            var strGuid = Request["guid"];
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetBGCodeAuth(g, isRole);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //工资权限设置
        public ActionResult GZAuthSet()
        {
            return View("gzAuth");
        }
        public JsonResult GetGZAuth()
        {
            var strGuid = Request["guid"];
            Guid g = this.CurrentUserInfo.UserGuid;
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetGZAuth(g, isRole);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //操作权限设置
        public ActionResult ActionAuthSet()
        {
            return View("actionAuth");
        }
        public JsonResult GetActionAuth()
        {
            var strGuid = Request["guid"];
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var dwList = authSet.GetActionAuth(g);
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //菜单权限设置
        public ActionResult MenuAuthSet()
        {
            return View("menuAuth");
        }
        public JsonResult GetMenuAuth()
        {
            var strGuid = Request["guid"];
            Guid g = this.CurrentUserInfo.UserGuid;
            Guid.TryParse(strGuid + "", out g);
            var isRole = (Request["isRole"] + "").Trim() == "1" ? true : false;
            var dwList = authSet.GetMenuAuth(g, isRole);
            
            return Json(new { rows = dwList }, JsonRequestBehavior.AllowGet);
        }
        //保存权限
        public JsonResult SaveAuthData() 
        {
            var roleOrUser = Request["roleOrUser"];
            var classId = Request["classId"];
            var authData = Request["authData"];
            var authDatas = JsonHelper.JsonToObject<List<AuthSetModel>>(authData);
            var bSave = authSet.SaveAuth(roleOrUser,classId, authDatas);
            return Json(new { msg=bSave?"保存成功":"保存失败"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 菜单维护
        /// </summary>
        /// <returns></returns>
        public ActionResult cdwh()
        {
            this.ModelUrl = "cdwh";
            return View("cdwh");
        }

        #region 单据流程配置
        public ActionResult djlcpz()
        {
            this.ModelUrl = "djlcpz";
            return View("djlcpz");
        }
        public ContentResult GetDocTypeList()
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var list = context.SS_DocTypeView.OrderBy(e => e.DocTypeKey).Where(e => (e.DocTypeUrl + "").ToLower() != "null" + "").Select(e => new
            {
                GUID = e.GUID,
                e.DocTypeName,
                e.DocTypeUrl
            }).ToList();
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        public ContentResult GetMenuRelFunList()
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var condition = Request["condition"];
            List<object> list = new List<object>();
            if (string.IsNullOrEmpty(condition))
            {
                list = context.SS_MenuRelFun.OrderBy(e => e.MenuKey).Where(e => (e.Scope + "").ToLower() != "null" + "").Select(e => new
                {
                    GUID = e.GUID,
                    MenuKey = e.MenuKey,
                    MenuName = e.MenuName,
                    Scope = e.Scope
                }).ToList<object>();
            }
            else
            {
                list = context.SS_MenuRelFun.OrderBy(e => e.MenuKey).Where(e => ((e.Scope + "").ToLower() != "null" + "") && (e.Scope.Contains(condition) || e.MenuKey.Contains(condition) || e.MenuName.Contains(condition))).Select(e => new
                {
                    GUID = e.GUID,
                    MenuKey = e.MenuKey,
                    MenuName = e.MenuName,
                    Scope = e.Scope
                }).ToList<object>();
            }
           
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        public ContentResult GetDocByFlow() 
        { 
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var id = Request["id"];
            Guid g;
            Guid.TryParse(id, out g);
            var list =Platform.Flow.Run.WorkFlowAPI.GetDocTypeByFlowId(g);
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }

        public JsonResult SaveDocFlow() 
        {
            var id = Request["id"];
            var version = Request["version"];
            var url = Request["url"];
            var arry = url.Split(',');
            Guid g;
            int i;
            Guid.TryParse(id, out g);
            int.TryParse(version, out i);

            var ret = Platform.Flow.Run.WorkFlowAPI.SaveDocFlow(g, i, arry);
            if (ret)
            {
                var data = new { isOk = true, msg = "保存成功！" };
                return Json(data);
            }
            else
            {
                var data = new { isOk = false,msg="保存失败！" };
                return Json(data);
            }
        }

        public ContentResult GetDocTypeByCondition()
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var condition = Request["condition"];
            var list = context.SS_DocTypeView.OrderBy(e => e.DocTypeKey).Where(e => (e.DocTypeName.Contains(condition)||e.DocTypeUrl.Contains(condition)) && (e.DocTypeUrl + "").ToLower() != "null" + "").Select(e => new
            {
                GUID = e.GUID,
                e.DocTypeName,
                e.DocTypeUrl
            }).ToList();
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        
        #endregion
        
       
        #region 公共 新建、修改、删除

        /// <summary>
        /// 新建、修改、删除

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
