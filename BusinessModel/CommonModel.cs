using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessModel
{
    //借方 贷方 单据转换 
    public class Bill
    {
        public int ClassId { get; set; }
        public string DocTypeKey { get; set; }
        public Guid GUID { get; set; }
        public string ChildGUID { get; set; }
        public string DocNum { get; set; }
        public string DocTypeName { get; set; }
        public string DocDate { get; set; }
        public string PersonName { get; set; }
        public string DepartmentName { get; set; }
        public string DWName { get; set; }
        public string DocMemo { get; set; }
        public double Total_XX { get; set; }
        public int? BillCount { get; set; }
        public Guid GUID_DW { get; set; }
        public string YWTypeKey { get; set; }
        public string YWTypeName { get; set; }
        public string DWKey { get; set; }
        public string PersonKey { get; set; }
        public string DepartmentKey { get; set; }
        public Guid GUID_Department { get; set; }
        public Guid GUID_Person { get; set; }
        public Guid GUID_YWType { get; set; }
        public Guid GUID_DocType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int SRWLTypeClassID { get; set; }
        public int DocFlag { get; set; }

        public List<BillDetail> Details { get; set; }

    }
    public class BillCN_PaymentNumber
    {
        public Guid? Guid_FunctionClass { get; set; }
        public string FunctionClassName { get; set; }
        public Guid? Guid_EconomyClass { get; set; }
        public string ExpendTypeKey { get; set; }
    }
    public class BillDetail
    {
        public string PersonName { get; set; }
        public string DocTypeKey { get; set; }
        public int ClassMainId { get; set; }
        public int ClassId { get; set; }
        public Guid GUID { get; set; }
        public Guid GUID_Main { get; set; }
        public double Total_XX { get; set; }
        public Guid? GUID_PaymentNumber { get; set; }
        public Guid? GUID_Project { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string BGCodeKey { get; set; }
        public string BGCodeName { get; set; }
        public Guid? GUID_BGCode { get; set; }
        public string Memo { get; set; }
        public string PaymentNumber { get; set; }
        public Guid? GUID_SettleType { get; set; }
        public string SettleTypeName { get; set; }
        public bool IsGuoKu { get; set; }
        public string BGSourceKey { get; set; }
        public bool? IsProject { get; set; }
        public string BGSourceName { get; set; }
        public string EconomyClassKey { get; set; }
        public string FinanceCode { get; set; }
        public string ExtraCode { get; set; }
        public string FunctionClassKey { get; set; }
        public string PersonKey { get; set; }
        public string DepartmentKey { get; set; }
        public string DepartmentName { get; set; }
        public Guid? GUID_Department { get; set; }
        public string FeeDate { get; set; }
        public string ProjectClassKey { get; set; }
        public double Total_Tax { get; set; }
        public string SettleTypeKey { get; set; }
        public string CustomerKey { get; set; }
        public string CustomerName { get; set; }
        public bool? IsCustomer { get; set; }
        public bool? IsVendor { get; set; }
        public string BGTypeKey { get; set; }
        /*基金*/
        public string JJTypeKey { get; set; }
        public string JJTypeName { get; set; }
        /// <summary>
        /// 支票号
        /*往来*/
        public Guid? GUID_WLType { get; set; }

        /// </summary>
        public string CheckNumber { get; set; }
        /// <summary>
        /// 收付款标志



        /// </summary>
        public bool? IsDC { get; set; }//1为出纳收款单 0 付款单 只有往来用
        public Guid BankAccountGuid { get; set; }
        public string BankName { get; set; }

        public Guid? Guid_FunctionClass { get; set; }
        public string FunctionClassName { get; set; }
        public Guid? Guid_EconomyClass { get; set; }
        public string ExpendTypeKey { get; set; }
        /*应付单*/
        public Guid? GUID_Cutomer { get; set; }
        /*收入管理*/
        public Guid? Guid_SRType { get; set; }
        public string SRTypeKey { get; set; }
        public Guid? Guid_ProjectClass { get; set; }
        /*收款管理*/
        public Guid? GUID_SRWLType { get; set; }
        public Guid? GUID_SKType { get; set; }
        /*为主单真正的GUID 公务卡报销g单*/
        public Guid BX_MainRealGUID { get; set; }
        public string ExtraCodeEx { get; set; }
    }
    /// <summary>
    /// 树节点模型
    /// </summary>
    public class TreeNodeModel
    {
        public string id { set; get; }
        public string text { set; get; }
        public string state { set; get; }
        public bool isCheck { set; get; }//checked
        public bool @checked { get; set; }
        public object attributes { set; get; }
        public List<TreeNodeModel> children { get; set; }
    }
}
