using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using BusinessModel;
using Platform.Flow.Run;
using Infrastructure;
using Business.CommonModule;

namespace Business.Casher
{
   
    public class 支票领取 : BaseDocument
    {
        public 支票领取() : base() { }
        public 支票领取(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 选单
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            Business.Reimbursement.单据列表 djlb = new Reimbursement.单据列表(this.OperatorId,this.ModelUrl);
            return djlb.History(conditions);
        }
        
        /// <summary>
        /// 返回值

        /// </summary>
        /// <param name="guid">业务单据GUID</param>
        /// <param name="ywKey">Key值</param>
        /// <returns>JsonModel 类型</returns>
        public new object Retrieve(Guid guid)
        {
            ZPLQ jmodel = new ZPLQ();
            List<CN_CheckDrawMainView> checkDrawMainViewList = this.BusinessContext.CN_CheckDrawMainView.Where(e => e.GUID_Doc == guid).ToList();
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = checkDrawMainViewList.Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (float)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (float)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        IsLQChecked=1
                    }
                ).ToList();

            jmodel.CN_CheckList = checkDrawMainList;

            string ywKey = string.Empty;
            if (checkDrawMainViewList.Count > 0)
            {
                ywKey = GetWyKeyByClassId(checkDrawMainViewList[0].ClassID);
                //业务单据
                var ywlist = GetYWDataList(guid, ywKey);
                if (ywlist != null)
                {
                    jmodel.YWDocList = ywlist;
                }
            }
           
