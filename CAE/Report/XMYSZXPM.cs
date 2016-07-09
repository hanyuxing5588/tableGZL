using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace CAE.Report
{
  public  class XMYSZXPM:BaseReport
    {
      public string SDate { get; set; }
      public string EDate { get; set; }
      public string DepartmentKeyStr{get;set;}
      public string FunClassStr{get;set;}
      public int MType{get;set;}
      public Guid OperatorId { get; set; }
      public string sqlAction = @"
            SELECT  '1' as t2,
                    a.GUID_Project,
                    ProjectKey ,
                    ProjectName ,
                    ISNULL(zjf, 0) as zjf ,
                    ISNULL(xmzjcb, 0) as xmzjcb  ,
                    ISNULL(zxs, 0) as zxs ,
                    ISNULL(xmzjcb, 0) - ISNULL(zxs, 0) AS ye ,
                    CAST(
                    CASE WHEN ISNULL(xmzjcb, 0) = 0 THEN 0
                         ELSE ISNULL(zxs, 0) / ISNULL(xmzjcb, 0)
                    END AS nvarchar(150)) AS 'zxfpbl'
            FROM    dbo.BG_MainView a
                    LEFT JOIN ( SELECT  SUM(Total_BG) AS 'zjf' ,
                                        GUID_BG_Main
                                FROM    dbo.BG_DetailView
                                WHERE   BGItemKey IN ( '03', '04', '07', '08' ) And LEN(BGCodeKey)=2 And BGYear={4}
                                GROUP BY GUID_BG_Main
                              ) b ON a.GUID = b.GUID_BG_Main
                    LEFT JOIN ( SELECT  SUM(Total_BG) AS 'xmzjcb' ,
                                        GUID_BG_Main
                                FROM    dbo.BG_DetailView
                                WHERE   BGItemKey IN ( '07', '08' ) And LEN(BGCodeKey)=2 And BGYear={4} 
                                GROUP BY GUID_BG_Main
                              ) c ON a.GUID = c.GUID_BG_Main
                    LEFT JOIN ( SELECT  SUM(Total_BX) AS 'zxs' ,
                                        GUID_Project
                                FROM    dbo.BX_Detail
                                WHERE   GUID_BX_Main IN ( SELECT    GUID
                                                          FROM      dbo.BX_Main
                                                          WHERE     (isnull(DocState,0) = 999 or isnull(DocState,0) = -1) and docDate BETWEEN '{0}' AND '{1}')
                                GROUP BY GUID_Project
                              ) d ON a.GUID_Project = d.GUID_Project
            WHERE   Invalid = 1 And BGStepKey='05' 
                   and a.GUID_Project in ( SELECT GUID FROM dbo.SS_Project WHERE IsFinance=1 AND IsStop=0 AND  GUID NOT IN (SELECT PGUID FROM dbo.SS_Project WHERE PGUID IS NOT NULL) AND ( StopYear IS NULL OR StopYear>={4}))
                    AND a.GUID_Project IS NOT NULL
                    AND a.DepartmentKey IN ( '{2}' )
                    and a.guid_project in (
                        SELECT  GUID_Data
FROM    dbo.SS_DataAuthSet
WHERE   ClassID = 5
       
        AND ( GUID_RoleOrOperator IN (
              SELECT    GUID_Role
              FROM      dbo.SS_RoleOperator
              WHERE     GUID_Operator = '{5}' )
              OR GUID_RoleOrOperator = '{5}'
            )
                    )
		            AND a.GUID_Project IN (SELECT GUID FROM dbo.SS_Project WHERE GUID_FunctionClass IN ('{3}') and GUID not in (select PGUID from SS_Project where PGUID is not null))

            and  guid in ( SELECT DISTINCT GUID_BG_Main FROM dbo.BG_Detail WHERE BGYear={4} )
            ORDER BY zxfpbl DESC
        
        ";
      public XMYSZXPM(string sDate, string eDate, string departmentKeyStr, string funClassStr, int mType) 
      {
          SDate = sDate;
          EDate = eDate;
          DepartmentKeyStr = departmentKeyStr;
          FunClassStr = funClassStr;
          MType = mType;
      }
      public XMYSZXPM() { }

      public DataTable GetReport(out string msg) 
      {
          msg = "";
          try
          {
              int BGYear = DateTime.Parse(SDate).Year;
              var sql = string.Format(sqlAction, SDate, EDate, DepartmentKeyStr, FunClassStr,BGYear,OperatorId);
              var dt = DataSource.ExecuteQuery(sql);
              var i = 1;
              //增加合计项
              var total = dt.NewRow();
              double zjf = 0;
              double xmzjcb = 0;
              double zxs = 0;
              
              foreach (DataRow dr in dt.Rows)
              {
                  zjf +=double.Parse(dr["zjf"].ToString());
                  xmzjcb += double.Parse(dr["xmzjcb"].ToString());
                  zxs += double.Parse(dr["zxs"].ToString());
                  dr["t2"] = i.ToString();
                  dr["zjf"] = GetChangeUnit(dr["zjf"] + "");
                  dr["xmzjcb"] = GetChangeUnit(dr["xmzjcb"] + "");
                  dr["zxs"] = GetChangeUnit(dr["zxs"] + "");
                  dr["ye"] = GetChangeUnit(dr["ye"] + "");
                  dr["zxfpbl"] = GetChangeUnitBFB(dr["zxfpbl"] + "");
                  i++;
              }
              
              total["ProjectKey"] = "";
              total["ProjectName"] = "合计";
              total["zjf"] = GetChangeUnit(zjf + "");
              total["zxs"] = GetChangeUnit(zxs + "");
              total["xmzjcb"] = GetChangeUnit(xmzjcb + "");
              total["ye"] = GetChangeUnit((xmzjcb-zxs) + "");
              total["zxfpbl"] = GetChangeUnitBFB((xmzjcb == 0 ? 0 : zxs / xmzjcb) + "");
              dt.Rows.Add(total);
              return dt;
          }
          catch (Exception ex)
          {
              msg = ex.Message.ToString();
              return null;
          }
         
      }
      public string GetChangeUnitBFB(string s) 
      {
          double d = 0;
          double.TryParse(s, out d);
          return (d).ToString("P");
      }
      public string GetChangeUnit(string s) 
      {
          double d = 0;
          double.TryParse(s, out d);
          return (d / MType).ToString("0.00");
      }
      //导出报表
      public override string GetExportPath(DataTable data, out string fileName, out string message)
      {
          this.tempalte = Path.Combine(this.tempalte, "bmgzmxb.xls");   
          fileName = "";
          message = "";
          try
          {
              if (data != null && data.Rows.Count <= 0)
              {
                  message = "1";
                  return "";
              }
              string filePath = ExportExcel.Export(data, this.tempalte, 2, 2, new List<ExcelCell>() { });
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

