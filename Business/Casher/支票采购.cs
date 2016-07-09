using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using BusinessModel;
using Platform.Flow.Run;

namespace Business.Casher
{
   
    public class 支票采购 : BaseDocument
    {
        public 支票采购() : base() { }
        public 支票采购(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 创建默认值
        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                CN_CheckNew model = new CN_CheckNew();
                model.FillDefault(this, this.OperatorId, this.ModelUrl);
                model.CheckNewDatetime = DateTime.Now;
                jmodel.m = model.Pick();

                CN_CheckView dModel = new CN_CheckView();
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.d.Add(fjgm);

                //List<JsonAttributeModel> picker = new List<JsonAttributeModel>();
                //SS_BankAccount bankaccount = this.InfrastructureContext.SS_BankAccount.OrderBy(e=>e.BankAccountKey).FirstOrDefault(e=>e.IsStop==false);
                //if (bankaccount != null)
                //{
                //    List<CN_CheckView> checkList = this.InfrastructureContext.CN_CheckView.Where(e=>e.GUID_BankAccount==bankaccount.GUID).ToList();
                //    foreach (CN_CheckView item in checkList)
                //    {

                //        picker.AddRange(item.Pick());
                //    }
                //    fjgm.r.Add(picker);                    
                //}
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                CN_CheckNew model = this.BusinessContext.CN_CheckNew.FirstOrDefault(e=>e.GUID==guid);
                if (model== null)                {
                    model = new CN_CheckNew();
                    model.FillDefault(this, OperatorId);
                }                
                jmodel.m = model.Pick();

                CN_CheckView checkView = this.BusinessContext.CN_CheckView.FirstOrDefault(e => e.GUID_BankAccount == guid);
                if (checkView != null)
                {
                    jmodel.m.AddRange(checkView.Pick());
                }

                //CN_CheckView dModel = new CN_CheckView();               
                //JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                //jmodel.d.Add(fjgm);

                //List<JsonAttributeModel> picker = new List<JsonAttributeModel>();
                //List<CN_CheckView> checkList = this.InfrastructureContext.CN_CheckView.Where(e => e.GUID_BankAccount == guid).ToList();
                //foreach (CN_CheckView item in checkList)
                //{
                //    picker.AddRange(item.Pick());
                //    fjgm.r.Add(picker);
                //}              
                
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

       /// <summary>
       /// 返回实体
       /// </summary>
       /// <param name="guid">银行账户的GUID</param>
        /// <param name="checkNewID">CN_CheckNew 的GUID</param>
       /// <returns></returns>
        public  JsonModel Retrieve(Guid guid,Guid checkNewID)
        {
            JsonModel jmodel = new JsonModel();
            try
            {
                CN_CheckNew model = this.BusinessContext.CN_CheckNew.FirstOrDefault(e => e.GUID == checkNewID);
                if (model == null)
                {
                    model = new CN_CheckNew();
                    model.FillDefault(this, OperatorId);
                }
                jmodel.m = model.Pick();

                CN_CheckView checkView = this.BusinessContext.CN_CheckView.FirstOrDefault(e => e.GUID_BankAccount == guid);
                if (checkView != null)
                {
                    jmodel.m.AddRange(checkView.Pick());
                }

                //CN_CheckView dModel = new CN_CheckView();
                //JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());

                //List<JsonAttributeModel> picker = new List<JsonAttributeModel>();
                //jmodel.d.Add(fjgm);

                //List<CN_CheckView> checkList = this.InfrastructureContext.CN_CheckView.Where(e => e.GUID_BankAccount == guid).ToList();
                //foreach (CN_CheckView item in checkList)
                //{
                //    picker.AddRange(item.Pick());
                //    fjgm.r.Add(picker);
                //}              

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
             
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            CN_CheckNew main = this.BusinessContext.CN_CheckNew.FirstOrDefault(e => e.GUID == guid);
            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
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

            CN_CheckNew checknew = new CN_CheckNew();
            JsonAttributeModel checknewAttr = jsonModel.m.IdAttribute(checknew.ModelName());
            if (checknewAttr == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(checknewAttr.v, out g) == false) return Guid.Empty;
            checknew = this.BusinessContext.CN_CheckNew.FirstOrDefault(e => e.GUID == g);            
            checknew.Fill(jsonModel.m);


            this.BusinessContext.ModifyConfirm(checknew);
            this.BusinessContext.SaveChanges();
            return checknew.GUID;
        }
           
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            CN_CheckNew main = new CN_CheckNew();       
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            int startNum = 0;
            int numberLen = main.StartNumber.Length;
            int.TryParse(main.StartNumber,out startNum);
            int endNum = 0;
            int.TryParse(main.StopNumber,out endNum);

            if (main != null)
            {
                List<string> list = GetCheckNumberList(startNum, endNum, numberLen);
                if (list.Count > 0)
                { 
                    for(int i=0;i<list.Count;i++)
                    {
                        var StateAttr = jsonModel.m.Find(e => e.n.ToLower() == "checknewstate");
                        var DatetimeAttr = jsonModel.m.Find(e => e.n.ToLower() == "checknewdatetime");
                        CN_Check checkModel = new CN_Check();
                        checkModel.Fill(jsonModel.m);
                        checkModel.CheckNumber=list[i];
                        checkModel.GUID = Guid.NewGuid();
                        checkModel.IsInvalid = true;
                        //if (StateAttr != null)
                        //{
                        //    checkModel.IsInvalid = StateAttr.v == "1" ? true : false;
                        //}
                        if (DatetimeAttr != null)
                        {
                            DateTime value;
                            if (DateTime.TryParse(DatetimeAttr.v, out value))
                            {
                                checkModel.InvalidDatetime = value;
                            }
                            
                        }
                        checkModel.CheckType = main.CheckType == null ? 0 : (int)main.CheckType;
                        this.BusinessContext.CN_Check.AddObject(checkModel);
                    }
                }
            }
            this.BusinessContext.CN_CheckNew.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 根据时间段创建发票的号码
        /// </summary>
        /// <param name="startNumber"></param>
        /// <param name="endNumber"></param>
        /// <returns></returns>
        private List<string> GetCheckNumberList(int startNumber,int endNumber,int length)
        {           
            List<string> list = new List<string>();
            string checkNumber=string.Empty;
            int count = (endNumber - startNumber)+1;
            if (count <=0) return list;            
            for (int i = 0; i < count; i++)
            {
                checkNumber = string.Format("{0:D" + length + "}", startNumber + i, (startNumber + i).ToString().Length);
                list.Add(checkNumber);
            }
           return list;

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
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                Guid value = jsonModel.m.Id(new CN_CheckNew().ModelName());
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
                        result = this.New();
                        strMsg = "删除成功！";
                    }
                    else
                    {
                        CN_Check checkModel = LoadCheckMain(jsonModel);
                        if (checkModel != null)
                        {
                            result = this.Retrieve(checkModel.GUID_BankAccount,value);
                        }
                        strMsg = "保存成功！";
                    }
                    OperatorLog.WriteLog(this.OperatorId, value, status, "支票采购", data);
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
                OperatorLog.WriteLog(this.OperatorId, "支票采购", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
       
        #region 验证
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
            CN_CheckNew main = LoadMain(jsonModel);
            CN_Check checkModel = LoadCheckMain(jsonModel);
            switch (status)
            {
                case "1": //新建                    
                    vResult =DataVerify(main,checkModel);
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        for (int i = 0; i < vResult.Validation.Count; i++)
                        {
                            strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                        }
                    }
                    break;
                case "2": //修改
                    vResult = DataVerify(main, checkModel);
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        for (int i = 0; i < vResult.Validation.Count; i++)
                        {
                            strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                        }
                    }
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new CN_CheckNew().ModelName());
                    vResult = DeleteVerify(value);
                    if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                    {
                        strMsg = vResult.Validation[0].Message + "<br>";//"\n";
                    }
                    break;

            }
            return strMsg;
        }
        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private CN_CheckNew LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            CN_CheckNew main = new CN_CheckNew();
            main.Fill(jsonModel.m); 
            return main;
        }
        /// <summary>
        /// 支票Model
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private CN_Check LoadCheckMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            CN_Check main = new CN_Check();
            main.Fill(jsonModel.m);
            return main;
        }
   
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(CN_CheckNew data,CN_Check check)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            CN_CheckNew mModel = data;
            object g;
            #region 支票信息
            if (check.GUID_BankAccount.IsNullOrEmpty())
            {
                str = "账户名称 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(check.GUID_BankAccount.GetType(), check.GUID_BankAccount.ToString(), out g) == false)
                {
                    str = "账户名称 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }


            #endregion


            #region   主表字段验证

            //开始票号
            if (string.IsNullOrEmpty(mModel.StartNumber))
            {
                str = "开始票号 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            //结束票号
            if (string.IsNullOrEmpty(mModel.StopNumber))
            {
                str = "结束票号 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }

            if (mModel.CheckType == null)
            {
                str = "支票类型 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }

            if (mModel.CheckNewState == null)
            {
                str = "支票状态 字段为必输项！";
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
        protected  VerifyResult DataVerify(CN_CheckNew data, CN_Check check)
        {
            VerifyResult result = new VerifyResult();          
            //主Model验证
            var vf_main = VerifyResultMain(data,check);
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
            CN_CashMain bxMain = new CN_CashMain();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (guid.IsNullOrEmpty())
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
                    str = "此单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
           
            return result;
        }

       
        #endregion

    }
}
