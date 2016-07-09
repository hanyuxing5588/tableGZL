using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using System.IO;


namespace CAE.Report
{
    /// <summary>
    /// 单位预算汇总表
    /// </summary>
    public class DWYSHZB:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用 
        /// <summary>
        /// 项目格式数据
        /// </summary>
        public string sqlProjectFormat = "select  distinct bg_mainview.GUID_Project,ProjectName,ProjectKey from BG_MainView "
                                        +"left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                                        +"bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                                        +"and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('08') and BGYear='{0}' "
                                        +"and GUID_DW in({1}) and BGStepKey in({2}) order by Guid_Project desc";
        /// <summary>
        /// 科目数据
        /// </summary>
        public string sqlBGCodeFormat = "select guid,pguid,bgcodekey,bgcodename from ss_bgcode where isstop=0 order by bgcodekey ";
       /// <summary>
       /// 查询数据
       /// </summary>
        public string sqlQueryFormat = "select bgdetail.GUID_BGCode,bgdetail.pguid,bgdetail.BGCodeKey,sum(bgdetail.Total_BG)Total_BG,bgmain.GUID_Project,bgmain.ProjectKey,bgmain.ProjectName from bg_detailview as bgdetail "
                                    +"left join ( "
                                    +"select  distinct bg_mainview.* from BG_MainView "
                                    +"left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                                    +"bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                                    +"and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('08') and BGYear='{0}' "
                                    +"and GUID_DW in({1}) and BGStepKey in({2}) "
                                    +") as bgmain on bgdetail.GUID_BG_Main=bgmain.GUID "
                                    + "where bgmain.guid_project is not null and BGYear='{0}' and BGItemKey in ('08')"
                                    + "group by bgdetail.GUID_BGCode,bgdetail.pguid,bgdetail.BGCodeKey,bgmain.GUID_Project,bgmain.ProjectKey,bgmain.ProjectName";
       public DWYSHZB(string key)
            : base(key)
        {
          
        }
       public override void Init()
       {
           this.tempalte = Path.Combine(this.tempalte, "dwyshzb.xls");
       }
       
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(BBSearchCondition conditionModel,string sqlFormat)
        {

            string strsql = sqlFormat;            
            string dwId=string.Empty;
            string bgSetpId = string.Empty;            
            if (conditionModel.Year == "0")
            {
                conditionModel.Year = "";
            }
            //开始时间

            DateTime StartDate = DateTime.MinValue;
            ///// 结束时间
            DateTime EndDate = DateTime.MaxValue;
            if (!string.IsNullOrEmpty(conditionModel.treeModel) && (conditionModel.treeValue != null && conditionModel.treeValue != Guid.Empty))
            {
                switch (conditionModel.treeModel.ToLower())
                {
                    case "ss_dw":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        dwId = GetByDWGUID(conditionModel.treeValue.ToString());
                        break;
                    case "bg_setup":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        bgSetpId = GetStepKey(conditionModel.treeValue.ToString());
                        break;
                }
            }
            //树节点条件
            if (conditionModel.TreeNodeList!=null && conditionModel.TreeNodeList.Count>0)
            {
                foreach (TreeNode item in conditionModel.TreeNodeList)
                {
                    switch (item.treeModel.ToLower())
                    {
                        case "ss_dw":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            dwId = GetByDWGUID(item.treeValue);
                            break;
                        case "bg_setup":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            bgSetpId = GetStepKey(item.treeValue);
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(dwId))
            {
                dwId = GetByDWGUID(Guid.Empty.ToString());
            }
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString()); 
            }            
            return strsql = string.Format(strsql,
                                          conditionModel.Year,
                                          dwId,
                                          bgSetpId                                         
                                          );
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        public DataTable GetReport(SearchCondition conditions, out string msgError)
        {
            msgError = "";
            string sqlStr = string.Empty;
            BBSearchCondition conditionModel = (BBSearchCondition)conditions;
            sqlStr = GetSql(conditionModel,sqlQueryFormat);
            DataTable orgdt = LoadData(sqlStr, ref msgError);//查询的数据
            sqlStr = GetSql(conditionModel,sqlProjectFormat);
            DataTable projectdt = LoadData(sqlStr, ref msgError);//项目数据
            sqlStr = GetSql(conditionModel, sqlBGCodeFormat);
            DataTable bgcodedt = LoadData(sqlStr, ref msgError);//科目数据
            DataTable newdt = CreateNewDataTable(projectdt);
            ReSetData(orgdt,bgcodedt,projectdt,ref newdt,conditionModel.RMBUnit);           
            return newdt;
        }
       
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        private DataTable LoadData(string sql, ref string msgerror)
        {
           var dt= DataSource.ExecuteQuery(sql);
           if (dt == null || dt.Rows.Count == 0)
           {
               msgerror = "服务器连接失败。";
               return null;
           }
           return dt;
        }
        /// <summary>
        /// 创建临时新表
        /// </summary>
        /// <returns></returns>
        private DataTable CreateNewDataTable(DataTable projectdt)
        {
            DataTable dt = new DataTable();
            //动态添加列
            dt.Columns.Add("bgcodekey");//科目编号
            dt.Columns.Add("bgcodename");//科目名称
            //添加列表头信息
            DataRow dr = dt.NewRow();
            dr["bgcodekey"] = "科目编号";
            dr["bgcodename"] = "科目名称";
            //项目表头信息
            if (projectdt != null && projectdt.Rows.Count > 0)
            {
                for (int i = 0; i < projectdt.Rows.Count; i++)
                {
                    dt.Columns.Add("p" + i);
                    dr["p" + i] = "(" + projectdt.Rows[i]["ProjectKey"] + ")" + projectdt.Rows[i]["ProjectName"];
                }
            }
            //合计
            dt.Columns.Add("totalbgall");//预算合计         
            dr["totalbgall"] = "合计";
            dt.Rows.Add(dr);
            return dt;
        }
        /// <summary>
        /// 重新设置数据
        /// </summary>
        /// <returns></returns>
        private void ReSetData(DataTable orgdt, DataTable bgcodedt, DataTable projectdt, ref DataTable newdt, string rmbUnit)
        {
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            DataRow pRow = null;//项目行
            if (bgcodedt == null || bgcodedt.Rows.Count == 0) return;
            var bgRows = bgcodedt.Select(" pguid is null ", " bgcodekey asc ");
            if (bgRows.Length > 0)
            {
                foreach (DataRow row in bgRows)
                {  
                    //创建一行数据
                    DataRow dr = newdt.NewRow();
                    dr["bgcodekey"] =row["bgcodekey"];
                    dr["bgcodename"] = row["bgcodename"];
                    var bgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
                    dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
                    if (projectdt != null)
                    {
                        for (int j = 0; j < projectdt.Rows.Count; j++)
                        {
                            pRow = projectdt.Rows[j];
                            //每个科目对应的项目金额
                            var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
                            dr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
                        }
                    }                   
                    newdt.Rows.Add(dr);
                    DataRow[] childDr = bgcodedt.Select(" pguid='" + row["guid"] + "'", "bgcodekey asc");
                    if (childDr != null && childDr.Length > 0)
                    {
                        ReSetChildData(orgdt,childDr,projectdt,ref newdt,rmbUnit);
                    }
                }
                //纵向合计（即项目合计）
                var hjbgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "len(bgcodekey)<>2");
                DataRow enddr = newdt.NewRow();
                enddr["bgcodekey"] = "合计";
                enddr["bgcodename"] = "";
                enddr["totalbgall"] = FormatMoney(1, double.Parse(hjbgTotal.ToString()) / unit); //orgdt.Select(" GUID_BGCode='" + row["guid"] + "'")
                if (projectdt != null)
                {
                    for (int j = 0; j < projectdt.Rows.Count; j++)
                    {
                        pRow = projectdt.Rows[j];
                        var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_Project='" + pRow["GUID_Project"] + "' and len(bgcodekey)<>2");
                        enddr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
                    }
                }
                newdt.Rows.Add(enddr);
            }
        }
        /// <summary>
        /// 重新设置子数据
        /// </summary>
        /// <returns></returns>
        private void ReSetChildData(DataTable orgdt, DataRow[] bgRows, DataTable projectdt, ref DataTable newdt, string rmbUnit)
        { 
             int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            DataRow pRow = null;//项目行
            if (bgRows == null || bgRows.Length == 0) return;            
            if (bgRows.Length > 0)
            {
                foreach (DataRow row in bgRows)
                {
                    //创建一行数据
                    DataRow dr = newdt.NewRow();
                    dr["bgcodekey"] = this.colChar + row["bgcodekey"];
                    dr["bgcodename"] = row["bgcodename"];
                    var bgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
                    dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
                    if (projectdt != null)
                    {
                        for (int j = 0; j < projectdt.Rows.Count; j++)
                        {
                            pRow = projectdt.Rows[j];
                            //每个科目对应的项目金额
                            var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
                            dr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
                        }
                    }
                    newdt.Rows.Add(dr);
                }
            }
        }
        /// <summary>
        /// 创建临时新表
        /// </summary>
        /// <returns></returns>
        //private DataTable CreateNewDataTable1(DataTable orgdt, DataTable bgcodedt, DataTable projectdt, string rmbUnit)
        //{
        //    int unit = 1;
        //    if (int.TryParse(rmbUnit, out unit) == false)
        //    {
        //        unit = 1;
        //    }
        //    DataTable dt = new DataTable();
        //    //动态添加列
        //    dt.Columns.Add("bgcodekey");//科目编号
        //    dt.Columns.Add("bgcodename");//科目名称
        //    //添加列表头信息
        //    DataRow dr = dt.NewRow();
        //    dr["bgcodekey"] = "科目编号";
        //    dr["bgcodename"] = "科目名称";
        //    //项目表头信息
        //    if (projectdt != null && projectdt.Rows.Count > 0)
        //    {
        //        for (int i = 0; i < projectdt.Rows.Count; i++)
        //        {
        //            dt.Columns.Add("p" + i);
        //            dr["p" + i] = "(" + projectdt.Rows[i]["ProjectKey"] + ")" + projectdt.Rows[i]["ProjectName"];
        //        }
        //    }
        //    //合计
        //    dt.Columns.Add("totalbgall");//预算合计         
        //    dr["totalbgall"] = "合计";
        //    dt.Rows.Add(dr);

