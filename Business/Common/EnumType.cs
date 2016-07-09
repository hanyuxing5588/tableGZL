using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Common
{
    public class EnumType
    {
        /// <summary>
        /// 审批状态
        /// </summary>
        public enum EnumApproveStatus
        {
            All = 0,//全部
            NotApprove = 1,//未审核
            Approved = 2,//已经审核           
            Approving = 3,//审核中            App=4//包含已经审批和审批中的
        }
        /// <summary>
        /// 支票状态
        /// </summary>
        public enum EnumCheckStatus
        {
            All = 0,//全部
            NotTake = 1,//未领取
            Taked = 2//已经领取
        }
        /// <summary>
        /// 提现状态
        /// </summary>
        public enum EnumWithdrawStatus
        {
            All = 0,//全部
            NotWithdraw = 1,//未提现
            Withdrawed = 2//已提现
        }
        /// <summary>
        /// 付款状态
        /// </summary>
        public enum EnumPayStatus
        {
            All = 0,//全部
            NotPay = 1,//未付款
            Payed = 2//已经付款
        }
        /// <summary>
        /// 凭证状态
        /// </summary>
        public enum EnumCertificateStatus
        {
            All = 0,//全部
            NotCertificate = 1,//未生成凭证
            Certificated=2//已经生成凭证
        }
        /// <summary>
        /// 作废状态
        /// </summary>
        public enum EnumCancelStatus
        {
            All = 0,//全部
            NotCancel = 1,//未作废
            Canceled=2//已经作废
        }
        /// <summary>
        /// 单据状态      
        ///与 ApproveState 字段审批状态 0未提交 1未审批 2审批中 3已经审批 
        ///单据状态：0表示新建-9表示作废-6表示恢复
        /// </summary>
        public enum EnumDocState
        {
            /// <summary>
            /// 未审批            /// </summary>
            NotApprove=0,
            /// <summary>
            /// 已审批 审批完成
            /// </summary>
            Approved=999,
            /// <summary>
            /// 审批中            /// </summary>
            Approving=1,
            /// <summary>
            /// 取消（作废）
            /// </summary>
            CancelState=9,
            /// <summary>
            /// 恢复状态            /// </summary>
            RcoverState=6,
            /// <summary>
            /// 流程结束
            /// </summary>
            ProcessComplete=-1
        }
        /// <summary>
        /// 操作类型
        /// </summary>
        public enum EnumOperateType
        { 
            /// <summary>
            /// 添加
            /// </summary>
            Add=1,
            /// <summary>
            /// 修改
            /// </summary>
            Update=2,
            /// <summary>
            /// 删除
            /// </summary>
            Delete=3,
            /// <summary>
            /// 视图 查看
            /// </summary>
            View=4

        }
        /// <summary>
        /// 预算类型
        /// </summary>
        public enum EnumBGType
        { 
            /// <summary>
            /// 全部
            /// </summary>
            ALL=0,
            /// <summary>
            /// 基本支出
            /// </summary>
            BasicPay,
            /// <summary>
            /// 项目支出
            /// </summary>
            ProjectPay
        }
        /// <summary>
        /// 主表Class值
        /// </summary>
        public enum EnumClass
        {
           Dw = 1,
           Department = 2,
           Person = 3,
           ProjectClass = 4,
           Project = 5,
           BGCode = 6,
           FunctionClass = 7,
           ExpendType = 8,
           Bank = 9,
           BankAccount = 10,
           SS_EconomyClass = 11,
           Province = 12,
           SS_BGSource = 13,
           SettleType = 14,
           SS_YWType = 15,
           SS_DocType = 16,
           SS_Customer = 17,
           SS_Traffic = 18,
           SS_Allowance = 19,
           SS_WLType = 20,
           SS_Scale = 21,
           SS_ServiceMan = 22,
           SS_BXMain = 23,
           SS_BXDetail = 24,
           BX_Travel = 26,
           BX_InvitFee = 27,
           BX_TravelAllowance = 28,
           PaymentNumber = 29,
           WL_Main = 30,
           WL_Detail = 31,
           SR_Main = 32,
           SR_Detail = 33,
           SS_SRType = 34,
           CN_Main = 35,
           CN_Detail = 36,
            SS_BGCodeMemo = 37,
           SSDocNumber = 38,
            SS_DocNumberAutoNumber = 39,
            SS_DocNumberDetail = 40,
           CN_Check = 41,
           CN_CheckDraw = 42,
           CN_CheckPrint = 43,
           CN_CheckNew = 44,
           SS_JJType = 47,
           JJ_Main = 48,
           JJ_Detail = 49,
           SA_Item = 50,
           SA_Plan = 51,
           SA_PlanItem = 52,
           SA_PlanAction = 53,
           SA_PlanActionDetail = 54,
           CN_CashMain = 55,
           CN_CashDetail = 56,
           BG_Type = 57,
           BG_Item = 58,
           BG_Step = 59,
           BG_Setup = 60,
           BG_SetupDetail = 61,
           BG_Main = 62,
           BG_Detail = 63,
           SS_MoneyUnit = 64,
           CW_AccountTitle = 65,
           CW_Period = 66,
           CW_PZType = 67,
           CW_PZMain = 68,
           CW_PZDetail = 69,
           PT_SolutionMain = 70,
           PT_SolutionDetail = 71,
           SS_InvitePerson = 72,
           CN_Journal = 73,
           HX_Main = 74,
           HX_Detail = 75,
           BX_CollectMain = 76,
           BX_CollectDetail = 77,
           WorkFlowState = 78,
           SS_CredentialType = 81,
           CN_CheckDrawDetail = 83,
           SS_YWItem = 84,
           CW_AccountTitleRule = 85,
           CN_CashRequirements = 86,
           SS_UIType = 87,
           SS_UITypeSet = 88,
           BG_SetupBGCode = 89,
           BG_DefaultMain = 90,
           BG_DefaultDetail = 91,
           SS_Designer_Main = 92,
           SS_StateFilter = 93,
           BG_Assign = 94,
           FlowGloableData = 95,
           BG_Preparers = 96,
           BG_Approver = 97,
           SS_Operator = 98,
           SS_PersonType = 99,
           SS_BaseSetInf = 103,
           SS_ComparisonMain = 104,
           SS_ComparisonDetail = 105,
           SS_PZTempleteMain = 107,
           SS_PZTempleteDetail = 108,
           SK_Main = 106,
           SS_CodeComparisonMain = 109,
           SS_CodeComparisonDetail = 110,
           AccountMain = 111,
           AccountDetail = 112,
           SS_SKType = 113,
           Contract_Type = 116,
           Contract_Main = 117,
           Contract_Detail = 118,
           SS_Goods = 119,
           CH_CollarMain = 120,
           CH_CollarDetail = 121,
           CH_PurchaseMain = 122,
           CH_PurchaseDetail = 123,
           CH_StockMain = 124,
           CH_StockDetail = 125,
           SS_GoodsType = 126,
           SS_GoodsUnit = 127,
           SS_Designer_Detail = 128,
            ExecuteGoalType = 129,
            ExecuteGoal = 130,
           SA_PlanArea = 172,
           SA_PlanPersonSet = 173,
           SA_PersonItemSet = 174,
           SA_Setup = 175,
           SA_PlanItemSetUp = 176,
           SA_PAPaymentnumber = 177,
           SS_PostType = 178,
           SS_RankType = 179,
           SS_PersonPost = 180,
           SS_PersonRank = 181,
           SP_Main = 182,
           SS_FYType = 183,
           SS_FYTypeDPComparison = 184,
           BG_HandleMethod = 185,
           BG_ControlMain = 186,
           BG_ControlDetail = 187,
           SS_Storage = 188,
           SS_Denominate = 189,
           SS_GoodStoreDetail = 190,
           SS_JJRuleSetup = 192,
           SS_JJWay = 193,
           SS_GoodStoreMain = 194
        }
        /// <summary>
        /// 出纳类型
        /// </summary>
        public enum EnumCNType
        { 
            出纳付款单=12,
            出纳收款单=13
        }

    }

}















