using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using System.Data;

namespace BaothApp.Controllers.会计.类款项汇总
{   
    public class lkxhzController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "lkxhz"; }
        }
        public override ViewResult Index()
        {
            return View("lkxhz");
        }
        /// <summary>
        /// 历史页面
        /// </summary>
        /// <returns></returns>
        public ViewResult lkxhzHistory()
        { 
            return View("History");
        }

        public override JsonResult Retrieve()
        {
            string message = string.Empty;
            Guid g;
            string strGuid = Request["GUID"];
            Guid.TryParse(strGuid, out g);

            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.类款项汇总(userGuid, this.ModelUrl);
            var data = obj.Retrieve(g, out message);
            if (!string.IsNullOrEmpty(message))
            {
                var o = "[{\"msg\":\"" + message + "\"}]";
                return Json(o);
            }
            //string rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
            return Json(data);
        }
      
        /// <summary>
        /// 返回数据
        /// </summary>
        /// <returns></returns>
        //public new ContentResult Retrieve()
        //{
        //    string message=string.Empty;
        //    Guid g;
        //    string strGuid=Request["GUID"];
        //    Guid.TryParse(strGuid,out g);

        //    var user = this.CurrentUserInfo;
        //    Guid userGuid = user.UserGuid;
        //    var obj=new Business.Accountant.类款项汇总(userGuid, this.ModelUrl);
        //    var data = obj.Retrieve(g, out message);
        //    if (!string.IsNullOrEmpty(message))
        //    {
        //        var o = "[{\"msg\":\"" + message + "\"}]";
        //        return Content(o);
        //    }
        //    string rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
        //    return Content(rowJson);           
        //}
        // 类款项汇总
        public ContentResult GetLoadlkxhzData()
        {
            string condition = Request["condition"];
            lkxhzCondition conditionModel = new lkxhzCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<lkxhzCondition>(condition);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var report = new Business.Accountant.类款项汇总(userGuid, this.ModelUrl);

            string msgError = "";
            var data = report.HZ_List(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
           // string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, data.Count);
            return Content(rowJson);
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            lkxhzHistoryCondition conditionModel = new lkxhzHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<lkxhzHistoryCondition>(condition);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj=new Business.Accountant.类款项汇总(userGuid, this.ModelUrl);
            List<object> result =obj.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        /// <summary>
        ///报表表头
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private DataRow GetHearData(DataTable dt, string type)
        {
            //DataRow row = dt.NewRow();
            //row["InvitePersonIDCard"] = "证件编号";
            //row["InvitePersonName"] = "姓名";
            //row["Total_BX"] = "总金额";
            //row["Total_Tax"] = "税金";
            //row["Total_Real"] = "实发金额";
            //if (type.ToLower() == "mx")
            //{
            //    row["DocNum"] = "制单编号";
            //}
            //return row;
            return null;
        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportlkxhzReport()
        {
            DataTable dt = new DataTable();

            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var report = new Business.Accountant.类款项汇总(userGuid, this.ModelUrl);

            int year = DateTime.Now.Year;
            string strYear = Request["year"];
            int.TryParse(strYear, out year);
            int month = DateTime.Now.Month;
            string strMonth = Request["month"];
            int.TryParse(strMonth, out month);


            string msgError = "";
            string fileName = string.Empty;
            if (this.Session["lwfgsydhz"] == null)
            {
                msgError = "1";
            }
            else
            {
                dt = (DataTable)this.Session["lwfgsydhz"];
            }
            string dateTime = year + "年" + month + "月";
            string pathFlie = report.GetExportPath(dt, dateTime, out fileName, out msgError);
            if (!string.IsNullOrEmpty(msgError))
            {
                if (msgError == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else
                {
                    return Content("导出错误！");
                }
            }
            return File(pathFlie, "application/msexcel", fileName);
        }
    }
}
