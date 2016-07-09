using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessModel;
using Infrastructure;
namespace Business.DocTrans
{
  public  class BGAssignToBGMain
    {
      public Guid BGAssignGuid { get; set; }
      public BGAssignToBGMain(Guid bgAssignGuid) 
      {
          BGAssignGuid = bgAssignGuid;
      }
      /// <summary>
      /// 单据转换（从默认值到预算编制）
      /// </summary>
      /// <returns></returns>
      public BG_MainView DocTransferBGMain(BusinessModel.BusinessEdmxEntities businessContext, Infrastructure.BaseConfigEdmxEntities bcContext, Guid operatorGUID, BG_AssignView objAssign) 
      {
         // BG_AssignView objAssign = businessContext.BG_AssignView.FirstOrDefault(e => e.GUID == BGAssignGuid);
          if (null == objAssign)
          {
              return null;
          }
          BG_MainView objBGMain = new BG_MainView();
          objBGMain.GUID = Guid.NewGuid();
          objBGMain.GUID_BG_Assign = objAssign.GUID;
          // 预算设置
          objBGMain.GUID_BGSetup = objAssign.GUID_BGSetUp;
          objBGMain.BGSetupKey = objAssign.BGSetupKey;
          objBGMain.BGSetupName = objAssign.BGSetupName;
          // 预算步骤
          objBGMain.GUID_BGStep = objAssign.GUID_BGStep;
          objBGMain.BGStepKey = objAssign.BGStepKey;
          objBGMain.BGStepName = objAssign.BGStepName;
          // 预算类型
          objBGMain.GUID_BGType = objAssign.GUID_BGTYPE;
          objBGMain.BGTypeKey = objAssign.BGTypeKey;
          objBGMain.BGTypeName = objAssign.BGTypeName;
          // 单位
          objBGMain.GUID_DW = objAssign.GUID_DW;
          objBGMain.DWKey = objAssign.DWKey;
          objBGMain.DWName = objAssign.DWName;
          // 部门
          objBGMain.GUID_Department = objAssign.GUID_Department;
          objBGMain.DepartmentKey = objAssign.DepartmentKey;
          objBGMain.DepartmentName = objAssign.DepartmentName;
          // 项目
          objBGMain.GUID_Project = objAssign.GUID_Project;
          objBGMain.ProjectKey = objAssign.ProjectKey;
          objBGMain.ProjectName = objAssign.ProjectName;
          // 单据类型，业务类型 UI类型
          objBGMain.GUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
          objBGMain.GUID_DocType = new Guid("91FF4EDE-6569-4A17-A8B6-9C675AF1E110");
          objBGMain.GUID_UIType = new Guid("2726487D-5CE7-456B-89EE-87064DC94FCA");

          // 货币单位
          BG_Setup bg_Setup = bcContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == objBGMain.BGSetupKey);
          var ss_MoneyUnit = businessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == bg_Setup.GUID_MoneyUnit);
          if (null == ss_MoneyUnit)
          {
              return null;
          }
          objBGMain.GUID_MoneyUnit = ss_MoneyUnit.GUID;
          objBGMain.MoneyUnitKey = ss_MoneyUnit.MoneyUnitKey;
          objBGMain.MoneyUnitName = ss_MoneyUnit.MoneyUnitName;

          // 功能分类
          objBGMain.GUID_FunctionClass = objAssign.GUID_FunctionClass;
          objBGMain.FunctionClassKey = objAssign.FunctionClassKey;
          objBGMain.FunctionClassName = objAssign.FunctionClassName;
          // 单据状态 版本
          objBGMain.DocState = 0;
          objBGMain.BGPeriod = 0;
          objBGMain.DocVerson = "1";
          objBGMain.Invalid = true;

          // 设置金额
          objBGMain.Total_BG = 0;
          objBGMain.Total_BG_CurYear = 0;
          objBGMain.Total_BG_PreYear = 0;

          // 制单人 ，制单日期，修改人，修改日期，单据日期，预算人
          SS_Operator Operator = bcContext.SS_Operator.FirstOrDefault(e => e.GUID == operatorGUID);
          objBGMain.GUID_Maker = Operator.GUID;
          objBGMain.MakeDate = DateTime.Now;

          objBGMain.GUID_Modifier = Operator.GUID;
          objBGMain.ModifyDate = DateTime.Now;

          objBGMain.DocDate = DateTime.Now;
          Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
          objBGMain.GUID_Person = person.GUID;
          objBGMain.PersonKey = person.PersonKey;
          objBGMain.PersonName = person.PersonName;


          return objBGMain;
      }
    }
}
