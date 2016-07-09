using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Business.Common;
using Business.CommonModule;
using BusinessModel;
using Infrastructure;
using System.IO;

namespace CAE.Report
{
    public class SaInfo
    {
        public int iActionMouth;
        public string strItemKey ;
        public string strItemName ;
        public string strItemType;
        public double dblItemValue;
        public string strColumns;
        public string DateTime;
        public string strItemString;
        public SaInfo()
        {
            iActionMouth = 0;
            strItemKey = "";
            strItemName = "";
            dblItemValue = 0F;
            strColumns = "";
            DateTime = "";
            strItemString = "";
            strItemType = "";
        }
    }
    public class LWFInfo
    {
        public double dblTotal;
        public string strTime;
        public string strMemo;
        public LWFInfo()
        {
            dblTotal = 0F;
            strTime = "";
            strMemo = "";
        }
    }
    public class GRSRCX:BaseReport
    {
        private string strLWF = "";
        private string strPerson = "";
        public string strHeadSql = "";
        public string strYear = "";
        public string strUserGuid = "";         // 操作员ID
        private string strPersonid = "";        // 操作员要查看的员工的id
        public string strPersonName = "";       // 操作员要查看的员工的名称
        public string strSaShowData = "";       // 工资数据
        private string strSaHeadColumns = "";   // 工资数据的表头
        private string strHzData = "";          // 收入汇总数据
        private string strReportData = "";      // 个人事项数据
        private string strLWFdata = "";         // 劳务费数据

        private Dictionary<string, string> DicFromula;
        public GRSRCX(string key)
            : base(key)
        {
          
        }

        public override void Init()
        {
            // 工资查询
            this.SqlFormat = "select actionMouth ,itemkey,itemname,itemtype,itemvalue,ItemDatetime,ItemString from dbo.SA_PlanActionDetailview "
                             + "where guid_person = '{0}' and actionyear = {1}  and GUID_PlanAction IN (SELECT GUID FROM dbo.SA_PlanAction WHERE ActionState=1)";
            // 工资项查询语句

            strHeadSql = "select distinct ItemName,ItemKey,ItemType  from SA_PlanActionDetailView where  ActionYear = {1} and guid_person = '{0}' and GUID_PlanAction IN (SELECT GUID FROM dbo.SA_PlanAction WHERE ActionState=1)  Order by ItemKey";

            // 查询操作员所对应的person数据
            strPerson = "select p.PersonName ,p.GUID ,d.IsDefault from dbo.SS_DataAuthSet d,ss_person p where " +
                        "guid_roleoroperator = '{0}' and classid = 3 and d.guid_data = p.guid";

            strLWF = "select m.guid ,d.Total_BX  ,m.DocDate ,m.DocMemo from bx_InviteFee d,bx_mainview m  where d.guid_bx_main = m.guid and m.doctypekey = '04' and d.GUID_InvitePerson = '{0}' and year(m.DocDate) = {1} and (m.DocState='-1' or m.DocState='999') order by m.DocDate ";
            this.tempalte = Path.Combine(this.tempalte, "grsrcx.xls");
        }

        public bool GetReport(string strYear, string strUserGuid,Dictionary<string,string> DicFromula,
            ref DataTable saShowTable,ref DataTable hzTable,ref DataTable reportTable,ref DataTable lwfData,ref string saColumns,ref string saShowTableData,
            ref string hzTableData,ref string reportTableData,ref string strLWFData,out string msgError)
        {
            this.strUserGuid = strUserGuid;
            this.strYear = strYear;
            this.DicFromula = DicFromula;
            string strErr = "";
            msgError = "";
            // 先查询操作员所对应的person
            bool bPerson = GetPerson(ref msgError);
            if (!bPerson)
            {
                return false;   
            }
            DataTable saTable = GetSaData(ref strErr);
            if(null == saTable)
            {
                msgError = "没有数据可以显示";
                return false;
            }

            DataTable headTable = GetHead(ref strErr);
            if (null == saTable)
            {
                msgError = "没有数据可以显示";
                return false;
            }
            // 获得工资数据，收入汇总数据，个人事项数据
            GetNewTable(ref saShowTable,ref hzTable,ref reportTable,ref saTable,ref headTable);
            saColumns = strSaHeadColumns;
            saShowTableData = strSaShowData;
            hzTableData = strHzData;
            reportTableData = strReportData;
            // 获取劳务费的信息
            GetLWFInfo(ref lwfData, ref strErr);
            strLWFData = this.strLWFdata;
            return true;
        }

