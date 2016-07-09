using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using System.Data.Objects;
using System.ComponentModel;
using System.Reflection;
using System.Linq.Expressions;
using BusinessModel;
namespace Business.Casher
{
    //核销保存
    public class HXBill
    {
        //如果单据类型是 04 需要算税





        //'计算税额
        //Private Sub DoTaxCaculte(ByRef oResult As OAODataLogic.ObjectResult)
        //Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
        //     ep.Parser(strFitler, out relust);


    }
    //单据转换类 选单 
    public class BillSelectFun
    {
        /*isDC 为0时 为贷方的出纳付款单 isdc为1是为 借方的出纳收款单 */
        /*金额大》0 为贷方的出纳付款单 金额《0 为 借方的出纳收款单*/
        /*借方 ISDC=0  贷方IsDC=1*/
        /*初始化参数*/
        private int ClassId = 36;
        private int ClassMainId = 35;
        /*系统结果*/
        public string ErrMsg { get; set; }  //错误信息
        /*借方 贷方*/
        public Bill CNBill { get; set; }  //出纳付款单或者出纳收款单
        public List<BillDetail> CNBillDetail { get; set; }    //明细
        public Bill CurBill { get; set; }  //默认为第一个冗余 
        public List<Bill> ListDebit { get; set; }  //为了各个单据转换方便 新建Bill的model 
        public List<BillDetail> ListDebitsDetail { get; set; }  //借方的明细

        public List<Bill> BillDoTaxs { get; set; }
        public List<Bill> ListCredit { get; set; }  //贷方
        public List<BillDetail> ListCreditsDetail { get; set; }  //贷方的明细





        private bool CNFalg { get; set; }  //可以表示为>0 出纳付款单12  <0出纳收款单13
        public Guid DwId { get; set; }  //单位ID 选择的默认以第一个为准





        public Guid DepartmentId { get; set; }  //部门ID 选择的默认以第一个为准





        public Guid PersonId { get; set; }  //人员ID 选择的默认以第一个为准





        public List<string> listGroupRuleFields { get; set; }  //分组规则字段
        public List<BusinessModel.SS_DocType> ListDocType { get; set; }  //存放单据列表          
        /*收款金额 及 银行和财政支付码汇总信息*/
        private List<BusinessModel.SS_BankAccountView> ListBankInfo { get; set; }  //银行信息 0为国库银行 1为基本账户银行





        private List<SumBillInfo> TotalBillInfo { get; set; }     //合计金额列表
        /*借款信息*/
        private List<BorrowTempModel> BorrowModels { get; set; }  //借款余额 需要的model 辅助接受数据 计算
        private BorrowInfo BorrowInfo { get; set; }  //借款信息 ***
        /*核销信息*/
        public Bill HXBill { get; set; } //核销
        public List<HX_Detail> ListHXDetail { get; set; } //核销的明细



        public int CurMainClassId { get; set; } //当前主表的ClassID
        public int CurDetailClassId { get; set; } //当前明细表的ClassID
        /*生成会计凭证*/

        public List<SS_PZTemplateMainView> listPZMain { get; set; }//凭证模板
        public List<Bill> ListAllBill { get; set; }
        public List<BillDetail> ListAllDetailBill { get; set; }
        public CW_PZMainView CwPZMain { get; set; } //会计凭证
        public List<CW_PZDetailView> ListPZDetail { get; set; }  //凭证明细
        public double SumTotal { get; set; } //总金额



        public int? Year { get; set; }//对方帐套年度
        public int CWPeriod { get; set; }//会计区间
        public string AccountKey { get; set; }//对方帐套
        public BusinessModel.CW_PZType CWPTType { get; set; }//凭证类型
        public DateTime DtDocTime { get; set; }
        public bool IsBorrow { get; set; }
        public int U8Num { get; set; }//凭证号

        /*用来区分是不是要在进行 出纳的单据转换*/
        /*提现管理 已经转过 所以不用再进行转换  逻辑问题:提现管理只能 一张一张核销 */
        public bool IsCNTrans { get; set; }
        public bool IsTXCashType { get; set; }//是存现单 还是提现单

        public Dictionary<EBillDocType, List<Guid>> dicTypeList = new Dictionary<EBillDocType, List<Guid>>();
        public BillSelectFun() { }
        public BillSelectFun(Dictionary<EBillDocType, List<Guid>> listId, bool IsDC)
        {
            this.dicTypeList = listId;
            this.IsBorrow = IsDC;
            IsCNTrans = false;
        }
        public bool Main()
        {
            //单据转换等提供数据支持

            try
            {
                this.InitData();
                this.InitFun();
                return true;
            }
            catch (Exception ex)
            {
                this.ErrMsg = ex.Message;
                return false;
            }
        }
        //初始化数据

        private void InitData()
        {
            using (var db = new BusinessEdmxEntities())
            {
                this.ListDebitsDetail = new List<BillDetail>();
                this.ListCreditsDetail = new List<BillDetail>();
                this.ListCredit = new List<Bill>();
                this.ListDebit = new List<Bill>();
                if (!this.SetBills(db)) return;
                this.listGroupRuleFields = this.GetDetailGroupRules(db, this.DwId);
                var bank = this.GetBankAccountGuidByGuoKu(db, true, DwId);
                if (bank == null)
                {
                    this.ErrMsg = "无法找到对应的银行账号，无法转换";
                    return;
                }
                this.ListBankInfo = new List<BusinessModel.SS_BankAccountView>();
                this.ListBankInfo.Add(bank);
                var bank1 = this.GetBankAccountGuidByGuoKu(db, false, DwId);
                if (bank1 == null)
                {
                    this.ErrMsg = "无法找到对应的银行账号，无法转换";
                    return;
                }
                this.ListBankInfo.Add(bank1);
                this.InitBorrow(db, this.PersonId, this.DepartmentId, this.DwId);
                this.listPZMain = GetPZTemplateMain(db);
                this.ListDocType = this.GetDocTypeIds(db);
                this.CWPTType = this.GetCWPTType(db);
            }

        }

        //单据变化  单据转换 生成凭证等 逻辑方法集合
        private void InitFun()
        {
            this.CNBillDetail = new List<BillDetail>();
            Guid guid = Guid.NewGuid();
            //单据转换 生成界面所需的两个列表


            double sumMoney = 0; string memo = "";
            if (this.IsCNTrans)
            { //单据转换
                if (!CashMainChangeCn()) return;
            }
            else
            {
                bool falg2 = this.BillChangeCN(this.ListDebit, this.ListDebitsDetail, ref sumMoney, ref memo, guid, true);
                bool falg1 = this.BillChangeCN(this.ListCredit, this.ListCreditsDetail, ref sumMoney, ref memo, guid, false);
                if (falg2 == false && falg1 == false) { this.ErrMsg = "单据转换失败"; return; }
                this.CreateCn(sumMoney, CurBill, memo, guid);
            }
            //this.SetDetailBillsToBill(this.ListDebit, this.ListDebitsDetail, true);//设置借方
            //this.SetDetailBillsToBill(this.ListCredit, this.ListCreditsDetail, false);//设置贷方
            this.ListAllBill = new List<Bill>();
            this.ListAllBill.AddRange(this.ListCredit);
            this.ListAllBill.AddRange(this.ListDebit);
            //生成借款信息
            this.BorrowInfo = GetBorrowInfo(this.CurBill);
            //加载总金额

            this.TotalBillInfo = this.GetTotalBillInfo(this.CNBillDetail);
            //生成核销对象
            this.HXBill = this.CreateHXBill();
            this.ListHXDetail = CreateHXBillDetail(this.ListDebitsDetail, this.ListCreditsDetail);
            //生成凭证对象
            var flag = this.CreatePZ(this.HXBill, this.ListHXDetail, this.ListDebitsDetail, this.ListCreditsDetail);

        }

        #region 单据转换
        //当为提现单的时候 直接复制 出纳单  
        public bool CashMainChangeCn()
        {
            this.CNBill = ListDebit[0];
            foreach (var item in ListDebitsDetail)
            {
                var dbent = new Infrastructure.BaseConfigEdmxEntities();
                var settleType = dbent.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "02");
                if (settleType != null)
                {
                    item.SettleTypeKey = settleType.SettleTypeKey;
                    item.SettleTypeName = settleType.SettleTypeName;
                    item.GUID_SettleType = settleType.GUID;
                }
                item.BankAccountGuid = item.IsGuoKu ? this.ListBankInfo[0].GUID : this.ListBankInfo[1].GUID;
                item.BankName = item.IsGuoKu ? this.ListBankInfo[0].BankAccountName : this.ListBankInfo[1].BankAccountName;
               
                this.CNBillDetail.Add(item);
            }
            //    this.CNFalg = ListCredit[0].Total_XX > 0;
            //    return true;
            //}
            //else
            //{
            //    this.ErrMsg = "提现单只能选取一张单据进行核销";
            //    return false;
            //}
            return true;
        }
        //单据类型 出纳付款单 和  出纳收款单