            return jmodel;
        }

        /// <summary>
        /// 返回值
        /// </summary>
        /// <param name="guid">业务单据GUID</param>
        /// <param name="ywKey">Key值</param>
        /// <returns>JsonModel 类型</returns>
        public ZPLQ RetrieveModel(Guid guid)
        {
            ZPLQ jmodel = new ZPLQ();
            List<CN_CheckDrawMainView> checkDrawMainViewList = this.BusinessContext.CN_CheckDrawMainView.Where(e => e.GUID_Doc == guid).ToList();
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = checkDrawMainViewList.Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (double)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (double)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        IsLQChecked = 1,
                        GUID_Doc=e.GUID_Doc
                    }
                ).OrderBy(e=>e.CheckNumber).ToList();

            jmodel.CN_CheckList = checkDrawMainList;

            string ywKey = string.Empty;
            if (checkDrawMainViewList.Count > 0)
            {
                ywKey = GetWyKeyByClassId(checkDrawMainViewList[0].ClassID);
                //业务单据
                var ywlist = GetYWDataList(guid, ywKey);
                if (ywlist != null)
                {
                    jmodel.YWDocList = ywlist;
                }
            }

            return jmodel;
        }

        /// <summary>
        /// 返回值
        /// </summary>
        /// <param name="guid">业务单据GUID</param>
        /// <param name="ywKey">Key值</param>
        /// <returns>JsonModel 类型</returns>
        public ZPLQ RetrieveModelByGuid(Guid guid)
        {
            ZPLQ jmodel = new ZPLQ();
            List<CN_CheckDrawMainView> checkDrawMainViewList = this.BusinessContext.CN_CheckDrawMainView.Where(e => e.GUID == guid).ToList();
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = checkDrawMainViewList.Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (double)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (double)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        IsLQChecked = 1,
                        GUID_Doc = e.GUID_Doc,
                        CheckDrawDatetime = DateTime.Now.ToString("yyyy-MM-dd")//支票领取时间 是当前时间 王娟定
                    }
                ).ToList();

            jmodel.CN_CheckList = checkDrawMainList;

            string ywKey = string.Empty;
            if (checkDrawMainViewList.Count > 0)
            {
                ywKey = GetWyKeyByClassId(checkDrawMainViewList[0].ClassID);
                //业务单据
                var ywlist = GetYWDataList(checkDrawMainViewList[0].GUID_Doc, ywKey);
                if (ywlist != null)
                {
                    if (jmodel.CN_CheckList.Count == 1) { 
                        var mainId=jmodel.CN_CheckList[0].GUID;
                        var docDetail = this.BusinessContext.CN_CheckDrawDetail.FirstOrDefault(e => e.GUID_CheckDrawMain == mainId);
                        if (docDetail != null) {
                            var c = ywlist.DetailList;
                            if (c.Count > 0)
                            {
                                var f = ywlist.DetailList.FirstOrDefault(E => E.GUID == docDetail.GUID_DocDetail);
                                var detailList = new List<YWDetailModel>();
                                detailList.Add(f);
                                foreach (var item in c)
                                {
                                    if (item.GUID != docDetail.GUID_DocDetail)
                                    {
                                        detailList.Add(item);
                                    }
                                }
                                ywlist.DetailList = detailList;
                            }

                        }
                     
                    }
                    jmodel.YWDocList = ywlist;
                }
            }

            return jmodel;
        }

        /// <summary>
        /// 支票Model列表
        /// </summary>
        /// <param name="guidList"></param>
        /// <returns></returns>
        private List<CheckModel> GetCheckModelList(List<Guid> guidList)
        {
            List<CN_CheckDrawMainView> checkDrawMainViewList = this.BusinessContext.CN_CheckDrawMainView.Where(e =>guidList.Contains(e.GUID)).ToList();
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = checkDrawMainViewList.Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (double)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (double)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        IsLQChecked=1,//已领用支票
                        GUID_Doc=e.GUID_Doc

                    }
                ).ToList();
            return checkDrawMainList;
        }
        /// <summary>
        /// 支票Model列表
        /// </summary>
        /// <param name="guidList"></param>
        /// <returns></returns>
        private List<CheckModel> GetCheckModelListByDocGUIDs(List<Guid> guidList)
        {
            List<CN_CheckDrawMainView> checkDrawMainViewList = this.BusinessContext.CN_CheckDrawMainView.Where(e =>e.GUID_Doc!=null && guidList.Contains((Guid)e.GUID_Doc)).ToList();
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = checkDrawMainViewList.Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (double)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (double)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        IsLQChecked = 1,//已领用支票                        GUID_Doc=e.GUID_Doc
                    }
                ).ToList();
            return checkDrawMainList;
        }

        /// <summary>
        /// 获取业务Key值
        /// </summary>
        /// <param name="classid"></param>
        /// <returns></returns>
        private string GetWyKeyByClassId(int? classid)
        {
            string wyKey = string.Empty;
            switch (classid)
            { 
                case 23: //报销
                    wyKey = Constant.YWTwo;
                    break;                
                case 32://"收入":
                    wyKey = Constant.YWThree;
                    break;
                case 48://"专用基金": 
                    wyKey=Constant.YWFour;                  
                    break;
                case 30://往来              
                    wyKey=Constant.YWFive;// "往来管理":                   
                    break;
                case 35://收款
                    wyKey=Constant.YWEightO;//"收付款管理":                    
                    break;
                case 55://现金
                    wyKey=Constant.YWEightT;//"提存现管理":                    
                    break;
                case 76://现金
                    wyKey ="76";//"公务卡管理":                    
                    break;
            }
            return wyKey;
        }
      
        /// <summary>
        /// 返回值
        /// </summary>
        /// <param name="guid">业务单据GUID</param>
        /// <param name="ywKey">Key值</param>
        /// <returns>JsonModel 类型</returns>
        public object Retrieve(Guid guid, string ywKey)
        { try
                {
            ZPLQ jmodel = new ZPLQ();
            
            //业务单据
            var ywlist = GetYWDataList(guid, ywKey);
            if (ywlist!=null)
            {
                jmodel.YWDocList = ywlist;
            }
            //支票列表
            List<CheckModel> checkDrawMainList = new List<CheckModel>();
            checkDrawMainList = this.BusinessContext.CN_CheckDrawMainView.Where(e => e.GUID_Doc == guid).Select(
                    e => new CheckModel
                    {
                        GUID = e.GUID,
                        BankAccountName = e.BankAccountName,
                        GUID_Check = e.GUID_Check == null ? Guid.Empty : (Guid)e.GUID_Check,
                        CheckNumber = e.CheckNumber,
                        PaymentNumber = e.PaymentNumber==""?null:e.PaymentNumber,
                        CheckPlan = e.CheckPlan == null ? 0F : (double)e.CheckPlan,
                        CheckMoney = e.CheckMoney == null ? 0F : (double)e.CheckMoney,
                        CheckUsed = e.CheckUsed,
                        CustomerName = e.CustomerName,
                        GUID_Customer=e.GUID_Customer,
                        IsLQChecked = 1////领过支票
                    }
                ).ToList();
            //如果没有值填充默认值

            if (checkDrawMainList == null || checkDrawMainList.Count == 0)
            {

                List<CheckModel> checkList = new List<CheckModel>();
                //支票号根据支付码与客户号分组 存放支票号与是否国库对应               
                //var paymentNumberList = ywlist.DetailList.GroupBy(e => new { e.PaymentNumber, e.GUID_Customer, e.IsGuoKu }).Select(e => new { e.Key.PaymentNumber, e.Key.IsGuoKu }).ToList();



                var paymentNumberList = from d in ywlist.DetailList
                                        group d by new { d.PaymentNumber, d.GUID_Customer, d.IsGuoKu, d.DocMemo, d.CustomerName } into temp
                                        select new { PaymentNumber = temp.Key.PaymentNumber, GUID_Customer = temp.Key.GUID_Customer, IsGuoKu = temp.Key.IsGuoKu, CustomerName = temp.Key.CustomerName, BX_Total = temp.Sum(e => e.BX_Total), PlanTotal = temp.Sum(e => e.PlanTotal), DocMemo = temp.Key.DocMemo };

                //获取银行账号信息
                List<SS_BankAccount> bankAccountList = this.InfrastructureContext.SS_BankAccount.Where(e => e.IsStop == false).ToList();


                //根据单据类型选择的支票 只有提现单选择现金支票 其余都选择转账支票
                int checkType = 1;//表示转账支票
                if (ywKey == Constant.YWEightT)//"提存现管理":
                {
                    checkType = 0;//现金支票
                }
                List<BusinessModel.CN_CheckView> checkNumberList = this.BusinessContext.CN_CheckView.Where(e =>
                   ((e.IsInvalid == false && e.InvalidDatetime == null) || (e.IsInvalid == false && e.InvalidDatetime >= DateTime.Now)) && e.CheckType == checkType)
                   .OrderBy(e => e.CheckNumber).ToList();

                bool? isguoku = false;
                List<string> takeCheckList = new List<string>();
                foreach (var item in paymentNumberList)
                {
                    if (item.IsGuoKu == "是")
                    {
                        isguoku = true;
                    }
                    else
                    {
                        isguoku = false;
                    }
                    //根据支付码中 判断是国库 然后根据是否国库去对应的银行账号
                    var bankAlist = bankAccountList.FindAll(e => e.IsGuoKu == isguoku);
                    if (bankAlist != null && bankAlist.Count > 1)
                    {
                        //如果有多个银行 去默认一个银行

                        bankAlist = bankAlist.FindAll(e => e.IsCash == true);
                        if (bankAlist == null || bankAlist.Count <= 0)
                        {
                            bankAlist = bankAccountList.FindAll(e => e.IsGuoKu == isguoku);
                        }
                    }
                    CheckModel checkmodel = new CheckModel();
                    //应该按照银行来取支票号 并且取最小的一个


                    if (bankAlist != null && bankAlist.Count > 0)
                    {
                        var model = checkNumberList.FindAll(e => e.GUID_BankAccount == bankAlist[0].GUID).Where(e => !takeCheckList.Contains(e.CheckNumber)).OrderBy(e => e.CheckNumber).FirstOrDefault();
                        if (model != null)
                        {
                            checkmodel.CheckNumber = model.CheckNumber;
                            checkmodel.BankAccountName = model.BankAccountName;
                            checkmodel.GUID_Check = model.GUID;
                            takeCheckList.Add(model.CheckNumber);

                        }
                    }
                    checkmodel.PaymentNumber = item.PaymentNumber == "" ? null : item.PaymentNumber;
                    checkmodel.CheckPlan = item.PlanTotal;
                    checkmodel.CheckMoney = item.BX_Total;
                    checkmodel.CheckUsed = item.DocMemo;
                    checkmodel.CustomerName = item.CustomerName;
                    checkmodel.GUID_Customer = item.GUID_Customer;
                    checkmodel.IsLQChecked = 0;//领过未支票


                    checkDrawMainList.Add(checkmodel);
                }

            }
            //else {
            //    var ent = this.BusinessContext.CN_CheckDrawMain.FirstOrDefault(e => e.GUID_Doc == guid);

            //}
           
            jmodel.CN_CheckList = checkDrawMainList;
            return jmodel;
                }
        catch (Exception ex)
        {

            throw ex;
        }
        }

        private List<CheckModel> BxDocTransferCheckDraw(YWMainModel MainObject)
        {
            return null;
            Dictionary<string, List<YWDetailModel>> group = new Dictionary<string, List<YWDetailModel>>();
            string groupkey = string.Empty;
            List<CheckModel> results = new List<CheckModel>();
            //获取结算方式为支票的明细
            List<YWDetailModel> rightDetails = new List<YWDetailModel>();
            foreach (YWDetailModel detail in MainObject.DetailList)
            {
                if (detail.IsCheck) rightDetails.Add(detail);
            }
            if (rightDetails.Count == 0) return results;
            foreach (YWDetailModel detail in rightDetails)
            {
                groupkey+=string.Format("{0}+",detail.GUID_Customer.IsNullOrEmpty()?"":detail.GUID_Customer.ToString());
                groupkey += string.Format("{0}+", detail.PaymentNumber);
                groupkey += string.Format("{0}+", detail.IsGuoKu);
                if (group.ContainsKey(groupkey))
                {
                    group[groupkey].Add(detail);
                }
                else
                {
                    group[groupkey] = new List<YWDetailModel>();
                }
            }
            foreach (var details in group.Values)
            {
                CheckModel item = new CheckModel();
                //获取银行账号信息
                List<SS_BankAccount> bankAccountList = this.InfrastructureContext.SS_BankAccount.Where(e => e.IsStop == false).ToList();
            }


            return results;
        }


        /// <summary>
        /// 获取最小支票号
        /// </summary>
        /// <param name="ywKey"></param>
        /// <param name="guid_bankAccount"></param>
        /// <returns></returns>
        private BusinessModel.CN_CheckView GetMinCheckModel(string ywKey, Guid guid_bankAccount, bool isGuoKu, List<string> existCheckNumber)
        {
            
            int checkType = 1;//表示转账支票
            if (ywKey == Constant.YWEightT)//"提存现管理":
            {
                checkType = 0;//现金支票
            }
            List<BusinessModel.CN_CheckView> checkNumberList = this.BusinessContext.CN_CheckView.Where(e =>
               ((e.IsInvalid == false && e.InvalidDatetime == null) || (e.IsInvalid == false && e.InvalidDatetime >= DateTime.Now)) && e.CheckType == checkType && e.GUID_BankAccount == guid_bankAccount && e.IsGuoKu == isGuoKu && !existCheckNumber.Contains(e.CheckNumber))
               .OrderBy(e => e.CheckNumber).ToList();
            if (checkNumberList.Count > 0)
            {
                return checkNumberList[0];
            }
            return null ;
        }
        /// <summary>
        /// 业务数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="ywKey"></param>
        /// <returns></returns>
        private YWMainModel GetYWDataList(Guid? guid, string ywKey)
        {

            //主表中的Data
            return GetYWMainList((Guid)guid, ywKey);
        }
        /// <summary>
        /// 获取业务数据信息
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="ywkey"></param>
        /// <returns></returns>
        private YWMainModel GetYWMainList(Guid guid, string ywkey)
        {
            YWMainModel list =new YWMainModel ();
            switch (ywkey)
            {
                case Constant.YWTwo: //"报销管理":
                    var bx_List = BX_List(guid);
                    if (bx_List != null)
                    {
                        list = bx_List;
                    }
                    break;
                case Constant.YWThree://"收入管理":
                case Constant.YWElevenO://"收入信息流转":
                    var sr_list = SR_List(guid);
                    if (sr_list != null)
                    {
                        list = sr_list;
                    }
                    break;
                case Constant.YWFour://"专用基金":
                    var jj_List = JJ_List(guid);
                    if (jj_List != null)
                    {
                        list = jj_List;
                    }
                    break;
                case Constant.YWFiveO://"单位往来":
                case Constant.YWFiveT://"个人往来":
                case Constant.YWFive:// "往来管理":
                    var wl_List = WL_List(guid);
                    if (wl_List != null )
                    {
                        list = wl_List;
                    }
                    break;
                case Constant.YWEightO://"收付款管理":
                    var cn_List = CN_List(guid);
                    if (cn_List != null)
                    {
                        list = cn_List;
                    }
                    break;
                case Constant.YWEightT://"提存现管理":
                    var cash_List = Cash_List(guid);
                    if (cash_List != null)
                    {
                        list = cash_List;
                    }
                    break;
                case "76":
                    var BCollect_List1 = BCollect_List(guid);
                    if (BCollect_List1 != null)
                    {
                        list = BCollect_List1;
                    }
                    break;
                    
            }
            return list;
        }
        /// <summary>
        /// 提存现管理
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel Cash_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            CN_CashMainView main = this.BusinessContext.CN_CashMainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                float plantotal = 0F;
                float bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.CN_CashDetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where d.GUID_CN_CashMain == main.GUID
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName ="",
                                   PlanTotal =0,
                                   BX_Total = (float)d.Total_Cash,
                                   DocMemo = d.CashMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department==null?Guid.Empty:(Guid)d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey ="",
                                   GUID_Customer=d.GUID_Cutomer,
                                    CustomerName=d.CustomerName
                               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    float.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    float.TryParse(f_bx.ToString(), out bx_total);
                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = 0;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;

            }
            return model;
        }
        /// <summary>
        /// 收付款管理

        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel CN_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            CN_MainView main = this.BusinessContext.CN_MainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                float plantotal = 0F;
                float bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.CN_DetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where d.GUID_CN_Main == main.GUID
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName ="",
                                   PlanTotal =0,
                                   BX_Total = (float)d.Total_CN,
                                   DocMemo = d.DetailMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department==null?Guid.Empty:(Guid)d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey = "",
                                   GUID_Customer =null,
                                   CustomerName =""
                               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    float.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    float.TryParse(f_bx.ToString(), out bx_total);
                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = main.BillCount == null ? 0 : (int)main.BillCount;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;

            }


            return model;
        }
        /// <summary>
        /// 往来

        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel WL_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            WL_MainView main = this.BusinessContext.WL_MainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                double plantotal = 0F;
                double bx_total = 0F;

                var sql = @"SELECT  DISTINCT
