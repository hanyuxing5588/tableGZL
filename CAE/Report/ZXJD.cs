using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Business.Common;
using Business.CommonModule;
using BusinessModel;
using Infrastructure;
using System.IO;

namespace CAE.Report
{
    public class ZXJD:BaseReport
    {
        IntrastructureFun db = new IntrastructureFun();
        public ZXJD(string key)
            : base(key)
        {
          
        }
        public override void Init()
        {
            //{0}DepartmentKey 部门 {1} {1}ProjectKey 项目Key  {2}Year 年 {3}StartMonth 月的开始时间 {4} EndMonth 结束月时间 {5}预算步骤 {6}预算来源 用来帅选报销detail {7}审批状态 {8}核销状态 {9}记账状态
            // {10} 查询总经费 {11} 查询预算下达  
            this.SqlFormat = "Select Project.GUID,Project.ProjectName,Project.ProjectKey,Project.PGUID,isnull(BG.PersonName,'') as PersonName," +
              "isnull(BG.iBGTotalSum,0) as iBGTotalSum,isnull(BG.iBGTotalXD,0) as iBGTotalXD,isnull(BX.iBXTotal,0) as iBXTotal " +
              ",isnull(total_bgglf,0) as itotal_bgglf,isnull(total_bxglf,0) as itotal_bxglf ,isnull(functionplan.total_plan,0)  as total_plan ,BG.GUID as bgguid " +
              "From    (SELECT * FROM  SS_Project WHERE GUID NOT IN (SELECT PGUID FROM SS_Project where pguid is NOT null) ) Project Left Join (" +
              "Select BGMain.guid,BGMain.GUID_Project,BGMain.ProjectName,BGMain.ProjectKey,BGMain.PersonName,BGTotal.iBGTotalSum,BGXD.iBGTotalXD From BG_MainView BGMain Left Join " +
              "(Select GUID_BG_Main,Sum(Total_BG) As iBGTotalSum From BG_DetailView Where len(bgcodekey)=2 and {10} And BGYear ='{2}'  Group By GUID_BG_Main) BGTotal " +
              "On BGMain.GUID=BGTotal.GUID_BG_Main Left Join " +
              "(Select GUID_BG_Main,Sum(Total_BG) As iBGTotalXD From BG_DetailView Where len(bgcodekey)=2 and {11} And BGYear ='{2}'  Group By GUID_BG_Main) BGXD " +
              "On BGMain.GUID=BGXD.GUID_BG_Main Where isnull(BGMain.Invalid,0)=1 and BGMain.DepartMentKey in(DepartMentKey) And BGMain.ProjectKey in({1}) And  BGStepKey in({5}) and guid in " +
              "(select guid_bg_main from bg_detailview where len(bgcodekey)=2 and bgyear='{2}')) BG On Project.GUID=BG.GUID_Project Left Join (" +
              "Select GUID_Project,Sum(Total_BX) As iBXTotal  From BX_DetailView Where DepartmentKey In(DepartMentKey) And ProjectKey In({1}) " +
              //"And GUID_PaymentNumber In(Select GUID from CN_PaymentNumberView Where isnull(BGSourceKey,1) In('1','2')) " +
              "And {6} " +
              "And GUID_BX_Main In(Select GUID From BX_MainView Where {7} and {8} and {9} and Year(DocDate)='{2}' And Month(DocDate)>='{3}'  And Month(DocDate)<='{4}') " +
              "and GUID_BX_Main not in(select guid from bx_main where  docState ='9') " +
              "Group By GUID_Project) BX On Project.GUID=BX.GUID_Project "
               +
              "left join ((select a.guid_functionclass,a.functionclassname,bg as total_bgglf,bx as total_bxglf from " +
              "(select main.guid_functionclass,main.functionclassname,sum(detail.total_bg) as bg from bg_mainview main left join bg_detailview detail on " +
              "main.guid=detail.guid_bg_main " +
              "where main.guid in(select guid_bg_main from bg_detail where bgyear='{2}' ) and isnull(main.Invalid,0)=1 and detail.bgyear='{2}' and len(detail.bgcodekey)=2 and detail.bgitemkey in('03','04') and main.BGStepKey={5} " +
              "group by guid_functionclass,main.functionclassname) a " +
              "left join " +
              "(select pay.guid_functionclass,pay.functionclassname,sum(detail.total_bx ) as bx " +
              "from bx_detailview detail " +
              "left join cn_paymentnumberview pay on detail.guid_paymentnumber=pay.guid " +
              "where detail.guid_bx_main in(select guid from bx_main where {7} and {8} and {9} and Year(DocDate)='{2}' And Month(DocDate)>='{3}' " +
              "And Month(DocDate)<='{4}') " +
              "and pay.isguoku=1 and isnull(pay.projectkey,'')='' " +
              "group by pay.guid_functionclass,pay.functionclassname) b on a.guid_functionclass=b.guid_functionclass)) glf " +
              "on project.guid_functionclass=glf.guid_functionclass "

             +
            " left join (select guid_functionclass,sum(total_plan) as total_plan from CW_AppropriationPlan where  planyear='{2}' " +
            " And planmonth>='{3}' And planmonth<='{4}' group by guid_dw,guid_functionclass) functionplan " +
            " on project.guid_functionclass=functionplan.guid_functionclass "

              + "Where Isnull(Project.IsStop,0)=0 and Isnull(Project.StopYear,0)<3000 And Project.ProjectKey In({1})"

            + " and " +
            "(project.guid in(select distinct guid_project from bg_mainview " +
            "where isnull(Invalid,0)=1 and guid in(select distinct guid_bg_main from bg_detail where BGYear='{2}') " +
            "and DepartMentKey in({0}) " +
            "And  BGStepKey in({5})) " +
            "or project.guid in(select distinct pguid from ss_project where guid in " +
            "(select guid_project from bg_mainview " +
            "where isnull(Invalid,0)=1 and guid in(select distinct guid_bg_main from bg_detail where BGYear='{2}') " +
            "and DepartMentKey in({0}) And  BGStepKey in({5})  ) ))";
            this.tempalte = Path.Combine(this.tempalte, "zxjd.xls");
        }
        public DataTable GetReport(SearchCondition conditions, out string msgError)
        {
            msgError = "";
            string sqlStr = string.Empty;
            BBSearchCondition conditionModel = (BBSearchCondition)conditions;            
            sqlStr = GetSql(conditionModel);
            DataTable orgdt = LoadData(sqlStr, ref msgError);
            if (null==orgdt)
            {
                msgError = "没有可以显示的数据!"; 
                return null;
            }
            DataTable newdt = CreateNewDataTable();
            ReSetData(orgdt, ref newdt, conditionModel.RMBUnit);
            return newdt;
        }
        public DataTable GetReportEx(SearchCondition conditions, out string msgError)
        {
            
            msgError = "";
            string sqlStr = string.Empty;
            BBSearchCondition conditionModel = (BBSearchCondition)conditions;
            sqlStr = GetSql(conditionModel);
            BusinessEdmxEntities bcontext=new BusinessEdmxEntities();
            var results = bcontext.ExecuteStoreQuery<ZXJDModel>(sqlStr).ToList();
            if (null == results || results.Count==0)
            {
                msgError = "没有可以显示的数据!";
                return null;
            }

            int curYear=int.Parse(conditionModel.Year);
            //搭建项目树
            Infrastructure.BaseConfigEdmxEntities incontext = new BaseConfigEdmxEntities();
            var ProjectViews = incontext.SS_ProjectView.Where(e => e.IsStop != true && (e.BeginYear == null || e.BeginYear <= curYear) && (e.StopYear == null || e.StopYear >= curYear)).OrderBy(e=>e.ProjectKey).ToList();
            var ProjectClassViews = incontext.SS_ProjectClassView.Where(e => e.IsStop != true && (e.BeginYear == null || e.BeginYear <= curYear) && (e.StopYear == null || e.StopYear >= curYear)).OrderBy(e=>e.ProjectClassKey).ToList();
            ReportTree mTree = new ReportTree();
            var tops = ProjectClassViews.FindAll(e => e.PGUID == null);
            foreach (var item in tops) BuildTree(item, null, ProjectClassViews, ProjectViews, mTree);

            //过滤掉本次不需要的树节点
            List<Guid> Keys = new List<Guid>();
            foreach (var item in results)
            {
                var currentNode = mTree.FindNode(item.GUID);
                if (currentNode != null)
                {
                    currentNode.Value.CopySpecialValue(item);
                    RetrieveRelativeKeys(currentNode, Keys);
                }
            }
            mTree.Nodes.RemoveAll(e => !Keys.Contains(e.Key));
            if (mTree.Nodes.Count == 0)
            {
                msgError = "没有可以显示的数据!";
                return null;
            }
            //向上汇总金额
            ZXJDModel HJ = new ZXJDModel();
            HJ.ProjectName = "合计";
            HJ.ProjectKey = string.Empty;
            var Roots = mTree.Roots;
            foreach (var root in Roots)
            {
                Sum(root);
                HJ.iBGTotalSum += root.Value.iBGTotalSum;
                HJ.iBGTotalXD += root.Value.iBGTotalXD;
                HJ.iBXTotal += root.Value.iBXTotal;
                HJ.itotal_bgglf += root.Value.itotal_bgglf;
                HJ.itotal_bxglf += root.Value.itotal_bxglf;
                HJ.total_plan += root.Value.total_plan;
            }
            
            //生成Datatable
            int iUnit = 1;
            iUnit = Int32.Parse(conditionModel.RMBUnit);
            
            DataTable newdt = CreateNewDataTable();
            foreach (var root in Roots)
            {
                CreateDataEx(root, ref newdt, iUnit,0);
            }
            DataRow newRow = newdt.NewRow();
            HJ.FillData(ref newRow, iUnit);
            newdt.Rows.Add(newRow);

            return newdt;
        }
        private void BuildTree(SS_ProjectClassView Current,Guid? ParentKey, List<SS_ProjectClassView> ProjectClassViews,List<SS_ProjectView> ProjectViews, ReportTree Tree)
        {
            Tree.AddNode(ZXJDModel.ConvertTo(Current),ParentKey);
            var children = ProjectClassViews.FindAll(e => e.PGUID == Current.GUID);
            if (children != null && children.Count > 0)
            {
                foreach (SS_ProjectClassView item in children)
                {
                    BuildTree(item, Current.GUID, ProjectClassViews,ProjectViews, Tree);
                }
            }
            var projects = ProjectViews.FindAll(e => e.GUID_ProjectClass == Current.GUID && e.PGUID==null);

            if (projects != null && projects.Count > 0)
            {
                foreach (SS_ProjectView item in projects)
                {
                    BuildTree(item, Current.GUID, ProjectViews, Tree);
                }
            }
        }
        private void BuildTree(SS_ProjectView Current, Guid? ParentKey, List<SS_ProjectView> ProjectViews, ReportTree Tree)
        {
            Tree.AddNode(ZXJDModel.ConvertTo(Current), ParentKey);
            var children = ProjectViews.FindAll(e => e.PGUID == Current.GUID);
            foreach (SS_ProjectView item in children)
            {
                BuildTree(item, Current.GUID, ProjectViews, Tree);
            }
        }
        private void RetrieveRelativeKeys(ReportTreeNode Node,List<Guid> Keys)
        {
            if (Node == null) return;
            Keys.Add(Node.Key);
            RetrieveRelativeKeys(Node.Parent, Keys);
        }
        private void Sum(ReportTreeNode Node)
        {
            var children=Node.Children;
            if (children.Count == 0) return;
            foreach (var item in children)
            {
                Sum(item);
                Node.Value.iBGTotalSum += item.Value.iBGTotalSum;
                Node.Value.iBGTotalXD += item.Value.iBGTotalXD;
                Node.Value.iBXTotal += item.Value.iBXTotal;
                Node.Value.itotal_bgglf += item.Value.itotal_bgglf;
                Node.Value.itotal_bxglf += item.Value.itotal_bxglf;
                Node.Value.total_plan += item.Value.total_plan;
            }
            
        }
        private void CreateDataEx(ReportTreeNode Node, ref DataTable newdt,int iUnit,int Level)
        {
            
            DataRow newRow = newdt.NewRow();
            Node.Value.ProjectName = PadLeft(Node.Value.ProjectName, Level);
            Node.Value.ProjectKey = PadLeft(Node.Value.ProjectKey, Level);
            Node.Value.FillData(ref newRow, iUnit);
            newdt.Rows.Add(newRow);
            Level = Level + 1;
            foreach (var item in Node.Children) CreateDataEx(item, ref newdt, iUnit, Level);
        }

