using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using Platform.Infrastructure;
using Platform.Infrastructure.Expression;
using System.Collections;
using System.Data.Objects;
namespace Platform.Flow.Run
{
    internal static  class EntityExt
    {
        /// <summary>
        /// 创建并初始化运行时节点实例

        /// </summary>
        /// <param name="obj">流程节点</param>
        /// <returns>运行时节点实例</returns>
        public static OAO_WorkFlowProcessNode CreateRunTime(this OAO_WorkFlowNode obj,Guid? processId=null)
        {
            OAO_WorkFlowProcessNode result = new OAO_WorkFlowProcessNode();
            result.WorkFlowNodeId = obj.Id;            
            result.CombineNodeId = obj.CombineNodeId;
            result.State = (int)ProcessNodeStateEnum.未处理;
            result.Id = Guid.NewGuid();
            //result.OAO_WorkFlowNode = obj;
            result.ConmbineNodeType = 10;//(obj.NodeType == 3 || obj.NodeType == 4)?obj.NodeType:null;
            result.SetExecutors(obj.GetExecutors(processId));
            return result;
        }
        //设置运行时节点执行人
        public static void SetExecutors(this OAO_WorkFlowProcessNode obj,List<OAO_WorkFlowNodeExecutor> executors)
        {
            foreach (var item in executors)
	        {
                OAO_WorkFlowProcessNodeExecutor ent = new OAO_WorkFlowProcessNodeExecutor();
                ent.Id = Guid.NewGuid();
                ent.ExecutorId = item.ExecutorId;
                ent.ExecutorVersion = item.ExecutorVersion;
                ent.ReciveDate = DateTime.Now;
                ent.WorkFlowProcessNodeId = obj.Id;
                obj.OAO_WorkFlowProcessNodeExecutor.Add(ent);
	        }
        
        }

        //设置运行时节点执行人
        public static void SetExecutorsState(this OAO_WorkFlowProcessNode obj,Guid Executor, ProcessNodeExecutorStateEnum state)
        {
            var executors = obj.OAO_WorkFlowProcessNodeExecutor;
            if (executors != null) {
                foreach (var item in executors)
                {
                    if (item.ExecutorId == Executor) continue;
                    item.State = (int)state;
                }
            }
        }
        //设置运行时节点执行人
        public static void SetExecutorsState(this OAO_WorkFlowProcessNode obj,Guid Executor)
        {
            var executors = obj.OAO_WorkFlowProcessNodeExecutor;
            if (executors != null)
            {
                OAO_WorkFlowProcessNodeExecutor executor = executors.FirstOrDefault(e => e.ExecutorId == Executor);
                executor.State = (int)ProcessNodeExecutorStateEnum.办理;
                executor.SendDate = DateTime.Now;
            }
            
        }
        //设置运行时节点执行人
        public static void SetExecutorsState(this OAO_WorkFlowProcessNode obj, Guid Executor,string comment,ObjectContext context)
        {
            var executor = context.CreateObjectSet<OAO_WorkFlowProcessNodeExecutor>().FirstOrDefault(e => e.ExecutorId == Executor);
            executor.State = (int)ProcessNodeExecutorStateEnum.办理;
            executor.Comment = comment;
            executor.SendDate = DateTime.Now;
            context.SaveChanges();
        }
        //获得运行时节点执行人
        public static ProcessNodeExecutorStateEnum GetExecutorsState(this OAO_WorkFlowProcessNode obj, Guid Executor)
        {
            ProcessNodeExecutorStateEnum state = ProcessNodeExecutorStateEnum.未阅;
            var executors = obj.GetExecutors();
            if (executors != null)
            {
                OAO_WorkFlowProcessNodeExecutor executor = executors.FirstOrDefault(e => e.ExecutorId == Executor);
                state =(ProcessNodeExecutorStateEnum) executor.State;//= (int)ProcessNodeExecutorStateEnum.办理;
            }
            return state;
        }
        //获得运行时节点执行人
        public static List<OAO_WorkFlowProcessNodeExecutor> GetExecutors(this OAO_WorkFlowProcessNode obj)
        {
            var db = Platform.DAL.DBFactory.CreateFlowDB();
            var executors = db.OAO_WorkFlowProcessNodeExecutor.Where(e => e.WorkFlowProcessNodeId == obj.Id).ToList();
            return executors;
        }
        ///根据流程节点 流程节点上配置的人

