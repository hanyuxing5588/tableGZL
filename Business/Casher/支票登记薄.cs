using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using BusinessModel;
using Platform.Flow.Run;
using Business.CommonModule;

namespace Business.Casher
{
   
    public class 支票登记薄 : BaseDocument
    {
        public 支票登记薄() : base() { }
        public 支票登记薄(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            CheckHistoryCondition historyconditions = (CheckHistoryCondition)conditions;
            IQueryable<CN_CheckView> main = this.BusinessContext.CN_CheckView; ;//或者用ModelUrl 02指现金报销单

            if (historyconditions != null)
            {

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_bank":
                            Guid g;
                            Guid.TryParse(historyconditions.treeValue,out g);
                            if (!g.IsNullOrEmpty())
                            {                                
                                main = main.Where(e=>e.GUID_Bank==g);
                            }
                            break;
                        case "ss_bankaccount":
                            Guid guid;
                            Guid.TryParse(historyconditions.treeValue,out guid);
                            if (!guid.IsNullOrEmpty())
                            {
                                main = main.Where(e => e.GUID_BankAccount == guid);
                            }
                            break;
                       
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CheckNumber))
                {
                    main = main.Where(e => e.CheckNumber.Contains(historyconditions.CheckNumber));
                }
                int checktype;                
                if (!string.IsNullOrEmpty(historyconditions.CheckType))
                {
                    if (int.TryParse(historyconditions.CheckType, out checktype))
                    {
                        main = main.Where(e => e.CheckType == checktype);
                    }
                }

            }            
            var o=from c in main
                  join dm in this.BusinessContext.CN_CheckDrawMainView on c.GUID equals dm.GUID_Check into temp
                  from dm in temp.DefaultIfEmpty()
                  select new {
                    c.GUID,
                    c.BankAccountName,
                    c.CheckNumber,
                    c.CheckType,
                    c.IsInvalid,
                    TakeState=dm.GUID==null?"未领取":"已领取",
                    dm.CheckDrawDatetime,
                    dm.PersonName,
                    dm.CheckUsed,
                    dm.CheckMoney
                  };
            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                e.BankAccountName,
                e.CheckNumber,
                CheckType=e.CheckType==0?"现金支票":"转账支票",
                IsInvalid=e.IsInvalid==false?"是":"否",
                e.TakeState,
                CheckDrawDatetime=e.CheckDrawDatetime==null?"":((DateTime)e.CheckDrawDatetime).ToString("yyyy-MM-dd"),
                e.PersonName,
                e.CheckUsed,
                e.CheckMoney
            }).OrderByDescending(e => e.CheckDrawDatetime).OrderByDescending(e => e.CheckNumber).ToList<object>();

            return mainList;

        }
    }
}
