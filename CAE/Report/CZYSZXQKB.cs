using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using CAE.Expression;

namespace CAE
{
    /// <summary>
    /// 财政执行预算情况表(国家测绘局)
    /// </summary>
    public class CZYSZXQKB:CZBKYLB
    {
        public CZYSZXQKB() { }
        public CZYSZXQKB(string OperatorKey, string OperatorName, DateTime StartDate, DateTime EndDate):base(OperatorKey,OperatorName,StartDate,EndDate) { }
        public CZYSZXQKB(string OperatorKey, string OperatorName, DateTime StartDate, DateTime EndDate,int qu) : base(OperatorKey, OperatorName, StartDate, EndDate, qu) { }
        private formula _iformula = null;
        public override formula Iformula
        {
            get
            {
                if (this.EndYearH == 2014)
                {
                    _iformula = Create1();
                }
                else
                {
                    _iformula =Create();
                }
                return _iformula;
            }
        }
        public double tryDouble(string str) 
        {
            double d=0.00;
            double.TryParse(str, out d);
            return d;
        }
        public void RemoveColumns(ref DataTable dt,bool isSJ=true) {
            if (dt == null) return ;
            var cloumns =new List<string>(){"t1","t5","t7","t17","t6","t8","t12","t13","t19","t22","t21"};
            var cloumnsRemove = new List<string>() ;
            var columnsOld = dt.Columns;
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                dr["t6"] = (tryDouble((dr["t7"] + "").Replace("%",""))/100 - tryDouble((dr["t17"] + "").Replace("%",""))/100).ToString("P");
                if (!isSJ) continue;
                var jc = dr["t22"];
                string str = "&nbsp;&nbsp;";
                int i = 0;
                if (int.TryParse((jc + "").ToString(), out i))
                {
                    i = 5 - i;
                }
                string strT = "";
                for (int j = 0; j < i; j++)
                {
                    strT += str;
                }
                dr[0] = strT + dr[0].ToString().Trim();
              
            }

            foreach (DataColumn item in columnsOld)
            {
                if (!cloumns.Contains(item.ColumnName))
                {
                    cloumnsRemove.Add(item.ColumnName);
                }
            }
            try
            {
                foreach (var item in cloumnsRemove)
                {
                    dt.Columns.Remove(item);
                }

            }
            catch (Exception exx)
            {
                
            }
          

        }
        public void FilterDataFor2014(ref DataTable dt) 
        {

            foreach (DataRow dr in dt.Rows)
            {
                //if (dr[0].ToString().Contains("住房改革支出"))
                //{
                //    var a = 1;
                //}
                dr["t19"] = "";
                var t15 = tryDouble((dr["t15"] + "").ToString());
                var t4 = tryDouble((dr["t4"] + "").ToString());
                var t16 = tryDouble((dr["t16"] + "").ToString());
                var t5 = tryDouble((dr["t5"] + "").ToString());

                var t7 = t4 == 0 ? 0 : (t15 / t4);
                var t17 = t5 == 0 ? 0 :(t16 / t5);
                dr["t7"] = t7.ToString("P");
                dr["t17"] = t17.ToString("P");
                dr["t6"] = (t7-t17).ToString("P");
                //第一行
                var jc = dr["t21"];
                string str = "&nbsp;&nbsp;";
                int i = 0;
                if (int.TryParse((jc + "").ToString(), out i))
                {
                    i = 5 - i;
                }
                string strT = "";
                for (int j = 0; j < i; j++)
                {
                    strT += str;
                }
                dr[0] = strT + dr[0].ToString().Trim();
              
            }
        }
        public ReportResult GetReport1(bool IsSJ=true)
        {
            if (EndDate.Year < 2014)
            {
                var c = this.GetReport();
                RemoveColumns(ref c.Result, IsSJ);
                return c;
            }
            else 
            {
                this.EndYearH = 2014;
                var dt = DataSource.ExecuteQuery("select *,'' as t17 from XMZXJDYLBB where year(docdate)=2014 and month(docDate)='" + EndDate.Month + "' order by Id");
                FilterDataFor2014(ref dt);
                var result = new ReportResult();
                result.Result = dt;
                return result;
            }
        }