        public static List<OAO_WorkFlowNodeExecutor> GetExecutors(this OAO_WorkFlowNode obj,Guid? processId=null)
        {
            List<OAO_WorkFlowNodeExecutor> listUser = new List<OAO_WorkFlowNodeExecutor>();
            //FlowContextProvider.Current
            var db = Platform.DAL.DBFactory.CreateFlowDB();
            if (obj.IsRuntimeUser != true)
            {
                listUser = db.OAO_WorkFlowNodeExecutor.Where(e => e.WorkFlowNodeId == obj.Id).ToList();
                //角色折射出来的人
                var usersByRole = obj.GetExecutorsByRole(db);
                if (usersByRole != null && usersByRole.Count > 0)
                {
                    listUser.AddRange(usersByRole);
                }
            }
            else {
                var entRunUsers = db.OAO_WorkFlowProcessRuntimeUsers.Where(e => e.WorkFlowProcessId == processId && e.WorkFlowNodeId == obj.Id).ToList();
                if (entRunUsers.Count>0) 
                {
                    listUser = entRunUsers.Select(e => new OAO_WorkFlowNodeExecutor { 
                       ExecutorId=e.ExecutorId==null?Guid.Empty:(Guid)e.ExecutorId,
                       ExecutorVersion=e.ExecutorVersion==null?1:(int)e.ExecutorVersion
                    }).ToList();
                }
            }
            return listUser;
        }
        //根据流程节点 找到角色 所对应的人
        public static List<OAO_WorkFlowNodeExecutor> GetExecutorsByRole(this OAO_WorkFlowNode obj,ObjectContext context)
        {
            List<OAO_WorkFlowNodeExecutor> listUser = new List<OAO_WorkFlowNodeExecutor>();
            try
            {
                string sqlFormat = " select newid() as Id,GUID as  ExecutorId,1 as ExecutorVersion,convert(uniqueidentifier,'{0}') as WorkFlowNodeId" +
                " from ss_operator where GUID in (select GUID_Operator from SS_RoleOperator where GUID_ROle in("+
                "select RoleId from dbo.OAO_WorkFlowNodeRole where WorkFlowNodeId='{0}'))";
            listUser= context.ExecuteStoreQuery<OAO_WorkFlowNodeExecutor>(string.Format(sqlFormat,obj.Id)).ToList();
            }
            catch (Exception)
            {

                throw;
            }
            return listUser;
        }
        /// <summary>
        /// 根据条件 转换 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Hashtable GetVariables(this OAO_WorkFlowProcessNode obj)
        {
            Hashtable hs=new Hashtable ();
            var db = Platform.DAL.DBFactory.CreateFlowDB();
            var routeConditions = db.OAO_WorkFlowProcessVariable.Include("OAO_WorkFlowVariable").Where(e =>e.WorkFlowProcessId==obj.WorkFlowProcessId)
                .Select(e=>new {
                        name=e.OAO_WorkFlowVariable.Name,val=e.VarValue}).ToList();
            foreach (var item in routeConditions)
	        {
		        hs.Add(item.name,item.val);
	        }
            return hs;
        }
        public static string GetConditionStr(this OAO_WorkFlowRoute route, Hashtable hsTable)
        {
            var db = Platform.DAL.DBFactory.CreateFlowDB();
            var conditions = db.OAO_WorkFlowRouteCondition.Where(e => e.WorkFlowRouteId == route.Id).ToList();
            StringBuilder sb = new StringBuilder();
            foreach (var item in conditions)
            {
                sb.Append(item.IsLeftBracket == true ? "(" : "");
                string varTemp = hsTable[item.VariableName].ToString();
                string strTemp = GetOpreatorMapFun(item.Operator, varTemp, item.VarValue);
                sb.Append(strTemp);
                sb.Append(item.IsRightBracket == true ? ")" : "");
                if (!string.IsNullOrEmpty(item.Relation))
                {
                    sb.Append(item.Relation.ToLower() == "and" ? "&&" : "||");
                }
            }
            return sb.ToString();
        }
        private static string GetOpreatorMapFun(string strOpreator, string variable, string strValue)
        {
            string strReturn = "", strFormat = ""; FunctionAuxiliary fa;
            switch (strOpreator.ToLower())
            {
                case "in":
                    strFormat = "@In(\"{0}\",\"{1}\")";
                    strFormat = string.Format(strFormat, variable, strValue);
                    fa = new InAuxiliary();
                    strReturn = fa.Perform(strFormat);
                    break;
                case "like":
                    strFormat = "@Like(\"{0}\",\"{1}\")";
                    strFormat = string.Format(strFormat, variable, strValue);
                    fa = new LikeAuxiliary();
                    strReturn = fa.Perform(strFormat);
                    break;
                case "startwith":
                    strFormat = "@StartWith(\"{0}\",\"{1}\")";
                    strFormat = string.Format(strFormat, variable, strValue);
                    fa = new StartWithAuxiliary();
                    strReturn = fa.Perform(strFormat);
                    break;
                case "endwith":
                    strFormat = "@EndWith(\"{0}\",\"{1}\")";
                    strFormat = string.Format(strFormat, variable, strValue);
                    fa = new EndWithAuxiliary();
                    strReturn = fa.Perform(strFormat);
                    break;
                default:
                    strReturn = variable + strOpreator + strValue;
                    break;
            }
            return strReturn;
        }
        public static OAO_WorkFlowProcess CreateRunTime(this OAO_WorkFlow obj,Guid executor)
        {
            OAO_WorkFlowProcess process = new OAO_WorkFlowProcess();
            process.Id = Guid.NewGuid();
            process.WorkFlowId = obj.Id;
            process.WorkFlowVersion = obj.Version;
            process.State =(int) ProcessStateEnum.运行;
            process.Createor = executor;
            process.CreatorVersion = 1;
            process.CreateDate = DateTime.Now;
            foreach (var item in obj.OAO_WorkFlowVariable)
	        {
		        OAO_WorkFlowProcessVariable owpv = new OAO_WorkFlowProcessVariable();
                owpv.Id = Guid.NewGuid();
                owpv.WorkFlowProcessId = process.Id;
                owpv.WorkFlowVariableId = item.Id;
                owpv.VarValue = item.VarValue;
                process.OAO_WorkFlowProcessVariable.Add(owpv);
	        }
            return process;

        }
        //public static void StartProcess(this OAO_WorkFlowProcess obj, ObjectContext context, OAO_WorkFlow FlowChart,Guid creatorID,Guid dataGuid,int classId,string docUrl)
        //{
        //    var flownode = FlowChart.OAO_WorkFlowNode.FirstOrDefault(e => e.NodeType ==5);
        //    var ent = new OAO_WorkFlowProcessNode();
        //    ent.WorkFlowProcessId = obj.Id;
        //    var com = new StartCommand(flownode.CreateRunTime(), ent, ActionPointEnum.Accept,context);
        //    com.CurrentExecutor = creatorID;
        //    com.Register();
        //    obj.AddDataAndVariables(context, 1, dataGuid, docUrl); //添加业务数据与流程之间的关系并且把业务数据写到流程变量中
        //    com.Send();
        //}
        public static void StartProcess(this OAO_WorkFlowProcess obj, ObjectContext context, OAO_WorkFlow FlowChart, Guid creatorID, Guid dataGuid, int classId, string docUrl,List<Type> tasks)
        {
            var flownode = FlowChart.OAO_WorkFlowNode.FirstOrDefault(e => e.NodeType == 5);
            var ent = new OAO_WorkFlowProcessNode();
            ent.WorkFlowProcessId = obj.Id;
            var com = new StartCommand(flownode.CreateRunTime(), ent, ActionPointEnum.Accept, context);
            com.TaskAll = tasks;
            com.CurrentExecutor = creatorID;
            com.Register();
            obj.AddDataAndVariables(context, 1, dataGuid, docUrl); //添加业务数据与流程之间的关系并且把业务数据写到流程变量中
            com.Send();
        }
        public static void StartProcess(this OAO_WorkFlowProcess obj, ObjectContext context, OAO_WorkFlow FlowChart, Guid creatorID, Guid dataGuid, int classId, string docUrl, List<Type> tasks,Guid OperatorId)
        {
            var flownode = FlowChart.OAO_WorkFlowNode.FirstOrDefault(e => e.NodeType == 5);
            var ent = new OAO_WorkFlowProcessNode();
            ent.WorkFlowProcessId = obj.Id;
            var com = new StartCommand(flownode.CreateRunTime(), ent, ActionPointEnum.Accept, context);
            com.TaskAll = tasks;
            com.OperatorId = OperatorId;
            com.CurrentExecutor = creatorID;
            com.Register();
            obj.AddDataAndVariables(context, 1, dataGuid, docUrl); //添加业务数据与流程之间的关系并且把业务数据写到流程变量中
            com.Send();
        }
        /// <summary>
        /// 获取节点任务
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Action">任务类型</param>
        /// <returns></returns>
        public static List<string> GetNodeTask(this OAO_WorkFlowProcessNode obj, ObjectContext context,TaskActionEnum Action)
        {
            if (Action == TaskActionEnum.所有)
            {
                var flowtasklist = context.CreateObjectSet<OAO_WorkFlowNodeTask>().Where(e => e.ActionPoint == obj.ActionResult && e.WorkFlowNodeId == obj.WorkFlowNodeId).Select(e => e.NodeTaskId).ToList();
                return flowtasklist;
            }
            else
            {
                var flowtasklist = context.CreateObjectSet<OAO_WorkFlowNodeTask>().Where(e => e.WorkFlowNodeId == obj.WorkFlowNodeId && e.ActionPoint == (int)Action).Select(e => e.NodeTaskId).ToList();
                return flowtasklist;
            }

        }
        /// <summary>
        /// 根据运行时节点获取流程运行时的变量

