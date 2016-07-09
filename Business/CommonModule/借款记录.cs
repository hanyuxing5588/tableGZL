using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using System.Data.Objects;
using BusinessModel;
namespace Business.CommonModule
{
    public class 借款记录 : BaseDocument
    {
        public 借款记录() : base() { }
        public 借款记录(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 借款统计信息
        /// </summary>
        /// <param name="conditions">条件</param>
        /// <returns> object List< /returns>
        public override List<BorrowModel> BorrowMoney(SearchCondition conditions)
        {
            string dwguid = string.Empty;//单位
            string depid=string.Empty;//部门
            string personid = string.Empty;//人员
            string startdate = string.Empty;//开始时间

            string enddate = string.Empty;//结束时间
            List<BorrowModel> list = new List<BorrowModel>();
            BorrowMoneyCondition borrowCondition = (BorrowMoneyCondition)conditions;

            int wl_detail_classid = Infrastructure.CommonFuntion.GetClassId(typeof(WL_Detail).Name);
            int cn_main_classid = Infrastructure.CommonFuntion.GetClassId(typeof(HX_Main).Name);
            int cn_detail_classid = Infrastructure.CommonFuntion.GetClassId(typeof(HX_Detail).Name);

            #region 条件
           
                if (borrowCondition.StartDate != null && borrowCondition.StartDate != DateTime.MinValue)
                {
                    //wl_main = wl_main.Where(e => e.DocDate >= borrowCondition.StartDate);
                    startdate = borrowCondition.StartDate.ToString("yyyy-MM-dd");
                    if (borrowCondition.EndDate != null && borrowCondition.EndDate != DateTime.MinValue)
                    {
                        //wl_main = wl_main.Where(e => e.DocDate <= borrowCondition.EndDate);
                        enddate = borrowCondition.EndDate.ToString("yyyy-MM-dd");
                        if (!string.IsNullOrEmpty(borrowCondition.treeModel) && (borrowCondition.treeValue != null && borrowCondition.treeValue != Guid.Empty))
                        {
                            switch (borrowCondition.treeModel.ToLower())
                            {
                                case "ss_department":
                                    List<SS_Department> depList = new List<SS_Department>();
                                    SS_Department dep = new SS_Department();
                                    dep.GUID = borrowCondition.treeValue;
                                    dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                                    // var depguid = depList.Select(e => e.GUID);
                                    var depguid = depList.Select(e => e.GUID).ToList();
                                    depid = depguid.SQLStrGUID(",");
                                    break;
                                case "ss_dw":
                                    List<SS_DW> dwList = new List<SS_DW>();
                                    SS_DW dw = new SS_DW();

                                    if (!borrowCondition.treeValue.IsNullOrEmpty())
                                    {
                                        dw.GUID = borrowCondition.treeValue;
                                    }
                                    dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                                    var dwguidList = dwList.Select(e => e.GUID).ToList();
                                    dwguid = dwguidList.SQLStrGUID(",");
                                    break;
                                case "ss_person":
                                    if (!borrowCondition.treeValue.IsNullOrEmpty())
                                    {
                                        personid = borrowCondition.treeValue.ToString();
                                    }
                                    break;
                            }
                        }
                        //查询借款核销明细

                    }
                }
            #endregion
            //ObjectParameter[] parameters ={
            //                        new ObjectParameter("dwguid",dwguid),
            //                        new ObjectParameter("depid",depid),
            //                        new ObjectParameter("personid",personid),
            //                        new ObjectParameter("startdate",startdate),
            //                        new ObjectParameter("enddate",enddate)
            //                        };
            //list = this.BusinessContext.ExecuteFunction<BorrowModel>("GetBorrowData", parameters).ToList<object>();
            list= this.BusinessContext.GetBorrowData(dwguid, depid, personid,startdate, enddate).ToList<BorrowModel>();
            return list;
        }
    }
}
