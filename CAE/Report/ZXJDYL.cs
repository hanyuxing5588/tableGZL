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
  /// 执行进度一览
  /// </summary>
  public class ZXJDYL1 : BaseReport
  {
      IntrastructureFun db = new IntrastructureFun();
      public ZXJDYL1(string key)
          : base(key)          
      {

      }
      string OneLevel = string.Empty;//项目一级
      string TwoLevel = string.Empty;//项目二级
      string ThreeLevel = string.Empty;//项目三级
      string FourLevel = string.Empty;//项目四级
      string FiveLevel = string.Empty;//项目五级
      public override void Init()
      {
          //{0}DepartmentKey 部门 {1}ProjectKey 项目Key {2}Year 年 {3}StartMonth 月的开始时间 {4} EndMonth 结束月时间 {5}预算步骤 {6}预算来源 {7}审批状态 {8}核销状态 {9}记账状态 {10}科目级次
          this.SqlFormat = "Select Project.GUID,Project.PGUID,Project.ProjectName,Project.ProjectKey,isnull(PBG.PersonName,'') as PersonName,isnull(BX.iBXTotal,0) as iBXTotal,isnull(PBG.iBGTotalSum,0) as iBGTotalSum,isnull(PBG.iBGTotalXD,0) as iBGTotalXD From SS_Project Project "
                           + " Left Join ( "
                           + " Select GUID_Project,Sum(Total_BX) As iBXTotal  From BX_DetailView "
                           + " Where DepartmentKey In({0}) "//{0}--DepartmentKey
                           + " And {6} " //--GUID_PaymentNumber In(Select GUID from CN_PaymentNumberView Where isnull(BGSourceKey,1) In('1','2')) 
                           + " And GUID_BX_Main In(Select GUID From BX_MainView Where  {7}  and  {8}  and  {9}  and Year(DocDate)='{2}' And Month(DocDate)>='{3}'  And Month(DocDate)<='{4}') 	"
                           + " And GUID_BX_Main not in (select GUID from BX_MainView where docState ='9') Group By GUID_Project "
                           + " ) BX On Project.GUID=BX.GUID_Project "
                           + " left join( "
                           + "     Select Project.GUID,isnull(BG.PersonName,'') as PersonName,isnull(BG.iBGTotalSum,0) as iBGTotalSum,isnull(BG.iBGTotalXD,0) as iBGTotalXD ,BG.guid as bgguid From SS_Project Project "
                           + "     Left Join ( "
                           + "         Select BGMain.guid,BGMain.GUID_Project,BGMain.ProjectName,BGMain.ProjectKey,BGMain.PersonName,BGTotal.iBGTotalSum,BGXD.iBGTotalXD From BG_MainView BGMain "
                           + "         Left Join ( "
                           + "         Select GUID_BG_Main,Sum(Total_BG) As iBGTotalSum From BG_DetailView "
                           + "         Where len(bgcodekey)=2 and BGItemKey in ('07','08') And BGYear ='{2}'  Group By GUID_BG_Main "
                           + "         ) BGTotal On BGMain.GUID=BGTotal.GUID_BG_Main "
                           + "         Left Join ( "
                           + "         Select GUID_BG_Main,Sum(Total_BG) As iBGTotalXD From BG_DetailView Where len(bgcodekey)=2 and BGItemKey in ('07','08') And BGYear ='{2}'  Group By GUID_BG_Main "
                           + "         ) BGXD On BGMain.GUID=BGXD.GUID_BG_Main "
                           + "         Where isnull(BGMain.Invalid,0)=1 and BGMain.DepartMentKey in({0}) "
                           + "         And BGMain.ProjectKey in({1}) "
                           + "         And  BGStepKey in({5}) "
                           + "         and guid in ( "
                           + "                 select guid_bg_main from bg_detailview where len(bgcodekey)=2 "
                           + "                 and bgyear='{2}' "
                           + "                 ) "
                           + "     ) BG On Project.GUID=BG.GUID_Project "
                           + "     Where Isnull(Project.IsStop,0)=0 and Isnull(Project.StopYear,0)<3000 "
                           + "     And Project.ProjectKey In({1}) "
                           + "     and (project.guid in( "
                           + "                 select distinct guid_project from bg_mainview "
                           + "                 where isnull(Invalid,0)=1 and guid in(select distinct guid_bg_main from bg_detail where BGYear='{2}') "
                           + "                 and DepartMentKey in({0}) "
                           + "                  And  BGStepKey in({5}) "
                           + "                 ) "
                           + "          or project.guid in( "
                           + "                 select distinct pguid from ss_project "
                           + "                 where guid in ( "
                           + "                         select guid_project from bg_mainview where isnull(Invalid,0)=1 "
                           + "                         and guid in(select distinct guid_bg_main from bg_detail where BGYear='{2}') "
                           + "                         and DepartMentKey in({0}) "
                           + "                         And  BGStepKey in({5}) "
                           + "                           ) "
                           + "                    ) "
                           + "       ) "
                           + " ) PBG On Project.GUID=PBG.GUID "
                           + " Where Isnull(Project.IsStop,0)=0 and Isnull(Project.StopYear,0)<3000 "
                           + " And Project.ProjectKey In({1}) "
                           + " And {10}" //Project.GUID In()
                           +" order by Project.ProjectKey";

          this.OneLevel = " select guid from ss_project  where  pguid is null and Isnull(IsStop,0)=0 and Isnull(StopYear,0)<3000 ";
          this.TwoLevel = " select guid from ss_project where pguid in(  "      		 // --二级 
                          +this.OneLevel
                         +"  )";
          this.ThreeLevel = "select guid from ss_project where pguid in(  "// --三级 
	                        +this.TwoLevel
                            +")";
          this.FourLevel = "select guid from ss_project where pguid in(  "// --四级 
                            + this.ThreeLevel
                            + ")";
          this.FiveLevel = "select guid from ss_project where pguid in(  "// --五级 
                           + this.FourLevel
                           + ")";
          this.tempalte = Path.Combine(this.tempalte, "zxjdyl.xls");
      }   
      /// <summary>
      /// 获取拼接的Sql
      /// </summary>
      /// <param name="condition"></param>
      /// <returns></returns>
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
              List<SS_Project> projectAllList = new List<SS_Project>();
              List<SS_Project> projectList = new List<SS_Project>();
              List<TreeNode> treeNodeList = conditionModel.TreeNodeList;
              List<string> typeList = treeNodeList.GetModelType();

              foreach (string item in typeList)
              {

                  switch (item.ToLower())
                  {    
                      case "ss_dw":
                      case "ss_department":
                          List<string> keysDep = new List<string>();
                          List<Guid> guidDep = treeNodeList.GetGUIDList(item);
                           List<SS_Department> depList = new List<SS_Department>();
                          List<SS_Department> depAllList = new List<SS_Department>();
                          depAllList = db.GetDepartment(true,this.OperatorKey);
                          foreach (Guid gdep in guidDep)
                          {
                              SS_Department dep = new SS_Department();
                              dep.GUID = gdep;
                              dep.RetrieveLeafs(depAllList, ref depList);
                             
                          }
                          if (depList.Count > 0)
                          {
                              var depguid = depList.Select(e => e.DepartmentKey).ToList();
                              if (depguid != null && depguid.Count > 0)
                              {
                                  keysDep.AddRange(depguid);
                              }
                              dwIdOrDepartId = keysDep.GetStrGUIDS();
                          }
                          break;
                      //case "ss_dw":
                      //    List<string> idsDW = new List<string>();
                      //    List<Guid> guidDW = treeNodeList.GetGUIDList(item);
                      //    List<SS_DW> dwAllList = new List<SS_DW>();
                      //    dwAllList = db.GetDWList(true,this.OperatorKey);
                      //    List<SS_DW> dwList = new List<SS_DW>();
                      //    foreach (Guid gdw in guidDW)
                      //    {
                      //        SS_DW dw = new SS_DW();
                      //        dw.GUID = gdw;
                      //        dw.RetrieveLeafs(dwAllList, ref dwList);
                      //        var dwguid = dwList.Select(e => e.DWKey).ToList();
                      //        if (dwguid != null && dwguid.Count > 0)
                      //        {
                      //            idsDW.AddRange(dwguid);
                      //        }
                      //    }
                      //    if (idsDW.Count > 0)
                      //    {
                      //        dwIdOrDepartId = " DWKey in(" +idsDW.GetStrGUIDS()+ ") ";
                      //    }
                      //    break;
                      case "ss_projectclass":
                      case "ss_project":
                          List<string> keysProject = new List<string>();
                          List<Guid> guidProject = treeNodeList.GetGUIDList(item);
                          int year;
                          int.TryParse(conditionModel.Year,out year);
                          projectAllList = db.GetProject(true, this.OperatorKey,year);
                          projectList = projectAllList.FindAll(e => guidProject.Contains(e.GUID));
                          if (projectList!=null && projectList.Count > 0)
                          {
                              var projectGUID = projectList.Select(e => e.ProjectKey).ToList();
                              if (projectGUID != null && projectGUID.Count > 0)
                              {
                                  keysProject.AddRange(projectGUID);
                              }
                              proKey = keysProject.GetStrGUIDS();
                          }
                          break;
                      //case "ss_projectclass":
                          ////List<string> keysProClass = new List<string>();
                          ////List<Guid> guidProClass = treeNodeList.GetGUIDList(item);
                          ////List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();
                          ////proClassList = db.GetProjectClass(true, this.OperatorKey);
                          ////projectAllList = db.GetProject(true, this.OperatorKey);     
                          ////foreach (Guid gproClass in guidProClass)
                          ////{
                          ////    SS_ProjectClass projectclassModel = new SS_ProjectClass();
                          ////    projectclassModel.GUID = gproClass;
                          ////    projectclassModel.RetrieveLeafs(proClassList,projectAllList,ref projectList);
                          ////    var projectUID = projectList.Select(e => e.ProjectKey).ToList();
                          ////    if (projectUID != null && projectUID.Count > 0)
                          ////    {
                          ////        keysProClass.AddRange(projectUID);
                          ////    }
                          ////}
                          ////if (keysProClass.Count > 0)
                          ////{
                          ////    proKey = keysProClass.GetStrGUIDS();
                          ////}
                          ////break;
                      case "bg_setup":
                          List<string> guidStep = treeNodeList.GetKeyList(item);
                          if (guidStep != null && guidStep.Count > 0)
                          {
                              bgSetpId = guidStep.GetStrGUIDS();
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
              proKey = GetByProjectKey(Guid.Empty.ToString(), 1,conditionModel.Year);
          }
          if (string.IsNullOrEmpty(bgSetpId))
          {
              bgSetpId = GetStepKey(Guid.Empty.ToString());
          }
          //预算来源
          conditionModel.BGResourceType = GetBGResourceType(conditionModel.BGResourceType);
         
          //付款状态
          conditionModel.PayStatus = GetPayStatus(conditionModel.PayStatus);
          //审批状态
          conditionModel.ApproveStatus = GetByApproveStatus(conditionModel.ApproveStatus);
          //核销状态
          conditionModel.HXStatus = GetHXStatus(conditionModel.HXStatus);
          //凭证状态（即记账状态）
          conditionModel.CertificateStatus = GetCertificateStatus(conditionModel.CertificateStatus);
          //项目级别
          conditionModel.ProjectLevel = GetProjectLevel(conditionModel.ProjectLevel);
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
                                        conditionModel.ProjectLevel
                                        );
      }
      /// <summary>
      /// 项目级别
      /// </summary>
      /// <param name="projectLevel"></param>
      /// <returns></returns>
      public string GetProjectLevel(string pLevel)
      {
          string projectLevel = "1=1";
          if (pLevel == "1")
          {
              projectLevel = "Project.GUID In(" + this.OneLevel + ")";
          }
          else if (pLevel == "2")
          {
              projectLevel = "Project.GUID In(" + this.TwoLevel + ")";
          }
          else if (pLevel == "3")
          {
              projectLevel = "Project.GUID In(" + this.ThreeLevel + ")";
          }
          else if (pLevel == "4")
          {
              projectLevel = "Project.GUID In(" + this.FourLevel + ")";
          }
          else if (pLevel == "5")
          {
              projectLevel = "Project.GUID In(" + this.FiveLevel + ")";
          }
          return projectLevel;
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
          string level =string.Empty;   
          BBSearchCondition conditionModel = (BBSearchCondition)conditions;
          if (!string.IsNullOrEmpty(conditionModel.ProjectLevel))
          {
              level = conditionModel.ProjectLevel;
          }
          sqlStr = GetSql(conditionModel);
          DataTable orgdt = LoadData(sqlStr, ref msgError);
          DataTable newdt = CreateNewDataTable();
          ReSetData(orgdt, ref newdt, conditionModel.RMBUnit,level);
          return newdt;
      }
      /// <summary>
      /// 重新设置数据
      /// </summary>
      /// <param name="orgdt">原始数据</param>
      /// <param name="newdt">新创建的临时表</param>
      /// <returns></returns>
      public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit,string level)
      {
          string strSpace = string.Empty;
          if (orgdt == null) return;
          int unit = 1;
          if (int.TryParse(rmbUnit, out unit) == false)
          {
              unit = 1;
          }
          //合计
          double hjTotalbg1 = 0F;//总经费
          double hjTotalbg2 = 0F;//预算下达
          double hjbxTotal=0F;//报销 
          if (orgdt.Rows.Count > 0)
          {
              //int index = 0;
              DataRow[] dtRow = null;
              if (string.IsNullOrEmpty(level) || level == "0")
              {
                  dtRow=orgdt.Select(" PGUID is null");
              }
              else
              {
                  dtRow = orgdt.Select();
              }
              foreach (DataRow row in dtRow)
              {
                  ReSetChildData(orgdt, row, ref newdt, rmbUnit, strSpace);
              }

              //合计计算
              DataRow hjRow = newdt.NewRow();
              hjTotalbg1 = orgdt == null ? 0F : double.Parse(orgdt.Compute("Sum(iBGTotalSum)", "true").ToString());
              hjTotalbg2 = orgdt == null ? 0F : double.Parse(orgdt.Compute("Sum(iBGTotalXD)", "true").ToString());
              hjbxTotal = orgdt == null ? 0F : double.Parse(orgdt.Compute("Sum(iBXTotal)", "true").ToString());
              hjRow["ProjectKey"] = "合计";
              hjRow["ProjectName"] = "";
              hjRow["iBGTotalSum"] = hjTotalbg1 == 0F ? "" : FormatMoney(1, hjTotalbg1 / unit);
              hjRow["iBGTotalXD"] = hjTotalbg2 == 0F ? "" : FormatMoney(1, hjTotalbg2 / unit);
              hjRow["iBXTotal"] = hjbxTotal == 0F ? "" : FormatMoney(1, hjbxTotal / unit);
              hjRow["ysjy"] = (hjTotalbg2 - hjbxTotal) == 0F ? "" : FormatMoney(1,(hjTotalbg2 - hjbxTotal)/unit);
              hjRow["wcbl"] =(hjTotalbg2==0F || hjbxTotal==0F)?"": FormatMoney(1, (hjbxTotal / hjTotalbg2) * 100) + "%";
              newdt.Rows.Add(hjRow);

          }
      }
      /// <summary>
      /// 重新设置子节点数据
      /// </summary>
      /// <param name="orgdt">原始数据</param>
      /// <param name="newdt">新创建的临时表</param>
      /// <returns></returns>
      public void ReSetChildData(DataTable orgdt, DataRow currentRow, ref DataTable newdt, string rmbUnit, string strSpace)
      {
          if (orgdt == null) return;
          int unit = 1;
          if (int.TryParse(rmbUnit, out unit) == false)
          {
              unit = 1;
          }
          double totalbg1 = 0F;//总经费
          double totalbg2 = 0F;//预算下达
          double bxTotal = 0F;//报销
          if (orgdt.Rows.Count > 0)
          {
              if (currentRow != null)
              {
                  DataRow[] dtRow;
                  dtRow = orgdt.Select(" PGUID is not null and PGUID='" + currentRow["guid"] + "'");
                  totalbg1 = 0F;//分摊实际数
                  totalbg2 = 0F;//项目支出 
                  //添加当前节点值
                  DataRow newRow = newdt.NewRow();
                  newRow["ProjectKey"] = strSpace + currentRow["ProjectKey"].ToString();
                  newRow["ProjectName"] = strSpace+currentRow["ProjectName"].ToString();
                  newRow["PersonName"] = currentRow["PersonName"].ToString();
                  DataTable childDt = orgdt.Clone();
                  GetLeafsData(orgdt, currentRow, ref childDt);
                  GetBGTotalValue(childDt, ref totalbg1, ref totalbg2,ref bxTotal);
                  //总经费
                  newRow["iBGTotalSum"] = totalbg1 == 0F ? "" : FormatMoney(1, totalbg1 / unit);
                  //预算下达                            
                  newRow["iBGTotalXD"] = totalbg2 == 0F ? "" : FormatMoney(1, totalbg2 / unit);
                  //报销 （执行数）iBXTotal
                  newRow["iBXTotal"] = bxTotal == 0F ? "" : FormatMoney(1, bxTotal / unit);
                  //预算结余
                  newRow["ysjy"] = (totalbg2 - bxTotal) == 0F ? "" : FormatMoney(1, (totalbg2 - bxTotal) / unit);
                  //完成比例
                  newRow["wcbl"] = (totalbg2 == 0F || bxTotal == 0F) ? "" : FormatMoney(1, (bxTotal / totalbg2) * 100) + "%";
                  newdt.Rows.Add(newRow);

                  //子节点
                  if (dtRow != null && dtRow.Length > 0)
                  {
                      foreach (DataRow row in dtRow)
                      {
                          var strSpace1 = strSpace + this.colChar;
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
      private void GetBGTotalValue(DataTable childDt, ref double totalbg1, ref double totalbg2,ref double bxtotal)
      {
          double totalbg = 0F;
          if (childDt != null && childDt.Rows.Count > 0)
          {
              foreach (DataRow row in childDt.Rows)
              {
                  //总经费
                  if (row["iBGTotalSum"] != null)
                  {
                      if (double.TryParse(row["iBGTotalSum"].ToString(), out totalbg))
                      {
                          totalbg1 += totalbg;
                      }
                  }
                  //预算下达
                  if (row["iBGTotalXD"] != null)
                  {
                      if (double.TryParse(row["iBGTotalXD"].ToString(), out totalbg))
                      {
                          totalbg2 += totalbg;
                      }
                  }
                  //报销（预算执行数）
                  if (row["iBXTotal"] != null)
                  {
                      if (double.TryParse(row["iBXTotal"].ToString(), out totalbg))
                      {
                          bxtotal += totalbg;
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
          if (orgdt == null) return;
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
          dt.Columns.Add("ProjectName");//项目名称
          dt.Columns.Add("ProjectKey");//项目Key         
          dt.Columns.Add("PersonName");//人员名称         
          dt.Columns.Add("iBGTotalSum");//预算总经费
          dt.Columns.Add("iBGTotalXD");//预算下达
          dt.Columns.Add("iBXTotal");//报销
          dt.Columns.Add("ysjy");//预算结余
          dt.Columns.Add("wcbl");//完成比例
          return dt;
      }
      /// <summary>
      /// 导出报表
      /// </summary>
      /// <param name="data"></param>
      /// <param name="fileName"></param>
      /// <param name="message"></param>
      /// <returns></returns>
      public string GetExportPath(DataTable data, out string fileName, out string message,ReportHeadModel reportHeadModel)
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
              string filePath = ExportExcel.Export(data, this.tempalte, 6, 0, new List<ExcelCell>() { new ExcelCell(1, 2, reportHeadModel.DepartmentName), new ExcelCell(0, 1, reportHeadModel.Year+"年度"), new ExcelCell(7, 2, reportHeadModel.RMBUnit) });
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
