using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Configuration;
using Platform.Infrastructure;
using Platform.Flow.Run;
using System.Data.Objects;
using Platform.Model;
using Platform.Infrastructure.InternalTask;
namespace Platform.Flow.Run
{

    internal static class FlowContextProvider
    {
        public static string name = string.Empty;

        private static InfrastructureEdmxEntities _current = null;

        /// <summary>
        /// 获取最近创建的ObjectContext实例
        /// </summary>
        public static InfrastructureEdmxEntities Current { get { return _current; } }

        /// <summary>
        /// 创建数据库连接
        /// </summary>
        /// <param name="name">
        /// 配置文件中的连接字符串名称
        /// 格式:name=connectionName
        /// </param>
        /// <returns></returns>
        public static InfrastructureEdmxEntities CreateContext()
        {
            return Platform.DAL.DBFactory.CreateFlowDB();
        }
    }

    internal abstract class Command
    {
        /// <summary>
        /// 登陆人Id
        /// </summary>
        public Guid OperatorId { get; set; }

        public List<Type> TaskAll { get; set; }
        /// <summary>
        /// 退回和同意的意见 
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 是否保存  保证同一Context
        /// </summary>
        public bool isSave { get; set; }
        protected OAO_WorkFlowProcessNode _nodePre = null;
        /// <summary>
        /// 当前流程运行时上一节点
        /// </summary>
        public OAO_WorkFlowProcessNode NodePre { get { return _nodePre; } }
        protected OAO_WorkFlowProcessNode _node = null;
        /// <summary>
        /// 当前流程运行时节点


        /// </summary>
        public OAO_WorkFlowProcessNode Node { get { return _node; } }

        protected ActionPointEnum _action = ActionPointEnum.Accept;
        /// <summary>
        /// 命令所涉及的发送行为
        /// </summary>
        public ActionPointEnum Action { get { return _action; } }

        /// <summary>
        /// 数据库上下文
        /// </summary>
        protected ObjectContext Context = null;

        /// <summary>
        /// 当前执行人
        /// </summary>
        public Guid CurrentExecutor = Guid.Empty;

        /// <summary>
        /// 执行任务
        /// </summary>
        public virtual void PerformTask(TaskActionEnum taskAction)
        {
            if (this.PermitPerformTask())
            {
                //变量和运行变量的ID
                Dictionary<string, Guid> dicVarName2Id = new Dictionary<string, Guid>();
                //1、拿到任务               
                var taskList = this.Node.GetNodeTask(Context, taskAction);
                if (taskList.Count <= 0)
                {
                    return; //没有任务执行返回
                }
                var process = Node.GetProcess(Context);
                var taskParma = new TaskParameter() { TaskAction = taskAction, FlowContext = Context, ActionPoint = this.Action, CurrentNode = Node, SenderNode = NodePre, Process = process,
                 OperatorId=this.OperatorId};
                
                foreach (var item in taskList)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    var iTask = TaskManager.CreateTask(item,this.TaskAll);
                    this.Node.IsTaskOver = iTask.Run(taskParma);
                    if (!this.Node.IsTaskOver)
                        break;

                }
                this.Context.SaveChanges();
            }
        }

