using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using Aspose.Excel;
namespace CAE
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
    public class SheetData
    {
        public SheetData() { }
        public DataTable table {get;set;}
        public int rowIndex { get; set; }
        public int colIndex { get; set; }
        public int index { get; set; }
        public List<ExcelCell> cellList { get; set; }
    }
 public   class ExportExcel
    {
        public static string sysTempPath = AppDomain.CurrentDomain.BaseDirectory + "ReportTemp";

        public static string Export(ref List<SheetData> SheetList,string template)
        {
            string fullFilePath = "";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string relativePath = sysTempPath + "\\" + fileName;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                if (!Directory.Exists(sysTempPath))
                {
                    Directory.CreateDirectory(sysTempPath);
                }
                string tempPlatePath = template;
                string templateFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPlatePath));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(templateFullPath);
                fullFilePath = sysTempPath + "\\" + fileName;
                int currentSheetIndex = 0;
                foreach(SheetData item in SheetList)
                {
                    DataTable dt = item.table;
                    int colIndex = item.colIndex;
                    int rowIndex = item.rowIndex;
                    List<ExcelCell> excelCells = item.cellList;
                    currentSheetIndex = item.index;
                    Worksheet curWs = excel.Worksheets[currentSheetIndex];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        for (int j = colIndex; j < dt.Columns.Count; j++)
                        {
                            Cell curCell = curWs.Cells[rowIndex, j];
                            if (!string.IsNullOrEmpty(dr[j] + ""))
                            {
                                dr[j] = dr[j].ToString().Replace("&nbsp", " ");
                            }
                            curCell.PutValue(dr[j]);
                        }
                        rowIndex++;
                    }
                    if (excelCells != null)
                    {
                        foreach (ExcelCell ec in excelCells)
                        {
                            Cell cell = curWs.Cells[ec.Row, ec.Col];
                            cell.PutValue(ec.Value);
                        }
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
       
        public static string Export(DataTable dt, string template, int rowIndex, int colIndex, List<ExcelCell> excelCells) 
        {
            string fullFilePath = "";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string relativePath = sysTempPath + "\\" + fileName;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                if (!Directory.Exists(sysTempPath))
                {
                    Directory.CreateDirectory(sysTempPath);
                }
                string tempPlatePath = template;
                string templateFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPlatePath));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(templateFullPath);
                fullFilePath = sysTempPath + "\\" + fileName;
                int currentSheetIndex = 0;
                Worksheet curWs = excel.Worksheets[currentSheetIndex];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    for (int j = colIndex; j < dt.Columns.Count; j++)
                    {
                        Cell curCell = curWs.Cells[rowIndex, j];
                        if (!string.IsNullOrEmpty(dr[j] + "")) {
                            dr[j] = dr[j].ToString().Replace("&nbsp", " ");
                        }
                        curCell.PutValue(dr[j]);
                    }
                    rowIndex++;
                }
                if (excelCells != null) 
                {
                    foreach (ExcelCell ec in excelCells)
                    {
                        Cell cell = curWs.Cells[ec.Row, ec.Col];
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
        public static string Export(DataTable dt, string template, int rowIndex, List<string> colNames, List<ExcelCell> excelCells,int startColumn=1)
        {
            string fullFilePath = "";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string relativePath = sysTempPath + "\\" + fileName;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                if (!Directory.Exists(sysTempPath))
                {
                    Directory.CreateDirectory(sysTempPath);
                }
                string tempPlatePath = template;
                string templateFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPlatePath));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(templateFullPath);
                fullFilePath = sysTempPath + "\\" + fileName;
                int currentSheetIndex = 0;
                Worksheet curWs = excel.Worksheets[currentSheetIndex];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    for (int j = 0; j < colNames.Count; j++)
                    {
                        Cell curCell = curWs.Cells[rowIndex, j + startColumn];
                        if (!string.IsNullOrEmpty(dr[colNames[j]] + ""))
                        {
                            dr[colNames[j]] = dr[colNames[j]].ToString().Replace("&nbsp", " ").Replace(";","");
                        }
                        curCell.PutValue(dr[colNames[j]]);
                    }
                    rowIndex++;
                }
                if (excelCells != null)
                {
                    foreach (ExcelCell ec in excelCells)
                    {
                        Cell cell = curWs.Cells[ec.Row, ec.Col];
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
        public static string ExportByStart(DataTable dt, string template, int rowIndex, int startColumn)
        {
            string fullFilePath = "";
            string fileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";
            string relativePath = sysTempPath + "\\" + fileName;
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                if (!Directory.Exists(sysTempPath))
                {
                    Directory.CreateDirectory(sysTempPath);
                }
                string tempPlatePath = template;
                string templateFullPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, tempPlatePath));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(templateFullPath);
                fullFilePath = sysTempPath + "\\" + fileName;
                int currentSheetIndex = 0;
                Worksheet curWs = excel.Worksheets[currentSheetIndex];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];
                    for (int j = startColumn; j < dt.Columns.Count; j++)
                    {
                        Cell curCell = curWs.Cells[rowIndex, j-startColumn];
                        if (!string.IsNullOrEmpty(dr[j] + ""))
                        {
                            dr[j] = dr[j].ToString().Replace("&nbsp", " ");
                        }
                        curCell.PutValue(dr[j]);
                    }
                    rowIndex++;
                }
               
                excel.Save(fullFilePath);
            }
            catch (Exception ex)
            {

                throw;
            }
            return fullFilePath;
        }
        
    }
    //导入类 未测试
   public class ImportExcel 
    {
        public static string GetSheet(string filePath,int sheetIndex=0) 
        {
            if (string.IsNullOrEmpty(filePath)) return "";
            try
            {
                Aspose.Excel.License l = new Aspose.Excel.License();
                l.SetLicense(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin\\Cbin", "Aspose.Total.Product.Family.lic"));
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(filePath);
                Worksheet curWs = excel.Worksheets[sheetIndex];
                return  curWs.Name;
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
                Aspose.Excel.Excel excel = new Aspose.Excel.Excel();
                excel.Open(filePath);
                Worksheet curWs = excel.Worksheets[sheetName];
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
        private static void InsertDataByDataTable(ref DataTable dt, Worksheet curWs,int rowDataStart=1)
        {
            var cols = curWs.Cells.Columns;
            var rows = curWs.Cells.Rows;
            for (int i = 0; i < cols.Count; i++)
            {
                Column col = cols[(byte)i];
                DataRow dataRow = dt.NewRow();
                for (int j = rowDataStart; j < rows.Count; j++)
                {
                    Row row = rows[j];
                    var cellValue = (curWs.Cells[row.Index, col.Index].Value + "").Trim().ToString();//默认为"";
                    dataRow[i] = cellValue;
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
                listColName.Add(string.IsNullOrEmpty(cellValue) ? "tempColName" + i : cellValue);
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
    }
}
