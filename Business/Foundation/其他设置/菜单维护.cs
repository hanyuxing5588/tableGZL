﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;
namespace Business.Foundation.其他设置
{
    public class 菜单维护 : BaseDocument
    {
        public 菜单维护() : base() { }
        public 菜单维护(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="status">状态1表示新建 2表示修改 3表示删除</param>
        /// <param name="jsonModel">JsonModel</param>
        /// <returns>JsonModel</returns>
        public override JsonModel Save(string status, JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            try
            {
                Guid value = jsonModel.m.Id(new SS_Menu().ModelName());
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

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Menu main = new SS_Menu();
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            this.InfrastructureContext.SS_Menu.AddObject(main);
            var sdtu = update(jsonModel, main);
          
            this.InfrastructureContext.SaveChanges();
            return main.GUID;

        }

        public SS_DocTypeUrl update(JsonModel jsonModel, SS_Menu main) 
        {
           
            SS_DocTypeUrl sdtu = new SS_DocTypeUrl();
            sdtu.Fill(jsonModel.m);
            if (sdtu.GUID_DocType ==Guid.Empty) 
            {
                return null;
            }
            sdtu.DocTypeUrl = main.scope;
            sdtu.GUID = Guid.NewGuid();
            var entDocTypeOld = this.InfrastructureContext.SS_DocTypeUrl.FirstOrDefault(e => e.GUID_DocType == sdtu.GUID_DocType);
            if (entDocTypeOld != null)
            {
                this.InfrastructureContext.SS_DocTypeUrl.DeleteObject(entDocTypeOld);
            }
            this.InfrastructureContext.SS_DocTypeUrl.AddObject(sdtu);
            return sdtu;
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>GUID</returns>
        protected override Guid Modify(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Menu main = new SS_Menu();
            Guid g = jsonModel.m.Id(main.ModelName());
            main = this.InfrastructureContext.SS_Menu.FirstOrDefault(e => e.GUID == g);
            main.Fill(jsonModel.m);
            this.InfrastructureContext.ModifyConfirm(main);
            var sdtu = update(jsonModel, main);
            this.InfrastructureContext.SS_DocTypeUrl.AddObject(sdtu);
            this.InfrastructureContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SS_Menu main = this.InfrastructureContext.SS_Menu.FirstOrDefault(e => e.GUID == guid);
            this.InfrastructureContext.DeleteConfirm(main);
            this.InfrastructureContext.SaveChanges();
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
                SS_MenuView main = this.InfrastructureContext.SS_MenuView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();
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

        #region 支出类型
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
            SS_Menu main = null;
            switch (status)
            {
                case "1": //新建
                    main = LoadMain(jsonModel);
                    vResult = InsertVerify(main);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new SS_Menu().ModelName());
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
        private SS_Menu LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_Menu main = new SS_Menu();
            main.Fill(jsonModel.m);
            return main;
        }
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_Menu data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            SS_Menu mModel = data;
            #region   主表字段验证
            //菜单编号

            if (string.IsNullOrEmpty(mModel.MenuKey))
            {
                str = "菜单编号 字段为必输入项！";
                resultList.Add(new ValidationResult("", str));

            }
            //菜单名称

            if (string.IsNullOrEmpty(mModel.MenuName))
            {
                str = "菜单名称 字段为必输入项！";
                resultList.Add(new ValidationResult("", str));

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
            SS_Menu model = (SS_Menu)data;
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
            SS_Menu main = new SS_Menu();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (main.GUID == null || main.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "支出类型GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

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
            SS_Menu model = (SS_Menu)data;

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