a.GUID,
a.BGCodeName,
ISNULL(a.Total_Plan,0) AS PlanTotal,
Total_WL AS BX_Total,
ActionMemo AS DocMemo,
ProjectKey,
ProjectName,
CASE WHEN b.IsGuoKu=1 THEN'是' ELSE '否' END AS IsGuoKu,
'{1}' AS DWName,
GUID_Department,
DepartmentName,
a.PaymentNumber,
BGCodeKey,
GUID_Cutomer,
CustomerName

 FROM WL_DetailView a
LEFT JOIN CN_PaymentNumber b ON a.GUID_PaymentNumber=b.GUID
WHERE a.GUID_WL_Main='{0}' AND a.IsCheck=1";
                sql=string.Format(sql,main.GUID,main.DWName);
                var detailList = this.BusinessContext.ExecuteStoreQuery<YWDetailModel>(sql).ToList();
                //明细信息
                //var detailList = (
                //               from d in this.BusinessContext.WL_DetailView
                //               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                //               from payment in temp.DefaultIfEmpty()
                //               where d.GUID_WL_Main == main.GUID && d.IsCheck==true
                //               select new YWDetailModel
                //               {
                //                   GUID = d.GUID,
                //                   BGCodeName = d.BGCodeName,
                //                   PlanTotal =d.Total_Plan,
                //                   BX_Total =d.Total_WL,
                //                   DocMemo = d.ActionMemo,
                //                   ProjectKey = d.ProjectKey,
                //                   ProjectName = d.ProjectName,
                //                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                //                   DWName = main.DWName,
                //                   GUID_Department = d.GUID_Department,
                //                   DepartmentName = d.DepartmentName,
                //                   PaymentNumber = payment.PaymentNumber,
                //                   BGCodeKey = d.BGCodeKey,
                //                   GUID_Customer = d.GUID_Cutomer,
                //                   CustomerName = d.CustomerName
                //               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    double.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    double.TryParse(f_bx.ToString(), out bx_total);
                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = (main.BillCount =="" || main.BillCount==null)? 0 :int.Parse(main.BillCount);
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;

            }
            return model;
        }
        /// <summary>
        /// 基金数据
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel JJ_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            JJ_MainView main = this.BusinessContext.JJ_MainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                float plantotal = 0F;
                float bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.JJ_DetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where d.GUID_JJ_Main == main.GUID && d.IsCheck==true
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName ="",
                                   PlanTotal = (float)d.Total_Plan,
                                   BX_Total = (float)d.Total_JJ,
                                   DocMemo = d.FeeMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department==null?Guid.Empty:(Guid)d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey ="",
                                   GUID_Customer = d.GUID_Cutomer,
                                   CustomerName = d.CustomerName
                               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    float.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    float.TryParse(f_bx.ToString(), out bx_total);
                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate =main.DocDate==null?"": ((DateTime)main.DocDate).ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = main.BillCount == null ? 0 : (int)main.BillCount;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;

            }

            return model;
        }
        /// <summary>
        /// 收入列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel SR_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            SR_MainView main = this.BusinessContext.SR_MainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                float plantotal = 0F;
                float bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.BX_DetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where d.GUID_BX_Main == main.GUID
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName = d.BGCodeName,
                                   PlanTotal = (float)d.Total_Plan,
                                   BX_Total = (float)d.Total_BX,
                                   DocMemo = d.FeeMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey = d.BGCodeKey,
                                   GUID_Customer = d.GUID_Cutomer,
                                   CustomerName = d.CustomerName
                               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    float.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    float.TryParse(f_bx.ToString(), out bx_total);
                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = main.BillCount == null ? 0 : (int)main.BillCount;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;

            }


            return model;
        }
       
        /// <summary>
        /// 报销列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel BX_List(Guid guid)
        {        
            YWMainModel model = new YWMainModel() ;
            BX_MainView main = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);            
            if (main != null)
            {
                
                double plantotal = 0F;
                double bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.BX_DetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where d.GUID_BX_Main == main.GUID && d.IsCheck == true
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName = d.BGCodeName,
                                   PlanTotal = d.Total_Plan,
                                   BX_Total = d.Total_BX,
                                   DocMemo = d.FeeMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey = d.BGCodeKey,
                                   GUID_Customer = d.GUID_Cutomer,
                                   CustomerName = d.CustomerName,
                                   IsCheck = d.IsCheck == true ? true : false
                               }).ToList();

