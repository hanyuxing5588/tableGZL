using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation
{   
    public class 往来单位档案 : BaseDocument
    {
        public 往来单位档案() : base() { }
        public 往来单位档案(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                //SS_Customer model = new SS_Customer();
               // model.FillDefault(this, this.OperatorId);
               // jmodel.m = model.Pick();

                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
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
            SS_Customer customer = new SS_Customer();
            customer.Fill(jsonModel.m);
            customer.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_Customer.AddObject(customer);
            this.InfrastructureContext.SaveChanges();
            return customer.GUID;
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
            SS_Customer customer = new SS_Customer();
            JsonAttributeModel id = jsonModel.m.IdAttribute(customer.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            customer = this.InfrastructureContext.SS_Customer.FirstOrDefault(e => e.GUID == g);
            customer.Fill(jsonModel.m);

            this.InfrastructureContext.ModifyConfirm(customer);
            this.InfrastructureContext.SaveChanges();
            return customer.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SS_Customer role = this.InfrastructureContext.SS_Customer.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(role);
            InfrastructureContext.SaveChanges();
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
                Guid value = jsonModel.m.Id(new SS_Customer().ModelName());
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
                SS_Customer main = this.InfrastructureContext.SS_Customer.FirstOrDefault(e => e.GUID == guid);
                if (main != null)
                {
                    jmodel.m = main.Pick();                           
                   
                }
                //查询所有信息
                IQueryable<SS_Customer> q = this.InfrastructureContext.SS_Customer.OrderBy(e => e.CustomerKey);
                List<SS_Customer> details = q == null ? new List<SS_Customer>() : q.ToList();
                if (details.Count > 0)
                {
                    JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                    jmodel.d.Add(jgm);
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
            SS_Customer main = null; ; //new BX_Main();
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
                    Guid value = jsonModel.m.Id(new SS_Customer().ModelName());
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
        private SS_Customer LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_Customer main = new SS_Customer();
            main.Fill(jsonModel.m);
            return main;
        }       
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_Customer data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            SS_Customer mModel = data;
            object g;

            #region   主表字段验证

            //单位编号
            if (string.IsNullOrEmpty(mModel.CustomerKey))
            {
                str = "单位编号 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.CustomerKey.GetType(), mModel.CustomerKey.ToString(), out g) == false)
                {
                    str = "单位编号 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            //单位名称
            if (string.IsNullOrEmpty(mModel.CustomerName))
            {
                str = "单位名称 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.CustomerName.GetType(), mModel.CustomerKey.ToString(), out g) == false)
                {
                    str = "单位名称 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //开户银行
            if (string.IsNullOrEmpty(mModel.CustomerBankName))
            {
                str = "开户银行 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.CustomerBankName.GetType(), mModel.CustomerKey.ToString(), out g) == false)
                {
                    str = "开户银行 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //银行账号
            if (string.IsNullOrEmpty(mModel.CustomerBankNumber))
            {
                str = "银行账号 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.CustomerBankNumber.GetType(), mModel.CustomerKey.ToString(), out g) == false)
                {
                    str = "银行账号 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            
            return resultList;

            #endregion
        }
        /// <summary>
        ///判断是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool IsExist(string key, string name)
        {
            SS_Customer model = this.InfrastructureContext.SS_Customer.FirstOrDefault(e=>e.CustomerKey==key && e.CustomerName==name);
            if (model != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            SS_Customer model = (SS_Customer)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //判断是否存在 
            if (model != null && !string.IsNullOrEmpty(model.CustomerName) && !string.IsNullOrEmpty(model.CustomerKey))
                if (IsExist(model.CustomerKey,model.CustomerName))
                {                  
                    ValidationResult vr = new ValidationResult("", "已经存在此客户信息！");                    
                    result._validation.Add(vr);
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
            SS_Customer bxMain = new SS_Customer();
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
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g)==false)
                {
                    str = "此单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
            //判断在其他单据中是否存在此客户，如果存在此客户信息部能删除此信息
            var list = CheckDocCustomer(guid);
            if (list != null && list.Count > 0)
            {
                resultList.AddRange(list);
            }
            if (resultList.Count > 0)
            {
                result._validation = resultList;
            }
            return result;
        }
        /// <summary>
        /// 检验单据是否存在客户信息
        /// </summary>
        /// <param name="customerGUID"></param>
        /// <returns></returns>
        private List<ValidationResult> CheckDocCustomer(Guid customerGUID)
        {            
              
            List<ValidationResult> list = new List<ValidationResult>();
            //报销
            string msg = string.Empty;
            var bx_model = this.BusinessContext.BX_Detail.FirstOrDefault(e=>e.GUID_Cutomer==customerGUID);
            if (bx_model != null)
            {
                msg = "报销单中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            var sk_model=this.BusinessContext.SK_Main.FirstOrDefault(e=>e.GUID_Customer==customerGUID);
            if(sk_model!=null)
            {
                 msg = "收款中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            var wl_model = this.BusinessContext.WL_Detail.FirstOrDefault(e=>e.GUID_Cutomer==customerGUID);
            if (wl_model != null)
            {
                msg = "往来单中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            var sr_model = this.BusinessContext.SR_Detail.FirstOrDefault(e=>e.GUID_Cutomer==customerGUID);
            if (sr_model != null)
            {
                msg = "收入单中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            var cash_model = this.BusinessContext.CN_CashDetail.FirstOrDefault(e=>e.GUID_Cutomer==customerGUID);
            if (cash_model != null)
            {
                msg = "现金单中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            var jj_model = this.BusinessContext.JJ_Detail.FirstOrDefault(e => e.GUID_Cutomer == customerGUID);
            if (cash_model != null)
            {
                msg = "基金单中存在此客户信息，不能删除！";
                list.Add(new ValidationResult("", msg));
                return list;
            }
            return list;
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
            SS_Customer model = (SS_Customer)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }            
            return result;
        }
        #endregion

    }   
}
