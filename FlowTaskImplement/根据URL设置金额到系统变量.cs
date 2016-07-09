using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Model;
using BusinessModel;
namespace FlowTaskImplement
{
    /// <summary>
    /// 系统的内置任务 默认从101开始
    /// </summary>
    public class 根据URL设置金额到系统变量 : Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "101";
        }
        public string GetTaskName()
        {
            return "获取金额";
        }

        public bool Run(object parma)
        { 
            
            //业务模型上下文
            var bContext = new BusinessModel.BusinessEdmxEntities();
            var paramsTask = parma as Platform.Model.TaskParameter;
            var context = paramsTask.FlowContext;
            var process = paramsTask.Process;

            //根据不同的URL 找到不同的表
            ProcessVariableProvider processVariable = new ProcessVariableProvider(context, process.Id);
            var docUrl= processVariable.GetValue(ProcessVariableProvider.internal_viewurl);
            var doc = paramsTask.Process.FindData(context, docUrl);
            if (doc == null) return false;
            try
            {
                double moneySum=0;
                var calculateResult = BusinessModelExt.GetMoneySumByGuidWithDocType(bContext, (Guid)doc.DataId, docUrl, out moneySum);
                if (!calculateResult) return false;
                //设置变量的值
                var varables = context.CreateObjectSet<OAO_WorkFlowProcessVariable>().Include("OAO_WorkFlowVariable").FirstOrDefault(e => e.WorkFlowProcessId == process.Id && e.OAO_WorkFlowVariable.Name == "金额(系统)");
                varables.VarValue = moneySum.ToString();
                context.SaveChanges();
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
