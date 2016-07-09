using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using System.Data.Objects;
using Business.CommonModule;
using BusinessModel;

namespace Business.Reimbursement
{
    public class CommonMethod:BaseDocument
    {
         public CommonMethod() : base() { }
         public CommonMethod(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
         ///// <summary>
         ///// 借款统计信息
         ///// </summary>
         ///// <param name="conditions">条件</param>
         ///// <returns> object List< /returns>
         //public override List<object> BorrowMoney(SearchCondition conditions)
         //{
         //    List<object> list = new List<object>();
         //    BorrowMoneyCondition borrowCondition = (BorrowMoneyCondition)conditions;
         //    IQueryable<WL_MainView> wl_main = this.BusinessContext.WL_MainView;//往来单（包含借款）

         //    IQueryable<HX_MainView> hx_main = this.BusinessContext.HX_MainView.Where(e => e.DocTypeKey == "10");//核销 中的借款
         //    int wl_main_classid = Infrastructure.CommonFuntion.GetClassId(typeof(WL_Main).Name);
         //    int wl_detail_classid = Infrastructure.CommonFuntion.GetClassId(typeof(WL_Detail).Name);
         //    int cn_main_classid = Infrastructure.CommonFuntion.GetClassId(typeof(HX_Main).Name);
         //    int cn_detail_classid = Infrastructure.CommonFuntion.GetClassId(typeof(HX_Detail).Name);

         //    if (borrowCondition != null)
         //    {
         //        if (borrowCondition.StartDate != null && borrowCondition.StartDate != DateTime.MinValue)
         //        {
         //            wl_main = wl_main.Where(e => e.DocDate >= borrowCondition.StartDate);
         //            hx_main = hx_main.Where(e => e.DocDate >= borrowCondition.StartDate);
         //        }
         //        if (borrowCondition.EndDate != null && borrowCondition.EndDate != DateTime.MinValue)
         //        {
         //            wl_main = wl_main.Where(e => e.DocDate <= borrowCondition.EndDate);
         //            hx_main = hx_main.Where(e => e.DocDate <= borrowCondition.EndDate);
         //        }
         //        if (!string.IsNullOrEmpty(borrowCondition.treeModel) && (borrowCondition.treeValue != null && borrowCondition.treeValue != Guid.Empty))
         //        {
         //            switch (borrowCondition.treeModel.ToLower())
         //            {
         //                case "ss_department":
         //                    List<SS_Department> depList = new List<SS_Department>();
         //                    SS_Department dep = new SS_Department();
         //                    dep.GUID = borrowCondition.treeValue;
         //                    dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
         //                    var depguid = depList.Select(e => e.GUID);
         //                    wl_main = wl_main.Where(e => depguid.Contains(e.GUID_Department));
         //                    hx_main = hx_main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));

         //                    break;
         //                case "ss_dw":
         //                    List<SS_DW> dwList = new List<SS_DW>();
         //                    SS_DW dw = new SS_DW();
         //                    dw.GUID = borrowCondition.treeValue;
         //                    dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
         //                    var dwguid = dwList.Select(e => e.GUID);
         //                    wl_main = wl_main.Where(e => dwguid.Contains(e.GUID_DW));
         //                    hx_main = hx_main.Where(e => dwguid.Contains(e.GUID_DW));
         //                    break;
         //                case "ss_person":
         //                    wl_main = wl_main.Where(e => e.GUID_Person == borrowCondition.treeValue);
         //                    hx_main = hx_main.Where(e => e.GUID_Person == borrowCondition.treeValue);
         //                    break;
         //            }
         //        }
         //        //查询借款核销明细
         //        var hx_wl_detail = this.BusinessContext.HX_Detail.Where(e => e.ClassID_Main == wl_main_classid && hx_main.Select(m => m.GUID).Contains(e.GUID_HX_Main));//借款核销明细
         //        //查询 出纳核销明细
         //        var hx_cn_detail = this.BusinessContext.HX_Detail.Where(e => e.ClassID_Main == cn_main_classid && hx_main.Select(m => m.GUID).Contains(e.GUID_HX_Main));//出纳核销明细

         //        var wl_detail = this.BusinessContext.WL_DetailView.Where(e => wl_main.Select(m => m.GUID).Contains(e.GUID_WL_Main));//往来明细

         //        //借款核销明细信息 （已经完成核销处理并生成出纳付款单的借款单状态为“已领款”）
         //        var hx_detailDrawMoneySum = from d in hx_wl_detail
         //                                    join dwl in wl_detail on d.GUID_Detail equals dwl.GUID
         //                                    join mwl in wl_main on dwl.GUID_WL_Main equals mwl.GUID
         //                                    where (mwl.DocState == "999" || mwl.DocState == "-1")// && hx_cn_detail.Count() > 0 //核销信息大于0
         //                                    group d by new { dwl.GUID_WL_Main, dwl.GUID_BGCode, dwl.GUID_ProjectKey } into temp
         //                                    select new
         //                                    {
         //                                        GUID_Main = (Guid)temp.Where(e => e.GUID_Main != null).Select(e => e.GUID_Main).FirstOrDefault(),
         //                                        Total_HX = temp.Sum(d => d.Total_HX),
         //                                        DrawMoneyType = "已领取"
         //                                    };

         //        //借款核销明细信息 （未进行核销处理并生成出纳付款单业务处理的借款单状态为“未领款”）
         //        var hx_detailNoDrawMoneySum = from d in hx_wl_detail
         //                                      join dwl in wl_detail on d.GUID_Detail equals dwl.GUID
         //                                      join mwl in wl_main on dwl.GUID_WL_Main equals mwl.GUID
         //                                      where (mwl.DocState != "999" || mwl.DocState != "-1") //&& hx_cn_detail.Count() > 0 //核销信息大于0
         //                                      group d by new { mwl.GUID, dwl.GUID_BGCode, dwl.GUID_ProjectKey } into temp
         //                                      select new
         //                                      {
         //                                          GUID_Main = (Guid)temp.Where(e => e.GUID_Main != null).Select(e => e.GUID_Main).FirstOrDefault(),
         //                                          Total_HX = temp.Sum(d => d.Total_HX),
         //                                          DrawMoneyType = "未领取"
         //                                      };
         //        var t = hx_detailNoDrawMoneySum.ToList();
         //        //借款核销明细信息
         //        //var hx_detailSum =hx_detailNoDrawMoneySum.Union(hx_detailDrawMoneySum);            
         //        var hx_detailSum = hx_detailNoDrawMoneySum;
         //        string CN_DocDate = DateTime.Now.ToString("yyyy-MM-dd");
         //        list = (from m in wl_main
         //                join dlw in wl_detail on m.GUID equals dlw.GUID_WL_Main
         //                join d in hx_detailSum on m.GUID equals d.GUID_Main into temp
         //                from d in temp.DefaultIfEmpty()
         //                select new
         //                {
         //                    DocDate = m.DocDate,
         //                    m.DepartmentName,
         //                    m.PersonName,
         //                    dlw.BGCodeName,
         //                    m.DocMemo,
         //                    Total_HX = 0,
         //                    dlw.WLTypeName,
         //                    dlw.SettleTypeName,
         //                    dlw.ProjectName,
         //                    dlw.CustomerName,
         //                    d.DrawMoneyType,
         //                    RePamyment = "未还款",
         //                    CN_DocDate = CN_DocDate
         //                })
         //                .AsEnumerable()
         //                 .Select(e => new
         //                 {
         //                     DocDate = e.DocDate.ToString("yyyy-MM-dd"),
         //                     e.DepartmentName,
         //                     e.PersonName,
         //                     e.BGCodeName,
         //                     e.DocMemo,
         //                     e.Total_HX,
         //                     e.WLTypeName,
         //                     e.SettleTypeName,
         //                     e.ProjectName,
         //                     e.CustomerName,
         //                     e.DrawMoneyType,
         //                     e.RePamyment,
         //                     e.CN_DocDate
         //                 }
         //                 ).ToList<object>();




         //    }

         //    return list;
         //}

         /// <summary>
         /// 获取预算执行情况统计数据(待完成)
         /// </summary>
         /// <param name="operatorId"></param>
         /// <returns></returns>
         //public override List<BudgetStatisticsModel> BudgetStatistics(SearchCondition conditions)
         //{
         //    BudgetStatisticsCondition bsCondition = (BudgetStatisticsCondition)conditions;
         //    JsonModel jsonmodel = new JsonModel();            
         //    int docDateYear = 0;
         //    if (bsCondition == null)
         //    {
         //        return null;
         //    }
         //    if (!string.IsNullOrEmpty(bsCondition.DocDate))
         //    {
         //        DateTime g;
         //        if (DateTime.TryParse(bsCondition.DocDate, out g))
         //        {
         //            docDateYear = g.Year;
         //        }
         //    }
         //    ObjectParameter[] parameters ={
         //                                   new ObjectParameter("operatorId",bsCondition.GUID_Operator),
         //                                   new ObjectParameter("dwId",bsCondition.GUID_DW),
         //                                   new ObjectParameter("mainID",bsCondition.GUID),
         //                                   new ObjectParameter("depId",bsCondition.GUID_Department),
         //                                   new ObjectParameter("projectId",bsCondition.GUID_Project),
         //                                   new ObjectParameter("bgcodeId",bsCondition.GUID_BGCode),
         //                                   new ObjectParameter("resourceId",bsCondition.GUID_BGResource),
         //                                   new ObjectParameter("year",docDateYear)
         //                               };
         //    List<BudgetStatisticsModel> list = this.BusinessContext.ExecuteFunction<BudgetStatisticsModel>("bx_GetBudgetStatistics", parameters).ToList();

         //    return list;
         //}
        // #region 历史记录
        // /// <summary>
        // /// 历史记录
        // /// </summary>
        // /// <param name="conditions">条件</param>
        // /// <returns>JsonModel</returns>
        // public override List<object> History(SearchCondition conditions)
        // {
        //     if (conditions != null)
        //     {
        //         var docType = conditions.ModelUrl;
        //         switch (docType)
        //         { 
        //             case "jkdtz":
        //                 return null;
        //             default:
        //                 return BX_History(conditions);
        //         }
        //     }
        //     return null;
        // }
        ///// <summary>
        ///// 报销历史记录
        ///// </summary>
        ///// <param name="conditions"></param>
        ///// <returns></returns>
        // private  List<object> BX_History(SearchCondition conditions)
        // {
        //     JsonModel jsonmodel = new JsonModel();
        //     BX_HistoryBaseCondition historyconditions = (BX_HistoryBaseCondition)conditions;
        //     IQueryable<BX_MainView> main = this.BusinessContext.BX_MainView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单
        //     IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
        //     List<SS_Department> depList = new List<SS_Department>();
        //     List<SS_DW> dwList = new List<SS_DW>();
        //     List<SS_Project> projectList = new List<SS_Project>();
        //     List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
        //     List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
        //     if (this.OperatorId.IsNullOrEmpty())
        //     {
        //         return null;
        //     }
        //     else
        //     {
        //         main = main.Where(e => e.GUID_Modifier == this.OperatorId);
        //         detail = detail.Where(e => e.GUID_Modifier == this.OperatorId);
        //     }
        //     if (historyconditions != null)
        //     {

        //         if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
        //         {
        //             int y;
        //             if (int.TryParse(historyconditions.Year, out y))
        //             {
        //                 main = main.Where(e => e.DocDate.Year == y);
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
        //         {
        //             int m;
        //             if (int.TryParse(historyconditions.Month, out m))
        //             {
        //                 main = main.Where(e => e.DocDate.Month == m);
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.DocNum))
        //         {
        //             main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
        //         }
        //         #region 审批状态条件

