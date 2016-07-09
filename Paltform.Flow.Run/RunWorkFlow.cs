using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.DAL;
using System.Data.Objects;
using Platform.Model;
using Infrastructure;
namespace Platform.Flow.Run
{
    public interface IRunWorkFlow
    {
       

        /// <summary>
        /// 流程上下文
        /// </summary>
        ObjectContext FlowContext { get; }
        /// <summary>
        /// 提交流程 如果没有自动启动流程
        /// </summary>
        /// <param name="docTypeUrl">通过它可以找配置的流程</param>
        /// <param name="executorId">执行人的ID，来源于操作表</param>
        /// <param name="docDataGuid">单据的运行数据的ID</param>
        /// <param name="classId">单据所在的数据所在的表</param>
        /// <param name="errMessage">如果提交出现问题的错误提示</param>
        /// <returns>返回是否执行成功</returns>
        //ResultMessager CommitFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId);
        /// <summary>
        /// 提交流程 如果没有自动启动流程
        /// </summary>
        /// <param name="docTypeUrl">通过它可以找配置的流程</param>
        /// <param name="executorId">执行人的ID，来源于操作表</param>
        /// <param name="docDataGuid">单据的运行数据的ID</param>
        /// <param name="classId">单据所在的数据所在的表</param>
        /// <param name="errMessage">如果提交出现问题的错误提示</param>
        /// <param name="errMessage">意见</param>
        /// <returns>返回是否执行成功</returns>
        ResultMessager CommitFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId,string comment="");
        /// <summary>
        /// 退回流程

