using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Foundation.角色用户;

namespace BaothApp.Controllers.基础.角色用户
{
    public class JCjsyhController : SpecificController
    {
        #region 角色设置
        ///Controller：JCjsyh
        ///scope：jssz
        /// <summary>
        /// 角色设置
        /// </summary>
        /// <returns></returns>
        public ActionResult jssz()
        {
            this.ModelUrl = "jssz";
            return View("jssz");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savejssz()
        {
            this.ModelUrl = "jssz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// 前台没有要返回的数据，只返回一个为空的格式数据类型
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievejssz()
        {
            this.ModelUrl = "jssz";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 用户分组

        ///Controller：JCjsyh
        ///scope：yhfz
        /// <summary>
        /// 用户分组
        /// </summary>
        /// <returns></returns>
        public ActionResult yhfz() {
            this.ModelUrl = "yhfz";
            return View("yhfz");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveyhfz() {
            this.ModelUrl = "yhfz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveyhfz()
        {
            this.ModelUrl = "yhfz";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 用户设置

        ///Controller：JCjsyh
        ///scope：yhsz
        /// <summary>
        /// 用户设置
        /// </summary>
        /// <returns></returns>
        public ActionResult yhsz()
        {
            this.ModelUrl = "yhsz";
            return View("yhsz");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveyhsz()
        {
            this.ModelUrl = "yhsz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public JsonResult UpdatePwd()
        {
            var OperGuid = Request["guid"];
            if (string.IsNullOrEmpty(OperGuid)) {
                OperGuid = this.CurrentUserInfo.UserGuid+"";
            }
            var oldPwd = Request["oldPwd"];
            var newPwd = Request["newPwd"];
            用户设置 yhsz = new 用户设置();
            Guid g;
            Guid.TryParse(OperGuid, out g);
            string strMsg = string.Empty;
            var b = yhsz.UpdatePwd(g, oldPwd, newPwd, out strMsg);
            //var data = newPwd;

            if (b)
            {
                var data = new { msg ="密码修改成功！<br> 您的新密码是：" + newPwd,type="success" };
                return Json(data);
            }
            else
            {
                var data = new { msg =strMsg,type="error" };
                return Json(data);  
            }

            //return Json(new { msg = b ? "密码修改成功！" + "</br>" + "您的新密码是：" + data : strMsg });
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveyhsz()
        {
            this.ModelUrl = "yhsz";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        /// <summary>
        /// 修改密码窗体
        /// </summary>
        /// <returns></returns>
        public ViewResult pwd()
        {
            return View("pwd");
        }
        
        #endregion


    }
}