        private void GetLWFInfo(ref DataTable table ,ref string strErr)
        {
            strLWF = string.Format(strLWF,strPersonid,this.strYear);
            DataTable dt = LoadData(strLWF, ref strErr);
            if (dt==null)
            {
                return;
            }
            DataRow[] pdr = dt.Select();
            int iLen = pdr.Length;
            Dictionary<string, LWFInfo> dic = new Dictionary<string, LWFInfo>();
            for (int i = iLen - 1; i >= 0; i--)
            {
                DataRow currRow = pdr[i];
                string strGUID = currRow["guid"].ToString();
                double dblTtotal_Bx = 0F;
                string strTotal = currRow["Total_BX"].ToString();
                if (strTotal!="")
                {
                    dblTtotal_Bx = Double.Parse(strTotal);
                }
                string strTime = currRow["DocDate"].ToString();
                string strDocMemo = currRow["DocMemo"].ToString();

                if (dic.ContainsKey(strGUID))
                {
                    dic[strGUID].dblTotal += dblTtotal_Bx;
                }
                else
                {
                    LWFInfo info = new LWFInfo();
                    info.dblTotal = dblTtotal_Bx;
                    info.strTime = strTime;
                    info.strMemo = strDocMemo;
                    dic.Add(strGUID,info);
                }
            }

            table.Columns.Add("name");
            table.Columns.Add("time");
            table.Columns.Add("money");
            table.Columns.Add("memo");

            string strRow = "";
            string strRows = "";
            double dblTotal = 0F;
            foreach(KeyValuePair<string,LWFInfo> pair in dic)
            {
                DataRow currRow = table.NewRow();
                DateTime time = DateTime.Parse(pair.Value.strTime);
                string strTime = time.ToShortDateString().ToString();
                currRow["name"] = strPersonName;
                currRow["time"] = strTime;
                string strMoney = FormatMoney(1, pair.Value.dblTotal);
                if(strMoney=="")
                {
                    strMoney = "0.00";
                }
                currRow["money"] = strMoney;
                dblTotal += pair.Value.dblTotal;
                currRow["memo"] = pair.Value.strMemo;
                table.Rows.Add(currRow);

                strRow = "{\\\"name\\\":\\\"" + strPersonName + "\\\",\\\"time\\\":\\\"" + strTime +
                    "\\\",\\\"money\\\":\\\"" + strMoney + "\\\",\\\"memo\\\":\\\"" + pair.Value.strMemo + "\\\"}";
                if (strRows == "")
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," + strRow;
                }
            }

            // 获得最后一行的合计
            DataRow lastRow = table.NewRow();
            lastRow["name"] = "合计";
            lastRow["time"] = "";
            string strTotalAll = FormatMoney(1, dblTotal);
            if(strTotalAll=="")
            {
                strTotalAll = "0.00";
            }
            lastRow["money"] = strTotalAll;
            lastRow["memo"] = "";
            table.Rows.Add(lastRow);

