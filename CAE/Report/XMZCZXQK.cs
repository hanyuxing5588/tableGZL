using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Infrastructure;
using Business.CommonModule;
using Business.Common;

namespace CAE.Report
{
  
  /// <summary>
  /// 项目支出执行情况
  /// </summary>
  public class XMZCZXQK : BaseReport
  {
      IntrastructureFun db = new IntrastructureFun();
      public XMZCZXQK(string key): base(key)          
      {

      }
      public override void Init()
      {
          this.SqlFormat = "select a.guid,a.pguid,a.bgcodekey,a.bgcodename,b.totalbg,c.totalbx from ss_bgcode a "
                        + "left join( "
                        + "select guid_BGcode,sum(total_BG) as totalbg from bg_detailview where bgyear='{0}' and "
                        + "guid_item in(select guid from bg_item where bgitemkey='08') "
                        + "and guid_bg_main in(select guid from bg_mainview where isnull(Invalid,0)=1 "
                        + "and guid_project in ({1}) and bgstepkey in ({2}) ) "
                        + "group by guid_bgcode) b on a.guid=b.guid_bgcode "
                        + "left join (select guid_bgcode,sum(total_bx) as totalbx from bx_detailview where   "
                        + "guid_project in ({1})  "
                        + "and guid_bx_main in(select guid from bx_main where   {3} and {4} and {5} and {6} and {7} "
                        + "and  DocDate>='{8}' and DocDate<='{9}')  "
                        + "and guid_bx_main not in (select GUID from bx_main where docState=9)  "
                        + "group by guid_bgcode) c on a.guid=c.guid_bgcode  "
                        + " where  bgcodekey not like '03%' and bgcodekey not like '05%' and isstop=0  order by bgcodekey ";
          this.tempalte = Path.Combine(this.tempalte, "czzczxqk.xls");
      }
     
