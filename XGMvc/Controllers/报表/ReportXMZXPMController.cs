using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;
using BusinessModel;
using CAE;
using Business.CommonModule;
using System.Data;
using Infrastructure;
using CAE.Report;
namespace BaothApp.Controllers.报表
{
    public class ReportXMZXPMController : SpecificController
    {
        public override string ModelUrl
        {
            get { return ""; }
        }
        public override ViewResult Index() 
        {
            ViewData["MType"] = 10000;// Request["MType"];
            ViewData["edate"] = Request["edate"];
            ViewData["sdate"] = Request["sdate"];
            var year = DateTime.Now.Year;
            int.TryParse(Request["edate"] + "", out year);
            ViewData["Year"] = year;
            ViewData["DepartmentKeys"] = Request["DepartmentKeys"];
            ViewData["funClass"]=Request["FunClassGuids"];
            return View();
        }
        public JsonResult GetLoadData() 
        {
            var Sdate = Request["Sdate"];
            var Edate = Request["Edate"];
            
            var DepartmentKeyStr = Request["DepartmentKeyStr"];
            var FunClass = Request["FunClass"];
            var MType =int.Parse( Request["MType"]+"");
           
            string msgError = "";
            var report = new XMYSZXPM(Sdate, Edate, DepartmentKeyStr.Replace(",", "','"), FunClass.Replace(",", "','"), MType);
            report.OperatorId = CurrentUserInfo.UserGuid;
            var data = report.GetReport(out msgError);
            if (data == null)
            {
                return Json(new { success = false, errMsg =msgError}, JsonRequestBehavior.AllowGet);
            }
            if (data.Rows.Count > 0)
            {
                Session["BMGZMX"] = data;
            }
            string strData = Utils.JsonHelper.DataTableToJsonNEW(data);//" + strData + "
            return Json(strData,JsonRequestBehavior.AllowGet);
        }
    }
}