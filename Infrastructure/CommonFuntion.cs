using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Linq.Expressions;
using Infrastructure.BaseDAL;
using System.Xml.Serialization;
using System.IO;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;
using System.Collections;

namespace Infrastructure
{
    public static class CommonFuntion
    {
       /// <summary>
       /// 调用存储过程返回列表集合
       /// </summary>
       /// <typeparam name="T">实体类</typeparam>
       /// <param name="context">数据库实体对象</param>
       /// <param name="procName">存储过程名称</param>
       /// <param name="obj">存储过程参数</param>
       /// <returns></returns>
       public static  List<T> GetList<T>(ObjectContext context,string procName, params ObjectParameter[] obj) where T : class
       {
           return context.ExecuteFunction<T>(procName, obj).ToList();
       }

       /// <summary>
       /// 获取基础档案
       /// </summary>
       /// <typeparam name="context">ObjectContext实体</typeparam>
       /// <typeparam name="T">实体类型</typeparam>
       /// <param name="whereLambda">条件</param>
       /// <returns>实体List</returns>
       public static  List<T> RetrieveModels<T>(ObjectContext context,Expression<Func<T, bool>> whereLambda) where T : class
       {
           List<T> list = new List<T>();
           list = context.CreateObjectSet<T>().Where(whereLambda).ToList();
           return list;
       }
         /// <summary>
         /// 判断是否是GUID类型
         /// </summary>
         /// <param name="guid">GUID字符串</param>
         /// <returns>Bool</returns>
       public static bool IsGUID(string guid)
       {
           Guid g;
           if (Guid.TryParse(guid, out g))
           {
               return true;
           }
           else
           {
               return false;
           }
       }
       public static string ConvertDateTimeToStr(DateTime? dt) 
       {
           if (dt == null) return "";
           return ((DateTime)dt).ToString("yyyy-MM-dd");
       }
         /// <summary>
         /// 转换成GUID
         /// </summary>
         /// <param name="guid">GUID字符串</param>
         /// <returns>GUID</returns>
       public static Guid ConvertGUID(string guid)
       {
           Guid g ;
           if (Guid.TryParse(guid, out g))
           { 
            
           }
           return g;
       }
       
       public static List<Guid> ChangeStrArrToGuidList(IList<string> guidStrArr)
       {
           List<Guid> listGuid = new List<Guid>();
           foreach (var item in guidStrArr)
           {
               Guid g;
               if (Guid.TryParse(item, out g))
               {
                   listGuid.Add(g);
               }
               
           }
           return listGuid;
       }
       public static List<int> ChangeStrArrToIntList(IList<string> guidStrArr)
       {
           List<int> listGuid = new List<int>();
           foreach (var item in guidStrArr)
           {
               int g;
               if (int.TryParse(item, out g))
               {
                   listGuid.Add(g);
               }

           }
           return listGuid;
       }
       /// <summary>
       /// 字符串中的特殊字符转换
       /// </summary>
       /// <param name="s"></param>
       /// <returns></returns>
       public static String StringToJson(String s)
       {
           if (string.IsNullOrEmpty(s))
           {
               return string.Empty;
           }
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
          // sb.Replace("\r\n", "<br/>");
           sb.Replace("\r\n"," ");
           return sb.ToString();
       }
       /// <summary>
       /// 根据表名或试图名获取classid
       /// </summary>
       /// <param name="tableOrViewName"></param>
       /// <returns></returns>
       public static int GetClassId(string tableOrViewName)
       {
           BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
           string modelname = tableOrViewName.ToLower();
           SS_Class item = context.SS_Class.FirstOrDefault(e => e.TableName.ToLower() == modelname || e.ViewName.ToLower() == modelname);
           if (item != null) return item.ClassID;
           return 0;

       }
       /// <returns></returns>
       public static int GetClassId(ObjectContext context, string tableOrViewName)
       {
           string modelname = tableOrViewName.ToLower();
           var item= context.CreateObjectSet<SS_Class>().FirstOrDefault(e => e.TableName.ToLower() == modelname || e.ViewName.ToLower() == modelname);
           if (item != null) return item.ClassID;
           return 0;

       }
       public static List<SS_DataAuthSet> RetrieveDataAuthSet(List<Guid> roleOrOperator, int ClassId)
       {
           string classid = ClassId.ToString();
           BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
           List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
           if (roleOrOperator == null) return result;
           IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => roleOrOperator.Contains(e.GUID_RoleOrOperator) && e.ClassID == classid);

