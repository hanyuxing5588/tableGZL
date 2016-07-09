using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessModel;
using System.Linq.Expressions;
using System.Data;
namespace Business.CommonModule
{
   public static class CommonBusinessSelect
    {
       /// <summary>
       /// 获取支票列表 -支票管理用


       /// </summary>
       /// <param name="guidBankaccount"></param>
       /// <param name="checkType"></param>
       /// <returns></returns>
       public static List<object> GetCheckList(Guid guidBankaccount, string checkNumber, string DocTypeKey, bool isInvalid = false,bool Asc=true)
       {
           var context = new BusinessModel.BusinessEdmxEntities();
           var guidList = context.SS_BankAccountView.Where(e => e.IsStop == false).OrderBy(e => e.BankAccountKey).Select(e => e.GUID);
         
           var q=  from e in context.CN_CheckView 
                   join y in context.CN_CheckDrawMain
                   on e.GUID equals y.GUID_Check into temp
                   from t in temp.DefaultIfEmpty()
                   select new
                    {
                        e.GUID,
                        e.CheckNumber,
                        CheckType = e.CheckType == 0 ? "现金支票" : "转账支票",
                        checkTypeSource= e.CheckType,
                        e.GUID_BankAccount,
                        e.BankAccountNo,
                        e.BankAccountName,
                        e.BankName,
                        e.DWName,
                        e.IsGuoKu,
                        t.CheckMoney,
                        t.CheckUsed,
                        IsInvalidSource=  e.IsInvalid,
                        e.InvalidDatetime,
                        LingYong = t.CheckMoney != null ? "已领取" : "未领取",
                        IsInvalid = e.IsInvalid == true ? "是" : "否"
                    };
          
            q=q.Where(e => guidList.Contains(e.GUID_BankAccount));
           if (guidBankaccount != null && guidBankaccount != Guid.Empty && !string.IsNullOrEmpty(guidBankaccount.ToString()))
           {
             q= q.Where(e => e.GUID_BankAccount == guidBankaccount);
           }
           if (isInvalid) {
               q = q.Where(e => (e.IsInvalidSource == false || e.IsInvalid == null));
           }
           if (!string.IsNullOrEmpty(checkNumber)) {
               q = q.Where(e => e.CheckNumber.Contains(checkNumber));
           }
           if (DocTypeKey=="21")
           {
               q = q.Where(e=>e.checkTypeSource==0);
           }

           q = Asc ? q.OrderBy(e => e.CheckNumber) : q.OrderByDescending(e => e.CheckNumber);

           var result = q.AsEnumerable().Select(
               e => new {
                   e.GUID,
                   e.CheckNumber,
                   CheckType = e.checkTypeSource == 0 ? "现金支票" : "转账支票",
                   checkTypeSource = e.CheckType,
                   e.GUID_BankAccount,
                   e.BankAccountNo,
                   e.BankAccountName,
                   e.BankName,
                   e.DWName,
                   e.IsGuoKu,
                   e.CheckMoney,
                   e.CheckUsed,
                   IsInvalidSource = e.IsInvalid,
                   InvalidDatetime=string.IsNullOrEmpty(e.InvalidDatetime+"")?"":e.InvalidDatetime.Value.ToString("yyyy-MM-dd"),
                   LingYong = e.CheckMoney != null ? "已领取" : "未领取",
                   IsInvalid = e.IsInvalid == "1" ? "是" : "否"
               }).ToList<object>();
           return result;

       }

       public static DataTable GetCheckTable(Guid guidBankaccount,string strWhere) 
       {
           if (strWhere == "")
           {
           }
           if (guidBankaccount != Guid.Empty) {
               strWhere =strWhere+ " and GUID_BankAccount='"+guidBankaccount+"'";
           }
           var sqlformat = @"
           
SELECT 
GUID,
CheckNumber,
CASE WHEN  CheckType = 0 THEN '现金支票' ELSE  '转账支票' END AS CheckType,
CheckType as checkTypeSource,
GUID_BankAccount,
BankAccountNo,
BankAccountName,
BankName,
DWName,
IsGuoKu,
IsInvalid as IsInvalidSource,
CASE WHEN  IsInvalid = 1 THEN '是' ELSE  '否' END AS IsInvalid,
InvalidDatetime
FROM CN_CheckView a
WHERE GUID NOT in (SELECT GUID_Check FROM  CN_CheckDrawMain GROUP BY GUID_Check) {0} order by CAST(CheckNumber AS INT) asc,ISNULL(IsInvalid,0)";
           var dt = Business.Common.DataSource.ExecuteQuery(string.Format(sqlformat, strWhere));
           return dt;
       }

       /*包括已当前经领取的 编辑用*/
       public static DataTable GetCheckTable(Guid guidBankaccount,string curCheckGuid, string strWhere)
       {
           if (strWhere == "")
           {
           }
           if (guidBankaccount != Guid.Empty)
           {
               strWhere = strWhere + " and GUID_BankAccount='" + guidBankaccount + "'";
           }

           string skap = string.Empty;
           if (!string.IsNullOrEmpty(curCheckGuid)) skap = "where GUID_Check not in ('" + curCheckGuid.Replace(",","','") + "')";

           var sqlformat = @"
           
SELECT 
GUID,
CheckNumber,
CASE WHEN  CheckType = 0 THEN '现金支票' ELSE  '转账支票' END AS CheckType,
CheckType as checkTypeSource,
GUID_BankAccount,
BankAccountNo,
BankAccountName,
BankName,
DWName,
IsGuoKu,
IsInvalid as IsInvalidSource,
CASE WHEN  IsInvalid = 1 THEN '是' ELSE  '否' END AS IsInvalid,
InvalidDatetime
FROM CN_CheckView a
WHERE GUID NOT in (SELECT GUID_Check FROM  CN_CheckDrawMain {0} GROUP BY GUID_Check) {1} order by CAST(CheckNumber AS INT) asc,ISNULL(IsInvalid,0)";
           var dt = Business.Common.DataSource.ExecuteQuery(string.Format(sqlformat, skap,strWhere));
           return dt;
       }
       /*包括已经领取的 编辑用*/
       public static DataTable GetCheckTable1(Guid guidBankaccount, string strWhere)
       {
           if (strWhere == "")
           {
           }
           if (guidBankaccount != Guid.Empty)
           {
               strWhere = strWhere + " and GUID_BankAccount='" + guidBankaccount + "'";
           }
           var sqlformat = @"
           
SELECT 
GUID,
CheckNumber,
CASE WHEN  CheckType = 0 THEN '现金支票' ELSE  '转账支票' END AS CheckType,
CheckType as checkTypeSource,
GUID_BankAccount,
BankAccountNo,
BankAccountName,
BankName,
DWName,
IsGuoKu,
IsInvalid as IsInvalidSource,
CASE WHEN  IsInvalid = 1 THEN '是' ELSE  '否' END AS IsInvalid,
InvalidDatetime
FROM CN_CheckView a
WHERE 1=1 {0} order by CAST(CheckNumber AS INT) asc,ISNULL(IsInvalid,0)";
           var dt = Business.Common.DataSource.ExecuteQuery(string.Format(sqlformat, strWhere));
           return dt;
       }
   }
}
