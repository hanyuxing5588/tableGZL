using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Data;
using BusinessModel;
using Business.Common;
namespace Business.CommonModule
{
 
    public class U8Result
    {
        public int ResultNumber = 0;
        public string ResultMessage = string.Empty;
    }

    static class PzExtends
    {
        /// <summary>
        /// 判断会计凭证是否在用友库已存在
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static bool ExistsInU8(this CW_PZMainView obj, BusinessEdmxEntities dbEntity, SqlConnection cnn, string databaseName="")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());
            string sql = "select cOutNo_ID from " + databaseName + "..gl_accvouch where cOutNo_ID='" + obj.GUID.ToString() + "'";
            if (cnn.State== ConnectionState.Closed) cnn.Open();
            SqlCommand cmm= cnn.CreateCommand();
            cmm.CommandText=sql;
            int result=cmm.ExecuteNonQuery();
            cnn.Close();
            return result<=0?false:true;
        }
        /// <summary>
        /// 判断数据库对象是否存在        /// </summary>
        /// <param name="obj"></param>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static bool ExistsDBObject(this CW_PZMainView obj,BusinessEdmxEntities dbEntity,string databseName="")
        {
            if (databseName == "") databseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());
            return CommonFun.IsDataBaseExsist(dbEntity, databseName);
        }
        public static string GetU8DataBaseName(this CW_PZMainView obj, BusinessEdmxEntities dbEntity)
        {
            return CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString()); 
        }

        /// <summary>
        /// 获得日期所属的用友会计期间
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static int GetUFPeriod(this CW_PZMainView obj, SqlConnection cnn,U8Result result)
        {
            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "Select iid from UFSystem..UA_Period where cAcc_ID='" + obj.ExteriorDataBase + "' and iYear=" + obj.DocDate.Year +
            " and dBegin<='" + obj.DocDate + "' and dEnd>='" + obj.DocDate + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    int i = 0;
                    int.TryParse((dt.Rows[0][0]+"").ToString(),out i);
                    return i;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "获取用友会计期间出错";
            return 0;
        }
        /// <summary>
        /// 判断指定的会计期间是否已经结账
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetPeriodIsClose(this CW_PZMainView obj, BusinessEdmxEntities dbEntity, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            result.ResultNumber = 1;
            result.ResultMessage = "";
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());


            string strSQL = "Select bflag from " + databaseName +
                "..gl_mend where iPeriod=" + obj.CWPeriod + " and iYear=" + obj.DocDate.Year;
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    object kk = dt.Rows[0][0];
                    if (kk == null) return false;
                    return (bool)kk;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "获取用友结账信息时出错";
            return false;
        }
        /// <summary>
        /// 获取号
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static int GetIno_id(this CW_PZMainView obj, BusinessEdmxEntities dbEntity, SqlConnection cnn, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());

            string strSQL = "Select isnull(max(ino_id),0)+1 as PZNumber from " + databaseName + "..gl_accvouch where iyear="
                + obj.DocDate.Year + " and iPeriod=" + obj.CWPeriod.ToString() + " and csign='" + obj.PZTypeKey + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return (int)dt.Rows[0][0];
                }
            }
            return 1;
        }
        //验证凭证号

        public static bool CheckIno_id(this CW_PZMainView obj, BusinessEdmxEntities dbEntity, SqlConnection cnn, int pzNumber, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());

            string strSQL = "Select ino_id as PZNumber from " + databaseName + "..gl_accvouch where iyear="
                + obj.DocDate.Year + " and iPeriod=" + obj.CWPeriod.ToString() + " and csign='" + obj.PZTypeKey + "' and ino_id='"+pzNumber+"'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                  return true;
                }
            }
            return false ;
        }
        /// <summary>
        /// 凭证类别及凭证类别排序是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckSign(this CW_PZMainView obj, BusinessEdmxEntities dbEntity, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, obj.ExteriorDataBase, obj.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "select * from " + databaseName + "..dsign where csign='" + obj.PZTypeKey + "' and isignseq=" + obj.PZTypeOrder;
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "凭证类别对应关系不正确，不能保存。";
            return false;
        }
        /// <summary>
        /// 获取用友科目信息
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static System.Data.DataRow GetU8Code(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "select ccode,bbank,bend,bdept,bperson,bcus,bsup,bitem,cass_item from " +
                databaseName + "..Code where cCode='" + obj.GetValue("AccountTitleKey") + "'"
                + " and iyear=" + pzmain.DocDate.Year;
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return dt.Rows[0];
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "用友会计科目不存在，不能保存。";
            return null;
        }
        /// <summary>
        /// 判断部门编号是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pzmain"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckcDeptCode(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "Select * from  " + databaseName + "..Department Where cDepCode='" + obj.GetValue("UF8870*DepartmentKey") + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "所设置的用友部门信息不正确，不能保存。";
            return false;
        }
        /// <summary>
        /// 判断人员编号是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pzmain"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckcPersonCode(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            /* 改动 从人员对应表把key 找到*/

            /**/
            string strSQL = "Select * from  " + databaseName + "..Person Where cPersonCode='" + obj.GetValue("UF8870*PersonKey") + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "所设置的用友人员信息不正确，不能保存。";
            return false;
        }
        /// <summary>
        /// 判断客户信息是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pzmain"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckcCusCode(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "Select * from  " + databaseName + "..Customer Where cCusCode='" + obj.GetValue("UF8870*CustomerKey") + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "所设置的用友客户信息不正确，不能保存。";
            return false;
        }
        /// <summary>
        /// 判断供应商信息是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pzmain"></param>
        /// <param name="cnn"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckcVenCode(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "Select * from  " + databaseName + "..Vendor Where cVenCode='" + obj.GetValue("UF8870*CustomerKey") + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "所设置的用友供应商信息不正确，不能保存。";
            return false;
        }
        /// <summary>
        /// 判断项目信息是否正确
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="pzmain"></param>
        /// <param name="cnn"></param>
        /// <param name="cass_Item"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool CheckcItemCode(this U8PzDetail obj, BusinessEdmxEntities dbEntity, CW_PZMainView pzmain, SqlConnection cnn, string cass_Item, U8Result result, string databaseName = "")
        {
            if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(dbEntity, pzmain.ExteriorDataBase, pzmain.FiscalYear.ToString());

            result.ResultNumber = 1;
            result.ResultMessage = "";
            string strSQL = "Select * from  " + databaseName + "..fitemss" + cass_Item + " Where cItemCode='" + obj.GetValue("UF8870*ProjectKey") + "'";
            if (cnn.State == ConnectionState.Closed) cnn.Open();
            SqlCommand cmm = cnn.CreateCommand();
            cmm.CommandText = strSQL;
            System.Data.DataSet ds = new System.Data.DataSet();
            System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
            da.Fill(ds);
            if (ds.Tables.Count > 0)
            {
                System.Data.DataTable dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                {
                    return true;
                }
            }
            result.ResultNumber = 0;
            result.ResultMessage = "所设置的用友项目或项目大类不正确，不能保存。";
            return false;
        }
    }

    public class U8PzDetail
    {
        private Dictionary<string, string> values = new Dictionary<string, string>();
        public U8PzDetail(CW_PZDetailView pzdetail)
        {
            System.Reflection.PropertyInfo[] infos = pzdetail.GetType().GetProperties();
            for (int i = 0; i < infos.Length; i++)
            {
                values.Add(infos[i].Name.ToUpper(), (infos[i].GetValue(pzdetail, null)+"").ToString());
            }
        }
        public U8PzDetail() { }
        public string GetValue(string AttributeName)
        {
            AttributeName = AttributeName.ToUpper();
            return values.ContainsKey(AttributeName) ? values[AttributeName] : string.Empty;
        }
        public void SetValue(string AttributeName, string NewValue)
        {
            AttributeName=AttributeName.ToUpper();
            if (values.ContainsKey(AttributeName))
            {
                values[AttributeName] = NewValue;
            }
            else
            {
                values.Add(AttributeName, NewValue);
            }
        }
    }
  
    //用友凭证
    public  class U8Certificate
    {
        private BusinessEdmxEntities dbEntity;
        public string alreadyPeriod=string.Empty;
        public string alreadyPzNumber=string.Empty;
        private string CloseSaveTrans = string.Empty;
        private SqlConnection U8Db; //u8数据库连接
        private string U8DataBaseName = "";
        //public U8Certificate()
        //{

        //    string connStr =string.Format( System.Configuration.ConfigurationManager.AppSettings["U8Config"],U8DataBaseName);
        //    U8Db = new SqlConnection(connStr);
        //}
        //public U8Certificate(string udataName)
        //{

        //    string connStr = string.Format(System.Configuration.ConfigurationManager.AppSettings["U8Config"], udataName);
        //    U8Db = new SqlConnection(connStr);
        //}
        public U8Certificate(BusinessEdmxEntities dbEntity)
        {
            this.dbEntity = dbEntity;
            string connStr = string.Format(System.Configuration.ConfigurationManager.AppSettings["U8Config"], U8DataBaseName);
            U8Db = new SqlConnection(connStr);
        }
        //获得凭证号
        public int GetPZNumber(CW_PZMainView PzMain,string databaseName="") 
        {
            return PzMain.GetIno_id(this.dbEntity, U8Db, databaseName);
        }
        //验证凭证号

        public bool CheckPZNumber(CW_PZMainView PzMain, int pzNumber, string databaseName = "")
        {
            return PzMain.CheckIno_id(this.dbEntity, U8Db, pzNumber, databaseName);
        }
        /// <summary>
        /// 新增凭证
        /// 0:新增成功
        /// 1:用友帐套号没有设置，不能保存凭证
        /// 2:用友数据库信息不完整
        /// 3:会计期间不能为空，无法存储用友凭证
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="OrignIno_id">给与特定的凭证号</param>
        public void Insert(CW_PZMainView PzMain,ref U8Result result, int OrignIno_id = 0)
        {
            try
            {
                alreadyPeriod = "";
                alreadyPzNumber = "";
                string UFAccountNumber = PzMain.ExteriorDataBase;
                if (UFAccountNumber == string.Empty || UFAccountNumber==null)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "用友帐套号没有设置，不能保存凭证";
                    return;
                }
                int UFAccountYear = (int)PzMain.ExteriorYear;
                if (UFAccountYear == 0)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "用友数据库信息不完整";
                    return;
                }
                if ((int)PzMain.FiscalYear == 0)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "会计期间不能为空，无法存储用友凭证";
                    return;
                }
                this.U8DataBaseName = CommonFun.GetU8MatchDataBase(this.dbEntity, PzMain.ExteriorDataBase, PzMain.FiscalYear.ToString()); 
                //1-替换科目、人员、部门、客户、供应商、项目
                List<U8PzDetail> u8details = ReplaceEx(PzMain, result, UFAccountNumber, UFAccountYear,false);
                if (result.ResultNumber == 0) return;
                //2-存盘
                //获取新建凭证sql语句
                List<string> sqlstatments = GetInsertStatements(PzMain, u8details, result, OrignIno_id);
                if (result.ResultNumber == 0) return;
                if (this.U8Db.State == ConnectionState.Closed) this.U8Db.Open();
                SqlCommand cmm = this.U8Db.CreateCommand();
                SqlTransaction trans = this.U8Db.BeginTransaction();
                cmm.Connection = this.U8Db;
                cmm.Transaction = trans;
                try
                {
                    foreach (string sql in sqlstatments)
                    {
                        cmm.CommandText = sql;
                        cmm.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                catch(Exception ex)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = ex.Message;
                    trans.Rollback();
                }
                finally
                {
                    this.U8Db.Close();
                }
            }
            catch
            {
                result.ResultNumber = 0;
                result.ResultMessage = "新增会计凭证时出现未知错误";
            }
        }
        /// <summary>
        /// 删除凭证
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="result"></param>
        public void Delete(CW_PZMainView PzMain, ref U8Result result)
        {
            result.ResultNumber = 1;
            result.ResultMessage = "";
            if (PzMain.ExteriorDataBase ==string.Empty)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "用友帐套号没有设置，无法删除用友凭证";
                return;
            }
            if (PzMain.ExteriorYear == 0)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "会计期间不能为空，无法删除用友凭证";
                return;
            }
            this.U8DataBaseName = CommonFun.GetU8MatchDataBase(this.dbEntity, PzMain.ExteriorDataBase, PzMain.FiscalYear.ToString());
            if (this.U8Db.State == ConnectionState.Closed) this.U8Db.Open();
            SqlCommand cmm = this.U8Db.CreateCommand();
            cmm.Connection = this.U8Db;
            cmm.CommandText = "delete " + this.U8DataBaseName + "..gl_accvouch where coutno_id='" + PzMain.GUID.ToString() + "'";
            cmm.ExecuteNonQuery();
        }
        /// <summary>
        /// 更新凭证
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="result"></param>
        /// <param name="OrignIno_id">给与特定的凭证号</param>
        public void Update(CW_PZMainView PzMain, ref U8Result result, int OrignIno_id = 0)
        {
            result.ResultNumber = 1;
            result.ResultMessage = "";
            if (PzMain.ExteriorDataBase == string.Empty)
            {

                result.ResultNumber = 0;
                result.ResultMessage = "用友帐套号没有设置，无法修改用友凭证";
                return;
            }
            if (PzMain.ExteriorYear == 0)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "会计期间不能为空，无法修改用友凭证";
                return;
            }
            this.U8DataBaseName = CommonFun.GetU8MatchDataBase(this.dbEntity, PzMain.ExteriorDataBase, PzMain.FiscalYear.ToString());
            List<U8PzDetail> u8details = ReplaceEx(PzMain, result, "", 0);
            if (result.ResultNumber == 0) return;
            //获取新建凭证sql语句
            //List<string> sqlstatments = GetInsertStatements(PzMain, u8details, result, PzMain.GetIno_id(this.U8Db));
            List<string> sqlstatments = GetInsertStatements(PzMain, u8details, result, OrignIno_id);
            if (result.ResultNumber == 0) return;

            if (this.U8Db.State == ConnectionState.Closed) this.U8Db.Open();
            SqlCommand cmm = this.U8Db.CreateCommand();
            SqlTransaction trans = this.U8Db.BeginTransaction();
            cmm.Connection = this.U8Db;
            cmm.Transaction = trans;
            try
            {
                //1-删除凭证
                cmm.CommandText = "delete " + this.U8DataBaseName + "..gl_accvouch where coutno_id='" + PzMain.GUID.ToString() + "'";
                cmm.ExecuteNonQuery();
                foreach (string sql in sqlstatments)
                {
                    cmm.CommandText = sql;
                    cmm.ExecuteNonQuery();
                }
                trans.Commit();
            }
            catch(Exception ex)
            {
                result.ResultNumber = 0;
                result.ResultMessage = ex.Message;
                trans.Rollback();
            }
            finally
            {
                this.U8Db.Close();
            }
        }
        /// <summary>
        /// 用本凭证中的值为用友凭证中的值
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="result"></param>
        /// <param name="UFAccountNumber"></param>
        /// <param name="UFAccountYear"></param>
        /// <returns></returns>
        private List<U8PzDetail> ReplaceEx(CW_PZMainView PzMain, U8Result result, string UFAccountNumber, int UFAccountYear,bool IsB=true)
        {
            List<U8PzDetail> resultdetails = new List<U8PzDetail>();
            Dictionary<string, string> TransferDic = new Dictionary<string, string>();
            Dictionary<string, string> TransferDicVendor = new Dictionary<string, string>();
            result.ResultNumber = 1;
            result.ResultMessage = string.Empty;
            SS_ComparisonMainView ComparisonMain=dbEntity.SS_ComparisonMainView.First(e=>e.GUID_AccountDetail==PzMain.GUID_AccountDetail);
            if (ComparisonMain == null)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "无法找到转换模板";
                return resultdetails;
            }

            List<SS_ComparisonDetailView> ComparisonDetails = null;
            if (IsB)
            {
               ComparisonDetails= dbEntity.SS_ComparisonDetailView.Where(e => e.GUID_ComparisonMain == ComparisonMain.GUID).ToList();
            }
            else {
                ComparisonDetails = dbEntity.SS_ComparisonDetailView.Where(e => e.GUID_ComparisonMain == ComparisonMain.GUID
                    && e.Comparisontype != "AccountTitle").ToList();
            }
            foreach (SS_ComparisonDetailView ComparisonDetail in ComparisonDetails)
            {
                string keyvalue = ComparisonDetail.GUID_Self.ToString();
                if (ComparisonDetail.Comparisontype == "Vendor")
                {
                    if (TransferDicVendor.ContainsKey(keyvalue) == false)
                    {
                        TransferDicVendor.Add(keyvalue, ComparisonDetail.ExteriorKey);
                    }
                }
                else
                {
                    if (TransferDic.ContainsKey(keyvalue) == false)
                    {
                        TransferDic.Add(keyvalue, ComparisonDetail.ExteriorKey);
                    }
                }
            }


            List<CW_PZDetailView> pzdetails = dbEntity.CW_PZDetailView.Where(e => e.GUID_PZMAIN == PzMain.GUID).OrderBy(e => e.OrderNum).ToList();
            foreach (CW_PZDetailView pzdetail in pzdetails)
            {
                U8PzDetail udetail = new U8PzDetail(pzdetail);
                //PZDetail上需要转换(如果为空则对方也为空)
                //科目GUID_AccountTitle
                CW_AccountTitle accounttitle = dbEntity.CW_AccountTitle.First(e => e.GUID == pzdetail.GUID_AccountTitle);
                TransferValue(udetail, "GUID_AccountTitle", "AccountTitleKey", TransferDic);
                //部门GUID_Deparmenet
                TransferValue(udetail, "GUID_Department", "DepartmentKey", TransferDic);
                if (udetail.GetValue("UF8870*DepartmentKey") == "") {
                    int a = 1;
                }
                //人员GUID_Person
                TransferValue(udetail, "GUID_Person", "PersonKey", TransferDic);
                //项目GUID_Project
                TransferValue(udetail, "GUID_Project", "ProjectKey", TransferDic);
                //结算方式GUID_SettleType
                TransferValue(udetail, "GUID_SettleType", "SettleTypeKey", TransferDic);
                //客户或供应商GUID_Customer
                if (accounttitle.IsCustomer == true)
                {
                    TransferValue(udetail, "GUID_Customer", "CustomerKey", TransferDic);
                }
                else
                {
                    TransferValue(udetail, "GUID_Customer", "CustomerKey", TransferDicVendor);
                }
                resultdetails.Add(udetail);
            }
            return resultdetails;
        }
        /// <summary>
        /// 转换值
        /// </summary>
        /// <param name="udetail"></param>
        /// <param name="AttributeName"></param>
        /// <param name="TargetAttributeName"></param>
        /// <param name="TransferDic"></param>
        private void TransferValue(U8PzDetail udetail, string AttributeName, string TargetAttributeName, Dictionary<string, string> TransferDic)
        {
            string keyvalue = udetail.GetValue(AttributeName);
            if (TransferDic.ContainsKey(keyvalue) == false)
            {
                udetail.SetValue(AttributeName, "");
            }
            else
            {
                udetail.SetValue("UF8870*" + TargetAttributeName, TransferDic[keyvalue]);
            }
        }
        /// <summary>
        /// 校验凭证是否符合用友U8 8.70的标准
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="U8Details"></param>
        /// <param name="result"></param>
        private void SaveVerify(CW_PZMainView PzMain, List<U8PzDetail> U8Details, U8Result result)
        {
            result.ResultNumber = 1;
            result.ResultMessage ="";
            //判断数据库对象是否存在

            if (this.U8DataBaseName=="") this.U8DataBaseName = PzMain.GetU8DataBaseName(this.dbEntity);
            if (PzMain.ExistsDBObject(this.dbEntity, this.U8DataBaseName) == false)
            {
               // UFDATA_" + obj.ExteriorDataBase + "_" + obj.ExteriorYear
                result.ResultNumber = 0;
                result.ResultMessage = "不存在" + PzMain.ExteriorYear + "年帐套为" + PzMain.AccountKey + "用友数据库！";
                return;
            }
            //0-判断是否已经保存过此会计凭证
            if (PzMain.ExistsInU8(this.dbEntity, this.U8Db, this.U8DataBaseName))
            {
                result.ResultNumber = 0;
                result.ResultMessage = "此会计凭证在用友凭证表中已经存在，不能再次保存。";
                return;
            }
            //2-判断本系统会计期间与用友会计期间是否匹配
            int i = PzMain.GetUFPeriod(this.U8Db,result);
            if (result.ResultNumber == 0) return;
            int j = (int)PzMain.CWPeriod;
            if (i != j)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "本系统会计期间与用友会计期间不匹配，不能保存。";
                return;
            }
            //3-判断凭证所属会计期间是否已经结账 (指定年Year(strTmp))
            bool blnOK = PzMain.GetPeriodIsClose(this.dbEntity, this.U8Db, result, this.U8DataBaseName);
            if (result.ResultNumber == 0) return;
            if (blnOK)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "凭证所属会计期间已经结账，不能保存。";
                return;
            }
            //4-凭证类别及凭证类别排序是否正确

            PzMain.CheckSign(this.dbEntity, this.U8Db, result, this.U8DataBaseName);
            if (result.ResultNumber == 0) return;
            //5-制单人不能为空
            if (PzMain.Maker == string.Empty)
            {
                result.ResultNumber = 0;
                result.ResultMessage = "制单人不能为空，不能保存。";
                return;
            }

            //循环校验凭证分录
            decimal md = 0, mc = 0;
            i = 0; int han = 0;
            foreach (U8PzDetail detail in U8Details)
            {
                han++;
                //获取借贷方
                bool IsDc = detail.GetValue("IsDC").ToLower()=="true"?true:false;
                if (IsDc)
                {
                    md += decimal.Parse(detail.GetValue("Total_PZ"));
                }
                else
                {
                    mc += decimal.Parse(detail.GetValue("Total_PZ"));
                }
                //判断摘要信息
                string memo = detail.GetValue("PZMemo");
                if (memo == string.Empty)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "第" + i + 1 + "行摘要为空，不能保存。";
                    return;
                }
                if (memo.Length > 120)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "第" + i + 1 + "行摘要长度大于120个字符，不能保存。";
                    return;
                }
                //获取用友科目信息
                DataRow dr = detail.GetU8Code(this.dbEntity, PzMain, this.U8Db, result, this.U8DataBaseName);
                if (result.ResultNumber == 0) return;
                string cCode = dr["cCode"].ToString();
                bool bEnd = dr["bEnd"] == null ? false : bool.Parse(dr["bEnd"].ToString());
                bool bBank = dr["bBank"] == null ? false : bool.Parse(dr["bBank"].ToString());
                bool bDept = dr["bDept"] == null ? false : bool.Parse(dr["bDept"].ToString());
                bool bPerson = dr["bPerson"] == null ? false : bool.Parse(dr["bPerson"].ToString());
                bool bCus = dr["bCus"] == null ? false : bool.Parse(dr["bCus"].ToString());
                bool bSup = dr["bSup"] == null ? false : bool.Parse(dr["bSup"].ToString());
                bool bItem = dr["bItem"] == null ? false : bool.Parse(dr["bItem"].ToString());
                string cass_Item = dr["cass_Item"].ToString();
                //6-判断用友会计科目是否为最末级
                if (!bEnd)
                {
                    result.ResultNumber = 0;
                    result.ResultMessage = "用友会计科目为非末级，不能保存。";
                    return;
                }
                //7-判断部门编号是否正确
                if (bDept || bPerson)
                {
                    detail.CheckcDeptCode(this.dbEntity, PzMain, this.U8Db, result, this.U8DataBaseName);
                    if (result.ResultNumber == 0) { 
                        han=0;
                    }
                    if (result.ResultNumber == 0) return;
                }
                //8-判断人员编号是否正确
                if (bPerson)
                {
                    detail.CheckcPersonCode(this.dbEntity, PzMain, this.U8Db, result, this.U8DataBaseName);
                    if (result.ResultNumber == 0) return;
                }
                //9-判断客户信息是否正确
                if (bCus)
                {
                    detail.CheckcCusCode(this.dbEntity, PzMain, this.U8Db, result, this.U8DataBaseName);
                    if (result.ResultNumber == 0) return;
                }
                //10-判断供应商信息是否正确
                if (bSup)
                {
                    detail.CheckcVenCode(this.dbEntity, PzMain, this.U8Db, result, this.U8DataBaseName);
                    if (result.ResultNumber == 0) return;
                }
                //11-判断项目信息是否正确
                if (bItem)
                {
                    if (cass_Item == string.Empty)
                    {
                        result.ResultNumber = 0;
                        result.ResultMessage = "用友科目未设置项目大类，不能保存。";
                        return;
                    }
                    detail.CheckcItemCode(this.dbEntity, PzMain, this.U8Db, cass_Item, result, this.U8DataBaseName);
                    if (result.ResultNumber == 0) return;
                }
              
            }
            //12-判断借贷方是否平衡

            if (Math.Abs(md - mc) > decimal.Parse("0.0001"))
            {
                result.ResultNumber = 0;
                result.ResultMessage = "凭证借贷方不相等，不能保存。";
                return;
            }
        }
        /// <summary>
        /// 获取新建凭证sql语句
        /// </summary>
        /// <param name="PzMain"></param>
        /// <param name="U8Details"></param>
        /// <param name="result"></param>
        /// <param name="OrignIno_id"></param>
        /// <returns></returns>
        private List<string> GetInsertStatements(CW_PZMainView PzMain, List<U8PzDetail> U8Details, U8Result result, int OrignIno_id=0)
        {
            if (this.U8DataBaseName == "") this.U8DataBaseName = CommonFun.GetU8MatchDataBase(this.dbEntity, PzMain.ExteriorDataBase, PzMain.FiscalYear.ToString());

            List<string> sqlstatments = new List<string>();
            result.ResultNumber = 1;
            result.ResultMessage = "";
            //校验
            SaveVerify(PzMain, U8Details, result);
            if (result.ResultNumber == 0) return sqlstatments;
            //拼写存盘用SQL
            string strMain, strValueMain;
            //会计期间
            strMain = "iPeriod"; strValueMain = PzMain.CWPeriod.ToString();
            //凭证类别
            strMain += ",cSign"; strValueMain += ",'" + PzMain.PZTypeKey + "'";
            //类别排序号
            strMain += ",isignseq"; strValueMain += "," + PzMain.PZTypeOrder;
            //凭证日期
            strMain += ",dbill_date"; strValueMain += ",'" + PzMain.DocDate.ToString() + "'";
            //凭证号
            strMain += ",iYear"; strValueMain += ",'" + PzMain.DocDate.Year + "'";
            strMain += ",iYPeriod"; strValueMain += ",'" + PzMain.DocDate.Year + GetUFPeriodAll(PzMain.CWPeriod.ToString()) + "'";

            int iNo_id = OrignIno_id > 0 ? OrignIno_id : PzMain.GetIno_id(this.dbEntity, this.U8Db,this.U8DataBaseName);
            strMain += ",ino_id"; strValueMain += "," + iNo_id;
            //凭证状态
            strMain += ",iflag"; strValueMain += ",null";
            //制单人
            strMain += ",cbill"; strValueMain += ",'" + PzMain.Maker + "'";
            //附单据数
            var billCount=(PzMain.BillCount==null?0:PzMain.BillCount);
            strMain += ",idoc"; strValueMain += "," +billCount;
            //外系统名称
            strMain += ",cOutSysName"; strValueMain += ",'BAOTH'";
            //外系统单据编号
            strMain += ",cOutNo_ID"; strValueMain += ",'" + PzMain.GUID.ToString() + "'";
            var orderNum = 1;
            foreach (U8PzDetail detail in U8Details)
            {
                string md, mc;
                string strDetail = strMain, strValueDetail = strValueMain;
                //行号
                strDetail += ",inid"; strValueDetail += "," + orderNum;
                orderNum++;
                //摘要
                var memo = (detail.GetValue("PZMemo") + " ");
                memo = memo.Length > 120 ? memo.Substring(0, 120) : memo;
                strDetail += ",cdigest"; strValueDetail += ",'" +memo + "'";
                //金额
                bool isDc = bool.Parse(detail.GetValue("IsDC")) ? true : false;
                if (isDc)
                {
                    md = detail.GetValue("Total_PZ"); mc = "0";
                }
                else
                {
                    mc = detail.GetValue("Total_PZ"); md = "0";
                }
                strDetail += ",md,mc"; strValueDetail += "," + md + "," + mc;

                //获取用友科目信息
                DataRow dr = detail.GetU8Code(this.dbEntity, PzMain, this.U8Db, result,this.U8DataBaseName);
                if (result.ResultNumber == 0) return sqlstatments;
                string cCode = dr["cCode"].ToString();
                strDetail += ",ccode"; strValueDetail += ",'" + cCode +"'";
                bool bEnd = dr["bEnd"] == null ? false : bool.Parse(dr["bEnd"].ToString());
                bool bBank = dr["bBank"] == null ? false : bool.Parse(dr["bBank"].ToString());
                bool bDept = dr["bDept"] == null ? false : bool.Parse(dr["bDept"].ToString());
                bool bPerson = dr["bPerson"] == null ? false : bool.Parse(dr["bPerson"].ToString());
                bool bCus = dr["bCus"] == null ? false : bool.Parse(dr["bCus"].ToString());
                bool bSup = dr["bSup"] == null ? false : bool.Parse(dr["bSup"].ToString());
                bool bItem = dr["bItem"] == null ? false : bool.Parse(dr["bItem"].ToString());
                string cass_Item = dr["cass_Item"].ToString();
                //如果为银行科目，并且为贷方，并且结算方式不为空时，设置结算方式
                if (bBank)
                {
                    if (detail.GetValue("SettleTypeKey") != string.Empty) { strDetail += ",csettle"; strValueDetail += ",'" + detail.GetValue("SettleTypeKey") + "'"; }
                    //票据号
                    strDetail += ",cn_id"; strValueDetail += ",'" + detail.GetValue("BillNum") + "'";
                    //票据日期
                    strDetail += ",dt_Date"; strValueDetail += ",Null";
                }
                //部门
                if (bDept || bPerson)
                {
                    strDetail += ",cDept_ID"; strValueDetail += ",'" + detail.GetValue("UF8870*DepartmentKey") + "'";
                }
                //人员
                if (bPerson)
                {
                    strDetail += ",cperson_id"; strValueDetail += ",'" + detail.GetValue("UF8870*PersonKey") + "'";
                }
                //客户
                if (bCus)
                {
                    strDetail += ",ccus_id"; strValueDetail += ",'" + detail.GetValue("UF8870*CustomerKey") + "'";
                }
                //供应商
                if (bSup)
                {
                    strDetail += ",csup_id"; strValueDetail += ",'" + detail.GetValue("UF8870*CustomerKey") + "'";
                }
                //项目
                if (bItem)
                {
                    strDetail += ",citem_id"; strValueDetail += ",'" + detail.GetValue("UF8870*ProjectKey") + "'";
                    //项目大类
                    strDetail += ",citem_class"; strValueDetail += ",'" + cass_Item + "'";
                }
                string itemsql = "Insert into  " + this.U8DataBaseName 
                    + "..gl_accvouch (" + strDetail + ") values (" + strValueDetail + ")";
                sqlstatments.Add(itemsql);
            }
            return sqlstatments;
        }

        private string GetUFPeriodAll(string iperiod)
        {
            switch (iperiod)
            {
                case "1":
                    return "01";
                case "2":
                    return "02";
                case "3":
                    return "03";
                case "4":
                    return "04";
                case "5":
                    return "05";
                case "6":
                    return "06";
                case "7":
                    return "07";
                case "8":
                    return "08";
                case "9":
                    return "09";
                default:
                    return iperiod;
            }
        }
        /// <summary>
        /// 根据凭证编号获取对应帐套GL_accvouch表信息
        /// </summary>
        /// <param name="exteriorDataBase"></param>
        /// <param name="year"></param>
        /// <param name="ino_id"></param>
        /// <returns></returns>
        public DataTable GetGL_accvouch(string exteriorDataBase, int year, string ino_id,string databaseName="")
        {
            try
            {
                if (databaseName == "") databaseName = CommonFun.GetU8MatchDataBase(this.dbEntity, exteriorDataBase, year.ToString());
                this.U8DataBaseName = databaseName;
                string strSQL = "select coutno_id,ino_id from " + this.U8DataBaseName + "..gl_accvouch ";
                if (!string.IsNullOrEmpty(ino_id))
                {
                    strSQL += " where ino_id='"+ino_id+"' ";
                }
                if (this.U8Db.State == ConnectionState.Closed) this.U8Db.Open();
                SqlCommand cmm = this.U8Db.CreateCommand();
                cmm.CommandText = strSQL;
                System.Data.DataSet ds = new System.Data.DataSet();
                System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    System.Data.DataTable dt = ds.Tables[0];
                    return dt;
                }
                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                this.U8Db.Close();
            }
        }
        /// <summary>
        /// 根据编号获取对应帐套GL_accvouch表信息
        /// </summary>
        /// <param name="exteriorDataBase"></param>
        /// <param name="year"></param>
        /// <param name="ino_id"></param>
        /// <returns></returns>
        public DataTable GetGL_accvouchByCoutNO_ID(string exteriorDataBase, int year, string coutno_id,string databaseName="")
        {
            try
            {
                if (databaseName == "") databaseName = Common.CommonFun.GetU8MatchDataBase(this.dbEntity, exteriorDataBase, year.ToString());
                this.U8DataBaseName = databaseName;
                if (string.IsNullOrEmpty(this.U8DataBaseName)) return null;
                string strSQL = "select distinct coutno_id,ino_id from " + this.U8DataBaseName + "..gl_accvouch ";
                if (!string.IsNullOrEmpty(coutno_id))
                {
                    strSQL += " where coutno_id='" + coutno_id + "' ";
                }
                if (this.U8Db.State == ConnectionState.Closed) this.U8Db.Open();
                SqlCommand cmm = this.U8Db.CreateCommand();
                cmm.CommandText = strSQL;
                System.Data.DataSet ds = new System.Data.DataSet();
                System.Data.SqlClient.SqlDataAdapter da = new SqlDataAdapter(cmm);
                da.Fill(ds);
                if (ds.Tables.Count > 0)
                {
                    System.Data.DataTable dt = ds.Tables[0];
                    return dt;
                }
                return null;
            }
            catch
            {
                return null;
            }
            finally
            {
                this.U8Db.Close();
            }
        }
    }
    
}
