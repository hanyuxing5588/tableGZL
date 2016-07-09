using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.CommonModule.Search
{
    /// <summary>
    /// 单据列表条件组合类
    /// </summary>
    public class DJLBCondition : BaseHistoryCondition
   {
        public bool IsShowDetail { get; set; }
       /// <summary>
       /// 开始时间
       /// </summary>
       public DateTime StartDate { set; get; }
       /// <summary>
       /// 结束时间
       /// </summary>
       public DateTime EndDate { set; get; }
        /// <summary>
        /// 对应树的模型名称
        /// </summary>
       public string treeModel { get; set; }
        /// <summary>
        /// 对应树当前节点的guid
        /// </summary>
       public Guid treeValue { get; set; }
        //public List<TreeCondition> treeNodeList { get; set; }
   }
   /// <summary>
   /// 单据列表的过滤
   /// </summary>
   /// 
    public class DJLBFilterCondition : BaseHistoryCondition
    {
        public bool IsShowDetail { get; set; }
        /// <summary>
        /// 开始时间

        /// </summary>
        public DateTime StartDate { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { set; get; }
       
        public List<TreeCondition> TreeNodeList { get; set; }
        public double? EndTotal { get; set; }
        public double? StartTotal { get; set; }
        public string GUID_ProjectEx { get; set; }//项目管理费用执行情况表（按科目） 2016-4-15
    }


    /// <summary>
    /// 支票领取的选单
    /// </summary>
    public class ZPLQSelectDocCondition : BaseHistoryCondition
    {
        public string Year { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string Month { set; get; }
        /// <summary>
        /// 对应树的模型名称
        /// </summary>
        public string treeModel { get; set; }
        /// <summary>
        /// 对应树当前节点的guid
        /// </summary>
        public Guid treeValue { get; set; }
    }
}
