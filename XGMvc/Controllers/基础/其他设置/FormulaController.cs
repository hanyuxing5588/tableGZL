using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.IBusiness;
using BaothApp.Utils;
namespace BaothApp.Controllers.基础.其他设置
{
    public class FormulaController : SpecificController
    {
        //
        private IFormula iformula = new Business.Foundation.其他设置.公式设置();

        #region 基础--其他设置--工资公式设置
        // GET: /Formula/
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public JsonResult GZSave()
        {
           var guid=Request["Id"];
           var items=Request["items"];
           Guid g;

           if(!Guid.TryParse(guid,out g)){
                return Json(new {msg="参数不正确！"});
           }
           var itemSaList = JsonHelper.JsonToObject<List<FormulaModel>>(items);
           var b = iformula.GZFormulaSave(g, itemSaList);
           return Json(new {msg=b?"保存成功！":"保存失败！"});
        }
        //删除
        public JsonResult IsCanDelete()
        {
            var guid = Request["Id"];
            var itemId = Request["itemId"];
            Guid g,g1;

            if (!Guid.TryParse(guid, out g) || !Guid.TryParse(itemId, out g1))
            {
                return Json(new { IsCan = "参数不正确！" });
            }
            var b = iformula.IsCanDelete(g,g1);
            return Json(new { IsCan = b?"1":"0"});
        }

        #endregion

        #region 基础--其他设置--预算公式设置
        /// <summary>
        /// 保存方法
        /// </summary>
        /// <returns></returns>
        public JsonResult YSSave() 
        {
            var guid = Request["id"];
            var items = Request["items"];
            Guid g;
           if(!Guid.TryParse(guid,out g)){
                return Json(new {msg="参数不正确！"});
           }
            //调用公共的接口(两个值：GUID_Item，ItemFormula)
           var itemSaList = JsonHelper.JsonToObject<List<FormulaModel>>(items);
            //要保存的方法，自己写自己的
           var b = iformula.YSFormulaSave(g, itemSaList);       
           return Json(new { msg = b ? "保存成功！" : "保存失败！" });
        }
        /// <summary>
        /// 是否删除
        /// </summary>
        /// <returns></returns>
        public JsonResult YSIsCanDelete()
        {
            var guid = Request["Id"];
            var itemId = Request["itemId"];
            Guid g, g1;

            if (!Guid.TryParse(guid, out g) || !Guid.TryParse(itemId, out g1))
            {
                return Json(new { IsCan = "参数不正确！" });
            }
            var b = iformula.YSIsCanDelete(g, g1);
            return Json(new { IsCan = b ? "1" : "0" });
        }
            
        #endregion

        #region 基础--其他设置--预算默认值设置
        /// <summary>
        /// 保存方法
        /// </summary>
        /// <returns></returns>
        public JsonResult YSMRZSave()
        {
            var guid = Request["id"];
            var items = Request["items"];
            Guid g;
            if (!Guid.TryParse(guid, out g))
            {
                return Json(new { msg = "参数不正确！" });
            }
            //调用公共的接口(两个值：GUID_Item，ItemFormula)
            var itemSaList = JsonHelper.JsonToObject<List<FormulaModel>>(items);
            //要保存的方法，自己写自己的
            var b = iformula.YSMRZFormulaSave(g, itemSaList);
            return Json(new { msg = b ? "保存成功！" : "保存失败！" });
        }
        /// <summary>
        /// 是否删除
        /// </summary>
        /// <returns></returns>
        public JsonResult YSMRZIsCanDelete()
        {
            var guid = Request["Id"];
            var itemId = Request["itemId"];
            Guid g, g1;

            if (!Guid.TryParse(guid, out g) || !Guid.TryParse(itemId, out g1))
            {
                return Json(new { IsCan = "参数不正确！" });
            }
            var b = iformula.YSMRZIsCanDelete(g, g1);
            return Json(new { IsCan = b ? "1" : "0" });
        }

        #endregion

    }
}
