using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Platform.Flow.Run;
using Business.CommonModule;
using BusinessModel;

namespace Business.Reimbursement
{    
    public class 应付单 : WLDocument
    {

        public 应付单() : base() { }
        public 应付单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();


                WL_MainView model = new WL_MainView();
                model.FillDefault(this, this.OperatorId,this.ModelUrl);
                jmodel.m = model.Pick();

                WL_DetailView dModel = new WL_DetailView();
                dModel.FillDetailDefault<WL_DetailView>(this, this.OperatorId, this.ModelUrl);
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.f.Add(fjgm);

                List<JsonAttributeModel> picker = dModel.Pick();

                CN_PaymentNumberView payment = new CN_PaymentNumberView();
                payment.FillCN_PaymentNumberDefault(this, this.ModelUrl);
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
                WL_MainView main = this.BusinessContext.WL_MainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();


                    IQueryable<WL_DetailView> q = this.BusinessContext.WL_DetailView.Where(e => e.GUID_WL_Main == guid).OrderBy(e => e.OrderNum);
                    List<WL_DetailView> details = q == null ? new List<WL_DetailView>() : q.ToList();
                    if (details.Count > 0)
                    {
                        JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                        jmodel.d.Add(jgm);
                        foreach (WL_DetailView detail in details)
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
                    WL_DetailView dModel = new WL_DetailView();
                    dModel.FillDetailDefault<WL_DetailView>(this, this.OperatorId,this.ModelUrl);
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
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            WL_Main main = this.BusinessContext.WL_Main.Include("WL_Detail").FirstOrDefault(e => e.GUID == guid);

            List<WL_Detail> details = new List<WL_Detail>();

            foreach (WL_Detail item in main.WL_Detail)
            {
                details.Add(item);
            }

            foreach (WL_Detail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
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
            WL_Main main = new WL_Main();
            WL_Detail tempdetail = new WL_Detail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.WL_Main.Include("WL_Detail").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate;
            }
            main.FillDefault(this, this.OperatorId,this.ModelUrl);
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
                foreach (WL_Detail detail in main.WL_Detail) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {
                List<WL_Detail> detailList = new List<WL_Detail>();
                foreach (WL_Detail detail in main.WL_Detail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (WL_Detail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = Grid.r.IndexOf(row);
                        detail.FillDetailDefault(this, this.OperatorId,this.ModelUrl);
                        detail.Fill(row);
                        detail.ResetDefault(this, this.OperatorId);
                        //支付码
                        detail.CN_PaymentNumber.Fill(row);
                        detail.CN_PaymentNumber.GUID_Project = detail.GUID_ProjectKey;
                        detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;

                        detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;

                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    orderNum++;
                    WL_Detail newitem = new WL_Detail();
                    newitem.FillDefault(this, this.OperatorId);
                    newitem.Fill(row);
                    newitem.OrderNum = Grid.r.IndexOf(row);

                    CN_PaymentNumber newnumber = new CN_PaymentNumber();
                    newitem.CN_PaymentNumber = newnumber;
                    newitem.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                    newitem.CN_PaymentNumber.Fill(row);
                    newitem.CN_PaymentNumber.GUID_Project = newitem.GUID_ProjectKey;
                    newitem.CN_PaymentNumber.GUID_BGCode = newitem.GUID_BGCode;

                    newitem.GUID_Person = main.GUID_Person;
                    newitem.GUID_WL_Main = main.GUID;
                    newitem.GUID_PaymentNumber = newitem.CN_PaymentNumber.GUID;
                    main.WL_Detail.Add(newitem);
                }
            }
            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
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
            WL_Main main = new WL_Main();
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();

            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                WL_Detail temp = new WL_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        orderNum++;
                        temp = new WL_Detail();
                        temp.FillDefault(this, this.OperatorId);
                        temp.Fill(row);
                        temp.OrderNum = Grid.r.IndexOf(row);

                        temp.CN_PaymentNumber = new CN_PaymentNumber();
                        temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                        temp.CN_PaymentNumber.Fill(row);
                        temp.CN_PaymentNumber.GUID_Project = temp.GUID_ProjectKey;
                        temp.CN_PaymentNumber.GUID_BGCode = temp.GUID_BGCode;

                        temp.GUID_Person = main.GUID_Person;
                        temp.GUID_WL_Main = main.GUID;
                        temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
                        main.WL_Detail.Add(temp);
                    }
                }
            }
            this.BusinessContext.WL_Main.AddObject(main);
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
                Guid value = jsonModel.m.Id(new WL_Main().ModelName());
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "应付单", data);
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
                OperatorLog.WriteLog(this.OperatorId, "应付单", ex.Message, data, false);
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
            WL_Main main = this.BusinessContext.WL_Main.FirstOrDefault(e => e.GUID == guid);
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
            WL_Main main = null; ; //new BX_Main();
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
        private WL_Main LoadBX_Main(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            WL_Main main = new WL_Main();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                WL_Detail temp = new WL_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new WL_Detail();
                        temp.CN_PaymentNumber = new CN_PaymentNumber();
                        temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                        temp.CN_PaymentNumber.Fill(row);

                        temp.Fill(row);
                        temp.GUID_Person = main.GUID_Person;
                        temp.GUID_WL_Main = main.GUID;
                        temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
                        main.WL_Detail.Add(temp);
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
        private List<ValidationResult> VerifyResult_WL_Detail(WL_Detail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            WL_Detail item = data;
            /// <summary>
            /// 明细表字段验证
            /// </summary>
            #region 明细表字段验证


            ////预算科目的GUID
            //if (item.GUID_BGCode.IsNullOrEmpty())
            //{
            //    str = "明细预算科目 字段为必输项!";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_BGCode.GetType(), item.GUID_BGCode.ToString(), out g) == false)
            //    {
            //        str = "明细预算科目格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}
            //报销金额
            if (item.Total_WL.ToString() == "")
            {
                str = "明细往来金额 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.Total_WL.GetType(), item.Total_WL.ToString(), out g) == false)
                {
                    str = "明细往来金额格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {
                    if (double.Parse(g.ToString()) == 0F)
                    {
                        str = "明细往来金额不能为零！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //摘要

            if (string.IsNullOrEmpty(item.ActionMemo))
            {
                str = "明细摘要 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.ActionMemo.GetType(), item.ActionMemo, out g) == false)
                {
                    str = "明细摘要格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }
            //项目GUID
            if (item.GUID_ProjectKey.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_ProjectKey.GetType(), item.GUID_ProjectKey.ToString(), out g) == false)
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
                //var vf_pn = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex);
                var vf_pn = base.VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex, item.WL_Main.GUID_DW);
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
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_WL_Main(WL_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            WL_Main mModel = data;
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
                str = "附单据数 格式不正确！";
                resultList.Add(new ValidationResult("", str));

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

            //报销人GUID
            if (mModel.GUID_Person.IsNullOrEmpty())
            {
                str = "借款人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "借款人格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //报销人部门




            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "报销部门 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "报销部门格式不正确！";
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
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            WL_Main model = (WL_Main)data;
            //主Model验证
            var vf_main = VerifyResult_WL_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<WL_Detail> detailList = new List<WL_Detail>();
            foreach (WL_Detail item in model.WL_Detail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (WL_Detail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_WL_Detail(item, rowIndex);
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
            WL_Main bxMain = new WL_Main();
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
                //if (Common.ConvertFunction.TryParse(mModel.GUID.GetType(), mModel.GUID.ToString(), out g) == false)
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
            WL_Main model = (WL_Main)data;
            WL_Main orgModel = this.BusinessContext.WL_Main.Include("WL_Detail").FirstOrDefault(e => e.GUID == model.GUID);
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
            var vf_main = VerifyResult_WL_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<WL_Detail> detailList = new List<WL_Detail>();
            foreach (WL_Detail item in model.WL_Detail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (WL_Detail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_WL_Detail(item, rowIndex);
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
