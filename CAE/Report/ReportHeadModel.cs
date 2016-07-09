using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAE.Report
{
   public class ReportHeadModel
    {
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { set; get; }
        /// <summary>
        /// 预算年度
        /// </summary>
        public string Year { set; get; }
        /// <summary>
        /// 货币单位
        /// </summary>
        public string RMBUnit { set; get; }
       /// <summary>
       /// 人数
       /// </summary>
        public string PersonCount { set; get; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string Month { set; get; }
        /// <summary>
        /// 打印日期
        /// </summary>
        public string PrintDate { set; get; }
        /// <summary>
        /// 扩展使用
        /// </summary>
        public string Maker { set; get; }
        /// <summary>
        /// 扩展使用
        /// </summary>
        public string Expand { set; get; }

    }
}