        /// <summary>
        /// 根据发送行为获取接下来要发送的节点
        /// </summary>
        /// <returns></returns>
        public virtual List<OAO_WorkFlowProcessNode> CreateNextNodes()
        {
            List<OAO_WorkFlowProcessNode> result = new List<OAO_WorkFlowProcessNode>();
            OAO_WorkFlowProcess process = null;
            if (this.NodePre.OAO_WorkFlowProcess == null)
            {
                process = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == this.NodePre.WorkFlowProcessId);
            }
            else
            {
                process = this.NodePre.OAO_WorkFlowProcess
;
            }
            FlowChart = this.Context.CreateObjectSet<OAO_WorkFlow>().Include("OAO_WorkFlowNode").Include("OAO_WorkFlowRoute").FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);//FlowChart.DeSerialize(process.Xml);//优化
            List<OAO_WorkFlowNode> fns = this.CreateNextNodes(FlowChart, this.Action);
            foreach (OAO_WorkFlowNode fn in fns)
            {
                result.Add(fn.CreateRunTime(process.Id));
            }
            return result;
        }
        public abstract List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i);
        public virtual void SaveChanges()
        {
            if (this.isSave)
            {
                this.Context.SaveChanges();
            }
        }
        ///
        public OAO_WorkFlow FlowChart = null;
        /// <summary>
        /// 注册接下来要发送的节点到数据库
        /// </summary>
        public abstract void Register();
        /// <summary>
        /// 注册接下来要发送的节点到数据库
        /// </summary>
        public abstract void RegisterNext();

        /// <summary>
        /// 设置本节点为完成状态




        /// </summary>
        public abstract void Terminate();
        /// <summary>
        /// 检查是否允许发送




        /// </summary>
        /// <returns></returns>
        public abstract bool PermitSend();
        /// <summary>
        /// 检查是否允许执行任务




        /// </summary>
        /// <returns></returns>
        public abstract bool PermitPerformTask();

        /// <summary>
        /// 发送



        /// </summary>
        public abstract void Send();
        public static Command CreateCommand(OAO_WorkFlowProcessNode node, ObjectContext context,List<Type> listTask, ActionPointEnum action = ActionPointEnum.Accept)
        {
            return CreateCommand(node, node, context,listTask, action);
        }
        public static Command CreateCommand(OAO_WorkFlowProcessNode node, ObjectContext context, ActionPointEnum action = ActionPointEnum.Accept)
        {
            return CreateCommand(node, node, context, action);
        }
        public static Command CreateCommand(OAO_WorkFlowProcessNode node, string connName, ActionPointEnum action = ActionPointEnum.Accept)
        {
            return CreateCommand(node, node, FlowContextProvider.CreateContext(), action);
        }
        public static Command CreateCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode nodePre, ObjectContext context,List<Type> listTask, ActionPointEnum action = ActionPointEnum.Accept, bool isSave = true)
        {
            Command com = null;
            var flowNode = Platform.DAL.DBFactory.CreateFlowDB().OAO_WorkFlowNode.FirstOrDefault(e => e.Id == node.WorkFlowNodeId);
            switch (flowNode.NodeType)
            {
                case 0:
                    com = new NormalCommand(node, nodePre, action, context, isSave);
                    break;
                case 1:
                    com = new CompetationCommand(node, nodePre, action, context, isSave);
                    break;
                case 2:
                    com = new SubFlowCommand(node, nodePre, action, context);
                    break;
                case 3:
                    com = new CombineAndCommand(node, nodePre, action, context);
                    break;
                case 4:
                    com = new CombineOrCommand(node, nodePre, action, context);
                    break;
                case 5:
                    com = new StartCommand(node, nodePre, action, context);
                    break;
                case 6:
                    com = new EndCommand(node, nodePre, action, context);
                    break;
                default:
                    break;
                
            }
            com.TaskAll = listTask;//任务从外面加载进来
            return com;
        }
        public static Command CreateCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode nodePre, ObjectContext context, ActionPointEnum action = ActionPointEnum.Accept, bool isSave = true)
        {
            Command com = null;
            var flowNode = Platform.DAL.DBFactory.CreateFlowDB().OAO_WorkFlowNode.FirstOrDefault(e => e.Id == node.WorkFlowNodeId);
            switch (flowNode.NodeType)
            {
                case 0:
                    com = new NormalCommand(node, nodePre, action, context, isSave);
                    break;
                case 1:
                    com = new CompetationCommand(node, nodePre, action, context, isSave);
                    break;
                case 2:
                    com = new SubFlowCommand(node, nodePre, action, context);
                    break;
                case 3:
                    com = new CombineAndCommand(node, nodePre, action, context);
                    break;
                case 4:
                    com = new CombineOrCommand(node, nodePre, action, context);
                    break;
                case 5:
                    com = new StartCommand(node, nodePre, action, context);
                    break;
                case 6:
                    com = new EndCommand(node, nodePre, action, context);
                    break;
                default:
                    break;
            }
            return com;
        }



    }

}
