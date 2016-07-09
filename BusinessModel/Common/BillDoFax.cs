using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.ComponentModel;
using System.Reflection;
using System.Linq.Expressions;
using BusinessModel;
namespace Business.Casher
{

    public class OAOResult
    {
        public int ResultNumber = 1;
        public string ResultMessage = string.Empty;
    }

    public class TempFee
    {
        public Guid Guid { get; set; }
        public double Total_Tax { get; set; }
        public double Total_BX { get; set; }
    }
    //算税
    public class BillDoFax
    {
        public static string GetInviteSql(Guid invitePersonGuid)
        {
            string sqlFormat = "select isnull(sum(Total_Real),0) as SumTotalReal,isnull(sum(Total_Tax),0) as SumTotalTax,isnull(sum(Total_Bx),0) as SumTotalBx from BX_InviteFee" +
                   " where guid_inviteperson='{0}' and  GUID_BX_main in (select guid from BX_main where  month(MakeDate)='{1}' and year(MakeDate)='{2}')   and  GUID_BX_main not in (select guid from BX_main where DocState=9) and GUID_BX_main " +
                    "In (select DataId from OAO_WOrkFlowProcessData)";
            return string.Format(sqlFormat, invitePersonGuid, DateTime.Now.Month, DateTime.Now.Year);
        }
        public  double GetPersonTax(ObjectContext context,double money,Guid personId) 
        {
            string sql = GetInviteSql(personId);

            var tm = context.ExecuteStoreQuery<TaxMoney>(sql).FirstOrDefault();
            tm.SumTotalReal = tm.SumTotalReal + money;
            var Tax = CalcTaxWithBehind(tm.SumTotalReal);
            return Tax - tm.SumTotalTax;

        }
        private Dictionary<Bill, List<TempFee>> needUpdates = new Dictionary<Bill, List<TempFee>>();

