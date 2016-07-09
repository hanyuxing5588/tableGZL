using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using Infrastructure;
using Business.Common;
using BusinessModel;
using System.Data;

namespace Business.CommonModule.Search
{
    //关联列表
    public class DocLinkListSearch : BaseSearch
    {
        public DJLBCondition DJLBCondition { get; set; }
        public string MainGuid { get; set; }
        //public DocListSearch() { }
        public DocLinkListSearch(BaseHistoryCondition baseCondition, Guid operatorId)
            : base(baseCondition, operatorId)
        {
            this.OperatorId = operatorId;
            this.DJLBCondition = baseCondition as DJLBCondition;
            if (this.DJLBCondition != null)
            {
                this.IsShowDetail = this.DJLBCondition.IsShowDetail;
            }
        }

        public override string GetSqlWhere()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" 1=1 ");
            sb.AppendFormat(" and main.GUID_maker='{0}'",OperatorId);
            sb.AppendFormat(" AND main.GUID<>'{0}' ", MainGuid);
            sb.AppendFormat(" AND main.GUID NOT IN (SELECT docid FROM  ss_guanlian ) ", OperatorId);
            sb.AppendFormat(" AND main.guid NOT IN (SELECT mainid FROM dbo.ss_guanlian ) ", OperatorId);
            sb.Append(GetWhereWithDocDate(DJLBCondition.StartDate.ToShortDateString(), DJLBCondition.EndDate.ToShortDateString()));
            sb.Append(AppendBaseWhere(DJLBCondition as BaseHistoryCondition));
            return sb.ToString();
        }
    }
    //关联列表
    public class DocLinkDocListSearch : BaseSearch
    {
        public DJLBCondition DJLBCondition { get; set; }
        public string MainGuid { get; set; }
        public DocLinkDocListSearch(BaseHistoryCondition baseCondition, Guid operatorId)
            : base(baseCondition, operatorId)
        {
            this.OperatorId = operatorId;
            this.DJLBCondition = baseCondition as DJLBCondition;
            if (this.DJLBCondition != null)
            {
                this.IsShowDetail = this.DJLBCondition.IsShowDetail;
            }
        }
        public override string GetSqlWhere()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" 1=1 ");
            sb.AppendFormat(" AND main.GUID  IN (SELECT docid FROM  ss_guanlian  where mainid='{0}') ", MainGuid);
            return sb.ToString();
        }
    }

   //单据列表查询
   public class DocListSearch:BaseSearch
   {
       public DJLBCondition DJLBCondition { get; set; }
       //public DocListSearch() { }
       public DocListSearch(BaseHistoryCondition baseCondition, Guid operatorId) :base(baseCondition,operatorId)
       {
           this.OperatorId = operatorId;
           this.DJLBCondition = baseCondition as DJLBCondition;
           if (this.DJLBCondition != null)
           {
               this.IsShowDetail = this.DJLBCondition.IsShowDetail;
           }
       }
       public  override string GetSqlWhere()
       {
           StringBuilder sb = new StringBuilder();
           sb.AppendFormat(" 1=1 ");
           if (DJLBCondition.DocType != Guid.Empty)
           {
               sb.AppendFormat(" and GUID_DocType='{0}'", DJLBCondition.DocType);
           }
           if (!string.IsNullOrEmpty(DJLBCondition.DocNum))
           {
               sb.AppendFormat(" and DocNum like '%{0}%' ", DJLBCondition.DocNum);
           }
           sb.Append(GetWhereWithDocDate(DJLBCondition.StartDate.ToShortDateString(), DJLBCondition.EndDate.ToShortDateString()));
           sb.Append(AppendBaseWhere(DJLBCondition as BaseHistoryCondition));
           if (!string.IsNullOrEmpty(DJLBCondition.treeModel) && DJLBCondition.treeValue != Guid.Empty)
           {
               sb.Append(" and ");
               sb.Append(GetTreeWhere(DJLBCondition.treeModel, DJLBCondition.treeValue));
           }
           /*加人员权限*/
            sb.AppendFormat(@" and main.GUID_Person in (SELECT  GUID_Data
FROM    SS_DataAuthSet
WHERE   GUID_RoleOrOperator ='{0}'
        OR ( GUID_RoleOrOperator IN ( SELECT    GUID_Role
                                      FROM      SS_RoleOperator
                                      WHERE     GUID_Operator ='{0}' ) )
                                      
                                      AND ClassID=3)", OperatorId);
            /*加人员权限*/
           return sb.ToString();
       }

       public override string GetTreeWhere(string treeModelName, Guid treevalue)
       {
           switch (this.YWKey)
           {
               case "02":
               case "04":
               case "0801":
               case "0802":
                   return BXMainSeries(treeModelName, treevalue);
               case "03":
               case "05":
               case "0501":
               case "0502":
                   return WLMainSeries(treeModelName, treevalue);
               case "1101":
                   return SKMainSeries(treeModelName, treevalue);
               default:
                   return "";
           }
       }

       private string BXMainSeries(string treeModelName, Guid treevalue)
       {
           string strsql = " 1=1 ";
           using (var iContext = new BaseConfigEdmxEntities())
           {
               var list = new List<Guid>();
               var tableName = treeModelName.Split('_')[1];
               switch (treeModelName.ToLower())
               {
                   case "ss_bgcode":
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_department":
                   case "ss_dw":

                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  main.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_departmentex"://自己不填写自己部门的问题
                        CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_project":
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_projectclass":
                       var guidList = new List<Guid>();
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref guidList);
                       foreach (var item in guidList)
                       {
                           CommonFun.RetrieveLeafModelIds(iContext, "ss_project", item, ref list);
                           if (list.Count > 0)
                           {
                               strsql = " detail.GUID_project in " + CommonFun.ChangeGuid(list);
                           }
                       }
                       break;
                   case "ss_person":
                       strsql = string.Format(" main.GUID_Person='{0}'", treevalue);
                       break;
               }
               return strsql;
           }
       }
       private string WLMainSeries(string treeModelName, Guid treevalue)
       {
           string strsql = " 1=1 ";
           using (var iContext = new BaseConfigEdmxEntities())
           {
               var list = new List<Guid>();
               var tableName = treeModelName.Split('_')[1];
               switch (treeModelName.ToLower())
               {
                   case "ss_bgcode":
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  detail.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_department":
                   case "ss_dw":

                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  main.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_project":
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  detail.GUID_projectKey in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_projectclass":
                       var guidList = new List<Guid>();
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref guidList);
                       foreach (var item in guidList)
                       {
                           CommonFun.RetrieveLeafModelIds(iContext, "ss_project", item, ref list);
                           if (list.Count > 0)
                           {
                               strsql = " detail.GUID_projectKey in " + CommonFun.ChangeGuid(list);
                           }
                       }
                       break;
                   case "ss_person":
                       strsql = string.Format(" main.GUID_Person='{0}'", treevalue);
                       break;
               }
               return strsql;
           }
       }
       private string SKMainSeries(string treeModelName, Guid treevalue)
       {
           string strsql = " 1=1 ";
           using (var iContext = new BaseConfigEdmxEntities())
           {
               var list = new List<Guid>();
               var tableName = treeModelName.Split('_')[1];
               switch (treeModelName.ToLower())
               {
                   case "ss_department":
                   case "ss_dw":

                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  main.GUID_" + tableName + " in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_project":
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref list);
                       if (list.Count > 0)
                       {
                           strsql = "  main.GUID_project in " + CommonFun.ChangeGuid(list);
                       }
                       break;
                   case "ss_projectclass":
                       var guidList = new List<Guid>();
                       CommonFun.RetrieveLeafModelIds(iContext, treeModelName, treevalue, ref guidList);
                       foreach (var item in guidList)
                       {
                           CommonFun.RetrieveLeafModelIds(iContext, "ss_project", item, ref list);
                           if (list.Count > 0)
                           {
                               strsql = " main.GUID_project in " + CommonFun.ChangeGuid(list);
                           }
                       }
                       break;
                   case "ss_person":
                       strsql = string.Format(" main.GUID_Person='{0}'", treevalue);
                       break;
                   case "ss_bgcode":
                       strsql = string.Format("   GUID_BGCode<>'' ");
                       break;

               }
               return strsql;
           }
       }
      
   }
   public class DocListFilterSearch : DocListSearch
   {
       public DJLBFilterCondition Condition { get; set; }
        
       public DocListFilterSearch(BaseHistoryCondition baseCondition, Guid operatorId):base(baseCondition,operatorId) 
       {
           Condition = baseCondition as DJLBFilterCondition;
       }
       private string GetJEWhere() 
       {
           StringBuilder sb = new StringBuilder(" and 1=1 ");
           if (Condition.StartTotal!=null)
            {
                sb.Append(" and Total_XX>=" + Condition.StartTotal);
            }
           if (Condition.EndTotal != null)
           {
               sb.Append(" and Total_XX<=" + Condition.EndTotal);
           }
           return sb.ToString();
       }
       public override string GetSqlWhere()
       {
           StringBuilder sb = new StringBuilder();
           sb.AppendFormat(" 1=1 ");
           if (Condition.DocType != Guid.Empty)
           {
               sb.AppendFormat(" and GUID_DocType='{0}'", Condition.DocType);
           }
           if (!string.IsNullOrEmpty(Condition.GUID_ProjectEx))
           {
               sb.AppendFormat("   AND detail.GUID_PaymentNumber  IN (SELECT GUID FROM dbo.CN_PaymentNumber WHERE GUID_ProjectEx in('{0}') )",Condition.GUID_ProjectEx.Replace(",","','"));
           }
           if (!string.IsNullOrEmpty(Condition.DocNum))
           {
               sb.AppendFormat(" and DocNum like '%{0}%' ", Condition.DocNum);
           }
           sb.Append(GetWhereWithDocDate(Condition.StartDate.ToShortDateString(), Condition.EndDate.ToShortDateString()));
           sb.Append(AppendBaseWhere(Condition as BaseHistoryCondition));
           if (Condition.TreeNodeList.Count > 0)
           {
               foreach (var item in Condition.TreeNodeList)
               {
                   sb.Append(" and ");
                   sb.Append(GetTreeWhere(item.treeModel, item.treeValue));
               }
             
           }
           sb.Append(GetJEWhere());

           return sb.ToString();
       }

       
   }

    /// <summary>
    /// 支票领取的选单
    /// </summary>
   public class ZPLQSearch : BaseSearch 
   {
       public ZPLQSelectDocCondition Condition { get; set; }

       public ZPLQSearch(BaseHistoryCondition baseCondition, Guid operatorId):base(baseCondition,operatorId)
       {
           Condition = baseCondition as ZPLQSelectDocCondition;
           if (!this.YWKeyList.Contains("76")) {
               this.YWKeyList.Add("76");//公务卡
           }
       }
       public override string GetSqlWhere()
       {
           StringBuilder sb = new StringBuilder();
           sb.AppendFormat(" 1=1 ");
           if (Condition.DocType != Guid.Empty)
           {
               sb.AppendFormat(" and GUID_DocType='{0}'", Condition.DocType);
           }
           if (!string.IsNullOrEmpty(Condition.DocNum))
           {
               sb.AppendFormat(" and DocNum like '%{0}%' ", Condition.DocNum);
           }
           sb.Append(GetWhereWithDocDate(Condition.Year, Condition.Month));
           sb.Append(AppendBaseWhere(Condition as BaseHistoryCondition));
           if (!string.IsNullOrEmpty(Condition.treeModel) && Condition.treeValue != Guid.Empty)
           {
               sb.Append(" and ");
               sb.Append(GetTreeWhere(Condition.treeModel, Condition.treeValue));
           }
           if (Condition.YWType == "02") {
               sb.AppendFormat(" and  detail.IsCheck=1");
           }
           return sb.ToString();
       }
       public override string GetWhereWithDocDate(string strYear, string strMonth)
       {
           int year;
           int.TryParse(strYear, out year);

           int month;
           int.TryParse(strMonth, out month);
           StringBuilder sb=new StringBuilder (" and 1=1 ");
           if (year != 0)
           {
               sb.AppendFormat(" and Year(docDate)='{0}'", year);
           }
           if (month != 0)
           {
               sb.AppendFormat(" and month(docDate)='{0}'", month);
           }
            return sb.ToString();
       }
     
   }
  

 
}
