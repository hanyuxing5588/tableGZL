using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Platform.Flow.Run;
using Business.CommonModule;
using BusinessModel;
using Infrastructure;
using System.Data.Objects;
using Business.IBusiness;
namespace Business.Approval
{
    public class 待办任务:IBusiness.IAgencyTask
    {


        #region 单据启用流程
        public ResultMessager DocSumitWorkFlow(string scope, Guid userId, Guid docId, int classId)
        {
            try
            {
                Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
                var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
                iRunWork.SetTasks(listTasks);
                iRunWork.SetOperatorId(userId);
                //拿到任务
                //根据任务去找到具体执行的对象
                var message = iRunWork.CommitFlow(scope, userId, docId, classId);
                //审批中 审批完成 
                var docState = iRunWork.ProcessStatus != 1 ? EnumType.EnumDocState.Approving : EnumType.EnumDocState.Approved;
                UpdateDocState(docId, classId, docState);
                return message;
            }
            catch (Exception ex)
            {
                return new ResultMessager() {  Resulttype=1, Msg=ex.Message};
            }
        }
        private void UpdateDocState(Guid docId,int classId, EnumType.EnumDocState docState) 
        {
            var bContext = new BusinessEdmxEntities();
            var idocState= ((int)docState).ToString();
            switch (classId)
            {
                case 23:
                    var q = bContext.BX_Main.FirstOrDefault(e => e.GUID == docId);
                    q.DocState = idocState;
                    break;
                case 30:
                    var q1 = bContext.WL_Main.FirstOrDefault(e => e.GUID == docId);
                    q1.DocState = idocState;
                    break;
                case 32:
                     var q2 = bContext.SR_Main.FirstOrDefault(e => e.GUID == docId);
                     q2.DocState = idocState;
                    break;
                case 55:
                    var q3 = bContext.CN_CashMain.FirstOrDefault(e => e.GUID == docId);
                    q3.DocState = idocState;
                    break;
                case 106:
                    var q4 = bContext.SK_Main.FirstOrDefault(e => e.GUID == docId);
                    q4.DocState = idocState;
                    break;
                case 76:
                    var c = bContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == docId).Select(e=>e.GUID_BXMain);
                    var qt = bContext.BX_Main.FirstOrDefault(e => c.Contains(e.GUID));
                    qt.DocState = idocState;
                    break;
                case 48:
                    var q1t = bContext.JJ_Main.FirstOrDefault(e =>e.GUID==docId);
                    q1t.DocState = (int)docState ;
                    break;
                case 90:
                    //预算初始值
                    var bgdefault = bContext.BG_DefaultMain.FirstOrDefault(e => e.GUID == docId);
                    bgdefault.DocState = (int)docState;
                    break;
                case 62:
                    //预算编制
                    var bgmain = bContext.BG_Main.FirstOrDefault(e => e.GUID == docId);
                    bgmain.DocState = (int)docState;
                    break;
                case 94:
                    //预算分配
                    var bgAssign = bContext.BG_Assign.FirstOrDefault(e => e.GUID == docId);
                    bgAssign.DocState = (int)docState;
                    break;
                default:
                    //预算分配
                    var bg = bContext.BG_Assign.FirstOrDefault(e => e.GUID == docId);
                    bg.DocState = (int)docState;
                    break;
            }
            bContext.SaveChanges();
        }


        public ResultMessager DocSendBackWorkFlow(string scope, Guid userId, Guid docId, int classId)
        {
            try
            {
                var iRunWork = new Platform.Flow.Run.RunWorkFlow();
                iRunWork.SetOperatorId(userId);
                var url = iRunWork.GetCurrentNodeUrlByDocID(docId);
                var tempScope = scope;
                if (url == "hxcl")
                {
                    scope = "hxcl";
                }
                Platform.Flow.Run.ResultMessager message = null;
                if (classId == 62 || classId==90) //预算编制
                {
                    message = iRunWork.RejectFlow(scope, userId, docId, classId);
                }
                else
                {
                    message = iRunWork.SendBackFlow(scope, userId, docId, classId);
                }
                UpdateDocState(docId, classId, EnumType.EnumDocState.Approving);
                WorkFlowAPI.UpdateVariables(iRunWork.ProcessId, tempScope);
                return message;
            }
            catch (Exception ex)
            {
                return new ResultMessager() { Resulttype = 1, Msg = ex.Message };
            }
        }