        //    DataRow row = null;
        //    DataRow pRow = null;
        //    for (int i = 0; i < bgcodedt.Rows.Count; i++)
        //    {
        //        row = bgcodedt.Rows[i];
        //        dr = dt.NewRow();
        //        dr["bgcodekey"] = this.colChar + row["bgcodekey"];
        //        dr["bgcodename"] = row["bgcodename"];
        //        var bgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
        //        dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
        //        if (projectdt != null)
        //        {
        //            for (int j = 0; j < projectdt.Rows.Count; j++)
        //            {
        //                pRow = projectdt.Rows[j];
        //                //每个科目对应的项目金额
        //                var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
        //                dr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
        //            }
        //        }
        //        //等级数据
        //        if (row["bgcodekey"] != null && row["bgcodekey"].ToString().Length == 2)
        //        {
        //            dr["bgcodekey"] = row["bgcodekey"];
        //        }
        //        dt.Rows.Add(dr);
        //    }
        //    //纵向合计（即项目合计）
        //    var hjbgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "len(bgcodekey)<>2");
        //    DataRow enddr = dt.NewRow();
        //    enddr["bgcodekey"] = "合计";
        //    enddr["bgcodename"] = "";
        //    enddr["totalbgall"] = FormatMoney(1, double.Parse(hjbgTotal.ToString()) / unit); //orgdt.Select(" GUID_BGCode='" + row["guid"] + "'")
        //    if (projectdt != null)
        //    {
        //        for (int j = 0; j < projectdt.Rows.Count; j++)
        //        {
        //            pRow = projectdt.Rows[j];
        //            var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_Project='" + pRow["GUID_Project"] + "' and len(bgcodekey)<>2");
        //            enddr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
        //        }
        //    }
        //    dt.Rows.Add(enddr);
        //    return dt;
        //}
  
        //导出报表
        public string GetExportPath(DataTable data, out string fileName, out string message, ReportHeadModel model)
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
                string filePath = ExportExcel.Export(data, this.tempalte, 3, 0, new List<ExcelCell>() {new ExcelCell(1, 2, model.Year) });
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }
    }
    
}