        /// <summary>
        /// 算税
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="InviteObjects">外聘人员单据</param>
        /// <param name="HxDateTime">核销日期</param>
        /// <param name="result">结果</param>
        public void DoTaxCaculte(ObjectContext context, List<Bill> InviteObjects, DateTime HxDateTime, OAOResult result)
        {
            //清空需要保存的单据，重新加载

            this.needUpdates.Clear();
            if (InviteObjects == null || InviteObjects.Count == 0) return;
            foreach (Bill item in InviteObjects)
            {
                CaculteTax(context, item, HxDateTime, result);
                if (result.ResultNumber == 0) return;
            }

        }
        /// <summary>
        /// 保存算税对象
        /// </summary>
        /// <param name="context"></param>
        /// <param name="InviteObjects"></param>
        /// <param name="HxDateTime"></param>
        /// <param name="result"></param>
        public void SaveAlreadyTaxObjects(ObjectContext context, List<Bill> InviteObjects, DateTime HxDateTime, OAOResult result)
        {
            List<string> UpdateSqls = new List<string>();
            result.ResultNumber = 1; result.ResultMessage = "";
            DoTaxCaculte(context, InviteObjects, HxDateTime, result);
            if (result.ResultNumber == 0) return;
            if (this.needUpdates.Count == 0) return;
            foreach (Bill InviteObject in this.needUpdates.Keys)
            {
                List<TempFee> Fees = this.needUpdates[InviteObject];
                List<BX_InviteFee> realFees = context.CreateObjectSet<BX_InviteFee>().Where(e => e.GUID_BX_Main == InviteObject.GUID).ToList();//获得所有外聘人员

                if (realFees == null || realFees.Count == 0) continue;
                foreach (TempFee Fee in Fees)
                {

                    var realFee = realFees.FirstOrDefault(e => e.GUID == Fee.Guid); ///realFees.FirstOrDefault(e => e.GUID == Fee.Guid);
                    if (realFee != null)
                    {
                        realFee.Total_Tax = Fee.Total_Tax; realFee.Total_BX = Fee.Total_BX;
                    }
                }
                var guidTemp = InviteObject.Details[0].GUID;
                var bxdetail = context.CreateObjectSet<BX_Detail>().FirstOrDefault(e => e.GUID == guidTemp);
                if (bxdetail != null)
                {
                    bxdetail.Total_Tax = InviteObject.Details[0].Total_Tax;
                    bxdetail.Total_BX = InviteObject.Details[0].Total_XX;
                }
            }
        }
        public void SaveAlreadyDoTaxObjects(ObjectContext context, List<Bill> InviteObjects)
        {
            List<string> UpdateSqls = new List<string>();
            if (this.needUpdates.Count == 0) return;
            foreach (Bill InviteObject in InviteObjects)
            {
                var bxdetail = context.CreateObjectSet<BX_Detail>().FirstOrDefault(e => e.GUID == InviteObject.Details[0].GUID);
                if (bxdetail != null)
                {
                    bxdetail.Total_Tax = InviteObject.Details[0].Total_Tax;
                    bxdetail.Total_BX = InviteObject.Details[0].Total_XX;
                }
            }
        }
        /// <summary>
        /// 算税函数
        /// </summary>
        /// <param name="context"></param>
        /// <param name="InviteObject"></param>
        /// <param name="HxDateTime"></param>
        /// <param name="result"></param>
        private void CaculteTax(ObjectContext context, Bill InviteObject, DateTime HxDateTime, OAOResult result)
        {
            double Tax = 0, cReal = 0, TotalTax = 0, TotalBx = 0;
            Dictionary<Guid, double> invitePersonRealDic = new Dictionary<Guid, double>();
            Dictionary<Guid, double> invitePersonTaxDic = new Dictionary<Guid, double>();
            if (InviteObject == null || InviteObject.Details == null || InviteObject.Details.Count == 0) return;
            List<BX_InviteFeeView> Fees = context.CreateObjectSet<BX_InviteFeeView>().Where(e => e.GUID_BX_Main == InviteObject.GUID).ToList();//获得所有外聘人员

            if (Fees == null || Fees.Count == 0) return;
            List<TempFee> tempFees = new List<TempFee>();
            //计算每个外聘人员的税
            foreach (BX_InviteFeeView Fee in Fees)
            {
                Tax = 0; cReal = 0;
                //判断外聘人员是否算税(在Person里并且是在职) 
                if (!JugdePersonIn(context, Fee.InvitePersonIDCard))
                {
                    cReal = Fee.Total_Real == null ? 0 : Fee.Total_Real;
                    string sql = GetInviteSql(Fee.GUID_InvitePerson, HxDateTime.Month, HxDateTime.Year, Fee.GUID);
                    TaxMoney tm = context.ExecuteStoreQuery<TaxMoney>(sql).FirstOrDefault();
                    if (tm != null)
                    {
                        if (invitePersonRealDic.ContainsKey(Fee.GUID_InvitePerson))
                        {
                            invitePersonRealDic[Fee.GUID_InvitePerson] += cReal;
                            cReal = invitePersonRealDic[Fee.GUID_InvitePerson];
                        }
                        else
                        {
                            invitePersonRealDic.Add(Fee.GUID_InvitePerson, cReal);
                        }
                        cReal = cReal + tm.SumTotalReal;
                    }

                    Tax = CalcTaxWithBehind(cReal);
                    if (tm != null)
                    {
                        Tax = Tax - tm.SumTotalTax;
                        if (invitePersonTaxDic.ContainsKey(Fee.GUID_InvitePerson))
                        {
                            Tax -= invitePersonTaxDic[Fee.GUID_InvitePerson];
                            invitePersonTaxDic[Fee.GUID_InvitePerson] += Tax;
                        }
                        else
                        {
                            invitePersonTaxDic.Add(Fee.GUID_InvitePerson, Tax);
                        }
                    }
                    Tax = Math.Round(Tax, 2);
                }
                var tempf = new TempFee();
                tempf.Guid = Fee.GUID;
                tempf.Total_Tax = Tax;
                tempf.Total_BX = Tax + Fee.Total_Real;
                tempFees.Add(tempf);
                TotalTax += Tax;
                TotalBx = Tax + TotalBx + Fee.Total_Real;
            }
            InviteObject.Details[0].Total_Tax = TotalTax;
            if (TotalTax != 0)
            {
                InviteObject.Details[0].Total_XX = TotalBx;
            }
            //加载需要更新的单据
            this.needUpdates.Add(InviteObject, tempFees);
        }
        /// <summary>
        /// 判断外聘人员是否算税(在Person里并且是在职)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="IDCard"></param>
        /// <returns></returns>
        private bool JugdePersonIn(ObjectContext context, string IDCard)
        {
            var ent = context.CreateObjectSet<BusinessModel.SS_PersonView>().FirstOrDefault(e => e.IDCard == IDCard&&e.IsTax==false);
            return ent == null ? false : true;//不存在是 true 存在为 false
        }
        /// <summary>
        /// 获得查询语句
        /// </summary>
        /// <param name="invitePersonGuid"></param>
        /// <param name="curMonth"></param>
        /// <param name="curYear"></param>
        /// <param name="inviteFeeGuid"></param>
        /// <returns></returns>
        public virtual string GetInviteSql(Guid invitePersonGuid, int curMonth, int curYear, Guid inviteFeeGuid)
        {
            string sqlFormat = "select isnull(sum(Total_Real),0) as SumTotalReal,isnull(sum(Total_Tax),0) as SumTotalTax,isnull(sum(Total_Bx),0) as SumTotalBx from BX_InviteFee" +
                    " where guid_bx_main in(select guid from bx_main" +
                    " where guid in(select guid_main from hx_detail" +
                    " where guid_hx_main in(select guid from hx_main" +
                    " where month(docdate)='{0}' and year(docdate)='{1}')) ) and guid_inviteperson='{2}' and guid<>'{3}'";
            return string.Format(sqlFormat, curMonth, curYear, invitePersonGuid, inviteFeeGuid);
        }
        /// <summary>
        /// 算税额
        /// </summary>
        /// <param name="iMoney"></param>
        /// <returns></returns>
        private double CalcTaxWithBehind(double iMoney)
        {
            double iTax, iM;//dTax税后收入 dm应纳税所得额
            if (iMoney <= 3360)
            {
                iM = (iMoney - 800) / 0.8;
                iTax = iM * 0.2;
            }
            else if (iMoney > 3360 && iMoney <= 21000)
            {
                iM = iMoney * (1 - 0.2) / (1 - 0.2 * (1 - 0.2));
                iTax = iM * 0.2;
            }
            else if (iMoney > 21000 && iMoney <= 49500)
            {
                iM = ((iMoney - 2000) * (1 - 0.2)) / (1 - 0.3 * (1 - 0.2));
                iTax = iM * 0.3 - 2000;
            }
            else
            {//49500
                iM = ((iMoney - 7000) * (1 - 0.2)) / (1 - 0.4 * (1 - 0.2));
                iTax = iM * 0.4 - 7000;

            }
            if (iTax < 0) return 0.00;
            return Math.Round(iTax, 2);
        }
    }
    public class TaxMoney
    {
        public double SumTotalReal { get; set; }
        public double SumTotalTax { get; set; }
        public double SumTotalBx { get; set; }
        public Guid guid_bx_main { get; set; }
    }
}