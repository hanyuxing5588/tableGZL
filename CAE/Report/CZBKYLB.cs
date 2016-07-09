using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.IO;
using CAE.Expression;
using System.Collections;

namespace CAE
{
    /// <summary>
    /// 财政拨款一览表(国家测绘局)
    /// </summary>
    public class CZBKYLB
    {
        public string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
        public DateTime StartDate = DateTime.Now;
        
        public DateTime EndDate = DateTime.Now;
        
        public string OperatorKey = string.Empty;
        
        public string OperatorName = string.Empty;

        public int Qualitiy = 1;
        
        private DataTable _resulttable = null;
        
        public DataTable ResultTable
        {
            get
             { 
                if (_resulttable == null) _resulttable = CreateDataTable();
                return _resulttable;
            }
        }

        private string _UFDataBase = string.Empty;

        public string UFDataBase
        {
            get
            {
                if (_UFDataBase == string.Empty) _UFDataBase = DataSource.GetUFDataBase(OperatorKey, EndDate.Year,DWKey);
                return _UFDataBase;
            }
        }

        private ExpressionParser _Parser = null;

        public ExpressionParser Parser
        {
            get
            {
                if (_Parser == null) _Parser = new ExpressionParser();
                return _Parser;
            }
        }

        private Dictionary<string, string> _ProUFProMap = null;

        public Dictionary<string, string> ProUFProMap
        {
            get
            {
                if (_ProUFProMap == null) _ProUFProMap = getProUFProMap();
                return _ProUFProMap;
            }
        }

        public int Period = -1;

        private DataTable _yhdzd = null;

        public DataTable 银行对账单

        {
            get
            {
                if (_yhdzd == null) _yhdzd = Get银行对账单();
                return _yhdzd;
            }
        }

        private List<Project> _projects = null;

        public List<Project> Projects
        {
            get
            {
                if (_projects == null) _projects = this.GetProjects();
                return _projects;
            }
        }

        private formula _iformula = null;

        public virtual formula Iformula
        {
            get
            {
                    if (this.EndYearH == 2014)
                    {
                        _iformula = formula.Create1();
                    }
                    else 
                    {
                        _iformula = formula.Create();
                    }
                return _iformula;
            }
        }
        /// <summary>
        /// 非金额类列
        /// </summary>
        public List<string> NotMoneyColumns = new List<string>() { "t1", "t19", "t20", "t21", "t22" };
        /// <summary>
        /// 百分比列
        /// </summary>
        public List<string> PercentColumns = new List<string>() { "t6", "t7", "t9", "t17" };

