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
    public class 公务卡汇总报销单 : BXDocument
    {
        public 公务卡汇总报销单() : base() { }
        public 公务卡汇总报销单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
                
        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                BX_CollectMainView model = new BX_CollectMainView();
                model.FillDefault(this, this.OperatorId);
                jmodel.m = model.Pick();

                BX_CollectDetailView dModel = new BX_CollectDetailView();
                
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.f.Add(fjgm);

                List<JsonAttributeModel> picker = dModel.Pick();
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
                    BX_CollectMainView main = this.BusinessContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == guid);

                    if (main != null)
                    {
                        jmodel.m = main.Pick();


                        IQueryable<BX_CollectDetailView> q = this.BusinessContext.BX_CollectDetailView.Where(e => e.GUID_BXCOLLECTMain == guid).OrderBy(e => e.OrderNum);
                        List<BX_CollectDetailView> details = q == null ? new List<BX_CollectDetailView>() : q.ToList();
                        if (details.Count > 0)
                        {
                            JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                            jmodel.d.Add(jgm);
                            foreach (BX_CollectDetailView detail in details)
                            {
                                List<JsonAttributeModel> picker = detail.Pick();
                                var list = BX_Detail(detail);//报销明细
                                if (list != null && list.Count > 0)
                                {
                                    picker.AddRange(list);
                                }
                                jgm.r.Add(picker);
                            }
                        }
                        //明细中f 填充默认值

                        BX_CollectDetailView dModel = new BX_CollectDetailView();
                        dModel.FillDetailDefault<BX_CollectDetailView>(this, this.OperatorId);
                        JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                        jmodel.f.Add(fjgm);

                        List<JsonAttributeModel> fpicker = dModel.Pick();

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
        /// 返回实体
        /// </summary>
        /// <param name="guids">多个GUID用逗号隔开</param>
        /// <returns></returns>
        public override JsonModel Retrieve(string guids)
        {
            List<Guid> guidList = new List<Guid>();
            Guid g;
            if (guids.IndexOf(',') >= 0)
            {
                string[] strArr = guids.Split(',');
                foreach (string item in strArr)
                {
                    if (!string.IsNullOrEmpty(item) && Guid.TryParse(item, out g))
                    {
                        if (!guidList.Contains(g))
                        {
                            guidList.Add(g);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(guids) && Guid.TryParse(guids, out g))
                {
                    if (!guidList.Contains(g))
                    {
                        guidList.Add(g);
                    }
                }
            }

            JsonModel jmodel = new JsonModel();
            try
            {
                BX_CollectMain collectMian = new BX_CollectMain();

                List<BX_MainView> main = this.BusinessContext.BX_MainView.Where(e => e.GUID != null && guidList.Contains(e.GUID)).ToList();
                List<Guid> guidMainList = main.Select(e => e.GUID).ToList();
                if (main != null && main.Count > 0)
                {

                    List<BX_DetailView> details = this.BusinessContext.BX_DetailView.Where(e => guidMainList.Contains(e.GUID_BX_Main)).OrderBy(e => e.OrderNum).ToList();
                       
                        if (details.Count > 0)
                        {
                            BX_CollectDetail collectDetail = new BX_CollectDetail();
                            JsonGridModel jgm = new JsonGridModel(collectDetail.ModelName());
                            jmodel.d.Add(jgm);
                            foreach (BX_DetailView detail in details)
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

                        ////明细中f 填充默认值

                        //BX_CollectDetailView dModel = new BX_CollectDetailView();
                        //dModel.FillDetailDefault<BX_CollectDetailView>(this, this.OperatorId);
                        //JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                        //jmodel.f.Add(fjgm);

                        //List<JsonAttributeModel> fpicker = dModel.Pick();

                        //fjgm.r.Add(fpicker);
                    
                    jmodel.s = new JsonMessage("", "", "");
                    
                }
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
        /// 报销明细
        /// </summary>
        /// <param name="collectDetail"></param>
        /// <returns></returns>
        private List<JsonAttributeModel> BX_Detail(BX_CollectDetailView collectDetail)
        {
            List<JsonAttributeModel> list = new List<JsonAttributeModel>();
            if (collectDetail != null)
            {
                BX_DetailView detail = this.BusinessContext.BX_DetailView.FirstOrDefault(e=>e.GUID==collectDetail.GUID_BXDetail);
                if (detail != null)
                {
                    list.AddRange(detail.Pick());
                    CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e=>e.GUID==detail.GUID_PaymentNumber);
                    if (payment != null)
                    {
                        list.AddRange(payment.Pick());
                    }
                }
                return list;
            }
            return null;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            BX_CollectMain main = this.BusinessContext.BX_CollectMain.FirstOrDefault(e => e.GUID == guid);

            List<BX_CollectDetail> details = new List<BX_CollectDetail>();

            foreach (BX_CollectDetail item in main.BX_CollectDetail)
            {
                details.Add(item);
            }

            foreach (BX_CollectDetail item in details) { BusinessContext.DeleteConfirm(item); }

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
            BX_CollectMain main = new BX_CollectMain(); BX_CollectDetail tempdetail = new BX_CollectDetail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.BX_CollectMain.Include("BX_CollectDetail").FirstOrDefault(e => e.GUID == g);
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

            // this.BusinessContext.ModifyConfirm(main);

            string detailModelName = tempdetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BX_CollectDetail detail in main.BX_CollectDetail) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {
                List<BX_CollectDetail> detailList = new List<BX_CollectDetail>();
                foreach (BX_CollectDetail detail in main.BX_CollectDetail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (BX_CollectDetail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        orderNum++;
                        detail.OrderNum = Grid.r.IndexOf(row);
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);
                        
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    BX_Detail detail = new BX_Detail();
                    detail.Fill(row);
                    detail.ResetDefault(this, this.OperatorId);
                    orderNum++;
                    BX_CollectDetail newitem = new BX_CollectDetail();
                    newitem.FillDefault(this, this.OperatorId);
                    newitem.Fill(row);
                    newitem.OrderNum = Grid.r.IndexOf(row);
                    newitem.GUID_BXCOLLECTMain = main.GUID;
                    newitem.GUID_BXDetail = detail.GUID;
                    newitem.GUID_BXMain = detail.GUID_BX_Main;

                    main.BX_CollectDetail.Add(newitem);                   
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
            BX_CollectMain main = new BX_CollectMain();
            // main.GUID = Guid.Empty;  
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();

            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {                
                BX_CollectDetail collectTemp= new BX_CollectDetail();
                BX_Detail temp = new BX_Detail();
                string detailModelName = collectTemp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new BX_Detail();
                        temp.Fill(row);
                        orderNum++;
                        BX_CollectDetail collectDetail = new BX_CollectDetail();
                        collectDetail.FillDefault(this, this.OperatorId);
                        collectDetail.GUID_BXCOLLECTMain = main.GUID;
                        collectDetail.GUID_BXDetail = temp.GUID;
                        collectDetail.GUID_BXMain = temp.GUID_BX_Main;
                        collectDetail.OrderNum = Grid.r.IndexOf(row);                      
                        

                        main.BX_CollectDetail.Add(collectDetail);
                    }
                }             
            }
            this.BusinessContext.BX_CollectMain.AddObject(main);
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
                Guid value = jsonModel.m.Id(new BX_CollectMain().ModelName());
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
                    OperatorLog.WriteLog(this.OperatorId, value, status, "公务卡汇总报销单", data);
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
                OperatorLog.WriteLog(this.OperatorId, "公务卡汇总报销单", ex.Message, data, false);
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
            BX_CollectMain main = this.BusinessContext.BX_CollectMain.FirstOrDefault(e => e.GUID == guid);
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
        /// 参照记录
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
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
            BX_CollectMain main = null; ; //new BX_Main();
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
        private BX_CollectMain LoadBX_Main(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            BX_CollectMain main = new BX_CollectMain();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                
                BX_Detail temp = new BX_Detail();
                BX_CollectDetail collectTemp = new BX_CollectDetail();
                string detailModelName = collectTemp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new BX_Detail();
                        temp.Fill(row);
                        BX_CollectDetail collectDetail = new BX_CollectDetail();
                        collectDetail.GUID_BXCOLLECTMain = main.GUID;
                        collectDetail.GUID_BXDetail = temp.GUID;
                        collectDetail.GUID_BXMain = temp.GUID_BX_Main;

                        main.BX_CollectDetail.Add(collectDetail);
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
        private List<ValidationResult> VerifyResult_Bx_Detail(BX_CollectDetail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BX_CollectDetail item = data;
            /// <summary>
            /// 明细表字段验证


            /// </summary>
            #region 明细表字段验证


            ////明细主表GUID
            //if (item.GUID_BXMain.IsNullOrEmpty())
            //{
            //    str = "明细报销主表GUID不能为空!";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_BXMain.GetType(), item.GUID_BXMain.ToString(), out g) == false)
            //    {
            //        str = "明细报销主表GUID格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}
            ////报销明细GUID
            //if (item.GUID_BXDetail.IsNullOrEmpty())
            //{
            //    str = "明细报销明细GUID不能为空!";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_BXMain.GetType(), item.GUID_BXMain.ToString(), out g) == false)
            //    {
            //        str = "明细报销明细GUID格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}            

            #endregion         
            return resultList;
        }
        
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_BX_Main(BX_CollectMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BX_CollectMain mModel = data;
            object g;

            #region   主表字段验证

            //汇总日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "汇总日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "汇总日期 格式不正确！";
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

            //汇总人GUID
            if (mModel.GUID_Person.IsNullOrEmpty())
            {
                str = "汇总人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "汇总人格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //部门名称
            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "部门名称 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "部门名称格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //单位GUID
            if (mModel.GUID_DW.IsNullOrEmpty())
            {
                str = "单位名称 字段为必输项!";
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
            BX_CollectMain model = (BX_CollectMain)data;
            //主Model验证
            var vf_main = VerifyResult_BX_Main(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            List<BX_CollectDetail> bx_DetailList = new List<BX_CollectDetail>();
            foreach (BX_CollectDetail item in model.BX_CollectDetail)
            {
                bx_DetailList.Add(item);
            }
            if (bx_DetailList != null && bx_DetailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BX_CollectDetail item in bx_DetailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        result._validation.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                List<ValidationResult> list = new List<ValidationResult>();
                list.Add(new ValidationResult("", "请添加明细列表信息！"));
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
            BX_CollectMain bxMain = new BX_CollectMain();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                //if (Common.ConvertFunction.TryParse(mModel.GUID.GetType(), mModel.GUID.ToString(), out g) == false)
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

            BX_CollectMain main = this.BusinessContext.BX_CollectMain.FirstOrDefault(e => e.GUID == guid);
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
            BX_CollectMain model = (BX_CollectMain)data;
            BX_CollectMain orgModel = this.BusinessContext.BX_CollectMain.FirstOrDefault(e => e.GUID == model.GUID);
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
            
            //if (WorkFlowAPI.ExistProcess(model.GUID))
            //{
            //    List<ValidationResult> resultList = new List<ValidationResult>();
            //    resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
            //    result._validation = resultList;
            //    return result;
            //}
            //作废           
            if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
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
            List<BX_CollectDetail> bx_DetailList = new List<BX_CollectDetail>();
            foreach (BX_CollectDetail item in model.BX_CollectDetail)
            {
                bx_DetailList.Add(item);
            }
            if (bx_DetailList != null && bx_DetailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BX_CollectDetail item in bx_DetailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
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
