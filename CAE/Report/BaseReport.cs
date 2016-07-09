using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.IO;
using Infrastructure;

namespace CAE.Report
{
   public  class BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用的
        public string tempalte =AppDomain.CurrentDomain.BaseDirectory+System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];
        public string OperatorKey = string.Empty;
        public string SqlFormat {get;set;}
        public virtual void Init(){
        
        }
        public BaseReport() { }
        public BaseReport(string key) 
        {
            DataSource.connStr = System.Configuration.ConfigurationManager.ConnectionStrings["BBConStr"].ConnectionString;
            this.OperatorKey = key;
            this.Init();
        }
        /// <summary>
        /// 获取预算步骤
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetStepKey(string guid)
        {
            string stepId = string.Empty;
            if (string.IsNullOrEmpty(guid) || guid == Guid.Empty.ToString())
            {
                return "'05'";//select BGStepKey from BG_Step where IsStop!=1 默认为执行预算
            }
            else
            {
                stepId = "'"+guid+"'";
            }
            return stepId;
        }
        /// <summary>
        /// 预算来源
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual string GetBGResourceType(string status)
        {
            string sqlCondition = " 1=1 ";
            switch (status)
            {
                case "1":
                    sqlCondition = "GUID_PaymentNumber in (select GUID from cn_paymentNumberView where isnull(BGSourceKey,1)=1) ";//当年预算
                    break;
                case "2":
                    sqlCondition = "GUID_PaymentNumber in (select GUID from cn_paymentNumberView where isnull(BGSourceKey,1)=2) ";//上年结转
                    break;
                default:
                    // sqlCondition = "GUID_PaymentNumber in (select GUID from cn_paymentNumberView where isnull(BGSourceKey,1) in(1,2)) ";//上年结转
                    break;
            }
            return sqlCondition;
        }
        /// <summary>
        /// 审批状态 0表示全部 1表示未审批 2表示已经审批
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual string GetByApproveStatus(string status)
        {
            string sqlCondition = "1=1";
            switch (status)
            {
                case "1":
                    sqlCondition = "isnull(Docstate,0)<>0 and isnull(Docstate,0)<>999 and isnull(Docstate,0)<>-1 ";
                    break;
                case "2":
                    sqlCondition = "isnull(Docstate,0)<>0 and (isnull(Docstate,0)=999 or isnull(Docstate,0)=-1) ";
                    break;
                case "3":
                    sqlCondition = " isnull(Docstate,0)<>999 and isnull(Docstate,0)<>-1 and  DocState != '' and  DocState != '0' ";
                    break;
                default:
                   
                    break;
            }
            return sqlCondition;
        }
        /// <summary>
        /// 付款状态


        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual string GetPayStatus(string status)
        {
            string sqlCondition = " 1=1 "; //全部
            switch (status)
            {
                case "1":
                    sqlCondition = " GUID_BX_Main in (select GUID from bx_mainView where   guid not in (select guid_main from hx_detail)) ";
                    break;
                case "2":
                    sqlCondition = " GUID_BX_Main in (select GUID from bx_mainView where   guid  in (select guid_main from hx_detail))";
                    break;
                default:
                    break;
            }
            return sqlCondition;
        }
        /// <summary>
        /// 核销状态


        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual string GetHXStatus(string status)
        {
            string sqlCondition = " 1=1 "; //全部
            switch (status)
            {
                case "1":
                    sqlCondition = "guid in (select guid_main from hx_detail)  ";
                    break;
                case "2":
                    sqlCondition = "guid not in (select guid_main from hx_detail) ";
                    break;
                default:
                    break;
            }
            return sqlCondition;
        }
        /// <summary>
        /// 凭证状态


        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public virtual string GetCertificateStatus(string status)
        {
            string sqlCondition = " 1=1 "; //全部
            if (status == "1")
            {
                sqlCondition = "guid not in(select guid_main from hx_detail where guid_hx_main in(select guid_hxmain from cw_pzmain)) ";
            }
            else if (status == "2")
            {
                sqlCondition = "guid  in(select guid_main from hx_detail where guid_hx_main in(select guid_hxmain from cw_pzmain))";
            }
            return sqlCondition;
        }
        /// <summary>
        /// 报表导出
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="fileName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual string GetExportPath(DataTable data, out string fileName, out string message)
        {
            fileName = "";
            message = "";
            try
            {
                if (data != null && data.Rows.Count <= 0)
                {
                    message = "1";
                    return "";
                }
                string filePath = ExportExcel.Export(data, this.tempalte, 2);
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }
        /// <summary>
        /// 获取部门ID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public virtual string GetByDepartmentId(string guid)
        {
            string depId = string.Empty;
            if (string.IsNullOrEmpty(guid) || guid == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetDepartmentGUID(true, this.OperatorKey);
                depId = depList.GetStrGUIDS();
            }
            else
            {
                depId = "'" + guid + "'";
            }
            return depId;
        }
        /// <summary>
        /// 获取部门Key
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public virtual string GetByDepartmentKey(string key)
        {
            string depKey = string.Empty;
            if (string.IsNullOrEmpty(key) || key == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetDepartmentKey(true, this.OperatorKey);
                
                depKey = depList.GetStrGUIDS();
            }
            else
            {
                depKey = "'" + key + "'";
            }
            return depKey;
        }
        /// <summary>
        /// 获取单位Key
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public virtual string GetByDWKey(string key)
        {
            string dwKey = string.Empty;
            if (string.IsNullOrEmpty(key) || key == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetDWKeyList(true, this.OperatorKey);
                dwKey = depList.GetStrGUIDS();
            }
            else
            {
                dwKey = "'" + key + "'";
            }
            return dwKey;
        }
        /// <summary>
        /// 获取单位GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public virtual string GetByDWGUID(string key)
        {
            string dwKey = string.Empty;
            if (string.IsNullOrEmpty(key) || key == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetDWGUIDList(true, this.OperatorKey);
                dwKey = depList.GetStrGUIDS();
            }
            else
            {
                dwKey = "'" + key + "'";
            }
            return dwKey;
        }

        /// <summary>
        /// 获取项目ID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetByProjectId(string guid, int type,string strYear)
        {
            IntrastructureFun db = new IntrastructureFun();
            string proId = string.Empty;
            int year = 0;
            int.TryParse(strYear, out year);
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            if (string.IsNullOrEmpty(guid) || guid == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetProjectGUID(true, this.OperatorKey,year);
                proId = depList.GetStrGUIDS();
            }
            else
            {
                //proId = "'" + guid + "'";
                List<SS_Project> projectList = new List<SS_Project>();
                Guid g;
                Guid.TryParse(guid, out g);
                if (type == 2)//项目分类
                {
                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
                    projectclassModel.GUID = g;
                    var projectClassList = db.GetProjectClass(true, this.OperatorKey,year);
                    var proList = db.GetProject(true,this.OperatorKey,year);
                    projectclassModel.RetrieveLeafs(projectClassList, proList, ref projectList);
                    var projectGUID = projectList.Select(e => e.GUID).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
                else
                {
                    SS_Project projectModel = new SS_Project();
                    projectModel.GUID = g;
                    var orgList = db.GetProject(true, this.OperatorKey,year);
                    projectModel.RetrieveLeafs(orgList, ref projectList);
                    var projectGUID = projectList.Select(e => e.GUID).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
            }
            return proId;
        }
        /// <summary>
        /// 获取项目Key
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetByProjectKey(string guid, int type,string strYear)
        {
            IntrastructureFun db = new IntrastructureFun();
            int year = 0;
            int.TryParse(strYear, out year);
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            string proId = string.Empty;
            if (string.IsNullOrEmpty(guid) || guid == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetProjectKey(true, this.OperatorKey,year);
                proId = depList.GetStrGUIDS();
            }
            else
            {
                //proId = "'" + guid + "'";
                List<SS_Project> projectList = new List<SS_Project>();
                Guid g;
                Guid.TryParse(guid, out g);
                if (type == 2)//项目分类
                {
                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
                    projectclassModel.GUID = g;
                    var projectClassList = db.GetProjectClass(true, this.OperatorKey,year);
                    var proList = db.GetProject(true, this.OperatorKey);
                    projectclassModel.RetrieveLeafs(projectClassList, proList, ref projectList);
                    var projectGUID = projectList.Select(e => e.ProjectKey).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
                else
                {
                    SS_Project projectModel = new SS_Project();
                    projectModel.GUID = g;
                    var orgList = db.GetProject(true, this.OperatorKey,year);
                    projectModel.RetrieveLeafs(orgList, ref projectList);
                    var projectGUID = projectList.Select(e => e.ProjectKey).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
            }
            return proId;
        }

        /// <summary>
        /// 获取项目Key
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public string GetByProjectKey1(string guid, int type, string strYear)
        {
            IntrastructureFun db = new IntrastructureFun();
            int year = 0;
            int.TryParse(strYear, out year);
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }
            string proId = string.Empty;
            if (string.IsNullOrEmpty(guid) || guid == Guid.Empty.ToString())
            {
                IntrastructureFun dbobj = new IntrastructureFun();
                var depList = dbobj.GetProjectKey(false, this.OperatorKey, year);
                proId = depList.GetStrGUIDS();
            }
            else
            {
                //proId = "'" + guid + "'";
                List<SS_Project> projectList = new List<SS_Project>();
                Guid g;
                Guid.TryParse(guid, out g);
                if (type == 2)//项目分类
                {
                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
                    projectclassModel.GUID = g;
                    var projectClassList = db.GetProjectClass(true, this.OperatorKey, year);
                    var proList = db.GetProject(false, this.OperatorKey);
                    projectclassModel.RetrieveLeafs(projectClassList, proList, ref projectList);
                    var projectGUID = projectList.Select(e => e.ProjectKey).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
                else
                {
                    SS_Project projectModel = new SS_Project();
                    projectModel.GUID = g;
                    var orgList = db.GetProject(false, this.OperatorKey, year);
                    projectModel.RetrieveLeafs(orgList, ref projectList);
                    var projectGUID = projectList.Select(e => e.ProjectKey).ToList();
                    proId = projectGUID.GetStrGUIDS();
                }
            }
            return proId;
        }
        /// <summary>
        /// 转化成货币表达式
        /// </summary>
        /// <param name="ftype">类型：0表示带￥的货币表达式 1表示不带￥的表达式，默认不带￥</param>
        /// <param name="fmoney">要转化的值</param>
        /// <returns>String</returns>
        public string FormatMoney(int ftype, double fmoney)
        {
            string _fmoney = string.Empty;
            if (fmoney == 0F)
            {
                return "";
            }
            fmoney = double.Parse(Convert.ToDouble(fmoney).ToString("0.00"));
            //进行四舍五入并保留2位小数
            fmoney = Math.Round(fmoney, 2, MidpointRounding.AwayFromZero);
            switch (ftype)
            {
                case 0:
                    _fmoney = string.Format("{0:C2}", fmoney);
                    break;
                case 1:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
                default:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
            }
            return _fmoney;
        }

        public object ConventDouble(string s)
        {
            double g;
            if (double.TryParse(s, out g)) return "'" + g + "'";
            return "null";
        }
        public double ConventDoubleRStr(string s)
        {
            double g;
            if (double.TryParse(s, out g)) return g;
            return 0;
        }

    }
}
