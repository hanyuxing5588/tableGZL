using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using System.IO;
using System.Data;

namespace Business.Accountant
{
    /*
     --劳务费个税月度汇总
    select distinct InviteFee.InvitePersonIDCard,InviteFee.InvitePersonName,a.Total_BX,a.Total_Tax,a.Total_Real 
    from bx_InviteFeeView InviteFee 
    left join 
    (
    select GUID_InvitePerson,sum(Total_BX) as Total_BX,sum(Total_Tax) as Total_Tax,sum(Total_Real) as Total_Real from bx_InviteFeeView  
    left join bx_main main on main.guid=bx_InviteFeeView.GUID_BX_Main where main.guid in (
							    select GUID_Main from hx_detail 
							    left join hx_main on hx_detail.guid_hx_main=hx_main.guid 
							    where DatePart(Year,hx_main.DocDate)='2013' and DatePart(Month,hx_main.DocDate)='6') 
    and InvitePersonIDCard not in (select isnull(IDCard,'''') from ss_person) 
    group by GUID_InvitePerson
    ) a on InviteFee.GUID_InvitePerson=a.GUID_InvitePerson 
    left join bx_main main on main.guid=InviteFee.GUID_BX_Main where main.guid in (
							    select GUID_Main from hx_detail 
							    left join hx_main on hx_detail.guid_hx_main=hx_main.guid 
							    where DatePart(Year,hx_main.DocDate)='2013' and DatePart(Month,hx_main.DocDate)='6'
							    ) 
    and InvitePersonIDCard not in (select isnull(IDCard,'''') from ss_person) 
    and InviteFee.InvitePersonName like '%%'
  */
    public class 劳务费个税月度汇总 : BaseDocument
    {