        private string PadLeft(string s,int level, string paddingString = "&nbsp")
        {
            for (int i = 0; i < level; i++)
            {
                s = paddingString + s;
            }
            return s;
        }
        private void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit)
        {
            if (orgdt.Rows.Count > 0)
            {
                DataRow[] pdr = orgdt.Select();
                foreach (DataRow row in pdr)
                {
                    DataRow newRow = newdt.NewRow();
                    ReSetChildData(row, ref newRow, rmbUnit);
                    newdt.Rows.Add(newRow);
                }
                DataRow TotalRow = newdt.NewRow();
                DataRow[] rows = newdt.Select();
                double dblBGTotalSum = 0F;
                double dblBGTotalXD = 0F;
                double dblBXTotalExecute = 0F;
                double dblManageriaExecute = 0F;
                double dblExecuteNum = 0F;
                double dblDirectCostBalance = 0F;
                double dblManageriaCostBalance = 0F;
                double dblBGTotalBalance = 0F;
                double dblDirectCostScale = 0F;
                double dblManageriaCostScale = 0F;
                double dblDoneScale = 0F;
                foreach (DataRow row in rows)
                {
                    if ((string)row["iBGTotalSum"]!="")
                    {
                        dblBGTotalSum += Double.Parse((string)row["iBGTotalSum"]);
                    }
                    if (row["iBGTotalXD"] != "")
                    {
                        dblBGTotalXD += Double.Parse((string)row["iBGTotalXD"]);
                    }
                    if (row["iBXTotalExecute"]!="")
                    {
                        dblBXTotalExecute += Double.Parse((string)row["iBXTotalExecute"]);
                    }
                    if (row["iManageriaExecute"] != "")
                    {
                        dblManageriaExecute += Double.Parse((string)row["iManageriaExecute"]);
                    }

                    if (row["ExecuteNum"] != "")
                    {
                        dblExecuteNum += Double.Parse((string)row["ExecuteNum"]);
                    }
                    if (row["DirectCostBalance"] != "")
                    {
                        dblDirectCostBalance += Double.Parse((string)row["DirectCostBalance"]);
                    }
                    if (row["ManageriaCostBalance"] != "")
                    {
                        dblManageriaCostBalance += Double.Parse((string)row["ManageriaCostBalance"]);
                    }
                    if (row["BGTotalBalance"] != "")
                    {
                        dblBGTotalBalance += Double.Parse((string)row["BGTotalBalance"]);
                    }

                    if (row["DirectCostScale"] != "")
                    {
                        string str = (string)row["DirectCostScale"];
                        str = str.Replace("%", "");
                        dblDirectCostScale += Double.Parse(str);
                    }
                }
                TotalRow["ProjectName"] = "合计";
                TotalRow["iBGTotalSum"] = FormatMoney(1, dblBGTotalSum);
                TotalRow["iBGTotalXD"] = FormatMoney(1, dblBGTotalXD);
                TotalRow["iBXTotalExecute"] = FormatMoney(1, dblBXTotalExecute);
                TotalRow["iManageriaExecute"] = FormatMoney(1, dblManageriaExecute);
                TotalRow["ExecuteNum"] = FormatMoney(1, dblExecuteNum);
                TotalRow["DirectCostBalance"] = FormatMoney(1, dblDirectCostBalance);
                TotalRow["ManageriaCostBalance"] = FormatMoney(1, dblManageriaCostBalance);
                TotalRow["BGTotalBalance"] = FormatMoney(1, dblBGTotalBalance);
                // 直接成本完成比例 = 直接成本执行/预算下达
                if (dblBGTotalXD == 0 || dblBXTotalExecute == 0)
                {
                    TotalRow["DirectCostScale"] = "";
                }
                else
                {
                    TotalRow["DirectCostScale"] = FormatMoney(1, (dblBXTotalExecute / dblBGTotalXD) * 100) + "%";
                }
                // 管理费完成比例 = 管理费执行/总经费-预算下达
                if (dblBGTotalSum - dblBGTotalXD == 0 || dblManageriaExecute == 0)
                {
                    TotalRow["ManageriaCostScale"] = "";
                }
                else
                {
                    TotalRow["ManageriaCostScale"] = FormatMoney(1, (dblManageriaExecute / (dblBGTotalSum - dblBGTotalXD)) * 100) + "%";
                }
                //完成比例 = （直接成本执行 + 管理费执行）/总经费  执行数/总经费
                if (dblBGTotalSum == 0 || dblManageriaExecute + dblBXTotalExecute == 0)
                {
                    TotalRow["DoneScale"] = "";
                }
                else
                {
                    TotalRow["DoneScale"] = FormatMoney(1, ((dblManageriaExecute + dblBXTotalExecute) / dblBGTotalSum) * 100) + "%";
                }
                
                newdt.Rows.Add(TotalRow);
            }
        }
        private  void ReSetChildData(DataRow oldRow, ref DataRow currentRow, string rmbUnit)
        {
            int iUnit = 1;
            iUnit = Int32.Parse(rmbUnit);
            currentRow["ProjectName"] = oldRow["ProjectName"];
            currentRow["ProjectKey"] = oldRow["ProjectKey"];
            currentRow["PersonName"] = oldRow["PersonName"];
            currentRow["iBGTotalSum"] = oldRow["iBGTotalSum"];
            currentRow["iBGTotalXD"] = oldRow["iBGTotalXD"];
            currentRow["iBXTotalExecute"] = oldRow["iBXTotal"];

            double dblBGTotalSum = 0F;   // 总经费
            double dbliBGTotalXD = 0F;   // 预算下达 
            double dblBXTotalExecute = 0F; // 直接成本执行

            double dblTotal_bgglf = 0F;  
            double dblTtotal_bxglf = 0F;
            double dbltotal_plan = 0F;
            double s = 0F;
            double.TryParse("",out s);
            double.TryParse(oldRow["iBGTotalSum"].ToString(), out dblBGTotalSum);
            double.TryParse(oldRow["iBGTotalXD"].ToString(), out dbliBGTotalXD);
            double.TryParse(oldRow["iBXTotal"].ToString(), out dblBXTotalExecute);
            double.TryParse(oldRow["itotal_bgglf"].ToString(), out dblTotal_bgglf);
            double.TryParse(oldRow["itotal_bxglf"].ToString(), out dblTtotal_bxglf);
            double.TryParse(oldRow["total_plan"].ToString(), out dbltotal_plan);
            // 管理费执行
            double dblManageriaExecute = dblTotal_bgglf==0?0F:(dblBGTotalSum - dbliBGTotalXD) * (dblTtotal_bxglf + dbltotal_plan) / dblTotal_bgglf;

            currentRow["iManageriaExecute"] = FormatMoney(1, dblManageriaExecute / iUnit);
            currentRow["iBGTotalSum"] = FormatMoney(1, dblBGTotalSum/iUnit);
            currentRow["iBGTotalXD"] = FormatMoney(1, dbliBGTotalXD /iUnit);
            currentRow["iBXTotalExecute"] = FormatMoney(1, dblBXTotalExecute / iUnit);

            // 执行数 = 直接成本执行 + 管理费执行
            currentRow["ExecuteNum"] = FormatMoney(1, (dblManageriaExecute + dblBXTotalExecute) / iUnit);
            // 直接成本结余 = 预算下达 - 直接成本执行
            currentRow["DirectCostBalance"] = FormatMoney(1, (dbliBGTotalXD - dblBXTotalExecute) / iUnit);
            // 管理费结余 = 总经费 - 预算下达 - 管理费执行
            currentRow["ManageriaCostBalance"] = FormatMoney(1, (dblBGTotalSum - dbliBGTotalXD - dblManageriaExecute) / iUnit);
            // 预算结余 = 直接成本结余 + 管理费结余;
            currentRow["BGTotalBalance"] = FormatMoney(1, (dblBXTotalExecute + dblBGTotalSum - dblManageriaExecute) / iUnit);
            // 直接成本完成比例 = 直接成本执行/预算下达
            if (dbliBGTotalXD == 0 || dblBXTotalExecute ==0)
            {
                currentRow["DirectCostScale"] = "";
            }
            else
            {
                currentRow["DirectCostScale"] = FormatMoney(1, (dblBXTotalExecute / dbliBGTotalXD) * 100) + "%";
            } 
            // 管理费完成比例 = 管理费执行/总经费-预算下达
            if (dblBGTotalSum - dbliBGTotalXD == 0 || dblManageriaExecute==0)
            {
                currentRow["ManageriaCostScale"] = "";
            }
            else
            {
                currentRow["ManageriaCostScale"] = FormatMoney(1, (dblManageriaExecute /(dblBGTotalSum - dbliBGTotalXD)) * 100) + "%";
            }
            //完成比例 = （直接成本执行 + 管理费执行）/总经费  执行数/总经费
            if (dblBGTotalSum == 0 || dblManageriaExecute + dblBXTotalExecute==0)
            {
                currentRow["DoneScale"] = "";
            }
            else
            {
                currentRow["DoneScale"] = FormatMoney(1, ((dblManageriaExecute + dblBXTotalExecute) / dblBGTotalSum) * 100) + "%";
            }

            
        }

