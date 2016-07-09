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
    public class 出纳付款单 : BaseDocument
    {

        public 出纳付款单() : base() { }
        public 出纳付款单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                CN_MainView model = new CN_MainView();
                model.FillDefault(this, this.OperatorId,this.ModelUrl);
                jmodel.m = model.Pick();

                CN_DetailView dModel = new CN_DetailView();
                dModel.FillDetailDefault<CN_DetailView>(this, this.OperatorId);
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.f.Add(fjgm);

                List<JsonAttributeModel> picker = dModel.Pick();

                CN_PaymentNumberView payment = new CN_PaymentNumberView();
                payment.FillCN_PaymentNumberDefault(this,this.ModelUrl);
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
                CN_MainView main = this.BusinessContext.CN_MainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();


                    IQueryable<CN_DetailView> q = this.BusinessContext.CN_DetailView.Where(e => e.GUID_CN_Main == guid).OrderBy(e => e.OrderNum);
                    List<CN_DetailView> details = q == null ? new List<CN_DetailView>() : q.ToList();
                    if (details.Count > 0)
                    {
                        JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                        jmodel.d.Add(jgm);
                        foreach (CN_DetailView detail in details)
                        {
                            List<JsonAttributeModel> picker = detail.Pick();
                            CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e => e.GUID == detail.GUID_PaymentNumber);
                            if (payment != null)
                            {
                                picker.AddRange(payment.Pick());
                            }
                            jgm.r.Add(picker);
                        }
                    }
                    //明细中f 填充默认值



                    CN_DetailView dModel = new CN_DetailView();
                    dModel.FillDetailDefault<CN_DetailView>(this, this.OperatorId);
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
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            CN_Main main = new CN_Main();           
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            //main.SubmitDate = DateTime.Now;
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CN_Detail temp = new CN_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        AddDetail(main, row, Grid.r.IndexOf(row));
                    }
                }
            }
            this.BusinessContext.CN_Main.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 添加明显信息
        /// </summary>
        /// <param name="main"></param>
        private void AddDetail(CN_Main main, List<JsonAttributeModel> row, int orderNum)
        {

            orderNum++;
            CN_Detail temp = new CN_Detail();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            temp.OrderNum = orderNum;

            temp.CN_PaymentNumber = new CN_PaymentNumber();
            temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            temp.CN_PaymentNumber.Fill(row);
            temp.CN_PaymentNumber.GUID_Project = temp.GUID_Project;         

            temp.GUID_Person = main.GUID_Person;
            temp.GUID_CN_Main = main.GUID;
            temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
            main.CN_Detail.Add(temp);

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
            CN_Main main = new CN_Main(); CN_Detail tempdetail = new CN_Detail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.CN_Main.Include("CN_Detail").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate;
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
             main.ResetDefault(this,this.OperatorId);
            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            //if (IsDateChange(orgDateTime, main.DocDate))
            //{
            //    main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString());
            //}

            string detailModelName = tempdetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (CN_Detail detail in main.CN_Detail) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {
                List<CN_Detail> detailList = new List<CN_Detail>();
                foreach (CN_Detail detail in main.CN_Detail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (CN_Detail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = Grid.r.IndexOf(row);
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);
                        detail.ResetDefault(this, this.OperatorId);

                        detail.CN_PaymentNumber.Fill(row);
                        detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
                        
                        detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    orderNum++;
                    AddDetail(main, row, Grid.r.IndexOf(row));
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
            CN_Main main = this.BusinessContext.CN_Main.Include("CN_Detail").FirstOrDefault(e => e.GUID == guid);

            List<CN_Detail> details = new List<CN_Detail>();

            foreach (CN_Detail item in main.CN_Detail)
            {
                details.Add(item);
            }

            foreach (CN_Detail item in details) { BusinessContext.DeleteConfirm(item); }

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
                Guid value = jsonModel.m.Id(new CN_Main().ModelName());
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "出纳付款单", data);
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
                OperatorLog.WriteLog(this.OperatorId, "出纳付款单", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
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
            CN_Main main = this.BusinessContext.CN_Main.FirstOrDefault(e => e.GUID == guid);
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
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            return new 历史记录().History(conditions);
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
            CN_Main main = null; 
            switch (status)
            {
                case "1": //新建
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);//修改验证
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new BX_Main().ModelName());
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
        private CN_Main LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            CN_Main main = new CN_Main();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CN_Detail temp = new CN_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new CN_Detail();
                        temp.CN_PaymentNumber = new CN_PaymentNumber();
                        temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                        temp.CN_PaymentNumber.Fill(row);

                        temp.Fill(row);
                        temp.GUID_Person = main.GUID_Person;
                        temp.GUID_CN_Main = main.GUID;
                        temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
                        main.CN_Detail.Add(temp);
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
        private List<ValidationResult> VerifyResultDetail(CN_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<CN_Detail> detailList = new List<CN_Detail>();
            foreach (CN_Detail item in data.CN_Detail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CN_Detail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_CN_Detail(item, rowIndex);
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
        private List<ValidationResult> VerifyResultMain(CN_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            CN_Main mModel = data;
            object g;

            #region   主表字段验证

            //报销日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "单据日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "单据日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //附单据数量



            if (mModel.BillCount != null && Common.ConvertFunction.TryParse(mModel.BillCount.GetType(), mModel.BillCount.ToString(), out g) == false)
            {
                str = "附单据数量 格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //摘要
            if (string.IsNullOrEmpty(mModel.DocMemo))
            {
                str = "摘要 字段为必输入项！";
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

            //领款人GUID
            if (mModel.GUID_Person.IsNullOrEmpty())
            {
                str = "领款人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "领款人 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //报销人部门

            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "领款人部门 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "领款人部门格式不正确！";
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
        private List<ValidationResult> VerifyResult_CN_Detail(CN_Detail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            CN_Detail item = data;
            /// <summary>
            /// 明细表字段验证
            /// </summary>
            #region 明细表字段验证


           
            //报销金额
            if (item.Total_CN.ToString() == "")
            {
                str = "明细付款金额 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.Total_CN.GetType(), item.Total_CN.ToString(), out g) == false)
                {
                    str = "明细付款金额格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {
                    if (double.Parse(g.ToString()) == 0F)
                    {
                        str = "明细付款金额不能为零！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //摘要

            if (string.IsNullOrEmpty(item.DetailMemo))
            {
                str = "明细摘要 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.DetailMemo.GetType(), item.DetailMemo, out g) == false)
                {
                    str = "明细摘要格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }
            //项目GUID
            if (item.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_Project.GetType(), item.GUID_Project.ToString(), out g) == false)
            {
                str = "明细项目格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //部门GUID
            if (item.GUID_Department.IsNullOrEmpty())
            {
                str = "明细部门 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                {
                    str = "明细部门格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            //结算方式GUID
            if (item.GUID_SettleType.IsNullOrEmpty())
            {
                str = "明细结算方式 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_SettleType.GetType(), item.GUID_SettleType.ToString(), out g) == false)
                {
                    str = "明细结算方式格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
          
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
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            CN_Main model = (CN_Main)data;
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

        /// <summary>
        /// 数据从数据库删除前验证
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            CN_Main bxMain = new CN_Main();
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
                    str = "此单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                    result._validation = resultList;
                    return result;
                }

            }
            //流程验证
            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此单正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
                return result;
            }
            //作废的不能删除

            CN_Main main = this.BusinessContext.CN_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == "9")
                {
                    str = "此单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                    result._validation = resultList;
                    return result;
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
            CN_Main model = (CN_Main)data;
            CN_Main orgModel = this.BusinessContext.CN_Main.Include("CN_Detail").FirstOrDefault(e => e.GUID == model.GUID);
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
            if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
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
}
