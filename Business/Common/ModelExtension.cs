using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure;
using System.Reflection;
using Business.Common;
using System.Data.Objects.DataClasses;
using BusinessModel;
namespace Business.Common
{
   public static class ModelExtension
    {
       //public static void SetValue<T>(this T obj, string proName, object o)
       //{
       //    PropertyInfo[] infos = obj.GetType().GetProperties();
       //    infos.SetValueSkipNotEmpty(obj, proName, o);
       //}
       public static void FillCN_PaymentNumberDefault(this CN_PaymentNumberView obj,BaseDocument baseDocument)
       {
           PropertyInfo[] infos = obj.GetType().GetProperties();
           infos.SetValue(obj,"IsGuoKu",false);
           infos.SetValue(obj, "IsProject",false);
           SS_BGSource bgsource = baseDocument.InfrastructureContext.SS_BGSource.FirstOrDefault(e=>e.BGSourceKey=="1");
           if (bgsource != null)
           {
               infos.SetValue(obj, "GUID_BGResource", bgsource.GUID);
               infos.SetValue(obj, "BGSourceKey", bgsource.BGSourceKey);
               infos.SetValue(obj, "BGSourceName", bgsource.BGSourceName);
           }
          // infos.SetValue(obj);
       }
       public static void FillCN_PaymentNumberDefault(this CN_PaymentNumberView obj, BaseDocument baseDocument, string docType)
       {
           PropertyInfo[] infos = obj.GetType().GetProperties();
           infos.SetValue(obj, "IsGuoKu", false);
           infos.SetValue(obj, "IsProject", false);
          
            SS_BGSource bgsource = baseDocument.InfrastructureContext.SS_BGSource.FirstOrDefault(e => e.BGSourceKey == "1");
            if (bgsource != null)
            {
                infos.SetValue(obj, "GUID_BGResource", bgsource.GUID);
                infos.SetValue(obj, "BGSourceKey", bgsource.BGSourceKey);
                infos.SetValue(obj, "BGSourceName", bgsource.BGSourceName);
            }
            if (docType.ToUpper() == "lwflkd".ToUpper())//劳务费

            {
                SS_ExpendType expendType = baseDocument.InfrastructureContext.SS_ExpendType.FirstOrDefault(e => e.ExpendTypeKey == "6");
                if (expendType != null)
                {
                    infos.SetValue(obj, "GUID_ExpendType", expendType.GUID);
                    infos.SetValue(obj, "ExpendTypeName", expendType.ExpendTypeName);
                    infos.SetValue(obj, "ExpendTypeKey", expendType.ExpendTypeKey);
                }
                SS_BGCode bgcode = baseDocument.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.BGCodeKey == "0220");
                if (bgcode != null)
                {
                    infos.SetValue(obj, "GUID_BGCode", bgcode.GUID);
                    infos.SetValue(obj, "BGCodeName", bgcode.BGCodeName);
                    infos.SetValue(obj, "BGCodeKey", bgcode.BGCodeKey);
                }
            }
           
           //其它报销单

           if (docType.ToLower() == "qtbxd")//现金报销单

           {
               //PropertyInfo[] infos = obj.GetType().GetProperties();
               infos.SetValue(obj, "IsGuoKu", false);
               infos.SetValue(obj, "IsProject", false);
           }
           #region 差旅报销单

           if (docType.ToLower() == "clbxd")
           {    
                //支出类型
                SS_ExpendType expendType = baseDocument.InfrastructureContext.SS_ExpendType.FirstOrDefault(e => e.ExpendTypeKey == "6");
                if (expendType != null)
                {
                    infos.SetValue(obj, "GUID_ExpendType", expendType.GUID);
                    infos.SetValue(obj, "ExpendTypeName", expendType.ExpendTypeName);
                    infos.SetValue(obj, "ExpendTypeKey", expendType.ExpendTypeKey);
                }
               //科目
                SS_BGCode bgcode = baseDocument.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.BGCodeKey == "0219");//差旅费