        /// </summary>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Hashtable GetVarible(this OAO_WorkFlowProcessNode obj, ObjectContext context,out Dictionary<string,Guid> dicVarKey2Value)
        {
            System.Collections.Hashtable hsTable = new Hashtable();
            dicVarKey2Value = new Dictionary<string, Guid>();
            var varibles = context.CreateObjectSet<OAO_WorkFlowProcessVariable>().Include("OAO_WorkFlowVariable").Where(e => e.WorkFlowProcessId == obj.WorkFlowProcessId).ToList();
            foreach (var item in varibles)
            {
                string key=item.OAO_WorkFlowVariable.Name;
                hsTable.Add(key, item.VarValue);
                /*
                 */
                dicVarKey2Value.Add(key, item.Id);
            }
            return hsTable;
        }
        public static void UpdateVarible(this OAO_WorkFlowProcessNode obj, ObjectContext context,Hashtable hsTable, Dictionary<string, Guid> dicVarKey2Value) 
        {
            List<Guid> listVarGuid = new List<Guid>();
            Dictionary<Guid, string> dicVarId2Value = new Dictionary<Guid, string>();
            foreach (var item in hsTable.Keys)
            {
                Guid guid;
                string tmep = item.ToString();
                if (dicVarKey2Value.TryGetValue(tmep, out guid))
                {
                    listVarGuid.Add(guid);
                    dicVarId2Value.Add(guid, hsTable[item].ToString());
                }
                else//任务中执行的时候新加的变量 存 
                {
                    //讨论
                }
            }
            if (listVarGuid.Count < 0) return;
            var varibles = context.CreateObjectSet<OAO_WorkFlowProcessVariable>().Where(e => e.WorkFlowProcessId == obj.WorkFlowProcessId).ToList();
            foreach (var item in varibles)
            {
                string val;
                if (dicVarId2Value.TryGetValue(item.Id, out val)) {
                    item.VarValue = val;
                }
            }
        }
    }
}
