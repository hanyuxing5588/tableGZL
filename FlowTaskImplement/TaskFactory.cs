using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform;
using System.Data.Objects;
using BusinessModel;
namespace FlowTaskImplement
{
  public  class TaskFactory
    {
      public static ITask CreateTask(string taskId) {
          ITask itask = null;
          switch (taskId)
          {
              case "GetTaskTest1":
                  itask = new 现金报销单转出纳单();
                  break;
              case "101":
                  itask = new 根据URL设置金额到系统变量();
                  break;
              case "102":
                  itask = new 修改为核销URL();
                  break;
              default:
                  break;
          }
          return itask;
      
      }
    }

  public static class MTaskTool
  {
      public static void ModifyConfirm(this ObjectContext obj, System.Data.Objects.DataClasses.IEntityWithKey entity)
      {
          obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Detached);
          obj.Attach(entity);
          obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Modified);
      }
      public static void DeleteConfirm(this ObjectContext obj, System.Data.Objects.DataClasses.IEntityWithKey entity)
      {
          obj.Attach(entity);
          obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Deleted);
      }

      public static Guid GetDefaultPersonId(this ObjectContext obj, Guid OperatorId)
      {
          string sql = string.Format("select GUID_Data from SS_DataAuthSet where GUID_RoleOrOperator in (" +
                                   "select GUID_Role from SS_RoleOperator where GUID_Operator='{0}'" +
                                   ") and ClassID=3 and IsDefault=1" +
                                   "union all " +
                                   "select GUID_Data from SS_DataAuthSet where GUID_RoleOrOperator in ('{0}') and ClassID=3 and IsDefault=1", OperatorId);
          var guids = obj.ExecuteStoreQuery<Guid>(sql).ToList();
          return guids.Count > 0 ? guids[0] : Guid.Empty;
      }
      public static string GetNextDocNum(BusinessEdmxEntities _bcontext, Guid dwGUID, Guid ywGUID, string date)
      {
          Infrastructure.BaseConfigEdmxEntities baseContext = new Infrastructure.BaseConfigEdmxEntities();
          string strDocNum = string.Empty;
          SS_DocNumber dnModel = _bcontext.SS_DocNumber.FirstOrDefault();
          string dwNum = string.Empty;
          string ywNum = string.Empty;
          string yNum = string.Empty;
          string mNum = string.Empty;
          int year = DateTime.Parse(date).Year;
          int month = DateTime.Parse(date).Month;
          int autoNumber = 0;
          bool isAdd = false;
          SS_DocNumberAutoNumber docNumberAutoNumberModel = new SS_DocNumberAutoNumber();
          var dnanModel = _bcontext.SS_DocNumberAutoNumber.FirstOrDefault(e => e.DocYear == year && e.DocMonth == month && e.GUID_DW == dwGUID && e.GUID_YWType == ywGUID);
          if (dnanModel != null)
          {
              docNumberAutoNumberModel = dnanModel;
              autoNumber = (int)dnanModel.AutoNumber + 1;
          }
          else
          {//添加  
              isAdd = true;
              autoNumber = (int)dnModel.AutoNumberBegin;
              docNumberAutoNumberModel.GUID = Guid.NewGuid();
              docNumberAutoNumberModel.GUID_DocNumber = dnModel.GUID;
              docNumberAutoNumberModel.DocYear = year;
              docNumberAutoNumberModel.DocMonth = month;
              docNumberAutoNumberModel.GUID_DW = dwGUID;
              docNumberAutoNumberModel.GUID_YWType = ywGUID;
              docNumberAutoNumberModel.AutoNumber = dnModel.AutoNumberBegin;
              _bcontext.SS_DocNumberAutoNumber.AddObject(docNumberAutoNumberModel);
          }
          if ((bool)dnModel.IsDW)
          {
              if ((bool)dnModel.IsDWKey)
              {
                  var dwModel = baseContext.SS_DW.FirstOrDefault(e => e.GUID == dwGUID);
                  dwNum = dwModel.DWKey;
              }
          }
          if ((bool)dnModel.IsYWType)
          {
              if ((bool)dnModel.IsYWTypeKey)
              {
                  var ywModel = baseContext.SS_YWType.FirstOrDefault(e => e.GUID == ywGUID);
                  ywNum = ywModel.YWTypeKey;
              }
          }
          if ((bool)dnModel.IsYear)
          {
              if (dnModel.YearFormat >= 0 && dnModel.YearFormat <= 4)
              {
                  yNum = docNumberAutoNumberModel.DocYear.ToString().Substring((4 - (int)dnModel.YearFormat), (int)dnModel.YearFormat);

              }
          }
          if ((bool)dnModel.IsMonth)
          {
              mNum = docNumberAutoNumberModel.DocMonth.ToString();
              if (docNumberAutoNumberModel.DocMonth < 10)
              {
                  mNum = "0" + mNum;
              }
          }

          string strAutoNumber = string.Empty;
          strAutoNumber = string.Format("{0:D" + dnModel.AutoNumberLong + "}", autoNumber, autoNumber.ToString().Length);
          string orderbyDocNum = MTaskTool.GetOrderByDocNum(dnModel, dwNum, ywNum, yNum, mNum);
          strDocNum = orderbyDocNum + strAutoNumber;
          if (isAdd == false)
          {
              //修改
              docNumberAutoNumberModel.AutoNumber = autoNumber;
          }

          return strDocNum;
      }
      /// <summary>
      /// 根据排序进行组织编号
      /// </summary>
      /// <param name="dnModel">SS_DocNumber模型</param>
      /// <param name="dwNum">单位编号</param>
      /// <param name="ywNum">业务编号</param>
      /// <param name="yNum">年编号</param>
      /// <param name="mNum">月编号</param>
      /// <returns>string</returns>
      public static string GetOrderByDocNum(SS_DocNumber dnModel, string dwNum, string ywNum, string yNum, string mNum)
      {
          string str = string.Empty;
          //根据排序进行组织编号
          Dictionary<int, string> dictionary = new Dictionary<int, string>();
          dictionary.Add((int)dnModel.Order_DW, dwNum);
          dictionary.Add((int)dnModel.Order_YWType, ywNum);
          dictionary.Add((int)dnModel.Order_Year, yNum);
          dictionary.Add((int)dnModel.Order_Month, mNum);
          var dict = dictionary.OrderBy(e => e.Key);
          foreach (KeyValuePair<int, string> item in dict)
          {
              str += item.Value;
          }
          return str;
      } 
  }
   
}
