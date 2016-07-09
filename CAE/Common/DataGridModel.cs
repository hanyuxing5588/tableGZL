using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Data;

/// <summary>
/// Summary description for DataGridModel
/// </summary>
namespace Common.DataGridModel
{

    public class JDataGrid
    {

        public int total { get; set; }
        public List<Dictionary<string, object>> rows { get; set; }
        public List<JColumn> columns { get; set; }
        /// <summary>
        /// List转换成行
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ConvertRows(IList list)
        {
            List<Dictionary<string, object>> rowsList = new List<Dictionary<string, object>>();
            if (list != null)
            {
                foreach (object obj in list)
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    Type t = obj.GetType();
                    foreach (PropertyInfo pi in t.GetProperties())
                    {
                        string key = pi.Name;
                        object value = pi.GetValue(obj, null);
                        dic.Add(key, value);
                    }
                    rowsList.Add(dic);
                }
            }
            return rowsList;
        }
        /// <summary>
        /// table 转化成行数据
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ConvertRows(DataTable dt)
        {
            List<Dictionary<string, object>> rowsList = new List<Dictionary<string, object>>();
            if (dt != null)
            {
                var colName = string.Empty;
                int i = 0;

                foreach (DataRow row in dt.Rows)
                {
                    //if (i>1)
                    //{
                    //    break;
                    //}
                    if (i > 0)
                    {
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        foreach (DataColumn col in dt.Columns)
                        {
                            colName = col.ColumnName;
                            dic.Add(colName, row[colName]);
                        }
                        rowsList.Add(dic);
                    }
                        i++;
                    
                }
            }
            return rowsList;
        }
        /// <summary>
        /// 转换成Json
        /// </summary>
        /// <returns></returns>
        public string ConvertToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{{\"total\":{0},\"rows\":[", total);
            //添加数据      
            if (rows != null && rows.Count > 0)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    sb.Append("{");
                    foreach (string key in rows[i].Keys)
                    {
                        if (rows[i][key] is ValueType)
                        {
                            sb.AppendFormat("\"{0}\":{1},", key, rows[i][key]);
                        }
                        else
                        {
                            sb.AppendFormat("\"{0}\":\"{1}\",", key, rows[i][key]);
                        }
                    }
                    sb.Remove(sb.Length - 1, 1);
                    sb.Append("}");
                    if (i != rows.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
            }
            sb.Append("],");
            sb.Append("\"columns\":[");
            //添加列      
            if (columns != null && columns.Count > 0)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    sb.Append(columns[i].ConvertToJson());
                    if (i != columns.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
            }
            sb.Append("]}");
            string str = sb.ToString();
            return str;
        }
        /// <summary>
        /// DataTable转换成列
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<JColumn> ConvertColumns(DataTable dt)
        {
            List<JColumn> jcolumn = new List<JColumn>();
            if (dt != null && dt.Rows.Count != 0)
            {
                var colName = string.Empty;
                var row = dt.Rows[0];
                foreach (DataColumn col in dt.Columns)
                {
                    colName = col.ColumnName;
                    JColumn colModel = new JColumn();                                        
                    colModel.field = colName;
                    colModel.title = row[colName] == DBNull.Value ? "" : row[colName].ToString();                    
                    colModel.width = 100;
                    colModel.resizable = true;
                    //colModel.align="center";
                    jcolumn.Add(colModel);
                }
            }
            return jcolumn;
        }
    }

    public class JColumn
    {
        
        //public int rowspan { get; set; }
        //public int colspan { get; set; }
        //public string align { get; set; }
        public bool resizable { get; set; }
        public string field { get; set; }
        public string title { get; set; }
        public int width { get; set; }
        public bool sortable{get;set;}
        public bool checkbox { get; set; }
       
        
       

        public string ConvertToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            //if (rowspan != null)
            //{
            //    sb.AppendFormat("\"rowspan\":{0},", rowspan);
            //}
            //if (colspan != null)
            //{
            //    sb.AppendFormat("\"colspan\":{0},", colspan);
            //}
            //sb.AppendFormat("\"align\":\"{0}\",", align);
            if (checkbox != null)
            {
                sb.AppendFormat("\"checkbox\":{0},", checkbox.ToString().ToLower());
            }
            else
            {
                sb.AppendFormat("\"checkbox\":{0},","false"); 
            }
            if (sortable != null)
            {
                sb.AppendFormat("\"sortable\":{0},", sortable.ToString().ToLower());
            }
            else
            {
                sb.AppendFormat("\"sortable\":{0},", "false");
            }
            sb.AppendFormat("\"field\":\"{0}\",", field);
            sb.AppendFormat("\"width\":{0},", width);           
            sb.AppendFormat("\"title\":\"{0}\",", title);
            sb.Remove(sb.Length - 1, 1);
            sb.Append("}");
            String str = sb.ToString();
            return str;
        }
    }
    
}