using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using BusinessModel;
namespace FlowTaskImplement
{
     //<summary>
     //系统的内置任务 默认从102开始
     //</summary>
    public class 收款凭单核销URL : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "110";
        }

        public string GetTaskName()
        {
            return "收款凭单核销URL";
        }

        public bool Run(object parma)
        {

            //业务模型上下文
          
            var paramsTask = parma as Platform.Model.TaskParameter;
            var context = paramsTask.FlowContext;
            var process = paramsTask.Process;
            var level = paramsTask.CurrentNode.GetNodeLevel(context);
            OAO_WorkFlowProcessData oao_workflowprocessdata = paramsTask.Process.FindData(context, "skpd");
            var bContext = new BusinessModel.BusinessEdmxEntities();
            var docType = BusinessModel.BusinessModelExt.GetDocTypeByUrl(bContext, "skpd");
            if (oao_workflowprocessdata == null) return false;
            paramsTask.Process.SetVariable(context, ProcessVariableProvider.internal_common, ((int)EBillDocType.收款管理).ToString());
            paramsTask.Process.AddDataAndVariables(context, level, (Guid)oao_workflowprocessdata.DataId, "hxcl");

            return true;
           
        }
        #endregion
    }
   
}