        //         if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
        //         {
        //             if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
        //             {
        //                 main = main.Where(e => e.DocState == "" || e.DocState == "0");
        //             }
        //             else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
        //             {
        //                 main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
        //             }
        //             else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//审批中
        //             {
        //                 main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
        //         {
        //             if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
        //             {
        //                 //main.GUID in (select GUID_Doc from cn_checkdrawmain)
        //                 var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
        //                 main = main.Where(e => !guidList.Contains(e.GUID));
        //             }
        //             else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
        //             {
        //                 var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
        //                 main = main.Where(e => guidList.Contains(e.GUID));
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
        //         {
        //             if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
        //             {
        //                 var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
        //                 main = main.Where(e => !guidList.Contains(e.GUID));
        //             }
        //             else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
        //             {
        //                 var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
        //                 main = main.Where(e => guidList.Contains(e.GUID));
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
        //         {
        //             if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
        //             {
        //                 var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                 main = main.Where(e => !guidList.Contains(e.GUID));
        //             }
        //             else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
        //             {
        //                 var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                 main = main.Where(e => guidList.Contains(e.GUID));
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
        //         {
        //             if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
        //             {
        //                 //在凭证主表中 存在核销明细表中的主表信息

        //                 var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
        //                 main = main.Where(e => !guidList.Contains(e.GUID));
        //             }
        //             else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//未生成凭证
        //             {
        //                 var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
        //                 var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                 main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));
        //             }
        //         }
        //         if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
        //         {
        //             if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
        //             {
        //                 //作废 DocState 为9
        //                 main = main.Where(e => e.DocState != "9");

