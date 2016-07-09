using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CAE;
using BaothApp.Utils;
using System.IO;
using System.Text;

namespace BaothApp.Controllers.报表
{
    public class ReportChartController : SpecificController
    {
        /*财政表的项目*/
        //public ContentResult CZXM() 
        //{
        //   var year=string.IsNullOrEmpty(Request["year"])?DateTime.Now.Year:int.Parse(Request["year"]);
        //   var dt = new CZYSZXQKB().GetXMToRow(year);
        //   return Content(JsonHelper.DataTableToJson(dt));
        //}
        public JsonResult CZXM()
        {
            var year = string.IsNullOrEmpty(Request["year"]) ? DateTime.Now.Year : int.Parse(Request["year"]);
            var dt = new ChartLineReport().GetXMToRow(year);
            return Json(dt,JsonRequestBehavior.AllowGet);
        }
        //预算执行情况进度分析
        public ActionResult LchartJDFX()
        {
            return View("lineChart");
        }
        //财政拨款专项经费执行情况图表
        public ActionResult PchartZXQK()
        {
            return View("pieChart");
        }
        //分部门借款情况分析图表
        public ActionResult ZchartQKFX()
        {
           ViewData["year"]= Request["year"];
           ViewData["month"] = Request["month"];
           ViewData["xmKey"] = Request["xmKey"];
           ViewData["isLine"] = Request["isLine"];
            return View("zzhuChart");
        }
        public ActionResult ZLinechartQKFX()
        {
            ViewData["year"] = Request["year"];
            ViewData["month"] = Request["month"];
            ViewData["xmKey"] = Request["xmKey"];
            ViewData["isLine"] = Request["isLine"];
            return View("zlineChart");
        }
        public JsonResult LoadChartData() 
        {
            int temp = 0;
            int.TryParse( Request["year"]+"",out temp);
            var year = temp;
            temp=0;
            int.TryParse(Request["month"] + "", out temp);
            var month = temp;
            temp = 0;
            int.TryParse(Request["xmKey"] + "", out temp);
            var xmkey = temp;
            temp = 0;
            int.TryParse(Request["isLine"] + "", out temp);
            var isLine = temp;
            if (isLine == 0)
            {//
                var report = new CAE.ChartLineReport(year, month, xmkey);
                var result = report.GetLineReport();
                if (!string.IsNullOrEmpty(result.Error))
                {
                    return Json(new { msg = result.Error });
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else 
            {
                var report = new CAE.ChartZhuReport(year, month, xmkey);
                var result = report.GetZhuReport();
                if (!string.IsNullOrEmpty(result.Error))
                {
                    return Json(new { msg = result.Error });
                }
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult FileData() 
        {
            ReportExportHelper helper = new ReportExportHelper();
            string svg = Request.Form["svg"].ToString();
            string type = Request.Form["type"].ToString();
            svg = svg.Replace("<svg xmlns:xlink=\"http://www.w3.org/1999/xlink\"", "<svg");
            svg = svg.Replace("<svg xmlns=\"http://www.w3.org/2000/svg\"", "<svg");

            helper.ExportData = new MemoryStream(Encoding.UTF8.GetBytes(svg));
            switch (type)
            {
                case "image/png":
                    helper.ExportType = ExportType.PNG;
                    break;
                case "image/jpeg":
                    helper.ExportType = ExportType.JPG;
                    break;
                case "application/pdf":
                    helper.ExportType = ExportType.PDF;
                    break;
                case "image/svg+xml":
                    helper.ExportType = ExportType.SVG;
                    break;
            }

            helper.FileName = "chart";
            helper.Export(System.Web.HttpContext.Current.Response);
            return null;
        }
    }
}
