using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.角色用户
{
    public class 用户分组 : BaseDocument
    {
        public 用户分组() : base() { }
        public 用户分组(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        /// 暂时没有用到，用的修改功能
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_Operator obj = new SS_Operator();
            obj.Fill(jsonModel.m);
            obj.GUID = Guid.NewGuid();

            this.InfrastructureContext.SS_Operator.AddObject(obj);
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
            SS_Role obj = new SS_Role();
            JsonAttributeModel id = jsonModel.m.IdAttribute(obj.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            obj = this.InfrastructureContext.SS_Role.FirstOrDefault(e => e.GUID == g);
            obj.Fill(jsonModel.m);
            //添加之前根据所对应的角色下面的人员先全部删除(删除表：SS_RoleOperator，条件：SS_Role.GUID==SS_RoleOperator.GUID.Role)
            List<SS_RoleOperator> roleOperator;
            roleOperator = this.InfrastructureContext.SS_RoleOperator.Where(e => e.GUID_Role == g).ToList();
            foreach (SS_RoleOperator item in roleOperator)
            {
                InfrastructureContext.DeleteConfirm(item);
            }
            //删除完成后，SS_RoleOperator中就没有了角色GUID和角色对应下的所有人员的GUID了，等于是SS_RoleOperator中没有了这条记录。
            //之后再将选择的角色和多对应的下的所有的人员添加进去
            if (jsonModel.d != null && jsonModel.d.Count > 0) {
                SS_Operator oper = new SS_Operator();
                string operModelName = oper.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(operModelName);
                if (Grid != null) {
                    foreach (List<JsonAttributeModel> row in Grid.r) {
                        SS_RoleOperator opleOper = new SS_RoleOperator();   //人员角色关系
                        //将页面上datagrid中的数据填充到模型中,记住要赋值
                        oper.Fill(row);
                        opleOper.GUID_Role = obj.GUID;
                        opleOper.GUID_Operator = oper.GUID;
                        this.InfrastructureContext.SS_RoleOperator.AddObject(opleOper);
                    }
                }
            }           
            this.InfrastructureContext.SaveChanges();
            return obj.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        /// 暂时没有用到，用的修改功能。
        protected override void Delete(Guid guid)
        {
            SS_Operator obj = this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == guid);
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
            //默认排序Orderby
            var details = this.InfrastructureContext.SS_Operator.OrderBy(e => e.OperatorKey).ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_Operator detail in details)
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
