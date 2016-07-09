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
    /// 总支出汇总表
    /// </summary>
    public class ZZCHZB:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用       
        public ZZCHZB(string key)
            : base(key)
        {
          
        }
        public override void Init()
        {
            this.SqlFormat = "select a.guid,a.pguid,a.bgcodekey,a.bgcodename,b.totalbg1,b.totalbg2 from ss_bgcode a "
                    +"left join "
                    +"( "
                           +"select bgcode.guid,item1.totalbg1 as totalbg1 ,item2.totalbg2 as totalbg2 from ss_bgcode bgcode "
                           + "left join( "
                           + "select guid_BGcode,sum(total_BG) as totalbg1 from bg_detailview where "
                           + "guid_item in(select guid from bg_item where bgitemkey in ('02','15','18')) "
                           + "and guid_bg_main in( "
                           + "        select guid from bg_mainview where isnull(invalid,0)=1 "
                           + "        and GUID_Department in ({0}) and bgtypekey='01'  and bgstepkey in ({1}) "
                           + "        ) "
                           + "and bgyear='{2}' group by guid_bgcode ) item1 on bgcode.guid=item1.guid_bgcode  "
                           + "left join( "
                           + "select guid_BGcode,sum(total_BG) as totalbg2 from bg_detailview where "
                           + "guid_item in(select guid from bg_item where bgitemkey in ('04','21','24','08')) "
                           + "and guid_bg_main in( "
                           + "        select guid from bg_mainview where isnull(invalid,0)=1 "
                           + "        and GUID_Department in ({0}) and bgtypekey='02'  and bgstepkey in ({1}) "
                           + "          )  "
                           + "and bgyear='{2}' group by guid_bgcode "
                           + ") item2 on bgcode.guid=item2.guid_bgcode "
                    +") as b on a.guid=b.guid "
                    + "where a.isstop=0 and  a.BGCodeKey in(select DISTINCT BGCodeKey from BG_SetupBGCodeView WHERE  BGSetupKey IN ({1}))     order by bgcodekey ";
           this.tempalte=Path.Combine(this.tempalte, "zzchzb.xls");        
      }
        /// <summary>
        /// 获取部门ID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private string GetByDepartmentId(string guid)
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
                depId = "'"+guid+"'";
            }
            return depId;
        }
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(BBSearchCondition conditionModel)
        {
         
            string strsql =this.SqlFormat;            
            string departId=string.Empty;
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
                        departId = GetByDepartmentId(conditionModel.treeValue.ToString());
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
                            departId = GetByDepartmentId(item.treeValue);
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
            if (string.IsNullOrEmpty(departId))
            {
                departId = GetByDepartmentId(Guid.Empty.ToString());
            }
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString()); 
            }            
            return strsql = string.Format(strsql,                                          
                                          departId,
                                          bgSetpId,
                                          conditionModel.Year
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
        ///// <summary>
        ///// 重新设置数据
        ///// </summary>
        ///// <param name="orgdt">原始数据</param>
        ///// <param name="newdt">新创建的临时表</param>
        ///// <returns></returns>
        //public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit)
        //{
        //    if (orgdt == null) return;
        //    int unit = 1;
        //    if (int.TryParse(rmbUnit, out unit)==false)
        //    {
        //        unit = 1;
        //    }
        //    double totalbg1 = 0F;//基本支出
        //    double totalbg2 = 0F;//项目支出 
        //    //合计
        //    double hjTotalbg1 = 0F;//合计基本支出
        //    double hjTotalbg2 = 0F;//合计项目预算
           

           
        //    if (orgdt.Rows.Count > 0)
        //    {
        //        foreach (DataRow row in orgdt.Rows)
        //        {
        //            totalbg1 = 0F;//基本支出
        //            totalbg2 = 0F;//项目支出 
        //            DataRow newRow=newdt.NewRow();
        //            newRow["bgcodekey"] =this.colChar + row["bgcodekey"].ToString();
        //            newRow["bgcodename"] = row["bgcodename"].ToString();                    
                    
        //                //计算顶级项
        //                if (row["bgcodekey"] != null && row["bgcodekey"].ToString().Length == 2)
        //                {
        //                    double pTotalbg1 = 0F;//顶级的基本支出
        //                    double pTotalbg2 = 0F;//顶级的项目支出
        //                    DataRow[] dr = orgdt.Select(" bgcodekey like '" + row["bgcodekey"].ToString() + "%' and len(bgcodekey)<>2 ");
        //                    if (dr.Length > 0)
        //                    {                               
        //                        foreach (DataRow r in dr)
        //                        {
        //                            if (r["totalbg1"] != null)
        //                            {
        //                                if (double.TryParse(r["totalbg1"].ToString(), out pTotalbg1))
        //                                {
        //                                    if (pTotalbg1 != 0F)
        //                                    {
        //                                        totalbg1 += pTotalbg1;
        //                                    }
        //                                }
        //                            }
        //                            if (r["totalbg2"] != null)
        //                            {
        //                                if (double.TryParse(r["totalbg2"].ToString(), out pTotalbg2))
        //                                {
        //                                    if (pTotalbg2 != 0F)
        //                                    {
        //                                        totalbg2 += pTotalbg2;
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    //基本支出
        //                    newRow["totalbg1"] = totalbg1 == 0F ?"": FormatMoney(1, totalbg1 / unit);                            
        //                    //项目支出                            
        //                    newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);                           
        //                    //预算合计
        //                    newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2)/unit);

        //                    newRow["bgcodekey"] =row["bgcodekey"].ToString();
        //                }
        //                else
        //                {
        //                    //基本支出
        //                    if (row["totalbg1"] != null)
        //                    {
        //                        if (double.TryParse(row["totalbg1"].ToString(), out totalbg1))
        //                        {
        //                            //基本支出
        //                            newRow["totalbg1"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
        //                        }
        //                    }
        //                    //项目支出
        //                    if (row["totalbg2"] != null)
        //                    {
        //                        if (double.TryParse(row["totalbg2"].ToString(), out totalbg2))
        //                        {
        //                            //项目支出                            
        //                            newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit); 
        //                        }
        //                    }
        //                    //预算合计
        //                    newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2) / unit);
        //                    //合计
        //                    hjTotalbg1 += totalbg1;//基本支出
        //                    hjTotalbg2 += totalbg2;//项目支出
                           
        //                }                    
                   
        //            newdt.Rows.Add(newRow);
        //        }
        //        //合计计算
        //        DataRow hjRow = newdt.NewRow();
        //        hjRow["bgcodekey"] = "合计";
        //        hjRow["bgcodename"] = "";
        //        hjRow["totalbg1"] = hjTotalbg1 == 0F ? "" : FormatMoney(1, hjTotalbg1 / unit);
        //        hjRow["totalbg2"] = hjTotalbg2 == 0F ? "" : FormatMoney(1, hjTotalbg2 / unit);
        //        hjRow["totalbgall"] = (hjTotalbg1 + hjTotalbg2) == 0 ? "" : FormatMoney(1, (hjTotalbg1 + hjTotalbg2) / unit);                   
        //        newdt.Rows.Add(hjRow);

        //    }
        //}
        /// <summary>
        /// 重新设置数据
        /// </summary>
        /// <param name="orgdt">原始数据</param>
        /// <param name="newdt">新创建的临时表</param>
        /// <returns></returns>
        public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit)
        {
            if (orgdt == null) return;
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            double totalbg1 = 0F;//基本支出
            double totalbg2 = 0F;//项目支出 
            //合计
            double hjTotalbg1 = 0F;//合计基本支出
            double hjTotalbg2 = 0F;//合计项目预算
            string pguid = string.Empty;//顶级GUID


            if (orgdt.Rows.Count > 0)
            {
                DataRow[] pdr = orgdt.Select(" pguid is null");
                foreach (DataRow row in pdr)
                {
                    pguid = row["guid"] == DBNull.Value ? "" : row["guid"].ToString();
                    totalbg1 = 0F;//基本支出
                    totalbg2 = 0F;//项目支出 
                    DataRow newRow = newdt.NewRow();
                    newRow["bgcodekey"] = row["bgcodekey"].ToString();
                    newRow["bgcodename"] = row["bgcodename"].ToString();
                    var pTotalbg1 = orgdt.Compute("Sum(totalbg1)", "pguid='" + pguid + "'");
                    pTotalbg1 = pTotalbg1 == DBNull.Value ? 0F : pTotalbg1;
                    double.TryParse(pTotalbg1.ToString(), out totalbg1);

                    var pTotalbg2 = orgdt.Compute("Sum(totalbg2)", "pguid='" + pguid + "'");
                    pTotalbg2 = pTotalbg2 == DBNull.Value ? 0F : pTotalbg2;
                    double.TryParse(pTotalbg2.ToString(), out totalbg2);
                    //基本支出
                    newRow["totalbg1"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
                    //项目支出                            
                    newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);
                    //预算合计
                    newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2) / unit);
                    newdt.Rows.Add(newRow);
                    //合计
                    hjTotalbg1 += totalbg1;//基本支出
                    hjTotalbg2 += totalbg2;//项目支出
                    DataRow[] childDr = orgdt.Select(" pguid='" + pguid + "'", " bgcodekey asc "); ;
                    if (childDr.Length > 0)
                    {
                        ReSetChildData(childDr, ref newdt, rmbUnit);
                    }

                }
                //合计计算
                DataRow hjRow = newdt.NewRow();
                hjRow["bgcodekey"] = "合计";
                hjRow["bgcodename"] = "";
                hjRow["totalbg1"] = hjTotalbg1 == 0F ? "" : FormatMoney(1, hjTotalbg1 / unit);
                hjRow["totalbg2"] = hjTotalbg2 == 0F ? "" : FormatMoney(1, hjTotalbg2 / unit);
                hjRow["totalbgall"] = (hjTotalbg1 + hjTotalbg2) == 0 ? "" : FormatMoney(1, (hjTotalbg1 + hjTotalbg2) / unit);
                newdt.Rows.Add(hjRow);

            }
        }
        /// <summary>
        /// 设置子数据
        /// </summary>
        /// <param name="drs"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        public void ReSetChildData(DataRow[] drs, ref DataTable newdt, string rmbUnit)
        {
            double totalbg1 = 0F;//基本支出
            double totalbg2 = 0F;//项目支出 
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            if (drs == null || drs.Length == 0) return;
            foreach (DataRow row in drs)
            {
                DataRow newRow = newdt.NewRow();
                newRow["bgcodekey"] = this.colChar + row["bgcodekey"].ToString();
                newRow["bgcodename"] = row["bgcodename"].ToString();
                //基本支出
                if (row["totalbg1"] != null)
                {
                    if (double.TryParse(row["totalbg1"].ToString(), out totalbg1))
                    {
                        //基本支出
                        newRow["totalbg1"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
                    }
                }
                //项目支出
                if (row["totalbg2"] != null)
                {
                    if (double.TryParse(row["totalbg2"].ToString(), out totalbg2))
                    {
                        //项目支出                            
                        newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);
                    }
                }
                //预算合计
                newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2) / unit);
                // 基本支出
                newRow["totalbg1"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
                //项目支出                            
                newRow["totalbg2"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);
                //预算合计
                newRow["totalbgall"] = (totalbg1 + totalbg2) == 0F ? "" : FormatMoney(1, (totalbg1 + totalbg2) / unit);

                newdt.Rows.Add(newRow);
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
            dt.Columns.Add("bgcodekey");//科目编号
            dt.Columns.Add("bgcodename");//科目编号
            dt.Columns.Add("totalbg1");//基本支出
            dt.Columns.Add("totalbg2");//预算支出
            dt.Columns.Add("totalbgall");//预算合计
           


            return dt;
        }
  
        //导出报表
        public string GetExportPath(DataTable data, out string fileName, out string message, ZZCHZBReportHead model)
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
                string filePath = ExportExcel.Export(data, this.tempalte, 11, 0, new List<ExcelCell>() { new ExcelCell(1, 8, model.DepartmentName), new ExcelCell(1, 9, model.Year), new ExcelCell(4, 9, model.RMBUnit) });
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
    /// <summary>
    /// 报表头文件
    /// </summary>
    public class ZZCHZBReportHead
    {
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { set; get; }
        /// <summary>
        /// 预算年度
        /// </summary>
        public string Year { set; get; }
        /// <summary>
        /// 货币单位
        /// </summary>
        public string RMBUnit { set; get; }
    }
}
