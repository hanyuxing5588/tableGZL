using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.IBusiness;
using Business.Common;

namespace Business.Foundation.其他设置
{
    public class 公式设置 : IBusiness.IFormula
    {
        #region 工资公式保存 IFormula 成员
        public bool GZFormulaSave(Guid planGuid, List<FormulaModel> listFormulaModel)
        {
            try
            {
                var IContext = DBFactory.GetCurInfrastructureDbContext();
                var ents = IContext.SA_PlanItem.Where(e => e.GUID_Plan == planGuid).ToList();
                var listGuid = new List<Guid>();
                for (int i = 0; i < listFormulaModel.Count; i++)
                {
                    var item = listFormulaModel[i];
                    var row = ents.FirstOrDefault(e => e.GUID_Plan == planGuid && e.GUID_Item == item.GUID_Item);
                    if (row != null)
                    {
                        row.ItemFormula = item.ItemFormula;
                        row.ItemOrder = (short)(i + 1);
                        listGuid.Add(row.GUID);
                    }
                    else
                    {
                        var ent = IContext.SA_PlanItem.CreateObject();
                        ent.GUID_Plan = planGuid;
                        ent.ItemFormula = item.ItemFormula;
                        ent.GUID_Item = item.GUID_Item;
                        ent.ItemOrder = (short)(i + 1);
                        ent.GUID = Guid.NewGuid();
                        IContext.SA_PlanItem.AddObject(ent);
                    }
                }
                var entDeletes = IContext.SA_PlanItem.Where(e => e.GUID_Plan == planGuid && !listGuid.Contains(e.GUID)).ToList();
                foreach (var item in entDeletes)
                {
                    IContext.SA_PlanItem.DeleteObject(item);
                }
                IContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool IsCanDelete(Guid palnGuid, Guid itemGUID)
        {
            var falg = true;
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var ents = IContext.SA_PlanItem.Where(e => e.GUID_Plan == palnGuid).ToList();
            foreach (var item in ents)
            {
                if (item.ItemFormula.ToLower().Contains(itemGUID.ToString().ToLower()))
                {
                    falg = false;
                    break;
                }
            }
            return falg;
        }
        #endregion

        #region 预算公式保存 IFormula 成员
        //保存方法
        public bool YSFormulaSave(Guid setupGuid, List<FormulaModel> listFormulaModel)
        {
            try
            {
                var IContext = DBFactory.GetCurInfrastructureDbContext();
                var ents = IContext.BG_SetupDetail.Where(e => e.GUID_BGSetup == setupGuid).ToList();
                foreach (var item in ents)
                {
                    IContext.BG_SetupDetail.DeleteObject(item);
                }
                for (int i = 0; i < listFormulaModel.Count; i++)
                {
                    var item = listFormulaModel[i];
                    var ent = IContext.BG_SetupDetail.CreateObject();
                    ent.GUID_BGSetup = setupGuid;
                    ent.ItemFormula = item.ItemFormula;
                    ent.GUID_Item = item.GUID_Item;
                    ent.ItemOrder = (short)(i + 1);
                    ent.GUID = Guid.NewGuid();
                    IContext.BG_SetupDetail.AddObject(ent);
                }
                IContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //判断是否删除(根据条件)
        public bool YSIsCanDelete(Guid setupGuid, Guid itemGUID)
        {
            var falg = true;
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var ents = IContext.BG_SetupDetail.Where(e => e.GUID_BGSetup == setupGuid).ToList();
            foreach (var item in ents)
            {
                if (item.ItemFormula.ToLower().Contains(itemGUID.ToString().ToLower()))
                {
                    falg = false;
                    break;
                }
            }
            return falg;
        }

        #endregion


        #region 预算默认值保存 IFormula 成员
        //保存方法
        public bool YSMRZFormulaSave(Guid setupGuid, List<FormulaModel> listFormulaModel)
        {
            try
            {
                var IContext = DBFactory.GetCurInfrastructureDbContext();
                var ents = IContext.BG_SetupDetail.Where(e => e.GUID_BGSetup == setupGuid).ToList();
                foreach (var item in ents)
                {
                    IContext.BG_SetupDetail.DeleteObject(item);
                }
                for (int i = 0; i < listFormulaModel.Count; i++)
                {
                    var item = listFormulaModel[i];
                    var ent = IContext.BG_SetupDetail.CreateObject();
                    ent.GUID_BGSetup = setupGuid;
                    ent.ItemDefaultFormula = item.ItemDefaultFormula;
                    ent.GUID_Item = item.GUID_Item;
                    ent.ItemOrder = (short)(i + 1);
                    ent.GUID = Guid.NewGuid();
                    IContext.BG_SetupDetail.AddObject(ent);
                }
                IContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        //判断是否删除(根据条件)
        public bool YSMRZIsCanDelete(Guid setupGuid, Guid itemGUID)
        {
            var falg = true;
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var ents = IContext.BG_SetupDetail.Where(e => e.GUID_BGSetup == setupGuid).ToList();
            foreach (var item in ents)
            {
                if (item.ItemDefaultFormula.ToLower().Contains(itemGUID.ToString().ToLower()))
                {
                    falg = false;
                    break;
                }
            }
            return falg;
        }

        #endregion



    }
}