        //             }
        //             else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
        //             {
        //                 //作废 DocState 为9
        //                 main = main.Where(e => e.DocState.Trim() == "9");

        //             }
        //         }
        //         #endregion

        //         if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
        //         {
        //             switch (historyconditions.treeModel.ToLower())
        //             {
        //                 case "ss_department":
        //                     SS_Department dep = new SS_Department();
        //                     dep.GUID = historyconditions.treeValue;
        //                     dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
        //                     var depguid = depList.Select(e => e.GUID);
        //                     main = main.Where(e => depguid.Contains(e.GUID_Department));
        //                     detail = detail.Where(e => depguid.Contains(e.GUID_Department));
        //                     break;
        //                 case "ss_dw":
        //                     SS_DW dw = new SS_DW();
        //                     dw.GUID = historyconditions.treeValue;
        //                     dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
        //                     var dwguid = dwList.Select(e => e.GUID);
        //                     main = main.Where(e => dwguid.Contains(e.GUID_DW));
        //                     break;
        //                 case "ss_project":
        //                     SS_Project projectModel = new SS_Project();
        //                     projectModel.GUID = historyconditions.treeValue;
        //                     projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
        //                     var projectGUID = projectList.Select(e => e.GUID);
        //                     detail = detail.Where(e => projectGUID.Contains((Guid)e.GUID_Project));
        //                     break;
        //                 case "ss_projectclass":
        //                     SS_ProjectClass projectclassModel = new SS_ProjectClass();
        //                     projectclassModel.GUID = historyconditions.treeValue;
        //                     projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
        //                     var projectUID = projectList.Select(e => e.GUID);
        //                     detail = detail.Where(e => projectUID.Contains((Guid)e.GUID_Project));
        //                     break;
        //                 case "ss_bgcode":
        //                     SS_BGCode bgcodeModel = new SS_BGCode();
        //                     bgcodeModel.GUID = historyconditions.treeValue;
        //                     bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
        //                     var bgcodeGUID = bgcodeList.Select(e => e.GUID);
        //                     detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
        //                     break;
        //                 case "ss_person":
        //                     main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
        //                     break;
        //             }
        //         }


