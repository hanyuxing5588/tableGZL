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
    public class BMYSHZB:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用 
        /// <summary>
        /// 项目格式数据
        /// </summary>
        public string sqlProjectFormat = "select  distinct bg_mainview.GUID_Project,ProjectName,ProjectKey from BG_MainView "
                                        +"left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                                        +"bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                                        +"and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('08') and BGYear='{0}' "
                                        + "and {1} and BGStepKey in({2}) order by Guid_Project desc"; //{1}单位或者部门
                                       // +"and GUID_DW in({1}) and BGStepKey in({2}) order by Guid_Project desc";


        /// <summary>
        /// 项目(权限过滤)格式数据
        /// </summary>
        public string sqlProjectFormatByAuth = "select * from ("
                         + "select  distinct bg_mainview.GUID_Project,ProjectName,ProjectKey from BG_MainView "
                         + "left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                         + "bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                         + "and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('08') and BGYear='{0}' "
                         + "and {1} and BGStepKey in({2})"
                         + ") a where a.guid_project in ( "
                         + "select guid_data from ss_dataauthset where guid_roleoroperator in ({3})"
                         + " and classid in (select classid from ss_class where tablename='ss_project')"
                         + " and (isTimelimited<>1 or (isTimeLimited=1 and starttime<=getdate() and stoptime>=getdate()))"
                         + ") order by a.Guid_Project desc"; //{3} 为操作员GUID和所在角色GUID的总和

        /// <summary>
        /// 科目数据
        /// </summary>
        public string sqlBGCodeFormat = "select guid,pguid,bgcodekey,bgcodename from ss_bgcode where bgcodekey not like '05%' and bgcodekey not like '03%' and  isstop=0 order by bgcodekey ";
        /// <summary>
        /// 人员
        /// </summary>
        public string sqlPersonFormat = "select count(*) from ss_personView where {1} and PersonTypeKey='01'";////{1}单位或者部门
       /// <summary>
       /// 查询总数据
       /// </summary>
        public string sqlAllQueryFormat = "select  distinct bg_mainview.* from BG_MainView "
                                    + "left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                                    + "bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                                    + "and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('04','08') and BGYear='{0}' "
                                    + "and {1} and BGStepKey in({2}) ";////{1}单位或者部门
                                   
                                    
        /// <summary>
        /// 查询数据
        /// </summary>
        public string sqlQueryFormat = "select bgdetail.GUID_BGCode,bgdetail.BGCodeKey,sum(bgdetail.Total_BG)Total_BG,bgmain.GUID_Project,bgmain.ProjectKey,bgmain.ProjectName from bg_detailview as bgdetail "
                                    + "left join ( "
                                    + "select  distinct bg_mainview.* from BG_MainView "
                                    + "left join bg_DetailView on bg_MainView.guid=bg_DetailView.GUID_BG_Main where "
                                    + "bg_mainView.guid in(select guid_bg_main from bg_detail where bgyear='{0}' ) "
                                    + "and isnull(bg_MainView.invalid,0)=1 and bgitemKey in ('08') and BGYear='{0}' "
                                    + "and {1} and BGStepKey in({2}) "//and GUID_DW in({1}) //{1}单位或者部门
                                    + ") as bgmain on bgdetail.GUID_BG_Main=bgmain.GUID "
                                    + "where bgmain.guid_project is not null and BGYear='{0}' and BGItemKey in ('08')"
                                    + "group by bgdetail.GUID_BGCode,bgdetail.BGCodeKey,bgmain.GUID_Project,bgmain.ProjectKey,bgmain.ProjectName";
        public BMYSHZB(string key)
            : base(key)
        {
          
        }
       public override void Init()
       {
           this.tempalte = Path.Combine(this.tempalte, "bmyshzb.xls");
       }
       
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(BBSearchCondition conditionModel,string sqlFormat)
        {

            string strsql = sqlFormat;            
            string dwIdOrDepartId="1=1 ";    
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
                        dwIdOrDepartId = " GUID_DW in(" + GetByDWGUID(conditionModel.treeValue.ToString()) + ")";
                        break;
                    case "ss_department":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        dwIdOrDepartId = " GUID_Department in(" + GetByDepartmentId(conditionModel.treeValue.ToString()) + ")";
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
                            dwIdOrDepartId = " GUID_DW in(" + GetByDWGUID(item.treeValue) + ") ";
                            break;
                        case "ss_department":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            dwIdOrDepartId = " GUID_Department in(" + GetByDepartmentId(item.treeValue) + ") ";
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
            
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString()); 
            }            
            return strsql = string.Format(strsql,
                                          conditionModel.Year,
                                          dwIdOrDepartId,
                                          bgSetpId                                         
                                          );
        }

        public string GetProjectSqlByAuth(BBSearchCondition conditionModel, string sqlFormat)
        {
            var sql = GetSql(conditionModel, sqlProjectFormat);
            //过滤权限;
            if (!string.IsNullOrEmpty(this.OperatorKey))
            {
                string rolesql = string.Format("select guid_role from ss_roleoperator where guid_operator='{0}'",
                    this.OperatorKey);
                var dt = DataSource.ExecuteQuery(rolesql);
                List<string> guids = new List<string>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[0]!=null) guids.Add("'" + row[0].ToString() + "'");
                    }
                }
                guids.Add("'" + this.OperatorKey + "'");
                string guidparm = string.Join(",", guids.ToArray());
                if (!string.IsNullOrEmpty(guidparm))
                {
                    string strsql = sqlFormat;
                    string dwIdOrDepartId = "1=1 ";
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
                                dwIdOrDepartId = " GUID_DW in(" + GetByDWGUID(conditionModel.treeValue.ToString()) + ")";
                                break;
                            case "ss_department":
                                if (conditionModel.treeValue == null)
                                {
                                    conditionModel.treeValue = Guid.Empty;
                                }
                                dwIdOrDepartId = " GUID_Department in(" + GetByDepartmentId(conditionModel.treeValue.ToString()) + ")";
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

                    if (conditionModel.TreeNodeList != null && conditionModel.TreeNodeList.Count > 0)
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
                                    dwIdOrDepartId = " GUID_DW in(" + GetByDWGUID(item.treeValue) + ") ";
                                    break;
                                case "ss_department":
                                    if (item.treeValue == null)
                                    {
                                        item.treeValue = Guid.Empty.ToString();
                                    }
                                    dwIdOrDepartId = " GUID_Department in(" + GetByDepartmentId(item.treeValue) + ") ";
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

                    if (string.IsNullOrEmpty(bgSetpId))
                    {
                        bgSetpId = GetStepKey(Guid.Empty.ToString());
                    }
                    sql = string.Format(strsql,
                                                  conditionModel.Year,
                                                  dwIdOrDepartId,
                                                  bgSetpId,guidparm
                                                  );
                }
                
            }
            return sql;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        public DataTable GetReport(SearchCondition conditions, out string msgError)
        {
            DataTable newdt = new DataTable();
            msgError = "";
            string sqlStr = string.Empty;
            BBSearchCondition conditionModel = (BBSearchCondition)conditions;
            sqlStr = GetSql(conditionModel, sqlAllQueryFormat);         
            DataTable orgAlldt = LoadData(sqlStr, ref msgError);//查询原数据的所有数据
            sqlStr = GetSql(conditionModel,sqlQueryFormat);           
            DataTable orgdt = LoadData(sqlStr, ref msgError);//查询的数据
            //sqlStr = GetSql(conditionModel,sqlProjectFormat);
            //根据权限过滤项目
            sqlStr = GetProjectSqlByAuth(conditionModel, sqlProjectFormatByAuth);
            DataTable projectdt = LoadData(sqlStr, ref msgError);//项目数据
            sqlStr = GetSql(conditionModel, sqlBGCodeFormat);
            DataTable bgcodedt = LoadData(sqlStr, ref msgError);//科目数据
            sqlStr = GetSql(conditionModel, sqlPersonFormat);//人员信息
            DataTable personDt = LoadData(sqlStr, ref msgError);
            CreateNewDataTable(projectdt, ref newdt);//创建临时新表
            ReSetData(orgdt, bgcodedt, projectdt, ref newdt, conditionModel.RMBUnit,orgAlldt,personDt);

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
        private void CreateNewDataTable(DataTable projectdt,ref DataTable newDt)
        {
            DataTable dt = newDt;
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
            dt.Columns.Add("zzsrbl");//占总收入比例
            dr["zzsrbl"] = "占总收入比例";
            dt.Columns.Add("personAvg");//人均
            dr["personAvg"] = "人均";
            dt.Rows.Add(dr);          
            
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="orgdt"></param>
        /// <param name="bgcodedt"></param>
        /// <param name="projectdt"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        /// <param name="orgAlldt"></param>
        /// <param name="persondt"></param>
        private void ReSetData(DataTable orgdt, DataTable bgcodedt,DataTable projectdt, ref DataTable newdt, string rmbUnit,DataTable orgAlldt,DataTable persondt)
        {
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
           
            DataTable dt = newdt;           
            DataRow pRow=null;
            DataRow dr = null;
            double hjAllTotal=0F;//总经费合计
            double personCount=0;//人数
            if(persondt!=null)
            {
                personCount=int.Parse(persondt.Rows[0][0].ToString());
            }
            //总费用
            hjAllTotal = orgAlldt == null ? 0 : Convert.ToDouble(orgAlldt.Compute("Sum(Total_BG)", "true"));
            hjAllTotal = hjAllTotal == null ? 0 : hjAllTotal;
            #region 总经费
            if (projectdt != null)
            {
                dr = dt.NewRow();
                dr["bgcodekey"] = this.colChar +"总经费";
                dr["bgcodename"] ="";
                for (int j = 0; j < projectdt.Rows.Count; j++)
                {
                    pRow = projectdt.Rows[j];
                    //每个科目对应的项目金额
                    var totalValue = orgAlldt == null ? 0 : orgAlldt.Compute("Sum(Total_BG)", " GUID_Project='" + pRow["GUID_Project"] + "'");
                    totalValue = totalValue == null ? 0 : totalValue;
                    dr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
                }
               
                dr["totalbgall"] =hjAllTotal==0F?"": FormatMoney(1,hjAllTotal/unit);
                dr["zzsrbl"] = "";
                dr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (hjAllTotal / unit) / personCount);
                dt.Rows.Add(dr);
            }
            #endregion
           
            if (bgcodedt == null || bgcodedt.Rows.Count == 0) return;
            var bgRows = bgcodedt.Select(" pguid is null ", " bgcodekey asc ");
            if (bgRows.Length > 0)
            {
                foreach (DataRow row in bgRows)
                {                   
                    dr = dt.NewRow();
                    dr["bgcodekey"] = row["bgcodekey"];
                    dr["bgcodename"] = row["bgcodename"];
                    var bgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
                    bgTotal = bgTotal == null ? 0 : bgTotal;
                    dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
                    if (projectdt != null)
                    {
                        for (int j = 0; j < projectdt.Rows.Count; j++)
                        {
                            pRow = projectdt.Rows[j];
                            //每个科目对应的项目金额
                            var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
                            totalValue = totalValue == null ? 0 : totalValue;
                            var dTotalValue = totalValue == DBNull.Value ? 0F : double.Parse(totalValue.ToString());
                            dr["p" + j] = dTotalValue == 0F ? "" : FormatMoney(1, dTotalValue / unit);
                            //占总收入比例
                            dr["zzsrbl"] = (dTotalValue == 0F || hjAllTotal == 0F) ? "" : FormatMoney(1, (dTotalValue / hjAllTotal) * 100) + "%";
                            //人均
                            dr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (dTotalValue / unit) / personCount);
                        }
                    }
                   
                    dt.Rows.Add(dr);
                     DataRow[] childDr = bgcodedt.Select(" pguid='" + row["guid"] + "'", "bgcodekey asc");
                    if (childDr != null && childDr.Length > 0)
                    {
                        ReSetChildData(orgdt,childDr,projectdt,ref newdt,rmbUnit, orgAlldt,persondt);
                    }
                }
                //纵向合计（即项目合计）
                var hjbgTotal = orgdt == null ? 0F : orgdt.Compute("Sum(Total_BG)", "len(bgcodekey)<>2");
                hjbgTotal = hjbgTotal == null ? 0F : hjbgTotal;
                var dhjbgTotal = hjbgTotal == DBNull.Value ? 0F : double.Parse(hjbgTotal.ToString());
                DataRow enddr = dt.NewRow();
                enddr["bgcodekey"] = "合计";
                enddr["bgcodename"] = "";
                enddr["totalbgall"] = FormatMoney(1, dhjbgTotal / unit); //orgdt.Select(" GUID_BGCode='" + row["guid"] + "'")
                if (projectdt != null)
                {
                    for (int j = 0; j < projectdt.Rows.Count; j++)
                    {
                        pRow = projectdt.Rows[j];
                        var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_Project='" + pRow["GUID_Project"] + "' and len(bgcodekey)<>2");
                        totalValue = totalValue == null ? 0 : totalValue;
                        enddr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
                    }

                }
                //占总收入比例
                enddr["zzsrbl"] = (dhjbgTotal == 0F || hjAllTotal == 0F) ? "" : FormatMoney(1, (dhjbgTotal / hjAllTotal) * 100) + "%";
                //人均
                enddr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (dhjbgTotal / unit) / personCount);
                dt.Rows.Add(enddr);
            }

            #region 原数据
            //for (int i = 0; i < bgcodedt.Rows.Count; i++)
            //{
            //    row=bgcodedt.Rows[i];
            //    dr = dt.NewRow();
            //    dr["bgcodekey"] =this.colChar+row["bgcodekey"];
            //    dr["bgcodename"] = row["bgcodename"];
            //    var bgTotal =orgdt==null?0:orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
            //    bgTotal = bgTotal == null ? 0 : bgTotal;
            //    dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
            //    if (projectdt != null)
            //    {
            //        for (int j = 0; j < projectdt.Rows.Count; j++)
            //        {
            //            pRow = projectdt.Rows[j];
            //            //每个科目对应的项目金额
            //            var totalValue = orgdt == null ?0: orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
            //            totalValue = totalValue == null ? 0 : totalValue;
            //            var dTotalValue=totalValue == DBNull.Value ? 0F:double.Parse(totalValue.ToString());
            //            dr["p" + j] = dTotalValue == 0F ? "" : FormatMoney(1, dTotalValue/ unit);
            //            //占总收入比例
            //            dr["zzsrbl"] = (dTotalValue == 0F || hjAllTotal==0F) ? "" : FormatMoney(1, (dTotalValue / hjAllTotal) * 100) + "%";
            //            //人均
            //            dr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (dTotalValue / unit) / personCount);
            //        }
            //    }
            //    //等级数据
            //    if (row["bgcodekey"] != null && row["bgcodekey"].ToString().Length == 2)
            //    {
            //        dr["bgcodekey"]=row["bgcodekey"];
            //    }
            //    dt.Rows.Add(dr);
            //}
            ////纵向合计（即项目合计）
            //var hjbgTotal = orgdt == null ? 0F : orgdt.Compute("Sum(Total_BG)", "len(bgcodekey)<>2");
            //hjbgTotal = hjbgTotal == null ? 0F : hjbgTotal;
            //var dhjbgTotal =hjbgTotal==DBNull.Value?0F:double.Parse(hjbgTotal.ToString());
            //DataRow enddr = dt.NewRow();
            //enddr["bgcodekey"] ="合计";
            //enddr["bgcodename"] ="";
            //enddr["totalbgall"] = FormatMoney(1, dhjbgTotal/ unit); //orgdt.Select(" GUID_BGCode='" + row["guid"] + "'")
            //if (projectdt != null)
            //{
            //    for (int j = 0; j < projectdt.Rows.Count; j++)
            //    {
            //        pRow = projectdt.Rows[j];
            //        var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_Project='" + pRow["GUID_Project"] + "' and len(bgcodekey)<>2");
            //        totalValue=totalValue==null?0:totalValue;
            //        enddr["p" + j] = totalValue == DBNull.Value ? "" : FormatMoney(1, double.Parse(totalValue.ToString()) / unit);
            //    }

            //}
            ////占总收入比例
            //enddr["zzsrbl"] = (dhjbgTotal == 0F || hjAllTotal==0F) ? "" : FormatMoney(1, (dhjbgTotal / hjAllTotal) * 100) + "%";
            ////人均
            //enddr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (dhjbgTotal / unit) / personCount);
            //dt.Rows.Add(enddr);
            #endregion

        }
        /// <summary>
        /// 设置子数据
        /// </summary>
        /// <param name="orgdt"></param>
        /// <param name="bgRows"></param>
        /// <param name="projectdt"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        /// <param name="orgAlldt"></param>
        /// <param name="persondt"></param>
        private void ReSetChildData(DataTable orgdt, DataRow[] bgRows, DataTable projectdt, ref DataTable newdt, string rmbUnit, DataTable orgAlldt, DataTable persondt)
        {
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            DataRow dr = null;
            DataRow pRow = null;
            double hjAllTotal = 0F;//总经费合计
            double personCount = 0;//人数
            if (persondt != null)
            {
                personCount = int.Parse(persondt.Rows[0][0].ToString());
            }
            if (bgRows.Length > 0)
            {
                foreach (DataRow row in bgRows)
                {
                    dr = newdt.NewRow();
                    dr["bgcodekey"] =this.colChar+row["bgcodekey"];
                    dr["bgcodename"] = row["bgcodename"];
                    var bgTotal = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "'");
                    bgTotal = bgTotal == null ? 0 : bgTotal;
                    dr["totalbgall"] = bgTotal == DBNull.Value ? "" : FormatMoney(1, double.Parse(bgTotal.ToString()) / unit); //横行科目合计
                    if (projectdt != null)
                    {
                        for (int j = 0; j < projectdt.Rows.Count; j++)
                        {
                            pRow = projectdt.Rows[j];
                            //每个科目对应的项目金额
                            var totalValue = orgdt == null ? 0 : orgdt.Compute("Sum(Total_BG)", "GUID_BGCode='" + row["guid"] + "' and GUID_Project='" + pRow["GUID_Project"] + "'");
                            totalValue = totalValue == null ? 0 : totalValue;
                            var dTotalValue = totalValue == DBNull.Value ? 0F : double.Parse(totalValue.ToString());
                            dr["p" + j] = dTotalValue == 0F ? "" : FormatMoney(1, dTotalValue / unit);
                            //占总收入比例
                            dr["zzsrbl"] = (dTotalValue == 0F || hjAllTotal == 0F) ? "" : FormatMoney(1, (dTotalValue / hjAllTotal) * 100) + "%";
                            //人均
                            dr["personAvg"] = personCount == 0 ? "" : FormatMoney(1, (dTotalValue / unit) / personCount);
                        }
                    }

                    newdt.Rows.Add(dr);
                }
            }
        }
  
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
                string filePath = ExportExcel.Export(data, this.tempalte, 3, 0, new List<ExcelCell>() { new ExcelCell(1, 1, model.DepartmentName), new ExcelCell(4,1, model.PersonCount), new ExcelCell(1, 2, model.Year), new ExcelCell(4, 2, model.RMBUnit) });
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
