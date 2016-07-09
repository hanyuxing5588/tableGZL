using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.薪酬设置
{
    public class 工资项数据加载方式设置 : BaseDocument
    {
        public 工资项数据加载方式设置() : base() { }
        public 工资项数据加载方式设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid, string type)
        {
            JsonModel jmodel = new JsonModel();
            try
            {
                #region Person
                if (type == "Plan")
                {
                    SA_PlanView main = this.InfrastructureContext.SA_PlanView.FirstOrDefault(e => e.GUID == guid);

                    if (main != null)
                    {
                        jmodel.m = main.Pick();
                        var q= this.InfrastructureContext.SA_PlanItemSetup.Join(this.InfrastructureContext.SA_SetUp, t => t.GUID_SetUP, e => e.GUID, (t, e) => new
                                {
                                    GUID =t.GUID,
                                    GUID_SetUp = e.GUID,
                                    e.SetUpName,
                                    t.IsStart,
                                    t.GUID_SA_PlanItem
                                });
                        var q1 =this.InfrastructureContext.SA_PlanItemView.Where(e => e.GUID_Plan == guid);
                        var q2 = from a in q1
                                 join a1 in q on a.GUID equals a1.GUID_SA_PlanItem into temp
                                 from t in temp.DefaultIfEmpty()
                                 select new SA_PlanItemEx
                                 {
                                    GUID =a.GUID,
                                    GUID_Plan= a.GUID_Plan,
                                    GUID_SetUp=  t.GUID_SetUp,
                                    ItemOrder = a.ItemOrder,
                                    ItemKey=  a.ItemKey,
                                    ItemName= a.ItemName,
                                    ItemTypeName = a.ItemType == 1 ? "金额" : a.ItemType == 2 ? "日期" : "文本",
                                    SetUpName= t.SetUpName,
                                    StartName = t.IsStart == true ? "是" : "否"
                                 };
                        var details = q2.OrderBy(e=>e.ItemOrder).ToList();
                        if (details.Count > 0)
                        {
                            JsonGridModel jgm = new JsonGridModel(typeof(SA_PlanItemEx).Name);
                            jmodel.d.Add(jgm);

                            foreach (SA_PlanItemEx detail in details)
                            {
                                List<JsonAttributeModel> picker = detail.ClassPick();
                                jgm.r.Add(picker);
                            }
                        }
                    }
                }
                #endregion
                jmodel.s = new JsonMessage("", "", "");
                return jmodel;
            }
            catch (Exception ex)
            {
                jmodel.result = JsonModelConstant.Error;
                jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return jmodel;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Save(List<SA_PlanItemSetupEx> item)
        {
            try
            {
                foreach (var it in item)
                {
                    var q = this.InfrastructureContext.SA_PlanItemSetup.FirstOrDefault(e => e.GUID_SA_PlanItem == it.GUID);
                    if (q != null)
                    {
                        //q.GUID_SA_PlanItem = (Guid)it.GUID_SA_PlanItem;
                        q.GUID_SetUP = (Guid)it.GUID_SetUP;
                        q.IsStart = (bool)it.IsStart;
                        this.InfrastructureContext.ModifyConfirm(q);
                    }
                    else {
                        if (it.GUID_SetUP == null) continue;
                        var ent = this.InfrastructureContext.SA_PlanItemSetup.CreateObject();
                        ent.GUID = Guid.NewGuid();
                        ent.GUID_SA_PlanItem = (Guid)it.GUID;
                        ent.GUID_SetUP = (Guid)it.GUID_SetUP;
                        ent.IsStart = it.IsStart == true;
                        this.InfrastructureContext.SA_PlanItemSetup.AddObject(ent);
                    }
                }
                this.InfrastructureContext.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class SA_PlanItemEx
    {
        public Guid? GUID { get; set; }
        public Guid? GUID_Plan { get; set; }
        public Guid? GUID_SetUp { get; set; }
        public string ItemKey { get; set; }
        public string ItemName { get; set; }
        public int? ItemType { get; set; }
        public string ItemTypeName { get; set; }
        public string SetUpName { get; set; }
        public bool? IsStart { get; set; }
        public string StartName { get; set; }
        public int? ItemOrder { get; set; }
    }
    public class SA_PlanItemSetupEx 
    {
        public Guid? GUID { get; set; }
        public Guid? GUID_SA_PlanItem { get; set; }
        public Guid? GUID_SetUP { get; set; }
        public bool? IsStart { get; set; }
    }
}
