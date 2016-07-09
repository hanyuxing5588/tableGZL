using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure;
using System.Reflection;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.ComponentModel;
using BusinessModel;
using Business.CommonModule;
using OAModel;

namespace Business.Common
{
    /// <summary>
    /// 单据超类
    /// </summary>
    public abstract class BaseDocument
    {
        public static BaseDocument CreatInstance(string DocTypeUrl, Guid OperatorId)
        {
            string temp = (DocTypeUrl+"").ToLower();
            switch (temp)
            {

              
                #region 报表
                case "xmjdylb":     //项目进度一览表
                case "yskmxb":      //应收款明细表
                case "czbkszylb":   //财政拨款手指一览表    
                #endregion
                #region 报销
                case "clbxd":       //差旅报销单



                    return new Reimbursement.差旅报销单(OperatorId, DocTypeUrl);
                case "sxfbxd":
                    return new Reimbursement.手续费报销单(OperatorId, DocTypeUrl);
                case "nbdzd":
                    return new Reimbursement.内部调帐单(OperatorId, DocTypeUrl);
                case "xjbxd":
                    return new Reimbursement.现金报销单(OperatorId, DocTypeUrl);
                case "lwflkd":
                    return new Reimbursement.劳务费领款单(OperatorId, DocTypeUrl);
                case "lsggzd":
                    return new Reimbursement.临时工工资单(OperatorId, DocTypeUrl);
                case "qcbxd":
                    return new Reimbursement.期初报销单(OperatorId, DocTypeUrl);
                case "qtbxd":
                    return new Reimbursement.其他报销单(OperatorId, DocTypeUrl);
                case "hkspd":
                    return new Reimbursement.汇款审批单(OperatorId, DocTypeUrl);
                case "gwkbxd":
                    return new Reimbursement.公务卡报销单(OperatorId, DocTypeUrl);
                case "gwkhzbxd":
                    return new Reimbursement.公务卡汇总报销单(OperatorId, DocTypeUrl);
                case "zpsld":
                    return new Reimbursement.支票申领单(OperatorId, DocTypeUrl);
                case "bxdlb":
                    return new Reimbursement.报销单列表(OperatorId, DocTypeUrl);
                case "jkdtz":
                    return new Reimbursement.借款单填制(OperatorId, DocTypeUrl);
                case "skpd":
                    return new Reimbursement.收款凭单(OperatorId, DocTypeUrl);
                case "yfd":
                    return new Reimbursement.应付单(OperatorId, DocTypeUrl);
                case "djlb":
                    return new Reimbursement.单据列表(OperatorId, DocTypeUrl);
                case "zyjjlzd":
                    return new Reimbursement.专用基金列支单(OperatorId, DocTypeUrl);
                #endregion

                case "borrow"://借款
                    return new CommonModule.借款记录(OperatorId, DocTypeUrl);
                case "budgetstatistics"://预算
                    //return new Reimbursement.CommonMethod(OperatorId, DocTypeUrl);//预算统计
                    return new CommonModule.预算统计(OperatorId, DocTypeUrl);
                case "history"://历史
                    return new CommonModule.历史记录(OperatorId, DocTypeUrl);

                #region 出纳
                case "xjtq":
                    return new Casher.现金提取(OperatorId, DocTypeUrl);
                case "xjcc":
                    return new Casher.现金存储(OperatorId, DocTypeUrl);
                case "cnfkd":
                    return new Casher.出纳付款单(OperatorId, DocTypeUrl);
                case "cnskd":
                    return new Casher.出纳收款单(OperatorId, DocTypeUrl);
                case "hxcl":
                    return new Casher.核销处理(OperatorId, DocTypeUrl);
                case"zpcg":
                    return new Casher.支票采购(OperatorId,DocTypeUrl);
                case "zpgl":
                    return new Casher.支票管理(OperatorId,DocTypeUrl);
                case"zplq":
                    return new Casher.支票领取(OperatorId,DocTypeUrl);
                case "zpdjb":
                    return new Casher.支票登记薄(OperatorId,DocTypeUrl);
                #endregion
                #region 会计
                case "ysdtz":
                    return new Accountant.应收单填制(OperatorId, DocTypeUrl);
                case "yfdtz":
                    return new Accountant.应付单填制(OperatorId, DocTypeUrl);
                case "kjpz":
                    return new Accountant.会计凭证(OperatorId,DocTypeUrl);
                case "lwfgsydhz":
                    return new Accountant.劳务费个税月度汇总(OperatorId,DocTypeUrl);
                case "dwrylkd":
                    return new Accountant.单位人员领款单(OperatorId,DocTypeUrl);
                case"gzd":
                    return new Accountant.工资发放(OperatorId,DocTypeUrl);
                case "lkxhz":
                    return new Accountant.类款项汇总(OperatorId,DocTypeUrl);
                case "gzlkxsz":
                    return new Accountant.工资类款项设置(OperatorId,DocTypeUrl);
                #endregion
                #region 预算
                case "ystz":     //预算调整
                    return new Budget.预算编制(OperatorId, DocTypeUrl);
                case "yskz":     //预算控制
                    return new Budget.预算控制(OperatorId, DocTypeUrl);
                case "ysfp":     //预算分配   东升添加 2014-4-9
                    return new Budget.预算分配(OperatorId, DocTypeUrl);
                case "ysbz":     // 预算编制 东升添加 2014-4-11
                    return new Budget.预算编制(OperatorId, DocTypeUrl);
                case "yscszsz":
                    return new Budget.预算初始值设置(OperatorId, DocTypeUrl);
                #endregion
                #region 收入
                case "srpd":
                    return new Income.收入凭单(OperatorId, DocTypeUrl);
                case "czsr":
                    return new Income.财政收入(OperatorId, DocTypeUrl);
                #endregion
                #region 基础  zzp

                    //组织机构
                case "dwda":        //单位档案
                    return new Foundation.组织机构.单位档案(OperatorId, DocTypeUrl);
                case "bmda":        //部门档案
                    return new Foundation.组织机构.部门档案(OperatorId, DocTypeUrl);
                case "ryda":        //人员档案
                    return new Foundation.组织机构.人员档案(OperatorId, DocTypeUrl);

                    //项目设置
                case "xmfl":        //项目分类
                    return new Foundation.项目设置.项目分类(OperatorId, DocTypeUrl);
                case"xmda":         //项目档案
                    return new Foundation.项目设置.项目档案(OperatorId, DocTypeUrl);

                case "kjkmsz":         //项目档案
                    return new Foundation.会计科目设置.会计科目设置(OperatorId, DocTypeUrl);

                    //用户角色
                case "jssz":        //角色设置
                    return new Foundation.角色用户.角色设置(OperatorId, DocTypeUrl);
                case "yhfz":        //用户分组
                    return new Foundation.角色用户.用户分组(OperatorId, DocTypeUrl);
                case "yhsz":        //用户设置
                    return new Foundation.角色用户.用户设置(OperatorId, DocTypeUrl);

                    //科目设置
                case "yskmzb":      //预算科目总表
                    return new Foundation.科目设置.预算科目总表(OperatorId, DocTypeUrl);
                case"yskmsz":       //预算科目设置
                    return new Foundation.科目设置.预算科目设置(OperatorId, DocTypeUrl);
                case "kmzysz":       //科目摘要设置
                    return new Foundation.科目设置.科目摘要设置(OperatorId, DocTypeUrl);
                
                    //薪酬设置
                case "gzxmsz":      //工资项目设置
                    return new Foundation.薪酬设置.工资项目设置(OperatorId, DocTypeUrl);
                case"gzjhsz":       //工资计划设置
                    return new Foundation.薪酬设置.工资计划设置(OperatorId, DocTypeUrl);
                case "gzjhrysz":    //工资计划人员设置
                    return new Foundation.薪酬设置.工资计划人员设置(OperatorId, DocTypeUrl);
                case "rygzmrzsz":    //人员工资默认值设置
                    return new Foundation.薪酬设置.人员工资默认值设置(OperatorId, DocTypeUrl);
                case "gzxsjjzfssz":    //工资项数据加载方式设置
                    return new Foundation.薪酬设置.工资项数据加载方式设置(OperatorId, DocTypeUrl);

                case "wldw":      //往来单位档案
                    return new Foundation.往来单位档案(OperatorId, DocTypeUrl);

                    //桌面设置
                case "tzgg":    //通知公告
                    return new Foundation.桌面设置.通知公告(OperatorId, DocTypeUrl);
                case "zcfg":
                    return new Foundation.桌面设置.政策法规(OperatorId, DocTypeUrl);
                case "wjlx":
                    return new Foundation.桌面设置.文件类型(OperatorId, DocTypeUrl);

                #endregion
                #region 其他设置

                case "wpryda":  //外聘人员档案
                    return new Foundation.其他设置.外聘人员档案(OperatorId, DocTypeUrl);
                case "wldwda":  //往来单位档案
                    return new Foundation.其他设置.往来单位档案(OperatorId, DocTypeUrl);
                case"ztsz": //帐套设置
                    return new Foundation.其他设置.帐套设置(OperatorId, DocTypeUrl);
                case"ztzbsz"://帐套子表设置
                    return new Foundation.其他设置.帐套字表设置(OperatorId, DocTypeUrl);
                case "djbh":   // 单据编号
                    return new Foundation.其他设置.单据编号(OperatorId, DocTypeUrl);
                case "zclx":   // 支出类型
                    return new Foundation.其他设置.支出类型(OperatorId, DocTypeUrl);
                case "jsfssz":   // 结算方式设置
                    return new Foundation.其他设置.结算方式设置(OperatorId, DocTypeUrl);
                case "sklxsz":   // 收款类型设置
                    return new Foundation.其他设置.收款类型设置(OperatorId, DocTypeUrl);
                case "srlxsz":   // 收入类型设置
                    return new Foundation.其他设置.收入类型设置(OperatorId, DocTypeUrl);
                case "rylbsz":   // 人员类别设置
                    return new Foundation.其他设置.人员类别设置(OperatorId, DocTypeUrl);
                case "zjlxsz":   // 证件类型设置
                    return new Foundation.其他设置.证件类型设置(OperatorId, DocTypeUrl);
                case "gnfl":   // 功能分类
                    return new Foundation.其他设置.功能分类(OperatorId, DocTypeUrl);
                case "jtgj":   // 交通工具

                    return new Foundation.其他设置.交通工具(OperatorId, DocTypeUrl);
                case "ccbzbz":   // 出差补助标准
                    return new Foundation.其他设置.出差补助标准(OperatorId, DocTypeUrl);
                case "yhzh":   // 银行账号
                    return new Foundation.其他设置.银行账号(OperatorId, DocTypeUrl);
                case "yhda":   // 银行设置
                    return new Foundation.其他设置.银行档案(OperatorId, DocTypeUrl);
                case "ysyflx":   // 应收应付类型
                    return new Foundation.其他设置.应收应付类型(OperatorId, DocTypeUrl);
                case "cdwh":   // 菜单维护
                    return new Foundation.其他设置.菜单维护(OperatorId, DocTypeUrl);
                #endregion

            }
            return null;
        }