        public CZBKYLB() { }
        public CZBKYLB(string OperatorKey, string OperatorName, DateTime StartDate, DateTime EndDate)
        {
            this.OperatorKey = OperatorKey;
            this.OperatorName = OperatorName;
            this.StartDate = StartDate;
            this.EndDate = EndDate;
            DataSource.connStr = System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
         
        }
        public CZBKYLB(string OperatorKey, string OperatorName, DateTime StartDate, DateTime EndDate,int Quanlity):this(OperatorKey,OperatorName,StartDate,EndDate)
        {
            this.Qualitiy = Quanlity;
        }
        public int EndYearH { get; set; }
        private string DWKey { get; set; }
        public bool GetDWKey()
        {
            string strDwKey = "select dwkey from ss_Dw where guid in(select GUID_Data from ss_dataauthset where (GUID_RoleOrOperator in (select guid from ss_operator  where operatorkey='" + OperatorKey + "')or GUID_RoleOrOperator in (select guid_role from ss_roleoperator where guid_operator in (select guid from ss_operator  where operatorkey='" + OperatorKey + "')) )and classId=1 and IsDefault=1)";
            var dt = DataSource.ExecuteQuery(strDwKey);
            if (dt == null || dt.Rows.Count == 0)
            {
                return false;
            }
            DWKey = dt.Rows[0][0].ToString();
            return true;
        }
        public ReportResult GetReport()
        {
            ReportResult result = new ReportResult();
            if (!GetDWKey()) {
                result.Error = true;
                result.ErrorMessage = "请设置默认单位！";
                return result;
            }
            if (this.UFDataBase == string.Empty)
            {
                result.Error = true;
                result.ErrorMessage = "未设置单位数据库对应关系！";
                return result;
            }
            result.Error = !DataSource.ContainsDatabaseCatalog(this.UFDataBase);
            if (result.Error)
            {
                result.ErrorMessage = EndDate.Year + "年帐套不存在，请重新选择！";
                return result;
            }
            this.EndYearH = EndDate.Year;
            //获得会计期间
            this.Period = DataSource.GetPeriod(UFDataBase,this.StartDate);
            string msg = "";
            var falg=LoadPreset(ref msg);
            if (!falg) {
                result.Error = true;
                result.ErrorMessage = msg;
                return result;
            }
            //从数据库获取值
            if (this.Iformula.sqlf != null) this.Iformula.sqlf.Fill(this);
            //计算行合计            RowHJ();
            //计算列之间的公式
            if (this.Iformula.colf != null) this.Iformula.colf.Fill(this);

         

            //今年可用额度特殊处理t13
            foreach (DataRow dr in ResultTable.Rows)
            {
                object t5 = dr["t5"]; //本年安排投资
                if (t5 != null && !string.IsNullOrEmpty(t5.ToString()) && double.Parse(t5.ToString())>0)
                {
                }
                else
                {
                    dr["t13"] = 0;
                }
            }
            foreach (DataRow dr in ResultTable.Rows)
            {
                foreach (DataColumn dc in ResultTable.Columns)
                {
                    if (this.NotMoneyColumns.Contains(dc.ColumnName)) continue;
                    object kk = dr[dc.ColumnName];
                    double ii;
                    if (kk == null)
                    {
                        ii = 0;
                    }
                    else
                    {
                        if (!double.TryParse(kk.ToString(), out ii)) ii = 0;
                    }
                    if (this.PercentColumns.Contains(dc.ColumnName))
                    {
                        ii =Convert.ToDouble(ii.ToString("0.0000"));
                        dr[dc.ColumnName] = ii.ToString("P");
                    }
                    else
                    {
                        //金额单位转换
                        ii = ii / this.Qualitiy;
                        dr[dc.ColumnName] = ii.ToString("0.00");
                    }
                }
            }
            result.Result = ResultTable;
            return result;
        }

        /// <summary>
        /// 创建存储报表数据的临时表
        /// </summary>
        /// <returns></returns>
        private DataTable CreateDataTable()
        {
            DataTable result = new DataTable();
            for (int i = 1; i <= 22; i++)
            {
                result.Columns.Add("t" + i, typeof(string));
            }
            DataColumn[] pcols = new DataColumn[] { result.Columns["t21"] };
            result.PrimaryKey = pcols;
            return result;
        }
        public DataTable dt17YKJHZXBL = null;
        //循环查找数据
        public DataTable ForFindData()
        {
            DataTable dt = new DataTable();
            int iBefYear,  iBefMonth;
            int iCurYear, iCurMonth;
            int iMonth = EndDate.Month, iYear = EndDate.Year;
            if ((13 - EndDate.Month) == 12)
            {
                iBefMonth = 12;
                iBefYear = EndDate.Year - 1;
            }
            else
            {
                iBefMonth = EndDate.Month - 1;
                iBefYear = EndDate.Year;
            }
            string strSql = "select * from XMZXJDYLBB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth + "' order by ID";
            dt= DataSource.ExecuteQuery(strSql);
            iCurYear = iYear;
            iCurMonth = iMonth;
            if (dt.Rows.Count == 0)
            {
                for (int i = 0; i < iBefMonth; i++)
                {
                    strSql = "select * from XMZXJDYLBB where year(docdate)='" + iBefYear + "' and month(docdate)='" + (iBefMonth-i) + "' order by ID";
                    dt=DataSource.ExecuteQuery(strSql);
                    if (dt.Rows.Count != 0)
                    {
                        iCurYear = iBefMonth;
                        iCurMonth = iBefMonth - i;
                        break;
                    }
                    if ((iBefMonth-i) == 1) break;
                }
            }
            //2014 .4 .11
            if (dt.Rows.Count > 0) {
                string strSql1 = "select * from dbo.internal_CZBKXMZXJD  where iyear='" + iYear + "' and imonth='" + iMonth + "' order by icellrowindex";
                this.dt17YKJHZXBL = DataSource.ExecuteQuery(strSql1);
            }
            //
            return dt;
          
        }
        /// <summary>
        /// 加载报表中预置数据        /// </summary>
        public virtual bool LoadPreset(ref string msg)
        {
            DataTable dt= ForFindData();
            if (dt.Rows.Count == 0) {
                msg = "找不到当年当月的的数据";
                return false;
            }
            int i=0;
            this.Iformula.pro.Order();
            if (this.Iformula.pro.rows.Count == dt.Rows.Count)
            {
                for (int m = 0; m < dt.Rows.Count; m++)
                {
                    var row = dt.Rows[m];
                    string ProName = row["t1"].ToString().Trim();
                    crow citem = this.Iformula.pro.rows[i];
                    if (citem != null)
                    {

                        DataRow item = ResultTable.NewRow();
                        item["t1"] = row["t1"].ToString().Replace("\\", "\\\\");
                        item["t20"] = citem.zjxz;
                        item["t21"] = citem.key;
                        item["t22"] = citem.level;
                        item["t2"] = row["t2"];
                        item["t3"] = row["t3"];
                        item["t4"] = row["t4"];
                        item["t18"] = row["t21"];
                        //2014 4 11 张龙 反馈 该 银行额度支出 用款计划数 用款计划执行比率
                        item["t14"] = row["t14"];
                        item["t16"] = row["t16"];
                        item["t17"] = this.dt17YKJHZXBL.Rows.Count == 0 || this.dt17YKJHZXBL.Rows.Count<=m? "" : this.dt17YKJHZXBL.Rows[m]["ivalue"];
                        //
                        System.Diagnostics.Trace.WriteLine(ProName + "====" + citem.key);
                        ResultTable.Rows.Add(item);
                    }
                    i++;
                }
                return true;
            }
            else 
            {
                int iMonth = EndDate.Month, iYear = EndDate.Year;
                string deleteSql = string.Format("delete from XMZXJDYLBB where  year(docdate)={0} and month(docdate)={1}", iYear, iMonth);
                DataSource.ExeSql(deleteSql);
                msg = "请当前年当年月的数据与系统配置不一致,请重新查询，手动录入数据";
                return false;
            }
        }