        #endregion

       
        public bool OpenDocForWorkFlow(Guid processID, Guid processNodeID, int nodeLevel, int nodeType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IBusiness.AgencyDoc> GetDocData(Guid userId)
        {
            var docList = new List<AgencyDoc>();
            var bContext = new BusinessEdmxEntities();
            
            //报销
            var tempList= bContext.BX_MainView.Where(e => e.GUID_Modifier==userId&&(e.DocState==""||e.DocState=="6"))
                .Select(e=>new AgencyDoc{
                    DocId=e.GUID,
                    DocTypeName=e.DocTypeName,
                    Scope=e.DocTypeUrl,
                    DocNum=e.DocNum,
                    DocDate=e.DocDate,
                    CreatePerson=e.PersonName,
                    DWName=e.DWName,
                    DeptmentName=e.DepartmentName,
                    Remark=e.DocMemo,
                    ClassId=23
                }).ToList();
            docList.AddRange(tempList);
            //往来
            tempList = bContext.WL_MainView.Where(e => e.GUID_Modifier == userId && (e.DocState == "" || e.DocState == "6"))
              .Select(e => new AgencyDoc
              {
                  DocId = e.GUID,
                  DocTypeName = e.DocTypeName,
                  Scope = e.DocTypeUrl,
                  DocNum = e.DocNum,
                  DocDate = e.DocDate,
                  CreatePerson = e.PersonName,
                  DWName = e.DWName,
                  DeptmentName = e.DepartmentName,
                  Remark = e.DocMemo,
                  ClassId = 30
              }).ToList();
            docList.AddRange(tempList);



            return docList.Select(e => new AgencyDoc
            {

                DocId =e.DocId,
                DocTypeName = e.DocTypeName,
                Scope = e.Scope,
                DocNum = e.DocNum,
                StrDocDate = e.DocDate == null ? "" : ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                CreatePerson = e.CreatePerson,
                DWName = e.DWName,
                DeptmentName = e.DeptmentName,
                Remark = e.Remark,
                ClassId = e.ClassId
            });

        }

        public IEnumerable<IBusiness.FlowDataModel> GetFlowDataModel(string  YWKey, Guid DocTypeGuid,Guid UserId, string DocNum)
        {
            try
            {
                var flowDataList = new List<FlowDataModel>();
                var ageneyexList = WorkFlowAPI.GetFlowDataEx(UserId);
                var bContext = new BusinessEdmxEntities();
                var iContext = new BaseConfigEdmxEntities();
                foreach (AgencyTaskEx item in ageneyexList)
                {
                    FlowDataModel fdm = GetChangeModel(item);
                    try
                    {
                        if (GetDocDateInfo(ref fdm, bContext, YWKey, DocTypeGuid))
                        {
                            try
                            {

                           
                            if (fdm == null || string.IsNullOrEmpty(fdm.DocNum))
                            {
                                continue;
                            }
                            if (string.IsNullOrEmpty(DocNum) || fdm.DocNum.Contains(DocNum))
                            {
                                flowDataList.Add(fdm);
                            }
                            if ((DocNum+"").Length == 10 && flowDataList.Count > 1)
                            {
                                break;
                            }
                            }
                            
                            catch (Exception ex)
                            {

                                throw;
                            }
                        }
                    }
                    catch (Exception ex) { 
                    
                    }
                }
                return flowDataList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private FlowDataModel GetChangeModel(AgencyTaskEx atask) 
        {
            try
            {

           
            var flowData = new FlowDataModel();
            flowData.AcceptDate = atask.AcceptDate;
            flowData.AcceptDate = atask.AcceptDate;
            flowData.StrAcceptDate = atask.AcceptDate == null ? "" : ((DateTime)atask.AcceptDate).ToString("yyyy-MM-dd HH:mm:ss");
            flowData.NodeName = atask.NodeName;
            flowData.NodeLevel = atask.NodeLevel;
            flowData.WorkFlowName = atask.WorkFlowName;
            flowData.ProcessDate = atask.ProcessDate;
            flowData.StrProcessDate = atask.ProcessDate == null ? "" : ((DateTime)atask.ProcessDate).ToString("yyyy-MM-dd HH:mm:ss");
            flowData.ProcessID = atask.ProcessID;
            flowData.ProcessNodeID = atask.ProcessNodeID;
            flowData.NodeType = atask.NodeType;

            return flowData;
            }
            catch (Exception ex)
            {

                return null;
            }
        }
        private bool GetDocDateInfo(ref FlowDataModel fdm,BusinessEdmxEntities bContext,string ywKey,Guid docTypeGuid)
        {
            try
            {
            Guid docId; string url;
           

          
            var bIsSucess = WorkFlowAPI.GetDocIdAndUrl(fdm.ProcessID, out docId, out url);
            if (url == "hxcl")
            {
                ywKey = "hxcl";
            }
            else if (url == "gzd") { 
                //2016-2-22
                ywKey = "gzd";
            }
            else if (url == "zplq")
            {
                url = "zpsld";
                var ssDoc = bContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl == url);
                if (ssDoc == null)
                {
                    fdm = null;
                    return false;
                };
                ywKey = ssDoc.YWTypeKey;
                //docTypeName = ssDoc.DocTypeName;
            }
            else
                if (ywKey == "00")
                {
                    var yekey = bContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl == url);
                    if (yekey == null)
                    {
                        fdm = null;
                        return false;
                    }
                    ywKey = yekey.YWTypeKey;
                }
            //if (docTypeGuid == Guid.Empty) {
            //    docId = docTypeGuid;
            //}
            switch (ywKey)
            {
                case "gzd":
                    var dt=Business.Common.DataSource.ExecuteQuery("SELECT  SUM(ItemValue) FROM    dbo.SA_PlanActionDetail WHERE   GUID_PlanAction = '"+docId+"' AND GUID_Item IN ( SELECT   GUID FROM     dbo.SA_Item WHERE    ItemName = '实发合计' )");
                    var d = dt.Rows[0][0].ToString();
                    double dd = 0;
                    double.TryParse(d, out dd);
                    var m = bContext.SA_PlanActionView.FirstOrDefault(e => e.GUID == docId);
                    fdm.DocId = docId;
                    fdm.DocNum = m.DocNum;
                    fdm.CreatePerson = m.Maker;
                    fdm.DeptmentName = "";
                    fdm.SumMoney = dd;
                    fdm.ClassId = 53;
                    fdm.Scope = "gzd";
                    break;
                case "hxcl": 
                    GetDocInfoWithHXCL(ref  fdm, bContext, ywKey, docTypeGuid,docId);
                    break;
                case "1111"://公务卡汇总报销单
                    var entBxC = bContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == docId);
                    var detailIds = bContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == docId).Select(e => e.GUID_BXDetail);
                    var jeBxC = bContext.BX_Detail.Where(e => detailIds.Contains(e.GUID)).Sum(e => e.Total_BX);
                     fdm.DocId = docId;
                     fdm.DocNum = entBxC.DocNum;
                     fdm.CreatePerson = entBxC.PersonName;
                     fdm.DeptmentName = entBxC.DepartmentName;
                    fdm.SumMoney = jeBxC;
                    fdm.ClassId = 76;
                    fdm.Scope = entBxC.DocTypeUrl;
                    break;
                case "1112"://收款
                case "1101":
                    var entSk = bContext.SK_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    var jeSk = bContext.SK_MainView.Where(e => e.GUID == docId).Sum(e => e.Total_SK);
                     fdm.DocId = docId;
                     fdm.DocNum = entSk.DocNum;
                     fdm.CreatePerson = entSk.PersonName;
                     fdm.DeptmentName = entSk.DepartmentName;
                    fdm.SumMoney = jeSk;
                    fdm.ClassId = 106;
                    fdm.Scope = entSk.DocTypeUrl;
                    break;
                case "05":
                case "0501":
                case "0502"://个人往来                    var q = bContext.WL_MainView.Where(e => e.GUID == docId);
                    if(docTypeGuid!=Guid.Empty){
                        q=bContext.WL_MainView.Where(e => e.GUID == docId&&e.GUID_DocType == docTypeGuid);
                    }
                    var ent=q.FirstOrDefault();
                    if (ent == null)return false ;
                    var je = bContext.WL_Detail.Where(e => e.GUID_WL_Main == ent.GUID).Sum(e => e.Total_WL);
                    fdm.DocId = docId;
                    fdm.DocNum = ent.DocNum;
                    fdm.CreatePerson = ent.PersonName;
                    fdm.DeptmentName = ent.DepartmentName;
                    fdm.SumMoney = je;
                    fdm.ClassId = 30;
                    fdm.Scope = ent.DocTypeUrl;
                    break;
                case "02"://报销
                    var qq = bContext.BX_MainView.Where(e => e.GUID == docId);
                    if(docTypeGuid!=Guid.Empty){
                        qq = bContext.BX_MainView.Where(e => e.GUID == docId && e.GUID_DocType == docTypeGuid);
                    }
                    var entBx = qq.FirstOrDefault();
                    if (entBx == null) return false;
                    var jeBx = bContext.BX_Detail.Where(e => e.GUID_BX_Main == entBx.GUID).Sum(e => e.Total_BX);
                    fdm.DocId = docId;
                    fdm.DocNum = entBx.DocNum;
                    fdm.ClassId = 23;
                    fdm.CreatePerson = entBx.PersonName;
                    fdm.DeptmentName = entBx.DepartmentName;
                    fdm.SumMoney = jeBx;
                    fdm.Scope = entBx.DocTypeUrl;
                    break;
                case "01"://预算编制或预算初始值设置 差classId 和 单据ID
                    
                    if (url == "yscszsz")
                    {
                        BG_DefaultMainView entBG = null;
                        if (docTypeGuid != Guid.Empty)
                        {
                            var doctypeobj = bContext.SS_DocTypeView.Where(e => e.GUID == docTypeGuid).FirstOrDefault();
                            if (doctypeobj.DocTypeKey != "32" && doctypeobj.DocTypeKey != "33") return false; 
                            entBG = bContext.BG_DefaultMainView.Where(e => e.GUID == docId).FirstOrDefault();
                        }
                        else
                        {
                            entBG = bContext.BG_DefaultMainView.Where(e => e.GUID == docId).FirstOrDefault();
                        }
                        

                        if (entBG == null) return false;
                        var jeBGDetail = bContext.BG_DefaultMainView.Where(e => e.GUID == entBG.GUID).Sum(e => e.Total_BG);
                        fdm.DocId = docId;
                        fdm.ClassId = 90;
                        fdm.DocNum = entBG.DocNum;
                        fdm.CreatePerson = entBG.PersonName;
                        fdm.DeptmentName = entBG.DepartmentName;
                        fdm.SumMoney = jeBGDetail;
                        fdm.ProjectKey = entBG.ProjectKey;
                        fdm.ProjectName = entBG.ProjectName;
                        fdm.BGStepName = entBG.BGSetupName;
                        fdm.BGTypeName = entBG.BGTypeName;
                        fdm.Scope = url;
                    }
                    else
                    {
                        BG_MainView entBG = null;
                        if (docTypeGuid != Guid.Empty)
                        {
                            entBG = bContext.BG_MainView.Where(e => e.GUID == docId && e.GUID_DocType==docTypeGuid).FirstOrDefault();
                        }
                        else
                        {
                            entBG = bContext.BG_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                        }

                        if (entBG == null) return false;
                        var jeBGDetail = bContext.BG_Main.Where(e => e.GUID == entBG.GUID).Sum(e => e.Total_BG);
                        fdm.DocId = docId;
                        fdm.ClassId=62;
                        fdm.DocNum = entBG.DocNum;
                        fdm.CreatePerson = entBG.PersonName;
                        fdm.DeptmentName = entBG.DepartmentName;
                        fdm.SumMoney = jeBGDetail;
                        fdm.ProjectKey = entBG.ProjectKey;
                        fdm.ProjectName = entBG.ProjectName;
                        fdm.BGStepName = entBG.BGSetupName;
                        fdm.BGTypeName = entBG.BGTypeName;
                        fdm.Scope = entBG.DocTypeUrl;
                    }
                    
                    break;
                default:
                    return false;
            }
            return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        private void GetDocInfoWithHXCL(ref FlowDataModel fdm, BusinessEdmxEntities bContext, string ywKey, Guid docTypeGuid, Guid docId)
        {
            var entBxC = bContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == docId);
            if (entBxC != null)
            {
                var detailIds = bContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == docId).Select(e => e.GUID_BXDetail);
                var jeBxC = bContext.BX_Detail.Where(e => detailIds.Contains(e.GUID)).Sum(e => e.Total_BX);
                fdm.DocId = docId;
                fdm.DocNum = entBxC.DocNum;
                fdm.CreatePerson = entBxC.PersonName;
                fdm.DeptmentName = entBxC.DepartmentName;
                fdm.SumMoney = jeBxC;
                fdm.ClassId = 76;
                fdm.Scope = "gwkhzbxd";
                return ;
            }
            var entSk = bContext.SK_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entSk != null)
            {
                var jeSk = bContext.SK_MainView.Where(e => e.GUID == docId).Sum(e => e.Total_SK);
                fdm.DocId = docId;
                fdm.DocNum = entSk.DocNum;
                fdm.CreatePerson = entSk.PersonName;
                fdm.DeptmentName = entSk.DepartmentName;
                fdm.SumMoney = jeSk;
                fdm.ClassId = 106;
                fdm.Scope = "skpd";
                return ;
            }

            var q = bContext.WL_MainView.Where(e => e.GUID == docId);
            if (docTypeGuid != Guid.Empty)
            {
                q = bContext.WL_MainView.Where(e => e.GUID == docId && e.GUID_DocType == docTypeGuid);
            }
            var ent = q.FirstOrDefault();
            if (ent != null)
            {
                var je = bContext.WL_Detail.Where(e => e.GUID_WL_Main == ent.GUID).Sum(e => e.Total_WL);
                fdm.DocId = docId;
                fdm.DocNum = ent.DocNum;
                fdm.CreatePerson = ent.PersonName;
                fdm.DeptmentName = ent.DepartmentName;
                fdm.SumMoney = je;
                fdm.ClassId = 30;
                fdm.Scope = ent.DocTypeUrl;
                return;
            }

            var qq = bContext.BX_MainView.Where(e => e.GUID == docId);
            if (docTypeGuid != Guid.Empty)
            {
                qq = bContext.BX_MainView.Where(e => e.GUID == docId && e.GUID_DocType == docTypeGuid);
            }
            var entBx = qq.FirstOrDefault();
            if (entBx != null){
                var jeBx = bContext.BX_Detail.Where(e => e.GUID_BX_Main == entBx.GUID).Sum(e => e.Total_BX);
                fdm.DocId = docId;
                fdm.DocNum = entBx.DocNum;
                fdm.ClassId = 23;
                fdm.CreatePerson = entBx.PersonName;
                fdm.DeptmentName = entBx.DepartmentName;
                fdm.SumMoney = jeBx;
                fdm.Scope = entBx.DocTypeUrl;
                return;
            }
        }
    }

}
