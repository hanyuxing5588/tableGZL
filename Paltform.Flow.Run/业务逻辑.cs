using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Infrastructure;
using Platform.Flow.Run;
using Platform.Model;
using System.Data.Objects;
using Infrastructure.Expression;
namespace Platform.Flow.Run
{
    
    //1、有FlowNode变为ProcessNode 2、注册节点 3、判断是否发送 否返回 4、发送 5、注册下一节点 6、结束

     internal class StartCommand : Command
    {
        public StartCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
        {
            this._node = node;
            this._nodePre = preNode;
            this._action = action;
        }
        /// <summary>
        /// 构造函数

        /// </summary>
        /// <param name="node">当前节点 有FlowNode转换过来</param>
        /// <param name="preNode">上一节点 例如判断节点是否在当前容器内 取得processID </param>
        /// <param name="action">是发送还是拒绝</param>
        /// <param name="context">上下文</param>
        /// <param name="isSave">当前上下文在send后是否保存</param>danban
        public StartCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
            : this(node, preNode, action)
        {
            this.Context = context;
            this.isSave = isSave;
        }
        public override void PerformTask(TaskActionEnum Action)
        {
            base.PerformTask(Action);
        }

        public override void Terminate()
        {
            //开始 是一个即收即发的节点
            this.Node.State = (int)ProcessNodeStateEnum.已处理;
        }

        public override bool PermitSend()
        {
            return true;
        }

        public override bool PermitPerformTask()
        {
            //节点执行任务 当第一个人执行的时候即执行了任务 以后再次经过该节点将不执行任务

            return true;
        }

        public override void RegisterNext()
        {
            var nodes = this.CreateNextNodes();
            foreach (var item in nodes)
            {
                Command com = CreateCommand(item, this.Node, this.Context, this.Action);
                com.TaskAll = TaskAll;
                com.Comment = this.Comment;
                com.CurrentExecutor = this.CurrentExecutor;
                com.OperatorId = this.OperatorId;
                com.Register();
                //接收时

                com.PerformTask(TaskActionEnum.接收);
            }
        }

        public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
        {
            var guid = this.Node.WorkFlowNodeId;
            if (i == ActionPointEnum.Accept)
            {
                return GetFlowNodeByAction(fc, guid,false);
            }
            else 
            {
                Guid proId = this.Node.WorkFlowProcessId;
                var process = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == proId && e.ParentId != null && e.ParentNodeId != null);//           if(this.Node.Process.TriggerProcessId
                if (process != null)//是子流程呢


                {
                    //找到触发进程的节点
                    var entProcessNode = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowNode").FirstOrDefault(e => e.Id == process.ParentNodeId);
                    //找到流程的FlowNode节点
                    //var flowChart = FlowChart.DeSerialize(entProcessNode.Process.Xml);
                    var entFlowNode = entProcessNode.OAO_WorkFlowNode;
                    return new List<OAO_WorkFlowNode>() { entFlowNode };
                }
            }
            return new List<OAO_WorkFlowNode>() { };
        }

