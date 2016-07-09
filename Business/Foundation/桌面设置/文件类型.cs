using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using OAModel;

namespace Business.Foundation.桌面设置
{
    public class 文件类型 : BaseDocument
    {   
        public 文件类型() : base() { }
        public 文件类型(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_OfficeFileType role = new SS_OfficeFileType();
            role.Fill(jsonModel.m);
            role.GUID = Guid.NewGuid();

            this.OAContext.SS_OfficeFileType.AddObject(role);
            this.OAContext.SaveChanges();
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
            SS_OfficeFileType role = new SS_OfficeFileType();
            JsonAttributeModel id = jsonModel.m.IdAttribute(role.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            role = this.OAContext.SS_OfficeFileType.FirstOrDefault(e => e.GUID == g);
            role.Fill(jsonModel.m);

            this.OAContext.ModifyConfirm(role);
            this.OAContext.SaveChanges();
            return role.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SS_OfficeFileType role = this.OAContext.SS_OfficeFileType.FirstOrDefault(e => e.GUID == guid);
            OAContext.DeleteConfirm(role);
            OAContext.SaveChanges();
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
                Guid value = jsonModel.m.Id(new SS_OfficeFileType().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 
                       // strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Insert(jsonModel);
                        }
                        break;
                    case "2": //修改
                       // strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Modify(jsonModel);
                        }
                        break;
                    case "3": //删除
                        //strMsg = DataVerify(jsonModel, status);
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



    }   

}
