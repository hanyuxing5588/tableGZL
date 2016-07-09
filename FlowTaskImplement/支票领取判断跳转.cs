using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using BusinessModel;
namespace FlowTaskImplement
{
    /// <summary>
    /// 系统的内置任务 默认从108开始
    /// </summary>
    public class 支票领取判断跳转 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "108";
        }

        public string GetTaskName()
        {
            return "支票领取判断跳转";
        }

        public bool Run(object parma)
        {

            //业务模型上下文
          
            var paramsTask = parma as Platform.Model.TaskParameter;
            var context = paramsTask.FlowContext;
            var process = paramsTask.Process;
            var level = paramsTask.CurrentNode.GetNodeLevel(context);
            var url = new ProcessVariableProvider(context,process.Id).GetValue(ProcessVariableProvider.internal_viewurl);
            OAO_WorkFlowProcessData oao_workflowprocessdata = paramsTask.Process.FindData(context, url);
            var bContext = new BusinessModel.BusinessEdmxEntities();
            var docType = EBillDocType.报销管理;
            if (oao_workflowprocessdata == null) return false;
            paramsTask.Process.SetVariable(context, ProcessVariableProvider.internal_common, ((int)docType).ToString());
     
            //设置变量的值
            var varables = context.CreateObjectSet<OAO_WorkFlowProcessVariable>().Include("OAO_WorkFlowVariable").FirstOrDefault(e => e.WorkFlowProcessId == process.Id && e.OAO_WorkFlowVariable.Name == "金额(系统)");
            var mxMianGuid=(Guid)oao_workflowprocessdata.DataId;
            var entDetailDoc = bContext.BX_Detail.FirstOrDefault(e => e.Total_Real <= 0 && e.GUID_BX_Main == mxMianGuid);
            /*如果存在金额小于等于0*/
            if (entDetailDoc != null)
            {  /*支票报销*/
                varables.VarValue ="1";
                paramsTask.Process.AddDataAndVariables(context, level, (Guid)oao_workflowprocessdata.DataId, "zpsld");
            }
            else {
                /*支票核销*/
                varables.VarValue = "-1";
                paramsTask.Process.AddDataAndVariables(context, level, (Guid)oao_workflowprocessdata.DataId, "hxcl");
            }
            context.SaveChanges();  
            return true;
           
        }
        #endregion
    }
   
}
