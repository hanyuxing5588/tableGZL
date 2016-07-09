using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using System.Data.Objects;
using Business.Accountant;
using BusinessModel;
namespace Business.Casher
{

    public class CHXSave : BaseDocument
    {
        public bool IsSave { get; set; }
        public JsonModel JsonModel { get; set; }
        /// <summary>
        /// 借方 贷方 单据转换 
        /// </summary>
        public Bill CNBill { set; get; }
        /// <summary>
        /// 单据明细列表
        /// </summary>
        public List<BillDetail> CNDetailList { set; get; }

        /// <summary>
        /// 核销
        /// </summary>
        public Bill HXBill { set; get; }
        /// <summary>
        /// 核销明细信息
        /// </summary>
        public List<HX_Detail> HXDetailList { set; get; }
        /// <summary>
        /// 凭证主表信息
        /// </summary>
        public CW_PZMainView PZMian { set; get; }
        /// <summary>
        /// 凭证明细信息
        /// </summary>
        public List<CW_PZDetailView> PZDetailList { set; get; }
        public DateTime docHxDate { get;set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string MsgError { set; get; }
        public CHXSave() : base() { }
        public CHXSave(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        public bool Save(int pzNum,List<Bill> doFaxBills,ref BillSelectFun bsf)
        {
            try
            {//加回滚


                foreach (var item in this.PZDetailList)
                {
                    if (item.GUID_AccountTitle == Guid.Empty) {
                        this.MsgError = "保存失败,会计凭证中科目不能为空！";
                        return false;
                    }
                }
                //保存到出纳付款单 或者 出纳收款单
                var cnMain= SaveCN(this.CNBill, this.CNDetailList);
                if (cnMain == null) {
                    this.MsgError = "保存出纳单失败！";
                    return false;
                }
                //单据号
                bsf.CNBill.DocNum = cnMain.DocNum;

                this.HXBill.GUID = Guid.NewGuid();
                SaveHX(this.HXBill, this.HXDetailList);
                this.PZMian.GUID = Guid.NewGuid();
                this.PZMian.GUID_HXMain = this.HXBill.GUID;
                if (IsSave)
                {
                
                    this.PZMian.GUID=Insert(JsonModel,this.HXBill.GUID);
                }
                else
                {
                    SaveCWPZ(this.PZMian, this.PZDetailList);
                }
              
                this.BusinessContext.SaveChanges();
                var cwPZMain = this.BusinessContext.CW_PZMainView.Where(e => e.GUID == this.PZMian.GUID).FirstOrDefault();
                if (cwPZMain == null) {
                    MsgError = "保存生成的会计凭证错误";
                    return false; 
                }
                //首先得向用友库中保存
                U8Result u8Result = new U8Result();
                U8Certificate u8Certificate = new U8Certificate(this.BusinessContext);
                if (u8Certificate.CheckPZNumber(cwPZMain,pzNum)){
                    MsgError = "该凭证号在用友库中已经存在,不能保存";
                    Delete(this.CNBill.GUID, this.HXBill.GUID, this.PZMian.GUID);
                    return false; 
                }
                u8Certificate.Insert(cwPZMain, ref u8Result,pzNum);
                if (!string.IsNullOrEmpty(u8Result.ResultMessage))
                {
                    //如果U8添加数据出错要回滚数据


                    Delete(this.CNBill.GUID, this.HXBill.GUID, this.PZMian.GUID);
                    this.MsgError = u8Result.ResultMessage;
                    return false;
                }
                //保存算税的单据

                OAOResult oaoResult=new OAOResult ();
                if (doFaxBills.Count > 0) {
                    BillDoFax bdFax = new BillDoFax();
                    var oResult = new OAOResult();
                  //  bdFax.DoTaxCaculte(this.BusinessContext, doFaxBills,DateTime.Now, oResult);
                    bdFax.SaveAlreadyTaxObjects(this.BusinessContext, doFaxBills, docHxDate, oResult);
                    this.BusinessContext.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                Delete(this.CNBill.GUID, this.HXBill.GUID, this.PZMian.GUID);
                this.MsgError = "保存错误！";
                return false;
            }

        }

        //保存到出纳付款单 或者 出纳收款单 Cn_Mani Ce_detail
        public CN_Main SaveCN(Bill bill, List<BillDetail> billDetailList)
        {
            if (bill == null) return null;
            CN_Main main = new CN_Main();            
            main.FillDefault(this, this.OperatorId);
            main.GUID = bill.GUID;
            main.GUID_DocType = bill.GUID_DocType;
            main.GUID_YWType = bill.GUID_YWType;
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");           
            main.DocMemo = bill.DocMemo;
            main.DocDate = docHxDate;
            if (billDetailList != null)
            {
                //添加明细信息
                AddCNDetail(main, billDetailList);
            }
            this.BusinessContext.CN_Main.AddObject(main);
            string actionMsg = "在CN_Main表中进行了新增操作,GUID={" + main.GUID + "}";
            AddoperateLog(this.OperatorId, actionMsg);
            return main;
        }
        /// <summary>
        /// 添加出纳明显信息
        /// </summary>
        /// <param name="main"></param>
        private void AddCNDetail(CN_Main main, List<BillDetail> detailList)
        {

            var orderNum = 0;
            foreach (BillDetail item in detailList)
            {
                orderNum++;
                //明细信息
                CN_Detail temp = new CN_Detail();
                temp.FillDefault(this, this.OperatorId);
                temp.GUID = item.GUID;
                temp.OrderNum = orderNum;
                temp.GUID_SettleType = (Guid)item.GUID_SettleType;
                temp.Total_CN = item.Total_XX;
                temp.GUID_BankAccount = item.BankAccountGuid;
                temp.GUID_Project = item.GUID_Project;
                temp.GUID_Department = main.GUID_Department;
                temp.GUID_Person = main.GUID_Person;
                temp.DetailMemo = item.Memo==null?"":item.Memo;
                temp.IsDC = item.IsDC == null ? false : (bool)item.IsDC;
                //财政支付码




                temp.CN_PaymentNumber = new CN_PaymentNumber();
                temp.CN_PaymentNumber.FillDefault(this, Guid.Empty);
                temp.CN_PaymentNumber.IsGuoKu = item.IsGuoKu;
                temp.CN_PaymentNumber.GUID_BGCode = item.GUID_BGCode;
                temp.CN_PaymentNumber.GUID_Project = temp.GUID_Project;

                temp.GUID_CN_Main = main.GUID;
                temp.GUID_PaymentNumber = temp.CN_PaymentNumber.GUID;
                main.CN_Detail.Add(temp);
                string actionMsg = "在" + temp.ModelName() + "表中进行了新增操作,GUID={" + temp.GUID + "}";
                AddoperateLog(this.OperatorId, actionMsg);
            }

        }
        //保存核销数据  Hx
        public void SaveHX(Bill hxBill, List<HX_Detail> hxDetailList)
        {
            if (hxBill == null) return;
            HX_Main main = new HX_Main();
            main.FillDefault(this, this.OperatorId);
            main.GUID = hxBill.GUID;
            main.GUID_Department = hxBill.GUID_Department;
            main.GUID_DW = hxBill.GUID_DW;
            main.DocDate = docHxDate;
            main.GUID_DocType = hxBill.GUID_DocType;
            main.GUID_YWType = hxBill.GUID_YWType;
            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");          
            if (hxDetailList != null)
            {
                //添加明细信息
                AddHXDetail(main, hxDetailList);
            }
            this.BusinessContext.HX_Main.AddObject(main);
            string actionMsg = "在" + main.ModelName() + "表中进行了新增操作,GUID={" + main.GUID + "}";
            AddoperateLog(this.OperatorId, actionMsg);

        }
        /// <summary>
        /// 添加核销明显信息
        /// </summary>
        /// <param name="main"></param>
        private void AddHXDetail(HX_Main main, List<HX_Detail> detailList)
        {

            var orderNum = 0;
            foreach (HX_Detail item in detailList)
            {
                orderNum++;
                //明细信息
                HX_Detail temp = new HX_Detail();
                temp.FillDefault(this, this.OperatorId);
                temp.GUID_HX_Main = main.GUID;
                temp.ClassID_Main = item.ClassID_Main;
                temp.GUID_Main = item.GUID_Main;
                temp.ClassID_Detail = item.ClassID_Detail;
                temp.GUID_Detail = item.GUID_Detail;
                temp.Total_HX = item.Total_HX;
                temp.IsDC = item.IsDC;
                //temp.DocDate = docHxDate;
                main.HX_Detail.Add(temp);
                string actionMsg = "在" + temp.ModelName() + "表中进行了新增操作,GUID={" + temp.GUID + "}";
                AddoperateLog(this.OperatorId, actionMsg);
            }

        }
        /// <summary>
        /// 保存会计凭证
        /// </summary>
        /// <param name="main"></param>
        /// <param name="detailList"></param>
        public void SaveCWPZ(CW_PZMainView pzmain, List<CW_PZDetailView> detailList)
        {
            if (pzmain == null)
            {
                this.MsgError = "凭证主表信息不能为空！";
                return;
            }
            CW_PZMain main = new CW_PZMain();
            main.FillDefault(this, this.OperatorId);
            main.GUID_CWPeriod = pzmain.GUID_CWPeriod;
            main.BillCount = pzmain.BillCount;
            main.DocDate = pzmain.DocDate;
           // main.GUID_DocType = pzmain.GUID_DocType;
           // main.GUID_YWType = pzmain.GUID_YWType;
            main.GUID_YWType = Guid.Parse("9F7C8EE4-56B6-4E5A-B515-02C845BAB9D1");
            main.GUID_DocType = Guid.Parse("5E942908-D959-4973-8A1F-F7372767E260");
            main.GUID_PZType = pzmain.GUID_PZType;
            main.GUID_HXMain = pzmain.GUID_HXMain;
            main.GUID_AccountDetail = pzmain.GUID_AccountDetail;
           // main.FillCommField(main);
            main.GUID =pzmain.GUID;
           
            // main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");                    
            var mainList= this.BusinessContext.CW_PZMain.Where(e => e.GUID_CWPeriod == main.GUID_CWPeriod && e.GUID_PZType == main.GUID_PZType).ToList();
            main.DocNum = mainList.Count+1;

            if (detailList != null)
            {
                //添加明细信息
                AddCWPZDetail(main, detailList);
            }
            this.BusinessContext.CW_PZMain.AddObject(main);
            string actionMsg = "在" + main.ModelName() + "表中进行了新增操作,GUID={" + main.GUID + "}";
            AddoperateLog(this.OperatorId, actionMsg);

        }
        /// <summary>
        /// 添加会计凭证明显信息
        /// </summary>
        /// <param name="main"></param>
        private void AddCWPZDetail(CW_PZMain main, List<CW_PZDetailView> detailList)
        {

            var orderNum = 0;
            if (detailList== null || detailList.Count == 0)
            {
                this.MsgError = "凭证明细信息无数据！";
                return;
            }
            foreach (CW_PZDetailView item in detailList)
            {
                orderNum++;
                CW_PZDetail temp = new CW_PZDetail();
                temp.FillDefault(this, this.OperatorId);
                //获取分录实际部门
                Guid? departmentid = null;
                if (string.IsNullOrEmpty(item.DepartmentName))
                {
                    departmentid = null;
                }
                else
                {
                    var dep = this.InfrastructureContext.SS_Department.Where(e => e.DepartmentName.ToLower() == item.DepartmentName.ToLower()).FirstOrDefault();
                    if (dep == null)
                        departmentid = null;
                    else
                        departmentid = dep.GUID;
                    
                }


                //明细信息
              
                // temp.FillCommField(item);               
                temp.GUID_PZMAIN = main.GUID;
                temp.PZMemo = item.PZMemo;
                temp.GUID_AccountTitle = item.GUID_AccountTitle;
                temp.Total_PZ = item.Total_PZ;
                temp.IsDC = item.IsDC;
                temp.GUID_Person = item.GUID_Person==Guid.Empty?null:item.GUID_Person;
                temp.GUID_Department = departmentid;
                temp.GUID_Project = item.GUID_Project;
                temp.GUID_Customer =item.GUID_Customer==Guid.Empty? null:item.GUID_Customer;
                temp.GUID_SettleType = item.GUID_SettleType;
                temp.BillNum = item.BillNum;
                temp.BillDate = DateTime.Now;
                temp.OrderNum = orderNum;
                temp.GUID = Guid.NewGuid();
                main.CW_PZDetail.Add(temp);
                string actionMsg = "在" + temp.ModelName() + "表中进行了新增操作,GUID={" + temp.GUID + "}";
                AddoperateLog(this.OperatorId, actionMsg);
            }

        }

        /// <summary>
        /// 添加操作记录
        /// </summary>
        /// <param name="operatorid"></param>
        /// <param name="actionMsg"></param>
        private void AddoperateLog(Guid operatorid, string actionMsg)
        {
            SS_OperateLog log = new SS_OperateLog();
            log.GUID = Guid.NewGuid();
            log.GUID_Operator = operatorid;
            log.IPAddress = " ";
            log.MACAddress = " ";
            log.ComputerName = " ";
            log.ActionTime = DateTime.Now;
            log.ActionMemo = actionMsg;
            this.BusinessContext.SS_OperateLog.AddObject(log);
        }
        /// <summary>
        /// 出纳删除
        /// </summary>
        /// <param name="guid">GUID</param>
        private  void CNDelete(Guid guid)
        {
            CN_Main main = this.BusinessContext.CN_Main.Include("CN_Detail").FirstOrDefault(e => e.GUID == guid);

            List<CN_Detail> details = new List<CN_Detail>();

            foreach (CN_Detail item in main.CN_Detail)
            {
                details.Add(item);
            }

            foreach (CN_Detail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);           
        }
        /// <summary>
        /// 核销删除
        /// </summary>
        /// <param name="guid">GUID</param>
        private void HXDelete(Guid guid)
        {
            HX_Main main = this.BusinessContext.HX_Main.Include("HX_Detail").FirstOrDefault(e => e.GUID == guid);

            List<HX_Detail> details = new List<HX_Detail>();

            foreach (HX_Detail item in main.HX_Detail)
            {
                details.Add(item);
            }

            foreach (HX_Detail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);
        }
        /// <summary>
        /// 凭证删除
        /// </summary>
        /// <param name="guid">GUID</param>
        private void PZDelete(Guid guid)
        {
            CW_PZMain main = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == guid);

            List<CW_PZDetail> details = new List<CW_PZDetail>();

            foreach (CW_PZDetail item in main.CW_PZDetail)
            {
                details.Add(item);
            }

            foreach (CW_PZDetail item in details) { BusinessContext.DeleteConfirm(item); }
            BusinessContext.Detach(main);
            BusinessContext.DeleteConfirm(main);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="cnGUID">出纳GUID</param>
        /// <param name="hxGUID">核销GUID</param>
        /// <param name="pzGUID">凭证</param>
        public  void Delete(Guid cnGUID,Guid hxGUID,Guid pzGUID)
        {
            try
            {
                if (cnGUID.IsNullOrEmpty())
                {
                    this.MsgError = "出纳GUID为空！";
                    return;
                }
                if (hxGUID.IsNullOrEmpty())
                {
                    this.MsgError = "核销GUID为空！";
                    return;
                }
                if (pzGUID.IsNullOrEmpty())
                {
                    this.MsgError = "凭证GUID为空！";
                    return;
                }
                CNDelete(cnGUID);
                HXDelete(hxGUID);
                PZDelete(pzGUID);
                this.BusinessContext.SaveChanges();
            }
            catch
            {
                this.MsgError = "出纳，核销，凭证回滚数据时失败！";
                return;
            }
        }



        public  Guid Insert(JsonModel jsonModel, Guid hxMian)
        {
            var hjpz= new 会计凭证(OperatorId, "kjpz");;
            if (jsonModel.m == null) return Guid.Empty;
            CW_PZMain main = new CW_PZMain();
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();
            main.GUID_PZType = this.BusinessContext.CW_PZType.FirstOrDefault().GUID;//默认为记
            main.GUID_HXMain = hxMian;
            var periodModel = hjpz.GetCWPeriodModel(jsonModel);
            main.GUID_AccountDetail = hjpz.GetAccountDetailGUID(periodModel);
            main.GUID_CWPeriod = hjpz.GetCWPeriodGUID(periodModel);
            var doctype = this.InfrastructureContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl.ToLower() == "kjpz");
            if (doctype != null)
            {
                main.GUID_DocType = doctype.GUID;
                main.GUID_YWType = (Guid)doctype.GUID_YWType;
            }

            var docNum = this.BusinessContext.CW_PZMain.Where(e => e.GUID_PZType == main.GUID_PZType && e.GUID_CWPeriod == main.GUID_CWPeriod).ToList().Count();
            main.DocNum = docNum + 1;


            List<CW_PZDetailView> insertdetails = new List<CW_PZDetailView>();
            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CW_PZDetail temp = new CW_PZDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        var rt = hjpz.AddDetailEx(main, row, Grid.r.IndexOf(row));
                        string actionMsg = "在CW_PZDetail表中进行了新增操作,GUID={" + rt.GUID + "}";
                        AddoperateLog(this.OperatorId, actionMsg);
                        CW_PZDetailView item = rt.ConvertToView(this.InfrastructureContext);
                        insertdetails.Add(item);
                    }
                }
            }
            this.PZDetailList = insertdetails;
            this.BusinessContext.CW_PZMain.AddObject(main);
            return main.GUID;
        }
    }
}
