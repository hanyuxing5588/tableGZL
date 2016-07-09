using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.薪酬设置
{
    public class 工资计划人员设置 : BaseDocument
    {   
        public 工资计划人员设置() : base() { }
        public 工资计划人员设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>工资计划设置
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SA_Plan role = new SA_Plan();           
            role.Fill(jsonModel.m);
            role.GUID = Guid.NewGuid();          

            this.InfrastructureContext.SA_Plan.AddObject(role);
            this.InfrastructureContext.SaveChanges();
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
            SA_Plan role = new SA_Plan();
            SA_PlanPersonSet planPerson = new SA_PlanPersonSet();
            JsonAttributeModel id = jsonModel.m.IdAttribute(role.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            role = this.InfrastructureContext.SA_Plan.FirstOrDefault(e => e.GUID == g);
            role.Fill(jsonModel.m);

            //修改之前要判断数据是否存在，条件（1、GUID_SA_Plan，GUID_SS_Bank）
            //1、存在，将之前的数据删除掉或是更新
            //2、不存在，将插入一条新的数据
            //在修改时，要删除一条数据，那么就将这条数据所对应的SA_PlanPersonSet表中的GUID一块删除
            string planPersonModelName = planPerson.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(planPersonModelName);
            var listDetail = this.InfrastructureContext.SA_PlanPersonSet.Where(e => e.GUID_SA_Plan == role.GUID).ToList();
            if (listDetail != null)
            {
                foreach (SA_PlanPersonSet detail in listDetail) 
                { 
                    this.InfrastructureContext.DeleteConfirm(detail); 
                }
            }

            //在修改时需要New一条数据，那么我们就将数据直接添加进去
            List<List<JsonAttributeModel>> newRows = Grid.r.FindByModelName(typeof(gzjhryszModel).Name);
            foreach (List<JsonAttributeModel> item in newRows)
            {
                SA_PlanPersonSet planper = new SA_PlanPersonSet();
                planper.Fill(item);
                //模型转换
                gzjhryszModel gModel = new gzjhryszModel();
                gModel.ClassFill(item);
                planper.GUID = Guid.NewGuid();
                planper.GUID_SA_Plan = role.GUID;
                planper.GUID_SS_Bank = gModel.GUID_SS_Bank;
                planper.GUID_SS_Person = gModel.GUID_SS_Person;
                this.InfrastructureContext.SA_PlanPersonSet.AddObject(planper);
            }

            //else//不删除走下面
            //{
            //    //Update时，两种情况：一是旧数据，二是新数据(区分：根据传过来的人员的GUID进行区分，前台将勾选上的Checkbox数据传过来)
            //    //判断是否有GUID
            //        foreach (SA_PlanPersonSet item in listDetail)
            //        {
            //            List<JsonAttributeModel> row = Grid.r.Find(item.GUID, planPersonModelName);
            //            if (row == null) this.InfrastructureContext.DeleteConfirm(item);
            //            else
            //            {
            //                item.Fill(row);
            //                //模型转换
            //                gzjhryszModel gModel = new gzjhryszModel();
            //                gModel.ClassFill(row);
            //                //在更新时要将所修改的数据的GUID传过来，
            //                //怎么样知道我修改的是哪一行的记录，传过来人员的GUID
            //                //item.GUID = Guid.NewGuid();
            //                item.GUID_SA_Plan = gModel.GUID_SA_Plan;
            //                item.GUID_SS_Bank = gModel.GUID_SS_Bank;
            //                item.GUID_SS_Person = gModel.GUID_SS_Person;
            //                //this.InfrastructureContext.ModifyConfirm(item);
            //                this.InfrastructureContext.SA_PlanPersonSet.AddObject(item);
            //            }
            //        }
            //    ////在修改时需要New一条数据，那么我们就将数据直接添加进去
            //    //List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(planPersonModelName);
            //    //foreach (List<JsonAttributeModel> item in newRows)
            //    //{
            //    //    SA_PlanPersonSet planper = new SA_PlanPersonSet();
            //    //    planper.Fill(item);
            //    //    //模型转换
            //    //    gzjhryszModel gModel = new gzjhryszModel();
            //    //    gModel.ClassFill(item);
            //    //    planper.GUID = Guid.NewGuid();
            //    //    planper.GUID_SA_Plan = gModel.GUID_SA_Plan;
            //    //    planper.GUID_SS_Bank = gModel.GUID_SS_Bank;
            //    //    planper.GUID_SS_Person = gModel.GUID_SS_Person;
            //    //    this.InfrastructureContext.SA_PlanPersonSet.AddObject(planper);
            //    //}
            //}
            
            //this.InfrastructureContext.ModifyConfirm(role);
            this.InfrastructureContext.SaveChanges();
            return role.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            SA_Plan role = this.InfrastructureContext.SA_Plan.FirstOrDefault(e => e.GUID == guid);
            InfrastructureContext.DeleteConfirm(role);
            InfrastructureContext.SaveChanges();
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {

            /*
             select PersonKey,PersonName,GUID_SA_Plan,BankKey,BankName from SS_PersonView 
            left join  (
            select GUID_SA_Plan,GUID_SS_Person,BankKey,BankName from  SA_PlanPersonSetview 
            where GUID_SA_Plan='FD92C4AE-079D-3040-BDE2-EB4A4512593B'
            )  a on a.GUID_SS_Person=SS_PersonView.GUID  order by SS_PersonView.personkey 
             */
            //返回列表数据
            var jmodel = new JsonModel();
            List<gzjhryszModel> details=new List<gzjhryszModel> ();
            //默认，guid为空的时候查询全部，默认是没有银行信息的
            if (guid == Guid.Empty)
            {
                details =(from p in InfrastructureContext.SS_PersonView
                         select new gzjhryszModel
                         {
                             GUID_SA_Plan =Guid.Empty,
                             PersonName = p.PersonName,
                             PersonKey = p.PersonKey,
                             BankKey ="",
                             BankName =""
                         }).ToList().OrderBy(e => e.PersonKey).ToList();
            }
            else
            {//根据条件去查询要显示的结果
                var saSet = from s in this.InfrastructureContext.SA_PlanPersonSetView
                            where s.GUID_SA_Plan == guid
                            select s;
                               
                details = (from p in this.InfrastructureContext.SS_PersonView
                           join s in saSet
                           on p.GUID equals s.GUID_SS_Person into temp
                           from s in temp.DefaultIfEmpty()                           
                           select new gzjhryszModel
                           {
                               GUID = s.GUID,
                               GUID_SA_Plan = s.GUID_SA_Plan,
                               GUID_SS_Person = p.GUID,
                               GUID_SS_Bank = s.GUID_SS_Bank,
                               PersonName = p.PersonName,
                               PersonKey = p.PersonKey,
                               BankKey = s.BankKey,
                               BankName = s.BankName
                           }).ToList().OrderBy(e => e.PersonKey).ToList();
            }
                             //this.InfrastructureContext.SS_PersonView.OrderBy(e => e.PersonKey).ToList();

            if (details.Count > 0) {
                JsonGridModel jgm = new JsonGridModel(typeof(SA_PlanPersonSet).Name);
                jmodel.d.Add(jgm);
                foreach (gzjhryszModel item in details)
                {
                    List<JsonAttributeModel> picker =item.ClassPick();
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
                Guid value = jsonModel.m.Id(new SA_Plan().ModelName());
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
                    result.s = new JsonMessage("提示...", strMsg, JsonModelConstant.Info);
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示...", strMsg, JsonModelConstant.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示...", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }       



    }

    public class gzjhryszModel
    { 
        public string PersonKey {set;get;}
        public string PersonName {set;get;}
        public Guid? GUID { set; get; }
        public Guid? GUID_SA_Plan {set;get;}
        public Guid? GUID_SS_Person { set; get; }
        public Guid? GUID_SS_Bank { set; get; }
        public string BankKey{set;get;}
        public string BankName { set; get; }
    }

}
