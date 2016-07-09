using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessModel;
using Infrastructure;
namespace Business.DocTrans
{
  public  class BGDefaultToBGMain
  {
      public Guid BGDefaultGuid { get; set; }
      public BGDefaultToBGMain(Guid bgDefaultGuid) 
      {
          BGDefaultGuid = bgDefaultGuid;
      }
      /// <summary>
      /// 单据转换（从默认值到预算编制）
      /// </summary>
      /// <returns></returns>
      public BG_MainView DocTransferBGMain(BusinessModel.BusinessEdmxEntities businessContext, Infrastructure.BaseConfigEdmxEntities bcContext, Guid operatorGUID) 
      {
          BG_DefaultMainView objDefault = businessContext.BG_DefaultMainView.FirstOrDefault(e => e.GUID == BGDefaultGuid);
          if(null==objDefault)
          {
              return null;            
          }

          BG_MainView objBGMain = new BG_MainView();
          objBGMain.GUID = Guid.NewGuid();
          objBGMain.GUID_BG_Assign = objDefault.GUID_BG_Assign;
          // 预算设置
          objBGMain.GUID_BGSetup = objDefault.GUID_BGSetup;
          objBGMain.BGSetupKey = objDefault.BGSetupKey;
          objBGMain.BGSetupName = objDefault.BGSetupName;
          // 预算步骤
          objBGMain.GUID_BGStep = objDefault.GUID_BGStep;
          objBGMain.BGStepKey = objDefault.BGStepKey;
          objBGMain.BGStepName = objDefault.BGStepName;
          // 预算类型
          objBGMain.GUID_BGType = objDefault.GUID_BGType;
          objBGMain.BGTypeKey = objDefault.BGTypeKey;
          objBGMain.BGTypeName = objDefault.BGTypeName;
          // 单位
          objBGMain.GUID_DW = objDefault.GUID_DW;
          objBGMain.DWKey = objDefault.DWKey;
          objBGMain.DWName = objDefault.DWName;
          // 部门
          objBGMain.GUID_Department = objDefault.GUID_Department;
          objBGMain.DepartmentKey = objDefault.DepartmentKey;
          objBGMain.DepartmentName = objDefault.DepartmentName;
          // 项目
          objBGMain.GUID_Project = objDefault.GUID_Project;
          objBGMain.ProjectKey = objDefault.ProjectKey;
          objBGMain.ProjectName = objDefault.ProjectName;
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
          objBGMain.GUID_FunctionClass = objDefault.GUID_FunctionClass;
          objBGMain.FunctionClassKey = objDefault.FunctionClassKey;
          objBGMain.FunctionClassName = objDefault.FunctionClassName;
          // 单据状态 版本
          objBGMain.DocState = 0;
          objBGMain.BGPeriod = 0;
          objBGMain.DocVerson = "1";
          objBGMain.Invalid = true;

          // 设置金额
          objBGMain.Total_BG = objDefault.Total_BG;
          objBGMain.Total_BG_CurYear = objDefault.Total_BG_CurYear;
          objBGMain.Total_BG_PreYear = objDefault.Total_BG_PreYear;

          // 制单人 ，制单日期，修改人，修改日期，单据日期，预算人
          SS_Operator Operator = bcContext.SS_Operator.FirstOrDefault(e => e.GUID == operatorGUID);
          objBGMain.GUID_Maker = Operator.GUID;
          objBGMain.MakeDate = DateTime.Now;

          objBGMain.GUID_Modifier = Operator.GUID;
          objBGMain.ModifyDate = DateTime.Now;

          objBGMain.DocDate = DateTime.Now;
          
          //Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
          //objBGMain.GUID_Person = person.GUID;
          //objBGMain.PersonKey = person.PersonKey;
          //objBGMain.PersonName = person.PersonName;

          //预算人要 赋值预算初始值
          objBGMain.GUID_Person = objDefault.GUID_Person;
          objBGMain.PersonKey = objDefault.PersonKey;
          objBGMain.PersonName = objDefault.PersonName;


          return objBGMain;
      }
      
  }
}
