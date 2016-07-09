using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using System.Data;

namespace BaothApp.Controllers.会计.单位人员领款单
{
    public class dwrylkdController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "dwrylkd"; }
        }


        public override ViewResult Index()
        {
            return View("dwrylkd");
        }
        // 单位人员领款单 汇总
        public ContentResult GetLoaddwrylkdHZData()
        {
            string condition = Request["condition"];
            lwfgsydhzCondition conditionModel = new lwfgsydhzCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<lwfgsydhzCondition>(condition);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var report = new Business.Accountant.单位人员领款单(userGuid, this.ModelUrl);

            string msgError = "";
            var data = report.HZ_List(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data != null && data.Count > 0)
            {
                var hjModel = new Business.Accountant.lwfgsydhzModel();
                hjModel.InvitePersonIDCard = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;合";
                hjModel.InvitePersonName = "&nbsp;&nbsp;计&nbsp;&nbsp;";
                hjModel.Total_BX = data.Sum(e => e.Total_BX);
                hjModel.Total_Real = data.Sum(e => e.Total_Real);
                hjModel.Total_Tax = data.Sum(e => e.Total_Tax);
                data.Add(hjModel);
            }
            //转换成Table            
            if (data != null && data.Count > 0)
            {
                DataTable dt = new DataTable();
                dt = BaothApp.Utils.JsonHelper.ListToDataTable("tb1", data);
                dt.Columns.Remove("GUID");
                dt.Columns.Remove("DocNum");
                DataRow row = GetHearData(dt, "hz");
                dt.Rows.InsertAt(row, 0);
                this.Session["dwrylkd"] = dt;
            }
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, data.Count);
            return Content(json);           
        }
        /// <summary>
        /// 明细
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoaddwrylkdMXData()
        {
            string condition = Request["condition"];
            
            lwfgsydhzCondition conditionModel = new lwfgsydhzCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<lwfgsydhzCondition>(condition);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var report = new Business.Accountant.单位人员领款单(userGuid, this.ModelUrl);

            string msgError = "";
            var data = report.MX_List(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data != null && data.Count > 0)
            {
                var hjModel = new Business.Accountant.lwfgsydhzModel();
                hjModel.InvitePersonIDCard = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;合";
                hjModel.InvitePersonName = "&nbsp;&nbsp;计&nbsp;&nbsp;";
                hjModel.Total_BX = data.Sum(e => e.Total_BX);
                hjModel.Total_Real = data.Sum(e => e.Total_Real);
                hjModel.Total_Tax = data.Sum(e => e.Total_Tax);
                data.Add(hjModel);
            }
            //转换成Table            
            if (data != null && data.Count > 0)
            {  
                DataTable dt = new DataTable();
                dt = BaothApp.Utils.JsonHelper.ListToDataTable("tb1", data);
                dt.Columns.Remove("GUID");
                DataRow row = GetHearData(dt,"mx");
                dt.Rows.InsertAt(row,0);
                this.Session["dwrylkd"] = dt;
            }
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, data.Count);
            return Content(json);
        }
        /// <summary>
        ///报表表头
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private DataRow GetHearData(DataTable dt,string type)
        {    
            DataRow row = dt.NewRow();
            row["InvitePersonIDCard"] = "证件编号";
            row["InvitePersonName"] = "姓名";
            row["Total_BX"] = "总金额";
            row["Total_Tax"] = "税金";
            row["Total_Real"] = "实发金额";
            if (type.ToLower() == "mx")
            {
                row["DocNum"] = "制单编号";
            }
            return row;
        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportdwrylkdReport()
        {
            DataTable dt = new DataTable();

            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var report = new Business.Accountant.单位人员领款单(userGuid, this.ModelUrl);

            int year = DateTime.Now.Year;
            string strYear = Request["year"];
            int.TryParse(strYear, out year);
            int month = DateTime.Now.Month;
            string strMonth = Request["month"];
            int.TryParse(strMonth, out month);


            string msgError = "";
            string fileName = string.Empty;
            if (this.Session["dwrylkd"] == null)
            {
                msgError = "1";
            }
            else
            {
                dt = (DataTable)this.Session["dwrylkd"];
            }
            string dateTime = year + "年" + month + "月";
            string pathFlie = report.GetExportPath(dt,dateTime, out fileName, out msgError);
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