        private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid guid, bool isReject)
        {
            var entListGuid = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
            return fc.OAO_WorkFlowNode.Where(e => entListGuid.Contains(e.Id) == true).ToList();
        }


        public override void Send()
        {
            //执行任务
            this.PerformTask(TaskActionEnum.发送);
            //发送

            if (this.PermitSend())
            {
                this.RegisterNext();
                this.Terminate();
            }
            this.SaveChanges();//改写 获得context的方法 在复杂流转中 使context执行保存一次.
        }

        public override void Register()
        {
            this.Node.SendDate = DateTime.Now;
            this.Node.ReciveDate = DateTime.Now;
            this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
           
            this.Node.ActionResult=(int)ActionPointEnum.Accept;
            this.Node.IsTaskOver = true;
            this.Node.IsAllTaskOver = true;
            this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
            
            this.Context.SaveChanges();


        }
    }
     internal class NormalCommand : Command
     {
         public NormalCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
         {
             this._node = node;
             this._nodePre = preNode;
             this._action = action;
         }
         public NormalCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
             : this(node, preNode, action)
         {
             this.Context = context;
             this.isSave = isSave;
         }
         public override void PerformTask(TaskActionEnum Action)
         {
             if (this.PermitPerformTask())
             {
                 base.PerformTask(Action);
             }
         }

         public override void Terminate()
         {
             //所有执行人改为已经办理;
             this.Node.SetExecutorsState(this.CurrentExecutor, ProcessNodeExecutorStateEnum.替办);
             this.Node.State = (int)ProcessNodeStateEnum.已处理;
             //重新赋值 如果是拒绝改变当前节点的action
             this.Node.ActionResult = (int)this.Action;
             this.Node.SendDate = DateTime.Now;
         }

         public override bool PermitSend()
         {
             var node = this.Node;
             if (node.State == (int)ProcessNodeStateEnum.已处理) return false;
             bool isSend = false;
             //获得人 改状态


             var executors = node.OAO_WorkFlowProcessNodeExecutor;
             //拒绝和任一是相等的
             if (node.OAO_WorkFlowNode.Strategy == (int)ExecutorStrategyEnum.任一 || this.Action == ActionPointEnum.Reject)
             {
                 isSend = true;
                 node.SetExecutorsState(this.CurrentExecutor, Comment, this.Context);
             }
             else
             {
                 isSend = true;
                 //找最后一个人是不是自己


                 foreach (var item in executors)
                 {
                     if (item.State != 2)
                     {
                         if (item.ExecutorId != this.CurrentExecutor)
                         {
                             isSend = false;
                             break;
                         }
                     }
                 }
                 node.SetExecutorsState(this.CurrentExecutor,Comment,this.Context);
             }
             this.Node.State = (int)ProcessNodeStateEnum.未处理;
             return isSend;
         }
         //发送短信和邮件
         public void SendMsgAndEmail()
         {
             //var strUserIds = this.Node.Executors;
             //var guidUserIds = Tools.StrChangeListGuid(strUserIds);
             //var entUsers = this.Context.CreateObjectSet<UserAccount.Models.UserAccount>().Where(e => guidUserIds.Contains(e.Id)).ToList();

             //if (this.NodePre.bInformMail == true||this.NodePre.bInformMessage==true) 
             //{
             //    foreach (var item in entUsers)
             //    {
             //        if(this.NodePre.bInformMail == true)
             //        SendEmailManager.SendEmail(item.Email);
             //        if (this.NodePre.bInformMail == true)
             //        SendSMSManager.SendSms(item.Phone);
             //    }

             //}
         }

         public override bool PermitPerformTask()
         {
             return true;
         }

         public override void RegisterNext()
         {
             var nodes = this.CreateNextNodes();
             foreach (var item in nodes)
             {
                 Command com = CreateCommand(item, this.Node, this.Context, this.Action);
                 com.TaskAll = TaskAll;
                 com.Comment = this.Comment;
                 com.CurrentExecutor = this.CurrentExecutor;
                 com.OperatorId = this.OperatorId;
                 com.Register();
                 //拒绝的时候刷新内置变量值

                 if (this.Action == ActionPointEnum.Reject)
                 {
                     OAO_WorkFlowProcessData wfpd = item.FindData(this.Context);
                     item.SetCurrentData(this.Context);
                 }


                 //接收时

                 com.PerformTask(TaskActionEnum.接收);
             }
         }

         public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
         {
             var guid = this.Node.WorkFlowNodeId;
             if (i == ActionPointEnum.Accept)
             {
                 return GetFlowNodeByAction(fc, guid, false);
             }
             else
             {
                 return GetFlowNodeByAction(fc, guid, true);
             }
         }

         /// <summary>
         /// 根据拒绝或者接受行为获取流程节点


         /// </summary>
         /// <param name="fc"></param>
         /// <param name="guid"></param>
         /// <returns></returns>
         private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid guid, bool isReject)
         {
             var entListGuid = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
             return fc.OAO_WorkFlowNode.Where(e => entListGuid.Contains(e.Id) == true).ToList();
         }

         public override void Send()
         {
             switch (this.Action)
             {
                 case ActionPointEnum.Accept:
                     this.PerformTask(TaskActionEnum.发送);
                     break;
                 case ActionPointEnum.Reject:
                     this.PerformTask(TaskActionEnum.拒绝);
                     break;
                 default:
                     this.PerformTask(TaskActionEnum.发送);
                     break;
             }

             if (this.PermitSend())
             {
                 this.RegisterNext();
                 this.Terminate();
             }
             this.SaveChanges();//有可能没发送 但也要改变processNode的状态



             this.SendMsgAndEmail();
         }

         public override void Register()
         {
             DateTime dt = DateTime.Now;
             this.Node.ReciveDate = dt;
             this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
             this.Node.ActionResult = (int)this.Action;
             this.Node.IsTaskOver = true;
             this.Node.IsAllTaskOver = true;
           
             this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);

             this.SendMsgAndEmail();
             this.Context.SaveChanges();//11
         }
     }
     internal class CompetationCommand : Command
     {
         public CompetationCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
         {
             this._node = node;
             this._nodePre = preNode;
             this._action = action;
         }
         public CompetationCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
             : this(node, preNode, action)
         {
             this.Context = context;
             this.isSave = isSave;
         }
         public override void PerformTask(TaskActionEnum Action)
         {
             if (this.PermitPerformTask())
             {
                 base.PerformTask(Action);
             }
         }

         public override void Terminate()
         {
             this.Node.ReciveDate = DateTime.Now;
             this.Node.State = (int)ProcessNodeStateEnum.已处理;
         }

         public override bool PermitSend()
         {
             return true;
         }

         public override bool PermitPerformTask()
         {
             return true;
         }

         public override void RegisterNext()
         {
             //竞争节点
             var nodes = this.CreateNextNodes();
             foreach (var item in nodes)
             {
                 Command com = CreateCommand(item, this.Node, this.Context, this.Action);
                 com.CurrentExecutor = this.CurrentExecutor;
                 com.TaskAll = TaskAll;
                 com.Comment = this.Comment;
                 com.OperatorId = this.OperatorId;
                 com.Register();
                 //接收时

                 com.PerformTask(TaskActionEnum.接收);
             }
         }
         //在找下一节点的时候 利用条件
         public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
         {
             var guid = this.Node.WorkFlowNodeId;
             if (i == ActionPointEnum.Reject)
             {
                 return GetFlowNodeByAction(fc, guid, true);
             }
             Guid fitGuid = Guid.Empty;
             var flowNode = fc.OAO_WorkFlowNode.First(e => e.Id == guid);
             var isYXJ = flowNode.IsFirst;
             var entListGuid = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.IsReject == false)
                 //.Select(e => new
                 //{
                 //    yxj = e.Priority,
                 //    toId = e.TargetNodeId,
                 //    conditions=e.Conditions,
                 //    id=e.Id
                 //})
             .OrderByDescending(e => e.Priority).ToList();//.OrderBy(e => e.yxj)
             //优先级排序 
             //1、选优先级最高并且符合条件的第一个


             //2、符合条件的第一个


             var hsTable = this.Node.GetVariables();
             foreach (var item in entListGuid)
             {
                 object relust = null;
                 string strFitler = item.GetConditionStr(hsTable);
                 try
                 {
                     ExpressionParser ep = new ExpressionParser();
                     ep.Parser(strFitler, out relust);
                 }
                 catch (Exception ex)
                 {
                       
                 }
                
                 if (relust != null && bool.Parse(relust.ToString().ToLower()) == true)
                 {
                     fitGuid = item.TargetNodeId;
                     break;
                 }
             }
             return fc.OAO_WorkFlowNode.Where(e => e.Id == fitGuid).ToList();
         }

         private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid guid, bool isReject)
         {
             var entListGuid1 = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
             return fc.OAO_WorkFlowNode.Where(e => entListGuid1.Contains(e.Id) == true).ToList();
         }

         public override void Send()
         {
             switch (this.Action)
             {
                 case ActionPointEnum.Accept:
                     this.PerformTask(TaskActionEnum.发送);
                     break;
                 case ActionPointEnum.Reject:
                     this.PerformTask(TaskActionEnum.拒绝);
                     break;
                 default:
                     this.PerformTask(TaskActionEnum.发送);
                     break;
             }
             if (this.PermitSend())
             {
                 this.RegisterNext();
                 this.Terminate();
             }
             this.SaveChanges();//有可能没发送 但也要改变processNode的状态


         }

         public override void Register()
         {
             DateTime dt = DateTime.Now;
             this.Node.ReciveDate = dt;
             this.Node.SendDate = dt;
             //this.Node.ProNodeId = this.NodePre.Id;
             this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
            
             this.Node.Id = Guid.NewGuid();
             this.Node.ActionResult = (int)this.Action;
             this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
             this.Send();
         }
     }
     internal class CombineAndCommand : Command
     {
         public CombineAndCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
         {
             this._node = node;
             this._nodePre = preNode;
             this._action = action;
         }
         public CombineAndCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
             : this(node, preNode, action)
         {
             this.Context = context;
             this.isSave = isSave;
         }
         public override void PerformTask(TaskActionEnum Action)
         {
             base.PerformTask(Action);
         }
         private bool bIsPassed = false;
         public override void Terminate()
         {
             if (!bIsPassed) return;
             var node = this.Node;
             var nodeChartId = node.WorkFlowNodeId;
             var nodes = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>()
                 .Where(e => e.WorkFlowProcessId == node.WorkFlowProcessId && e.CombineNodeId == nodeChartId).ToList();
             foreach (var item in nodes)
             {
                 item.SetExecutorsState(this.CurrentExecutor, ProcessNodeExecutorStateEnum.替办);
             }
             //此状态不生效
             this.Node.State = (int)ProcessNodeStateEnum.已处理;
         }
         public override bool PermitSend()
         {
             var node = this.Node;
             bool isSend = true;
             if (node.CombineNodeId == this.NodePre.CombineNodeId)
             {
                 //判断是不是最后一个节点 //看方位点3
                 var nodes = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().Where(e => e.WorkFlowProcessId == node.WorkFlowProcessId && e.CombineNodeId == node.WorkFlowNodeId).ToList();
                 if (this.Action == ActionPointEnum.Reject)
                 {
                     foreach (var item in nodes)
                     {
                         item.State = (int)ProcessNodeStateEnum.已处理;
                     }
                     isSend = true;
                 }
                 else
                 {
                     foreach (var item in nodes)
                     {
                         if (item.State != (int)ProcessNodeStateEnum.已处理 && item.WorkFlowNodeId != this.NodePre.WorkFlowNodeId)
                         {
                             isSend = false;
                             break;
                         }
                     }
                 }
                 this.bIsPassed = isSend;
             }
             this.Node.State = (int)ProcessNodeStateEnum.等待;
             return isSend;
         }
         public override bool PermitPerformTask()
         {
             return true;
         }
         public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
         {
             var guid = this.Node.WorkFlowNodeId;
             //如果是拒绝 下一节点即为节点的容器



             if (i == ActionPointEnum.Reject)
             {
                 int sourceArch = 1;
                 if (this.NodePre.CombineNodeId != guid)
                 {
                     sourceArch = 3;
                 }
                 return GetFlowNodeByAction(fc, guid, true, sourceArch);
             }
             else
             {
                 int sourceArch = 3;//在容器里面向外面发送 也可以用ID是否等于容器判断
                 if (this.NodePre.CombineNodeId != guid)
                 {
                     sourceArch = 1;//在容器的外面向里面发送



                 }
                 //var entListGuid = fc.FlowRoutes.Where(e => e.SourceNodeId == guid && e.SourceArch.Value == sourceArch && e.Reject == false).Select(e => e.TargetNodeId).ToList();
                 //return fc.FlowNodes.Where(e => entListGuid.Contains(e.Id) == true).ToList();
                 return GetFlowNodeByAction(fc, guid, false, sourceArch);
             }
         }
         private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid guid, bool isReject, int sourceArch)
         {
             var entListGuid1 = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.SourceArch.Value == sourceArch && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
             return fc.OAO_WorkFlowNode.Where(e => entListGuid1.Contains(e.Id) == true).ToList();
         }
         public override void RegisterNext()
         {
             var nodes = this.CreateNextNodes();
             foreach (var item in nodes)
             {
                 Command com = CreateCommand(item, this.Node, this.Context, this.Action);
                 com.Comment = this.Comment;
                 com.CurrentExecutor = this.CurrentExecutor;
                 com.TaskAll = TaskAll;
                 com.OperatorId = this.OperatorId;
                 com.Register();
             }
         }
         public override void Send()
         {
             if (this.PermitSend())
             {
                 this.RegisterNext();
                 this.Terminate();
             }
             this.Context.SaveChanges();//有可能没发送 但也要改变processNode的状态


         }
         public override void Register()
         {
             if (this.NodePre.CombineNodeId == null)
             {
                 DateTime dt = DateTime.Now;
                 this.Node.ReciveDate = dt;
                 this.Node.SendDate = dt;
                 //this.Node.ProNodeId = this.NodePre.Id;
               
                 this.Node.Id = Guid.NewGuid();
                 this.Node.ActionResult = (int)this.Action;
                 this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
             }
             this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
             this.Send();
         }
     }
     internal class CombineOrCommand : Command
     {
         public CombineOrCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
         {
             this._node = node;
             this._nodePre = preNode;
             this._action = action;
         }
         public CombineOrCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
             : this(node, preNode, action)
         {
             this.Context = context;
             this.isSave = isSave;
         }
         public override void PerformTask(TaskActionEnum Action)
         {
             base.PerformTask(Action);
         }
         public override void Terminate()
         {
             if (this.NodePre.CombineNodeId != this.Node.WorkFlowNodeId) return;
             var node = this.Node;

             var nodes = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>()
                 .Where(e => e.WorkFlowProcessId == node.WorkFlowProcessId && (e.CombineNodeId == node.WorkFlowNodeId || e.WorkFlowNodeId == node.WorkFlowNodeId)).ToList();//this.Node.Id
             foreach (var item in nodes)
             {
                 if (item.WorkFlowNodeId == this.Node.WorkFlowNodeId)
                 {
                     item.State = (int)ProcessNodeStateEnum.已处理;
                 }
                 else
                 {
                     item.SetExecutorsState(this.CurrentExecutor, ProcessNodeExecutorStateEnum.替办);
                 }
             }
         }

         public override bool PermitSend()
         {
             //var node = this.Node;
             //bool isSend = true ;
             //if (node.Id == this.NodePre.CombineNodeId)
             //{
             //   isSend = true;
             //}
             //return isSend;
             this.Node.State = (int)ProcessNodeStateEnum.等待;
             return true;
         }

         public override bool PermitPerformTask()
         {
             return true;
         }


         public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
         {

             var guid = this.Node.WorkFlowNodeId;
             if (i == ActionPointEnum.Reject)
             {
                 int sourceArch = 1;
                 if (this.NodePre.CombineNodeId != guid)
                 {
                     sourceArch = 3;
                 }
                 bool isReject = true;
                 return GetFlowNodeByAction(fc, guid, isReject, sourceArch);
             }
             else
             {
                 int sourceArch = 3;//在容器里面向外面发送 也可以用ID是否等于容器判断
                 if (this.NodePre.CombineNodeId != guid)
                 {
                     sourceArch = 1;//在容器的外面向里面发送



                 }
                 //var entListGuid = fc.FlowRoutes.Where(e => e.SourceNodeId == guid && e.SourceArch.Value == sourceArch && e.Reject == false).Select(e => e.TargetNodeId).ToList();
                 //return fc.FlowNodes.Where(e => entListGuid.Contains(e.Id) == true).ToList();
                 return GetFlowNodeByAction(fc, guid, false, sourceArch);
             }
         }

         private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid guid, bool isReject, int sourceArch)
         {
             var entListGuid = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == guid && e.SourceArch.Value == sourceArch && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
             return fc.OAO_WorkFlowNode.Where(e => entListGuid.Contains(e.Id) == true).ToList();
         }



         public override void RegisterNext()
         {
             var nodes = this.CreateNextNodes();
             foreach (var item in nodes)
             {
                 Command com = CreateCommand(item, this.Node, this.Context, this.Action);
                 com.Comment = this.Comment;
                 com.CurrentExecutor = this.CurrentExecutor;
                 com.TaskAll = TaskAll;
                 com.OperatorId = this.OperatorId;
                 com.Register();
             }
         }

         public override void Send()
         {
             if (this.PermitSend())
             {
                 this.RegisterNext();
                 this.Terminate();
             }
             this.SaveChanges();//有可能没发送 但也要改变processNode的状态



         }

         public override void Register()
         {
             if (this.NodePre.CombineNodeId == null)
             {
                 DateTime dt = DateTime.Now;
                 this.Node.ReciveDate = dt;
                 this.Node.SendDate = dt;
                 this.Node.Id = Guid.NewGuid();
                
                 //this.Node.ProNodeId = this.NodePre.Id;
                 this.Node.ActionResult = (int)this.Action;
                 this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
             }
             this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
             this.Send();
         }
     }
     internal class SubFlowCommand : Command
     {
         public SubFlowCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
         {
             this._node = node;
             this._nodePre = preNode;
             this._action = action;
         }
         public SubFlowCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = false)
             : this(node, preNode, action)
         {
             this.Context = context;
             this.isSave = isSave;
         }
         public override void PerformTask(TaskActionEnum Action)
         {
             base.PerformTask(Action);
         }

         public override void Terminate()
         {
             //if (this.firstFalg) return;

         }
         public override bool PermitSend()
         {
             if (this.Action != ActionPointEnum.Reject)
             {
                 this.Node.State = (int)ProcessNodeStateEnum.等待;
             }
             return true;
         }

         public override bool PermitPerformTask()
         {
             return true;
         }

         public override void RegisterNext()
         {
             var node = this.Node;
             var nodes = this.CreateNextNodes();
             foreach (var item in nodes)
             {

                 if (item.OAO_WorkFlowNode.NodeType == (int)NodeTypeEnum.开始)//是开始节点


                 {
                     OAO_WorkFlowProcessNode pn = new OAO_WorkFlowProcessNode();
                     pn.WorkFlowProcessId = this.processId;
                     Command com = CreateCommand(item, pn, this.Context, this.Action);
                     com.Comment = this.Comment;
                     com.CurrentExecutor = this.CurrentExecutor;
                     com.TaskAll = TaskAll;
                     com.OperatorId = this.OperatorId;
                     com.Register();
                     if (this.SubFlowChart.IsAutoStart == true) //自动发送
                     {
                         com.Send();
                     }
                 }
                 else
                 {
                     Command com = CreateCommand(item, node, this.Context, this.Action);
                     com.Comment = this.Comment;
                     com.CurrentExecutor = this.CurrentExecutor;
                     com.TaskAll = TaskAll;
                     com.Register();

                 }
             }

             var subFlowProcess = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.ParentId == node.WorkFlowProcessId && e.ParentNodeId == node.Id);
             if (subFlowProcess != null)
             {
                 subFlowProcess.State = (int)ProcessStateEnum.运行;
             }
             if (this.Action != ActionPointEnum.Reject)
             {
                 this.Node.State = (int)ProcessNodeStateEnum.未处理;
             }
         }

         public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
         {
             var node = this.Node;
             var nodeMappingSubFlow = this.SubFlowChart;
             //拒绝
             if (i == ActionPointEnum.Reject)
             {
                 // FlowChart fc1 = FlowChart.DeSerialize(nodeMappingSubFlow.Xml);
                 return GetFlowNodeByAction(fc, this.Node.WorkFlowNodeId, true);
                 //if (flowNodeList == null)
                 //{
                 //    return null;
                 //}
                 //return GetFlowNodeByAction(fc, flowNodeList[0].Id, true);
                 //var entEnd = fc1.FlowNodes.FirstOrDefault(e => e.Type == NodeTypeEnum.结束);
                 //if (entEnd != null)
                 //{
                 //return new List<FlowNode>() { entEnd };  

                 //}
             }
             //接受

             if (this.firstFalg)
             {
                 OAO_WorkFlow fc1 = this.Context.CreateObjectSet<OAO_WorkFlow>().Include("OAO_WorkFlowNode").Include("OAO_WorkFlowRoute").FirstOrDefault(e => e.Id == nodeMappingSubFlow.Id);
                 var entStart = fc1.OAO_WorkFlowNode.FirstOrDefault(e => e.NodeType == 5);// NodeTypeEnum.开始);
                 if (entStart != null)
                 {
                     return new List<OAO_WorkFlowNode>() { entStart };
                 }
             }
             else
             {
                 fc = nodeMappingSubFlow;
                 return GetFlowNodeByAction(fc, node.WorkFlowNodeId, false);
             }
             return new List<OAO_WorkFlowNode>();
         }

         private static List<OAO_WorkFlowNode> GetFlowNodeByAction(OAO_WorkFlow fc, Guid nodeId, bool isReject)
         {
             var entListGuid = fc.OAO_WorkFlowRoute.Where(e => e.SourceNodeId == nodeId && e.IsReject == isReject).Select(e => e.TargetNodeId).ToList();
             return fc.OAO_WorkFlowNode.Where(e => entListGuid.Contains(e.Id) == true).ToList();
         }

         public override void Send()
         {
             if (this.PermitSend())
             {
                 this.RegisterNext();
                 this.Terminate();
             }
             this.SaveChanges();//有可能没发送 但也要改变processNode的状态


         }
         //当前节点 对应 流程的flowchart
         private OAO_WorkFlow SubFlowChart = null;
         private bool firstFalg = false;
         private Guid processId = Guid.Empty;
         public override void Register()
         {
             if (this.Action == ActionPointEnum.Reject)
             {
                 //var entProcess = this.Context.CreateObjectSet<Process>().FirstOrDefault(e => e.Id == this.NodePre.ProcessId);//主流程



                 //process转换成flowchart
                 //通过flowchart找到 flownode
                 //this.Node.ProcessId = this.NodePre.Process.Id;
                 //this.Node.Id = Guid.NewGuid();
                 //this.Node.RestrainAction =this.Action;
                 //this.Node.Process = entProcessNode.Process;
                 DateTime dt = DateTime.Now;
                 this.Node.ReciveDate = dt;
                 this.Node.SendDate = dt;
                 //this.Node.ProNodeId = this.NodePre.Id;
                 this.Node.ActionResult = (int)this.Action;
                 this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
               
                 this.Node.Id = Guid.NewGuid();
                 this.Node.State = (int)ProcessNodeStateEnum.已处理;
                 this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
                 this.Send();
                 return;
             }
             var entProcess1 = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == this.NodePre.WorkFlowProcessId && e.ParentId != null);
             if (entProcess1 == null)//判断是 开始  还是 结束
             {//开始


                 firstFalg = true;
                 DateTime dt = DateTime.Now;
                 this.Node.ReciveDate = dt;
                 this.Node.SendDate = dt;
                 //this.Node.ProNodeId = this.NodePre.Id;
                 this.Node.ActionResult = (int)this.Action;
                 this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
                 this.Node.Id = Guid.NewGuid();
                 this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
                 //新建流程
                 var entProcess = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().Include("OAO_WorkFlow")
                     .Include("OAO_WorkFlow.OAO_WorkFlowNode").Include("OAO_WorkFlow.OAO_WorkFlowRoute").FirstOrDefault(e => e.Id == this.Node.WorkFlowProcessId);
                 OAO_WorkFlow fc = entProcess.OAO_WorkFlow;
                 var flowNode = fc.OAO_WorkFlowNode.FirstOrDefault(e => e.Id == this.Node.WorkFlowNodeId);
                 if (flowNode != null)
                 {
                     var flow = this.Context.CreateObjectSet<OAO_WorkFlow>().First(e => e.Id == flowNode.SubWorkFlowId);
                     if (flow != null)
                     {
                         var ent = this.Context.CreateObject<OAO_WorkFlowProcess>();
                         ent.ParentId = this.Node.WorkFlowProcessId;
                         ent.ParentNodeId = this.Node.Id;
                         ent.Id = Guid.NewGuid();
                         ent.State = (int)ProcessStateEnum.运行;
                         ent.CreateDate = DateTime.Now;
                         ent.Createor = this.CurrentExecutor;
                         this.Context.CreateObjectSet<OAO_WorkFlowProcess>().AddObject(ent);
                         this.Context.SaveChanges();
                         this.SubFlowChart = flow;
                         this.processId = ent.Id;
                     }
                 }
             }
             else
             {
                 var entSubProcess = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == this.NodePre.WorkFlowProcessId);//子流程


                 var entProcessNode = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("Process").FirstOrDefault(e => e.WorkFlowProcessId == entSubProcess.ParentId && e.Id == entSubProcess.ParentNodeId);
                 entProcessNode.ReciveDate = DateTime.Now;
                 entProcessNode.State = (int)ProcessNodeStateEnum.已处理;
                 this.Node.WorkFlowProcessId = entProcessNode.WorkFlowProcessId;
                 this.Node.Id = entProcessNode.Id;
                 this.Node.ActionResult = entProcessNode.ActionResult;
                 this.Node.OAO_WorkFlowProcess = entProcessNode.OAO_WorkFlowProcess;
                 this.SubFlowChart = this.Context.CreateObjectSet<OAO_WorkFlow>()
                  .Include("OAO_WorkFlow.OAO_WorkFlowNode").Include("OAO_WorkFlow.OAO_WorkFlowRoute").FirstOrDefault(e => e.Id == entProcessNode.WorkFlowProcessId);
             }
             this.Send();
         }
     }
     internal class EndCommand : Command
    {
        public EndCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action)
        {
            this._node = node;
            this._nodePre = preNode;
            this._action = action;
        }
        public EndCommand(OAO_WorkFlowProcessNode node, OAO_WorkFlowProcessNode preNode, ActionPointEnum action, ObjectContext context, bool isSave = true)
            : this(node, preNode, action)
        {
            this.Context = context;
            this.isSave = isSave;
        }
        public override void PerformTask(TaskActionEnum Action)
        {
            base.PerformTask(Action);
        }

        public override void Terminate()
        {
            var entGuid = this.Node.WorkFlowProcessId;
            var nodes = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").Where(e => e.WorkFlowProcessId == entGuid).ToList();
            foreach (var item in nodes)
            {
                item.SetExecutorsState(this.CurrentExecutor,ProcessNodeExecutorStateEnum.替办);
               
              
                item.OAO_WorkFlowProcess.State =(int) ProcessStateEnum.完成;
            }
            this.Node.SendDate = DateTime.Now;
            this.Node.State = (int)ProcessNodeStateEnum.已处理;
            //修改流程运行时的状态仍然不起作用 状态仍然是不变
            var process = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == entGuid);
            process.State = (int)ProcessStateEnum.完成; ;
        }
        public override bool PermitSend()
        {
            if (this.Node.State ==(int) ProcessNodeStateEnum.已处理) return false;
            return true;
        }

        public override bool PermitPerformTask()
        {
            return true;
        }

        public override void RegisterNext()
        {
            var node = this.Node;
            var nodes = this.CreateNextNodes();
            foreach (var item in nodes)
            {
                Command com = CreateCommand(item, node, this.Context, this.Action);
                com.CurrentExecutor = this.CurrentExecutor;
                com.Comment = this.Comment;
                com.TaskAll = TaskAll;
                com.OperatorId = this.OperatorId;
                com.Register();
            }
        }

        public override List<OAO_WorkFlowNode> CreateNextNodes(OAO_WorkFlow fc, ActionPointEnum i)
        {
            //拒绝
            if (i == ActionPointEnum.Reject) 
            {
                //Guid proNodeID = (Guid)this.Node.ProNodeId;
                //var entPreProcessNode = this.Context.CreateObjectSet<ProcessNode>().FirstOrDefault(e => e.Id == proNodeID);
                //return fc.FlowNodes.Where(e => e.Id == entPreProcessNode.ChartNodeId).ToList(); 
                return new List<OAO_WorkFlowNode>();
            }
            Guid proId = this.Node.WorkFlowProcessId;
            //找到流程的ID 辨别时候是子流程

            var process = this.Context.CreateObjectSet<OAO_WorkFlowProcess>().FirstOrDefault(e => e.Id == proId && e.ParentNodeId != null && e.ParentId != null);//           if(this.Node.Process.TriggerProcessId
            if (process != null)//是子流程呢

            {
                //找到触发进程的节点


                var entProcessNode = this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().Include("OAO_WorkFlowProcess").FirstOrDefault(e => e.Id == process.ParentNodeId);
                //找到流程的FlowNode节点
                var flowChart =  this.Context.CreateObjectSet<OAO_WorkFlow>()
                 .Include("OAO_WorkFlow.OAO_WorkFlowNode").FirstOrDefault(e => e.Id == entProcessNode.WorkFlowProcessId);
                var entFlowNode = flowChart.OAO_WorkFlowNode.First(e => e.Id == entProcessNode.WorkFlowNodeId);
                return new List<OAO_WorkFlowNode>() { entFlowNode };
            }
            return new List<OAO_WorkFlowNode>() { };
        }

        //private static List<FlowNode> GetFlowNodeByAction(FlowChart fc, Guid nodeId,bool isReject)
        //{
        //    //改这里 
        //    var entListGuid = fc.FlowRoutes.Where(e => e.SourceNodeId == nodeId && e.Reject == isReject).Select(e => e.TargetNodeId).ToList();
        //    return fc.FlowNodes.Where(e => entListGuid.Contains(e.Id) == true).ToList();
        //}

        public override void Send()
        {
            if (this.PermitSend())
            {
                this.RegisterNext();
                this.Terminate();
            }
            this.SaveChanges();//有可能没发送 但也要改变processNode的状态

        }

        public override void Register()
        {
            DateTime dt = DateTime.Now;
            this.Node.ReciveDate = dt;
            this.Node.SendDate = dt;
            this.Node.ActionResult = (int)this.Action;
            //this.Node.ProNodeId = this.NodePre.Id;
            this.Node.WorkFlowProcessId = this.NodePre.WorkFlowProcessId;
           
            this.Node.Id = Guid.NewGuid();
            this.Context.CreateObjectSet<OAO_WorkFlowProcessNode>().AddObject(this.Node);
            this.Send();
        }
    }
}