        /// <summary>
        /// 获得本地项目编号到用友库中项目编号的映射
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> getProUFProMap()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string strsql = "select distinct(cProKey) as a,cUFProKey as b from IF_Pro where iyear=" + this.StartDate.Year + " order by cProkey";
            var dt = DataSource.ExecuteQuery(strsql);
            foreach (DataRow dr in dt.Rows)
            {
                string p1 = dr["a"].ToString();
                string p2 = dr["b"].ToString();
                if (!result.ContainsKey(p1)) result.Add(p1, p2);
            }
            return result;
        }

        public string GetUFProKey(string ProKey)
        {
            ProKey = ProKey.Trim();
            return this.ProUFProMap.ContainsKey(ProKey) ? this.ProUFProMap[ProKey].Trim() : "";
        }

        private DataTable Get银行对账单()
        {
            bool bLastDay = EndDate.AddDays(1).Day == 1 ? true : false;
            int iMonth = EndDate.Month, iYear = EndDate.Year;
            if (!bLastDay)
            {
                if ((13 - EndDate.Month) == 12)
                {
                    iMonth = 12;
                    iYear = EndDate.Year - 1;
                }
                else
                {
                    iMonth = EndDate.Month - 1;
                    iYear = EndDate.Year;
                }
            }
            string strSQL = "select * from YHHZDZDB where year(docdate)='" + iYear + "' and month(docdate)='" + iMonth +"' order by docid";
            return DataSource.ExecuteQuery(strSQL);
        }

        private List<Project> GetProjects()
        {
            string strSql = "select cProKey,cProName,ZJXZKey,cProClassKey,cParentKey from project where iyear=" + this.StartDate.Year;
            var dt= DataSource.ExecuteQuery(strSql);
            List<Project> result = new List<Project>();
            foreach (DataRow dr in dt.Rows)
            {
                Project item = new Project();
                item.cProKey = dr["cProKey"] == null ? "" : dr["cProKey"].ToString();
                item.cProName = dr["cProName"] == null ? "" : dr["cProName"].ToString();
                item.ZJXZKey = dr["ZJXZKey"] == null ? "" : dr["ZJXZKey"].ToString();
                item.cProClassKey = dr["cProClassKey"] == null ? "" : dr["cProClassKey"].ToString();
                item.cParentKey = dr["cParentKey"] == null ? "" : dr["cParentKey"].ToString();
                result.Add(item);
            }
            return result;
        }

