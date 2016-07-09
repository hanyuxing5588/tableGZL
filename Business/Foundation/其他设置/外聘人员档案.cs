using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.其他设置
{
    public class 外聘人员档案 : BaseDocument
    {
        public 外聘人员档案() : base() { }
        public 外聘人员档案(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_InvitePerson ssInvitePerson = new SS_InvitePerson();
            ssInvitePerson.Fill(jsonModel.m);
            ssInvitePerson.GUID = Guid.NewGuid();
            this.InfrastructureContext.SS_InvitePerson.AddObject(ssInvitePerson);
            this.InfrastructureContext.SaveChanges();
            return ssInvitePerson.GUID;
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
            SS_InvitePerson role = new SS_InvitePerson();
            JsonAttributeModel id = jsonModel.m.IdAttribute(role.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            role = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == g);
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
            SS_InvitePerson role = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == guid);
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
            var jmodel = new JsonModel();
            var detail = this.InfrastructureContext.SS_InvitePersonView.OrderBy(e => e.InvitePersonName).ToList();
            if (detail.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(detail[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_InvitePersonView item in detail)
                {
                    List<JsonAttributeModel> picker = item.Pick();
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
                Guid value = jsonModel.m.Id(new SS_InvitePerson().ModelName());
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


        #region 外聘人员档案验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_InvitePersonView main = null;
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
                    Guid value = jsonModel.m.Id(new SS_InvitePerson().ModelName());
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
        private SS_InvitePersonView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_InvitePersonView main = new SS_InvitePersonView();
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
            SS_InvitePersonView model = (SS_InvitePersonView)data;
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
        private List<ValidationResult> VerifyResultMain(SS_InvitePersonView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //返回一条指定的数据
            SS_InvitePersonView ssGUID = this.InfrastructureContext.SS_InvitePersonView.FirstOrDefault(e => e.InvitePersonIDCard == data.InvitePersonIDCard);
            //根据返回来的Key值查找GUID
            var ssGuid = Guid.Empty;
            //根据返回来的Key值查找名称
            var ssInveName = "";
            if (ssGUID != null)
            {
                ssGuid = ssGUID.GUID; ;
            }
            SS_InvitePersonView mModel = data;
            object g;

            #region   主表字段验证
            if (mModel.InvitePersonName == "")
            {
                str = "外聘人员名称 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            if (mModel.InvitePersonIDCard == "")
            {
                str = "证件号码 不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                //判断是New && Update
                if (ssGuid != data.GUID)
                {
                    //判断选择的证件类型与输入的证件号码是否匹配
                    var isIDCard = this.InfrastructureContext.SS_InvitePersonView.Where(e => e.InvitePersonIDCard == mModel.InvitePersonIDCard).ToList();
                    if (isIDCard.Count > 0)
                    {
                        var isIDCardName = isIDCard[0].InvitePersonName;
                        str = "与外聘人员" + isIDCardName + "的证件号码重复，请重新输入！";
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
            SS_InvitePerson ssMain;
            ssMain = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == guid);
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 主表验证

            //劳务费明细BX_InviteFee验证
            var bxInviteFeeEnt = this.BusinessContext.BX_InviteFee.FirstOrDefault(e => e.GUID_InvitePerson == guid);
            if (bxInviteFeeEnt != null)
            {
                str = "该外聘人员在劳务费明细中已经被引用！不能删除！";
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
            SS_InvitePersonView model = (SS_InvitePersonView)data;
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