        private List<BusinessModel.SS_DocType> GetDocTypeIds(ObjectContext context)
        {
            return context.CreateObjectSet<BusinessModel.SS_DocType>().Where(e => e.DocTypeKey == "13" || e.DocTypeKey == "12").ToList();//("select * from ss_docType where DocTypeKey=12 or DocTypeKey=13 order by DocTypeKey").ToList();
        }
        //为选择的数据源 提供sql
        private List<string> SetSqlByDocTypeAndGuids(EBillDocType docType, List<Guid> listGuid)
        {
            string mainSql = "", detailSql = "";
            switch (docType)
            {
                    /*解决劳务费 差旅费 摘要生产规则*/
                case EBillDocType.报销管理:
                    mainSql = "select 0 as  DocFlag,BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,23 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_bx) from bx_detail where guid_bx_main=bx_mainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from bx_mainview where GUID in ({0})";
                    detailSql = @"SELECT  b.Guid_FunctionClass ,
        b.FunctionClassName ,
        b.Guid_EconomyClass ,
        b.ExpendTypeKey ,
        24 AS ClassId ,
        23 AS ClassMainId ,
        a.PersonName AS PersonName ,
        '' AS DocTypeKey ,
        NULL AS IsDC ,
        SettleTypeName ,
        a.GUID ,
        a.GUID_BX_Main AS GUID_Main ,
        a.GUID_BGCode ,
        a.BGCodeKey ,
        a.BGCodeName ,
        a.ProjectName ,
        a.GUID_Project ,
        CASE WHEN c.DocTypeKey='03' THEN '报'+c.DocMemo WHEN c.DocTypeKey='04' THEN '领'+c.DocMemo else FeeMemo END  AS Memo ,
        a.Total_Bx AS Total_XX ,
        a.GUID_PaymentNumber ,
        a.GUID_SettleType ,
        a.ProjectKey ,
        b.PaymentNumber ,
        b.IsGuoKu ,
        b.BGSourceKey ,
        b.IsProject ,
        b.BGSourceName ,
        b.EconomyClassKey ,
        b.FinanceCode ,
        b.ExtraCode ,
        b.FunctionClassKey ,
        a.DepartmentKey ,
        a.DepartmentName ,
        FeeDate AS DocDate ,
        a.ProjectClassKey ,
        a.Total_Tax ,
        a.SettleTypeKey ,
        a.CustomerKey ,
        a.CustomerName ,
        a.IsCustomer ,
        a.IsVendor ,
        a.BGTypeKey ,
        a.GUID_Cutomer,a.GUID_Department,
        b.ExtraCodeEx
FROM    bx_detailview a
        LEFT JOIN dbo.CN_PaymentNumberView b ON a.GUID_PaymentNumber = b.GUID
        LEFT JOIN dbo.BX_MainView c ON c.GUID=a.GUID_BX_Main where a.GUID_BX_Main in ({0})";
                    this.CurMainClassId = 23;
                    this.CurDetailClassId = 24;
                    break;
                case EBillDocType.收入管理:
                    mainSql = "select 1 as  DocFlag,BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,32 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_sr) from sr_detail where guid_sr_main=sr_mainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from sr_mainview  where GUID in ({0})";
                    detailSql = "select a.Guid_SRType,a.SRTypeKey,a.Guid_ProjectClass,b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,33 as ClassId,32 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.GUID_SR_Main as GUID_Main,a.GUID_BGCode, a.BGCodeKey,a.BGCodeName,a.ProjectName,a.GUID_ProjectKey,ActionMemo as Memo,IsNUll(a.Total_SR,0) as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,ActionDate as DocDate,a.ProjectClassKey,IsNUll(a.Total_Tax,0) as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,Convert(bit,'') as IsCustomer,Convert(bit,'') as IsVendor,a.BGTypeKey,a.GUID_Cutomer,a.GUID_Department,b.ExtraCodeEx from sr_detailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where a.GUID_SR_Main in ({0})";
                    this.CurMainClassId = 32;
                    this.CurDetailClassId = 33;
                    break;
                case EBillDocType.往来管理:
                    mainSql = "select distinct  2 as  DocFlag,Convert(int,BillCount) as BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,30 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_wl) from wl_detail where guid_wl_main=wl_mainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from wl_mainview  where GUID in ({0})";
                    detailSql = "select a.GUID_WLTYpe,a.WLTypeKey,b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,31 as ClassId,30 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,a.IsDC,SettleTypeName,a.GUID,a.guid_wl_main as GUID_Main,a.GUID_BGCode, a.BGCodeKey,a.BGCodeName,a.ProjectName,a.GUID_ProjectKey,ActionMemo as Memo,a.total_wl as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,ActionDate as DocDate,a.ProjectClassKey,Convert(float,'') as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,Convert(bit,'') as IsCustomer,Convert(bit,'') as IsVendor,a.BGTypeKey,a.GUID_Cutomer,a.GUID_Department,b.ExtraCodeEx from wl_detailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where  a.guid_wl_main in ({0})";
                    this.CurMainClassId = 30;
                    this.CurDetailClassId = 31;
                    break;
                case EBillDocType.提现管理:
                    mainSql = "select 3 as  DocFlag,0 as BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,55 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_cash) from cn_cashdetail where guid_cn_cashmain=cn_cashmainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from cn_cashmainview where GUID in ({0})";
                    detailSql = "select b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,56 as ClassId,55 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.guid_cn_cashmain as GUID_Main,null as GUID_BGCode,'' as BGCodeKey,'' as BGCodeName,a.ProjectName,a.GUID_Project,CashMemo as Memo,a.total_cash as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,RZDate as DocDate,a.ProjectClassKey,Convert(float,'') as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,Convert(bit,'') as IsCustomer,Convert(bit,'') as IsVendor,'' as BGTypeKey,a.GUID_Cutomer,a.GUID_Department,b.ExtraCodeEx from cn_cashdetailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where a.guid_cn_cashmain in ({0})";
                    IsCNTrans = true;
                    this.CurMainClassId = 55;
                    this.CurDetailClassId = 56;
                    break;
                case EBillDocType.收款管理:
                    mainSql = "select 4 as  DocFlag, BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,106 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,total_sk as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey,SRWLTypeClassID from SK_mainview where GUID in ({0})";
                    detailSql = "select 106 as ClassId,GUID_SRWLType,106 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.guid as GUID_Main,null as GUID_BGCode, '' as BGCodeKey,'' as BGCodeKey,a.ProjectName,a.GUID_Project, '收'+CustomerName+ DocMemo AS Memo ,a.total_sk as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,a.DocDate,'' as ProjectClassKey,Convert(float,'') as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,Convert(bit,'') as IsCustomer,Convert(bit,'') as IsVendor,'' as BGTypeKey,a.GUID_SKType,a.GUID_Customer as GUID_Cutomer,a.GUID_Department from sk_mainview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where a.guid in ({0})";
                    this.CurMainClassId = 106;
                    this.CurDetailClassId = -1;

                    break;
                case EBillDocType.公务卡汇总管理:
                    mainSql = "select 5 as  DocFlag,0 as BillCount,GUID_DW,GUID_Department,GUID_Person,GUID_YWType,GUID_DocType,23 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select IsNull(sum(total_bx),0) from bx_detail where guid in (select GUID_BXDetail from bx_collectdetail where GUID_BXCOLLECTMain = BX_Collectmainview.guid)) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from BX_Collectmainview where GUID in ({0})";
                    //detailSql = "select 77 as ClassId,76 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.GUID_BX_Main as GUID_Main,a.GUID_BGCode, a.BGCodeKey,a.BGCodeName,a.ProjectName,a.GUID_Project,FeeMemo as Memo,a.Total_Bx as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,FeeDate as DocDate,a.ProjectClassKey,IsNull(a.Total_Tax,0) as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,a.IsCustomer,a.IsVendor,a.BGTypeKey from bx_detailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where a.GUID_BX_Main  in ({0})";
                    detailSql = "select b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,24 as ClassId,23 as ClassMainId,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,c.GUID_BXCOLLectMain as GUID_Main,"
                            + " a.GUID_BGCode, a.BGCodeKey,a.BGCodeName,a.ProjectName,a.GUID_Project,FeeMemo as Memo,a.Total_Bx as Total_XX,a.GUID_PaymentNumber,"
                            + " a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,"
                            + " a.GUID_BX_Main as BX_MainRealGUID,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,a.GUID_Department,FeeDate as DocDate,a.ProjectClassKey,IsNull(a.Total_Tax,0) as Total_Tax,"
                            + " a.SettleTypeKey,a.CustomerKey,a.CustomerName,a.IsCustomer,a.IsVendor,a.BGTypeKey,a.GUID_Cutomer,b.ExtraCodeEx from bx_detailview a "
                            + " left join dbo.CN_PaymentNumberView b "
                            + " on a.GUID_PaymentNumber=b.GUID "
                            + " left join bx_collectdetail c on a.guid=c.GUID_BXDetail"
                            + " LEFT JOIN dbo.BX_Main d ON a.GUID_BX_Main = d.GUID"
                            + " where a.GUID in( select GUID_BXDetail from bx_collectdetail where GUID_bxCollectMain  in ({0})) order  BY d.DocNum ,  a.OrderNum";
                    this.CurMainClassId = 76;
                    this.CurDetailClassId = 77;
                    break;
                case EBillDocType.专用基金管理:
                    mainSql = "select 6 as  DocFlag,BillCount,GUID_DW,GUID_Department,DocTypeKey,GUID_Person,GUID_YWType,GUID_DocType,48 as ClassId , Guid,DocNum,DocTypeName,Convert(nvarchar,DocDate,102) as DocDate,DWName,DepartmentName,PersonName,(select sum(total_jj) from jj_detail where guid_jj_main=jj_mainview.guid) as Total_XX ,DocMemo,YWTypeKey,YWTypeName,DWKey,PersonKey,DepartmentKey from jj_mainview where GUID in ({0})";
                    detailSql = "select b.Guid_FunctionClass,b.FunctionClassName,b.Guid_EconomyClass,b.ExpendTypeKey,49 as ClassId,48 as ClassMainId,JJTypeKey,JJTypeName,a.PersonName as PersonName,'' as DocTypeKey,null as IsDC,SettleTypeName,a.GUID,a.guid_jj_main as GUID_Main,null as GUID_BGCode, '' as BGCodeKey,'' as BGCodeKey,a.ProjectName,a.GUID_Project,FeeMemo as Memo,a.total_jj as Total_XX,a.GUID_PaymentNumber,a.GUID_SettleType,a.ProjectKey,b.PaymentNumber,b.IsGuoKu,b.BGSourceKey,b.IsProject,b.BGSourceName,b.EconomyClassKey,b.FinanceCode,b.ExtraCode,b.FunctionClassKey,a.DepartmentKey,a.DepartmentName,a.GUID_Department,'' as DocDate,a.ProjectClassKey,Convert(float,'') as Total_Tax,a.SettleTypeKey,a.CustomerKey,a.CustomerName,Convert(bit,'') as IsCustomer,Convert(bit,'') as IsVendor,a.BGTypeKey,a.GUID_Cutomer from jj_detailview a left join dbo.CN_PaymentNumberView b on a.GUID_PaymentNumber=b.GUID where  a.guid_jj_main  in ({0})";
                    this.CurMainClassId = 48;
                    this.CurDetailClassId = 49;
                    break;
            }
            return new List<string> { mainSql, detailSql };
        }
        private string ChangeListGuidToStr(List<Guid> listGuid)
        {
            string strFormat = "'{0}',";
            string strParams = "";
            foreach (var item in listGuid)
            {
                strParams += string.Format(strFormat, item);
            }
            return strParams.Substring(0, strParams.Length - 1);
        }
        //'获得业务单据的总余额

