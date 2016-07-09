using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
namespace FlowTaskImplement
{
    public class 预算运行时分配人任务 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "103";
        }

        public string GetTaskName()
        {
            return "设置运行时的人员";
        }

        public bool Run(object parma)
        {
            //业务模型上下文
            var paramsTask = parma as Platform.Model.TaskParameter;
            var context = paramsTask.FlowContext;
            //流程运行时
            var process = paramsTask.Process;
            //找到具体的流程
            var workFlow=context.CreateObjectSet<OAO_WorkFlow>().FirstOrDefault(e=>e.Id==process.WorkFlowId&&e.Version==process.WorkFlowVersion);
            var processData = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.ProcessId == process.Id);
            if(workFlow==null)return false;
            if (processData == null) return false;
            var bContext = new BusinessModel.BusinessEdmxEntities();
            var nodeConfigExcurtor = bContext.SS_RunTimeUsersSet.Where(e => e.DocId == processData.DataId).ToList(); ;
            //删除此流程运行时的人
            var users= context.CreateObjectSet<OAO_WorkFlowProcessRuntimeUsers>().Where(e => e.WorkFlowProcessId == process.Id).ToList();
            foreach (var item in users)
            {
                context.CreateObjectSet<OAO_WorkFlowProcessRuntimeUsers>().DeleteObject(item);
            }
            //增加运行时此流程的人
            var docId = Guid.Empty; //预算分配的Id   
            foreach (var item in nodeConfigExcurtor)
            {
                var ent = context.CreateObjectSet<OAO_WorkFlowProcessRuntimeUsers>().CreateObject();
                ent.Id = Guid.NewGuid();
                ent.ExecutorId = item.OperatorId;
                ent.ExecutorVersion = 1;
                ent.WorkFlowNodeId = item.WorkFlowId;
                ent.WorkFlowProcessId = process.Id;
                context.CreateObjectSet<OAO_WorkFlowProcessRuntimeUsers>().AddObject(ent);
            }
            //判断变量
            //预算分配
            var ysfp = bContext.BG_AssignView.FirstOrDefault(e => e.GUID == processData.DataId);
            var varables = context.CreateObjectSet<OAO_WorkFlowProcessVariable>().Include("OAO_WorkFlowVariable")
                 .FirstOrDefault(e => e.WorkFlowProcessId == process.Id && e.OAO_WorkFlowVariable.Name == "预算初始值显示");
            if (ysfp != null && !string.IsNullOrEmpty(ysfp.FinanceCode)&&ysfp.BGTypeKey=="02")
            {
                //是国库 并且是项目支出 才会有预算初始值设置
                process.AddDataAndVariables(context, 3, (Guid)processData.DataId, "yscszsz");
                process.SetVariable(context, ProcessVariableProvider.internal_common, processData.Url);
                //设置变量的值 
                varables.VarValue = "显示";
            }
            else {
                varables.VarValue = "隐藏";
                process.AddDataAndVariables(context, 3, (Guid)processData.DataId, "ysbz");
                process.SetVariable(context, ProcessVariableProvider.internal_common, processData.Url);
            }
            context.SaveChanges();
            return true;
        }

        #endregion
    }
}