        public 劳务费个税月度汇总() : base() { }
        public 劳务费个税月度汇总(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 劳务费个税月度汇总
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<lwfgsydhzModel> HZ_List(SearchCondition conditions,out string message)
        {
            message = string.Empty;
            lwfgsydhzCondition condtionModel = (lwfgsydhzCondition)conditions;
            if (conditions != null)
            {
                IQueryable<Guid> guid_bx_mainByhxDate = null;
                if (condtionModel.Month == 0)
                {
                    guid_bx_mainByhxDate = from d in this.BusinessContext.HX_Detail
                                           join m in this.BusinessContext.HX_Main on d.GUID_HX_Main equals m.GUID
                                           where m.DocDate.Year == condtionModel.Year 
                                           select d.GUID_Main;
                }
                else {
                    guid_bx_mainByhxDate = from d in this.BusinessContext.HX_Detail
                                           join m in this.BusinessContext.HX_Main on d.GUID_HX_Main equals m.GUID
                                           where m.DocDate.Year == condtionModel.Year && m.DocDate.Month == condtionModel.Month
                                           select d.GUID_Main;
                }
                //根据核销日期查询报销GUID
                /*
                 select GUID_InvitePerson,sum(Total_BX) as Total_BX,sum(Total_Tax) as Total_Tax,sum(Total_Real) as Total_Real from bx_InviteFeeView  
               left join bx_main main on main.guid=bx_InviteFeeView.GUID_BX_Main where main.guid in (
                                           select GUID_Main from hx_detail 
                                           left join hx_main on hx_detail.guid_hx_main=hx_main.guid 
                                           where DatePart(Year,hx_main.DocDate)='2013' and DatePart(Month,hx_main.DocDate)='6') 
               and InvitePersonIDCard not in (select isnull(IDCard,'''') from ss_person) 
               group by GUID_InvitePerson
                 */
                //人员IdCards
                var personIDCards = this.InfrastructureContext.SS_Person.Select(e => e.IDCard).ToList();
                //统计金额
                var feeTotalList = from fee in this.BusinessContext.BX_InviteFeeView
                                   join m in this.BusinessContext.BX_Main on fee.GUID_BX_Main equals m.GUID 
                                   //from m in mTemp.DefaultIfEmpty()
                                   where guid_bx_mainByhxDate.Contains(m.GUID) && !personIDCards.Contains(fee.InvitePersonIDCard)
                                   group fee by fee.GUID_InvitePerson into temp
                                   select new
                                   {
                                       GUID_InvitePerson = temp.Key,
                                       Total_BX = temp.Sum(e => e.Total_BX),
                                       Total_Tax = temp.Sum(e => e.Total_Tax),
                                       Total_Real = temp.Sum(e => e.Total_Real)
                                   };
                /*
                 select distinct InviteFee.InvitePersonIDCard,InviteFee.InvitePersonName,a.Total_BX,a.Total_Tax,a.Total_Real 
                from bx_InviteFeeView InviteFee 
                left join 
                 */
                //劳务费汇总
                var q = from fee in this.BusinessContext.BX_InviteFeeView
                        join tl in feeTotalList on fee.GUID_InvitePerson equals tl.GUID_InvitePerson
                        join m in this.BusinessContext.BX_Main on fee.GUID_BX_Main equals m.GUID into mTemp
                        from m in mTemp.DefaultIfEmpty()
                        where guid_bx_mainByhxDate.Contains(m.GUID) && !personIDCards.Contains(fee.InvitePersonIDCard)
                        select new lwfgsydhzModel
                        {
                            //GUID=fee.GUID,
                            InvitePersonIDCard=fee.InvitePersonIDCard,
                            InvitePersonName=fee.InvitePersonName,
                            Total_BX=tl.Total_BX,
                            Total_Tax=tl.Total_Tax,
                            Total_Real=tl.Total_Real
                        };
                if (!string.IsNullOrEmpty(condtionModel.InvitePersonName))
                {
                    q = q.Where(e => e.InvitePersonName == condtionModel.InvitePersonName);
                }
                return q.Distinct().OrderBy(e=>e.InvitePersonIDCard).ToList();
            }

            return null;
        }
        /// <summary>
        /// 明细信息
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public List<lwfgsydhzModel> MX_List(SearchCondition conditions, out string message)
        {
            /*
                 select * from bx_InviteFeeView InviteFee 
                left join bx_main main on main.guid=InviteFee.GUID_BX_Main  
                where main.guid in (
	                select GUID_Main from hx_detail 
	                left join hx_main on hx_detail.guid_hx_main=hx_main.guid 
	                where DatePart(Year,hx_main.DocDate)=''2013'' and DatePart(Month,hx_main.DocDate)=''6'') 
	                and InvitePersonIDCard not in (select isnull(IDCard,'''') from ss_person
                ) and invitepersonname like '%%'
             */
            message = string.Empty;
            lwfgsydhzCondition condtionModel = (lwfgsydhzCondition)conditions;
            if (conditions != null)
            {
                //根据核销日期查询报销GUID
                IQueryable<Guid> guid_bx_mainByhxDate = null;
                if (condtionModel.Month == 0)
                {
                    guid_bx_mainByhxDate = from d in this.BusinessContext.HX_Detail
                                           join m in this.BusinessContext.HX_Main on d.GUID_HX_Main equals m.GUID
                                           where m.DocDate.Year == condtionModel.Year
                                           select d.GUID_Main;
                }
                else {
                    guid_bx_mainByhxDate = from d in this.BusinessContext.HX_Detail
                                           join m in this.BusinessContext.HX_Main on d.GUID_HX_Main equals m.GUID
                                           where m.DocDate.Year == condtionModel.Year && m.DocDate.Month == condtionModel.Month
                                           select d.GUID_Main;
                }          
                //人员IdCards
                var personIDCards = this.InfrastructureContext.SS_Person.Select(e => e.IDCard).ToList();
                       
                //劳务费汇总
                var q = from fee in this.BusinessContext.BX_InviteFeeView                        
                        join m in this.BusinessContext.BX_Main on fee.GUID_BX_Main equals m.GUID into mTemp
                        from m in mTemp.DefaultIfEmpty()
                        where guid_bx_mainByhxDate.Contains(m.GUID) && !personIDCards.Contains(fee.InvitePersonIDCard)
                        select new lwfgsydhzModel
                        {
                            DocNum=m.DocNum,
                            InvitePersonIDCard = fee.InvitePersonIDCard,
                            InvitePersonName = fee.InvitePersonName,
                            Total_BX = fee.Total_BX,
                            Total_Tax = fee.Total_Tax,
                            Total_Real = fee.Total_Real
                        };
                if (!string.IsNullOrEmpty(condtionModel.InvitePersonName))
                {
                    q = q.Where(e => e.InvitePersonName.Contains(condtionModel.InvitePersonName));
                }
                return q.Distinct().OrderBy(e => e.InvitePersonIDCard).ToList();
            }

            return null;
        }
        /// <summary>
        /// 导出报表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public  string GetExportPath(DataTable data,string timeDate, out string fileName, out string message)
        {
            var templateName = "dwrylkd.xls";// "lwfgsydhz.xls"; 与单位人员领款单 可以通用一个
            fileName = "";
            message = "";
            try
            {
                if (data != null && data.Rows.Count <= 0)
                {
                    message = "1";
                    return "";
                }
                List<ExcelCell> excelCellList = new List<ExcelCell>();
                //标题
                ExcelCell cell = new ExcelCell();
                cell.Row = 0;
                cell.Col = 0;
                cell.Value = timeDate+"劳务费个税月度汇总";
                excelCellList.Add(cell);


                var templatePath = Path.Combine(ComBaseReport.template,templateName);
                string filePath = ExportExcel.Export(data, templatePath, 1, 0, excelCellList);
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }
        
    }
      
   
}
