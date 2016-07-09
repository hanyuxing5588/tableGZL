using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using BusinessModel;
namespace FlowTaskImplement
{
    public class 专用基金判断是否是现金 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {//26000 19899 6100
            return "112";
        }

        public string GetTaskName()
        {
            return "专用基金判断是否是现金";
        }

        public bool Run(object parma)
        {

            //业务模型上下文            var paramsTask = parma as Platform.Model.TaskParameter;
            var context = paramsTask.FlowContext;
            var process = paramsTask.Process;
            var level = paramsTask.CurrentNode.GetNodeLevel(context);
            var url = new ProcessVariableProvider(context,process.Id).GetValue(ProcessVariableProvider.internal_viewurl);
            OAO_WorkFlowProcessData oao_workflowprocessdata = paramsTask.Process.FindData(context, url);
            
            var bContext = new BusinessModel.BusinessEdmxEntities();
            var ent = bContext.JJ_Detail.FirstOrDefault(e => e.GUID_JJ_Main == oao_workflowprocessdata.DataId);
            //ent.Total_JJ
            //if (ent.DocState) { 
                
            //}
            var docType = BusinessModel.BusinessModelExt.GetDocTypeByUrl(bContext, url);
            if (oao_workflowprocessdata == null) return false;
            paramsTask.Process.SetVariable(context, ProcessVariableProvider.internal_common, ((int)EBillDocType.公务卡汇总管理).ToString());
            paramsTask.Process.AddDataAndVariables(context, level, (Guid)oao_workflowprocessdata.DataId, "hxcl");

            return true;
           
        }
        #endregion
    }
}
