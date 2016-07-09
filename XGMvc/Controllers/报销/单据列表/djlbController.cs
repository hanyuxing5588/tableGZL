using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.Reimbursement;
using Business.CommonModule;
using Business.CommonModule.Search;
using System.Data;
using System.Reflection;
namespace BaothApp.Controllers.报销.报销单列表

{   
    public class djlbController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "djlb"; }
        }
        //带反核销的单据列表
        public  ViewResult fhxcl()
        {
            ViewData["scope"] = "djlb";
            ViewData["status"] = this.Request["status"];
            ViewData["guid"] = this.Request["guid"];
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            return View("fhxdjlb");
        }

        //反核销
        public JsonResult FHX() {
            var billId=Request["billId"];
                var DocTypeKey = Request["DocTypeKey"];
            var dls = new 单据列表(this.CurrentUserInfo.UserGuid, "djlb");
            Guid billIdGUid = Guid.Empty;
            if (Guid.TryParse(billId, out billIdGUid))
            {
                var c = dls.FanHX(billIdGUid, DocTypeKey);
                return Json(new {msg=c});
            }
            return Json(new { msg = "参数错误" });
        }
        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            return View("djlb");
        }

        public ViewResult filter() 
        {
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            ViewData["ModelUrl"] = "filter";
            return View("filter");
        }

        /// <summary>
        /// 保存功能
        /// </summary>
        /// <returns></returns>
        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }

        public ActionResult Export() 
        {
           

            try
            {
                var list = Session["djlbData"] as List<DocListSearchResult>;
                if (list == null) {
                    return Content("导出错误");
                }
                var dt = list.ToDataTable();
               var  template = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], "djlb.xls");
               var export = CAE.ExportExcel.ExportByStart(dt, template, 2, 9);
               var fileName = System.IO.Path.GetFileName(template);
               return File(export, "application/msexcel", fileName);
            }
            catch (Exception ex)
            {
                return Content("导出错误");//，详细信息为："+ex.Message.ToString());
            }
        }
        /// <summary>
        /// 报销单数据列表数据

        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<DJLBCondition>(condition);
            var dls = new DocListSearch(conditionModel,this.CurrentUserInfo.UserGuid);
            var list = dls.GetResult();
            if (list == null || list.Count == 0)
            {
                return Content(null);
            }
            Session["djlbData"] = list;
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        public ContentResult GLHistory() {
            string condition = Request["condition"];
            string guid = Request["guid"];
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<DJLBCondition>(condition);
            var dls = new DocLinkListSearch(conditionModel, this.CurrentUserInfo.UserGuid);
            dls.MainGuid = guid;
            var list = dls.GetResult();
            if (list == null || list.Count == 0)
            {
                return Content(null);
            }
            
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }

        public ContentResult GLDocHistory()
        {
            string condition = Request["condition"];
            string guid = Request["guid"];
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<DJLBCondition>(condition);
            var dls = new DocLinkDocListSearch(conditionModel, this.CurrentUserInfo.UserGuid);
            dls.MainGuid = guid;
            var list = dls.GetResult();
            if (list == null || list.Count == 0)
            {
                return Content(null);
            }

            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        //过滤
        public  ContentResult HistoryFilter()
        {
            string condition = Request["condition"];
            string flag = Request["falg"];
            var conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<DJLBFilterCondition>(condition);
            if (flag == "14") {
                var pId = conditionModel.TreeNodeList[1].treeValue;
                conditionModel.TreeNodeList.RemoveAt(1);
                conditionModel.GUID_ProjectEx = pId;
            }
            var dls = new DocListFilterSearch(conditionModel, this.CurrentUserInfo.UserGuid);
            var list = dls.GetResult();
            if (list == null || list.Count == 0)
            {
                return Content(null); 
            }
            Session["djlbData"] = list;
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }

        /// <summary>
        /// 报表穿透
        /// </summary>
        /// <returns></returns>
        public ViewResult XMZXZCQKReportThrough()
        {
            ViewData["status"] = "4";
            ViewData["scope"] = "djlb";
            var bgcode = Request["bgcode"];
            var bgresource = Request["bgresource"];
            var StartDate = Request["StartDate"];
            var EndDate = Request["EndDate"];
            var PayStatus = Request["PayStatus"];
            var ApproveStatus = Request["ApproveStatus"];
            var CertificateStatus = Request["CertificateStatus"];
            var project = Request["project"];
            string projectvalues = string.Empty;
            string bgcodevalues = string.Empty;
            ViewData["currentDate"] = EndDate; //DateTime.Now.ToShortDateString();
            ViewData["startDate"] = StartDate;// DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            if (!string.IsNullOrEmpty(bgcode))
            {
                var sql = string.Format("select Guid from ss_bgcode where bgcodekey like '{0}%' and guid not in (select distinct PGUID from ss_bgcode where PGUID is not null) order by bgcodekey",
                    bgcode);

                var bgcodes = this.Actor.InfrastructureContext.ExecuteStoreQuery<resultparms>(sql).ToList();
                foreach (resultparms item in bgcodes)
                {
                    bgcodevalues = bgcodevalues + item.Guid.ToString() + ",";
                }
                if (!string.IsNullOrEmpty(bgcodevalues)) bgcodevalues = bgcodevalues.Substring(0, bgcodevalues.Length - 1);
            }
            if (!string.IsNullOrEmpty(project))
            {
                var projectkeys = project.Split(',');
                var projectguids = this.Actor.InfrastructureContext.SS_Project.Where(e => projectkeys.Contains(e.ProjectKey)).Select(e => e.GUID).ToList<Guid>();
                foreach (Guid item in projectguids)
                {
                    projectvalues = projectvalues + item.ToString() + ",";
                }
                if (!string.IsNullOrEmpty(projectvalues)) projectvalues = projectvalues.Substring(0, projectvalues.Length - 1);
            }
            ViewData["through"] = 1;
            ViewData["bgcode"] = bgcodevalues;
            ViewData["bgresource"] = bgresource;
            ViewData["PayStatus"] = PayStatus;
            ViewData["ApproveStatus"] = ApproveStatus;
            ViewData["CertificateStatus"] = CertificateStatus;
            ViewData["project"] = projectvalues;
            ViewData["WithdrawStatus"] = "0";
            ViewData["CheckStatus"] = "0";
            ViewData["BGType"] = "0";
            ViewData["projectmodel"] = "SS_Project";
            ViewData["bgcodemodel"] = "SS_BGCode";
            return View("djlb");
        }
        public ViewResult ZXJDReportThrough()
        {
            ViewData["status"] = "4";
            ViewData["scope"] = "djlb";
            var bgcode = Request["bgcode"];
            var department = Request["department"];
            var bgresource = Request["bgresource"];
            var StartDate = Request["StartDate"];
            var EndDate = Request["EndDate"];
            var PayStatus = Request["PayStatus"];
            var ApproveStatus = Request["ApproveStatus"];
            var CertificateStatus = Request["CertificateStatus"];
            var project = Request["project"];
            string projectvalues = string.Empty;
            string bgcodevalues = string.Empty;
            ViewData["currentDate"] = EndDate; //DateTime.Now.ToShortDateString();
            ViewData["startDate"] = StartDate;// DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            if (!string.IsNullOrEmpty(project))
            {
                projectvalues = project;
            }
            ViewData["through"] = 1;
            ViewData["bgcode"] = bgcodevalues;
            ViewData["bgresource"] = bgresource;
            ViewData["PayStatus"] = PayStatus;
            ViewData["ApproveStatus"] = ApproveStatus;
            ViewData["CertificateStatus"] = CertificateStatus;
            ViewData["project"] = projectvalues;
            ViewData["department"] = department;
            ViewData["WithdrawStatus"] = "0";
            ViewData["CheckStatus"] = "0";
            ViewData["BGType"] = "0";
            ViewData["projectmodel"] = "SS_Project";
            ViewData["bgcodemodel"] = "SS_BGCode";
            return View("djlb");
        }

        /// <summary>
        /// 执行率汇总表
        /// </summary>
        /// <returns></returns>
        public ViewResult ZXLHZBReportThrough()
        {
            ViewData["status"] = "4";
            ViewData["scope"] = "djlb";
            var bgcode = Request["bgcode"];
            var department = Request["department"];
            var departmentex = Request["departmentex"];
            var bgresource = Request["bgresource"];
            var StartDate = Request["StartDate"];
            var FunClass = Request["FunClass"];
            var EndDate = Request["EndDate"];
            var PayStatus = Request["PayStatus"];
            var ApproveStatus = Request["ApproveStatus"];
            var CertificateStatus = Request["CertificateStatus"];
            var project = Request["project"];
            string projectvalues = string.Empty;
            string bgcodevalues = string.Empty;
            ViewData["currentDate"] = EndDate; //DateTime.Now.ToShortDateString();
            ViewData["startDate"] = StartDate;// DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            if (!string.IsNullOrEmpty(project))
            {
                projectvalues = project;
            }
            if (!string.IsNullOrEmpty(bgcode))
            {
                var sql = string.Format("select Guid from ss_bgcode where guid='{0}' or pguid='{0}'",
                    bgcode);

                var bgcodes = this.Actor.InfrastructureContext.ExecuteStoreQuery<resultparms>(sql).ToList();
                foreach (resultparms item in bgcodes)
                {
                    bgcodevalues = bgcodevalues + item.Guid.ToString() + ",";
                }
                if (!string.IsNullOrEmpty(bgcodevalues)) bgcodevalues = bgcodevalues.Substring(0, bgcodevalues.Length - 1);
            }
            ViewData["through"] = 4;
            ViewData["bgcode"] = bgcodevalues;
            ViewData["bgresource"] = bgresource;
            ViewData["PayStatus"] = PayStatus;
            ViewData["ApproveStatus"] = ApproveStatus;
            ViewData["CertificateStatus"] = CertificateStatus;
            ViewData["project"] = projectvalues;
            ViewData["departmentex"] = departmentex;
            ViewData["department"] = department;
            ViewData["WithdrawStatus"] = "0";
            ViewData["CheckStatus"] = "0";
            ViewData["BGType"] = "0";
            ViewData["projectmodel"] = "SS_Project";
            ViewData["bgcodemodel"] = "SS_BGCode";
            ViewData["FunClass"] = FunClass;
            return View("djlb");
        }


        //项目管理费用执行情况表（按科目）
        public ViewResult XMZXZCQKReportThrough1()
        {
            ViewData["status"] = "4";
            ViewData["scope"] = "djlb";
            var bgcode = Request["bgcode"];
            var bgresource = Request["bgresource"];
            var StartDate = Request["StartDate"];
            var EndDate = Request["EndDate"];
            var PayStatus = Request["PayStatus"];
            var ApproveStatus = Request["ApproveStatus"];
            var CertificateStatus = Request["CertificateStatus"];
            var project = Request["project"];
            string projectvalues = string.Empty;
            string bgcodevalues = string.Empty;
            ViewData["currentDate"] = EndDate; //DateTime.Now.ToShortDateString();
            ViewData["startDate"] = StartDate;// DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            if (!string.IsNullOrEmpty(bgcode))
            {
                var sql = string.Format("select Guid from ss_bgcode where bgcodekey in( '{0}') and guid not in (select distinct PGUID from ss_bgcode where PGUID is not null) order by bgcodekey",
                    bgcode.Replace(",","','"));

                var bgcodes = this.Actor.InfrastructureContext.ExecuteStoreQuery<resultparms>(sql).ToList();
                foreach (resultparms item in bgcodes)
                {
                    bgcodevalues = bgcodevalues + item.Guid.ToString() + ",";
                }
                if (!string.IsNullOrEmpty(bgcodevalues)) bgcodevalues = bgcodevalues.Substring(0, bgcodevalues.Length - 1);
            }
            if (!string.IsNullOrEmpty(project))
            {
                var projectkeys = project.Split(',');
                var projectguids = this.Actor.InfrastructureContext.SS_Project.Where(e => projectkeys.Contains(e.ProjectKey)).Select(e => e.GUID).ToList<Guid>();
                foreach (Guid item in projectguids)
                {
                    projectvalues = projectvalues + item.ToString() + ",";
                }
                if (!string.IsNullOrEmpty(projectvalues)) projectvalues = projectvalues.Substring(0, projectvalues.Length - 1);
            }
            ViewData["through"] = 14;
            ViewData["bgcode"] = bgcodevalues;
            ViewData["bgresource"] = bgresource;
            ViewData["PayStatus"] = PayStatus;
            ViewData["ApproveStatus"] = ApproveStatus;
            ViewData["CertificateStatus"] = CertificateStatus;
            ViewData["project"] = projectvalues;
            ViewData["WithdrawStatus"] = "0";
            ViewData["CheckStatus"] = "0";
            ViewData["BGType"] = "0";
            ViewData["projectmodel"] = "SS_Project";
            ViewData["bgcodemodel"] = "SS_BGCode";
            return View("djlb");
        }
    }
    public static class DataTableExt 
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            List<PropertyInfo> pList = new List<PropertyInfo>();
            Type type = typeof(T);
            DataTable dt = new DataTable();
            Array.ForEach<PropertyInfo>(type.GetProperties(), p => { 
                pList.Add(p); 
                dt.Columns.Add(p.Name,typeof(string)); 
            });
            foreach (var item in list)
            {
                DataRow dr = dt.NewRow();
                pList.ForEach(p => dr[p.Name] = p.GetValue(item, null)+"");
                dt.Rows.Add(dr);
            }
            return dt;

        }
    }

    class resultparms
    {
        public Guid Guid { get; set; }
    }
}