        public double GetDocMainRemainMoney(Bill bill, List<BillDetail> wlDetail, int isDc)
        {
            var Details = wlDetail.Where(e => e.GUID_Main == bill.GUID);
            double GetDocMainRemainMoney = 0;
            foreach (BillDetail item in Details)
            {
                GetDocMainRemainMoney += GetDetailRemainMoney(item, isDc == 1 ? true : false);
            }
            return GetDocMainRemainMoney;
        }
        //'获得单据明细的余额(等于业务单据金额-核销明细中的金额)
        //'Detail 业务单据明细
        //'IsDebit 是否为借方
        public double GetDetailRemainMoney(BillDetail Detail, bool isdebit = true)
        {
            var money = Detail.Total_XX;
            var hxDetailList = GetHxDetails(Detail.GUID, Detail.ClassId);
            if (Detail.ClassId != 31)
            {//不是往来


                foreach (var hxDetail in hxDetailList)
                {
                    money = hxDetail.IsDC == true ? money - hxDetail.Total_HX : money + hxDetail.Total_HX;
                }
            }
            else
            {
                if (Detail.IsDC == true)
                {
                    if (isdebit)
                    {
                        foreach (var hxDetail in hxDetailList)
                        {
                            money = hxDetail.IsDC == true ? money - hxDetail.Total_HX : money + hxDetail.Total_HX;
                        }
                    }
                    else
                    {
                        money = 0;
                        foreach (var hxDetail in hxDetailList)
                        {
                            money = hxDetail.IsDC == true ? money + hxDetail.Total_HX : money - hxDetail.Total_HX;
                        }
                    }
                }
                else
                {
                    if (isdebit)
                    {
                        foreach (var hxDetail in hxDetailList)
                        {
                            money = hxDetail.IsDC == true ? money - hxDetail.Total_HX : money + hxDetail.Total_HX;
                        }
                    }
                    else
                    {
                        money = 0;
                        foreach (var hxDetail in hxDetailList)
                        {
                            money = hxDetail.IsDC == true ? money + hxDetail.Total_HX : money - hxDetail.Total_HX;
                        }
                    }
                }

            }
            return money;
        }
        public List<HX_Detail> GetHxDetails(Guid docDetailId, int classId)
        {
            string sqlFormat = "select * from HX_Detail where GUID_Detail='{0}' and ClassID_Detail='{1}'";
            using (var db = new BusinessEdmxEntities())
            {
                return db.HX_Detail.Where(e => e.GUID_Detail == docDetailId && e.ClassID_Detail == classId).ToList();
            }
        }
        //'判断往来的借贷方向
        //'返回值 1 表示在借方，0表示在贷方，2表在借贷双方
        public int GetWLDebitCredit(Bill billWL, List<BillDetail> wlDetail)
        {
            double mRemainMoney = 0;
            double TotalMoney = 0;
            int IsDC = 1;
            IsDC = wlDetail[0].IsDC == true ? 0 : 1;
            TotalMoney = billWL.Total_XX;
            mRemainMoney = GetDocMainRemainMoney(billWL, wlDetail, IsDC);
            if (IsDC == 1)
            {//如果为应收单
                if (TotalMoney == mRemainMoney)
                {
                    IsDC = 0;
                }
                else if (mRemainMoney == 0)
                {
                    IsDC = 1;
                }
                else
                {
                    IsDC = 2;

                }
            }
            else
            {

                if (TotalMoney == mRemainMoney)
                {
                    IsDC = 1;
                }
                else if (mRemainMoney == 0)
                {
                    IsDC = 0;
                }
                else
                {
                    IsDC = 2;

                }

            }
            return IsDC;
        }
        //初始化DetailBill到bill上

        public void SetDetailBillsToBill(List<Bill> listBill, List<BillDetail> listDetail, bool bIsFalg)
        {
            foreach (var item in listBill)
            {
                var listDetailTemp = listDetail.Where(e => e.GUID_Main == item.GUID).ToList();
                foreach (var itemDetail in listDetail)
                {
                    itemDetail.IsDC = bIsFalg;
                }
                item.Details = listDetailTemp;
            }
        }
        //根据不同的单据类型 将单据放入借方和贷方中去

        public bool GetDebitsAndCredits(List<Bill> listBill, List<BillDetail> listDetail, EBillDocType DocType)
        {

            switch (DocType)
            {
                case EBillDocType.公务卡汇总管理:
                case EBillDocType.专用基金管理:
                case EBillDocType.报销管理:
                    this.ListDebit = listBill;
                    this.ListDebitsDetail = listDetail;
                    break;
                case EBillDocType.收入管理:
                    this.ListCredit = listBill;
                    this.ListCreditsDetail = listDetail;
                    break;
                case EBillDocType.往来管理:
                    if (IsBorrow)
                    {
                        this.ListCredit = listBill;
                        this.ListCreditsDetail = listDetail;
                    }
                    else //从选单过来
                    {
                        foreach (var item in listBill)
                        {
                            var listItemDetail = listDetail.Where(e => e.GUID_Main == item.GUID).ToList();
                            var dTag = GetWLDebitCredit(item, listItemDetail);
                            if (dTag == 1)
                            {
                                /*调整代码*/
                                this.ListCredit.Add(item);
                                this.ListCreditsDetail.AddRange(listItemDetail);
                            }
                            else if (dTag == 0)//王娟让调整 
                            {
                                /*调整代码*/
                                this.ListDebit.Add(item);
                                this.ListDebitsDetail.AddRange(listItemDetail);
                            
                            }
                            else
                            {
                                this.ListDebit.Add(item);
                                this.ListDebitsDetail.AddRange(listItemDetail);
                                this.ListCredit.Add(item);
                                this.ListCreditsDetail.AddRange(listItemDetail);
                            }
                        }
                    }

                    break;
                case EBillDocType.提现管理:
                    var cnTrans = new CDocTrans();
                    cnTrans.ListBill = listBill;
                    cnTrans.ListBillDetail = listDetail;
                    this.IsTXCashType = listBill[0].DocTypeKey == "21";//是否提现单 单条核销 多张单据一同核销有问题
                    List<Bill> listMain; List<BillDetail> listMainDetail;
                    string msgError = "";
                    cnTrans.CashTransferCN(DtDocTime, EnumType.EnumCNType.出纳收款单, out listMain, out listMainDetail, out msgError);
                    if (string.IsNullOrEmpty(msgError))
                    {
                        this.ListDebit = listMain;
                        this.ListDebitsDetail = listMainDetail;
                    }
                    cnTrans.CashTransferCN(DtDocTime, EnumType.EnumCNType.出纳付款单, out listMain, out listMainDetail, out msgError);
                    if (string.IsNullOrEmpty(msgError))
                    {
                        this.ListCredit = listMain;
                        this.ListCreditsDetail = listMainDetail;
                    }
                    
                    break;
                case EBillDocType.收款管理:
                    var cnTrans1 = new CDocTrans();
                    cnTrans1.ListBill = listBill;
                    cnTrans1.ListBillDetail = listDetail;
                    listMain = new List<Bill>();
                    listMainDetail = new List<BillDetail>();
                    string msgError1 = "";
                    cnTrans1.SkDocTransferDoc(out listMain, out listMainDetail, out msgError1);

                    if (!string.IsNullOrEmpty(msgError1))
                    {
                        this.ErrMsg = msgError1;
                        return false;
                    }
                    foreach (var item in listMain)
                    {
                        var listItemDetail = listDetail.Where(e => e.GUID_Main == item.GUID).ToList();
                        if (item.ClassId == (int)EnumType.EnumClass.SR_Main)
                        {
                            this.ListCredit.Add(item);
                            foreach (var item1 in listItemDetail)
                            {
                                item1.SRTypeKey = (item1.GUID_SKType+"").Trim().ToString();
                                
                            }
                            this.ListCreditsDetail.AddRange(listItemDetail);
                        }
                        else if (item.ClassId == (int)EnumType.EnumClass.WL_Main)
                        {

                            var dTag = GetWLDebitCredit(item, listItemDetail);
                            if (dTag == 1)
                            {
                                this.ListDebit.Add(item);
                                this.ListDebitsDetail.AddRange(listItemDetail);
                            }
                            else if (dTag == 0)
                            {
                                this.ListCredit.Add(item);
                                this.ListCreditsDetail.AddRange(listItemDetail);
                            }
                            else
                            {
                                this.ListDebit.Add(item);
                                this.ListDebitsDetail.AddRange(listItemDetail);
                                this.ListCredit.Add(item);
                                this.ListCreditsDetail.AddRange(listItemDetail);
                            }
                        }
                        else
                        {
                            this.ListDebit.Add(item);
                            this.ListDebitsDetail.AddRange(listDetail.Where(e => e.GUID_Main == item.GUID));
                        }
                    }
                    break;

            }

            return true;
        }
        //设置选单的数据源
        private bool SetBills(ObjectContext context)
        {
            foreach (var item in dicTypeList.Keys)
            {
                var sqlArr = this.SetSqlByDocTypeAndGuids(item, dicTypeList[item]);
                var guids = ChangeListGuidToStr(dicTypeList[item]);
                string strSqlMain = string.Format(sqlArr[0], guids);
                string strSqlDetail = string.Format(sqlArr[1], guids);
                var bills = context.ExecuteStoreQuery<Bill>(strSqlMain).ToList();
                var billDetails = context.ExecuteStoreQuery<BillDetail>(strSqlDetail).ToList();
                this.BillDoTaxs = bills.Where(e => e.DocTypeKey == "04").ToList();
                if (this.BillDoTaxs.Count > 0)
                {
                    this.SetDetailBillsToBill(bills, billDetails, true);
                    //算税
                    BillDoFax billDoc = new BillDoFax();
                    var oResult = new OAOResult();
                    billDoc.DoTaxCaculte(context, this.BillDoTaxs, this.DtDocTime, oResult);
                }
                var flag = this.GetDebitsAndCredits(bills, billDetails, item);
                if (!flag) return false;
            }


            if (ListDebit.Count > 0)
            {
                this.DwId = this.ListDebit[0].GUID_DW;
                this.DepartmentId = this.ListDebit[0].GUID_Department;
                this.PersonId = this.ListDebit[0].GUID_Person;
                this.CurBill = this.ListDebit[0];
                return true;
            }
            if (ListCredit.Count > 0)
            {
                this.DwId = this.ListCredit[0].GUID_DW;
                this.DepartmentId = this.ListCredit[0].GUID_Department;
                this.PersonId = this.ListCredit[0].GUID_Person;
                this.CurBill = this.ListCredit[0];
                return true;
            }
            this.ErrMsg = "没有找到可转换的单据,请从新查询进行选择！";
            return false; ;
        }
        //获得明细分组的规则

        private List<string> GetDetailGroupRules(ObjectContext context, Guid dwGuid)
        {
            var result1 = context.CreateObjectSet<SS_BaseSetInfView>().Where(e => e.SetTypeKey == "GroupBy" && e.GUID_DW == dwGuid).FirstOrDefault();
            var result = result1.SetValue;
            return result.Split(',').ToList();
        }
        //生成分组规则的key 每个明细有自己对应的key值 相同的key 合并放到一起

