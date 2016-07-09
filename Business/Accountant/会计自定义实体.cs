using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Accountant
{
    class 会计自定义实体
    {
    }
    public class lwfgsydhzModel
    {
        public Guid GUID { set; get; }
        public string InvitePersonIDCard { set; get; }
        public string InvitePersonName { set; get; }
        public string DocNum { set; get; }
        public double Total_BX { set; get; }
        public double Total_Tax { set; get; }
        public double Total_Real { set; get; }
       
    }
    /// <summary>
    /// 类款项汇总
    /// </summary>
    public class lkxhzModel
    {
        /// <summary>
        /// 列表头信息
        /// </summary>
        public List<FieldModel> FieldList { set; get; }
        /// <summary>
        /// 数据
        /// </summary>
        public object objData{ set; get; }
    }
    public class FieldModel
    {
        public FieldModel(string fieldName, string fieldTitle)
        {
            this.FieldName = fieldName;
            this.FieldTitle = fieldTitle;
        }
        public string FieldName{set;get;}
        public string FieldTitle{ set; get; }
    }
    public class lkxhzDetailModel
    { 
       public Guid GUID_Item{set;get;}
       public string ItemName{set;get;}
       public string ItemKey { set; get; }
       public int? ItemType{set;get;}
       public string PaymentNumber{set;get;}
       public string IsProject{set;get;}
       public string FinanceCode{set;get;}
       public string EconomyClassName {set;get;}
       public string EconomyClassKey{set;get;}
       public string ExpendTypeKey{set;get;}
       public string ExpendTypeName{set;get;}
       public string BGSourceKey{set;get;}
       public string BGSourceName{set;get;}
       public double? ItemValue{set;get;}
       public DateTime? ItemDateTime{set;get;}
       public string ItemString { set; get; }
    }
    /// <summary>
    /// 工资Item项
    /// </summary>
    public class SA_PlanItemModel
    { 
       public Guid? GUID_Item{set;get;}
       public string ItemName{set;get;}
       public string ItemKey { set; get; }
    }
    /// <summary>
    /// 工资项数据加载
    /// </summary>
    public class SA_PlanItemSetupModel
    {
        public Guid? GUID { set; get; }
        public Guid? GUID_Item { set; get; }
        public string ItemName { set; get; }
        public string ItemType { set; get; }
        public Guid? GUID_SetUP { set; get; }
        public string SetUpName { set; get; }
        public string SetUpKey { set; get; }
        public string IsStart { set; get; }
    }

}