        public virtual string GetExportPath(out string fileName)
        {
            var data = GetReport();
            string template="czbkszylb.xls";
            if (this.EndYearH == 2014)
            {
                template= "czbkszylb2014.xls";
            }
            template = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], template);
            fileName = "";
          
            string filePath = ExportExcel.Export(data.Result, template, 2, 1, new List<ExcelCell>() { new ExcelCell() { Col = 13, Row = 0, Value = this.EndDate.ToShortDateString() } });
            fileName = Path.GetFileName(filePath);
            return filePath;
        }

        /// <summary>
        /// 行合计
        /// </summary>
        private void RowHJ()
        {
            int rowscount=ResultTable.Rows.Count;
            //一般行合计
            for (int lv = 2; lv <= 5; lv++)
            {
                for (int h = 0; h < ResultTable.Columns.Count; h++)//hanyx
                {
                    DataColumn dc = ResultTable.Columns[h];//hanyx
                    if (this.NotMoneyColumns.Contains(dc.ColumnName)) continue;
                    int startAutoSum = 0;
                    for (int i = startAutoSum; i < rowscount-1; i++)
                    {
                        

                        DataRow dr = ResultTable.Rows[i];
                        //过滤掉不合计的行
                        if (this.Iformula.srs != null && this.Iformula.srs.Contains(dr.ProKey(), dc.ColumnName)) continue;

                        int drlevel = dr.Level();
                        if (drlevel != lv) continue;
                        double tvalue=0;
                        bool ok = false;
                        for (int j = i + 1; j < rowscount - 1; j++)
                        {
                            DataRow ndr = ResultTable.Rows[j];
                            int nlevel = ndr.Level();
                            if (nlevel == (drlevel - 1))
                            {
                                double ivalue;
                                if (!double.TryParse(ndr[dc.ColumnName].ToString(), out ivalue)) ivalue = 0;
                                tvalue += ivalue;
                                ok = true;
                            }
                            else if (nlevel>=drlevel)
                            {
                                break;
                            }
                        }
                        if (ok) {
                            if (dc.ColumnName == "t14")
                            {
                                double dTemp = 0;
                                double.TryParse(dr[dc.ColumnName].ToString(),out dTemp);
                                if (dTemp < tvalue)
                                {
                                    dr[dc.ColumnName] = tvalue;
                                }
                            }
                            else 
                            {
                                dr[dc.ColumnName] = tvalue;
                            }
                        }
                    }
                }
            }
            //总合计
            DataRow totalrow = ResultTable.Rows[rowscount - 1];
            foreach (DataColumn dc in ResultTable.Columns)
            {
                if (this.NotMoneyColumns.Contains(dc.ColumnName)) {
                    if (dc.ColumnName != "t1")
                    {
                        totalrow[dc.ColumnName] = "";
                    }
                    else 
                    {
                        totalrow[dc.ColumnName] = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + totalrow[dc.ColumnName];
                    }
                    continue; 
                }
                bool ok = false;
                double tvalue = 0;
                for (int i = 0; i < rowscount - 1; i++)
                {
                    DataRow dr = ResultTable.Rows[i];
                    int drlevel = dr.Level();
                    if (drlevel == 5)
                    {
                        double ivalue;
                        if (!double.TryParse(dr[dc.ColumnName].ToString(), out ivalue)) ivalue = 0;
                        tvalue += ivalue;
                        ok = true;
                    }
                }
                if (ok) totalrow[dc.ColumnName] = tvalue;
            }
        }

        public bool Save(List<CczylbModel> listModel,string date,string unit,ref string msg) 
        {
            DateTime dt;
            if (!DateTime.TryParse(date, out dt)) 
            {
                msg = "查询日期错误，不能保存";
                return false;
            }
            int uniti = 0;
            if(!int.TryParse(unit,out uniti)){
                msg = "查询单位错误，不能保存";
                return false;
            }
            if (listModel == null || listModel.Count == 0) {
                msg = "没有要保存的数据";
                return false;
            }
            try
            {
                string delsql = string.Format("delete from XMZXJDYLBB where year(DocDate)='{0}' and month(DocDate)='{1}'", dt.Year, dt.Month);
                string del17Colsql = string.Format("delete from internal_CZBKXMZXJD where iyear={0} and imonth={1}", dt.Year, dt.Month);
                System.Collections.ArrayList al = new System.Collections.ArrayList();
                al.Add(delsql);
                al.Add(del17Colsql);
                string insertSqlformat = "insert into XMZXJDYLBB("+
                        "t1,t2,t3,t4,t5,t6,t7,t8,t9,t10,t11,t12,t13,t14,t15,t16,t19,t20,t21,DocDate,ID)" +
                "values('{0}',{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},'{17}','{18}','{19}','{20}')";
                string insertSql17 = "insert into internal_CZBKXMZXJD(imonth,iyear,icellrowindex,ivalue) values ({0},{1},{2},{3})";
                for(int i=0;i<listModel.Count;i++)
                {
                    var item=listModel[i];
                    string sql = string.Format(insertSqlformat,
                        item.t1.Replace("&nbsp;", ""), GetTrim(item.t2), GetTrim(item.t3), GetTrim(item.t4), GetTrim(item.t5), 0.00, 0.00, GetTrim(item.t8), 0.00, GetTrim(item.t10), 
                        GetTrim(item.t11), GetTrim(item.t12), GetTrim(item.t13), GetTrim(item.t14), GetTrim(item.t15), GetTrim(item.t16),GetTrim(item.t20), item.t21,item.t22,dt.ToShortDateString(), i + 1);
                    al.Add(sql);
                    string sqlTemp = string.Format(insertSql17, dt.Month, dt.Year, i + 3, GetDouble(item.t17));
                    al.Add(sqlTemp);
                }
                DataSource.ExecuteNonQueryLst(ref al);
                return true;
            }
            catch (Exception ex)
            {
                return false ;
            }

        }
        public double GetDouble(string strT) 
        {
            double d=0.00;
            if (strT.Contains("%"))
            {
                strT = strT.Replace("%", "");
                double.TryParse(strT, out d);
                d = d / 100;
            }
            else 
            {
                double.TryParse(strT, out d);
                d=d>1? d / 100:d;
            }
            return d;
        }

        public string GetTrim(string val)
        {
            return string.IsNullOrEmpty(val.Trim()) ? "0.00" : val;

        }


     
    }
    public class CZBKYLBImport 
    {
        public ImportClass importClass = new ImportClass();
        //银行对账单导入
        public string GetUploadFileData(string filePath,DateTime dt)
        {
            //返回计划列表项字段
            string message = string.Empty;
            ImportBankDZD(filePath, out message);
            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }
            int year = dt.Year, month = dt.Month;
            var listSql = new ArrayList() ;//{ string."update   XMZXJDYLBB set t14=0 where year(docdate)='{0}' and month(docdate)='{1}'" };
            var sql = string.Format("update   XMZXJDYLBB set t14=0 where year(docdate)='{0}' and month(docdate)='{1}'", dt.Year, dt.Month);
            listSql.Add(sql);
            string strFormat = " update  XMZXJDYLBB set t14='{3}' where Id='{0}' and year(docdate)='{1}' and month(docdate)='{2}'";
            double sum = 0;
            for (int i = 0; i < importClass.Rows.Count; i++)
			{
                sum +=(importClass.Moneys[i] + importClass.Adds[i]);
			    listSql.Add(string.Format(strFormat,importClass.Rows[i],year,month,importClass.Moneys[i]+importClass.Adds[i]));
            }
            listSql.Add(string.Format("update  XMZXJDYLBB set t14='{0}' where Id in (select top 1 Id from XMZXJDYLBB where year(docdate)='{1}' and month(docdate)='{2}' order by Id desc )", sum,year,month));
            try 
	        {	        
		       DataSource.ExecuteNonQueryLst(ref listSql);
	        }
	        catch (Exception ex)
	        {
		        message=ex.Message;
	        }
            return message;
        }
        public void ImportBankDZD(string filePath, out string msg)
        {
            msg = "";
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(filePath);
                Aspose.Excel.Worksheet curWs = excel.Worksheets[0];
                TakeData(curWs);
                ////获得Excel的列
                //var colNames = GetColNames(curWs);
                ////根据列创建表
                //dt = CreateDataTable(colNames);
                ////插入数据
                //var dt123=new DataTable();
                //TakeData(ref dt123, curWs);
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
            }
        }
        public  void TakeData(Aspose.Excel.Worksheet curWs) 
        {
            //开始行
            int startRow = 5;
            int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CZYLBImportStartRow"] + "", out startRow);
            string strCols = System.Configuration.ConfigurationManager.AppSettings["CZYLBImportStartCols"] + "";
            if (strCols == "") {
                strCols = "8,11,12";
            }
            var listStrCol = strCols.Split(',');
            var listCol = new List<int>() ;
            foreach (var item in listStrCol)
            {
                listCol.Add(int.Parse(item));
            }
            //取值列 默认顺序 值 增加值  行
           
            var cols = curWs.Cells.Columns;
            var rows = curWs.Cells.Rows;
            for (int j = startRow; j < rows.Count; j++)
            {
                var cellValue3 = (curWs.Cells[j, listCol[2]].Value + "").Trim().ToString();//默认为"";
                if (string.IsNullOrEmpty(cellValue3)) continue;
                importClass.Rows.Add(cellValue3);

                var cellValue1 = (curWs.Cells[j,listCol[0]].Value + "").Trim().ToString();//默认为"";
                double d = 0;
                double.TryParse(cellValue1, out d);
                importClass.Moneys.Add(d);
                var cellValue2 = (curWs.Cells[j,listCol[1]].Value + "").Trim().ToString();//默认为"";
                double.TryParse(cellValue2, out d);
                importClass.Adds.Add(d);
               
            }
            }
    }

    public class ImportClass 
    {
        public ImportClass() {
            Moneys = new List<double>();
            Rows = new List<string>();
            Adds = new List<double>();
        }
        public List<double> Moneys { get; set; }
        public List<string> Rows { get; set; }
        public List<double> Adds { get; set; }
    }
    /// <summary>
    /// 扩展方法类    /// </summary>
    public static class ExtendClsss
    {

        public static string ProKey(this DataRow obj)
        {
            return obj["t21"].ToString().Trim();
        }

        public static string ProName(this DataRow obj)
        {
            return obj["t1"].ToString().Trim();
        }

        public static string Zjxz(this DataRow obj)
        {
            return obj["t20"].ToString().Trim();
        }

        public static int Level(this DataRow obj)
        {
            int i = 0;
            int.TryParse(obj["t22"].ToString().Trim(),out i);
            return i;
        }
    }

    public class Project
    {
        public string cProKey { get; set; }
        public string cProName { get; set; }
        public string ZJXZKey { get; set; }
        public string cProClassKey { get; set; }
        public string cParentKey { get; set; }
    }

    [Serializable]
    [XmlRoot("formula")]
    public class formula
    {
        [XmlElement("sqlformula")]
        public sqlformula sqlf { get; set; }
        [XmlElement("colformula")]
        public colformula colf { get; set; }
        [XmlElement("projectset")]
        public projectset pro { get; set; }
        [XmlElement("skiprowsum")]
        public skiprowsum srs { get; set; }
        
        public static formula Create()
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
        public static formula Create1()
        {
            string basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = basepath.EndsWith("\\") ? basepath + "bin\\Common\\CZBKYLBFormula2014.xml" : basepath + "\\bin\\Common\\CZBKYLBFormula2014.xml";
            XmlSerializer xs = new XmlSerializer(typeof(formula));

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            formula mFormula = new formula();
            mFormula = xs.Deserialize(fs) as formula;
            fs.Close();
            return mFormula;
        }
    }

    public class sqlformula
    {
        [XmlElement("ccol")]
        public List<ccol> cols { get; set; }

        public void Fill(CZBKYLB value)
        {
            if (cols == null || cols.Count == 0) return;
            DataTable ResultTable = value.ResultTable;
            foreach (ccol icol in cols)
            {   
                string colkey = icol.key;
                //4.11 张龙 不可设置 自己手工录入
                if (colkey == "t14") continue;
                //
                string colvalue = icol.value;
                if (colkey == null || colkey == string.Empty) continue;
                if (colvalue != null && colvalue != string.Empty)
                {
                    foreach (DataRow row in ResultTable.Rows)
                    {
                        row[icol.key] = icol.RetrieveValue(value, value.GetUFProKey(row.ProKey()), row.Zjxz());
                    }
                }


                foreach (crow irow in icol.rows)
                {
                    string prokey = irow.key.Trim();
                    string sqlstatement = irow.value.Trim();
                    if (prokey == null || sqlstatement == null || prokey == string.Empty || sqlstatement == string.Empty) continue;
                    DataRow drow = ResultTable.Rows.Find(prokey);
                    if (drow == null) continue;
                    drow[icol.key] =irow.RetrieveValue(value, value.GetUFProKey(drow.ProKey()), drow.Zjxz());
                }
            }
        }
    }

    public class colformula
    {
        [XmlElement("ccol")]
        public List<ccol> cols;

        public void Fill(CZBKYLB value)
        {
            if (cols == null || cols.Count == 0) return;
            DataTable ResultTable = value.ResultTable;
            
            foreach (ccol icol in cols)
            {
                string colkey = icol.key;
                string colvalue = icol.value;
                if (colkey == null || colkey == string.Empty) continue;
                if (colvalue != null && colvalue != string.Empty)
                {
                    foreach (DataRow row in ResultTable.Rows)
                    {
                        row[colkey] = icol.RetrieveValue(value.Parser, ResultTable.Columns, row);
                    }
                }


                foreach (crow irow in icol.rows)
                {
                    string prokey = irow.key.Trim();
                    string valuef = irow.value.Trim();
                    if (prokey == null || valuef == null || prokey == string.Empty || valuef == string.Empty) continue;
                    DataRow drow = ResultTable.Rows.Find(prokey);
                    if (drow == null) continue;
                    drow[icol.key] =icol.RetrieveValue(value.Parser, ResultTable.Columns, drow);
                }
            }
            
        }
    }

    public class rowformula
    {
        [XmlElement("crow")]
        public List<crow> rows;
    }

    public class skiprowsum
    {
        [XmlElement("skipitem")]
        public List<skipitem> items;
        public bool Contains(string rowkey,string colkey) 
        {
            if (items == null) return false;
            colkey=colkey.Trim();
            rowkey=rowkey.Trim();
            return items.Find(e => e.colkey.Trim() == colkey && (e.rowkey.Trim() == rowkey || e.rowkey == "*")) != null ? true : false;
        }
    }
  
    public class crow
    {
        [XmlAttribute]
        public string key;
        [XmlAttribute]
        public string value;
        [XmlAttribute]
        public string zjxz;
        [XmlAttribute("Type")]
        public string iType;
        [XmlAttribute]
        public string level;
        [XmlAttribute]
        public int index;
        public string RetrieveValue(CZBKYLB param,string ProKey,string ZJXZ)
        {
            if (iType == "1") return value;
            if (value == null || value == string.Empty || key == null || key == string.Empty) return "0";
            string cform = value;
            string iyear = param.EndDate.Year.ToString(), smonth = param.StartDate.Month.ToString(),
                   emonth = param.EndDate.Month.ToString(), sdate = param.StartDate.ToShortDateString(),
                   edate = param.EndDate.ToShortDateString();
            cform = cform.Replace("@year", iyear).Replace("@smonth", smonth).Replace("@emonth", emonth)
                   .Replace("@sdate", sdate).Replace("@edate", edate).Replace("@prokey", ProKey).Replace("@zjxz", ZJXZ)
                   .Replace("@ufdatabase", param.UFDataBase).Replace("@period", param.Period.ToString()).Replace("&lt;", "<").Replace("&gt;", ">");

            object rstvalue = DataSource.GetMoney(cform);
            rstvalue = rstvalue == null ? "0" : rstvalue;
            return rstvalue.ToString();
        }

        public string RetrieveValue(ExpressionParser Parser, DataColumnCollection columns, DataRow row)
        {
            if (value == null || value == string.Empty) return "0";
            string cform = value;
            for (int k = columns.Count - 1; k > 0; k--)
            {
                DataColumn dc = columns[k];
                string colname = "@" + dc.ColumnName;
                string value1 = row[dc.ColumnName].ToString();
                double money;
                if (double.TryParse(value1, out money))
                {
                    cform = cform.Replace(colname, money.ToString());
                }
                else
                {
                    cform = cform.Replace(colname, "0");
                }
            }
            object rstvalue = null;
            try
            {
                cform = cform.Replace("--", "+").Replace("+-", "-");
                Parser.Parser(cform, out rstvalue);
            }
            catch (Exception ex)
            {
                rstvalue = 0;
            }
            rstvalue = rstvalue == null ? "0" : rstvalue;
            return rstvalue.ToString();
        }
    }

    public class ccol
    {
        [XmlAttribute]
        public string key { get; set; }
        [XmlAttribute("Type")]
        public string iType { get; set; }
        [XmlAttribute]
        public string value { get; set; }
        [XmlElement("crow")]
        public List<crow> rows { get; set; }

        public string RetrieveValue(CZBKYLB param, string ProKey, string ZJXZ)
        {
            if (iType == "1") return value;
            if (value == null || value == string.Empty) return "0";
            string cform = value;
            string iyear = param.EndDate.Year.ToString(), smonth = param.StartDate.Month.ToString(),
                   emonth = param.EndDate.Month.ToString(), sdate = param.StartDate.ToShortDateString(),
                   edate = param.EndDate.ToShortDateString();
            cform = cform.Replace("@year", iyear).Replace("@smonth", smonth).Replace("@emonth", emonth)
                   .Replace("@sdate", sdate).Replace("@edate", edate).Replace("@prokey", ProKey).Replace("@zjxz", ZJXZ)
                   .Replace("@ufdatabase", param.UFDataBase).Replace("@period", param.Period.ToString()).Replace("&lt;","<").Replace("&gt;",">");

            object rstvalue = DataSource.GetMoney(cform);
            rstvalue = rstvalue == null ? "0" : rstvalue;
            return rstvalue.ToString();
        }

        public string RetrieveValue(ExpressionParser Parser,DataColumnCollection columns, DataRow row)
        {

            if (value == null || value == string.Empty) return "0";
            string cform = value;
            for(int k=columns.Count-1;k>0;k--)
            {
                DataColumn dc = columns[k];
                string colname = "@" + dc.ColumnName;
                string value1 = row[dc.ColumnName].ToString();
                double money;
                if (double.TryParse(value1, out money))
                {
                    cform = cform.Replace(colname, money.ToString());
                }
                else
                {
                    cform = cform.Replace(colname, "0");
                }
            }
            object rstvalue = null;
            try
            {
                cform = cform.Replace("--", "+").Replace("+-", "-");
                Parser.Parser(cform, out rstvalue);
            }
            catch (Exception ex)
            {
                rstvalue = 0;
            }
            double result1 = Convert.ToDouble(rstvalue);
            if (double.IsInfinity(result1) || double.IsNaN(result1) || double.IsNegativeInfinity(result1)) result1 = 0;
            
            return result1.ToString();
        }
    }

    public class projectset
    {
        [XmlElement("crow")]
        public List<crow> rows;

        public void Order()
        {
            if (rows != null)
            {
                rows.Sort((left, right) =>
                    {
                        if (left.index > right.index)
                            return 1;
                        else if (left.index == right.index)
                            return 0;
                        else
                            return -1;
                    }
                );
            }
        }

    }

    public class skipitem
    {
        [XmlAttribute]
        public string colkey { get; set; }
        [XmlAttribute]
        public string rowkey { get; set; }
    }

    public class CczylbModel 
    {
        public string t1 { get; set; }
        public string t2 { get; set; }
        public string t3 { get; set; }
        public string t4 { get; set; }
        public string t5 { get; set; }
        public string t6 { get; set; }
        public string t7 { get; set; }
        public string t8 { get; set; }
        public string t9 { get; set; }
        public string t10 { get; set; }
        public string t11{ get; set; }
        public string t12 { get; set; }
        public string t13 { get; set; }
        public string t14{ get; set; }
        public string t15 { get; set; }
        public string t16 { get; set; }
        public string t17 { get; set; }
        public string t18 { get; set; }
        public string t19 { get; set; }
        public string t20 { get; set; }
        public string t21 { get; set; }
        public string t22 { get; set; }
        public string t23 { get; set; }
        public string DocDate { get; set; }
        public int ID { get; set; }
    }
}
/*
 账面支出
 select a-b as c from (
          select sum(md)-sum(mc) as a from @ufdatabase..gl_accvouch  where citem_id='@prokey' and dbill_date&gt;='@sdate' and dbill_date&lt;='@edate'
          ) z left join (
          select sum(md)-sum(mc) as b from @ufdatabase..gl_accvouch  where citem_id='@prokey' and dbill_date&gt;='@sdate' and dbill_date&lt;='@edate' and (ccode like '1100202%' or ccode like '1100304%'  or ccode like '1100401%')
          ) k on 1=1
 */