           if (q != null)
           {
               result = q.ToList();
           }
           DateTime dt = DateTime.Now;
           result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
           return result;
       }
       public static IQueryable<Guid> GetRoleIds(Guid userId, BaseConfigEdmxEntities context)
       {
           var q = context.SS_RoleOperator.Where(e => e.GUID_Operator == userId).Select(e => e.GUID_Role);
           return context.SS_Role.Where(e => q.Contains(e.GUID)).Select(e => e.GUID);
       }
       public static List<SS_DataAuthSet> RetrieveDefaultDataAuthSet(List<Guid> roleOrOperator, int ClassId)
       {
           string classid = ClassId.ToString();
           BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
           List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
           if (roleOrOperator == null) return result;
           IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => e.IsDefault == true && roleOrOperator.Contains(e.GUID_RoleOrOperator) && e.ClassID == classid);

           if (q != null)
           {
               result = q.ToList();
           }
           DateTime dt = DateTime.Now;
           result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
           return result;
       }
        /// <summary>
       ///     函数功能：   进行货币单位的转换，dblMoney是希望杯转换的数值，SrcMoneyUnitUnitMultiple是当前所使用的货币单位，TargetMoneyUnitMultiple
       ///                 是转换后的货币单位
       ///      author:     dongsheng.zhang
       ///        日期:     2014-4-23
        /// </summary>
        /// <param name="dblMoney"></param>
        /// <param name="objSrcMoneyUnit"></param>
        /// <param name="objTargetMoneyUnit"></param>
        /// <returns></returns>
       public static double ConvertMoneyUnit(double dblMoney, int SrcMoneyUnitUnitMultiple, int TargetMoneyUnitMultiple)
       {
           double dblProportion = (double)SrcMoneyUnitUnitMultiple / (double)TargetMoneyUnitMultiple;
           return ConvertMoneyUnit(dblMoney,dblProportion);
       }

       public static double ConvertYUANtoOtherMoneyUnit(double dblMoney, int objTargetMoneyUnit)
       {
           double dblProportion = 1.0 / objTargetMoneyUnit;
           return dblMoney * dblProportion;
       }

       public static double ConverOtherMoneyUnitToYUAN(double dblMoney, int SrcMoneyUnitUnitMultiple)
       {
           return dblMoney * SrcMoneyUnitUnitMultiple;
       }

       private static double ConvertMoneyUnit(double dblMoney, double dblProportion)
       {
           return dblMoney * dblProportion;
       }
       public static T DeserialalizeXML<T>(string xmlPath)
       {
           XmlSerializer xs = new XmlSerializer(typeof(T));

           FileStream fs = new FileStream(xmlPath, FileMode.Open, FileAccess.Read);
           return (T)xs.Deserialize(fs);
       }
       /// <summary>
       /// 获取ClassID
       /// </summary>
       /// <param name="classList"></param>
       /// <param name="mainTabelName"></param>
       /// <param name="tableName"></param>
       /// <param name="classId"></param>
       public static void GetClassId(List<SS_Class> classList, string tableName, out string classId)
       {
           var strclassId = string.Empty;
           var classModel = classList.Find(e => e.TableName.Trim().ToLower() == tableName.Trim().ToLower());
           strclassId = classModel == null ? "" : classModel.ClassID.ToString();
           classId = strclassId;
       }
        /// <summary>
        /// 根据提供的文件名称获取相应文件的后缀名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
       public static string GetFileType(string fileName) 
       {
           string retType = string.Empty;
           FileInfo fi = new FileInfo(fileName);
           string fileType = fi.Extension;
           switch (fileType)
           {
               case ".bmp": retType = "image/bmp"; break;
               case ".gif": retType = "image/gif"; break;
               case ".jpeg": retType = "image/jpeg"; break;
               case ".tiff": retType = "image/tiff"; break;
               case ".x-dcx": retType = "image/x-dcx"; break;
               case ".x-pcx": retType = "image/x-pcx"; break;
               case ".html": retType = "text/html"; break;
               case ".htm": retType = "text/html"; break;
               case ".txt": retType = "text/plan"; break;
               case ".xml": retType = "text/xml"; break;
               case ".afp": retType = "application/afp"; break;
               case ".pdf": retType = "application/pdf"; break;
               case ".rtf": retType = "application/rtf"; break;
               case ".doc": 
               case ".docx":
                   retType = "application/msword"; break;
               case ".xlsx":
               case ".xls": retType = "application/vnd.ms-excel"; break;
               default:
                   retType = "application/octet-stream"; break;
           }
           return retType;
       }
      
    }

    public class DataSource
    {

        public static string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["BBConStr"].ConnectionString;
        static bool manualConfig = false;
        /// <summary>
        /// 
        /// </summary>
        public DataSource()
        {
        }
        /// </summary>
        /// <param name="paraName">存储过程参数名称</param>
        /// <param name="para">存储过程参数</param>
        /// <param name="ProcedureName">存储过程的名称</param>
        public static void ExecProcedure(string paraName, string para, string ProcedureName)
        {
            SqlConnection conn = Connection();
            SqlCommand Command = new SqlCommand(ProcedureName, conn);
            Command.CommandType = CommandType.StoredProcedure;
            Command.CommandTimeout = 1000;
            SqlParameter parameter = Command.Parameters.Add(
              paraName, SqlDbType.UniqueIdentifier, 32);
            parameter.Value = new Guid(para);
            Command.ExecuteNonQuery();
        }

        /// <summary>
        /// 2008-01-16 add 可以执行存储过程的ExecuteQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sql, CommandType type, SqlParameter[] parms)
        {
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandType = type;
                cmd.CommandText = sql;
                for (int i = 0; i < parms.Length; i++)
                {
                    cmd.Parameters.Add(parms[i]);
                }
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return dt;
        }

        /// <summary>
        /// 2007-04-28 add 可以执行存储过程的ExecuteNonQuery
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <param name="parms"></param>
        public static void ExecuteNonQuery(string sql, CommandType type, SqlParameter[] parms)
        {
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = type;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 1000;
            for (int i = 0; i < parms.Length; i++)
            {
                cmd.Parameters.Add(parms[i]);
            }
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        public static void ExecuteNonQuery(string sql, CommandType type, SqlParameter[] parms, SqlTransaction trans)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.Transaction = trans;
            cmd.CommandType = type;
            cmd.CommandText = sql;
            cmd.Connection = trans.Connection;
            cmd.CommandTimeout = 1000;
            for (int i = 0; i < parms.Length; i++)
            {
                cmd.Parameters.Add(parms[i]);
            }
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool ManualConfig
        {
            get { return manualConfig; }
            set { manualConfig = value; }
        }


        //数据库连接对象

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection Connection()
        {
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            return conn;
        }


        //直接执行一SQL语句,没有返回结果
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        public static void ExecuteNonQuery(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 1000;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public static void ExecuteNonQueryNoTimeOut(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = 0;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //直接执行一SQL语句,没有返回结果（传数据库联接对象）
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmd"></param>
        public static void ExecuteNonQuery(string sql, SqlCommand cmd)
        {
            cmd.CommandText = sql;
            cmd.CommandTimeout = 1000;
            cmd.ExecuteNonQuery();
        }

        //直接执行一SQL语句传数据库联接对象）

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="cmd"></param>
        public static object ExecuteScalar(string sql, SqlCommand cmd)
        {
            object o = null;
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;

            try
            {

                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];

            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            if (dt.Rows.Count > 0)
            {
                o = dt.Rows[0][0];
                return o;
            }
            return null;
        }

        public static object ExecuteScalar(string sql, CommandType type, SqlParameter[] parms)
        {
            object o = null;
            if (sql + "" == "")
            {
                return null;
            }
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = type;
            cmd.CommandText = sql;
            cmd.CommandTimeout = 1000;
            for (int i = 0; i < parms.Length; i++)
            {
                cmd.Parameters.Add(parms[i]);
            }
            DataTable dt = null;

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            if (dt.Rows.Count > 0)
            {
                o = dt.Rows[0][0];
                return o;
            }
            return null;
        }

        public static void ExecuteNonQueryInner(string sql)
        {
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandTimeout = 1000;
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //执行无返回结果的 SQL 语句集合
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrSQLst"></param>
        public static void ExecuteNonQueryLst(ref ArrayList arrSQLst)
        {
            SqlConnection objConnection = new SqlConnection(connStr);
            SqlTransaction objTransaction = null;
            string strBigSQL = string.Empty;
            try
            {
                objConnection.Open();
                objTransaction = objConnection.BeginTransaction();
                SqlCommand objCommand;

                //执行每一 SQL 语句
                for (int i = 0; i < arrSQLst.Count; i++)
                {
                    strBigSQL = strBigSQL + arrSQLst[i] + ";\n";
                }
                objCommand = new SqlCommand(strBigSQL, objConnection, objTransaction);
                objCommand.ExecuteNonQuery();
                //提交事务
                objTransaction.Commit();
            }
            catch (Exception strInfo)
            {
                if (objTransaction != null)
                {
                    objTransaction.Rollback();
                }
                throw strInfo;
            }
            finally
            {
                if (objConnection.State == ConnectionState.Open)
                    objConnection.Close();
            }
        }
        public static void ExecuteNonQueryLst(IList arrSQLst)
        {
            SqlConnection objConnection = new SqlConnection(connStr);
            SqlTransaction objTransaction = null;
            string strBigSQL = string.Empty;
            try
            {
                objConnection.Open();
                objTransaction = objConnection.BeginTransaction();
                SqlCommand objCommand;

                //执行每一 SQL 语句
                for (int i = 0; i < arrSQLst.Count; i++)
                {
                    strBigSQL = strBigSQL + arrSQLst[i] + ";\n";
                }
                objCommand = new SqlCommand(strBigSQL, objConnection, objTransaction);
                objCommand.ExecuteNonQuery();
                //提交事务
                objTransaction.Commit();
            }
            catch (Exception strInfo)
            {
                if (objTransaction != null)
                {
                    objTransaction.Rollback();
                }
                throw strInfo;
            }
            finally
            {
                if (objConnection.State == ConnectionState.Open)
                    objConnection.Close();
            }
        }
        //执行无返回结果的 SQL 语句集合
        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrSQLst"></param>
        /// <param name="objConnection"></param>
        /// <param name="objTransaction"></param>
        public static void ExecuteNonQueryLst(ref ArrayList arrSQLst, SqlConnection objConnection, SqlTransaction objTransaction)
        {
            SqlCommand objCommand;
            //执行每一 SQL 语句
            for (int i = 0; i < arrSQLst.Count; i++)
            {
                objCommand = new SqlCommand(arrSQLst[i].ToString(), objConnection, objTransaction);
                objCommand.CommandTimeout = 1000;
                objCommand.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteQuery(string sql)
        {
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;
            string ConnectionString = connStr;// System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return dt;
        }
        /// <summary>
        /// 执行数据库操作

        /// </summary>
        /// <param name="sql"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sql, string connectionString)
        {
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;
            try
            {
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return dt;
        }
        public static DataSet ExeSql(string sql)
        {
            if (sql + "" == "")
            {
                return null;
            }

            DataSet ds = null;
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                ds = new DataSet();
                adapter.Fill(ds);
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return ds;
        }
        public static DataSet ExeSql(ArrayList arrSQLst)
        {
            string sql = "";
            for (int i = 0; i < arrSQLst.Count; i++)
            {
                sql = sql + arrSQLst[i] + ";\n";
            }
            if (sql + "" == "")
            {
                return null;
            }

            DataSet ds = null;
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DBConStr"].ConnectionString;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                ds = new DataSet();
                adapter.Fill(ds);
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return ds;
        }
        public static DataSet ExeSql(ArrayList arrSQLst, string connStr)
        {
            string sql = "";
            for (int i = 0; i < arrSQLst.Count; i++)
            {
                sql = sql + arrSQLst[i] + ";\n";
            }
            if (sql + "" == "")
            {
                return null;
            }

            DataSet ds = null;
            try
            {
                SqlConnection conn = new SqlConnection(connStr);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                ds = new DataSet();
                adapter.Fill(ds);
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return ds;
        }
        //执行sql语句，获取 DataTable 对象
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sql, SqlConnection conn)
        {
            DataTable dt = null;
            if (sql + "" == "")
            {
                return null;
            }
            try
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static DataTable ExecuteQuery(string sql, SqlTransaction trans)
        {
            DataTable dt = null;
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter();
                SqlCommand cmd = new SqlCommand();
                cmd.Transaction = trans;
                cmd.CommandText = sql;
                cmd.Connection = trans.Connection;
                cmd.CommandTimeout = 1000;
                sda.SelectCommand = cmd;
                DataSet ds = new DataSet();
                sda.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(sql + ex);
            }

            return dt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql)
        {
            object o = null;
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;
            string ConnectionString = connStr;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            if (dt.Rows.Count > 0)
            {
                o = dt.Rows[0][0];
                return o;
            }
            return null;
        }

        /// <summary>
        /// 简单处理查询条目为空情况

        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteScalar2(string sql)
        {
            object o = null;
            if (sql + "" == "")
            {
                return null;
            }

            DataTable dt = null;
            string ConnectionString = connStr;
            try
            {
                SqlConnection conn = new SqlConnection(ConnectionString);
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandTimeout = 1000;
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                dt = ds.Tables[0];
                conn.Close();
            }
            catch (Exception e)
            {
                throw new ApplicationException(sql + e);
            }
            if (dt.Rows.Count > 0)
                o = dt.Rows[0][0];
            else
                o = "";
            return o;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strComma"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string splitByComma(string strComma, string columnName)
        {
            string wantedStr = columnName + "='" + strComma + "'";
            if (strComma.Contains(","))
            {
                wantedStr = "";
                string[] strArray = strComma.Split(',');
                foreach (string i in strArray)
                    wantedStr = wantedStr + columnName + "='" + i + "' or ";
                wantedStr = "(" + wantedStr.Substring(0, wantedStr.Length - 3) + ")";
            }
            return wantedStr;
        }
        /// <summary>
        /// 获取最直接上级,用于处理消息产生时的zb02_hiber,partymana_hiber表

        /// </summary>
        /// <param name="dmcod"></param>
        /// <returns></returns>
        public static string GetNearestParent(string dmcod, string tblcod)
        {
            string strSql = "select dmparentcod from [" + tblcod + "] where dmcod = '" + dmcod + "' and (dmparentlev =1 or dmparentlev = 0)";
            string result = "";
            try
            {
                DataTable dt = DataSource.ExecuteQuery(strSql);
                if (dt.Rows.Count > 0)
                {
                    result = dt.Rows[0]["dmparentcod"].ToString();
                }
                return result;
            }
            catch (Exception Error)
            {
                throw new ApplicationException(Error + "");
            }

        }
        public static string systemid = "2021";
        public static string GetLastSignDelete(string Str, string sign)
        {
            if (Str.Length > 0)
            {
                if (Str.LastIndexOf(sign) == Str.Length - 1)
                {
                    Str = Str.Substring(0, Str.Length - 1);
                }
            }
            return Str;
        }
        public static void GetLastSignDelete(ref string Str, string sign)
        {
            if (Str.Length > 0)
            {
                if (Str.LastIndexOf(sign) == Str.Length - 1)
                {
                    Str = Str.Substring(0, Str.Length - 1);
                }
            }
        }

        public static string GetUFDataBase(string OptKey, int cYear, string dwKey)
        {
            string Sql = "select cZTKey from IF_DW_ZT where cDWKey='" + dwKey + "' and iYear=" + cYear;
            DataTable dt = DataSource.ExecuteQuery(Sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }

            return string.Empty;
        }

        public static bool ContainsDatabaseCatalog(string DbName)
        {
            string SQL = "select dbid,name from master..sysdatabases where name='" + DbName + "'";
            DataTable dt = DataSource.ExecuteQuery(SQL);
            return dt.Rows.Count > 0 ? true : false;
        }

        public static object GetMoney(string msql)
        {
            DataTable dt = DataSource.ExecuteQuery(msql);
            object result = null;
            if (dt.Rows.Count > 0)
            {
                result = dt.Rows[0][0];
            }
            return result;
        }

        public static int GetPeriod(string UFDataBase, DateTime StartDate)
        {
            if (string.IsNullOrEmpty(UFDataBase)) return 0;
            string account = UFDataBase.Split(new char[] { '_' })[1];
            string strsql = "select iId from ufsystem..ua_period where dBegin<='" + StartDate.Year + "-" + StartDate.Month + "-01" + "' and dEnd>='" + StartDate.Year + "-" + StartDate.Month + "-28" + "' and cAcc_Id='"
                            + account + "'";
            var dt = DataSource.ExecuteQuery(strsql);
            if (dt.Rows.Count > 0)
            {
                return int.Parse(dt.Rows[0][0].ToString());
            }
            return 0;
        }
    }
}
