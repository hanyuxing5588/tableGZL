using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure;
using Business.Common;
using BusinessModel;

namespace Business.CommonModule
{
    public  class BaseSearch
    {
        public string ModelName { get; set; }
        public string ErrMsg { get; set; }
        public string YWKey { get; set; }
        public bool IsShowDetail { get; set; }
        public Guid OperatorId { get; set; }
        public string SqlFormat = "{0} {1} from {2}  where {3} order by {4}";
        public  List<string> YWKeyList=new List<string> { "02", "03", "04", "05", "0802","1101"};//"0801",
        public BaseHistoryCondition BaseCondition { get; set; }
        public BaseSearch(BaseHistoryCondition baseCondition, Guid operatorId) 
        {
            this.BaseCondition = baseCondition as BaseHistoryCondition;
            this.IsShowDetail = false;
        }
        public virtual string GetAuthWhere() 
        {
            return "";
        }
        public virtual string GetTreeWhere(string treeModelName, Guid treevalue)
        {
            string strsql = " 1=1 ";
            using (var iContext = new BaseConfigEdmxEntities())
            {
                var list = new List<Guid>();
                var tableName = treeModelName.Split('_')[1];
                switch (treeModelName.ToLower())
                {
                    case "ss_bgcode":
                        CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                        if (list.Count > 0)
                        {
                            strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        }
                        break;
                    case "ss_department":
                    case "ss_dw":
                    
                        CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                        if (list.Count > 0)
                        { 
                            strsql = "  main.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        }
                        break;
                    case "ss_project":
                        CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                        if (list.Count > 0)
                        { 
                            strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        }
                        break;
                    case "ss_projectclass":
                        var guidList = new List<Guid>();
                        CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, "GUID_Projectclass", ref guidList);
                        foreach (var item in guidList)
                        {
                            CommonFun.RetrieveLeafModelIds(iContext, "ss_project", item, ref list);
                        }
                        if (list.Count > 0)
                        {
                            strsql = " detail.GUID_project in " + CommonFun.ChangeGuid(list);
                        }
                        break;
                    case "ss_person":
                        strsql = string.Format(" main.GUID_Person='{0}'", treevalue);
                        break;
                }
                return strsql;
            }
        }
        public virtual string GetTreeWhere(string treeModelName, string treevalues)
        {
                string strsql = " 1=1 ";
                if (string.IsNullOrEmpty(treevalues)) return strsql;
                var list = Infrastructure.CommonFuntion.ChangeStrArrToGuidList(treevalues.Split(','));
                if (list.Count == 0) return strsql;

                var tableName = treeModelName.Split('_')[1];
                switch (treeModelName.ToLower())
                {
                    case "ss_bgcode":
                        strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        break;

                    case "ss_departmentex"://自己不填写自己部门的问题
                        strsql = "  detail.GUID_department  in " + CommonFun.ChangeGuid(list);
                        break;
                    case "ss_department":
                    case "ss_dw":
                        strsql = "  main.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        break;
                    case "ss_project":
                        if (list.Contains(new Guid("00000000-0000-0000-0000-000000000001")))
                        {
                            strsql = "  detail.GUID_" + tableName + " is null " ;
                        }
                        else
                        {
                            strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                        }
                        break;
                    case "ss_projectclass":
                            strsql = " detail.GUID_project in " + CommonFun.ChangeGuid(list);
                        break;
                    case "ss_person":
                        strsql = string.Format(" detail.GUID_Person in"+ CommonFun.ChangeGuid(list));
                        break;
                    case "ss_functionclass":
                        strsql = " detail.GUID_project in (select guid from ss_project in " + CommonFun.ChangeGuid(list) + ")";
                        break;
                    case "ss_funclass":/* 项目分摊功能分类所用*/
                        strsql = " detail.GUID_PaymentNumber in (select guid from CN_PaymentNumber where GUID_FunctionClass in " + CommonFun.ChangeGuid(list) + ")";
                        //GUID_PaymentNumber IN (SELECT GUID FROM dbo.CN_PaymentNumber WHERE GUID_FunctionClass IN ())
                        break;
                    case "ss_doctype":
                        strsql = " guid_docTYpe in " + CommonFun.ChangeGuid(list);
                        break;
                }
                return strsql;
        }
        public virtual string GetFieldGuidWithTableName(string ModelName)
        {
            return "GUID_" + ModelName;
        }

