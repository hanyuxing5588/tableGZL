using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using BusinessModel;
using Platform.Flow.Run;
using Infrastructure;
using System.Data;


namespace Business.Accountant
{
    public class 工资发放 : BaseDocument
    {

        public 工资发放() : base() { }
        public 工资发放(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值

        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                //返回计划列表项字段

                List<string> planItemContaintProperty = new List<string>();
                planItemContaintProperty.AddRange(new string[] { "GUID", "ItemName", "ItemKey", "ItemType" }.ToList());

                SA_PlanActionView model = new SA_PlanActionView();
                model.FillDefault(this, this.OperatorId, this.ModelUrl);
                model.ActionState = 0;
                model.ActionYear = DateTime.Now.Year;
                model.ActionMouth = DateTime.Now.Month;
                /*判断月份*/
                var itemSaPlan = this.BusinessContext.SA_PlanAction.FirstOrDefault(e => e.ActionYear == model.ActionYear && e.ActionMouth == model.ActionMouth);
                if (itemSaPlan != null)
                {
                    if (model.ActionMouth == 12)
                    {
                        model.ActionYear = model.ActionYear + 1;
                        model.ActionMouth = 1;
                    }
                    else
                    {
                        model.ActionMouth = model.ActionMouth + 1;
                    }
                }
                this.DocDate = model.DocDate;
                model.ActionPeriod = 1;
                model.ActionTimes = 1;
                model.GUID = Guid.Empty;
                jmodel.m = model.Pick();



                SA_PlanActionDetailView dModel = new SA_PlanActionDetailView();
                //添加默认值（为工资列表项）

                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                var itemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == model.GUID_Plan && e.IsStop == false);
                foreach (SA_PlanItemView item in itemList)
                {
                    fjgm.r.Add(item.Pick(planItemContaintProperty));
                }
                jmodel.f.Add(fjgm);

                Guid g = Guid.Parse("0D1A6DFD-53A1-4900-8D98-E7E8193403EF");
                //明细信息
                var detailList = this.InfrastructureContext.SA_PlanPersonSetView.OrderBy(e => e.DepartmentKey).Where(e => e.GUID_SA_Plan == model.GUID_Plan).ToList();
                List<List<JsonAttributeModel>> detailListPicker = new List<List<JsonAttributeModel>>();
                foreach (SA_PlanPersonSetView item in detailList)
                {
                    List<JsonAttributeModel> attributeList = new List<JsonAttributeModel>();
                    //人员工资计划设置
                    SA_PlanPersonSetModel setModel = new SA_PlanPersonSetModel();
                    setModel.GUID_Person = item.GUID_SS_Person;
                    setModel.PersonKey = item.PersonKey;
                    setModel.PersonName = item.PersonName;
                    setModel.GUID_Department = item.GUID_Department;
                    setModel.DepartmentName = item.DepartmentName;
                    setModel.GUID_Bank = item.GUID_SS_Bank;
                    setModel.BankName = item.BankName;
                    setModel.BankCardNo = item.BankCardNo;

                    attributeList.AddRange(setModel.ClassPick());
                    attributeList.AddRange(addItemToGrid(model.GUID_Plan));
                    detailListPicker.Add(attributeList);
                }
                JsonGridModel djgm = new JsonGridModel(dModel.ModelName());
                djgm.r.AddRange(detailListPicker);
                jmodel.d.Add(djgm);
                //一开始就加载默认值

                jmodel = AutoSetValueData(null, jmodel);
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<JsonAttributeModel> gzItem = null;
        public List<JsonAttributeModel> addItemToGrid(Guid planId)
        {
            if (gzItem != null) return gzItem;
            List<JsonAttributeModel> attributeList = new List<JsonAttributeModel>();
            var planItemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == planId).ToList();
            foreach (var item in planItemList)
            {
                var m = "SA_PlanItem";
                var n = item.ItemType + "ItemName" + item.ItemKey;
                attributeList.Add(new JsonAttributeModel() { m = m, n = n, v = "" });
            }
            return attributeList;

        }
        /// <summary>
        /// 创建默认值根据计划

        /// </summary>
        /// <returns></returns>
        public JsonModel NewByPlan(Guid planId)
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                //返回计划列表项字段

                List<string> planItemContaintProperty = new List<string>();
                planItemContaintProperty.AddRange(new string[] { "GUID", "ItemName", "ItemKey", "ItemType" }.ToList());