            strRow = "{\\\"name\\\":\\\"合计\\\",\\\"time\\\":\\\"\\\",\\\"money\\\":\\\"" + strTotalAll + "\\\",\\\"memo\\\":\\\"\\\"}";
            strRows = strRows + "," + strRow;
            strLWFdata = "{\\\"total\\\":" + (dic.Count+1).ToString() + ",\\\"rows\\\":[" + strRows + "]}";
        }
        private bool GetPerson(ref string strErr)
        {

            strPerson = string.Format(strPerson, this.strUserGuid);
            DataTable dt = LoadData(strPerson, ref strErr);
            if(dt==null)
            {
                strErr = "没有找到操作人员所对应的人员";
                return false;
            }
            DataRow[] pdr = dt.Select();
            int iLen = pdr.Length;

            for (int i = iLen-1; i >=0; i--)
            {
                DataRow currRow = pdr[i];
                this.strPersonid = currRow["GUID"].ToString();
                this.strPersonName = currRow["PersonName"].ToString();
                string strDefault = currRow["IsDefault"].ToString();
                if (strDefault == "True")
                {
                    break;
                }
            }

            return true;
        }
        // saShowTalbe 是界面上要展示的工资，totalTable 是个人收入汇总表 reportTable 是事项报告表
        private void GetNewTable(ref DataTable saShowTalbe,ref DataTable totalTable,ref DataTable reportTable,ref DataTable saTable,ref DataTable headTable)
        {
            // itemLis 按顺序存储工资项
            List<MyField> itemList = GetItemSort(ref headTable);
            // saInfoList 存储的是个人工资信息，杂乱无章
            List<SaInfo> saInfoList = GetSaInfo(ref saTable);
            if (saInfoList.Count==0)
            {
                return;
            }
            // 以月份做key ，以工资项做key,value是查询月这个工资项的总金额，已经是考虑到按星期发放时的汇总需要
            Dictionary<int, Dictionary<string, SaInfo>> gridInfo = GetGridInfo(ref saInfoList);
            // 获得每个工资的合计
            Dictionary<string, double> totalDic = GetTotal(ref gridInfo, ref itemList);
            GetNewTable(ref saShowTalbe, ref totalTable, ref reportTable, ref gridInfo, ref totalDic, ref itemList);
        }
        // gridInfo 已经按月份进行了排序
        private void GetNewTable(ref DataTable saShowTalbe, ref DataTable totalTable, ref DataTable reportTable, 
            ref Dictionary<int, Dictionary<string, SaInfo>> gridInfo, ref Dictionary<string, double> totalDic, ref List<MyField> itemList)
        { 
            // 先处理显示工资
            string strRow = "";
            string strRows = "";
            double dblGZs = 0F;     // 工资上半年  职务工资、级别工资之和   
            double dblGZx = 0F;     // 工资下半年  职务工资、级别工资之和
            double dblJTs = 0F;     // 津贴上半年  应发工资减去职务工资、级别工资
            double dblJTx = 0F;     // 津贴下半年  应发工资减去职务工资、级别工资

            double dblGZ = 0F;
            double dblJJ = 0F;
            double dblQT = 0F;
            double dblTotal = 0F;

            string strGZSR = DicFromula["GZSR"];
            string strJTQT = DicFromula["JTQT"];
            string strGZ = DicFromula["GZ"];
            string strJJ = DicFromula["JJ"];
            string strQT = DicFromula["QT"];

            // 获得事项报告表的数据
            dblGZ = GetValueEx(strGZ, ref totalDic);
            dblJJ = GetValueEx(strJJ,ref totalDic);
            dblQT = GetValueEx(strQT,ref totalDic);
            dblTotal = dblGZ + dblJJ + dblQT;
            // 组装页面上用于显示工资的table
            GetSaShowTable(ref saShowTalbe, ref gridInfo, ref itemList, ref totalDic);

            // 事项报告
            foreach(KeyValuePair<int,Dictionary<string, SaInfo>> monthPair in gridInfo)
            {
                Dictionary<string, SaInfo> saDic = monthPair.Value;

                // 获得个人收入汇总表的数据
                if (monthPair.Key<=6)
                {
                    dblGZs = dblGZs + GetValue(strGZSR, ref saDic);
                    dblJTs += GetValue(strJTQT, ref saDic);
                }
                else
                {
                    dblGZx = dblGZx + GetValue(strGZSR, ref saDic);
                    dblJTx += GetValue(strJTQT, ref saDic);
                }
                strRow = "{\\\"PersonName\\\":\\\"" + this.strPersonName + "\\\",\\\"Time\\\":\\\""+ GetShowTime(monthPair.Key) +"\\\"";
                // 根据工资项的最大集合进行拼接，二月比一月多发了一个工资项的工资，对于这种情况，一月份也要显示
                foreach(MyField item in itemList)
                {
                    double dblValue = 0F;
                    string strValue = "";
                    if (saDic.ContainsKey(item.strKey))
                    {
                        SaInfo info = saDic[item.strKey];
                        if (info.strItemType == "1")
                        {
                            dblValue = saDic[item.strKey].dblItemValue;
                            strValue = FormatMoney(1, dblValue);
                            if(strValue=="")
                            {
                                strValue = "0.00";
                            }
                        }
                        else if(info.strItemType=="2")
                        {
                            strValue = info.DateTime;
                        }
                        else
                        {
                            strValue = info.strItemString; 
                        }
                    }
                     
                    strRow = strRow + "," + "\\\"" + item.strKey + "\\\":\\\"" + strValue + "\\\"";
                }
                strRow += "}";
                if (strRows=="")
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," + strRow;
                }
            }
            // 最后一行的合计
            strRow = "{\\\"PersonName\\\":\\\"合计\\\",\\\"Time\\\":\\\"\\\"";
            foreach (MyField item in itemList)
            {
                double dblValue = 0F;
                string strValue = "";
                if (totalDic.ContainsKey(item.strKey))
                {
                    dblValue = totalDic[item.strKey];
                    strValue = FormatMoney(1, dblValue);
                    if (strValue == "")
                    {
                        strValue = "0.00";
                    }
                }
                else
                {
                    strValue = "";
                }
                strRow = strRow + "," + "\\\"" + item.strKey + "\\\":\\\"" + strValue + "\\\"";
            }
            strRow += "}";
            strRows = strRows + "," + strRow;

