using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;

namespace Business.Foundation.其他设置
{
    public class 帐套字表设置 : BaseDocument
    {
        public 帐套字表设置() : base() { }
        public 帐套字表设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 默认初始值   --目前没有用到--
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            //也是默认返回的数据列表

            var jmodel = new JsonModel();
            return jmodel;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            AccountDetail obj = new AccountDetail();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.AccountDetails.AddObject(obj);
            this.InfrastructureContext.SaveChanges();
            return obj.GUID;
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
            AccountDetail obj = new AccountDetail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.AccountDetails.FirstOrDefault(e => e.GUID == g);
            obj.Fill(jsonModel.m);

            this.InfrastructureContext.ModifyConfirm(obj);
            this.InfrastructureContext.SaveChanges();
            return obj.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            AccountDetail obj = this.InfrastructureContext.AccountDetails.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(obj);
            InfrastructureContext.SaveChanges();
        }

        /// <summary>
        /// 初始化时返回一个空的jsonModel
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            //todo 列表数据返回
            var jmodel = new JsonModel();
            return jmodel;
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
                Guid value = jsonModel.m.Id(new AccountDetail().ModelName());
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
                        //result = this.New();
                        result = this.Retrieve(value);
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

        #region 帐套字表设置验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            AccountDetailView main = null;
            switch (status)
            {
                case "1":
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2":
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);   //修改
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3":
                    Guid value = jsonModel.m.Id(new AccountDetail().ModelName());
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
        private AccountDetailView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            AccountDetailView main = new AccountDetailView();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            AccountDetailView model = (AccountDetailView)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(AccountDetailView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //返回一条指定的数据
            AccountDetailView ssKey = this.InfrastructureContext.AccountDetailViews.FirstOrDefault(e => e.accountkey == data.accountkey);
            //根据返回来的Key值查找GUID
            var acGuid = Guid.Empty;
            if (ssKey != null)
            {
                acGuid = ssKey.GUID;
            }
            AccountDetailView mModel = data;
            object g;

            #region   主表字段验证
            if (mModel.accountkey == "" || mModel.accountkey == null)
            {
                str = "帐套编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.accountname == "" || mModel.accountname == null)
            {
                str = "帐套名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.FiscalYear == null || mModel.FiscalYear == 0)
            {
                str = "会计年度 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.ExteriorYear == null)
            {
                str = "外接数据库年份 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            //修改数据的GUID,不走
            if (acGuid != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.accountkey == ssKey.accountkey)
                    {
                        str = "帐套编号 不能重复！";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
            }
            return resultList;

            #endregion
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
            AccountDetail acMain;
            acMain = this.InfrastructureContext.AccountDetails.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 主字段验证
            
            //报销单GUID
            if (acMain.GUID == null || acMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //会计期间验证
            CW_Period cw_PeriodEnt = this.BusinessContext.CW_Period.FirstOrDefault(e => e.GUID_AccountDetail == guid);
            if (cw_PeriodEnt != null)
            {
                str = "该帐套子表在会计期间中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //会计凭证验证
            CW_PZMain cw_PZMainEnt = this.BusinessContext.CW_PZMain.FirstOrDefault(e => e.GUID_AccountDetail == guid);
            if (cw_PZMainEnt != null)
            {
                str = "该帐套子表在会计凭证中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //基础档案对照主表验证
            SS_ComparisonMainView ss_ComparisonMainEnt = this.BusinessContext.SS_ComparisonMainView.FirstOrDefault(e => e.GUID_AccountDetail == guid);
            if (ss_ComparisonMainEnt != null)
            {
                str = "该帐套子表在基础档案对照主表中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //凭证模板主表验证
            SS_PZTemplateMainView ss_PZTemplateMainEnt = this.BusinessContext.SS_PZTemplateMainView.FirstOrDefault(e => e.GUID_AccountDetail == guid);
            if (ss_PZTemplateMainEnt != null)
            {
                str = "该帐套子表在凭证模板主表中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }

            #endregion

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
            AccountDetailView model = (AccountDetailView)data;
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
