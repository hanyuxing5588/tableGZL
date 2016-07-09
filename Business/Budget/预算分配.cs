using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using Business.CommonModule;
using Platform.Flow.Run;
using BusinessModel;
using Platform.Flow;
namespace Business.Budget
{
  
    public class 预算分配 : BaseDocument
    {
        private Guid defaultGUID_DocType;
        private Guid defaultGUID_UIType;
        public 预算分配() : base() {
            defaultGUID_DocType = new Guid("63A2F559-2DF4-49E2-960B-B067C5424C6C");
            defaultGUID_UIType = new Guid("0717A6EC-A337-46E4-ADA2-E5D169870E06");
        }
        public 预算分配(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) {
            defaultGUID_DocType = new Guid("63A2F559-2DF4-49E2-960B-B067C5424C6C");
            defaultGUID_UIType = new Guid("0717A6EC-A337-46E4-ADA2-E5D169870E06");
        }

        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {

                JsonModel jmodel = new JsonModel();
                BG_AssignView model = new BG_AssignView();
                model.FillDefault(this, this.OperatorId);
                jmodel.m = model.Pick();
                // 预算设置
                JsonAttributeModel jmBGSetup = new JsonAttributeModel();
                BG_SetupView objBGSetup = this.InfrastructureContext.BG_SetupView.FirstOrDefault(e=> e.BGSetupKey=="08");
                jmBGSetup.n = "GUID_BGSetUp";
                jmBGSetup.v = objBGSetup.GUID.ToString();
                jmodel.m.Add(jmBGSetup);

                // 预算类型
                JsonAttributeModel jmBGType = new JsonAttributeModel();
                jmBGType.n = "GUID_BGTYPE";
                jmBGType.v = objBGSetup.GUID_BGType.ToString();
                jmodel.m.Add(jmBGType);

                // 预算步骤 
                JsonAttributeModel jmBGStep = new JsonAttributeModel();
                jmBGStep.n = "GUID_BGStep";
                jmBGStep.v = objBGSetup.GUID_BGStep.ToString();
                jmodel.m.Add(jmBGStep);

                //上级步骤
                JsonAttributeModel jmPBGSetup = new JsonAttributeModel();
                jmPBGSetup.n = "GUID_PStep";
                jmPBGSetup.v = objBGSetup.PBG_StepGUID.ToString();
                jmodel.m.Add(jmPBGSetup);
                //是否启用
                JsonAttributeModel jmIsPStep = new JsonAttributeModel();
                jmIsPStep.n = "IsPStep";
                jmIsPStep.v = "1";
                jmodel.m.Add(jmIsPStep);



                //Init(ref jmodel);


                //预算年度
                JsonAttributeModel jmBGYear = new JsonAttributeModel();
                jmBGYear.n = "BGYear";
                jmBGYear.v = (DateTime.Now.Year + 1).ToString();
                jmodel.m.Add(jmBGYear);

                var cyear = DateTime.Now.Year + 1;
                //开始时间，结束时间
                JsonAttributeModel jmBeginDate = new JsonAttributeModel();
                jmBeginDate.n = "BeginDate";
                jmBeginDate.v = cyear + "-1-1"; 
                jmodel.m.Add(jmBeginDate);
                JsonAttributeModel jmStopDate = new JsonAttributeModel();
                jmStopDate.n = "StopDate";
                jmStopDate.v = cyear + "-12-31";
                jmodel.m.Add(jmStopDate);

                //添加默认编制人变量和审批人变量
                List<JsonGridModel> list = new List<JsonGridModel>();
                //编制人
                BG_PreparersView pModel = new BG_PreparersView();
                JsonGridModel fpjgm = new JsonGridModel(pModel.ModelName());
                pModel.Variable = "预算编制";
                List<JsonAttributeModel> fpicker = pModel.Pick();
                fpjgm.r.Add(fpicker);
                pModel = new BG_PreparersView();
                pModel.Variable = "预算初始值";
                fpicker = pModel.Pick();
                fpjgm.r.Add(fpicker);
                list.Add(fpjgm);
                //审批人
                BG_ApproverView aModel = new BG_ApproverView();
                JsonGridModel fajgm = new JsonGridModel(aModel.ModelName());
                aModel.Variable = "部门预算审批";
                List<JsonAttributeModel> paicker = aModel.Pick();
                fajgm.r.Add(paicker);
                aModel = new BG_ApproverView();
                aModel.Variable = "财务预算审批";
                paicker = aModel.Pick();
                fajgm.r.Add(paicker);
                list.Add(fajgm);

                jmodel.d.AddRange(list);


                string strModelName = model.ModelName();
                foreach (JsonAttributeModel item in jmodel.m)
                {
                    item.m = strModelName;
                }
                
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Init(ref JsonModel jm)
        {
            SS_Operator Operator = this.OperatorId == Guid.Empty ? null : this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == this.OperatorId);
            if (Operator != null)
            {
                Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
                if (person != null)
                {
                    JsonAttributeModel jmDw = new JsonAttributeModel();
                    jmDw.n = "GUID_DW";
                    jmDw.v = person.GUID_DW.ToString();

                    JsonAttributeModel jmDep = new JsonAttributeModel();
                    jmDep.n = "GUID_Department";
                    jmDep.v = person.GUID_Department.ToString();

                    JsonAttributeModel jmOperator = new JsonAttributeModel();
                    jmOperator.n = "GUID_Maker";
                    jmOperator.v = Operator.GUID.ToString();

                    JsonAttributeModel jmMakeDate = new JsonAttributeModel();
                    jmMakeDate.n = "MakeDate";
                    jmMakeDate.v = DateTime.Now.ToString("yyyy-MM-dd HH:mm");


                    jm.m.Add(jmDw);
                    jm.m.Add(jmDep);
                    jm.m.Add(jmOperator);
                    jm.m.Add(jmMakeDate);
                }
            }
        }
        /**
         *      函数功能: 预算分配单据生成时，有两个编制人，两个审批人，这四个对象是随着业务分配对象
         *               一同存在的，因此在New() 函数中添加，这样界面上的grid就有这四个对象了
         *               Variable 现在是在后台设置，这样做不好，但是现在不知道该如何在前台进行配置
         *               
         *          日期:2014-4-11
         *        author: dongsheng.zhang
         * 
         * */
        private List<JsonGridModel> GetPreparesAndApprovers()
        {
            List<JsonGridModel> list = new List<JsonGridModel>();
          

            return list;
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
                BG_AssignView main = this.BusinessContext.BG_AssignView.FirstOrDefault(e => e.GUID == guid);
                if (main != null)
                {
                    jmodel.m = main.Pick();
                    //明细信息
                    GetDetail(jmodel, guid);
                }
                jmodel.s = new JsonMessage("", "", "");
                return jmodel;
            }
            catch (Exception ex)
            {
                // throw ex;
                jmodel.result = JsonModelConstant.Error;
                jmodel.s = new JsonMessage("提示...", "获取数据错误！", JsonModelConstant.Error);
                return jmodel;
            }
        }
        /// <summary>
        /// 明细列表信息
        /// </summary>
        /// <returns></returns>
        private void GetDetail(JsonModel jmodel,Guid guid)
        {
            List<JsonGridModel> list = new List<JsonGridModel>();
            //编制人
            var prepas = this.BusinessContext.BG_PreparersView.Where(e => e.GUID_BG_Assign == guid).OrderBy(e=>e.Variable).ToList();
            JsonGridModel fpjgm = new JsonGridModel("BG_Preparers");
            foreach (var item in prepas)
            {
                List<JsonAttributeModel> paicker = item.Pick();
                fpjgm.r.Add(paicker);
            }
            list.Add(fpjgm);
            //审批人
            var apprs = this.BusinessContext.BG_ApproverView.Where(e => e.GUID_BG_Assign == guid).OrderBy(e => e.Variable).ToList();
            JsonGridModel fajgm = new JsonGridModel("BG_Approver");
            foreach (var item in apprs)
            {
                List<JsonAttributeModel> paicker = item.Pick();
                fajgm.r.Add(paicker);
            }
            list.Add(fajgm);

            jmodel.d.AddRange(list);
            
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            BG_Assign main = new BG_Assign();
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            main.DocState = 1;
            main.GUID_DocType = defaultGUID_DocType;
            main.GUID_UIType = defaultGUID_UIType;
            main.BGMonth = 0;

            //if (jsonModel.d != null && jsonModel.d.Count > 0)
            //{
            //    string detailModelName = new SS_RunTimeUsersSet().ModelName();
            //    JsonGridModel Grid = jsonModel.d.Find(detailModelName);
            //    if (Grid != null)
            //    {
            //        foreach (List<JsonAttributeModel> row in Grid.r)
            //        {
            //            SS_RunTimeUsersSet objRtus = new SS_RunTimeUsersSet();
            //            objRtus.Fill(row);
            //            objRtus.DocId = main.GUID;
            //            this.BusinessContext.SS_RunTimeUsersSet.AddObject(objRtus);
            //        }
            //    }
            //}

            //编制人
            JsonGridModel pGrid = jsonModel.d.Find("BG_Preparers");
            if (pGrid != null)
            {
                foreach (List<JsonAttributeModel> row in pGrid.r)
                {
                    BG_Preparers item = new BG_Preparers();
                    item.Fill(row);
                    item.GUID_BG_Assign = main.GUID;
                    item.GUID = Guid.NewGuid();
                    this.BusinessContext.BG_Preparers.AddObject(item);
                }
            }
            //审批人
            pGrid = jsonModel.d.Find("BG_Approver");
            if (pGrid != null)
            {
                foreach (List<JsonAttributeModel> row in pGrid.r)
                {
                    BG_Approver item = new BG_Approver();
                    item.Fill(row);
                    item.GUID_BG_Assign = main.GUID;
                    item.GUID = Guid.NewGuid();
                    this.BusinessContext.BG_Approver.AddObject(item);
                }
            }



            this.BusinessContext.BG_Assign.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 添加明细信息
        /// </summary>
        /// <param name="main"></param>
        private void AddDetail(BG_Assign main, JsonModel jsonModel)
        {
            string detailModelName = new SS_RunTimeUsersSet().ModelName();
            JsonGridModel Grid = jsonModel.d.Find(detailModelName);
            if (Grid != null)
            {               
                foreach (List<JsonAttributeModel> row in Grid.r)
                {
                    AddBGPreparers(main, row);
                }
            }
            BG_Approver atemp = new BG_Approver();
            string detailaModelName = atemp.ModelName();
            JsonGridModel aGrid = jsonModel.d.Find(detailaModelName);
            if (aGrid != null)
            {
                foreach (List<JsonAttributeModel> row in aGrid.r)
                {
                    AddBGApprover(main, row);
                }
            }
        }
        /// <summary>
        /// 预算编制人
        /// </summary>
        /// <param name="main"></param>
        /// <param name="row"></param>
        private void AddBGPreparers(BG_Assign main, List<JsonAttributeModel> row)
        {
            //预算编制人
            BG_Preparers temp = new BG_Preparers();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            temp.GUID_BG_Assign = main.GUID;
            main.BG_Preparers.Add(temp);
        }
        /// <summary>
        /// 预算编制人
        /// </summary>
        /// <param name="main"></param>
        /// <param name="row"></param>
        private void AddBGApprover(BG_Assign main, List<JsonAttributeModel> row)
        {
            //预算编制人
            BG_Approver temp = new BG_Approver();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            temp.GUID_BG_Assign = main.GUID;
            main.BG_Approver.Add(temp);
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
            BG_Assign main = new BG_Assign(); 
           
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == g);
            //if (main != null)
            //{
            //    orgDateTime = (DateTime)main.DocDate;
            //}
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this,this.OperatorId);

