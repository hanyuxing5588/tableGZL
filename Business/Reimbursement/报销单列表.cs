using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using Platform.Flow.Run;
using Business.CommonModule;
using BusinessModel;
namespace Business.Reimbursement
{
    public class 报销单列表 : BaseDocument
    {
        public 报销单列表() : base() { }
        public 报销单列表(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 单据列表记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        public override List<object> History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BX_BillListCondition historyconditions = (BX_BillListCondition)conditions;
            IQueryable<BX_MainView> main = this.BusinessContext.BX_MainView;
            IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();

            if (historyconditions != null)
            {
                ////业务类型
                //if (!historyconditions.GUID_YWType.IsNullOrEmpty())
                //{
                //    main = main.Where(e => e.GUID_YWType == historyconditions.GUID_YWType);
                //}
                #region 审批状态条件



                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                    {
                        main = main.Where(e => e.DocState == "" || e.DocState == "0");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                    {
                        main = main.Where(e => e.DocState == "999" || e.DocState == "-1");
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                    {
                        main = main.Where(e => e.DocState != "999" && e.DocState != "-1" && e.DocState != "" && e.DocState != "0");
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
                    {
                        //(main.GUID in (select GUID_Main from hx_detail where GUID_HX_Main not in (select GUID_HXMain from cw_pzmain)) or main.GUID not in (select GUID_Main from hx_detail))"
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
                        main.Where(e => e.DocState == "9");
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
                    {
                        //作废 DocState 为9
                        main.Where(e => e.DocState != "9");
                    }
                }
                #endregion
                //单据类型
                if (!string.IsNullOrEmpty(historyconditions.DocType) && historyconditions.DocType != Guid.Empty.ToString())
                {
                    Guid g;
                    if (Guid.TryParse(historyconditions.DocType, out g))
                    {
                        main = main.Where(e => e.GUID_DocType == g);
                    }
                }
                //开始时间
                if (!historyconditions.StartDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate >= historyconditions.StartDate);
                }
                //结束时间
                if (!historyconditions.EndDate.IsNullOrEmpty())
                {
                    main = main.Where(e => e.DocDate <= historyconditions.EndDate);
                }
                //单据号
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }

                //树条件
                #region 树条件

                #region 单个树条件
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
                            detail = detail.Where(e => depguid.Contains(e.GUID_Department));
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
                #endregion

                #region 多个树节点
                #region 多个树结构 TreeNodeList中每个treeModel对应一个treeValue

                if (historyconditions.TreeNodeList != null && historyconditions.TreeNodeList.Count > 0)
                {
                    List<TreeNode> treeNodeList = historyconditions.TreeNodeList;
                    List<string> typeList = treeNodeList.GetModelType();

                    foreach (string item in typeList)
                    {

                        switch (item.ToLower())
                        {
                            case "ss_person":
                                List<Guid> guidList = treeNodeList.GetGUIDList(item);
                                if (guidList.Count > 0)
                                {
                                    main = main.Where(e => guidList.Contains(e.GUID_Person));
                                }
                                break;
                            case "ss_department":
                                List<Guid> idsDep = new List<Guid>();
                                List<Guid> guidDep = treeNodeList.GetGUIDList(item);
                                foreach (Guid gdep in guidDep)
                                {
                                    SS_Department dep = new SS_Department();
                                    dep.GUID = gdep;
                                    dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                                    var depguid = depList.Select(e => e.GUID).ToList();
                                    if (depguid != null && depguid.Count > 0)
                                    {
                                        idsDep.AddRange(depguid);
                                    }
                                }
                                if (idsDep.Count > 0)
                                {
                                    main = main.Where(e => idsDep.Contains(e.GUID_Department));
                                    detail = detail.Where(e => idsDep.Contains(e.GUID_Department));
                                }
                                break;
                            case "ss_dw":
                                List<Guid> idsDW = new List<Guid>();
                                List<Guid> guidDW = treeNodeList.GetGUIDList(item);
                                foreach (Guid gdw in guidDW)
                                {
                                    SS_DW dw = new SS_DW();
                                    dw.GUID = gdw;
                                    dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                                    var dwguid = dwList.Select(e => e.GUID).ToList();
                                    if (dwguid != null && dwguid.Count > 0)
                                    {
                                        idsDW.AddRange(dwguid);
                                    }
                                }
                                if (idsDW.Count > 0)
                                {
                                    main = main.Where(e => idsDW.Contains(e.GUID_DW));
                                }
                                break;
                            case "ss_project":
                                List<Guid> idsProject = new List<Guid>();
                                List<Guid> guidProject = treeNodeList.GetGUIDList(item);
                                foreach (Guid gproject in guidProject)
                                {
                                    SS_Project projectModel = new SS_Project();
                                    projectModel.GUID = gproject;
                                    projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                                    var projectGUID = projectList.Select(e => e.GUID).ToList();
                                    if (projectGUID != null && projectGUID.Count > 0)
                                    {
                                        idsProject.AddRange(projectGUID);
                                    }
                                }
                                if (idsProject.Count > 0)
                                {
                                    detail = detail.Where(e => idsProject.Contains((Guid)e.GUID_Project));
                                }
                                break;
                            case "ss_projectclass":
                                List<Guid> idsProClass = new List<Guid>();
                                List<Guid> guidProClass = treeNodeList.GetGUIDList(item);
                                foreach (Guid gproClass in guidProClass)
                                {
                                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
                                    projectclassModel.GUID = gproClass;
                                    projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                                    var projectUID = projectList.Select(e => e.GUID).ToList();
                                    if (projectUID != null && projectUID.Count > 0)
                                    {
                                        idsProClass.AddRange(projectUID);
                                    }
                                }
                                if (idsProClass.Count > 0)
                                {
                                    detail = detail.Where(e => idsProClass.Contains((Guid)e.GUID_Project));
                                }
                                break;
                            case "ss_bgcode":
                                List<Guid> idsBgcode = new List<Guid>();
                                List<Guid> guidBgcode = treeNodeList.GetGUIDList(item);
                                foreach (Guid gBgcode in guidBgcode)
                                {
                                    SS_BGCode bgcodeModel = new SS_BGCode();
                                    bgcodeModel.GUID = gBgcode;
                                    bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                                    var bgcodeGUID = bgcodeList.Select(e => e.GUID).ToList();
                                    if (bgcodeGUID != null && bgcodeGUID.Count > 0)
                                    {
                                        idsBgcode.AddRange(bgcodeGUID);
                                    }
                                }
                                if (idsBgcode.Count > 0)
                                {
                                    detail = detail.Where(e => idsBgcode.Contains(e.GUID_BGCode));
                                }
                                break;
                            case "ss_functionclass":
                                List<Guid> idsFProject = new List<Guid>();
                                List<Guid> guidFProject = treeNodeList.GetGUIDList(item);
                                foreach (Guid gFproject in guidFProject)
                                {
                                    SS_FunctionClass projectModel = new SS_FunctionClass();
                                    projectModel.GUID = gFproject;
                                    projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                                    var projectGUID = projectList.Select(e => e.GUID).ToList();
                                    if (projectGUID != null && projectGUID.Count > 0)
                                    {
                                        idsFProject.AddRange(projectGUID);
                                    }
                                }
                                if (idsFProject.Count > 0)
                                {
                                    detail = detail.Where(e => idsFProject.Contains((Guid)e.GUID_Project));
                                }
                                break;
                            case "ss_doctype":
                                List<Guid> idsDoctype = new List<Guid>();
                                List<Guid> guidDoctype = treeNodeList.GetGUIDList(item);
                                if (guidDoctype.Count > 0)
                                {
                                    main = main.Where(e => guidDoctype.Contains((Guid)e.GUID_DocType));
                                }
                                break;

                        }
                    }
                }
                #endregion

                #endregion

                #endregion
                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey ==Constant.BGTypeOne); //基本支出"01"

                    }
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.ProjectPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeTwo);//项目支出 "02"
                    }
                }
                #endregion

            }
            #region 显示明细列表
            if (historyconditions != null && historyconditions.IsShowDetail)
            {
                #region 显示科目名称与编码明细信息

                var dbdetai = from a in detail
                              group a by new { a.GUID_BX_Main, a.GUID_BGCode, a.BGCodeName, a.BGCodeKey } into g
                              select new { key = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
                var o = (from d in dbdetai
                         join m in main on d.key.GUID_BX_Main equals m.GUID //into temp
                         select new
                         {
                             m.GUID,
                             m.DocNum,
                             m.DocDate,
                             m.DepartmentName,
                             m.PersonName,
                             m.BillCount,
                             d.Total_BX,
                             m.DocMemo,
                             m.YWTypeName,
                             m.DocTypeName,
                             m.MakeDate,
                             m.DocState,
                             d.key.BGCodeName,
                             d.key.BGCodeKey
                         });
                //支票领用主表
                var checkGuidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc).ToList();
                //现金需求关系表 
                var cashGuidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain).ToList();
                //核销Detail(付款状态应用)
                var HXguidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main).ToList();
                //会计凭证Main
                var PZguidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main).ToList();
                var PZNotguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main).ToList();
                var PZNotdetailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main).ToList();
                var mainList = o.AsEnumerable().Select(e => new
                {
                    e.GUID,
                    e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    e.DepartmentName,
                    e.PersonName,
                    e.BillCount,
                    e.Total_BX,
                    e.DocMemo,
                    e.YWTypeName,
                    e.DocTypeName,
                    e.MakeDate,
                    ApproveStatus = (e.DocState == "" || e.DocState == "0") ? "未审核" : ((e.DocState == "-1" || e.DocState == "999") ? "已审批" : "审批中"),
                    CheckStatus = (checkGuidList.Contains(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.Contains(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.Contains(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.Contains(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.Contains(e.GUID) || !PZNotguidList.Contains(e.GUID) ? "未生成凭证" : "未知"),
                    e.BGCodeName,
                    e.BGCodeKey
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total_BX >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total_BX <= EndTotal);
                    }
                }

                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_BX_Main into g
                              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.DocState });
                //支票领用主表
                var checkGuidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc).ToList();
                //现金需求关系表 
                var cashGuidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain).ToList();
                //核销Detail(付款状态应用)
                var HXguidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main).ToList();
                //会计凭证Main
                var PZguidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main).ToList();
                var PZNotguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main).ToList();
                var PZNotdetailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main).ToList();
                var mainList = o.AsEnumerable().Select(e => new
                {
                    e.GUID,
                    e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    e.DepartmentName,
                    e.PersonName,
                    e.BillCount,
                    e.Total_BX,
                    e.DocMemo,
                    e.YWTypeName,
                    e.DocTypeName,
                    e.MakeDate,
                    ApproveStatus = (e.DocState == "" || e.DocState == "0") ? "未审核" : ((e.DocState == "-1" || e.DocState == "999") ? "已审批" : "审批中"),
                    CheckStatus = (checkGuidList.Contains(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.Contains(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.Contains(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.Contains(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.Contains(e.GUID) || !PZNotguidList.Contains(e.GUID) ? "未生成凭证" : "未知")
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total_BX >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total_BX <= EndTotal);
                    }
                }

                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();
            }
            #endregion

        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid"></param>
        protected override void Delete(Guid guid)
        {
            BX_MainView model = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);
            if (model != null)
            {
                if (model.DocTypeUrl != null)
                {
                    var obj = CreatInstance(model.DocTypeUrl, OperatorId);
                    JsonModel jmodel = new JsonModel();
                    jmodel.m = model.Pick();
                    obj.Save(((int)Business.Common.EnumType.EnumOperateType.Delete).ToString(), jmodel);
                }
            }

        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="status">状态1表示新建 2表示修改 3表示删除</param>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>JsonModel</returns>
        public override JsonModel Save(string status, JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                Guid value = jsonModel.m.Id(new BX_Main().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 

                        break;
                    case "2": //修改

                        break;
                    case "3": //删除
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            this.Delete(value);
                        }
                        break;

                }
                if (string.IsNullOrEmpty(strMsg))
                {
                    if (status == "3")//删除时返回默认值
                    {
                        strMsg = "删除成功！";
                    }
                    else
                    {
                        result = this.Retrieve(value);
                        strMsg = "保存成功！";
                    }
                    OperatorLog.WriteLog(this.OperatorId, value, status, "报销单列表", data);
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                OperatorLog.WriteLog(this.OperatorId, "报销单列表", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="jsonModel">JsonModel</param>
        /// <param name="status">状态</param>
        /// <returns>string</returns>
        private string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            BX_Main main = null; ; //new BX_Main();
            switch (status)
            {
                case "1": //新建

                    break;
                case "2": //修改

                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new BX_Main().ModelName());
                    vResult = DeleteVerify(value);
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        strMsg = vResult.Validation[0].Message + "<br>";//"\n";
                    }
                    break;

            }
            return strMsg;
        }
        /// <summary>
        /// 数据从数据库删除前验证
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BX_Main bxMain = new BX_Main();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                //if (Common.ConvertFunction.TryParse(mModel.GUID.GetType(), mModel.GUID.ToString(), out g) == false)
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "报销单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
            //流程验证
            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此报销单正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
            }
            //作废的不能删除

            BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == "9")
                {
                    str = "此报销单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            return result;
        }

    }
}
