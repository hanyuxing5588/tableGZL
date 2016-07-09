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
    /// 项目执行进度一览表(国家测绘局)
    /// </summary>
    public class XMZCJDYLB
    {
        public string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
        private List<IFZT> _IFZTs = null;
        public List<IFZT> IFZTs
        {
            get
            {
                if (_IFZTs == null) _IFZTs = CreateIFZTs();
                return _IFZTs;
            }
        }
        public string loginKey { get; set; }
        public XMZCJDYLBConfig XMZCJDYLBConfig { get; set; }
        public XMZCJDYLB(string key) 
        {
            loginKey = key;
            this.InitConfig();
        }
        public void InitConfig(){
            var xmlPath =System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"bin\\Common\\XMZCJDYLBConfig.xml");
            XMZCJDYLBConfig = Infrastructure.CommonFuntion.DeserialalizeXML<XMZCJDYLBConfig>(xmlPath);
        }

        private DataTable CreateTempTable(int count) 
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < count+1; i++)
            {
                dt.Columns.Add(new DataColumn("T"+i));
            }
            return dt;
        }

        public string GetExportPath(string dtEndYaer, string dtStartYear,string mType, out string fileName,out string message)
        {
            fileName = "";
            message = "";
            try
            {
               
                string tempalte = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], "xmzxjdylb.xls");
                
                var data = Load(dtEndYaer, dtStartYear, mType, out message);
                string filePath = ExportExcel.Export(data, tempalte, 2);
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {

                message = ex.Message ;
                return "";
            }
          
        }
        private string DWKey { get; set; }
        private bool GetDWKey() 
        {
            string strDwKey = "select dwkey from ss_Dw where guid in(select GUID_Data from ss_dataauthset where (GUID_RoleOrOperator in (select guid from ss_operator  where operatorkey='" + loginKey + "')or GUID_RoleOrOperator in (select guid_role from ss_roleoperator where guid_operator in (select guid from ss_operator  where operatorkey='" + loginKey + "')) )and classId=1 and IsDefault=1)";
            var dt=  DataSource.ExecuteQuery(strDwKey);
            if (dt == null||dt.Rows.Count==0) {
                return false;
            }
            DWKey = dt.Rows[0][0].ToString();
            return true;
        }
        private DataTable GetLoadData(int endYear)
        {
           
            int ANPAICOL = 20 + endYear - 2006;
            var tempTable = CreateTempTable(ANPAICOL);
            string ProKey="";
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string strSQL = "select * from CZZXJDMain where year(docdate)='" + endYear + "' order by docid";
            DataTable dt = DataSource.ExecuteQuery(strSQL,connStr);
            ANPAICOL = ANPAICOL - 1;

            if (dt.Rows.Count == 0)
            {   
               
                strSQL = "select t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16,t17,t18,t19,t20,t21,t22,t23,t24,t25 from CZZXJDMain where year(docdate)='" + (endYear) + "' order by docid";
               
            }
            if (dt.Rows.Count > 0)
            {

              
                for (int i = 0; i < dt.Rows.Count;i++ )
                {
                    DataRow rowNew =tempTable.NewRow();
                    var row = dt.Rows[i];
                    ProKey = string.IsNullOrEmpty(row["t1"]+"")?"":row["t1"].ToString();
                    if (dic.Keys.Contains(ProKey))
                    {
                        dic[ProKey] = i;
                    }
                    else 
                    {
                        dic.Add(ProKey, i);
                    }
                    rowNew[0] = string.IsNullOrEmpty(row["t1"] + "") ? "" : row["t1"].ToString();
                    rowNew[1] = string.IsNullOrEmpty(row["t2"] + "") ? "" : row["t2"].ToString();
                    for (int j = 2; j < 8; j++)
                    {
                        string rowName = "t" + (j + 1).ToString();
                        rowNew[j] = string.IsNullOrEmpty(row[rowName] + "") ? "0.00" : double.Parse(row[rowName].ToString()).ToString("##0.00");
                    }
                    for (int h = 8; h < 25; h++)
                    {
                        
                        string rowName = "t" + (h + 1).ToString();
                        if (h == 17) {
                            rowNew[h + 1] = row[rowName];
                            continue;
                        }
                        rowNew[h + 1] = string.IsNullOrEmpty(row[rowName] + "") ? "0.00" : double.Parse(row[rowName].ToString()).ToString("##0.00");
                    }
                    double t1= string.IsNullOrEmpty(row["t8"] + "") ? 0: double.Parse(row["t8"].ToString());
                    double t2=string.IsNullOrEmpty(row["t9"] + "") ? 0: double.Parse(row["t9"].ToString());
                    rowNew[8] = (t1 - t2).ToString("##0.00");
                    if (i==90) {
                        var a = 1;
                    }
                    rowNew[ANPAICOL-1] = string.IsNullOrEmpty(row["t25"] + "") ? "0.00" : double.Parse(row["t25"].ToString()).ToString("##0.00"); ;
                    tempTable.Rows.Add(rowNew);

                }
            }
            if (endYear >= 2013) 
            {
                string strsql = "select t1,t15 from CZZXJDMain where year(docdate)='" +(endYear - 1) + "' order by docid";
                var dtTemp = DataSource.ExecuteQuery(strsql, connStr);
                if (dtTemp.Rows.Count> 0) 
                {
                    foreach (DataRow row in dtTemp.Rows)
                    {
                        ProKey = string.IsNullOrEmpty(row["t1"]  +"")?"":row["t1"].ToString();
                        if (dic.ContainsKey(ProKey)) {
                            tempTable.Rows[dic[ProKey]][14] = string.IsNullOrEmpty(row["t15"] + "") ? "0.00" : double.Parse(row["t15"].ToString()).ToString("##0.00");
                        }
                    }
                }
            }
            return tempTable;
        }
        public DataTable Load(string dtEndYaer,string dtStartYear,string mType,out string msgError) 
        {
            msgError="";
            int mtype = 10000;
            int.TryParse(mType, out mtype);
            int  cYear =DateTime.Now.Year;
            DateTime dtEnd,dtStart;
            if (string.IsNullOrEmpty(dtEndYaer) || !DateTime.TryParse(dtEndYaer, out dtEnd) || !DateTime.TryParse(dtStartYear, out dtStart)) return null;
            int sYear = dtEnd.Year;
            DataTable dt = GetLoadData(sYear);

            if (!GetDWKey()) {
                msgError ="请设置默认单位！";
                return null;
            }
            string UFDataBase = this.IFZTs.GetZTKey(sYear);
            if (string.IsNullOrEmpty(UFDataBase)) 
            {
                msgError =sYear+ "帐套不存在,请重新选择";
                return null;
            }
            int iPeriod = DataSource.GetPeriod(UFDataBase, dtStart);
            double col16SumValue = 0;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr[0].ToString() == "2008zd03")
                {
                  //  var hanyuxing = 1;
                }
                var al=new  System.Collections.ArrayList();
                //本年支出
                string strsql = ProBNZC(dr[0].ToString(), UFDataBase, dtEnd.ToShortDateString(), dtStart.ToShortDateString(), iPeriod,dtEnd.Year);
                //var dtBNZC = DataSource.ExecuteQuery(strsql);
                al.Add(strsql);
                string strsql1 = ProBNJK(dr[0].ToString(), UFDataBase, dtEnd.ToShortDateString(), dtStart.ToShortDateString(), iPeriod, dtEnd.Year);
                al.Add(strsql1);
                //var dtBNJK = DataSource.ExecuteQuery(strsql1);
                string strsql2 = ProYQJK(dr[0].ToString(), UFDataBase, dtEnd.ToShortDateString(), dtStart.ToShortDateString(), iPeriod, dtEnd.Year);
                al.Add(strsql2);
                //var dtYQJK = DataSource.ExecuteQuery(strsql2);
                var ds = DataSource.ExeSql(al,connStr);
                DataTable dtBNZC = ds.Tables[0], dtBNJK = ds.Tables[1], dtYQJK = ds.Tables[2];
                if (dtBNZC.Rows.Count > 0)
                {
                    dr[13] = string.IsNullOrEmpty(dtBNZC.Rows[0][0] + "") ? "0.00" : double.Parse(dtBNZC.Rows[0][0].ToString()).ToString("##0.00");
                }
                else 
                {
                    dr[13] = "0.00";
                }
                //本年借款
              
                if (dtBNJK.Rows.Count > 0)
                {
                    double t1 = string.IsNullOrEmpty(dtBNJK.Rows[0][0] + "") ? 0.00 : double.Parse(dtBNJK.Rows[0][0].ToString());
                    double t2 = string.IsNullOrEmpty(dtBNJK.Rows[1][0] + "") ? 0.00 : double.Parse(dtBNJK.Rows[1][0].ToString());
                    dr[10] = (t1+t2).ToString("##0.00");
                }
                else
                {
                    dr[10] = "0.00";
                }
                //以前年度借款
             
                if (dtYQJK.Rows.Count > 0)
                {
                    double t1 = string.IsNullOrEmpty(dtYQJK.Rows[0][0] + "") ? 0.00 : double.Parse(dtYQJK.Rows[0][0].ToString());
                    double t2 = string.IsNullOrEmpty(dtYQJK.Rows[1][0] + "") ? 0.00 : double.Parse(dtYQJK.Rows[1][0].ToString());
                    dr[11] = (t1 + t2).ToString("##0.00");
                }
                else
                {
                    dr[11] = "0.00";
                }
                //本年安排
                
                RetrieveBNAP(sYear, dtStart, dtEnd, dr);
                dr[12] = (double.Parse(dr[10].ToString()) + double.Parse(dr[11].ToString())).ToString("##0.00");
                dr[15] = (double.Parse(dr[13].ToString()) + double.Parse(dr[14].ToString())).ToString("##0.00");
                dr[9] = (double.Parse(dr[12].ToString()) + double.Parse(dr[15].ToString())).ToString("##0.00");
                dr[7] = GetAPHJ(dr, dt.Columns.Count);
                dr[8] = (double.Parse(dr[7].ToString()) - double.Parse(dr[9].ToString())).ToString("##0.00");
                if (double.Parse(dr[7].ToString()) != 0)
                {
                    double t100= (((double.Parse(dr[12].ToString()) + double.Parse(dr[15].ToString())) / double.Parse(dr[7].ToString())));
                    col16SumValue+=t100;
                    dr[16] = t100.ToString("P");
                    dr[17] = ((double.Parse(dr[15].ToString()) / double.Parse(dr[7].ToString())) * 1.0000).ToString("P");
                }
                else {
                    double t100 = double.Parse(dr[16].ToString());
                    col16SumValue += t100;
                    dr[16] = double.Parse(dr[16].ToString()).ToString("P");
                    dr[17] = double.Parse(dr[17].ToString()).ToString("P");
                }
               
            }

            HZCount(1, 1, ref dt, col16SumValue,mtype);
            return dt;
        }
        //给安排合计
        private string GetAPHJ(DataRow dr,int colCount) 
        {
            double d = 0;
            for (int i = 19; i < colCount-1; i++)
            {
                double dTemp=0;
                double.TryParse(dr[i].ToString(), out dTemp);
                d = dTemp + d;
            }
           return d.ToString("##0.00");
        }
        private void HZCount(int row, int col,ref DataTable dt ,double t16SumValue,int mtype) 
        {
            double cuountTemp = 0;
            DataRow dr = dt.NewRow();
            dr[0] = "";
            dr[1] = "总计";
            for (int i = 2; i < dt.Columns.Count; i++) 
            {
                dr[i] = 0;
               
            }
            dt.Rows.Add(dr);
           
            for (int i = 2; i < dt.Columns.Count; i++)
            {
                if (i != 17)
                {
                    int j = 0;
                    for (; j < dt.Rows.Count; j++)
                    {
                        double t1;
                        if (double.TryParse(dt.Rows[j][i].ToString(), out t1))
                        {
                            cuountTemp = cuountTemp + t1;
                            dt.Rows[j][i] = (t1 / mtype).ToString("##0.00");
                        }
                    }
                    if (i == 16)
                    {
                        if (dt.Rows.Count - 4 > 0 && ((dt.Rows.Count - 5) != 0))
                        {
                            dt.Rows[j - 1][i] = (t16SumValue / (dt.Rows.Count - 2)).ToString("P");
                            //Cell.SetCellDouble j, i, 0, Format(countTemp / (Cell.GetRows(0) - 5), "0000.####")
                        }
                    }
                    else
                    {
                        dt.Rows[j - 1][i] = (cuountTemp / mtype).ToString("##0.00"); ;
                    }
                }
                cuountTemp = 0;
               
            }
            double t = 0.00;
            var rowLast = dt.Rows[dt.Rows.Count - 1];
            double.TryParse(rowLast[7].ToString(), out t);
            if (t > 0 || t < 0)
            {
               
                rowLast[17] = (double.Parse(rowLast[15].ToString()) / double.Parse(rowLast[7].ToString())).ToString("P");
            }
        }
        //本年支出
        private string ProBNZC(string strfile, string UFDataBase, string DTPEnd, string DTPStart, int i, int Year)
        {
            string sql = "";
            if (Year > 2013)
            {
                var sqlConfig = XMZCJDYLBConfig.SqlConfig.FirstOrDefault(e => e.Year == Year);
                if (sqlConfig != null)
                {
                    sql = string.Format(sqlConfig.BNZCSql, UFDataBase, strfile, DTPStart, DTPEnd);
                }
            }
            else { 
                sql = string.Format("select sum(md) from   {0}..gl_accvouch where citem_id='{1}' and ccode like '501%'   and dbill_date between '{2}' and '{3}'", UFDataBase, strfile, DTPStart, DTPEnd);
            }
            return sql;
        }
        //本年借款
        private string ProBNJK(string strfile, string UFDataBase, string DTPEnd, string DTPStart, int i, int Year) 
        {
            string sql = "";
            if (Year > 2013)
            {
                var sqlConfig = XMZCJDYLBConfig.SqlConfig.FirstOrDefault(e => e.Year == Year);
                if (sqlConfig != null)
                {
                    sql = string.Format(sqlConfig.BNJKSql, UFDataBase,i, strfile, DTPStart, DTPEnd);
                }
               
            }
            else 
            {
                string str1 = string.Format(" select sum(mb) from {0}..gl_accmultiass where  iperiod={1} and citem_id='{2}' and (ccode like '11002%' or ccode like '11004%')", UFDataBase, i, strfile);
                string str2 = string.Format(" select SUM(MD)-SUM(MC) from {0}..gl_accvouch  where citem_id='{1}' and (ccode like '11004%' or ccode like '11002%')    and dbill_date between '{2}' and '{3}'", UFDataBase, strfile, DTPStart, DTPEnd);
                sql = string.Format(" {0} Union All {1}", str1, str2);
            }
            return sql;
        }
        //以前借款
        private string ProYQJK(string strfile, string UFDataBase, string DTPEnd, string DTPStart, int i, int Year)
        {
            string sql = "";
            if (Year > 2013) 
            {
                var sqlConfig = XMZCJDYLBConfig.SqlConfig.FirstOrDefault(e => e.Year == Year);
                if (sqlConfig != null)
                {
                    sql = string.Format(sqlConfig.YQJKSql, UFDataBase, i, strfile, DTPStart, DTPEnd);
                }
            }
            else
            {
                string strsql1 = string.Format(" select sum(mb) from {0}..gl_accmultiass where  iperiod={1} and citem_id='{2}' and ccode like '1100304%'", UFDataBase, i, strfile);
                string strsql2 = string.Format(" select  (sum(md)-sum(mc)) from  {0}..gl_accvouch where citem_id='{1}' and ccode like '1100304%'   and dbill_date between '{2}' and '{3}'", UFDataBase, strfile, DTPStart, DTPEnd);
                sql= string.Format(" {0} Union All {1}", strsql1, strsql2);
            }
            return sql;
        }
        //获得本年安排的SqlFormat
        private string ProBNAP(int year) {
            string sqlFormat = "";
            if (year > 2013)
            {
                var sqlConfig = XMZCJDYLBConfig.SqlConfig.FirstOrDefault(e => e.Year == year);
                if (sqlConfig != null)
                {
                    sqlFormat = sqlConfig.BNAPSql;
                }
            }
            else 
            {
                sqlFormat="select (sum(mc)-sum(md)) as a,{0} as b from {1}..gl_accvouch where citem_id = '{2}' and ccode like '401%' and dbill_date between '{3}' and '{4}'";
            }
            return sqlFormat;
            
        }
        //获得本年安排的值

        private void RetrieveBNAP(int sYear, DateTime dtStart, DateTime dtEnd, DataRow dr)
        {
            int ANPAICOL;
            //Dim ANPAICOL As Integer '2006年的安排是第20列依次类推,2013年只看2012年的安排
            int curYear;
            string ufstr;
            string startDate = dtStart.ToShortDateString();
            string endDate = dtEnd.ToShortDateString();
         
            StringBuilder sb = new StringBuilder();
            for (int h = 2011; h <= sYear; h++)
            {
                curYear = h;
                ANPAICOL = 20 + (h - 2006);
                ufstr = this.IFZTs.GetZTKey(h);
                if (!string.IsNullOrEmpty(ufstr))
                {
                    if (h < sYear)
                    {
                        startDate = curYear + ".01.01";
                        endDate = curYear + ".12.31";
                    }
                    else {
                        startDate = dtStart.ToShortDateString();
                        endDate = dtEnd.ToShortDateString();
                    }
                    string strSQLFormat = ProBNAP(curYear);
                    //string strSQLFormat1 = "select (sum(mc)-sum(md)) as a," + curYear + " as b from " + ufstr 
                    //    + "..gl_accvouch where citem_id = '" + dr[0].ToString() + "'and ccode like '401%' and dbill_date >= '"
                    //    + startDate + "' and dbill_date <= '" + endDate + "'";
                    
                    sb.AppendLine(string.Format(strSQLFormat,curYear,ufstr, dr[0].ToString(),startDate,endDate));
                    if (h!=sYear) sb.AppendLine("union all");
                }
            }
            
            try
            {
                var dtTemp = DataSource.ExecuteQuery(sb.ToString());
                foreach (DataRow dritem in dtTemp.Rows)
                {
                    curYear = int.Parse(dritem["b"].ToString());
                    ANPAICOL = 20 + (curYear - 2006);
                    dr[ANPAICOL - 1] = string.IsNullOrEmpty(dritem[0] + "") ? "0.00" : double.Parse(dritem[0].ToString()).ToString("##0.00");
                }
            }
            catch (Exception ex)
            {
                //dr[ANPAICOL - 1] = 0.00;
            }
        }

        private List<IFZT> CreateIFZTs()
        {
            
            
            //string strSQL = "select * from IF_DW_ZT where cDWKey=(select cDWKey from personview where cPerKey='" + loginKey + "') and cZTKey in (select name from master..sysdatabases)";
            string strSQL = "select * from IF_DW_ZT where cDWKey='" + DWKey + "' and cZTKey in (select name from master..sysdatabases)";
            var dt = DataSource.ExecuteQuery(strSQL, connStr);
            List<IFZT> result = new List<IFZT>();
            foreach (DataRow dr in dt.Rows)
            {
                IFZT item = new IFZT();
                item.iYear = dr["iYear"].ToString();
                item.cDWKey = dr["cDWKey"].ToString();
                item.cZTKey = dr["cZTKey"].ToString();
                result.Add(item);
            }
            return result;
        }
    }

    public class IFZT
    {
        public string iYear { get; set; }
        public string cDWKey { get; set; }
        public string cZTKey { get; set; }
    }

    public static class ExtendClass
    {
        public static string GetZTKey(this List<IFZT> objs, string iYear)
        {
            IFZT item = objs.Find(e => e.iYear.Trim() == iYear.Trim());
            return item == null ? "" : item.cZTKey.Trim();
        }
        public static string GetZTKey(this List<IFZT> objs, int iYear)
        {
            return objs.GetZTKey(iYear.ToString());
        }
    }

    [Serializable]
    [XmlRoot("Config")]
    public class XMZCJDYLBConfig
    {
        [XmlElement("SqlConfig")]
        public List<SqlConfig> SqlConfig { get; set; }
    }
    public class SqlConfig 
    {
         [XmlAttribute("Year")]
        public int Year { get; set; }
         [XmlAttribute("本年支出Sql")]
        public string BNZCSql { get; set; }
         [XmlAttribute("本年借款Sql")]
        public string BNJKSql { get; set; }
         [XmlAttribute("以前借款Sql")]
        public string YQJKSql { get; set; }
         [XmlAttribute("本年安排Sql")]
        public string BNAPSql { get; set; }
    }

}
