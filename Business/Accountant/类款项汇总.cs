using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using System.Data;
using Business.CommonModule;
using System.IO;
using Infrastructure;

namespace Business.Accountant
{
   
    public class 类款项汇总 : BaseDocument
    {
        /*
            select  pad.guid_item,si.itemname,si.itemtype,
            pn.paymentnumber,pn.isproject,pn.financecode, pn.economyclassname,pn.economyclasskey,
            pn.expendtypekey,pn.expendtypename,pn.bgsourcekey,pn.bgsourcename,   
            sum(cast(round(pad.itemvalue,2) as decimal(18,2))) as itemvalue,max(itemdatetime) as itemdatetime,max(itemstring) as itemstring  
            from sa_planactiondetail pad  
            join sa_planaction pa on pad.guid_planaction=pa.guid  and pa.actionyear='2014' and pa.actionmouth='11'
            join sa_planactionpaymentnumber pap on pap.guid_planactiondetail =pad.guid  
            join sa_item si on si.guid=pad.guid_item    
            join cn_paymentnumberview pn on pn.guid=pap.guid_paymentnumber  
            where pad.guid_item in (
			            select guid_data from ss_dataauthset where classid=50 
			            and guid_roleoroperator in ('{1F0CC434-B671-41C2-8139-509E64E76A73}',
						                '{36D41490-7083-4065-8136-763675685464}',
						                '{13128149-C30C-493C-9523-3EB2D53E973F}',
						                '{A22AFD12-D902-4D59-90FD-4628626B37C6}',
						                '{EEEED012-B056-8742-A7F6-B460FCE8A09F}',
						                '{E10E7E0E-E59D-4738-A85D-65B1C00D1A18}')
		        
			            )  
            and pad.guid_person in (
			            select guid_data from ss_dataauthset where classid=3 and guid_roleoroperator in ('{1F0CC434-B671-41C2-8139-509E64E76A73}','{36D41490-7083-4065-8136-763675685464}','{13128149-C30C-493C-9523-3EB2D53E973F}','{A22AFD12-D902-4D59-90FD-4628626B37C6}','{EEEED012-B056-8742-A7F6-B460FCE8A09F}','{E10E7E0E-E59D-4738-A85D-65B1C00D1A18}')
			
			            )     
            group by pad.guid_item,si.itemname,si.itemtype,pn.paymentnumber,  pn.isproject,pn.financecode,pn.economyclassname,pn.economyclasskey,pn.expendtypekey,pn.expendtypename,pn.bgsourcekey,   pn.bgsourcename 

            order by pn.paymentnumber,pn.isproject,pn.financecode,  pn.economyclassname,pn.economyclasskey,pn.expendtypekey,pn.expendtypename,pn.bgsourcekey,pn.bgsourcename 

         */
        /*
        select * from SS_RoleOperator 
        join SS_Role on SS_RoleOperator.GUID_Operator='{1F0CC434-B671-41C2-8139-509E64E76A73}' 
        and SS_RoleOperator.GUID_Role=SS_Role.GUID
         */
        public 类款项汇总() : base() { }
        public 类款项汇总(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 类款项汇总
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<lkxhzModel> HZ_List(SearchCondition conditions, out string message)
        {
            List<lkxhzModel> returnList = new List<lkxhzModel>();
            //工资列表项
            var siList = this.InfrastructureContext.SA_Item.OrderBy(e => e.ItemKey).ToList();
            //表头信息
            lkxhzModel returnDetailModel = new lkxhzModel();
            returnDetailModel.FieldList = CreateFieldList(siList);
            
            List<Dictionary<string, string>> detailDataList = new List<Dictionary<string, string>>();
            message = string.Empty;
            lkxhzCondition condtionModel = (lkxhzCondition)conditions;
            int year=0;
            int.TryParse(condtionModel.Year.ToString(),out year);
            int month=0;
            int.TryParse(condtionModel.Month.ToString(),out month);
            //列表项权限
            var itemDataset = IntrastructureCommonFun.GetDataSet(this.InfrastructureContext, "50", this.OperatorId.ToString()).ToList();
            //人员权限
             var personDataset = IntrastructureCommonFun.GetDataSet(this.InfrastructureContext, "3", this.OperatorId.ToString()).ToList();
            
             List<lkxhzDetailModel> detailList = new List<lkxhzDetailModel>();
            if (conditions != null)
            {                
                detailList = (from pad in this.BusinessContext.SA_PlanActionDetail
                              join pa in this.BusinessContext.SA_PlanAction on pad.GUID_PlanAction equals pa.GUID
                              join pap in this.BusinessContext.SA_PlanActionPaymentnumber on pad.GUID equals pap.GUID_PlanActionDetail
                              //join si in siList on pad.GUID_Item equals si.GUID
                              join pn in this.BusinessContext.CN_PaymentNumberView on pap.GUID_Paymentnumber equals pn.GUID
                              where pa.ActionYear == year && pa.ActionMouth == month
                               //&& itemDataset.Contains(pad.GUID_Item) && personDataset.Contains(pad.GUID_Person)
                              group pad by new
                              {
                                  pad.GUID_Item,
                                  //si.ItemName,
                                  //si.ItemKey,
                                  //si.ItemType,
                                  pn.PaymentNumber,
                                  pn.IsProject,
                                  pn.FinanceCode,
                                  pn.EconomyClassName,
                                  pn.EconomyClassKey,
                                  pn.ExpendTypeKey,
                                  pn.ExpendTypeName,
                                  pn.BGSourceKey,
                                  pn.BGSourceName
                              } into temp                             
                              select new lkxhzDetailModel
                              {
                                  GUID_Item = temp.Key.GUID_Item,
                                  //ItemName = temp.Key.ItemName,
                                  //ItemKey = temp.Key.ItemKey,
                                  //ItemType = temp.Key.ItemType,
                                  PaymentNumber = temp.Key.PaymentNumber,
                                  IsProject = temp.Key.IsProject == true ? "是" : "否",
                                  FinanceCode = temp.Key.FinanceCode,
                                  EconomyClassName = temp.Key.EconomyClassName,
                                  EconomyClassKey = temp.Key.EconomyClassKey,
                                  ExpendTypeKey = temp.Key.ExpendTypeKey,
                                  ExpendTypeName = temp.Key.ExpendTypeName,
                                  BGSourceKey = temp.Key.BGSourceKey,
                                  BGSourceName = temp.Key.BGSourceName,
                                  ItemValue = temp.Sum(e => e.ItemValue),
                                  ItemDateTime = temp.Max(e => e.ItemDatetime), //((DateTime)temp.Max(e => e.ItemDatetime)).ToString("yyyy-MM-dd"),
                                  ItemString = temp.Max(e => e.ItemString+"")

                              }).OrderBy(e => new { e.PaymentNumber,e.IsProject,e.FinanceCode,e.EconomyClassName,e.EconomyClassKey,
                                        e.ExpendTypeKey,e.ExpendTypeName,e.BGSourceKey,e.BGSourceName
                              }).ToList();
                
            }
            //支付码信息
            var paymentList = detailList.Select(e => new { e.PaymentNumber,e.IsProject,e.FinanceCode,e.EconomyClassKey,e.ExpendTypeKey,
                                                         e.BGSourceKey
                                                    }).Distinct().ToList();
            //组织数据
            for (int i = 0; i < paymentList.Count; i++)
            { 
                var row=paymentList[i];
                bool flag=true;
                Dictionary<string,string> dic=new Dictionary<string,string> ();
                dic["PaymentNumber"]=row.PaymentNumber;
                dic["IsProject"]=row.IsProject;
                dic["FinanceCode"]=row.FinanceCode;
                dic["EconomyClassKey"]=row.EconomyClassKey;
                dic["ExpendTypeKey"] = row.ExpendTypeKey;
                dic["BGSourceKey"] = row.BGSourceKey;
                var itemList=detailList.FindAll(e=>e.PaymentNumber==row.PaymentNumber && e.IsProject==row.IsProject && e.FinanceCode==row.FinanceCode && e.EconomyClassKey==row.EconomyClassKey && e.ExpendTypeKey==row.ExpendTypeKey && e.BGSourceKey==row.BGSourceKey);
               foreach(lkxhzDetailModel item in itemList)
               {
                var itemModel = siList.Find(e=>e.GUID==item.GUID_Item);
                if (itemModel != null)
                {
                    //dic["GUID_Item"]=item.GUID_Item.ToString();
                    dic["ItemName" + itemModel.ItemKey] =item.ItemValue==null?"":item.ItemValue.ToString();
                    //dic["ItemType"]=item.ItemType.ToString();
                    dic["ItemValue"] = itemModel.ItemKey;
                }
                if(flag==true)
                {
                    dic["ItemDateTime"]=item.ItemDateTime.ToString();
                    dic["ItemString"]=item.ItemString;
                    dic["EconomyClassName"] = item.EconomyClassName;
                    dic["ExpendTypeName"] = item.ExpendTypeName;
                    dic["BGSourceName"] = item.BGSourceName;
                    flag=false;
                }
               }
                detailDataList.Add(dic);
            }
            returnDetailModel.objData = detailDataList;
            returnList.Add(returnDetailModel);
            return returnList;
        }
        /// <summary>
        /// 创建列表头
        /// </summary>
        /// <returns></returns>
        public List<FieldModel> CreateFieldList(List<SA_Item> salist)
        {
            //表头列表信息
            List<FieldModel> fieldList = new List<FieldModel>();
            fieldList.Add(new FieldModel("PaymentNumber", "财政支付令"));
            fieldList.Add(new FieldModel("IsProject", "是否项目"));
            fieldList.Add(new FieldModel("FinanceCode", "政府收支分类"));
            fieldList.Add(new FieldModel("EconomyClassName", "经济分类"));
            fieldList.Add(new FieldModel("ExpendTypeKey", "支出类型"));
            fieldList.Add(new FieldModel("BGSourceName", "预算来源"));

            //工资列表项            
            foreach (SA_Item item in salist)
            {
                fieldList.Add(new FieldModel("ItemName"+item.ItemKey, item.ItemName));
            }
            return fieldList;
        }
        /// <summary>
        /// 返回数据
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<lkxhzModel> Retrieve(Guid guid, out string message)
        {
            List<lkxhzModel> returnList = new List<lkxhzModel>();
            //工资列表项
            var siList = this.InfrastructureContext.SA_Item.OrderBy(e => e.ItemKey).ToList();
            //表头信息
            lkxhzModel returnDetailModel = new lkxhzModel();
            returnDetailModel.FieldList = CreateFieldList(siList);

            List<Dictionary<string, string>> detailDataList = new List<Dictionary<string, string>>();
            message = string.Empty; 
            List<lkxhzDetailModel> detailList = new List<lkxhzDetailModel>();
          
                detailList = (from pad in this.BusinessContext.SA_PlanActionDetail
                              join pa in this.BusinessContext.SA_PlanAction on pad.GUID_PlanAction equals pa.GUID
                              join pap in this.BusinessContext.SA_PlanActionPaymentnumber on pad.GUID equals pap.GUID_PlanActionDetail
                              //join si in siList on pad.GUID_Item equals si.GUID
                              join pn in this.BusinessContext.CN_PaymentNumberView on pap.GUID_Paymentnumber equals pn.GUID
                              where pa.GUID==guid                              
                              group pad by new
                              {
                                  pad.GUID_Item,
                                  //si.ItemName,
                                  //si.ItemKey,
                                  //si.ItemType,
                                  pn.PaymentNumber,
                                  pn.IsProject,
                                  pn.FinanceCode,
                                  pn.EconomyClassName,
                                  pn.EconomyClassKey,
                                  pn.ExpendTypeKey,
                                  pn.ExpendTypeName,
                                  pn.BGSourceKey,
                                  pn.BGSourceName
                              } into temp
                              select new lkxhzDetailModel
                              {
                                  GUID_Item = temp.Key.GUID_Item,
                                  //ItemName = temp.Key.ItemName,
                                  //ItemKey = temp.Key.ItemKey,
                                  //ItemType = temp.Key.ItemType,
                                  PaymentNumber = temp.Key.PaymentNumber,
                                  IsProject = temp.Key.IsProject == true ? "是" : "否",
                                  FinanceCode = temp.Key.FinanceCode,
                                  EconomyClassName = temp.Key.EconomyClassName,
                                  EconomyClassKey = temp.Key.EconomyClassKey,
                                  ExpendTypeKey = temp.Key.ExpendTypeKey,
                                  ExpendTypeName = temp.Key.ExpendTypeName,
                                  BGSourceKey = temp.Key.BGSourceKey,
                                  BGSourceName = temp.Key.BGSourceName,
                                  ItemValue = temp.Sum(e => e.ItemValue),
                                  ItemDateTime = temp.Max(e => e.ItemDatetime), //((DateTime)temp.Max(e => e.ItemDatetime)).ToString("yyyy-MM-dd"),
                                  ItemString = temp.Max(e => e.ItemString + "")

                              }).OrderBy(e => new
                              {
                                  e.PaymentNumber,
                                  e.IsProject,
                                  e.FinanceCode,
                                  e.EconomyClassName,
                                  e.EconomyClassKey,
                                  e.ExpendTypeKey,
                                  e.ExpendTypeName,
                                  e.BGSourceKey,
                                  e.BGSourceName
                              }).ToList();

            
            //支付码信息
            var paymentList = detailList.Select(e => new
            {
                e.PaymentNumber,
                e.IsProject,
                e.FinanceCode,
                e.EconomyClassKey,
                e.ExpendTypeKey,
                e.BGSourceKey
            }).Distinct().ToList();
            //组织数据
            for (int i = 0; i < paymentList.Count; i++)
            {
                var row = paymentList[i];
                bool flag = true;
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["PaymentNumber"] = row.PaymentNumber;
                dic["IsProject"] = row.IsProject;
                dic["FinanceCode"] = row.FinanceCode;
                dic["EconomyClassKey"] = row.EconomyClassKey;
                dic["ExpendTypeKey"] = row.ExpendTypeKey;
                dic["BGSourceKey"] = row.BGSourceKey;
                var itemList = detailList.FindAll(e => e.PaymentNumber == row.PaymentNumber && e.IsProject == row.IsProject && e.FinanceCode == row.FinanceCode && e.EconomyClassKey == row.EconomyClassKey && e.ExpendTypeKey == row.ExpendTypeKey && e.BGSourceKey == row.BGSourceKey);
                foreach (lkxhzDetailModel item in itemList)
                {
                    var itemModel = siList.Find(e => e.GUID == item.GUID_Item);
                    if (itemModel != null)
                    {
                        //dic["GUID_Item"]=item.GUID_Item.ToString();
                        dic["ItemName" + itemModel.ItemKey] = item.ItemValue == null ? "" : item.ItemValue.ToString();
                        //dic["ItemType"]=item.ItemType.ToString();
                        dic["ItemValue"] = itemModel.ItemKey;
                    }
                    if (flag == true)
                    {
                        dic["ItemDateTime"] = item.ItemDateTime.ToString();
                        dic["ItemString"] = item.ItemString;
                        dic["EconomyClassName"] = item.EconomyClassName;
                        dic["ExpendTypeName"] = item.ExpendTypeName;
                        dic["BGSourceName"] = item.BGSourceName;
                        flag = false;
                    }
                }
                detailDataList.Add(dic);
            }
            returnDetailModel.objData = detailDataList;
            returnList.Add(returnDetailModel);
            return returnList;
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            //select * from SA_PlanActionView where PlanName like '%%' and ActionYear = '2014' and ActionMouth ='11'  order by docnum desc
            lkxhzHistoryCondition conditionModel = (lkxhzHistoryCondition)conditions;            
            var q = this.BusinessContext.SA_PlanActionView.AsQueryable();
            if (conditionModel != null)
            {
                if (!conditionModel.GUID_Plan.IsNullOrEmpty())
                {
                    q = q.Where(e=>e.GUID_Plan==conditionModel.GUID_Plan);
                }
                int year = 0;
                int.TryParse(conditionModel.Year,out year);
                if (year != 0)
                {
                    q = q.Where(e=>e.ActionYear==year);
                }
                int month = 0;
                int.TryParse(conditionModel.Month,out month);
                if (month != 0)
                {
                    q = q.Where(e=>e.ActionMouth==month);
                }
                if (!string.IsNullOrEmpty(conditionModel.PayOutState))
                {
                    int payoutState = 0;
                    int.TryParse(conditionModel.PayOutState,out payoutState);
                    q = q.Where(e => e.ActionState == payoutState);
                }
            }
            var objlist = q.AsEnumerable().Select(e => new {e.GUID,e.DocNum,e.PlanName,e.ActionYear,e.ActionMouth,DocDate=((DateTime)e.DocDate).ToString("yyyy-MM-dd"),e.ActionPeriod,e.ActionTimes,e.Descrip}).ToList<object>();
            return objlist;
        }
        /// <summary>
        /// 导出报表
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fileName"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public string GetExportPath(DataTable data, string timeDate, out string fileName, out string message)
        {
            var templateName = "lkxhz.xls";
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
                cell.Value = timeDate + "类款项汇总";
                excelCellList.Add(cell);


                var templatePath = Path.Combine(ComBaseReport.template, templateName);
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