        private string MakeKeyByGroupRule(BillDetail billDetail, List<string> listGroupRuleFiled)
        {
            string key = "";
            var props = TypeDescriptor.GetProperties(billDetail);
            foreach (var field in listGroupRuleFiled)
            {
                if (field == "Guid_PaymentNumber")//属性名和数据字段名 一直不管转换小写还是大写都是写死的
                {
                    var payNumber = billDetail.GetValue<BillDetail>("PaymentNumber");//props[""].GetValue(billDetail);
                    key += string.IsNullOrEmpty(payNumber + "") ? "" : (payNumber.ToString() + billDetail.GetValue<BillDetail>("IsGuoKu"));
                }
                else
                {
                    key += billDetail.GetValue<BillDetail>(field); ;
                }
            }
            return key;
        }
        private BillDetail MergeBillDetail(List<BillDetail> DebitDetails, bool IsDebit)
        {
            var cnDetail = new BillDetail(); var firstDebit = DebitDetails[0];
            var memo = ""; double sumHJSumMoney = 0;
            foreach (var item in DebitDetails)
            {
                memo = this.SumStrAB(memo, item.Memo);
                sumHJSumMoney = IsDebit ? sumHJSumMoney + item.Total_XX - item.Total_Tax : sumHJSumMoney - item.Total_XX + item.Total_Tax;
            }

            cnDetail.CopyCommField(firstDebit);
            cnDetail.Memo = memo;
            cnDetail.Total_XX = sumHJSumMoney;

            cnDetail.ClassId = this.ClassId;
            cnDetail.ClassMainId = this.ClassMainId;
            return cnDetail;
        }
        private void CreateCn(double sumHJMoney, Bill curBill, string memo, Guid guid)
        {
            this.CNFalg = sumHJMoney > 0; //出纳付款单 ：出纳收款单
            var cn = new Bill();
            cn.GUID_DW = this.DwId;
            cn.ClassId = this.ClassMainId;
            cn.DWName = curBill.DWName;
            cn.DocNum = "";
            cn.GUID = guid;
            cn.Total_XX = Math.Abs(sumHJMoney);
            cn.DepartmentName = curBill.DepartmentName;
            cn.DocTypeName = this.CNFalg ? "出纳付款单" : "出纳收款单";
            cn.GUID_YWType = (Guid)this.ListDocType[0].GUID_YWType;
            cn.PersonName = curBill.PersonName;
            cn.GUID_Department = this.DepartmentId;
            cn.GUID_Person = this.PersonId;
            cn.BillCount = curBill.BillCount;
            cn.DocTypeKey = sumHJMoney > 0 ? "12" : "13";
            cn.GUID_DocType = this.CNFalg ? this.ListDocType[0].GUID : this.ListDocType[1].GUID;
            cn.DocMemo = memo;
            cn.DocDate = DtDocTime.ToString("yyyy-MM-dd");
            this.CNBill = cn;
            if (CNFalg) { this.ListCredit.Add(cn); this.ListCreditsDetail.AddRange(this.CNBillDetail); }
            else { this.ListDebit.Add(cn); this.ListDebitsDetail.AddRange(this.CNBillDetail); }

        }
        //单据转换
        private bool BillChangeCN(List<Bill> Debits, List<BillDetail> DebitDetails, ref double sumHJMoney, ref string memo, Guid guidCN, bool isDebit)
        {
            if (Debits.Count == 0 || DebitDetails.Count == 0) return false;
            Bill curBill = null;
            foreach (Bill bill in Debits)
            {

                if (this.DepartmentId != bill.GUID_Department) { this.ErrMsg = "单据转换中单据的部门不一致不能进行转换!"; return false; }
                if (this.DwId != bill.GUID_DW) { this.ErrMsg = "单据转换中单据的单位不一样不能进行转换！"; return false; }
                if (this.PersonId != bill.GUID_Person) { this.ErrMsg = "单据转换中单据的人员不一致不能进行转换"; return false; }
                memo = this.SumStrAB(memo, bill.DocMemo);
                sumHJMoney = isDebit ? sumHJMoney + bill.Total_XX : sumHJMoney - bill.Total_XX;
                curBill = bill;
            }

            //分组明细
            var dicBillDetailGroup = new Dictionary<string, List<BillDetail>>();
            foreach (var billDetail in DebitDetails)
            {
                string key = this.MakeKeyByGroupRule(billDetail, this.listGroupRuleFields);
                if (string.IsNullOrEmpty(key)) { this.ErrMsg = "单据转换过程中,明细中缺少分组的字段值，转换失败"; return false; }
                if (dicBillDetailGroup.ContainsKey(key))
                {
                    dicBillDetailGroup[key].Add(billDetail); ;
                }
                else
                {
                    dicBillDetailGroup.Add(key, new List<BillDetail> { billDetail });
                }
            }
            //转换明细
            foreach (var item in dicBillDetailGroup.Keys)
            {
                var billDetails = dicBillDetailGroup[item];
                var billCnDetail = this.MergeBillDetail(billDetails, isDebit);
                var cnBill = billCnDetail;
                cnBill.IsDC = isDebit;
                
                cnBill.BankAccountGuid = cnBill.IsGuoKu ? this.ListBankInfo[0].GUID : this.ListBankInfo[1].GUID;
                cnBill.BankName = cnBill.IsGuoKu ? this.ListBankInfo[0].BankAccountName : this.ListBankInfo[1].BankAccountName;
                cnBill.GUID_Main = guidCN;
                cnBill.DocTypeKey = sumHJMoney > 0 ? "12" : "13";
                cnBill.GUID = Guid.NewGuid();
        
                this.CNBillDetail.Add(cnBill);
            }
            //出纳的单据 放在借方还是贷方
            return true;
        }
        /*基础方法*/
        private string ChangeGuid(List<Guid> listGuid)
        {
            string strTemp = "({0})", strFormat = "'{0}'", strDeal = "";
            foreach (var item in listGuid)
            {
                strDeal += string.Format(strFormat, item);
            }
            return strTemp = string.Format(strTemp, strDeal.TrimEnd(','));
        }
        public string FormatMoney(int ftype, double fmoney)
        {
            string _fmoney = string.Empty;
            if (fmoney == 0F)
            {
                return "";
            }
            fmoney = double.Parse(Convert.ToDouble(fmoney).ToString("0.00"));
            switch (ftype)
            {
                case 0:
                    _fmoney = string.Format("{0:C2}", fmoney);
                    break;
                case 1:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
                default:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
            }
            return _fmoney;
        }
        private string SumStrAB(string memo1, string memo2, char c = '、')
        {
            if (memo1 == null) memo1 = memo1 + "";
            if (memo2 == null) memo2 = memo2 + "";
            string memo = "";
            if (memo1.Contains(memo2)) return memo1;
            memo = string.IsNullOrEmpty(memo2) ? memo2 : c + memo2;
            memo = string.IsNullOrEmpty(memo1) ? memo2 : memo1 + memo;
            return memo;
        }

        #endregion
        #region  借款信息
        public BorrowInfo GetBorrowInfo(Bill curBill)
        {
            var borrow = new BorrowInfo();
            borrow.DepartmentName = curBill.DepartmentName;
            borrow.DWName = curBill.DWName;
            borrow.PersonName = curBill.PersonName;
            borrow.Money = this.BorrowMoney(this.BorrowModels, false);
            borrow.PersonId = curBill.GUID_Person;
            borrow.DwId = curBill.GUID_DW;
            borrow.DepartmentId = curBill.GUID_Department;
            return borrow;
        }
        public double GetBorrowSumMoney() 
        {
            using (var db=new BusinessEdmxEntities())
            {
                InitBorrow(db, Guid.Empty, Guid.Empty, Guid.Empty);
            }
            return BorrowMoney(this.BorrowModels, false);
        }
        public void InitBorrow(ObjectContext context, Guid personId, Guid departmentId, Guid dwId)
        {
            var borrowModels = new List<BorrowTempModel>();
            string sql = "";
            string sqlFormat = "select a.guid as wlguid,a.guid_wl_main as wlmainguid,a.total_wl as total_wl, a.isdc as wldc, b.guid as hxguid, b.isdc as hxdc,b.total_hx as total_hx " +
                               "from wl_detailView a join hx_detail b on b.guid_detail=a.guid " +
                               "where a.guid_wl_main in (select GUID from WL_MainView where DocTypeKey='10' {0}) ";
            if (personId != Guid.Empty)
                sql = string.Format(sqlFormat, string.Format("and GUID_Person='{0}' and GUID_Department='{1}' and GUID_DW='{2}'", personId, departmentId, dwId));
            else {
                sql = string.Format(sqlFormat,"");
            }
            this.BorrowModels = context.ExecuteStoreQuery<BorrowTempModel>(sql).ToList();
        }
        public double BorrowMoney(List<BorrowTempModel> borrowModels, bool isDebit)
        {
            string sql = string.Empty;

            Dictionary<Guid, Guid> wlDic = new Dictionary<Guid, Guid>();
            Dictionary<Guid, double> mDic = new Dictionary<Guid, double>();
            foreach (var model in borrowModels)
            {
                Guid wlGuid = model.wlguid;
                Guid wlMainGuid = model.wlmainguid;
                double hxMoney = model.total_hx;
                var hxdc = model.hxdc == true ? true : false;
                var wldc = model.wldc == true ? true : false;
                var wlMoney = isDebit == wldc ? model.total_wl : 0;
                if (mDic.ContainsKey(wlGuid))
                {
                    wlMoney = GetDetailRemainMoney(isDebit, hxdc, wldc, hxMoney, mDic[wlGuid]);
                    mDic[wlGuid] = wlMoney;
                }
                else
                {
                    wlDic.Add(wlGuid, wlMainGuid);
                    wlMoney = GetDetailRemainMoney(isDebit, hxdc, wldc, hxMoney, wlMoney);
                    mDic.Add(wlGuid, wlMoney);
                }
            }
            double doubleResult = 0;
            foreach (var item in mDic.Keys)
            {
                var doubleResult1 = mDic[item];
                //If wlResultDic.Exists(wlDic(myKey)) = False Then wlResultDic.Add wlDic(myKey), wlmoney
                if (doubleResult1 > 0)
                {
                    doubleResult += doubleResult1;
                }
            }
            //         For Each myKey In mDic.keys
            //    wlmoney = mDic(myKey)
            //    If wlmoney > 0 Then
            //    If wlResultDic.Exists(wlDic(myKey)) = False Then wlResultDic.Add wlDic(myKey), wlmoney
            //    GetWlDebitsRemainMoneyEx = GetWlDebitsRemainMoneyEx + wlmoney
            //    End If
            //Next
            return doubleResult;
        }
        private double GetDetailRemainMoney(bool isDebit, bool hxdc, bool wldc, double hxMoney, double wlMoney)
        {
            if (wldc)
            {
                if (isDebit)
                {
                    wlMoney = isDebit == hxdc ? wlMoney - hxMoney : wlMoney + hxMoney;
                }
                else //如果应收单在贷方，那么余额为核销明细中借方金额-贷方金额
                {
                    wlMoney = hxdc ? wlMoney + hxMoney : wlMoney - hxMoney;
                }
            }
            else
            {
                if (isDebit)
                {
                    wlMoney = wlMoney - hxMoney;
                }
                else //'如果应付单在借方，那么余额为核销明细中贷方金额-借方金额
                {
                    wlMoney = isDebit == hxdc ? wlMoney - hxMoney : wlMoney + hxMoney;
                }
            }
            return wlMoney;
        }
        #endregion
        #region 加载总金额

