using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Platform.Flow.Run;
using Business.CommonModule;
using BusinessModel;
namespace Business.Casher
{    
    public class 现金存储 : BaseDocument
    {

        public 现金存储() : base() { }
        public 现金存储(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值

        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                CN_CashMainView model = new CN_CashMainView();
                model.FillDefault(this, this.OperatorId, this.ModelUrl);
                jmodel.m = model.Pick();

                CN_CashDetailView dModel = new CN_CashDetailView();
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.f.Add(fjgm);
                List<JsonAttributeModel> picker = new List<JsonAttributeModel>();
                CN_PaymentNumberView payment = new CN_PaymentNumberView();
                payment.FillCN_PaymentNumberDefault(this);
                picker.AddRange(payment.Pick());

                fjgm.r.Add(picker);
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
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
                CN_CashMainView main = this.BusinessContext.CN_CashMainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();


                    IQueryable<CN_CashDetailView> q = this.BusinessContext.CN_CashDetailView.Where(e => e.GUID_CN_CashMain == guid).OrderBy(e => e.OrderNum);
                    List<CN_CashDetailView> details = q == null ? new List<CN_CashDetailView>() : q.ToList();
                    if (details.Count > 0)
                    {
                        JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                        jmodel.d.Add(jgm);
                        foreach (CN_CashDetailView detail in details)
                        {
                            List<JsonAttributeModel> picker = detail.Pick();
                            CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e => e.GUID == detail.GUID_PaymentNumber);
                            if (payment != null)
                            {
                                picker.AddRange(payment.Pick());
                            }
                            //添加支票信息
                            var checkAttribute = GetCheckAttribute(main, detail);
                            if (checkAttribute != null && checkAttribute.Count > 0)
                            {
                                picker.AddRange(checkAttribute);
                            }
                            jgm.r.Add(picker);
                        }

                    }
                    //明细中f 填充默认值

                    CN_CashDetailView dModel = new CN_CashDetailView();
                    dModel.FillDetailDefault<CN_CashDetailView>(this, this.OperatorId);
                    JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                    jmodel.f.Add(fjgm);

                    List<JsonAttributeModel> fpicker = dModel.Pick();

                    CN_PaymentNumberView fpayment = new CN_PaymentNumberView();
                    fpayment.FillCN_PaymentNumberDefault(this);
                    fpicker.AddRange(fpayment.Pick());