        public string GetExportPath(DataTable data, out string fileName, out string message, ReportHeadModel reportHeadModel)
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
                string filePath = ExportExcel.Export(data, this.tempalte, 4, 0, new List<ExcelCell>() { new ExcelCell(1, 2, reportHeadModel.DepartmentName), new ExcelCell(0, 1, reportHeadModel.Year + "年度"), new ExcelCell(13, 2, reportHeadModel.RMBUnit) });
                fileName = Path.GetFileName(filePath);
                return filePath;

            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }
        private DataTable CreateNewDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ProjectName");//项目名称 
            dt.Columns.Add("ProjectKey");//项目Key         
            dt.Columns.Add("PersonName");//人员名称         
            dt.Columns.Add("iBGTotalSum");//预算总经费

            dt.Columns.Add("iBGTotalXD");//预算下达
            dt.Columns.Add("iBXTotalExecute");//直接成本执行
            dt.Columns.Add("iManageriaExecute");//管理费执行
            dt.Columns.Add("ExecuteNum");//执行数
            dt.Columns.Add("DirectCostBalance");//直接成本结余
            dt.Columns.Add("ManageriaCostBalance");//管理费结余
            dt.Columns.Add("BGTotalBalance");//预算结余
            dt.Columns.Add("DirectCostScale");//直接成本完成比例
            dt.Columns.Add("ManageriaCostScale");//管理费完成比例
            dt.Columns.Add("DoneScale");//完成比例
            return dt;
        }
        private DataTable LoadData(string sql, ref string msgerror)
        {
            var dt = DataSource.ExecuteQuery(sql);
            if (dt == null || dt.Rows.Count == 0)
            {
                msgerror = "服务器连接失败。";
                return null;
            }
            return dt;
        }
        // 总经费
        private  string GetBGResourceType1(string status)
        {
            string sqlCondition = " 1=1 ";
            switch (status)
            {
                case "1":
                    sqlCondition = " BGItemKey in ('21','24','08','04') ";//当年预算
                    break;
                case "2":
                    sqlCondition = " BGItemKey in ('20','23','07','03') ";//上年结转
                    break;
                default:
                     sqlCondition = " BGItemKey in ('20','23','21','24','07','08','03','04') ";//全部
                    break;
            }
            return sqlCondition;
        }
        private string GetBGResourceType2(string status)
        {
            string sqlCondition = " 1=1 ";
            switch (status)
            {
                case "1":
                    sqlCondition = " BGItemKey in ('08') ";//当年预算
                    break;
                case "2":
                    sqlCondition = " BGItemKey in ('07') ";//上年结转
                    break;
                default:
                    sqlCondition = " BGItemKey in ('07','08') ";//全部
                    break;
            }
            return sqlCondition;
        }

        private string GetLink(string str)
        {
            string strLink = "";
            string[] array = str.Split(',');
            foreach(string item in array)
            {
                string strKey  = "'" + item + "'";
                if (strLink=="")
                {
                    strLink = strKey;
                }
                else
                {
                    strLink = strLink + "," + strKey;
                }
            }
            return strLink;
        }
        public string GetSql(BBSearchCondition conditionModel)
        {

            string strsql = this.SqlFormat;
            string proKey = string.Empty;
            string bgSetpId = string.Empty;
            string dwIdOrDepartId = string.Empty;
            string startMonth = "1";
            string endMonth = "12";

            if (conditionModel.Year == "0")
            {
                conditionModel.Year = "";
            }
            //开始月
            if (!string.IsNullOrEmpty(conditionModel.StartMonth))
            {
                startMonth = conditionModel.StartMonth;
            }
            //结束月

            if (!string.IsNullOrEmpty(conditionModel.EndMonth))
            {
                endMonth = conditionModel.EndMonth;
            }
            //开始时间


            DateTime StartDate = DateTime.MinValue;
            ///// 结束时间
            DateTime EndDate = DateTime.MaxValue;

            #region 多个树结构 TreeNodeList中每个treeModel对应一个treeValue
            if (conditionModel.TreeNodeList != null && conditionModel.TreeNodeList.Count > 0)
            {
                List<TreeNode> treeNodeList = conditionModel.TreeNodeList;
                foreach (TreeNode item in treeNodeList)
                {
                    switch (item.treeModel.ToLower())
                    {
                        case "ss_department":
                            if (item.treeValue == "" || item.treeValue == null)
                            {
                                dwIdOrDepartId = GetByDepartmentKey(Guid.Empty.ToString());
                            }
                            else
                            {
                                dwIdOrDepartId = GetLink(item.treeValue);
                            }
                            
                            break;
                        case "ss_project":
                            if (item.treeValue == "" || item.treeValue == null)
                            {
                                proKey = GetByProjectKey(Guid.Empty.ToString(), 1, conditionModel.Year);
                            }
                            else
                            {
                                proKey = GetLink(item.treeValue);
                            }
                            
                            break;
                        case "bg_setup":
                            if (item.treeValue == "" || item.treeValue == null)
                            {
                                bgSetpId = GetStepKey(Guid.Empty.ToString());
                            }
                            else
                            {
                                bgSetpId = GetStepKey(item.treeValue);
                            }                           
                            break;
                    }
                }
            }

            #endregion
            if (string.IsNullOrEmpty(dwIdOrDepartId))
            {
                dwIdOrDepartId = GetByDepartmentKey(Guid.Empty.ToString());
            }
            if (string.IsNullOrEmpty(proKey))
            {
                proKey = GetByProjectKey(Guid.Empty.ToString(), 1, conditionModel.Year);
            }
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString());
            }
            //预算来源
            conditionModel.BGResourceType = GetBGResourceType(conditionModel.BGResourceType);
            //审批状态
            conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
            //核销状态
            conditionModel.HXStatus = GetHXStatus(conditionModel.HXStatus);
            //凭证状态（即记账状态）
            conditionModel.CertificateStatus = GetCertificateStatus(conditionModel.CertificateStatus);
            string strBGAll = GetBGResourceType1(conditionModel.BGResourceType);
            string strBGXiaDa = GetBGResourceType2(conditionModel.BGResourceType);
            //{0}DepartmentKey 部门 {1} {1}ProjectKey 项目Key  {2}Year 年 {3}StartMonth 月的开始时间 {4} EndMonth 结束月时间 {5}预算步骤 {6}预算来源 用来帅选报销detail {7}审批状态 {8}核销状态 {9}记账状态
            // {10} 查询总经费 {11} 查询预算下达 
            return strsql = string.Format(strsql,
                                          dwIdOrDepartId,
                                           proKey,
                                          conditionModel.Year,
                                          startMonth,
                                          endMonth,
                                          bgSetpId,
                                          conditionModel.BGResourceType,
                                          conditionModel.ApproveStatus,
                                          conditionModel.HXStatus,
                                          conditionModel.CertificateStatus,
                                          strBGAll,
                                          strBGXiaDa
                                          );
        }
    }

    class ZXJDModel
    {
        public Guid GUID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectKey { get; set; }
        public Guid? PGUID { get; set; }
        public string PersonName { get; set; }
        public double iBGTotalSum { get; set; }
        public double iBGTotalXD { get; set; }
        public double iBXTotal { get; set; }
        public double itotal_bgglf { get; set; }
        public double itotal_bxglf { get; set; }
        public double total_plan { get; set; }
        public Guid? bgguid { get; set; }

        public static ZXJDModel ConvertTo(SS_ProjectView value)
        {
            return  new ZXJDModel
            {
                GUID=value.GUID,
                ProjectName=value.ProjectName,
                ProjectKey=value.ProjectKey,
                PGUID=value.PGUID
            };
        }

        public static ZXJDModel ConvertTo(SS_ProjectClassView value)
        {
            return new ZXJDModel
            {
                GUID=value.GUID,
                ProjectKey=value.ProjectClassKey,
                ProjectName=value.ProjectClassName,
                PGUID=value.PGUID
            };
        }

        public void CopySpecialValue(ZXJDModel value)
        {
            this.PersonName = value.PersonName;
            this.iBXTotal = value.iBXTotal;
            this.iBGTotalSum = value.iBGTotalSum;
            this.iBGTotalXD = value.iBGTotalXD;
            this.itotal_bgglf = value.itotal_bgglf;
            this.itotal_bxglf = value.itotal_bxglf;
            this.total_plan = value.total_plan;
        }

        public void FillData(ref DataRow currentRow, int iUnit)
        {
            currentRow["ProjectName"] = this.ProjectName;
            currentRow["ProjectKey"] = this.ProjectKey;
            currentRow["PersonName"] = this.PersonName;


            double dblBGTotalSum = this.iBGTotalSum;   // 总经费

            double dbliBGTotalXD = this.iBGTotalXD;   // 预算下达 
            double dblBXTotalExecute = this.iBXTotal; // 直接成本执行

            double dblTotal_bgglf = this.itotal_bgglf;
            double dblTtotal_bxglf = this.itotal_bxglf;
            double dbltotal_plan = this.total_plan;
            double s = 0F;

            // 管理费执行

            double dblManageriaExecute = dblTotal_bgglf == 0 ? 0F : (dblBGTotalSum - dbliBGTotalXD) * (dblTtotal_bxglf + dbltotal_plan) / dblTotal_bgglf;

            currentRow["iManageriaExecute"] = FormatMoney(1, dblManageriaExecute / iUnit);
            currentRow["iBGTotalSum"] = FormatMoney(1, dblBGTotalSum / iUnit);
            currentRow["iBGTotalXD"] = FormatMoney(1, dbliBGTotalXD / iUnit);
            currentRow["iBXTotalExecute"] = FormatMoney(1, dblBXTotalExecute / iUnit);

            // 执行数 = 直接成本执行 + 管理费执行

            currentRow["ExecuteNum"] = FormatMoney(1, (dblManageriaExecute + dblBXTotalExecute) / iUnit);
            // 直接成本结余 = 预算下达 - 直接成本执行
            currentRow["DirectCostBalance"] = FormatMoney(1, (dbliBGTotalXD - dblBXTotalExecute) / iUnit);
            // 管理费结余 = 总经费 - 预算下达 - 管理费执行

            currentRow["ManageriaCostBalance"] = FormatMoney(1, (dblBGTotalSum - dbliBGTotalXD - dblManageriaExecute) / iUnit);
            // 预算结余 = 直接成本结余 + 管理费结余;
            currentRow["BGTotalBalance"] = FormatMoney(1, (dblBXTotalExecute + dblBGTotalSum - dblManageriaExecute) / iUnit);
            // 直接成本完成比例 = 直接成本执行/预算下达
            if (dbliBGTotalXD == 0 || dblBXTotalExecute == 0)
            {
                currentRow["DirectCostScale"] = "";
            }
            else
            {
                currentRow["DirectCostScale"] = FormatMoney(1, (dblBXTotalExecute / dbliBGTotalXD) * 100) + "%";
            }
            // 管理费完成比例 = 管理费执行/总经费-预算下达
            if (dblBGTotalSum - dbliBGTotalXD == 0 || dblManageriaExecute == 0)
            {
                currentRow["ManageriaCostScale"] = "";
            }
            else
            {
                currentRow["ManageriaCostScale"] = FormatMoney(1, (dblManageriaExecute / (dblBGTotalSum - dbliBGTotalXD)) * 100) + "%";
            }
            //完成比例 = （直接成本执行 + 管理费执行）/总经费  执行数/总经费

            if (dblBGTotalSum == 0 || dblManageriaExecute + dblBXTotalExecute == 0)
            {
                currentRow["DoneScale"] = "";
            }
            else
            {
                currentRow["DoneScale"] = FormatMoney(1, ((dblManageriaExecute + dblBXTotalExecute) / dblBGTotalSum) * 100) + "%";
            }
        }

        /// <summary>
        /// 转化成货币表达式
        /// </summary>
        /// <param name="ftype">类型：0表示带￥的货币表达式 1表示不带￥的表达式，默认不带￥</param>
        /// <param name="fmoney">要转化的值</param>
        /// <returns>String</returns>
        protected string FormatMoney(int ftype, double fmoney)
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
    }

    class ReportTree
    {
        private List<ReportTreeNode> _TreeNodes = new List<ReportTreeNode>();

        public List<ReportTreeNode> Nodes { get { return _TreeNodes; } }

        public List<ReportTreeNode> Roots
        {
            get
            {
                return this._TreeNodes.FindAll(e => e.Parentkey == null);
            }
        }

        public void AddNode(ZXJDModel Value, Guid? Parentkey = null)
        {
            var cnode = this._TreeNodes.Find(e => e.Key == Value.GUID);
            if (cnode == null)
            {
                _TreeNodes.Add(new ReportTreeNode(this, Value.GUID, Value, Parentkey));
            }
            else
            {
                cnode.Parentkey = Parentkey;
            }
        }

        public void RemoveNode(Guid Key)
        {
            this._TreeNodes.RemoveAll(e => e.Key == Key);
        }

        public ReportTreeNode FindNode(Guid Key)
        {
            return this._TreeNodes.Find(e => e.Key == Key);
        }
    }

    class ReportTreeNode
    {
        private Guid _key = Guid.Empty;

        public Guid Key
        {
            get { return _key; }
        }

        private ZXJDModel _value = null;

        public ZXJDModel Value
        {
            get { return _value; }
        }

        private Guid? _parentkey = null;

        public Guid? Parentkey
        {
            get { return _parentkey; }
            set { _parentkey = value; }
        }

        private ReportTree _tree = null;

        public ReportTree Tree
        {
            get { return _tree; }
        }


        public List<ReportTreeNode> Children
        {
            get
            {
                return this.Tree.Nodes.FindAll(e => e._parentkey != null && e._parentkey == this.Key);
            }
        }


        public ReportTreeNode Parent
        {
            get
            {
                return this.Tree.Nodes.Find(e => e.Key == this._parentkey);
            }
        }

        public ReportTreeNode(ReportTree Tree,Guid Key, ZXJDModel Value, Guid? Parentkey):this(Tree,Key,Value)
        {
            this._parentkey = Parentkey;
        }
        public ReportTreeNode(ReportTree Tree, Guid Key, ZXJDModel Value):this(Tree,Key)
        {
            this._value = Value;
        }
        public ReportTreeNode(ReportTree Tree, Guid Key)
        {
            this._tree = Tree;
            this._key = Key;
        }
    }
}
