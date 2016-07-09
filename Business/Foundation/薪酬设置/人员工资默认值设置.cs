using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure;
using Business.Common;
using System.Reflection;
using Platform.Flow.Run;
using System.Data.Objects;

namespace Business.Foundation.薪酬设置
{
    public class 人员工资默认值设置 : BaseDocument
    {
        public 人员工资默认值设置() : base() { }
        public 人员工资默认值设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

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
                if (type == "Person")
                {
                    SS_PersonView main = this.InfrastructureContext.SS_PersonView.FirstOrDefault(e => e.GUID == guid);

                    if (main != null)
                    {
                        jmodel.m = main.Pick();
                        foreach (var it in jmodel.m)
                        {
                            if (it.v=="Null")
                            {
                                it.v = "";
                            }
                        }
                        List<SA_ItemEx> details = new List<SA_ItemEx>();
                        var temp = this.InfrastructureContext.SA_ItemView.OrderBy(e => e.ItemKey).ToList();
                        var list = this.InfrastructureContext.SA_PersonItemSet.Where(e => e.GUid_SS_Person == guid).ToList();
                        details = temp.Select(
                            e => new SA_ItemEx
                            {
                                GUID = e.GUID,
                                ItemKey = e.ItemKey,
                                ItemName = e.ItemName,
                                ItemType = e.ItemType,
                                ItemTypeName = e.ItemType == 1 ? "数字" : e.ItemType == 2 ? "日期" : "文本",
                                IsStop = e.IsStop,
                                TingYong = e.IsStop == true ? "是" : "否",
                                DefaultValue = ""
                            }).ToList();


                        for (int i = 0, j = details.Count; i < j; i++)
                        {
                            for (int m = 0, n = list.Count; m < n; m++)
                            {
                                if (details[i].GUID == list[m].Guid_SA_Item)
                                {
                                    details[i].DefaultValue = list[m].DefaultValue;
                                    continue;
                                }
                            }
                        }
                        if (details.Count > 0)
                        {
                            JsonGridModel jgm = new JsonGridModel(typeof(SA_ItemEx).Name);
                            jmodel.d.Add(jgm);

                            foreach (SA_ItemEx detail in details)
                            {
                                List<JsonAttributeModel> picker = detail.ClassPick();
                                jgm.r.Add(picker);
                            }
                        }
                    }
                }
                #endregion
                #region Item
                if (type == "Item")
                {
                    SA_Item main = this.InfrastructureContext.SA_Item.FirstOrDefault(e => e.GUID == guid);

                    if (main != null)
                    {
                        jmodel.m = main.Pick();
                        for (int i = 0,j = jmodel.m.Count; i < j; i++)
                        {
                            if (jmodel.m[i].n == "ItemType")
                            {
                                jmodel.m[i].v = Convert.ToInt32(jmodel.m[i].v) == 1 ? "数字" : Convert.ToInt32(jmodel.m[i].v) == 2 ? "日期" : "文本";
                            }
                        }
                        var temp = this.InfrastructureContext.SS_PersonView.OrderBy(e => e.PersonKey);
                        var list = this.InfrastructureContext.SA_PersonItemSet.Where(e => e.Guid_SA_Item == guid).ToList();
                        List<SA_PersonEx> details = new List<SA_PersonEx>();
                        details = temp.Select(
                            e => new SA_PersonEx
                            {
                                GUID = e.GUID,
                                PersonKey = e.PersonKey,
                                PersonName = e.PersonName,
                                DepartmentName = e.DepartmentName,
                                DWName = e.DWName,
                                DefaultValue = ""
                            }).ToList();
                        for (int i = 0, j = details.Count; i < j; i++)
                        {
                            for (int m = 0, n = list.Count; m < n; m++)
                            {
                                if (details[i].GUID == list[m].GUid_SS_Person)
                                {
                                    details[i].DefaultValue = list[m].DefaultValue;
                                    continue;
                                }
                            }
                        }
                        if (details.Count > 0)
                        {
                            JsonGridModel jgm = new JsonGridModel(typeof(SA_PersonEx).Name);
                            jmodel.d.Add(jgm);

                            foreach (SA_PersonEx detail in details)
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
        public bool Save(List<Item> item)
        {

            try
            {
                foreach (var it in item)
                {
                    var SA_PersonItemSetModel = this.InfrastructureContext.SA_PersonItemSet.FirstOrDefault(e => e.GUid_SS_Person == it.GUID_SS_Person && e.Guid_SA_Item == it.GUID_SA_Item);
                    if (SA_PersonItemSetModel != null)
                    {
                        SA_PersonItemSetModel.DefaultValue = it.DefaultValue;
                        this.InfrastructureContext.ModifyConfirm(SA_PersonItemSetModel);
                    }
                    else
                    {
                        SA_PersonItemSet sa = new SA_PersonItemSet();
                        sa.Guid = Guid.NewGuid();
                        sa.Guid_SA_Item = (Guid)it.GUID_SA_Item;
                        sa.GUid_SS_Person = it.GUID_SS_Person;
                        sa.DefaultValue = it.DefaultValue;
                        this.InfrastructureContext.SA_PersonItemSet.AddObject(sa);
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

    public class SA_ItemEx 
    {
        public Guid? GUID { get; set; }
        public string ItemKey { get; set; }
        public string ItemName { get; set; }
        public int? ItemType { get; set; }
        public string ItemTypeName { get; set; }
        public bool? IsStop { get; set; }
        public string TingYong { get; set; }
        public string DefaultValue { get; set; }
    }

    public class SA_PersonEx 
    {
        public Guid? GUID { get; set; }
        public string PersonKey { get; set; }
        public string PersonName { get; set; }
        public string DepartmentName { get; set; }
        public string DWName { get; set; }
        public string DefaultValue { get; set; }
    }

    public class Item 
    {
        public Guid? GUID { get; set; }
        public Guid? GUID_SS_Person { get; set; }
        public Guid? GUID_SA_Item { get; set; }
        public string DefaultValue { get; set; }
    }
}
