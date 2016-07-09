using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.角色用户
{
    public class 用户设置 : BaseDocument
    {
        public 用户设置() : base() { }
        public 用户设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        //zzp

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Operator oper = new SS_Operator();          
            oper.Fill(jsonModel.m);
            oper.GUID = Guid.NewGuid();
            //将密码加密
            if (oper.Password != "")
            {
                Infrastructure.Encryption encryption = new Infrastructure.Encryption();
                oper.Password = encryption.DigestStrToHexStr(oper.Password);
            }
            else {//如果忘了输入密码，那么默认给个默认值！
                oper.Password = "123456";
                Encryption encryption = new Encryption();
                oper.Password = encryption.DigestStrToHexStr(oper.Password);
            }

            //拿到d中的数据后，判断.
            //根据模型名称得到Grid中的数据集，进行循环
            //将人员跟角色中的GUID与SS_RoleOperator中的GUID做匹配，然后根据对应的GUID做相应的添加
            if (jsonModel.d.Count > 0 && jsonModel.d != null)
            {
                SS_Role role = new SS_Role();
                string roleModelName = role.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(roleModelName);
                if (Grid != null)
                {                   
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        role.Fill(row);
                        SS_RoleOperator roleoper = new SS_RoleOperator();   //人员角色关系
                        roleoper.GUID_Operator = oper.GUID;
                        roleoper.GUID_Role = role.GUID;
                        this.InfrastructureContext.SS_RoleOperator.AddObject(roleoper);
                    }
                }
            }
   
            this.InfrastructureContext.SS_Operator.AddObject(oper);            
            this.InfrastructureContext.SaveChanges();
            return oper.GUID;
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
            SS_Operator oper = new SS_Operator();
            JsonAttributeModel id = jsonModel.m.IdAttribute(oper.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            oper = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == g);
             //将密码加密，如果没有修改当前密码，则原密码不动，如果修改了，则更新最新的密码
            var password=jsonModel.m.GetValueByAttribute("Password").ToString();
            var strPassword=string.Empty;
            if (password != oper.Password)
            {
                Infrastructure.Encryption encryption = new Infrastructure.Encryption();
                strPassword = encryption.DigestStrToHexStr(password);

            }
            oper.Fill(jsonModel.m);
            //修改完成后
            if (strPassword != "")
            {
                oper.Password = strPassword;
            }
           
            //添加之前先清空d中的数据(条件：SS_RoleOperator中的GUID_Operator == 自身的 SS_Operator中的GUID)
            List<SS_RoleOperator> roleoperList;
            roleoperList = this.InfrastructureContext.SS_RoleOperator.Where(e => e.GUID_Operator == g).ToList();
            foreach (SS_RoleOperator item in roleoperList)
            {
                InfrastructureContext.DeleteConfirm(item);
            }
            //删除完成后，SS_RoleOperator中就没有了GUID为SS_Operator中的GUID的角色
            //在从新根据GUID将角色添加到SS_RoleOperator中
            if (jsonModel.d.Count > 0 && jsonModel.d != null)
            {
                SS_Role role = new SS_Role();
                string roleModelName = role.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(roleModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        role.Fill(row);
                        SS_RoleOperator roleoper = new SS_RoleOperator();   //人员角色关系
                        roleoper.GUID_Operator = oper.GUID;
                        roleoper.GUID_Role = role.GUID;
                        this.InfrastructureContext.SS_RoleOperator.AddObject(roleoper);
                    }
                }
            }          

            this.InfrastructureContext.ModifyConfirm(oper);
            this.InfrastructureContext.SaveChanges();
            return oper.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            List<SS_RoleOperator> roleoperList;
            //得到一个 SS_RoleOperator 集合
            roleoperList = this.InfrastructureContext.SS_RoleOperator.Where(e => e.GUID_Operator == guid).ToList();
            //将SS_RoleOperator关系表中人员对应的多个角色一一删除
            foreach (SS_RoleOperator item in roleoperList)
            {
                InfrastructureContext.DeleteConfirm(item);
            }
            //根据匹配的GUID，将这条记录删除
            SS_Operator role = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(role);
            InfrastructureContext.SaveChanges();
        }

        /// <summary>
        /// 初始化时返回数据列表
        /// 与增、删、改、查没有关联，是在删除或保存成功之后返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_Role.ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_Role detail in details)
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
                Guid value = jsonModel.m.Id(new SS_Operator().ModelName());
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

        #region 角色设置验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_Operator main = null;
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
                    Guid value = jsonModel.m.Id(new SS_Operator().ModelName());
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
        private SS_Operator LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_Operator main = new SS_Operator();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_Operator data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();

            //返回一条指定的数据
            SS_Operator ssKey = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.OperatorKey == data.OperatorKey);
            //根据返回来的Key值查找GUID,针对修改
            var rGUID = Guid.Empty;
            if (ssKey != null)
            {
                rGUID = ssKey.GUID;
            }
            SS_Operator mModel = data;
            object g;

            #region 主表字段验证
            if (string.IsNullOrEmpty(mModel.OperatorKey + "".Trim()))
            {
                str = "编号 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (string.IsNullOrEmpty(mModel.OperatorKey + "".Trim()))
            {
                str = "名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            //如果输入时间有效期的情况
            if (mModel.IsTimeLimited == true)
            {
                if (mModel.StartTime.IsNullOrEmpty())
                {
                    str = "开始时间 不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }
                if (mModel.StopTime.IsNullOrEmpty())
                {
                    str = "结束时间 不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }
                //时间比较
                if (mModel.StopTime < mModel.StartTime)
                {
                    str = "结束时间不能小于开始时间！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            if (rGUID != data.GUID)
            {
                if (ssKey != null)
                {
                    if (mModel.OperatorKey == data.OperatorKey)
                    {
                        str = "编号 不能重复！";
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
            SS_Operator model = (SS_Operator)data;
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
            SS_Operator ssOperator = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 主表字段验证

            //报销单GUID
            if (ssOperator == null)
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }

            //角色用户关联表SS_RoleOperator
            //var ssRoleOperator = this.InfrastructureContext.SS_RoleOperator.FirstOrDefault(e => e.GUID_Operator == guid);
            //if (ssRoleOperator != null)
            //{
            //    str = "该操作员在角色关联中已经被引用！不能删除！";
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
            SS_Operator model = (SS_Operator)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;
        }

        #endregion


        public bool UpdatePwd(Guid OperGuid, string oldPwd, string newPwd,out string msg)
        {
            //加密
            Infrastructure.Encryption encryption = new Infrastructure.Encryption();
            SS_Operator ssOperator = new SS_Operator();
            ssOperator = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperGuid);
            //验证结果
            string str = string.Empty;
            if (ssOperator != null)
            {
                //如果输入的原密码与数据库中的密码不匹配
                var oldPwds = encryption.DigestStrToHexStr(oldPwd);
                if (oldPwds != ssOperator.Password)
                {
                    str = "原密码输入有误，请重新输入！";
                    //需要返回
                    msg = str;
                    return false;
                }
                else
                {
                    //如果输入的原密码相等，则将下面的新密码更新到数据库，并且加密
                    ssOperator.Password = encryption.DigestStrToHexStr(newPwd);
                    this.InfrastructureContext.ModifyConfirm(ssOperator);
                    this.InfrastructureContext.SaveChanges();
                    msg = str;
                    return true;
                }
            }
            else
            {
                str = "没有对应的数据！";
                msg = str;
                return false;
            }
           
        }


    }
}
