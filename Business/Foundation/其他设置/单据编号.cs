using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using BusinessModel;
namespace Business.Foundation.其他设置
{
    public class 单据编号 : BaseDocument
    {
        public 单据编号() : base() { }
        public 单据编号(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        // 并不是真的new一个新的对象出来，而是从数据库中获取唯一的一条数据
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                List<SS_DocNumber> listDocNum = this.BusinessContext.SS_DocNumber.ToList();
                if (listDocNum.Count!=1)
                {
                    return null;
                }

                SS_DocNumber objDocNum = listDocNum[0];
                jmodel.m = objDocNum.Pick();
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override Guid Modify(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_DocNumber objDocNumber = new SS_DocNumber();
            JsonAttributeModel id = jsonModel.m.IdAttribute(objDocNumber.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            objDocNumber = this.BusinessContext.SS_DocNumber.FirstOrDefault(e => e.GUID == g);
            objDocNumber.Fill(jsonModel.m);
            this.BusinessContext.ModifyConfirm(objDocNumber);
            this.BusinessContext.SaveChanges();
            return objDocNumber.GUID;
        }

        public override JsonModel Retrieve(Guid guid)
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                SS_DocNumber objDocNumber = this.BusinessContext.SS_DocNumber.FirstOrDefault(e => e.GUID == guid);
                if (objDocNumber==null)
                {
                    return jmodel;
                }
                jmodel.m = objDocNumber.Pick();
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private SS_DocNumber LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_DocNumber main = new SS_DocNumber();
            main.Fill(jsonModel.m);
            return main;
        }
        private string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_DocNumber main = null; ; //new BX_Main();
            switch (status)
            {
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);//修改验证
                    strMsg = DataVerifyMessage(vResult);
                    break;
            }
            return strMsg;
        }
        protected override VerifyResult ModifyVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            SS_DocNumber objDocNumber = (SS_DocNumber)data;
            List<ValidationResult> resultList = new List<ValidationResult>();

            if (objDocNumber.YearFormat != 4 && objDocNumber.YearFormat != 2)
            {
                resultList.Add(new ValidationResult("", "年格式位数只能为2或4中的一个"));
            }

            if (objDocNumber.Order_Year==null||objDocNumber.Order_Year < 1 || objDocNumber.Order_Year > 4)
            {
                resultList.Add(new ValidationResult("", "年号排序必须填写且只能在1至之间"));
            }
            if (objDocNumber.Order_Month == null || objDocNumber.Order_Month < 1 || objDocNumber.Order_Year > 4)
            {
                resultList.Add(new ValidationResult("", "月排序必须填写且只能在1至之间"));
            }
            if (objDocNumber.Order_DW ==null || objDocNumber.Order_DW < 1 || objDocNumber.Order_Year > 4)
            {
                resultList.Add(new ValidationResult("", "单位排序必须填写且只能在1至之间"));
            }
            if (objDocNumber.Order_YWType == null || objDocNumber.Order_YWType < 1 || objDocNumber.Order_Year > 4)
            {
                resultList.Add(new ValidationResult("", "业务排序必须填写且只能在1至之间"));
            }

            if (resultList.Count==0)
            {
                int iSum = 0;
                int iCount = 0;
                if((bool)objDocNumber.IsYear)
                {
                    iCount++;
                    iSum += (int)objDocNumber.Order_Year;
                }

                if((bool)objDocNumber.IsMonth)
                {
                    iCount++;
                    iSum += (int)objDocNumber.Order_Month;
                }
                if ((bool)objDocNumber.IsDW)
                {
                    iCount++;
                    iSum += (int)objDocNumber.Order_DW;
                }

                if ((bool)objDocNumber.IsYWType)
                {
                    iCount++;
                    iSum += (int)objDocNumber.Order_YWType;
                }

                int iTmp = ((iCount + 1) * iCount) / 2;
                if(iTmp!=iSum)
                {
                    resultList.Add(new ValidationResult("", "启用的排序编号必须连续且不能有重复"));
                }
            }
            result._validation = resultList;

            return result;
        }
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
        public override JsonModel Save(string status, JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            try
            {
                Guid value = jsonModel.m.Id(new SS_DocNumber().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "2": //修改
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Modify(jsonModel);
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
