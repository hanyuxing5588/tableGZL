using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using System.IO;


namespace CAE.Report
{
    /// <summary>
    /// 个税申报表
    /// </summary>
    public class GSSBB:BaseReport
    {
        public string colChar = "&nbsp&nbsp&nbsp";//用于显示分级用       
        public GSSBB(string key)
            : base(key)
        {
          
        }
        private string lwfSql = " select GUID_InvitePerson,'' as  PersonKey," +
	"InvitePersonName as PersonName,"+
	"CredentialTypeName,"+
	"InvitePersonIDCard as IDCard,"+

     "   '001' as gjdp,"+
     "   '' as zybm,"+
	"'02' as DepartmentKey,"+
	"'02' as sdxm,"+
      "   Sum(Total_bx) as sre,"+
      "   '' as mssre,"+
      "   '' as yxkcdsf,"+
     "   '800' as fykcbz,"+
	 " '' as zykcdjke,"+
     "    '' as ynssde,"+
	" '' as sl,"+
	" Sum(Total_Tax) as ykse"+
 " from dbo.BX_InviteFeeView where (IsUnit<>1 or GUID_InvitePerson in ( select guid from ss_person where IsTax=1))   " +//不是本单位 或者 时本单位但是 必须是算税的
 " and GUID_bx_main in (select guid from BX_Main where Month(Makedate)={1} and Year(MakeDate)={0} and GUID in (select DataId from dbo.OAO_WorkFlowProcessData where ProcessId in(  select id from oao_workflowProcess where state=1)))" +//流程结束后
" group by InvitePersonIDCard,InvitePersonName,CredentialTypeName,GUID_InvitePerson";
        public override void Init()
        {


            this.SqlFormat = "select newid() as GUID_InvitePerson,personTemp.PersonKey,personTemp.PersonName,personTemp.CredentialTypeName,personTemp.IDCard,'001' as gjdq,''zybm, "
                            + " '01' as DepartmentKey ,"
                            + " '0101'  as sdxm , "   //所得项目
                            + "b.ItemValue as sre,"//收入额
                            + " '' as mssre,"//免税收入额
                            + "'' as yxkcdsf, "//允许扣除的税费
                            + " '3500.00' as fykcbz,"
                            + " '' as zykcdjke, "//准予扣除的捐款额
                            + " '' as ynssde,"//应纳税所得额
                            + "'' as sl,"
                            +"a.ItemValue as ykse "
                            + "  from (select GUID,PersonKey,PersonName,CredentialTypeName,IDCard,PersonTypeName from SS_PersonView  where IsTax<>1 and GUID in(select GUID_Person from SA_PlanActionDetailView where actionYear='{0}' and actionMouth='{1}')) personTemp "
                            + " left join (select  GUID_Person,PersonKey,PersonName,ItemName,ItemValue from SA_PlanActionDetailView  where actionYear='{0}' and actionMouth='{1}' and ItemName in('所得税') )a on personTemp.GUID=a.GUID_Person"
                            + " left join (select  GUID_Person,PersonKey,PersonName,ItemName,ItemValue from SA_PlanActionDetailView  where actionYear='{0}' and actionMouth='{1}' and ItemName in('应纳税所得额') )b on personTemp.GUID=b.GUID_Person"  
                            + " order by personTemp.PersonKey ";
            this.tempalte = Path.Combine(this.tempalte, "gssbb.xls");        
      }
        
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(gssbbCondition conditionModel)
        {
         
            string strsql =this.SqlFormat;
            strsql = this.lwfSql + " union " + strsql;
            if (conditionModel.Year == "0")
            {
                conditionModel.Year = DateTime.Now.Year.ToString();
            }
            if (conditionModel.Month == "0")
            {
                conditionModel.Month = DateTime.Now.Month.ToString();
            }
           
           
            return strsql = string.Format(strsql,
                                          conditionModel.Year,
                                          conditionModel.Month
                                          );
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        public DataTable GetReport(SearchCondition conditions, out string msgError)
        {
            msgError = "";
            string sqlStr = string.Empty;
            gssbbCondition conditionModel = (gssbbCondition)conditions;
            sqlStr = GetSql(conditionModel);
            DataTable dt = LoadData(sqlStr, ref msgError);
            return dt;
        }
       
      
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        private DataTable LoadData(string sql, ref string msgerror)
        {
           var dt= DataSource.ExecuteQuery(sql);
           if (dt == null || dt.Rows.Count == 0)
           {
               msgerror = "无数据！";
               return null;
           }
           return dt;
        }
        public string tempalte = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];
        //导出报表
        public override string GetExportPath(DataTable data, out string fileName, out string message)
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
                string filePath = ExportExcel.Export(data, this.tempalte, 2, 2, new List<ExcelCell>() { });
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
