using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using System.Data.Objects;
using System.ComponentModel;
using System.Reflection;
using System.Linq.Expressions;
using Infrastructure;
using BusinessModel;
namespace Business.Casher
{
  
    public class 核销处理 : BaseDocument
    {

        public 核销处理() : base() { }
        public 核销处理(Guid operatorId, string modelUrl) : base(operatorId, modelUrl) { }

        #region 选单查询数据

        #region 选择各类单据的查询语句模板

        /*收入*/ //GUID_Department=''{42C526DC-C22E-40D5-B035-ABFFA6EA7E62}'' GUID_Person=''{E591C5E1-EF7E-488C-BB35-F5404C506926}'' and month(DocDate)=''3''
        private string GetFormatSR() 
        {
            return "select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_sr) from sr_detail where guid_sr_main=sr_mainview.guid) as Total_XX"
        + " from sr_mainview  where docState<>9 and guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum";
        }
        /*往来*/
        private string GetFormatWL()
        {
            return "select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_wl) from wl_detail where guid_wl_main=wl_mainview.guid) as Total_XX from wl_mainview "
        + " where   guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum";
        }
        /*提现*/
        private string GetFormatTX()
        {
            return "select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_cash) from cn_cashdetail where guid_cn_cashmain=cn_cashmainview.guid) as Total_XX "
            + " from cn_cashmainview  where docState<>9 and guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum";
        }
        /*收款*/
        private string GetFormatSK()
        {
            return "select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,total_sk as Total_XX from SK_mainview  where  docState<>9 and guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum";
        }
        /*公务卡汇总*/
        private string GetFormatGWK() 
        {
            return "select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_bx) from bx_detail where guid in (select guid_bxdetail from BX_Collectdetail "
+ " where guid_bxcollectmain=BX_Collectmainview.guid)) as Total_XX from BX_Collectmainview  where docState<>9 and guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum";

        }
        /*基金*/
        private string GetFormatJJ() 
        {
            return " select distinct DocMemo,Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_jj) from jj_detail where guid_jj_main=jj_mainview.guid) as Total_XX from jj_mainview  where guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} order by DocNum ";
        }
        /*现金*/
        private string GetFormatBX() 
        {
            return "select distinct Guid,DocNum,DWName,DepartmentName,PersonName,DocTypeName,convert(char(10),DocDate,120) as DocDate,(select sum(total_bx) from bx_detail where guid_bx_main=bx_mainview.guid) as Total_XX ,DocMemo from bx_mainview "
               + " where docState<>9 and guid not in (select Guid_Main from HX_DetailView) and year(DocDate)='{0}' {1} {2} {3} "
               + " order by DocNum";
        }
        #endregion
        // 查询单据记录
        private List<object> SelectDocList(BillHistoryCondition conditions, string strSqlFormat)
        {
            var historyconditions = conditions;
            string pFilter = "";string strMainGuid = ""; ;//部门和人员的过滤条件
            if (conditions.Month.Trim() == "0") conditions.Month = "";
            if (conditions.treeModel == null) conditions.treeModel = "";
            if (conditions.treeModel.ToLower() == "ss_project"){//效率待改进
                List<SS_Project> projectList = new List<SS_Project>();
                SS_Project projectModel = new SS_Project();
                projectModel.GUID = historyconditions.treeValue;
                projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                var projectGUID = projectList.Select(e => e.GUID);
                var mainGuidList = this.BusinessContext.BX_DetailView.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project)).Select(e => e.GUID_BX_Main).Distinct().ToList();
                strMainGuid = ChangeListGuidToStr(mainGuidList);
            }
            if (conditions.treeModel.ToLower() == "ss_projectclass"){
                List<SS_Project> projectList = new List<SS_Project>();
                SS_ProjectClass projectclassModel = new SS_ProjectClass();
                projectclassModel.GUID = historyconditions.treeValue;
                projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                var projectUID = projectList.Select(e => e.GUID);
                var mainGuidList = this.BusinessContext.BX_DetailView.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project)).Select(e => e.GUID_BX_Main).Distinct().ToList();
                strMainGuid = ChangeListGuidToStr(mainGuidList);
            }
            if (conditions.treeModel == "SS_Person")
            {//人员
                pFilter = string.Format(" and GUID_Person='{0}'", conditions.treeValue);
            }
            if (conditions.treeModel == "SS_Department")
            {//部门
                pFilter = string.Format(" and GUID_Department='{0}'", conditions.treeValue);
            }
            if (!string.IsNullOrEmpty(strMainGuid)) { //项目
                pFilter = string.Format(" and Guid in({0})", strMainGuid);
            }
            if (!string.IsNullOrEmpty(conditions.Month))
            {//月份
                conditions.Month = string.Format(" and month(DocDate)='{0}'", conditions.Month);
            }
            if (!string.IsNullOrEmpty(conditions.DocNum))
            {//单号
                conditions.DocNum = string.Format(" and DocNum like '%{0}%'", conditions.DocNum);
            }
            string sql = string.Format(strSqlFormat, conditions.Year, conditions.Month, pFilter, conditions.DocNum);
            using (var context = new BusinessEdmxEntities())
            {
                var entList = context.ExecuteStoreQuery<SelectDocModel>(sql).ToList<object>();
                return entList;
            }
        }
        //选单
        public override List<object> History(SearchCondition conditions)
        {
            if (conditions != null)
            {
                BillHistoryCondition billconditions = (BillHistoryCondition)conditions;
                if (billconditions == null) return null;
                int iBiilDocType;
                var docType = int.TryParse(billconditions.DocYWType, out iBiilDocType) ? (EBillDocType)iBiilDocType : EBillDocType.报销管理;
                switch (docType)
                {
                    case EBillDocType.报销管理:
                        return SelectDocList(billconditions,this.GetFormatBX());
                    case EBillDocType.收入管理:
                        return SelectDocList(billconditions, this.GetFormatSR());
                    case  EBillDocType.往来管理:
                        return SelectDocList(billconditions, this.GetFormatWL());
                    case EBillDocType.提现管理:
                        return SelectDocList(billconditions, this.GetFormatTX());
                    case EBillDocType.收款管理:
                        return SelectDocList(billconditions, this.GetFormatSK());
                    case EBillDocType.公务卡汇总管理:
                        return SelectDocList(billconditions, this.GetFormatGWK());
                    case EBillDocType.专用基金管理:
                        return SelectDocList(billconditions, this.GetFormatJJ());
                }
            }
            return null;
        }
        #endregion

        #region 选单 确定转换数据
        //如果是人员        public override object ChangeDocData(string guids, string docType,bool isBorrow,DateTime dt)
        {
            var dicType2Name = this.GetDicType2Guid(guids, docType);
            BillSelectFun billSelectFun = new BillSelectFun(dicType2Name, isBorrow);
            billSelectFun.DtDocTime = dt;
            billSelectFun.Main();
            return billSelectFun.GetShowModel();
           
        }
        #endregion

        //借款
        public override List<object> BorrowList(string userBorrowID)
        {
            //不支持分布和核销
            Guid gid = Guid.Empty;
            if (!string.IsNullOrEmpty(userBorrowID)) {
                Guid.TryParse(userBorrowID, out gid);
            }
            string sqlFormat = " select *  from (" +
             "select  GUID,DocNum,convert(nvarchar,DocDate,23) as DocDate,PersonName,DWName,DepartmentName,MakeDate from wl_mainview where doctypekey=10 and guid in (select guid_main from hx_Detail where  classId_Main=30  group by guid_main having count(*)=1 ) " +
             " and {0} )a " +
             " join" +
             " (select GUID_WL_Main,sum(Total_WL) as Total_Real from wl_detail  group by  GUID_WL_Main)  b on a.GUID=b.GUID_WL_Main  order by docNUm,MakeDate desc";
            string sql = string.Format(sqlFormat, gid == Guid.Empty ? "1=1" : string.Format("GUID_Person='{0}'", gid));
            return this.BusinessContext.ExecuteStoreQuery<JKModel>(sql).ToList<object>();

        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="paramss"></param>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public override object SaveWithReturnObj(string paramss, JsonModel jsonModel)
        {
            string strMsg = string.Empty;
            var strParams = paramss.Split('#');
            string guids = strParams[0];//单据Id
            string docType = strParams[1];//单据类型
            string strisDC = strParams[2];
            int pzNum = 0;
            int.TryParse(strParams[4],out pzNum);
            DateTime dt = DateTime.Now;
            DateTime.TryParse(strParams[3], out dt);
            bool isDC = strisDC == "1"?true:false;
            bool isSave = string.IsNullOrEmpty(strParams[5] + "").ToString() == "1";
            var dicType2Guid = GetDicType2Guid(guids, docType);
            BillSelectFun billSelectFun = new BillSelectFun(dicType2Guid, isDC);
            billSelectFun.DtDocTime = dt;
            billSelectFun.Main();
            var doFaxBills=billSelectFun.BillDoTaxs;
            if (string.IsNullOrEmpty(billSelectFun.ErrMsg))
            {
                CHXSave objSave = new CHXSave(this.OperatorId, this.ModelUrl);
                objSave.JsonModel = jsonModel;
                objSave.IsSave = (jsonModel.m.Count > 0 && jsonModel.d.Count > 0 )? true : false;
                objSave.CNBill = billSelectFun.CNBill;
                //核销验证                 
                if (CheckHXAll(dicType2Guid,out strMsg))
                {
                    objSave.docHxDate = dt;               
                    objSave.CNDetailList = billSelectFun.CNBillDetail;
                    objSave.HXBill = billSelectFun.HXBill;
                    objSave.HXDetailList = billSelectFun.ListHXDetail;
                    objSave.PZMian = billSelectFun.CwPZMain;
                    objSave.PZDetailList = billSelectFun.ListPZDetail;

                    objSave.Save(pzNum, doFaxBills, ref billSelectFun);
                    if (!string.IsNullOrEmpty(objSave.MsgError))
                    {
                        strMsg = objSave.MsgError;
                    }
                    else
                    {
                        if (objSave.IsSave)
                        {
                            billSelectFun.ListPZDetail = objSave.PZDetailList;
                        }
                    }


                }                
            }
            else
            {
                strMsg = billSelectFun.ErrMsg;
            }
            if (string.IsNullOrEmpty(strMsg))
            {
                
                strMsg = "保存成功！";
                return billSelectFun.GetShowModel();
            }
            return new { error=strMsg};           
        }
        /// <summary>
        /// 校验核销数据 在核销表中是否存在此数据，如果已经核销的不能再核销
        /// </summary>
        /// <param name="guids"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CheckHXAll(Dictionary<EBillDocType, List<Guid>>  dicType2Name,out string msg) 
        {
            msg = "";
            foreach (var item in dicType2Name.Keys)
	        {
                if (!CheckHX(dicType2Name[item], false, item, out msg)) 
                {
                    return false;
                }
		        
	        }
            return true;
           
        }
        public bool CheckHX(List<Guid> guids,bool isDC, EBillDocType docType, out string msg)
        {
            msg = string.Empty;
            if (guids == null && guids.Count == 0)
            {
                msg = "核销单据不能为空！";
                return false;
            }
            if (docType == EBillDocType.往来管理)
            {
                var list = this.BusinessContext.HX_Detail.Where(e => guids.Contains(e.GUID_Main) && e.IsDC==isDC).ToList();
                if (list != null && list.Count > 0)
                {
                    msg = "此单据已核销！";
                    return false;
                }
            }
            else
            {
                var list = this.BusinessContext.HX_Detail.Where(e => guids.Contains(e.GUID_Main)).ToList();
                if (list != null && list.Count > 0)
                {
                    msg = "此单据已核销！";
                    return false;
                }
            }
            return true;
        }

        public Dictionary<EBillDocType, List<Guid>> GetDicType2Guid(string guids,string docType) 
        {
            Dictionary<EBillDocType, List<Guid>> dic = new Dictionary<EBillDocType, List<Guid>>();
            var guidArr = guids.Split('$');
            var docTypeArr = docType.Split('$');
             for (int i = 0; i < docTypeArr.Length; i++)
			{
			    var item=docTypeArr[i];
                var type = GetEBillDocType(item);
                var list = ChangeStrArr(guidArr[i].Split(',').ToList());
                dic.Add(type, list);
            }
             return dic;

        } 
        public List<Guid> ChangeStrArr(IList<string> listStr) 
        {
            List<Guid> listGuid = new List<Guid>();
            foreach (var item in listStr)
            {
                Guid gid;
                if (Guid.TryParse(item,out gid)) {
                    listGuid.Add(gid);
                }
            }
            return listGuid;
        }
        public EBillDocType GetEBillDocType(string item) 
        {
            int i = 0;
            int.TryParse(item, out i);
            return (EBillDocType)i;
        }

        //获取借款总金额
        public override JsonModel Retrieve(string guid)
        {
            var jm = new JsonModel();
            BillSelectFun billSelectFun = new BillSelectFun();
            jm.result= billSelectFun.GetBorrowSumMoney().ToString("f2");
            return jm;
        }
        private string ChangeListGuidToStr(List<Guid> listGuid)
        {
            string strFormat = "'{0}',";
            string strParams = "";
            foreach (var item in listGuid)
            {
                strParams += string.Format(strFormat, item);
            }
            return strParams.Substring(0, strParams.Length - 1);
        }
        //打印数据
        public PrintModel GetPrintData(string docGuid, string docTypeKey, string paymentnumber)
        {
            
            //07 汇款报销单
            switch (docTypeKey)
            {
                case "0"://汇款报销单 //EBillDocType.报销管理
                    return GetPrintMain_BX_Model(docGuid,paymentnumber);
                case "4"://收入凭单//var skglKey=((int)EBillDocType.收款管理).ToString();
                    return GetPrintSK_Main_Model(docGuid);
                case "2"://往来//var skglKey=((int)EBillDocType.收款管理).ToString();
                    return GetPrintWL_Main_Model(docGuid, paymentnumber);
                default:
                    break;
            }
            return null;
        }
        private PrintModel GetPrintWL_Main_Model(string docGuid, string paymentnumber)
        {
            Guid gid = Guid.Empty;
            if (!string.IsNullOrEmpty(docGuid))
            {
                Guid.TryParse(docGuid, out gid);
            }

            var sql = string.Format(@"SELECT  CAST(YEAR(docdate) AS NVARCHAR(10)) AS Year ,
        CAST(MONTH(docdate) AS NVARCHAR(10)) AS Month ,
        CAST(DAY(docdate) AS NVARCHAR(10)) AS DAY ,
        SUM(a.Total_WL) AS Total_BX ,
        a.CustomerName AS skName ,
        CustomerBankName AS hrBankName ,
        CustomerBankNumber AS skBankAccountNo ,
        b.PaymentNumber AS CN_PaymentNumber ,
        DocDate ,
        DocMemo
FROM    dbo.WL_DetailView a
        LEFT JOIN dbo.WL_MainView c ON a.GUID_WL_Main = c.GUID
        LEFT JOIN dbo.CN_PaymentNumber b ON a.GUID_PaymentNumber = b.GUID
        LEFT JOIN dbo.SS_Customer d ON a.GUID_Cutomer=d.GUID
WHERE   GUID_WL_Main = '{0}'
        AND b.PaymentNumber = '{1}'
GROUP BY a.GUID_WL_Main ,
        a.CustomerName ,
        d.CustomerBankName ,
        d.CustomerBankNumber ,
        b.PaymentNumber ,
        c.DocDate ,
        c.DocMemo
        
        ", docGuid, paymentnumber);
            PrintModel model = new PrintModel();
            try
            {
                model = this.BusinessContext.ExecuteStoreQuery<PrintModel>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {

            }


            if (!string.IsNullOrEmpty(model.CN_PaymentNumber))
            {
                model.hkBankAccountNo = "7111010173300000990";
            }
            else
            {
                model.hkBankAccountNo = "7111010182600296021";
            }
            model.Year = DateTime.Now.Year.ToString();
            model.Month = DateTime.Now.Month.ToString();
            model.Day = DateTime.Now.Day.ToString();
            model.Total_BX = Math.Round(model.Total_BX, 2);
            return model;
        }
        private PrintModel GetPrintMain_BX_Model(string docGuid,string paymentnumber)
        {
            Guid gid = Guid.Empty;
            if (!string.IsNullOrEmpty(docGuid))
            {
                Guid.TryParse(docGuid, out gid);
            }
            
            ////明细信息
            //var detail = this.BusinessContext.BX_DetailView.Where(e=>e.GUID_BX_Main==gid);
            //var main = this.BusinessContext.BX_MainView.Where(e=>e.GUID==gid);
            
            //var dbdetai = from a in detail
            //              group a by new { a.GUID_BX_Main, a.CustomerName, a.GUID_PaymentNumber, a.CustomerBankNumber, a.CustomerBankName } into g
            //              select new { GUID_BX_Main = g.Key.GUID_BX_Main, CustomerName = g.Key.CustomerName, CustomerBankNumber = g.Key.CustomerBankNumber, CustomerBankName = g.Key.CustomerBankName,GUID_PaymentNumber = g.Key.GUID_PaymentNumber, Total_BX = g.Sum(a => a.Total_Real) };
            //var o = (from d in dbdetai
            //         join m in main on d.GUID_BX_Main equals m.GUID //into temp
            //         join p in this.BusinessContext.CN_PaymentNumberView  on  d.GUID_PaymentNumber equals p.GUID into temp 
            //         from p in temp.DefaultIfEmpty()
            //         where d.GUID_BX_Main != null && m.GUID != null && p.PaymentNumber==paymentnumber
            //         select new { m.GUID, m.DocNum, m.DocDate, m.DWName, m.PersonName, m.BillCount, d.Total_BX,d.CustomerName,d.CustomerBankNumber,d.CustomerBankName, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey,p.PaymentNumber });

            //var model = o.AsEnumerable().Select(e => new PrintModel
            //{               
                
            //    Year=e.DocDate.Year.ToString(),
            //    Month=e.DocDate.Month.ToString(),
            //    Day=e.DocDate.Day.ToString(),
            //    skName=e.CustomerName,              
            //    Total_BX = e.Total_BX,
            //    DocMemo=e.DocMemo,
            //    CN_PaymentNumber=e.PaymentNumber,
            //    skBankAccountNo=e.CustomerBankNumber,
            //    hrBankName = e.CustomerBankName
            //}).FirstOrDefault();
            var sql =string.Format(@"SELECT  
	    CAST( YEAR(docdate) AS NVARCHAR(10)) AS Year,
	    CAST(   month(docdate) AS NVARCHAR(10))  AS Month,
	    CAST(      DAY(docdate) AS NVARCHAR(10)) AS DAY,
		SUM(Total_Real) AS Total_BX ,
        CustomerName AS skName ,
        CustomerBankName  AS hrBankName,
        CustomerBankNumber  AS skBankAccountNo,
        b.PaymentNumber AS CN_PaymentNumber,
        DocDate,DocMemo
FROM    dbo.BX_DetailView a
LEFT JOIN dbo.BX_MainView c ON a.GUID_BX_Main =c.GUID
LEFT JOIN dbo.CN_PaymentNumber b ON a.GUID_PaymentNumber=b.GUID
WHERE   GUID_BX_Main = '{0}' and b.PaymentNumber='{1}'
GROUP BY a.GUID_BX_Main ,
        a.CustomerName ,
        a.CustomerBankName ,
        a.CustomerBankNumber ,
        b.PaymentNumber,c.DocDate,c.DocMemo
        ", docGuid,paymentnumber);
            PrintModel model = new PrintModel();
            try
            {
                 model = this.BusinessContext.ExecuteStoreQuery<PrintModel>(sql).FirstOrDefault();
            }
            catch (Exception ex)
            {
                
            }
       

            if (!string.IsNullOrEmpty(model.CN_PaymentNumber))
            {
                model.hkBankAccountNo="7111010173300000990";
            }
            else 
            {
                model.hkBankAccountNo = "7111010182600296021";
            }
            model.Year = DateTime.Now.Year.ToString();
            model.Month = DateTime.Now.Month.ToString();
            model.Day = DateTime.Now.Day.ToString();
            model.Total_BX =Math.Round( model.Total_BX,2);
           return model;
        }
        /// <summary>
        /// 收入凭单打印数据
        /// </summary>
        /// <param name="docGuid"></param>
        /// <returns></returns>
        private PrintModel GetPrintSK_Main_Model(string docGuid)
        {
            Guid gid = Guid.Empty;
            if (!string.IsNullOrEmpty(docGuid))
            {
                Guid.TryParse(docGuid, out gid);
            }

            //明细信息
           
            var main = this.BusinessContext.SK_MainView.Where(e => e.GUID == gid);           
            var o = (
                     from m in main 
                     join p in this.BusinessContext.CN_PaymentNumberView on m.GUID_PaymentNumber equals p.GUID into temp
                     from p in temp.DefaultIfEmpty()
                     where  m.GUID != null 
                     select new { m.GUID, m.DocNum, m.DocDate, m.DWName, m.PersonName, m.BillCount, m.Total_SK, m.CustomerName, m.CustomerBankNumber,m.CustomerBankName, m.DocMemo, m.YWTypeName, m.DocTypeName, m.MakeDate, m.YWTypeKey, p.PaymentNumber });

            var model = o.AsEnumerable().Select(e => new PrintModel
            {

                Year = e.DocDate.Year.ToString(),
                Month = e.DocDate.Month.ToString(),
                Day = e.DocDate.Day.ToString(),
                skName = e.CustomerName,
                Total_BX =e.Total_SK,
                DocMemo = e.DocMemo,
                CN_PaymentNumber = e.PaymentNumber,
                skBankAccountNo = e.CustomerBankNumber,
                hrBankName = e.CustomerBankName
            }).FirstOrDefault();
            return model;
        }
        /// <summary>
        /// 获取汇款审批单凭证
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<TreeNodeModel> GetTreehkspdpz(string guid)
        {
            var List = GetPaymentnumberList1(guid);            
            var pagedhUrl = "/Print/dhpz";//电汇凭证
            var pagexhUrl = "/Print/xhpz";//信汇凭证
            var paynumberList = List.Select(e=>e.PaymentNumber).Distinct().ToList();

            List<TreeNodeModel> list = new List<TreeNodeModel>();
            foreach (var item in paynumberList)
            {                
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = Guid.NewGuid().ToString();

                treeModel.@checked = false;
                treeModel.text ="中国电信(电汇凭证)";
                Dictionary<string, string> dic = new Dictionary<string, string>();                
                dic["URL"] = pagedhUrl;
                dic["PaymentNumber"] = item;
                treeModel.attributes = dic;
                list.Add(treeModel);

                TreeNodeModel treeModel1 = new TreeNodeModel();
                treeModel1.state = "open";
                treeModel1.id = Guid.NewGuid().ToString();
                treeModel1.@checked = false;
                treeModel1.text = "中国电信(信汇凭证)";
                Dictionary<string, string> dic1 = new Dictionary<string, string>();            
                dic1["URL"] = pagexhUrl;
                dic1["PaymentNumber"] = item;
                treeModel1.attributes = dic1;
                list.Add(treeModel1);
            }
            return list;

        }
        public List<CN_PaymentNumberView> GetPaymentnumberList(string docGuid)
        {
            var gGuid = Guid.Empty;
            Guid.TryParse(docGuid, out gGuid);
            var detail = this.BusinessContext.BX_Detail.FirstOrDefault(e => e.GUID_BX_Main == gGuid);
            if(detail!=null){
            var list = this.BusinessContext.CN_PaymentNumberView.Where(e=>detail.GUID_PaymentNumber==e.GUID).ToList();
                return list;
            }
            var detailDetail = this.BusinessContext.WL_Detail.FirstOrDefault(e => e.GUID_WL_Main == gGuid);
            if (detailDetail != null)
            {
                var list = this.BusinessContext.CN_PaymentNumberView.Where(e => detailDetail.GUID_PaymentNumber == e.GUID).ToList();
                return list;
            }
            return null;
        }

        public List<CN_PaymentNumberView> GetPaymentnumberList1(string docGuid)
        {
            var gGuid = Guid.Empty;
            Guid.TryParse(docGuid, out gGuid);
            var detail = this.BusinessContext.BX_Detail.Where(e => e.GUID_BX_Main == gGuid).Select(e => e.GUID_PaymentNumber).ToList();

            if (detail != null && detail.Count>0)
            {
                var list = this.BusinessContext.CN_PaymentNumberView.Where(e =>detail.Contains(e.GUID)).ToList();
                return list;
            }
            var detailDetail = this.BusinessContext.WL_Detail.Where(e => e.GUID_WL_Main == gGuid).Select(e => e.GUID_PaymentNumber).ToList();;
            if (detailDetail != null && detailDetail.Count > 0)
            {
                var list = this.BusinessContext.CN_PaymentNumberView.Where(e => detailDetail.Contains(e.GUID)).ToList();
                return list;
            }
            return null;
        }
    }

    #region 模型
    public class JKModel 
    {
        public Guid Guid { get; set; }
        public string DocNum{get;set;}
        public double Total_Real { get; set; }
        public string DocDate { get; set; }
        public string PersonName { get; set; }
        public string DWName { get; set; }
        public string DepartmentName { get; set; }
    }
    public class HXShowModel
    {
        public CW_PZMainView DwPZMain { get; set; }
        public List<CW_PZDetailView> DwPZDetails { get; set; }
        public List<Bill> listDebit  {  get; set; }
        public List<Bill> listCredit { get; set; }
        public BorrowInfo borrow { get; set; }
        public List<SumBillInfo> listSum { get; set; }
        public List<object> listKJPZ { get; set; }
        public double SumTotal { get; set; }
        public string DocDate { get; set; }
        public string zt { get; set; }
        public int? yzt { get; set; }
        public int kjqj { get; set; }
        public string error { get; set; }
        public int u8Num { get; set; }

    }

    //public enum EBillDocType 
    //{
    //    报销管理=0,
    //    收入管理=1,
    //    往来管理=2,
    //    提现管理=3,
    //    收款管理=4,
    //    公务卡汇总管理=5,
    //    专用基金管理=6
    //}
    //单据转换模型 
    public class BillChangeModel 
    {
        public List<Bill>   JFDoc { get; set; }
        public List<KJPZBillDetail> KJDoc { get; set; }
    }
  
    public class SumBillInfo
    {
        public string BankName{get;set;}
        public double Total_XX{get;set;}
        public string PaymentNumber{get;set;}
    }
    public class BorrowInfo
    {
        public string PersonName { get; set; }
        public string DepartmentName { get; set; }
        public string DWName { get; set; }
        public double Money { get; set; }
        public Guid PersonId { get; set; }
        public Guid DwId { get; set; }
        public Guid DepartmentId { get; set; }


    }
    //会计凭证单据
    public class KJPZBillDetail 
    {

    }
    //选单的单据模型

    public class SelectDocModel 
    {
        public Guid Guid { get; set; }
        public string DocNum { get; set; }
        public string DWName { get; set; }
        public string DepartmentName { get; set; }
        public string PersonName { get; set; }
        public string DocTypeName { get; set; }
        public string DocDate { get; set; }
        public double? Total_XX { get; set; }
        public string DocMemo { get; set; }
    }
    //计算借款信息准备的model
    public class BorrowTempModel 
    { 
        public Guid wlguid { get; set; }
        public Guid wlmainguid { get; set; }
        public double total_wl { get; set; }
        public bool wldc { get; set; }
        public Guid hxguid { get; set; }
        public bool hxdc { get; set; }
        public double total_hx { get; set; }

    }
    //打印模板
    public class PrintModel
    {
        public string hkBankAccountNo { get; set; }
        /// <summary>
        /// 收款人名称
        /// </summary>
        public string skName { set; get; }
        /// <summary>
        /// 收款人账号
        /// </summary>
        public string skBankAccountNo { set; get; }
        /// <summary>
        /// 收款省
        /// </summary>
        public string skProvince { set; get; }
        /// <summary>
        /// 收款市/县
        /// </summary>
        public string skCity { set; get; }
        /// <summary>
        /// 汇入行名称
        /// </summary>
        public string hrBankName { set; get; }
        /// <summary>
        /// 金额
        /// </summary>
        public double Total_BX { set; get; }
        /// <summary>
        /// 支付密码
        /// </summary>
        public string CN_PaymentNumber { set; get; }
        /// <summary>
        /// 附近信息以及用途
        /// </summary>
        public string DocMemo { set; get; }
        
        /// <summary>
        /// 年
        /// </summary>
        public string Year { set; get; }
        /// <summary>
        /// 月
        /// </summary>
        public string Month { set; get; }
        /// <summary>
        /// 日
        /// </summary>
        public string Day { set; get; }
        
    }
    #endregion
}
