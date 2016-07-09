using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using System.Data.Objects;
using BusinessModel;

namespace FlowTaskImplement
{

    public class 获取预算类型并设置流程操作员 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "201";
        }

        public string GetTaskName()
        {
            return "获取预算类型并设置流程操作员";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var context = paramsTask.FlowContext as InfrastructureEdmxEntities;
                //流程运行时
                var process = paramsTask.Process;
                

                //获取预算分配中的预算类型

                var workFlow =context.OAO_WorkFlow.FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);
                var processData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id);
                if (workFlow == null) return false;
                if (processData == null) return false;
                var bContext = new BusinessModel.BusinessEdmxEntities();
                var ysfpMain = bContext.BG_AssignView.FirstOrDefault(e => e.GUID == processData.DataId);
                var 预算类型= ysfpMain.BGTypeName;
                var 变量ID = context.OAO_WorkFlowProcessVariableView.FirstOrDefault(
                    e => e.WorkFlowProcessId == process.Id && e.Name == "预算类型").Id;

                var 变量 = context.OAO_WorkFlowProcessVariable.FirstOrDefault(e => e.Id == 变量ID);
                变量.VarValue = 预算类型;
                context.ModifyConfirm(变量);
                

                //给动态分配编制人和审批人 
                var 遍审人s = context.OAO_WorkFlowProcessRuntimeUsers.Where(e => e.WorkFlowProcessId == process.Id);
                foreach (var item in 遍审人s)
                {
                    context.DeleteConfirm(item);
                }


                List<OAO_WorkFlowProcessRuntimeUsers> rus = new List<OAO_WorkFlowProcessRuntimeUsers>();
                //审批人
                var sps = bContext.BG_Approver.Where(e => e.GUID_BG_Assign == ysfpMain.GUID).ToList();
                
                foreach (var item in sps)
                {
                    //获得对应的流程节点
                    var wn = context.OAO_WorkFlowNode.FirstOrDefault(e => e.WorkFlowId == process.WorkFlowId
                        && e.Name == item.Variable);
                    context.OAO_WorkFlowProcessRuntimeUsers.AddObject(new OAO_WorkFlowProcessRuntimeUsers
                    {
                         Id=Guid.NewGuid(),
                         ExecutorId=item.GUID_Operator,
                         ExecutorVersion=1,
                         WorkFlowProcessId=process.Id,
                         WorkFlowNodeId = wn.Id
                    });
                }
                
                
                //编制人
                var bzs = bContext.BG_Preparers.Where(e => e.GUID_BG_Assign == ysfpMain.GUID).ToList();

                foreach (var item in bzs)
                {
                    //获得对应的流程节点
                    var wn = context.OAO_WorkFlowNode.FirstOrDefault(e => e.WorkFlowId == process.WorkFlowId
                        && e.Name == item.Variable);
                    context.OAO_WorkFlowProcessRuntimeUsers.AddObject(new OAO_WorkFlowProcessRuntimeUsers
                    {
                        Id = Guid.NewGuid(),
                        ExecutorId = item.GUID_Operator,
                        ExecutorVersion = 1,
                        WorkFlowProcessId = process.Id,
                        WorkFlowNodeId = wn.Id
                    });
                }
            
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion

    }

    public class 预算分配生成编制初始值 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "202";
        }

        public string GetTaskName()
        {
            return "预算分配生成编制初始值";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var context = paramsTask.FlowContext as InfrastructureEdmxEntities;
                //流程运行时
                var process = paramsTask.Process;
                var curNode = paramsTask.CurrentNode;
                var level = paramsTask.CurrentNode.GetNodeLevel(context);
                
                //获取预算分配中的预算类型
                var workFlow = context.OAO_WorkFlow.FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);
                var processData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url == "ysfp");
                if (processData == null) return true;
                if (workFlow == null) return false;
                if (processData == null) return false;
                var bContext = new BusinessModel.BusinessEdmxEntities();
                var ysfpMain = bContext.BG_AssignView.FirstOrDefault(e => e.GUID == processData.DataId);
                var operatorId = paramsTask.OperatorId;
                var personId = bContext.GetDefaultPersonId(operatorId);

                BusinessModel.BG_DefaultMain dMain = new BG_DefaultMain();
                dMain.GUID_BGSetup = ysfpMain.GUID_BGSetUp;
                dMain.GUID_BG_Assign = ysfpMain.GUID;
                dMain.GUID_DW = ysfpMain.GUID_DW;
                dMain.GUID_Department = ysfpMain.GUID_Department;
                dMain.GUID_FunctionClass = ysfpMain.GUID_FunctionClass;
                dMain.GUID_Project = ysfpMain.GUID_Project;
                var doctype=bContext.SS_DocType.FirstOrDefault(e => e.DocTypeKey == "33");
                dMain.GUID_DocType = doctype.GUID;
                if (doctype.GUID_YWType!=null) dMain.GUID_YWType = (Guid)doctype.GUID_YWType;
                dMain.GUID_UIType = Guid.Parse("B2639101-7D4F-47B2-8ADF-AB24694E1828");
                dMain.GUID = Guid.NewGuid();
                dMain.DocDate = DateTime.Now;
                dMain.MakeDate = DateTime.Now;
                dMain.ModifyDate = DateTime.Now;
                dMain.GUID_Person = personId;
                dMain.GUID_Maker = paramsTask.OperatorId;
                dMain.GUID_Modifier = paramsTask.OperatorId;
                dMain.DocState = 0;
                dMain.DocVerson = "1";
                dMain.BGPeriod = 0;
                //总金额
                var stepId=ysfpMain.GUID_PStep==null || ysfpMain.GUID_PStep==Guid.Empty?" is null ":" ='" + ysfpMain.GUID_PStep.ToString()+ "' ";
                var projectid=ysfpMain.GUID_Project==null || ysfpMain.GUID_Project==Guid.Empty?" is null ":" ='" + ysfpMain.GUID_Project.ToString()+ "' ";
                if (ysfpMain.IsPStep == true)
                {
                    string sql = string.Format("select * from BG_DefaultDetail where guid_Bg_main in (" +
                        "select GUID from BG_DefaultMainView where GUID_BGStep{0} and GUID_DW='{1}' and GUID_Department='{2}'" +
                        " and GUID_Project{3})", stepId, ysfpMain.GUID_DW, ysfpMain.GUID_Department, projectid);
                    var defaults = bContext.ExecuteStoreQuery<BG_DefaultDetail>(sql).ToList();
                    if (defaults != null && defaults.Count > 0)
                    {
                        var curdefault = defaults.Find(e => e.BGYear == ysfpMain.BGYear);
                        if (curdefault != null) dMain.Total_BG = curdefault.Total_BG;
                    }
                }
                dMain.DocNum = MTaskTool.GetNextDocNum(bContext, (Guid)dMain.GUID_DW, dMain.GUID_YWType, dMain.DocDate.ToString());
                LoadBGDefaultDetails(dMain, ysfpMain.BGYear, bContext);
                bContext.BG_DefaultMain.AddObject(dMain);
                bContext.SaveChanges();
                //更新变量表 数据关联表

                paramsTask.Process.AddDataAndVariables(context, level, dMain.GUID, "yscszsz");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion

        private void LoadBGDefaultDetails(BG_DefaultMain BGDefaultMain, int BGYear, BusinessEdmxEntities context)
        {

            var listBG_SetupDetailView = context.BG_SetupDetailView.Where(e => e.GUID_BGSetup == BGDefaultMain.GUID_BGSetup).OrderBy(e => e.ItemOrder).ToList();
            var listBG_SetupBGCodeView = context.BG_SetupBGCodeView.Where(e => e.GUID_BGSetup == BGDefaultMain.GUID_BGSetup && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey).ToList();
            foreach (var BGCodeItem in listBG_SetupBGCodeView)
            {
                foreach (var SetupDetail in listBG_SetupDetailView)
                {
                    BG_DefaultDetail item = new BG_DefaultDetail();
                    item.GUID = Guid.NewGuid();
                    item.GUID_BG_Main = BGDefaultMain.GUID;
                    item.BGYear = BGYear;
                    item.BGMonth = 1;
                    item.BGMemo = "";
                    item.Total_BG = 0;
                    item.GUID_BGCode = BGCodeItem.GUID_BGCode;
                    item.GUID_Item = SetupDetail.GUID_Item;
                    context.BG_DefaultDetail.AddObject(item);
                }
            }
        }
    }

    public class 预算分配生成编制 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "203";
        }

        public string GetTaskName()
        {
            return "预算分配生成编制";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var sendNodeName = paramsTask.SenderNode.OAO_WorkFlowNode.Name;
                if (sendNodeName == "预算初始值") return true;
                
                
                var context = paramsTask.FlowContext as InfrastructureEdmxEntities;
                //流程运行时
                var process = paramsTask.Process;
                var level = paramsTask.CurrentNode.GetNodeLevel(context);

                //获取预算分配中的预算类型
                var workFlow = context.OAO_WorkFlow.FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);
                var processData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url=="ysfp");
                if (processData ==null) return true;
                
                if (workFlow == null) return false;
                if (processData == null) return false;
                var bContext = new BusinessModel.BusinessEdmxEntities();
                var ysfpMain = bContext.BG_AssignView.FirstOrDefault(e => e.GUID == processData.DataId);
                var operatorId = paramsTask.OperatorId;
                var personId = bContext.GetDefaultPersonId(operatorId);

                BusinessModel.BG_Main dMain = new BG_Main();
                dMain.GUID_BGSetup = ysfpMain.GUID_BGSetUp;
                dMain.GUID_BG_Assign = ysfpMain.GUID;
                dMain.GUID_DW = ysfpMain.GUID_DW;
                dMain.GUID_Department = ysfpMain.GUID_Department;
                dMain.GUID_FunctionClass = ysfpMain.GUID_FunctionClass;
                dMain.GUID_Project = ysfpMain.GUID_Project;
                
                var doctype = bContext.SS_DocType.FirstOrDefault(e => e.DocTypeKey == "30");
                dMain.GUID_DocType = doctype.GUID;
                if (doctype.GUID_YWType != null) dMain.GUID_YWType = (Guid)doctype.GUID_YWType;
                dMain.GUID_UIType = Guid.Parse("2726487D-5CE7-456B-89EE-87064DC94FCA");
                dMain.GUID = Guid.NewGuid();
                dMain.DocDate = DateTime.Now;
                dMain.MakeDate = DateTime.Now;
                dMain.ModifyDate = DateTime.Now;
                dMain.GUID_Person = personId;
                dMain.GUID_Maker = paramsTask.OperatorId;
                dMain.GUID_Modifier = paramsTask.OperatorId;
                dMain.DocState = 0;
                dMain.DocVerson = "1";
                dMain.BGPeriod = 0;
                dMain.Invalid = true;
                dMain.Total_BG = 0;
                dMain.Total_BG_CurYear = 0;
                dMain.Total_BG_PreYear = 0;
                //总金额
                var stepId = ysfpMain.GUID_PStep == null || ysfpMain.GUID_PStep == Guid.Empty ? " is null " : " ='" + ysfpMain.GUID_PStep.ToString() + "' ";
                var projectid = ysfpMain.GUID_Project == null || ysfpMain.GUID_Project == Guid.Empty ? " is null " : " ='" + ysfpMain.GUID_Project.ToString() + "' ";
                if (ysfpMain.IsPStep == true)
                {
                    string sql = string.Format("select * from BG_Detail where guid_Bg_main in (" +
                        "select GUID from BG_MainView where GUID_BGStep{0} and GUID_DW='{1}' and GUID_Department='{2}'" +
                        " and GUID_Project{3})", stepId, ysfpMain.GUID_DW, ysfpMain.GUID_Department, projectid);
                    var defaults = bContext.ExecuteStoreQuery<BG_DefaultDetail>(sql).ToList();
                    if (defaults != null && defaults.Count > 0)
                    {
                        var curdefault = defaults.Find(e => e.BGYear == ysfpMain.BGYear);
                        if (curdefault != null) dMain.Total_BG = curdefault.Total_BG;
                    }
                }
                dMain.DocNum = MTaskTool.GetNextDocNum(bContext, (Guid)dMain.GUID_DW, dMain.GUID_YWType, dMain.DocDate.ToString());
                LoadBGDetails(dMain, ysfpMain.BGYear, bContext);
                bContext.BG_Main.AddObject(dMain);
                bContext.SaveChanges();
                //更新变量表 数据关联表

                paramsTask.Process.AddDataAndVariables(context, level, dMain.GUID, "ysbz");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion

        private void LoadBGDetails(BG_Main BGMain,int BGYear,BusinessEdmxEntities context)
        {

            var listBG_SetupDetailView = context.BG_SetupDetailView.Where(e => e.GUID_BGSetup == BGMain.GUID_BGSetup).OrderBy(e => e.ItemOrder).ToList();
            var listBG_SetupBGCodeView = context.BG_SetupBGCodeView.Where(e => e.GUID_BGSetup == BGMain.GUID_BGSetup && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey).ToList();
            foreach (var BGCodeItem in listBG_SetupBGCodeView)
            {
                foreach (var SetupDetail in listBG_SetupDetailView)
                {
                    BG_Detail item = new BG_Detail();
                    item.GUID = Guid.NewGuid();
                    item.GUID_BG_Main = BGMain.GUID;
                    item.BGYear = BGYear;
                    item.BGMonth = 1;
                    item.BGMemo = "";
                    item.Total_BG = 0;
                    item.GUID_BGCode = BGCodeItem.GUID_BGCode;
                    item.GUID_Item = SetupDetail.GUID_Item;
                    context.BG_Detail.AddObject(item);
                }
            }
        }
    }

    public class 预算初始值生成预算 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "204";
        }

        public string GetTaskName()
        {
            return "预算初始值生成预算";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var context = paramsTask.FlowContext as InfrastructureEdmxEntities;
                //流程运行时
                var process = paramsTask.Process;
                var curNode = paramsTask.CurrentNode;
                var level = paramsTask.CurrentNode.GetNodeLevel(context);

                //获取预算分配中的预算类型
                var workFlow = context.OAO_WorkFlow.FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);
                var processData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url == "yscszsz");
                if (processData == null) return true;
                if (workFlow == null) return false;
                if (processData == null) return false;
                var bContext = new BusinessModel.BusinessEdmxEntities();
                var BGDefaultMain = bContext.BG_DefaultMainView.FirstOrDefault(e => e.GUID == processData.DataId);
                var operatorId = paramsTask.OperatorId;
                var personId = bContext.GetDefaultPersonId(operatorId);

                




                BusinessModel.BG_Main dMain = new BG_Main();
                dMain.GUID_BGSetup = BGDefaultMain.GUID_BGSetup;
                dMain.GUID_BG_Assign = BGDefaultMain.GUID_BG_Assign;
                dMain.GUID_DW = BGDefaultMain.GUID_DW;
                dMain.GUID_Department = BGDefaultMain.GUID_Department;
                dMain.GUID_FunctionClass = BGDefaultMain.GUID_FunctionClass;
                dMain.GUID_Project = BGDefaultMain.GUID_Project;
                dMain.Total_BG = BGDefaultMain.Total_BG;
                dMain.Total_BG_CurYear = BGDefaultMain.Total_BG_CurYear;
                dMain.Total_BG_PreYear = BGDefaultMain.Total_BG_PreYear;
                
                var doctype = bContext.SS_DocType.FirstOrDefault(e => e.DocTypeKey == "30");
                dMain.GUID_DocType = doctype.GUID;
                if (doctype.GUID_YWType != null) dMain.GUID_YWType = (Guid)doctype.GUID_YWType;
                dMain.GUID_UIType = Guid.Parse("2726487D-5CE7-456B-89EE-87064DC94FCA");
                dMain.GUID = Guid.NewGuid();
                dMain.DocDate = DateTime.Now;
                dMain.MakeDate = DateTime.Now;
                dMain.ModifyDate = DateTime.Now;
                dMain.GUID_Person =BGDefaultMain.GUID_Person==null?personId:BGDefaultMain.GUID_Person;
                dMain.GUID_Maker = paramsTask.OperatorId;
                dMain.GUID_Modifier = paramsTask.OperatorId;
                dMain.DocState = 0;
                dMain.DocVerson = "1";
                dMain.BGPeriod = 0;
                dMain.Invalid = true;


                dMain.DocNum = MTaskTool.GetNextDocNum(bContext, (Guid)dMain.GUID_DW, dMain.GUID_YWType, dMain.DocDate.ToString());
                bContext.BG_Main.AddObject(dMain);
                LoadBGDetails(BGDefaultMain, dMain.GUID, bContext);
                bContext.SaveChanges();
                //更新变量表 数据关联表

                paramsTask.Process.AddDataAndVariables(context, level + 1, dMain.GUID, "ysbz");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion

        private void LoadBGDetails(BG_DefaultMainView BGDefaultMain, Guid BGMainId, BusinessEdmxEntities context)
        {
            var BGDefaultDetails = context.BG_DefaultDetailView.Where(e => e.GUID_BG_Main == BGDefaultMain.GUID).ToList();
            foreach (var defaultitem in BGDefaultDetails)
            {
                BG_Detail item = new BG_Detail();
                item.GUID = Guid.NewGuid();
                item.GUID_BG_Main = BGMainId;
                item.BGYear = defaultitem.BGYear;
                item.BGMonth = defaultitem.BGMonth;
                item.BGMemo = defaultitem.BGMemo;
                item.Total_BG = defaultitem.Total_BG;
                item.GUID_BGCode = defaultitem.GUID_BGCode;
                item.GUID_Item = defaultitem.GUID_Item;
                context.BG_Detail.AddObject(item);
            }
            
        }
    }

    public class 预算拒绝 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "211";
        }

        public string GetTaskName()
        {
            return "预算拒绝";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var context = paramsTask.FlowContext as InfrastructureEdmxEntities;
                var targetNode =context.OAO_WorkFlowNode.FirstOrDefault(e=>e.Id==paramsTask.SenderNode.WorkFlowNodeId);
                //流程运行时
                var process = paramsTask.Process;
                if (targetNode == null) return true;
                string sql = string.Empty;
                if (targetNode.Name == "预算初始值")
                {
                    var ysbzData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url.ToLower() == "ysbz");
                    if (ysbzData != null)
                    {
                        sql += string.Format("delete BG_Detail where GUID_BG_Main='{0}' " +
                                              "delete BG_Main where GUID='{0}' " +
                                              "delete OAO_WorkFlowProcessData where DataId='{0}' ", ysbzData.Id);
                    }
                }
                else
                {
                    var ysbzData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url.ToLower() == "ysbz");
                    var yscszszData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.ProcessId == process.Id && e.Url.ToLower() == "yscszsz");
                    if (ysbzData != null)
                    {
                        sql += string.Format("delete BG_Detail where GUID_BG_Main='{0}' " +
                                              "delete BG_Main where GUID='{0}' " +
                                              "delete OAO_WorkFlowProcessData where DataId='{0}' ", ysbzData.Id);
                    }
                    if (yscszszData != null)
                    {
                        sql += string.Format("delete BG_DefaultDetail where GUID_BG_Main='{0}' " +
                                             "delete BG_DefaultMain where GUID='{0}' " +
                                             "delete OAO_WorkFlowProcessData where DataId='{0}' ", yscszszData.Id);
                    }
                }
                
                
                //删除生成的预算单据
                if (!string.IsNullOrEmpty(sql)) context.ExecuteStoreCommand("sql");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        #endregion
    }
}
