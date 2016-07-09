using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Linq.Expressions;

namespace BusinessModel
{
    public enum EBillDocType
    {
        报销管理 = 0,
        收入管理 = 1,
        往来管理 = 2,
        提现管理 = 3,
        收款管理 = 4,
        公务卡汇总管理 = 5,
        专用基金管理 = 6,
        空=7
    }
   public class BusinessModelExt
    {
       /// <summary>
       /// 根据url获取单据类型 没有补充完成。

       /// </summary>
       /// <param name="url"></param>
       /// <returns></returns>
       public static EBillDocType GetDocTypeByUrl(ObjectContext context,string url) 
       {
          EBillDocType edDoctype= EBillDocType.空;
          var docType = context.CreateObjectSet<BusinessModel.SS_DocTypeView>().FirstOrDefault(e => e.DocTypeUrl == url);
           if (docType!=null) {
               switch (docType.YWTypeKey) {
                   case "02":
                       if (docType.DocTypeKey == "24")
                       {
                           edDoctype = EBillDocType.公务卡汇总管理;
                       }
                       else
                       {
                           edDoctype = EBillDocType.报销管理;
                       }
                       break;
                   case "03":
                       edDoctype = EBillDocType.收入管理;
                       break;
                   case "0502":
                       edDoctype = EBillDocType.往来管理;
                       break;
                   case "04":
                       edDoctype = EBillDocType.专用基金管理;
                       break;
                   case "0801":
                       edDoctype = EBillDocType.收款管理;
                       break;
                   case "0802":
                       edDoctype = EBillDocType.提现管理;
                       break;
               }
           
           }
           return edDoctype;
       }
       public static EBillDocType GetDocTypeByUrl(string ywKey,string docTypeKey)
       {
           EBillDocType edDoctype = EBillDocType.空;
               switch (ywKey)
               {
                   case "02":
                       if (docTypeKey == "24")
                       {
                           edDoctype = EBillDocType.公务卡汇总管理;
                       }
                       else
                       {
                           edDoctype = EBillDocType.报销管理;
                       }
                       break;
                   case "03":
                       edDoctype = EBillDocType.收入管理;
                       break;
                   case "0502":
                       edDoctype = EBillDocType.往来管理;
                       break;
                   case "04":
                       edDoctype = EBillDocType.专用基金管理;
                       break;
                   case "0801":
                       edDoctype = EBillDocType.收款管理;
                       break;
                   case "0802":
                       edDoctype = EBillDocType.提现管理;
                       break;
               }

           return edDoctype ;
       }
       //设置流程单据的状态
       public static void SetProcessDocStatus(ObjectContext context, EBillDocType docType, Guid docGuid) 
       {
           switch (docType) {
               case EBillDocType.报销管理:
                   var ent = context.CreateObjectSet<BX_Main>().FirstOrDefault(e => e.GUID== docGuid);
                   if (ent != null) {
                       ent.DocState = "999";
                   }
                   break;
               case EBillDocType.往来管理:
                    var ent1= context.CreateObjectSet<WL_Main>().FirstOrDefault(e => e.GUID== docGuid);
                    if (ent1 != null)
                    {
                        ent1.DocState = "999";
                   }
                   break;
               case EBillDocType.提现管理:
                   break;
               case EBillDocType.收款管理:
                   break;
               case EBillDocType.公务卡汇总管理:
                    var ent2 = context.CreateObjectSet<BX_CollectMain>().FirstOrDefault(e => e.GUID == docGuid);
                    if (ent2!= null)
                    {
                        ent2.DocState = "999";
                   }
                   break;
           }
       }
       public static bool GetMoneySumByGuidWithDocType(ObjectContext context, Guid docGuid, string docUrl, out double moneySum) 
       {
           var docType = GetDocTypeByUrl(context, docUrl);
           moneySum = 0;
           switch (docType) { 
               case EBillDocType.报销管理:
                    var listDetail = context.CreateObjectSet<BX_Detail>().Where(e => e.GUID_BX_Main == docGuid).ToList();
                    if(listDetail==null)return false;
                     moneySum = listDetail.Sum(e => e.Total_BX);
                   break;
               case EBillDocType.收入管理:
                    var listDetail1 = context.CreateObjectSet<SR_Detail>().Where(e => e.GUID_SR_Main == docGuid).ToList();
                    if(listDetail1==null)return false;
                    moneySum = listDetail1.Sum(e => e.Total_SR);
                   break;
               case EBillDocType.往来管理:
                    var listDetail2 = context.CreateObjectSet<WL_Detail>().Where(e => e.GUID_WL_Main == docGuid).ToList();
                    if(listDetail2==null)return false;
                    moneySum = listDetail2.Sum(e => e.Total_WL);
                   break;

               case EBillDocType.提现管理:
                    var listDetail3 = context.CreateObjectSet<CN_CashDetail>().Where(e => e.GUID_CN_CashMain == docGuid).ToList();
                    if(listDetail3==null)return false;
                    moneySum = (double)listDetail3.Sum(e => e.Total_Cash);
                   break;
               case EBillDocType.收款管理:
                    var skMian = context.CreateObjectSet<SK_Main>().FirstOrDefault(e => e.GUID == docGuid);
                    if (skMian == null) return false;
                    moneySum = skMian.Total_SK;
                   break;
               case EBillDocType.公务卡汇总管理:
                     
                   //方式一
                   var listDetail4 = context.CreateObjectSet<BX_CollectDetail>().Include("BX_Detail").Where(e => e.GUID_BXCOLLECTMain == docGuid).ToList();
                   if (listDetail4 == null) return false;
                   moneySum = listDetail4.Sum(e => e.BX_Detail.Total_BX);
                   //方式二

                   //var listDetail4 = context.CreateObjectSet<BX_Detail>()
                   //    .Join(context.CreateObjectSet<BX_CollectDetail>()
                   //    .Where(n=>n.GUID_BXCOLLECTMain==docGuid), e => e.GUID, t => t.GUID_BXDetail, (e, t) => e).ToList();
                   //if (listDetail4 == null) return false;
                   //moneySum = listDetail4.Sum(e => e.Total_BX);
                   break;
               case EBillDocType.专用基金管理:
                   var listDetail5 = context.CreateObjectSet<JJ_Detail>().Where(e => e.GUID_JJ_Main == docGuid).ToList();
                   if (listDetail5 == null) return false;
                   moneySum = listDetail5.Sum(e => e.Total_JJ);
                   break;

           }
           return true;
       }
    
    }


   public static class PredicateBuilder 
   {
       public static Expression<Func<T, bool>> True<T>() { return v => true; }
       public static Expression<Func<T, bool>> False<T>() { return v => false; }
       public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) 
       {
           var invokeExpr = Expression.Invoke(expr2, expr1.Parameters);
           return Expression.Lambda<Func<T,bool>>(Expression.Or(expr1.Body,invokeExpr),expr1.Parameters);
       }
       public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2) 
       {
           var invokeExpr = Expression.Invoke(expr2, expr1.Parameters);
           return Expression.Lambda<Func<T, bool>>(Expression.And(expr1.Body, invokeExpr), expr1.Parameters);
       }
   }
}
