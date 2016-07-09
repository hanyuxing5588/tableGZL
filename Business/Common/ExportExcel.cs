using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Aspose.Cells;
using System.Data.OleDb;
namespace Business.Common
{
    public class ExcelCell 
    {
        public ExcelCell() { }
        public ExcelCell(int col, int row, object v)
        {
            this.Col=col;
            this.Row=row;
            this.Value=v;
        }
        public int Col { get; set; }
        public int Row { get; set; }
        public object Value { get; set; }
    }
    class ExportExcel
    {
        public static string sysTempPath = AppDomain.CurrentDomain.BaseDirectory + "\\ReportTemp";
        public static string Export(DataTable dt, string template, int rowIndex, int colIndex, List<ExcelCell> excelCells) 
        {
            string fullFilePath = "";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string relativePath = sysTempPath + "\\" + fileName;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                //Aspose.Cells.License l = new Aspose.Cells.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                if (!Directory.Exists(sysTempPath))
                {
                    Directory.CreateDirectory(sysTempPath);
                }
                string tempPlatePath = template;
                string templateFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPlatePath));
                //Aspose.Cells.Workbook excel = new Aspose.Cells.Workbook();
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                int currentSheetIndex = 0;
                if (File.Exists(templateFullPath))
                {
                    excel.Open(templateFullPath);
                }
                fullFilePath = sysTempPath + "\\" + fileName;
                