        private BusinessModel.BusinessEdmxEntities _bcontext = null;
        public BusinessModel.BusinessEdmxEntities BusinessContext
        {
            get
            {
                if (_bcontext == null) _bcontext = new BusinessEdmxEntities();
                return _bcontext;
            }
        }

        private Infrastructure.BaseConfigEdmxEntities _icontext = null;
        public Infrastructure.BaseConfigEdmxEntities InfrastructureContext
        {
            get
            {
                if (_icontext == null) _icontext = new BaseConfigEdmxEntities();
                return _icontext;
            }
        }

        private OAModel.OAEntities _oacontext = null;
        public OAModel.OAEntities OAContext
        {
            get
            {
                //if (_oacontext == null) _icontext = new BaseConfigEdmxEntities();
                if (_oacontext == null) _oacontext = new OAModel.OAEntities();
                return _oacontext;
            }
        }
        public string _modelUrl = string.Empty;

        public string ModelUrl
        {
            get { return _modelUrl; }
        }

        public string ErrorCode = string.Empty;

        protected Guid _operatorId = Guid.Empty;
        /// <summary>
        /// 操作员





        /// </summary>
        public Guid OperatorId
        {
            get { return _operatorId; }
            set { _operatorId = value; }
        }

        protected BaseDocument() { }
        protected BaseDocument(Guid OperatorId, string ModelUrl) { this._operatorId = OperatorId; this._modelUrl = ModelUrl; }

        /// <summary>
        /// 返回新建单据的默认值





        /// </summary>
        /// <returns></returns>
        public virtual JsonModel New()
        {

            return null;
        }
        /// <summary>
        /// 根据单据guid返回单据详细信息
        /// </summary>
        /// <param name="guid">单据guid</param>
        /// <returns></returns>
        public virtual JsonModel Retrieve(Guid guid)
        {
            return null;
        }
        /// 根据人员或工资项guid返回单据详细信息
        /// </summary>
        /// <param name="guid">人员或工资项guid</param>
        /// <param name="type">人员或工资项type</param>
        /// <returns></returns>
        public virtual JsonModel Retrieve(Guid guid,string type)
        {
            return null;
        }
        /// <summary>
        /// 根据单据guid返回单据详细信息
        /// </summary>
        /// <param name="guid">单据guid</param>
        /// <returns></returns>
        public virtual JsonModel Retrieve(string guid)
        {
            return null;
        }
        /// <summary>
        /// 信息插入数据库操作




        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        protected virtual Guid Insert(JsonModel jsonModel) { return Guid.Empty; }
        public virtual Guid Insert(JsonModel jsonModel,bool IsSave=true) { return Guid.Empty; }
        /// <summary>
        /// 信息从数据库中删除





        /// </summary>
        /// <param name="guid">主单据guid</param>
        /// <returns></returns>
        protected virtual void Delete(Guid guid) { }
        /// <summary>
        /// 信息更新到数据库
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        protected virtual Guid Modify(JsonModel jsonModel) { return Guid.Empty; }
        /// <summary>
        /// 保存单据到数据库
        /// </summary>
        /// <param name="status">单据状态</param>
        /// <param name="jsonModel">单据数据</param>
        /// <returns></returns>
        public virtual JsonModel Save(string status, JsonModel jsonModel)
        {
            return null;
        }
        public virtual object SaveWithReturnObj(string status, JsonModel jsonModel)
        {
            return null;
        }
        /// <summary>
        /// 修改单据状态
        /// </summary>
        /// <param name="guid">单据GUID</param>
        /// <param name="docState">单据状态</param>
        /// <returns>bool</returns>
        public virtual bool UpdateDocState(Guid guid, Business.Common.EnumType.EnumDocState docState)
        {
            return false;
        }
       
