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
    /// 管理费用执行查询
    /// </summary>
    public class GLFYZXCX:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用的
        public string DepName { get; set; }//报表导出用
        public string Date { get; set; }//报表导出用
        public GLFYZXCX(string key):base(key)
        {
          
        }
        public override void Init()
        {
            this.SqlFormat = " select a.pguid,a.bgcodekey,a.bgcodename,b.totalbg,c.totalbx,a.guid from ss_bgcode a "
           + " left join "
           + " (select guid_BGcode,sum(total_BG) as totalbg "
           + "  from bg_detailview "
           + "  where bgyear='{0}' "//Year
           + "  and guid_item in(select guid from bg_item where bgitemkey in ('02','15','18')) "
           + "  and guid_bg_main in(select guid from bg_mainview where isnull(Invalid,0)=1 "
           + "  and guid_department in ({1}) and bgstepkey in ({2})) "//{1}departmentGUID {2} BGStepKey
           + "  group by guid_bgcode) b on a.guid=b.guid_bgcode "
           + " left join "
           + " ( "
           + " select guid_bgcode,sum(total_bx) as totalbx from bx_detailview where  guid_department  in ({1})  "//departmentGUID
           + " and GUID_BX_Main in (select GUID from bx_mainView "
           + "              where  {3} and {4} and {5} and {6} and {7} and  DocDate>='{8}' and DocDate<='{9}' "
           + "             ) " //{3}BGResourceType ,{4}PayStatus ,{5}ApproveStatus",{6}HXStatus ,{7} CertificateStatus ,{8}StartDate,{9}EndDate
           + " and GUID_BX_Main not in (select GUID from bx_MainView where docState=9) "
           + " and guid_paymentnumber in(select guid from cn_paymentnumberview where isnull(projectkey,'')='' "
            //and isnull(isguoku,0)=1 "
           + " ) "
           + " group by guid_bgcode) c on a.guid=c.guid_bgcode  "
            + " where  isstop=0 and bgcodekey not like '05%' order by bgcodekey ";
           this.tempalte=Path.Combine(this.tempalte, "glfyzxcx.xls");        
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
            //预算来源
            conditionModel.BGResourceType = GetBGResourceType(conditionModel.BGResourceType);
            //起始日期
            if (!string.IsNullOrEmpty(conditionModel.StartDate) && conditionModel.StartDate != DateTime.MinValue.ToString())
            {
                DateTime.TryParse(conditionModel.StartDate, out StartDate);
            }
            //结束日期
            if (!string.IsNullOrEmpty(conditionModel.EndDate) && conditionModel.EndDate != DateTime.MaxValue.ToString())
            {
                DateTime.TryParse(conditionModel.EndDate, out EndDate);
            }  
            //付款状态

            conditionModel.PayStatus=GetPayStatus(conditionModel.PayStatus);
            //审批状态

            conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
            //核销状态

            conditionModel.HXStatus=GetHXStatus(conditionModel.HXStatus);
            //凭证状态

            conditionModel.CertificateStatus=GetCertificateStatus(conditionModel.CertificateStatus);
            return strsql = string.Format(strsql, 
                                          conditionModel.Year,
                                          departId,
                                          bgSetpId, 
                                          conditionModel.BGResourceType,
                                          conditionModel.PayStatus,
                                          conditionModel.ApproveStatus,
                                          conditionModel.HXStatus,
                                          conditionModel.CertificateStatus,
                                          StartDate,
                                          EndDate);
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
            if (orgdt == null) return;
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit)==false)
            {
                unit = 1;
            }
            double totalbg = 0F;//预算
            double totalbx = 0F;//执行数           
            //合计
            double hjTotalbg = 0F;//合计预算
            double hjTotalbx = 0F;//合计执行数
            if (orgdt.Rows.Count > 0)
            {
              
                #region 预算
                DataRow[] pdr = orgdt.Select(" pguid is null");
                string pguid = string.Empty;
                foreach (DataRow row in pdr)
                {
                    
                    pguid = row["guid"].ToString();
                    totalbg = 0F;//预算
                    totalbx = 0F;//执行数
                    DataRow newRow = newdt.NewRow();
                    newRow["guid"] = row["guid"].ToString();
                    newRow["bgcodekey"] = row["bgcodekey"].ToString();
                    newRow["bgcodename"] = row["bgcodename"].ToString();
                    var sumTotalbg = orgdt.Compute("Sum(totalbg)", "pguid='" + pguid + "'");
                    sumTotalbg = sumTotalbg == DBNull.Value ? 0F : sumTotalbg;
                    var sumTotalbx = orgdt.Compute("Sum(totalbx)", "pguid='" + pguid + "'") ;
                    sumTotalbx = sumTotalbx == DBNull.Value ? 0F : sumTotalbx;
                    totalbg = orgdt == null ? 0F : double.Parse(sumTotalbg.ToString());
                    totalbx = orgdt == null ? 0F : double.Parse(sumTotalbx.ToString());

                    newRow["totalbg"] = totalbg == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbg / unit);
                    newRow["totalbx"] = totalbx == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbx / unit);

                    newRow["jy"] = (totalbg - totalbx) == 0 ? "" : FormatMoney(1, (totalbg - totalbx) / unit);//预算数-执行数；
                    newRow["jybl"] = ((totalbg - totalbx) == 0F || totalbg == 0) ? "" : FormatMoney(1, ((totalbg - totalbx) / totalbg) * 100) + "%";//结余/预算数，以百分比数字显示
                    newRow["wcl"] = (totalbx == 0F || totalbg == 0) ? "" : FormatMoney(1, (totalbx / totalbg) * 100) + "%";//执行数/预算数，以百分比数字显示

                    hjTotalbg += totalbg;//合计预算
                    hjTotalbx += totalbx;//合计执行数   

                    newdt.Rows.Add(newRow);

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
                hjRow["totalbg"] = hjTotalbg == 0F ? "" : FormatMoney(1, hjTotalbg / unit);
                hjRow["totalbx"] = hjTotalbx == 0F ? "" : FormatMoney(1, hjTotalbx / unit);
                hjRow["jy"] = (hjTotalbg - hjTotalbx) == 0 ? "" : FormatMoney(1, (hjTotalbg - hjTotalbx) / unit);
                hjRow["jybl"] = ((hjTotalbg - hjTotalbx) == 0F || hjTotalbg == 0) ? "" : FormatMoney(1, ((hjTotalbg - hjTotalbx) / hjTotalbg) * 100) + "%";//结余/预算数，以百分比数字显示
                hjRow["wcl"] = (hjTotalbx == 0F || hjTotalbg == 0) ? "" : FormatMoney(1, (hjTotalbx / hjTotalbg) * 100) + "%";//执行数/预算数，以百分比数字显示
                newdt.Rows.Add(hjRow);

                #endregion
            }

            #region 原数据
            //if (orgdt.Rows.Count > 0)
            //{
            //    foreach (DataRow row in orgdt.Rows)
            //    {
            //         totalbg = 0F;//预算
            //         totalbx = 0F;//执行数
            //        DataRow newRow=newdt.NewRow();
            //        newRow["bgcodekey"] =this.colChar + row["bgcodekey"].ToString();
            //        newRow["bgcodename"] = row["bgcodename"].ToString();
            //        if (row["totalbg"] != null)
            //        {
            //            if (double.TryParse(row["totalbg"].ToString(), out totalbg))
            //            {
            //                if (totalbg == 0F)
            //                {
            //                    newRow["totalbg"] = DBNull.Value;
            //                }
            //                else
            //                {
            //                    newRow["totalbg"] = FormatMoney(1, totalbg / unit);
            //                }
            //            }                       
            //        }
                   
            //        if (row["totalbx"] != null)
            //        {
            //            //计算顶级项

            //            if (row["bgcodekey"] != null && row["bgcodekey"].ToString().Length == 2)
            //            {
            //                double pTotalbx = 0F;
            //                DataRow[] dr = orgdt.Select(" bgcodekey like '" + row["bgcodekey"].ToString() + "%' and len(bgcodekey)<>2 ");
            //                if (dr.Length > 0)
            //                {                               
            //                    foreach (DataRow r in dr)
            //                    {
            //                        if (double.TryParse(r["totalbx"].ToString(), out pTotalbx))
            //                        {
            //                            if (pTotalbx != 0F)
            //                            {
            //                                totalbx += pTotalbx;
            //                            }                                       
            //                        }
            //                    }
            //                }
            //                if (totalbx == 0F)
            //                {
            //                    newRow["totalbx"] = DBNull.Value;
            //                }
            //                else
            //                {
            //                    newRow["totalbx"] = FormatMoney(1, totalbx / unit);
            //                }
            //                newRow["bgcodekey"] =row["bgcodekey"].ToString();
            //            }
            //            else
            //            {
            //                if (double.TryParse(row["totalbx"].ToString(), out totalbx))
            //                {
            //                    if (totalbx == 0F)
            //                    {
            //                        newRow["totalbx"] = DBNull.Value;
            //                    }
            //                    else
            //                    {
            //                        newRow["totalbx"] = FormatMoney(1, totalbx / unit);
            //                    }
            //                }
                           
            //            }
            //        }
            //        newRow["jy"] = (totalbg - totalbx) == 0 ? "" : FormatMoney(1, (totalbg - totalbx) / unit);//预算数-执行数；
            //        newRow["jybl"] = ((totalbg - totalbx)==0F ||totalbg == 0F) ? "" : FormatMoney(1, ((totalbg - totalbx) / totalbg) * 100) + "%";//结余/预算数，以百分比数字显示
            //        newRow["wcl"] = (totalbx ==0F|| totalbg == 0F)? "" : FormatMoney(1, (totalbx / totalbg) * 100) + "%";//执行数/预算数，以百分比数字显示
            //        //合计计算
            //        if (row["bgcodekey"] != null && row["bgcodekey"].ToString().Length != 2)
            //        {
            //            hjTotalbg += totalbg;//合计预算
            //            hjTotalbx += totalbx;//合计执行数                        
            //        }
            //        newdt.Rows.Add(newRow);
            //    }
            //    //合计计算
            //    DataRow hjRow = newdt.NewRow();
            //    hjRow["bgcodekey"] = "合计";
            //    hjRow["bgcodename"] = "";
            //    hjRow["totalbg"] = hjTotalbg == 0F ? "" : FormatMoney(1, hjTotalbg / unit);
            //    hjRow["totalbx"] = hjTotalbx == 0F ? "" : FormatMoney(1,hjTotalbx/unit);
            //    hjRow["jy"] = (hjTotalbg - hjTotalbx) == 0 ? "" : FormatMoney(1, (hjTotalbg - hjTotalbx) / unit);
            //    hjRow["jybl"] = ((hjTotalbg - hjTotalbx)==0F || hjTotalbg == 0) ? "" : FormatMoney(1, ((hjTotalbg - hjTotalbx) / hjTotalbg) * 100) + "%";//结余/预算数，以百分比数字显示
            //    hjRow["wcl"] = (hjTotalbx==0F ||hjTotalbg == 0) ? "" : FormatMoney(1, (hjTotalbx / hjTotalbg) * 100) + "%";//执行数/预算数，以百分比数字显示
            //    newdt.Rows.Add(hjRow);
            //}
            #endregion
        }
        /// <summary>
        /// 设置科目子数据
        /// </summary>
        /// <param name="orgdt"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        public void ReSetChildData(DataRow[] rows, ref DataTable newdt, string rmbUnit)
        {
            double totalbg = 0F;//预算
            double totalbx = 0F;//执行数
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            if (rows.Length > 0)
            {
                foreach (DataRow row in rows)
                {
                    totalbg = 0F;//预算
                    totalbx = 0F;//执行数
                    DataRow newRow = newdt.NewRow();
                    newRow["guid"] = row["guid"].ToString();
                    newRow["bgcodekey"] = "&nbsp&nbsp&nbsp" + row["bgcodekey"].ToString();
                    newRow["bgcodename"] = row["bgcodename"].ToString();
                    if (row["totalbg"] != null)
                    {
                        if (double.TryParse(row["totalbg"].ToString(), out totalbg))
                        {
                            newRow["totalbg"] = totalbg == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbg / unit);
                        }
                    }
                    if (row["totalbx"] != null)
                    {
                        if (double.TryParse(row["totalbx"].ToString(), out totalbx))
                        {
                            newRow["totalbx"] = totalbx == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbx / unit);
                        }
                    }
                    newRow["jy"] = (totalbg - totalbx) == 0 ? "" : FormatMoney(1, (totalbg - totalbx) / unit);//预算数-执行数；
                    newRow["jybl"] = ((totalbg - totalbx) == 0F || totalbg == 0) ? "" : FormatMoney(1, ((totalbg - totalbx) / totalbg) * 100) + "%";//结余/预算数，以百分比数字显示
                    newRow["wcl"] = (totalbx == 0F || totalbg == 0) ? "" : FormatMoney(1, (totalbx / totalbg) * 100) + "%";//执行数/预算数，以百分比数字显示
                    newdt.Rows.Add(newRow);
                }
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
            dt.Columns.Add("guid");//科目编号
            dt.Columns.Add("bgcodekey");//科目编号
            dt.Columns.Add("bgcodename");//科目编号
            dt.Columns.Add("totalbg");//科目编号
            dt.Columns.Add("totalbx");//科目编号
            dt.Columns.Add("jy");//结余
            dt.Columns.Add("jybl");//结余比率
            dt.Columns.Add("wcl");//完成率

            return dt;
        }
  
        //导出报表
        public override string GetExportPath(DataTable data, out string fileName, out string message)
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
                data.Columns.RemoveAt(0);
                string filePath = ExportExcel.Export(data, this.tempalte, 6,0, new List<ExcelCell>() { new ExcelCell(1,4,this.DepName),new ExcelCell(4,4,this.Date)});
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
