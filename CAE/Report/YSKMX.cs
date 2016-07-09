using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Xml.Serialization;
namespace CAE
{
    /// <summary>
    /// 应收款明细表(国家测绘局)
    /// </summary>
    public class YSKMX
    {
        public string tempalte ="";
        public string OperatorKey = string.Empty;

        public string OperatorName = string.Empty;

        public YSKMX(string key) {
            this.OperatorKey = key;
         
        }
     
        private DataTable CreateTempTable(int count)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < count + 1; i++)
            {
                dt.Columns.Add(new DataColumn("T" + i));
            }
            return dt;
        }
        /*3为人员 2为部门 1为借款日期*/
        public DataTable GetReport(string iJKDateOrDepOrPer,string mType,out string msgError)
        {
            msgError = "";
            int mtype=10000;
            int.TryParse(mType, out mtype);
            DataTable dtResult = CreateTempTable(10);
            string strOrder="";
            if (iJKDateOrDepOrPer.Contains("1"))
            {
                strOrder = "PersonKey";
            }
            if (iJKDateOrDepOrPer.Contains("2"))
            {
                strOrder = strOrder.Length > 0 ? strOrder + ",Departmentkey" : "Departmentkey";
            }
            if (iJKDateOrDepOrPer.Contains("3"))
            {
                strOrder = strOrder.Length > 0 ? strOrder + ",DocDate" : "DocDate";
            }
            string strWhereTemp =  "";
            if (iJKDateOrDepOrPer.Contains("4"))
            {
                strWhereTemp = "  ";
            }
            else {
                strWhereTemp = " and balance<>0 ";
            }
            strOrder = string.IsNullOrEmpty(strOrder) ? " order by Departmentkey asc,docDate asc " : " order by " + strOrder;
            string strSQL = "select b.DepartmentName,convert(nvarchar,c.docDate,23) as DocDate,DocNum,DocMemo,Loan,Repayment,Balance,Remark,PersonName from rp_ysk c"+
                " left join SS_Person a on a.GUID=GUID_Person"+
                " Left Join ss_Department b on b.Guid=c.GUID_DepartMent  where 1=1 " + strWhereTemp + strOrder;
            var dtTemp = DataSource.ExecuteQuery(strSQL);
            if (dtTemp.Rows.Count <= 0) return dtTemp;
            var dic = new Dictionary<int, double>();
            dic.Add(4, 0);
            dic.Add(5, 0);
            dic.Add(6, 0);
            for (int i = 0; i < dtTemp.Rows.Count; i++)
            {
                var drTempRow = dtTemp.Rows[i];
                for (int j = 0; j < dtTemp.Columns.Count; j++)
                {
                    if (j == 4||j==5||j==6)
                    {
                        double d;
                        if (!string.IsNullOrEmpty(drTempRow[j] + "") && double.TryParse(drTempRow[j].ToString(), out d))
                        {
                            dic[j]+= d;
                            drTempRow[j] = (d / mtype).ToString("0.00");
                        }
                        else
                        {
                            drTempRow[j] = "0.00";
                        }
                    }
                }
            }
            
            GetPersonName(ref dtResult);
            DataRow drLast = dtTemp.NewRow();
            drLast[0] = "合计";
            drLast[4] = (dic[4]/mtype).ToString("0.00");
            drLast[5] = (dic[5] / mtype).ToString("0.00");
            drLast[6] = (dic[6] / mtype).ToString("0.00");
            dtTemp.Rows.Add(drLast);
            return dtTemp;
        }
        private void GetPersonName(ref DataTable dt) 
        {
            return;
            foreach (DataRow dr in dt.Rows)
            {   
                string steTemp = string.IsNullOrEmpty(dr[7]+"")?"":dr[7].ToString();
                if (steTemp=="") 
                {
                    string drTemp=string.IsNullOrEmpty(dr[3]+"")?"":dr[3].ToString();
                    int index=drTemp.IndexOf("借");
                    if (index < 0) continue;
                    string strName = drTemp.Substring(0, index);
                    string strSQL = "select PersonName as cPerName,DepartmentName as  cDepName,Personkey as  cPerKey,DepartmentKey as cDepKey from ss_personview where PersonName='" + strName + "'";
                    var dt1 = DataSource.ExecuteQuery(strSQL);
                    if (dt1.Rows.Count > 0) {
                        dr[7] = dt1.Rows[0]["cPerName"];
                        dr[8] = dt1.Rows[0]["cDepName"];
                        dr[9] = dt1.Rows[0]["cPerKey"];
                        dr[10] = dt1.Rows[0]["cDepKey"];
                    }
                }
            }
        }
        private string GetAuthWhere()
        {
            return "";
        }
        public string GetExportPath(string iJKDateOrDepOrPer,string mType,out string fileName) 
        {
            this.tempalte = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], "yskmxb.xls");
           fileName = "";
           string msg = "";
           var data=GetReport(iJKDateOrDepOrPer,mType,out msg);
           string filePath=ExportExcel.Export(data, this.tempalte,2);
           fileName=Path.GetFileName(filePath);
           return filePath;
        }
    }
    public class YSKMXWrite 
    {
       public  YSKMXWrite() {
       }
       public string GetU8账面支出(int i)
       {
           var xmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Common\\YSKBBWriteConfig.xml");
           var sqlConfig =Infrastructure.CommonFuntion.DeserialalizeXML<YSKLRConfig>(xmlPath);
           var sql = "";
           switch (i)
           {
               case 0:
                   sql = sqlConfig.SqlConfig[0].SCSql;
                   break;
               case 1:
                   sql = sqlConfig.SqlConfig[0].DNSql;
                   break;
               default:
                   sql = sqlConfig.SqlConfig[0].YQSql;
                   break;
           }
           var result = DataSource.ExecuteScalar(sql);
           if (string.IsNullOrEmpty(result+"")) {
               return "0.00";
           }
           return string.Format("{0:N}",double.Parse(result.ToString()));
       }
       
    }
    [Serializable]
    [XmlRoot("Config")]
    public class YSKLRConfig
    {
        [XmlElement("SqlConfig")]
        public List<SqlYSKConfig> SqlConfig { get; set; }
    }
    public class SqlYSKConfig
    {
        [XmlAttribute("实存账户Sql")]
        public string SCSql { get; set; }
        [XmlAttribute("当年借款Sql")]
        public string DNSql { get; set; }
        [XmlAttribute("以前年度借款Sql")]
        public string YQSql { get; set; }
    }
}
