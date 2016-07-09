using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;
namespace Business.CommonModule
{    
    public class 历史记录 : BaseDocument
    {
        public 历史记录() : base() { }
        public 历史记录(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        public string ErrorCode = string.Empty;
        #region 历史记录
        /// <summary>
        /// 历史记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        public override List<object> History(SearchCondition conditions)
        {
            if (conditions != null)
            {
                var docType = conditions.ModelUrl;
                switch (docType)
                {
                                
                    case"skpd"://收款凭单
                        return SK_History(conditions);
                    case "jkdtz"://借款单填制     
                    case "yfd"://应付单

                    case "ysdtz"://应收单填制

                    case "yfdtz"://应付单填制

                        return WL_History(conditions);
                    case"cnfkd"://出纳付款单

                    case"cnskd"://出纳收款单

                        return CN_History(conditions);
                    case "srpd"://收入凭单
                    case "czsr"://财政收入
                        return SR_History(conditions);
                    case "xjcc"://现金存储
                    case"xjtq"://现金提取
                        return Cash_History(conditions);
                    case "gwkhzbxd"://公务卡汇总报销单

                        return BX_GWDHZ_History(conditions);
                    case "zyjjlzd"://专用基金列支单

                        return JJ_History(conditions);
                    case "kjpz"://会计凭证
                        return PZ_History(conditions);
                    case"gzd":
                        return GZD_History(conditions);
                    default://报销单

                        return BX_History(conditions);
                }
            }
            return null;
        }
        /// <summary>
        /// 报销历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> BX_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BX_MainView> main = this.BusinessContext.BX_MainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main=main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单

            }

            IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);
                detail = detail.Where(e => e.GUID_Maker == this.OperatorId);//
            }
            //结算方式
            
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {
                       
                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                // 时间条件
                if (!historyconditions.StartDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate >= historyconditions.StartDate);
                }
                if (!historyconditions.EndDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate<= historyconditions.EndDate);
                }

                #region 审批状态条件


                
                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {                       
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件


                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState) && e.ApproveState == Constant.NewNotApproveState);                      
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState== Constant.NewApprovedState);
                      //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧        
                        var docState = ((int)EnumType.EnumDocState.Approving).ToString();
                        main = main.Where(e => e.DocState==docState||(!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                #region 支票状态

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 提现状态

                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态

                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 付款状态

                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态

                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态

                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"
                        
                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e =>guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 作废状态

                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e=>e.GUID_Project!=null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e =>e.GUID_Project!=null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_BX_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                PersonName = e.PersonName == null ? "" : e.PersonName,
                BillCount = e.BillCount == null ? 0 : e.BillCount,
                Total=e.Total_BX,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,               
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
   
        /// <summary>
        /// 往来单历史记录
        /// </summary>                                                           
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> WL_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<WL_MainView> main = this.BusinessContext.WL_MainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }

            IQueryable<WL_DetailView> detail = this.BusinessContext.WL_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);
                detail = detail.Where(e => e.GUID_Maker == this.OperatorId);
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {
                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                // 时间条件
                if (!historyconditions.StartDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate >= historyconditions.StartDate);
                }
                if (!historyconditions.EndDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate <= historyconditions.EndDate);
                }

                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧    
                        var docState=((int)EnumType.EnumDocState.Approving).ToString();
                        main = main.Where(e => e.DocState == docState || (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e =>e.GUID_ProjectKey!=null && projectGUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e =>e.GUID_ProjectKey!=null && projectUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_BGCode!=null && bgcodeGUID.Contains((Guid)e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_WL_Main into g
                          select new { GUID_WL_Main = g.Key, Total_WL = g.Sum(a => a.Total_WL) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_WL_Main equals m.GUID //into temp
                     where d.GUID_WL_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_WL, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate,m.YWTypeKey });

            //
            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                PersonName = e.PersonName == null ? "" : e.PersonName,
                BillCount = e.BillCount == null ? "0" : e.BillCount,
                Total= e.Total_WL,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        /// <summary>
        /// 出纳
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> CN_History(SearchCondition conditions)
        {//hanyx
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<CN_MainView> main = this.BusinessContext.CN_MainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }
            IQueryable<CN_DetailView> detail = this.BusinessContext.CN_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);
                detail = detail.Where(e => e.GUID_CN_Main != null && main.Select(m => m.GUID).Contains((Guid)e.GUID_CN_Main));
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {                       
                        main = main.Where(e =>e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                        main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态



                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }

                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废



                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != "9");

                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废



                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState.Trim() == "9");

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains((Guid)e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            //SS_BGCode bgcodeModel = new SS_BGCode();
                            //bgcodeModel.GUID = historyconditions.treeValue;
                            //bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            //var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            //detail = detail.Where(e => e.GUID_BGCode != null && bgcodeGUID.Contains((Guid)e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by new { a.GUID_CN_Main,a.SettleTypeName,a.BankAccountName} into g
                          select new { GUID_CN_Main = g.Key.GUID_CN_Main,g.Key.SettleTypeName,g.Key.BankAccountName,Total_CN = g.Sum(a => a.Total_CN) };
            var o = (from d in dbdetai
                     //join a in this.InfrastructureContext.SS_BankAccountView on 
                     join m in main on d.GUID_CN_Main equals m.GUID //into temp
                     where d.GUID_CN_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, d.Total_CN, m.YWTypeName, d.SettleTypeName, d.BankAccountName, m.Maker, m.MakeDate, m.ModifyDate, m.DocMemo, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                Total = e.Total_CN,
                e.YWTypeName,
                e.SettleTypeName,//结算类型
                e.BankAccountName,//银行信息
                e.Maker,
                MakeDate=e.MakeDate.ToString("yyyy-MM-dd"),
                ModifyDate = e.ModifyDate == null ? "" : ((DateTime)e.ModifyDate).ToString("yyyy-MM-dd"),
                e.DocMemo,
                e.YWTypeKey

            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        /// <summary>
        /// 收入历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> SR_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<SR_MainView> main = this.BusinessContext.SR_MainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }

            IQueryable<SR_DetailView> detail = this.BusinessContext.SR_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);
                detail = detail.Where(e => e.GUID_Maker == this.OperatorId);
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {
                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                        main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态



                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }

                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                           // detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e =>e.GUID_ProjectKey!=null && projectGUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => projectUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e =>e.GUID_BGCode!=null && bgcodeGUID.Contains((Guid)e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_SR_Main into g
                          select new { GUID_SR_Main = g.Key, Total_SR = g.Sum(a => a.Total_SR) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_SR_Main equals m.GUID //into temp
                     where d.GUID_SR_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_SR, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                PersonName = e.PersonName == null ? "" : e.PersonName,
                BillCount = e.BillCount == null ? 0 : e.BillCount,
                Total=e.Total_SR,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,
                e.YWTypeKey 
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        /// <summary>
        /// 收款历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> SK_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            var main = this.BusinessContext.SK_MainView.Where(e=>1==1);
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }
                       
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);               
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    main = main.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {
                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                        main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }


                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                           
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            //SS_BGCode bgcodeModel = new SS_BGCode();
                            //bgcodeModel.GUID = historyconditions.treeValue;
                            //bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            //var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                           
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
          
            var o = (from m in main
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, m.Total_SK, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                PersonName = e.PersonName == null ? "" : e.PersonName,
                BillCount = e.BillCount == null ? 0 : e.BillCount,
                Total = e.Total_SK,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        /// <summary>
        /// 现金存取记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        private  List<object> Cash_History(SearchCondition conditions)
        {
            List<object> list = new List<object>();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            if (historyconditions.RequestType == "xq")//需求
            {
                //清空防止干扰
                historyconditions.PayStatus = "";
                historyconditions.CheckStatus = "";
                historyconditions.WithdrawStatus = "1";//未提现

                historyconditions.PayStatus = "";
                
                historyconditions.CertificateStatus = "1";  //未生成凭证; Common.EnumType.EnumCertificateStatus.NotCertificate;
                historyconditions.ApproveStatus = "2";     
                //2016-1-13王娟要求改为已审批
                //审批中2014 9.23 张龙确定过
                //Common.EnumType.EnumApproveStatus.Approved.ToString();//hanyx
                historyconditions.SettleTypeKey = "01";     //必须是现金

                historyconditions.OperatorId = null;
                list = Cash_XQ_History(historyconditions);
            }
            else
            {
                list = Cash_CQ_History(historyconditions);
            }
            return list;
        }

        /// <summary>
        /// 现金存取历史记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        private List<object> Cash_CQ_History(HistoryCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = conditions;
            IQueryable<CN_CashMainView> main = this.BusinessContext.CN_CashMainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }

            IQueryable<CN_CashDetailView> detail = this.BusinessContext.CN_CashDetailView.Where(e =>e.GUID_CN_CashMain!=null && main.Select(m => m.GUID).Contains((Guid)e.GUID_CN_CashMain));
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);                
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {
                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                // 时间条件
                if (!historyconditions.StartDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate >= historyconditions.StartDate);
                }
                if (!historyconditions.EndDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate <= historyconditions.EndDate);
                }

                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                        main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取

                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取

                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态

                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现

                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现

                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态

                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款

                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款

                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }


                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                #region 树查询信息



                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e =>e.GUID_Department!=null && depguid.Contains((Guid)e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            //SS_BGCode bgcodeModel = new SS_BGCode();
                            //bgcodeModel.GUID = historyconditions.treeValue;
                            //bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            //var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            //detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }
                #endregion

            }
            //明细信息
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_CN_CashMain into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Cash) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.YWTypeName, m.DocTypeName, m.DWName, m.DepartmentName, m.PersonName, m.DocDate, d.Total_BX, m.MakeDate, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                e.DocNum,
                e.YWTypeName,
                e.DocTypeName,
                e.DWName,
                e.DepartmentName,
                e.PersonName,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                Total = e.Total_BX,
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();


            return mainList;
        }
      
        /// <summary>
        /// 现金存取需求记录

        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        private List<object> Cash_XQ_History(HistoryCondition conditions)
        {            
            //HistoryCondition historycondion = new HistoryCondition()
            //historycondion
            List<object> list = new List<object>();
            if (conditions.YWTypeKey != null)
            {
                //ModelURl 根据业务类型并且选择的单价重新赋值

                Guid g;
                conditions.ModelUrl = string.Empty;
                if(conditions.GUID_DocType!=null && conditions.GUID_DocType!=Guid.Empty)
                {
                    if (Guid.TryParse(conditions.GUID_DocType.ToString(), out g))
                    {
                        var doctypeView = this.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.GUID == g);
                        if(doctypeView!=null)
                        {
                            conditions.ModelUrl = doctypeView.DocTypeUrl;
                        }
                    }
                }
                var guidOperator = this.OperatorId;
                this.OperatorId = Guid.Empty;
                string ywType=conditions.YWTypeKey.ToString();
                switch (ywType)
                {
                    case Constant.YWTwo://"报销管理":
                        list = BX_History(conditions);
                        break;
                    case Constant.YWThree://"收入管理":
                    case Constant.YWElevenO://"收入信息流转":
                        list = SR_History(conditions);
                        break;
                    case Constant.YWFour: //"专用基金":
                        list = JJ_History(conditions);
                        break;
                    case Constant.YWFive://往来管理
                    case Constant.YWFiveO://"单位往来":
                    case Constant.YWFiveT://"个人往来":
                        list = WL_History(conditions);
                        break;
                    case Constant.YWEight://出纳管理
                    case Constant.YWEightO: //"收付款管理":                                      
                        list = CN_History(conditions);
                        break;
                    case Constant.YWEightT://"提存现管理":
                        list = Cash_CQ_History(conditions);
                        break;
                    default:
                        var listTemp = BX_History(conditions);
                        list.AddRange(listTemp);
                         listTemp = SR_History(conditions);
                         list.AddRange(listTemp);
                         listTemp = JJ_History(conditions);
                         list.AddRange(listTemp);
                         listTemp = WL_History(conditions);
                         list.AddRange(listTemp);
                         listTemp = CN_History(conditions);
                         list.AddRange(listTemp);
                         listTemp = Cash_CQ_History(conditions);
                         list.AddRange(listTemp); 
                        break;
                }
                this.OperatorId = guidOperator;
            }
         
            return list;

        }
        #region 公务卡汇总报销单



        /// <summary>
        /// 公务卡汇总数据



        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public  List<object> BX_GWDHZ_History(SearchCondition conditions)
        {
            List<object> list = new List<object>();
            HistoryCondition historycondition = (HistoryCondition)conditions;
            if (historycondition != null)
            {
                if (historycondition.RequestType == "cz")//参照
                {
                    conditions.ModelUrl = "gwkbxd";//查找公务卡信息

                    list = OrgBX_History(conditions);
                }
                else
                {
                    list = BX_gwdhzbxd_History(conditions);
                }
            }
            return list;
        }
        /// <summary>
        /// 公务卡汇总报销历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> BX_gwdhzbxd_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BX_CollectMainView> main = this.BusinessContext.BX_CollectMainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }            
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);               
            }
            if (historyconditions != null)
            {                
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => e.DocDate!=null &&((DateTime)e.DocDate).Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e =>e.DocDate!=null && ((DateTime)e.DocDate).Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    {
                        List<string> list = Constant.NotApproveState.ListState(); //"",0
                        List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                        main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    {
                        List<string> list = Constant.ApprovedState.ListState();
                        main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                        //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    {
                        List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                        List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                        main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                        //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态



                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取



                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }

                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));                          
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            //SS_Project projectModel = new SS_Project();
                            //projectModel.GUID = historyconditions.treeValue;
                            //projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            //var projectGUID = projectList.Select(e => e.GUID);
                            //main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            //SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            //projectclassModel.GUID = historyconditions.treeValue;
                            //projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            //var projectUID = projectList.Select(e => e.GUID);
                            //detail = detail.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            //SS_BGCode bgcodeModel = new SS_BGCode();
                            //bgcodeModel.GUID = historyconditions.treeValue;
                            //bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            //var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            //detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            ////明细信息
            //var dbdetai = from a in detail
            //              group a by a.GUID_BXCOLLECTMain into g
            //              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.) };
            var o = (from m in main
                     select new { m.GUID, m.DocNum, m.DWName, m.SubmitDate, m.DocState, m.DocDate, m.DepartmentName, m.MakeDate, m.YWTypeKey });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                e.DWName,
                SubmitDate = e.SubmitDate == null ? "" : DateTime.Parse(e.SubmitDate.ToString()).ToString("yyyy-MM-dd"),
                e.DocState,
                DocDate =e.DocDate==null?"":((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,               
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        /// <summary>
        /// 参照报销历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> OrgBX_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BX_MainView> main = this.BusinessContext.BX_MainView.Where(e =>e.DocState!="9");
            //并且在公务卡汇总报销单重不存在此报销单

            main = main.Where(e=>!this.BusinessContext.BX_CollectDetail.Select(ee=>ee.GUID_BXMain).Contains(e.GUID));
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }
            IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            //参照去掉操作员条件

            //if (this.OperatorId.IsNullOrEmpty())
            //{
            //    return null;
            //}
            //else
            //{

            //}
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => e.DocDate.Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate.Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 状态条件




                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    //if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    //{
                    //    List<string> list = Constant.NotApproveState.ListState(); //"",0
                    //    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                    //    main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    //}
                    //else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    //{
                    //    List<string> list = Constant.ApprovedState.ListState();
                    //    main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                    //    //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    //}
                    //else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    //{
                    //    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧

                    //    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧                     
                    //    main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                    //    //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    //}
                }
                #endregion

               
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取

                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取



                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }

                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态



                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现



                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态



                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款



                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }

                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    string docState = ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString();
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != docState);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == docState);
                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                           // detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            if (historyconditions.StartDate + "" != "" && historyconditions.EndDate + "" != "")
            {
                main = main.Where(e => e.DocDate >= historyconditions.StartDate && e.DocDate <= historyconditions.EndDate);
            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_BX_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey,m.Maker });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                //PersonName = e.PersonName == null ? "" : e.PersonName,//报销人

                Maker=e.Maker,//制单人

                BillCount = e.BillCount == null ? 0 : e.BillCount,
                Total = e.Total_BX,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }

        /// <summary>
        /// 基金历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> JJ_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<JJ_MainView> main = this.BusinessContext.JJ_MainView;
            if (!string.IsNullOrEmpty(historyconditions.ModelUrl))
            {
                main = main.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);
            }

            IQueryable<JJ_DetailView> detail = this.BusinessContext.JJ_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (!this.OperatorId.IsNullOrEmpty())
            {
                main = main.Where(e => e.GUID_Maker == this.OperatorId);
                detail = detail.Where(e => e.GUID_Maker == this.OperatorId);
            }
            if (historyconditions != null)
            {
                //结算方式
                if (!string.IsNullOrEmpty(historyconditions.SettleTypeKey))
                {
                    detail = detail.Where(e => e.SettleTypeKey == historyconditions.SettleTypeKey);
                }
                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => e.DocDate!=null && ((DateTime)e.DocDate).Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => e.DocDate!=null && ((DateTime)e.DocDate).Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件



                #region 审批状态

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态

                {
                    //if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批

                    //{
                    //    List<int?> list = Constant.NotApproveState.ListIntState(); //"",0
                    //    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件

                    //    main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                    //}
                    //else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批

                    //{
                    //    List<int?> list = Constant.ApprovedState.ListIntState();
                    //    main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                    //    //  main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    //}
                    //else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中

                    //{
                    //    List<int?> NotApproveList = Constant.NotApproveState.ListIntState();//旧

                    //    List<int?> ApprovedList = Constant.ApprovedState.ListIntState();//旧                     
                    //    main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                    //    //main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    //}
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态

                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取

                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取

                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态

                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现

                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现

                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态

                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款

                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款

                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态

                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证

                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"

                        // 核销明细表中不存在核销主表GUID不在凭证表中 或者 主表中的GUID不在核销明细表中
                        //GUID不在凭证表中 或者 不在核销表中
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));

                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//生成生成凭证
                    {
                        // //main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main in (select GUID_HXMain from cw_pzmain))
                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID_HXMain).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != 9 || e.DocState==null);

                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废

                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState== 9);

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => depguid.Contains(e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => dwguid.Contains(e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            //SS_BGCode bgcodeModel = new SS_BGCode();
                            //bgcodeModel.GUID = historyconditions.treeValue;
                            //bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            //var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            //detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);                        
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = (from a in detail
                          group a by a.GUID_JJ_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_JJ) }).ToList();
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate,m.YWTypeKey }).ToList();

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate =e.DocDate==null?"":((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
                PersonName = e.PersonName == null ? "" : e.PersonName,
                BillCount = e.BillCount == null ? 0 : e.BillCount,
                Total = e.Total_BX,
                DocMemo = e.DocMemo == null ? "" : e.DocMemo,
                YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
                DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
                e.MakeDate,
                e.YWTypeKey
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;
        }
        #endregion
        /// <summary>
        /// 凭证历史记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private List<object> PZ_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            PZHistoryCondition historyconditions = (PZHistoryCondition)conditions;
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            if (string.IsNullOrEmpty(historyconditions.FiscalYear) || string.IsNullOrEmpty(historyconditions.AccountKey)) {
                ErrorCode = "会计年度不存在";
                return null;
            }

            string DataBaseString = CommonFun.GetU8MatchDataBase(this.BusinessContext, historyconditions.AccountKey, historyconditions.FiscalYear);
            if (string.IsNullOrEmpty(DataBaseString))
            {
                ErrorCode = "用友帐套不存在";
                return null;
            }

            bool DataBaseExsist = CommonFun.IsDataBaseExsist(this.BusinessContext, DataBaseString);
            if (DataBaseExsist == false)
            {
                ErrorCode = "用友帐套不存在";
                return null;
            }

            StringBuilder sql = new StringBuilder("select distinct GUID,FiscalYear,CWPeriod,DocNum,GUID_Maker,GUID_PZType,PZTypeName,convert(nvarchar,DocDate,23) as DocDate,Maker,ExteriorDataBase,a.Ino_id as ino_id,convert(nvarchar,MakeDate,23) as MakeDate,YWTypeKey" );
           sql.AppendFormat(" from CW_PZMainView join  {0}..gl_accvouch a on convert(nvarchar(50),CW_PZMainView.GUID) =a.coutno_id where 1=1 ",DataBaseString);
           // "  ";// and FiscalYear='{0}' and CWPeriod='{1}' and ExteriorDataBase='002 ' order by ExteriorDataBase,CWPeriod,DocNum";
            if (historyconditions != null)
            {
                //会计年度
                if (!string.IsNullOrEmpty(historyconditions.FiscalYear) && historyconditions.FiscalYear != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.FiscalYear, out y))
                    {
                        sql.AppendFormat(" and FiscalYear='{0}' ",historyconditions.FiscalYear);
                    }
                }
                //凭证期间
                if (!string.IsNullOrEmpty(historyconditions.CWPeriod) && historyconditions.CWPeriod != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.CWPeriod, out m))
                    {
                        sql.AppendFormat(" and CWPeriod='{0}' ", historyconditions.CWPeriod);
                    }
                }
                //凭证编号          
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    int docnum;
                    if (int.TryParse(historyconditions.DocNum, out docnum))
                    {
                        sql.AppendFormat(" and DocNum='{0}' ", historyconditions.DocNum);
                    }
                }
                //凭证日期
                if (!string.IsNullOrEmpty(historyconditions.DocDate))
                {
                    DateTime d;
                    if (DateTime.TryParse(historyconditions.DocDate, out d))
                    {
                        sql.AppendFormat(" and DocDate='{0}' ", historyconditions.DocDate);
                    }
                }
                //凭证类型
                if (!string.IsNullOrEmpty(historyconditions.GUID_PZType))
                {
                    Guid g;
                    if (Guid.TryParse(historyconditions.GUID_PZType, out g))
                    {
                        sql.AppendFormat(" and GUID_PZType='{0}' ", historyconditions.GUID_PZType);
                    }
                    else
                    {
                        sql.AppendFormat(" and PZTypeName='{0}' ", historyconditions.GUID_PZType);
                    }
                }
                //凭证帐套
                if (!string.IsNullOrEmpty(historyconditions.AccountKey))
                {
                    sql.AppendFormat(" and ExteriorDataBase='{0}' ", historyconditions.AccountKey);
                }
                //制单人


                if (!string.IsNullOrEmpty(historyconditions.GUID_Maker))
                {
                    Guid g;
                    if (historyconditions.GUID_Maker != Guid.Empty.ToString() && Guid.TryParse(historyconditions.GUID_Maker, out g))
                    {
                        sql.AppendFormat(" and GUID_Maker='{0}' ", historyconditions.GUID_Maker);
                    }

                }

                //对方凭证编号
                if (!string.IsNullOrEmpty(historyconditions.Ino_ID))
                {
                    //对方凭证编号                    
                    sql.AppendFormat(" and ino_id='{0}' ", historyconditions.Ino_ID);

                }
            }
            try
            {
                var tempSql = sql.ToString() +" order by  docNum";
                var result = this.BusinessContext.ExecuteStoreQuery<GL_accvouchModel>(tempSql);
                return result.ToList<object>();
            }
            catch (Exception ex)
            {
                ErrorCode = ex.Message;
                return null;
            }
          
        }

        /// <summary>
        /// 工资发放(工资单)
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public  List<object> GZD_History(SearchCondition conditions)
        {

            HistoryCondition conditionModel = (HistoryCondition)conditions;
            var q = this.BusinessContext.SA_PlanActionView.AsQueryable();
            if (conditionModel != null)
            {
                if (!conditionModel.GUID_Plan.IsNullOrEmpty())
                {
                    q = q.Where(e => e.GUID_Plan == conditionModel.GUID_Plan);
                }
                int year = 0;
                int.TryParse(conditionModel.Year, out year);
                if (year != 0)
                {
                    q = q.Where(e => e.ActionYear == year);
                }
                int month = 0;
                int.TryParse(conditionModel.Month, out month);
                if (month != 0)
                {
                    q = q.Where(e => e.ActionMouth == month);
                }
                if (!string.IsNullOrEmpty(conditionModel.PayOutState))
                {
                    int payoutState = 0;
                    if (int.TryParse(conditionModel.PayOutState, out payoutState) && payoutState == -1)
                    {
                    }
                    else
                    {
                        q = q.Where(e => e.ActionState == payoutState);
                    }
                }
            }
            var objlist = q.AsEnumerable().OrderByDescending(e => e.DocDate).Select(e => new { e.GUID, e.DocNum, e.PlanName, e.ActionYear, e.ActionMouth, DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"), e.ActionPeriod, e.ActionTimes, e.Descrip, e.YWTypeKey }).Distinct().ToList<object>();
            return objlist;
        }
        #endregion

        #region IHistory 成员
        //东升实现 自己传参数

        public List<object> GetYSTZHistory(string strYear,string strBGType,string strBGStep,string strProject,string strDepartment)
        {
            List<object> list = new List<object>();
            int iYear = Int32.Parse(strYear);
            var guid_bg_mainSet = from detail in this.BusinessContext.BG_Detail
                                  where detail.BGYear == iYear
                                  select detail.GUID_BG_Main;
            guid_bg_mainSet = guid_bg_mainSet.Distinct();
            string [] arrayProject = strProject.Split(',');
            HashSet<Guid> hsProjectGUID = new HashSet<Guid>();
            foreach(string str in arrayProject)
            {
                hsProjectGUID.Add(new Guid(str));
            }

            string [] arrayDepartment = strDepartment.Split(',');
            HashSet<Guid> hsDepartment = new HashSet<Guid>();
            foreach(string str in arrayDepartment)
            {
                hsDepartment.Add(new Guid(str));
            }

            IQueryable<BG_MainView> main = null;
            if(strBGType=="02"){
                main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true && guid_bg_mainSet.Contains(e.GUID) &&
                e.BGTypeKey == strBGType && e.BGStepKey == strBGStep && hsProjectGUID.Contains((Guid)e.GUID_Project) && hsDepartment.Contains((Guid)e.GUID_Department));  
            }
            else
            {
                main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true && guid_bg_mainSet.Contains(e.GUID) &&
                e.BGTypeKey == strBGType && e.BGStepKey == strBGStep && hsDepartment.Contains((Guid)e.GUID_Department));  
            }

            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            IntrastructureFun objIF = new Infrastructure.IntrastructureFun();
            var DepartmentAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            var PersonAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            var ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            IQueryable<BG_MainView> mainSetWithAuth = null;
            if (strBGType == "02")
            {
                mainSetWithAuth = main.Where(e => PersonAuth.Contains((Guid)e.GUID_Person)
                && ProjectAuth.Contains((Guid)e.GUID_Project));
            }
            else
            {
                main.Where(e =>  PersonAuth.Contains((Guid)e.GUID_Person)
                && DepartmentAuth.Contains((Guid)e.GUID_Department));
            }

            list = mainSetWithAuth.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                e.DocVerson,
                e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.PersonName
                
            }).OrderByDescending(e => e.DocNum).ToList<object>();

            return list;
        }

        #endregion
    }
    public class GL_accvouch 
    {
        public string Ino_id { get; set; }
    }
    public class GL_accvouchModel
    {
        //GUID,FiscalYear,CWPeriod,DocNum,GUID_Maker,GUID_PZType,PZTypeName,DocDate,Maker,ExteriorDataBase,a.Ino_id as ino_id,MakeDate,YWTypeKey
        public Guid GUID { set; get; }
        public int FiscalYear { set; get; }
        public int CWPeriod { get; set; }
        public int DocNum { get; set; }
        public Guid GUID_Maker { set; get; }
        public Guid GUID_PZType { set; get; }
        public string PZTypeName { set; get; }
        public string DocDate { get; set; }
        public string Maker { get; set; }
        public string ExteriorDataBase { get; set; }
        public short? ino_id { get; set; }
        public string MakeDate { get; set; }
        public string YWTypeKey { get; set; }
    }
}
