using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using Business.Common;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.Configuration;
namespace BaothApp.Utils
{
    public class JsonGridHelper<T>
    {
        private List<T> targets = null;
        private PropertyInfo[] propertyInfos = null;
        private Dictionary<string, PropertyInfo> propertyInfoDic = null;
        public JsonGridHelper(List<T> targets)
        {
            this.propertyInfos = typeof(T).GetProperties();
            if (this.propertyInfos != null)
            {
                this.propertyInfoDic = new Dictionary<string, PropertyInfo>();
                for (int i = 0; i < this.propertyInfos.Length; i++)
                {
                    this.propertyInfoDic.Add(this.propertyInfos[i].Name.ToLower(), this.propertyInfos[i]);
                }
            }
            this.targets = targets;
        }

        public string stringify()
        {
            return this.stringify(this.propertyInfoDic.Keys.ToArray<string>());
        }

        public string stringify(string[] filters)
        {
            StringBuilder result = new StringBuilder();
            string resultvalue = "";
            foreach (T target in this.targets)
            {
                List<string> pairs = this.getPairs(filters, target);
                string item = "{" + String.Join(",", pairs.ToArray()) + "},";
                result.Append(item);
            }
            if (result.Length > 0)
            {
                resultvalue = result.ToString();
                resultvalue = resultvalue.Substring(0, resultvalue.Length - 1);
            }
            return "[" + resultvalue + "]";
        }

        public string stringify(List<string> filters,bool isFilter=true)
        {
            if (isFilter&&filters == null) return string.Empty;
            string[] input = filters.ToArray();
            return this.stringify(input);
        }