                if (bgcode != null)
                {
                    infos.SetValue(obj, "GUID_BGCode", bgcode.GUID);
                    infos.SetValue(obj, "BGCodeName", bgcode.BGCodeName);
                    infos.SetValue(obj, "BGCodeKey", bgcode.BGCodeKey);
                }
               //经济分类
                SS_EconomyClass economyClass = baseDocument.InfrastructureContext.SS_EconomyClass.FirstOrDefault(e=>e.EconomyClassKey=="302");//
                if (economyClass != null)
                {
                    infos.SetValue(obj, "GUID_EconomyClass", economyClass.GUID);
                    infos.SetValue(obj, "EconomyClassName", economyClass.EconomyClassName);
                    infos.SetValue(obj, "EconomyClassKey", economyClass.EconomyClassKey);
                }
           }
           #endregion 
           #region  劳务费领款单
           //经济分类
           if (docType.ToLower() == "lwflkd")
           {
               SS_EconomyClass economyClass = baseDocument.InfrastructureContext.SS_EconomyClass.FirstOrDefault(e => e.EconomyClassKey == "302");//
               if (economyClass != null)
               {
                   infos.SetValue(obj, "GUID_EconomyClass", economyClass.GUID);
                   infos.SetValue(obj, "EconomyClassName", economyClass.EconomyClassName);
                   infos.SetValue(obj, "EconomyClassKey", economyClass.EconomyClassKey);
               }
           }
           #endregion

           //借款单填制

