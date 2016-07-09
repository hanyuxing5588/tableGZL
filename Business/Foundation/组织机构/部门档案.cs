using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;

namespace Business.Foundation.组织机构
{
    public class 部门档案 : BaseDocument
    {
        public 部门档案() : base() { }
        public 部门档案(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Department obj = new SS_Department();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_Department.AddObject(obj);
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
            SS_Department obj = new SS_Department();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_Department.FirstOrDefault(e => e.GUID == g);
            if (obj.PGUID != null)
            {
                UpdateParentDepartment(obj.PGUID);
            }
            obj.Fill(jsonModel.m);

            this.InfrastructureContext.ModifyConfirm(obj);
            this.InfrastructureContext.SaveChanges();
            return obj.GUID;
        }
        /// <summary>
        /// 如果子项有一项为启动项，此项的父项已经停用，要把此项的父项改无启动
        /// </summary>
        /// <param name="pGUID"></param>
        private void UpdateParentDepartment(Guid? pGUID)
        {
            var dwModel = this.InfrastructureContext.SS_Department.FirstOrDefault(e => e.GUID == pGUID);
            if (dwModel != null)
            {
                if (dwModel.IsStop == true)
                {
                    dwModel.IsStop = false;
                    this.InfrastructureContext.ModifyConfirm(dwModel);
                }
                if (dwModel.PGUID != null)
                {
                    UpdateParentDepartment(dwModel.PGUID);
                }
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SS_Department obj = this.InfrastructureContext.SS_Department.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(obj);
            InfrastructureContext.SaveChanges();
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid) {
            //todo 列表数据返回
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_DepartmentView.OrderBy(e=>e.DepartmentKey).ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_DepartmentView detail in details)
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
                Guid value = jsonModel.m.Id(new SS_Department().ModelName());
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

        #region 部门档案验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_DepartmentView main = null;
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
                    Guid value = jsonModel.m.Id(new SS_Department().ModelName());
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
        private SS_DepartmentView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_DepartmentView main = new SS_DepartmentView();
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
            SS_DepartmentView model = (SS_DepartmentView)data;
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
        private List<ValidationResult> VerifyResultMain(SS_DepartmentView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SS_DepartmentView ssKey = this.InfrastructureContext.SS_DepartmentView.FirstOrDefault(e => e.DepartmentKey == data.DepartmentKey);
            //根据返回来的Key值查找GUID， 针对修改
            var depGUID = Guid.Empty;
            if (ssKey != null)
            {
                depGUID = ssKey.GUID;
            }

            SS_DepartmentView mModel = data;
            object g;

            #region   主表字段验证
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
                str = "所属单位编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.DWName == "")
            {
                str = "所属单位名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            //修改数据时，如果不是同一条数据则走if，否则通过
            if (depGUID != data.GUID)
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
            SS_Department ssMain;
            ssMain = this.InfrastructureContext.SS_Department.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID
            if (ssMain.GUID == null || ssMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //人员验证
            SS_Person ssPersonEnt = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID_Department == guid);
            if (ssPersonEnt != null)
            {
                str = "该部门已有人员被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //报销单BX_Main验证
            BX_Main bxMainEnt = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (bxMainEnt != null)
            {
                str = "该部门在报销单中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //报销单明细BX_Default验证
            BX_Detail bxDefaultEnt = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            if (bxDefaultEnt != null)
            {
                str = "该部门在报销单明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算BG_Main验证
            BG_Main bgMainEnt = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (bgMainEnt != null)
            {
                str = "该部门在预算中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            
            //预算分配表BG_Assing验证
            BG_Assign bgAssignEnt = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Department == guid);
            if (bgAssignEnt != null)
            {
                str = "该部门在预算分配中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //往来WL_Main验证
            WL_Main wlMainEnt = this.BusinessContext.WL_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (wlMainEnt != null)
            {
                str = "该部门在往来中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //往来明细WL_Detail验证
            WL_Detail wlDetailEnt = this.BusinessContext.WL_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            if (wlDetailEnt != null)
            {
                str = "该部门在往来明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收入SR_Main验证
            SR_Main srMainEnt = this.BusinessContext.SR_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (srMainEnt != null)
            {
                str = "该部门在收入中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收入明细SR_DetailView验证
            SR_Detail srDetailEnt = this.BusinessContext.SR_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            if (srDetailEnt != null)
            {
                str = "该部门在收入明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //出纳CN_Main验证
            CN_Main cnMainEnt = this.BusinessContext.CN_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (cnMainEnt != null)
            {
                str = "该部门在出纳中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算编制初始值表BG_DefaultMain验证
            BG_DefaultMain bgDefaulMainEnt = this.BusinessContext.BG_DefaultMain.FirstOrDefault(e => e.GUID_Department == guid);
            if (bgDefaulMainEnt != null)
            {
                str = "该部门在预算编制中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //出差补助明细BX_TravelAllowance
            BX_TravelAllowance bxTravelAllowaneEnt = this.BusinessContext.BX_TravelAllowance.FirstOrDefault(e => e.GUID_Department == guid);
            if (bxTravelAllowaneEnt != null)
            {
                str = "该部门在出差补助明细中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //提存现单CN_CashMain
            CN_CashMain cnCheckEnt = this.BusinessContext.CN_CashMain.FirstOrDefault(e => e.GUID_Department == guid);
            if (cnCheckEnt != null)
            {
                str = "该部门在提存现单中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //核销HX_Main验证
            HX_Main hxMainEnt = this.BusinessContext.HX_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (hxMainEnt != null)
            {
                str = "该部门在核销中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //收款凭单SK_Main
            SK_Main skMainEnt = this.BusinessContext.SK_Main.FirstOrDefault(e => e.GUID_Department == guid);
            if (skMainEnt != null)
            {
                str = "该部门在收款凭单中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            //预算控制BG_ControlMain验证
            BG_ControlMain bgControlMain = this.BusinessContext.BG_ControlMain.FirstOrDefault(e => e.GUID_Department == guid);
            if (bgControlMain != null)
            {
                str = "该部门在预算控制中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
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
            SS_DepartmentView model = (SS_DepartmentView)data;
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
