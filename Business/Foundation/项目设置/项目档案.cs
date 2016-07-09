using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;

namespace Business.Foundation.项目设置
{
    public class 项目档案 : BaseDocument
    {
        public 项目档案() : base() { }
        public 项目档案(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Project obj = new SS_Project();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_Project.AddObject(obj);
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
            SS_Project obj = new SS_Project();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.GUID == g);
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
            SS_Project obj = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.GUID == guid);
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
            var details = this.InfrastructureContext.SS_ProjectView.Where(e=>e.IsStop==false).OrderBy(e => e.ProjectKey).ToList();

            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_ProjectView detail in details)
                {
                    List<JsonAttributeModel> picker = detail.Pick();
                    jgm.r.Add(picker);
                }
            }
            return jmodel;
        }
        /// <summary>
        /// 根据类型返回数据
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonModel AllRetrieve(int type)
        {
            //todo 列表数据返回
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_ProjectView.OrderBy(e => e.ProjectKey).ToList();
            if (type == 0)
            {
                details = details.FindAll(e => e.IsStop == false);
            }
            else if (type == 1)
            {
                details = details.FindAll(e => e.IsStop == true);
            }
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_ProjectView detail in details)
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
                Guid value = jsonModel.m.Id(new SS_Project().ModelName());
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

        #region 项目分类验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_ProjectView main = null;
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
                    Guid value = jsonModel.m.Id(new SS_Project().ModelName());
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
        private SS_ProjectView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_ProjectView main = new SS_ProjectView();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_ProjectView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SS_Project ssKey = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.ProjectKey == data.ProjectKey);
            //根据返回来的Key值查找GUID， 针对修改
            var depGUID = Guid.Empty;
            if (ssKey != null)
            {
                depGUID = ssKey.GUID;
            }

            SS_ProjectView mModel = data;
            object g;

            #region   主表字段验证

            if (mModel.ProjectKey == "")
            {
                str = "项目编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.ProjectName == "")
            {
                str = "项目名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.ProjectClassKey == "")
            {
                str = "所属分类编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.ProjectClassName == "")
            {
                str = "所属分类名称 不能为空！";
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
            if (mModel.BeginYear == null)
            {
                str = "请输入开始年度！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.StopYear != null)
            {
                //结束年度不能小于开始年度
                if (mModel.StopYear < mModel.BeginYear)
                {
                    str = "结束年度不能小于开始年度！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            //修改数据时，如果不是同一条数据则走if，否则通过
            if (depGUID != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.ProjectKey == ssKey.ProjectKey)
                    {
                        str = "项目编号 不能重复！";
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
            SS_ProjectView model = (SS_ProjectView)data;
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
            SS_Project ssMain;
            ssMain = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.GUID == guid);

            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID
            if (ssMain==null||ssMain.GUID ==Guid.Empty)
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }

            #region 目前没有用到

            //预算分配表BG_Assing验证
            BG_Assign bgAssignEnt = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Project == guid);
            if (bgAssignEnt != null)
            {
                str = "该项目在预算分配中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算控制明细BG_ControlMain验证
            BG_ControlMain bgControlMainEnt = this.BusinessContext.BG_ControlMain.FirstOrDefault(e => e.GUID_Project == guid);
            if (bgControlMainEnt != null)
            {
                str = "该项目在预算控制中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算控制明细BG_ControlDetail验证
            BG_ControlDetail bgControlDetailEnt = this.BusinessContext.BG_ControlDetail.FirstOrDefault(e => e.GUID_Project == guid);
            if (bgControlDetailEnt != null)
            {
                str = "该项目在预算控制明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算编制初始值BG_DefaultMain验证
            BG_DefaultMain bgDefaultMainEnt = this.BusinessContext.BG_DefaultMain.FirstOrDefault(e => e.GUID_Project == guid);
            if (bgDefaultMainEnt != null)
            {
                str = "该项目在预算编制初始值中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算编制BG_Main验证
            BG_Main bgMainEnt = this.BusinessContext.BG_Main.FirstOrDefault(e=>e.GUID_Project == guid);
            if (bgMainEnt != null)
            {
                str = "该项目在预算编制中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //报销单明细BX_Default验证
            BX_Detail bxDefaultEnt = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_Project == guid);
            if (bxDefaultEnt != null)
            {
                str = "该项类在报销单明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //财政支付码CN_PaymentNumber验证
            CN_PaymentNumber cnPaymentNumber = this.BusinessContext.CN_PaymentNumber.FirstOrDefault(e => e.GUID_Project == guid);
            if (cnPaymentNumber != null)
            {
                str = "该项目在财政支付码中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收款凭单SK_Main验证
            SK_Main skMain = this.BusinessContext.SK_Main.FirstOrDefault(e => e.GUID_Project == guid);
            if (skMain != null)
            {
                str = "该项目在收款凭单中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //出纳明细CN_Detail验证
            CN_Detail cnMainEnt = this.BusinessContext.CN_Detail.FirstOrDefault(e => e.GUID_Project == guid);
            if (cnMainEnt != null)
            {
                str = "该项目分类在出纳明细中已经被引用！不能删除！";
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
            SS_ProjectView model = (SS_ProjectView)data;
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
