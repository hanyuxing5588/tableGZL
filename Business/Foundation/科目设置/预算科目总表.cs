using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;

namespace Business.Foundation.科目设置
{
    public class 预算科目总表 : BaseDocument
    {
        public 预算科目总表() : base() { }
        public 预算科目总表(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_BGCode obj = new SS_BGCode();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_BGCode.AddObject(obj);
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
            SS_BGCode obj = new SS_BGCode();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.GUID == g);
            obj.Fill(jsonModel.m);
            //修改时可能修改父项，把父项修改为空值的情况
            SS_BGCode tempobj = new SS_BGCode();
            tempobj.Fill(jsonModel.m);
            if (tempobj.PGUID == null)
            {
                obj.PGUID = null;
            }
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
            SS_BGCode obj = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.GUID == guid);
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
            var details = this.InfrastructureContext.SS_BGCodeView.OrderBy(e => e.BGCodeKey).ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_BGCodeView detail in details)
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
                Guid value = jsonModel.m.Id(new SS_BGCode().ModelName());
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


        #region 预算科目总表验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_BGCode main = null;
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
                    Guid value = jsonModel.m.Id(new SS_BGCode().ModelName());
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
        private SS_BGCode LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_BGCode main = new SS_BGCode();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_BGCode data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //返回一条指定的数据
            SS_BGCode ssKey = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.BGCodeKey == data.BGCodeKey);
            //根据返回来的Key值查找GUID
            var dwGuid = Guid.Empty;
            if (ssKey != null)
            {
                dwGuid = ssKey.GUID;
            }
            SS_BGCode mModel = data;
            object g;

            #region   主表字段验证

            if (mModel.BGCodeKey == "")
            {
                str = "科目编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.BGCodeName == "")
            {
                str = "科目名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.BeginYear == null)
            {
                str = "开始年度 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.GUID_EconomyClass.IsNullOrEmpty())
            {
                str = "经济分类名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.StopYear != null)
            {
                //var BeginY = (DateTime)mModel.BeginYear;
                //var StopY = (DateTime)mModel.StopYear;
                //结束年度不能小于开始年度
                if (mModel.StopYear < mModel.BeginYear)
                {
                    str = "结束年度不能小于开始年度！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            //修改数据的GUID,不走
            //如果修改时同一条数据的话，GUID相同可以保存，如果修改的不是同一条数据，而科目编码相同，则不能保存，应提示信息
            if (dwGuid != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.BGCodeKey == ssKey.BGCodeKey)
                    {
                        str = "科目编号 不能重复！";
                        resultList.Add(new ValidationResult("", str));
                    }
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
            SS_BGCode model = (SS_BGCode)data;
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
            SS_BGCode ssBGCode;
            ssBGCode = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 删除验证字段

            //报销单GUID
            if (ssBGCode.GUID == null || ssBGCode.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算控制明细BG_ControlDetail验证
            BG_ControlDetail bgControlDetailEnt = this.BusinessContext.BG_ControlDetail.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (bgControlDetailEnt != null)
            {
                str = "该预算科目在预算控制明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算编制初始值明细BG_DefaultDetail验证
            BG_DefaultDetail bgDefaultDetailEnt = this.BusinessContext.BG_DefaultDetail.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (bgDefaultDetailEnt != null)
            {
                str = "该预算科目在预算编制初始明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算科目明细BG_Detail
            BG_Detail bgDetailEnt = this.BusinessContext.BG_Detail.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (bgDetailEnt != null)
            {
                str = "该预算科目在预算明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算编制科目设置BG_SetupBGCode验证
            BG_SetupBGCode bgSetupBGCodeEnt = this.InfrastructureContext.BG_SetupBGCode.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (bgSetupBGCodeEnt != null)
            {
                str = "该预算科目在预算编制科目中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //报销单明细BX_Default验证
            BX_Detail bxDefaultEnt = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (bxDefaultEnt != null)
            {
                str = "该预算科目在报销单明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //财政支付码CN_PaymentNumber验证
            CN_PaymentNumber cnPaymentNumberEnt = this.BusinessContext.CN_PaymentNumber.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (cnPaymentNumberEnt != null)
            {
                str = "该预算科目在财政支付码中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收入明细SR_DetailView验证
            SR_Detail srDetailEnt = this.BusinessContext.SR_Detail.FirstOrDefault(e => e.GUID_BGCode == guid);
            if (srDetailEnt != null)
            {
                str = "该预算科目在收入明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //往来明细WL_Detail验证
            WL_Detail wlDetailEnt = this.BusinessContext.WL_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            if (wlDetailEnt != null)
            {
                str = "该预算科目在往来明细中已经被引用！不能删除！";
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
            SS_BGCode model = (SS_BGCode)data;
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
