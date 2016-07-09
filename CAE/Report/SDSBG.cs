using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CAE.Report
{
    //扣缴个人所得税报告表
   public class SDSBG
    {
       public string tempalte = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];
        public string Year { get; set; }
        public string Month { get; set; }
        public string TemplateName { get; set; }
        public string SqlFormat { get; set; }

        public DataTable GetCreateDT() 
        {
           
            var dt = new DataTable();
            dt.Columns.Add("XH", typeof(String));
            dt.Columns.Add("PersonName", typeof(String));
            dt.Columns.Add("SFZJType", typeof(String));
            dt.Columns.Add("IDCard", typeof(String));
            dt.Columns.Add("SDXM", typeof(String));
            dt.Columns.Add("SDQJ", typeof(String));
            dt.Columns.Add("ItemValue", typeof(String));
            dt.Columns.Add("mse", typeof(String));
            dt.Columns.Add("ylbx", typeof(String));
            dt.Columns.Add("ylbx1", typeof(String));
            dt.Columns.Add("ksybx", typeof(String));
            dt.Columns.Add("kzfgjj", typeof(String));
            dt.Columns.Add("czyz", typeof(String));
            dt.Columns.Add("yxkcdesf", typeof(String));
            dt.Columns.Add("qt", typeof(String));
            dt.Columns.Add("hj", typeof(String));
            dt.Columns.Add("JCFY", typeof(String));
            dt.Columns.Add("zxkcdjze", typeof(String));
            
            dt.Columns.Add("jse", typeof(String));
            dt.Columns.Add("sl", typeof(String));
            dt.Columns.Add("sskcs", typeof(String));
            dt.Columns.Add("dks", typeof(String));

            dt.Columns.Add("jmse", typeof(String));
            dt.Columns.Add("ykjse", typeof(String));
            dt.Columns.Add("ykjse1", typeof(String));
            dt.Columns.Add("ybtse", typeof(String));
            dt.Columns.Add("remark", typeof(String));
            return dt;
        }
        public SDSBG() 
        {
            //PersonName  GJ SFZJType  IDCard  ReMark
            TemplateName = "sdsbg.xls";
            SqlFormat = @"SELECT DISTINCT
        XH ,
        PersonName ,
        SFZJType ,
        IDCard ,
        SDXM ,
        SDQJ ,
        ItemValue ,
        mse ,/*免税所得*/
        ylbx ,
        ylbx1 ,
        ksybx ,
        kzfgjj ,
        czyz ,
        yxkcdesf ,
        qt ,
        ( ISNULL(ylbx, 0) + ISNULL(ylbx1, 0) + ISNULL(ksybx, 0)
          + ISNULL(kzfgjj, 0) + ISNULL(yxkcdesf, 0) + ISNULL(qt, 0) ) AS hj ,
        JCFY ,
        zxkcdjze ,
        ( ItemValue - mse - ( ISNULL(ylbx, 0) + ISNULL(ylbx1, 0)
                              + ISNULL(ksybx, 0) + ISNULL(kzfgjj, 0)
                              + ISNULL(yxkcdesf, 0) + ISNULL(qt, 0) ) - jcfy-qqkk+qt1jse ) AS jse ,
        sl ,
        sskcs ,
        dks ,
        jmse ,
        dks AS ykjse ,
        dks AS ykjse1 ,
        ybtse ,
        remark,qqkk,qt1jse
FROM    ( SELECT    '0' AS XH ,
                    a.PersonName ,
                    b.CredentialTypeName AS SFZJType ,
                    b.IDCard ,
                    '0101-工资、薪金所得－普通月工资' AS SDXM ,
                    '{0}02' AS SDQJ ,
                    ItemValue ,
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = '919B2C37-7A78-BB42-97EE-739F86E1811A'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS mse ,/*免税额*/
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = '50D03D9B-25D4-B14A-98EE-FB6990C93C8D'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS ylbx ,/*扣养老保险*/
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = 'B957D9DD-0E97-4749-A78F-1B28B6F808E2'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS ylbx1 ,/*扣医疗保险*/
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = '2B39D3AB-A0D9-7149-AB7A-07290A998325'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS ksybx ,/*扣失业保险*/
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = 'EFC69C83-2BC2-694B-B802-A357ABC32DF5'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS kzfgjj ,/*扣住房公积金*/
                    '' czyz ,
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = '4B4C6576-0ACF-4EAC-88A5-C4B0BCEC4C87'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS yxkcdesf ,/*减少额度*/
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = 'FD763FB0-081E-7E4B-8FB1-560B84D124DA'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS qt ,/*扣款*/
                   --( mse+ylbx1+ksybx+kzfgjj+yxkcdesf+qt) AS hj
                    '3500' AS JCFY ,
                    '' AS zxkcdjze ,
                    '' AS jse ,
                    '0' AS sl ,
                    '0' AS sskcs ,
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = '91174047-1B1C-0F47-828D-C3A105F6465D'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS dks ,
                    '' AS jmse ,
                    '' AS ybtse ,
                    '' AS remark,
                    ( SELECT    ItemValue
                                          FROM      dbo.SA_PlanActionDetail
                                          WHERE     GUID_Person = a.GUID_Person
                                                    AND GUID_Item = 'B3A22A4C-AE4B-4041-B9B8-FF65E022C892'
                                                    AND GUID_PlanAction IN (
                                                    SELECT  GUID
                                                    FROM    dbo.SA_PlanAction
                                                    WHERE   ActionYear = {0}
                                                            AND ActionMouth ={1}
                                                            AND ActionState = 1 )
                                        ) AS qqkk ,/*缺勤扣款*/
                        ( SELECT    ItemValue
                                          FROM      dbo.SA_PlanActionDetail
                                          WHERE     GUID_Person = a.GUID_Person
                                                    AND GUID_Item = '096EF024-A1E2-854E-9EF9-7601838A94FF'
                                                    AND GUID_PlanAction IN (
                                                    SELECT  GUID
                                                    FROM    dbo.SA_PlanAction
                                                    WHERE   ActionYear = {0}
                                                            AND ActionMouth ={1}
                                                            AND ActionState = 1 )
                                        ) AS qt1jse /*其他计税额qqkk qtjse*/
          FROM      dbo.SA_PlanActionDetailView a
                    LEFT JOIN dbo.SS_PersonView b ON b.GUID = a.GUID_Person
          WHERE     GUID_PlanAction IN (
                    SELECT  GUID
                    FROM    dbo.SA_PlanAction
                    WHERE   GUID_Plan = 'DDA6C0DD-EBC8-47FD-8087-84DF718953EB'
                            AND ActionYear = {0}
                            AND ActionMouth ={1}
                            AND ActionState = 1 )
                    AND GUID_Item = '02DD226D-EFE0-A64E-B64D-7F5DD5C50641'/*ying fa heji*/
          UNION ALL
          SELECT    '0' AS XH ,
                    a.PersonName ,
                    b.CredentialTypeName AS SFZJType ,
                    b.IDCard ,
                    '0103-工资、薪金所得－全年一次性奖金' AS SDXM ,
                    '{0}02' AS SDQJ ,
                    ItemValue ,
                    '' AS mse ,  /*免税额*/
                    '' AS ylbx , /*扣养老保险*/
                    '' AS ylbx1 ,/*扣医疗保险*/
                    '' AS ksybx ,/*扣失业保险*/
                    '' AS kzfgjj ,/*扣住房公积金*/
                    '' czyz ,
                    '' AS yxkcdesf ,/*减少额度*/
                    '' AS qt ,/*扣款*/
                   --( mse+ylbx1+ksybx+kzfgjj+yxkcdesf+qt) AS hj
                    '3500' AS JCFY ,
                    '' AS zxkcdjze ,
                    '' AS jse ,
                    '0' AS sl ,
                    '0' AS sskcs ,
                    ( SELECT    ItemValue
                      FROM      dbo.SA_PlanActionDetail
                      WHERE     GUID_Person = a.GUID_Person
                                AND GUID_Item = 'AFA11E05-0717-314F-9289-5FD79B01C3BB'
                                AND GUID_PlanAction IN (
                                SELECT  GUID
                                FROM    dbo.SA_PlanAction
                                WHERE   ActionYear = {0}
                                        AND ActionMouth ={1}
                                        AND ActionState = 1 )
                    ) AS dks ,
                    '' AS jmse ,
                    '' AS ybtse ,
                    '' AS remark,'' as qqkk,'' as  qt1jse
          FROM      dbo.SA_PlanActionDetailView a
                    LEFT JOIN dbo.SS_PersonView b ON b.GUID = a.GUID_Person
          WHERE     GUID_PlanAction IN (
                    SELECT  GUID
                    FROM    dbo.SA_PlanAction
                    WHERE   GUID_Plan = 'DDA6C0DD-EBC8-47FD-8087-84DF718953EB'
                            AND ActionYear = {0}
                            AND ActionMouth ={1}
                            AND ActionState = 1 )
                    AND GUID_Item = '17AA1EBA-6AF4-3045-8C72-E49081FAA472'
                    AND a.ItemValue > 0
          UNION ALL
          SELECT    '0' AS XH ,
                    InvitePersonName AS PersonName ,
                    CredentialTypeName AS SFZJType ,
                    InvitePersonIDCard AS IDCard ,
                    '0400-劳务报酬所得' AS SDXM ,
                    '{0}02' AS SDQJ ,
                    SUM(Total_BX) AS ItemValue ,
                    '' AS mse ,
                    '' AS ylbx ,
                    '' AS yblx1 ,
                    '' AS ksybx ,
                    '' AS kzfgjj ,
                    '' AS czyz ,
                    '' AS yxkcdesf ,
                    '' AS qt ,
                    CASE WHEN ISNULL(SUM(Total_BX), 0) > 4000
                         THEN ISNULL(SUM(Total_BX), 0) * 0.2
                         ELSE '800'
                    END AS JCFY ,
                    '' AS zxkcdjze ,
                    '' AS jse ,
                    '0' AS sl ,
                    '0' AS sskcs ,
                    SUM(Total_Tax) AS dks ,
                    '' AS jmse ,
                    '' AS ybtse ,
                    '' AS remark,'' as qqkk,'' as  qt1jse
          FROM      dbo.BX_InviteFeeview
          WHERE     GUID_InvitePerson NOT IN (
                    SELECT  GUID_Person
                    FROM    dbo.SA_PlanActionDetail
                    WHERE   GUID_PlanAction IN (
                            SELECT  GUID
                            FROM    dbo.SA_PlanAction
                            WHERE   GUID_Plan = 'DDA6C0DD-EBC8-47FD-8087-84DF718953EB'
                                    AND ActionYear = {0}
                                    AND ActionMouth ={1}
                                    AND ActionState = 1 ) )
                    AND GUID_BX_Main IN (
                    SELECT  GUID_Main
                    FROM    dbo.HX_Detail
                    WHERE   ClassID_Main = '23'
                            AND GUID_HX_Main IN (
                            SELECT  GUID
                            FROM    dbo.HX_Main
                            WHERE   YEAR(DocDate) = {0}
                                    AND MONTH(DocDate) ={1} ) )
          GROUP BY  InvitePersonName ,
                    CredentialTypeName ,
                    InvitePersonIDCard
        ) a
ORDER BY sdxm ,
        a.PersonName



";
          

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
                string filePath = ExportExcel.Export(data, tempalte1, 7, 0);
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

                double ItemValue = 0, mse = 0,  ylbx=0,ylbx1=0,ksybx=0 ,
        kzfgjj =0,
        czyz = 0,
        yxkcdesf = 0,
        qt = 0, hj = 0, JCFY = 0,
        zxkcdjze = 0,
        jse = 0,
        sl = 0,
        sskcs = 0,
        dks = 0,
        jmse = 0, ykjse = 0, ykjse1 = 0, ybtse = 0;
              
                var dt = DataSource.ExecuteQuery(sql);
                var i = 2;

                var dtColne = GetCreateDT();
                //foreach (DataRow item in dt.Rows)
                for (int ii = 0; ii < dt.Rows.Count; ii++)
                {
                    var row = dtColne.NewRow();
                    var item = dt.Rows[ii];
                    row["XH"] = i++;
                    //PersonName SFZJType IDCard SDXM
                    row["PersonName"] = item["PersonName"] + "";
                    row["SFZJType"] = item["SFZJType"] + "";
                    row["IDCard"] = item["IDCard"] + "";
                    row["SDXM"] = item["SDXM"] + "";//SDQJ
                    row["SDQJ"] =(Year+"")+(Month.Length==2?(Month+""):("0"+Month));
                    string d1,d2;
                    GetSL(item["jse"]+"",out d1,out d2);
                    item["sl"]=d1;
                    item["sskcs"] = d2;
                  
                    ItemValue  = Sum(item["ItemValue"] + "", ItemValue);
                    mse        = Sum(item["mse"] + "", mse);
                    ylbx       = Sum(item["ylbx"] + "", ylbx);
                    ylbx1      = Sum(item["ylbx1"] + "", ylbx1);
                    ksybx      = Sum(item["ksybx"] + "", ksybx);
                    kzfgjj     = Sum(item["kzfgjj"] + "", kzfgjj);
                    czyz       = Sum(item["czyz"] + "", czyz);
                    yxkcdesf   = Sum(item["yxkcdesf"] + "", yxkcdesf);
                    qt         = Sum(item["qt"] + "", qt);
                    hj         = Sum(item["hj"] + "", hj);
                    JCFY       = Sum(item["JCFY"] + "", JCFY);
                    zxkcdjze   = Sum(item["zxkcdjze"] + "", zxkcdjze);
                    jse        = Sum(item["jse"] + "", jse);
                    sl         = Sum(item["sl"] + "", sl);
                    sskcs      = Sum(item["sskcs"] + "", sskcs);
                    dks        = Sum(item["dks"] + "", dks);
                    jmse       = Sum(item["jmse"] + "", jmse);
                    ykjse      = Sum(item["ykjse"] + "", ykjse);
                    ykjse1     = Sum(item["ykjse1"] + "", ykjse1);
                    ybtse      = Sum(item["ybtse"] + "", ybtse);

                    row["sskcs"] = GetQFW(item["sskcs"] + "");
                    row["ItemValue"] = GetQFW(item["ItemValue"] + "");
                    row["mse"] = GetQFW(item["mse"] + "");
                    row["ylbx"] = GetQFW(item["ylbx"] + "");
                    row["ylbx1"] = GetQFW(item["ylbx1"] + "");
                    row["ksybx"] = GetQFW(item["ksybx"] + "");
                    row["kzfgjj"] = GetQFW(item["kzfgjj"] + "");
                    row["czyz"] = GetQFW(item["czyz"] + "");
                    row["yxkcdesf"] = GetQFW(item["yxkcdesf"] + "");
                    row["qt"] = GetQFW(item["qt"] + "");
                    row["hj"] = GetQFW(item["hj"] + "");
                    row["JCFY"] = GetQFW(item["JCFY"] + "");
                    row["zxkcdjze"] = GetQFW(item["zxkcdjze"] + "");
                    row["jse"] = GetQFWC(item["jse"] + "");
                    row["sl"] = GetQFW(item["sl"] + "")+"%";
                    row["sskcs"] = GetQFW(item["sskcs"] + "");
                    row["dks"] = GetQFW(item["dks"] + "");
                    row["jmse"] = GetQFW(item["jmse"] + "");
                    row["ykjse"] = GetQFW(item["ykjse"] + "");
                    row["ykjse1"] = GetQFW(item["ykjse1"] + "");
                    row["ybtse"] = GetQFW(item["ybtse"] + "");
                    dtColne.Rows.Add(row);
                }
                var rown =dtColne.NewRow();
               rown["XH"] = "1";
               rown["ItemValue"]  = GetQFW( ItemValue+"");
               rown["mse"]        = GetQFW(mse    +"");  
               rown["ylbx"]       = GetQFW(ylbx  +"");   
               rown["ylbx1"]      = GetQFW(ylbx1 +"");   
               rown["ksybx"]      = GetQFW(ksybx +"");   
               rown["kzfgjj"]     = GetQFW(kzfgjj +"");  
               rown["czyz"]       = GetQFW(czyz +"");    
               rown["yxkcdesf"]   = GetQFW(yxkcdesf +"");
               rown["qt"]         = GetQFW(qt +"");      
               rown["hj"]         = GetQFW(hj     +"");  
               rown["JCFY"]       = GetQFW(JCFY  +"");   
               rown["zxkcdjze"]   = GetQFW(zxkcdjze+""); 
               rown["jse"]        = GetQFW(jse  +"");    
               rown["sl"]         = GetQFW(sl +"")+"%";      
               rown["sskcs"]      = GetQFW(sskcs  +"");  
               rown["dks"]        = GetQFW(dks +"");     
               rown["jmse"]       = GetQFW(jmse  +"");   
               rown["ykjse"]      = GetQFW(ykjse +"");   
               rown["ykjse1"]     = GetQFW(ykjse1  +"");
               rown["ybtse"]      =GetQFW( ybtse+"");
               dtColne.Rows.InsertAt(rown, 0);
               return dtColne;

            }
            catch (Exception ex)
            {
                Msg = ex.Message;
                return null;
            }
          
        }
        public string GetQFWC(string v)
        {
            double d = 0;
            double.TryParse(v, out d);
            if (d < 0) return "0.00";
            return String.Format("{0:N}", d);
        }
        public string GetQFW(string v)
        {
            double d = 0;
            double.TryParse(v, out d);
            return String.Format("{0:N}", d);
        }
        public void GetSL(string v,out string sl,out string sss) 
        {
             sl = "0.00";
             sss = "0";
            if (string.IsNullOrEmpty(v)) return;
            double d = 0;
            if (!double.TryParse(v, out d)) return;
            var tax = d;
            if (tax < 0) { sl = "0"; sss = "0"; return; }
            if (tax>0&&tax <= 1500) { sl = "3%"; sss = "0"; return; }
            if (tax > 1500 && tax <= 4500)    { sl = "10"; sss = "105";  return; }
            if (tax > 4500 && tax <= 9000)    { sl = "20"; sss = "555";  return; }
            if (tax > 9000 && tax <= 35000)   { sl = "25"; sss = "1005"; return; }
            if (tax > 35000 && tax <= 55000)  { sl = "30"; sss = "2755"; return; }
            if (tax > 55000 && tax <= 80000)  { sl = "35"; sss = "5505"; return; }
                //if (tax > 80000) return "3%";
                sl = "45"; sss = "13505";
        }
        public double Sum(string v, double s) 
        {
            if (string.IsNullOrEmpty(v)) return s;
            double d = 0;
            double.TryParse(v, out d);
            return s + d;
        }

    }
}