        //     }
        //     //明细信息
        //     var dbdetai = from a in detail
        //                   group a by a.GUID_BX_Main into g
        //                   select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
        //     var o = (from d in dbdetai
        //              join m in main on d.GUID_BX_Main equals m.GUID //into temp
        //              where d.GUID_BX_Main != null && m.GUID != null
        //              select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate });

        //     var mainList = o.AsEnumerable().Select(e => new
        //     {
        //         e.GUID,
        //         DocNum = e.DocNum == null ? "" : e.DocNum,
        //         DocDate = e.DocDate.ToString("yyyy-MM-dd"),
        //         DepartmentName = e.DepartmentName == null ? "" : e.DepartmentName,
        //         PersonName = e.PersonName == null ? "" : e.PersonName,
        //         BillCount = e.BillCount == null ? 0 : e.BillCount,
        //         e.Total_BX,
        //         DocMemo = e.DocMemo == null ? "" : e.DocMemo,
        //         YWTypeName = e.YWTypeName == null ? "" : e.YWTypeName,
        //         DocTypeName = e.DocTypeName == null ? "" : e.DocTypeName,
        //         e.MakeDate
        //     }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

        //     return mainList;
        // }
        //#endregion
         public FlowData GetDataFlowByDoc(Guid docId,string url,BusinessEdmxEntities bContext,BaseConfigEdmxEntities iContext) 
         {
             var ssDoc = iContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl == url);
             if (ssDoc == null) return null;
             var flowData=new FlowData();
             switch (ssDoc.YWTypeKey)
             {
                 case "0502"://个人往来
                     var ent = bContext.WL_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                     var je = bContext.WL_Detail.Where(e => e.GUID_WL_Main == docId).Sum(e => e.Total_WL);
                     flowData.DocNum = ent.DocNum;
                     flowData.BXUserName = ent.PersonName;
                     flowData.DocName = ssDoc.DocTypeName;
                     flowData.SumMoney = je;
                     break;
                 case "02"://报销
                     var entBx = bContext.BX_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                     var jeBx = bContext.BX_Detail.Where(e => e.GUID_BX_Main == docId).Sum(e => e.Total_BX);
                     flowData.DocNum = entBx.DocNum;
                     flowData.BXUserName = entBx.PersonName;
                     flowData.DocName = ssDoc.DocTypeName;
                     flowData.SumMoney = jeBx;
                     break;
                 case "01"://预算
                    var entBG = bContext.BG_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                     var jeBGDetail = bContext.BG_Detail.Where(e => e.GUID_BG_Main== docId).Sum(e => e.Total_BG);
                     flowData.DocNum = entBG.DocNum;
                     flowData.BXUserName = entBG.PersonName;
                     flowData.DocName = ssDoc.DocTypeName;
                     flowData.SumMoney = jeBGDetail;
                     break;
                 default:
                     break;
             }
             return flowData;

 
         }
    }
    public class FlowData 
    {
        public FlowData() { }
        public FlowData(string bxUserName,string docName,double sumMoney,string docNum) 
        {
            this.BXUserName = bxUserName;
            this.DocName = docName;
            this.SumMoney = sumMoney;
            this.DocNum = docNum;
        }
        public string BXUserName { get; set; }
        public string DocName { get; set; }
        public double? SumMoney { get; set; }
        public string DocNum { get; set; }
    }
}