        public virtual string GetBaseTopSql()
        {
            //var km = (this.YWKey == "04" || this.YWKey == "0802")||!DJLBCondition.IsShowDetail ? "" : "BGCodeName,BGCodeKey,";//基金
            //var billSumCount = this.YWKey == "0802" ? "" : "IsNUll(convert (nvarchar,BillCount),'') as BillCount,";//现金提取
            var result = "select distinct main.GUID,DocNum,convert(nvarchar,DocDate,23) as DocDate,ModelName,main.DepartmentName,main.Maker,main.PersonName,Total_XX as Total," +
                         (IsShowDetail? "BGCodeName,BGCodeKey,":"")+
                         "DocMemo,YWTypeName,DocTypeKey,DocTypeName,convert(nvarchar,main.MakeDate,23) as MakeDate,DocTypeUrl,IsNUll(convert (nvarchar,BillCount),'') as BillCount,";// +km + billSumCount;

            return result;
        }
        public virtual string GetTailSql()
        {
            //string djlbShowDetail = DJLBCondition.IsShowDetail ? " left join {1} detail on main.GUID=detail.{2}  " : "";
            var sql = string.Format(" {0} main " +
               " left join {1} detail on main.GUID=detail.{2}" +
                //" left join SS_Projectview Project on detail.{4}=project.guid "+ 
             " left join (select {2},sum({3}) as Total_XX from {1} a Group By {2}) DetailSum on DetailSum.{2}=main.GUID {4}",
             this.TableMain, this.TableDetail, GetFieldGuidWithTableName(ModelName), GetTotalFieldByYWForTail(this.YWKey),this.PaymentNumber);//, this.YWKey == "03" ? "guid_projectkey" : "guid_project");//收入
            return sql;
        }
        public virtual string GetSqlWhere()
        {
            return " 1=1 ";
        }
        public virtual string AppendBaseWhere(BaseHistoryCondition historBaseCondition) 
        {
            StringBuilder sb = new StringBuilder();
            if (historBaseCondition.DocType != Guid.Empty)
            {
                sb.AppendFormat(" and GUID_DocType='{0}'", historBaseCondition.DocType);
            }
            if (!string.IsNullOrEmpty(historBaseCondition.ApproveStatus) && historBaseCondition.ApproveStatus != "0")
            {
                sb.Append(" and ");
                sb.Append(GetWhereWithApproveStatus(historBaseCondition.ApproveStatus));
            }

            if (!string.IsNullOrEmpty(historBaseCondition.CheckStatus)&&historBaseCondition.CheckStatus != "0")
            {
                sb.Append(" and ");
                sb.Append(GetWhereWithCheckStatus(historBaseCondition.CheckStatus));
            }

            if (!string.IsNullOrEmpty(historBaseCondition.WithdrawStatus) && historBaseCondition.WithdrawStatus != "0")
            {
                sb.Append(" and ");
                sb.Append(GetWhereWithDrawStatus(historBaseCondition.WithdrawStatus));
            }

            if (!string.IsNullOrEmpty(historBaseCondition.PayStatus) && historBaseCondition.PayStatus != "0")
            {
                sb.Append(" and ");
                sb.Append(GetWhereWithPayStatus(historBaseCondition.PayStatus));
            }

            if (!string.IsNullOrEmpty(historBaseCondition.CertificateStatus) && historBaseCondition.CertificateStatus != "0")
            {
                sb.Append(" and ");
                sb.Append(GetWhereWithCertificateStatus(historBaseCondition.CertificateStatus));
            }

            if (!string.IsNullOrEmpty(historBaseCondition.CancelStatus) && historBaseCondition.CancelStatus != "0")
            {

                sb.Append(" and ");
                sb.Append(GetWhereWithCancelStatus(historBaseCondition.CancelStatus));
            }
            if (!string.IsNullOrEmpty(historBaseCondition.DocNum))
            {
                sb.AppendFormat(" and DocNum like '%{0}%' ", historBaseCondition.DocNum);
            }
            return sb.ToString();
        }
        public virtual string GetSql()
        {

            var sqlTop = GetBaseTopSql();
            var sqlMid = GetSPStateCondition() + GetZPStateCondition() + GetTXStateCondition() + GetFKStateCondition() + GetPZStateCondition() + GetZFStateCondition();
            var sqlTail = GetTailSql();
            var sqlWhere = GetSqlWhere();
            var sqlOrderBy = GetOrderFiled();
            return string.Format(SqlFormat, sqlTop, sqlMid, sqlTail, sqlWhere, sqlOrderBy);
        }
        private string GetTotalFieldByYWForTail(string ywTypeKey)
        {
            var jeFiled = "";
            switch (ywTypeKey)
            {
                case "02":
                    jeFiled = "Total_bx";
                    break;
                case "03":
                    jeFiled = "Total_SR";
                    break;
                case "04":
                    jeFiled = "Total_JJ";
                    break;
                case "0801":
                    jeFiled = "Total_CN";
                    break;
                case "0802":
                    jeFiled = "Total_cash";
                    break;
                case "1101":
                    jeFiled = "Total_SK";
                    break;
                case "05":
                case "0501":
                case "0502":
                    jeFiled = "Total_WL";
                    break;
                default:
                    jeFiled = "Total_bx";
                    break;
            }
            return jeFiled;
        }
        private string GetOrderFiled()
        {
            return "DocTypeKey,DocNum Desc ";
        }
        #region 各个状态的 条件值
        /// 审批状态 0 标示全部 1未审核 2已审核  3审核中
        private string GetApproveStatusArr(string approveStatus) 
        {
            StringBuilder sb = new StringBuilder(" 1<>1 ");
            var statusArr = approveStatus.Split(',');
            foreach (var item in statusArr)
            {
                if (!string.IsNullOrEmpty(item)) {
                   var tempWhere=  GetWhereWithApproveStatus(item);
                   if (!string.IsNullOrEmpty(tempWhere))
                   {
                       sb.Append(" or ");
                       sb.Append(tempWhere);
                   }
                }
            }
            return sb.ToString();
        }
        public virtual string GetWhereWithApproveStatus(string approveStatus)
        {
            if (approveStatus.Contains(',')) {
                return GetApproveStatusArr(approveStatus);
            }
            else if (approveStatus == "0") {
                return " ";
            }
            else if (approveStatus == "1")
            {
                return " (main.DocState=0 or main.DocState='') ";
            }
            else if (approveStatus == "2")
            {
                return "(main.DocState=-1 or main.DocState='999') ";
            }
            else {
                return " (main.DocState<>-1 and main.DocState<>'999' and main.DocState<>0 and main.DocState<>'') ";
            }
        }
        // 支票状态 O全部 1未领取 2已领取

