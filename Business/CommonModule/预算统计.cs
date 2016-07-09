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
    public class 预算统计 : BaseDocument
    {
        public 预算统计() : base() { }
        public 预算统计(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        // 01	上年管理费
        // 02	本年管理费
        public string[] arrjc = new string[] { "01", "02" };//基本支出 预算条目Key值
        //07	上年项目直接成本
        //08	本年项目直接成本
        public string[] arrxm = new string[] { "07", "08" };//项目支出 预算条目Key值
        //当年预算
        public string[] currentYearItemKey = new string[] {"02","08" };
        //去年结转
        public string[] LastYearItemKey = new string[] {"01","07" };
        /// <summary>
        /// 年份
        /// </summary>
        public int currentYear = DateTime.Now.Year;
        /// <summary>
        /// 业务Key
        /// </summary>
        public string YWKey = string.Empty;
        /// <summary>
        /// 获取预算执行情况统计数据(待完成)
        /// </summary>
        /// <param name="operatorId"></param>
        /// <returns></returns>
        public override List<BudgetModel> BudgetStatistics(Guid operatorId, Guid docGuid, List<BudgetModel> listBudgetModel, int year, string ywKey)
        {
            if (listBudgetModel == null) return null;
            Guid dwID = Guid.Empty;
            var depId = listBudgetModel.Where(e => e.GUID_Department != null).Select(e => e.GUID_Department).Distinct();
            var proId = listBudgetModel.Where(e => e.GUID_Project != null).Select(e => e.GUID_Project).Distinct();
            var bgcodeId = listBudgetModel.Where(e => e.GUID_BGCode != null).Select(e => e.GUID_BGCode).Distinct();
            var sourceId = listBudgetModel.Where(e => e.GUID_BGResource != null).Select(e => e.GUID_BGResource).Distinct();
            var bgTypeName = listBudgetModel.Where(e => e.BGTypeName != null).Select(e => e.BGTypeName).Distinct().ToList();
            this.currentYear = year;
            this.YWKey = ywKey;
            //根据预算类型 得到预算预算条目 与预算来源 关联
            List<string> bgitemKey = new List<string>();
            foreach(string item in bgTypeName)
            {
                if (item == "基本支出")
                {
                    bgitemKey.AddRange(arrjc.ToList());
                }
                else if (item == "项目支出")
                {  
                    bgitemKey.AddRange(arrjc.ToList());
                }
            }

            //预算信息          
            var list = (from m in this.BusinessContext.BG_MainView
                        join d in this.BusinessContext.BG_DetailView on m.GUID equals d.GUID_BG_Main
                        where bgitemKey.Contains(d.BGItemKey) && d.BGYear == year                        
                        select new BudgetModel
                        {
                            GUID_Department =m.GUID_Department,
                            DepartmentName = m.DepartmentName,
                            GUID_DW = m.GUID_DW,
                            DWKey = m.DWKey,
                            DWName = m.DWName,
                            GUID_BGCode = d.GUID_BGCode,
                            BGCodeKey = d.BGCodeKey,
                            BGCodeName =d.BGCodeName,
                            GUID_Project = m.GUID_Project,
                            ProjectKey = m.ProjectKey,
                            ProjectName = m.ProjectName,
                            TotalPlan =m.Total_BG,
                            BGItemKey=d.BGItemKey
                        }).Where(e => depId.Contains(e.GUID_Department) && proId.Contains(e.GUID_Project) && bgcodeId.Contains(e.GUID_BGCode)).ToList();

           //将界面传过来的列表分组
           var bmListByUI = DetailGroup(listBudgetModel);
           //合并处理
            MergeList(bmListByUI, list, docGuid);
            return bmListByUI;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bgmDBList"></param>
        /// <param name="bgTypeName"></param>
        /// <param name="bgResourceName"></param>
        public void GetBGTotalPlanByBGTypeAndBGResource( ref List<BudgetModel> bgmDBList, string bgTypeName, string bgResourceName) 
        {
            if (bgmDBList != null && bgmDBList.Count > 0)
                {
                    if (bgTypeName == "基本支出")
                    {
                        bgmDBList = bgmDBList.FindAll(e => arrjc.ToList().Contains(e.BGItemKey));
                    }
                    else
                    {
                        bgmDBList = bgmDBList.FindAll(e => arrxm.ToList().Contains(e.BGItemKey));
                    }
                    if (bgResourceName == "当年预算")
                    {
                        bgmDBList = bgmDBList.FindAll(e => currentYearItemKey.ToList().Contains(e.BGItemKey));
                    }
                    else
                    {
                        bgmDBList = bgmDBList.FindAll(e =>LastYearItemKey.ToList().Contains(e.BGItemKey));
                    }
                }

        }
        /// <summary>
        /// 合并数据列表数据
        /// </summary>
        /// <param name="dList"></param>
        /// <param name="bgmList"></param>
        /// <returns></returns>
        private void MergeList(List<BudgetModel> bmListByUI, List<BudgetModel> bmListByDB,Guid docGUID)
        {
            if (bmListByUI.Count <= 0) return ;
            //合并   
            foreach (BudgetModel item in bmListByUI)
            {  
                var bgmDB = bmListByDB.FindAll(e => e.GUID_BGCode == item.GUID_BGCode && e.GUID_Project == item.GUID_Project && e.GUID_Department == e.GUID_Department); //&& e.GUID_BGResource==e.GUID_BGResource 
                if (bgmDB != null && bgmDB.Count > 0)
                {
                    GetBGTotalPlanByBGTypeAndBGResource(ref bgmDB, item.BGTypeName, item.BGSourceName);
                }
                //累计发生 数据库中 该项目 该部门 该科目 不是此单据的所有值的和 不包含 本单据的该项目 该部门 该科目的本次发生值
                item.AddUpTotal = GetAddUpTotalByYWKey(item.GUID_Project, item.GUID_Department, item.GUID_BGCode, docGUID, item.GUID_BGResource);
                if (bgmDB != null)
                {
                    item.TotalPlan = bgmDB.Sum(e=>e.TotalPlan);
                    item.BalanceTotal = (double)item.TotalPlan - item.AddUpTotal-item.ThisTimeTotal;
                    item.CompletionRate = (item.TotalPlan == null || item.TotalPlan == 0F) ? 0 + "%" : ((item.AddUpTotal+item.ThisTimeTotal) / (double)item.TotalPlan * 100).ToString("0.00") + "%";//（累计发生+本次发生）/预算金额*100%
                }
                else
                {                      
                    item.BalanceTotal =0- item.AddUpTotal - item.ThisTimeTotal;
                    item.CompletionRate ="0%";//（累计发生+本次发生）/预算金额*100%                   
                }                        
            }           
        }
        /// <summary>
        /// 明细分组
        /// </summary>
        /// <param name="dList"></param>
        /// <returns></returns>
        private List<BudgetModel> DetailGroup(List<BudgetModel> dList)
        {
             return (from b in dList
                        //where b.GUID == null
                     group b by new { b.GUID_BGCode, b.BGCodeKey, b.BGCodeName, b.GUID_Project, b.ProjectKey, b.ProjectName, b.GUID_Department, b.DepartmentName, b.GUID_BGResource, b.BGSourceName } into temp
                        select new BudgetModel
                        {
                            GUID_BGCode = temp.Key.GUID_BGCode,
                            BGCodeKey=temp.Key.BGCodeKey,
                            BGCodeName = temp.Key.BGCodeName,
                            GUID_Project = temp.Key.GUID_Project,
                            ProjectKey = temp.Key.ProjectKey,
                            ProjectName = temp.Key.ProjectName,
                            GUID_Department = temp.Key.GUID_Department,
                            DepartmentName = temp.Key.DepartmentName,
                            ThisTimeTotal =temp.Sum(e => e.ThisTimeTotal),
                            TotalPlan =0,
                            GUID_BGResource=temp.Key.GUID_BGResource,
                            BGSourceName = temp.Key.BGSourceName
                        }).ToList();
        }
        /// <summary>
        /// 报销累计金额
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="departmentId"></param>
        /// <param name="bgcodeId"></param>
        /// <returns></returns>
        private double BX_AddUpTotal(Guid? projectId, Guid? departmentId, Guid? bgcodeId, Guid docGuid, Guid? bgresourceId)
        {
            var q = from m in this.BusinessContext.BX_MainView
                    join d in this.BusinessContext.BX_DetailView on m.GUID equals d.GUID_BX_Main
                    join p in this.BusinessContext.CN_PaymentNumberView on d.GUID_PaymentNumber equals p.GUID
                    where d.GUID_BGCode == bgcodeId && d.GUID_Project == projectId && d.GUID_Department == departmentId && p.GUID_BGResource == bgresourceId
                          && m.GUID != docGuid && m.DocDate.Year == this.currentYear
                    select new { d.GUID_BGCode, d.GUID_Project, d.GUID_Department, d.Total_Real, p.GUID_BGResource };
            var sumValue = (from s in q
                            group s by new { s.GUID_BGCode, s.GUID_Project, s.GUID_Department, s.GUID_BGResource } into temp
                            select new { total = temp.Sum(e => e.Total_Real) }).ToList();
            if (sumValue.Count > 0)
            {
                double d = 0F;
                double.TryParse(sumValue[0].total.ToString(), out d);
                return d;
            }
            return 0F;
        }
        /// <summary>
        /// 往来累计金额
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="departmentId"></param>
        /// <param name="bgcodeId"></param>
        /// <returns></returns>
        private double WL_AddUpTotal(Guid? projectId, Guid? departmentId, Guid? bgcodeId, Guid docGuid, Guid? bgresourceId)
        {
            var q = from m in this.BusinessContext.WL_MainView
                    join d in this.BusinessContext.WL_DetailView on m.GUID equals d.GUID_WL_Main
                    join p in this.BusinessContext.CN_PaymentNumberView on d.GUID_PaymentNumber equals p.GUID
                    where d.GUID_BGCode == bgcodeId && d.GUID_ProjectKey == projectId && d.GUID_Department == departmentId && p.GUID_BGResource == bgresourceId
                          && m.GUID != docGuid && m.DocDate.Year == this.currentYear
                    select new { d.GUID_BGCode, d.GUID_ProjectKey, d.GUID_Department, d.Total_WL, p.GUID_BGResource };
            var sumValue = (from s in q
                            group s by new { s.GUID_BGCode, s.GUID_ProjectKey, s.GUID_Department, s.GUID_BGResource } into temp
                            select new { total = temp.Sum(e => e.Total_WL) }).ToList();
            if (sumValue.Count > 0)
            {
                double d = 0F;
                double.TryParse(sumValue[0].total.ToString(), out d);
                return d;
            }
            return 0F;
        }
        /// <summary>
        /// 根据业务Key得到累加值
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="departmentId"></param>
        /// <param name="bgcodeId"></param>
        /// <param name="docGuid"></param>
        /// <param name="bgresourceId"></param>
        /// <returns></returns>
        private double GetAddUpTotalByYWKey(Guid? projectId, Guid? departmentId, Guid? bgcodeId, Guid docGuid, Guid? bgresourceId)
        {
            var ywKey = this.YWKey;
            double d = 0F;
            switch (YWKey)
            {
                case Constant.YWTwo: //"报销管理":
                    d = BX_AddUpTotal(projectId,departmentId,bgcodeId,docGuid,bgresourceId);
                    break;
                case Constant.YWFive:// "往来管理":
                    d=WL_AddUpTotal(projectId, departmentId, bgcodeId, docGuid, bgresourceId);
                    break;
            }
            return d;
        }
    }
    
    public class BudgetModel
    {
        /// <summary>
        /// 明细ID
        /// </summary>
      public Guid? GUID { set; get; }
      public Guid? GUID_DW { set; get; }
      public string DWKey { set; get; }
      public string DWName { set; get; }
      public Guid? GUID_BGCode { set; get; }
      public string BGCodeKey { set; get; }
      public string BGCodeName { set; get; }
      public Guid? GUID_Project { set; get; }
      public string ProjectKey { set; get; }
      public string ProjectName { set; get; }
      public Guid? GUID_BGResource { set; get; }
      public string BGSourceName { set; get; }
      public double? TotalPlan { set; get; }
      public Guid? GUID_Department { set; get; }
      public string DepartmentName { set; get; }
    /// <summary>
    /// 余额
    /// </summary>
      public double BalanceTotal { set; get; }
    /// <summary>
    /// 完成比例
    /// </summary>
      public string CompletionRate { set; get; }
        /// <summary>
        /// 累计发生
        /// </summary>
      public double AddUpTotal { set; get; }
        /// <summary>
        /// 本次发生
        /// </summary>
      public double ThisTimeTotal { set; get; }
        /// <summary>
        /// 预算类型名称
        /// </summary>
      public string BGTypeName { set; get; }
        /// <summary>
        /// 预算条目Key
        /// </summary>
      public string BGItemKey { set; get; }
    }
}
