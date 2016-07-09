using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;

namespace FlowTaskImplement
{
    public class 劳务费算税 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "104";
        }

        public string GetTaskName()
        {
            return "劳务费提交算税";
        }

        public bool Run(object parma)
        {
            try
            {
                //业务模型上下文
                var paramsTask = parma as Platform.Model.TaskParameter;
                var context = paramsTask.FlowContext;
                //流程运行时
                var process = paramsTask.Process;
                //找到具体的流程
                var workFlow = context.CreateObjectSet<OAO_WorkFlow>().FirstOrDefault(e => e.Id == process.WorkFlowId && e.Version == process.WorkFlowVersion);
                var processData = context.CreateObjectSet<OAO_WorkFlowProcessData>().FirstOrDefault(e => e.ProcessId == process.Id);
                if (workFlow == null) return false;
                if (processData == null) return false;
                using (var bContext = new BusinessModel.BusinessEdmxEntities())
                {
                    var bxMain = bContext.BX_MainView.FirstOrDefault(e => e.GUID == processData.DataId);
                    var doFax = new Business.Casher.GCYDoFax(bxMain.GUID);
                    doFax.DoTaxCaculte(bContext, true);
                }
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
               return false;
            }

        }

        #endregion

    }
}