//                var sql = string.Format(@"SELECT  d.GUID ,
//        BGCodeName ,
//
//         CAST( Total_Plan AS DECIMAL(18,2)) as PlanTotal1 ,
//       CAST( Total_BX AS DECIMAL(18,2))  AS  BX_Total1 ,
//        FeeMemo AS DocMemo,
//        ProjectKey,
//        ProjectName,
//        CASE WHEN IsGuoKu=1 THEN '是' ELSE '否' END  AS IsGuoKu,
//        c.DWName,
//        d.GUID_Department,
//        d.DepartmentName,
//        PaymentNumber,
//        BGCodeKey,
//        GUID_Cutomer AS  GUID_Customer,
//        CustomerName,
//        IsCheck
//FROM    BX_DetailView d
//        LEFT JOIN CN_PaymentNumber b ON d.GUID_PaymentNumber = b.GUID
//        LEFT JOIN dbo.BX_MainView c ON d.GUID_BX_Main=c.GUID
//WHERE   d.GUID_BX_Main = '{0}'
//        AND d.IsCheck = 1
//        ", main.GUID);
//                var detailList = this.BusinessContext.ExecuteStoreQuery<YWDetailModel>(sql).ToList();
                //主表信息
                if (detailList.Count > 0)
                {
                    var d = detailList.Sum(e => e.PlanTotal);
                    double.TryParse(d.ToString(), out bx_total);
                    var c = detailList.Sum(e => e.BX_Total);
                    double.TryParse(c.ToString(),out bx_total);

                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = main.BillCount==null?0:(int)main.BillCount;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;


            }
            return model;
        }
        /// <summary>
        /// 公务卡
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private YWMainModel BCollect_List(Guid guid)
        {
            YWMainModel model = new YWMainModel();
            var bxcollectMain = this.BusinessContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == guid).Select(e=>e.GUID_BXMain);
            var main = this.BusinessContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {

                float plantotal = 0F;
                float bx_total = 0F;
                //明细信息
                var detailList = (
                               from d in this.BusinessContext.BX_DetailView
                               join payment in this.BusinessContext.CN_PaymentNumber on d.GUID_PaymentNumber equals payment.GUID into temp
                               from payment in temp.DefaultIfEmpty()
                               where bxcollectMain.Contains(d.GUID_BX_Main) && d.IsCheck == true
                               select new YWDetailModel
                               {
                                   GUID = d.GUID,
                                   BGCodeName = d.BGCodeName,
                                   PlanTotal = (float)d.Total_Plan,
                                   BX_Total = (float)d.Total_BX,
                                   DocMemo = d.FeeMemo,
                                   ProjectKey = d.ProjectKey,
                                   ProjectName = d.ProjectName,
                                   IsGuoKu = payment.IsGuoKu == true ? "是" : "否",
                                   DWName = main.DWName,
                                   GUID_Department = d.GUID_Department,
                                   DepartmentName = d.DepartmentName,
                                   PaymentNumber = payment.PaymentNumber,
                                   BGCodeKey = d.BGCodeKey,
                                   GUID_Customer = d.GUID_Cutomer,
                                   CustomerName = d.CustomerName,
                                   IsCheck = d.IsCheck == true ? true : false
                               }).ToList();

                //主表信息
                if (detailList.Count > 0)
                {
                    var f = detailList.Sum(e => e.PlanTotal);
                    float.TryParse(f.ToString(), out plantotal);

                    var f_bx = detailList.Sum(e => e.BX_Total);
                    float.TryParse(f_bx.ToString(), out bx_total);

                    model.DetailList = detailList;
                }
                model.GUID = main.GUID;
                model.DocNum = main.DocNum;
                model.DocDate = main.DocDate.ToString("yyyy-MM-dd");
                model.PersonName = main.PersonName;
                model.PlanTotal = plantotal;
                model.BX_Total = bx_total;
                model.DocMemo = main.DocMemo;
                model.DepartmentName = main.DepartmentName;
                model.DWName = main.DWName;
                model.BillCount = 0;
                model.DocTypeName = main.DocTypeName;
                model.DocTypeKey = main.DocTypeKey;


            }
            return model;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="status"></param>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public ResponseJsonMessage Save(string ywkey, string jsonModel)
        {
            ResponseJsonMessage result = new ResponseJsonMessage();
            string strMsg=string.Empty;
            var main_classid =0;
            var detail_classid =0;
            Business.Casher.ZPLQ obj_Model=null ;
            object objCheck=null;
            List<Guid> docGUID = new List<Guid>();
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                switch (ywkey)
                {
                    case Constant.YWTwo: //"报销管理":
                        main_classid = GetClassID(typeof(BX_Main).Name);
                        detail_classid = GetClassID(typeof(BX_Detail).Name);
                        obj_Model = JsonToModel(jsonModel);
                       
                        objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                        //Save_BX_CheckDraw(bx_Model,out strMsg);
                        break;
                    case Constant.YWThree://"收入管理":
                    case Constant.YWElevenO://"收入信息流转":
                          main_classid = GetClassID(typeof(SR_Main).Name);
                          detail_classid = GetClassID(typeof(SR_Detail).Name);
                          obj_Model = JsonToModel(jsonModel);
                          objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                        break;
                    case Constant.YWFour://"专用基金":
                          main_classid = GetClassID(typeof(JJ_Main).Name);
                          detail_classid = GetClassID(typeof(JJ_Detail).Name);
                          obj_Model = JsonToModel(jsonModel);
                          objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                        break;
                    case Constant.YWFiveO://"单位往来":
                    case Constant.YWFiveT://"个人往来":
                    case Constant.YWFive:// "往来管理":
                         main_classid = GetClassID(typeof(WL_Main).Name);
                          detail_classid = GetClassID(typeof(WL_Detail).Name);
                          obj_Model = JsonToModel(jsonModel);
                          objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                        break;
                    case Constant.YWEightO://"收付款管理":
                          main_classid = GetClassID(typeof(CN_Main).Name);
                          detail_classid = GetClassID(typeof(CN_Detail).Name);
                          obj_Model = JsonToModel(jsonModel);
                          objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);                        
                        break;
                    case Constant.YWEightT://"提存现管理":
                           main_classid = GetClassID(typeof(CN_CashMain).Name);
                          detail_classid = GetClassID(typeof(CN_CashDetail).Name);
                          obj_Model = JsonToModel(jsonModel);
                          objCheck=SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                          break;
                    case "76": //"报销管理":
                          main_classid = GetClassID(typeof(BX_CollectMain).Name);
                          detail_classid = GetClassID(typeof(BX_Detail).Name);
                          obj_Model = JsonToModel(jsonModel);

                          objCheck = SaveCheckDraw(obj_Model, main_classid, detail_classid, out strMsg);
                          //Save_BX_CheckDraw(bx_Model,out strMsg);
                          break;

                }
                if (string.IsNullOrEmpty(strMsg))
                {
                    result.result = JsonModelConstant.Success;
                    if (obj_Model != null)
                    {
                        docGUID.Add(obj_Model.YWDocList.GUID);
                    }
                    if (docGUID != null && docGUID.Count>0)
                    {

                        result.data = GetCheckModelListByDocGUIDs(docGUID);;
                    }
                    //obj_Model.YWDocList.GUID;
                    var remark = "单据号：" + obj_Model.YWDocList.DocNum + "、业务类型编码:" + ywkey;
                    strMsg = "保存成功！";
                    result.Id = obj_Model.YWDocList.GUID;
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                    /*成功后 提交流程*/
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                }
                return result;
            }
            catch (Exception ex)
            {
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "保存数据错误！", JsonModelConstant.Error);
                return result;
            }           
           
        }
        public object ChangeCheck(string checkDrawMianGuid,string ywkey)
        {
            List<string> strList = new List<string>();
            if (checkDrawMianGuid.IndexOf(',') > 0)
            {
                strList.AddRange(checkDrawMianGuid.Split(',').ToList());
            }
            else
            {
                strList.Add(checkDrawMianGuid);
            }
            List<Guid> guidList = new List<Guid>();
            Guid g;
            foreach (string item in strList)
            { 
                if(!string.IsNullOrEmpty(item))
                {
                    if (Guid.TryParse(item, out g))
                    {
                        guidList.Add(g);
                    }
                }
            }
            var errorMsg = "";
            var checkGUID =Guid.Empty;
            var checkDrawMainList = this.BusinessContext.CN_CheckDrawMain.Where(e=>guidList.Contains(e.GUID)).ToList();
            if (checkDrawMainList == null || checkDrawMainList.Count == 0)
            {
                return new { msg = "要换的支票不存在", check = "" };
            }
            List<CheckModel> CheckModelList = new List<CheckModel>();
            List<string> takeCheckList = new List<string>();
            bool isGuoKu = false;
            for (int i = 0; i < checkDrawMainList.Count; i++)
            {
               var model=checkDrawMainList[i];
                checkGUID = checkDrawMainList[i].GUID_Check;
                CheckModel checkmodel = new CheckModel();
                //应该按照银行来取支票号 并且取最小的一个
                var checkViewModel = this.BusinessContext.CN_CheckView.FirstOrDefault(e=>e.GUID==model.GUID_Check);
                if (checkmodel != null)
                {
                    isGuoKu = isGuoKu = checkViewModel.IsGuoKu == null ? false : (bool)checkViewModel.IsGuoKu;
                    checkmodel.CheckNumber = checkViewModel.CheckNumber;
                    checkmodel.BankAccountName = checkViewModel.BankAccountName;
                    //checkmodel.GUID_Check = checkViewModel.GUID;
                    var mincheckModel = GetMinCheckModel(ywkey, checkViewModel.GUID_BankAccount, isGuoKu, takeCheckList);
                    if(checkmodel!=null)
                    {
                        checkmodel.CheckNumber = mincheckModel.CheckNumber;
                        checkmodel.GUID_Check = mincheckModel.GUID;
                        checkmodel.GUID_BankAccount = mincheckModel.GUID_BankAccount;
                    }
                    takeCheckList.Add(checkmodel.CheckNumber);
                   
                }
                checkmodel.PaymentNumber = model.PaymentNumber == "" ? null : model.PaymentNumber;
                checkmodel.CheckPlan = model.CheckPlan == null ? 0F : (float)model.CheckPlan;
                checkmodel.CheckMoney = model.CheckMoney == null ? 0F : (float)model.CheckMoney;
                checkmodel.CheckUsed = model.CheckUsed;
                var custoemr = this.InfrastructureContext.SS_Customer.FirstOrDefault(e => e.GUID == model.GUID_Customer);
                if (custoemr != null)
                {
                    checkmodel.CustomerName = custoemr.CustomerName;
                    checkmodel.GUID_Customer = model.GUID_Customer;
                }
                checkmodel.IsLQChecked = 0;//未领过支票
                CheckModelList.Add(checkmodel);
                

               //删除
                DeleteCheckDraw(checkDrawMainList[i]);
               
            }


            //Guid guid_Account =Guid.Empty;
            //bool isGuoKu = false;
            ////作废支票
            //var check = this.BusinessContext.CN_CheckView.FirstOrDefault(e => e.GUID == checkGUID);
            //if (check != null)
            //{
            //    guid_Account = check.GUID_BankAccount;
            //    isGuoKu = check.IsGuoKu==null?false:(bool)check.IsGuoKu;
            //    //修改支票为作废支票
            //    check.IsInvalid = true;
            //    this.BusinessContext.ModifyConfirm(check);
            //}

            //返回最小支票
            //var checkModel = GetMinCheckModel(ywkey, guid_Account, isGuoKu);
            //object obj = null;
            //if (checkModel != null)
            //{
            //    obj = new { GUID_Check = checkModel.GUID, CheckNumber = checkModel.CheckNumber };
            //}
            object obj = CheckModelList;
            //找到最小的支票号
            return new { msg = "换票成功", check=obj }; 
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="main"></param>
        private void DeleteCheckDraw(CN_CheckDrawMain main)
        {
            //删除操作
            var checkDetail = this.BusinessContext.CN_CheckDrawDetail.Where(e => e.GUID_CheckDrawMain ==main.GUID).ToList();
            foreach (var item in checkDetail)
            {
                this.BusinessContext.CN_CheckDrawDetail.DeleteObject(item);
            }
            this.BusinessContext.CN_CheckDrawMain.DeleteObject(main);

            //作废支票
            var check = this.BusinessContext.CN_Check.FirstOrDefault(e => e.GUID == main.GUID_Check);
            if (check != null)
            { 
                //修改支票为作废支票
                check.IsInvalid = true;
                this.BusinessContext.ModifyConfirm(check);
            }
            this.BusinessContext.SaveChanges();
        }
        /// <summary>
        /// 报销支票领取
        /// </summary>
        /// <param name="bx_Model"></param>
        /// <param name="msg"></param>       
        private void Save_BX_CheckDraw(ZPLQ bx_Model, out string msg)
        {
            msg = string.Empty;
            var cn_checkList = bx_Model.CN_CheckList;
            var main_classid = GetClassID(typeof(BX_Main).Name);
            var bx_main= bx_Model.YWDocList;

            //领取主表
            CN_CheckDrawMain main = new CN_CheckDrawMain();
            main.FillDefault(this, this.OperatorId);
            main.GUID = Guid.NewGuid();
            if (cn_checkList.Count > 0)
            {
                main.GUID_Check = cn_checkList[0].GUID;
            }
            main.CheckDrawDatetime = DateTime.Now;
            main.GUID_Doc = bx_main.GUID;
            main.ClassID = main_classid;

            //明细信息
            if (bx_main != null)
            {

                var detailList = bx_main.DetailList;
                if (detailList.Count > 0)
                {
                    var detail_classid = GetClassID(typeof(BX_Detail).Name);
                    foreach (YWDetailModel item in detailList)
                    {
                        CN_CheckDrawDetail detail = new CN_CheckDrawDetail();
                        detail.GUID = Guid.NewGuid();
                        detail.GUID_CheckDrawMain = main.GUID;
                        detail.GUID_DocMain = bx_main.GUID;
                        detail.MainClassID = main_classid;
                        detail.GUID_DocDetail = item.GUID;
                        detail.DetailClassID = detail_classid;
                        main.CN_CheckDrawDetail.Add(detail);
                    }
                }
            }
            
            this.BusinessContext.CN_CheckDrawMain.AddObject(main);
            this.BusinessContext.SaveChanges();

        }
        /// <summary>
        /// 支票领取保存
        /// </summary>
        /// <param name="bx_Model"></param>
        /// <param name="mainClassid"></param>
        /// <param name="detailClassid"></param>
        /// <param name="msg"></param>
        private object SaveCheckDraw(ZPLQ bx_Model,int mainClassid,int detailClassid, out string msg)
        {
            msg = string.Empty;
            var cn_checkList = bx_Model.CN_CheckList;           
            var bx_main = bx_Model.YWDocList;
            List<Guid> guids = new List<Guid>();
            //不能重复领用
            var isExistEnt = this.BusinessContext.CN_CheckDrawMain.FirstOrDefault(e => e.GUID_Doc == bx_main.GUID);
            if (isExistEnt != null) {
                //isExistEnt.CheckMoney = cn_checkList[0].CheckMoney;
                //this.BusinessContext.SaveChanges();
                guids.Add(isExistEnt.GUID);
                return GetCheckModelList(guids);
            }
         
            foreach (CheckModel checkModel in cn_checkList)
            {
                //领取主表
                CN_CheckDrawMain main = new CN_CheckDrawMain();
                main.FillDefault(this, this.OperatorId);
                main.GUID = Guid.NewGuid();
                if (cn_checkList.Count > 0)
                {
                    main.GUID_Check = checkModel.GUID_Check;
                }
                main.CheckDrawDatetime = DateTime.Now;
                main.GUID_Doc = bx_main.GUID;
                main.ClassID = mainClassid;
                main.GUID_Customer = checkModel.GUID_Customer;
                main.CheckPlan = checkModel.CheckPlan;
                main.CheckMoney = checkModel.CheckMoney;
                main.CheckUsed = checkModel.CheckUsed;
                //主ModelGUID
                guids.Add(main.GUID);
                //明细信息
                if (bx_main != null)
                {
                    var detailList = bx_main.DetailList;
                    if (detailList.Count > 0)
                    {
                        foreach (YWDetailModel item in detailList)
                        {
                            //支付码 相同 客户相同 备注相同
                            if (ConvertStr(item.PaymentNumber) == ConvertStr(checkModel.PaymentNumber) && ConvertStr(item.CustomerName) == ConvertStr(checkModel.CustomerName) && ConvertStr(item.DocMemo) == ConvertStr(checkModel.CheckUsed))
                            {
                                CN_CheckDrawDetail detail = new CN_CheckDrawDetail();
                                detail.GUID = Guid.NewGuid();
                                detail.GUID_CheckDrawMain = main.GUID;
                                detail.GUID_DocMain = bx_main.GUID;
                                detail.MainClassID = mainClassid;
                                detail.GUID_DocDetail = item.GUID;
                                detail.DetailClassID = detailClassid;
                                main.CN_CheckDrawDetail.Add(detail);
                            }
                        }
                    }
                }

                //更改为无效状态
                UpdateCheckIsInvalid(checkModel.CheckNumber,true);
               
                this.BusinessContext.CN_CheckDrawMain.AddObject(main);
            }
            this.BusinessContext.SaveChanges();
            //返回CheckModel
           return GetCheckModelList(guids);
        }
        private string ConvertStr(object obj)
        {
            string str = string.Empty;
            if (obj != null && obj.ToString() != "")
            {
                str = obj.ToString().Trim();
            }
            return str;
            
        }
        /// <summary>
        /// 更改是否无效状态
        /// </summary>
        /// <param name="checkNumber"></param>
        /// <param name="isInvalid"></param>
        private void UpdateCheckIsInvalid(string checkNumber,bool isInvalid)
        {
            var model = this.BusinessContext.CN_Check.FirstOrDefault(e=>e.CheckNumber==checkNumber);
            if (model != null)
            {                
                model.IsInvalid = isInvalid;
                this.BusinessContext.ModifyConfirm(model);
            }
        }
        /// <summary>
        /// 获取Class ID
        /// </summary>
        /// <param name="modelName"></param>
        /// <returns></returns>
        private int GetClassID(string modelName)
        {
            int iValue = 0;
            var model = this.InfrastructureContext.SS_Class.FirstOrDefault(e => e.TableName.ToLower() == modelName.ToLower());
            if (model != null)
            {
                iValue = model.ClassID;
            }
            return iValue;
        }
        ///// <summary>
        ///// 发票信息
        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private CN_Check CN_CheckModel(JsonModel jsonModel)
        //{
        //    CN_Check checkModel = new CN_Check();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string modelName = checkModel.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(modelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                checkModel.Fill(row);
        //            }
        //        }               
        //    }
        //    return checkModel;
        //}
        ///// <summary>
        ///// 现金
        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private CN_CashMain Cash_MainModel(JsonModel jsonModel)
        //{
        //    CN_CashMain main = new CN_CashMain();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string mainModelName = main.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(mainModelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                main.Fill(row);
        //            }
        //        }
        //        CN_CashDetail detail = new CN_CashDetail();
        //        string detailModelName = detail.ModelName();
        //        JsonGridModel dGrid = jsonModel.d.Find(detailModelName);
        //        if (dGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in dGrid.r)
        //            {
        //                detail = new CN_CashDetail();
        //                detail.Fill(row);
        //                main.CN_CashDetail.Add(detail);
        //            }
        //        }
        //    }
        //    return main;   
        //}
        ///// <summary>
        ///// 出纳
        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private CN_Main CN_MainModel(JsonModel jsonModel)
        //{
        //    CN_Main main = new CN_Main();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string mainModelName = main.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(mainModelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                main.Fill(row);
        //            }
        //        }
        //        CN_Detail detail = new CN_Detail();
        //        string detailModelName = detail.ModelName();
        //        JsonGridModel dGrid = jsonModel.d.Find(detailModelName);
        //        if (dGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in dGrid.r)
        //            {
        //                detail = new CN_Detail();
        //                detail.Fill(row);
        //                main.CN_Detail.Add(detail);
        //            }
        //        }
        //    }
        //    return main;   
        //}
        ///// <summary>
        ///// 往来

        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private WL_Main WL_MainModel(JsonModel jsonModel)
        //{
        //    WL_Main main = new WL_Main();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string mainModelName = main.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(mainModelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                main.Fill(row);
        //            }
        //        }
        //        WL_Detail detail = new WL_Detail();
        //        string detailModelName = detail.ModelName();
        //        JsonGridModel dGrid = jsonModel.d.Find(detailModelName);
        //        if (dGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in dGrid.r)
        //            {
        //                detail = new WL_Detail();
        //                detail.Fill(row);
        //                main.WL_Detail.Add(detail);
        //            }
        //        }
        //    }
        //    return main;   
        //}
        ///// <summary>
        ///// 基金
        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private JJ_Main JJ_MainModel(JsonModel jsonModel)
        //{
        //    JJ_Main main = new JJ_Main();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string mainModelName = main.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(mainModelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                main.Fill(row);
        //            }
        //        }
        //        JJ_Detail detail = new JJ_Detail();
        //        string detailModelName = detail.ModelName();
        //        JsonGridModel dGrid = jsonModel.d.Find(detailModelName);
        //        if (dGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in dGrid.r)
        //            {
        //                detail = new JJ_Detail();
        //                detail.Fill(row);
        //                main.JJ_Detail.Add(detail);
        //            }
        //        }
        //    }
        //    return main;   
        //}
        ///// <summary>
        ///// 收入
        ///// </summary>
        ///// <param name="jsonModel"></param>
        ///// <returns></returns>
        //private SR_Main SR_MainModel(JsonModel jsonModel)
        //{
        //    SR_Main main = new SR_Main();
        //    if (jsonModel.d != null && jsonModel.d.Count > 0)
        //    {
        //        string mainModelName = main.ModelName();
        //        JsonGridModel mGrid = jsonModel.d.Find(mainModelName);
        //        if (mGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in mGrid.r)
        //            {
        //                main.Fill(row);
        //            }
        //        }
        //        SR_Detail detail = new SR_Detail();
        //        string detailModelName = detail.ModelName();
        //        JsonGridModel dGrid = jsonModel.d.Find(detailModelName);
        //        if (dGrid != null)
        //        {
        //            foreach (List<JsonAttributeModel> row in dGrid.r)
        //            {
        //                detail = new SR_Detail();
        //                detail.Fill(row);
        //                main.SR_Detail.Add(detail);
        //            }
        //        }
        //    }
        //    return main;            
        //}
       /// <summary>
       /// Json 转换成Model
       /// </summary>
       /// <param name="jsonModel"></param>
       /// <returns></returns>
        private ZPLQ JsonToModel(string jsonModel)
        {
            ZPLQ main = new ZPLQ();
            if (string.IsNullOrEmpty(jsonModel)) return main;
            main =JsonHelp.JsonToObject<ZPLQ>(jsonModel);
            return main;
        }

        public List<TreeNodeModel> GetTreeZPLQ(string guid,bool IsXJ=false)
        {
            Guid g;
            Guid.TryParse(guid,out g);
            var List =RetrieveModel(g);
            var typeKey = new string[] {"21"};//提现单
            var typeName = "转账";
            var pageUrl = "/Print/zplqzz";//支票领取 转账
            var pagefmUrl = "/Print/zplqzzfm";//支票领取 转账
            if (List != null && List.YWDocList != null)
            {
                if (typeKey.ToList().Contains(List.YWDocList.DocTypeKey))
                {
                    typeName = "现金";
                    pageUrl = "/Print/zplqzz1";//支票领取现金 
                    pagefmUrl = "/Print/zplqzzfm";//支票领取 现金
                }
            }
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            if (IsXJ) {
                typeName = "现金";
                pageUrl = "/Print/zplqzz1";//支票领取现金 
                pagefmUrl = "/Print/zplqzzfm";//支票领取 现金
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id =guid;
                treeModel.@checked = false;
                treeModel.text =List.YWDocList.DocTypeKey=="20"?"存现审批单":"提现审批单";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["GUID"] = guid;
                dic["URL"] = List.YWDocList.DocTypeKey == "20" ? "/Print/xjcc" : "/Print/xjtq";
                treeModel.attributes = dic;
                list.Add(treeModel);
            }

         
            foreach (var item in List.CN_CheckList)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = item.GUID.ToString();
                treeModel.@checked = false;
                treeModel.text = CommonFuntion.StringToJson(item.BankAccountName) + "(" + typeName + ")-正面";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                dic["URL"] = pageUrl;
                treeModel.attributes = dic;
                list.Add(treeModel);

                TreeNodeModel treeModel1 = new TreeNodeModel();
                treeModel1.state = "open";
                treeModel1.id = item.GUID.ToString();
                treeModel1.@checked = false;
                treeModel1.text = CommonFuntion.StringToJson(item.BankAccountName) + "(" + typeName + ")-反面";
                Dictionary<string, string> dic1 = new Dictionary<string, string>();
                dic1["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                dic1["URL"] = pagefmUrl;
                treeModel1.attributes = dic1;
                list.Add(treeModel1);
            }
            
            return list;
          
        }
               
    }
    /// <summary>
    /// 支票领取
    /// </summary>
    public class ZPLQ
    {
        public ZPLQ()
        {
    
        }
        public ZPLQ(List<CheckModel> checkList, YWMainModel ywdocList)
        {
            this.CN_CheckList = checkList;
            this.YWDocList = ywdocList;
        }
        /// <summary>
        /// 支票列表信息
        /// </summary>
        public List<CheckModel> CN_CheckList { set; get; }
        /// <summary>
        /// 业务单据列表
        /// </summary>
        public YWMainModel YWDocList { set; get; }
        
    }
    /// <summary>
    /// 支票信息
    /// </summary>
    public class CheckModel
    { 
        /// <summary>
        /// 编号
        /// </summary>
         public Guid GUID {set;get;}
        /// <summary>
        /// 银行账户GUID
        /// </summary>
         public Guid GUID_BankAccount { set; get; }
        /// <summary>
        /// 银行账户名称
        /// </summary>
         public string BankAccountName{set;get;}
        /// <summary>
        /// 支票编号
        /// </summary>
         public Guid GUID_Check{set;get;}
        /// <summary>
        /// 支票号
        /// </summary>
         public string CheckNumber{set;get;}
        /// <summary>
        /// 支付码
        /// </summary>
         public string PaymentNumber{set;get;}
        /// <summary>
        /// 限额
        /// </summary>
         public double CheckPlan { set; get; }
        /// <summary>
        /// 金额
        /// </summary>
        public double CheckMoney{set;get;}
        /// <summary>
        /// 用途
        /// </summary>
        public string CheckUsed{set;get;}
        /// <summary>
        /// 客户GUID
        /// </summary>
        public Guid? GUID_Customer { set; get; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { set; get; }
        /// <summary>
        /// 是否领取过支票的标识
        /// </summary>
        public int IsLQChecked { get; set; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public Guid? GUID_Doc { set; get; }
        public Guid? GUID_DetailDoc { set; get; }
        /// <summary>
        /// 时间
        /// </summary>
        public string CheckDrawDatetime { set; get; }
        
    }
    /// <summary>
    /// 业务主模型

    /// </summary>
    public class YWMainModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public Guid GUID { set; get; }
        /// <summary>
        /// 单据编号
        /// </summary>
        public string DocNum { set; get; }
        /// <summary>
        /// 单据日期
        /// </summary>
        public string DocDate { set; get; }
        /// <summary>
        /// 领款人

        /// </summary>
        public string PersonName { set; get; }
        /// <summary>
        /// 限额
        /// </summary>
        public double PlanTotal { set; get; }
        /// <summary>
        /// 报销金额
        /// </summary>
        public double BX_Total { set; get; }
        /// <smmary>
        /// 摘要
        /// </summary>
        public string DocMemo { set; get; }
        /// <summary>
        /// 领款部门
        /// </summary>
        public string DepartmentName { set; get; }
        /// <summary>
        /// 领款单位
        /// </summary>
        public string DWName { set; get; }
        /// <summary>
        /// 附单据数
        /// </summary>
        public int BillCount { set; get; }
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocTypeName { set; get; }
        /// <summary>
        /// 单据类型Key
        /// </summary>
        public string DocTypeKey { set; get; }
        /// <summary>
        /// 明细列表
        /// </summary>
        public List<YWDetailModel> DetailList { set; get; }
    }
    public class YWDetailModel
    {
        public Guid GUID { set; get; }
        /// <summary>
        /// 科目名称
        /// </summary>
        public string BGCodeName { set; get; }
        /// <summary>
        /// 限额
        /// </summary>
        public double PlanTotal { set; get; }
        /// <summary>
        /// 金额
        /// </summary>
        public double BX_Total { set; get; }

        public decimal PlanTotal1 { set; get; }
        /// <summary>
        /// 金额
        /// </summary>
        public decimal BX_Total1 { set; get; }
        /// <summary>
        /// 摘要
        /// </summary>
        public string DocMemo { set; get; }
        /// <summary>
        /// 项目Key
        /// </summary>
        public string ProjectKey { set; get; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { set; get; }
        /// <summary>
        /// 是否是国库

        /// </summary>
        public string IsGuoKu { set; get; }
        /// <summary>
        /// 付款单位名称
        /// </summary>
        public string DWName { set; get; }
        /// <summary>
        /// 部门编号
        /// </summary>
        public Guid GUID_Department{set;get;}
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { set; get; }
        /// <summary>
        /// 财政支付码
        /// </summary>
        public string PaymentNumber { set; get; }
        /// <summary>
        /// 科目编码
        /// </summary>
        public string BGCodeKey { set; get; }
        /// <summary>
        /// 客户GUID
        /// </summary>
        public Guid? GUID_Customer { set; get; }
        /// <summary>
        /// 客户信息（指付款单位）
        /// </summary>
        public string CustomerName { set; get; }
        /// <summary>
        /// 结算方式是否是支票
        /// </summary>
        public bool IsCheck { get; set; }
       
    }
    
}
