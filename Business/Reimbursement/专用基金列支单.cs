using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Platform.Flow.Run;
using Business.CommonModule;
using Infrastructure;
using BusinessModel;

namespace Business.Reimbursement
{   
    public class 专用基金列支单 : BaseDocument
    {

        public 专用基金列支单() : base() { }
        public 专用基金列支单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                JJ_MainView model = new JJ_MainView();
                model.FillDefault(this, this.OperatorId);
                jmodel.m = model.Pick();
                JJ_DetailView detail = new JJ_DetailView();
                detail.FillDetailDefault(this,this.OperatorId,this.ModelUrl);

                List<JsonAttributeModel> picker = detail.Pick();
                CN_PaymentNumberView payment = new CN_PaymentNumberView();
                payment.FillCN_PaymentNumberDefault(this);
                picker.AddRange(payment.Pick());

                jmodel.m.AddRange(picker);
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
                JJ_MainView main = this.BusinessContext.JJ_MainView.FirstOrDefault(e => e.GUID == guid);
                if (main != null)
                {
                    jmodel.m = main.Pick();
                    JJ_DetailView detail = this.BusinessContext.JJ_DetailView.FirstOrDefault(e=>e.GUID_JJ_Main==main.GUID);
                    if (detail != null)
                    {
                        //明细信息
                        jmodel.m.AddRange(detail.Pick());
                        //支付码
                        if (detail.GUID_PaymentNumber != null)
                        {
                            CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e => e.GUID == detail.GUID_PaymentNumber);
                            if (payment != null)
                            {
                                jmodel.m.AddRange(payment.Pick());
                            }
                        }
                    }
                    //支票信息
                   string mainModel=main.ModelName();
                   var checkModel=GetCheckModel(guid,mainModel);
                   if (checkModel != null)
                   {
                       jmodel.m.AddRange(checkModel.Pick());   
                   }

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
        /// 获取支票信息
        /// </summary>
        /// <param name="mainGUID"></param>
        /// <param name="mainModelName"></param>
        /// <returns></returns>
        private CN_Check GetCheckModel(Guid mainGUID, string mainModelName)
        {
            int classid = CommonFuntion.GetClassId(mainModelName);
            CN_CheckDrawMainView drawMain = this.BusinessContext.CN_CheckDrawMainView.FirstOrDefault(e=>e.GUID_Doc==mainGUID && e.ClassID==classid);
            if (drawMain != null)
            {
                if (!drawMain.GUID_Check.IsNullOrEmpty())
                {
                    var checkModel = this.BusinessContext.CN_Check.FirstOrDefault(e => e.GUID == drawMain.GUID_Check);
                    return checkModel;
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
            JJ_Main main = this.BusinessContext.JJ_Main.FirstOrDefault(e => e.GUID == guid);          

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
            JJ_Main main = new JJ_Main(); 
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.JJ_Main.FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate==null?DateTime.MinValue:(DateTime)main.DocDate;
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
             main.ResetDefault(this,this.OperatorId);
            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            //if (IsDateChange(orgDateTime, (DateTime)main.DocDate))
            //{
            //    main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, (Guid)main.GUID_YWType, main.DocDate.ToString());
            //}
           
            List<JJ_Detail> detailList = new List<JJ_Detail>();
            foreach (JJ_Detail detail in main.JJ_Detail)
            {
                detailList.Add(detail);
            }
            
            foreach (JJ_Detail detail in detailList)
            {                                     
                detail.FillDefault(this, this.OperatorId);
                detail.Fill(jsonModel.m);
                detail.GUID_Person = main.GUID_Person;
                detail.GUID_Department = main.GUID_Department;
                detail.FeeMemo = main.DocMemo;
                detail.ResetDefault(this, this.OperatorId);
      
                detail.CN_PaymentNumber.Fill(jsonModel.m);
                detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;

                detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
                this.BusinessContext.ModifyConfirm(detail);
               
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
            JJ_Main main = new JJ_Main();           
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            main.SubmitDate = DateTime.Now;
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW,(Guid) main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");
            JJ_Detail detail = new JJ_Detail();
            detail.FillDetailDefault(this,this.OperatorId);
            detail.Fill(jsonModel.m);
            detail.GUID = Guid.NewGuid();
            detail.GUID_JJ_Main = main.GUID;
            detail.FeeMemo = main.DocMemo;
           
            //支付码
            detail.CN_PaymentNumber = new CN_PaymentNumber();
            detail.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            detail.CN_PaymentNumber.Fill(jsonModel.m);
            detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;

            detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
            main.JJ_Detail.Add(detail);


            this.BusinessContext.JJ_Main.AddObject(main);
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
                Guid value = jsonModel.m.Id(new JJ_Main().ModelName());
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "专用基金列支单", data);
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
                OperatorLog.WriteLog(this.OperatorId, "专用基金列支单", ex.Message, data, false);
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
            JJ_Main main = this.BusinessContext.JJ_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                main.DocState = (int)docState;
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
            return new 历史记录(this.OperatorId,this.ModelUrl).History(conditions);
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
            JJ_Main main = null; ; //new BX_Main();
            switch (status)
            {
                case "1": //新建
                    main = LoadSK_Main(jsonModel);//.Fill(jsonModel.m);
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
                    main = LoadSK_Main(jsonModel);
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
        private JJ_Main LoadSK_Main(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            JJ_Main main = new JJ_Main();
            main.Fill(jsonModel.m);
            //明细
            JJ_Detail detail = new JJ_Detail();
            detail.Fill(jsonModel.m);
            main.JJ_Detail.Add(detail);
            //支付码
            detail.CN_PaymentNumber = new CN_PaymentNumber();
            detail.CN_PaymentNumber.FillDefault(this, Guid.Empty);
            detail.CN_PaymentNumber.Fill(jsonModel.m);

            return main;
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
            if (data.GUID_BGResource.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGResource.GetType(), data.GUID_BGResource.ToString(), out g) == false)
            {
                str = "预算来源格式不能为空！";
                resultList.Add(new ValidationResult("", str));

            }

            #endregion

            return resultList;
        }
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_JJ_Main(JJ_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            JJ_Main mModel = data;
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
                str = "经办人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "经办人 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //报销人部门
            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "部门名称 字段为必输项!";
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
            JJ_Main model = (JJ_Main)data;
            //主Model验证
            var vf_main = VerifyResult_JJ_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //验证码验证
            if (model.JJ_Detail != null)
            {
                List<JJ_Detail> detailList=new List<JJ_Detail> ();
                foreach(JJ_Detail item in model.JJ_Detail)
                {
                    detailList.Add(item);
                }
                if (detailList.Count > 0)
                {
                    foreach (JJ_Detail item in detailList)
                    {
                        if (item.CN_PaymentNumber != null)
                        {
                            var vf_payment = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, 0);
                            if (vf_payment != null && vf_payment.Count > 0)
                            {
                                result._validation.AddRange(vf_payment);
                            }
                        }
                    }
                }
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
            JJ_Main bxMain = new JJ_Main();
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

            JJ_Main main = this.BusinessContext.JJ_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == 9)
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
            JJ_Main model = (JJ_Main)data;
            JJ_Main orgModel = this.BusinessContext.JJ_Main.FirstOrDefault(e => e.GUID == model.GUID);
            
            //流程验证
            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //作废           
            if (orgModel != null && orgModel.DocState == 9 && model.DocState != (int)Business.Common.EnumType.EnumDocState.RcoverState)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //主Model验证
            var vf_main = VerifyResult_JJ_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }

            //验证码验证
            if (model.JJ_Detail != null)
            {
                foreach (JJ_Detail detail in model.JJ_Detail)
                {
                    var vf_payment = VerifyResult_CN_PaymentNumber(detail.CN_PaymentNumber, 0);
                    if (vf_payment != null && vf_payment.Count > 0)
                    {
                        result._validation.AddRange(vf_payment);
                    }
                }
            } //验证码验证
            if (model.JJ_Detail != null)
            {
                List<JJ_Detail> detailList = new List<JJ_Detail>();
                foreach (JJ_Detail item in model.JJ_Detail)
                {
                    detailList.Add(item);
                }
                if (detailList.Count > 0)
                {
                    foreach (JJ_Detail item in detailList)
                    {
                        if (item.CN_PaymentNumber != null)
                        {
                            var vf_payment = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, 0);
                            if (vf_payment != null && vf_payment.Count > 0)
                            {
                                result._validation.AddRange(vf_payment);
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion

    }
}
