using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.薪酬设置
{
    public class 工资项目设置 : BaseDocument
    {
        public 工资项目设置() : base() { }
        public 工资项目设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SA_Item role = new SA_Item();
            role.Fill(jsonModel.m);
            role.GUID = Guid.NewGuid();

            this.InfrastructureContext.SA_Item.AddObject(role);
            this.InfrastructureContext.SaveChanges();
            return role.GUID;
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
            SA_Item role = new SA_Item();
            JsonAttributeModel id = jsonModel.m.IdAttribute(role.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            role = this.InfrastructureContext.SA_Item.FirstOrDefault(e => e.GUID == g);
            role.Fill(jsonModel.m);

            this.InfrastructureContext.ModifyConfirm(role);
            this.InfrastructureContext.SaveChanges();
            return role.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SA_Item role = this.InfrastructureContext.SA_Item.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(role);
            InfrastructureContext.SaveChanges();
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// 返回一个空的格式数据
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            JsonModel jmodel = new JsonModel();
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
                Guid value = jsonModel.m.Id(new SA_Item().ModelName());
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

        #region 工资项目设置

        #region 角色设置验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SA_Item main = null;
            switch (status)
            {
                case "1":
                    main = LoadMain(jsonModel);
                    vResult = InsertVerify(main);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2":
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3":
                    Guid value = jsonModel.m.Id(new SA_Item().ModelName());
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
        private SA_Item LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SA_Item main = new SA_Item();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SA_Item data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SA_Item ssKey = this.InfrastructureContext.SA_Item.FirstOrDefault(e => e.ItemKey == data.ItemKey);
            //根据返回来的Key值查找GUID,针对修改
            var rGUID = Guid.Empty;
            if (ssKey != null)
            {
                rGUID = ssKey.GUID;
            }
            SA_Item mModel = data;
            object g;

            #region 主表字段验证
            if (string.IsNullOrEmpty(mModel.ItemKey + "".Trim()))
            {
                str = "工资项目编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (string.IsNullOrEmpty(mModel.ItemName + "".Trim()))
            {
                str = "工资项目名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (rGUID != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.ItemKey == data.ItemKey)
                    {
                        str = "工资项目编号 不能重复！";
                        resultList.Add(new ValidationResult("", str));
                    }
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
            SA_Item model = (SA_Item)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
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
            SA_Item ssItem = this.InfrastructureContext.SA_Item.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 主表字段验证

            //报销单GUID
            if (ssItem == null)
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }

            //人员工资默认值设置SA_PersonItemSet
            var saPersonItemSetEnt = this.InfrastructureContext.SA_PersonItemSet.FirstOrDefault(e => e.Guid_SA_Item == guid);
            if (saPersonItemSetEnt != null)
            {
                str = "该工资项目在人员默认值设置中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //工资计划执行明细SA_PlanActionDetail
            var saPlanActionEnt = this.BusinessContext.SA_PlanActionDetail.FirstOrDefault(e => e.GUID_Item == guid);
            if (saPlanActionEnt != null)
            {
                str = "该工资项目在工资计划执行明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //工资计划项目SA_PlanItem
            var saPlanItemEnt = this.InfrastructureContext.SA_PlanItem.FirstOrDefault(e => e.GUID_Item == guid);
            if (saPlanItemEnt != null)
            {
                str = "该工资项目在工资计划项目中已经被引用！不能删除！";
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
            SA_Item model = (SA_Item)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;
        }

        #endregion


        #endregion

    }

}