                    fjgm.r.Add(fpicker);
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
        /// 获取支票属性
        /// </summary>
        /// <param name="mainView">现金主Model信息</param>
        /// <param name="detailView">明显信息Model信息</param>
        /// <returns></returns>
        private List<JsonAttributeModel> GetCheckAttribute(CN_CashMainView mainView, CN_CashDetailView detailView)
        {
            List<JsonAttributeModel> list = new List<JsonAttributeModel>();
            var mainModel = GetCheckDrawMainView(mainView, detailView);
            if (mainModel != null)
            {
                var checkModel = this.InfrastructureContext.CN_CheckView.FirstOrDefault(e => e.GUID == mainModel.GUID_Check);
                
                if (checkModel != null)
                {
                    list = checkModel.Pick();
                }
            }

            return list;
        }
        /// <summary>
        /// 获取支票领取主Model信息
        /// </summary>
        /// <param name="mainView"></param>
        /// <param name="detailView"></param>
        /// <returns></returns>
        private CN_CheckDrawMainView GetCheckDrawMainView(CN_CashMainView mainView, CN_CashDetailView detailView)
        {
            var detailModel = this.BusinessContext.CN_CheckDrawDetailView.FirstOrDefault(e => e.GUID_DocMain == mainView.GUID && e.GUID_DocDetail == detailView.GUID);
            if (detailModel != null)
            {
                var mainModel = this.BusinessContext.CN_CheckDrawMainView.FirstOrDefault(e => e.GUID == detailModel.GUID_CheckDrawMain);
                if (mainModel != null)
                {
                    return mainModel;
                }
            }
            return null;
        }
        /// <summary>
        /// 明细信息（主要 是否入账）
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private CN_CashDetail GetCNCashDetailModel(JsonModel jsonModel)
        {
            CN_CashDetail detail = new CN_CashDetail();
            JsonAttributeModel model = null;
            for (int i = 0; i < jsonModel.m.Count; i++)
            {
                model = jsonModel.m[i];
                if (model.m.ToLower() == "CN_CashDetail".ToLower() && model.n.ToLower() == "IsRZ".ToLower())
                {
                    if (model.v.ToLower() == "true")
                    {
                        detail.IsRZ = 1;
                    }
                    else
                    {
                        detail.IsRZ = 0;
                    }
                    break;
                }
            }
            return detail;
        }
        /// <summary>
        /// 获取支票领取主Model信息
        /// </summary>
        /// <param name="mainView"></param>
        /// <param name="detailView"></param>
        /// <returns></returns>
        private CN_CheckDrawMain GetCheckDrawMain(CN_CashMain main, CN_CashDetail detail)
        {
            var detailModel = this.BusinessContext.CN_CheckDrawDetail.FirstOrDefault(e => e.GUID_DocMain == main.GUID && e.GUID_DocDetail == detail.GUID);
            if (detailModel != null)
            {
                var mainModel = this.BusinessContext.CN_CheckDrawMain.FirstOrDefault(e => e.GUID == detailModel.GUID_CheckDrawMain);
                if (mainModel != null)
                {
                    return mainModel;
                }
            }
            return null;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {

            CN_CashMain main = this.BusinessContext.CN_CashMain.Include("CN_CashDetail").FirstOrDefault(e => e.GUID == guid);


            List<CN_CashDetail> details = new List<CN_CashDetail>();

            foreach (CN_CashDetail item in main.CN_CashDetail)
            {
                details.Add(item);
            }

            foreach (CN_CashDetail item in details) { BusinessContext.DeleteConfirm(item); }

            DeleteCheckDraw(main.GUID);

            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
        }
        /// <summary>
        /// 删除支票信息
        /// </summary>
        /// <param name="guid"></param>
        private void DeleteCheckDraw(Guid cashMianGuid)
        {
            List<CN_CheckDrawMain> mainList = this.BusinessContext.CN_CheckDrawMain.Include("CN_CheckDrawDetail").Where(e => e.GUID_Doc == cashMianGuid).ToList();
            List<CN_CheckDrawDetail> detailList = new List<CN_CheckDrawDetail>();
            for (int i = 0; i < mainList.Count; i++)
            {
                foreach (CN_CheckDrawDetail item in mainList[i].CN_CheckDrawDetail)
                {
                    detailList.Add(item);
                }
            }
            foreach (CN_CheckDrawDetail item in detailList)
            {
                this.BusinessContext.DeleteConfirm(item);
            }
            foreach (CN_CheckDrawMain item in mainList)
            {
                this.BusinessContext.DeleteConfirm(item);
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
            CN_CashMain main = new CN_CashMain();
            CN_CashDetail tempdetail = new CN_CashDetail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.CN_CashMain.Include("CN_CashDetail").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate;
            }
            main.FillDefault(this, this.OperatorId, this.ModelUrl);
            main.Fill(jsonModel.m);
             main.ResetDefault(this,this.OperatorId);
            //明细信息
            tempdetail = GetCNCashDetailModel(jsonModel);//添加是否记账

            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            //if (IsDateChange(orgDateTime, main.DocDate))
            //{
            //    main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, main.GUID_YWType, main.DocDate.ToString());
            //}

            string detailModelName = tempdetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (CN_CashDetail detail in main.CN_CashDetail)
                {
                    this.BusinessContext.DeleteConfirm(detail);
                    DeleteCheckDraw(main.GUID, detail.GUID);
                }
            }
            else
            {
                List<CN_CashDetail> detailList = new List<CN_CashDetail>();
                foreach (CN_CashDetail detail in main.CN_CashDetail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (CN_CashDetail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null)
                    {
                        this.BusinessContext.DeleteConfirm(detail);
                        DeleteCheckDraw(main.GUID, detail.GUID);
                    }
                    else
                    {
                        orderNum++;
                        detail.OrderNum = Grid.r.IndexOf(row);
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);

                        if (tempdetail != null)
                        {
                            detail.IsRZ = tempdetail.IsRZ;
                            if (detail.IsRZ != null && detail.IsRZ == 1) detail.RZDate = main.DocDate;
                            else detail.RZDate = null;
                        }

                        detail.CN_PaymentNumber.Fill(row);
                        detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
                        detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;

                        //修改支票领用主表
                        ModifyCheckDraw(main, detail, row);
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                var settlytypeguid = this.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "02").GUID;
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    orderNum++;
                    AddDetail(main, row, Grid.r.IndexOf(row), tempdetail, settlytypeguid.ToString());

                }
            }
            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 修改支票领用信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="guid_2"></param>
        private void ModifyCheckDraw(CN_CashMain cashMian, CN_CashDetail cashDeatil, List<JsonAttributeModel> row)
        {
            List<JsonAttributeModel> list = new List<JsonAttributeModel>();
            var mainModel = GetCheckDrawMain(cashMian, cashDeatil);
            if (mainModel != null)
            {
                mainModel.FillCommField(row, (new CN_CashDetail()).ModelName());
                CN_PaymentNumber payment = this.BusinessContext.CN_PaymentNumber.FirstOrDefault(e => e.GUID == cashDeatil.GUID_PaymentNumber);
                if (payment != null)
                {
                    mainModel.PaymentNumber = payment.PaymentNumber;
                }
                if (!cashMian.GUID_UIType.IsNullOrEmpty())
                {
                    mainModel.GUID_UIType = cashMian.GUID_UIType;
                }
                //支票管理
                CN_CheckView checkModel = new CN_CheckView();
                checkModel.Fill(row);
                if (checkModel != null)
                {
                    mainModel.GUID_Check = checkModel.GUID;
                }
            }
            if (mainModel != null)
            {
                this.BusinessContext.ModifyConfirm(mainModel);
            }
        }
        /// <summary>
        /// 删除提现信息
        /// </summary>
        /// <param name="guidMian">主Model的GUID</param>
        /// <param name="guidDetail">明细的GUID</param>
        private void DeleteCheckDraw(Guid guidMian, Guid guidDetail)
        {
            if (guidDetail.IsNullOrEmpty() || guidMian.IsNullOrEmpty()) return;
            CN_CheckDrawDetail detail = this.BusinessContext.CN_CheckDrawDetail.FirstOrDefault(e => e.GUID_DocDetail == guidDetail && e.GUID_DocMain == guidMian);
            if (detail != null)
            {
                BusinessContext.DeleteConfirm(detail);
            }
            CN_CheckDrawMain main = this.BusinessContext.CN_CheckDrawMain.FirstOrDefault(e => e.GUID == detail.GUID_CheckDrawMain);
            if (main != null)
            {
                this.BusinessContext.DeleteConfirm(main);
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
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            CN_CashMain main = new CN_CashMain();
            main.FillDefault(this, this.OperatorId, this.ModelUrl);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");
            main.DocState = "0";
            var settlytypeguid = this.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "02").GUID;

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CN_CashDetail temp = new CN_CashDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        orderNum++;
                        AddDetail(main, row, Grid.r.IndexOf(row), temp, settlytypeguid.ToString());
                    }
                }
            }
            this.BusinessContext.CN_CashMain.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
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
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                Guid value = jsonModel.m.Id(new CN_CashMain().ModelName());
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "现金存储", data);
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
                OperatorLog.WriteLog(this.OperatorId, "现金存储", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
        /// <summary>
        /// 更改单据状态

        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="docState">单据状态</param>
        /// <returns>Bool</returns>
        public override bool UpdateDocState(Guid guid, EnumType.EnumDocState docState)
        {
            CN_CashMain main = this.BusinessContext.CN_CashMain.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                main.DocState = ((int)docState).ToString();
                main.SubmitDate = DateTime.Now;
                this.BusinessContext.SaveChanges();
                return true;
            }
            return false;
        }
       
        /// <summary>
        /// 添加明显信息
        /// </summary>
        /// <param name="main"></param>
        private void AddDetail(CN_CashMain main, List<JsonAttributeModel> row, int orderNum, CN_CashDetail tempDetail, string settletypeguid = "")
        {

            orderNum++;
            CN_CashDetail temp = new CN_CashDetail();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            temp.OrderNum = orderNum;
            temp.GUID = Guid.NewGuid();
            temp.IsDC = true;
            if (!string.IsNullOrEmpty(settletypeguid)) temp.GUID_SettleType = Guid.Parse(settletypeguid);
            if (tempDetail != null)
            {
                temp.IsRZ = tempDetail.IsRZ;
                if (temp.IsRZ != null && temp.IsRZ == 1) temp.RZDate = main.DocDate;
                else temp.RZDate = null;
            }
            temp.CN_PaymentNumber = new CN_PaymentNumber();
            temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            temp.CN_PaymentNumber.Fill(row);
            temp.CN_PaymentNumber.GUID_Project = temp.GUID_Project;
            
            temp.GUID_Person = main.GUID_Person;
            temp.GUID_CN_CashMain = main.GUID;
            temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
            main.CN_CashDetail.Add(temp);
            //添加支票信息
            AddCN_CheckDrawMain(main, row, temp);

        }
        /// <summary>
        /// 添加支票领用主表
        /// </summary>
        private void AddCN_CheckDrawMain(CN_CashMain main, List<JsonAttributeModel> row, CN_CashDetail detail)
        {
            int cashMainClassId = Infrastructure.CommonFuntion.GetClassId(typeof(CN_CashMain).Name);
            CN_CheckDrawMain cashMain = new CN_CheckDrawMain();
            cashMain.FillCommField(row, (new CN_CashDetail()).ModelName());
            cashMain.Fill(row);
            cashMain.GUID = Guid.NewGuid();
            cashMain.CheckDrawDatetime = DateTime.Now;
            cashMain.CheckMoney = detail.Total_Cash;
            cashMain.GUID_Doc = main.GUID;
            cashMain.ClassID = cashMainClassId;
            cashMain.GUID_UIType = main.GUID_UIType;
            //支票管理
            CN_Check checkModel = new CN_Check();
            checkModel.Fill(row);
            if (checkModel != null)
            {
                cashMain.GUID_Check = checkModel.GUID;
            }

            CN_PaymentNumber payment = new CN_PaymentNumber();
            payment.Fill(row);
            cashMain.PaymentNumber = payment.PaymentNumber;
            cashMain.GUID_UIType = main.GUID_UIType;
            AddCN_CheckDrawDetail(cashMain, main, cashMainClassId, detail);
            this.BusinessContext.CN_CheckDrawMain.AddObject(cashMain);


        }
        /// <summary>
        /// 支票领用明细表
        /// </summary>      
        /// <param name="checkMain">支票领用主表</param>
        /// <param name="cashMain">提现主表</param>
        /// <param name="cashMainClassId">提现ClassID</param>
        /// <param name="cashDetail">提现明细</param>       
        private void AddCN_CheckDrawDetail(CN_CheckDrawMain checkMain, CN_CashMain cashMain, int cashMainClassId, CN_CashDetail cashDetail)
        {
            CN_CheckDrawDetail detail = new CN_CheckDrawDetail();
            detail.GUID = Guid.NewGuid();
            detail.GUID_CheckDrawMain = checkMain.GUID;
            detail.GUID_DocMain = cashMain.GUID;
            detail.MainClassID = cashMainClassId;
            detail.GUID_DocDetail = cashDetail.GUID;
            int detailClassId = Infrastructure.CommonFuntion.GetClassId(typeof(CN_CashDetail).Name);
            detail.DetailClassID = detailClassId;
            checkMain.CN_CheckDrawDetail.Add(detail);
        }
        /// <summary>
        /// 需求记录
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            return new 历史记录(this.OperatorId, this.ModelUrl).History(conditions);
        }
        ///// <summary>
        ///// 历史记录
        ///// </summary>
        ///// <param name="conditions">条件</param>
        ///// <returns>JsonModel</returns>
        //public override List<object> History(SearchCondition conditions)
        //{
        //    JsonModel jsonmodel = new JsonModel();
        //    CN_HistoryBaseCondition historyconditions = (CN_HistoryBaseCondition)conditions;
        //    IQueryable<CN_CashMainView> main = this.BusinessContext.CN_CashMainView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单
        //    IQueryable<BX_DetailView> detail = this.BusinessContext.BX_DetailView;
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
        //        detail = detail.Where(e => e.GUID_Modifier == this.OperatorId);
        //    }
        //    if (historyconditions != null)
        //    {

        //        if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
        //        {
        //            int y;
        //            if (int.TryParse(historyconditions.Year, out y))
        //            {
        //                main = main.Where(e => e.DocDate.Year == y);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
        //        {
        //            int m;
        //            if (int.TryParse(historyconditions.Month, out m))
        //            {
        //                main = main.Where(e => e.DocDate.Month == m);
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(historyconditions.DocNum))
        //        {
        //            main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
        //        }

        //        #region 树查询信息
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
        //                    detail = detail.Where(e => depguid.Contains(e.GUID_Department));
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
        //                    detail = detail.Where(e => projectGUID.Contains((Guid)e.GUID_Project));
        //                    break;
        //                case "ss_projectclass":
        //                    SS_ProjectClass projectclassModel = new SS_ProjectClass();
        //                    projectclassModel.GUID = historyconditions.treeValue;
        //                    projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
        //                    var projectUID = projectList.Select(e => e.GUID);
        //                    detail = detail.Where(e => projectUID.Contains((Guid)e.GUID_Project));
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
        //        #endregion

        //    }
        //    //明细信息
        //    var dbdetai = from a in detail
        //                  group a by a.GUID_BX_Main into g
        //                  select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_Real) };
        //    var o = (from d in dbdetai
        //             join m in main on d.GUID_BX_Main equals m.GUID //into temp
        //             where d.GUID_BX_Main != null && m.GUID != null
        //             select new { m.GUID, m.DocNum, m.YWTypeName, m.DocTypeName, m.DWName, m.DepartmentName, m.PersonName, m.DocDate, d.Total_BX, m.MakeDate });

        //    var mainList = o.AsEnumerable().Select(e => new
        //    {
        //        e.GUID,
        //        e.DocNum,
        //        e.YWTypeName,
        //        e.DocTypeName,
        //        e.DWName,
        //        e.DepartmentName,
        //        e.PersonName,
        //        DocDate = e.DocDate.ToString("yyyy-MM-dd"),
        //        e.Total_BX,
        //        e.MakeDate
        //    }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

        //    return mainList;
        //}       

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
            CN_CashMain main = null; ; //new BX_Main();
            switch (status)
            {
                case "1": //新建
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        for (int i = 0; i < vResult.Validation.Count; i++)
                        {
                            strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                        }
                    }
                    break;
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);//修改验证
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        for (int i = 0; i < vResult.Validation.Count; i++)
                        {
                            strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                        }
                    }
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new CN_CashMain().ModelName());
                    vResult = DeleteVerify(value);
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        strMsg = vResult.Validation[0].Message + "<br>";//"\n";
                    }
                    break;

            }
            return strMsg;
        }
        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private CN_CashMain LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            CN_CashMain main = new CN_CashMain();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CN_CashDetail temp = new CN_CashDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new CN_CashDetail();
                        temp.CN_PaymentNumber = new CN_PaymentNumber();
                        temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                        temp.CN_PaymentNumber.Fill(row);

                        temp.Fill(row);
                        temp.GUID_Person = main.GUID_Person;
                        temp.GUID_CN_CashMain = main.GUID;
                        temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
                        main.CN_CashDetail.Add(temp);
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
        private List<ValidationResult> VerifyResultDetail(CN_CashDetail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            CN_CashDetail item = data;
            /// <summary>
            /// 明细表字段验证



            /// </summary>
            #region 明细表字段验证




            //报销金额
            if (item.Total_Cash.ToString() == "")
            {
                str = "明细金额 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.Total_Cash.GetType(), item.Total_Cash.ToString(), out g) == false)
                {
                    str = "明细报销金额格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {
                    if (double.Parse(g.ToString()) == 0F)
                    {
                        str = "明细报销金额不能为零！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //摘要

            if (string.IsNullOrEmpty(item.CashMemo))
            {
                str = "明细备注 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.CashMemo.GetType(), item.CashMemo, out g) == false)
                {
                    str = "明细备注格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }
            //项目GUID
            if (item.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_Project.GetType(), item.GUID_Project.ToString(), out g) == false)
            {
                str = "明细项目格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            ////部门GUID
            //if (item.GUID_Department.IsNullOrEmpty())
            //{
            //    str = "明细部门 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
            //    {
            //        str = "明细部门格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}
            ////结算方式GUID
            //if (item.GUID_SettleType.IsNullOrEmpty())
            //{
            //    str = "明细结算方式 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_SettleType.GetType(), item.GUID_SettleType.ToString(), out g) == false)
            //    {
            //        str = "明细结算方式格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}

            //财政支付码GUID
            if (item.GUID_PaymentNumber.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_PaymentNumber.GetType(), item.GUID_PaymentNumber.ToString(), out g) == false)
            {
                str = "明细财政支付码格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }


            #endregion

            #region 支付码验证



            if (item.CN_PaymentNumber != null)
            {
                var vf_pn = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex);
                if (vf_pn != null && vf_pn.Count > 0)
                {
                    resultList.AddRange(vf_pn);
                }
            }
            #endregion
            return resultList;
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
                        str = "支付码不能为空！";
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
            //if (data.GUID_EconomyClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_EconomyClass.GetType(), data.GUID_EconomyClass.ToString(), out g) == false)
            //{
            //    str = "经济分类格式不能为空！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
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
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(CN_CashMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            CN_CashMain mModel = data;
            object g;

            #region   主表字段验证

            //报销日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "提现日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "提现日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            //摘要
            if (mModel.DocMemo != null && Common.ConvertFunction.TryParse(mModel.DocMemo.GetType(), mModel.DocMemo, out g) == false)
            {
                str = "摘要 格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }

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

            ////报销人GUID
            //if (mModel.GUID_Person.IsNullOrEmpty())
            //{
            //    str = "报销人 字段为必输项！";
            //    resultList.Add(new ValidationResult("", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
            //    {
            //        str = "报销人格式不正确！";
            //        resultList.Add(new ValidationResult("", str));

            //    }
            //}
            //报销人部门





            //if (mModel.GUID_Department.IsNullOrEmpty())
            //{
            //    str = "报销部门 字段为必输项!";
            //    resultList.Add(new ValidationResult("", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
            //    {
            //        str = "报销部门格式不正确！";
            //        resultList.Add(new ValidationResult("", str));

            //    }
            //}
            ////单位GUID
            //if (mModel.GUID_DW.IsNullOrEmpty())
            //{
            //    str = "单位 字段为必输项!";
            //    resultList.Add(new ValidationResult("", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(mModel.GUID_DW.GetType(), mModel.GUID_DW.ToString(), out g) == false)
            //    {
            //        str = "单位格式不正确！";
            //        resultList.Add(new ValidationResult("", str));

            //    }
            //}

            return resultList;

            #endregion
        }
        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            CN_CashMain model = (CN_CashMain)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<CN_CashDetail> detailList = new List<CN_CashDetail>();
            foreach (CN_CashDetail item in model.CN_CashDetail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CN_CashDetail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResultDetail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        result._validation.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                List<ValidationResult> list = new List<ValidationResult>();
                list.Add(new ValidationResult("", "请添加明细科目信息！"));
                result._validation = list;

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
            CN_CashMain bxMain = new CN_CashMain();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (guid.IsNullOrEmpty())
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
                    str = "报销单GUID格式不正确！";
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


            CN_CashMain main = this.BusinessContext.CN_CashMain.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == "9")
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
            CN_CashMain model = (CN_CashMain)data;
            CN_CashMain orgModel = this.BusinessContext.CN_CashMain.Include("CN_CashDetail").FirstOrDefault(e => e.GUID == model.GUID);
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
                resultList.Add(new ValidationResult("", "此报销单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //作废           
            if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此报销单已经作废，不能进行修改！"));
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
            List<CN_CashDetail> detailList = new List<CN_CashDetail>();
            foreach (CN_CashDetail item in model.CN_CashDetail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CN_CashDetail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResultDetail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        result._validation.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                List<ValidationResult> list = new List<ValidationResult>();
                list.Add(new ValidationResult("", "请添加明细科目信息！"));
                result._validation = list;
            }

            return result;
        }
        #endregion

    }
}
