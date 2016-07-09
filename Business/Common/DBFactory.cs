using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessModel;
using Infrastructure;
using System.Data.Objects;
using System.Runtime.Remoting.Messaging;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
namespace Business.Common
{
        /// <summary>
        /// 数据访问工厂
        /// </summary>
        public static class DBFactory
        {

            /// <summary>
            /// 保证了线程内上下文实例唯一
            /// </summary>
            /// <returns></returns>
            public static BusinessModel.BusinessEdmxEntities GetCurBusinessContext()
            {
                //这里的GetData()方法的key不能和上下文的一样
                var _dbSession = CallContext.GetData("DbBSession") as BusinessModel.BusinessEdmxEntities;
                if (_dbSession == null)
                {
                    _dbSession = new BusinessModel.BusinessEdmxEntities();
                    //将值设置到数据槽里面去
                    CallContext.SetData("DbBSession", _dbSession);
                }
                return _dbSession;
            }
            public static BaseConfigEdmxEntities GetCurInfrastructureDbContext()
            {
                var _dbSession = CallContext.GetData("DbISession") as BaseConfigEdmxEntities;
                if (_dbSession == null)
                {
                    _dbSession = new BaseConfigEdmxEntities();
                    CallContext.SetData("DbISession", _dbSession);
                }
                return _dbSession;
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
