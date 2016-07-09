using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.IBusiness
{

    public class FormulaModel 
    {
        public Guid GUID_Item { get; set; }
        public string ItemFormula { get; set; }
        public string ItemDefaultFormula { get; set; }

    }
   public  interface  IFormula
    {
       //工资公式
       bool GZFormulaSave(Guid planGuid,List<FormulaModel> listFormulaModel);
       bool IsCanDelete(Guid palnGuid,Guid itemGUID);
       //预算公式
       bool YSFormulaSave(Guid setupGuid, List<FormulaModel> listFormulaModel);
       bool YSIsCanDelete(Guid setupGuid, Guid itemGUID);
       //预算默认值设置
       bool YSMRZFormulaSave(Guid setupGuid, List<FormulaModel> listFormulaModel);
       bool YSMRZIsCanDelete(Guid setupGuid, Guid itemGUID);

    }
}