        public List<SumBillInfo> GetTotalBillInfo(List<BillDetail> CnBillDetail)
        {
            List<SumBillInfo> listBill = new List<SumBillInfo>();
            var dic = new Dictionary<string, int>();
            foreach (var item in CnBillDetail)
            {
                string key = item.BankAccountGuid + item.PaymentNumber;
                if (dic.ContainsKey(key))
                {
                    var sumBillInfo = listBill[dic[key]];
                    sumBillInfo.Total_XX += item.Total_XX;
                }
                else
                {
                    listBill.Add(new SumBillInfo() { BankName = item.BankName, Total_XX = item.Total_XX, PaymentNumber = item.PaymentNumber });
                    dic.Add(key, listBill.Count - 1);
                }
            }
            this.SumTotal = listBill.Sum(e => e.Total_XX);
            return listBill;
        }

        #endregion
        #region 生成核销对象
        public Bill CreateHXBill()
        {
            var bill = new Bill();
            bill.GUID = Guid.NewGuid();
            bill.DocDate = DtDocTime.ToShortDateString();//DateTime.Now.ToShortDateString();服务器时间

            bill.GUID_DW = CurBill.GUID_DW;
            bill.GUID_Department = CurBill.GUID_Department;
            bill.GUID_Person = CurBill.GUID_Person;
            bill.GUID_DocType = new Guid("2F4E699C-A237-4489-86E3-FDA2F5FB7CA2");
            bill.GUID_YWType = new Guid("5195506C-6EEB-4C3F-9FEF-3A4CD6E30537");
            return bill;
        }
        public List<HX_Detail> CreateHXBillDetail(List<BillDetail> ListDebitsDetail, List<BillDetail> ListCreditsDetail)
        {
            var listHXDetail = new List<HX_Detail>();
            foreach (var item in ListDebitsDetail)//借方
            {
                var HXDetail = new HX_Detail();
                HXDetail.GUID_HX_Main = this.HXBill.GUID;
                HXDetail.ClassID_Detail = item.ClassId;
                HXDetail.ClassID_Main = item.ClassMainId;
                HXDetail.GUID_Main = item.GUID_Main;
                HXDetail.GUID_Detail = item.GUID;
                /*业务单据金额 减去已经核销过的*/
                HXDetail.Total_HX = item.Total_XX; //GetHXMoneyAndIsDC2(false, item.Total_XX, item.ClassMainId, item) - GetDetailRemainMoney();
                HXDetail.IsDC = true;
                listHXDetail.Add(HXDetail);
            }
            foreach (var item in ListCreditsDetail)//贷方
            {
                var HXDetail = new HX_Detail();
                HXDetail.GUID_HX_Main = this.HXBill.GUID;
                HXDetail.GUID_Main = item.GUID_Main;
                HXDetail.GUID_Detail = item.GUID;
                HXDetail.ClassID_Detail = item.ClassId;
                HXDetail.ClassID_Main = item.ClassMainId;
                HXDetail.Total_HX = item.Total_XX; //GetHXMoneyAndIsDC2(false, item.Total_XX, item.ClassMainId, item) - GetDetailRemainMoney(); ;
                HXDetail.IsDC = false;
                listHXDetail.Add(HXDetail);
            }
            //往来的dc 不一样





            return listHXDetail;
        }
        public double GetHXMoneyAndIsDC2(bool isDc, double totalDetail, int docType, BillDetail detail)
        {
            double totalHX = totalDetail;
            switch (docType)
            {
                //new Dictionary<int, string> { { 0, "23,24" }, { 1, "32,33" }, { 2, "30,31" }, { 3, "55,56" }, { 4, "106," }, { 5, "76,77" }, { 6, "48,49" } };
                case 23://报销管理
                    totalHX = isDc ? totalDetail : 0 - totalDetail;
                    break;
                case 35://出纳
                    if (detail.IsDC == true && this.CNFalg == false)
                    {
                        totalHX = 0 - totalDetail;
                    }
                    break;
                case 76://往来

                    totalHX = isDc ? totalDetail : 0 - totalDetail;
                    break;
                case 30://基金
                    totalHX = detail.IsDC == true ? 0 - totalDetail : totalDetail;
                    break;
                case 55:
                case 32://出纳
                    totalHX = isDc ? 0 - totalDetail : totalDetail;
                    break;
            }
            return totalHX;
        }
        /*二次核销的时候用 '获得单据明细的余额(等于业务单据金额-核销明细中的金额)'Detail 业务单据明细'IsDebit 是否为借方*/
        public double GetDetailRemainMoney()
        {
            return 0;
        }
        #endregion
        #region 生成会计凭证
        private string Sect1 = "[Sep-]";
        private string Sect2 = "[Sep.]";
        public CW_PZMainView PZMain { get; set; }
        public List<CW_PZDetailView> PZDetailView { get; set; }
        public BusinessModel.CW_PZType GetCWPTType(ObjectContext context)
        {
            return context.CreateObjectSet<BusinessModel.CW_PZType>().FirstOrDefault();
        }
        private bool bSetDC = false;
        public bool CreatePZ(Bill hxBill, List<HX_Detail> hxBillDetail, List<BillDetail> listDebitsDetail, List<BillDetail> listCreditsDetail)
        {
            DateTime dt = DtDocTime;
            //var docDate = this.HXBill.DocDate; var dtTime = DateTime.TryParse(docDate, out dt);
            int year = dt.Year;
            // '获得核销明细 获得业务单据和业务单据明细 
            //获得Hx的规则Key
            var listRuleKey = this.GetPzTempleteRuleKey(hxBillDetail);
            //获得Pz规则模板\
            //listDebitsDetail.AddRange(listCreditsDetail);
            this.ListAllDetailBill = new List<BillDetail>();
            
            ListAllDetailBill.AddRange(listCreditsDetail);
            ListAllDetailBill.AddRange(listDebitsDetail);
         
            //'获得凭证Main
            SS_PZTemplateMainView pzMain = this.GetPzTepleteMainByRule(listRuleKey, this.listPZMain, year, ListAllDetailBill);
            if (pzMain == null)
            {
                this.ErrMsg = "找不到生成会计凭证的模板";
                return false;
            }
            //会计科目
            this.CwPZMain = this.GetPzMain(pzMain, dt);
            if (this.CwPZMain == null)
            {
                this.ErrMsg = "生成会计凭证的失败";
                return false;
            }
            //生成凭证号

            this.GetU8CertificateNum(this.CwPZMain);
            // '获得模板所有凭证明细

            List<SS_PZTemplateDetail> listPZTemplateDetail = this.GetTemplateDetail(pzMain.GUID);
            //根据明细模板 生成会计明细
            this.ListPZDetail = new List<CW_PZDetailView>();

            /*提现单 颠倒了一下*/
            if (this.dicTypeList.Keys.Contains(EBillDocType.提现管理))
            {
                this.bSetDC = false;
                this.ListPZDetail.AddRange(this.GetPzDetailsByDocDetailsEx(listCreditsDetail, listPZTemplateDetail));
                this.bSetDC = true;
                this.ListPZDetail.AddRange(this.GetPzDetailsByDocDetailsEx(listDebitsDetail, listPZTemplateDetail));
            }
            else
            {
                this.bSetDC = true;
                this.ListPZDetail.AddRange(this.GetPzDetailsByDocDetailsEx(listDebitsDetail, listPZTemplateDetail));
                this.bSetDC = false;
                this.ListPZDetail.AddRange(this.GetPzDetailsByDocDetailsEx(listCreditsDetail, listPZTemplateDetail));
            }
            this.ListPZDetail= this.ListPZDetail.OrderBy(e => e.OrderNum).ToList();
            return false;
        }

        //'根据业务明细获得凭证明细
        public List<CW_PZDetailView> GetPzDetailsByDocDetailsEx(List<BillDetail> listAllDocDetail, List<SS_PZTemplateDetail> listPtDetail)
        {
            List<CW_PZDetailView> lsit = new List<CW_PZDetailView>();
            foreach (var item in listAllDocDetail)
            {
                GetPzDetailsByDocDetailsEx(item, listPtDetail, ref lsit);
            }
            return lsit;
        }
        public List<CW_PZDetailView> GetPzDetailsByDocDetailsEx(BillDetail docDetail, List<SS_PZTemplateDetail> listPtDetail, ref List<CW_PZDetailView> list)
        {

            //foreach (var item in listPtDetail)
            //解决顺序进行排序
            for (int i = listPtDetail.Count-1; i >=0; i--)
            {
                var item = listPtDetail[i];
                string RuleString = item.PZCondition;
                int RuleClassId = item.ClassID;
                if (RuleClassId != docDetail.ClassId) continue;
                if (IsSuitRuleEx(docDetail, RuleString))
                {
                    var detial = this.GetPzDetailsByDocDetailEx(docDetail, item);
                    if (detial != null)
                    {
                        list.Add(detial);
                        //break;
                    }
                }
            }
            return list;
        }
        private bool IsTax = false;
        public void GetDCJudege(ref CW_PZDetailView cw, BillDetail docDetail, string ruleString)
        {
            if (this.IsCNTrans) {
                if (this.IsTXCashType)
                {
                    cw.IsDC = !(bool)docDetail.IsDC;
                }
                else {
                    cw.IsDC = (bool)docDetail.IsDC;
                }
                
                //if (docDetail.DocTypeKey == "12")
                //{
                //    if (cw.Total_PZ < 0)
                //    {
                //        cw.Total_PZ = 0 - cw.Total_PZ;
                //        cw.IsDC = !cw.IsDC;
                //    }
                //}
                //else
                //{//出纳付款单


                //    if (cw.Total_PZ < 0)
                //    {
                //        cw.IsDC = !cw.IsDC;
                //        cw.Total_PZ = 0 - cw.Total_PZ;
                //    }
                //}
                return;
            }
          
            if (docDetail.ClassId == 36)
            {//出纳收款单

                if (docDetail.DocTypeKey == "13")
                {
                    if (cw.Total_PZ < 0)
                    {
                        cw.Total_PZ = 0 - cw.Total_PZ;
                        cw.IsDC = !cw.IsDC;
                    }
                }
                else 
                {//出纳付款单

                  
                    if (cw.Total_PZ < 0)
                    {
                        cw.IsDC = !cw.IsDC;
                        cw.Total_PZ = 0 - cw.Total_PZ;
                    } 
                }
                return;
            }
            cw.IsDC = this.bSetDC;
            if (IsTax)//劳务费保险单
            {
                cw.IsDC = false;
                IsTax = false;
                return;
            }
            if (!string.IsNullOrEmpty(ruleString))//差旅报销单用
            {
                var strArr = ruleString.Replace(Sect2, "$").Split('$');
                if (strArr[0] == "0")
                {
                    cw.IsDC = strArr[1] == "1" ? true : false;
                }
            }

        }
        // '根据一条业务明细生成凭证明细

