using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Infrastructure.InternalTask;
using Platform.Model;
using BusinessModel;
using System.Reflection;
namespace FlowTaskImplement
{
   public class 现金报销单转出纳单:Platform.ITask
    {
        #region ITask 成员

        public string GetTaskId()
        {
            return "GetTaskTest1";
        }

        public string GetTaskName()
        {
            return "现金报销单转出纳单";
        }

        public bool Run(object parma)
        {
            //业务模型上下文
            BusinessEdmxEntities bContext = new BusinessModel.BusinessEdmxEntities();
            var paramsTask = parma as Platform.Model.TaskParameter;
            var context=paramsTask.FlowContext;
            var level=paramsTask.CurrentNode.GetNodeLevel(context);
            //删除已经装换过的单据
            paramsTask.Process.RemoveData(context, "cnfkd");
            //找到要转换的单据
            OAO_WorkFlowProcessData xjbxd = paramsTask.Process.FindData(context, "xjbxd");
            if (xjbxd == null) return false;
            var bxMain = bContext.BX_Main.FirstOrDefault(e => e.GUID == xjbxd.DataId);
            var cnfkd = new CN_Main();
            cnfkd.GUID_Maker = new Guid("63C75613-0B7B-45A1-AE16-071E8F4E7224");
            cnfkd.GUID_Modifier = new Guid("63C75613-0B7B-45A1-AE16-071E8F4E7224");
            cnfkd.DocNum = "2014253";
            cnfkd.GUID = Guid.NewGuid();
            cnfkd.GUID_Department = bxMain.GUID_Department;
            cnfkd.GUID_DW = bxMain.GUID_DW;
            cnfkd.GUID_Maker = bxMain.GUID_Maker;
            cnfkd.GUID_Person = bxMain.GUID_Person;
            cnfkd.MakeDate = DateTime.Now;
            cnfkd.ModifyDate = DateTime.Now;
            cnfkd.GUID_DocType = new Guid("BC84A5E1-1DA8-4945-8E97-20D158EF71B3");
            cnfkd.GUID_YWType = new Guid("5BC4B281-B29A-4212-82BA-5ED337556CA9");
            cnfkd.DocDate = DateTime.Now;
            bContext.CN_Main.AddObject(cnfkd);
            bContext.SaveChanges();
            //更新变量表 数据关联表            
            paramsTask.Process.AddDataAndVariables(context, level, cnfkd.GUID, "cnfkd");


            return true;
        }


        #endregion
    }

   public static class ModelExt 
   {
       public static void SetValueSkipNotEmpty(this PropertyInfo[] infos, object obj, string PropertyName, object value)
       {
           if (infos == null || infos.Length == 0) return;
           PropertyInfo info = infos.FirstOrDefault(e => e.Name.ToLower() == PropertyName.ToLower());
           if (info == null) return;
           if (IsEmptyProperty(info.GetValue(obj, null)))
           {
               info.SetValue(obj, value, null);
           }
       }
       public static bool IsEmptyProperty(object value)
       {
           if (value == null) return true;
           Type mtype = value.GetType();
           if (mtype == typeof(string) && (string)value == string.Empty) return true;
           if (mtype == typeof(Guid) && ((Guid)value) == Guid.Empty) return true;
           if (mtype == typeof(DateTime) && ((DateTime)value) == DateTime.MinValue) return true;
           return false;
       }
   }
}