            //string detailModelName = new SS_RunTimeUsersSet().ModelName();
            //JsonGridModel Grid = jsonModel.d.Find(detailModelName);
            //if (Grid != null)
            //{
            //    foreach (List<JsonAttributeModel> row in Grid.r)
            //    {
            //        JsonAttributeModel jm = row.Find(e=>e.n == "GUID");
            //        Guid guid = new Guid(jm.v);
            //        SS_RunTimeUsersSet objRtus = this.BusinessContext.SS_RunTimeUsersSet.FirstOrDefault(e => e.Id == guid);
            //        if (objRtus!=null)
            //        {
            //            objRtus.Fill(row);
            //            this.BusinessContext.ModifyConfirm(objRtus);
            //        }
            //    }
            //}
            //编制人
            JsonGridModel pGrid = jsonModel.d.Find("BG_Preparers");
            if (pGrid != null)
            {
                foreach (List<JsonAttributeModel> row in pGrid.r)
                {
                    JsonAttributeModel jm = row.Find(e => e.n == "GUID");
                    Guid guid = new Guid(jm.v);
                    var item = this.BusinessContext.BG_Preparers.FirstOrDefault(e => e.GUID == guid);
                    if (item != null)
                    {
                        item.Fill(row);
                        this.BusinessContext.ModifyConfirm(item);
                    }
                }
            }
            //审批人
            pGrid = jsonModel.d.Find("BG_Approver");
            if (pGrid != null)
            {
                foreach (List<JsonAttributeModel> row in pGrid.r)
                {
                    JsonAttributeModel jm = row.Find(e => e.n == "GUID");
                    Guid guid = new Guid(jm.v);
                    var item = this.BusinessContext.BG_Approver.FirstOrDefault(e => e.GUID == guid);
                    if (item != null)
                    {
                        item.Fill(row);
                        this.BusinessContext.ModifyConfirm(item);
                    }
                }
            }

            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            try
            {
                var bgMain = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID_BG_Assign == guid);
                if (bgMain != null) {
                    var details = this.BusinessContext.BG_Detail.Where(e => e.GUID_BG_Main == bgMain.GUID).ToList();
                    foreach (var item in details)
                    {
                        this.BusinessContext.DeleteConfirm(item);
                    }
                    this.BusinessContext.DeleteConfirm(bgMain);
                }

