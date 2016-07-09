using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using Platform.Flow.Run;
using BusinessModel;
namespace Business.Reimbursement
{    
    public class 差旅报销单 : BXDocument
    {
        public 差旅报销单() : base() { }
        public 差旅报销单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        public override bool UpdateDocState(Guid guid, EnumType.EnumDocState docState)
        {
            BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
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
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                BX_MainView model = new BX_MainView();
                model.FillDefault(this, this.OperatorId);
                jmodel.m = model.Pick();

                BX_DetailView dModel = new BX_DetailView();
                dModel.FillDetailDefault<BX_DetailView>(this, this.OperatorId,this.ModelUrl);
                jmodel.m.AddRange(dModel.Pick());

                CN_PaymentNumberView payment = new CN_PaymentNumberView();
                payment.FillCN_PaymentNumberDefault(this, this.ModelUrl);
                jmodel.m.AddRange(payment.Pick());

                BX_TravelView tModel = new BX_TravelView();
                tModel.FillBX_TravelDefault(this);
                JsonGridModel fjgm = new JsonGridModel(tModel.ModelName());
                jmodel.f.Add(fjgm);
                List<JsonAttributeModel> picker = tModel.Pick();
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
                BX_MainView main = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    GetBXDetail(jmodel, main);
                    GetBXTravelDetail(jmodel, main);
                    GetRetrieveDefault(jmodel);
                    jmodel.s = new JsonMessage("", "", "");
                }
                else
                {
                    jmodel.result = JsonModelConstant.Info;
                    jmodel.s = new JsonMessage("提示", "无数据！", JsonModelConstant.Info);
                }
                jmodel.m.Add(new JsonAttributeModel() { m = "BX_Main", n = "GLDH", v = GetGLDocNum(guid) });
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
        /// 报销明细信息(主页面信息)
        /// </summary>
        /// <param name="jmodel"></param>
        /// <param name="main"></param>
        private void GetBXDetail(JsonModel jmodel, BX_MainView main)
        {
            if (main == null) return;
            jmodel.m = main.Pick();//主单信息
            BX_DetailView detailView = this.BusinessContext.BX_DetailView.FirstOrDefault(e => e.GUID_BX_Main == main.GUID);
            if (detailView != null)
            {
                jmodel.m.AddRange(detailView.Pick());
                CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e => e.GUID == detailView.GUID_PaymentNumber);
                if (payment != null)
                {
                    jmodel.m.AddRange(payment.Pick());
                }
            }

        }
        /// <summary>
        /// 差旅费明细信息
        /// </summary>
        /// <param name="jmodel"></param>
        /// <param name="main"></param>
        private void GetBXTravelDetail(JsonModel jmodel, BX_MainView main)
        {
            //差旅信息明细          
            var detailList = this.BusinessContext.BX_TravelView.Where(e => e.GUID_BX_Main == main.GUID).OrderBy(e => e.OrderNum).ToList();
            if (detailList != null && detailList.Count > 0)
            {
                JsonGridModel feeJgm = new JsonGridModel(detailList[0].ModelName());
                jmodel.d.Add(feeJgm);
                foreach (BX_TravelView item in detailList)
                {
                    List<JsonAttributeModel> colList = item.Pick();
                    colList[2].v = item.StartDate.ToString();
                    colList[3].v = item.ArriveDate.ToString();
                    feeJgm.r.Add(colList);
                }
            }
            //出差补助明细
            var traveAllowanceList = this.BusinessContext.BX_TravelAllowanceView.Where(e=>e.GUID_BX_Main==main.GUID).OrderBy(e=>e.AllowanceKey).ToList();
            if (traveAllowanceList != null && traveAllowanceList.Count > 0)
            {
                JsonGridModel feeJgm = new JsonGridModel(traveAllowanceList[0].ModelName());
                jmodel.d.Add(feeJgm);
                foreach (BX_TravelAllowanceView item in traveAllowanceList)
                {
                    List<JsonAttributeModel> colList = item.Pick();
                    feeJgm.r.Add(colList);
                }
            }
        }
        /// <summary>
        /// 获取返回值的默认值
        /// </summary>
        /// <param name="jmodel"></param>
        private void GetRetrieveDefault(JsonModel jmodel)
        {
            //明细中f 填充默认值

            BX_TravelView tModel = new BX_TravelView();
            tModel.FillBX_TravelDefault(this);
            JsonGridModel fjgm = new JsonGridModel(tModel.ModelName());
            jmodel.f.Add(fjgm);
            List<JsonAttributeModel> picker = tModel.Pick();
            fjgm.r.Add(picker);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            BX_Main main = new BX_Main();
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                
                AddBXTravalDetail(jsonModel, main);
                //AddBXDetail(jsonModel, main);
            }
            this.BusinessContext.BX_Main.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 添加报销明细信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
        private void AddBXDetail(JsonModel jsonModel, BX_Main main)
        {
            //明细信息
            BX_Detail detail = new BX_Detail();
            detail.GUID = Guid.Empty;
            detail.FillDefault(this, this.OperatorId);
            detail.Fill(jsonModel.m);
            detail.GUID = Guid.NewGuid();
            detail.GUID_Person = main.GUID_Person;
            detail.GUID_BX_Main = main.GUID;
            detail.GUID_Department = main.GUID_Department;
            detail.Total_Real = detail.Total_BX;

            detail.CN_PaymentNumber = new CN_PaymentNumber();
            detail.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            detail.CN_PaymentNumber.Fill(jsonModel.m);
            detail.CN_PaymentNumber.GUID = Guid.NewGuid();
            detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
            detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;

            detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
            main.BX_Detail.Add(detail);

        }
        /// <summary>
        /// 添加劳务费明细
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
        private void AddBXTravalDetail(JsonModel jsonModel, BX_Main main)
        {
            //添加差旅明细信息
            string dModelName = new BX_Travel().ModelName();
            JsonGridModel detailGrid = jsonModel.d.Find(dModelName);
            if (detailGrid != null)
            {
                int orderNum = 0;
                foreach (List<JsonAttributeModel> row in detailGrid.r)
                {
                    BX_Travel travel = new BX_Travel();
                    orderNum++;
                    travel.FillDefault(this, this.OperatorId);
                    travel.Fill(row);
                    travel.GUID = Guid.NewGuid();
                    travel.OrderNum = detailGrid.r.IndexOf(row);
                    travel.GUID_BX_Main = main.GUID;
                    main.BX_Travel.Add(travel);
                }

            }
            var FeeMoney = main.BX_Travel.Sum(e => e.TicketMoney);// * e.TicketCount
            if (FeeMoney == null) FeeMoney = 0;
            //出差补助明细
            string travelAllowanceName = new BX_TravelAllowance().ModelName();
            detailGrid = jsonModel.d.Find(travelAllowanceName);
            if (detailGrid != null)
            {
                int orderNum = 0;
                foreach (List<JsonAttributeModel> row in detailGrid.r)
                {
                    BX_TravelAllowance travelAllowance = new BX_TravelAllowance();
                    orderNum++;
                   // travelAllowance.FillDefault(this, this.OperatorId);
                    travelAllowance.Fill(row);
                    travelAllowance.GUID = Guid.NewGuid();
                    travelAllowance.GUID_BX_Main = main.GUID;
                    
                    main.BX_TravelAllowance.Add(travelAllowance);
                }

            }
            var sumAllowance = main.BX_TravelAllowance.Sum(e => e.AllowenMoney);

            //明细信息
            BX_Detail detail = new BX_Detail();
            detail.GUID = Guid.Empty;
            detail.FillDefault(this, this.OperatorId);
            detail.Fill(jsonModel.m);
            detail.GUID = Guid.NewGuid();
            detail.GUID_Person = main.GUID_Person;
            detail.GUID_BX_Main = main.GUID;
            detail.GUID_Department = main.GUID_Department;
            detail.Total_BX = sumAllowance +(double) FeeMoney;
            detail.Total_Real = detail.Total_BX;

            detail.CN_PaymentNumber = new CN_PaymentNumber();
            detail.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            detail.CN_PaymentNumber.Fill(jsonModel.m);
            detail.CN_PaymentNumber.GUID = Guid.NewGuid();
            detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
            detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;

            detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
            main.BX_Detail.Add(detail);

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
            BX_Main main = new BX_Main();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.BX_Main.Include("BX_Detail").Include("BX_Detail.CN_PaymentNumber").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate;
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
             main.ResetDefault(this,this.OperatorId);
            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            //if (CreateDocNumber.IsDateChange(this.BusinessContext, orgDateTime, main.DocDate))
            //{
            //    main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString());
            //}

            ModifyBxDetail(jsonModel, main);
            ModifyBxTravelDetail(jsonModel, main);
            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 修改报销明细信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        private void ModifyBxDetail(JsonModel jsonModel, BX_Main main)
        {
            BX_Detail detailModel = new BX_Detail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(detailModel.ModelName());
            if (id == null)
            {
                foreach (BX_Detail item in main.BX_Detail) { this.BusinessContext.DeleteConfirm(item); }
            }
            else
            {

                List<BX_Detail> detailList = new List<BX_Detail>();
                foreach (BX_Detail detail in main.BX_Detail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (BX_Detail detail in detailList)
                {
                    JsonAttributeModel row = jsonModel.m.IdAttribute(detail.ModelName());

                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = orderNum;
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(jsonModel.m);
                        detail.ResetDefault(this, this.OperatorId);
                        detail.CN_PaymentNumber.Fill(jsonModel.m);
                        detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
                        detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;
                        detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
                        detail.Total_Real=  detail.Total_BX ;
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }

            }

        }
        /// <summary>
        /// 修改差旅明细信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        private void ModifyBxTravelDetail(JsonModel jsonModel, BX_Main main)
        {
            double money = 0;
            ModifyBxTravel(jsonModel, main,ref money);
            ModifyTravelAllowance(jsonModel,main,ref money);
            /*明细*/
            BX_Detail detailModel = new BX_Detail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(detailModel.ModelName());
            if (id == null)
            {
                foreach (BX_Detail item in main.BX_Detail) { this.BusinessContext.DeleteConfirm(item); }
            }
            else
            {

                List<BX_Detail> detailList = new List<BX_Detail>();
                foreach (BX_Detail detail in main.BX_Detail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (BX_Detail detail in detailList)
                {
                    JsonAttributeModel row = jsonModel.m.IdAttribute(detail.ModelName());

                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = orderNum;
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(jsonModel.m);
                        detail.ResetDefault(this, this.OperatorId);
                        detail.CN_PaymentNumber.Fill(jsonModel.m);
                        detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
                        detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;
                        detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
                        detail.Total_BX = money;
                        detail.Total_Real = detail.Total_BX;
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }

            }
        }

        /// <summary>
        /// 差旅细信息信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        private void ModifyBxTravel(JsonModel jsonModel, BX_Main main,ref double money)
        {
            BX_Travel tempFee = new BX_Travel();
            string detailModelName = tempFee.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                //差旅费明细
                foreach (BX_Travel detail in main.BX_Travel) { this.BusinessContext.DeleteConfirm(detail); }              
            }
            else
            {
                //差旅费明细                
                List<BX_Travel> detailList = new List<BX_Travel>();
                foreach (BX_Travel detail in main.BX_Travel)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (BX_Travel detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = Grid.r.IndexOf(row) + 1;
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);

                        var ticket = row.FirstOrDefault(e => e.n == "TicketCount");
                        if (ticket != null)
                        {
                            int i = 0;
                            int.TryParse(ticket.v, out i);
                            detail.TicketCount = i;
                        }
                        else {
                            detail.TicketCount = null;
                        }
                        detail.TicketCount = detail.TicketCount == 0 ? null : detail.TicketCount;
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                if (newRows != null && newRows.Count > 0)
                {
                    AddBxTravelEx(newRows, main, Grid.r);
                }
                var FeeMoney = main.BX_Travel.Sum(e => e.TicketMoney);// * e.TicketMoney);
                if (FeeMoney == null) {
                    FeeMoney = 0;
                }
                money += (double)FeeMoney;
            }
        }
        /// <summary>
        /// 差旅补助明细息信
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        private void ModifyTravelAllowance(JsonModel jsonModel, BX_Main main, ref double money)
        {
            BX_TravelAllowance tempFee = new BX_TravelAllowance();
            string detailModelName = tempFee.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BX_TravelAllowance detail in main.BX_TravelAllowance) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {               
                //差旅补助明细
                List<BX_TravelAllowance> traveAllowanceList = new List<BX_TravelAllowance>();
                foreach (BX_TravelAllowance detail in main.BX_TravelAllowance)
                {
                    traveAllowanceList.Add(detail);
                }
                foreach (BX_TravelAllowance detail in traveAllowanceList)
                {
                    this.BusinessContext.DeleteConfirm(detail);
                    //List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    //if (row == null) 
                    //else
                    //{                       
                    // //   detail.FillDefault(this, this.OperatorId);
                    //    detail.Fill(row);
                       
                    //    this.BusinessContext.ModifyConfirm(detail);

                    //}
                }
                List<List<JsonAttributeModel>> newRows = Grid.r;//查找GUID为空的行，并且为新增
                if (newRows != null && newRows.Count > 0)
                {
                    AddTravelAllowance(newRows, main);
                }
                var AllowanceSum = main.BX_TravelAllowance.Sum(e => e.AllowenMoney);
                money += AllowanceSum;
            }
        }

        /// <summary>
        /// 差旅明细信息
        /// </summary>
        /// <param name="newRows"></param>
        /// <param name="main"></param>
        /// <param name="orderNum"></param>
        private void AddBxTravel(List<List<JsonAttributeModel>> newRows, BX_Main main, int orderNum)
        {
            foreach (List<JsonAttributeModel> row in newRows)
            {
                orderNum++;
                BX_Travel newitem = new BX_Travel();
                newitem.FillDefault(this, this.OperatorId);
                newitem.Fill(row);
                newitem.OrderNum = orderNum;
                newitem.GUID_BX_Main = main.GUID;
                main.BX_Travel.Add(newitem);
            }
        }
        /// <summary>
        /// 差旅明细信息
        /// </summary>
        /// <param name="newRows"></param>
        /// <param name="main"></param>
        /// <param name="orderNum"></param>
        private void AddBxTravelEx(List<List<JsonAttributeModel>> newRows, BX_Main main, List<List<JsonAttributeModel>> totals)
        {
            foreach (List<JsonAttributeModel> row in newRows)
            {
                
                BX_Travel newitem = new BX_Travel();
                newitem.FillDefault(this, this.OperatorId);
                newitem.Fill(row);
                newitem.OrderNum = totals.IndexOf(row) + 1;
                newitem.GUID_BX_Main = main.GUID;
                main.BX_Travel.Add(newitem);
            }
        }
        /// <summary>
        /// 差旅补助明细信息
        /// </summary>
        /// <param name="newRows"></param>
        /// <param name="main"></param>
        /// <param name="orderNum"></param>
        private void AddTravelAllowance(List<List<JsonAttributeModel>> newRows, BX_Main main)
        {
            foreach (List<JsonAttributeModel> row in newRows)
            {               
                BX_TravelAllowance newitem = new BX_TravelAllowance();
                newitem.FillDefault(this, this.OperatorId);
                newitem.Fill(row);
                newitem.GUID = Guid.NewGuid();
                newitem.GUID_BX_Main = main.GUID;
                main.BX_TravelAllowance.Add(newitem);
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
            List<BX_Detail> details = new List<BX_Detail>();

            foreach (BX_Detail item in main.BX_Detail)
            {
                details.Add(item);
            }

            foreach (BX_Detail item in details) { BusinessContext.DeleteConfirm(item); }

            //删除差旅费
            List<BX_Travel> feeList = new List<BX_Travel>();
            foreach (BX_Travel item in main.BX_Travel)
            {
                feeList.Add(item);
            }
            if (feeList != null && feeList.Count > 0)
            {
                foreach (BX_Travel item in feeList)
                {
                    BusinessContext.DeleteConfirm(item);
                }
            }
            //删除补助差旅费
            List<BX_TravelAllowance> travelAllowanceList = new List<BX_TravelAllowance>();
            foreach (BX_TravelAllowance item in main.BX_TravelAllowance)
            {
                travelAllowanceList.Add(item);
            }
            if (feeList != null && feeList.Count > 0)
            {
                foreach (BX_TravelAllowance item in travelAllowanceList)
                {
                    BusinessContext.DeleteConfirm(item);
                }
            }

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
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                Guid value = jsonModel.m.Id(new BX_Main().ModelName());
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

                    OperatorLog.WriteLog(this.OperatorId, value, status, "差旅报销单", data);
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
                OperatorLog.WriteLog(this.OperatorId, "差旅报销单", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }       
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
            BX_Main main = null; ; //new BX_Main();
            switch (status)
            {
                case "1": //新建
                    main = LoadBX_Main(jsonModel);//.Fill(jsonModel.m);
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
                    main = LoadBX_Main(jsonModel);
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
                    Guid value = jsonModel.m.Id(new BX_Main().ModelName());
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
        private BX_Main LoadBX_Main(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            BX_Main main = new BX_Main();
            main.Fill(jsonModel.m);
            //明细信息
            BX_Detail detail = new BX_Detail();
            detail.Fill(jsonModel.m);
            detail.GUID_Department = main.GUID_Department;

            //支付码
            CN_PaymentNumber payment = new CN_PaymentNumber();
            payment.Fill(jsonModel.m);
            detail.CN_PaymentNumber = payment;
            main.BX_Detail.Add(detail);
            //差旅费明细
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                BX_Travel temp = new BX_Travel();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new BX_Travel();
                        temp.Fill(row);
                        main.BX_Travel.Add(temp);
                    }
                }
            }
            //差旅补助明细
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                BX_TravelAllowance temp = new BX_TravelAllowance();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new BX_TravelAllowance();
                        temp.Fill(row);
                        main.BX_TravelAllowance.Add(temp);
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
        private List<ValidationResult> VerifyResult_Bx_Detail(BX_Detail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BX_Detail item = data;

            /// <summary>
            /// 明细表字段验证
            /// </summary>
            #region 明细表字段验证

            //预算科目的GUID
            if (item.GUID_BGCode.IsNullOrEmpty())
            {
                str = "预算科目 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_BGCode.GetType(), item.GUID_BGCode.ToString(), out g) == false)
                {
                    str = "预算科目格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            //项目GUID
            if (item.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_Project.GetType(), item.GUID_Project.ToString(), out g) == false)
            {
                str = "项目格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //部门GUID
            if (item.GUID_Department.IsNullOrEmpty())
            {
                str = "部门 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                {
                    str = "部门格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //结算方式GUID
            if (item.GUID_SettleType.IsNullOrEmpty())
            {
                str = "结算方式 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_SettleType.GetType(), item.GUID_SettleType.ToString(), out g) == false)
                {
                    str = "结算方式格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            //财政支付码GUID
            if (item.GUID_PaymentNumber.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_PaymentNumber.GetType(), item.GUID_PaymentNumber.ToString(), out g) == false)
            {
                str = "财政支付码格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //出差事由 不能为空
            if (string.IsNullOrEmpty(item.FeeMemo))
            {
                str = "出差事由 字段为必输项！";
                resultList.Add(new ValidationResult("", str));                
            }
            else
            {
                if ((item.FeeMemo + "").Trim().Length > 200)//hanyx Update
                {
                    str = "出差事由 字段最长为200字符！";
                    resultList.Add(new ValidationResult("", str));
                }
            }


            #endregion

            #region 支付码验证

            if (item.CN_PaymentNumber != null)
            {
                //var vf_pn = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex);
                var vf_pn = base.VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex, item.BX_Main.GUID_DW);
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
                resultList.Add(new ValidationResult("", str));

            }
            //是否国库
            if (data.IsGuoKu.ToString() == "")
            {
                str = "是否国库 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(data.IsGuoKu.GetType(), data.IsGuoKu.ToString(), out g) == false)
                {
                    str = "是否国库格式不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }
                else
                {

                    //如果不为空则,则支付码不能为空
                    if (data.IsGuoKu == true && string.IsNullOrEmpty(data.PaymentNumber))
                    {
                        str = "财政支付令不能为空！";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
            }
            //是否项目
            if (data.IsProject.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.IsProject.GetType(), data.IsProject.ToString(), out g) == false)
            {
                str = "项目格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //功能分类GUID
            if (data.GUID_FunctionClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_FunctionClass.GetType(), data.GUID_FunctionClass.ToString(), out g) == false)
            {
                str = "功能分类格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //预算科目GUID
            if (data.GUID_BGCode.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGCode.GetType(), data.GUID_BGCode.ToString(), out g) == false)
            {
                str = "预算科目格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }
            //经济分类GUID
            if (data.GUID_EconomyClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_EconomyClass.GetType(), data.GUID_EconomyClass.ToString(), out g) == false)
            {
                str = "经济分类格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }
            //支出类型GUID
            if (data.GUID_ExpendType.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_ExpendType.GetType(), data.GUID_ExpendType.ToString(), out g) == false)
            {
                str = "支出类型格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }
            //项目GUID
            if (data.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_Project.GetType(), data.GUID_Project.ToString(), out g) == false)
            {
                str = "项目格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }
            //项目财政编号
            if (!string.IsNullOrEmpty(data.FinanceProjectKey) && Common.ConvertFunction.TryParse(data.FinanceProjectKey.GetType(), data.FinanceProjectKey.ToString(), out g) == false)
            {
                str = "项目财政编号格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }
            //预算来源GUID
            if(data.GUID_BGResource.IsNullOrEmpty())
            {
                 str = "预算来源 为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(data.GUID_BGResource.GetType(), data.GUID_BGResource.ToString(), out g) == false)
                {
                    str = "预算来源格式不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }
            }

            #endregion

            return resultList;
        }
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_BX_Main(BX_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BX_Main mModel = data;
            object g;

            #region   主表字段验证

            //报销日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "报销日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "报销日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //报销人
            if (mModel.GUID_Person.IsNullOrEmpty())
            {
                str = "报销人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "报销人 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //单位名称
            if (mModel.GUID_DW.IsNullOrEmpty())
            {
                str = "单位名称 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_DW.GetType(), mModel.GUID_DW.ToString(), out g) == false)
                {
                    str = "单位名称 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //部门名称
            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "部门名称 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "部门名称 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //摘要
            if (string.IsNullOrEmpty(mModel.DocMemo))
            {
                str = "摘要 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            //附单据数量
            if (string.IsNullOrEmpty(mModel.BillCount+""))
            {
                str = "附单数据为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            //if (mModel.BillCount != null)
            //{
            //    str = "附单据数量 格式不正确！";
            //    resultList.Add(new ValidationResult("", str));

            //}           
            
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
            return resultList;

            #endregion
        }
        /// <summary>
        /// 差旅明细信息验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_TravelDetail(BX_Main data)
        {
            List<ValidationResult> resultList = new List<ValidationResult>();
            //差旅明细信息
            var vf_Travel = VerifyResult_Travel(data);
            if (vf_Travel != null)
            {
                resultList.AddRange(vf_Travel);
            }
            //补助差旅明细信息
            var vf_TravelAllowance = VerifyResult_TravelAllowance(data);
            if (vf_TravelAllowance!=null)
            {
                resultList.AddRange(vf_TravelAllowance);
            }

            return resultList;
        }
        /// <summary>
        /// 差旅明细验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_Travel(BX_Main data)
        {
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            string str = string.Empty;
            BX_Main mModel = data;
            if (mModel == null) return null;
            List<BX_Travel> detailList = new List<BX_Travel>();
            foreach (BX_Travel item in mModel.BX_Travel)
            {
                detailList.Add(item);
            }
            if (detailList.Count == 0)
            {
                return resultList;
                //str = "尚未添加差旅费明细项！";
                //resultList.Add(new ValidationResult("", str));
            }
            int rowIndex = 0;
            foreach (BX_Travel item in detailList)
            {
                rowIndex++;
                //出发时间
                if (item.StartDate.IsNullOrEmpty())
                {
                    str = "出发时间 字段为必输项！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
                else
                {
                    if (Common.ConvertFunction.TryParse(item.StartDate.GetType(), item.StartDate.ToString(), out g) == false)
                    {
                        str = "出发时间 字段格式不正确！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
                //到达时间
                if (item.ArriveDate.IsNullOrEmpty())
                {
                    str = "到达时间 字段为必输项！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
                else
                {
                    if (Common.ConvertFunction.TryParse(item.ArriveDate.GetType(), item.StartDate.ToString(), out g) == false)
                    {
                        str = "到达时间 字段格式不正确！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
                //出发地点
                if (string.IsNullOrEmpty(item.PlaceFrom))
                {
                    str = "出发地点 字段为必输项！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    if ((item.PlaceFrom+"").Trim().Length > 50)
                    {
                        str = "出发地点 字段最长为50字符！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
                //到达地点
                if (string.IsNullOrEmpty(item.PlaceTo))
                {
                    str = "到达地点 字段为必输项！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    if ((item.PlaceTo+"").Trim().Length > 50)
                    {
                        str = "到达地点 字段最长为50字符！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }

                //交通工具

                if (item.GUID_Traffic.IsNullOrEmpty())
                {
                    str = "交通工具 字段为必输项！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
                else
                {
                    if (Common.ConvertFunction.TryParse(item.GUID_Traffic.GetType(), item.GUID_Traffic.ToString(), out g) == false)
                    {
                        str = "交通工具GUID 字段格式不正确！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
                //据张
                if (item.TicketCount != 0 &&item.TicketCount!=null&& Common.ConvertFunction.TryParse(item.TicketCount.GetType(), item.TicketCount.ToString(), out g) == false)
                {
                    //str = "据张 字段格式不正确！";
                    //resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    item.TicketCount = 0;
                }
                //金额
                if (item.TicketMoney == 0)
                {
                    item.TicketMoney = 0;
                    //str = "金额 字段格式不正确！";
                    //resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }

            return resultList;
        }
        /// <summary>
        /// 补助差旅明细验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_TravelAllowance(BX_Main data)
        {
            List<ValidationResult> resultList = new List<ValidationResult>();
            string str = string.Empty;
            if (data == null) return null;
            List<BX_TravelAllowance> detailList = new List<BX_TravelAllowance>();
            foreach (BX_TravelAllowance item in data.BX_TravelAllowance)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count==0)
            {
                //str = "补助差旅明细尚未填写！";
                //resultList.Add(new ValidationResult("", str));
                return resultList;               
            }
            int rowIndex = 0;
            object g;
            foreach (BX_TravelAllowance item in detailList)
            {
                rowIndex++;
                if (item.GUID_Person.IsNullOrEmpty())
                {
                    str = "出差人 字段为必输项！";
                    resultList.Add(new ValidationResult("", str));
                }
                if (string.IsNullOrEmpty(item.AllowanceDays+"") || (item.AllowanceDays != 0 && Common.ConvertFunction.TryParse(item.AllowanceDays.GetType(), item.AllowanceDays.ToString(), out g) == false))
                {
                    //str = "天数 字段为必输项！";
                    //resultList.Add(new ValidationResult("", str));
                }

                if (item.AllowancePrice.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.AllowancePrice.GetType(), item.AllowancePrice.ToString(), out g) == false)
                {
                    //str = "标准 字段格式不正确！";
                    //resultList.Add(new ValidationResult("", str));
                }
                if (item.AllowenMoney.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.AllowenMoney.GetType(), item.AllowenMoney.ToString(), out g) == false)
                {
                    str = "金额 字段格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
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
            BX_Main model = (BX_Main)data;
            //主Model验证
            var vf_main = VerifyResult_BX_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<BX_Detail> bx_DetailList = new List<BX_Detail>();
            foreach (BX_Detail item in model.BX_Detail)
            {
                bx_DetailList.Add(item);
            }
            if (bx_DetailList != null && bx_DetailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BX_Detail item in bx_DetailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        result._validation.AddRange(vf_detail);
                    }
                }
            }
            //差旅费明细验证
            var vfTravel = VerifyResult_TravelDetail(model);
            if (vfTravel != null && vfTravel.Count > 0)
            {
                result._validation.AddRange(vfTravel);
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
            BX_Main bxMain = new BX_Main();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
                return result;
               
            }
            else
            {
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "报销单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                    result._validation = resultList;
                    return result;
                }

            }
            //流程验证
            //作废时也要判断 不能删除？待定

            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此报销单正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
                return result;
            }
            //作废的不能删除
            BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == "9")
                {
                    str = "此报销单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                    result._validation = resultList;
                    return result;
                }
            }
            result._validation = resultList;
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
            BX_Main model = (BX_Main)data;
            BX_Main orgModel = this.BusinessContext.BX_Main.Include("BX_Detail").FirstOrDefault(e => e.GUID == model.GUID);
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
            if (WorkFlowAPI.ExistProcessCurPerson(model.GUID,OperatorId))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此差旅费正在流程审核中，不能进行修改！"));
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
            var vf_main = VerifyResult_BX_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<BX_Detail> bx_DetailList = new List<BX_Detail>();
            foreach (BX_Detail item in model.BX_Detail)
            {
                bx_DetailList.Add(item);
            }
            if (bx_DetailList != null && bx_DetailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BX_Detail item in bx_DetailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        result._validation.AddRange(vf_detail);
                    }
                }
            }
            //差旅费明细验证
            var vfTravel = VerifyResult_TravelDetail(model);
            if (vfTravel != null && vfTravel.Count > 0)
            {
                result._validation.AddRange(vfTravel);
            }

            return result;
        }


    }
}
