using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using System.Text;
using Infrastructure;
using Business.Foundation.科目设置;
using BaothApp.Utils;

namespace BaothApp.Controllers.基础.科目设置
{
    public class JCkmszController : SpecificController
    {

        #region  预算科目总表
        ///Controller：JCkmsz
        ///scope：yskmzb
        /// <summary>
        /// 预算科目总表
        /// </summary>
        /// <returns></returns>
        public ActionResult yskmzb()
        {
            this.ModelUrl = "yskmzb";
            return View("yskmzb");
        }
        /// <summary>
        /// 单位保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Saveyskmzb()
        {
            this.ModelUrl = "yskmzb";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveyskmzb()
        {
            this.ModelUrl = "yskmzb";
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        #endregion

        #region 预算科目设置
        ///Controller：JCkmsz
        ///scope：yskmsz
        /// <summary>
        /// 预算科目设置
        /// </summary>
        /// <returns></returns>
        public ActionResult yskmsz()
        {
            this.ModelUrl = "yskmsz";
            return View("yskmsz");
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrieveyskmsz()
        {
            var obj = new Business.Foundation.科目设置.预算科目设置(this.Guid, this.ModelUrl);
            //得到预算科目的数据
            var list = obj.Retrieve();
            //科目设置
            list.Add(new BGCodeSetUpModel
             {
                 GUID = Guid.Empty,
                 PGUID = Guid.Empty,
                 BGCodeName = "合计",
                 GUID_BG_SetupBGCode = null, //目前没有用到，删除的时候可能用到，BG_SetupBGCode表中的GUID
                 RateNum = null,
                 _parentId = null
             });
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 按条件查询
        /// </summary>
        /// <returns></returns>
        public JsonResult Retrievekm()
        {
            var strGuid = Request["guid"];
            Guid g;
            Guid.TryParse(strGuid, out g);
            Business.Foundation.科目设置.预算科目设置 obj = new Business.Foundation.科目设置.预算科目设置(this.Guid, this.ModelUrl);
            var list = obj.Retrievekm(g);
            list.Add(new BGCodeSetUpModel
                              {
                                  GUID = Guid.Empty,
                                  PGUID = Guid.Empty,
                                  BGCodeName = "合计",
                                  GUID_BG_SetupBGCode = Guid.Empty,
                                  RateNum = null,
                                  _parentId = null
                              });
            return Json(new { rows = list }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 保存方法
        /// </summary>
        /// <returns></returns>
        public JsonResult SaveJCkm()
        {
            Business.Foundation.科目设置.预算科目设置 obj = new Business.Foundation.科目设置.预算科目设置(this.Guid, this.ModelUrl);
            var ssSetupId = Request["ssSetupId"];  //id
            var bgCodeData = Request["authData"];
            var bgCodeDatas = JsonHelper.JsonToObject<List<BGCodeSetUpModel>>(bgCodeData);
            var bSave = obj.SaveJC(ssSetupId, bgCodeDatas);
            return Json(new { msg = bSave ? "保存成功" : "保存失败" }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 科目摘要设置
        ///Controller：JCkmsz
        ///scope：kmzysz
        /// <summary>
        /// 科目摘要设置
        /// </summary>
        /// <returns></returns>
        public ActionResult kmzysz()
        {
            this.ModelUrl = "kmzysz";
            return View("kmzysz");
        }
        /// <summary>
        /// 科目摘要保存
        /// </summary>
        /// <returns></returns>
        public JsonResult Savekmzysz()
        {
            this.ModelUrl = "kmzysz";
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 查询Grid表格中的数据,根据左侧树的条件进行筛选(SS_BGCodeMemo)
        /// </summary>
        /// <returns>guid：前台传过来的预算科目的guid（SS_BGCode）</returns>
        /// 
        public JsonResult Retrievekmzysz()
        {
            this.ModelUrl = "kmzysz";
            var guid = Request["id"];
            Guid g;
            Guid.TryParse(guid, out g);

            JsonModel result = this.Actor.Retrieve(g);
            return Json(result);
        }
        //public JsonResult Retrievekmzysz() {
        //    this.ModelUrl = "kmzysz";
        //    var guid = Request["id"];
        //    Guid gid;
        //    if (!Guid.TryParse(guid + "", out gid)) {
        //        return null;
        //    }
        //    var context=this.Actor.InfrastructureContext;
        //    var o = context.SS_BGCodeMemo.Join(context.SS_BGCode, e => e.GUID_BGCode, t => t.GUID, (e, t) => new {
        //        e.GUID,
        //        e.GUID_BGCode,
        //        IsDefault=e.IsDefault,
        //        e.BGCodeMemo,
        //        t.BGCodeName,
        //        t.BGCodeKey
        //    }).Where(e => e.GUID_BGCode == gid).ToList();
        //    return Json(o,JsonRequestBehavior.AllowGet);
        //}

        #endregion

    }


}
