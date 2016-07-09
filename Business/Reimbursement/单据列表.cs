using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using Platform.Flow.Run;
using BusinessModel;
using Business.Casher;
namespace Business.Reimbursement
{

    public class 单据列表 : BaseDocument
    {
        public 单据列表() : base() { }
        public 单据列表(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 单据列表记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        public override List<object> History(SearchCondition conditions)
        {
            //1.首先判断单据类型
            //2.如果是业务类型，调用不同的方法查找数据
            //3.如果是单据类型Doctype，根据单据类型查找数据
            //4.
            List<djlbModel> list = new List<djlbModel>();
           // List<SS_DocType> doctypeList = new List<SS_DocType>();
            List<SS_YWType> ywtypeList = new List<SS_YWType>();
            ywtypeList = this.InfrastructureContext.SS_YWType.Where(e => e.IsStop == false).OrderBy(e => e.YWTypeKey).ToList();
           // doctypeList = this.InfrastructureContext.SS_DocType.Where(e => e.IsStop == false).OrderBy(e => e.DocTypeKey).ToList();
            if (conditions != null)
            {
                BillListConddition historyconditions = (BillListConddition)conditions;
                var treeList = historyconditions.TreeNodeList;
                bool isYWType = false;
                #region 过滤条件
                if (treeList != null && treeList.Count > 0)
                {
                    foreach (TreeNode item in treeList)
                    {
                        switch (item.treeModel.ToLower())
                        {
                            case "ss_ywtype"://单据类型 (树分类)
                                isYWType = true;
                                #region 单据类型
                                if (item.treeValue.IndexOf(",") >= 0)
                                {
                                    #region 选择多个单据
                                    string[] ids = item.treeValue.Split(',');
                                    List<string> idList = ids.ToList();
                                    for (int i = 0; i < idList.Count; i++)
                                    {
                                        if (!string.IsNullOrEmpty(idList[i]))
                                        {
                                            //单据类型获取数据
                                            var rList = djlbList(idList[i], ywtypeList, historyconditions);
                                            if (rList != null && rList.Count > 0)
                                            {
                                                list.AddRange(rList);
                                            }

                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    #region 选择一个单据
                                    if (!string.IsNullOrEmpty(item.treeValue))
                                    {
                                        //单据类型获取数据
                                        var rList = djlbList(item.treeValue, ywtypeList, historyconditions);
                                        if (rList != null && rList.Count > 0)
                                        {
                                            list.AddRange(rList);
                                        }

                                    }
                                    #endregion
                                }


                                #endregion
                                break;
                        }
                        //TreeNode最后一个节点没有单据类型，取所有的单据
                        if (item == treeList[treeList.Count - 1] && isYWType == false)
                        {
                            var allList = ALLdjlbList(ywtypeList, historyconditions);
                            if (allList != null && allList.Count > 0)
                            {
                                list.AddRange(allList);
                            }
                        }

                    }
                }
                #endregion
                else
                {
                    #region 查询条件
                    if (!string.IsNullOrEmpty(historyconditions.YWType) && historyconditions.YWType!=Guid.Empty.ToString())
                    {
                        Guid g;
                        if (Guid.TryParse(historyconditions.YWType, out g))
                        {
                            list = djlbList(historyconditions.YWType,ywtypeList,historyconditions);
                        }
                    }
                    else
                    {
                        //选择全部
                        list = ALLdjlbList(ywtypeList,historyconditions);
                    }
                    #endregion 
                }

            }
            return list.Distinct().ToList<object>();
        }
        /// <summary>
        /// 单据类型数据
        /// </summary>
        /// <param name="guid">业务类型GUID</param>
        /// <param name="ywtypeList">业务类型数据集</param>
        /// <param name="historyconditions">条件</param>
        /// <returns>djlbModel List</returns>
        private List<djlbModel> djlbList(string guid, List<SS_YWType> ywtypeList, BillListConddition historyconditions)
        {
            List<djlbModel> list = new List<djlbModel>();
            var ywNameList = ywtypeList.FindAll(e => e.GUID != null && e.GUID.ToString() == guid);
            if (ywNameList != null && ywNameList.Count > 0)
            {
                #region 根据不同的单据类型调用不同的方法
                //根据不同的单据类型调用不用的业务类型
                var ywTypeKey = ywNameList[0].YWTypeKey.Trim();
                switch (ywTypeKey)
                {
                    case Constant.YWTwo: //"报销管理":
                        var bx_List = BX_History(historyconditions);
                        if (bx_List != null && bx_List.Count > 0)
                        {
                            list = bx_List;
                        }
                        break;
                    case Constant.YWThree://"收入管理":
                    case Constant.YWElevenO://"收入信息流转":
                        var sr_list = SR_History(historyconditions);
                        if (sr_list != null && sr_list.Count > 0)
                        {
                            list=sr_list;
                        }
                        break;
                    case Constant.YWFour://"专用基金":
                        var jj_List = JJ_History(historyconditions);
                        if (jj_List != null && jj_List.Count > 0)
                        {
                            list = jj_List;
                        }
                        break;
                    case Constant.YWFiveO://"单位往来":
                    case Constant.YWFiveT://"个人往来":
                    case Constant.YWFive:// "往来管理":
                        var wl_List = WL_History(historyconditions);
                        if (wl_List != null && wl_List.Count > 0)
                        {
                            list=wl_List;
                        }
                        break;                   
                    case Constant.YWEightO://"收付款管理":
                        var cn_List = CN_History(historyconditions);
                        if (cn_List != null && cn_List.Count > 0)
                        {
                            list=cn_List;
                        }
                        break;
                    case Constant.YWEightT://"提存现管理":
                        var cash_List = Cash_History(historyconditions);
                        if (cash_List != null && cash_List.Count > 0)
                        {
                            list=cash_List;
                        }
                        break;
                    
                }
                #endregion
            }
            return list;

        }
        /// <summary>
        /// 所有单据数据
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="ywtypeList"></param>
        /// <param name="historyconditions"></param>
        /// <returns></returns>
        private List<djlbModel> ALLdjlbList(List<SS_YWType> ywtypeList, BillListConddition historyconditions)
        {
                     List<djlbModel> list = new List<djlbModel>();           
                    //"报销管理":
                        var bx_List = BX_History(historyconditions);
                        if (bx_List != null && bx_List.Count > 0)
                        {
                            list.AddRange(bx_List);
                        }                       
                    //"收入管理":
                   // "收入信息流转":
                        var sr_list = SR_History(historyconditions);
                        if (sr_list != null && sr_list.Count > 0)
                        {
                            list.AddRange(sr_list);
                        }                       
                   //"专用基金":

                    //   
                    //"单位往来":
                     //"个人往来":
                        var wl_List = WL_History(historyconditions);
                        if (wl_List != null && wl_List.Count > 0)
                        {
                            list.AddRange(wl_List);
                        }                      
                   
                   //"收付款管理":
                        var cn_List = CN_History(historyconditions);
                        if (cn_List != null && cn_List.Count > 0)
                        {
                            list.AddRange(cn_List);
                        }
                        
                    //"提存现管理":
                        var cash_List = Cash_History(historyconditions);
                        if (cash_List != null && cash_List.Count > 0)
                        {
                            list.AddRange(cash_List);
                        }
                       
                    //case "收入信息流转":
                   

               
            return list;

        }

        #region 列表数据
        /// <summary>
        /// 报销管理
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <return>object </return></returns>
        private List<djlbModel> BX_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<BX_MainView> main = this.BusinessContext.BX_MainView;
            IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();          

            if (historyconditions != null)
            {
                #region 审批状态
                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                        IQueryable<BX_MainView> mainTemp = null;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询单个数据时

                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<string> list = Constant.NotApproveState.ListState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                    var mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }

                                #endregion

                                #endregion
                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时
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
                                List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);                                
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);                              
                            }
                        }
                        #endregion
                        #endregion
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
                       main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                       main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion
                #endregion

