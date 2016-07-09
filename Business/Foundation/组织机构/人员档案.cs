using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using System.Text.RegularExpressions;

namespace Business.Foundation.组织机构
{
    public class 人员档案 : BaseDocument
    {
        public 人员档案() : base() { }
        public 人员档案(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Person obj = new SS_Person();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_Person.AddObject(obj);
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
            SS_Person obj = new SS_Person();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == g);
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
            SS_Person obj = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(obj);
            InfrastructureContext.SaveChanges();
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            //todo 列表数据返回
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_PersonView.OrderBy(e => e.PersonKey).ToList();//进行排序
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_PersonView detail in details)
                {
                    List<JsonAttributeModel> picker = detail.Pick();
                    jgm.r.Add(picker);
                }
            }
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
                Guid value = jsonModel.m.Id(new SS_Person().ModelName());
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

        #region 人员档案验证

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
            SS_PersonView main = null; ; //new BX_Main();
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
                    Guid value = jsonModel.m.Id(new SS_Person().ModelName());
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
        private SS_PersonView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_PersonView main = new SS_PersonView();
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
            SS_PersonView model = (SS_PersonView)data;
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
        private List<ValidationResult> VerifyResultMain(SS_PersonView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SS_PersonView ssKey = this.InfrastructureContext.SS_PersonView.FirstOrDefault(e => e.PersonKey == data.PersonKey);
            //根据返回来的Key值查找GUID， 针对修改
            var perGUID = Guid.Empty;
            if (ssKey != null)
            {
                perGUID = ssKey.GUID;
            }
            SS_PersonView mModel = data;
            object g;

            #region   主表字段验证
            if (mModel.PersonKey == "")
            {
                str = "人员编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.PersonName == "")
            {
                str = "人员名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.DepartmentKey == "")
            {
                str = "部门编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.DepartmentName == "")
            {
                str = "部门名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.DWKey == "")
            {
                str = "单位编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.DWName == "")
            {
                str = "单位名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.GUID_PersonType.IsNullOrEmpty())
            {
                str = "人员类别 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_PersonType.GetType(), mModel.GUID_PersonType.ToString(), out g) == false)
                {
                    str = "人员类别 格式不正确!";
                    resultList.Add(new ValidationResult("", str));
                }
            }

            if (mModel.BankCardNo != "")
            {
                //var bankCardno = @"/^\d{16}|\d{19}$/";
                //if (!Regex.IsMatch(mModel.BankCardNo, bankCardno))
                //{
                //    str = "银行账号只能是16或者19位数字，请输入正确账号！";
                //    resultList.Add(new ValidationResult("", str));
                //}
            }

            if (mModel.IDCard == "")
            {
                str = "证件号码 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (mModel.IDCardType == "")
                {
                    str = "请选择证件类型！";
                    resultList.Add(new ValidationResult("", str));
                }
                else
                {
                    //判断选择的证件类型与输入的证件号码是否匹配

                    switch (mModel.IDCardType)
                    {
                        case "01":  //身份证

                            var isIDCard = @"(^\d{18}$)|(^\d{15}$)";
                            if (!Regex.IsMatch(mModel.IDCard, isIDCard))
                            {
                                str = "证件号码 格式不正确！";
                                //resultList.Add(new ValidationResult("", str));
                            }
                            break;
                        case "02":   //军官证

                            var IDOff = @"^[0-9]*$";
                            if (!Regex.IsMatch(mModel.IDCard, IDOff))
                            {
                                str = "请输入数字！";
                                //resultList.Add(new ValidationResult("", ""));
                            }
                            break;
                        case "03":   //护照
                            var IDProtection = @"^[0-9]*$";
                            if (!Regex.IsMatch(mModel.IDCard, IDProtection))
                            {
                                str = "请输入数字！";
                                //resultList.Add(new ValidationResult("", ""));
                            }
                            break;
                        case "04":   //港澳通行证

                            var IDga = @"^[0-9]*$";
                            if (!Regex.IsMatch(mModel.IDCard, IDga))
                            {
                                str = "请输入数字！";
                                //resultList.Add(new ValidationResult("", ""));
                            }
                            break;
                        case "05":  //其他
                            var IDqt = @"^[0-9]*$";
                            if (!Regex.IsMatch(mModel.IDCard, IDqt))
                            {
                                str = "请输入数字！";
                                //resultList.Add(new ValidationResult("", str));
                            }
                            break;
                    }
                }
            }
            if (perGUID != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.DepartmentKey == ssKey.DepartmentKey)
                    {
                        str = "部门编号 不能重复！";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
            }

            #endregion

            return resultList;
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
            SS_Person bxMain = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 主表字段验证

            //报销单GUID
            if (bxMain == null)
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }

            //外聘人员SS_InvitePerson验证
            var invidPerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == guid);
            if (invidPerson != null)
            {
                str = "该人员已经在外聘人员中存在,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //报销 建立约束关系 BX_Main
            var bxEnt = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (bxEnt != null)
            {
                str = "该人员在报销单中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            var bxDetailEnt = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_Person == guid);
            if (bxDetailEnt != null)
            {
                str = "该人员报销明细中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //往来WL_Main验证
            var wlEnt = this.BusinessContext.WL_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (wlEnt != null)
            {
                str = "该人员在往来人员中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //往来WL_Detail
            var wlDetailEnt = this.BusinessContext.WL_Detail.FirstOrDefault(e => e.GUID_Person == guid);
            if (wlDetailEnt != null)
            {
                str = "该人员在往来明细中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算BG_Main
            var bgMainEnt = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (bgMainEnt != null)
            {
                str = "该人员在预算单中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收入SR_Main
            var srMainEnt = this.BusinessContext.SR_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (srMainEnt != null)
            {
                str = "该人员在收入单中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收入明细SR_Detail
            var srDetailEnt = this.BusinessContext.SR_Detail.FirstOrDefault(e => e.GUID_Person == guid);
            if (srDetailEnt != null)
            {
                str = "该人员在收入明细中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //出纳CN_Main
            var cnMainEnt = this.BusinessContext.CN_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (cnMainEnt != null)
            {
                str = "该人员在出纳单中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //出纳明细CN_Detail
            var cnDetailEnt = this.BusinessContext.CN_Detail.FirstOrDefault(e => e.GUID_Person == guid);
            if (cnDetailEnt != null)
            {
                str = "该人员在出纳明细中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收款凭单SK_Main
            var skMainEnt = this.BusinessContext.SK_Main.FirstOrDefault(e => e.GUID_Person == guid);
            if (skMainEnt != null)
            {
                str = "该人员在收款凭单中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //工资计划执行明细SA_PlanActionDetail
            var saPlanActionEnt = this.BusinessContext.SA_PlanActionDetail.FirstOrDefault(e => e.GUID_Person == guid);
            if (saPlanActionEnt != null)
            {
                str = "该人员在工资计划执行明细中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //人员工资默认值设置SA_PersonItemSet
            var saPersonItemSetEnt = this.InfrastructureContext.SA_PersonItemSet.FirstOrDefault(e => e.GUid_SS_Person == guid);
            if (saPlanActionEnt != null)
            {
                str = "该人员在人员工资默认值设置中已经被引用,不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //工资计划人员设置表
            var saPlanPsersonSetEnt = this.InfrastructureContext.SA_PlanPersonSet.FirstOrDefault(e => e.GUID_SS_Person == guid);
            if (saPlanPsersonSetEnt != null)
            {
                str = "该人员在工资计划人员设置中已经被引用！不能删除！";
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
            SS_PersonView model = (SS_PersonView)data;
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
