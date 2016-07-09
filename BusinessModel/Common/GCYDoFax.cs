using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using BusinessModel;

namespace Business.Casher
{
 /// <summary>
 /// 针对工程院的算税标准
 /// </summary>
 /// eg: var doFax=new GCYDoFax(GUID);
 ///   doFax.DoTaxCaculte(context,false||true);
    public class GCYDoFax : BillDoFax
    {
        public Guid DocId { get; set; } 
        public GCYDoFax(Guid docGUID) 
        {
            this.DocId = docGUID;
        }
        private string mainSql = "select 0 as  DocFlag,BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,23 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_bx) from bx_detail where guid_bx_main=bx_mainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from bx_mainview where GUID='{0}'";
        private string detailSql = "select b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,24 as ClassId,23 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.GUID_BX_Main as GUID_Main,a.GUID_BGCode, a.BGCodeKey,a.BGCodeName,a.ProjectName,a.GUID_Project,FeeMemo as Memo,a.Total_Bx as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,FeeDate as DocDate,a.ProjectClassKey,a.Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,a.IsCustomer,a.IsVendor,a.BGTypeKey from bx_detailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where a.GUID_BX_Main='{0}'";
        
        public override string GetInviteSql(Guid invitePersonGuid, int curMonth, int curYear, Guid inviteFeeGuid)
        {
            string sqlFormat = "select isnull(sum(Total_Real),0) as SumTotalReal,isnull(sum(Total_Tax),0) as SumTotalTax,isnull(sum(Total_Bx),0) as SumTotalBx from BX_InviteFee" +
                   " where guid_inviteperson='{0}' and guid<>'{1}' and  GUID_BX_main in (select guid from BX_main where  month(MakeDate)='{2}' and year(MakeDate)='{3}')   and  GUID_BX_main not in (select guid from BX_main where DocState=9) and GUID_BX_main " +
                    "In (select DataId from OAO_WOrkFlowProcessData)";
            return string.Format(sqlFormat,invitePersonGuid, inviteFeeGuid,curMonth,curYear);
        }
        public void DoTaxCaculte(ObjectContext context,bool isSave=true) 
        {
            string strSqlMain = string.Format(mainSql, DocId);
            string strSqlDetail = string.Format(detailSql, DocId);
            var bills = context.ExecuteStoreQuery<Bill>(strSqlMain).ToList();
            var billDetails = context.ExecuteStoreQuery<BillDetail>(strSqlDetail).ToList();
            SetDetailBillsToBill(bills, billDetails);
            var oResult = new OAOResult();
            if (isSave)
            {
                SaveAlreadyTaxObjects(context, bills, DateTime.Now, oResult);
                context.SaveChanges();
              
            }
            else {
                DoTaxCaculte(context, bills, DateTime.Now, oResult);
            }
        }
        //初始化DetailBill到bill上
        public void SetDetailBillsToBill(List<Bill> listBill, List<BillDetail> listDetail)
        {
            foreach (var item in listBill)
            {
                var listDetailTemp = listDetail.Where(e => e.GUID_Main == item.GUID).ToList();
                item.Details = listDetailTemp;
            }
        }
    }
}