                SA_PlanActionView model = new SA_PlanActionView();
                model.FillDefault(this, this.OperatorId, this.ModelUrl);
                model.ActionState = 0;
                model.ActionYear = DateTime.Now.Year;
                model.ActionMouth = DateTime.Now.Month;
                model.ActionPeriod = 1;
                model.ActionTimes = 1;
                //更改计划GUID
                model.GUID_Plan = planId;
                var plan = this.InfrastructureContext.SA_PlanView.FirstOrDefault(e => e.GUID == planId);
                model.PlanName = plan.PlanName;
                jmodel.m = model.Pick();
                SA_PlanActionDetailView dModel = new SA_PlanActionDetailView();
                //添加默认值（为工资列表项）
              
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                var itemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == model.GUID_Plan && e.IsStop == false);
                foreach (SA_PlanItemView item in itemList)
                {
                    fjgm.r.Add(item.Pick(planItemContaintProperty));
                }
                jmodel.f.Add(fjgm);

                //明细信息
                var detailList = this.InfrastructureContext.SA_PlanPersonSetView.OrderBy(e => e.PersonKey).Where(e => e.GUID_SA_Plan == model.GUID_Plan).ToList();
                List<List<JsonAttributeModel>> detailListPicker = new List<List<JsonAttributeModel>>();
                foreach (SA_PlanPersonSetView item in detailList)
                {
                    List<JsonAttributeModel> attributeList = new List<JsonAttributeModel>();
                    //人员工资计划设置
                    SA_PlanPersonSetModel setModel = new SA_PlanPersonSetModel();
                    setModel.GUID_Person = item.GUID_SS_Person;
                    setModel.PersonKey = item.PersonKey;
                    setModel.PersonName = item.PersonName;
                    setModel.GUID_Department = item.GUID_Department;
                    setModel.DepartmentName = item.DepartmentName;
                    setModel.GUID_Bank = item.GUID_SS_Bank;
                    setModel.BankName = item.BankName;
                    setModel.BankCardNo = item.BankCardNo;

                    attributeList.AddRange(setModel.ClassPick());

                    detailListPicker.Add(attributeList);
                }

                ////添加合计                
                //SA_PlanPersonSetModel hjModel = new SA_PlanPersonSetModel();
                //hjModel.PersonKey = "合计";
                //detailListPicker.Add(hjModel.ClassPick());

                JsonGridModel djgm = new JsonGridModel(dModel.ModelName());
                djgm.r.AddRange(detailListPicker);
                jmodel.d.Add(djgm);
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 发放更改状态

        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public JsonMessage UpdateActionState(Guid guid)
        {
            JsonMessage msg = new JsonMessage();
            try
            {
                using (var db=new BusinessModel.BusinessEdmxEntities())
                {
                    var model = db.SA_PlanAction.FirstOrDefault(e => e.GUID == guid);
                    if (model != null)
                    {
                        if (model.ActionState.ToString() == "1")
                        {
                            msg.t = "提示";
                            msg.m = "工资已经发放！";
                            msg.i = JsonModelConstant.Success;
                            return msg;
                        }
                        else
                        {
                            model.ActionState = 1;
                        }
                    }

                    // this.BusinessContext.ModifyConfirm(model);
                    db.SaveChanges();
                    msg.t = "提示";
                    msg.m = "工资发放成功！";
                    msg.i = JsonModelConstant.Success;
                    return msg;
                }
              
            }
            catch (Exception ex)
            {
                msg.t = "提示";
                msg.m = "工资发放失败！";
                msg.i = JsonModelConstant.Error;
                return msg;
            }
        }
        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            JsonModel jmodel = new JsonModel();
            try
            {
                SA_PlanActionView main = this.BusinessContext.SA_PlanActionView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    //计划列表项

                    List<string> planItemContaintProperty = new List<string>();
                    planItemContaintProperty.AddRange(new string[] { "GUID", "ItemName", "ItemKey", "ItemType" }.ToList());

                    jmodel.m = main.Pick();
                    //  var details = this.BusinessContext.SA_PlanActionDetailView.Where(e => e.GUID_PlanAction == guid).OrderBy(e => e.PersonKey).ToList(); 
                    string sql = string.Format("select SA_PlanActionDetailView.* from  SA_PlanActionDetailView left join SA_PlanItem on SA_PlanActionDetailView.GUID_Item=SA_PlanItem.GUID_Item where GUID_PlanAction='{0}' order by DepartmentKey,ItemOrder", guid);
                    //明细信息
                    var details = this.BusinessContext.ExecuteStoreQuery<SA_PlanActionDetailView>(sql).ToList();
                    List<List<JsonAttributeModel>> detailListPicker = new List<List<JsonAttributeModel>>();
                    List<Guid> personGUIDList = new List<Guid>();
                    personGUIDList = details.Select(e => e.GUID_Person).Distinct().ToList();
                    var flag = false;
                    foreach (Guid guiditem in personGUIDList)
                    {
                        flag = false;
                        var rowDetail = details.FindAll(e => e.GUID_Person == guiditem).ToList().OrderBy(e => e.ItemOrderNum);
                        List<JsonAttributeModel> row = new List<JsonAttributeModel>();

                        foreach (SA_PlanActionDetailView item in rowDetail)
                        {
                            if (flag == false)
                            {
                                flag = true;
                                //明细中查找人员工资计划设置项
                                SA_PlanPersonSetModel setModel = new SA_PlanPersonSetModel();
                                setModel.GUID_Person = item.GUID_Person;
                                setModel.PersonKey = item.PersonKey;
                                setModel.PersonName = item.PersonName;
                                setModel.GUID_Department = item.GUID_Department;
                                setModel.DepartmentName = item.DepartmentName;
                                setModel.GUID_Bank = item.GUID_Bank;
                                setModel.BankName = item.BankName;
                                setModel.BankCardNo = item.BankCardNo;
                                row.AddRange(setModel.ClassPick());
                            }


                            //工资项目
                            SA_PlanItemView saItemModel = new SA_PlanItemView();

                            saItemModel.GUID = item.GUID_Item;
                            saItemModel.ItemName = item.ItemName;
                            saItemModel.ItemKey = item.ItemKey;
                            saItemModel.ItemType = item.ItemType;
                            //合并的字段名称：类型+ItemName+ItemKey的值

                            var comItemValue = string.Empty;
                            switch (item.ItemType)
                            {
                                case 2://时间
                                    comItemValue = item.ItemDatetime.IsNullOrEmpty() ? "" : ((DateTime)item.ItemDatetime).ToString("yyyy-MM-dd");
                                    break;
                                case 3:
                                    comItemValue = item.ItemString;
                                    break;
                                default:
                                    comItemValue = item.ItemValue == null ? "0" : item.ItemValue.ToString();
                                    break;
                            }
                            row.AddRange(saItemModel.PickMergeN(comItemValue, planItemContaintProperty));//"ItemName", "ItemKey",
                            //隐藏的ItemDetailGUID
                            row.Add(new JsonAttributeModel("gzlkxsz" + item.ItemKey, item.GUID.ToString(), saItemModel.ModelName()));

                            //添加明细Model
                            SA_PlanActionDetailModel detailModel = new SA_PlanActionDetailModel();
                            detailModel.GUID = item.GUID;

                            row.AddRange(detailModel.ClassPick());
                        }
                        detailListPicker.Add(row);
                    }

                    JsonGridModel djgm = new JsonGridModel(typeof(SA_PlanActionDetail).Name);
                    djgm.r.AddRange(detailListPicker);
                    jmodel.d.Add(djgm);

                    //添加默认值（为工资列表项）

                    JsonGridModel fjgm = new JsonGridModel(typeof(SA_PlanActionDetail).Name);
                    List<SA_ItemModel> tempItemList = details.OrderBy(e => e.ItemOrderNum).Select(e => new SA_ItemModel
                    {
                        GUID = e.GUID_Item,
                        ItemName = e.ItemName,
                        ItemKey = e.ItemKey,
                        ItemType = e.ItemType

                    }).ToList();
                    List<SA_ItemModel> itemList = new List<SA_ItemModel>();
                    foreach (SA_ItemModel item in tempItemList)
                    {
                        var model = itemList.FirstOrDefault(e => e.GUID == item.GUID);
                        if (model == null)
                        {
                            itemList.Add(item);
                        }
                    }

                    foreach (SA_ItemModel item in itemList)
                    {
                        SA_PlanItemView planItemModel = new SA_PlanItemView();
                        planItemModel.ItemKey = item.ItemKey;
                        planItemModel.ItemName = item.ItemName;
                        planItemModel.ItemType = item.ItemType;
                        fjgm.r.Add(planItemModel.Pick(planItemContaintProperty));
                    }
                    jmodel.f.Add(fjgm);

                }
                jmodel.s = new JsonMessage("", "", "");

                return jmodel;
            }
            catch (Exception ex)
            {
                // throw ex;
                jmodel.result = JsonModelConstant.Error;
                jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return jmodel;
            }
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SA_PlanAction main = new SA_PlanAction();
            main.FillDefault(this, this.OperatorId, this.ModelUrl);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            Guid ywGUID;
            var doctypeModel = this.InfrastructureContext.SS_DocType.FirstOrDefault(e => e.GUID == main.GUID_Doctype);
            var strGUID_YWType = doctypeModel.GUID_YWType == null ? "" : doctypeModel.GUID_YWType.ToString();
            Guid.TryParse(strGUID_YWType, out ywGUID);
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, ywGUID, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            //工资列表数据
            var saitemList = this.InfrastructureContext.SA_Item.ToList();
            var dicItemKeyToGuid = GetItemKeyToGuid(saitemList);
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {

                SA_PlanActionDetailModel temp = new SA_PlanActionDetailModel();
                string detailModelName = typeof(SA_PlanActionDetail).Name;
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        AddDetail(main, row, orderNum, dicItemKeyToGuid);
                    }
                }

            }

            this.BusinessContext.SA_PlanAction.AddObject(main);

            this.BusinessContext.SaveChanges();

            return main.GUID;
        }
        private Dictionary<string, Guid> GetItemKeyToGuid(List<SA_Item> listSaItem)
        {
            var dic = new Dictionary<string, Guid>();
            foreach (var item in listSaItem)
            {
                dic.Add(item.ItemKey, item.GUID);
            }
            return dic;
        }
        /// <summary>
        /// 添加明细信息
        /// </summary>
        /// <param name="main"></param>
        //public Dictionary<>
        private void AddDetail(SA_PlanAction main, List<JsonAttributeModel> row, int orderNum, Dictionary<string, Guid> dicItem)
        {
            //SA_PlanActionDetailModel itemDetail = new SA_PlanActionDetailModel();
            //itemDetail.ClassFill(row);
            //人员工资计划设置
            SA_PlanPersonSetModel setModel = new SA_PlanPersonSetModel();
            setModel.ClassFill(row);
            //合计不需要保存
            if (setModel.GUID_Person.IsNullOrEmpty() || setModel.PersonKey.Trim() == "合计") return;
            //工资列表项
            List<JsonAttributeModel> itemRow = row.Find(typeof(SA_PlanItem).Name);
            if (itemRow != null && itemRow.Count > 0)
            {
                var itemNvalue = string.Empty;
                foreach (JsonAttributeModel item in itemRow)
                {
                    itemNvalue = item.n.ToLower();
                    if (itemNvalue.Contains("itemname"))
                    {
                        //明显信息
                        SA_PlanActionDetail detail = new SA_PlanActionDetail();
                        orderNum++;

                        //detail.FillDefault(this, this.OperatorId);
                       // detail.Fill(row);
                        detail.GUID = Guid.NewGuid();
                        detail.GUID_PlanAction = main.GUID;
                        detail.GUID_Person = setModel.GUID_Person == null ? Guid.Empty : (Guid)setModel.GUID_Person;
                        detail.GUID_Department = setModel.GUID_Department == null ? Guid.Empty : (Guid)setModel.GUID_Department;
                        detail.GUID_Bank = setModel.GUID_Bank;
                        detail.BankCardNo = setModel.BankCardNo;
                        //工资Item项


                        var itemnameIndex = itemNvalue.IndexOf("itemname");
                        var itemkey = itemNvalue.Substring(itemnameIndex + 8);
                        var itemType = itemNvalue.Substring(0, 1);
                        var guidItem = dicItem[itemkey];
                        if (guidItem != null)
                        {
                            detail.GUID_Item = guidItem;
                            switch (itemType)
                            {
                                case "2"://日期
                                    DateTime dTime;
                                    if (DateTime.TryParse(item.v, out dTime))
                                    {
                                        detail.ItemDatetime = dTime;
                                    }
                                    break;
                                case "3"://文本
                                    detail.ItemString = item.v;
                                    break;
                                default://金钱
                                    double itemValue = 0F;
                                    double.TryParse(item.v, out itemValue);
                                    detail.ItemValue = itemValue;
                                    break;

                            }

                        }
                        //工资项对应的Detail明细GUID
                        var itemDetaiRow = itemRow.Find(e => e.n + "".ToLower() == "gzlkxsz" + itemkey);
                        if (itemDetaiRow != null)
                        {
                            Guid gId;
                            if (Guid.TryParse(itemDetaiRow.v, out gId))
                            {
                                detail.GUID = gId;
                            }
                        }
                        detail.ItemOrderNum = orderNum;

                        main.SA_PlanActionDetail.Add(detail);
                    }
                    else { break; }
                }
            }

        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>GUID</returns>
        protected override Guid Modify(JsonModel jsonModel)
        {
            DateTime orgDateTime = DateTime.MinValue;
            if (jsonModel.m == null) return Guid.Empty;
            SA_PlanAction main = new SA_PlanAction();
            SA_PlanActionDetail tempdetail = new SA_PlanActionDetail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.SA_PlanAction.FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate == null ? orgDateTime : (DateTime)(main.DocDate);
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);



            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            if (IsDateChange(orgDateTime, (DateTime)main.DocDate))
            {
                //待更改


                main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, main.GUID_Doctype, main.DocDate.ToString());
            }

            string detailModelName = tempdetail.ModelName();

            //先删除然后再添加

            //删除
            List<SA_PlanActionDetail> detailList = main.SA_PlanActionDetail.ToList();
            foreach (SA_PlanActionDetail detail in detailList)
            {
                this.BusinessContext.DeleteConfirm(detail);
            }

            // 工资列表数据
            var saitemList = this.InfrastructureContext.SA_Item.ToList();
            var dicKeyToGuid = this.GetItemKeyToGuid(saitemList);
            // 添加
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                SA_PlanActionDetailModel temp = new SA_PlanActionDetailModel();
                //string detailModelName = typeof(SA_PlanActionDetail).Name;
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        //添加时，如果明细中没有GUID生成一个GUID，否则就用原有的GUID
                        AddDetail(main, row, orderNum, dicKeyToGuid);

                    }
                }
            }
            try
            {
                this.BusinessContext.ModifyConfirm(main);

                this.BusinessContext.SaveChanges();
            }
            catch (Exception ex)
            {
                
                throw;
            }
          
            return main.GUID;

        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SA_PlanAction main = this.BusinessContext.SA_PlanAction.Include("SA_PlanActionDetail").FirstOrDefault(e => e.GUID == guid);

            List<SA_PlanActionDetail> details = new List<SA_PlanActionDetail>();

            foreach (SA_PlanActionDetail item in main.SA_PlanActionDetail)
            {
                details.Add(item);
            }

            foreach (SA_PlanActionDetail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="status">状态1表示新建 2表示修改 3表示删除</param>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>JsonModel</returns>
        public override JsonModel Save(string status, JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            try
            {
                Guid value = jsonModel.m.Id(new SA_PlanAction().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Insert(jsonModel);
                        }
                        break;
                    case "2": //修改
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Modify(jsonModel);
                        }
                        break;
                    case "3": //删除
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            this.Delete(value);
                        }
                        break;

                }
                if (string.IsNullOrEmpty(strMsg))
                {
                    if (status == "3")//删除时返回默认值
                    {
                        result = this.New();
                        strMsg = "删除成功！";
                    }
                    else
                    {
                        result = this.Retrieve(value);
                        strMsg = "保存成功！";
                    }
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
        public int ActionMonth { get; set; }
        public int ActionYear { get; set; }
        public DateTime? DocDate { get; set; }
        /// <summary>
        /// 计算获取数据
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public JsonModel JSGetData(JsonModel jsonModel)
        {
            try
            {
                JsonAttributeModel id = jsonModel.m.IdAttribute("SA_PlanAction");
                Guid g=Guid.Empty;
                Guid.TryParse(id.v,out g);
                SA_PlanActionView main = this.BusinessContext.SA_PlanActionView.FirstOrDefault(e => e.GUID == g);
                if (main != null)
                {
                    //计划列表项

                    this.DocDate = main.DocDate;
                    this.ActionMonth = (int)main.ActionMouth;
                    this.ActionYear = (int)main.ActionYear;

                    var planItemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == main.GUID_Plan).ToList();

                    //计划列表项字段

                    List<string> planItemContaintProperty = new List<string>();
                    planItemContaintProperty.AddRange(new string[] { "GUID", "ItemName", "ItemKey" }.ToList());

                    jsonModel.m = main.Pick();
                    //步骤：


                    //1.找到对应的Item工资项


                    //2.根据每行数据，获取每行工资Item数据
                    //3.根据工资项的公式对把对应的GUID替换成对应的值


                    //4.根据此项找对应的公式项，根据公式计算                  

                    //明细信息                  
                    if (jsonModel.d != null && jsonModel.d.Count > 0)
                    {
                        //暂时部门 临时处理待优化 从页面传值Name
                        var departmentList = this.InfrastructureContext.SS_Department.ToList();
                        var bankList = this.InfrastructureContext.SS_Bank.ToList();

                        SA_PlanActionDetailModel temp = new SA_PlanActionDetailModel();
                        string detailModelName = typeof(SA_PlanActionDetail).Name;
                        JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                        if (Grid != null)
                        {
                            //计算表达式


                            Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();

                            foreach (List<JsonAttributeModel> row in Grid.r)
                            {
                                var personKey = row.GetValueByAttribute("PersonKey") + "";
                                var personGUID = row.GetValueByAttribute("GUID_Person");
                                Guid gPersonGuid;
                                Guid.TryParse(personGUID.ToString(), out gPersonGuid);
                                if (personKey.Trim() == "合计")//待处理 前台控制不提交合计数据
                                {
                                    Grid.r.Remove(row);
                                    break;
                                }

                                //把对应Item数据对应上对应的值                         
                                Dictionary<string, string> itemDic = new Dictionary<string, string>();
                                GetItemGUIDValueDic(row, planItemList, ref itemDic);

                                //把对应Item数据对应上对应的值,替换到对应的公式对应的值，并计算出值


                                ReSetJsonAttributeModel(gPersonGuid, row, planItemList, itemDic, ep);

                                //条件GUID对应的Name设置值

                                var strDepGUID = row.GetValueByAttribute("GUID_Department") + "";
                                Guid gDepGuid;
                                Guid.TryParse(strDepGUID, out gDepGuid);

                                AddRowDepartmentName(row, departmentList, gDepGuid);
                                //银行
                                var strbankGUID = row.GetValueByAttribute("GUID_Bank") + "";
                                Guid gBankGuid;
                                Guid.TryParse(strbankGUID, out gBankGuid);

                                AddRoBankName(row, bankList, gBankGuid);
                            }

                        }
                    }

                }

                jsonModel.s = new JsonMessage("", "", "");
                return jsonModel;
            }
            catch (Exception ex)
            {
                JsonModel jmodel=new JsonModel ();
                jmodel.result = JsonModelConstant.Error;
                jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return jmodel;
            }
        }
        private List<SA_PlanItemView> PlanItemList { get; set; }
        /// <summary>
        /// JsonAttributeModel 根据公式重新赋值

        /// </summary>
        /// <param name="row"></param>
        /// <param name="planItemList"></param>
        /// <param name="itemDic"></param>
        /// <param name="ep"></param>

        private void ReSetJsonAttributeModel(Guid personId, List<JsonAttributeModel> row, List<SA_PlanItemView> planItemList1, Dictionary<string, string> itemDic, Infrastructure.Expression.ExpressionParser ep)
        {
            //把对应Item数据对应上对应的值,替换到对应的公式对应的值，并计算出值

            if (PlanItemList == null || PlanItemList.Count == 0)
            {
                PlanItemList = GetCalculateSort(planItemList1);
            }
            foreach (var item in PlanItemList)
            {
                var itemKey = item.ItemKey;
                //保护Item项 更改数据
                var itemName = "ItemName" + itemKey;
                var curRow = row.FirstOrDefault(e => e.n.Contains(itemName));
                if (curRow != null)
                {
                    var itemformula = item.ItemFormula;
                    if (string.IsNullOrEmpty(itemformula)) continue;
                    //替换公式中对应的值

                    foreach (KeyValuePair<string, string> kv in itemDic)
                    {
                        var key = kv.Key;
                        var value = kv.Value;
                        if (itemformula.Contains(key))
                        {
                            itemformula = itemformula.Replace("{" + key + "}", string.IsNullOrEmpty(value.Trim()) ? "0" : value);
                            itemformula = FilterFormula(itemformula, personId);
                        }
                    }
                    bool isC = true;
                    if (itemformula.Contains("@")) isC = false;
                    /*算公式*/
                    itemformula = FilterFormula(itemformula, personId);
                    object relust;
                    try
                    {
                        if (isC)
                        {
                            var success = ep.Parser(itemformula, out relust);
                        }
                        else
                        {
                            relust = itemformula;
                        }
                    }
                    catch (Exception ex)
                    {
                        relust = 0;
                    }
                    //判断一下 success 正确才赋值


                    curRow.v = relust == null ? "" : relust.ToString();
                    //更新字典里面的值 hanyx 计算过程中 有的值 可能是动态生成的 
                    if (itemDic.ContainsKey(item.GUID_Item.ToString()))
                    {
                        itemDic[item.GUID_Item.ToString()] = curRow.v;
                    }

                }
            }

        }

        /*计算公式的优先级*/
        public List<SA_PlanItemView> GetCalculateSort(List<SA_PlanItemView> list)
        {
            List<SA_PlanItemView> listSaPlanView = new List<SA_PlanItemView>();
            var listHasGS = list.Where(e => e.ItemFormula != "").ToList();
            var listNoHasGS = list.Where(e => e.ItemFormula == "").ToList();
            listSaPlanView.AddRange(listNoHasGS);
            GetRomoveHasGS(listNoHasGS, listHasGS, ref listSaPlanView);
            return listSaPlanView;
        }
        /*递归移除有公式的*/
        public void GetRomoveHasGS(List<SA_PlanItemView> listNoHasGS, List<SA_PlanItemView> listHasGS, ref  List<SA_PlanItemView> listSaPlanView)
        {
            var listHasGSS = new List<SA_PlanItemView>();
            for (int i = 0; i < listHasGS.Count; i++)
            {
                if (GetReplaceToNull(listHasGS[i].ItemFormula, listNoHasGS))
                {
                    listSaPlanView.Add(listHasGS[i]);
                    listNoHasGS.Add(listHasGS[i]);
                }
                else
                {
                    listHasGSS.Add(listHasGS[i]);
                }
            }
            if (listHasGSS.Count == listHasGS.Count) return;
            GetRomoveHasGS(listNoHasGS, listHasGSS, ref listSaPlanView);
        }
        /*替换看可不可以用美公式的组代替*/
        public bool GetReplaceToNull(string itemformula, List<SA_PlanItemView> listSaPlanView)
        {
            for (int i = 0; i < listSaPlanView.Count; i++)
            {
                itemformula = itemformula.Replace("{" + listSaPlanView[i].GUID_Item.ToString() + "}", "");
            }
            return !itemformula.Contains("{");
        }


        /*过滤算税的表达式*/
        public string FilterFormula(string strFormula, Guid personId)
        {
            if (strFormula.Contains("CTax("))
            {
                var index1 = strFormula.IndexOf("CTax(");
                var index2 = strFormula.IndexOf(")", index1);
                var val = strFormula.Substring(index1 + 5, index2 - index1 - 5);
                double d = 0;
                if (double.TryParse(val, out d))
                {
                    var calculateTaxVal = CalculateTax(d);
                    //算税
                    if (strFormula.Contains("CTax("))
                    {
                        strFormula = strFormula.Replace("CTax(" + val + ")", calculateTaxVal.ToString("0.00"));
                    }
                }

            }
            if (strFormula.Contains("CServiceFee()"))
            {
                var serviceFee = UpMonthServiceFee(personId);
                strFormula = strFormula.Replace("CServiceFee()", serviceFee.ToString("0.00"));
            }
            if (strFormula.Contains("@GiveOutDate") || strFormula.Contains("@GIVEOUTDATE"))
            {
                var strTemp = this.DocDate == null ? DateTime.Now.ToShortDateString() : this.DocDate.Value.ToShortDateString();
                strFormula = strFormula.Replace("@GiveOutDate", strTemp).Replace("@@GIVEOUTDATE", strTemp);
            }
            return strFormula;
        }
        /*计算税的公式 CTax()*/
        public double CalculateTax(double valueGZ)
        {
            double tax = 0;
            double taxL = valueGZ - 3500;
            if (taxL > 0 && taxL <= 1500)
            {
                tax = taxL * 0.03;
            }
            else if (taxL > 1500 && taxL <= 4500)
            {
                tax = taxL * 0.1 - 105;
            }
            else if (taxL > 4500 && taxL <= 9000)
            {
                tax = taxL * 0.2 - 555;
            }
            else if (taxL > 9000 && taxL <= 35000)
            {
                tax = taxL * 0.25 - 1005;
            }
            else if (taxL > 35000 && taxL <= 55000)
            {
                tax = taxL * 0.3 - 2755;
            }
            else if (taxL > 55000 && taxL <= 80000)
            {
                tax = taxL * 0.35 - 5505;
            }
            else if (taxL > 80000)
            {
                tax = taxL * 0.45 - 13505;
            }
            else
            {
                tax = 0;
            }
            return tax;
        }
        /*上一个月的劳务费*/
        private Dictionary<Guid, double> DicPerson2G = null;
        public double UpMonthServiceFee(Guid personId)
        {
            if (DicPerson2G != null)
            {
                return DicPerson2G.ContainsKey(personId) ? DicPerson2G[personId] : 0;
            }
            string lwfFormatSql = " select GUID_InvitePerson,Sum(Total_bx) as SumTotal, Sum(Total_Tax) as SumTax" +
   " from dbo.BX_InviteFeeView where (GUID_InvitePerson in ( select guid from ss_person where IsTax<>1))   " +//是本单位 不算税

   " and GUID_bx_main in (select guid from BX_Main where Month(Makedate)={1} and Year(MakeDate)={0} and GUID in (select DataId from dbo.OAO_WorkFlowProcessData where ProcessId in(  select id from oao_workflowProcess where state=1)))" +//流程结束后

   " group by GUID_InvitePerson";
            int am = ActionMonth - 1, ay = ActionYear;
            if (ActionMonth == 1)
            {
                ay = ActionYear - 1;
                am = 12;
            }
            DicPerson2G = new Dictionary<Guid, double>();
            string sql = string.Format(lwfFormatSql, ay, am);
            var listTaxPerson = BusinessContext.ExecuteStoreQuery<TaxPerson>(sql).ToList();
            foreach (var item in listTaxPerson)
            {
                DicPerson2G.Add(item.GUID_InvitePerson, item.SumTotal);
            }

            return 0;
        }
        /// <summary>
        /// item GUID对应Value字典表

        /// </summary>
        /// <param name="row"></param>
        /// <param name="planItemList"></param>
        /// <param name="dic"></param>
        private void GetItemGUIDValueDic(List<JsonAttributeModel> row, List<SA_PlanItemView> planItemList, ref Dictionary<string, string> dic)
        {
            foreach (JsonAttributeModel attr in row)
            {

                //保护Item项 更改数据
                var itemName = attr.n.ToLower();
                if (itemName.Contains("itemname"))
                {
                    itemName = itemName.Substring(1);
                    var itemkey = itemName.Replace("itemname", "");
                    var list = planItemList.Find(e => e.ItemKey + "".Trim() == itemkey);
                    if (list != null)
                    {
                        double dValue = 0;
                        double.TryParse(attr.v, out dValue);
                        dic[list.GUID_Item.ToString()] = attr.v == "" ? "0" : dValue.ToString();
                    }
                }
            }
            if (planItemList.Count == dic.Keys.Count) return;
            //如果工资计划又工资项 并且 做工资的时候没有此工资项 那么就设置为0
            var listNew=new List<string>();
            foreach (var item in planItemList)
            {
                var str = item.GUID_Item.ToString();
                if (!dic.ContainsKey(str)) {
                    listNew.Add(str);
                }
            }
            foreach (var item in listNew)
            {
                dic.Add(item, "0");
            }
        }

        /// <summary>
        /// 自动设置值

        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public JsonModel AutoSetValueData(JsonModel jsonModel, JsonModel mainJsonModel)
        {
            jsonModel = new JsonModel();
            SA_PlanAction main = new SA_PlanAction();
            main.Fill(mainJsonModel.m);
            if (main == null) return jsonModel;
            try
            {
                //计划工资项列表

                var pisJosonModel = RetrievePlanItemSetup(main.GUID_Plan);
                jsonModel.d = pisJosonModel.d;
                return SetValueData(jsonModel, mainJsonModel);
            }
            catch (Exception ex)
            {
                mainJsonModel.result = JsonModelConstant.Error;
                mainJsonModel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return mainJsonModel;
            }
        }
        /// <summary>
        /// 设置值
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public JsonModel SetValueData(JsonModel jsonModel, JsonModel mainJsonModel)
        {
            //设置值步骤：
            //1.判断工资数据加载类型（待根据类型加载工资项对应的值）
            //2.把对应的工资项根据 加载类型获取值 并把值赋值到对应的人上
            //(1) 首先获取工资加载数据类型
            //(2)然后根据工资加载数据类型获取对应 类型中的数据
            JsonModel jmodel = new JsonModel();
            try
            {
                //加载主表信息
                SA_PlanAction main = new SA_PlanAction();
                main.Fill(mainJsonModel.m);

                if (main == null) return jmodel;
                //计划工资项列表
                var planItemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == main.GUID_Plan).ToList();
                #region  工资数据加载类型数据
                //前端传人工资数据加载类型集合
                List<SA_PlanItemSetupModel> planitemSetupList = new List<SA_PlanItemSetupModel>();

                //工资数据加载类型
                var setUpList = this.InfrastructureContext.SA_SetUp.OrderBy(e => e.SetUpKey).ToList();
                // 工资计划人员设置 SA_PlanPersonSet
                var planPersonSetList = this.InfrastructureContext.SA_PlanPersonSet.Where(e => e.GUID_SA_Plan == main.GUID_Plan);
                var planPersonList = planPersonSetList.Select(e => e.GUID_SS_Person).ToList();
                //人员的工资项设置数据 SA_PersonItemSetView
                var personItemSetList = this.InfrastructureContext.SA_PersonItemSetView.Where(e => planPersonList.Contains(e.GUid_SS_Person)).ToList();

                //前端传人工资数据加载类型集合

                if (jsonModel.d != null && jsonModel.d.Count > 0)
                {
                    JsonGridModel Grid = jsonModel.d.Find(typeof(SA_PlanItemSetupModel).Name);
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        var isStart = row.GetValueByAttribute("IsStart") + "";
                        if (isStart == "true" || isStart == "是")
                        {
                            SA_PlanItemSetupModel saplanitemsetmodel = new SA_PlanItemSetupModel();
                            saplanitemsetmodel.ClassFill(row);
                            planitemSetupList.Add(saplanitemsetmodel);
                        }

                    }
                }
                #endregion
                #region 设置值


                //根据工作计划加载对应的人和工资项
                //明细信息                  
                if (mainJsonModel.d != null && mainJsonModel.d.Count > 0)
                {
                    //暂时部门 临时处理待优化 从页面传值Name
                    var departmentList = this.InfrastructureContext.SS_Department.ToList();
                    var bankList = this.InfrastructureContext.SS_Bank.ToList();

                    var planModel = this.InfrastructureContext.SA_PlanView.FirstOrDefault(e => e.GUID == main.GUID_Plan);
                    //上期数据获取 待优化 有那个类型的数据加载那类
                    List<SA_PlanActionDetailView> preDetailList = new List<SA_PlanActionDetailView>();
                    preDetailList = GetPreDataDetailList(planModel, main);

                    //当期数据 
                    List<SA_PlanActionDetailView> currentDetailList = new List<SA_PlanActionDetailView>();
                    currentDetailList = GetCurrentDataDetailList(planModel, main);

                    Infrastructure.Expression.ExpressionParser ep = new Infrastructure.Expression.ExpressionParser();
                    SA_PlanActionDetailModel temp = new SA_PlanActionDetailModel();
                    string detailModelName = typeof(SA_PlanActionDetail).Name;
                    JsonGridModel Grid = mainJsonModel.d.Find(detailModelName);
                    if (Grid != null)
                    {
                        //把对应Item数据中的GUID对应列表表头字段值  
                        //GUID与字段对应关系                      
                        Dictionary<string, string> itemFieldGUIDDic = new Dictionary<string, string>();
                        Dictionary<string, string> itemValueGUID = new Dictionary<string, string>();
                        //遍历Gird数据，根据设置的 工作计划加载项 计算数据
                        //foreach (List<JsonAttributeModel> row in Grid.r)
                        for (int i = 0; i < Grid.r.Count; i++)
                        {

                            List<JsonAttributeModel> row = Grid.r[i];
                            var personKey = row.GetValueByAttribute("PersonKey") + "";
                            //if (i == Grid.r.Count-1)//合计hanyx
                            //{
                            //    Grid.r.Remove(row);
                            //    break;
                            //}

                            //Item字典表

                            if (itemValueGUID.Count == 0)
                            {
                                GetItemDic(row, planItemList, ref itemValueGUID, ref itemFieldGUIDDic);
                            }
                            //设置值
                            ReSetJsonAttributeModelValue(row, itemFieldGUIDDic, planitemSetupList, setUpList, personItemSetList, preDetailList, currentDetailList, planItemList, itemValueGUID, ep);
                            //条件GUID对应的Name设置值


                            var strDepGUID = row.GetValueByAttribute("GUID_Department") + "";
                            Guid gDepGuid;
                            Guid.TryParse(strDepGUID, out gDepGuid);

                            AddRowDepartmentName(row, departmentList, gDepGuid);
                            //银行
                            var strbankGUID = row.GetValueByAttribute("GUID_Bank") + "";
                            Guid gBankGuid;
                            Guid.TryParse(strbankGUID, out gBankGuid);

                            AddRoBankName(row, bankList, gBankGuid);
                        }
                    }
                }
                #endregion
                return mainJsonModel;
            }
            catch (Exception ex)
            {
                mainJsonModel.result = JsonModelConstant.Error;
                mainJsonModel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return mainJsonModel;
            }

        }
        /// <summary>
        /// 设置JsonAttributeModel值

        /// </summary>
        /// <param name="row"></param>
        /// <param name="itemFieldGUIDDic"></param>
        /// <param name="planitemSetupList"></param>
        /// <param name="setUpList"></param>
        /// <param name="personItemSetList"></param>
        /// <param name="preDetailList"></param>
        /// <param name="currentDetailList"></param>
        /// <param name="planItemList"></param>
        /// <param name="itemValueGUID"></param>
        /// <param name="ep"></param>
        private void ReSetJsonAttributeModelValue(List<JsonAttributeModel> row, Dictionary<string, string> itemFieldGUIDDic, List<SA_PlanItemSetupModel> planitemSetupList, List<SA_SetUp> setUpList
            , List<SA_PersonItemSetView> personItemSetList, List<SA_PlanActionDetailView> preDetailList, List<SA_PlanActionDetailView> currentDetailList, List<SA_PlanItemView> planItemList,
            Dictionary<string, string> itemValueGUID, Infrastructure.Expression.ExpressionParser ep)
        {
            var personGUID = row.GetValueByAttribute("GUID_Person");
            Guid gPersonGuid;
            Guid.TryParse(personGUID.ToString(), out gPersonGuid);
            foreach (JsonAttributeModel attr in row)
            {
                //保护Item项 更改数据
                var itemName = attr.n.ToLower();
                itemName = itemName.Substring(1);
                if (!itemName.ToLower().Contains("itemname")) continue;
                var itemGUID = itemFieldGUIDDic[itemName];
                Guid gItemGUID;
                Guid.TryParse(itemGUID, out gItemGUID);
                //根据ItemGUID查找对应的加载类型


                SA_PlanItemSetupModel planitemsetupModel = planitemSetupList.Find(e => e.GUID_Item == gItemGUID);
                if (planitemsetupModel == null) continue;
                SA_SetUp setupModel = setUpList.Find(e => e.GUID == planitemsetupModel.GUID_SetUP);
                switch (setupModel.SetUpKey)
                {
                    case "01"://手工录入
                        //不作任何处理
                        break;
                    case "02"://默认值获取

                        SetDefaultValue(attr, personItemSetList, gPersonGuid, gItemGUID);
                        break;
                    case "03"://上期数据获取
                        SetPreDataValue(attr, preDetailList, gPersonGuid, gItemGUID);
                        break;
                    case "04"://当期数据获取
                        SetCurrentDataValue(attr, currentDetailList, gPersonGuid, gItemGUID);
                        break;
                    case "05"://变量计算
                        SetGSValue(attr, planItemList, itemValueGUID, ep, gItemGUID, gPersonGuid);
                        break;
                    case "06"://公式加载
                        SetGSValue(attr, planItemList, itemValueGUID, ep, gItemGUID, gPersonGuid);
                        break;
                }

            }
        }
        /// <summary>
        /// 获取Item 字典表

        /// </summary>
        /// <param name="row"></param>
        /// <param name="planItemList"></param>
        /// <param name="GUIDValuedic"></param>
        /// <param name="fieldNameGUIDdic"></param>
        private void GetItemDic(List<JsonAttributeModel> row, List<SA_PlanItemView> planItemList, ref Dictionary<string, string> GUIDValuedic, ref Dictionary<string, string> fieldNameGUIDdic)
        {
            foreach (JsonAttributeModel attr in row)
            {
                //Item项 更改数据
                var itemName = attr.n.ToLower();
                if (itemName.Contains("itemname"))
                {
                    itemName = itemName.Substring(1);
                    var itemkey = itemName.Replace("itemname", "");
                    var list = planItemList.Find(e => e.ItemKey + "".Trim() == itemkey);
                    if (list != null)
                    {
                        double dValue = 0;
                        double.TryParse(attr.v, out dValue);
                        //GUID:Value
                        GUIDValuedic[list.GUID_Item.ToString().ToUpper()] = attr.v == "" ? "0" : dValue.ToString();
                        //字段名:GUID
                        fieldNameGUIDdic[itemName] = list.GUID_Item.ToString().ToLower();
                    }
                }
            }

        }

        /// <summary>
        /// 添加部门GUID对应的名称

        /// </summary>
        /// <param name="row"></param>
        /// <param name="dep"></param>
        /// <param name="guidDep"></param>
        private void AddRowDepartmentName(List<JsonAttributeModel> row, List<SS_Department> dep, Guid guidDep)
        {
            var model = dep.Find(e => e.GUID == guidDep);
            if (model != null)
            {
                row.Add(new JsonAttributeModel("DepartmentName", model.DepartmentName, "SA_PlanPersonSetModel"));
            }
        }
        private void AddRoBankName(List<JsonAttributeModel> row, List<SS_Bank> dep, Guid guidDep)
        {
            var model = dep.Find(e => e.GUID == guidDep);
            if (model != null)
            {
                row.Add(new JsonAttributeModel("BankName", model.BankName, "SA_PlanPersonSetModel"));
            }
        }
        /// <summary>
        /// 上期数据获取
        /// </summary>
        /// <param name="planModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
        private List<SA_PlanActionDetailView> GetPreDataDetailList(SA_PlanView planModel, SA_PlanAction main)
        {
            List<SA_PlanActionDetailView> list = new List<SA_PlanActionDetailView>();
            if (planModel != null)
            {
                var year = main.ActionYear == null ? 0 : (int)main.ActionYear;
                var month = main.ActionMouth == null ? 0 : (int)main.ActionMouth;
                var peroid = main.ActionPeriod == null ? 0 : (int)main.ActionPeriod;
                var q = this.BusinessContext.SA_PlanActionDetailView.Where(e => e.GUID_Plan == planModel.GUID);
                switch (planModel.PlanAreaName)
                {
                    case "年":
                        year = year - 1;
                        list = q.Where(e => e.ActionYear == year).ToList();
                        break;
                    case "季":
                        var monthList = PreQuarterMonth(month);
                        q = q.Where(e => e.ActionMouth != null && monthList.Contains((int)e.ActionMouth));
                        if (month <= 3)
                        {
                            //上一年


                            year = year - 1;
                            list = q.Where(e => e.ActionYear == year).ToList();
                        }
                        else
                        {
                            list = q.Where(e => e.ActionYear == year).ToList();
                        }
                        break;
                    case "月":
                        if (month == 1)
                        {
                            month = 12;
                            year = year - 1;
                        }
                        else
                        {
                            month = month - 1;
                        }
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month).ToList();
                        break;
                    case "询":
                        if (peroid == 1)
                        {
                            peroid = 3;
                            month = month - 1;
                        }
                        else
                        {
                            peroid = peroid - 1;
                        }
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;
                    case "周":
                        if (peroid == 1)
                        {
                            peroid = 4;
                            month = month - 1;
                        }
                        else
                        {
                            peroid = peroid - 1;
                        }
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;
                    case "日":
                        if (peroid == 1)
                        {
                            month = month - 1;
                            peroid = GetDaysByMonth(year, month);
                        }
                        else
                        {
                            peroid = peroid - 1;
                        }
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;

                }
            }
            return list;
        }
        /// <summary>
        /// 当期数据获取
        /// </summary>
        /// <param name="planModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
        private List<SA_PlanActionDetailView> GetCurrentDataDetailList(SA_PlanView planModel, SA_PlanAction main)
        {
            List<SA_PlanActionDetailView> list = new List<SA_PlanActionDetailView>();
            if (planModel != null)
            {
                var year = main.ActionYear == null ? 0 : (int)main.ActionYear;
                var month = main.ActionMouth == null ? 0 : (int)main.ActionMouth;
                var peroid = main.ActionPeriod == null ? 0 : (int)main.ActionPeriod;
                var q = this.BusinessContext.SA_PlanActionDetailView.Where(e => e.GUID_Plan == planModel.GUID);
                switch (planModel.PlanAreaName)
                {
                    case "年":
                        list = q.Where(e => e.ActionYear == year).ToList();
                        break;
                    case "季":
                        var monthList = QuarterMonth(month);
                        q = q.Where(e => e.ActionYear == year && e.ActionMouth != null && monthList.Contains((int)e.ActionMouth));
                        break;
                    case "月":
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month).ToList();
                        break;
                    case "询":
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;
                    case "周":
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;
                    case "日":
                        list = q.Where(e => e.ActionYear == year && e.ActionMouth == month && e.ActionPeriod == peroid).ToList();
                        break;

                }
            }
            return list;
        }

        /// <summary>
        /// 获取月的天数
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private int GetDaysByMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }
        /// <summary>
        /// 月份的天数


        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        private int GetMonthDays(int year, int month)
        {
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 12:
                    return 31;
                case 4:
                case 6:
                case 8:
                case 9:
                case 11:
                    return 30;
                case 2:
                    return TwoMonthDay(year);

            }
            return 0;
        }
        /// <summary>
        /// 二月天数
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        private int TwoMonthDay(int year)
        {
            if (year % 400 == 0 && year % 100 != 0 && year % 4 == 0)
            {
                return 29;
            }
            else
            {
                return 28;
            }
        }
        /// <summary>
        /// 根据月获取季度月
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        private List<int> QuarterMonth(int month)
        {
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return new List<int>() { 1, 2, 3 };
                case 4:
                case 5:
                case 6:
                    return new List<int>() { 4, 5, 6 };
                case 7:
                case 8:
                case 9:
                    return new List<int>() { 7, 8, 9 };
                case 11:
                case 12:
                case 13:
                    return new List<int>() { 11, 12, 13 };
            }
            return null;
        }
        /// <summary>
        /// 上一个季度


        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        private List<int> PreQuarterMonth(int month)
        {
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return new List<int>() { 11, 12, 13 };
                case 4:
                case 5:
                case 6:
                    return new List<int>() { 1, 2, 3 };
                case 7:
                case 8:
                case 9:
                    return new List<int>() { 4, 5, 6 };
                case 11:
                case 12:
                case 13:
                    return new List<int>() { 7, 8, 9 };
            }
            return null;
        }
        /// <summary>
        /// 默认值获取


        /// </summary>
        /// <param name="jsonModel"></param>
        private void SetDefaultValue(JsonAttributeModel attr, List<SA_PersonItemSetView> personItemSetList, Guid personGuid, Guid itemGuid)
        {
            var model = personItemSetList.Find(e => e.GUid_SS_Person == personGuid && e.Guid_SA_Item == itemGuid);
            if (model != null)
            {
                attr.v = model.DefaultValue;
            }
        }
        /// <summary>
        /// 公式设置值


        /// </summary>
        /// <param name="attr"></param>
        /// <param name="?"></param>
        private void SetGSValue(JsonAttributeModel attr, List<SA_PlanItemView> planItemList, Dictionary<string, string> itemDic, Infrastructure.Expression.ExpressionParser ep, Guid itemGUID, Guid PersonId)
        {
            var planItemModel = planItemList.Find(e => e.GUID_Item == itemGUID);
            if (planItemModel != null)
            {
                var itemformula = planItemModel.ItemFormula.ToUpper();
                if (string.IsNullOrEmpty(itemformula)) return;
                //替换公式中对应的值


                foreach (KeyValuePair<string, string> kv in itemDic)
                {
                    var key = kv.Key.ToUpper();
                    var value = kv.Value;
                    if (itemformula.Contains(key))
                    {
                        itemformula = itemformula.Replace("{" + key + "}", string.IsNullOrEmpty(value.Trim()) ? "0" : value);
                        itemformula = FilterFormula(itemformula, PersonId);
                    }
                }
                bool isC = true;
                if (itemformula.Contains("@")) isC = false;
                itemformula = FilterFormula(itemformula, PersonId);

                object relust;
                try
                {
                    if (isC)
                    {
                        var success = ep.Parser(itemformula, out relust);
                    }
                    else
                    {
                        relust = itemformula;
                    }

                }
                catch
                {
                    relust = null;
                }
                //判断一下 success 正确才赋值


                attr.v = relust == null ? "" : relust.ToString();
                //if (itemDic.ContainsKey(planItemModel.GUID_Item.ToString()))
                //{
                //    itemDic[planItemModel.GUID_Item.ToString()] = curRow.v;
                //}
            }
        }
        /// <summary>
        /// 获取上期数据
        /// </summary>
        private void SetPreDataValue(JsonAttributeModel attr, List<SA_PlanActionDetailView> planActionDetailList, Guid personGuid, Guid itemGuid)
        {
            var model = planActionDetailList.Find(e => e.GUID_Person == personGuid && e.GUID_Item == itemGuid);
            if (model != null)
            {
                attr.v = model.ItemValue == null ? "" : model.ItemValue.ToString();
            }
        }
        /// <summary>
        /// 设置获取当期数据
        /// </summary>
        private void SetCurrentDataValue(JsonAttributeModel attr, List<SA_PlanActionDetailView> planActionDetailList, Guid personGuid, Guid itemGuid)
        {
            var model = planActionDetailList.Find(e => e.GUID_Person == personGuid && e.GUID_Item == itemGuid);
            if (model != null)
            {
                attr.v = model.ItemValue == null ? "" : model.ItemValue.ToString();
            }
        }
        /// <summary>
        /// 获取上传数据（导入数据）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public JsonModel GetUploadFileData(string filePath, JsonModel jsonModel)
        {
           
            JsonModel jmodel = new JsonModel();
            jmodel.m = jsonModel.m;
            //主表信息
            SA_PlanAction model = new SA_PlanAction();
            model.Fill(jsonModel.m);
            
            //返回计划列表项字段

            List<string> planItemContaintProperty = new List<string>();
            planItemContaintProperty.AddRange(new string[] { "GUID", "ItemName", "ItemKey", "ItemType" }.ToList());
            string message = string.Empty;
            var sheetName = ImportExcel.GetSheet(filePath, 0);
            var data = ImportExcel.Import(filePath, sheetName, out message);
            if (!string.IsNullOrEmpty(message))
            {
                jmodel.s = new JsonMessage("提示", message, "error");
                jmodel.result = JsonModelConstant.Error;
                return jmodel;
            }
            //1.根据选择的计划加载对应的计划项

            //2.组织数据 人对应的Item列

            //3.返回

            //人员信息
            var personList = this.InfrastructureContext.SS_PersonView.ToList();
            //计划人员设置信息
            var planPersonList = this.InfrastructureContext.SA_PlanPersonSetView.OrderBy(e => e.DepartmentKey).Where(e => e.GUID_SA_Plan == model.GUID_Plan).ToList();
            //计划工资项
          
        
            var itemList = this.InfrastructureContext.SA_PlanItemView.OrderBy(e => e.ItemOrder).Where(e => e.GUID_Plan == model.GUID_Plan && e.IsStop == false).ToList();
            //行数据

            //上期数据 2016-3-17
            var planModel1 = this.InfrastructureContext.SA_PlanView.FirstOrDefault(e => e.GUID == model.GUID_Plan);
            //上期数据获取 待优化 有那个类型的数据加载那类
            List<SA_PlanActionDetailView> preDetailList = GetPreDataDetailList(planModel1, model);

            jmodel.m.Add(new JsonAttributeModel() { m = "SA_PlanAction", n = "PlanName", v = planModel1.PlanName });
            jmodel.m.Add(new JsonAttributeModel() { m = "SA_PlanAction", n = "DWName", v = "国家基础地理信息中心" });
            List<List<JsonAttributeModel>> detailListPicker = new List<List<JsonAttributeModel>>();
            for (int i = 0, j = data.Rows.Count; i < j; i++)//第一行为表头信息
            {
                var row = data.Rows[i];
              
                List<JsonAttributeModel> attributeList = new List<JsonAttributeModel>();
                //添加人员工资计划
                var personName = (row[1] + "").Replace(",","").Replace("，","").Trim();//姓名 替换掉用友导出的,和空格
                var personModel = personList.Find(e => e.PersonName + "" == personName);
                if (personModel == null) continue;
                var preRow= preDetailList.Where(e => e.GUID_Person == personModel.GUID);//上一个月的个人发放工资明细
                var planModel = planPersonList.Find(e => e.PersonName == personName);
                //人员工资计划设置
                SA_PlanPersonSetModel setModel = new SA_PlanPersonSetModel();
                if (planModel != null)
                {
                    setModel.GUID_Person = planModel.GUID_SS_Person;
                    setModel.PersonKey = planModel.PersonKey;
                    setModel.PersonName = planModel.PersonName;
                    setModel.GUID_Department = planModel.GUID_Department;
                    setModel.DepartmentName = planModel.DepartmentName;
                    setModel.GUID_Bank = planModel.GUID_SS_Bank;
                    setModel.BankName = planModel.BankName;
                    setModel.BankCardNo = planModel.BankCardNo;
                }
                else
                {
                    setModel.GUID_Person = personModel.GUID;
                    setModel.PersonKey = personModel.PersonKey;
                    setModel.PersonName = personModel.PersonName;
                    setModel.GUID_Department = personModel.GUID_Department;
                    setModel.DepartmentName = personModel.DepartmentName;
                    setModel.GUID_Bank = null;
                    setModel.BankName = "";
                    setModel.BankCardNo = personModel.BankCardNo;
                }

                attributeList.AddRange(setModel.ClassPick());

                //添加明细工资项信息

                var headRow = data.Rows[0];
                var colCount = data.Columns.Count;
                for (int m = 0; m < itemList.Count; m++)
                {
                    var gzItem=itemList[m];
                    if (data.Columns.Contains(gzItem.ItemName))//从excel列中找不到 去上一个工资取数
                    {
                        var columnValue = (row[gzItem.ItemName] + "").Replace(",", "").Replace(",", "").Replace(",", "");
                        if (string.IsNullOrEmpty(columnValue))
                        {
                            columnValue = "0.00";
                        }
                        //工资项目                       
                        attributeList.AddRange(gzItem.PickMergeN(columnValue + "", planItemContaintProperty)); //"ItemName", "ItemKey",
                    }
                    else 
                    {
                        var ent = preRow.FirstOrDefault(e => e.ItemName == gzItem.ItemName);
                        attributeList.AddRange(gzItem.PickMergeN((ent.ItemValue+ "").Replace(",", "").Replace(",", "").Replace(",", "") + "", planItemContaintProperty)); //"ItemName", "ItemKey",
                    }
                }
                detailListPicker.Add(attributeList);

            }
            //明细信息
            JsonGridModel djgm = new JsonGridModel("SA_PlanActionDetail");
            djgm.r.AddRange(detailListPicker);
            jmodel.d.Add(djgm);
            //添加默认值（为工资列表项）
            return jmodel;

        }
        /// <summary>
        /// 添加默认值


        /// </summary>
        /// <param name="jmodel"></param>
        private void AddJsonModel_F(JsonModel jmodel, List<SA_PlanItemView> itemList, string modelName, List<string> planItemContaintProperty)
        {
            //添加默认值（为工资列表项）


            JsonGridModel fjgm = new JsonGridModel(modelName);

            foreach (SA_PlanItemView item in itemList)
            {
                fjgm.r.Add(item.Pick(planItemContaintProperty));
            }
            jmodel.f.Add(fjgm);
        }
        /// <summary>
        /// 判断编号是否设置日期并且是否改变
        /// </summary>
        /// <param name="orgDateTime">原日期</param>
        /// <param name="currentDateTime">当前修改日期</param>
        /// <returns>Bool</returns>
        private bool IsDateChange(DateTime orgDateTime, DateTime currentDateTime)
        {
            bool returnValue = false;
            if (currentDateTime != null && currentDateTime != DateTime.MinValue)
            {

                SS_DocNumber dnModel = this.BusinessContext.SS_DocNumber.FirstOrDefault();
                if ((bool)dnModel.IsYear || (bool)dnModel.IsMonth)//生成的编号设置了时间
                {
                    if (orgDateTime.Year != currentDateTime.Year || orgDateTime.Month != currentDateTime.Month)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// 更改单据状态


        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="docState">单据状态</param>
        /// <returns>Bool</returns>
        public override bool UpdateDocState(Guid guid, Business.Common.EnumType.EnumDocState docState)
        {
            try
            {
                var bcontext = new BusinessEdmxEntities();
                var model = bcontext.SA_PlanAction.FirstOrDefault(e => e.GUID == guid);
                if (model != null)
                {
                    model.ActionState =(int)docState;
                }
                bcontext.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        //public override bool UpdateDocState(Guid guid, Guid processId)
        //{
        //    try
        //    {
        //        var bcontext = new BusinessEdmxEntities();
            
        //        var model = bcontext.SA_PlanAction.FirstOrDefault(e => e.GUID == guid);
        //        if (model != null)
        //        {
        //            model.ActionState = sttea;
        //        }
        //        bcontext.SaveChanges();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        return false;
        //    }
        //}
        /// <summary>
        /// 工资项数据加载方式设置


        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        public JsonModel SaveItemSet(JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            if (jsonModel.m == null) return null;

            SA_PlanItemSetup main = new SA_PlanItemSetup();
            //工资列表数据            
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                SA_PlanActionPaymentnumber temp = new SA_PlanActionPaymentnumber();
                string detailModelName = typeof(SA_PlanActionPaymentnumber).Name;
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        UpdateSA_PlanItemSetup(row);
                    }
                }
            }

            this.InfrastructureContext.SaveChanges();
            return result;
        }

        /// <summary>
        /// 添加工资类款项设置明细信息


        /// </summary>
        /// <param name="main"></param>
        private void UpdateSA_PlanItemSetup(List<JsonAttributeModel> row)
        {
            //工资项数据加载方式设置


            SA_PlanItemSetupModel itemSet = new SA_PlanItemSetupModel();
            itemSet.ClassFill(row);
            if (!itemSet.GUID.IsNullOrEmpty())
            {
                var model = this.InfrastructureContext.SA_PlanItemSetup.FirstOrDefault(e => e.GUID == itemSet.GUID);
                if (model != null)
                {
                    model.GUID_SetUP = itemSet.GUID_SetUP.IsNullOrEmpty() ? Guid.Empty : (Guid)itemSet.GUID_SetUP;
                    model.IsStart = itemSet.IsStart == "是" ? true : false;
                    this.InfrastructureContext.ModifyConfirm(model);
                }
            }
        }

        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public JsonModel RetrievePlanItemSetup(Guid planGuid)
        {
            /*
             select tem.guid_item,tem.guid as planitemGUID,itemName,
            case itemtype when 1 then '金额' 
            when 2 then '日期' 
            when 3 then '文本' 
            end as itemtype,
            isnull(ssu.guid,(select top 1 guid from SA_SetUp where setupkey='01')) as setupGUID 
            ,setupkey,isnull(ssu.setupname,'手工录入') as setupName,itemOrder,spis.IsStart      
            from (  
	            select spi.guid,si.itemname,si.guid as guid_item,si.itemtype,spi.itemorder  
	            from SA_Plan sp join SA_Planitem spi on  sp.guid=spi.guid_plan  join SA_Item si on spi.guid_item=si.guid 
	            where sp.guid='{FD92C4AE-079D-3040-BDE2-EB4A4512593B}'  
            ) tem  
            left join SA_PlanItemSetup spis   on tem.guid=spis.guid_sa_planitem   
            left join SA_SetUp ssu   on spis.guid_setup=ssu.guid  order by itemorder 
             */
            JsonModel jmodel = new JsonModel();
            List<string> fieldList = new List<string>();
            fieldList.AddRange(new string[] { "PlanKey", "PlanName", "GUID" }.ToList());
            SA_PlanView main = this.InfrastructureContext.SA_PlanView.FirstOrDefault(e => e.GUID == planGuid);
            if (main != null)
            {
                jmodel.m = main.Pick(fieldList);
            }

            try
            {
                var setupModel = this.InfrastructureContext.SA_SetUp.FirstOrDefault(e => e.SetUpKey == "01");
                var setupList = (from sapi in this.InfrastructureContext.SA_PlanItemView
                                 join piSet in this.InfrastructureContext.SA_PlanItemSetup on sapi.GUID equals piSet.GUID_SA_PlanItem into temp
                                 from piSet in temp.DefaultIfEmpty()
                                 join set in this.InfrastructureContext.SA_SetUp on piSet.GUID_SetUP equals set.GUID into tempSet
                                 from set in tempSet.DefaultIfEmpty()
                                 where sapi.GUID_Plan == planGuid
                                 orderby sapi.ItemOrder
                                 select new SA_PlanItemSetupModel
                                 {
                                     GUID = piSet.GUID,
                                     GUID_Item = sapi.GUID_Item,
                                     ItemName = sapi.ItemName,
                                     ItemType = sapi.ItemType == 1 ? "现金" : (sapi.ItemType == 2 ? "日期" : "文本"),
                                     GUID_SetUP = piSet.GUID_SetUP == null ? setupModel.GUID : piSet.GUID_SetUP,
                                     SetUpName = set.SetUpName == null ? setupModel.SetUpName : set.SetUpName,
                                     SetUpKey = set.SetUpKey == null ? setupModel.SetUpKey : set.SetUpName,
                                     IsStart = piSet.IsStart == true ? "是" : "否"
                                 }).ToList();
                List<List<JsonAttributeModel>> rowAll = new List<List<JsonAttributeModel>>();
                foreach (SA_PlanItemSetupModel item in setupList)
                {
                    rowAll.Add(item.ClassPick());
                }
                JsonGridModel djgm = new JsonGridModel(typeof(SA_PlanItemSetupModel).Name);
                djgm.r.AddRange(rowAll);
                jmodel.d.Add(djgm);


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

        #region 导出
        /// <summary>
        /// 创建导出的数据

        /// </summary>
        /// <param name="colName"></param>
        /// <param name="josnModel"></param>
        /// <returns></returns>
        public DataTable CreateExportData(List<ColModel> colNameList, JsonModel josnModel)
        {
            DataTable dt = new DataTable();
            dt = CreateNewDataTable(colNameList);
            var rows = josnModel.d[0].r;
            DataRow dtRow = null;
            int rowsCount = rows.Count;
            int colCount = dt.Columns.Count;
            for (int i = 0; i < rowsCount; i++)
            {
                dtRow = dt.NewRow();
                //int j = 0;
                foreach (JsonAttributeModel item in rows[i])
                {
                    dtRow[item.n] = item.v;
                    double d = 0F;
                    if (item.n.ToLower() != "personkey" && item.n.ToLower() != "personname")
                    {
                        if (double.TryParse(item.v, out d))
                        {
                            dtRow[item.n] = FormatMoney(1, d);
                        }
                    }
                    if (i == rowsCount - 1)//处理合计
                    {
                        if (item.n.ToLower() == "personkey")
                        {
                            dtRow[item.n] = "";
                        }
                        if (item.n.ToLower() == "personname")
                        {
                            dtRow[item.n] = "合计";
                        }
                    }
                }
                dt.Rows.Add(dtRow);
            }
            return dt;
        }
        /// <summary>
        /// 创建Datatable表

        /// </summary>
        /// <param name="colNameArr"></param>
        /// <returns></returns>
        private DataTable CreateNewDataTable(List<ColModel> colNameList)
        {
            DataTable dt = new DataTable();
            for (int i = 0; i < colNameList.Count; i++)
            {
                dt.Columns.Add(colNameList[i].ColKey);
            }
            //添加一行数据

            var row = dt.NewRow();
            for (int i = 0; i < colNameList.Count; i++)
            {
                row[i] = colNameList[i].ColName;
            }
            dt.Rows.Add(row);
            return dt;
        }

        /// <summary>
        /// 转化成货币表达式
        /// </summary>
        /// <param name="ftype">类型：0表示带￥的货币表达式 1表示不带￥的表达式，默认不带￥</param>
        /// <param name="fmoney">要转化的值</param>
        /// <returns>String</returns>
        public string FormatMoney(int ftype, double fmoney)
        {
            string _fmoney = string.Empty;

            fmoney = double.Parse(Convert.ToDouble(fmoney).ToString("0.00"));
            switch (ftype)
            {
                case 0:
                    _fmoney = string.Format("{0:C2}", fmoney);
                    break;
                case 1:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
                default:
                    _fmoney = string.Format("{0:N2}", fmoney);
                    break;
            }
            return _fmoney;
        }
        #endregion

        #region 验证
        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="jsonModel">JsonModel</param>
        /// <param name="status">状态</param>
        /// <returns>string</returns>
        private string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SA_PlanAction main = null; ; //new BX_Main();

            switch (status)
            {
                case "1": //新建
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    var v = DetailItemVerify(jsonModel);
                    if (v != null)
                    {
                        vResult._validation.Add(v);
                    }
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);//修改验证
                    var v1 = DetailItemVerify(jsonModel);
                    if (v1 != null)
                    {
                        vResult._validation.Add(v1);
                    }
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new SA_PlanAction().ModelName());
                    vResult = DeleteVerify(value);
                    strMsg = DataVerifyMessage(vResult);
                    break;

            }
            return strMsg;
        }
        /// <summary>
        /// 验证工资项

        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private ValidationResult DetailItemVerify(JsonModel jsonModel)
        {
            List<JsonAttributeModel> itemCol = jsonModel.d[0].r[0].Find(typeof(SA_PlanItem).Name);
            if (itemCol == null || itemCol.Count == 0)
            {
                ValidationResult model = new ValidationResult();
                model.MemberName = "";
                model.Message = "工资项为0项，请设置工资项！";
                return model;
            }
            return null;
        }
        /// <summary>
        /// 验证信息
        /// </summary>
        /// <param name="vResult"></param>
        /// <returns></returns>
        private string DataVerifyMessage(VerifyResult vResult)
        {
            string strMsg = string.Empty;
            if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
            {
                for (int i = 0; i < vResult.Validation.Count; i++)
                {
                    strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                }
            }
            return strMsg;
        }
        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private SA_PlanAction LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SA_PlanAction main = new SA_PlanAction();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                SA_PlanActionDetail temp = new SA_PlanActionDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null && Grid.r != null)
                {
                    for (int i = 0; i < Grid.r.Count - 1; i++)
                    {
                        List<JsonAttributeModel> row = Grid.r[i];
                        temp = new SA_PlanActionDetail();
                        temp.Fill(row);
                        SA_PlanPersonSetModel detailModel = new SA_PlanPersonSetModel();
                        detailModel.ClassFill(row);
                        temp.GUID_Person = detailModel.GUID_Person == null ? Guid.Empty : (Guid)detailModel.GUID_Person;
                        temp.GUID_Department = detailModel.GUID_Department == null ? Guid.Empty : (Guid)detailModel.GUID_Department;
                        temp.GUID_Bank = detailModel.GUID_Bank;
                        temp.BankCardNo = detailModel.BankCardNo;

                        main.SA_PlanActionDetail.Add(temp);
                    }
                }
            }

            return main;
        }
        /// <summary>
        /// 明显表验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetail(SA_PlanAction data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<SA_PlanActionDetail> detailList = new List<SA_PlanActionDetail>();
            foreach (SA_PlanActionDetail item in data.SA_PlanActionDetail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (SA_PlanActionDetail item in detailList)
                {
                    rowIndex++;
                    //if (rowIndex == detailList.Count - 1) continue;
                    var vf_detail = VerifyResult_gzd_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                resultList.Add(new ValidationResult("", "请添加明细科目信息！"));

            }

            return resultList;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SA_PlanAction data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            SA_PlanAction mModel = data;

            object g;

            #region   主表字段验证

            //发放日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "发放日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "发放日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }


            //制单日期
            if (mModel.MakeDate.IsNullOrEmpty())
            {
                str = "制单日期 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.MakeDate.GetType(), mModel.MakeDate.ToString(), out g) == false)
                {
                    str = "制单日期格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            return resultList;

            #endregion
        }
        /// <summary>
        /// 支付码验证



        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_CN_PaymentNumber(CN_PaymentNumber data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            /// <summary>
            /// 财富支付码表字段验证
            /// </summary>
            #region 财富支付码表字段验证


            if (!string.IsNullOrEmpty(data.PaymentNumber) && Common.ConvertFunction.TryParse(data.PaymentNumber.GetType(), data.PaymentNumber, out g) == false)
            {
                str = "财政支付码格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //是否国库
            if (data.IsGuoKu.ToString() == "")
            {
                str = "是否国库 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(data.IsGuoKu.GetType(), data.IsGuoKu.ToString(), out g) == false)
                {
                    str = "是否国库格式不能为空！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {

                    //如果不为空则,则支付码不能为空
                    if (data.IsGuoKu == true && string.IsNullOrEmpty(data.PaymentNumber))
                    {
                        str = "财政支付令不能为空！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //是否项目
            if (data.IsProject.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.IsProject.GetType(), data.IsProject.ToString(), out g) == false)
            {
                str = "项目格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //功能分类GUID
            if (data.GUID_FunctionClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_FunctionClass.GetType(), data.GUID_FunctionClass.ToString(), out g) == false)
            {
                str = "功能分类格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //预算科目GUID
            if (data.GUID_BGCode.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGCode.GetType(), data.GUID_BGCode.ToString(), out g) == false)
            {
                str = "预算科目格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //经济分类GUID
            if (data.GUID_EconomyClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_EconomyClass.GetType(), data.GUID_EconomyClass.ToString(), out g) == false)
            {
                str = "经济分类格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //支出类型GUID
            if (data.GUID_ExpendType.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_ExpendType.GetType(), data.GUID_ExpendType.ToString(), out g) == false)
            {
                str = "支出类型格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //项目GUID
            if (data.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_Project.GetType(), data.GUID_Project.ToString(), out g) == false)
            {
                str = "项目格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //项目财政编号
            if (!string.IsNullOrEmpty(data.FinanceProjectKey) && Common.ConvertFunction.TryParse(data.FinanceProjectKey.GetType(), data.FinanceProjectKey.ToString(), out g) == false)
            {
                str = "项目财政编号格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //预算来源GUID
            if (data.GUID_BGResource.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGResource.GetType(), data.GUID_BGResource.ToString(), out g) == false)
            {
                str = "预算来源格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }

            #endregion

            return resultList;
        }
        /// <summary>
        /// 明显表验证


        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_gzd_Detail(SA_PlanActionDetail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            SA_PlanActionDetail item = data;

            /// <summary>
            /// 明细表字段验证

            /// </summary>
            #region 明细表字段验证


            //人员GUID
            if (item.GUID_Person.IsNullOrEmpty())
            {
                str = "明细人员 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Person.GetType(), item.GUID_Person.ToString(), out g) == false)
                {
                    str = "明细人员格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            //部门
            if (item.GUID_Department.IsNullOrEmpty())
            {
                str = "明细所属部门 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                {
                    str = "明细所属部门格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            //银行
            if (item.GUID_Bank.IsNullOrEmpty())
            {
                str = "明细所属银行 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Bank.GetType(), item.GUID_Bank.ToString(), out g) == false)
                {
                    str = "明细所属银行 格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            //银行卡号
            if (string.IsNullOrEmpty(item.BankCardNo))
            {
                str = "明细银行卡号字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            }

            ////工资款项GUID
            //if (item.GUID_Item.IsNullOrEmpty())
            //{
            //    str = "工资款项隐藏GUID不能为空！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_Item.GetType(), item.GUID_Item.ToString(), out g) == false)
            //    {
            //        str = "工资款项隐藏GUID不能为空！！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}


            ////人员GUID
            //if (item.GUID_Person.IsNullOrEmpty())
            //{
            //    str = "明细人员 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_Person.GetType(), item.GUID_Person.ToString(), out g) == false)
            //    {
            //        str = "明细人员格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}

            //金额
            //if (item.ItemValue.ToString() == "")
            //{
            //    str = "明细金额 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.ItemValue.GetType(), item.ItemValue.ToString(), out g) == false)
            //    {
            //        str = "明细金额格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //    else
            //    {
            //        if (double.Parse(g.ToString()) == 0F)
            //        {
            //            str = "明细金额不能为零！";
            //            resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            //        }
            //    }
            //}



            #endregion

            #region 支付码验证




            //if (item.CN_PaymentNumber != null)
            //{
            //    var vf_pn = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex);
            //    if (vf_pn != null && vf_pn.Count > 0)
            //    {
            //        resultList.AddRange(vf_pn);
            //    }
            //}
            #endregion
            return resultList;
        }

        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            SA_PlanAction model = (SA_PlanAction)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //判断同一工资计划 年月期间 是否已经发放
            var mainList = this.BusinessContext.SA_PlanActionView.Where(e => e.GUID_Plan == model.GUID_Plan && e.ActionYear == model.ActionYear && e.ActionMouth == model.ActionMouth && e.ActionPeriod == model.ActionPeriod).ToList();
            if (mainList != null && mainList.Count > 0)
            {
                Guid mainId = mainList[0].GUID;
                var detailList = this.BusinessContext.SA_PlanActionDetailView.Where(e => e.GUID_PlanAction == mainId).Select(e => new SA_PlanPersonSetModel { PersonName = e.PersonName, PersonKey = e.PersonKey }).Distinct().ToList();
                if (detailList != null && detailList.Count > 0)
                {
                    var str = mainList[0].PlanName + "工资计划 <br>" + mainList[0].ActionYear + "年" + mainList[0].ActionMouth + "月<br>已有下列人员发放工资：<br>";
                    foreach (SA_PlanPersonSetModel item in detailList)
                    {
                        str += item.PersonName + "(" + item.PersonKey + ") <br>";
                    }
                    result._validation.Add(new ValidationResult("", str));
                }
            }
            //明细验证
            var vf_detail = VerifyResultDetail(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;

        }

        /// <summary>
        /// 数据从数据库删除前验证


        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            SA_PlanAction bxMain = new SA_PlanAction();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "无删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "单据GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
            //流程验证

            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此单据正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
            }

            return result;
        }

        /// <summary>
        /// 数据更新到数据库验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult ModifyVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            SA_PlanAction model = (SA_PlanAction)data;

            //流程验证

            if (WorkFlowAPI.ExistProcessCurPerson(model.GUID,OperatorId))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }


            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            var vf_detail = VerifyResultDetail(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;
        }
        #endregion

    }

    public class SA_PlanActionDetailModel
    {
        /// <summary>
        /// 明细GUID
        /// </summary>
        public Guid GUID { set; get; }

    }
    public class SA_PlanPersonSetModel
    {
        public Guid? GUID_Person { set; get; }
        public string PersonKey { set; get; }
        public string PersonName { set; get; }
        public Guid? GUID_Department { set; get; }
        public string DepartmentName { set; get; }
        public Guid? GUID_Bank { set; get; }
        public string BankName { set; get; }
        public string BankCardNo { set; get; }
    }
    public class SA_ItemModel
    {
        public Guid GUID { set; get; }
        public string ItemName { set; get; }
        public string ItemKey { set; get; }
        public int? ItemType { set; get; }
    }
    public class TaxPerson
    {
        //GUID_InvitePerson,Sum(Total_bx) as SumTotal, Sum(Total_Tax) as SumTax
        public Guid GUID_InvitePerson { get; set; }
        public double SumTotal { get; set; }
        public double SumTax { get; set; }
    }
    public class ColModel
    {
        public string ColKey { set; get; }
        public string ColName { set; get; }
    }

}