        public  string GetExportPath(out string fileName,string rmbType)
        {
            var data = GetReport1(false);
            string template = "czbkszylb.xls";
            if (this.EndYearH == 2014)
            {
                template = "CZYSZXQKB2014.xls";
            }
            template = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], template);
            fileName = "";
            var rmbtypeName = GetRMBTypeName(rmbType);
            List<ExcelCell> excelCellList = new List<ExcelCell>();
            excelCellList.Add(new ExcelCell(8,1,this.EndDate.ToShortDateString()));//Col = 8, Row = 1, Value = this.EndDate.ToShortDateString()
            excelCellList.Add(new ExcelCell(7, 1, "单位：" + rmbtypeName));
            string filePath = ExportExcel.Export(data.Result, template, 4, new List<string>() { "t1","t5", "t7", "t17", "t6", "t8", "t12", "t13" }, excelCellList,0);
            fileName = Path.GetFileName(filePath);
            return filePath;
        }

        private string GetRMBTypeName(string rmbType)
        {
            string strRMB = "元";
            switch (rmbType)
            {
                case "10000":
                    strRMB = "万元";
                    break;
                case "1000":
                    strRMB = "千元";
                    break;
                default:
                    strRMB = "元";
                    break;
            }
            return strRMB;
        }
        public  formula Create()
        {
            string basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = basepath.EndsWith("\\") ? basepath + "bin\\Common\\CZBKYLBFormula.xml" : basepath + "\\bin\\Common\\CZBKYLBFormula.xml";
            XmlSerializer xs = new XmlSerializer(typeof(formula));

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            formula mFormula = new formula();
            mFormula = xs.Deserialize(fs) as formula;
            fs.Close();
            return mFormula;
        }
        public  formula Create1()
        {
            string basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = basepath.EndsWith("\\") ? basepath + "bin\\Common\\CZYSZXQKBFormula2014.xml" : basepath + "\\bin\\Common\\CZYSZXQKBFormula2014.xml";
            XmlSerializer xs = new XmlSerializer(typeof(formula));

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            formula mFormula = new formula();
            mFormula = xs.Deserialize(fs) as formula;
            fs.Close();
            return mFormula;
        }

        //public void RemoveColumnsT(ref DataTable dt, bool isSJ = true)
        //{
        //    if (dt == null) return;
        //    var cloumns = new List<string>() { "t1", "t5", "t7", "t17", "t6", "t8", "t12", "t13", "t19", "t22", "t21" };
        //    var cloumnsRemove = new List<string>();
        //    var columnsOld = dt.Columns;
        //    foreach (System.Data.DataRow dr in dt.Rows)
        //    {
        //        dr["t6"] = (tryDouble((dr["t7"] + "").Replace("%", "")) / 100 - tryDouble((dr["t17"] + "").Replace("%", "")) / 100).ToString("P");
        //        if (!isSJ) continue;
        //        var jc = dr["t22"];
        //        string str = "&nbsp;&nbsp;";
        //        int i = 0;
        //        if (int.TryParse((jc + "").ToString(), out i))
        //        {
        //            i = 5 - i;
        //        }
        //        string strT = "";
        //        for (int j = 0; j < i; j++)
        //        {
        //            strT += str;
        //        }
        //        dr[0] = strT + dr[0].ToString().Trim();

        //    }

        //    foreach (DataColumn item in columnsOld)
        //    {
        //        if (!cloumns.Contains(item.ColumnName))
        //        {
        //            cloumnsRemove.Add(item.ColumnName);
        //        }
        //    }
        //    try
        //    {
        //        foreach (var item in cloumnsRemove)
        //        {
        //            dt.Columns.Remove(item);
        //        }

        //    }
        //    catch (Exception exx)
        //    {

        //    }


        //}
        //public ReportResult GetReportT(bool IsSJ = true)
        //{
        //    var c = this.GetReport();
        //    RemoveColumns(ref c.Result, IsSJ);
        //    return c;
        //}
    
    }
    /*财政小表中的项目*/
    public class XM 
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Level { get; set; }
    }
    
    public class ChartLineData 
    {
        public ChartLineData() {
            DTBL = new List<double>();
            ZXBL = new List<double>();
            JHC = new List<double>();
        }
        public List<double> DTBL { get; set; }
        public List<double> ZXBL { get; set; }
        public List<double> JHC { get; set; }
        public string Error { get; set; }
    }
    public class ChartLineReport  
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int XMID { get; set; }
        public ChartLineReport() { }
        public ChartLineReport(int year,int month,int xmid) {
            this.Year = year;
            this.Month = month;
            this.XMID = xmid;
        }
        public double tryDouble(string str)
        {
            double d = 0.00;
            double.TryParse(str, out d);
            return d;
        }
        //循环查找数据
        public DataTable ForFindData(int year,int month)
        {
            DataTable dt = new DataTable();
            int iBefYear, iBefMonth;
            int iCurYear, iCurMonth;
            int iMonth = month, iYear = year;
            if ((13 - iMonth) == 12)
            {
                iBefMonth = 12;
                iBefYear = iYear - 1;
            }
            else
            {
                iBefMonth = iMonth - 1;
                iBefYear =iYear;
            }
            string strSql = "select t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth + "' and ID='"+XMID+"' order by ID";
            if (XMID == -1) {
                strSql = "select top 1 t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth + "'  order by ID desc";
            }
            dt = DataSource.ExecuteQuery(strSql);
            iCurYear = iYear;
            iCurMonth = iMonth;
            if (dt.Rows.Count == 0)
            {
                for (int i = 0; i < iBefMonth; i++)
                {
                    strSql = "select t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iBefYear + "' and month(docdate)='" + (iBefMonth - i) + "' and ID='" + XMID + "' order by ID";
                    if (XMID == -1)
                    {
                        strSql = "select top 1 t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iBefYear + "' and month(docdate)='" + (iBefMonth - i) + "'  order by ID desc";
                    }
                    dt = DataSource.ExecuteQuery(strSql);
                    if (dt.Rows.Count != 0)
                    {
                        iCurYear = iBefMonth;
                        iCurMonth = iBefMonth - i;
                        break;
                    }
                    if ((iBefMonth - i) == 1) break;
                }
            }
           
            return dt;

        }
        public void setValueFor2014(ref ChartLineData chartData,int month) 
        {
            DataTable dt = ForFindData(this.Year, month);
            if (dt != null && dt.Rows.Count > 0)
            {


                var t15 = tryDouble((dt.Rows[0]["t15"] + "").ToString());
                var t4 = tryDouble((dt.Rows[0]["t4"] + "").ToString());
                var t16 = tryDouble((dt.Rows[0]["t16"] + "").ToString());
                var t5 = tryDouble((dt.Rows[0]["t5"] + "").ToString());

                var t7 =Math.Round((t15 / t4)*100,2);
                var t17 = Math.Round((t16 / t5) * 100, 2);
                chartData.DTBL.Add(t7);
                chartData.ZXBL.Add(t17);
                chartData.JHC.Add(Math.Round((t7 - t17),2));
            }
        }

        public ChartLineData GetLineReport()
        {

            string msg = "";
            var chartData = new ChartLineData();
            for (int i = 1; i <= 12; i++)
            {
                if (i > Month)
                {
                    chartData.DTBL.Add(0.00);
                    chartData.ZXBL.Add(0.00);
                    chartData.JHC.Add(0.00);
                }
                else
                {
                    setValueFor2014(ref chartData, i);
                }
            }

            return chartData;
        }


        //未加载项目用的
        public List<XM> GetXMToRow(int year)
        {
            var listXM = new List<XM>() ;
            string strSQL = "select t1,ID,t20 from XMZXJDYLBB where year(docdate)='2014' and month(docdate)='" + 1 + "' order by ID";
            var dt = DataSource.ExecuteQuery(strSQL);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var xm = new XM() { Name = dt.Rows[i]["t1"].ToString(), Key = dt.Rows[i]["ID"].ToString() };
                if (xm.Name == "七、基建经费-职工宿舍（实存）" || dt.Rows[i]["t20"].ToString()=="2012sc") continue;
                if (dt.Rows.Count - 1 == i)
                {
                    xm.Name = "全部";
                    xm.Key = "-1";
                    listXM.Insert(0, xm);
                }
                else
                {
                    listXM.Add(xm);
                }
            }
            return listXM;
        }
    }

    public class ChartZhuData 
    {
        public ChartZhuData() 
        {
            JinEData = new List<double>();
            BLData = new List<double>();
        }
        public List<double> JinEData { get; set; }
        public List<double> BLData { get; set; }
        public string Error { get; set; }
    }
    public class ChartZhuReport
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int XMID { get; set; }
        public ChartZhuReport() { }
        public ChartZhuReport(int year, int month, int xmid)
        {
            this.Year = year;
            this.Month = month;
            this.XMID = xmid;
        }
        public double tryDouble(string str)
        {
            double d = 0.00;
            double.TryParse(str, out d);
            return d;
        }
        //循环查找数据
        public DataTable ForFindData(int year, int month)
        {
            DataTable dt = new DataTable();
            int iBefYear, iBefMonth;
            int iCurYear, iCurMonth;
            int iMonth = month, iYear = year;
            if ((13 - iMonth) == 12)
            {
                iBefMonth = 12;
                iBefYear = iYear - 1;
            }
            else
            {
                iBefMonth = iMonth - 1;
                iBefYear = iYear;
            }
            string strSql = "select t8,t12,t13,t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth + "' and ID='" + XMID + "' order by ID";
            if (XMID == -1)
            {
                strSql = "select top 1 t8,t12,t13,t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth + "'  order by ID desc";
            }
            dt = DataSource.ExecuteQuery(strSql);
            iCurYear = iYear;
            iCurMonth = iMonth;
            if (dt.Rows.Count == 0)
            {
                for (int i = 0; i < iBefMonth; i++)
                {
                    strSql = "select t8,t12,t13,t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iBefYear + "' and month(docdate)='" + (iBefMonth - i) + "' and ID='" + XMID + "' order by ID";
                    if (XMID == -1)
                    {
                        strSql = "select top 1 t8,t12,t13,t1,t15,t4,t16,t5 from XMZXJDYLBB where year(docdate)='" + iBefYear + "' and month(docdate)='" + (iBefMonth - i) + "'  order by ID desc";
                    }
                    dt = DataSource.ExecuteQuery(strSql);
                    if (dt.Rows.Count != 0)
                    {
                        iCurYear = iBefMonth;
                        iCurMonth = iBefMonth - i;
                        break;
                    }
                    if ((iBefMonth - i) == 1) break;
                }
            }

            return dt;

        }
        public void setValueFor2014(ref ChartZhuData chartData, int month)
        {
            DataTable dt = ForFindData(this.Year, month);
            if (dt != null && dt.Rows.Count > 0)
            {


                var t15 = tryDouble((dt.Rows[0]["t15"] + "").ToString());
                var t4 = tryDouble((dt.Rows[0]["t4"] + "").ToString());
                var t16 = tryDouble((dt.Rows[0]["t16"] + "").ToString());
                var t5 = tryDouble((dt.Rows[0]["t5"] + "").ToString());

                var t7 =t4==0?0: Math.Round((t15 / t4) * 100, 2);//本月执行比率
                var t17 =t5==0?0: Math.Round((t16 / t5) * 100, 2);//计划执行比率
                //t5 本年安排投资
                chartData.JinEData.Add(Math.Round(tryDouble((dt.Rows[0]["t5"] + "").ToString())/10000,2));
                //t15 当年额度支出
                chartData.JinEData.Add(Math.Round(tryDouble((dt.Rows[0]["t15"] + "").ToString())/10000,2));
                //t8 账面支出
                chartData.JinEData.Add(Math.Round(tryDouble((dt.Rows[0]["t8"] + "").ToString()) / 10000,2));
                //t12 应收款
                chartData.JinEData.Add(Math.Round(tryDouble((dt.Rows[0]["t12"] + "").ToString()) / 10000,2));
                //t13 今年可用额度
                chartData.JinEData.Add(Math.Round(tryDouble((dt.Rows[0]["t13"] + "").ToString()) / 10000, 2));

                //上月执行比率
                DataTable dtUpMonth = null;
                if ((month - 1) == 0)
                {
                    dtUpMonth = ForFindData(this.Year-1, 12);
                }
                else
                {
                    dtUpMonth = ForFindData(this.Year, month - 1);
                }
                //上月执行比率 
                var tUp15 = tryDouble((dtUpMonth.Rows[0]["t15"] + "").ToString());
                var tUp4 = tryDouble((dtUpMonth.Rows[0]["t4"] + "").ToString());
                var tUp7 =tUp4==0?0: Math.Round((tUp15 / tUp4) * 100, 2);//上月执行比率
                //上月执行比率
                chartData.BLData.Add(tUp7);
                //提高比率
                chartData.BLData.Add(Math.Round(t7-tUp7,2));
                //本月执行比率
                chartData.BLData.Add(t7);
                //计划执行率
                chartData.BLData.Add(t17);
                //差额
                chartData.BLData.Add(Math.Round((t7 - t17),2));
                
            }
        }

        public ChartZhuData GetZhuReport()
        {

            string msg = "";
            var chartData = new ChartZhuData();
            setValueFor2014(ref chartData, Month);
            return chartData;
        }


    }
   
}
