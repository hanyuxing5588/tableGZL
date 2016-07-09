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
    /// 项目支出汇总表
    /// </summary>
    public class XMZCHZB:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用       
        public XMZCHZB(string key)
            : base(key)
        {
          
        }
        public override void Init()
        {
            this.SqlFormat = "select bgcode.guid,bgcode.projectkey,bgcode.projectname,bgcode.pguid,item1.totalbg1 as totalbg1,item2.totalbg2 as totalbg2 from ss_project bgcode "
                          + "left join( "
                          + "    select a.guid_project,b.totalbg1 from bg_mainview a "
                          + "    left join (select guid_bg_main,sum(total_bg) as totalbg1 from bg_detailview where "
                          + "    bgitemkey in ('04','21','24') and pguid is not null  and bgyear='{0}' " //{0}[%预算年度%]
                          + "    group by guid_bg_main "
                          + "    ) b on a.guid=b.guid_bg_main   "
                          + "where a.guid in (select guid_bg_main from bg_detail where bgyear='{0}') "
                          + "and isnull(a.invalid,0)=1 and a.departmentkey in ({1})  and bgtypekey='02' and bgstepkey in ({2}) "
                          + ") item1 on bgcode.guid=item1.guid_project "
                          + "left join(select a.guid_project,b.totalbg2 from bg_mainview a "
                          + "     left join (select guid_bg_main,sum(total_bg) as totalbg2 from bg_detailview where "
                          + "            bgitemkey='08'  and pguid is not null  and bgyear='{0}' "
                          + "            group by guid_bg_main "
                          + "            ) b  on a.guid=b.guid_bg_main   "
                          + "where a.guid in (select guid_bg_main from bg_detail where bgyear='{0}') and isnull(a.invalid,0)=1 "
                          + "            and a.departmentkey in ({1}) and bgtypekey='02'  and bgstepkey  in ({2}) "
                          + ") item2 on bgcode.guid=item2.guid_project "
                          + "where isnull(isStop,0)=0 and isnull(isfinance,0)=1 order by projectkey ";
           this.tempalte=Path.Combine(this.tempalte, "xmzchzb.xls");        
      }
        
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(BBSearchCondition conditionModel)
        {
         
            string strsql =this.SqlFormat;            
            string departKey=string.Empty;
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
                    case "ss_department":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        departKey = GetByDepartmentKey(conditionModel.treeValue.ToString());
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
                        case "ss_department":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            departKey = GetByDepartmentKey(item.treeValue);
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
            if (string.IsNullOrEmpty(departKey))
            {
                departKey = GetByDepartmentKey(Guid.Empty.ToString());
            }
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString()); 
            }            
            return strsql = string.Format(strsql,
                                          conditionModel.Year,
                                          departKey,
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
            sqlStr = GetSql(conditionModel);
            DataTable orgdt = LoadData(sqlStr, ref msgError);
            DataTable newdt = CreateNewDataTable();
            ReSetData(orgdt, ref newdt, conditionModel.RMBUnit);
            return newdt;
        }
        /// <summary>
        /// 重新设置数据
        /// </summary>
        /// <param name="orgdt">原始数据</param>
        /// <param name="newdt">新创建的临时表</param>
        /// <returns></returns>
        public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit)
        {
            string strSpace = string.Empty;
            if (orgdt == null) return;
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit)==false)
            {
                unit = 1;
            }            
            //合计
            double hjTotalbg1 = 0F;//合计分摊实际数
            double hjTotalbg2 = 0F;//合计项目预算
            if (orgdt.Rows.Count > 0)
            {
                //int index = 0;
                DataRow[] dtRow = orgdt.Select(" pguid is null");              
                foreach (DataRow row in dtRow)
                {
                    ReSetChildData(orgdt, row, ref newdt, rmbUnit, strSpace);
                }                   
                   
                //合计计算
                DataRow hjRow = newdt.NewRow();
                string valuestr = orgdt.Compute("Sum(totalbg1)", "true").ToString();
                hjTotalbg1 = orgdt == null ? 0F : string.IsNullOrEmpty(valuestr) ? 0F : double.Parse(valuestr);
                valuestr = orgdt.Compute("Sum(totalbg2)", "true").ToString();
                hjTotalbg2 = orgdt == null ? 0F : string.IsNullOrEmpty(valuestr) ? 0F : double.Parse(valuestr);
                hjRow["projectkey"] = "合计";
                hjRow["projectname"] = "";
                hjRow["totalbg1"] = hjTotalbg1 == 0F ? "" : FormatMoney(1, hjTotalbg1 / unit);
                hjRow["totalbg2"] = hjTotalbg2 == 0F ? "" : FormatMoney(1, hjTotalbg2 / unit);
                hjRow["totalbgall"] = (hjTotalbg1 + hjTotalbg2) == 0 ? "" : FormatMoney(1, (hjTotalbg1 + hjTotalbg2) / unit);                   
                newdt.Rows.Add(hjRow);

            }
        }
        /// <summary>
        /// 重新设置子节点数据
        /// </summary>
        /// <param name="orgdt">原始数据</param>
        /// <param name="newdt">新创建的临时表</param>
        /// <returns></returns>
        public void ReSetChildData(DataTable orgdt,DataRow currentRow,ref DataTable newdt, string rmbUnit,string strSpace)
        {
            if (orgdt == null) return;
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            double totalbg1 = 0F;//分摊实际数
            double totalbg2 = 0F;//项目支出           
            if (orgdt.Rows.Count > 0)
            {
                if (currentRow != null)
                {
                    DataRow[] dtRow;
                    dtRow = orgdt.Select(" pguid is not null and pguid='" + currentRow["guid"] + "'");                    
                    totalbg1 = 0F;//分摊实际数
                    totalbg2 = 0F;//项目支出 
                    //添加当前节点值
                    DataRow newRow = newdt.NewRow();
                    newRow["projectkey"] = strSpace + currentRow["projectkey"].ToString();
                    newRow["projectname"] = currentRow["projectname"].ToString();
                    DataTable childDt = orgdt.Clone();
                    GetLeafsData(orgdt, currentRow, ref childDt);
                    GetBGTotalValue(childDt, ref totalbg1, ref totalbg2);
                    //分摊实际数
                    newRow["totalbg1"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
                    //项目支出                            
                    newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);
                    //预算合计
                    newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2) / unit);

                    newdt.Rows.Add(newRow);

                   //子节点
                    if (dtRow != null && dtRow.Length > 0)
                    {                                       
                        foreach (DataRow row in dtRow)
                        {
                            var strSpace1 =strSpace+ this.colChar;
                            ReSetChildData(orgdt, row, ref newdt, rmbUnit, strSpace1);
                        }                       
                    }
                }


            }
        }
        /// <summary>
        /// 设置预算值
        /// </summary>
        /// <param name="childDt"></param>
        /// <param name="totalbg1"></param>
        /// <param name="totalbg2"></param>
        private void GetBGTotalValue(DataTable childDt, ref double totalbg1, ref double totalbg2)
        {
            double totalbg = 0F;
            if (childDt != null && childDt.Rows.Count > 0)
            {
                foreach (DataRow row in childDt.Rows)
                {
                    //分摊实际数
                    if (row["totalbg1"] != null)
                    {
                        if (double.TryParse(row["totalbg1"].ToString(), out totalbg))
                        {
                            totalbg1 += totalbg;
                        }
                    }
                    //项目支出
                    if (row["totalbg2"] != null)
                    {
                        if (double.TryParse(row["totalbg2"].ToString(), out totalbg))
                        {
                            totalbg2 += totalbg;
                        }
                    } 
                }
            }
        }
        
        /// <summary>
        /// 获取末级节点数据
        /// </summary>
        /// <param name="orgdt"></param>
        /// <param name="currentRow"></param>
        /// <param name="childDt"></param>
        private void GetLeafsData(DataTable orgdt, DataRow currentRow, ref DataTable childDt)
        {
            DataTable dt = childDt;
            if (orgdt == null) return ;
            DataRow[] childRow = orgdt.Select(" pguid='" + currentRow["guid"] + "'");
            if (childRow.Length > 0)
            {
                foreach (DataRow row in childRow)
                {
                    GetLeafsData(orgdt, row, ref childDt);
                }
            }
            else
            {
                dt.ImportRow(currentRow);
            }

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
        private DataTable CreateNewDataTable()
        {
            DataTable dt = new DataTable();            
            dt.Columns.Add("projectkey");//项目Key
            dt.Columns.Add("projectname");//项目名称
            dt.Columns.Add("totalbg1");//分摊实际数
            dt.Columns.Add("totalbg2");//项目直接成本
            dt.Columns.Add("totalbgall");//项目成本合计 
            return dt;
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
                string filePath = ExportExcel.Export(data, this.tempalte, 10, 0, new List<ExcelCell>() { new ExcelCell(1, 7, model.DepartmentName), new ExcelCell(1, 8, model.Year), new ExcelCell(4, 8, model.RMBUnit) });
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
