using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Flow.Run
{
    /// <summary>
    /// 流程办理信息
    /// </summary>
    public class PassNodeData
    {
        public string passPersons { get; set; }
        public string nodeName { get; set; }
        public string reciveDate { get; set; }
        public string comment { get; set; }
        public string sendDate { get; set; }
        public string passState { get; set; }
        public int? falg { get; set; }
        public Guid nodeID { get; set; }
        public int tempExePersonStatus { get; set; }
        public DateTime? reciveTDate { get; set; }
    }
    /// <summary>
    /// 给待办功能用的 待办任务扩展类
    /// </summary>
    public class AgencyTaskEx : AgencyTask
    {
        public string WorkFlowName { get; set; }
        public DateTime? ProcessDate { get; set; }
    }
    //待办数据
    public class AgencyTask
    {
        public AgencyTask() { }
        public AgencyTask(Guid processId, Guid processNodeId, int? nodeLevel, string nodeName, DateTime? acceptDate, int nodeType)
        {

            this.ProcessID = processId;
            this.ProcessNodeID = processNodeId;
            this.NodeLevel = nodeLevel;
            this.NodeName = nodeName;
            this.AcceptDate = acceptDate;
            this.NodeType = nodeType;
        }
        public Guid ProcessID { get; set; }
        public Guid ProcessNodeID { get; set; }
        public int? NodeLevel { get; set; }
        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }
        /// <summary>
        /// 接受时间
        /// </summary>
        public DateTime? AcceptDate { get; set; }
        /// <summary>
        /// 节点类型
        /// </summary>
        public int NodeType { get; set; }
    }
    public enum MessagerIconEnum
    {
        error,
        info,
        question,
        warning
    }
    public class ResultMessager
    {
        /// <summary>
        /// 0:正y确ā?1:错洙误ó



        /// </summary>
        public int Resulttype = 0;

        /// <summary>
        /// 标括题琣
        /// </summary>
        public string Title
        {
            set;
            get;
        }
        /// <summary>
        /// 消?息￠内ú容╕



        /// </summary>
        public string Msg
        {
            set;
            get;
        }
        /// <summary>
        /// Icon类え型í



        public int Status { get; set; }
        /// </summary>
        public MessagerIconEnum Icon
        {
            set;
            get;
        }
    }
    //流程节点的Model
    public class FlowNodeModel
    {
        public FlowNodeModel() { }
        public FlowNodeModel(string flowNodeName, Guid nodeId, Guid flowId, int version)
        {
            WorkFlowNodeName = flowNodeName;
            WorkFlowNodeId = nodeId;
            WorkFlowId = flowId;
            WorkFlowVersion = version;
        }
        public string WorkFlowNodeName { get; set; }
        public Guid WorkFlowNodeId { get; set; }
        public Guid WorkFlowId { get; set; }
        public int WorkFlowVersion { get; set; }
        public int? Sort { get; set; }
        public int? NodeType { get; set; }
        public Guid ProcessId { get; set; }
        public Guid ProcessNodeId { get; set; }
    }
    public class ResultFlowBox
    {
        public string Url { get; set; }
        public string DataId { get; set; }
        public string Msg { get; set; }
        public string Common { get; set; }
    }
    //流程节点在运行过程中的不同状态
    public enum NodeRunState
    {
        无状态 = -1,
        未处理 = 0,
        处理中 = 1,
        处理完成 = 2,
    }
    //预算转换的状态
    public enum BGTransStatus
    {
        不转换 = 0,
        预算分配到预算初始值 = 1,
        预算分配到预算编制 = 2,
        预算初始值到预算编制 = 3,
    }
    //节点的发送方向 退回还是发送 节点和人
    public enum NodeSendDir 
    {
       无=0,
       发送=1,
       退回=2
    }

}