        protected List<string> getPairs(string[] attrNames, T target)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < attrNames.Length; i++)
            {
                result.Add(this.getPair(attrNames[i], target));
            }
            return result;
        }

        protected string getValue(string attrName, T target)
        {
            attrName = attrName.ToLower();
            if (this.propertyInfoDic.ContainsKey(attrName))
            {
                //return this.propertyInfoDic[attrName].GetValue(target,null).ToString();
                return this.propertyInfoDic[attrName].GetValue(target);
            }
            return string.Empty;
        }

        protected string getPair(string attrName, T target)
        {
           var value = Regex.Replace(this.getValue(attrName, target), "\\s{2,}", "");//利用曾泽去掉name中间的空格
            return "\"" + attrName + "\":\"" + value + "\"";
        }
    }

    public class JsonHelper
    {
        /// <summary>
        /// 对象序列化


        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string objectToJson(object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            ScriptingJsonSerializationSection section = WebConfigurationManager.GetSection("system.web.extensions/scripting/webServices/jsonSerialization") as ScriptingJsonSerializationSection;
            if (section != null)
            {
                jss.MaxJsonLength = section.MaxJsonLength;
                jss.RecursionLimit = section.RecursionLimit;
            }
            return jss.Serialize(obj);
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object JsonToObject(string obj, Type t)
        {
            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                return jss.Deserialize(obj, t);
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string obj) where T:class
        {
            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                return jss.Deserialize<T>(obj);
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        
        /// <summary>
        /// JsonDeserialize
        /// </summary>
        /// <typeparam name="T">实例对象名称</typeparam>
        /// <param name="jsonString">要反序列化得json字符串</param>
        /// <returns></returns>
        public static T JsonDeserialize<T>(string jsonString) 
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString));
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            ms.Dispose();
            return obj;
        }

        /// <summary>
        /// JsonSerialize
        /// </summary>
        /// <typeparam name="T">实例对象名称</typeparam>
        /// <param name="obj">要序列化得json字符串</param>
        /// <returns></returns>
        public static string JsonSerialize<T>(T obj) 
        {
            System.Runtime.Serialization.Json.DataContractJsonSerializer serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.WriteObject(ms, obj);
            string retVal = Encoding.UTF8.GetString(ms.ToArray());
            ms.Dispose();
            return retVal;
        }
        /// <summary>
        /// DataTable到Json转换
        /// {jsonName:[{},{}]}
        /// </summary>
        /// <param name="jsonName"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string DataTableToJson(string jsonName, DataTable dt)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"" + jsonName + "\":[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":\"" + stringToJson(dt.Rows[i][j].ToString()) + "\"");
                        if (j < dt.Columns.Count - 1)
                        {
                            jsonBuilder.Append(",");
                        }
                    }
                    jsonBuilder.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        jsonBuilder.Append(",");
                    }
                }
            }
            jsonBuilder.Append("]}");           
            return jsonBuilder.ToString();
        }
        public static string DataTableToJson( DataTable dt)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("\"" + dt.Columns[j].ColumnName.ToString() + "\":\"" + stringToJson(dt.Rows[i][j].ToString()) + "\"");
                        if (j < dt.Columns.Count - 1)
                        {
                            jsonBuilder.Append(",");
                        }
                    }
                    jsonBuilder.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        jsonBuilder.Append(",");
                    }
                }
            }
            jsonBuilder.Append("]");
            return jsonBuilder.ToString();
        }

        public static string DataTableToJsonNEW(DataTable dt)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    jsonBuilder.Append("{");
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        jsonBuilder.Append("'" + dt.Columns[j].ColumnName.ToString() + "':'" + stringToJson1(dt.Rows[i][j].ToString()) + "'");
                        if (j < dt.Columns.Count - 1)
                        {
                            jsonBuilder.Append(",");
                        }
                    }
                    jsonBuilder.Append("}");
                    if (i < dt.Rows.Count - 1)
                    {
                        jsonBuilder.Append(",");
                    }
                }
            }
            jsonBuilder.Append("]");
            return jsonBuilder.ToString();
        }
        /// <summary>
        /// List 转换成 Datatable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">表名称</param>
        /// <param name="IL">List集合</param>
        /// <returns></returns>
        public static DataTable ListToDataTable<T>(string tableName, IList<T> IL)
        {
            DataTable dt = new DataTable();
            dt.TableName = tableName;           
            if (IL.Count > 0)
            {
                T obj = Activator.CreateInstance<T>();
                Type type = obj.GetType();
                PropertyInfo[] pis = type.GetProperties(); 
                //添加表头
                foreach (PropertyInfo item in pis)
                {                    
                        dt.Columns.Add(item.Name);                   
                }
                //填充数据
                for (int i = 0; i < IL.Count; i++)
                {
                    DataRow row = dt.NewRow();
                    for (int j = 0; j < pis.Length; j++)
                    {
                        var columnName = pis[j].Name;
                        var columnValue = pis[j].GetValue(IL[i], null);
                        row[columnName] = columnValue;
                    }
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static String stringToJson(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {

                char c = s[i];
                switch (c)
                {
                    
                    case '\'':
                        sb.Append("\\\'");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\b':      //退格
                        sb.Append("\\b");
                        break;
                    case '\f':      //走纸换页
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n"); //换行    
                        break;
                    case '\r':      //回车
                        sb.Append("\\r");
                        break;
                    case '\t':      //横向跳格
                        sb.Append("\\t");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Replace("\r\n", "<br/>");
                        //var temp = Regex.Replace(sb.ToString(), "\\s{2,}", "");//利用正则去掉key中间的空格
            var temp = TrimMiddle(sb.ToString());//利用正则去掉key中间的空格
            return temp;
        }

        public static String stringToJson1(String s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {

                char c = s[i];
                switch (c)
                {

                    case '\'':
                        sb.Append("'");
                        break;
                    case '"':
                        sb.Append("");
                        break;
                    case '\b':      //退格




                        sb.Append("");
                        break;
                    case '\f':      //走纸换页
                        sb.Append("");
                        break;
                    case '\n':
                        sb.Append(""); //换行    
                        break;
                    case '\r':      //回车
                        sb.Append("");
                        break;
                    case '\t':      //横向跳格
                        sb.Append("");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            sb.Replace("\r\n", "");
            var temp = Regex.Replace(sb.ToString(), "\\s{2,}", "");//利用正则去掉key中间的空格



            return temp;
        }
        /// <summary>
        /// 分页的Json格式
        /// </summary>
        /// <param name="jsonRow">行的Json数据 如：[{},{},{},...]</param>
        /// <param name="totalCount">总条数</param>
        /// <returns>string</returns>
        public static string PageJsonFormat(string rowJson, int totalCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"total\":" + totalCount);
            sb.Append(",\"rows\":");
            sb.Append(rowJson);
            sb.Append("}");
            return sb.ToString();
           
        }
        public static string PageTotalJsonFormat(string rowJson,string rowTotal, int totalCount)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"total\":" + totalCount);
            sb.Append(",\"rows\":");
            sb.Append(rowJson);
            sb.Append(",\"footer\":");
            sb.Append(rowTotal);
            sb.Append("}");
            return sb.ToString();
        }
        public static string TrimMiddle(string s)
        {
            bool BeginBlank = true;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c != ' ')
                {
                    BeginBlank = false;
                }
                else if (c == ' ' && BeginBlank == false)
                {
                    continue;
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}