                BG_Assign main = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == guid);
                //List<SS_RunTimeUsersSet> listRtus = this.BusinessContext.SS_RunTimeUsersSet.Where(e=>e.DocId==main.GUID).ToList();
                //foreach(SS_RunTimeUsersSet item in listRtus)
                //{
                //    BusinessContext.DeleteConfirm(item);
                //}
                //编制人
                var pres = this.BusinessContext.BG_Preparers.Where(e => e.GUID_BG_Assign == guid).ToList();
                foreach (var item in pres) this.BusinessContext.DeleteConfirm(item);
                var aprs = this.BusinessContext.BG_Approver.Where(e => e.GUID_BG_Assign == guid).ToList();
                foreach (var item in aprs) this.BusinessContext.DeleteConfirm(item);


                BusinessContext.DeleteConfirm(main);
                BusinessContext.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }
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
                Guid value = jsonModel.m.Id(new BG_Assign().ModelName());
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
                            result = this.Retrieve(value);
                            result.d.Clear();
                            List<JsonGridModel> listPreparesAndApprovers = GetPreparesAndApprovers();
                            result.d.AddRange(listPreparesAndApprovers);
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
                    result.s = new JsonMessage("提示...", strMsg, JsonModelConstant.Info);
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示...", strMsg, JsonModelConstant.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示...", "系统错误！", JsonModelConstant.Error);
                return result;
            }
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
        public override bool UpdateDocState(Guid guid, EnumType.EnumDocState docState)
        {
            BG_Assign main = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                main.DocState = (int)docState;
                this.BusinessContext.SaveChanges();
                return true;
            }
            return false;
        }

        public List<object> GetWorkFlow()
        {
            List<object> list = new List<object>();
            List<FlowNodeModel> nodes = WorkFlowAPI.GetNodeByWorkFlow("ysfp");
            if (nodes != null)
            {
                foreach (FlowNodeModel item in nodes)
                {
                    string strJson = "{\"WorkFlowId\":\"" + item.WorkFlowId.ToString() + "\",\"WorkFlowNodeId\":\"" + item.WorkFlowNodeId.ToString() +
                        "\",\"State\":\"\",\"WorkFlowNodeName\":\"" + item.WorkFlowNodeName + "\",\"StateKey\":\"\"}";
                    object obj = JsonHelp.JsonToObject<object>(strJson);
                    list.Add(obj);
                }
            }
            return list;
        }
        // 搜索特定条件的数据  预算设置为内部项目支出  部门为默认的部门  年度为当前年度 项目是所有有权限的项目 查询所有未分配的数据
        public List<object> SearchHistoryEx()
        {
            List<object> list = new List<object>();
            DateTime time = DateTime.Now;
            int iYear = time.Year;

            Guid guid_Department = Guid.Empty;
            Guid guid_Dw = Guid.Empty;
            SS_Operator Operator = this.OperatorId == Guid.Empty ? null : this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == this.OperatorId);
            if (Operator != null)
            {
                Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
                if (person != null)
                {
                    guid_Department = person.GUID_Department;
                    guid_Dw = person.GUID_DW;
                }
            }
            if(guid_Department==Guid.Empty || guid_Dw==Guid.Empty)
            {
                return list;
            }

            BG_Setup objBGSetup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e=> e.BGSetupKey=="08");
            SS_DepartmentView Department = this.InfrastructureContext.SS_DepartmentView.FirstOrDefault(e => e.GUID == guid_Department);
            IntrastructureFun fun = new IntrastructureFun();
            List<Guid> projectList = fun.GetProjectGUID(true,this.OperatorId.ToString());
            List<BG_Assign> assignList = this.BusinessContext.BG_Assign.Where(e => e.GUID_Department == guid_Department &&
                   e.GUID_DW == guid_Dw && e.GUID_BGSetUp == objBGSetup.GUID && e.BGYear == iYear &&
                   projectList.Contains((Guid)e.GUID_Project)).ToList();

            foreach (Guid id in projectList)
            {
                BG_Assign objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == guid_Department &&
                e.GUID_DW == guid_Dw && e.GUID_BGSetUp == objBGSetup.GUID && e.BGYear == iYear &&
                id == (Guid)e.GUID_Project);

                SS_Project objProject = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.GUID == id);

                object obj = GetObject(Department, objProject, objAssign, iYear, 1);
                if (null != obj)
                {
                    list.Add(obj);
                }
            }

            return list;
        }
        public List<object> SearchHistory(SearchCondition conditions)
        {
            List<object> list = new List<object>();
            ysfpCondition conditionModel = (ysfpCondition)conditions;

            // 获得预算设置
            Guid Guid_BG_Setup = new Guid(conditionModel.GUID_BGSetUp);
            BG_SetupView objSetup = this.InfrastructureContext.BG_SetupView.FirstOrDefault(e => e.GUID == Guid_BG_Setup);
            int iSelectState = Int32.Parse(conditionModel.ysfpState);
            int iYear = Int32.Parse(conditionModel.Year);
            Guid Guid_Department = new Guid(conditionModel.Guid_Department);
            Guid Guid_Dw = new Guid(conditionModel.Guid_Dw);
            TreeNode node = conditionModel.TreeNodeList.FirstOrDefault(e=>e.treeModel=="SS_Project");
            List<Guid> proList = GetGuidList(node.treeValue);

            SS_DepartmentView Department = this.InfrastructureContext.SS_DepartmentView.FirstOrDefault(e=> e.GUID==Guid_Department);

            if (objSetup.BGTypeKey == "01")   // 基本支出
            {
                // 根据部门，年 单位 预算设置 查找预算分配
                BG_Assign objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == Guid_Department &&
                    e.GUID_DW == Guid_Dw && e.GUID_BGSetUp == Guid_BG_Setup && e.BGYear == iYear);

                object obj = GetObject(Department, null, objAssign,iYear, iSelectState);
                if(null!=obj)
                {
                    list.Add(obj);
                }
            }
            else
            { 
                // 根据部门，单位 ，部门，年 预算步骤查询预算分配
                List<BG_Assign> assignList = this.BusinessContext.BG_Assign.Where(e => e.GUID_Department == Guid_Department &&
                    e.GUID_DW == Guid_Dw && e.GUID_BGSetUp == Guid_BG_Setup && e.BGYear == iYear && 
                    proList.Contains((Guid)e.GUID_Project)).ToList();

                foreach (Guid id in proList)
                {
                    BG_Assign objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == Guid_Department &&
                    e.GUID_DW == Guid_Dw && e.GUID_BGSetUp == Guid_BG_Setup && e.BGYear == iYear &&
                    id==(Guid)e.GUID_Project);

                    SS_Project objProject = this.InfrastructureContext.SS_Project.FirstOrDefault(e=>e.GUID==id);

                    object obj = GetObject(Department, objProject, objAssign, iYear, iSelectState);
                    if(null!=obj)
                    {
                        list.Add(obj);
                    }
                }
            }
            return list;
        }

        private object GetObject(SS_DepartmentView objDepartment, SS_Project objProject, BG_Assign objAssign,int iYear,int iSelectState)
        {
            string strGuid = "";
            string strYsfpState = "未分配";
            string strFolwState = "未提交流程";
            string strPrincipal = "";
            if(null!=objAssign)
            {
                strGuid = objAssign.GUID.ToString();
                strYsfpState = "已分配";
            }

            if (null != objAssign)
            {
                string strErr = "";
                FlowNodeModel objFnm = WorkFlowAPI.GetCurNodeByDocId(objAssign.GUID,out strErr);
                if(null!=objFnm)
                {
                    strFolwState = objFnm.WorkFlowNodeName;
                }

                if(objAssign.DocState==999)
                {
                    strFolwState = "流程已结束";
                }
            }
            string strJson = "";
            object obj = null;
            if (null == objProject)     // 基本支出
            {
                strJson = "{\"GUID\":\"" + strGuid + "\",\"FunctionClass\":\"\",\"GUID_Department\":\"" + objDepartment.GUID.ToString() + "\",\"GUID_Dw\":\"" + objDepartment.GUID_DW.ToString() +
                    "\",\"GUID_Project\":\"\",\"ProjectKey\":\"\",\"ProjectName\":\"\",\"DWName\":\"" + objDepartment.DWName +
                    "\",\"DepartmentName\":\"" + objDepartment.DepartmentName + "\",\"Principal\":\"\",\"ysfpState\":\"" + strYsfpState + "\",\"FlowState\":\"" + strFolwState + "\"}";
            }
            else
            {
                // 查找所对应的预算编制 ，注意，要检查预算编制是不是在流程之中
                List<Guid> listByYear = this.BusinessContext.BG_Detail.Where(e=>e.BGYear==iYear).Select(e=> e.GUID_BG_Main).ToList().Distinct().ToList();
                BG_MainView objBGMain = this.BusinessContext.BG_MainView.FirstOrDefault(e => e.GUID_Department == objDepartment.GUID && e.GUID_DW == objDepartment.GUID_DW &&
                    listByYear.Contains(e.GUID) && (Guid)e.GUID_Project == objProject.GUID && e.Invalid==true);
                // 如果是通过流程生成的预算编制，那么它在流程中，如果是在非流程中人工手动添加的，那么其实是不在流程中的，而且这个预算编制也无法提交流程
                // 以上信息和李斌确认 2014-6-24
                if(null!=objBGMain)
                {
                    bool bExist = WorkFlowAPI.ExistProcess(objBGMain.GUID);
                    if(bExist)
                    {
                        strPrincipal = objBGMain.PersonName;
                    }
                }

                strJson = "{\"GUID\":\"" + strGuid + "\",\"FunctionClass\":\"" + objProject.GUID_FunctionClass + "\",\"GUID_Department\":\"" + objDepartment.GUID.ToString() + "\",\"GUID_Dw\":\"" + objDepartment.GUID_DW.ToString() +
                    "\",\"GUID_Project\":\"" + objProject.GUID.ToString() + "\",\"ProjectKey\":\"" + objProject.ProjectKey + "\",\"ProjectName\":\""
                    + objProject.ProjectName + "\",\"DWName\":\"" + objDepartment.DWName + "\",\"DepartmentName\":\"" + objDepartment.DepartmentName+ "\",\"Principal\":\"" + strPrincipal +
                    "\",\"ysfpState\":\"" + strYsfpState + "\",\"FlowState\":\"" + strFolwState + "\"}";
            }
            // 如果是0 表示查询全部，有预算分配就返回预算分配 没有预算分配就返回未分配的内容
            if(iSelectState==0)
            {
                obj = JsonHelp.JsonToObject<object>(strJson);
            }
            else if(iSelectState==1)  // 1 表示查询那些未分配的内容
            {
                if(objAssign==null)
                {
                    obj = JsonHelp.JsonToObject<object>(strJson);
                }
            }
            else  // 查询那些已经分配的内容
            {
                if (objAssign!=null)
                {
                    obj = JsonHelp.JsonToObject<object>(strJson);
                }
            }

            
            return obj;
        }


        public List<object> GetFlowNode()
        {
            List<FlowNodeModel> nodes = WorkFlowAPI.GetNodeByWorkFlow("ysfp");
            List<object> list = new List<object>();
            foreach (FlowNodeModel item in nodes)
            {
                string strJson = "{\"history-SS_RunTimeUsersSet-Id\":\"" + Guid.NewGuid().ToString() + "\",\"history-SS_RunTimeUsersSet-Sort\":\"" + item.Sort.ToString() +
                    "\",\"history-SS_RunTimeUsersSet-WorkFlowId\":\"" + item.WorkFlowNodeId.ToString() + "\",\"history-SS_RunTimeUsersSet-WorkFlowNodeId\":\"" + item.WorkFlowNodeId.ToString() +
                    "\",\"history-SS_RunTimeUsersSet-OperatorId\":\"\",\"history-SS_RunTimeUsersSet-OperatorName\":\"\",\"history-SS_RunTimeUsersSet-WorkFlowNodeName\":\"" + item.WorkFlowNodeName + "\"}";
                object obj = JsonHelp.JsonToObject<object>(strJson);
                list.Add(obj);
            }
            
            return list;
        }
        //历史
        /// <summary>
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            List<YSFPHistoryResult> list = new List<YSFPHistoryResult>();
            ysfpCondition conditionModel = (ysfpCondition)conditions;
            var objIF = new Infrastructure.IntrastructureFun();
            // 获得部门的权限            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            // 获得项目权限
            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            
            List<Guid> ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            // 先获取预算类型            Guid Guid_Type = new Guid(conditionModel.BGType);
            BG_Type type = this.InfrastructureContext.BG_Type.FirstOrDefault(e => e.GUID == Guid_Type);            
            // 获得预算步骤
            Guid Guid_BGStep = new Guid(conditionModel.BGStep);
            // 获得年            int iYear = Int32.Parse(conditionModel.Year);

            // 获得所选中的部门，如果部门都没有选，那么就默认的获得所有部门            TreeNode departmentNode = conditionModel.TreeNodeList.FirstOrDefault(e => e.treeModel == "SS_Department");
            
            TreeNode proNode = conditionModel.TreeNodeList.FirstOrDefault(e => e.treeModel == "SS_Project");
            TreeNode functionClassNode = conditionModel.TreeNodeList.FirstOrDefault(e => e.treeModel == "SS_FunctionClass");


            string AssignSql = string.Format("select a.GUID,a.DWName,a.DepartmentName,a.ProjectName,a.ProjectKey,a.BGSetupName,a.Maker,CONVERT(varchar(100), a.MakeDate, 20) as MakeDate,a.GUID_Department,a.GUID_Project," +
                //--预算分配状态
                "CASE WHEN b.ProcessId is null THEN '未分配' ELSE '已分配' END as yslbState," +
                //--预算初始值编制状态
                "case when b.ProcessId in (select ProcessId from OAO_WorkFlowProcessData where Url='yscszsz') " +
                "then '已编制' when b.ProcessId is not null then '未编制' else '' end as yscszbzState," +
                //--预算初始值审批状态
                "case when a.BGTypeKey!='02' then '' " +
                "when b.ProcessId in (select WorkFlowProcessId from OAO_WorkFlowProcessNode where State=0 and WorkFlowNodeId in (select Id from OAO_WorkFlowNode where Name='预算初始值')) then '未提交' " +
                "when b.ProcessId is not null then '已提交' else '' end as yscszspState," +
                //--预算编制状态
                "case when a.BGTypeKey!='02' then '' " +
                "when b.ProcessId in (select ProcessId from OAO_WorkFlowProcessData where Url='ysbz') then '已编制' " +
                "when b.ProcessId is not null then '未编制' else '' end as ysbzState," +
                //--预算编制审批状态
                "case when b.ProcessId in (select WorkFlowProcessId from OAO_WorkFlowProcessNode where State=0 and WorkFlowNodeId in (select Id from OAO_WorkFlowNode where Name not in ('部门预算审批','财务预算审批'))) then '未提交' " +
                "when b.ProcessId is not null then '已提交' else '' end as ysbzspState," +
                //--预算审批状态
                "case when b.ProcessId in (select WorkFlowProcessId from OAO_WorkFlowProcessNode where State=0 and WorkFlowNodeId in (select Id from OAO_WorkFlowNode where Name !='财务预算审批')) then '部门未提交' " +
                "when b.ProcessId in (select WorkFlowProcessId from OAO_WorkFlowProcessNode where State=0 and WorkFlowNodeId in (select Id from OAO_WorkFlowNode where Name='财务预算审批')) then '部门已提交' " +
                "when b.ProcessId in (select Id from OAO_WorkFlowProcess where State=1) then '审批完成' else '' end as ysspState " +

                "from BG_AssignView a left join OAO_WorkFlowProcessData b on a.GUID=b.DataId " +
                "where a.BGYear={0} and a.GUID_BGStep='{1}' and a.GUID_BGTYPE='{2}'  ", iYear, Guid_BGStep, Guid_Type);

            if (!string.IsNullOrEmpty(proNode.treeValue)) {
                AssignSql += string.Format(" and a.GUID_Project in ('{0}')", proNode.treeValue.Replace(",", "','"));
            }
            if (!string.IsNullOrEmpty(functionClassNode.treeValue))
            {
                AssignSql += string.Format(" and a.GUID_FunctionClass in ('{0}')", functionClassNode.treeValue.Replace(",", "','"));
            }

            if (!string.IsNullOrEmpty(departmentNode.treeValue))
            {
                AssignSql += string.Format(" and a.GUID_Department in ('{0}')", departmentNode.treeValue.Replace(",", "','"));
            }
            var YSFPHistoryResults = this.BusinessContext.ExecuteStoreQuery<YSFPHistoryResult>(AssignSql).ToList();


            //if (!string.IsNullOrEmpty(departmentNode.treeValue.Trim())) //如果有部门过滤条件
            //{
            //    List<Guid> depList = GetDepartmentCheck(departmentNode);
            //    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => depList.Contains(e.GUID_Department));
            //}
            //if (type.BGTypeKey == "02") // 如果是项目支出
            //{
            //    List<Guid> proList = GetProject(proNode, functionClassNode);
            //    if (proList.Count > 0) //如果有项目过滤条件
            //    {
            //        List<Guid> proListEx = proList.Intersect(ProjectAuth).ToList();// 获得交集 过滤掉那些通过功能分类获得没有权限的项目
            //        YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.GUID_Project != null && proListEx.Contains((Guid)e.GUID_Project));
            //    }

            //}
            //流程相关过滤条件
            
            //预算分配状态
            if (conditionModel.ysfpState != "0")
            {
                if (conditionModel.ysfpState == "1")   // 1表示查询所有未提交的
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysfpState == "未提交");
                }
                else // 查询所有已提交的
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysfpState == "已提交");
                }
            }

            //预算初始值编制状态
            //如果是项目支出才考虑次条件
            if (type.BGTypeKey == "02")
            {
                if (conditionModel.yscszbzState != "0")
                {

                    if (conditionModel.yscszbzState == "1")   // 未编制
                    {
                        YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.yscszbzState == "未编制");
                    }
                    else //已编制
                    {
                        YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.yscszbzState == "已编制");
                    }
                }
            }
            //预算初始值审批状态
            //如果是项目支出才考虑次条件
            if (type.BGTypeKey == "02")
            {
                if (conditionModel.yscszspState != "0")
                {
                    if (conditionModel.yscszspState == "1")   // 未提交
                    {
                        YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.yscszspState == "未提交");
                    }
                    else // 已提交
                    {
                        YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.yscszspState == "已提交");
                    }
                }
            }
            

            //预算编制状态
            if (conditionModel.ysbzState != "0")
            {
                if (conditionModel.ysbzState == "1")   // 未编制
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysbzState == "未编制");
                }
                else //已编制
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysbzState == "已编制");
                }
            }

            //预算编制审批状态
            if (conditionModel.ysbzspState != "0")
            {
                if (conditionModel.ysbzspState == "1")   // 未提交
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysbzspState == "未提交");
                }
                else //已提交
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysbzspState == "已提交");
                }
            }

            //预算审批状态
            if (conditionModel.ysspState != "0")
            {
                if (conditionModel.ysspState == "1") //部门未提交
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysspState == "部门未提交");
                }
                else if (conditionModel.ysspState == "2") //部门已提交
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysspState == "部门已提交");
                }
                else //审批完成
                {
                    YSFPHistoryResults = YSFPHistoryResults.FindAll(e => e.ysspState == "审批完成");
                }
            }

            List<object> rs = new List<object>();
            foreach (var item in YSFPHistoryResults) rs.Add(item);
            return rs;

        }

        //返回的项目没有考虑权限
        private List<Guid> GetProject(TreeNode proNode,TreeNode functionClassNode)
        {
            List<Guid> list = GetGuidList(proNode.treeValue);
            HashSet<Guid> proHash = new HashSet<Guid>();
            foreach(Guid item in list)
            {
                proHash.Add(item);                    
            }

            List<Guid> fcList = GetGuidList(functionClassNode.treeValue);
            foreach (Guid item in fcList)
            {
                list = GetProjectByFunctionClass(item);
                foreach (Guid id in list)
                {
                    proHash.Add(id);
                }
            }

            return proHash.ToList();
        }
        private List<Guid> GetProjectByFunctionClass(Guid fcId)
        {
            List<Guid> list = this.InfrastructureContext.SS_Project.Where(e => e.GUID_FunctionClass == fcId).Select(e => e.GUID).ToList();
            return list;
        }
        private List<Guid> GetDepartmentCheck(TreeNode node)
        {
            List<Guid> list = GetGuidList(node.treeValue);
            if(list.Count==0)
            {
                IntrastructureFun db = new IntrastructureFun();
                // 根据权限获得所有的有操作权限的部门
                List<SS_Department> depList= db.GetDepartment(true, this.OperatorId.ToString()).ToList();
                list = depList.Select(e => e.GUID).ToList();
            }
            return list;
        }
        private List<Guid> GetGuidList(string strGuidLink)
        {
            List<Guid> list = new List<Guid>();
            string[] array = strGuidLink.Split(',');
            foreach(string item in array)
            {
                if(item!="")
                {
                    list.Add(new Guid(item));
                }                
            }

            return list;
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        //public List<object> BG_History(SearchCondition conditions)
        //{
        //    JsonModel jsonmodel = new JsonModel();
        //    HistoryCondition historyconditions = (HistoryCondition)conditions;
        //    IQueryable<BG_AssignView> main = this.BusinessContext.BG_AssignView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单
          
        //    List<SS_Department> depList = new List<SS_Department>();
        //    List<SS_DW> dwList = new List<SS_DW>();
        //    List<SS_Project> projectList = new List<SS_Project>();
        //    List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
        //    List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
        //    if (this.OperatorId.IsNullOrEmpty())
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        main = main.Where(e => e.GUID_Modifier == this.OperatorId);
        //    }
        //    if (historyconditions != null)
        //    {

        //        if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
        //        {
        //            int y;
        //            if (int.TryParse(historyconditions.Year, out y))
        //            {

        //                main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Year == y);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
        //        {
        //            int m;
        //            if (int.TryParse(historyconditions.Month, out m))
        //            {
        //                main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Month == m);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.DocNum))
        //        {
        //            main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
        //        }
        //        #region 审批状态条件

        //        if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
        //        {
        //            if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
        //            {
        //                main = main.Where(e => e.DocState == null || e.DocState == 0);
        //            }
        //            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
        //            {
        //                main = main.Where(e => e.DocState == 999 || e.DocState == -1);
        //            }
        //            else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//审批中
        //            {
        //                main = main.Where(e => e.DocState != 999 && e.DocState != -1 && e.DocState != null && e.DocState != 0);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
        //        {
        //            if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
        //            {
        //                //main.GUID in (select GUID_Doc from cn_checkdrawmain)
        //                var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
        //                main = main.Where(e => !guidList.Contains(e.GUID));
        //            }
        //            else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
        //            {
        //                var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
        //                main = main.Where(e => guidList.Contains(e.GUID));
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
        //        {
        //            if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
        //            {
        //                var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
        //                main = main.Where(e => !guidList.Contains(e.GUID));
        //            }
        //            else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
        //            {
        //                var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
        //                main = main.Where(e => guidList.Contains(e.GUID));
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
        //        {
        //            if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
        //            {
        //                var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                main = main.Where(e => !guidList.Contains(e.GUID));
        //            }
        //            else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
        //            {
        //                var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                main = main.Where(e => guidList.Contains(e.GUID));
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
        //        {
        //            if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
        //            {
        //                //在凭证主表中 存在核销明细表中的主表信息

        //                var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
        //                main = main.Where(e => !guidList.Contains(e.GUID));
        //            }
        //            else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//未生成凭证
        //            {
        //                var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
        //                var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
        //                main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
        //        {
        //            if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
        //            {
        //                //作废 DocState 为9
        //                main = main.Where(e => e.DocState != 9);

        //            }
        //            else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
        //            {
        //                //作废 DocState 为9
        //                main = main.Where(e => e.DocState == 9);

        //            }
        //        }
        //        #endregion

        //        if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
        //        {
        //            switch (historyconditions.treeModel.ToLower())
        //            {
        //                case "ss_department":
        //                    SS_Department dep = new SS_Department();
        //                    dep.GUID = historyconditions.treeValue;
        //                    dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
        //                    var depguid = depList.Select(e => e.GUID);
        //                    main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
        //                    //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
        //                    break;
        //                case "ss_dw":
        //                    SS_DW dw = new SS_DW();
        //                    dw.GUID = historyconditions.treeValue;
        //                    dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
        //                    var dwguid = dwList.Select(e => e.GUID);
        //                    main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
        //                    break;
        //                case "ss_project":
        //                    SS_Project projectModel = new SS_Project();
        //                    projectModel.GUID = historyconditions.treeValue;
        //                    projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
        //                    var projectGUID = projectList.Select(e => e.GUID);
        //                    main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
        //                    break;
        //                case "ss_projectclass":
        //                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
        //                    projectclassModel.GUID = historyconditions.treeValue;
        //                    projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
        //                    var projectUID = projectList.Select(e => e.GUID);
        //                    main = main.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
        //                    break;
        //                case "ss_bgcode":
        //                    SS_BGCode bgcodeModel = new SS_BGCode();
        //                    bgcodeModel.GUID = historyconditions.treeValue;
        //                    bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
        //                    var bgcodeGUID = bgcodeList.Select(e => e.GUID);
        //                    detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
        //                    break;
        //                case "ss_person":
        //                    main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
        //                    break;
        //            }
        //        }


        //    }
        //    //明细信息
        //    var dbdetai = from a in detail
        //                  group a by a.GUID_BG_Main into g
        //                  select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_BG) };
        //    var o = (from d in dbdetai
        //             join m in main on d.GUID_BX_Main equals m.GUID //into temp
        //             //join s in this.InfrastructureContext
        //             where d.GUID_BX_Main != null && m.GUID != null
        //             select new { m.GUID, m.DocNum, m.BGSetupName, m.ProjectName, m.ProjectKey, m.BGStepName, m.BGTypeName, m.DepartmentName, m.Maker, m.DocDate, m.PersonName, d.Total_BX, m.MakeDate });

        //    var mainList = o.AsEnumerable().Select(e => new
        //    {
        //        e.GUID,
        //        DocNum = e.DocNum == null ? "" : e.DocNum,
        //        DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
        //        BGSetupName = e.BGSetupName,
        //        e.ProjectName,
        //        e.ProjectKey,
        //        e.BGStepName,
        //        e.BGTypeName,
        //        e.DepartmentName,
        //        e.Maker,
        //        e.MakeDate
        //    }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

        //    return mainList;

        //}
        /// <summary>
        /// 参照
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<object> CZ_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单
            IQueryable<BG_DetailView> detail = this.BusinessContext.BG_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                main = main.Where(e => e.GUID_Modifier == this.OperatorId);
            }
            if (historyconditions != null)
            {

                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件

                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                    {
                        main = main.Where(e => e.DocState == null || e.DocState == 0);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                    {
                        main = main.Where(e => e.DocState == 999 || e.DocState == -1);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//审批中
                    {
                        main = main.Where(e => e.DocState != 999 && e.DocState != -1 && e.DocState != null && e.DocState != 0);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
                    {
                        //在凭证主表中 存在核销明细表中的主表信息

                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//未生成凭证
                    {
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != 9);

                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == 9);

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_BG_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_BG) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     //join s in this.InfrastructureContext
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.BGSetupName, m.ProjectName, m.ProjectKey, m.BGStepName, m.BGTypeName, m.DepartmentName, m.Maker, m.DocDate, m.PersonName, d.Total_BX, m.MakeDate });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                BGSetupName = e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.Maker,
                e.MakeDate
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;

        }

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
            List<ValidationResult> vrList = new List<ValidationResult>();
            BG_Assign main = null; ; //new BX_Main();
            List<SS_RunTimeUsersSet> listRtus = new List<SS_RunTimeUsersSet>();
            switch (status)
            {
                case "1": //新建
                    //main = LoadMain(jsonModel,ref listRtus);//.Fill(jsonModel.m);
                    //vResult = InsertVerify(main);//
                    //vrList = VerifyResultRtus(ref listRtus);
                    //vResult._validation.AddRange(vrList);
                    vResult = InsertVerifyEx(jsonModel);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    //main = LoadMain(jsonModel, ref listRtus);
                    //vResult = ModifyVerify(main);//修改验证
                    //vrList = VerifyResultRtus(ref listRtus);
                    //vResult._validation.AddRange(vrList);
                    vResult = ModifyVerifyEx(jsonModel);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new BG_Assign().ModelName());
                    vResult = DeleteVerify(value);
                    strMsg = DataVerifyMessage(vResult);
                    break;

            }
            return strMsg;
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
        private BG_Assign LoadMain(JsonModel jsonModel,ref List<SS_RunTimeUsersSet> RtusList)
        {
            if (jsonModel.m == null) return null;
            BG_Assign main = new BG_Assign();
            main.Fill(jsonModel.m);

            // 获得
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                string detailModelName = new SS_RunTimeUsersSet().ModelName();
                JsonGridModel pGrid = jsonModel.d.Find(detailModelName);
                if (pGrid != null)
                {
                    foreach (List<JsonAttributeModel> row in pGrid.r)
                    {
                        SS_RunTimeUsersSet objRtus = new SS_RunTimeUsersSet();
                        objRtus.Fill(row);
                        RtusList.Add(objRtus);
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
        private List<ValidationResult> VerifyResultDetail(BG_Assign data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<BG_Preparers> detailList = new List<BG_Preparers>();
            foreach (BG_Preparers item in data.BG_Preparers)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BG_Preparers item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_BGPreparers_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                resultList.Add(new ValidationResult("", "请添加编制人信息！"));

            }
            //审批人
            List<BG_Approver> adetailList = new List<BG_Approver>();
            foreach (BG_Approver item in data.BG_Approver)
            {
                adetailList.Add(item);
            }
            if (adetailList != null && adetailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BG_Approver item in adetailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_BGApprover_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                resultList.Add(new ValidationResult("", "请添加审批人信息！"));

            }
            return resultList;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(BG_Assign data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BG_Assign mModel = data;
            object g;

            #region   主表字段验证
            
            //制单人

            if (mModel.GUID_Maker.IsNullOrEmpty())
            {
                str = "制单人 不能为空!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Maker.GetType(), mModel.GUID_Maker.ToString(), out g) == false)
                {
                    str = "制单人格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //最后修改人
            if (mModel.GUID_Modifier.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.GUID_Modifier.GetType(), mModel.GUID_Modifier.ToString(), out g) == false)
            {
                str = "最后修改人格式不正确！";
                resultList.Add(new ValidationResult("", str));

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
            //最后修改日期



            if (mModel.ModifyDate.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.ModifyDate.GetType(), mModel.ModifyDate.ToString(), out g) == false)
            {
                str = "最后修改日期格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //提交日期
            if (mModel.SubmitDate.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.SubmitDate.GetType(), mModel.SubmitDate.ToString(), out g) == false)
            {
                str = "提交日期格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }

            
            //预算部门
            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "预算部门 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "预算部门格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //单位GUID
            if (mModel.GUID_DW.IsNullOrEmpty())
            {
                str = "单位 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_DW.GetType(), mModel.GUID_DW.ToString(), out g) == false)
                {
                    str = "单位格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            //预算类型和项目对应关系
            var bgtype = this.InfrastructureContext.BG_Type.FirstOrDefault(e => e.GUID == mModel.GUID_BGTYPE);
            if (bgtype!=null)
            {
                if (bgtype.BGTypeKey == "02") //项目支出
                {
                    if (mModel.GUID_Project.IsNullOrEmpty())
                    {
                        str = "当前预算设置下预算类型为项目预算,项目不能为空!";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
                else //基本支出
                {
                    mModel.GUID_Project = null;
                }
            }


            // 功能分类 和项目要相对应
            // 检查功能分类和项目之间的对应关系
            SS_ProjectView viewProject = this.InfrastructureContext.SS_ProjectView.FirstOrDefault(e => e.GUID == mModel.GUID_Project);
            if (null == viewProject)
            {
                if (!mModel.GUID_FunctionClass.IsNullOrEmpty())
                {
                    // 没有选择项目，那么就不要选择功能分类
                    str = "没有选择项目，功能分类也不能选择!";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            else
            {
                // 功能分类和项目不对应
                if (viewProject.GUID_FunctionClass != mModel.GUID_FunctionClass)
                {
                    str = "项目与功能分类不对应，功能分类应选择: " + viewProject.FunctionClassName;
                    resultList.Add(new ValidationResult("", str));
                }
            }

            if (mModel.BeginDate>mModel.StopDate)
            {
                str = "开始时间不能小于完成时间!";
                resultList.Add(new ValidationResult("", str));
            }

            return resultList;

            #endregion
        }

        private List<ValidationResult> VerifyResultRtus(ref List<SS_RunTimeUsersSet> listRtus)
        {
            List<ValidationResult> resultList = new List<ValidationResult>();
            string str = "";
            foreach (SS_RunTimeUsersSet item in listRtus)
            {
                if(item.WorkFlowId==null)
                {
                    str = "系统错误," + item.WorkFlowNodeName + "所对应的WorkFlowId为空";
                    resultList.Add(new ValidationResult("", str));
                }
                if(item.WorkFlowNodeId==null)
                {
                    str = "系统错误," + item.WorkFlowNodeName + "所对应的WorkFlowNodeId为空";
                    resultList.Add(new ValidationResult("", str));
                }
                if(item.OperatorId==null)
                {
                    str = item.WorkFlowNodeName + "环节没有分配操作人员!";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            return resultList;
        }
        /// <summary>
        /// 编制人验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_BGPreparers_Detail(BG_Preparers data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BG_Preparers item = data;
            /// <summary>
            /// 明细表字段验证
            /// </summary>
            #region 明细表字段验证
            //编制人
            if (item.GUID_Operator.IsNullOrEmpty())
            {
                str = "明细编制人GUID 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Operator.GetType(), item.GUID_Operator.ToString(), out g) == false)
                {
                    str = "明细编制人GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //变量
            if (item.Variable.ToString() == "")
            {
                str = "明细变量值 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            
            #endregion


            return resultList;
        }

        /// <summary>
        /// 审批验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_BGApprover_Detail(BG_Approver data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BG_Approver item = data;
            #region 明细表字段验证
            //编制人
            if (item.GUID_Operator.IsNullOrEmpty())
            {
                str = "明细审批人名称GUID 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Operator.GetType(), item.GUID_Operator.ToString(), out g) == false)
                {
                    str = "明细审批人名称GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //变量
            if (item.Variable.ToString() == "")
            {
                str = "明细变量值 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }

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
            BG_Assign model = (BG_Assign)data;

            // 如果预算分配已经存在，那么不可以再新增
            BG_Assign objAssign = null;
            if (model.GUID_Project != null)
            {
                objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == model.GUID_Department &&
                    e.GUID_DW == model.GUID_DW && e.GUID_BGSetUp == model.GUID_BGSetUp && e.GUID_Project == model.GUID_Project && e.BGYear==model.BGYear);//添加时间条件
            }
            else
            {
                objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == model.GUID_Department &&
                    e.GUID_DW == model.GUID_DW && e.GUID_BGSetUp == model.GUID_BGSetUp && e.GUID_Project == null && e.BGYear == model.BGYear);//添加时间条件
            }

            if (objAssign != null)
            {
                string str = "预算分配已经存在，不能新增!";
                result._validation.Add(new ValidationResult("", str));
            }

            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;

        }

        private VerifyResult InsertVerifyEx(JsonModel jsonModel)
        {
            VerifyResult result = new VerifyResult();
            //主单验证
            BG_Assign model = new BG_Assign();
            model.Fill(jsonModel.m);
            // 如果预算分配已经存在，那么不可以再新增

            BG_Assign objAssign = null;
            if (model.GUID_Project != null)
            {
                objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == model.GUID_Department &&
                    e.GUID_DW == model.GUID_DW && e.GUID_BGSetUp == model.GUID_BGSetUp && e.GUID_Project == model.GUID_Project && e.BGYear == model.BGYear);//添加时间条件
            }
            else
            {
                objAssign = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == model.GUID_Department &&
                    e.GUID_DW == model.GUID_DW && e.GUID_BGSetUp == model.GUID_BGSetUp && e.GUID_Project == null && e.BGYear == model.BGYear);//添加时间条件
            }

            if (objAssign != null)
            {
                string str = "改类型预算分配已经存在，不能保存!";
                result._validation.Add(new ValidationResult("", str));
            }

            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }

            //验证明细
            // 获得
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                //编制人
                JsonGridModel pGrid = jsonModel.d.Find("BG_Preparers");
                if (pGrid != null)
                {
                    foreach (List<JsonAttributeModel> row in pGrid.r)
                    {
                        BG_Preparers item = new BG_Preparers();
                        item.Fill(row);
                        if (item.GUID_Operator.IsNullOrEmpty())
                        {
                            string str = "没有设置" + item.Variable + "的编制人，不能保存！";
                            result._validation.Add(new ValidationResult("", str));
                        }
                    }
                }
                //审批人
                pGrid = jsonModel.d.Find("BG_Approver");
                if (pGrid != null)
                {
                    foreach (List<JsonAttributeModel> row in pGrid.r)
                    {
                        BG_Approver item = new BG_Approver();
                        item.Fill(row);
                        if (item.GUID_Operator.IsNullOrEmpty())
                        {
                            string str = "没有设置" + item.Variable + "的审批人，不能保存！";
                            result._validation.Add(new ValidationResult("", str));
                        }
                    }
                }
                
            }
            else
            {
                string str = "没有对应的编制人和审批人设置，不能保存！";
                result._validation.Add(new ValidationResult("", str));
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
            BG_Assign bg_Assign = new BG_Assign();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID
            if (bg_Assign.GUID == null || bg_Assign.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "此单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
            //流程验证
            
            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此单正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
            }
            //作废的不能删除
            BG_Assign main = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == int.Parse("9"))
                {
                    str = "此单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                }
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
            BG_Assign model = (BG_Assign)data;
            BG_Assign orgModel = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == model.GUID);
            if (orgModel != null)
            {
                if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
                    result._validation = resultList;
                    return result;
                }
            }
            //流程验证
            
            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //作废           
            if (orgModel != null && orgModel.DocState == int.Parse("9") && model.DocState != (int)Business.Common.EnumType.EnumDocState.RcoverState)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;
        }

        private VerifyResult ModifyVerifyEx(JsonModel jsonModel)
        {
            VerifyResult result = new VerifyResult();
            //主单验证
            BG_Assign model = new BG_Assign();
            model.Fill(jsonModel.m);
            BG_Assign orgModel = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID == model.GUID);
            if (orgModel != null)
            {
                if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
                    result._validation = resultList;
                    return result;
                }
            }
            //流程验证

            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //作废           
            if (orgModel != null && orgModel.DocState == int.Parse("9") && model.DocState != (int)Business.Common.EnumType.EnumDocState.RcoverState)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }


            //验证明细
            // 获得
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                //编制人
                JsonGridModel pGrid = jsonModel.d.Find("BG_Preparers");
                if (pGrid != null)
                {
                    foreach (List<JsonAttributeModel> row in pGrid.r)
                    {
                        BG_Preparers item = new BG_Preparers();
                        item.Fill(row);
                        if (item.GUID_Operator.IsNullOrEmpty())
                        {
                            string str = "没有设置" + item.Variable + "的编制人，不能保存！";
                            result._validation.Add(new ValidationResult("", str));
                        }
                    }
                }
                //审批人
                pGrid = jsonModel.d.Find("BG_Approver");
                if (pGrid != null)
                {
                    foreach (List<JsonAttributeModel> row in pGrid.r)
                    {
                        BG_Approver item = new BG_Approver();
                        item.Fill(row);
                        if (item.GUID_Operator.IsNullOrEmpty())
                        {
                            string str = "没有设置" + item.Variable + "的审批人，不能保存！";
                            result._validation.Add(new ValidationResult("", str));
                        }
                    }
                }

            }
            else
            {
                string str = "没有对应的编制人和审批人设置，不能保存！";
                result._validation.Add(new ValidationResult("", str));
            }

            return result;
        }
        #endregion

    }


    /// <summary>
    /// 预算分配历史查询结果模型类
    /// </summary>
    public class YSFPHistoryResult
    {
        public Guid GUID { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string DWName { get; set; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 项目编码
        /// </summary>
        public string ProjectKey { get; set; }
        /// <summary>
        /// 预算设置
        /// </summary>
        public string BGSetupName { get; set; }
        /// <summary>
        /// 制单人
        /// </summary>
        public string Maker { get; set; }
        /// <summary>
        /// 制单日期
        /// </summary>
        public string MakeDate { get; set; }
        /// <summary>
        /// 预算分配状态
        /// </summary>
        public string ysfpState { get; set; }
        /// <summary>
        /// 预算初始值编制状态
        /// </summary>
        public string yscszbzState { get; set; }
        /// <summary>
        /// 预算初始值审批状态
        /// </summary>
        public string yscszspState { get; set; }
        /// <summary>
        /// 预算编制状态
        /// </summary>
        public string ysbzState { get; set; }
        /// <summary>
        /// 预算编制审批状态
        /// </summary>
        public string ysbzspState { get; set; }
        /// <summary>
        /// 预算审批状态
        /// </summary>
        public string ysspState { get; set; }

        public Guid GUID_Department { get; set; }

        public Guid? GUID_Project { get; set; }
    }
}