        public virtual string GetWhereWithCheckStatus(string checkStatus)
        {
            if (checkStatus == "0")
            {
                return "";
            }
            else if (checkStatus == "1")
            {
                return " main.guid not in (select GUID_Doc from cn_checkdrawmain) ";
            }
            else
            {
                return " main.guid  in (select GUID_Doc from cn_checkdrawmain) ";
            }
        }
        /// 提现状态 0全部 1未提现 2已提现

        public virtual string GetWhereWithDrawStatus(string drawStatus)
        {
            if (drawStatus == "0")
            {
                return "";
            }
            else if (drawStatus == "1")
            {
                return " main.guid not in (select GUID_DocMain from CN_CashRequirements) ";
            }
            else
            {
                return " main.guid  in (select GUID_DocMain from CN_CashRequirements) ";
            }
        }
        // 付款状态 0全部 1未付款 2已付款

        public virtual string GetWhereWithPayStatus(string payStatus)
        {
            if (payStatus == "0")
            {
                return "";
            }
            else if (payStatus == "1")
            {
                return " main.guid not in (select GUID_Main from hx_detail) ";
            }
            else
            {
                return " main.guid  in (select GUID_Main from hx_detail) ";
            }
        }
        // 凭证状态 0全部 1未生成凭证 2已经生成凭证
        public virtual string GetWhereWithCertificateStatus(string certificateStatus)
        {
            if (certificateStatus == "0")
            {
                return "";
            }
            else if (certificateStatus == "1")
            {
                if (YWKey == "76") {
                    return " main.guid not in (select guid from BxCollectmainINPzmainView) ";
                }
                return " main.guid not in (select guid from bxmaininpzmainview) ";
            }
            else
            {
                if (YWKey == "76")
                {
                    return " main.guid not in (select guid from BxCollectmainINPzmainView) ";
                }
                return " main.guid in (select guid from bxmaininpzmainview) ";
            }
        }
        //作废状态 0全部 1表示未作废 2已作废 
        public virtual string GetWhereWithCancelStatus(string cancelStatus)
        {
            if (cancelStatus == "0")
            {
                return "";
            }
            else if (cancelStatus == "1")
            {
                return " main.DocState<>9 ";
            }
            else
            {
                return " main.DocState=9 ";
            }
        }
        public virtual string GetWhereWithDocDate(string startDate, string endDate) 
        {
            string sql = "";
            var dt=DateTime.Now;
            DateTime.TryParse(endDate, out dt);
            sql = string.Format(" and DocDate between '{0}' and '{1}'", startDate, dt.AddDays(1).ToShortDateString());
            return sql;
        }
        #endregion
        #region 单据各种状态 显示值
        public virtual string GetSPStateCondition(string fieldName = "ApproveStatus") 
        {
            return string.Format("{0}=Case main.DocState when -1 then '已审核' when 999 then '已审核' when 1 then '审核中'   Else '未审核' end, ", fieldName);
        }
        public virtual string GetZPStateCondition(string fieldName = "CheckStatus") 
        {
            return string.Format("{0}=case when main.guid in (select GUID_Doc from cn_checkdrawmain) then '已领取' else '未领取' end, ", fieldName);
        }
        public virtual string GetTXStateCondition(string fieldName = "WithdrawStatus")
        {
            return string.Format("{0}=case when main.guid in (select GUID_DocMain from CN_CashRequirements) then '已提现' else '未提现' end, ", fieldName);
        }
        public virtual string GetFKStateCondition(string fieldName = "PayStatus")
        {
            return string.Format("{0}=case when main.guid in (select GUID_Main from hx_detail) then '已付款'  else '未付款'end, ", fieldName);
        }
        public virtual string GetPZStateCondition(string fieldName = "CertificateStatus")
        {
            //  when main.guid in (SELECT GUID_Pconversion_Main FROM dbo.SS_DocTransformation WHERE ClassID_AConversion_Main=32 ) then '已生成凭证' 
            if (YWKey == "1101") {
                return string.Format(@"{0}=case when main.guid in (select guid from SKmainINPzmainView) then '已生成凭证' 
            
            else '未生成凭证'end, ", fieldName);
            }
            if (YWKey == "76")
            {
                return string.Format(@"{0}=case when main.guid in (select guid from BxCollectmainINPzmainView) then '已生成凭证' 
            
            else '未生成凭证'end, ", fieldName);
            }
            else
            {
                return string.Format(@"{0}=case when main.guid in (select guid from bxmaininpzmainview) then '已生成凭证' 
            
            else '未生成凭证'end, ", fieldName);
            }

        }
        public virtual string GetZFStateCondition(string fieldName = "CancelStatus")
        {

            return string.Format("{0}=Case main.DocState when 9 then '已作废' Else '未作废' end ", fieldName);

        }
        #endregion

        public string TableMain { get; set; }
        public string TableDetail { get; set; }
        public string PaymentNumber { get; set; }
        public string WherePaymentNumber { get; set; }

        public virtual void SetTableName(string YWTypeKey)
        {
            PaymentNumber = string.Empty; WherePaymentNumber = string.Empty;
            switch (YWTypeKey)
            {
                case "02":
                    TableMain = "(select 'BX_Main' as ModelName,* from BX_MainView) ";
                    TableDetail = "BX_DetailView";
                    ModelName = "BX_Main";
                    PaymentNumber = " left join (select guid,bgsourcekey from cn_paymentnumberview) pay on detail.guid_paymentnumber=pay.guid ";
                    WherePaymentNumber = "pay.bgsourcekey";
                    break;
                case "03":
                    TableMain = "(select 'SR_Main' as ModelName,* from SR_MainView) ";
                    TableDetail = "( select guid_ProjectKey as guid_project,* from  SR_DetailView) ";
                    ModelName = "SR_Main";
                    break;
                case "04":
                    TableMain ="(select 'JJ_Main' as ModelName,* from JJ_MainView) ";// "JJ_MainView";
                    TableDetail = " (select '' as BGCodeName,'' as BGCodeKey,'' as GUID_BGCode,*  from  JJ_DetailView)";
                    ModelName = "JJ_Main";
                    break;
                case "0801":
                    TableMain = "(select 'CN_Main' as ModelName,* from cn_mainview)";
                    TableDetail = " (select '' as BGCodeName,'' as BGCodeKey,'' as GUID_BGCode,*  from  cn_DetailView)";
                    ModelName = "CN_Main";
                    break;
                case "0802":
                    TableMain = "(select '' as BillCount,'CN_CashMain' as ModelName, * from CN_CashMainView) ";
                    TableDetail = " (select '' as BGCodeName,'' as BGCodeKey,'' as GUID_BGCode,*  from  CN_CashDetailView)";
                    ModelName = "CN_CashMain";
                    break;
                case "05":
                case "0501":
                case "0502":
                    TableMain = "(select 'WL_Main' as ModelName, * from WL_MainView) ";// "WL_MainView"; where DocTypeUrl='yfd'
                    TableDetail = "(select guid_ProjectKey as guid_project,* from  WL_DetailView)";
                    ModelName = "WL_Main";
                    break;
                case "1101":
                    TableMain = "(select 'SK_main' as ModelName,* from SK_mainView)";
                    TableDetail = " (select '' as BGCodeName,'' as BGCodeKey,'' as GUID_BGCode,guid as GUID_SK_main, GUID_Project,Total_SK  from  SK_mainView)";
                    ModelName = "SK_main";
                    break;
                case "76":
                    TableMain = @"(SELECT   'BXCOLLECTMain' AS ModelName ,
        0 AS BillCount ,
       *
FROM    BX_Collectmainview )";
                    TableDetail = "( SELECT  a1.*,c1.GUID_BXCOLLECTMain FROM bx_detailview a1 left join bx_collectdetail c1 on a1.guid=c1.GUID_BXDetail) ";
                    ModelName = "BXCOLLECTMain";
                    break;
                default:
                    TableMain = "";
                    TableDetail = "";
                    break;
            }
            this.YWKey = YWTypeKey;
        }



        public virtual List<DocListSearchResult> GetResultFilterDetail(List<DocListSearchResult> docList)
        {
            var listDocNew = new List<DocListSearchResult>();
            Dictionary<Guid, int> dicGuid = new Dictionary<Guid, int>();
            foreach (var item in docList)
            {
                if (dicGuid.ContainsKey(item.GUID))
                {
                    listDocNew.Add(item);
                }
                else
                {
                    dicGuid.Add(item.GUID, 1);
                }

            }
            return listDocNew;
        }
        public virtual List<DocListSearchResult> GetResult()
        {
            List<DocListSearchResult> listSr = new List<DocListSearchResult>();
            try
            {
                if (BaseCondition != null)
                {
                    SetTableName(BaseCondition.YWType);
                }
                using (var context = new BusinessModel.BusinessEdmxEntities())
                {
                    if (string.IsNullOrEmpty(this.TableMain))
                    {
                        var list = this.YWKeyList;
                        foreach (var ywkey in list)
                        {
                            SetTableName(ywkey);
                            listSr.AddRange(ExeSql(context));
                        }
                    }
                    else
                    {
                        listSr.AddRange(ExeSql(context));
                    }
                }
                return listSr;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message.ToString();
                return null;

            }

        }
        private List<DocListSearchResult> ExeSql(BusinessEdmxEntities context)
        {
            try
            {
                string sql = GetSql();
                return context.ExecuteStoreQuery<DocListSearchResult>(sql).ToList();
            }
            catch (Exception ex)
            {
                //2016.3.28 由于提现单 在查询的时候报错 所以改为异常捕获
                return new List<DocListSearchResult>();
            }
               

        }
    }
    /// <summary>
    /// 单据列表Model
    /// </summary>
    public class DocListSearchResult
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Guid GUID { get; set; }
        /// <summary>
        /// 单据类型编码
        /// </summary>
        public string DocTypeKey { get; set; }

        /// <summary>
        /// 制单日期
        /// </summary>
        public string MakeDate { get; set; }
        /// <summary>
        /// 人员名称
        /// </summary>
        public string PersonName { get; set; }
      
        /// <summary>
        /// 附件数
        /// </summary>
        public string BillCount { get; set; }
        /// <summary>
        /// 科目名称
        /// </summary>
        public string BGCodeName { get; set; }
        /// <summary>
        /// 科目键

        /// </summary>
        public string BGCodeKey { get; set; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocTypeUrl { get; set; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }



        
        /// <summary>
        /// 业务名称
        /// </summary>
        public string YWTypeName { get; set; }
       
        /// 单据类型名称
        /// </summary>
        public string DocTypeName { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string DocNum { get; set; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public string DocDate { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 制单人

        /// </summary>
        public string Maker { get; set; }

          /// <summary>
        /// 金额
        /// </summary>
        public double? Total { get; set; }
        /// <summary>
        /// 摘要
        /// </summary>    
        public string DocMemo { get; set; }
       
        /// <summary>
        /// 审批状态
        /// </summary>
        public string ApproveStatus { get; set; }
        /// <summary>
        /// 领取状态
        /// </summary>
        public string CheckStatus { get; set; }
        /// <summary>
        /// 提现状态
        /// </summary>
        public string WithdrawStatus { get; set; }
        /// <summary>
        /// 付款状态
        /// </summary>
        public string PayStatus { get; set; }
        /// <summary>
        /// 凭证状态
        /// </summary>
        public string CertificateStatus { get; set; }
     
      
        /// <summary>
        /// 作废（取消）状态
        /// </summary>
        public string CancelStatus { get; set; }
        /// <summary>
        ///已经提交未审批
        /// </summary>
        public string SubmitNotApprove { get; set; }

    }
}
