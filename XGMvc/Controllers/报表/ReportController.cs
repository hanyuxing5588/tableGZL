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
    public class ReportController : SpecificController
    {
        public override string ModelUrl
        {
            get { return ""; }
        }
        #region 项目管理费用执行情况表(按科目)
        // 项目支出执行查询
        public ViewResult XMGLFYFORKM()
        {
            var dt = DateTime.Now;
            if (!DateTime.TryParse(Request["edate"], out dt))
            {
                dt = DateTime.Now;
            };
            ViewData["ProjectKey"] = Request["ProjectKey"] + "";
            ViewData["StartDate"] = string.IsNullOrEmpty(Request["sdate"] + "") ? DateTime.Now.Year + "-01-01" : Request["sdate"];
            ViewData["EndDate"] = string.IsNullOrEmpty(Request["edate"] + "") ? DateTime.Now.ToString("yyyy-MM-dd") : Request["edate"];
            ViewData["Year"] = dt.Year;
            return View("xmglfyforkm");
        }
        // 项目管理费用执行情况表(按科目)
        public ContentResult GetLoadXMGLFYFORKMData()
        {
            string condition = Request["condition"];

            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMGLFYFORKM(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMGLFYFORKM"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出ExecelReport项目管理费用执行情况表(按科目)
        public ActionResult ExportXMGLFYFORKMReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMGLFYFORKM"] != null)
            {
                dt = (DataTable)Session["XMGLFYFORKM"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMGLFYFORKM(key);

            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHead reportheadModel = new CAE.Report.ReportHead();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHead>(reporthead);

            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        //XMGLFYFORKM
        #region 项目管理费用执行情况表(按项目)
        // 项目支出执行查询
        public ViewResult XMGLFYFORXM()
        {
            var dt = DateTime.Now;
            if (!DateTime.TryParse(Request["edate"], out dt))
            {
                dt = DateTime.Now;
            };
            ViewData["ProjectKey"] = Request["ProjectKey"] + "";
            ViewData["StartDate"] = string.IsNullOrEmpty(Request["sdate"] + "") ? DateTime.Now.Year + "-01-01" : Request["sdate"];
            ViewData["EndDate"] = string.IsNullOrEmpty(Request["edate"] + "") ? DateTime.Now.ToString("yyyy-MM-dd") : Request["edate"];
            ViewData["Year"] = dt.Year;
            return View("xmglfyforxm");
        }
        // 项目管理费用执行情况表(按项目)
        public ContentResult GetLoadXMGLFYFORXMData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMGLFYFORXM(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMGLFYFORXM"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出ExecelReport项目管理费用执行情况表(按项目)
        public ActionResult ExportXMGLFYFORXMReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMGLFYFORXM"] != null)
            {
                dt = (DataTable)Session["XMGLFYFORXM"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMGLFYFORXM(key);

            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHead reportheadModel = new CAE.Report.ReportHead();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHead>(reporthead);

            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 部门预算执行排名
        // 部门预算执行排名查询
        public ViewResult BMYSZXPM()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("bmyszxpm");
        }
        // 部门预算执行排名数据
        public JsonResult GetLoadBMYSZXPMData()
        {
            var funcIds=Request["FunClass"];
            var Sdate = Request["Sdate"];
            var Edate = Request["Edate"];
            var MType = Request["MType"];
            int unitMoney = int.Parse(MType);
            var BGYear = DateTime.Parse(Sdate).Year;
            if (string.IsNullOrEmpty(funcIds)) return Json(null);
            funcIds = "'" + funcIds.Replace(",", "','") + "'";
            BusinessEdmxEntities context = new BusinessEdmxEntities();
            var sql = string.Format(
                "select w.*,n.DepartmentName from (select a.DepartmentKey," +
                "sum(ISNULL(b.zjf,0)) as zjf,sum(ISNULL(c.xmzjcb,0)) as xmzjcb,sum(ISNULL(f.zxs,0)) as zxs " +
                "from BG_MainView a " +
                "left join " +
                "(select sum(Total_BG) as zjf,GUID_BG_Main from BG_DetailView where BGItemKey in ('03','04','07','08') and BGYear={3} and LEN(BGCodeKey)=2 group by GUID_BG_Main) b " +
                "on a.GUID=b.GUID_BG_Main " +
                "left join " +
                "(select sum(Total_BG) as xmzjcb,GUID_BG_Main from BG_DetailView where BGItemKey in ('07','08') and BGYear={3} and LEN(BGCodeKey)=2 group by GUID_BG_Main) c " +
                "on a.GUID=c.GUID_BG_Main " +
                "left join " +
                "(select d.GUID_Project,SUM(d.Total_BX) as zxs from BX_DetailView d left join BX_MainView e on d.GUID_BX_Main=e.GUID " +
                "where (isnull(DocState,0)=999 or isnull(DocState,0)=-1) and e.DocDate>='{1}' and e.DocDate<='{2}' and d.GUID_Project in (" +
                "select GUID from SS_Project where GUID_FunctionClass in ({0}) " +
                "and GUID not in (select PGUID from SS_Project where PGUID is not null) " +
                ") group by d.GUID_Project) f " +
                "on a.GUID_Project=f.GUID_Project " +
                "where a.GUID_Project in (" +
                "select GUID from SS_Project where GUID_FunctionClass in ({0}) " +
                "and GUID not in (select PGUID from SS_Project where PGUID is not null) " +
                "and a.guid in (select guid_bg_main from bg_detail where bgyear={3}) " +
                "and isnull(a.Invalid,0)=1 and a.BGStepKey='05') group by a.DepartmentKey" +
                ") w left join SS_Department n on w.DepartmentKey=n.DepartmentKey " +
                " where w.DepartmentKey!='05' and w.DepartmentKey!='02'   order by zxs desc", funcIds, Sdate, Edate, BGYear);
            var results = context.ExecuteStoreQuery<bmyszxpmResult>(sql).ToList();
            foreach (var item in results)
            {
                item.Org_xmzjcb = item.xmzjcb;
                item.Org_zjf = item.zjf;
                item.Org_zxs = item.zxs;
                item.jy = item.xmzjcb - item.zxs;
                item.Org_jy = item.jy;
                item.zxl = item.xmzjcb == 0 ? 0 : item.zxs / item.xmzjcb;               
            }
            results = results.OrderByDescending(e => e.zxl).ToList();
            int i = 1;
            bmyszxpmResult total = new bmyszxpmResult();
            foreach (var item in results)
            {
                total.xmzjcb += item.xmzjcb;
                total.zjf += item.zjf;
                total.zxs += item.zxs;
                item.pm = i.ToString();
                i++;
            }
            //增加合计项
            total.DepartmentName = "合计";
            total.Org_xmzjcb = total.xmzjcb;
            total.Org_zjf = total.zjf;
            total.Org_zxs = total.zxs;
            total.jy = total.xmzjcb - total.zxs;
            total.Org_jy = total.jy;
            total.zxl = total.xmzjcb == 0 ? 0 : total.zxs / total.xmzjcb;
            results.Add(total);
            return Json(results, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 项目支出执行
        // 项目支出执行查询
        public ViewResult XMZXZCCX()
        {
            var dt=DateTime.Now;
            if (!DateTime.TryParse(Request["edate"], out dt)) {
                dt = DateTime.Now;
            };
            ViewData["ProjectKey"] = Request["ProjectKey"] + "";
            ViewData["StartDate"] = string.IsNullOrEmpty(Request["sdate"] + "") ? DateTime.Now.Year + "-01-01" : Request["sdate"];
            ViewData["EndDate"] = string.IsNullOrEmpty(Request["edate"] + "") ? DateTime.Now.ToString("yyyy-MM-dd") : Request["edate"];
            ViewData["Year"] = dt.Year;
            return View("xmzxzcqk");
        }
        // 项目支出执行查询数据
        public ContentResult GetLoadXMZCZXQKData() 
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMZCZXQK(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMZCZXQK"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出ExecelReport项目执行查询表

        public ActionResult ExportXMZCZXQKReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMZCZXQK"] != null)
            {
                dt = (DataTable)Session["XMZCZXQK"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMZCZXQK(key);   
         
            var reporthead= Request["ReportHead"];
            CAE.Report.ReportHead reportheadModel = new CAE.Report.ReportHead();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHead>(reporthead);

            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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

        #endregion

        #region 执行率汇总表
        // 执行率汇总表
        public ViewResult ZXLHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("zxlhzb");
        }
        // 执行率汇总表
        public ContentResult GetLoadZXLHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.ZXLHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["ZXLHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 执行率汇总表

        /// </summary>
        /// <returns></returns>
        public ActionResult ExportZXLHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["ZXLHZB"] != null)
            {
                dt = (DataTable)Session["ZXLHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.GLFYZXCX(key);
            report.colChar = "";
            report.DepName = Request["depname"];
            report.Date = Request["date"];
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion


        #region 项目分摊执行情况
        // 项目分摊执行情况
        public ViewResult XMFTZXQK()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("xmftzxqk");
        }
        // 项目分摊执行情况
        public ContentResult GetLoadXMFTZXQKData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMFTZXQK(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMFTZXQK"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 项目分摊执行情况

        /// </summary>
        /// <returns></returns>
        public ActionResult ExportXMFTZXQKReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMFTZXQK"] != null)
            {
                dt = (DataTable)Session["XMFTZXQK"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMFTZXQK(key);
            report.colChar = "";
            report.DepName = Request["depname"];
            report.Date = Request["date"];
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion


        #region 管理费用执行查询
        // 管理费用执行查询
        public ViewResult GLFYZXCX()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("glfyzxcx");
        }
        // 管理费用执行数据
        public ContentResult GetLoadGLFYZXCXData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.GLFYZXCX(key);
          
            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["GLFYZXCX"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        
        /// <summary>
        /// 管理费执行查询报告
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportGLFYZXCXReport()
        {
            DataTable dt = new DataTable();
            if (Session["GLFYZXCX"] != null)
            {
                dt = (DataTable)Session["GLFYZXCX"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.GLFYZXCX(key);
            report.colChar = "";
            report.DepName = Request["depname"];
            report.Date = Request["date"];
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 基本支出预算科目汇总表
        public ViewResult JBZCYSKMHZB()
        {
            return View("jbzcyskmhzb");
        }
        //查询
        public ContentResult GetLoadJBZCYSKMHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.JBZCYSKMHZB(key);
            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["jbzcyskmhzb"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出Execel
        public ActionResult ExportJBZCYSKMHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["jbzcyskmhzb"] != null)
            {
                dt = (DataTable)Session["jbzcyskmhzb"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.JBZCYSKMHZB(key);
            report.DepName = Request["depname"];
            report.Date = Request["date"];
            report.Unit = Request["unit"];
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region  项目支出科目汇总表 项目费用预算科目汇总表
        public ViewResult XMFYYSKMHZB()
        {
            return View("xmfyyskmhzb");
        }
        //查询
        public ContentResult GetLoadXMFYYSKMHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMFYYSKMHZB(key);
            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["xmzckmhzb"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        //导出Execel
        public ActionResult ExportXMFYYSKMHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["xmzckmhzb"] != null)
            {
                dt = (DataTable)Session["xmzckmhzb"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMFYYSKMHZB(key);
            report.DepName = Request["depname"];
            report.Date = Request["date"];
            report.Unit = Request["unit"];
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 总支出汇总表
        // 总支出汇总表
        public ViewResult ZZCHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("zzchzb");
        }
        // 总支出汇总表
        public ContentResult GetLoadZZCHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.ZZCHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["ZZCHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 总支出汇总表报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportZZCHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["ZZCHZB"] != null)
            {
                dt = (DataTable)Session["ZZCHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.ZZCHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ZZCHZBReportHead reportheadModel = new CAE.Report.ZZCHZBReportHead();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ZZCHZBReportHead>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 项目分类汇总表
        /// <summary>
        /// 项目分类汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult XMFLHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("xmflhzb");
        }
        /// <summary>
        /// 加载项目分类汇总表
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadXMFLHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMFLHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMFLHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 项目分类汇总表报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportXMFLHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMFLHZB"] != null)
            {
                dt = (DataTable)Session["XMFLHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMFLHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.XMFLHZBReportHead reportheadModel = new CAE.Report.XMFLHZBReportHead();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.XMFLHZBReportHead>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 人员支出汇总表
        /// <summary>
        /// 人员支出汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult RYZCHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("ryzchzb");
        }
        /// <summary>
        /// 加载 人员支出汇总表
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadRYZCHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.RYZCHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["RYZCHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 人员支出汇总表报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportRYZCHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["RYZCHZB"] != null)
            {
                dt = (DataTable)Session["RYZCHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.RYZCHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 单位预算汇总表
        /// <summary>
        /// 单位预算汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult DWYSHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("dwyshzb");
        }
        /// <summary>
        /// 加载 单位预算汇总表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetLoadDWYSHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.DWYSHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Json(o,JsonRequestBehavior.AllowGet);
            }
            if (data.Rows.Count > 0)
            {
                Session["DWYSHZB"] = data;
            }            
            Common.DataGridModel.JDataGrid dg = new Common.DataGridModel.JDataGrid();
            dg.total = data.Rows.Count - 1;
            dg.rows = Common.DataGridModel.JDataGrid.ConvertRows(data);
            dg.columns = Common.DataGridModel.JDataGrid.ConvertColumns(data);
            //var json = dg.ConvertToJson();
            return Json(dg, JsonRequestBehavior.AllowGet);
        }
       
        /// <summary>
        /// 单位预算汇总表 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportDWYSHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["DWYSHZB"] != null)
            {
                dt = (DataTable)Session["DWYSHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.DWYSHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 单位预算汇总表
        /// <summary>
        /// 部门预算汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult BMYSHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("bmyshzb");
        }
        /// <summary>
        /// 加载 部门预算汇总表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetLoadBMYSHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.BMYSHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Json(o, JsonRequestBehavior.AllowGet);
            }
            if (data.Rows.Count > 0)
            {
                Session["BMYSHZB"] = data;
            }
            Common.DataGridModel.JDataGrid dg = new Common.DataGridModel.JDataGrid();
            dg.total = data.Rows.Count - 1;
            dg.rows = Common.DataGridModel.JDataGrid.ConvertRows(data);
            dg.columns = Common.DataGridModel.JDataGrid.ConvertColumns(data);
            //var json = dg.ConvertToJson();
            return Json(dg, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 部门预算汇总表 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportBMYSHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["BMYSHZB"] != null)
            {
                dt = (DataTable)Session["BMYSHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.BMYSHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 项目支出科目汇总表
        /// <summary>
        /// 项目支出科目汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult XMZCKMHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("xmzckmhzb");
        }
        /// <summary>
        /// 加载 项目支出科目汇总表
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadXMZCKMHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMZCKMHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMZCKMHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 项目支出科目汇总表 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportXMZCKMHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMZCKMHZB"] != null)
            {
                dt = (DataTable)Session["XMZCKMHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMZCKMHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 项目支出汇总表
        /// <summary>
        /// 项目支出汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult XMZCHZB()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("xmzchzb");
        }
        /// <summary>
        /// 加载 项目支出汇总表
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadXMZCHZBData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.XMZCHZB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["XMZCHZB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 项目支出汇总表 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportXMZCHZBReport()
        {
            DataTable dt = new DataTable();
            if (Session["XMZCHZB"] != null)
            {
                dt = (DataTable)Session["XMZCHZB"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.XMZCHZB(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 执行进度一览
        /// <summary>
        /// 执行进度一览
        /// </summary>
        /// <returns></returns>
        public ViewResult ZXJDYL()
        {
            ViewData["StartDate"] = DateTime.Now.Year + "-01-01";
            ViewData["EndDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["Year"] = DateTime.Now.Year;
            return View("zxjdyl");
        }
        /// <summary>
        /// 加载 执行进度一览
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadZXJDYLData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.ZXJDYL(key);

            string msgError = "";
            var data = report.GetReportEx(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["ZXJDYL"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 执行进度一览 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportZXJDYLReport()
        {
            DataTable dt = new DataTable();
            if (Session["ZXJDYL"] != null)
            {
                dt = (DataTable)Session["ZXJDYL"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.ZXJDYL(key);
            var reporthead = Request["ReportHead"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
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
        #endregion

        #region 执行进度(直接成本+管理费分摊)

        /// <summary>
        /// 执行进度一览

        /// </summary>
        /// <returns></returns>
        public ViewResult ZXJD()
        {
            return View("zxjd");
        }
        /// <summary>
        /// 加载 执行进度一览

        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadzxjdData()
        {
            string condition = Request["condition"];
            BBSearchCondition conditionModel = new BBSearchCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BBSearchCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.ZXJD(key);

            string msgError = "";
            var data = report.GetReportEx(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["ZXJD"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }
        private void DeleteLineBreak(ref string str)
        {
            str = str.Replace("\r\n", "");
            str = str.Replace("\r", "");
            str = str.Replace("\n", "");
            str = str.Replace("\b", "");
            str = str.Replace("\f", "");
            str = str.Replace("\t", "");
            str = str.Replace("\'", "\\'");
            str = str.Replace("\"", "\\\"");
            str = str.Replace("\\", "\\\\");
        }
        public ActionResult ExportZXJDReportEx()
        {
            string strPath = Request["FilePath"].ToString();
            string strFileName = System.IO.Path.GetFileName(strPath);
            return File(strPath, "application/msexcel", strFileName);
        }
        /// <summary>
        /// 执行进度一览 报表导出
        /// </summary>
        /// <returns></returns>
        public ContentResult ExportZXJDReport()
        {
            DataTable dt = new DataTable();
            if (Session["ZXJD"] != null)
            {
                dt = (DataTable)Session["ZXJD"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.ZXJD(key);
            var year = Request["Year"];
            var moneyUnit = Request["Money"];
            var DepartmentName = Request["Department"];
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel.Year = year;
            reportheadModel.RMBUnit = moneyUnit;
            reportheadModel.DepartmentName = DepartmentName;
            //reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else
                {
                    return Content("导出错误！");
                }
            }
            DeleteLineBreak(ref pathFlie);
            string strJson = "{\"File\":\"" + pathFlie + "\"}";
            return Content(strJson);
            //return File(pathFlie, "application/msexcel", fileName);
        }
        #endregion

        #region 部门工资项目汇总

        /// <summary>
        /// 部门工资项目汇总

        /// </summary>
        /// <returns></returns>
        public ViewResult BMGZXMHZ()
        {
            return View("bmgzxmhz");
        }
        /// <summary>
        /// 加载 部门工资项目汇总

        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadBMGZXMHZData()
        {
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.BMGZHZ(key);
            var year = Request["Year"];
            var emonth = Request["EMonth"];
            var smonth = Request["SMonth"];
            var Department = Request["Department"];
            var PlanName = Request["PlanName"];
            string msgError = "";
            string strColumns = "";
            string strData = "";
            var data = report.GetReport(year,smonth, emonth, Department, PlanName, ref strColumns, ref strData, out msgError);
            if (data==null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + msgError + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["BMGZHZ"] = data;
            }
            string strJson = "{\"success\": true,\"column\":" + "\"" + strColumns + "\"" +  ",\"data\":\"" + strData + "\"}";

            return Content(strJson);
        }
        public ActionResult ExportBMGZXMHZReportEx()
        {
            string strPath = Request["FilePath"].ToString();
            string strFileName = System.IO.Path.GetFileName(strPath);
            return File(strPath, "application/msexcel", strFileName);
        }
        /// <summary>
        /// 执行进度一览 报表导出
        /// </summary>
        /// <returns></returns>
        public ContentResult ExportBMGZXMHZReport()
        {
            DataTable dt = new DataTable();
            if (Session["BMGZHZ"] != null)
            {
                dt = (DataTable)Session["BMGZHZ"];
            }
            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.BMGZHZ(key);
            var emonth = Request["EMonth"];
            var smonth = Request["SMonth"];
            string strDepartmentName = Request["Department"].ToString();
            string strSAPlan = Request["SAPlan"];
            DateTime time = DateTime.Now;
            string strPrintDate = time.ToLongDateString();
            string strOperatorName = this.CurrentUserInfo.UserName;
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            reportheadModel.DepartmentName = strDepartmentName;
            reportheadModel.Month = smonth + "-" + emonth;
            reportheadModel.PrintDate = "打印日期: " + strPrintDate;
            reportheadModel.Maker = "制表人: " + strOperatorName;
            reportheadModel.Expand = strSAPlan;
            //reportheadModel = BaothApp.Utils.JsonHelper.JsonToObject<CAE.Report.ReportHeadModel>(reporthead);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else
                {
                    return Content("导出错误！");
                }
            }

            DeleteLineBreak(ref pathFlie);
            string strJson = "{\"File\":\"" + pathFlie + "\"}";
            return Content(strJson);
            //return File(pathFlie, "application/msexcel", fileName);
        }
        #endregion

        #region  部门工资明细表
        public ViewResult BMGZMX()
        {
            return View("bmgzmx");
        }

        public ContentResult GetLoadBMGZMXData()
        {
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
        
            var year =int.Parse( Request["Year"]);
            //var month = int.Parse(Request["Month"]);
            var emonth = int.Parse(Request["EMonth"]);
            var smonth = int.Parse(Request["SMonth"]);
            var DepartmentKeyStr = Request["DepartmentKeyStr"];
            var GUID_Plan =Guid.Parse(Request["GUID_Plan"]);
            var report = new CAE.Report.BMMXB(year,smonth, emonth, GUID_Plan, DepartmentKeyStr);
            string msgError = "";
          
            var data = report.GetReport();
            if (data == null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + report.Msg + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["BMGZMX"] = data;
            }
            string strColumns = report.GetColums();
            string strData = Utils.JsonHelper.DataTableToJsonNEW(data);//" + strData + "
            string strJson = "{\"success\": true,\"column\":\""+strColumns+"\",\"data\":[\""+strData+"\"]}";

            return Content(strJson);
        }
        public ActionResult ExcelBMGZMXData()
        {
            DataTable dt = new DataTable();            
            string fileName, errorMsg;
            if (Session["BMGZMX"] != null)
            {
                dt = (DataTable)Session["BMGZMX"];
            }
            else
            {
                errorMsg = "-1";
            }
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();

            var year = int.Parse(Request["Year"]);
            //var month = int.Parse(Request["Month"]);
            var emonth = int.Parse(Request["EMonth"]);
            var smonth = int.Parse(Request["SMonth"]);
            var DepartmentKeyStr = Request["DepartmentKeyStr"];
            var GUID_Plan = Guid.Parse(Request["GUID_Plan"]);
            var report = new CAE.Report.BMMXB(year, smonth, emonth, GUID_Plan, DepartmentKeyStr);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else if (errorMsg == "-1")
                {
                    return Content("数据已经过期,请查询后再导出！"); 
                }
                else
                {
                    return Content("导出错误！");
                }
            }
            return File(pathFlie, "application/msexcel", fileName);
        }
        #endregion

        #region 个人收入查询
        public ViewResult GRSRCX()
        {
            return View("grsrcx");
        }
        public ContentResult GetLoadGRSRCXData()
        {
            var year = Request["Year"];
            var GZSR = Request["GZSR"];
            var JTQT = Request["JTQT"];
            var GZ = Request["GZ"];
            var JJ = Request["JJ"];
            var QT = Request["QT"];
            Dictionary<string, string> DicFormula = new Dictionary<string, string>();
            DicFormula.Add("GZSR", GZSR);
            DicFormula.Add("JTQT", JTQT);
            DicFormula.Add("GZ", GZ);
            DicFormula.Add("JJ", JJ);
            DicFormula.Add("QT", QT);

            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.GRSRCX(key);
            string strErr = "";
            string strColumns = "";
            string strSaShowData = "";
            string strHzData = "";
            string strReportData = "";
            string strLWFData = "";
            DataTable saShowTable = new DataTable();
            DataTable hzTable = new DataTable();
            DataTable reportTable = new DataTable();
            DataTable lwfTable = new DataTable();
            bool bReport = report.GetReport(year,key,DicFormula,ref saShowTable,ref hzTable,ref reportTable,
                ref lwfTable,ref strColumns,ref strSaShowData,ref strHzData,ref strReportData,ref strLWFData ,out strErr);

            if (saShowTable.Rows.Count > 0)
            {
                Session["GRSRsa"] = saShowTable;
            }
            else
            {
                Session["GRSRsa"] = null;
                strSaShowData = "";
            }
            //if (hzTable.Rows.Count > 0)
            //{
            //    Session["GRSRhz"] = hzTable;
            //}
            //else
            //{
            //    Session["GRSRhz"] = null;
            //    strHzData = "";
            //}
            if (reportTable.Rows.Count > 0)
            {
                Session["GRSRbg"] = reportTable;
                Session["GRSRhz"] = reportTable;
            }
            else
            {
                Session["GRSRbg"] = null;
                strReportData = "";
            }
            if (lwfTable.Rows.Count > 0)
            {
                Session["GRSRlwf"] = lwfTable;
            }
            else
            {
                Session["GRSRlwf"] = null;
                strLWFData = "";
            }
            string strJson = "{\"success\": true,\"column\":" + "\"" + strColumns + "\"" + ",\"saData\":\"" + strSaShowData +
                "\",\"HzData\":\"" + strHzData + "\",\"ReportData\":\"" + strReportData + "\",\"lwfData\":\"" + strLWFData + "\"}";
            return Content(strJson);
        }

        public ContentResult ExportGRSRReport()
        {
            DataTable saTable = new DataTable();
            if (Session["GRSRsa"] != null)
            {
                saTable = (DataTable)Session["GRSRsa"];
            }

            DataTable hzTable = new DataTable();
            if (Session["GRSRhz"] != null)
            {
                hzTable = (DataTable)Session["GRSRhz"];
            }

            DataTable bgTable = new DataTable();
            if (Session["GRSRbg"] != null)
            {
                bgTable = (DataTable)Session["GRSRbg"];
            }

            DataTable lwfTable = new DataTable();
            if (Session["GRSRlwf"] != null)
            {
                lwfTable = (DataTable)Session["GRSRlwf"];
            }

            string fileName, errorMsg;
            var user = this.CurrentUserInfo;
            string key = user.UserKey;
            var report = new CAE.Report.GRSRCX(key);
            CAE.Report.ReportHeadModel reportheadModel = new CAE.Report.ReportHeadModel();
            string pathFlie = report.GetExportPath(saTable, hzTable,bgTable, lwfTable,out fileName, out errorMsg, reportheadModel);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else
                {
                    return Content("导出错误！");
                }
            }

            DeleteLineBreak(ref pathFlie);
            string strJson = "{\"File\":\"" + pathFlie + "\"}";
            return Content(strJson);
        }

        public ActionResult ExportGRSRReportEx()
        {
            string strPath = Request["FilePath"].ToString();
            string strFileName = System.IO.Path.GetFileName(strPath);
            return File(strPath, "application/msexcel", strFileName);
        }
        #endregion

        #region 个税申报表
        public ViewResult gssbb()
        {
            return View("gssbb");
        }
        /// <summary>
        /// 加载 项目支出汇总表
        /// </summary>
        /// <returns></returns>
        public ContentResult GetLoadGSSBBData()
        {
            string condition = Request["condition"];
            gssbbCondition conditionModel = new gssbbCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<gssbbCondition>(condition);
            var user = this.CurrentUserInfo;
            string key = user.UserGuid.ToString();
            var report = new CAE.Report.GSSBB(key);

            string msgError = "";
            var data = report.GetReport(conditionModel, out msgError);
            if (data == null)
            {
                var o = "[{\"msg\":\"" + msgError + "\"}]";
                return Content(o);
            }
            if (data.Rows.Count > 0)
            {
                Session["GSSBB"] = data;
            }
            var json = Utils.JsonHelper.DataTableToJson(data);
            return Content(json);
        }

        /// <summary>
        /// 项目支出汇总表 报表导出
        /// </summary>
        /// <returns></returns>
        public ActionResult ExportGSSBBReport()
        {
            DataTable dt = new DataTable();            
            string fileName, errorMsg;
            if (Session["GSSBB"] != null)
            {
                dt = (DataTable)Session["GSSBB"];
            }
            else
            {
                errorMsg = "-1";
            }

            var user = this.CurrentUserInfo;
            string key = user.UserKey;        
            var report = new CAE.Report.GSSBB(key);
            string pathFlie = report.GetExportPath(dt, out fileName, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    return Content("没有可导出的数据！");
                }
                else if (errorMsg == "-1")
                {
                    return Content("数据已经过期,请查询后再导出！"); 
                }
                else
                {
                    return Content("导出错误！");
                }
            }
            return File(pathFlie, "application/msexcel", fileName);
        }
        #endregion

        #region 国家基础地理信息中心2015年财政预算执行情况 2015-10-29
        public ViewResult CZYSZXQK() 
        {

            return View("CZYSZXQK");
        }
        public ContentResult GetCZYSZXQKData()
        {
            try
            {
                var SDate = Request["SDate"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["SDate"] + "");
                var EDate = Request["EDate"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["EDate"] + "");
                var report = new CZYSZXQK();
                report.StartDate = SDate;
                report.EndDate = EDate;
                var data = report.GetReport();
                var json = Utils.JsonHelper.DataTableToJson(data);
                var ents = BaothApp.Utils.JsonHelper.JsonToObject<List<CZYSZXQKModel>>(json);
                report.Save(ents);
                Session["IsSave"] = 0;
                if (!string.IsNullOrEmpty(report.msg))
                {
                    return Content(report.msg);
                }
                return Content(json);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
           
          
        }

        public JsonResult SaveCZYSZXQKData()
        {
            var SDate = Request["date"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["date"] + "");
            var EDate = Request["date"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["date"] + "");
            var data = Request["data"];
            var ents = BaothApp.Utils.JsonHelper.JsonToObject<List<CZYSZXQKModel>>(data);
            var report = new CZYSZXQK();
            report.StartDate = SDate;
            report.EndDate = EDate;
            Session["IsSave"] = 1;
            var remsg = report.Save(ents);
            return Json(remsg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportCZYSZXQK()
        {
            
            DataTable dt = new DataTable();
            var report = new CZYSZXQK();
            var SDate = Request["SDate"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["SDate"] + "");
            var EDate = Request["EDate"] + "" == "" ? DateTime.Now : DateTime.Parse(Request["EDate"] + "");
            report.StartDate = SDate;
            report.EndDate = EDate;
            dt = report.GetExprotDataTable();
            var fileName = "";
            var pathFlie = report.GetExportPath(dt, 5, out fileName);
            return File(pathFlie, "application/msexcel", fileName);
        }
        #endregion

        #region 工行代发模板银行报盘表
        public ActionResult ghyhbpb() 
        {
            return View("ghyhbpb");
        }
        public ContentResult GetLoadghyhbpbData()
        {
            var report = new CAE.Report.GZReport();
            var year = Request["Year"];
            var smonth = Request["Month"];
            var panID = Request["PlanId"];
            string msgError = "";
            report.Year = year;
            report.Month = smonth;
            report.PlanId = panID;
            var data = report.GetGHReport(out msgError);
            if (data == null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + msgError + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["GHYHBPB"] = data;
            }
            string strData = Utils.JsonHelper.DataTableToJson(data);
            return Content(strData);
        }
        public ActionResult ExportghyhbpbReport()
        {
            DataTable dt = new DataTable();
            var report = new CAE.Report.GZReport();
            if (Session["GHYHBPB"] != null)
            {
                dt = (DataTable)Session["GHYHBPB"];
            }
            else
            {
               
                var year = Request["Year"];
                var smonth = Request["Month"];
                var panID = Request["PlanId"];
                string msgError = "";
                report.Year = year;
                report.Month = smonth;
                report.PlanId = panID;
                dt = report.GetGHReport(out msgError);

            }
            string filename = "", errorMsg="";
            string pathFlie = report.GetExportPath(dt, out filename, out errorMsg);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                    return Content("导出错误！");
            }
            return File(pathFlie, "application/msexcel", filename);
        }
        #endregion

        #region 中信银行代发模板银行报盘表
        public ActionResult zxyhbpb()
        {
            return View("zxyhbpb");
        }
        public ContentResult GetLoadzxyhbpbData()
        {
            var report = new CAE.Report.GZReport(true);
            var year = Request["Year"];
            var smonth = Request["Month"];
            var panID = Request["PlanId"];
            string msgError = "";
            report.Year = year;
            report.Month = smonth;
            report.PlanId = panID;
            var data = report.GetGHReport(out msgError);
            if (data == null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + msgError + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["zxyhbpb"] = data;
            }
            string strData = Utils.JsonHelper.DataTableToJson(data);
            return Content(strData);
        }
        public ActionResult ExportzxyhbpbReport()
        {
            DataTable dt = new DataTable();
            var report = new CAE.Report.GZReport(true);
            if (Session["zxyhbpb"] != null)
            {
                dt = (DataTable)Session["zxyhbpb"];
            }
            else
            {

                var year = Request["Year"];
                var smonth = Request["Month"];
                var panID = Request["PlanId"];
                string msgError = "";
                report.Year = year;
                report.Month = smonth;
                report.PlanId = panID;
                dt = report.GetGHReport(out msgError);

            }
            string filename = "", errorMsg = "";
            string pathFlie = report.GetExportPath(dt, out filename, out errorMsg);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                return Content("导出错误！");
            }
            return File(pathFlie, "application/msexcel", filename);
        }
        #endregion

        #region 个人所得税基础信息表(A表) 
        public ActionResult KJGRSDSBGB()
        {
            return View("KJGRSDSBGB");
        }
        public ContentResult GetLoadKJGRSDSBGBData()
        {
            var report = new CAE.Report.KJGRSDSBGB();
            var year = Request["Year"];
            var smonth = Request["Month"];
            string msgError = "";
            report.Year = year;
            report.Month = smonth;
            var data = report.GetGHReport(out msgError);
            if (data == null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + msgError + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["KJGRSDSBGB"] = data;
            }
            string strData = Utils.JsonHelper.DataTableToJson(data);
            return Content(strData);
        }
        public ActionResult ExportKJGRSDSBGBReport()
        {
            DataTable dt = new DataTable();
            var report = new CAE.Report.KJGRSDSBGB();
            if (Session["KJGRSDSBGB"] != null)
            {
                dt = (DataTable)Session["KJGRSDSBGB"];
            }
            else
            {

                var year = Request["Year"];
                var smonth = Request["Month"];
                string msgError = "";
                report.Year = year;
                report.Month = smonth;
                dt = report.GetGHReport(out msgError);

            }
            string filename = "", errorMsg = "";
            string pathFlie = report.GetExportPath(dt, out filename, out errorMsg);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                return Content("导出错误！");
            }
            return File(pathFlie, "application/msexcel", filename);
        }
        #endregion
        #region 扣缴个人所得税报告表
        public ActionResult sdsbg()
        {
            return View("sdsbg");
        }
        public ContentResult GetLoadsdsbgData()
        {
            var report = new CAE.Report.SDSBG();
            var year = Request["Year"];
            var smonth = Request["Month"];
            string msgError = "";
            report.Year = year;
            report.Month = smonth;
            var data = report.GetGHReport(out msgError);
            if (data == null)
            {
                string strErr = "{\"success\": false ,\"errMsg\":\"" + msgError + "\"}";
                return Content(strErr);
            }
            if (data.Rows.Count > 0)
            {
                Session["SDSBG"] = data;
            }
            string strData = Utils.JsonHelper.DataTableToJson(data);
            return Content(strData);
        }
        public ActionResult ExportsdsbgReport()
        {
            DataTable dt = new DataTable();
            var report = new CAE.Report.SDSBG();
            if (Session["SDSBG"] != null)
            {
                dt = (DataTable)Session["SDSBG"];
            }
            else
            {

                var year = Request["Year"];
                var smonth = Request["Month"];
                string msgError = "";
                report.Year = year;
                report.Month = smonth;
                dt = report.GetGHReport(out msgError);

            }
            string filename = "", errorMsg = "";
            string pathFlie = report.GetExportPath(dt, out filename, out errorMsg);

            if (!string.IsNullOrEmpty(errorMsg))
            {
                return Content("导出错误！");
            }
            return File(pathFlie, "application/msexcel", filename);
        }
        #endregion
    }
    public class User
    {
        public User()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string Hobby { get; set; }
    }

    class bmyszxpmResult
    {
        public string DepartmentKey { get; set; }
        public string DepartmentName { get; set; }
        public double zjf { get; set; }
        public double xmzjcb { get; set; }
        public double zxs { get; set; }
        public double jy { get; set; }
        public double zxl { get; set; }
        public string pm { get; set; }
        public double Org_zjf { get; set; }
        public double Org_xmzjcb { get; set; }
        public double Org_zxs { get; set; }
        public double Org_jy { get; set; }
    }
}