        ResultMessager SendBackFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId, string comment="");

        /// <summary>
        /// 流程拒绝
        /// </summary>
        /// <param name="docTypeUrl"></param>
        /// <param name="executorId"></param>
        /// <param name="docDataGuid"></param>
        /// <param name="classId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        ResultMessager RejectFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId, string comment = "");

        /// <summary>
        /// 获得Url和数据的Id
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ResultFlowBox GetViewFlowBox(AgencyTask value);
        void SetTasks(IList<Type> listTask);
        void SetOperatorId(Guid operatorid);
        int? ProcessStatus { get; }
        Guid ProcessId { get; }
        
    }
    //以后运行
    public class RunWorkFlow : IRunWorkFlow
    {
        //任务列表
        List<Type> TaskAll { get; set; }//程序中所有任务列表集合        //登陆人
        Guid OperatorId { get; set; }
        //文档路径 通过它可以找到和流程的对应关系
        string docTypeUrl { get; set; }
        //执行人的ID
        Guid executorId { get; set; }
        //单据数据的ID
        Guid dataGuid { get; set; }
        //可以通过其找到 真正的数据表
        int classId { get; set; }
        //返回的错误信息
        string errMessage { get; set; }
        //提交的意见

        string comment{get;set;}
        /*方法内参数*/
        ObjectContext context = null;
        bool bIsConnection = true;
        public ObjectContext FlowContext
        {
            get
            {
                if (this.bIsConnection) return this.context;
                return null;
            }
        }
        //存在关系的流程的对应的ID
        public Guid processId;
        public int? ProcessStatus { get; set; }
        public RunWorkFlow()
        {
            try
            {
                context = DAL.DBFactory.CreateFlowDB();
            }
            catch (Exception)
            {
                bIsConnection = false;
            }


        }


        #region 提交流程
        private bool CommitFlow(string docTypeUrl, Guid executorId, Guid dataGuid, int classId, out string errMessage)
        {
            if (!bIsConnection)
            {
                errMessage = "数据库连接失败！";
            }
            if (dataGuid == Guid.Empty || executorId == Guid.Empty)
            {
                errMessage = "用户过期！";
            }
            this.docTypeUrl = docTypeUrl;
            this.executorId = executorId;
            this.dataGuid = dataGuid;
            this.classId = classId;
            errMessage = this.errMessage;
            if (CheckFlowIsRunByDoc(out processId))//检查有没有保存好的 表单数据和流程运行时的对应            {
               
                //存在对应关系 获取运行流程的节点


                var processNode = GetCurrentProcessNode(processId);
                if (processNode == null)
                {
                    SetProcessStatus(processId);
                    errMessage = "当前流程已经结束";
                    return false;
                }

                if (!this.CheckPersonIsOK(processNode.Id))
                {
                    errMessage = "流程当前节点为【" + processNode.OAO_WorkFlowNode.Name + "】,您不是此节点的操作人!";
                    SetProcessStatus(processId);
                    return false;
                }
                //检查节点和提交的位置是否url相同
                if (!this.CheckUrl(processNode, docTypeUrl)) {
                    errMessage = "当请流程节点为【" + processNode.OAO_WorkFlowNode.Name + "】,您没有提交权限!";
                    SetProcessStatus(processId);
                    return false;
                }
                var bIsPassed = this.CheckPersionPassed(processNode.Id, ref errMessage);
                if (!bIsPassed)
                {
                    SetProcessStatus(processId);
                    return false;
                }
                //执行流程的节点


                if (this.RunProcess(processNode))
                {
                    UpdateStartFlowNodeExecutor(processNode);
                    errMessage = GetSuccesMessage(processNode.Id);
                    SetProcessStatus(processId);
                    return true;
                }
                return false;
            }
            else
            {
                //不存在 对应关系
                OAO_WorkFlow flow = null;
                //看次单据有没有配置流程  有 获取配置流程
                if (CheckDocIsWorkFlow(out flow))
                {
                    //存在流程 将流程转换为运行时



                    OAO_WorkFlowProcess process = flow.CreateRunTime(this.executorId);

                    context.CreateObjectSet<OAO_WorkFlowProcess>().AddObject(process);
                    context.SaveChanges();
                    processId = process.Id;
                    //开始流程运行时
                    process.StartProcess(context, flow, executorId, dataGuid, classId, docTypeUrl,TaskAll,this.OperatorId);
                    //插入提交人及提交建议
                    InsertStartFlowNodeExecutor(processId);
                    errMessage = GetSuccesMessage(Guid.Empty);
                    return true;
                }
                else
                {
                    errMessage = "当前单据未设置业务流程，请设置流程后再提交!";
                }

            }
            return false;
        }
        private void InsertStartFlowNodeExecutor(Guid processId) 
        {
            var startNode = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").FirstOrDefault(e => e.OAO_WorkFlowNode.NodeType == 5 && e.WorkFlowProcessId == processId);
            if (startNode != null)
            {
                var nodeExecutor = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().CreateObject();
                nodeExecutor.Id = Guid.NewGuid();
                nodeExecutor.WorkFlowProcessNodeId = startNode.Id;
                nodeExecutor.ReciveDate =DateTime.Now.AddSeconds(-1);
                nodeExecutor.SendDate = DateTime.Now.AddSeconds(-1);
                nodeExecutor.Comment = comment;
                nodeExecutor.ExecutorId = executorId;
                nodeExecutor.State =(int) ProcessNodeExecutorStateEnum.办理;
                nodeExecutor.ExecutorVersion = 1;// preProcessNode.OAO_WorkFlowProcess.CreatorVersion;
                this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().AddObject(nodeExecutor);
                context.SaveChanges();
            }
        }
        private void UpdateStartFlowNodeExecutor(OAO_WorkFlowProcessNode processNode)
        {
            if (processNode.OAO_WorkFlowNode.NodeType != 5) return;
            var startNode = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").FirstOrDefault(e => e.Id == processNode.Id);
            if (startNode != null)
            {

                var nodeExecutor = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().Where(e=>e.WorkFlowProcessNodeId==startNode.Id).ToList();
                foreach (var item in nodeExecutor)
                {
                    item.SendDate = DateTime.Now;
                    item.Comment = comment;
                   // item.ReciveDate =DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                    item.State =(int) ProcessNodeExecutorStateEnum.办理;
                    item.ExecutorVersion = 1;
                 }
                startNode.SendDate = DateTime.Now;
                //nodeExecutor.Id = Guid.NewGuid();
                //nodeExecutor.WorkFlowProcessNodeId = startNode.Id;
                //nodeExecutor.ReciveDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                //nodeExecutor.SendDate = DateTime.Now;
                //nodeExecutor.Comment = comment;
                //nodeExecutor.ExecutorId = executorId;
                //nodeExecutor.State = (int)ProcessNodeExecutorStateEnum.办理;
                //nodeExecutor.ExecutorVersion = 1;// preProcessNode.OAO_WorkFlowProcess.CreatorVersion;
                //this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().AddObject(nodeExecutor);
                context.SaveChanges();
            }
        }
        ResultMessager IRunWorkFlow.CommitFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId,string comment="")
        {
            this.comment = comment;
            ResultMessager resultMsg = new ResultMessager();
            resultMsg.Title = "系统提示";
            try
            {
             
                string strMsg = "";
                bool bSuccess = CommitFlow(docTypeUrl, executorId, docDataGuid, classId, out strMsg);
                resultMsg.Msg = strMsg;
                if (bSuccess)
                {
                    resultMsg.Icon = MessagerIconEnum.info;
                    resultMsg.Resulttype = 0;
                }
                else
                {
                    resultMsg.Icon = MessagerIconEnum.error;
                    resultMsg.Resulttype = 1;
                }

            }
            catch (Exception ex)
            {
                resultMsg.Msg = ex.Message.ToString();
                resultMsg.Icon = MessagerIconEnum.error;
                resultMsg.Resulttype = 1;
            }
            return resultMsg;
        }

        #endregion
        private void SetProcessStatus(Guid processId)
        {
            var ent = context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderByDescending(e=>e.Flag).FirstOrDefault(e => e.ProcessId == processId);
            if (ent != null)
            {
              
                ProcessStatus = ent.Url=="hxcl"||ent.Flag>=2?1:2;
                if (ProcessStatus == 2)
                {
                    var process = context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == processId);
                    if (process.State == 1)
                    {
                        ProcessStatus = 1;
                    }
                }

            }
        }
        /*退回用 给当前结点的URL*/
        public string GetCurrentNodeUrlByDocID(Guid docDataGuid)
        {
            this.docTypeUrl = docTypeUrl;
            this.executorId = executorId;
            this.dataGuid = docDataGuid;

            this.classId = classId;
            var bDoc = CheckFlowIsRunByDoc(out processId);//根据数据ID 取得流程的ID
            if (!bDoc)
            {
                return "";
            }
            //获取运行流程的节点
            var processNode = GetCurrentProcessNode(processId);
            if (processNode != null)
            {
                var level = processNode.GetNodeLevel(context);
                if (level == 1) return "";//开始不检查 为退回


                //等号为预算编制加的 如与核销重新考虑
                var oaoProcessData = context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e => e.ProcessId == processNode.WorkFlowProcessId).OrderByDescending(e => e.Flag).FirstOrDefault(e => e.Flag <= level);
                if (oaoProcessData != null)
                {
                    return oaoProcessData.Url;
                }

            }
            return "";
        }
        //检查提交的节点是否和当前的Url相同
        private bool CheckUrl(OAO_WorkFlowProcessNode processNode, string docTypeUrl)
        {
            var level = processNode.GetNodeLevel(context);
            if (level == 1) return true;//开始不检查 为退回

                //等号为预算编制加的 如与核销重新考虑
                var oaoProcessData = context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e=>e.ProcessId==processNode.WorkFlowProcessId).OrderByDescending(e => e.Flag).FirstOrDefault(e => e.Flag <= level);
                
                if (oaoProcessData == null) return false;
            return docTypeUrl == oaoProcessData.Url;
            
        }
        //检查人是否在当前办理节点中
        private bool CheckPersonIsOK(Guid processNodeId)
        {
            var exeCutor = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().FirstOrDefault(e => e.ExecutorId == this.executorId && e.WorkFlowProcessNodeId == processNodeId);
            if (exeCutor == null)
            {
                return false;
            }
            return true;
        }
        //办理信息
        private string GetSuccesMessage(Guid processNodeId)
        {
            int processNodeState = (int)ProcessNodeStateEnum.已处理;
            OAO_WorkFlowProcessNode proNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").
                    FirstOrDefault(e =>e.WorkFlowProcessId == processId && e.State != processNodeState);
            if (proNode != null)
            {
                if (proNode.Id == processNodeId)
                {

                    return "提交成功！";
                }
                return "已经提交到【" + proNode.OAO_WorkFlowNode.Name + "】节点";
            }
            return "提交成功,当前流程已经办理结束";
        }
        private string GetSuccesBackMessage(Guid processNodeId)
        {
            int processNodeState = (int)ProcessNodeStateEnum.已处理;
            OAO_WorkFlowProcessNode proNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").
                    FirstOrDefault(e => e.WorkFlowProcessId == processId && e.State != processNodeState);
            if (proNode != null)
            {
                if (proNode.Id == processNodeId)
                {

                    return "退回成功！";
                }
                var str = proNode.OAO_WorkFlowNode.Name;
                if (string.IsNullOrEmpty(str)) {
                    return "退回成功！";
                }
                return "已经退回到【" + str + "】节点";
            }
            return "退回成功！";
        }
        //检查此人是否办理过
        private bool CheckPersionPassed(Guid processNodeId, ref string errOr)
        {
            errOr = "";
            int stateTB = (int)ProcessNodeExecutorStateEnum.替办;
            int stateBL = (int)ProcessNodeExecutorStateEnum.办理;
            OAO_WorkFlowProcessNodeExecutor oaoExetor = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>()
                .FirstOrDefault(e => e.ExecutorId == this.executorId &&
                    e.WorkFlowProcessNodeId == processNodeId && (e.State == stateTB || e.State == stateBL));
            if (oaoExetor == null)
            {
                return true;
            }
            if (stateTB == oaoExetor.State)
            {
                errOr = "流程已经流程到下一节点";
            }
            else
            {
                errOr = "已经提交过,不能再次提交节点";
            }
            return false;
        }
        //检查单据是否有配置的流程
        private bool CheckDocIsWorkFlow(out OAO_WorkFlow workFlow)
        {
            workFlow = null;
            var doc = context.CreateObjectSet<OAO_WorkFlowDocType>().OrderByDescending(e=>e.WorkFlowVersion).FirstOrDefault(e => e.DocTypeUrl == docTypeUrl);
            if (doc != null)
            {
                Guid workFlowId = (Guid)doc.Guid_WorkFlow;
                workFlow = context.CreateObjectSet<OAO_WorkFlow>().Include("OAO_WorkFlowVariable")
                    .Include("OAO_WorkFlowNode")
                    .Include("OAO_WorkFlowRoute").FirstOrDefault(e => e.Id == workFlowId&&e.Version==doc.WorkFlowVersion);
                if (workFlow == null)
                    return false;
                return true;
            }
            return false;
            //var doc=context.CreateObjectSet<oao_
        }
        //检查是否有运行的运行单据的流程
        private bool CheckFlowIsRunByDoc(out Guid ProcessId)
        {
            ProcessId = new Guid();
            var doc = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.DataId == dataGuid);
            if (doc != null)
            {
                ProcessId = (Guid)doc.ProcessId;
                return true;
            }
            return false;
            //var doc=context.CreateObjectSet<oao_
        }
        //找到运行流程的当前节点
        private OAO_WorkFlowProcessNode GetCurrentProcessNode(Guid processId)
        {
            int processNodeState = (int)ProcessNodeStateEnum.已处理;
            var processNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
                FirstOrDefault(e => e.WorkFlowProcessId == processId && e.State != processNodeState);
            return processNode;
        }
        //运行流程的节点
        private bool RunProcess(OAO_WorkFlowProcessNode processNode)
        {
            try
            {
                Command com = Command.CreateCommand(processNode, context, ActionPointEnum.Accept);
                com.TaskAll = this.TaskAll;
                com.OperatorId = this.OperatorId;
                com.CurrentExecutor = executorId;
                com.Comment = this.comment;
                com.Send();
            }
            catch (Exception ex)
            {
                this.errMessage = ex.Message.ToString(); //"操作失败，当前流程已经提交";//
                return false;
            }
            return true;
        }
     
        
       

        #region IRunWorkFlow 成员


        public ResultFlowBox GetViewFlowBox(AgencyTask value)
        {
            ResultFlowBox result=new ResultFlowBox();
            if (value.ProcessNodeID == Guid.Empty || value.ProcessID == Guid.Empty) return result;
            OAO_WorkFlowProcessNode node = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().FirstOrDefault(e => e.Id == value.ProcessNodeID);
            if (node != null)
            {
                Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
                var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
                Command m = Command.CreateCommand(node, this.context, ActionPointEnum.Doing);
                m.TaskAll = listTasks;
                m.PerformTask(TaskActionEnum.过程中);
            }
            ProcessVariableProvider pvp = new ProcessVariableProvider(value.ProcessID);
            result.DataId = pvp.GetValue(ProcessVariableProvider.internal_viewid);
            result.Url = pvp.GetValue(ProcessVariableProvider.internal_viewurl);
            result.Msg = pvp.GetValue(ProcessVariableProvider.internal_error);
            result.Common = pvp.GetValue(ProcessVariableProvider.internal_common);
            return result;
        }

        #endregion
        /// <summary>
        /// 到支票领取的 
        /// </summary>
        /// <returns></returns>
        public bool IsZPLQToHXCLNotBack(Guid docDataguid) {

            var sqlFormat = "SELECT * FROM dbo.BX_MainView WHERE  DocTypeKey='05' AND GUID='{0}' AND EXISTS(SELECT * FROM oao_workflowProcessData WHERE DataId='{0}' AND flag>=2 )";
            var sql = string.Format(sqlFormat, docDataguid);
            try
            {
                var dt = DataSource.ExecuteQuery(sql);
                if (dt!=null&&dt.Rows.Count > 0) {
                    return true;
                }
            }
            catch (Exception)
            {

            }
            return false;
        }

        #region 退回

        public ResultMessager SendBackFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId,string comment="")
        {
            this.comment = comment;
            ResultMessager resultMsg = new ResultMessager();
            resultMsg.Title = "系统提示";
            if (!bIsConnection)
            {
                resultMsg.Msg = "数据库连接失败！";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            if (docDataGuid == Guid.Empty || executorId == Guid.Empty)
            {
                resultMsg.Msg = "用户过期！";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            this.docTypeUrl = docTypeUrl;
            this.executorId = executorId;
            this.dataGuid = docDataGuid;
           
            this.classId = classId;
            var bDoc=  CheckFlowIsRunByDoc(out processId);//根据数据ID 取得流程的ID
            if (!bDoc) {
                resultMsg.Msg = "没有配置流程！";
                resultMsg.Resulttype = 2;
                return resultMsg;
            }
            var processNode = GetCurrentProcessNode(processId);
            //获取运行流程的节点
            if (IsZPLQToHXCLNotBack(dataGuid)) {
                //支票领取的单子 直接退回到会计审批 并且删除支票领用
                resultMsg.Msg = "支票已经领取不能退回！";
                resultMsg.Resulttype = 2;
                return resultMsg;
            }

          
            bool IsEnd = false;
            OAO_WorkFlowProcessNode processEndNode = null;
            if (processNode == null)
            {
                var processNodeList = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
            Where(e => e.WorkFlowProcessId == processId &&e.OAO_WorkFlowNode.NodeType!=6).OrderBy(e=>e.OAO_WorkFlowNode.NodeLevel).OrderByDescending(e=>e.ReciveDate).ToList();
                if (processNodeList != null&&processNodeList.Count>0) {
                    processNode = processNodeList.FirstOrDefault();
                }
                IsEnd = true;
                processEndNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
            FirstOrDefault(e => e.WorkFlowProcessId == processId && e.OAO_WorkFlowNode.NodeType == 6); 
                //resultMsg.Msg = "已经退回到【" + processNode.OAO_WorkFlowNode.Name + "】节点";
                //resultMsg.Resulttype = 1;
                //return resultMsg;
            }

            //如果是开始节点就不能再退回了
            if (processNode.OAO_WorkFlowNode.NodeType == 5) {
                resultMsg.Msg = "单据不可退回，请修改后提交";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            if (!this.CheckPersonIsOK(processNode.Id))
            {
                //结束的时候可能 没有人 所以结束的流程不验证
                if (processEndNode.OAO_WorkFlowNode.NodeType != 6)
                {
                    resultMsg.Msg = "流程当前节点为【" + processNode.OAO_WorkFlowNode.Name + "】,您不是此节点的操作人!";
                    resultMsg.Resulttype = 1;
                }
                return resultMsg;
            }
            //检查节点和提交的位置是否url相同
            if (!this.CheckUrl(processNode, docTypeUrl))
            {
                resultMsg.Msg = "非流程激活单据,无法提交!";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            //执行流程的节点

            if (IsEnd) {
                processNode = processEndNode;
            }
            if (this.RunBackProcess(processNode))
            {
                resultMsg.Msg = GetSuccesBackMessage(processNode.Id);
                return resultMsg;
            }
            else {
                resultMsg.Msg = this.errMessage;
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
        }
        private bool RunBackProcessToStartNode(OAO_WorkFlowProcessNode processNode) 
        {
           var preProcessNode = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").OrderBy(e =>e.OAO_WorkFlowNode.NodeLevel)
                   .FirstOrDefault(e => e.OAO_WorkFlowNode.NodeLevel < processNode.OAO_WorkFlowNode.NodeLevel //退回的节点与现在节点间距最小 临近
                                   && e.WorkFlowProcessId == processNode.WorkFlowProcessId
                                   && e.OAO_WorkFlowNode.NodeType == 5 //是普通节点 因为只有普通节点上有人
                                    );//不能使容器内节点
           var delProcessNodes = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode")
                 .Where(e => e.OAO_WorkFlowNode.NodeLevel > preProcessNode.OAO_WorkFlowNode.NodeLevel && e.WorkFlowProcessId == processNode.WorkFlowProcessId)
                 .ToList();
           //任务退回
           /*to do*/
           var delProcessIds = delProcessNodes.Select(e => e.Id).ToList();
           //删除节点的办理的人
           var executors = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>()
               .Include("OAO_WorkFlowProcessNode")
               .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode")
               .Where(e => delProcessIds.Contains(e.OAO_WorkFlowProcessNode.Id)).ToList();
           foreach (var item in executors)
           { 
               //记录已经办理过的节点 和 办理相关信息
               SetOAO_WorkFlowSendBackRecord(this.context, item, item.OAO_WorkFlowProcessNode, processNode);
               this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().DeleteObject(item);
           }
           //删除办理的节点
           foreach (var item in delProcessNodes)
           {
               this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().DeleteObject(item);
           }
           //procesData数据删除
           var processDatas = this.context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e => e.Flag > preProcessNode.OAO_WorkFlowNode.NodeLevel && e.ProcessId == preProcessNode.WorkFlowProcessId).ToList();
           foreach (var item in processDatas)
           {
               this.context.CreateObjectSet<OAO_WorkFlowProcessData>().DeleteObject(item);
           }
           var nodeStartExtcutor = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().FirstOrDefault(e => e.WorkFlowProcessNodeId == preProcessNode.Id);
           if (nodeStartExtcutor == null)
           {

               var nodeExecutor = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().CreateObject();
               nodeExecutor.Id = Guid.NewGuid();
               nodeExecutor.WorkFlowProcessNodeId = preProcessNode.Id;
               nodeExecutor.ReciveDate = DateTime.Now;
               nodeExecutor.ExecutorId = (Guid)preProcessNode.OAO_WorkFlowProcess.Createor;
               nodeExecutor.ExecutorVersion = 1;// preProcessNode.OAO_WorkFlowProcess.CreatorVersion;
               this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().AddObject(nodeExecutor);
               //记录退回到开始节点



               SetOAO_WorkFlowSendBackRecord(this.context, nodeExecutor, preProcessNode, processNode);
           }
           else {
               SetOAO_WorkFlowSendBackRecord(this.context, nodeStartExtcutor, preProcessNode, processNode);
               nodeStartExtcutor.Comment = "";
               nodeStartExtcutor.SendDate = null;
               nodeStartExtcutor.State = 0;
               nodeStartExtcutor.ReciveDate = DateTime.Now;
           }
           preProcessNode.SendDate = null;
           preProcessNode.State = (int)ProcessNodeStateEnum.未处理;//qw
           this.context.SaveChanges();
           return true;
        }
        private bool RunBackProcess(OAO_WorkFlowProcessNode processNode)
        {
            try
            {
                //找到要退回的节点
                var preProcessNode = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").OrderByDescending(e => e.OAO_WorkFlowNode.NodeLevel)
                    .FirstOrDefault(e => e.OAO_WorkFlowNode.NodeLevel < processNode.OAO_WorkFlowNode.NodeLevel //退回的节点与现在节点间距最小 临近
                                    &&e.WorkFlowProcessId==processNode.WorkFlowProcessId
                                    && e.OAO_WorkFlowNode.NodeType == 0 //是普通节点 因为只有普通节点上有人
                                     );//不能使容器内节点
                if (preProcessNode == null) {
                    //有可能要退回到开始节点
                    if (RunBackProcessToStartNode(processNode)) {
                        return true;
                    }
                    this.errMessage = "找不到要退回的节点";
                    return false;
                }
                //从外部不能退回到内部
                if (preProcessNode.OAO_WorkFlowNode.CombineNodeId != null && processNode.OAO_WorkFlowNode.CombineNodeId!=preProcessNode.OAO_WorkFlowNode.CombineNodeId)
                {
                    this.errMessage = "退回的节点在容器内，无法退回";
                    return false;
                }
                //改变状态
                var preExecutors = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().Where(e =>e.WorkFlowProcessNodeId==preProcessNode.Id).ToList();
                foreach (var item in preExecutors)
                {
                    //保存当前办理人得记录
                    SetOAO_WorkFlowSendBackRecord(context, item, item.OAO_WorkFlowProcessNode, processNode);
                    item.ReciveDate = DateTime.Now;
                    item.SendDate = null;
                    item.State = 0;//未阅
                    item.Comment = "";
                   
                }
                preProcessNode.State = (int)ProcessNodeStateEnum.未处理;
                preProcessNode.SendDate = null;
                context.SaveChanges();
                //保存退回的当前节点


                preProcessNode.State = (int)ProcessNodeStateEnum.未处理;
                var delProcessNodes = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode")
                    .Where(e => e.OAO_WorkFlowNode.NodeLevel > preProcessNode.OAO_WorkFlowNode.NodeLevel && e.WorkFlowProcessId == processNode.WorkFlowProcessId)
                    .ToList();
                //任务退回
                /*to do*/
                var delProcessIds=delProcessNodes.Select(e=>e.Id).ToList();
                //删除节点的办理的人
                var executors = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>()
                    .Include("OAO_WorkFlowProcessNode")
                    .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode")
                    .Where(e=>delProcessIds.Contains(e.OAO_WorkFlowProcessNode.Id)).ToList();
                foreach (var item in executors)
                {
                    //记录已经办理人的信息
                    SetOAO_WorkFlowSendBackRecord(context, item, item.OAO_WorkFlowProcessNode, processNode);
                    this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().DeleteObject(item);
                }
                //删除办理的节点
                foreach (var item in delProcessNodes)
                {
                    this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().DeleteObject(item);
                }
                //procesData数据删除
                var processDatas = this.context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e => 
                    (e.Flag > preProcessNode.OAO_WorkFlowNode.NodeLevel&&e.ProcessId==preProcessNode.WorkFlowProcessId)
                    ||(e.Url=="hxcl"&&e.ProcessId==preProcessNode.WorkFlowProcessId)
                    ).ToList();
                foreach (var item in processDatas)
                {
                    this.context.CreateObjectSet<OAO_WorkFlowProcessData>().DeleteObject(item);
                }
                //找见变量比对
                //var processData = this.context.CreateObjectSet<OAO_WorkFlowProcessData>().OrderBy(e => e.Flag).FirstOrDefault(e => e.ProcessId == preProcessNode.WorkFlowProcessId);
                //if (processData != null)
                //{
                //    preProcessNode.OAO_WorkFlowProcess.SetVariable(this.context, ProcessVariableProvider.internal_viewurl, processData.Url);
                //}
                //修改流程状态
                var process = this.context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == ProcessId);
                if (process != null) {
                    process.State = (int)ProcessStateEnum.运行;
                }
                this.context.SaveChanges();
                
            }
            catch (Exception ex)
            {
                this.errMessage = ex.Message.ToString(); //"操作失败，当前流程已经提交";//
                return false;
            }
            return true;
        }
        private void SetOAO_WorkFlowSendBackRecord(ObjectContext context, OAO_WorkFlowProcessNodeExecutor item, OAO_WorkFlowProcessNode processNode, OAO_WorkFlowProcessNode curProcessNode) 
        {
           
            var entSendRecord = context.CreateObjectSet<OAO_WorkFlowSendBackRecord>().CreateObject();
            if (curProcessNode==null||processNode.Id != curProcessNode.Id)
            {
                entSendRecord.Id = Guid.NewGuid();
                entSendRecord.ReciveDate = item.ReciveDate;
                entSendRecord.Comment = item.Comment;
                entSendRecord.falg = processNode.OAO_WorkFlowNode.NodeLevel;
                entSendRecord.ProcessNodeId = processNode.Id;
                entSendRecord.WorkFlowId = processNode.OAO_WorkFlowNode.Id;
                entSendRecord.WorkFlowName = processNode.OAO_WorkFlowNode.Name;
                entSendRecord.ExecutorId = item.ExecutorId;
                entSendRecord.ExecutorState = item.State;
                entSendRecord.ProcessId = processNode.WorkFlowProcessId;
                entSendRecord.IsBack = false;
                entSendRecord.SendDate = item.SendDate;
            }
            else
            {
                entSendRecord.Id = Guid.NewGuid();
                entSendRecord.ReciveDate = item.ReciveDate;
                entSendRecord.Comment = comment;
                entSendRecord.falg = processNode.OAO_WorkFlowNode.NodeLevel;
                entSendRecord.ProcessNodeId = processNode.Id;
                entSendRecord.WorkFlowId = processNode.OAO_WorkFlowNode.Id;
                entSendRecord.WorkFlowName = processNode.OAO_WorkFlowNode.Name;
                entSendRecord.ExecutorId = item.ExecutorId;
                entSendRecord.SendDate = DateTime.Now;

                if (this.executorId == item.ExecutorId)
                {
                    entSendRecord.ExecutorState = item.State;
                    entSendRecord.ExecutorState = (int)ProcessNodeExecutorStateEnum.办理;
                }
                else 
                {
                    entSendRecord.ExecutorState = item.State;
                    entSendRecord.ExecutorState = (int)ProcessNodeExecutorStateEnum.替办;
                }
                entSendRecord.ProcessId = processNode.WorkFlowProcessId;
                entSendRecord.IsBack = true;
            }
            context.CreateObjectSet<OAO_WorkFlowSendBackRecord>().AddObject(entSendRecord);
        }
      
        #endregion


        #region 节点拒绝
        public ResultMessager RejectFlow(string docTypeUrl, Guid executorId, Guid docDataGuid, int classId, string comment = "")
        {
            this.comment = comment;
            ResultMessager resultMsg = new ResultMessager();
            resultMsg.Title = "系统提示";
            if (!bIsConnection)
            {
                resultMsg.Msg = "数据库连接失败！";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            if (docDataGuid == Guid.Empty || executorId == Guid.Empty)
            {
                resultMsg.Msg = "用户过期！";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            this.docTypeUrl = docTypeUrl;
            this.executorId = executorId;
            this.dataGuid = docDataGuid;

            this.classId = classId;
            var bDoc = CheckFlowIsRunByDoc(out processId);//根据数据ID 取得流程的ID
            if (!bDoc)
            {
                resultMsg.Msg = "没有配置流程！";
                resultMsg.Resulttype = 2;
                return resultMsg;
            }
            var processNode = GetCurrentProcessNode(processId);
            //获取运行流程的节点
            if (IsZPLQToHXCLNotBack(dataGuid))
            {
                //支票领取的单子 直接退回到会计审批 并且删除支票领用
                resultMsg.Msg = "支票已经领取不能退回！";
                resultMsg.Resulttype = 2;
                return resultMsg;
            }


            bool IsEnd = false;
            OAO_WorkFlowProcessNode processEndNode = null;
            if (processNode == null)
            {
                var processNodeList = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
            Where(e => e.WorkFlowProcessId == processId && e.OAO_WorkFlowNode.NodeType != 6).OrderBy(e => e.OAO_WorkFlowNode.NodeLevel).OrderByDescending(e => e.ReciveDate).ToList();
                if (processNodeList != null && processNodeList.Count > 0)
                {
                    processNode = processNodeList.FirstOrDefault();
                }
                IsEnd = true;
                processEndNode = context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Include("OAO_WorkFlowNode").
            FirstOrDefault(e => e.WorkFlowProcessId == processId && e.OAO_WorkFlowNode.NodeType == 6);
                //resultMsg.Msg = "已经退回到【" + processNode.OAO_WorkFlowNode.Name + "】节点";
                //resultMsg.Resulttype = 1;
                //return resultMsg;
            }

            //临时处理--如果是预算分配流程流程编制节点咱时不能再后退了
            var NodeList = new List<string> { "财务预算审批", "部门预算审批" };
            if (processNode != null && (classId == 62 || classId==90))
            {
                var curNodeName = processNode.OAO_WorkFlowNode.Name;
                if (!NodeList.Contains(curNodeName))
                {
                    resultMsg.Msg = "当前节点无法退回！";
                    resultMsg.Resulttype = 2;
                    return resultMsg;
                }
            }


            //如果是开始节点就不能再退回了
            if (processNode.OAO_WorkFlowNode.NodeType == 5)
            {
                resultMsg.Msg = "单据不可退回，请修改后提交";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            if (!this.CheckPersonIsOK(processNode.Id))
            {
                //结束的时候可能 没有人 所以结束的流程不验证

                if (processEndNode.OAO_WorkFlowNode.NodeType != 6)
                {
                    resultMsg.Msg = "流程当前节点为【" + processNode.OAO_WorkFlowNode.Name + "】,您不是此节点的操作人!";
                    resultMsg.Resulttype = 1;
                }
                return resultMsg;
            }
            //检查节点和提交的位置是否url相同
            if (!this.CheckUrl(processNode, docTypeUrl))
            {
                resultMsg.Msg = "非流程激活单据,无法提交!";
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
            //执行流程的节点


            if (IsEnd)
            {
                processNode = processEndNode;
            }
            if (this.RunBackProcess(processNode))
            {
                resultMsg.Msg = GetSuccesBackMessage(processNode.Id);
                return resultMsg;
            }
            else
            {
                resultMsg.Msg = this.errMessage;
                resultMsg.Resulttype = 1;
                return resultMsg;
            }
        }
        #endregion

        #region 设置任务


        public void SetTasks(IList<Type> listTask)
        {
            this.TaskAll = listTask.ToList();  
        }
        public void SetOperatorId(Guid operatorid)
        {
            this.OperatorId = operatorid;
        }
        #endregion



        #region IRunWorkFlow 成员


        public Guid ProcessId
        {
            get { return processId; }
        }

        #endregion


        #region 为了支票领取单 单写的 过了支票领取 不管什么节点 都退回到会计审批
        public bool SendBackForZP( OAO_WorkFlowProcessNode processNode) {
            try
            {

           
            if(processNode==null)return false;
            //找到要退回的节点
            var preProcessNode = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").FirstOrDefault(e => e.OAO_WorkFlowNode.Name == "会计审批");
            //改变状态
            var preExecutors = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().Where(e => e.WorkFlowProcessNodeId == preProcessNode.Id).ToList();
            foreach (var item in preExecutors)
            {
                //保存当前办理人得记录
                var entSendRecord = context.CreateObjectSet<OAO_WorkFlowSendBackRecord>().CreateObject();
                entSendRecord.Id = Guid.NewGuid();
                entSendRecord.ReciveDate = item.ReciveDate;
                entSendRecord.Comment = comment;
                entSendRecord.falg = processNode.OAO_WorkFlowNode.NodeLevel;
                entSendRecord.ProcessNodeId = processNode.Id;
                entSendRecord.WorkFlowId = processNode.OAO_WorkFlowNode.Id;
                entSendRecord.WorkFlowName = processNode.OAO_WorkFlowNode.Name;
                entSendRecord.ExecutorId = item.ExecutorId;
                entSendRecord.SendDate = DateTime.Now;

                if (this.executorId == item.ExecutorId)
                {
                    entSendRecord.ExecutorState = item.State;
                    entSendRecord.ExecutorState = (int)ProcessNodeExecutorStateEnum.办理;
                }
                else 
                {
                    entSendRecord.ExecutorState = item.State;
                    entSendRecord.ExecutorState = (int)ProcessNodeExecutorStateEnum.替办;
                }
                entSendRecord.ProcessId = processNode.WorkFlowProcessId;
                entSendRecord.IsBack = true;
                context.CreateObjectSet<OAO_WorkFlowSendBackRecord>().AddObject(entSendRecord);
                item.ReciveDate = DateTime.Now;
                item.SendDate = null;
                item.State = 0;//未阅
                item.Comment = "";

            }
            preProcessNode.State = (int)ProcessNodeStateEnum.未处理;
            preProcessNode.SendDate = null;
           
            context.SaveChanges();

            var delProcessNodes = this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode")
                .Where(e => e.OAO_WorkFlowNode.NodeLevel > preProcessNode.OAO_WorkFlowNode.NodeLevel && e.WorkFlowProcessId == processNode.WorkFlowProcessId)
                .ToList();
            //任务退回

            /*to do*/
            var delProcessIds = delProcessNodes.Select(e => e.Id).ToList();
            //删除节点的办理的人

            var executors = this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>()
                .Include("OAO_WorkFlowProcessNode")
                .Include("OAO_WorkFlowProcessNode.OAO_WorkFlowNode")
                .Where(e => delProcessIds.Contains(e.OAO_WorkFlowProcessNode.Id)).ToList();
            foreach (var item in executors)
            {
                //记录已经办理人的信息
                SetOAO_WorkFlowSendBackRecord(context, item, item.OAO_WorkFlowProcessNode, processNode);
                this.context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().DeleteObject(item);
            }
            //删除办理的节点

            foreach (var item in delProcessNodes)
            {
                this.context.CreateObjectSet<OAO_WorkFlowProcessNode>().DeleteObject(item);
            }
            //procesData数据删除
            var processDatas = this.context.CreateObjectSet<OAO_WorkFlowProcessData>().Where(e =>
                (e.Flag > preProcessNode.OAO_WorkFlowNode.NodeLevel && e.ProcessId == preProcessNode.WorkFlowProcessId)
                || (e.Url == "hxcl" && e.ProcessId == preProcessNode.WorkFlowProcessId)
                ).ToList();
            foreach (var item in processDatas)
            {
                this.context.CreateObjectSet<OAO_WorkFlowProcessData>().DeleteObject(item);
            }
            //修改流程状态

            var process = this.context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == ProcessId);
            if (process != null)
            {
                process.State = (int)ProcessStateEnum.运行;
            }
            this.context.SaveChanges();
            //修改变量
            var sql = @"UPDATE dbo.OAO_WorkFlowProcessVariable SET VarValue='hkspd' WHERE
 WorkFlowVariableId IN (SELECT Id FROM OAO_WorkFlowVariable WHERE WorkFlowId='" + preProcessNode.OAO_WorkFlowNode.WorkFlowId+ "'AND Name='internal_viewurl')";
            this.context.ExecuteStoreCommand(sql);
            this.context.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion
    }
  
}