                var curWs = excel.Worksheets[currentSheetIndex];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    for (int j = colIndex; j < dt.Columns.Count; j++)
                    {
                        var curCell = curWs.Cells[rowIndex, j];
                        object dtvalue = dr[j];
                        if (dtvalue != null)
                        {
                            if (dtvalue.GetType() == typeof(string))
                            {
                                dtvalue = dtvalue.ToString().Replace("&nbsp;", " ");
                            }
                        }

                        curCell.PutValue(dtvalue);
                    }
                    rowIndex++;
                }
                if (excelCells != null) 
                {
                    foreach (ExcelCell ec in excelCells)
                    {
                        var cell = curWs.Cells[ec.Row, ec.Col];
                        cell.PutValue(ec.Value);
                    }
                }
                excel.Save(fullFilePath);
            }
            catch (Exception ex)
            {

                throw;
            }
            return fullFilePath;
        }
        public static string Export(DataTable dt, string template, int rowIndex, int colIndex)
        {
            return Export(dt, template, rowIndex, colIndex,null);
        }
        public static string Export(DataTable dt, string template, int rowIndex) 
        {
            return Export(dt, template, rowIndex, 0);
        }
    }
    //导入类 未测试

    public class ImportExcel
    {
        public static string GetSheet(string filePath, int sheetIndex = 0)
        {
            if (string.IsNullOrEmpty(filePath)) return "";
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                var excel = new Aspose.Cells.Workbook();
                excel.Open(filePath);
                var curWs = excel.Worksheets[sheetIndex];
                return curWs.Name;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public static DataTable Import(string filePath, string sheetName)
        {
            DataTable dt = null;
            if (string.IsNullOrEmpty(filePath)) return dt;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                Aspose.Cells.Workbook excel = new Aspose.Cells.Workbook();
                excel.Open(filePath);
                var curWs = excel.Worksheets[sheetName];
                //获得Excel的列
                var colNames = GetColNames(curWs);
                //根据列创建表
                dt = CreateDataTable(colNames);
                //插入数据
                InsertDataByDataTable(ref dt, curWs);
                return dt;
            }
            catch (Exception ex)
            {
                return dt;
            }
        }
        public static DataTable ExcelToDS(string Path)
        {
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
            System.Data.OleDb.OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = "select * from [sheet1$]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds.Tables[0];
        } 
        public static DataTable Import(string filePath, string sheetName,out string message)
        {
            message = string.Empty;
            DataTable dt = null;
            if (string.IsNullOrEmpty(filePath))
            {
                message = "路径不能为空！";
                return dt;
            }
            try
            {
                //Aspose.Excel.License l = new Aspose.Excel.License();
                //l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                //var excel = new Aspose.Cells.Workbook();
                //excel.Open(filePath);
                //var curWs = excel.Worksheets[sheetName];
                ////获得Excel的列
                //var colNames = GetColNames(curWs);
                ////根据列创建表
                //dt = CreateDataTable(colNames);
                ////插入数据
                //InsertDataByDataTable(ref dt, curWs, out message);
               dt= ExcelToDS(filePath);
                return dt;
            }
            catch (Exception ex)
            {
                message = "数据导入转换失败！";
                return dt;
            }
        }
        public static List<DataTable> ImportAll(string filePath,out string message)
        {
            message = string.Empty;
            if (string.IsNullOrEmpty(filePath))
            {
                message = "路径不能为空！";
                return null;
            }
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                Aspose.Cells.Workbook excel = new Aspose.Cells.Workbook();
                excel.Open(filePath);
                List<DataTable> listDT = new List<DataTable>();
                for (int i = 0; i < 3; i++)
                {
                    var curWs = excel.Worksheets[i];
                    //获得Excel的列
                    //var colNames = GetColNames(curWs);
                    //根据列创建表
                   DataTable dt = CreateDataTable(8);
                    //插入数据
                   InsertDataByDataTable1(ref dt, curWs, out message);
                    listDT.Add(dt);
                }

                return listDT;
            }
            catch (Exception ex)
            {
                message = "数据导入转换失败！";
                return null;
            }
        }


        private static void InsertDataByDataTable(ref DataTable dt, Worksheet curWs, int rowDataStart = 1)
        {
            //var cols = curWs.Cells.Columns;            
            //var rows = curWs.Cells.Rows;
            
            //for (int i = 0; i < cols.Count; i++)
            //{
            //    Column col = cols[(byte)i];
            //    DataRow dataRow = dt.NewRow();
            //    for (int j = rowDataStart; j < rows.Count; j++)
            //    {
            //        Row row = rows[j];
            //        var cellValue = (curWs.Cells[row.Index, col.Index].Value + "").Trim().ToString();//默认为"";
            //        dataRow[i] = cellValue;
            //    }
            //    dt.Rows.Add(dataRow);
            //}
            var cols = dt.Columns;
            var rows = curWs.Cells.Rows;
            var flag = true;
            for (int j = rowDataStart; j < rows.Count; j++)
            {
                if (flag == false) break;
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < cols.Count; i++)
                {                   
                    Row row = rows[j];
                    var cellValue = (curWs.Cells[row.Index, i].Value + "").Trim().ToString();//默认为"";
                    int g;
                    if (i==0 && int.TryParse(cellValue,out g)==false)
                    {
                        flag = false;
                        break;
                    }
                    dataRow[i] = cellValue;                   
                }
                dt.Rows.Add(dataRow);
            }

        }
        private static void InsertDataByDataTable1(ref DataTable dt, Worksheet curWs, out string mesage, int rowDataStart = 1)
        {
            mesage = string.Empty;
            var cols = dt.Columns;
            var rows = curWs.Cells.Rows;
            List<string> keyList = new List<string>();
            for (int j = rowDataStart; j < rows.Count; j++)
            {
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < cols.Count; i++)
                {
                    Row row = rows[j];
                    var cellValue = (curWs.Cells[row.Index, i].Value + "").Trim().ToString();//默认为"";
                    //校验重复数据 只校验第一列
                    dataRow[i] = cellValue;
                }
                dt.Rows.Add(dataRow);
            }

        }
        private static void InsertDataByDataTable(ref DataTable dt, Worksheet curWs,out string mesage,int rowDataStart = 1)
        {
            mesage = string.Empty;
            var cols = dt.Columns;
            var rows = curWs.Cells.Rows;
            var flag = true;
            List<string> keyList = new List<string>();
            for (int j = rowDataStart; j < rows.Count; j++)
            {
                if (flag == false) break;
                DataRow dataRow = dt.NewRow();
                for (int i = 0; i < cols.Count; i++)
                {
                    Row row = rows[j];
                    var cellValue = (curWs.Cells[row.Index, i].Value + "").Trim().ToString();//默认为"";
                    //int g;
                    //if (i == 0 && int.TryParse(cellValue, out g) == false)
                    //{
                    //    flag = false;
                    //    break;
                    //}
                    //校验重复数据 只校验第一列
                    if (i == 0)
                    {
                        if (!keyList.Contains(cellValue))
                        {
                            keyList.Add(cellValue);
                        }
                        else
                        {
                            int rowIndex = j + 1;
                            mesage = "Excel中第" + rowIndex + "行数据重复！导入失败！";
                            return;
                        }
                    }
                    dataRow[i] = cellValue.Replace(",", "").Replace(",", "").Replace(",", "").Replace(",", "").Replace(",", "");
                }
                dt.Rows.Add(dataRow);
            }

        }
        private static List<string> GetColNames(Worksheet curWs, int rowIndex = 0)
        {

            List<string> listColName = new List<string>();
            var cloumns = curWs.Cells.Columns;
            for (int i = 0; i < cloumns.Count; i++)
            {
                Column col = cloumns[(byte)i];
                var cellValue = (curWs.Cells[rowIndex, col.Index].Value + "").Trim().ToString();//默认为"";
                if (!string.IsNullOrEmpty(cellValue) && !listColName.Contains(cellValue))
                {
                    //string.IsNullOrEmpty(cellValue) ? "tempColName" + i : 
                    listColName.Add(cellValue);
                }
                else
                {
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        break;
                    }
                }
            }
            return listColName;
        }
        private static DataTable CreateDataTable(List<string> colNames)
        {
            var dt = new DataTable();
            foreach (var name in colNames)
            {
                dt.Columns.Add(new DataColumn(name));
            }
            return dt;

        }
        private static DataTable CreateDataTable(int i)
        {
            var dt = new DataTable();
            var name = "col";
            for (int j = 0; j < i;j++ )
            {
                dt.Columns.Add(new DataColumn(name+j));
            }
            return dt;

        }
    }
}
