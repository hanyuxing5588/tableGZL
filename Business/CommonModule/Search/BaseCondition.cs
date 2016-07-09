using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.CommonModule
{

    public class BaseCondition
    {
        public Guid DocType { get; set; }
        public string YWType { get; set; }
    }
   /// <summary>
   /// 树节点
   /// </summary>
    public class TreeCondition
    {
        /// <summary>
        /// 树上选中节点的模型名
        /// </summary>
        public string treeModel { get; set; }
        /// <summary>
        /// 树上选中节点的GUID
        /// </summary>
        public string treeValue { get; set; }//多个GUID用逗号分开
    }
   /// <summary>
   /// 历史
   /// </summary>
   public class BaseHistoryCondition : BaseCondition
   {
       /// <summary>
       /// 报销单号
       /// </summary>
       public string DocNum { get; set; }
       /// 审批状态 0 标示全部 1未审核 2已审核 3审核中       /// </summary>
       public string ApproveStatus { set; get; }
       /// <summary>
       /// 支票状态 O全部 1未领取 2已领取       /// </summary>
       public string CheckStatus { set; get; }
       /// <summary>
       /// 提现状态 0全部 1未提现 2已提现       /// </summary>
       public string WithdrawStatus { set; get; }
       /// <summary>
       /// 付款状态 0全部 1未付款 2已付款       /// </summary>
       public string PayStatus { set; get; }
       /// <summary>
       /// 凭证状态 0全部 1未生成凭证 2已经生成凭证
       /// </summary>
       public string CertificateStatus { set; get; }
       /// <summary>
       /// 作废状态 0全部 1表示未作废 2已作废 
       /// </summary>
       public string CancelStatus { set; get; }

   }
}