        /// <summary>
        /// 获取流程代办显示信息
        /// </summary>
        /// <param name="guid">主表Guid</param>
        /// <returns>代办信息</returns>
        public virtual Dictionary<string, string> ProcessBoxInormation(Guid guid)
        {
            if (guid == null || guid == Guid.Empty)
            {
                return null;
            }


            BX_MainView main = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);

            if (main == null)
            {
                return null;
            }
            IQueryable<BX_Detail> q = BusinessContext.BX_Detail.Where(e => e.GUID_BX_Main == guid);
            double money = 0;
            if (q != null)
            {
                money = q.Sum(e => e.Total_Real);
            }



            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("creator", main.PersonName);
            result.Add("doctypename", main.DocTypeName);
            result.Add("docnum", main.DocNum);
            result.Add("money", money.ToString());
            return result;
        }
        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual VerifyResult InsertVerify(object data)
        {
            return VerifyResult.Sucess;
        }
        /// <summary>
        /// 数据从数据库删除前验证





        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected virtual VerifyResult DeleteVerify(Guid guid)
        {
            return VerifyResult.Sucess;
        }
        /// <summary>
        /// 数据更新到数据库验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual VerifyResult ModifyVerify(object data)
        {
            return VerifyResult.Sucess;
        }
        /// <summary>
        /// 获取单据历史信息
        /// </summary>
        /// <param name="conditions">查询条件</param>
        /// <returns></returns>
        public virtual List<object> History(SearchCondition conditions)
        {
            return null;
        }
        /// <summary>
        /// 借款单信息





        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>List Object</returns>
        public virtual List<BorrowModel> BorrowMoney(SearchCondition conditions)
        {
            return null;
        }
        /// <summary>
        /// 预算
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns>JsonModel</returns>
        public virtual List<BudgetModel> BudgetStatistics(Guid operatorId, Guid docGuid, List<BudgetModel> conditions, int year, string ywKey)
        {
            return null;
        }
        //核销的单据转换


