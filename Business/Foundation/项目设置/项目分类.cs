using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;

namespace Business.Foundation.项目设置
{
    public class 项目分类 : BaseDocument
    {
        public 项目分类() : base() { }
        public 项目分类(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_ProjectClass obj = new SS_ProjectClass();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();
            var list = this.InfrastructureContext.SS_ProjectClass.ToList();
            if (obj.PGUID != null)
            {
                UpdateParentIsStop(list, (Guid)obj.PGUID);
            }
            this.InfrastructureContext.SS_ProjectClass.AddObject(obj);
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
            SS_ProjectClass obj = new SS_ProjectClass();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_ProjectClass.FirstOrDefault(e => e.GUID == g);
            obj.Fill(jsonModel.m);
            var list = this.InfrastructureContext.SS_ProjectClass.ToList();
            if (obj.PGUID != null)
            {
                UpdateParentIsStop(list, (Guid)obj.PGUID);
            }
            this.InfrastructureContext.ModifyConfirm(obj);
            this.InfrastructureContext.SaveChanges();
            return obj.GUID;
        }
        /// <summary>
        /// 如果此项为启动项，则父项也要改为启动项
        /// </summary>
        /// <param name="parentId"></param>
        private void UpdateParentIsStop(List<SS_ProjectClass> list,Guid parentId)
        {
            var model = list.Find(e=>e.GUID==parentId);
            if (model != null)
            {
                if (model.IsStop == true)
                {
                    model.IsStop = false;
                    this.InfrastructureContext.ModifyConfirm(model);
                }
                if (model.PGUID != null)
                {
                    UpdateParentIsStop(list, (Guid)model.PGUID);
                }
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SS_ProjectClass obj = this.InfrastructureContext.SS_ProjectClass.FirstOrDefault(e => e.GUID == guid);
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
            //e.BeginYear <= DateTime.Now.Year && e.StopYear >= DateTime.Now.Year) || (e.IsStop == false && e.BeginYear <= DateTime.Now.Year && e.StopYear == null)
            var details = this.InfrastructureContext.SS_ProjectClassView.Where(e => e.IsStop == false).OrderBy(e=>e.ProjectClassKey).ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_ProjectClassView detail in details)
                {
                    List<JsonAttributeModel> picker = detail.Pick();
                    jgm.r.Add(picker);
                }
            }
            return jmodel;
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public  JsonModel AllRetrieve(int type)
        {
            //type 默认全部数据 0为停用数据 1 指未停用数据
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_ProjectClassView.OrderBy(e=>e.ProjectClassKey).ToList();
            if (type ==0)
            {
                details = details.FindAll(e=>e.IsStop==false);
            }
            else if (type == 1)
            {
                details = details.FindAll(e=>e.IsStop==true);
            }          
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_ProjectClassView detail in details)
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
                Guid value = jsonModel.m.Id(new SS_ProjectClass().ModelName());
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
            SS_ProjectClass main = null;
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
                    Guid value = jsonModel.m.Id(new SS_ProjectClass().ModelName());
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
        private SS_ProjectClass LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_ProjectClass main = new SS_ProjectClass();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_ProjectClass data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SS_ProjectClass ssKey = this.InfrastructureContext.SS_ProjectClass.FirstOrDefault(e => e.ProjectClassKey == data.ProjectClassKey);
            //根据返回来的Key值查找GUID， 针对修改
            var depGUID = Guid.Empty;
            if (ssKey != null)
            {
                depGUID = ssKey.GUID;
            }

            SS_ProjectClass mModel = data;
            object g;

            #region   主表字段验证

            if (mModel.ProjectClassKey == "")
            {
                str = "项目分类编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.ProjectClassName == "")
            {
                str = "项目分类名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.BeginYear == null)
            {
                str = "项目分类启用年度 不能为空！";
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
                    if (mModel.ProjectClassKey == ssKey.ProjectClassKey)
                    {
                        str = "项目分类编号 不能重复！";
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
            SS_ProjectClass model = (SS_ProjectClass)data;
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
            SS_ProjectClass ssMain;
            ssMain = this.InfrastructureContext.SS_ProjectClass.FirstOrDefault(e => e.GUID == guid);

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
            //项目SS_Project
            SS_Project ssProjectEnt = this.InfrastructureContext.SS_Project.FirstOrDefault(e => e.GUID_ProjectClass == guid);
            if (ssProjectEnt != null)
            {
                str = "该项目分类在项目中已经被引用！不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            
            #region 目前没有用到

            ////预算分配表BG_Assing验证
            //BG_Assign bgAssignEnt = this.BusinessContext.BG_Assign.FirstOrDefault(e => e.GUID_Project == guid);
            //if (bgAssignEnt != null)
            //{
            //    str = "该项目分类在预算分配中已经被引用！不能删除！";
            //    resultList.Add(new ValidationResult("", str));
            //    result._validation = resultList;
            //}
            ////报销单明细BX_Default验证
            //BX_Detail bxDefaultEnt = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_Project == guid);
            //if (bxDefaultEnt != null)
            //{
            //    str = "该项目分类在报销单明细中已经被引用！不能删除！";
            //    resultList.Add(new ValidationResult("", str));
            //    result._validation = resultList;
            //}

            ////出纳明细CN_DetailView验证
            //CN_DetailView cnMainEnt = this.BusinessContext.CN_DetailView.FirstOrDefault(e => e.GUID_Department == guid);
            //if (cnMainEnt != null)
            //{
            //    str = "该项目分类在出纳明细中已经被引用！不能删除！";
            //    resultList.Add(new ValidationResult("", str));
            //    result._validation = resultList;
            //}
            ////收入明细SR_Detail验证
            //SR_Detail srDetailEnt = this.BusinessContext.SR_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            //if (srDetailEnt != null)
            //{
            //    str = "该项目分类在收入明细中已经被引用！不能删除！";
            //    resultList.Add(new ValidationResult("", str));
            //    result._validation = resultList;
            //}
            ////往来明细WL_Detail验证
            //WL_Detail wlDetailEnt = this.BusinessContext.WL_Detail.FirstOrDefault(e => e.GUID_Department == guid);
            //if (wlDetailEnt != null)
            //{
            //    str = "该项目分类在往来明细中已经被引用！不能删除！";
            //    resultList.Add(new ValidationResult("", str));
            //    result._validation = resultList;
            //}

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
            SS_ProjectClass model = (SS_ProjectClass)data;
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