        public CW_PZDetailView GetPzDetailsByDocDetailEx(BillDetail docDetail, SS_PZTemplateDetail ptDetail)
        {
            CW_PZDetailView cw = new CW_PZDetailView();
            string pzRule = ptDetail.PZCondition;
            int classId = ptDetail.ClassID;
            //获得摘要
            string memoRule = ptDetail.PZMemoRule;
            cw.GUID_PZMAIN = this.CwPZMain.GUID;
            cw.PZMemo = GetPzDetailMemo(memoRule, docDetail);
            //'获得科目
            //'获得科目对照规则
            var comparisionMain = this.GetCodeComparisonMain(docDetail, ptDetail);
            Guid accountGuid = Guid.Empty;
            BusinessModel.CW_AccountTitle cwAccountTitle = null;
            if (comparisionMain != null)
            {
                accountGuid = (Guid)GetPzDetailBGCode(docDetail, comparisionMain);
                if (accountGuid != null && accountGuid != Guid.Empty)
                {
                    cwAccountTitle = GetCW_AccountTitle((Guid)accountGuid);
                    if (cwAccountTitle != null)
                    {
                        cw.AccountTitleName = cwAccountTitle.AccountTitleName;
                        cw.GUID_AccountTitle = cwAccountTitle.GUID;
                        cw.AccountTitleKey = cwAccountTitle.AccountTitleKey;
                        cw.GUID_AccountTitle = cwAccountTitle.GUID;
                    }
                }
            }
            //解决排序问题
            cw.OrderNum = ptDetail.OrderNum == null ? 0 : int.Parse(ptDetail.OrderNum+"");

            string ruleString = ptDetail.PZMoneyRule;
            // 金额
            var moneyValue = GetPzDetailMoney(ruleString, docDetail);
            if (moneyValue == 0)
            {
                IsTax = false;
                return null;
            }
            cw.Total_PZ = moneyValue;
            // cw.IsDC = b;
            this.GetDCJudege(ref cw, docDetail, ptDetail.PZIsDCRule);
            //SettleType
            ruleString = ptDetail.PZSettleTypeRule;
            Guid g = Guid.Empty;
            Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
            cw.GUID_SettleType = g;


            //BillNum
            ruleString = ptDetail.PZCheckRule;
            cw.BillNum = (GetPzDetailOther(ruleString, docDetail) + "").ToString();
            //BillDate
            ruleString = ptDetail.PZDateRule;
            DateTime dt;
            DateTime.TryParse((GetPzDetailOther(ruleString, docDetail) + "").ToString(), out dt);
            cw.BillDate = dt;
            //部门
            if (cwAccountTitle != null && cwAccountTitle.IsDepartment == true)
            {
                ruleString = ptDetail.PZDepartmentRule;
                if (ruleString.ToLower().Contains("guid_dydepartment") && docDetail.GUID_Project!=null)
                {
                    try
                    {
                        //特殊性 如果部门是GUID_dyDepartment 直接去ss_prodepartview 这个表中拿出 对应项目所属 部门
                        var departmentDT = Business.Common.DataSource.ExecuteQuery("select  DepartmentName,GUID_Department from ss_prodepartview where GUID_Project='" + docDetail.GUID_Project + "'");
                        if (departmentDT != null && departmentDT.Rows.Count > 0)
                        {
                            cw.GUID_Department = new Guid(departmentDT.Rows[0]["GUID_Department"].ToString());
                            cw.DepartmentName = departmentDT.Rows[0]["DepartmentName"].ToString();
                        }
                        else
                        {
                            ruleString = ruleString.Replace("_dy", "_");
                            Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
                            if (g != Guid.Empty)
                            {
                                cw.GUID_Department = g;
                                cw.DepartmentName = docDetail.DepartmentName;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                  
                }
                else
                {
                    ruleString = ruleString.Replace("_dy", "_");
                    Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
                    if (g != Guid.Empty)
                    {
                        cw.GUID_Department = g;
                        cw.DepartmentName = docDetail.DepartmentName;
                    }
                }
            }
            //项目
            if (cwAccountTitle != null && cwAccountTitle.IsProject == true)
            {
                ruleString = ptDetail.PZProjectRule;
                if (docDetail.ClassId == 31 || docDetail.ClassId == 33)
                {
                    cw.GUID_Project = docDetail.GUID_Project;
                    cw.ProjectName = docDetail.ProjectName;
                    cw.ProjectKey = docDetail.ProjectKey;
                }
                else
                {
                    Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
                    if (g != Guid.Empty)
                    {
                        cw.ProjectName = docDetail.ProjectName;
                        cw.ProjectKey = docDetail.ProjectKey;
                    }
                    cw.GUID_Project = g;
                }

            }
            //客户
            if (cwAccountTitle != null && (cwAccountTitle.IsCustomer == true || cwAccountTitle.IsVCustomer == true))
            {
                ruleString = ptDetail.PZCustomerRule;
                Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
                if (g != Guid.Empty)
                {
                    cw.CustomerName = docDetail.CustomerName;
                }
                cw.GUID_Customer = g;
            }
            //人员
            if (cwAccountTitle != null && cwAccountTitle.IsPerson == true)
            {
                ruleString = ptDetail.PZPersonRule;
                Guid.TryParse((GetPzDetailOther(ruleString, docDetail) + "").Trim().ToString(), out g);
                if (g != Guid.Empty)
                {
                    cw.PersonName = docDetail.PersonName;
                }
                cw.GUID_Person = g;
            }
            // '赋默认值





            ruleString = ptDetail.PZConditionValue;
            if (!string.IsNullOrEmpty(ruleString.Trim()))
            {
                SetPzDetailDefaultValue(ref cw, ruleString);
            }
            return cw;


        }
        //给会计凭证明细设置默认值

        public void SetPzDetailDefaultValue(ref CW_PZDetailView cw, string ruleString)
        {
            var ruleArr = ruleString.Replace(Sect1, "$").Split('$');
            string[] rItems;
            foreach (var item in ruleArr)
            {
                rItems = item.Split('=');
                if (rItems.Length < 2) continue;
                cw.SetValue<CW_PZDetailView>(rItems[0], rItems[1]);
            }
        }
        //根据凭证明细规则获得部门,项目,客户,人员 只在当前业务明细及关联的业务单据上找属性拼:类.字段|类.字段(如果类=0那么字段及为字符串常量)
        public object GetPzDetailOther(string ruleString, BillDetail docDetail)
        {
            object object1 = null;
            if (string.IsNullOrEmpty(ruleString.Trim())) return "";
            var ruleArr = ruleString.Replace(Sect1, "$").Split('$');
            int classId = 0; string headerOrValue = ""; string relust = "";
            foreach (var item in ruleArr)
            {
                var ruleDetailArr = item.Replace(Sect2, "$").Split('$');
                int.TryParse(ruleDetailArr[0], out classId);
                headerOrValue = ruleDetailArr[1];
            
               
                if (classId == 0)
                {
                    return headerOrValue;
                }
                else
                {
                    object1 = docDetail.GetValue<BillDetail>(headerOrValue);
                    if (object1 == null)
                    {
                        var bill = ListAllBill.Where(e => e.GUID == docDetail.GUID_Main).FirstOrDefault();
                        return bill.GetValue<Bill>(headerOrValue);
                    }
                    return object1;

                }

            }
            return relust;
        }
        //金额
        public double GetPzDetailMoney(string moneyRuleString, BillDetail docDetail)
        {
            double dResult = 0;
            if (string.IsNullOrEmpty(moneyRuleString.Trim())) return dResult;
            var ruleArr = moneyRuleString.Replace(Sect1, "$").Split('$');
            int classId = 0; string headerOrValue = "";
            foreach (var item in ruleArr)
            {
                if (string.IsNullOrEmpty(item.Trim())) continue;
                var ruleDetailArr = item.Replace(Sect2, "$").Split('$');
                if (ruleDetailArr.Length < 2) continue;
                int.TryParse(ruleDetailArr[0], out classId);
                headerOrValue = ruleDetailArr[1];
                if (classId == 0)
                {
                    return dResult;
                }
                else
                {
                    if (headerOrValue.ToLower() == "total_tax")
                    {
                        IsTax = true;
                        return docDetail.Total_Tax;
                    }
                    else if (headerOrValue.ToLower().StartsWith("total_"))
                    {
                        return docDetail.Total_XX;
                    }
                    var main = ListAllBill.FirstOrDefault(e => e.GUID == docDetail.GUID_Main);
                    var dTotal = GetRelationDocsValue(main, docDetail, headerOrValue, classId, true);
                    return dTotal == null ? 0 : double.Parse(dTotal.ToString());
                }

            }
            return dResult;
        }
        //科目显示用

        public BusinessModel.CW_AccountTitle GetCW_AccountTitle(Guid guid)
        {
            using (var db = new BusinessEdmxEntities())
            {
                return db.CW_AccountTitle.Where(e => e.GUID == guid).FirstOrDefault();
            }

        }
        public string GetValueByDetailClassId(string HeaderOrValue, BillDetail bill)
        {
            using (var db = new BusinessEdmxEntities())
            {
                //switch (classId)
                //{
                //    //case 24:
                //    //case 36:
                //    //    var detail=db.CN_DetailView.Where(e=>e.
                //    //default:
                //    //    break;
                //}
                return "";
            }
        }
        //判断科目对照规则是否符合
        private bool IsSuitRuleEx(BillDetail bill, string strRule)
        {

            if (string.IsNullOrEmpty(strRule)) return true;
            var ruleArr = strRule.Replace(Sect1, "$").Split('$');
            foreach (var rule in ruleArr)
            {
                if (string.IsNullOrEmpty(rule)) continue;
                var ruleArrDetail = rule.Replace(Sect2, "$").Split('$');
                if (ruleArrDetail.Length < 2) continue;
                int ClassID = 0;
                int.TryParse(ruleArrDetail[0], out ClassID);
                string HeaderOrValue = ruleArrDetail[1];
                if (ClassID == 0)
                {
                    strRule = strRule.Replace((Sect1 + rule + Sect1), HeaderOrValue);
                }
                else
                {
                    string value = "";
                    if (HeaderOrValue == "DocTypeKey")
                    {
                        var main = this.ListAllBill.Where(e => e.GUID == bill.GUID_Main).FirstOrDefault();
                        if (main != null)
                        {
                            value = main.DocTypeKey;
                        }
                    }
                    else
                    {
                        var c = bill.GetValue<BillDetail>(HeaderOrValue);
                        if (c != null)
                        {
                            value = (c + "").Trim();
                        }
                    }
                    if (HeaderOrValue.ToLower() == "bgsourcekey" && value == "")
                    {
                        value = "1";
                    }
                    if ((value + "").ToLower() == "true") value = "1";
                    else if ((value + "").ToLower() == "false") value = "0";
                    else value = "\"" + value + "\"";
                    strRule = strRule.Replace(Sect1 + rule + Sect1, value);
                }
            }
            object relust = null;
            try
            {
                if (strRule.Contains("like") || strRule.Contains("Like"))
                {
                    var contains = strRule.Replace("\"", "'");
                    var sql = "select * from  SS_DocTypeUrl where " + contains;
                    using (var db = new BusinessEdmxEntities())
                    {
                       var ents= db.ExecuteStoreQuery<SS_DocTypeUrl>(sql).ToList();
                       if (ents != null && ents.Count > 0) {
                           return true;
                       }
                       return false;
                    }

                }
                else
                {
                    Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
                    var Success = ep.Parser(ChangeCondition(strRule), out relust);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            if ((bool)relust == false)
            {
                return false;
            }
            return true;
        }
        //'获得科目对照规则
        private List<SS_CodeComparisonDetail> listCodeComparisonDetail { get; set; }
        private List<SS_CodeComparisonMain> listCodeComparisonMain { get; set; }
        private SS_CodeComparisonMain GetCodeComparisonMain(BillDetail billDetail, SS_PZTemplateDetail ptDetail)
        {
            using (var db = new BusinessEdmxEntities())
            {
                Guid g = Guid.Empty;
                Guid.TryParse(ptDetail.PZCodeRule, out g);
                var sscodeMain = db.SS_CodeComparisonMain.Where(e => e.GUID_PZCodeRule == g && e.Condition != "").ToList();
                this.listCodeComparisonMain = this.SortComparisonMain(sscodeMain);
                var ids = sscodeMain.Select(e => e.GUID).ToList();
                this.listCodeComparisonDetail = db.SS_CodeComparisonDetail.Where(e => ids.Contains(e.GUID_CodeComparisonMain)).ToList();
                foreach (var item in sscodeMain)
                {
                    var a = 1;
                    if (item.GUID+"" == "61706837-7E90-4367-9129-121237DC2743") {
                        a = 2;
                    }
                    if (this.IsSuitRuleEx(billDetail, item.Condition))
                    {
                        return item;
                    }
                }
                return null;
            }

        }
        public string GetRelationDocsValue(Bill billMain, BillDetail bill, string colName, int classId, bool isMoney = false)
        {
            var listOao = DocTransferPz.GetDocRelationDocs(BusinessContext, billMain, bill, this.ListHXDetail, HXBill);
            double sumMoney = 0;
            foreach (var item in listOao)
            {
                if (item.ClassId == classId)
                {
                    if (isMoney == true)
                    {
                        var str = item.GetValue(colName.ToLower());
                        double dTemp = 0;
                        double.TryParse(str, out dTemp);
                        sumMoney += dTemp;
                    }
                    else
                    {
                        return item.GetValue(colName.ToLower());
                    }
                }
            }
            return Math.Round(sumMoney, 2).ToString();
        }
        //根据凭证明细规则获得科目
        //'ComparisionMain 科目对照规则
        private Guid? GetPzDetailBGCode(BillDetail billDetail, SS_CodeComparisonMain codeComparisonMain)
        {
            if (codeComparisonMain == null) return Guid.Empty;
            string ruleString = codeComparisonMain.ConditionDetail.Trim();
            if (string.IsNullOrEmpty(ruleString)) return Guid.Empty;
            var rules = ruleString.Replace(Sect2, "$").Split('$');
            if (rules.Length < 2) return Guid.Empty;
            int classId;
            int.TryParse(rules[0], out classId);
            string headerName = rules[1].ToString();
            string ruleValue = "";
            var bill = ListAllBill.FirstOrDefault(e => e.GUID == billDetail.GUID_Main);
            if (billDetail.ClassId == classId)
            {
                if (headerName == "GUID_WLType" && billDetail.GUID_SRWLType != null && billDetail.GUID_SRWLType != Guid.Empty)
                { //收款凭单
                    ruleValue = billDetail.GUID_SRWLType.ToString(); ;
                }
                else
                {

                    ruleValue = (billDetail.GetValue<BillDetail>(headerName) + "").ToString();
                }
            }
            else if (bill.ClassId == classId)
            {
                ruleValue = (bill.GetValue<Bill>(headerName) + "").ToString();
            }
            else
            {
                ruleValue = GetRelationDocsValue(bill, billDetail, headerName, classId);
                if (ruleValue == "0")
                {
                    ruleValue = (billDetail.GetValue<BillDetail>(headerName) + "").ToString();
                }
            }
            //2016 4 19加规则 如果明细中附加码不为空 按照附加码找返回
            var ECode =string.IsNullOrEmpty(billDetail.ExtraCode)?billDetail.ExtraCodeEx:billDetail.ExtraCode;
            if (!string.IsNullOrEmpty(ECode)) {
                var dt = Business.Common.DataSource.ExecuteQuery(string.Format("  SELECT ComparisonType FROM dbo.SS_CodeComparisonDetail WHERE GUID_CodeComparisonMain='{0}' and    GUID_Self IN (SELECT GUID FROM dbo.SS_ProjectFZM WHERE ExtraCode='{1}')",codeComparisonMain.GUID,ECode));
                if (dt != null && dt.Rows.Count > 0) {
                    return new Guid(dt.Rows[0]["ComparisonType"].ToString());
                }
            }
            //var g = new Guid(ruleValue);
            //var ent = ComparisionDetails.FirstOrDefault(e => e.GUID_Self == g);
            var ComparisionDetails = listCodeComparisonDetail.Where(e => e.GUID_CodeComparisonMain == codeComparisonMain.GUID).ToList();
            if (ComparisionDetails != null && ComparisionDetails.Count > 0) {
                var g = Guid.NewGuid();
                Guid.TryParse(ruleValue, out g);
                var a = ComparisionDetails.FirstOrDefault(e => e.GUID_Self == g);
                if (a != null) {
                    return a.ComparisonType;
                }

            }
            foreach (var comparisionDetail in ComparisionDetails)
            {
                if ((comparisionDetail.GUID_Self == null || comparisionDetail.GUID_Self.ToString() == ruleValue) && comparisionDetail.ClassID == codeComparisonMain.ClassID)
                {
                    return comparisionDetail.ComparisonType;
                }
            }
            return Guid.Empty;//错误的逻辑

        }
        //对科目对照规则按条件个数排序
        public List<SS_CodeComparisonMain> SortComparisonMain(List<SS_CodeComparisonMain> listSSCode)
        {
            var listSSCodeComparisonMain = new List<SS_CodeComparisonMain>();
            Dictionary<SS_CodeComparisonMain, int> dic = new Dictionary<SS_CodeComparisonMain, int>();
            dic.Where(e => e.Value == 1);
            foreach (var item in listSSCode)
            {
                var strRuleArr = (item.Condition + "").Replace(Sect1, "$").Split('$');
                dic.Add(item, strRuleArr.Length);
            }
            var dicResult = dic.OrderBy(e => e.Value).ToList();
            foreach (var item in dicResult)
            {
                listSSCodeComparisonMain.Add(item.Key);
            }
            return listSSCodeComparisonMain;
        }
        //临时方法 主要为了 公务卡报销单汇总的时候用的 优化时候统一 处理 一个业务只访问数据库一次

        private BusinessEdmxEntities _icontext = null;
        public BusinessEdmxEntities BusinessContext
        {
            get
            {
                if (_icontext == null) _icontext = new BusinessEdmxEntities();
                return _icontext;
            }
        }
        public string GetValueBxMian(BillDetail docDetail, string headValue)
        {
            var dbBxMain = BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == docDetail.BX_MainRealGUID);
            if (dbBxMain != null)
            {
                return (dbBxMain.GetValue(headValue) + "").ToString();
            }
            return "";
        }
        //'根据凭证明细规则获得摘要 只在当前业务明细及关联的业务单据上找属性拼:类.字段|类.字段(如果类=0那么字段及为字符串常量)
        public string GetPzDetailMemo(string ruleString, BillDetail docDetail)
        {
            try
            {
              
                var billMain = ListAllBill.Where(e => e.GUID == docDetail.GUID_Main).FirstOrDefault();
                var ruleSection = ruleString.Replace(Sect1, "$").Split('$');
                string result = "";
                string[] ruleTemp; int classId = 0; string headOrValue = "";
                if (IsTXCashType)//提现单 2016-5-26 提现小猴提
                {
                    result = billMain.PersonName + "-提现";
                    return result;
                }
                foreach (var rule in ruleSection)
                {
                    if (string.IsNullOrEmpty(rule)) continue;
                    ruleTemp = rule.Replace(Sect2, "$").Split('$');
                    if (ruleTemp.Length < 2) continue;
                    int.TryParse(ruleTemp[0], out classId);
                    headOrValue = ruleTemp[1];
                    if (classId == 0)
                    {
                        result += headOrValue;
                    }
                    else
                    {
                        /*做特殊的处理 为公务卡汇总单 这个方法待优化*/
                        if (classId == 23 && billMain.DocFlag == 5)
                        {
                            result += GetValueBxMian(docDetail, headOrValue);
                            continue;
                        }
                        /***/
                        if (classId == docDetail.ClassId)
                        {
                            //FeeMemo DetailMemo ActionMemo DocMemo ActionMemo
                            if(headOrValue.Contains("Memo")){
                                headOrValue="Memo";
                            }
                            result += docDetail.GetValue<BillDetail>(ChangeMemoColumn(headOrValue)).ToString();
                        }
                        else if (classId == billMain.ClassId)
                        {

                            result += billMain.GetValue<Bill>(headOrValue).ToString();
                        }
                        else
                        {
                            result += docDetail.GetValue<BillDetail>(ChangeMemoColumn(headOrValue)).ToString();
                        }
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string ChangeMemoColumn(string headOrValue)
        {
            if (headOrValue.Contains("Memo"))
            {
                return "Memo";
            }
            return headOrValue;
        }
        //获得会计区间 
        public CW_PeriodView GetCwPeriodGuid(Guid accountGuid)
        {
            Guid gid = Guid.Empty;
            int year = DtDocTime.Year;
            int month = DtDocTime.Month;
            using (var db = new BusinessEdmxEntities())
            {
                var cw = db.CW_PeriodView.Where(e => e.FiscalYear == year && e.CWPeriod == month && e.GUID_AccountDetail == accountGuid).FirstOrDefault();
                if (cw == null)
                {
                    return null;
                }
                this.Year = year;
                this.AccountKey = cw.AccountKey;
                this.CWPeriod = month;
                return cw;
            }
        }
        public CW_PZMainView GetPzMain(SS_PZTemplateMainView pzMain, DateTime docDate)
        {
            // '获得凭证头规则字符串 规则:款及凭证类别|日期(类.字段)|付单据数(类.字段)(求和)
            string pzHeader = pzMain.PZHeader;
            if (string.IsNullOrEmpty(pzHeader)) { this.ErrMsg = "没有凭证头规则，无法生成会计凭证"; return null; }
            var pzRule = pzHeader.Replace(Sect1, "$").Split('$');
            CW_PZMainView cw = new CW_PZMainView();
            cw.GUID_DW = this.DwId;
            //获得凭证类别
            CW_PeriodView cwperiod = GetCwPeriodGuid(pzMain.GUID_AccountDetail);
            if (cwperiod == null) { this.ErrMsg = "没有会计区间，无法生成会计凭证"; return null; }
            cw.GUID = Guid.NewGuid();
            cw.GUID_PZType = this.CWPTType.GUID;
            cw.PZTypeKey = this.CWPTType.PZTypeKey;
            cw.GUID_CWPeriod = cwperiod.GUID;
            cw.GUID_AccountDetail = (Guid)cwperiod.GUID_AccountDetail;
            cw.DocDate = docDate;//获取凭证日期
            cw.BillCount = this.GetBillCount();
            cw.ExteriorYear = cwperiod.ExteriorYear;
            cw.FiscalYear = cwperiod.FiscalYear;
            cw.GUID_DW = DwId;
            cw.ExteriorDataBase = pzMain.AccountKey;
            cw.CWPeriod = cwperiod.CWPeriod;

            return cw;
        }
        public int GetBillCount()
        {
            var count = 0;
            foreach (var item in this.ListDebit)
            {
                var i=0;
                int.TryParse(item.BillCount+"",out i);
                count += i;
            }
        
            return count;

        }
        public List<int> GetPzTempleteRuleKey(List<HX_Detail> hxBillDetail)
        {
            if (hxBillDetail == null) return null;
            return hxBillDetail.Select(e => e.ClassID_Main).Distinct().ToList();
        }
        public SS_PZTemplateMainView GetPzTepleteMainByRule(List<int> rules, List<SS_PZTemplateMainView> listTemplate, int year, List<BillDetail> listCreditsAndDebitsDetail)
        {
            //var dicKey = new Dictionary<string, string>();
            foreach (var template in listTemplate)
            {
                // '判断单位是否符合
                if (this.DwId != template.GUID_DW) continue;
                if (year != template.FiscalYear) continue;
                //检查DataSourceSet中的类ID是否包括单据
                string ruleString = template.DataSourceSet;
                //比较是包含在在模板规则中
                bool falg = true;
                if (!string.IsNullOrEmpty(ruleString))
                {
                    var strArr = ruleString.Replace(Sect1, "&").Split('&');
                    foreach (var item in rules)
                    {
                        if (!strArr.Contains(item.ToString()))
                        {
                            falg = false;
                            continue;
                        }
                    }
                }
                //检查MainCondtion中的条件是否满足
                bool bIsCondtionResult = false;
                string ruleCondition = template.MainCondition;
                bIsCondtionResult = this.TemplateMainRuleSuit(ruleCondition, listCreditsAndDebitsDetail);
                if (bIsCondtionResult && falg) return template;
            }
            this.ErrMsg = "待生成凭证帐套不唯一，无法生成会计凭证";
            return null;
        }
        //判断会计模板条件判断 类.字段=值 and 字段=值 or 字段=值|类.字段=值 and 字段=值 or 字段=值

        private bool TemplateMainRuleSuit(string ruleCondition, List<BillDetail> listBill)
        {
            if (string.IsNullOrEmpty(ruleCondition)) return false;
            //截取条件
            var ruleConditionArr = ruleCondition.Replace(Sect1, "$").Split('$');
            var dicRule = new Dictionary<string, string>();//明细classId 对应的条件





            foreach (var item in ruleConditionArr)
            {
                var mstr = item.Replace(Sect2, "$").Split('$');
                string mstemp = mstr[0].Trim();
                if (!dicRule.Keys.Contains(mstemp) && mstr.Length == 2)
                {
                    dicRule.Add(mstemp, mstr[1].Trim());
                }
            }
            PropertyInfo[] infos = typeof(BillDetail).GetProperties();
            foreach (var item in listBill)
            {
                if (!dicRule.ContainsKey(item.ClassId.ToString())) return false;
                string condition = dicRule[item.ClassId.ToString()];
                foreach (PropertyInfo prop in infos)
                {   //全部是小写


                  condition=  System.Text.RegularExpressions.Regex.Replace(condition, prop.Name, "\"" + prop.GetValue(item) + "\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    //condition = condition.Replace((prop.Name + "").ToString().ToLower().Trim(), "\"" + prop.GetValue(item) + "\"");
                }
                object relust = null;
                Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
                var Success = ep.Parser(ChangeCondition(condition), out relust);
                if (relust == null || (bool)relust == false)
                {
                    return false;
                }
            }
            return true;
        }
        //将条件的condition 转换成 changeCondition
        public string ChangeCondition(string condition)
        {
            condition = condition.Replace(" And ", "&").Replace(" OR ", "|").Replace("<>", "!=");
            condition = condition.Replace("and ", "&").Replace(" Or ", "|");//有错误


            condition = condition.Replace(" and ", "&").Replace(" Or ", "|");
            condition = condition.Replace(" AND ", "&").Replace(" or ", "|");
            condition = condition.Replace(" Like ", "∩").Replace(" like ", "∩");
            condition = condition.Replace("%", "");
           
            return condition;
        }
        public List<SS_PZTemplateMainView> GetPZTemplateMain(ObjectContext context)
        {
            return context.CreateObjectSet<SS_PZTemplateMainView>().ToList();
        }
        public List<SS_PZTemplateDetail> GetTemplateDetail(Guid pzTemplateMainId)
        {
            using (var db = new BusinessEdmxEntities())
            {
                return db.SS_PZTemplateDetail.Where(e => e.GUID_PZTemplateMain == pzTemplateMainId).ToList();
            }
        }
        //获得用友凭证号

        public void GetU8CertificateNum(CW_PZMainView cw)
        {
            try
            {
                var u8Certificate = new U8Certificate(this.BusinessContext);
                //u8Certificate.U8DataBaseName = ;
                this.U8Num = u8Certificate.GetPZNumber(cw);
            }
            catch (Exception ex)
            {
                this.U8Num = 0;
            }
        }
        #endregion
        #region 如果是外聘人员领款单需要算税  bx_mainview DocTypekey=04
        #endregion
        #region 根据是否国库和单位 获得银行账户
        BusinessModel.SS_BankAccountView GetBankAccountGuidByGuoKu(ObjectContext context, bool isGK, Guid dwGuid)
        {
            /*是国库 单位一样 不停用即可  不是国库 单位一样 不停用 并且是基本账户*/
            Expression<Func<BusinessModel.SS_BankAccountView, bool>> predicate = null;
            predicate = n => n.IsGuoKu == isGK && n.GUID_DW == dwGuid;
            if (!isGK)
            {
                predicate = n => n.IsBasic == true;
            }
            return context.CreateObjectSet<BusinessModel.SS_BankAccountView>().Where(predicate).FirstOrDefault();
        }
        #endregion

        public HXShowModel GetShowModel()
        {
            var hxModel = new HXShowModel();
            hxModel.DwPZMain = this.CwPZMain;
            hxModel.DwPZDetails = this.ListPZDetail;
            hxModel.borrow = this.BorrowInfo;
            hxModel.listSum = this.TotalBillInfo;
            hxModel.listCredit = this.ListCredit;
            hxModel.listDebit = this.ListDebit;
            //isdc=0 出纳付款单 贷方 isdc=1 出纳收款单 借方
            hxModel.listKJPZ = this.ListPZDetail == null ? null : this.ListPZDetail.Select(e => new
            {
                e.GUID,
                IsDC = e.IsDC,
                e.PZMemo,
                e.GUID_AccountTitle,
                e.GUID_Customer,
                e.GUID_Department,
                e.GUID_Person,
                e.GUID_Project,
                e.AccountTitleKey,
                e.AccountTitleName,
                Total_JF = e.IsDC ? e.Total_PZ : 0,
                Total_DF = e.IsDC ? 0 : e.Total_PZ,
                e.DepartmentName,
                e.ProjectName,
                e.ProjectKey,
                e.CustomerName,
                e.PersonName
            }).ToList<object>();
            hxModel.DocDate = DtDocTime.ToString("yyyy-MM-dd");
            hxModel.SumTotal = this.SumTotal;
            hxModel.zt = this.AccountKey;
            hxModel.yzt = this.Year;
            hxModel.kjqj = this.CWPeriod;
            hxModel.u8Num = this.U8Num;
            hxModel.error = this.ErrMsg;
            return hxModel;
        }

    }

}
