using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace CAE.Report
{
    //个人所得税基础信息表（A表）
  public  class KJGRSDSBGB
    {
        public string tempalte = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];
        public string Year { get; set; }
        public string Month { get; set; }
        public string TemplateName { get; set; }
        public string SqlFormat { get; set; }
        public KJGRSDSBGB() 
        {
            //PersonName  GJ SFZJType  IDCard  ReMark
            TemplateName = "kjgrsdsbgb.xls";
            SqlFormat = @"SELECT DISTINCT
        *
FROM    ( SELECT    
'0' as XH,
a.PersonName ,
                    '中国' AS GJ ,
                    b.CredentialTypeName AS SFZJType ,
                    b.IDCard ,
                    '否' AS ReMark
          FROM      dbo.SA_PlanActionDetailView a
                    LEFT JOIN dbo.SS_PersonView b ON b.GUID = a.GUID_Person
          WHERE        GUID_PlanAction IN ( SELECT GUID
                                         FROM   dbo.SA_PlanAction
                                         WHERE  GUID_Plan='DDA6C0DD-EBC8-47FD-8087-84DF718953EB' and ActionYear = {0}
                                                AND ActionMouth = {1}
                                                AND ActionState = 1 )
                    AND GUID_Item = '8D1C5E7D-4BE7-FF4A-9287-A3CED714EDD3'
          UNION ALL
          SELECT    '0' as XH,InvitePersonName AS PersonName ,
                    '中国' AS GJ ,
                    CredentialTypeName AS SFZJType ,
                    InvitePersonIDCard AS IDCard ,
                    '否' AS ReMark
          FROM      dbo.BX_InviteFeeview
          WHERE     
 GUID_BX_Main  IN (SELECT GUID FROM dbo.BX_Main WHERE YEAR(DocDate)={0} AND MONTH(DocDate)={1})
           AND 
GUID_BX_Main IN (
                   SELECT GUID_Main FROM dbo.HX_Detail WHERE ClassID_Main='23' AND GUID_HX_Main IN (SELECT GUID FROM dbo.HX_Main  WHERE YEAR(DocDate)={0} AND MONTH(DocDate)={1}))
        ) a
ORDER BY a.PersonName";
          

        }
        //导出报表
        public  string GetExportPath(DataTable data, out string fileName, out string message)
        {
            var tempalte1 = Path.Combine(this.tempalte,TemplateName);
            fileName = "";
            message = "";
            try
            {
                if (data != null && data.Rows.Count <= 0)
                {
                    message = "1";
                    return "";
                }
                string filePath = ExportExcel.Export(data, tempalte1, 3, 0);
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }
        }
        public DataTable GetGHReport(out string Msg) 
        {
            Msg = "";
            int y = 0, m = 0;
            int.TryParse(Year, out y);
            int.TryParse(Month, out m);
            if (Month == "1")
            {
                Year = (y - 1).ToString(); ;
                Month = "12";
            }
            else
            {
                Month = (m - 1).ToString(); ;
            }
           var sql = string.Format(SqlFormat, Year, Month);
            try
            {
              
                var dt = DataSource.ExecuteQuery(sql);
                var i = 1;
                foreach (DataRow item in dt.Rows)
                {
                    item["XH"] = i++;
                }
                return dt;

            }
            catch (Exception ex)
            {
                Msg = ex.Message;
                return null;
            }
          
        }
    }
}