            strSaShowData = "{\\\"total\\\":" + (gridInfo.Count+1).ToString() + ",\\\"rows\\\":[" + strRows + "]}";
            // 个人收入汇总
            GetHZ(ref totalTable, dblGZs, dblGZx, dblJTs, dblJTx);
            // 个人事项表
            GetReportTable(ref reportTable, dblGZ, dblJJ, dblQT);
        }

        public string GetExportPath(DataTable saTable,DataTable hzTable,DataTable bgTable,DataTable lwfTable, out string fileName, out string message, ReportHeadModel reportHeadModel)
        {

            fileName = "";
            message = "";

            try
            {
                List<SheetData> SheetList = new List<SheetData>();
                int iSaCount = saTable.Rows.Count;
                int iHzCount = hzTable.Rows.Count;
                int iBgCount = bgTable.Rows.Count;
                int iLwfCount = lwfTable.Rows.Count;
                if(iSaCount>0)
                {
                    SheetData sd = new SheetData();
                    sd.index = 0;
                    sd.rowIndex = 0;
                    sd.colIndex = 0;
                    sd.cellList = new List<ExcelCell>();
                    sd.table = saTable;
                    SheetList.Add(sd);
                }

                if(iHzCount>0)
                {
                    SheetData sd = new SheetData();
                    sd.index = 1;
                    sd.rowIndex = 2;
                    sd.colIndex = 0;
                    sd.cellList = new List<ExcelCell>();
                    sd.table = hzTable;
                    SheetList.Add(sd);
                }

                if(iBgCount>0)
                {
                    SheetData sd = new SheetData();
                    sd.index = 2;
                    sd.rowIndex = 3;
                    sd.colIndex = 0;
                    sd.cellList = new List<ExcelCell>();
                    sd.table = bgTable;
                    SheetList.Add(sd);
                }

                if(iLwfCount>0)
                {
                    SheetData sd = new SheetData();
                    sd.index = 3;
                    sd.rowIndex = 1;
                    sd.colIndex = 0;
                    sd.cellList = new List<ExcelCell>();
                    sd.table = lwfTable;
                    SheetList.Add(sd);
                }
                string filePath = ExportExcel.Export(ref SheetList, this.tempalte);
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }
        }
        // 个人事项表
        private void GetReportTable(ref DataTable reportTable, double dblGZ, double dblJJ, double dblQT)
        {
            InitReportTable(ref reportTable);
            DataRow currRow = reportTable.NewRow();
            string strTotal = FormatMoney(1, dblGZ + dblJJ + dblQT);
            if(strTotal=="")
            {
                strTotal = "0.00";
            }

            string strGz = FormatMoney(1, dblGZ);
            if (strGz=="")
            {
                strGz = "0.00";
            }

            string strJJ = FormatMoney(1, dblJJ);
            if(strJJ=="")
            {
                strJJ = "0.00";
            }

            string strQT = FormatMoney(1, dblQT);
            if(strQT=="")
            {
                strQT = "0.00";
            }
            currRow["total"] = strTotal;
            currRow["gz"] = strGz;
            currRow["jj"] = strJJ;
            currRow["qt"] = strQT;
            reportTable.Rows.Add(currRow);
            string strRow = "{\\\"total\\\":\\\"" + strTotal + "\\\",\\\"gz\\\":\\\"" + strGz +
                "\\\",\\\"jj\\\":\\\"" + strJJ + "\\\",\\\"qt\\\":\\\"" + strQT + "\\\"}";
            strReportData = "{\\\"total\\\":1,\\\"rows\\\":[" + strRow + "]}";
        }
        private void InitReportTable(ref DataTable reportTable)
        {
            reportTable.Columns.Add("total");
            reportTable.Columns.Add("gz");
            reportTable.Columns.Add("jj");
            reportTable.Columns.Add("qt");
        }
        // 组装页面上显示个人收入汇总的表
        private void GetHZ(ref DataTable totalTable, double dblGZs, double dblGZx, double dblJTs, double dblJTx)
        {
            string strRow = "";
            string strRows = "";

            InitHz(ref totalTable);
            DataRow currRow = totalTable.NewRow();
            string strGZs = FormatMoney(1, dblGZs);
            string strJTs = FormatMoney(1, dblJTs);
            if(strGZs=="")
            {
                strGZs = "0.00";
            }

            if (strJTs=="")
            {
                strJTs = "0.00";
            }
            currRow["time"] = "上半年";
            currRow["shouru"] = strGZs;
            currRow["jintie"] = strJTs;
            totalTable.Rows.Add(currRow);
            strRow = "{\\\"time\\\":\\\"上半年\\\",\\\"shouru\\\":\\\"" + strGZs + "\\\",\\\"jintie\\\":\\\"" + strJTs + "\\\"}";            
            strRows = strRow;
            currRow = totalTable.NewRow();
            currRow["time"] = "下半年";
            string strGZx = FormatMoney(1, dblGZx);
            string strJTx = FormatMoney(1, dblJTx);
            if (strGZx=="")
            {
                strGZx = "0.00";
            }

            if(strJTx=="")
            {
                strJTx = "0.00";
            }
            currRow["shouru"] = strGZx;
            currRow["jintie"] = strJTx;
            totalTable.Rows.Add(currRow);
            strRow = "{\\\"time\\\":\\\"下半年\\\",\\\"shouru\\\":\\\"" + strGZx + "\\\",\\\"jintie\\\":\\\"" + strJTx + "\\\"}";
            strRows = strRows + "," + strRow;
            strHzData = "{\\\"total\\\":2,\\\"rows\\\":[" + strRows + "]}";
        }
        private void InitHz(ref DataTable totalTable)
        {
            totalTable.Columns.Add("time");
            totalTable.Columns.Add("shouru");
            totalTable.Columns.Add("jintie");
        }
        // 组装页面上显示工资的table
        private void GetSaShowTable(ref DataTable saShowTalbe, ref  Dictionary<int, Dictionary<string, SaInfo>> gridInfo, ref List<MyField> itemList, ref Dictionary<string, double> totalDic)
        {
            InitSaShowTable(ref saShowTalbe, ref itemList);
            // 由于列是动态的，因此，我们要把一行，也就是标题那一行放进来，不然，excel在导出的时候没有标题
            DataRow currRow = saShowTalbe.NewRow();
            // 先将第一行填充好
            currRow["PersonName"] = "人员名称";
            currRow["Time"] = "发放时间";
            foreach (MyField item in itemList)
            {
                string strItemName = item.strName;
                currRow[item.strKey] = item.strName;
            }
            saShowTalbe.Rows.Add(currRow);
            foreach (KeyValuePair<int, Dictionary<string, SaInfo>> monthPair in gridInfo)
            {
                Dictionary<string, SaInfo> saDic = monthPair.Value;
                currRow = saShowTalbe.NewRow();
                currRow["PersonName"] = this.strPersonName;
                currRow["Time"] = GetShowTime(monthPair.Key);
                foreach (MyField item in itemList)
                {
                    if (saDic.ContainsKey(item.strKey))
                    {
                        SaInfo info = saDic[item.strKey];
                        if (info.strItemType=="1")
                        {
                            string strMoney = FormatMoney(1, saDic[item.strKey].dblItemValue);
                            if(strMoney=="")
                            {
                                strMoney = "0.00";
                            }
                            currRow[item.strKey] = strMoney;
                        }
                        else if (info.strItemType == "2")
                        {
                            currRow[item.strKey] = info.DateTime;
                        }
                        else
                        {
                            currRow[item.strKey] = info.strItemString;
                        }
                    }
                    else
                    {
                        currRow[item.strKey] = "";
                    }
                }
                saShowTalbe.Rows.Add(currRow);
            }
            // 添加最后一行的数据
            currRow = saShowTalbe.NewRow();
            currRow["PersonName"] = "合计";
            currRow["Time"] = "";
            foreach (MyField item in itemList)
            {
                if (totalDic.ContainsKey(item.strKey))
                {
                    string strTotal = FormatMoney(1, totalDic[item.strKey]);
                    if(strTotal=="")
                    {
                        strTotal = "0.00";
                    }
                    currRow[item.strKey] = strTotal;
                }
                else
                { 
                    currRow[item.strKey] = "";
                }
            }
            saShowTalbe.Rows.Add(currRow);
        }
        private void InitSaShowTable(ref DataTable saShowTalbe, ref List<MyField> itemList)
        {
            saShowTalbe.Columns.Add("PersonName");
            saShowTalbe.Columns.Add("Time");
            foreach (MyField item in itemList)
            {
                saShowTalbe.Columns.Add(item.strKey);
            }
        }
        private double GetValueEx(string strFormula, ref Dictionary<string, double> totalDic)
        { 
            List<string> list = AnalyseFromula(strFormula);
            double dblValue = 0F;
            foreach (string str in list)
            {
                dblValue = 0F;
                string strReplace = "[" + str + "]";
                if (totalDic.ContainsKey(str))
                {
                    dblValue = totalDic[str];
                }
                string strValue = dblValue.ToString();
                strFormula = strFormula.Replace(strReplace, strValue);
            }
            dblValue = 0F;
            object obj = new object();
            Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
            ep.Parser(strFormula, out obj);
            dblValue = Double.Parse(obj.ToString());
            return dblValue;
        }
        private double GetValue(string strFormula, ref Dictionary<string, SaInfo> saDic)
        {
            List<string> list = AnalyseFromula(strFormula);
            double dblValue = 0F;
            foreach(string str in list)
            {
                dblValue = 0F;
                string strReplace = "["+ str +"]";
                if (saDic.ContainsKey(str))
                {
                    dblValue = saDic[str].dblItemValue;
                }
                string strValue = dblValue.ToString();
                strFormula = strFormula.Replace(strReplace, strValue);
            }
            dblValue = 0F;
            object obj = new object();
            Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
            ep.Parser(strFormula, out obj);
            dblValue = Double.Parse(obj.ToString());
            return dblValue;
        }
        // 解析公式
        private List<string> AnalyseFromula(string strFormula)
        {
            List<string> list = new List<string>();
            int iLen = strFormula.Length;
            int iStart = 0;
            int iEnd = 0;
            for (int i = 0; i < iLen;i++ )
            {
                char ch = strFormula[i];
                if (ch == '[')
                {
                    iStart = i;
                }
                else if(ch==']')
                {
                    iEnd = i;
                    string strSub = strFormula.Substring(iStart + 1, iEnd-iStart - 1);
                    list.Add(strSub);
                }
            }
            return list;
        }
        // 获得发放时间
        private string GetShowTime(int iMonth)
        { 
            string strMonth = "";
            if (iMonth >= 10)
            {
                strMonth = iMonth.ToString();
            }
            else
            {
                strMonth = "0" + iMonth.ToString();
            }
            string strShowTime = this.strYear + strMonth;
            return strShowTime;
        }
        // 获得每个工资项的汇总情况,统计合计内容
        private Dictionary<string, double> GetTotal(ref Dictionary<int, Dictionary<string, SaInfo>> gridInfo, ref List<MyField> itemList)
        {
            Dictionary<string, double> totalDic = new Dictionary<string, double>();  
            foreach (KeyValuePair<int, Dictionary<string, SaInfo>> monthPair in gridInfo)
            {
                Dictionary<string, SaInfo> saDic = monthPair.Value;
                foreach (KeyValuePair<string, SaInfo> saPair in saDic)
                {
                    SaInfo item = saPair.Value;
                    if(item.strItemType !="1")
                    {
                        continue;
                    }

                    if (totalDic.ContainsKey(saPair.Key))
                    {
                        totalDic[saPair.Key] += item.dblItemValue;
                    }
                    else
                    {
                        totalDic.Add(saPair.Key,item.dblItemValue);
                    }
                }
            }
            return totalDic;
        }
        private Dictionary<int, Dictionary<string,SaInfo>> GetGridInfo( ref List<SaInfo> saInfoList)
        {
            List<SaInfo> list = saInfoList.OrderBy(e=> e.iActionMouth).ToList();
            Dictionary<int, Dictionary<string,SaInfo>> GridInfo = new Dictionary<int,Dictionary<string,SaInfo>>();
            int iLen = list.Count;
            for (int i = 0; i < iLen;i++ )
            {
                SaInfo info = list[i];
                // 如果有这个月分的信息
                if (GridInfo.ContainsKey(info.iActionMouth))
                {
                    Dictionary<string, SaInfo> dic = GridInfo[info.iActionMouth];
                    // 如果这个月份的工资项里有这一项
                    if(dic.ContainsKey(info.strItemKey))
                    {
                        // 同一个月内相同的工资项相加
                        if (info.strItemType=="1")      // 等于1 是工资项
                        {
                            SaInfo tmp = dic[info.strItemKey];
                            tmp.dblItemValue = tmp.dblItemValue + info.dblItemValue;
                        }

                    }
                    else
                    {
                        dic.Add(info.strItemKey, info);
                    }
                }
                else
                {
                    Dictionary<string, SaInfo> dic = new Dictionary<string, SaInfo>();
                    dic.Add(info.strItemKey,info);
                    GridInfo.Add(info.iActionMouth, dic);
                }
            }
            return GridInfo;
        }
        
        private List<SaInfo> GetSaInfo(ref DataTable saTable)
        {
            List<SaInfo> list = new List<SaInfo>();
            DataRow[] pdr = saTable.Select();
            int iLen = pdr.Length;
            for (int i = 0; i < iLen; i++)
            {
                DataRow currRow = pdr[i];
                SaInfo info = new SaInfo();
                // 月
                string strActionMouth = currRow["actionMouth"].ToString();
                if (strActionMouth!="")
                {
                    info.iActionMouth = Int32.Parse(strActionMouth);
                }
                else
                {
                    info.iActionMouth = 0;
                }
                // itemkey
                info.strItemKey = currRow["itemkey"].ToString();
                // itemname
                info.strItemName = currRow["itemname"].ToString();

                // itemtype
                info.strItemType = currRow["itemtype"].ToString();
                // itemDateTime 
                info.DateTime = currRow["ItemDatetime"].ToString();
                if (info.DateTime!="")
                {
                    DateTime dt = DateTime.Parse(info.DateTime);
                    info.DateTime = dt.ToShortDateString();
                }
                
                // itemString
                info.strItemString = currRow["ItemString"].ToString();

                // itemvalue
                string strValue = currRow["itemvalue"].ToString();
                if(strValue=="")
                {
                    info.dblItemValue = 0F;
                }
                else
                {
                    info.dblItemValue = Double.Parse(strValue);
                }
                list.Add(info);
            }
            return list;
        }
        // 获得排序后的工资项的key,同时拼出列名
        private List<MyField> GetItemSort(ref DataTable headTable)
        {
            List<MyField> list = new List<MyField>();
            DataRow[] pdr = headTable.Select();
            int iLen = pdr.Length;
            string strRow = "";
            string strRows = "";
            for (int i = 0; i < iLen;i++ )
            {
                DataRow currRow = pdr[i];
                MyField field = new MyField();
                
                string strItemKey = currRow["ItemKey"].ToString();
                string strItemType = currRow["itemtype"].ToString();
                field.strKey = strItemKey;
                field.strName = currRow["ItemName"].ToString();
                list.Add(field);

                if (strItemType == "1")
                {
                    strRow = "{field:'" + currRow["ItemKey"].ToString() + "',title:'" + currRow["ItemName"].ToString() +
                    "',width:100, halign:'center',align: 'right',styler: function(value,row,index){return 'color:blue';}}";
                }
                else
                {
                    strRow = "{field:'" + currRow["ItemKey"].ToString() + "',title:'" + currRow["ItemName"].ToString() +
                    "',width:100, halign:'center',align: 'left'}";
                }

                if (strRows=="")
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," + strRow;
                }
            }
            //strRows = "{field:'PersonName',title:'人员名称',width:100,halign:'center',align: 'left',frozen:true},{field:'Time',frozen:true,title:'发放年月',width:100,halign:'center',align: 'left'}," + strRows;
            strRows = "[[" + strRows + "]]";
            strSaHeadColumns = strRows;
            return list;
        }
        private DataTable GetHead(ref string msgerror)
        {
            SetHeadSql();
            DataTable dt = LoadData(this.strHeadSql, ref msgerror);
            if (null == dt)
            {
                return null;
            }
            return dt;
        }
        private void SetHeadSql()
        {
            this.strHeadSql = string.Format(this.strHeadSql, this.strPersonid, this.strYear);
        }
        private DataTable GetSaData(ref string msgerror)
        {
            SetSaSql();
            DataTable dt = LoadData(this.SqlFormat, ref msgerror);
            if(null==dt)
            {
                return null;
            }
            return dt;
        }
        private  void SetSaSql() 
        {
            this.SqlFormat = string.Format(this.SqlFormat, this.strPersonid, this.strYear);
        }

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
    }

}