           if (docType.ToLower() == "jkdtz")
           { 
            
           }
       }  

       /// <summary>
       /// 填充明细默认值

       /// </summary>
       /// <typeparam name="T">实体类型</typeparam>
       /// <param name="obj">实体</param>
       /// <param name="document">BaseDocument 类</param>
       /// <param name="OperatorId">操作员GUID</param>
       public static void FillDetailDefault<T>(this T obj, BaseDocument document, Guid OperatorId,string docType) where T : EntityObject
       {
           PropertyInfo[] infos = obj.GetType().GetProperties();
           SS_Operator Operator = OperatorId == Guid.Empty ? null : document.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
           if (Operator != null)
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_Maker", Operator.GUID);
               infos.SetValueSkipNotEmpty(obj, "Maker", Operator.OperatorName);
               infos.SetValueSkipNotEmpty(obj, "MakeDate", DateTime.Now);
               infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
               infos.SetValue(obj, "Modifier", Operator.OperatorName);
               infos.SetValue(obj, "ModifyDate", DateTime.Now);
           }
           Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
           //获取默认部门
           var defDepartment = Operator.DefaultDepartment();

           var defDw = Operator.DefaultDW();
           if (person != null)
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_Person", person.GUID);
               infos.SetValueSkipNotEmpty(obj, "PersonName", person.PersonName);
               infos.SetValueSkipNotEmpty(obj, "PersonKey", person.PersonKey);
               infos.SetValueSkipNotEmpty(obj, "GUID_Department", defDepartment == null ? person.GUID_Department : defDepartment.GUID);
               infos.SetValueSkipNotEmpty(obj, "DepartmentName", defDepartment == null ? person.DepartmentName : defDepartment.DepartmentName);
               infos.SetValueSkipNotEmpty(obj, "DepartmentKey", defDepartment == null ? person.DepartmentKey : defDepartment.DepartmentKey);
               infos.SetValueSkipNotEmpty(obj, "OfficialCard", person.OfficialCard);
           }
           SS_SettleType settleType = null;
           if (docType == "zpsld" || docType == "gwkbxd")//支票申领单,公务卡报销单

           {
               settleType = document.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "02" && e.IsStop == false);
           }
           else if (docType == "hkspd")//汇款审批单

           {
               settleType = document.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "03" && e.IsStop == false);
           }           
           else
           {
               settleType = document.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "01" && e.IsStop == false);
           }
           if (settleType != null)
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_SettleType", settleType.GUID);
               infos.SetValueSkipNotEmpty(obj, "SettleTypeName", settleType.SettleTypeName);
               infos.SetValueSkipNotEmpty(obj, "SettleTypeKey", settleType.SettleTypeKey);
           }
           BG_Type bgtype = document.InfrastructureContext.BG_Type.FirstOrDefault(e => e.BGTypeKey == Constant.BGTypeOne && e.IsStop == false);//"01" 
           if (bgtype != null)
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_BGType", bgtype.GUID);
               infos.SetValueSkipNotEmpty(obj, "BGTypeName", bgtype.BGTypeName);
               infos.SetValueSkipNotEmpty(obj, "BGTypeKey", bgtype.BGTypeKey);
           }
           if (docType == "lwflkd")
           {
               SS_BGCode bgcode = document.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.BGCodeKey == "0220");//0310
               if (bgcode != null)
               {
                   var bgcodeChild = document.InfrastructureContext.SS_BGCode.Where(e => e.PGUID == bgcode.GUID).OrderBy(e => e.BGCodeKey).FirstOrDefault();
                   if (bgcodeChild != null) {
                       bgcode = bgcodeChild;
                   }
                   infos.SetValue(obj, "GUID_BGCode", bgcode.GUID);
                   infos.SetValue(obj, "BGCodeName", bgcode.BGCodeName);
                   infos.SetValue(obj, "BGCodeKey", bgcode.BGCodeKey);
                   infos.SetValue(obj, "GUID_EconomyClass", bgcode.GUID_EconomyClass);
               }
           }
           if (docType == "clbxd")
           {
               SS_BGCode bgcode = document.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.BGCodeKey == "0219");//差旅费

               if (bgcode != null)
               {
                   infos.SetValue(obj, "GUID_BGCode", bgcode.GUID);
                   infos.SetValue(obj, "BGCodeName", bgcode.BGCodeName);
                   infos.SetValue(obj, "BGCodeKey", bgcode.BGCodeKey);
                   infos.SetValue(obj, "GUID_EconomyClass", bgcode.GUID_EconomyClass);
               }            
           }
           //借款单填制

           if (docType == "jkdtz")
           {
               SS_WLType wltype = document.InfrastructureContext.SS_WLType.FirstOrDefault(e => e.WLTypeKey == "1080104");//其他应收款/个人/基本账户（项目支出）
               if (wltype != null)
               {
                   infos.SetValue(obj,"GUID_WLType",wltype.GUID);
                   infos.SetValue(obj, "WLTypeName", wltype.WLTypeName);
                   infos.SetValue(obj, "WLTypeKey", wltype.WLTypeKey);
               }
               infos.SetValue(obj, "ActionDate", DateTime.Now);//添加默认发生时间

           }
           if (docType == "yfd")//应付单 11003
           {
               //往来类型

               SS_WLType wltype = document.InfrastructureContext.SS_WLType.FirstOrDefault(e => e.WLTypeKey == "11007");//
               if (wltype != null)
               {
                   infos.SetValue(obj, "GUID_WLType", wltype.GUID);
                   infos.SetValue(obj, "WLTypeName", wltype.WLTypeName);
                   infos.SetValue(obj, "WLTypeKey", wltype.WLTypeKey);
               }
               ////客户类型
               //SS_Customer customer = document.InfrastructureContext.SS_Customer.FirstOrDefault(e => e.CustomerKey == "HDSBZX");//海淀社保中心(供应商)
               //if (customer != null) 
               //{
               //    infos.SetValue(obj, "GUID_Cutomer", customer.GUID);
               //    infos.SetValue(obj, "CustomerName", customer.CustomerName);
               //    infos.SetValue(obj, "CustomerKey", customer.CustomerKey);
               //}
           }
           if (docType == "zyjjlzd")//专用基金列支单

           {
               SS_JJType jjtype = document.InfrastructureContext.SS_JJType.FirstOrDefault(e=>e.JJTypeKey=="01");
               if (jjtype != null)
               {
                   infos.SetValue(obj, "GUID_JJType", jjtype.GUID);
                   infos.SetValue(obj, "JJTypeName", jjtype.JJTypeName);
                   infos.SetValue(obj, "JJTypeKey", jjtype.JJTypeKey);
               }
           }

       }
       /// <summary>
       /// 填充差旅默认值

       /// </summary>
       /// <param name="obj"></param>
       /// <param name="baseDocument"></param>
       public static void FillBX_TravelDefault(this BX_TravelView obj, BaseDocument baseDocument)
       {
           PropertyInfo[] info = obj.GetType().GetProperties();
           SS_TrafficView traffic = baseDocument.InfrastructureContext.SS_TrafficView.FirstOrDefault(e=>e.TrafficKey=="02");//火车
           if (traffic != null)
           {
               info.SetValue(obj,"GUID_Traffic",traffic.GUID);
               info.SetValue(obj, "TrafficName", traffic.TrafficName);
               info.SetValue(obj, "TrafficKey", traffic.TrafficKey);
           }
       }
       /// <summary>
       /// 收款凭单
       /// </summary>
       /// <param name="obj"></param>
       /// <param name="baseDocument"></param>
       public static void FillSK_MainDefault<T>(this T obj, BaseDocument baseDocument, Guid OperatorId) where T : EntityObject
       {
           PropertyInfo[] infos = obj.GetType().GetProperties();
           infos.SetValueSkipNotEmpty(obj, "GUID", Guid.NewGuid());
           infos.SetValueSkipNotEmpty(obj, "DocDate", DateTime.Now);
           SS_Operator Operator = OperatorId == Guid.Empty ? null : baseDocument.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
           if (Operator != null)
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_Maker", Operator.GUID);
               infos.SetValueSkipNotEmpty(obj, "Maker", Operator.OperatorName);
               infos.SetValueSkipNotEmpty(obj, "MakeDate", DateTime.Now);
               infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
               infos.SetValue(obj, "Modifier", Operator.OperatorName);
               infos.SetValue(obj, "ModifyDate", DateTime.Now);
              // infos.SetValue(obj, "SubmitDate",DateTime.Now);

               Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
               if (person != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_Person", person.GUID);
                   infos.SetValueSkipNotEmpty(obj, "PersonName", person.PersonName);
                   infos.SetValueSkipNotEmpty(obj, "PersonKey", person.PersonKey);
                   infos.SetValueSkipNotEmpty(obj, "GUID_Department", person.GUID_Department);
                   infos.SetValueSkipNotEmpty(obj, "DepartmentName", person.DepartmentName);
                   infos.SetValueSkipNotEmpty(obj, "DepartmentKey", person.DepartmentKey);
                   infos.SetValueSkipNotEmpty(obj, "GUID_DW", person.GUID_DW);
                   infos.SetValueSkipNotEmpty(obj, "DWName", person.DWName);
                   infos.SetValueSkipNotEmpty(obj, "DWKey", person.DWKey);
               }
               //单据类型
               var doctype = baseDocument.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl.ToLower() == baseDocument.ModelUrl.ToLower());
               if (doctype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_DocType", doctype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "GUID_YWType", (Guid)doctype.GUID_YWType);
               }
               //收款类型
               SS_SKType sktype = baseDocument.InfrastructureContext.SS_SKType.FirstOrDefault(e => e.IsStop == false);
               if (doctype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_SKType", sktype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "SKTypeName", sktype.SKTypeName);
                   infos.SetValueSkipNotEmpty(obj, "SKTypeKey", sktype.SKTypeKey);
               }
               //界面类型
               SS_UIType uitype = baseDocument.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "1401");//收入信息流转
                if (uitype != null)
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                    infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                    infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
                }
               //结算方式
                SS_SettleType settleType = baseDocument.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "01" && e.IsStop == false);
                if (settleType != null)
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_SettleType", settleType.GUID);
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeName", settleType.SettleTypeName);
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeKey", settleType.SettleTypeKey);
                }
               
           }

       }
       /// <summary>
       /// 主表赋默认值
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="obj"></param>
       /// <param name="?"></param>
       public static void FillDefault<T>(this T obj, BaseDocument document, Guid OperatorId,string ModelUrl) where T : EntityObject
       {
           PropertyInfo[] infos = obj.GetType().GetProperties();
           infos.SetValueSkipNotEmpty(obj, "GUID", Guid.NewGuid());
           infos.SetValueSkipNotEmpty(obj, "DocDate", DateTime.Now);
           SS_Operator Operator = OperatorId == Guid.Empty ? null : document.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
           if (Operator != null)           
           {
               infos.SetValueSkipNotEmpty(obj, "GUID_Maker", Operator.GUID);
               infos.SetValueSkipNotEmpty(obj, "Maker", Operator.OperatorName);
               infos.SetValueSkipNotEmpty(obj, "MakeDate", DateTime.Now);
               infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
               infos.SetValue(obj, "Modifier", Operator.OperatorName);
               infos.SetValue(obj, "ModifyDate", DateTime.Now);

               Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
               if (person != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_Person", person.GUID);
                   infos.SetValueSkipNotEmpty(obj, "PersonName", person.PersonName);
                   infos.SetValueSkipNotEmpty(obj, "PersonKey", person.PersonKey);
                   infos.SetValueSkipNotEmpty(obj, "GUID_Department", person.GUID_Department);
                   infos.SetValueSkipNotEmpty(obj, "DepartmentName", person.DepartmentName);
                   infos.SetValueSkipNotEmpty(obj, "DepartmentKey", person.DepartmentKey);
                   infos.SetValueSkipNotEmpty(obj, "GUID_DW", person.GUID_DW);
                   infos.SetValueSkipNotEmpty(obj, "DWName", person.DWName);
                   infos.SetValueSkipNotEmpty(obj, "DWKey", person.DWKey);
               }

               var doctype = document.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl.ToLower() == document.ModelUrl.ToLower());
               if (doctype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_DocType", doctype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "GUID_YWType", (Guid)doctype.GUID_YWType);
               }
           }
           if (ModelUrl == "yfd" || ModelUrl=="yfdtz")//应付单
           {
               SS_UIType uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "050101");//应付单

               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType",uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }
           }
           if (ModelUrl == "xjtq")//现金提取
           {
               SS_UIType uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey =="080201");//
               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }
           }
           if (ModelUrl == "xjcc")//现金存储
           {
               SS_UIType uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "080202");//
               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }
           }
           if (ModelUrl == "cnfkd")//出纳付款单
           {
               SS_UIType uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "080101");//
               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }
           }
           if (ModelUrl == "kjpz")//会计凭证
           {
               var pzType = document.BusinessContext.CW_PZType.FirstOrDefault(e => e.PZTypeKey == "记");
               if (pzType != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_PZType", pzType.GUID);
                   infos.SetValueSkipNotEmpty(obj, "PZTypeName", pzType.PZTypeName);
                   infos.SetValueSkipNotEmpty(obj, "PZTypeKey", pzType.PZTypeKey);
               }
               var uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "090201");//
               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }
               var doctype = document.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl.ToLower() == document.ModelUrl.ToLower());
               if (doctype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_DocType", doctype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "DocTypeName", doctype.DocTypeName);
                   infos.SetValueSkipNotEmpty(obj, "DocTypeKey", doctype.DocTypeKey);
               }
               var ywtype = document.InfrastructureContext.SS_YWType.FirstOrDefault(e => e.YWTypeKey == "0902");//核算管理
               if(ywtype!=null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_YWType", ywtype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "YWTypeName", ywtype.YWTypeName);
                   infos.SetValueSkipNotEmpty(obj, "YWTypeKey", ywtype.YWTypeKey);
               }
           }
           if (ModelUrl == "gzd")//工资单 （工资发放）
           {
               SA_PlanView saPlan = document.InfrastructureContext.SA_PlanView.Where(e => e.IsStop == false || e.IsStop == null).OrderBy(e => e.PlanKey).OrderByDescending(e => e.IsDefault).FirstOrDefault(); //e => e.PlanKey == "001"//在职基本工资发放
               if (saPlan != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_Plan", saPlan.GUID);
                   infos.SetValueSkipNotEmpty(obj, "PlanName", saPlan.PlanName);
                   infos.SetValueSkipNotEmpty(obj, "PlanKey", saPlan.PlanKey);
               }
               SS_UIType uitype = document.InfrastructureContext.SS_UIType.FirstOrDefault(e => e.UITypeKey == "0701");//工资发放
               if (uitype != null)
               {
                   infos.SetValueSkipNotEmpty(obj, "GUID_UIType", uitype.GUID);
                   infos.SetValueSkipNotEmpty(obj, "UITypeName", uitype.UITypeName);
                   infos.SetValueSkipNotEmpty(obj, "UITypeKey", uitype.UITypeKey);
               }

                    
           }


       }
    }
}
