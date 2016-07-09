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
    public struct MyField
    {
        public string strName;
        public string strTotal;
        public string strKey;
        public MyField(string strName, string strTotal,string strkey="")
        {
            this.strName = strName;
            this.strTotal = strTotal;
            this.strKey =  strkey;
        }
    }
    public struct DepInfo
    {
        public string strCount ;
        public string strDepName;
        public DepInfo(string strCount,string strDepName)
        {
            this.strCount = strCount;
            this.strDepName = strDepName;
        }

    }
    public class BMGZHZ : BaseReport
    {
        string strHeadSql = "";
        string strPersonCount = "";
        string strYear = "";
        string strSMonth = "";
        string strEMonth = "";
        string strDepartmentKey = "";
        string strPlanName = "";
        string mstrData = "";
        List<MyField> mFieldList = new List<MyField>(); 
        public BMGZHZ(string key)
            : base(key)
        {
          
        }
        public override void Init()
        {
            this.SqlFormat = "select departmentkey ,max(departmentname) departmentname,guid_plan,max(planname) planname,plankey,guid_item,max(itemname) " +
                               "itemname,itemkey,sum(Itemvalue) total_Item from SA_PlanActionDetailView   where PlanName = '{4}' And ActionYear = {0}  "+
                               "And ActionMouth>={1} and ActionMouth<={2}  And GUID_PlanAction IN (SELECT GUID FROM dbo.SA_PlanAction WHERE ActionState=1)  and departmentkey in{3} group by departmentkey,guid_plan,plankey,guid_item,itemkey  order by departmentkey,plankey ";

            strHeadSql = @"SELECT ItemName,ItemKey,ItemType FROM SA_PlanItemView a
LEFT JOIN dbo.SA_Plan b ON a.GUID_Plan=b.GUID
where   PlanName = '{0}' ORDER BY ItemOrder";
            strPersonCount = @"SELECT  departmentkey ,
        departmentname ,
        guid_department ,
        COUNT(*) as count
FROM    ( SELECT    GUID_Person ,
                    departmentkey ,
                    departmentname ,
                    guid_department
          FROM      SA_PlanActionDetailView
          WHERE     ActionYear = {0}
                    AND ActionMouth >={1} and ActionMouth<={2} and PlanName='{3}' 
                    And GUID_PlanAction IN (SELECT GUID FROM dbo.SA_PlanAction WHERE ActionState=1)
          GROUP BY  guid_department ,
                    departmentkey ,
                    departmentname ,
                    GUID_Person
        ) a
GROUP BY departmentkey ,
        departmentname ,
        guid_department
ORDER BY departmentkey";
            this.tempalte = Path.Combine(this.tempalte, "bmgzhz.xls");
        }
        public DataTable GetReport(string strYear,string strSMonth, string strEMonth, string strDepartmentKey, string strPlanName, ref string strColums,ref string strData, out string msgError)
        {
            this.strYear = strYear;
            this.strSMonth = strSMonth;
            this.strEMonth = strEMonth;
            this.strDepartmentKey = strDepartmentKey;
            this.strPlanName = strPlanName;
            string strErr = "";
            DataTable headTable = GetTableHead(ref strErr);            
            if(null == headTable)
            {
                msgError = "没有数据可以显示";
                return null;
            }

            DataTable personCountTable = GetPersonCountTable(ref strErr);
            if (null == personCountTable)
            {
                msgError = "没有数据可以显示";
                return null;
            }

            DataTable saTable = GetSaData(ref strErr);
            if (null == saTable)
            {
                msgError = "没有数据可以显示";
                return null;
            }

            DataTable newTable = new DataTable();
            GetTableReturn(ref newTable, ref headTable, ref personCountTable, ref saTable);
            msgError = "";
            strColums = GetColums();
            strData = GetData(ref newTable, ref mFieldList);
            return newTable;
        }
        private string GetData(ref DataTable newTable,ref List<MyField> saList)
        {
            DataRow[] pdr = newTable.Select();
            int iLen = pdr.Length;
            string strRows = "";
            for (int i = 1; i < iLen;i++ )
            {
                DataRow currRow = pdr[i];
                string strRow = "{\\\"DepartmentName\\\":\\\"" + currRow["DepartmentName"] + "\\\",\\\"PersonCount\\\":\\\"" + currRow["PersonCount"] + "\\\"";
                foreach (MyField item in saList)
                {
                    strRow = strRow + "," + "\\\"" + item.strKey + "\\\":\\\""+ currRow[item.strKey] +"\\\"";
                }
                strRow += "}";
                if (strRows == "")
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," + strRow;
                }
            }
            string strData = "{\\\"total\\\":" + iLen.ToString() + ",\\\"rows\\\":[" + strRows + "]}";
            return strData;
        }
        private string GetColums()
        {
            string strLink = "";
            foreach (MyField item in mFieldList)
            {
                string strColum = "{width:100,field:'" + item.strKey + "',title:'" + item.strName + "', halign:'center',align: 'right',styler: function(value,row,index){return 'color:blue';}}";
                if (strLink == "")
                {
                    strLink = strColum;
                }
                else
                {
                    strLink = strLink + "," + strColum;
                }
            }
            strLink = "{field:'DepartmentName',title:'部门',width:100,halign:'center',align: 'left'},{field:'PersonCount',title:'人数',width:100,halign:'center',align: 'left'}," + strLink;
            strLink = "[[" + strLink + "]]";
            return strLink;
        }
        // 根据三个table获得要显示的数据
        private DataTable GetTableReturn(ref DataTable newTable, ref DataTable headTable, ref DataTable personCountTable, ref DataTable saTable)
        {
            List<string> depList = GetDepartmentList(ref personCountTable);
            List<MyField> saList = GetItemList(ref headTable);
            Dictionary<string, DepInfo> personDic = GetPersonCount(ref personCountTable);
            Dictionary<string, Dictionary<string, MyField>> dataDic = GetData(ref saTable);
            GetNewTable(ref newTable, ref saList, ref depList, ref personDic, ref dataDic);
            return newTable;
        }
        // 获得要显示的数据
        private void GetNewTable(ref DataTable newTable, ref List<MyField> saList, ref  List<string> depList, ref Dictionary<string, DepInfo> personDic, ref Dictionary<string, Dictionary<string, MyField>> dataDic)
        {
            InitTable(ref newTable, ref saList);
            AddRows(ref newTable, ref saList, ref depList, ref personDic, ref dataDic);
        }
        // 添加数据
        private void AddRows(ref DataTable newTable, ref List<MyField> saList, ref  List<string> depList, ref Dictionary<string, DepInfo> personDic, ref Dictionary<string, Dictionary<string, MyField>> dataDic)
        {
            // 由于列是动态的，因此，我们要把一行，也就是标题那一行放进来，不然，excel在导出的时候没有标题
            DataRow currRow = newTable.NewRow();
            // 先将第一行填充好
            currRow["DepartmentName"] = "部门";
            currRow["PersonCount"] = "人数";
            foreach (MyField item in saList)
            {
                string strItemName = item.strName;
                currRow[item.strKey] = item.strName;
            }
            newTable.Rows.Add(currRow);
            int iPersonCount = 0;
            Dictionary<string, double> sumDic = new Dictionary<string, double>();
            // 以部门作为第一层循环
            foreach (KeyValuePair<string, DepInfo> kvp in personDic)
            {
                if (!dataDic.ContainsKey(kvp.Key))
                {
                    continue;
                }
                currRow = newTable.NewRow();
                string strDepKey = kvp.Key;
                DepInfo info = kvp.Value;
                string strCount = info.strCount;
                string strDepName = info.strDepName;
                currRow["DepartmentName"] = strDepName;
                currRow["PersonCount"] = strCount;
                iPersonCount = iPersonCount + Int32.Parse(strCount);
                // 找到部门所对应的工资情况
                if (dataDic.ContainsKey(strDepKey))
                {
                    Dictionary<string, MyField> dic = dataDic[strDepKey];
                    // 按照工资项的顺序添加
                    foreach (MyField item in saList)
                    {
                        string strItemKey = item.strKey;
                        if(dic.ContainsKey(strItemKey))
                        {
                            MyField field = dic[strItemKey];
                            currRow[strItemKey] = field.strTotal;
                            double dblTotal = 0;
                            if (field.strTotal!="")
                            {
                                dblTotal = Double.Parse(field.strTotal);
                            }
                            if (!sumDic.ContainsKey(strItemKey))
                            {
                                sumDic[strItemKey] = dblTotal;
                            }
                            else
                            {
                                sumDic[strItemKey] = dblTotal + sumDic[strItemKey];
                            }
                        }
                    }
                }
                newTable.Rows.Add(currRow);
            }

            // 添加最后一行数据
            currRow = newTable.NewRow();
            currRow["DepartmentName"] = "合计";
            currRow["PersonCount"] = iPersonCount.ToString();
            foreach (MyField item in saList)
            {
                string strItemKey = item.strKey;
                if(sumDic.ContainsKey(strItemKey))
                {
                    currRow[strItemKey] = FormatMoney(1,sumDic[strItemKey]);
                }        
            }
            newTable.Rows.Add(currRow);
        }
        // 初始化table
        private void InitTable(ref DataTable newTable,ref List<MyField> saList)
        {
            newTable.Columns.Add("DepartmentName");
            newTable.Columns.Add("PersonCount");
            foreach (MyField item in saList)
            {
                newTable.Columns.Add(item.strKey);
            }
        }
        // 获得部门的信息，关键是要一个排序的信息
        private List<string> GetDepartmentList(ref DataTable personCountTable)
        {
            List<string> list = new List<string>();
            DataRow[] pdr = personCountTable.Select();
            foreach (DataRow row in pdr)
            {
                list.Add((string)row["departmentKey"]);
            }
            return list;
        }
        // key是部门的key，value是工资项做key，金额做value
        private Dictionary<string, Dictionary<string, MyField>> GetData(ref DataTable saTable)
        {
            Dictionary<string, Dictionary<string, MyField>> dic = new Dictionary<string, Dictionary<string, MyField>>();
            DataRow[] pdr = saTable.Select();
            foreach (DataRow row in pdr)
            {
                string strDepKey = row["departmentkey"].ToString();
                string strItemKey = row["itemkey"].ToString();
                string strItemName = row["itemname"].ToString();
                string strTotal = "";
                double dblTotal = 0F;
                if(row["total_Item"].ToString() != "")
                {
                    double.TryParse(row["total_Item"].ToString(), out dblTotal);
                    strTotal = FormatMoney(1, dblTotal);
                }

                MyField item = new MyField(strItemName, strTotal);
                if (dic.ContainsKey(strDepKey))
                {
                    Dictionary<string, MyField> valueDic = dic[strDepKey];
                    if (!valueDic.ContainsKey(strItemKey))
                    {
                        valueDic.Add(strItemKey,item);
                    }
                }
                else
                {
                    Dictionary<string, MyField> valueDic = new Dictionary<string, MyField>();
                    valueDic.Add(strItemKey,item);
                    dic.Add(strDepKey, valueDic);
                }
            }

            return dic;
        }
        // 获得部门发工资的人数情况
        private Dictionary<string, DepInfo> GetPersonCount(ref DataTable personCountTable)
        {
            Dictionary<string, DepInfo> Dic = new Dictionary<string, DepInfo>();
            DataRow[] pdr = personCountTable.Select();
            foreach (DataRow row in pdr)
            {
                string strCount = row["count"].ToString();
                string strName = row["departmentname"].ToString();
                string strKey = row["departmentkey"].ToString();
                DepInfo item = new DepInfo(strCount,strName);
                Dic.Add(strKey,item);
            }
            return Dic;
        }

        // 获得工资项的排列情况
        private List<MyField> GetItemList(ref DataTable headTable)
        {
            List<MyField> list = new List<MyField>();
            DataRow[] pdr = headTable.Select();
            foreach (DataRow row in pdr)
            {
                MyField field = new MyField();
                field.strKey = row["ItemKey"].ToString();
                field.strName = row["ItemName"].ToString();
                list.Add(field);
            }
            mFieldList = list;
            return list;
        }

        // 设置查询语句
        private void SetSql()
        {
            string strKeyLink = GetDepartmentKey();

            this.SqlFormat = string.Format(this.SqlFormat,strYear,strSMonth,strEMonth,strKeyLink,strPlanName);
        }

        // 获得部门人数数据信息
        private DataTable GetPersonCountTable(ref string msgerror)
        {
            SetPersonCountSql();
            DataTable PersonCountTable = LoadData(strPersonCount, ref msgerror);
            if (null == PersonCountTable)
            {
                return null;
            }
            return PersonCountTable;
        }
        // 获得部门信息在查询语句中的具体查询信息
        private string GetDepartmentKey()
        {
            string strLink = "(";
            string[] keyArray = strDepartmentKey.Split(',');
            foreach(string item in keyArray)
            {
                string strKey = "'" + item + "'";
                if (strLink == "(")
                {
                    strLink = strLink + strKey;
                }
                else
                {
                    strLink = strLink + "," +strKey;
                }
            }
            strLink = strLink + ")";
            return strLink;
        }
        // 获得工资信息
        private DataTable GetSaData(ref string msgerror)
        {
            SetSql();
            DataTable SaTable = LoadData(this.SqlFormat, ref msgerror);
            if(null==SaTable)
            {
                return null;
            }
            return SaTable;
        }
        private void SetPersonCountSql()
        {
            
            strPersonCount = string.Format(strPersonCount, strYear,strSMonth, strEMonth, strPlanName);
        }

        // 获得第一行标题的信息
        private DataTable GetTableHead(ref string msgerror)
        {
            SetHeadSql();
            DataTable headTable = LoadData(strHeadSql, ref msgerror);
            if(null==headTable)
            {
                return null;
            }

            return headTable;
        }
        private void SetHeadSql()
        {
            strHeadSql = string.Format(strHeadSql, strPlanName);//, strYear, strMonth);
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
        // 导出函数
        public string GetExportPath(DataTable data, out string fileName, out string message, ReportHeadModel reportHeadModel)
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
                int iCount = data.Rows.Count;
                string filePath = ExportExcel.Export(data, this.tempalte, 2, 0, new List<ExcelCell>() { new ExcelCell(1, 1, reportHeadModel.DepartmentName),
                    new ExcelCell(13, 1, reportHeadModel.Month),new ExcelCell(0,iCount+3,reportHeadModel.Expand),new ExcelCell(3,iCount+3,reportHeadModel.Maker),
                        new ExcelCell(13,iCount+3,reportHeadModel.PrintDate) });
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
