using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using System.Data.Objects;
using Infrastructure;
namespace Platform.Flow.Run
{
    public static class WorkFlowAPI
    {
        /// <summary>
        /// 根据流程运行时找到对应的url和单据Id
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="context"></param>
        /// <param name="docGUID"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool GetDocIdAndUrl(Guid processId, ObjectContext context, out Guid docGUID, out string url)
        {
            docGUID = Guid.Empty;
            url = "";
            var entProcess = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderByDescending(e => e.Flag).FirstOrDefault(e => e.ProcessId == processId);
            if (entProcess == null) return false;
            docGUID = (Guid)entProcess.DataId;
            url = entProcess.Url;
            return true;
        }
        public static bool GetDocIdAndUrl(Guid processId, out Guid docGUID, out string url)
        {
            docGUID = Guid.Empty;
            url = "";
            using (var context = DAL.DBFactory.CreateFlowDB())
            {
                var entProcess = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderByDescending(e => e.Flag).FirstOrDefault(e => e.ProcessId == processId);
                if (entProcess == null) return false;
                docGUID = (Guid)entProcess.DataId;
                url = entProcess.Url;
            }

            return true;
        }
        //保存新的processData;
        public static void SaveProcessData(Guid dataID,string preUrl, string url,Guid processId)
        {
            using (var context = DAL.DBFactory.CreateFlowDB())
            {
                var flag = context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e => e.ProcessId == processId).Max(e => e.Flag);
                int f = (int)flag + 1;
                //var ent = context.CreateObjectSet<OAO_WorkFlowProcessData>().CreateObject();
                //ent.DataId = dataID;
                //ent.Id = Guid.NewGuid();
                //ent.ProcessId = processId;
                //ent.Flag = flag + 1;
                //ent.Url = url;
                //context.CreateObjectSet<OAO_WorkFlowProcessData>().AddObject(ent);
                var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == processId);
                process.AddDataAndVariables(context, f, dataID, url);
                process.SetVariable(context, ProcessVariableProvider.internal_common, preUrl);
                context.SaveChanges();
            }
        }
        /// <summary>
        /// 根据单据ID 找到配置流程的当前节点


        /// </summary>
        /// <param name="docId">单据Id</param>
        /// <param name="error">返回的错误信息</param>
        /// <returns></returns>
        public static FlowNodeModel GetCurNodeByDocId(Guid docId, out string error)
        {
            error = "";
            try
            {
                var context = DAL.DBFactory.CreateFlowDB();
                var process = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.DataId == docId);
                if (process == null)
                {
                    error = "没有找到该单据的流程";//没启动或者没有对应流程



                    return null;
                }
                //var proNode = context.OAO_WorkFlowProcessNode.Where(e => e.WorkFlowProcessId == process.ProcessId);
                int processNodeState = (int)ProcessNodeStateEnum.已处理;
                var processNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
                    FirstOrDefault(e => e.WorkFlowProcessId == process.ProcessId && e.State != processNodeState);
                if (processNode == null)
                {
                    error = "找不到当前单据所对流程的节点";//没有运行时节点



                    return null;
                }
                var node = processNode.OAO_WorkFlowNode;
                var result=new FlowNodeModel(node.Name, node.Id, node.WorkFlowId, node.WorkFlowVersion);
                result.ProcessId = processNode.WorkFlowProcessId;
                result.ProcessNodeId = processNode.Id;
                result.NodeType = node.NodeType;
                return result;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                return null;
            }

        }

        //删除单据在流程中的所有信息 
        public static void DeleteProcessInfoByDocId(Guid docId) 
        {
            using (var context=DAL.DBFactory.CreateFlowDB())
            {
                var processData = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.DataId == docId);
                if (processData != null) {
                    var processId= processData.ProcessId;
                    //运行时的人
                    var executors = context.OAO_WorkFlowProcessNodeExecutor.Include("OAO_WorkFlowProcessNode")
                        .Where(e => e.OAO_WorkFlowProcessNode.WorkFlowProcessId == processId).ToList();
                    foreach (var item in executors)
                    {
                        context.OAO_WorkFlowProcessNodeExecutor.DeleteObject(item);
                    }
                    //流程变量
                    var variables = context.OAO_WorkFlowProcessVariable.Where(e => e.WorkFlowProcessId == processId).ToList();
                    foreach (var item in variables)
                    {
                        context.OAO_WorkFlowProcessVariable.DeleteObject(item);
                    }
                    //流程节点
                    var processNode = context.OAO_WorkFlowProcessNode.Where(e => e.WorkFlowProcessId == processId).ToList();
                    foreach (var item in processNode)
                    {
                        context.OAO_WorkFlowProcessNode.DeleteObject(item);
                    }
                    //退回记录
                    var backRecode = context.OAO_WorkFlowSendBackRecord.Where(e => e.ProcessId == processId).ToList();
                    foreach (var item in backRecode)
                    {
                        context.OAO_WorkFlowSendBackRecord.DeleteObject(item);
                    }
                    //流程数据
                    var process1Data = context.OAO_WorkFlowProcessData.Where(e => e.ProcessId == processId).ToList();
                    foreach (var item in process1Data)
                    {
                        context.OAO_WorkFlowProcessData.DeleteObject(item);
                    }
                }
                context.SaveChanges();
            }
        }
        /// <summary>
        /// 根据单据ID 和节点ID 返回该节点的状态


        /// </summary>
        /// <param name="docId">单据ID</param>
        /// <param name="nodeId">节点的ID</param>
        /// <param name="error">返回的错误信息</param>
        /// <returns>error：-1  right：0||1||2</returns>
        public static NodeRunState GetNodePassStatusByDocId(Guid docId, Guid nodeId, out string error)
        {
            error = "";
            try
            {
                var context = DAL.DBFactory.CreateFlowDB();
                var process = context.OAO_WorkFlowProcessData.FirstOrDefault(e => e.DataId == docId);
                if (process == null)
                {
                    error = "没有找到该单据的流程";//没启动或者没有对应流程


                    return NodeRunState.无状态;
                }
                var proNode = context.OAO_WorkFlowProcessNode.Where(e => e.WorkFlowProcessId == process.Id);
                var processNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
                    FirstOrDefault(e => e.WorkFlowProcessId == process.Id && e.OAO_WorkFlowNode.Id == nodeId);
                if (processNode == null)
                {
                    error = "找不到当前单据所对流程的节点";//没有运行时节点


                    return NodeRunState.无状态;
                }
                return (NodeRunState)processNode.State;
            }
            catch (Exception ex)
            {
                error = ex.Message.ToString();
                return NodeRunState.无状态;
            }

        }

        /// <summary>
        /// 保存预算单据的转化状态到流程中 (在预算)
        /// </summary>
        /// <param name="transBeforeDocId">单据转换前的单据ID </param>
        /// <param name="transAfterDocId">单据转换后的单据</param>
        /// <param name="bgTrans">是从什么单据到什么单据</param>
        public static bool SaveBGDocTransToProcess(Guid transBeforeDocId, Guid transAfterDocId, BGTransStatus bgTrans)
        {
            if (transAfterDocId == Guid.Empty || transAfterDocId == Guid.Empty || transAfterDocId == transBeforeDocId) return false;
            string url = "";//URl
            int falg = 0;//步骤的ID
            switch (bgTrans)
            {
                case BGTransStatus.预算分配到预算初始值:
                    url = "yscszsz";
                    falg = 6;//小一步 方便提交流程的时候 小于节点的nodeLevel并且是最近的一个

                    break;
                case BGTransStatus.预算分配到预算编制:
                case BGTransStatus.预算初始值到预算编制:
                    url = "ysbz";
                    falg = 8;//小一步 方便提交流程的时候 小于节点的nodeLevel并且是最近的一个

                    break;
                default:
                    break;
            }
            try
            {
                if (url == "" || falg == 0) return false;
                var context = DAL.DBFactory.CreateFlowDB();
                var processData = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.DataId == transBeforeDocId);
                if (processData != null)
                {
                    var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == processData.ProcessId);
                    process.AddDataAndVariables(context, falg, transAfterDocId, url);
                    process.SetVariable(context, ProcessVariableProvider.internal_common, url);
                    context.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Guid? GetExcutorByProcessNodeId(Guid ProcessNodeId)
        {
            var context = DAL.DBFactory.CreateFlowDB();
            var item = context.OAO_WorkFlowProcessNodeExecutor.FirstOrDefault(e => e.WorkFlowProcessNodeId == ProcessNodeId);
            if (item != null) return item.ExecutorId;
            return null;
        }

        /// <summary>
        /// 获得已经发布的流程信息

        /// </summary>
        /// <returns></returns>
        public static List<object> GetWorkFlowInfo()
        {
            var context = DAL.DBFactory.CreateFlowDB();
            return (from a in context.OAO_WorkFlow
                    where a.State == 1
                    select new
                    {
                        id = a.Id,
                        text = a.Name,
                        Version = a.Version
                    }).ToList().Select(e => new { id = e.id + "#" + e.Version, text = e.text }).ToList<object>();
        }

        public static List<object> GetDocTypeByFlowId(Guid id)
        {
            var context = DAL.DBFactory.CreateFlowDB();
            return context.OAO_WorkFlowDocType.Where(e => e.Guid_WorkFlow == id).Select(e => e.DocTypeUrl).ToList<object>();
        }
        /// <summary>
        /// 根据单据的作用域 返回对应配置流程的所有节点
        /// </summary>
        /// <param name="docTypeUrl">例如 ysfp(预算分配） </param>
        /// <returns>List<OAO_WorkFlowNode></returns>
        public static List<FlowNodeModel> GetNodeByWorkFlow(string docTypeUrl)
        {

            List<FlowNodeModel> listFlowNode = new List<FlowNodeModel>();
            try
            {
                var context = DAL.DBFactory.CreateFlowDB();
                var docTypeEnt = context.CreateObjectSet<OAO_WorkFlowDocType>().OrderByDescending(e => e.WorkFlowVersion).FirstOrDefault(e => e.DocTypeUrl == docTypeUrl);
                if (docTypeEnt == null)
                {
                    return null;
                }
                else
                {
                    listFlowNode = context.CreateObjectSet<OAO_WorkFlowNode>().Where(e => e.WorkFlowId == docTypeEnt.Guid_WorkFlow
                        && e.WorkFlowVersion == docTypeEnt.WorkFlowVersion && e.IsRuntimeUser == true
                        && (e.NodeType != 5 || e.NodeType != 6)).OrderBy(e => e.NodeLevel).Select(e => new FlowNodeModel
                        {
                            WorkFlowId = e.WorkFlowId,
                            WorkFlowNodeName = e.Name,
                            WorkFlowVersion = e.WorkFlowVersion,
                            WorkFlowNodeId = e.Id,
                            Sort = e.NodeLevel


                        }).ToList<FlowNodeModel>();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return listFlowNode;
        }
        public static bool SaveDocFlow(Guid id, int version, string[] url)
        {
            try
            {
                var context = DAL.DBFactory.CreateFlowDB();
                var ents = context.OAO_WorkFlowDocType.Where(e => e.Guid_WorkFlow == id && e.WorkFlowVersion == version).ToList();
                foreach (var item in ents)
                {
                    context.OAO_WorkFlowDocType.DeleteObject(item);
                }
                for (int i = 0; i < url.Length; i++)
                {

                    var ent = context.OAO_WorkFlowDocType.CreateObject();
                    ent.Guid = Guid.NewGuid();
                    ent.Guid_WorkFlow = id;
                    ent.DocTypeUrl = url[i];
                    ent.WorkFlowVersion = version;
                    context.OAO_WorkFlowDocType.AddObject(ent);
                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region 待办流程
        public static List<AgencyTask> GetFlowData(Guid executorId)
        {
            int state = (int)ProcessNodeStateEnum.已处理;
            int stateWY = (int)ProcessNodeExecutorStateEnum.未阅;
            int stateYY = (int)ProcessNodeExecutorStateEnum.已阅;
            using (var context = DAL.DBFactory.CreateFlowDB())
            {
                var executors = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().
                    Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode")
                    .Where(e => e.ExecutorId == executorId && (e.State == stateWY || e.State == stateYY) && e.OAO_WorkFlowProcessNode.State != state)
                    .OrderByDescending(e => e.ReciveDate);
                return executors.Select(e => new AgencyTask
                {
                    ProcessID = e.OAO_WorkFlowProcessNode.WorkFlowProcessId,
                    ProcessNodeID = e.OAO_WorkFlowProcessNode.Id,
                    AcceptDate = e.OAO_WorkFlowProcessNode.ReciveDate,
                    NodeName = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.Name,
                    NodeLevel = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeLevel,
                    NodeType = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeType
                }).ToList();
            }
        }
        public static List<AgencyTaskEx> GetFlowDataEx(Guid executorId)
        {
            int state = (int)ProcessNodeStateEnum.已处理;
            int stateWY = (int)ProcessNodeExecutorStateEnum.未阅;
            int stateYY = (int)ProcessNodeExecutorStateEnum.已阅;
            using (var context = DAL.DBFactory.CreateFlowDB())
            {
                var executors = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().
                    Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode.OAO_WorkFlow")
                    .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowProcess")
                    .Where(e => e.ExecutorId == executorId && (e.State == stateWY || e.State == stateYY) && e.OAO_WorkFlowProcessNode.State != state)
                    .OrderByDescending(e => e.ReciveDate);
                return executors.Select(e => new AgencyTaskEx
                {
                    ProcessID = e.OAO_WorkFlowProcessNode.WorkFlowProcessId,
                    ProcessNodeID = e.OAO_WorkFlowProcessNode.Id,
                    WorkFlowName = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.OAO_WorkFlow.Name,
                    ProcessDate = e.OAO_WorkFlowProcessNode.OAO_WorkFlowProcess.CreateDate,
                    AcceptDate = e.OAO_WorkFlowProcessNode.ReciveDate,
                    NodeName = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.Name,
                    NodeLevel = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeLevel,
                    NodeType = e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeType
                }).ToList();
            }
        }
        //给单据的流程办理信息   
        public static List<PassNodeData> GetProccessDoed(Guid docDocGuid, out ResultMessager message)
        {
            List<PassNodeData> listPND = new List<PassNodeData>();
            message = new ResultMessager();
            message.Title = "提示";
            message.Icon = MessagerIconEnum.info;
            //根据单据ID 找到流程运行时


            using (var context = DAL.DBFactory.CreateFlowDB())
            {

                var processIdList = context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e => e.DataId == docDocGuid).Select(e => e.ProcessId).Distinct().ToList();
                if (processIdList.Count < 1)
                {
                    message.Msg = "没有流程办理信息";
                    return listPND;
                }
                var c = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>()
                    .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowProcess")
                    .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode")
                    .Where(e => processIdList.Contains(e.OAO_WorkFlowProcessNode.WorkFlowProcessId))
                    .OrderBy(e => e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeLevel).Select(e => new
                    {
                        e.OAO_WorkFlowProcessNode.Id,
                        e.ExecutorId,
                        e.ReciveDate,
                        e.OAO_WorkFlowProcessNode.SendDate,
                        e.Comment,
                        exeState = e.State,
                        e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.Name,
                        e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.NodeLevel,
                        e.OAO_WorkFlowProcessNode.OAO_WorkFlowNode.Strategy,
                        e.OAO_WorkFlowProcessNode.State
                    }).ToList();
                if (c.Count <= 0)
                {
                    message.Msg = "没有流程办理信息";
                    return listPND;
                }
                List<Guid> listGuid = c.Select(e => e.ExecutorId).ToList();
                BaseConfigEdmxEntities s = new BaseConfigEdmxEntities();
                var personList = s.SS_Operator.Where(e => listGuid.Contains(e.GUID)).Select(e => new { e.GUID, e.OperatorName }).ToList();

                //节点
                Dictionary<Guid, int> dicNodeToPND = new Dictionary<Guid, int>();
                foreach (var item in c)
                {
                    if (dicNodeToPND.ContainsKey(item.Id))
                    {
                        int v = dicNodeToPND[item.Id];
                        var temp = personList.FirstOrDefault(e => e.GUID == item.ExecutorId);
                        if (temp == null) continue;
                        string personName = temp.OperatorName;
                        if ((ProcessNodeStateEnum)item.State != ProcessNodeStateEnum.已处理)
                        {
                            if (item.Strategy == 0)
                            {
                                listPND[v].passPersons += "," + personName;
                            }
                            else
                            {
                                string status = (ProcessNodeExecutorStateEnum)item.exeState == ProcessNodeExecutorStateEnum.办理 ? "" : "(未办理)";
                                listPND[v].passPersons += "," + personName + status;
                            }
                            continue;
                        }
                        //gcy 待修改 hanyx todo
                        if ((item.exeState == (int)ProcessNodeExecutorStateEnum.未阅 && item.State == (int)ProcessNodeExecutorStateEnum.办理) || item.exeState == (int)ProcessNodeExecutorStateEnum.办理)
                        {
                            var pnd = listPND[v];
                            if (pnd.tempExePersonStatus != (int)ProcessNodeExecutorStateEnum.未阅)
                            {
                                pnd.passPersons = personName;
                                pnd.tempExePersonStatus = (int)ProcessNodeExecutorStateEnum.未阅;
                            }
                            else
                            {
                                pnd.passPersons += "," + personName;
                            }
                            pnd.comment +=";"+ item.Comment;
                        }
                    }
                    else
                    {
                        PassNodeData passNode = new PassNodeData();
                        passNode.nodeID = item.Id;
                        passNode.nodeName = item.Name;
                        passNode.reciveTDate = item.ReciveDate;
                        passNode.falg = item.NodeLevel;
                        var temp = personList.FirstOrDefault(e => e.GUID == item.ExecutorId);
                        if (temp == null) continue;
                        passNode.passPersons = item.Strategy == 0 ? temp.OperatorName : (ProcessNodeExecutorStateEnum)item.exeState == ProcessNodeExecutorStateEnum.办理 ? temp.OperatorName :  (temp.OperatorName+"(未办理)");
                        passNode.comment = item.Comment;
                        passNode.tempExePersonStatus = item.exeState;
                        passNode.passState = (ProcessNodeStateEnum)item.State == ProcessNodeStateEnum.已处理 ?
                            "已处理" : (ProcessNodeStateEnum)item.State == ProcessNodeStateEnum.未处理 ? "未处理" : "正在处理";
                        passNode.reciveDate = item.ReciveDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        passNode.sendDate = item.SendDate != null ? item.SendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                        listPND.Add(passNode);
                        dicNodeToPND.Add(passNode.nodeID, listPND.Count - 1);
                    }
                }
                
                return listPND;
            }
        }

        //给单据的流程办理信息   
        public static List<PassNodeData> GetProccessDoedContainsBackRecord(Guid docDocGuid, out ResultMessager message)
        {
            List<PassNodeData> listPND = new List<PassNodeData>();
            message = new ResultMessager();
            message.Title = "提示";
            message.Icon = MessagerIconEnum.info;
            //根据单据ID 找到流程运行时


            using (var context = DAL.DBFactory.CreateFlowDB())
            {
                var process = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.DataId == docDocGuid);
                if (process == null)
                {
                    message.Msg = "没有流程办理信息";
                    return listPND;
                }
                var c = context.CreateObjectSet<OAO_WorkFlowSendBackRecord>()
                    .Where(e => e.ProcessId == process.ProcessId&&e.ExecutorState==2)//人必须时已经执行退回操作的
                    .OrderBy(e => e.falg).Select(e => new
                    {
                        Id = e.ProcessNodeId,
                        ExecutorId = e.ExecutorId,
                        ReciveDate = e.ReciveDate,
                        SendDate = e.SendDate,
                        exeState = e.ExecutorState,
                        Name = e.WorkFlowName,
                        NodeLevel = e.falg,
                        e.IsBack,
                        e.Comment,
                        e.falg,
                        State = 2//ProcessNodeStateEnum.已处理
                    }).ToList();
              
                var listGuid = c.Select(e => e.ExecutorId).ToList();
                BaseConfigEdmxEntities s = new BaseConfigEdmxEntities();
                var personList = s.SS_Operator.Where(e => listGuid.Contains(e.GUID)).Select(e => new { e.GUID, e.OperatorName }).ToList();

                //节点
                Dictionary<string, int> dicNodeToPND = new Dictionary<string, int>();
                foreach (var item in c)
                {
                    var key = item.Id + item.ReciveDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    if (dicNodeToPND.ContainsKey(key))
                    {
                        int v = dicNodeToPND[key];
                        var temp = personList.FirstOrDefault(e => e.GUID == item.ExecutorId);
                        if (temp == null) continue;
                        string personName = temp.OperatorName;
                        if ((ProcessNodeStateEnum)item.State == ProcessNodeStateEnum.等待)
                        {
                            string status = (ProcessNodeExecutorStateEnum)item.exeState == ProcessNodeExecutorStateEnum.办理 ? "" : "(未办理)";
                            listPND[v].passPersons += "," + personName + status;
                            continue;
                        }
                        //gcy 待修改 hanyx todo
                        if ((item.exeState == (int)ProcessNodeExecutorStateEnum.未阅 && item.State == (int)ProcessNodeExecutorStateEnum.办理) || item.exeState == (int)ProcessNodeExecutorStateEnum.办理)
                        {
                            var pnd = listPND[v];
                            if (pnd.tempExePersonStatus != (int)ProcessNodeExecutorStateEnum.未阅)
                            {
                                pnd.passPersons = personName;
                                pnd.tempExePersonStatus = (int)ProcessNodeExecutorStateEnum.未阅;
                            }
                            else
                            {
                                pnd.passPersons += "," + personName;
                            }
                            pnd.comment +=";" +item.Comment;
                        }
                      
                    }
                    else
                    {
                        PassNodeData passNode = new PassNodeData();
                        passNode.falg = item.falg;
                        passNode.nodeID = (Guid)item.Id;
                        passNode.reciveTDate = item.ReciveDate;
                        passNode.nodeName = item.Name;
                        var temp = personList.FirstOrDefault(e => e.GUID == item.ExecutorId);
                        if (temp == null) continue;
                        passNode.passPersons = temp.OperatorName;
                        passNode.tempExePersonStatus = (int)item.exeState;
                        passNode.comment = item.Comment;
                        passNode.passState = (ProcessNodeStateEnum)item.State == ProcessNodeStateEnum.已处理 ?
                            "已处理" : (ProcessNodeStateEnum)item.State == ProcessNodeStateEnum.未处理 ? "未处理" : "正在处理";
                        if (item.IsBack)
                        {
                            passNode.passState = "已处理（退回）";
                        }
                        passNode.reciveDate = item.ReciveDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        passNode.sendDate = item.SendDate != null ? item.SendDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                        listPND.Add(passNode);
                        dicNodeToPND.Add(key, listPND.Count - 1);
                    }
                }
                var listPND1 = GetProccessDoed(docDocGuid,out message);
                listPND.AddRange(listPND1);
                if (listPND.Count <= 0)
                {
                    message.Msg = "没有流程办理信息";
                    return listPND;
                }
                return listPND.OrderBy(e=>e.reciveTDate).ToList();
            }
        }
        public static bool ExistProcessWithCommit(Guid docDataGuid)
        {
            try
            {
                using (var context = DAL.DBFactory.CreateFlowDB())
                {
                    var c = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.DataId == docDataGuid);
                    if (c == null) return false;
                    var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == c.ProcessId);
                    //var db = new Platform.Model.InfrastructureEdmxEntities();
                    if (process == null) return false;
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public static bool ExistProcess(Guid docDataGuid)
        {
            try
            {
                using (var context = DAL.DBFactory.CreateFlowDB())
                {
                    var c = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderBy(e=>e.Flag).FirstOrDefault(e => e.DataId == docDataGuid);
                    if (c == null) return false;
                    if (c.Url == "hxcl" || c.Url == "zplq") {
                        return true;
                    }
                    if (c == null) return false;
                    var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == c.ProcessId);
                    //var db = new Platform.Model.InfrastructureEdmxEntities();
                    if (process == null) return false;
                    if (process.State ==(int) ProcessStateEnum.完成) {
                        return true;
                    }
                    int processNodeState = (int)ProcessNodeStateEnum.已处理;
                    //当前节点
                    var processNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").
                        FirstOrDefault(e => e.WorkFlowProcessId == process.Id && e.State != processNodeState);
                    if (processNode.OAO_WorkFlowNode.NodeType == 5)
                    {
                        return false;
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        //根据单子
        /*判断在单据是不是可以进行修改  1流程结束 不能修改 2、在流程中 必须审批人去修改*/
        public static bool ExistProcessCurPerson(Guid docDataGuid,Guid UserId)
        {
            using (var context = DAL.DBFactory.CreateFlowDB())
            {
               
                    var c = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderBy(e=>e.Flag).FirstOrDefault(e => e.DataId == docDataGuid);
                if(c==null)return false;
                int state = (int)ProcessNodeStateEnum.已处理;
                int stateWY = (int)ProcessNodeExecutorStateEnum.未阅;
                int stateYY = (int)ProcessNodeExecutorStateEnum.已阅;
                var executor = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().
                    Include("OAO_WorkFlowProcessNode")
                    .Where(e => e.ExecutorId == UserId && (e.State == stateWY || e.State == stateYY)&&
                        e.OAO_WorkFlowProcessNode.State != state).ToList();
                if (executor == null&&executor.Count==0) return true;
                var processIdList = executor.Select(e => e.OAO_WorkFlowProcessNode.WorkFlowProcessId).ToList<Guid>();
                if (processIdList.Count == 0) {
                    return true;
                }
                var workFlowProcessData = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderByDescending(e => e.Flag).FirstOrDefault(e => e.DataId == docDataGuid);
               
                if (workFlowProcessData == null) return false;

                //if (workFlowProcessData.Url == "hxcl" || workFlowProcessData.Url == "zplq")
                //{
                //    return true;
                //}
                if (!processIdList.Contains((Guid)workFlowProcessData.ProcessId))
                {
                    return true;
                }
                var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == workFlowProcessData.ProcessId);
                //var db = new Platform.Model.InfrastructureEdmxEntities();
                if (process == null) return false;
                if (process.State == (int)ProcessStateEnum.完成)
                {
                    return true;
                }
            }
            return false;
          
        }
        #endregion
        public static bool UpdateVariables(Guid workFlowProcessId,string value,string varName="internal_viewurl") {
            try
            {
                using (var context = DAL.DBFactory.CreateFlowDB())
                { 
                   var sqlFormat="UPDATE  OAO_WorkFlowProcessVariable SET VarValue='{0}' WHERE Id IN (SELECT Id FROM OAO_WorkFlowProcessVariableView WHERE WorkFlowProcessId='{1}' AND Name='{2}')";
                    var sql=string.Format(sqlFormat,value,workFlowProcessId,varName);
                   var i= context.ExecuteStoreCommand(sql);
                   return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        
        }
    }

}