      /// <summary>
      /// 获取拼接的Sql
      /// </summary>
      /// <param name="condition"></param>
      /// <returns></returns>
      public string GetSql(BBSearchCondition conditionModel)
      {

          string strsql = this.SqlFormat;
          string proId = string.Empty;
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
                  case "ss_project":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      proId = GetByProjectId(conditionModel.treeValue.ToString(),1,conditionModel.Year); //1表示项目
                      break;
                  case "bg_setup":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      bgSetpId = GetStepKey(conditionModel.treeValue.ToString());
                      break;                  
                  case "ss_projectclass":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      proId = GetByProjectId(conditionModel.treeValue.ToString(),2,conditionModel.Year);// 1表示项目 2表示项目分类                      
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
                      case "ss_project":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                         proId = GetByProjectId(item.treeValue.ToString(),1,conditionModel.Year);//1表示项目 2表示项目分类    
                          break;
                      case "bg_setup":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                          bgSetpId = GetStepKey(item.treeValue);
                          break;
                      case "ss_projectclass":
                          if (conditionModel.treeValue == null)
                          {
                              conditionModel.treeValue = Guid.Empty;
                          }
                          proId = GetByProjectId(item.treeValue.ToString(), 2,conditionModel.Year);// 1表示项目 2表示项目分类    
                          break;
                  }
              }
          }
          if (string.IsNullOrEmpty(proId))
          {
              proId = GetByProjectId(Guid.Empty.ToString(), 1, conditionModel.Year);
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

          conditionModel.PayStatus = GetPayStatus(conditionModel.PayStatus);
          //审批状态

          conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
          //核销状态

          conditionModel.HXStatus = GetHXStatus(conditionModel.HXStatus);
          //凭证状态

          conditionModel.CertificateStatus = GetCertificateStatus(conditionModel.CertificateStatus);
          return strsql = string.Format(strsql,
                                        conditionModel.Year,
                                        proId,
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
          if (int.TryParse(rmbUnit, out unit) == false)
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
                  newRow["bgcodekey"] =row["bgcodekey"].ToString();
                  newRow["bgcodename"] = row["bgcodename"].ToString();

                  var sumTotalbg = orgdt.Compute("Sum(totalbg)", "pguid='" + pguid + "'");
                  sumTotalbg = sumTotalbg == DBNull.Value ? 0F : sumTotalbg;
                  var sumTotalbx = orgdt.Compute("Sum(totalbx)", "pguid='" + pguid + "'");
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
                      ReSetChildData(childDr,ref newdt,rmbUnit);
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
      }
      /// <summary>
      /// 设置科目子数据
      /// </summary>
      /// <param name="orgdt"></param>
      /// <param name="newdt"></param>
      /// <param name="rmbUnit"></param>
      public void ReSetChildData(DataRow[] rows,ref DataTable newdt, string rmbUnit)
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
            foreach(DataRow row in rows)
            {
                totalbg = 0F;//预算
                totalbx = 0F;//执行数
                DataRow newRow = newdt.NewRow();
                newRow["bgcodekey"] = "&nbsp&nbsp&nbsp" + row["bgcodekey"].ToString();
                newRow["bgcodename"] = row["bgcodename"].ToString();
                if (row["totalbg"] != null)
                {
                    if (double.TryParse(row["totalbg"].ToString(), out totalbg))
                    {
                        newRow["totalbg"] = totalbg == 0F ? DBNull.Value.ToString(): FormatMoney(1, totalbg / unit);                        
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
          var dt = DataSource.ExecuteQuery(sql);
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
          dt.Columns.Add("totalbg");//科目编号
          dt.Columns.Add("totalbx");//科目编号
          dt.Columns.Add("jy");//结余
          dt.Columns.Add("jybl");//结余比率
          dt.Columns.Add("wcl");//完成率
          return dt;
      }
      /// <summary>
      /// 导出报表
      /// </summary>
      /// <param name="data"></param>
      /// <param name="fileName"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public string GetExportPath(DataTable data, out string fileName, out string message,ReportHead reportHeadModel)
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
              string filePath = ExportExcel.Export(data, this.tempalte, 6, 0, new List<ExcelCell>() { new ExcelCell(1, 1, reportHeadModel.ProjectKey), new ExcelCell(3, 1, reportHeadModel.ProjectName), new ExcelCell(1, 2, reportHeadModel.Year), new ExcelCell(6, 2, reportHeadModel.RMBUnit) });
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
   /// 报表头信息
   /// </summary>
  public class ReportHead
  {
      /// <summary>
      /// 项目Key
      /// </summary>
      public string ProjectKey { set; get; }
      /// <summary>
      /// 项目名称
      /// </summary>
      public string ProjectName { set; get; }
      /// <summary>
      ///预算年度
      /// </summary>
      public string Year { set; get; }
      /// <summary>
      /// 金额单位
      /// </summary>
      public string RMBUnit { set; get; }
  }

    //项目管理费用执行情况表(按项目)
  public class XMGLFYFORXM : BaseReport
  {
      IntrastructureFun db = new IntrastructureFun();
      public XMGLFYFORXM(string key)
          : base(key)
      {

      }
      public override void Init()
      {
          this.SqlFormat = @"select a.guid,a.pguid,a.bgcodekey,a.bgcodename,b.totalbg,c.totalbx from ss_bgcode a 
                      left join( 
                      select guid_BGcode,sum(total_BG) as totalbg from bg_detailview where bgyear='{0}' and 
                      guid_item in(select guid from bg_item where bgitemkey='08') 
                      and guid_bg_main in(select guid from bg_mainview where isnull(Invalid,0)=1 
                      and guid_project in ({1}) and bgstepkey in ({2}) ) 
                      group by guid_bgcode) b on a.guid=b.guid_bgcode 
                      left join (select guid_bgcode,sum(total_bx) as totalbx from bx_detailview where 
                          GUID_PaymentNumber IN (
        SELECT  GUID
        FROM    dbo.CN_PaymentNumber
        WHERE   GUID_ProjectEx in ({1}) )
                      and guid_bx_main in(select guid from bx_main where   {3} and {4} and {5} and {6} and {7} 
                      and  DocDate>='{8}' and DocDate<='{9}') 
                      and guid_bx_main not in (select GUID from bx_main where docState=9) 
                      group by guid_bgcode) c on a.guid=c.guid_bgcode 
                       where  bgcodekey not like '03%' and bgcodekey not like '05%' and isstop=0  order by bgcodekey ";
          this.tempalte = Path.Combine(this.tempalte, "XMGLFYFORXM.xls");
      }
      public string GetProClassEnd(int year, Guid ProjectClassID)
      {
          var projectclasses = new List<SS_ProjectClass>();
          var projectClassList = db.GetProjectClass(true, this.OperatorKey, year);
          var obj = projectClassList.FirstOrDefault(e => e.GUID == ProjectClassID);
          if (obj == null) return ProjectClassID.ToString(); ;
          obj.RetrieveLeafs(projectClassList, ref projectclasses);
          if (projectclasses.Count == 0) return ProjectClassID.ToString();
          return projectclasses.Select(e => e.GUID).ToList().GetStrGUIDS(); ;
      }
      /// <summary>
      /// 获取拼接的Sql
      /// </summary>
      /// <param name="condition"></param>
      /// <returns></returns>
      public string GetSql(BBSearchCondition conditionModel)
      {

          string strsql = this.SqlFormat;
          string proId = string.Empty;
          string bgSetpId = string.Empty;
          if (conditionModel.Year == "0")
          {
              conditionModel.Year = "";
          }
          //开始时间
          var guidExs = "";
          DateTime StartDate = DateTime.MinValue;
          ///// 结束时间
          DateTime EndDate = DateTime.MaxValue;
          if (!string.IsNullOrEmpty(conditionModel.treeModel) && (conditionModel.treeValue != null && conditionModel.treeValue != Guid.Empty))
          {
              switch (conditionModel.treeModel.ToLower())
              {
                  case "ss_project":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                    
                      proId = GetByProjectId(conditionModel.treeValue.ToString(), 1, conditionModel.Year); //1表示项目
                      break;
                  case "bg_setup":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      bgSetpId = GetStepKey(conditionModel.treeValue.ToString());
                      break;
                  case "ss_projectclass":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                  
                      proId = GetByProjectId(conditionModel.treeValue.ToString(), 2, conditionModel.Year);// 1表示项目 2表示项目分类                      
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
                      case "ss_project":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                          guidExs = conditionModel.treeValue.ToString();
                          proId = GetByProjectId(item.treeValue.ToString(), 1, conditionModel.Year);//1表示项目 2表示项目分类    
                          break;
                      case "bg_setup":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                          bgSetpId = GetStepKey(item.treeValue);
                          break;
                      case "ss_projectclass":
                          if (conditionModel.treeValue == null)
                          {
                              conditionModel.treeValue = Guid.Empty;
                          }
                          var tempClass = GetProClassEnd(int.Parse(conditionModel.Year),new Guid( item.treeValue));
                          proId = GetByProjectId(item.treeValue.ToString(), 2, conditionModel.Year);// 1表示项目 2表示项目分类    
                          break;
                  }
              }
          }
          if (string.IsNullOrEmpty(proId))
          {
              proId = GetByProjectId(Guid.Empty.ToString(), 1, conditionModel.Year);
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


          conditionModel.PayStatus = GetPayStatus(conditionModel.PayStatus);
          //审批状态


          conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
          //核销状态


          conditionModel.HXStatus = GetHXStatus(conditionModel.HXStatus);
          //凭证状态


          conditionModel.CertificateStatus = GetCertificateStatus(conditionModel.CertificateStatus);
          return strsql = string.Format(strsql,
                                        conditionModel.Year,
                                        proId,
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
          if (int.TryParse(rmbUnit, out unit) == false)
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
                  newRow["bgcodekey"] = row["bgcodekey"].ToString();
                  newRow["bgcodename"] = row["bgcodename"].ToString();

                  var sumTotalbg = orgdt.Compute("Sum(totalbg)", "pguid='" + pguid + "'");
                  sumTotalbg = sumTotalbg == DBNull.Value ? 0F : sumTotalbg;
                  var sumTotalbx = orgdt.Compute("Sum(totalbx)", "pguid='" + pguid + "'");
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
          var dt = DataSource.ExecuteQuery(sql);
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
          dt.Columns.Add("totalbg");//科目编号
          dt.Columns.Add("totalbx");//科目编号
          dt.Columns.Add("jy");//结余
          dt.Columns.Add("jybl");//结余比率
          dt.Columns.Add("wcl");//完成率

          return dt;
      }
      /// <summary>
      /// 导出报表
      /// </summary>
      /// <param name="data"></param>
      /// <param name="fileName"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public string GetExportPath(DataTable data, out string fileName, out string message, ReportHead reportHeadModel)
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
              string filePath = ExportExcel.Export(data, this.tempalte, 6, 0, new List<ExcelCell>() { new ExcelCell(1, 1, reportHeadModel.ProjectKey), new ExcelCell(3, 1, reportHeadModel.ProjectName), new ExcelCell(1, 2, reportHeadModel.Year), new ExcelCell(6, 2, reportHeadModel.RMBUnit) });
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
    //项目管理费用执行情况表(按科目)

  public class XMGLFYFORKM : BaseReport
  {
      IntrastructureFun db = new IntrastructureFun();
      public string SqlFormatView { get; set; }
      public XMGLFYFORKM(string key)
          : base(key)
      {

      }
      public override void Init()
      {
          this.SqlFormat = @"select  SUM(ISNULL(b.totalbg ,0)) as totalbg,SUM(ISNULL(c.totalbx ,0)) as totalbx from ss_bgcode a 
                      left join( 
                      select guid_BGcode,sum(total_BG) as totalbg from bg_detailview where bgyear='{0}'   AND BGCodeKey in('{10}') and 
                      guid_item in(select guid from bg_item where bgitemkey='04') 
                      and guid_bg_main in(select guid from bg_mainview where isnull(Invalid,0)=1 
                      and guid_project in ({1}) and bgstepkey in ({2}) ) 
                      group by guid_bgcode) b on a.guid=b.guid_bgcode 
                      left join (select guid_bgcode,sum(total_bx) as totalbx from bx_detailview where 
                      GUID_PaymentNumber IN (
        SELECT  GUID
        FROM    dbo.CN_PaymentNumber
        WHERE   GUID_ProjectEx = '{11}' ) 
                      AND BGCodeKey in( '{10}')
                       and guid_bx_main in(select guid from bx_main where   {3} and {4} and {5} and {6} and {7} 
                      and  DocDate>='{8}' and DocDate<='{9}') 
                      and guid_bx_main not in (select GUID from bx_main where docState=9) 
                      group by guid_bgcode) c on a.guid=c.guid_bgcode 
                       where  bgcodekey not like '03%' and bgcodekey not like '05%' and isstop=0  AND LEN(a.BGCodeKey)>2  AND BGCodeKey in('{10}')   GROUP BY  a.PGUID ";
          this.SqlFormatView = @"SELECT  GUID ,
        PGUID ,
        ProjectName ,
        ProjectKey ,
        GUID_FunctionClass ,
        GUID_ProjectClass ,
        IsFinance
FROM    SS_ProjcetExView
WHERE   PGUID IS NULL
        AND IsFinance = 1
        AND IsStop = 0
        AND StopYear IS NULL
ORDER BY ProjectKey";
          this.tempalte = Path.Combine(this.tempalte, "XMGLFYFORKM.xls");
      }

      /// <summary>
      /// 获取拼接的Sql
      /// </summary>
      /// <param name="condition"></param>
      /// <returns></returns>
      public string GetSql(BBSearchCondition conditionModel,string guid_ProjectEx)
      {

          string strsql = this.SqlFormat;
          string proId = string.Empty;
          string bgSetpId = string.Empty;
          string bgCodeKey = "";
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
              
                  case "ss_project":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      proId = GetByProjectId(conditionModel.treeValue.ToString(), 1, conditionModel.Year); //1表示项目
                      break;
                  case "bg_setup":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      bgSetpId = GetStepKey(conditionModel.treeValue.ToString());
                      break;
                  case "ss_projectclass":
                      if (conditionModel.treeValue == null)
                      {
                          conditionModel.treeValue = Guid.Empty;
                      }
                      proId = GetByProjectId(conditionModel.treeValue.ToString(), 2, conditionModel.Year);// 1表示项目 2表示项目分类                      
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
                      case "ss_code":
                          bgCodeKey = item.treeValue;
                          break;
                      case "ss_project":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                          proId = GetByProjectId(item.treeValue.ToString(), 1, conditionModel.Year);//1表示项目 2表示项目分类    
                          break;
                      case "bg_setup":
                          if (item.treeValue == null)
                          {
                              item.treeValue = Guid.Empty.ToString();
                          }
                          bgSetpId = GetStepKey(item.treeValue);
                          break;
                      case "ss_projectclass":
                          if (conditionModel.treeValue == null)
                          {
                              conditionModel.treeValue = Guid.Empty;
                          }
                          proId = GetByProjectId(item.treeValue.ToString(), 2, conditionModel.Year);// 1表示项目 2表示项目分类    
                          break;
                  }
              }
          }
          if (string.IsNullOrEmpty(proId))
          {
              proId = GetByProjectId(Guid.Empty.ToString(), 1, conditionModel.Year);
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


          conditionModel.PayStatus = GetPayStatus(conditionModel.PayStatus);
          //审批状态


          conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
          //核销状态


          conditionModel.HXStatus = GetHXStatus(conditionModel.HXStatus);
          //凭证状态


          conditionModel.CertificateStatus = GetCertificateStatus(conditionModel.CertificateStatus);
          return strsql = string.Format(strsql,
                                        conditionModel.Year,
                                        proId,
                                        bgSetpId,
                                        conditionModel.BGResourceType,
                                        conditionModel.PayStatus,
                                        conditionModel.ApproveStatus,
                                        conditionModel.HXStatus,
                                        conditionModel.CertificateStatus,
                                        StartDate,
                                        EndDate,bgCodeKey.Replace(",","','"),guid_ProjectEx);
      }
      /// <summary>
      /// 加载数据
      /// </summary>
      /// <param name="conditions"></param>
      /// <param name="msgError"></param>
      /// <returns></returns>
      public DataTable GetReport(SearchCondition conditions, out string msgError)
      {
          DataTable newdt = CreateNewDataTable();
          
          msgError = "";
          try
          {

         
          BBSearchCondition conditionModel = (BBSearchCondition)conditions;
          
          //sqlStr = GetSql(conditionModel);
          DataTable orgdt = LoadData(this.SqlFormatView, ref msgError);
        
          ReSetData(orgdt, ref newdt, conditionModel.RMBUnit,  conditionModel);
          }
          catch (Exception ex)
          {

              throw ex;
          }
          return newdt;
      }

      public IList<string> GetChangeStr(string str) {
          return str.Split(',').ToList();
      }
      /// <summary>
      /// 重新设置数据
      /// </summary>
      /// <param name="orgdt">原始数据</param>
      /// <param name="newdt">新创建的临时表</param>
      /// <returns></returns>
      public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit, BBSearchCondition bbSearchCondition)
      {
          if (orgdt == null) return;
          int unit = 1;
          if (int.TryParse(rmbUnit, out unit) == false)
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
              var ArrSearch = GetChangeStr(bbSearchCondition.ProjectKeys);
              string pguid = string.Empty;
              foreach (DataRow row in orgdt.Rows)
              {
                  if (!ArrSearch.Contains(row["ProjectKey"].ToString())) {
                      continue;
                  }
                  string msgError = "";
                  
                  pguid = row["guid"].ToString();
                  bbSearchCondition.TreeNodeList.Add(new TreeNode() { treeModel = "ss_project", treeValue =pguid});
                  var sqlStr = GetSql(bbSearchCondition,pguid);
                  DataTable orgdt1 = LoadData(sqlStr, ref msgError);
                  totalbg = 0F;//预算
                  totalbx = 0F;//执行数

                  DataRow newRow = newdt.NewRow();
                  newRow["ProjectName"] = row["ProjectName"].ToString();
                  newRow["ProjectKey"] = row["ProjectKey"].ToString();
                 // newRow["ProjectID"] = row["GUID"].ToString();

                  totalbg = orgdt1 == null || orgdt1.Rows.Count <= 0 ? 0F : double.Parse(orgdt1.Rows[0][0] + "".ToString());
                  totalbx = orgdt1 == null || orgdt1.Rows.Count <= 0 ? 0F : double.Parse(orgdt1.Rows[0][1] + "".ToString());

                   newRow["totalbg"] = totalbg == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbg / unit);
                  newRow["totalbx"] = totalbx == 0F ? DBNull.Value.ToString() : FormatMoney(1, totalbx / unit);

                  newRow["jy"] = (totalbg - totalbx) == 0 ? "" : FormatMoney(1, (totalbg - totalbx) / unit);//预算数-执行数；
                  newRow["jybl"] = ((totalbg - totalbx) == 0F || totalbg == 0) ? "" : FormatMoney(1, ((totalbg - totalbx) / totalbg) * 100) + "%";//结余/预算数，以百分比数字显示
                  newRow["wcl"] = (totalbx == 0F || totalbg == 0) ? "" : FormatMoney(1, (totalbx / totalbg) * 100) + "%";//执行数/预算数，以百分比数字显示

                  hjTotalbg += totalbg;//合计预算
                  hjTotalbx += totalbx;//合计执行数   

                  newdt.Rows.Add(newRow);

                  //DataRow[] childDr = orgdt.Select(" pguid='" + pguid + "'", " bgcodekey asc "); ;
                  //if (childDr.Length > 0)
                  //{
                  //    ReSetChildData(childDr, ref newdt, rmbUnit);
                  //}


              }

              //合计计算
              DataRow hjRow = newdt.NewRow();
              hjRow["ProjectKey"] = "合计";
              hjRow["ProjectName"] = "";
              hjRow["totalbg"] = hjTotalbg == 0F ? "" : FormatMoney(1, hjTotalbg / unit);
              hjRow["totalbx"] = hjTotalbx == 0F ? "" : FormatMoney(1, hjTotalbx / unit);
              hjRow["jy"] = (hjTotalbg - hjTotalbx) == 0 ? "" : FormatMoney(1, (hjTotalbg - hjTotalbx) / unit);
              hjRow["jybl"] = ((hjTotalbg - hjTotalbx) == 0F || hjTotalbg == 0) ? "" : FormatMoney(1, ((hjTotalbg - hjTotalbx) / hjTotalbg) * 100) + "%";//结余/预算数，以百分比数字显示
              hjRow["wcl"] = (hjTotalbx == 0F || hjTotalbg == 0) ? "" : FormatMoney(1, (hjTotalbx / hjTotalbg) * 100) + "%";//执行数/预算数，以百分比数字显示
              newdt.Rows.Add(hjRow);


          }
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
                  newRow["ProjectKey"] = "&nbsp&nbsp&nbsp" + row["ProjectKey"].ToString();
                  newRow["ProjectName"] = row["ProjectName"].ToString();
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
          var dt = DataSource.ExecuteQuery(sql);
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
          dt.Columns.Add("ProjectName");//科目编号
          dt.Columns.Add("ProjectKey");//科目编号
          dt.Columns.Add("totalbg");//科目编号
          dt.Columns.Add("totalbx");//科目编号
          dt.Columns.Add("jy");//结余
          dt.Columns.Add("jybl");//结余比率
          dt.Columns.Add("wcl");//完成率
          dt.Columns.Add("ProjectID");//科目编号

          return dt;
      }
      /// <summary>
      /// 导出报表
      /// </summary>
      /// <param name="data"></param>
      /// <param name="fileName"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public string GetExportPath(DataTable data, out string fileName, out string message, ReportHead reportHeadModel)
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
              string filePath = ExportExcel.Export(data, this.tempalte, 6, 0, new List<ExcelCell>() { new ExcelCell(1, 1, reportHeadModel.ProjectKey), new ExcelCell(3, 1, reportHeadModel.ProjectName), new ExcelCell(1, 2, reportHeadModel.Year), new ExcelCell(6, 2, reportHeadModel.RMBUnit) });
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
