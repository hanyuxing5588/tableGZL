using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;
using CAE;

namespace BaothApp.Controllers.报表
{
    public class GCYReportController : SpecificController
    {
        public override string ModelUrl
        {
            get { return ""; }
        }

        /// <summary>
        /// 财政预算执行进度情况表        /// </summary>
        /// <returns></returns>
        public ViewResult CZYSZXJDQKB()
        {
            return View("czyszxqkb");
        }
        //财政预算执行进度情况表
        public ContentResult GetLoadCZYSZXJDQKBData()
        {
            string startDate = Request["startDate"];
            string endDate = Request["endDate"];
            string mType = Request["mType"];
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            int quantility = 1;
            //if (!int.TryParse(mType, out quantility)) quantility = 1;

            var report = new CAE.CZYSZXQKB(key, "", DateTime.Parse(startDate), DateTime.Parse(endDate), quantility);
            ReportResult result = report.GetReport1();
            if (result.Error)
            {
                var o = "[{\"msg\":\"" + result.ErrorMessage + "\"}]";
                return Content(o);
            }

          
            var json = Utils.JsonHelper.DataTableToJson(result.Result);
            return Content(json);
        }
        //导出ExecelReport财政预算执行进度情况表
        public ActionResult ExportCZYSZXJDQKBReport()
        {
            try
            {
                string startDate = Request["startDate"];
                string endDate = Request["endDate"];
                string mType = Request["mType"];
                var user = this.CurrentUserInfo;
                string fileName;
                string key = user.UserKey;
                var report = new CAE.CZYSZXQKB(key, "", DateTime.Parse(startDate), DateTime.Parse(endDate));
                string pathFlie = report.GetExportPath(out fileName,mType);
                return File(pathFlie, "application/msexcel", fileName);
            }
            catch (Exception ex)
            {
                return Content("导出错误");//，详细信息为："+ex.Message.ToString());
            }
        }
        /// <summary>
        /// 项目支出执行一览表
        /// </summary>
        /// <returns></returns>
        public  ViewResult XMZCZXCX()
        {
            return View("xmzxjdylb");
        }
        /// <summary>
        /// 财政拨款收支一览表
        /// </summary>
        /// <returns></returns>
        public ViewResult CZBKSZYLB()
        {
            return View("czbkszylb");
        }
        /// <summary>
        /// 应收明细表
        /// </summary>
        /// <returns></returns>
        public ViewResult YSKMXB()
        {
            return View("yskmxb");
        }
        //http://localhost:3708/GCYReport/GetLoadCZYSZXJDQKBData

        //项目执行进度数据
        public ContentResult GetLoadXMZCJDData() 
        {
            string startDate=Request["startDate"];
            string endDate=Request["endDate"];
            string mType = "1";//Request["mType"];
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.XMZCJDYLB(key);
            string msgError = "";
            var data = report.Load(endDate, startDate,mType,out msgError);
            if (data == null) {
                var o ="[{\"msg\":\""+msgError+"\"}]";
                return Content(o);
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出ExecelReport项目执行进度一览表
        public ActionResult ExportXMZCJDReport()
        {
            string startDate = Request["startDate"];
            string endDate = Request["endDate"];
            string mType = Request["mType"];
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.XMZCJDYLB(key);
            string fileName,errorMsg;
            string pathFlie = report.GetExportPath(endDate, startDate, mType, out fileName,out errorMsg);
            if (errorMsg != "")
            {
                return Content("导出错误");
            }
            return File(pathFlie, "application/msexcel", fileName);
        }
        //应收明细数据
        public ContentResult GetLoadYSKMCData() 
        {
            string orderKey = Request["orderKey"];
            string mType = Request["mType"];
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.YSKMX(key);
            string msgError = "";
            var data = report.GetReport(orderKey, mType,out msgError);
            if (data == null)
            {
                var o = "[{msg:" + msgError + "}]";
                return Content(o);
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出ExecelReport应收款明细
        public ActionResult ExportYSKMCReport()
        {
            try
            {
                string orderKey = Request["orderKey"];
                string mType = Request["mType"];
                var user = this.CurrentUserInfo;
                string key = user.UserKey;
                var report = new CAE.YSKMX(key);
                string fileName;
                string pathFlie = report.GetExportPath(orderKey, mType, out fileName);
                return File(pathFlie, "application/msexcel", fileName);
            }
            catch (Exception ex)
            {
                return Content("导出错误");//，详细信息为："+ex.Message.ToString());
            }
        }

        //财政拨款一览数据
        public ContentResult GetLoadCZBKData()
        {
            string startDate = Request["startDate"];
            string endDate = Request["endDate"];
            string mType = Request["mType"];
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            int quantility = 1;
            //if (!int.TryParse(mType, out quantility)) quantility = 1;

            var report = new CAE.CZBKYLB(key, "", DateTime.Parse(startDate), DateTime.Parse(endDate), quantility);
            ReportResult result = report.GetReport();
            if (result.Error)
            {
                var o = "[{\"msg\":\"" + result.ErrorMessage + "\"}]";
                return Content(o);
            }

            foreach (System.Data.DataRow dr in result.Result.Rows)
            {
                var jc = dr["t22"];
                string str = "&nbsp;&nbsp;";
                int i=0;
                if (int.TryParse((jc+"").ToString(), out i))
                {
                    i = 5-i;
                }
                string strT="";
                for (int j = 0; j < i; j++)
                {
                    strT += str;
                }
                dr[0] = strT + dr[0].ToString().Trim();
            }

            var json = Utils.JsonHelper.DataTableToJson(result.Result);
            return Content(json);
        }
        //导出ExecelReport财政拨款一览
        public ActionResult ExportCZBKReport()
        {
            try
            {
                string startDate = Request["startDate"];
                string endDate = Request["endDate"];
                string mType = Request["mType"];
                var user = this.CurrentUserInfo;
                string fileName;
                string key = user.UserKey;
                var report = new CAE.CZBKYLB(key, "", DateTime.Parse(startDate), DateTime.Parse(endDate));
                string pathFlie = report.GetExportPath(out fileName);
                return File(pathFlie, "application/msexcel", fileName);
            }
            catch (Exception ex)
            {
                return Content("导出错误");//，详细信息为："+ex.Message.ToString());
            }
        }
        //导入财政拨款一览表的本年支用额度
        public JsonResult ImportCZBKReport() 
        {
            string endDateStr = Request["endDate"];
            string path = Request["filePath"];
            DateTime dt=DateTime.Now;
            if (!DateTime.TryParse(endDateStr, out dt) || string.IsNullOrEmpty(path)) {
                return Json(new { success=false,msg="导入参数错误!"});
            }
            var import = new CZBKYLBImport();
            var msg= import.GetUploadFileData(path, dt);
            if (string.IsNullOrEmpty(msg))
            {
                return Json(new {success=true, msg = "导入成功!" });
            }
            else 
            {
                return Json(new { success = false, msg = "导入失败,原因:" + msg });
            }
        }
        //保存财政拨款一览
        public JsonResult SaveCZBKData() 
        {
        
            var data=Request["data"];
            var date = Request["endDate"];
            var unit = Request["mType"];
            var models = BaothApp.Utils.JsonHelper.JsonToObject<List<CczylbModel>>(data);
            var report = new CAE.CZBKYLB();
            string errormsg = "";
            bool b=report.Save(models,date,unit,ref errormsg);
            return Json(new { success = b,msg=errormsg }, JsonRequestBehavior.AllowGet);
        }
     
       
    }
}