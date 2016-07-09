using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace CAE.Report
{
  public  class GZReport
    {
        public string tempalte = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];
        public string Year { get; set; }
        public string Month { get; set; }
        public string PlanId { get; set; }
        public string TemplateName { get; set; }
        public string SqlFormat { get; set; }
        public GZReport(bool IsZXBank=false) 
        {
            if (IsZXBank)
            {
                TemplateName = "zxyhbpb.xls";
                SqlFormat = @"SELECT  
        BankCardNo ,
        PersonName ,
 '001' AS DoMoney ,
   '' AS CLBZ ,
        ItemValue ,
        '' AS ReMark
       
     
FROM    dbo.SA_PlanActionDetailView
WHERE   
    GUID_Bank = '260294AE-226D-4635-862D-0C9025C0B01A'  and
    GUID_PlanAction IN ( SELECT GUID
                             FROM   dbo.SA_PlanAction
                             WHERE  ActionYear = {0}
                                    AND ActionMouth ={1}
                                    AND ActionState = 1 AND GUID_Plan='{2}')
        AND GUID_Item = '8D1C5E7D-4BE7-FF4A-9287-A3CED714EDD3'  ORDER BY PersonKey";
            }
            else 
            {
                SqlFormat = @"SELECT  PersonName ,
        BankCardNo ,
        ItemValue ,
        '' AS ReMark ,
        '' AS DoMoney ,
        '' AS CLBZ
FROM    dbo.SA_PlanActionDetailView
WHERE   
    GUID_Bank = '41F790A5-A003-459A-85E4-A436A9831D7E'  and
    GUID_PlanAction IN ( SELECT GUID
                             FROM   dbo.SA_PlanAction
                             WHERE  ActionYear = {0}
                                    AND ActionMouth ={1}
                                    AND ActionState = 1 AND GUID_Plan='{2}')
        AND GUID_Item = '8D1C5E7D-4BE7-FF4A-9287-A3CED714EDD3'  ORDER BY PersonKey";
                TemplateName = "ghyhbpb.xls";
            }

        }

        public string GetQFW(string v) 
        {
            double d = 0;
            double.TryParse(v, out d);
           return String.Format("{0:N}", d);
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
                foreach (DataRow item in data.Rows)
                {
                    item["ItemValue"] = GetQFW(item["ItemValue"] + "");
                }
                string filePath = ExportExcel.Export(data, tempalte1, 1, 0);
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

           var sql = string.Format(SqlFormat, Year, Month, PlanId);
            try
            {
                var dt = DataSource.ExecuteQuery(sql);
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