                #region 提现状态
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 付款状态
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态
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
                #region 树条件筛选

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
                                    if (depguid != null && depguid.Count>0)
                                    {
                                        idsDep.AddRange(depguid);
                                    }
                                }
                                if (idsDep.Count > 0)
                                {
                                    main = main.Where(e => idsDep.Contains(e.GUID_Department));
                                   // detail = detail.Where(e => idsDep.Contains(e.GUID_Department));
                                }
                                break;
                            case "ss_dw":
                                 List<Guid> idsDW = new List<Guid>();
                                List<Guid> guidDW = treeNodeList.GetGUIDList(item);
                                foreach (Guid gdw in guidDW)
                                {
                                    SS_DW dw = new SS_DW();
                                    dw.GUID =gdw;
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {   
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出

                    }
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.ProjectPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeTwo);//项目支出
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
                             d.key.BGCodeKey,
                             m.DocTypeUrl, 
                             m.ApproveState
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
               
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID=e.GUID,
                    DocNum=e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount==null?0:(int)e.BillCount,
                    Total = e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState==Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus=e.DocState==((int)Business.Common.EnumType.EnumDocState.CancelState).ToString()?"已作废":"未作废", 
                    SubmitNotApprove=(oldList.Contains(e.DocState) || e.ApproveState==Constant.NewNotApproveState)?"未审批":"",//已经提交未审批
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    DocTypeUrl=e.DocTypeUrl,
                    ModelName = "BX_Main"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_BX_Main into g
                              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { 
                             m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName,m.DocTypeKey, m.DocTypeName, m.MakeDate, m.DocState,
                             m.DocTypeUrl,m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    DocTypeKey = e.DocTypeKey,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批
                    BGCodeName = "",
                    BGCodeKey = "",
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "BX_Main"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        }
        /// <summary>
        /// 收入管理
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <return>object </return></returns>
        private List<djlbModel> SR_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<SR_MainView> main = this.BusinessContext.SR_MainView;
            IQueryable<SR_DetailView> detail = this.BusinessContext.SR_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();

            if (historyconditions != null)
            {

                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                        IQueryable<SR_MainView> mainTemp = null;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询多个个数据时
                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<string> list = Constant.NotApproveState.ListState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                   var  mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                   if (mainTemp != null)
                                   {
                                       mainTemp.Union(mainList);
                                   }
                                   else
                                   {
                                       mainTemp = mainList;
                                   }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }   
                                }
                                
                                #endregion
                              
                                #endregion
                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时

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
                                List<string> list = Constant.ApprovedState.ListState();// "999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);                               
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"",0
                                List<string> ApprovedList = Constant.ApprovedState.ListState();//旧     "999","-1"                
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                
                            }
                        }
                        #endregion
                        #endregion
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
                       main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                       main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                 #region 提现状态
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 付款状态
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态
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
                #region 树条件筛选

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
                            detail = detail.Where(e => e.GUID_ProjectKey != null && projectUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => e.GUID_ProjectKey != null && bgcodeGUID.Contains((Guid)e.GUID_ProjectKey));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }
                #endregion

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
                                   // detail = detail.Where(e => idsDep.Contains(e.GUID_Department));
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
                                    detail = detail.Where(e => idsProject.Contains((Guid)e.GUID_ProjectKey));
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
                                    detail = detail.Where(e => idsProClass.Contains((Guid)e.GUID_ProjectKey));
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
                                    detail = detail.Where(e => e.GUID_BGCode != null && idsBgcode.Contains((Guid)e.GUID_BGCode));
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
                                    detail = detail.Where(e => idsFProject.Contains((Guid)e.GUID_ProjectKey));
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出"01"

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
                              group a by new { a.GUID_SR_Main, a.GUID_BGCode, a.BGCodeName, a.BGCodeKey } into g
                              select new { key = g.Key, Total_BX = g.Sum(a => a.Total_SR) };

                var o = (from d in dbdetai
                         join m in main on d.key.GUID_SR_Main equals m.GUID //into temp
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
                             d.key.BGCodeKey,
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName="SR_Main"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_SR_Main into g
                              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_SR) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, m.YWTypeName, 
                             m.DocTypeName, m.MakeDate, m.DocState,m.DocTypeUrl,m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX == null ? 0 : (double)e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批 
                    BGCodeName ="",
                    BGCodeKey ="",
                    DocTypeUrl=e.DocTypeUrl,
                    ModelName = "SR_Main"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        }
        /// <summary>
        /// 往来管理
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <return>object </return></returns>
        private List<djlbModel> WL_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<WL_MainView> main = this.BusinessContext.WL_MainView;
            IQueryable<WL_DetailView> detail = this.BusinessContext.WL_DetailView;
            IQueryable<WL_MainView> mainTemp = null;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();

            if (historyconditions != null)
            {

                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                       
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询单个数据时

                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<string> list = Constant.NotApproveState.ListState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                    var mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }

                                #endregion

                                #endregion
                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时
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
                                List<string> list = Constant.ApprovedState.ListState();// "999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"",0
                                List<string> ApprovedList = Constant.ApprovedState.ListState();//旧     "999","-1"                
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);

                            }
                        }
                        #endregion
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
                       main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                       main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 提现状态
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

               #region 付款状态
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态
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
                #region 树条件筛选

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
                            detail = detail.Where(e => projectGUID.Contains((Guid)e.GUID_ProjectKey));
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
                #endregion
                              
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
                                   // detail = detail.Where(e => idsDep.Contains(e.GUID_Department));
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
                                    detail = detail.Where(e => e.GUID_ProjectKey != null && idsProject.Contains((Guid)e.GUID_ProjectKey));
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
                                    detail = detail.Where(e => e.GUID_ProjectKey != null && idsProClass.Contains((Guid)e.GUID_ProjectKey));
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
                                    detail = detail.Where(e => e.GUID_BGCode != null && idsBgcode.Contains((Guid)e.GUID_BGCode));
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
                                    detail = detail.Where(e => e.GUID_ProjectKey != null && idsFProject.Contains((Guid)e.GUID_ProjectKey));
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出"01"

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
                              group a by new { a.GUID_WL_Main, a.GUID_BGCode, a.BGCodeName, a.BGCodeKey } into g
                              select new { key = g.Key, Total_WL = g.Sum(a => a.Total_WL) };
                var o = (from d in dbdetai
                         join m in main on d.key.GUID_WL_Main equals m.GUID //into temp
                         select new
                         {
                             m.GUID,
                             m.DocNum,
                             m.DocDate,
                             m.DepartmentName,
                             m.PersonName,
                             m.BillCount,
                             d.Total_WL,
                             m.DocMemo,
                             m.YWTypeName,
                             m.DocTypeName,
                             m.MakeDate,
                             m.DocState,
                             d.key.BGCodeName,
                             d.key.BGCodeKey,
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = string.IsNullOrEmpty(e.BillCount) ? 0 :int.Parse(e.BillCount),
                    Total = e.Total_WL == null ? 0 : (double)e.Total_WL,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批 
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "WL_Main"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_WL_Main into g
                              select new { GUID_BX_Main = g.Key, Total_WL = g.Sum(a => a.Total_WL) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_WL, m.DocMemo, 
                             m.YWTypeName, m.DocTypeName, m.MakeDate, m.DocState,m.DocTypeUrl,m.ApproveState 
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = string.IsNullOrEmpty(e.BillCount) ? 0 : int.Parse(e.BillCount),
                    Total = e.Total_WL == null ? 0 : (double)e.Total_WL,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批  
                    BGCodeName ="",
                    BGCodeKey ="",
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "WL_Main"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        }
        /// <summary>
        /// 收付款管理
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <return>djlbModel List </return></returns>
        private List<djlbModel> CN_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<CN_MainView> main = this.BusinessContext.CN_MainView;
            IQueryable<CN_DetailView> detail = this.BusinessContext.CN_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();

            if (historyconditions != null)
            {

                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                        IQueryable<CN_MainView> mainTemp = null;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询单个数据时

                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<string> list = Constant.NotApproveState.ListState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                    var mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }

                                #endregion

                                #endregion
                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时
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
                                List<string> list = Constant.ApprovedState.ListState();// "999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"",0
                                List<string> ApprovedList = Constant.ApprovedState.ListState();//旧     "999","-1"                
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);

                            }
                        }
                        #endregion
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
                       main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                       main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                 #region 提现状态
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 付款状态
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态
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
                #region 树条件筛选

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
                                //detail = detail.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                          
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
                                   // detail = detail.Where(e => e.GUID_Department != null && idsDep.Contains((Guid)e.GUID_Department));
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
                                //List<Guid> idsBgcode = new List<Guid>();
                                //List<Guid> guidBgcode = treeNodeList.GetGUIDList(item);
                                //foreach (Guid gBgcode in guidBgcode)
                                //{
                                //    SS_BGCode bgcodeModel = new SS_BGCode();
                                //    bgcodeModel.GUID = gBgcode;
                                //    bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                                //    var bgcodeGUID = bgcodeList.Select(e => e.GUID).ToList();
                                //    if (bgcodeGUID != null && bgcodeGUID.Count > 0)
                                //    {
                                //        idsBgcode.AddRange(bgcodeGUID);
                                //    }
                                //}
                                //detail = detail.Where(e => idsBgcode.Contains(e.GUID_BGCode));
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    //if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    //{
                    //    detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出"01"

                    //}
                    //if (historyconditions.BGType == ((int)EnumType.EnumBGType.ProjectPay).ToString())
                    //{
                    //    detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeTwo);//项目支出 "02"
                    //}
                }
                #endregion

            }
            #region 显示明细列表
            if (historyconditions != null && historyconditions.IsShowDetail)
            {
                #region 显示科目名称与编码明细信息

                var dbdetai = from a in detail
                              group a by new { a.GUID_CN_Main} into g
                              select new { key = g.Key, Total_BX = g.Sum(a => a.Total_CN) };
                var o = (from d in dbdetai
                         join m in main on d.key.GUID_CN_Main equals m.GUID //into temp
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
                             BGCodeName="",//d.key.BGCodeName,
                             BGCodeKey="",//d.key.BGCodeKey
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount =e.BillCount==null? 0 : (int)e.BillCount,
                    Total = e.Total_BX == null ? 0 : (double)e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批  
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "CN_Main"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_CN_Main into g
                              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_CN) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, m.BillCount, d.Total_BX, m.DocMemo, 
                             m.YWTypeName, m.DocTypeName, m.MakeDate, m.DocState,m.DocTypeUrl,m.ApproveState 
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX == null ? 0 : (double)e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批   
                    BGCodeName = "",
                    BGCodeKey = "",
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "CN_Main"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        }
        /// <summary>
        /// 提存现管理
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns>djlbModel List</returns>
        private List<djlbModel> Cash_History(SearchCondition conditions)
        {             
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<CN_CashMainView> main = this.BusinessContext.CN_CashMainView;
            IQueryable<CN_CashDetailView> detail = this.BusinessContext.CN_CashDetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();          

            if (historyconditions != null)
            {
               
                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                        IQueryable<CN_CashMainView> mainTemp = null;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询单个数据时
                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<string> list = Constant.NotApproveState.ListState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<string> list = Constant.ApprovedState.ListState();//"999","-1"
                                    var mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"","0"
                                    List<string> ApprovedList = Constant.ApprovedState.ListState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }

                                #endregion
                                #endregion
                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时
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
                                List<string> list = Constant.ApprovedState.ListState();// "999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<string> NotApproveList = Constant.NotApproveState.ListState();//旧 //"",0
                                List<string> ApprovedList = Constant.ApprovedState.ListState();//旧     "999","-1"                
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);

                            }
                        }
                        #endregion
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
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 付款状态
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main=main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                #endregion

                #region 凭证状态
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
                #region 树条件筛选

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
                            main = main.Where(e =>e.GUID_Department!=null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e =>e.GUID_Department!=null && depguid.Contains((Guid)e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e =>e.GUID_DW!=null && dwguid.Contains((Guid)e.GUID_DW));
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
                                    main = main.Where(e => e.GUID_Person != null && guidList.Contains((Guid)e.GUID_Person));
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
                                    if (depguid != null && depguid.Count>0)
                                    {
                                        idsDep.AddRange(depguid);
                                    }
                                }
                                if (idsDep.Count > 0)
                                {
                                    main = main.Where(e => e.GUID_Department != null && idsDep.Contains((Guid)e.GUID_Department));
                                   // detail = detail.Where(e => e.GUID_Department != null && idsDep.Contains((Guid)e.GUID_Department));
                                }
                                break;
                            case "ss_dw":
                                 List<Guid> idsDW = new List<Guid>();
                                List<Guid> guidDW = treeNodeList.GetGUIDList(item);
                                foreach (Guid gdw in guidDW)
                                {
                                    SS_DW dw = new SS_DW();
                                    dw.GUID =gdw;
                                    dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                                    var dwguid = dwList.Select(e => e.GUID).ToList();
                                    if (dwguid != null && dwguid.Count > 0)
                                    {
                                        idsDW.AddRange(dwguid);
                                    }
                                }
                                if (idsDW.Count > 0)
                                {
                                    main = main.Where(e => e.GUID_DW != null && idsDW.Contains((Guid)e.GUID_DW));
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
                                //List<Guid> idsBgcode = new List<Guid>();
                                //List<Guid> guidBgcode = treeNodeList.GetGUIDList(item);
                                //foreach (Guid gBgcode in guidBgcode)
                                //{
                                //    SS_BGCode bgcodeModel = new SS_BGCode();
                                //    bgcodeModel.GUID = gBgcode;
                                //    bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                                //    var bgcodeGUID = bgcodeList.Select(e => e.GUID).ToList();
                                //    if (bgcodeGUID != null && bgcodeGUID.Count > 0)
                                //    {
                                //        idsBgcode.AddRange(bgcodeGUID);
                                //    }
                                //}
                                //detail = detail.Where(e => idsBgcode.Contains(e.GUID_BGCode));
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    //if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    //{
                    //    detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出"01"

                    //}
                    //if (historyconditions.BGType == ((int)EnumType.EnumBGType.ProjectPay).ToString())
                    //{
                    //    detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeTwo);//项目支出 "02"
                    //}
                }
                #endregion

            }
            #region 显示明细列表
            if (historyconditions != null && historyconditions.IsShowDetail)
            {
                #region 显示科目名称与编码明细信息

                var dbdetai = from a in detail
                              group a by new { a.GUID_CN_CashMain } into g//, a.GUID_BGCode, a.BGCodeName, a.BGCodeKey
                              select new { key = g.Key, Total_Cash = g.Sum(a => a.Total_Cash) };
                var o = (from d in dbdetai
                         join m in main on d.key.GUID_CN_CashMain equals m.GUID //into temp
                         select new
                         {
                             m.GUID,
                             m.DocNum,
                             m.DocDate,
                             m.DepartmentName,
                             m.PersonName,
                             BillCount=0,//m.BillCount,
                             d.Total_Cash,
                             m.DocMemo,
                             m.YWTypeName,
                             m.DocTypeName,
                             m.MakeDate,
                             m.DocState,
                             BGCodeName="",// d.key.BGCodeName,
                             BGCodeKey="",//d.key.BGCodeKey
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_Cash == null ? 0 : (double)e.Total_Cash,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批  
                    BGCodeName= e.BGCodeName,
                    BGCodeKey=e.BGCodeKey,
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "CN_CashMain"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_CN_CashMain into g
                              select new { GUID_BX_Main = g.Key, Total_Cash = g.Sum(a => a.Total_Cash) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
                         select new { m.GUID, m.DocNum, m.DocDate, m.DepartmentName, m.PersonName, BillCount = 0, d.Total_Cash, m.DocMemo, 
                             m.YWTypeName, m.DocTypeName, m.MakeDate, m.DocState ,m.DocTypeUrl,m.ApproveState
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
                //未审批
                List<string> oldList = Constant.NotApproveState.ListState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<string> approvedList = Constant.ApprovedState.ListState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate.ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_Cash == null ? 0 : (double)e.Total_Cash,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate.ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == ((int)Business.Common.EnumType.EnumDocState.CancelState).ToString() ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批  
                    BGCodeName ="",
                    BGCodeKey ="",
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "CN_CashMain"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        
        }

        /// <summary>
        /// 专用基金
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <return>object </return></returns>
        private List<djlbModel> JJ_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            BillListConddition historyconditions = (BillListConddition)conditions;
            IQueryable<JJ_MainView> main = this.BusinessContext.JJ_MainView;
            IQueryable<JJ_DetailView> detail = this.BusinessContext.JJ_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();

            if (historyconditions != null)
            {
               
                #region 审批状态条件

                #region 审批状态
                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    //审批状态为多个时，拆分字符串，查询数据
                    if (historyconditions.ApproveStatus.IndexOf(",") >= 0)
                    {
                        #region ID为多个拆分字符串
                        string[] ids = historyconditions.ApproveStatus.Split(',');
                        List<string> idList = ids.ToList();
                        IQueryable<JJ_MainView> mainTemp = null;
                        for (int i = 0; i < idList.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(idList[i]))
                            {
                                #region 查询单个数据时

                                #region 审批状态

                                if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                                {
                                    List<int?> list = Constant.NotApproveState.ListIntState(); //"",0
                                    List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件                                   
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)));
                                    }
                                    else
                                    {
                                        mainTemp = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                                {
                                    List<int?> list = Constant.ApprovedState.ListIntState();//"999","-1"
                                    var mainList = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }
                                else if (idList[i] == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                                {
                                    List<int?> NotApproveList = Constant.NotApproveState.ListIntState();//旧 //"","0"
                                    List<int?> ApprovedList = Constant.ApprovedState.ListIntState();//旧   //"999","-1"                  
                                    var mainList = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);
                                    if (mainTemp != null)
                                    {
                                        mainTemp.Union(mainList);
                                    }
                                    else
                                    {
                                        mainTemp = mainList;
                                    }
                                }

                                #endregion

                            }
                        }
                        if (mainTemp != null)
                        {
                            main = mainTemp;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 查询单个数据时
                        if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                        {
                            if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                            {
                                List<int?> list = Constant.NotApproveState.ListIntState(); //"",0
                                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                                main = main.Where(e => list.Contains(e.DocState) || notApproveList.Contains(e.ApproveState));
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                            {
                                List<int?> list = Constant.ApprovedState.ListIntState();// "999","-1"
                                main = main.Where(e => list.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState);
                            }
                            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approving).ToString())//审批中
                            {
                                List<int?> NotApproveList = Constant.NotApproveState.ListIntState();//旧 //"",0
                                List<int?> ApprovedList = Constant.ApprovedState.ListIntState();//旧     "999","-1"                
                                main = main.Where(e => (!ApprovedList.Contains(e.DocState) && !NotApproveList.Contains(e.DocState)) || e.ApproveState == Constant.NewApprovingState);

                            }
                        }
                        #endregion
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
                #endregion

                #region 作废状态
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != 9);
                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == 9);
                    }
                }
                #endregion

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
                #region 树条件筛选

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
                                    // detail = detail.Where(e => idsDep.Contains(e.GUID_Department));
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
                                //List<Guid> idsBgcode = new List<Guid>();
                                //List<Guid> guidBgcode = treeNodeList.GetGUIDList(item);
                                //foreach (Guid gBgcode in guidBgcode)
                                //{
                                //    SS_BGCode bgcodeModel = new SS_BGCode();
                                //    bgcodeModel.GUID = gBgcode;
                                //    bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                                //    var bgcodeGUID = bgcodeList.Select(e => e.GUID).ToList();
                                //    if (bgcodeGUID != null && bgcodeGUID.Count > 0)
                                //    {
                                //        idsBgcode.AddRange(bgcodeGUID);
                                //    }
                                //}
                                //if (idsBgcode.Count > 0)
                                //{
                                //    detail = detail.Where(e => idsBgcode.Contains(e.GUID_BGCode));
                                //}
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

                #region  预算类型
                if (!string.IsNullOrEmpty(historyconditions.BGType))
                {
                    if (historyconditions.BGType == ((int)EnumType.EnumBGType.BasicPay).ToString())
                    {
                        detail = detail.Where(e => e.BGTypeKey == Constant.BGTypeOne); //基本支出"01"

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
                              group a by new { a.GUID_JJ_Main} into g
                              select new { key = g.Key, Total_BX = g.Sum(a => a.Total_JJ) };
                var o = (from d in dbdetai
                         join m in main on d.key.GUID_JJ_Main equals m.GUID //into temp
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
                             BGCodeName="",
                             BGCodeKey="",
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<int?> oldList = Constant.NotApproveState.ListIntState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<int?> approvedList = Constant.ApprovedState.ListIntState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate =e.DocDate==null?"":((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX == null ? 0 : (double)e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate == null ? "" : ((DateTime)e.MakeDate).ToString("yyyy-MM-dd"),                    
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == (int)Business.Common.EnumType.EnumDocState.CancelState ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批 
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "JJ_Main"
                });
                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            else
            {
                #region 明细信息
                var dbdetai = from a in detail
                              group a by a.GUID_JJ_Main into g
                              select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_JJ) };
                var o = (from d in dbdetai
                         join m in main on d.GUID_BX_Main equals m.GUID //into temp
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
                             m.DocTypeUrl,
                             m.ApproveState
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
                //未审批
                List<int?> oldList = Constant.NotApproveState.ListIntState(); //"",0
                List<string> notApproveList = Constant.ListNotApproveState();//新系统中的审批条件
                //已审核
                List<int?> approvedList = Constant.ApprovedState.ListIntState();//"-1","999"
                var mainList = o.AsEnumerable().Select(e => new djlbModel
                {
                    GUID = e.GUID,
                    DocNum = e.DocNum,
                    DocDate = e.DocDate == null ? "" : ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                    DepartmentName = e.DepartmentName,
                    PersonName = e.PersonName,
                    BillCount = e.BillCount == null ? 0 : (int)e.BillCount,
                    Total = e.Total_BX == null ? 0 : (double)e.Total_BX,
                    DocMemo = CommonFuntion.StringToJson(e.DocMemo),
                    YWTypeName = e.YWTypeName,
                    DocTypeName = e.DocTypeName,
                    MakeDate = e.MakeDate == null ? "" : ((DateTime)e.MakeDate).ToString("yyyy-MM-dd"),
                    ApproveStatus = (oldList.Contains(e.DocState) || notApproveList.Contains(e.ApproveState)) ? "未审核" : ((approvedList.Contains(e.DocState) || e.ApproveState == Constant.NewApprovedState) ? "已审核" : "审核中"),
                    CheckStatus = (checkGuidList.ContainsGUID(e.GUID) ? "已领取" : "未领取"),
                    WithdrawStatus = (cashGuidList.ContainsGUID(e.GUID) ? "已提现" : "未提现"),
                    PayStatus = (HXguidList.ContainsGUID(e.GUID) ? "已付款" : "未付款"),
                    CertificateStatus = PZguidList.ContainsGUID(e.GUID) ? "已生成凭证" : (PZNotdetailGuidList.ContainsGUID(e.GUID) || !PZNotguidList.ContainsGUID(e.GUID) ? "未生成凭证" : "未知"),
                    CancelStatus = e.DocState == (int)Business.Common.EnumType.EnumDocState.CancelState ? "已作废" : "未作废",
                    SubmitNotApprove = (oldList.Contains(e.DocState) || e.ApproveState == Constant.NewNotApproveState) ? "未审批" : "",//已经提交未审批 
                    BGCodeName ="",
                    BGCodeKey ="",
                    DocTypeUrl = e.DocTypeUrl,
                    ModelName = "JJ_Main"
                });

                #endregion
                #region 金额条件
                if (!string.IsNullOrEmpty(historyconditions.StartTotal))
                {
                    double StartTotal = 0;
                    if (double.TryParse(historyconditions.StartTotal, out StartTotal))
                    {
                        mainList = mainList.Where(e => e.Total >= StartTotal);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.EndTotal))
                {
                    double EndTotal = 0;
                    if (double.TryParse(historyconditions.EndTotal, out EndTotal))
                    {
                        mainList = mainList.Where(e => e.Total <= EndTotal);
                    }
                }
                #endregion
                return mainList.OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<djlbModel>();
            }
            #endregion
        }

        #endregion

        ///// <summary>
        ///// 删除
        ///// </summary>
        ///// <param name="guid"></param>
        //protected override void Delete(Guid guid)
        //{
        //    BX_MainView model = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);
        //    if (model != null)
        //    {
        //        if (model.DocTypeUrl != null)
        //        {
        //            var obj = CreatInstance(model.DocTypeUrl, OperatorId);
        //            JsonModel jmodel = new JsonModel();
        //            jmodel.m = model.Pick();
        //            obj.Save(((int)Business.Common.EnumType.EnumOperateType.Delete).ToString(), jmodel);
        //        }
        //    }

        //}
        /// <summary>
        /// 删除 （暂时没有用，前端控制调用不同模块中的Save保存方法，删除数据）
        /// </summary>
        /// <param name="guid"></param>
        protected void Delete(Guid guid, JsonModel jmodel,string docTypeUrl)
        {            
            if (!string.IsNullOrEmpty(docTypeUrl))
            {
                var obj = CreatInstance(docTypeUrl, OperatorId);                    
                obj.Save(((int)Business.Common.EnumType.EnumOperateType.Delete).ToString(), jmodel);
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
                string modelName = jsonModel.m.GetModelName();
                Guid value = jsonModel.m.Id(modelName);

                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 

                        break;
                    case "2": //修改
                         strMsg = DataVerify(jsonModel, status);
                        break;
                    case "3": //删除
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            //Delete(value,jsonModel);
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "单据列表", data);
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
                OperatorLog.WriteLog(this.OperatorId, "单据列表", ex.Message, data, false);
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
            switch (status)
            {
                case "1": //新建

                    break;
                case "2": //修改

                    break;
                case "3": //删除
                    
                    break;

            }
            return strMsg;
        }
        /// <summary>
        /// 验证信息
        /// </summary>
        /// <param name="vResult"></param>
        /// <returns></returns>
        private string DataVerifyMessage(VerifyResult vResult)
        {
            string strMsg = string.Empty;
            if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
            {
                for (int i = 0; i < vResult.Validation.Count; i++)
                {
                    strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                }
            }
            return strMsg;
        }
        /// <summary>
        /// 数据更新到数据库验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected  VerifyResult ModifyVerify(Guid mainGUID,string docType)
        {
            var docTypeModel = this.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => (e.IsStop == false || e.IsStop == null) && e.DocTypeUrl == docType);
            if (docTypeModel != null)
            {
                //#region 根据不同的单据类型调用不同的方法
                ////根据不同的单据类型调用不用的业务类型
                //var ywTypeKey = docTypeModel.YWTypeKey.Trim();
                //switch (ywTypeKey)
                //{
                //    case Constant.YWTwo: //"报销管理":
                        
                //        break;
                //    case Constant.YWThree://"收入管理":
                //    case Constant.YWElevenO://"收入信息流转":
                //        var sr_list = SR_History(historyconditions);
                //        if (sr_list != null && sr_list.Count > 0)
                //        {
                //            list = sr_list;
                //        }
                //        break;
                //    case Constant.YWFour://"专用基金":
                //        var jj_List = JJ_History(historyconditions);
                //        if (jj_List != null && jj_List.Count > 0)
                //        {
                //            list = jj_List;
                //        }
                //        break;
                //    case Constant.YWFiveO://"单位往来":
                //    case Constant.YWFiveT://"个人往来":
                //    case Constant.YWFive:// "往来管理":
                //        var wl_List = WL_History(historyconditions);
                //        if (wl_List != null && wl_List.Count > 0)
                //        {
                //            list = wl_List;
                //        }
                //        break;
                //    case Constant.YWEightO://"收付款管理":
                //        var cn_List = CN_History(historyconditions);
                //        if (cn_List != null && cn_List.Count > 0)
                //        {
                //            list = cn_List;
                //        }
                //        break;
                //    case Constant.YWEightT://"提存现管理":
                //        var cash_List = Cash_History(historyconditions);
                //        if (cash_List != null && cash_List.Count > 0)
                //        {
                //            list = cash_List;
                //        }
                //        break;

                //}
                //#endregion
            }

            ////验证结果
            //VerifyResult result = new VerifyResult();
            //BX_Main model = (BX_Main)data;
            //BX_Main orgModel = this.BusinessContext.BX_Main.Include("BX_Detail").FirstOrDefault(e => e.GUID == model.GUID);
            //if (orgModel != null)
            //{
            //    if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
            //    {
            //        List<ValidationResult> resultList = new List<ValidationResult>();
            //        resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
            //        result._validation = resultList;
            //        return result;
            //    }
            //}
            ////流程验证
            //
            //if (WorkFlowAPI.ExistProcess(model.GUID))
            //{
            //    List<ValidationResult> resultList = new List<ValidationResult>();
            //    resultList.Add(new ValidationResult("", "此报销单正在流程审核中，不能进行修改！"));
            //    result._validation = resultList;
            //    return result;
            //}
            ////作废           
            //if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
            //{
            //    List<ValidationResult> resultList = new List<ValidationResult>();
            //    resultList.Add(new ValidationResult("", "此报销单已经作废，不能进行修改！"));
            //    result._validation = resultList;
            //    return result;
            //}

           
            return null;
        }
        /// <summary>
        /// 报销验证
        /// </summary>
        /// <returns></returns>
        private List<ValidationResult> BX_Verify()
        {
            return null;
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

        //反核销
        public string FanHX(Guid billId, string docTypeKey)
        {
            var entDocType = this.BusinessContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeKey == docTypeKey);
            switch (entDocType.YWTypeKey)
            {
                case "0801"://出纳
                  return "此业务类型不能进行核销";
            }
            try
            {

           
            var hxDetial = this.BusinessContext.HX_Detail.FirstOrDefault(e => e.GUID_Main == billId);
            if (hxDetial == null) {
                return "请核销后在进行反核销";
            }
            var hxDetailList = this.BusinessContext.HX_Detail.Where(e => e.GUID_HX_Main == hxDetial.GUID_HX_Main).ToList();
            var cnMainList = hxDetailList.Where(e => e.ClassID_Main == 35 || e.ClassID_Main == 36).Select(e => e.GUID_Main);
            //删除出纳
            CN_Main main = this.BusinessContext.CN_Main.Include("CN_Detail").FirstOrDefault(e => cnMainList.Contains(e.GUID));
            if (main != null)
            {
                List<CN_Detail> details = new List<CN_Detail>();
                foreach (CN_Detail item in main.CN_Detail)
                {
                    details.Add(item);
                }
                foreach (CN_Detail item in details) { BusinessContext.DeleteConfirm(item); }
                BusinessContext.DeleteConfirm(main);
            }
         

            //删除凭证
            CW_PZMain pzmain = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID_HXMain ==hxDetial.GUID_HX_Main);
            if (pzmain != null)
            {
                List<CW_PZDetail> pzdetails = new List<CW_PZDetail>();
                foreach (CW_PZDetail item in pzmain.CW_PZDetail)
                {
                    pzdetails.Add(item);
                }
                foreach (CW_PZDetail item in pzdetails) { BusinessContext.DeleteConfirm(item); }
                BusinessContext.DeleteConfirm(pzmain);
            }
            try
            {

          
            //删除用有凭证
            CW_PZMainView pzView = this.BusinessContext.CW_PZMainView.FirstOrDefault(e => e.GUID_HXMain == hxDetial.GUID_HX_Main);     
            U8Certificate u8obj = new U8Certificate(this.BusinessContext);
            U8Result result1 = new U8Result();
            u8obj.Delete(pzView, ref result1);
            }
            catch (Exception ex)
            {

            }
            //核销删除
            HX_Main HXMain = this.BusinessContext.HX_Main.Include("HX_Detail").FirstOrDefault(e => e.GUID == hxDetial.GUID_HX_Main);

            List<HX_Detail> hxdetails = new List<HX_Detail>();

            foreach (HX_Detail item in HXMain.HX_Detail)
            {
                hxdetails.Add(item);
            }
            foreach (HX_Detail item in hxdetails) { BusinessContext.DeleteConfirm(item); }
            BusinessContext.DeleteConfirm(HXMain);
            this.BusinessContext.SaveChanges();
            return "反核销成功";
            }
            catch (Exception ex)
            {

                return ex.Message.ToString();
            }
        }  
    }
    /// <summary>
    /// 单据列表Model
    /// </summary>
    public class djlbModel
    { 
        /// <summary>
        /// 编号
        /// </summary>
        public Guid GUID { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public string DocNum {get; set;}
        /// <summary>
        /// 单据日期
        /// </summary>
        public string DocDate {get;set;}
        /// <summary>
        /// 部门名称
        /// </summary>
         public string DepartmentName{get;set;}
        /// <summary>
        /// 人员名称
        /// </summary>
        public string PersonName{get;set;}
        /// <summary>
        /// 附件数
        /// </summary>
        public int BillCount{get;set;}
        /// <summary>
        /// 金额
        /// </summary>
        public double Total {get;set;}
        /// <summary>
        /// 摘要
        /// </summary>    
        public string DocMemo{get;set;}  
        /// <summary>
        /// 业务名称
        /// </summary>
        public string YWTypeName{get;set;}
        /// <summary>
        /// 单据类型编码
        /// </summary>
        public string DocTypeKey{get;set;}
        /// 单据类型名称
        /// </summary>
        public string DocTypeName { get; set; }  
        /// <summary>
        /// 制单日期
        /// </summary>
        public string MakeDate {get;set;}
        /// <summary>
        /// 审批状态
        /// </summary>
        public string ApproveStatus{get;set;}
        /// <summary>
        /// 领取状态
        /// </summary>
        public string CheckStatus{get;set;}
        /// <summary>
        /// 提现状态
        /// </summary>
        public string WithdrawStatus {get;set;}
        /// <summary>
        /// 付款状态
        /// </summary>
        public string PayStatus{get;set;}
        /// <summary>
        /// 凭证状态
        /// </summary>
        public string CertificateStatus{get;set;}
        /// <summary>
        /// 科目名称
        /// </summary>
        public string BGCodeName{get;set;}
        /// <summary>
        /// 科目键
        /// </summary>
        public string BGCodeKey{get; set;}
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocTypeUrl { get; set; }
        /// <summary>
        /// 模型名称
        /// </summary>
        public string ModelName { get; set; }
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