        public virtual object ChangeDocData(string guids, string docType,bool isDC,DateTime dt) 
        {
            return null;
        }
        public virtual List<object> BorrowList()
        {
            return null;
        }
        public virtual List<object> BorrowList(string userBorrow)
        {
            return null;
        }
        public virtual Platform.Flow.Run.ResultMessager WorkFlowCommitVerify(Guid dataId) { return null; }
    }

    /// <summary>
    /// 报销单总类
    /// </summary>
    public abstract class BXDocument : BaseDocument
    {

        protected BXDocument() { }
        protected BXDocument(Guid OperatorId, string ModelUrl) { this._operatorId = OperatorId; this._modelUrl = ModelUrl; }
        public string GetGLDocNum(Guid main)
        {
            try
            {
                var sql = string.Format("SELECT dbo.fn_txtsum('{0}')", main);
                var dt = DataSource.ExecuteQuery(sql);
                if (dt != null)
                {
                    return (dt.Rows[0][0] + "").ToString();
                }
                //data
            }
            catch (Exception)
            {
                
                throw;
            }
            return "";
        }

        private List<SS_BaseSetInfView> _BaseSetInfViews = null;
        protected List<SS_BaseSetInfView> BaseSetInfViews(Guid GuidDw)
        {
            if (_BaseSetInfViews == null)
            {
                IQueryable<SS_BaseSetInfView> query = this.BusinessContext.SS_BaseSetInfView.Where(e => e.GUID_DW == GuidDw);
                if (query != null) _BaseSetInfViews = query.ToList<SS_BaseSetInfView>();
            }
            return _BaseSetInfViews;
        }

        protected bool CheckPaymentNumberInSave(Guid GuidDw)
        {
            var infs = this.BaseSetInfViews(GuidDw);
            if (infs == null) return false;
            var item = infs.FirstOrDefault(e => e.SetTypeKey.ToLower() == "submitcheck" && e.SetKey == "001");
            if (item == null) return false;
            return item.SetValue == "1" ? true : false;
        }
        protected SS_BaseSetInfView CheckPaymentNumberInSumitFlow(Guid GuidDw)
        {
            var infs = this.BaseSetInfViews(GuidDw);
            if (infs == null) return null;
            var item = infs.FirstOrDefault(e => e.SetTypeKey.ToLower() == "submitcheck" && e.SetKey == "002");
            return item;
        }
        /// <summary>
        /// 支付码验证


        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual List<ValidationResult> VerifyResult_CN_PaymentNumber(CN_PaymentNumber data, int rowIndex, Guid GuidDw)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            /// <summary>
            /// 财富支付码表字段验证
            /// </summary>
            #region 财富支付码表字段验证


            if (!string.IsNullOrEmpty(data.PaymentNumber) && Common.ConvertFunction.TryParse(data.PaymentNumber.GetType(), data.PaymentNumber, out g) == false)
            {
                str = "财政支付码格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //是否国库
            if (data.IsGuoKu.ToString() == "")
            {
                str = "是否国库 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(data.IsGuoKu.GetType(), data.IsGuoKu.ToString(), out g) == false)
                {
                    str = "是否国库格式不能为空！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {
                    var checkpaymentnumber = this.CheckPaymentNumberInSave(GuidDw);
                    //如果不为空则,则支付码不能为空
                    if (checkpaymentnumber == true && data.IsGuoKu == true && string.IsNullOrEmpty(data.PaymentNumber))
                    {
                        str = "财政支付令不能为空！";
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //是否项目
            if (data.IsProject.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.IsProject.GetType(), data.IsProject.ToString(), out g) == false)
            {
                str = "项目格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //功能分类GUID
            if (data.GUID_FunctionClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_FunctionClass.GetType(), data.GUID_FunctionClass.ToString(), out g) == false)
            {
                str = "功能分类格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //预算科目GUID
            if (data.GUID_BGCode.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGCode.GetType(), data.GUID_BGCode.ToString(), out g) == false)
            {
                str = "预算科目格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //经济分类GUID
            if (data.GUID_EconomyClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_EconomyClass.GetType(), data.GUID_EconomyClass.ToString(), out g) == false)
            {
                str = "经济分类格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //支出类型GUID
            if (data.GUID_ExpendType.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_ExpendType.GetType(), data.GUID_ExpendType.ToString(), out g) == false)
            {
                str = "支出类型格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //项目GUID
            if (data.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_Project.GetType(), data.GUID_Project.ToString(), out g) == false)
            {
                str = "项目格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //项目财政编号
            if (!string.IsNullOrEmpty(data.FinanceProjectKey) && Common.ConvertFunction.TryParse(data.FinanceProjectKey.GetType(), data.FinanceProjectKey.ToString(), out g) == false)
            {
                str = "项目财政编号格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //预算来源GUID
            if (data.GUID_BGResource.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGResource.GetType(), data.GUID_BGResource.ToString(), out g) == false)
            {
                str = "预算来源格式不能为空！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }

            #endregion

            return resultList;
        }
        /// <summary>
        /// 流程提交前验证

        /// </summary>
        /// <returns></returns>
        public override Platform.Flow.Run.ResultMessager WorkFlowCommitVerify(Guid dataId)
        {
            Platform.Flow.Run.ResultMessager resultMsg = new Platform.Flow.Run.ResultMessager();
            resultMsg.Title = "系统提示";
            resultMsg.Resulttype = 0;
            var errorstr = "";
            //获取当前流程节点
            var curflownode = Platform.Flow.Run.WorkFlowAPI.GetCurNodeByDocId(dataId, out errorstr);
            if (curflownode == null) return resultMsg;
            //获取配置选项
            var dw = this.GetDW(dataId);
            if (dw.IsNullOrEmpty()) return resultMsg;
            var basesetinf = this.CheckPaymentNumberInSumitFlow(dw);
            if (basesetinf == null) return resultMsg;
            if ((curflownode.WorkFlowNodeName+"").ToLower() != basesetinf.SetValue.ToLower()) return resultMsg;
            var PaymentNumbers = this.GetPaymentNumbers(dataId);
            VerifyResult vr = new VerifyResult();
            //财政支付令验证

            WorkFlowCommit_Verify_CN_PaymentNumber(vr, PaymentNumbers);
            
            if (vr.Validation.Count > 0)
            {
                resultMsg.Resulttype = 1;
                resultMsg.Msg = vr.Message();
            }
            return resultMsg;
        }

        protected virtual Guid GetDW(Guid dataId)
        {
            var main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == dataId);
            return main==null?new Guid():main.GUID_DW;
        }

        protected virtual List<CN_PaymentNumber> GetPaymentNumbers(Guid dataId)
        {
            List<CN_PaymentNumber> results = new List<CN_PaymentNumber>();
            var main = this.BusinessContext.BX_Main.Include("BX_Detail").FirstOrDefault(e => e.GUID == dataId);
            foreach (BX_Detail item in main.BX_Detail) results.Add(item.CN_PaymentNumber);
            return results;
        }

        protected virtual void WorkFlowCommit_Verify_CN_PaymentNumber(VerifyResult verifyResult, List<CN_PaymentNumber> PaymentNumbers)
        {
            //财政支付令验证

            var rowIndex = 0;
            foreach (CN_PaymentNumber item in PaymentNumbers)
            {
                rowIndex++;
                if (item.IsGuoKu == true && string.IsNullOrEmpty(item.PaymentNumber))
                {
                    verifyResult.Validation.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "财政支付令不能为空！"));
                }
            }
        }
    }
    /// <summary>
    /// 往来业务(报销)单据
    /// </summary>
    public abstract class WLDocument : BXDocument
    {
        protected WLDocument() { }
        protected WLDocument(Guid OperatorId, string ModelUrl) { this._operatorId = OperatorId; this._modelUrl = ModelUrl; }

        protected override Guid GetDW(Guid dataId)
        {
            var main = this.BusinessContext.WL_Main.FirstOrDefault(e => e.GUID == dataId);
            return main == null ? new Guid() : main.GUID_DW;
        }

        protected override List<CN_PaymentNumber> GetPaymentNumbers(Guid dataId)
        {
            List<CN_PaymentNumber> results = new List<CN_PaymentNumber>();
            var main = this.BusinessContext.WL_Main.Include("WL_Detail").FirstOrDefault(e => e.GUID == dataId);
            foreach (WL_Detail item in main.WL_Detail) results.Add(item.CN_PaymentNumber);
            return results;
        }
    }

    public abstract class SKDocument : BXDocument
    {
        protected SKDocument() { }
        protected SKDocument(Guid OperatorId, string ModelUrl) { this._operatorId = OperatorId; this._modelUrl = ModelUrl; }

        protected override Guid GetDW(Guid dataId)
        {
            var main = this.BusinessContext.SK_Main.FirstOrDefault(e => e.GUID == dataId);
            return main == null ? new Guid() : main.GUID_DW;
        }

        protected override List<CN_PaymentNumber> GetPaymentNumbers(Guid dataId)
        {
            List<CN_PaymentNumber> results = new List<CN_PaymentNumber>();
            var main = this.BusinessContext.SK_Main.FirstOrDefault(e => e.GUID == dataId);
            results.Add(main.CN_PaymentNumber);
            return results;
        }

        protected override void WorkFlowCommit_Verify_CN_PaymentNumber(VerifyResult verifyResult, List<CN_PaymentNumber> PaymentNumbers)
        {
            foreach (CN_PaymentNumber item in PaymentNumbers)
            {
                if (item.IsGuoKu == true && string.IsNullOrEmpty(item.PaymentNumber))
                {
                    verifyResult.Validation.Add(new ValidationResult("", "财政支付令不能为空！"));
                }
            }
        }
    }

    /// <summary>
    /// 验证结果
    /// </summary>
    public class VerifyResult
    {
        private static VerifyResult _sucess = new VerifyResult();
        /// <summary>
        /// 成功
        /// </summary>
        public static VerifyResult Sucess
        {
            get { return _sucess; }
        }

        public List<ValidationResult> _validation = new List<ValidationResult>();

        public List<ValidationResult> Validation { get { return _validation; } }

        public string Message()
        {
            string strMsg = string.Empty;
            for (int i = 0; i < this._validation.Count; i++)
            {
                strMsg += _validation[i].MemberName + _validation[i].Message + "<br>";//"\n";
            }
            return strMsg;
        }
    }

    /// <summary>
    /// 验证信息
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets the name of the member.
        /// </summary>
        public string MemberName { get; set; }
        /// <summary>
        /// Gets or sets the validation result message.
        /// </summary>
        public string Message { get; set; }

        public ValidationResult() { }
        public ValidationResult(string memberName, string message)
        {
            this.MemberName = memberName;
            this.Message = message;
        }
    }

    /// <summary>
    /// 扩展方法
    /// </summary>
    public static class DocumentModelExtension
    {
        public static object GetValue<T>(this T t, string properName)
        {
           PropertyInfo [] proInof= t.GetType().GetProperties();
           var p= proInof.Where(e => e.Name.ToLower() == properName.ToLower()).FirstOrDefault();
           if (p == null) return null;
            PropertyInfo infos = t.GetType().GetProperty(properName,BindingFlags.Public|BindingFlags.Instance|BindingFlags.IgnoreCase);
            if (infos == null) return null;
            return infos.GetValue(t);

        }
        /// <summary>
        /// 转换字符串到对应正确的数据格式值




        /// </summary>
        /// <param name="PropertyType">属性类型</param>
        /// <param name="s">字符串值</param>
        /// <param name="result">返回值</param>
        /// <returns>true:转换成功 false:转换失败</returns>
        public static bool TryParse(Type PropertyType, string s, out object result)
        {

            string typeName = PropertyType.FullName;
            result = null;
            bool returnbool = false;
            if (typeName.Contains(typeof(string).FullName))
            {
                result = s;
                returnbool = true;
                return returnbool;
            }
            else if (typeName.Contains(typeof(int).FullName))
            {
                int temp;
                returnbool = int.TryParse(s, out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(decimal).FullName))
            {
                decimal temp;
                returnbool = decimal.TryParse(s, out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(float).FullName))
            {
                float temp;
                returnbool = float.TryParse(s, out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(bool).FullName))
            {
                bool temp;
                returnbool = bool.TryParse(s, out temp);
                result = temp;
                if (s == "是")
                {
                    result = true;
                    returnbool = true;
                }
                return returnbool;
            }
            else if (typeName.Contains(typeof(DateTime).FullName))
            {
                DateTime temp;
                returnbool = DateTime.TryParse(s, out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(Guid).FullName))
            {
                Guid temp;
                returnbool = Guid.TryParse(s, out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(double).FullName))
            {
                double temp;
                returnbool = double.TryParse(s.Trim(), out temp);
                result = temp;
                return returnbool;
            }
            else if (typeName.Contains(typeof(byte[]).FullName))
            {
                returnbool = true;
                result = s.ToByteArray();
                return returnbool;
            }
            else if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                returnbool = true;
                result = ConvertToNullType(PropertyType, s);
                return returnbool;
            }
            return returnbool;
        }

        public static CW_PZDetailView ConvertToView(this CW_PZDetail obj, BaseConfigEdmxEntities InfrastructureContext)
        {
            CW_PZDetailView item = new CW_PZDetailView();
            //转换成view
            item.GUID = obj.GUID;
            item.GUID_AccountTitle = obj.GUID_AccountTitle;
            item.GUID_Customer = obj.GUID_Customer;
            item.GUID_Department = obj.GUID_Department;
            item.GUID_Person = obj.GUID_Person;
            item.GUID_Project = obj.GUID_Project;
            item.GUID_PZMAIN = obj.GUID_PZMAIN;
            item.GUID_SettleType = obj.GUID_SettleType;
            item.IsDC = obj.IsDC;
            item.BillDate = obj.BillDate;
            item.BillNum = obj.BillNum;
            item.OrderNum = obj.OrderNum;
            item.PZMemo = obj.PZMemo;
            item.Total_PZ = obj.Total_PZ;

            if (item.GUID_AccountTitle != null)
            {
                
                var celement = InfrastructureContext.CW_AccountTitle.Where(e => e.GUID == item.GUID_AccountTitle).FirstOrDefault();
                if (celement != null)
                {
                    item.AccountTitleKey = celement.AccountTitleKey;
                    item.AccountTitleName = celement.AccountTitleName;
                }
            }
            if (item.GUID_Project != null)
            {
                var celement = InfrastructureContext.SS_Project.Where(e => e.GUID == item.GUID_Project).FirstOrDefault();
                if (celement != null)
                {
                    item.ProjectKey = celement.ProjectKey;
                    item.ProjectName = celement.ProjectName;
                }
            }

            if (item.GUID_Department != null)
            {
                var celement = InfrastructureContext.SS_Department.Where(e => e.GUID == item.GUID_Department).FirstOrDefault();
                if (celement != null)
                {
                    item.DepartmentKey = celement.DepartmentKey;
                    item.DepartmentName = celement.DepartmentName;
                }
            }
            if (item.GUID_Customer != null)
            {
                var celement = InfrastructureContext.SS_Customer.Where(e => e.GUID == item.GUID_Customer).FirstOrDefault();
                if (celement != null)
                {
                    item.CustomerName = celement.CustomerName;
                    item.CustomerKey = celement.CustomerKey;
                }
            }
            if (item.GUID_Person != null)
            {
                var celement = InfrastructureContext.SS_Person.Where(e => e.GUID == item.GUID_Person).FirstOrDefault();
                if (celement != null)
                {
                    item.PersonKey = celement.PersonKey;
                    item.PersonName = celement.PersonName;
                }
            }
            if (item.GUID_SettleType != null)
            {
                var celement = InfrastructureContext.SS_SettleType.Where(e => e.GUID == item.GUID_SettleType).FirstOrDefault();
                if (celement != null)
                {
                    item.SettleTypeKey = celement.SettleTypeKey;
                    item.SettleTypeName = celement.SettleTypeName;
                }
            }

            return item;
        }

        public static object ConvertToNullType(Type convertsionType, object value)
        {
            if (value == null || value.ToString().Length == 0)
            {
                return null;
            }
            //如果ConvertsionType为nullable类，声明一个NullableConverte类，该类提供了Nullable类到基础员类型的转换
            NullableConverter nullableconverter = new NullableConverter(convertsionType);
            //将conertsionType转换为Nullable对应的基础基元类型
            convertsionType = nullableconverter.UnderlyingType;
            //return Convert.ChangeType(value, convertsionType);
            object result;
            TryParse(convertsionType, value.ToString(), out result);
            return result;
        }
        /// <summary>
        /// 判断是否是合法的提取（注入）属性





        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool LegalProeprtyType(this PropertyInfo obj)
        {
            Type itype = obj.PropertyType;
            string typeName = itype.FullName;
            if (typeName.Contains(typeof(string).FullName)) return true;
            if (typeName.Contains(typeof(DateTime).FullName)) return true;
            if (typeName.Contains(typeof(int).FullName)) return true;
            if (typeName.Contains(typeof(double).FullName)) return true;
            if (typeName.Contains(typeof(decimal).FullName)) return true;
            if (typeName.Contains(typeof(float).FullName)) return true;
            if (typeName.Contains(typeof(Guid).FullName)) return true;
            if (typeName.Contains(typeof(bool).FullName)) return true;
            if (typeName.Contains(typeof(byte[]).FullName)) return true;

            //if (itype == typeof(string)) return true;
            //if (itype == typeof(DateTime)) return true;
            //if (itype == typeof(int)) return true;
            //if (itype == typeof(double)) return true;
            //if (itype == typeof(decimal)) return true;
            //if (itype == typeof(float)) return true;
            //if (itype == typeof(Guid)) return true;
            //if (itype == typeof(bool)) return true;
            //if (itype == typeof(byte[])) return true;
            if (itype.IsGenericType && itype.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) return true;
            return false;
        }
        /// <summary>
        /// 判断是否是null或空值 guid=Guid.Empty DateTime=DateTime.MinValue
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsEmptyProperty(object value)
        {
            if (value == null) return true;
            Type mtype = value.GetType();
            if (mtype == typeof(string) && (string)value == string.Empty) return true;
            if (mtype == typeof(Guid) && ((Guid)value) == Guid.Empty) return true;
            if (mtype == typeof(DateTime) && ((DateTime)value) == DateTime.MinValue) return true;
            return false;
        }

        /// <summary>
        /// 扩展string
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string ArrayToString(this byte[] objs)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < objs.Length; i++)
            {
                result.Add(objs[i].ToString());
            }
            return string.Join("-", result);
        }
        /// <summary>
        /// 扩展byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string obj)
        {
            if (obj.Trim() == string.Empty) return null;
            string[] temps = obj.Split('-');
            List<byte> result = new List<byte>();
            for (int i = 0; i < temps.Length; i++)
            {
                result.Add(byte.Parse(temps[i]));
            }
            return result.ToArray();
        }
        /// <summary>
        /// 设置属性值



        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj">将设置其属性值的对象</param>
        /// <param name="value">此属性的新字符串值</param>
        public static void SetValue(this PropertyInfo info, object obj, string value)
        {
            object newvalue;
            if (DocumentModelExtension.TryParse(info.PropertyType, value, out newvalue))
            {
                info.SetValue(obj, newvalue, null);
            }
            else if (info.PropertyType.FullName.Contains(typeof(Guid).FullName) && info.Name.ToLower() == "guid_project")
            {
                info.SetValue(obj, null, null);
            }
        }
        /// <summary>
        /// 返回该属性的值





        /// </summary>
        /// <param name="info"></param>
        /// <param name="obj">将返回其属性值的对象</param>
        /// <returns></returns>
        public static string GetValue(this PropertyInfo info, object obj)
        {
            object value = info.GetValue(obj, null);
            if (value == null) return string.Empty;
            Type vType = value.GetType();
            if (vType == typeof(DateTime)) return ((DateTime)value).ToString("yyyy-MM-dd");
            if (vType == typeof(byte[])) return ((byte[])value).ArrayToString();
            if (vType == typeof(Guid)) return ((Guid)value) == Guid.Empty ? "" : value.ToString();
            return value.ToString();
        }
        public static void SetValue(this PropertyInfo[] infos, object obj, string PropertyName, object value)
        {
            if (infos == null || infos.Length == 0) return;
            PropertyInfo info = infos.FirstOrDefault(e => e.Name.ToLower() == PropertyName.ToLower());
            if (info == null) return;
            info.SetValue(obj, value, null);
        }
        /// <summary>
        /// 赋值时跳过非null或非string.empty或DateTime.MinValue,Guid.Empty的值



        /// </summary>
        /// <param name="infos"></param>
        /// <param name="obj"></param>
        /// <param name="PropertyName"></param>
        /// <param name="value"></param>
        public static void SetValueSkipNotEmpty(this PropertyInfo[] infos, object obj, string PropertyName, object value)
        {
            if (infos == null || infos.Length == 0) return;
            PropertyInfo info = infos.FirstOrDefault(e => e.Name.ToLower() == PropertyName.ToLower());
            if (info == null) return;
            if (DocumentModelExtension.IsEmptyProperty(info.GetValue(obj, null)))
            {
                info.SetValue(obj, value, null);
            }
        }
        /// <summary>
        /// 获得对象的模型名称





        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ModelName<T>(this T obj) where T : EntityObject
        {
            string name = obj.GetType().Name;
            if (name.Trim().ToLower() != "view" && name.Trim().ToLower().EndsWith("view"))
            {
                name = name.Substring(0, name.Length - 4);
            }
            return name;
        }
        /// <summary>
        /// 对数据对象赋值





        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void SetValue<T>(this T obj, string PropertyName, object value) where T : EntityObject
        {
            PropertyInfo[] infos = obj.GetType().GetProperties();
            infos.SetValue(obj, PropertyName, value);
        }
        /// <summary>
        /// 设置模型对象从数据库删除
        /// </summary>
        /// <param name="obj">上下文</param>
        /// <param name="entity">模型对象</param>
        public static void DeleteConfirm(this ObjectContext obj, System.Data.Objects.DataClasses.IEntityWithKey entity)
        {
            obj.Attach(entity);
            obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Deleted);
        }
        /// <summary>
        /// 设置模型对象插入到数据库
        /// </summary>
        /// <param name="obj">上下文</param>
        /// <param name="entity">模型对象</param>
        public static void InsertConfirm(this ObjectContext obj, System.Data.Objects.DataClasses.IEntityWithKey entity)
        {
            obj.Attach(entity);
            obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Added);
        }
        /// <summary>
        /// 设置模型对象更新到数据库
        /// </summary>
        /// <param name="obj">上下文</param>
        /// <param name="entity">模型对象</param>
        public static void ModifyConfirm(this ObjectContext obj, System.Data.Objects.DataClasses.IEntityWithKey entity)
        {
            obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Detached);
            obj.Attach(entity);
            obj.ObjectStateManager.ChangeObjectState(entity, System.Data.EntityState.Modified);
        }

        /// <summary>
        /// 赋默认值





        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="?"></param>
        public static void FillDefault<T>(this T obj, BaseDocument document, Guid OperatorId) where T : EntityObject
        {
            PropertyInfo[] infos = obj.GetType().GetProperties();
            infos.SetValueSkipNotEmpty(obj, "GUID", Guid.NewGuid());
            infos.SetValueSkipNotEmpty(obj, "DocDate", DateTime.Now);
            SS_Operator Operator = OperatorId == Guid.Empty ? null : document.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
            if (Operator != null)
            {
                infos.SetValueSkipNotEmpty(obj, "GUID_Maker", Operator.GUID);
                infos.SetValueSkipNotEmpty(obj, "Maker", Operator.OperatorName);
                infos.SetValueSkipNotEmpty(obj, "MakeDate", DateTime.Now);
                infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
                infos.SetValue(obj, "Modifier", Operator.OperatorName);
                infos.SetValue(obj, "ModifyDate", DateTime.Now);


                //获取默认部门
                var defDepartment = Operator.DefaultDepartment();

                var defDw = Operator.DefaultDW();




                Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
                if (person != null)
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_Person", person.GUID);
                    infos.SetValueSkipNotEmpty(obj, "PersonName", person.PersonName);
                    infos.SetValueSkipNotEmpty(obj, "PersonKey", person.PersonKey);
                    infos.SetValueSkipNotEmpty(obj, "GUID_Department",defDepartment==null?person.GUID_Department:defDepartment.GUID);
                    infos.SetValueSkipNotEmpty(obj, "DepartmentName", defDepartment==null?person.DepartmentName:defDepartment.DepartmentName);
                    infos.SetValueSkipNotEmpty(obj, "DepartmentKey", defDepartment==null?person.DepartmentKey:defDepartment.DepartmentKey);
                    infos.SetValueSkipNotEmpty(obj, "GUID_DW", defDw==null?person.GUID_DW:defDw.GUID);
                    infos.SetValueSkipNotEmpty(obj, "DWName", defDw==null?person.DWName:defDw.DWName);
                    infos.SetValueSkipNotEmpty(obj, "DWKey", defDw==null?person.DWKey:defDw.DWKey);
                }

                var doctype = document.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl.ToLower() == document.ModelUrl.ToLower());
                if (doctype != null)
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_DocType", doctype.GUID);
                    infos.SetValueSkipNotEmpty(obj, "GUID_YWType", (Guid)doctype.GUID_YWType);
                }
            }

        }

        /// <summary>
        /// 重置默认信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="OperatorId"></param>
        public static void ResetDefault<T>(this T obj, BaseDocument document, Guid OperatorId) where T : EntityObject
        {
            PropertyInfo[] infos = obj.GetType().GetProperties();
            SS_Operator Operator = OperatorId == Guid.Empty ? null : document.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
            if (Operator != null)
            {
                infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
                infos.SetValue(obj, "Modifier", Operator.OperatorName);
            }
            infos.SetValue(obj, "ModifyDate", DateTime.Now);
        }
        /// <summary>
        /// 填充明细默认值



        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="obj">实体</param>
        /// <param name="document">BaseDocument 类</param>
        /// <param name="OperatorId">操作员GUID</param>
        public static void FillDetailDefault<T>(this T obj, BaseDocument document, Guid OperatorId) where T : EntityObject
        {
            PropertyInfo[] infos = obj.GetType().GetProperties();
            SS_Operator Operator = OperatorId == Guid.Empty ? null : document.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
            if (Operator != null)
            {
                infos.SetValueSkipNotEmpty(obj, "GUID_Maker", Operator.GUID);
                infos.SetValueSkipNotEmpty(obj, "Maker", Operator.OperatorName);
                infos.SetValueSkipNotEmpty(obj, "MakeDate", DateTime.Now);
                infos.SetValue(obj, "GUID_Modifier", Operator.GUID);
                infos.SetValue(obj, "Modifier", Operator.OperatorName);
                infos.SetValue(obj, "ModifyDate", DateTime.Now);
            }
            Infrastructure.SS_PersonView person = Operator.DefaultPersonView();
            //获取默认部门
            var defDepartment = Operator.DefaultDepartment();

            var defDw = Operator.DefaultDW();
            if (person != null)
            {
                infos.SetValueSkipNotEmpty(obj, "GUID_Person", person.GUID);
                infos.SetValueSkipNotEmpty(obj, "PersonName", person.PersonName);
                infos.SetValueSkipNotEmpty(obj, "PersonKey", person.PersonKey);
                infos.SetValueSkipNotEmpty(obj, "GUID_Department", defDepartment == null ? person.GUID_Department : defDepartment.GUID);
                infos.SetValueSkipNotEmpty(obj, "DepartmentName", defDepartment == null ? person.DepartmentName : defDepartment.DepartmentName);
                infos.SetValueSkipNotEmpty(obj, "DepartmentKey", defDepartment == null ? person.DepartmentKey : defDepartment.DepartmentKey);
            }
            SS_SettleType settleType = document.InfrastructureContext.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "01" && e.IsStop == false);
            if (settleType != null)
            {
                if (document.ModelUrl == "lsggzd" || document.ModelUrl == "hkspd")
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_SettleType",new Guid("E0714910-6AE8-4B96-ACC9-77CE39ED72E9"));
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeName", "汇款");
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeKey", "03");
                }
                else if (document.ModelUrl == "zpsld" || document.ModelUrl == "gwkbxd")
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_SettleType", new Guid("85455E7E-2CAB-451E-BD0F-DC8860667084"));
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeName", "支票");
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeKey", "02");
                }
                else
                {
                    infos.SetValueSkipNotEmpty(obj, "GUID_SettleType", settleType.GUID);
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeName", settleType.SettleTypeName);
                    infos.SetValueSkipNotEmpty(obj, "SettleTypeKey", settleType.SettleTypeKey);
                }
            }
            BG_Type bgtype = document.InfrastructureContext.BG_Type.FirstOrDefault(e => e.BGTypeKey == "01" && e.IsStop == false);
            if (bgtype != null)
            {
                infos.SetValueSkipNotEmpty(obj, "GUID_BGType", bgtype.GUID);
                infos.SetValueSkipNotEmpty(obj, "BGTypeName", bgtype.BGTypeName);
                infos.SetValueSkipNotEmpty(obj, "BGTypeKey", bgtype.BGTypeKey);
            }

        }
        /// <summary>
        /// 根据json模型填充对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        public static void Fill<T>(this T obj, List<JsonAttributeModel> json) where T : EntityObject
        {
            obj.Fill(json, null);
        }
        /// <summary>
        /// 根据json模型填充对象（跳过填充不参与的字段）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        /// <param name="ExceptPropertyNames">不参与填充的字段名</param>
        public static void Fill<T>(this T obj, List<JsonAttributeModel> json, List<string> ExceptPropertyNames) where T : EntityObject
        {
            if (json == null || json.Count == 0) return;
            if (ExceptPropertyNames == null) ExceptPropertyNames = new List<string>();
            PropertyInfo[] infos = obj.GetType().GetProperties();
            string model = obj.ModelName().ToLower();
            foreach (JsonAttributeModel item in json)
            {
                PropertyInfo info = null;
                info = infos.FirstOrDefault(e => e.Name == item.n && !ExceptPropertyNames.Contains(e.Name) && item.m.ToLower() == model);
                if (info != null && info.LegalProeprtyType())
                {
                    info.SetValue(obj, item.v);
                }
            }
        }

        /// <summary>
        /// 根据json模型填充对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        public static void ClassFill<T>(this T obj, List<JsonAttributeModel> json) where T : class
        {
            obj.ClassFill(json, null);
        }
        /// <summary>
        /// 根据json模型填充对象（跳过填充不参与的字段）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        /// <param name="ExceptPropertyNames">不参与填充的字段名</param>
        public static void ClassFill<T>(this T obj, List<JsonAttributeModel> json, List<string> ExceptPropertyNames) where T : class
        {
            if (json == null || json.Count == 0) return;
            if (ExceptPropertyNames == null) ExceptPropertyNames = new List<string>();
            PropertyInfo[] infos = obj.GetType().GetProperties();

            string model = typeof(T).Name.ToLower();
            foreach (JsonAttributeModel item in json)
            {
                PropertyInfo info = null;
                info = infos.FirstOrDefault(e => e.Name == item.n && !ExceptPropertyNames.Contains(e.Name) && item.m.ToLower() == model);
                if (info != null && info.LegalProeprtyType())
                {
                    info.SetValue(obj, item.v);
                }
            }
        }

        /// <summary>
        /// 根据json模型填充相同属性对象


        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        ///<param name="modelName">数据源模型名称（即：数据中的模型中相同的属性赋值到T实体对应的属性值）</param>
        public static void FillCommField<T>(this T obj, List<JsonAttributeModel> json, string modelName) where T : EntityObject
        {
            obj.FillCommField(json, null, modelName);
        }
        /// <summary>
        /// 根据json模型填充对象（跳过填充不参与的字段）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="json">json模型</param>
        /// <param name="ExceptPropertyNames">不参与填充的字段名</param>
        ///<param name="modelName">数据源模型名称（即：数据中的模型中相同的属性赋值到T实体对应的属性值）</param>
        public static void FillCommField<T>(this T obj, List<JsonAttributeModel> json, List<string> ExceptPropertyNames, string modelName) where T : EntityObject
        {
            if (json == null || json.Count == 0) return;
            if (ExceptPropertyNames == null) ExceptPropertyNames = new List<string>();
            PropertyInfo[] infos = obj.GetType().GetProperties();
            foreach (JsonAttributeModel item in json)
            {
                PropertyInfo info = null;
                info = infos.FirstOrDefault(e => e.Name == item.n && !ExceptPropertyNames.Contains(e.Name) && item.m.ToLower() == modelName);
                if (info != null && info.LegalProeprtyType())
                {
                    info.SetValue(obj, item.v);
                }
            }
        }
        /// <summary>
        /// 相同Model的数据添加向对应的相同属性赋值


        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="model">数据Model</param>
        public static void FillCommField<T>(this T obj, T model) where T : EntityObject
        {
            if (model == null) return;           
            PropertyInfo[] infos = obj.GetType().GetProperties();
            PropertyInfo[] modelInfos = obj.GetType().GetProperties();
            foreach (PropertyInfo item in modelInfos)
            {
                var value = item.GetValue(model);
                if (value != null && !string.IsNullOrEmpty(value) && value!="0001-01-01" && value!=DateTime.MinValue.ToString() && value!=Guid.Empty.ToString())
                {
                    PropertyInfo info = null;
                    info = infos.FirstOrDefault(e => e.Name == item.Name);
                    if (info != null && info.LegalProeprtyType())
                    {
                        info.SetValue(obj, item.GetValue(model));
                    }
                }
            }
        }
        public static void CopyCommField<T>(this T obj, T model) where T : class
        {
            if (model == null) return;
            PropertyInfo[] infos = obj.GetType().GetProperties();
            PropertyInfo[] modelInfos = obj.GetType().GetProperties();
            foreach (PropertyInfo item in modelInfos)
            {
                var value = item.GetValue(model);
                if (value != null && !string.IsNullOrEmpty(value) && value != "0001-01-01" && value != DateTime.MinValue.ToString() && value != Guid.Empty.ToString())
                {
                    PropertyInfo info = null;
                    info = infos.FirstOrDefault(e => e.Name == item.Name);
                    if (info != null && info.LegalProeprtyType())
                    {
                        info.SetValue(obj, item.GetValue(model));
                    }
                }
            }
        }
        /// <summary>
        /// 从对象提取json模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static List<JsonAttributeModel> Pick<T>(this T obj) where T : EntityObject
        {
            return obj.Pick(null);
        }

        /// <summary>
        /// 从对象提取json模型（跳过提取不参与的字段）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="containsPropertyNames">筛选包含字段名</param>
        /// <returns></returns>
        public static List<JsonAttributeModel> Pick<T>(this T obj, List<string> containsPropertyNames) where T : EntityObject
        {
            List<JsonAttributeModel> result = new List<JsonAttributeModel>();
            PropertyInfo[] infos = obj.GetType().GetProperties();

            if (infos == null || infos.Length == 0) return result;
            //if (ExceptPropertyNames == null) ExceptPropertyNames = new List<string>();
            string model = obj.ModelName();
            for (int i = 0; i < infos.Length; i++)
            {
                PropertyInfo info = infos[i];
                if (!info.LegalProeprtyType()) continue;
                if (containsPropertyNames != null && containsPropertyNames.Count > 0)//筛选包含的字段
                {
                    if (containsPropertyNames.Contains(info.Name))
                    {
                        result.Add(new JsonAttributeModel(info.Name, info.GetValue(obj), model));
                    }
                }
                else
                {
                    result.Add(new JsonAttributeModel(info.Name, info.GetValue(obj), model));
                    
                }
            }
            return result;
        }// merge 

        public static List<JsonAttributeModel> PickMergeN<T>(this T obj,string mergeValue) where T : EntityObject
        {
            return obj.PickMergeN(mergeValue, null);
        }
       /// <summary>
        /// 合并项并包含的数据列
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="obj"></param>
       /// <param name="name"></param>
       /// <param name="mergeName"></param>
       /// <param name="mergeValue"></param>
       /// <param name="containPropertyNames">包含的数据字段集合</param>
       /// <returns></returns>
        public static List<JsonAttributeModel> PickMergeN<T>(this T obj,string mergeValue, List<string> containPropertyNames) where T : EntityObject
        {
            List<JsonAttributeModel> result = new List<JsonAttributeModel>();
            PropertyInfo[] infos = obj.GetType().GetProperties();
            string itemNameField="ItemName";
            string itemKeyField="ItemKey";
            string itemTypeField="ItemType";          

            if (infos == null || infos.Length == 0) return result;

            string model = obj.ModelName();
            //合并字段
            PropertyInfo mergeinfo = infos.FirstOrDefault(e => e.Name.ToLower() + "" == itemKeyField.ToLower() + "");
            //n 名称要合并的字段值

            string keyValue = mergeinfo.GetValue(obj);

            PropertyInfo itemType = infos.FirstOrDefault(e => e.Name.ToLower() + "" == itemTypeField.ToLower() + "");
            //n 名称要合并的字段值

            string itemTypeValue = itemType.GetValue(obj);

            for (int i = 0; i < infos.Length; i++)
            {
                PropertyInfo info = infos[i];
                if (!info.LegalProeprtyType()) continue;
                if (containPropertyNames!=null)
                {
                    if (containPropertyNames.Contains(info.Name.Trim()))
                    {
                        if (info.Name.ToLower() == itemNameField.ToLower())
                        {
                            //如果工资类型是金钱类型，不是金钱类型的并且有数据，填充0
                            if (itemTypeValue == "1")
                            {
                                double d;
                                if (double.TryParse(mergeValue, out d))
                                {
                                    result.Add(new JsonAttributeModel(itemTypeValue + info.Name + keyValue, mergeValue, model));
                                }
                                else
                                {
                                    result.Add(new JsonAttributeModel(itemTypeValue + info.Name + keyValue, "0", model));
                                }
                            }
                            else
                            {
                                result.Add(new JsonAttributeModel(itemTypeValue + info.Name + keyValue, mergeValue, model));
                            }
                        }
                        else
                        {
                            result.Add(new JsonAttributeModel(info.Name, info.GetValue(obj), model));
                        }
                        
                    }
                
                }
                else
                {
                    if (info.Name.ToLower() == itemNameField.ToLower())
                    {
                        result.Add(new JsonAttributeModel(itemTypeValue+info.Name + keyValue, mergeValue, model));
                    }
                    else
                    {
                        result.Add(new JsonAttributeModel(info.Name, info.GetValue(obj), model));
                    }
                }

            }
            return result;
        }

        public static void AddJsonAttrModel(this List<JsonAttributeModel> listJabModel, string name, string value, string model) 
        {
            listJabModel.Add(new JsonAttributeModel(name, value, model));
        }

        /// <summary>
        /// 从类对象提取json模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static List<JsonAttributeModel> ClassPick<T>(this T obj) where T : class
        {
            return obj.ClassPick(null);
        }
        /// <summary>
        /// 从对象提取json模型（跳过提取不参与的字段）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="ExceptPropertyNames">不参与提取的字段名</param>
        /// <returns></returns>
        public static List<JsonAttributeModel> ClassPick<T>(this T obj, List<string> ExceptPropertyNames) where T : class
        {
            List<JsonAttributeModel> result = new List<JsonAttributeModel>();
            PropertyInfo[] infos = obj.GetType().GetProperties();

            if (infos == null || infos.Length == 0) return result;
            if (ExceptPropertyNames == null) ExceptPropertyNames = new List<string>();
            string model = obj.GetType().Name;
            for (int i = 0; i < infos.Length; i++)
            {
                PropertyInfo info = infos[i];
                if (ExceptPropertyNames.Contains(info.Name) || !info.LegalProeprtyType()) continue;
                result.Add(new JsonAttributeModel(info.Name, info.GetValue(obj), model));
            }
            return result;
        }
    }

    /// <summary>
    /// 查询条件模型
    /// </summary>
    public class SearchCondition
    {
        /// <summary>
        /// 作用域的URL
        /// </summary>
        public string ModelUrl { set; get; }
        public Guid? OperatorId { get; set; }
    }
}
