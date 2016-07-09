using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessModel;
using Infrastructure;
namespace Business.DocTrans
{
   public class BGAssignToBGDefault
    {
        public Guid BGAssignGuid { get; set; }
        public BGAssignToBGDefault(Guid bgAssignGuid)
        {
            BGAssignGuid = bgAssignGuid;
        }
        /// <summary>
        /// 单据转换（从默认值到预算编制） 单据编号不在这里生成
            
        /// </summary>
        /// <returns></returns>
        public BG_DefaultMainView DocTransferBGDefault(BusinessModel.BusinessEdmxEntities businessContext, Infrastructure.BaseConfigEdmxEntities bcContext, Guid operatorGUID, BG_AssignView objAssign)
        {
            //BG_AssignView objAssign = businessContext.BG_AssignView.FirstOrDefault(e => e.GUID == BGAssignGuid);
            //if(null==objAssign)
            //{
            //    return null;            
            //}
            BG_DefaultMainView objDefaultMain = new BG_DefaultMainView();
            objDefaultMain.GUID = Guid.NewGuid();
            objDefaultMain.GUID_BG_Assign = objAssign.GUID;
            // 预算设置
            objDefaultMain.GUID_BGSetup = objAssign.GUID_BGSetUp;
            objDefaultMain.BGSetupKey = objAssign.BGSetupKey;
            objDefaultMain.BGSetupName = objAssign.BGSetupName;
            // 预算步骤
            objDefaultMain.GUID_BGStep = objAssign.GUID_BGStep;
            objDefaultMain.BGStepKey = objAssign.BGStepKey;
            objDefaultMain.BGStepName = objAssign.BGStepName;
            // 预算类型
            objDefaultMain.GUID_BGType = objAssign.GUID_BGTYPE;
            objDefaultMain.BGTypeKey = objAssign.BGTypeKey;
            objDefaultMain.BGTypeName = objAssign.BGTypeName;
            // 单位
            objDefaultMain.GUID_DW = objAssign.GUID_DW;
            objDefaultMain.DWKey = objAssign.DWKey;
            objDefaultMain.DWName = objAssign.DWName;
            // 部门
            objDefaultMain.GUID_Department = objAssign.GUID_Department;
            objDefaultMain.DepartmentKey = objAssign.DepartmentKey;
            objDefaultMain.DepartmentName = objAssign.DepartmentName;
            // 项目
            objDefaultMain.GUID_Project = objAssign.GUID_Project;
            objDefaultMain.ProjectKey = objAssign.ProjectKey;
            objDefaultMain.ProjectName = objAssign.ProjectName;
            // 单据类型，业务类型 UI类型
            objDefaultMain.GUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
            objDefaultMain.GUID_DocType = new Guid("471DEAB3-AC63-43A9-9041-B43BF912FA26");
            objDefaultMain.GUID_UIType = new Guid("B2639101-7D4F-47B2-8ADF-AB24694E1828");

            // 货币单位
            BG_Setup bg_Setup = bcContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == objDefaultMain.BGSetupKey);
            var ss_MoneyUnit = businessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == bg_Setup.GUID_MoneyUnit);
            if (null == ss_MoneyUnit)
            {
                return null;
            }
            objDefaultMain.GUID_MoneyUnit = ss_MoneyUnit.GUID;
            objDefaultMain.MoneyUnitKey = ss_MoneyUnit.MoneyUnitKey;
            objDefaultMain.MoneyUnitName = ss_MoneyUnit.MoneyUnitName;

            // 功能分类
            objDefaultMain.GUID_FunctionClass = objAssign.GUID_FunctionClass;
            objDefaultMain.FunctionClassKey = objAssign.FunctionClassKey;
            objDefaultMain.FunctionClassName = objAssign.FunctionClassName;
            // 单据状态 版本
            objDefaultMain.DocState = 0;
            objDefaultMain.BGPeriod = 0;
            objDefaultMain.DocVerson = "1";

            // 设置金额
            objDefaultMain.Total_BG = 0;
            objDefaultMain.Total_BG_CurYear = 0;
            objDefaultMain.Total_BG_PreYear = 0;

            // 制单人 ，制单日期，修改人，修改日期，单据日期，预算人
            SS_Operator Operator = bcContext.SS_Operator.FirstOrDefault(e => e.GUID == operatorGUID);
            objDefaultMain.GUID_Maker = Operator.GUID;
            objDefaultMain.MakeDate = DateTime.Now;

            objDefaultMain.GUID_Modifier = Operator.GUID;
            objDefaultMain.ModifyDate = DateTime.Now;

            objDefaultMain.DocDate = DateTime.Now;

            Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
            objDefaultMain.GUID_Person = person.GUID;
            objDefaultMain.PersonKey = person.PersonKey;
            objDefaultMain.PersonName = person.PersonName;


            return objDefaultMain;
        }
    }
}
