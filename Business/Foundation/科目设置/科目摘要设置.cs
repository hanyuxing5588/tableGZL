using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.科目设置
{
    public class 科目摘要设置 : BaseDocument
    {
        public 科目摘要设置() : base() { }
        public 科目摘要设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            //根据模型名称得到Gird中的数据集，进行循环
            //将预算科目中的GUID与摘要表的GUID_BGCode进行匹配，根据对应的GUID做相应的添加
            if (jsonModel.d.Count > 0 && jsonModel.d != null) {
                SS_BGCodeMemo ssbg = new SS_BGCodeMemo();
                string bgCodeMemoName = ssbg.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(bgCodeMemoName);
                if (Grid != null) {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        SS_BGCodeMemo bgCode = new SS_BGCodeMemo();
                        bgCode.Fill(row);
                        bgCode.GUID = Guid.NewGuid();
                        this.InfrastructureContext.SS_BGCodeMemo.AddObject(bgCode);
                    }
                }
            }
            this.InfrastructureContext.SaveChanges();
            return Guid.Empty;

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
            SS_BGCodeMemo main = new SS_BGCodeMemo(); SS_BGCodeMemo tempdetail = new SS_BGCodeMemo();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            //默认值，不需要
            //main = this.InfrastructureContext.SS_BGCodeMemo.FirstOrDefault(e => e.GUID == g);
            //main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);

            string detailModelName = tempdetail.ModelName();
            //在修改时，如果要删除一行的数据，那么就将这条数据所对应科目表的GUID一块删除。
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            var listDetail = this.InfrastructureContext.SS_BGCodeMemo.Where(e => e.GUID_BGCode == main.GUID_BGCode).ToList();
            if (Grid == null)
            {
                foreach (SS_BGCodeMemo detail in listDetail) { this.InfrastructureContext.DeleteConfirm(detail); }
            }
                //不删除走下面
            else
            {
                 //如果是Update时，将走下面的             
                foreach (SS_BGCodeMemo detail in listDetail)
                {
                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.InfrastructureContext.DeleteConfirm(detail);
                    else
                    {
                        detail.Fill(row);
                        //this.BusinessContext.ModifyConfirm(detail);
                        this.InfrastructureContext.ModifyConfirm(detail);
                    }
                }
                //在修改时，如果要新增一条数据，不管什么科目，就走下面的。
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    SS_BGCodeMemo bgCode = new SS_BGCodeMemo();
                    bgCode.Fill(row);
                    bgCode.GUID = Guid.NewGuid();
                    this.InfrastructureContext.SS_BGCodeMemo.AddObject(bgCode);
                }
            }
            this.InfrastructureContext.SaveChanges();
            return main.GUID_BGCode;


        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            JsonModel result = new JsonModel();
            try
            {
                //SS_BGCodeMemo obj = this.InfrastructureContext.SS_BGCodeMemo.FirstOrDefault(e => e.GUID_BGCode == guid);
                var listDetail = this.InfrastructureContext.SS_BGCodeMemo.Where(e => e.GUID_BGCode == guid).ToList();
                if (listDetail.Count != 0)
                {
                    foreach (SS_BGCodeMemo detail in listDetail) { this.InfrastructureContext.DeleteConfirm(detail); }
                }
                InfrastructureContext.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
            
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid) {
            //todo 列表数据返回
            var jmodel = new JsonModel();
            var details = this.InfrastructureContext.SS_BGCodeMemoView.Where(e=>e.GUID_BGCode==guid).OrderBy(e=>e.BGCodeKey).ToList();
            if (details.Count > 0)
            {
                JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                jmodel.d.Add(jgm);
                foreach (SS_BGCodeMemoView detail in details)
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
                Guid value = jsonModel.m.Id(new SS_BGCodeMemo().ModelName());
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
                        // strMsg = DataVerify(jsonModel, status);
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

        #region 科目摘要设置验证

        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_BGCodeMemoView main = null;
            switch (status)
            {
                case "1":
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2":
                    //main = LoadMain(jsonModel);
                    //vResult = ModifyVerify(main);   //修改
                    //strMsg = DataVerifyMessage(vResult);
                    break;
                case "3":
                    Guid value = jsonModel.m.Id(new SS_BGCodeMemo().ModelName());
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
        private SS_BGCodeMemoView LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_BGCodeMemoView main = new SS_BGCodeMemoView();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                SS_BGCodeMemoView temp = new SS_BGCodeMemoView();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new SS_BGCodeMemoView();
                        temp.Fill(row);
                        main = temp;
                    }
                }
            }

            return main;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_BGCodeMemoView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //返回一条指定的数据
            //SS_BGCodeMemo ssKey = this.InfrastructureContext.SS_BGCodeMemo.FirstOrDefault(e => e.BGCodeKey == data.BGCodeKey);
            //根据返回来的Key值查找GUID
            var dwGuid = Guid.Empty;
            //if (ssKey != null)
            //{
            //    dwGuid = ssKey.GUID;
            //}
            SS_BGCodeMemoView mModel = data;
            object g;

            #region   主表字段验证

            return resultList;
            #endregion
        }

        /// <summary>
        /// datagrid验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetail(SS_BGCodeMemoView data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //datagrid验证
            List<SS_BGCodeMemoView> detailList = new List<SS_BGCodeMemoView>();
            detailList.Add(data);
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (SS_BGCodeMemoView item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_SS_BGCodeMemo(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
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
            SS_BGCodeMemoView model = (SS_BGCodeMemoView)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }

            //datagrid验证
            //SS_BGCode models = (SS_BGCode)data;
            var vf_detail = VerifyResultDetail(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
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
            SS_BGCodeMemoView ssBGCodeMemo;
            var ssBGCodeMemoList = this.InfrastructureContext.SS_BGCodeMemo.Where(e => e.GUID_BGCode == guid).ToList();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();

            #region 删除验证字段

            //报销单GUID
            if (ssBGCodeMemoList.Count == 0)
            {
                str = "没有可删除的数据！";
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
            SS_BGCodeMemoView model = (SS_BGCodeMemoView)data;
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;
        }

        /// <summary>
        /// 明显表验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_SS_BGCodeMemo(SS_BGCodeMemoView data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            SS_BGCodeMemoView item = data;
            /// <summary>
            /// 明细表字段验证

            /// </summary>
            #region 明细表字段验证

           // 预算科目摘要的GUID
            if (item.GUID_BGCode.IsNullOrEmpty())
            {
                str = "科目编码或者科目名称 字段为必输项!";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_BGCode.GetType(), item.GUID_BGCode.ToString(), out g) == false)
                {
                    str = "科目编码或者科目名称 格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            //if (string.IsNullOrEmpty(item.BGCodeKey))
            //{
            //    str = "预算科目编码 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            //}
            //if (string.IsNullOrEmpty(item.BGCodeName))
            //{
            //    str = "预算科目名称 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            //}
            if (string.IsNullOrEmpty(item.BGCodeMemo))
            {
                str = "预算科目摘要 字段为必填项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            }

            #endregion

            return resultList;
        }


        #endregion

    }
}
