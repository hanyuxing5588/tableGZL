using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;

namespace Business.CommonModule
{
    public class 查询条件
    {
    }
    /// <summary>
    /// 树节点
    /// </summary>
    public class TreeNode : SearchCondition
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
    /// 历史查询条件
    /// </summary>
    public class HistoryBaseCondition : SearchCondition
    {
        /// <summary>
        /// 报销单号
        /// </summary>
        public string DocNum { get; set; }
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { get; set; }
        /// <summary>
        /// 月份
        /// </summary>
        public string Month { get; set; }
        /// <summary>
        /// 树上选中节点的模型名
        /// </summary>
        public string treeModel { get; set; }
        /// <summary>
        /// 树上选中节点的GUID
        /// </summary>
        public Guid treeValue { get; set; }
        /// <summary>
        /// 审批状态 0 标示全部 1未审核 2已审核  3审核中
        /// </summary>
        public string ApproveStatus { set; get; }
        /// <summary>
        /// 支票状态 O全部 1未领取 2已领取
        /// </summary>
        public string CheckStatus { set; get; }
        /// <summary>
        /// 提现状态 0全部 1未提现 2已提现
        /// </summary>
        public string WithdrawStatus { set; get; }
        /// <summary>
        /// 付款状态 0全部 1未付款 2已付款
        /// </summary>
        public string PayStatus { set; get; }
        /// <summary>
        /// 凭证状态 0全部 1未生成凭证 2已经生成凭证
        /// </summary>
        public string CertificateStatus { set; get; }
        /// <summary>
        /// 作废状态 0全部 1表示未作废 2已作废 
        /// </summary>
        public string CancelStatus { set; get; }

        ///// <summary>
        ///// 操作员GUID
        ///// </summary>
        //public Guid OperatorID { set; get; }
    }
    /// <summary>
    /// 历史查询条件类
    /// </summary>
    public class HistoryCondition : HistoryBaseCondition
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { set; get; }//StartDate
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { set; get; }//EndDate
        /// <summary>
        /// 请求类型
        /// </summary>
        public string RequestType { set; get; }
        #region 现金存取应用
        /// <summary>
        /// 单据类型
        /// </summary>
        public Guid GUID_DocType { set; get; }
        /// <summary>
        /// 业务类型Key
        /// </summary>
        public string YWTypeKey { set; get; }
        /// <summary>
        /// 结算方式
        /// </summary>
        public string SettleTypeKey { set; get; }
        #endregion

        #region 工资单
        /// <summary>
        /// 工资计划GUID
        /// </summary>
        public Guid? GUID_Plan { set; get; }       
        /// <summary>
        /// 发放状态
        /// </summary>
        public string PayOutState { set; get; }
        #endregion
    }   
    /// <summary>
    /// 个人借款信息查询条件
    /// </summary>
    public class BorrowMoneyCondition : SearchCondition
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { set; get; }
        /// <summary>
        /// 树上选中节点的模型名
        /// </summary>
        public string treeModel { get; set; }
        /// <summary>
        /// 树上选中节点的GUID
        /// </summary>
        public Guid treeValue { get; set; }

    }
    /// <summary>
    /// 预算查询条件
    /// </summary>
    public class BudgetStatisticsCondition : SearchCondition
    {
        /// <summary>
        /// 报销年
        /// </summary>
        public string DocDate { set; get; }
        /// <summary>
        /// 单位GUID
        /// </summary>
        public Guid GUID_DW { set; get; }
        /// <summary>
        /// 部门GUID
        /// </summary>
        public Guid GUID_Department { set; get; }
        /// <summary>
        /// 项目
        /// </summary>
        public Guid GUID_Project { set; get; }
        /// <summary>
        /// 科目
        /// </summary>
        public Guid GUID_BGCode { set; get; }
        /// <summary>
        ///预算来源
        /// </summary>
        public Guid GUID_BGResource { set; get; }
        /// <summary>
        /// 报销主GUID
        /// </summary>
        public Guid GUID { set; get; }
        /// <summary>
        /// 操作人GUID
        /// </summary>
        public Guid GUID_Operator { set; get; }

    }
    /// <summary>
    /// 报销单据列表查询条件
    /// </summary>
    public class BX_BillListCondition : HistoryBaseCondition
    {        
        /// <summary>
        /// 单据类型
        /// </summary>
        public string DocType { set; get; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { set; get; }
        /// <summary>
        /// 是否显示明细
        /// </summary>
        public bool IsShowDetail { set; get; }
        /// <summary>
        /// 预算类型 0表示全部 1表示基本支出 2表示项目支出
        /// </summary>
        public string BGType{set;get;}
        /// <summary>
        /// 开始金额
        /// </summary>
        public string StartTotal { set; get; }
        /// <summary>
        /// 结束金额
        /// </summary>
        public string EndTotal { set; get; }
        /// <summary>
        /// 树节点List
        /// </summary>
        public List<TreeNode> TreeNodeList { set; get; }
        
      
    }
    /// <summary>
    /// 单据列表
    /// </summary>
    public class BillListConddition : BX_BillListCondition
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string YWType { set; get; }
    }
    /// <summary>
    /// 单据列表历史查询条件
    /// </summary>
    public class BillHistoryCondition : HistoryCondition
    {
        /// <summary>
        /// 单据业务类型
        /// </summary>
        public string DocYWType { set; get; }
    }
    /// <summary>
    /// 凭证条件
    /// </summary>
    public class PZHistoryCondition : HistoryCondition
    {   
        /// <summary>
        /// 会计年度
        /// </summary>
        public string FiscalYear {set;get;}
        /// <summary>
        /// 凭证期间
        /// </summary>
        public string CWPeriod{set;get;}
        /// <summary>
        /// 凭证编号
        /// </summary>
        public string DocNum{set;get;}
        /// <summary>
        /// 凭证类型
        /// </summary>
        public string GUID_PZType{set;get;}
        /// <summary>
        /// 凭证类型
        /// </summary>
        public string DocDate{set;get;}
        /// <summary>
        /// 制单人
        /// </summary>
        public string GUID_Maker{set;get;}
        /// <summary>
        /// 凭证帐套(ExteriorDataBase)
        /// </summary>
        public string AccountKey { set; get; }
        /// <summary>
        /// 对方帐套
        /// </summary>
        public string Ino_ID { set; get; }
    }
    /// <summary>
    /// 支票查询条件
    /// </summary>
    public class CheckHistoryCondition : TreeNode
    {
        /// <summary>
        /// 支票号
        /// </summary>
        public string CheckNumber { set; get; }
        /// <summary>
        /// 支票类型
        /// </summary>
        public string CheckType { set; get; }
    }

    #region 报表查询条件
    /// <summary>
    /// 报表查询条件
    /// </summary>
    public class BBSearchCondition : SearchCondition
    {
        /// <summary>
        /// 树节点列表
        /// </summary>
        public List<TreeNode> TreeNodeList { set; get; }
        /// <summary>
        /// 年度
        /// </summary>
        public string Year { set; get; }     
        /// <summary>
        /// 货币单位
        /// </summary>
        public string RMBUnit { set; get; }
        /// <summary>
        /// 预算类型
        /// </summary>
        public string BGResourceType { set; get; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartDate { set; get; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndDate { set; get; }
        /// <summary>
        /// 审批状态 0 标示全部 1未审核 2已审核  3审核中
        /// </summary>
        public string ApproveStatus { set; get; }
        /// <summary>
        /// 核销状态 0表全部 1表示未核销 2表示已核销
        /// </summary>
        public string HXStatus { set; get; }
        /// <summary>
        /// 付款状态 0全部 1未付款 2已付款
        /// </summary>
        public string PayStatus { set; get; }
        /// <summary>
        /// 凭证状态 0全部 1未生成凭证 2已经生成凭证
        /// </summary>
        public string CertificateStatus { set; get; }
        /// <summary>
        /// 树上选中节点的模型名
        /// </summary>
        public string treeModel { get; set; }
        /// <summary>
        /// 树上选中节点的GUID
        /// </summary>
        public Guid treeValue { get; set; }
        /// <summary>
        /// 开始月
        /// </summary>
        public string StartMonth { get; set; }
        /// <summary>
        /// 结束月
        /// </summary>
        public string EndMonth { get; set; }
        /// <summary>
        /// 项目级别
        /// </summary>
        public string ProjectLevel { get; set; }

        public string ProjectKeys { get; set; }
    }
    #endregion
    /// <summary>
    /// 劳务费个税月度汇总
    /// </summary>
    public class lwfgsydhzCondition:SearchCondition
    {
        /// <summary>
        /// 年度
        /// </summary>
        public int Year { set; get; }
        /// <summary>
        /// 月度
        /// </summary>
        public int Month { set; get; }
        /// <summary>
        /// 外聘人员名称
        /// </summary>
        public string InvitePersonName { set; get; }
    }
    public class FlowNodeInfo
    {
        public string StateKey{get;set;}
        public string WorkFlowNodeId{get;set;}
    }
    public class ysfpCondition : SearchCondition
    {
        public List<TreeNode> TreeNodeList { set; get; }
        // 流程节点状态
        public List<FlowNodeInfo> NodeCondition {set;get;}

        // 预算部门
        public string Guid_Department { get; set; }
        // 预算部门
        public string Guid_Dw { get; set; }
        // 预算设置
        public string GUID_BGSetUp { get; set; }
        // 预算步骤
        public string BGStep { get; set; }
        // 预算类型
        public string BGType { get; set; }
        // 年
        public string Year { get; set; }
        // 预算分配状态
        public string ysfpState { get; set; }
        // 预算初始值编制状态
        public string yscszbzState { get; set; }
        // 预算初始值审批状态
        public string yscszspState { get; set; }
        // 预算编制状态
        public string ysbzState { get; set; }
        // 预算编制审批状态
        public string ysbzspState { get; set; }
        // 预算审批状态
        public string ysspState { get; set; }
    }
    /// <summary>
    /// 类款项汇总
    /// </summary>
    public class lkxhzCondition : SearchCondition
    {
        /// <summary>
        /// 年度
        /// </summary>
        public int Year { set; get; }
        /// <summary>
        /// 月度
        /// </summary>
        public int Month { set; get; } 
    }
    /// <summary>
    /// 历史记录
    /// </summary>
    public class lkxhzHistoryCondition : SearchCondition
    {
        /// <summary>
        /// 工资计划GUID
        /// </summary>
        public Guid? GUID_Plan { set; get; }
        /// <summary>
        /// 年份
        /// </summary>
        public string Year { set; get; }
        /// <summary>
        /// 月度
        /// </summary>
        public string Month { set; get; }
        /// <summary>
        /// 发放状态
        /// </summary>
        public string PayOutState { set; get; }
    }
    /// <summary>
    /// 个税申报表
    /// </summary>
    public class gssbbCondition : SearchCondition
    {
        /// <summary>
        /// 年度
        /// </summary>
        public string Year { set; get; }
        /// <summary>
        /// 月度
        /// </summary>
        public string Month { set; get; }
       
    }
    /// <summary>
    /// 条件扩展
    /// </summary>
    public static class ConditionExtension
    {
        /// <summary>
        /// 获取TreeNode List的GUID集合
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="modelName"></param>
        public static List<Guid> GetGUIDList(this List<TreeNode> obj, string modelName)
        {
            List<Guid> list = new List<Guid>();
            if (obj == null) return null;
            var tn = obj.FindAll(e=>e.treeModel==modelName);
            Guid g;
            foreach (TreeNode item in tn)
            {
                if (item.treeValue.IndexOf(",") >= 0)
                {
                    string[] ids = item.treeValue.Split(',');
                    List<string> idlist = ids.ToList();
                    foreach (string strItem in idlist)
                    {
                        if (Guid.TryParse(strItem, out g))
                        {
                            if (!list.Contains(g))
                            {
                                list.Add(g);
                            }
                        }
                    }
                }
                else
                {
                    if (Guid.TryParse(item.treeValue, out g))
                    {
                        if (!list.Contains(g))
                        {
                            list.Add(g);
                        }
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 获取TreeNode List的Key集合
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="modelName"></param>
        public static List<string> GetKeyList(this List<TreeNode> obj, string modelName)
        {
            List<string> list = new List<string>();
            if (obj == null) return null;
            var tn = obj.FindAll(e => e.treeModel == modelName);            
            foreach (TreeNode item in tn)
            {
                if (item.treeValue.IndexOf(",") >= 0)
                {
                    string[] ids = item.treeValue.Split(',');
                    List<string> idlist = ids.ToList();
                    foreach (string strItem in idlist)
                    {
                        if (!string.IsNullOrEmpty(strItem))
                        {
                            if (!list.Contains(strItem))
                            {
                                list.Add(strItem);
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.treeValue))
                    {
                        if (!list.Contains(item.treeValue))
                        {
                            list.Add(item.treeValue);
                        }
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 获取TreeNode的GUID集合
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="modelName"></param>
        public static List<Guid> GetGUIDList(this TreeNode obj)
        {
            List<Guid> list = new List<Guid>();
            if (obj == null) return null;           
            Guid g;
            if (obj.treeValue.IndexOf(",") >= 0)
            {
                string[] ids = obj.treeValue.Split(',');
                List<string> idlist = ids.ToList();
                foreach (string strItem in idlist)
                {
                    if (Guid.TryParse(strItem, out g))
                    {
                        if (!list.Contains(g))
                        {
                            list.Add(g);
                        }
                    }
                }
            }
            else
            {
                if (Guid.TryParse(obj.treeValue, out g))
                {
                    if (!list.Contains(g))
                    {
                        list.Add(g);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 获取TreeNode的Key集合
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="modelName"></param>
        public static List<string> GetKeyList(this TreeNode obj)
        {
            List<string> list = new List<string>();
            if (obj == null) return null;
            Guid g;
            if (obj.treeValue.IndexOf(",") >= 0)
            {
                string[] ids = obj.treeValue.Split(',');
                List<string> idlist = ids.ToList();
                foreach (string strItem in idlist)
                {
                    if (!string.IsNullOrEmpty(strItem))
                    {
                        if (!list.Contains(strItem))
                        {
                            list.Add(strItem);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(obj.treeValue))
                {
                    if (!list.Contains(obj.treeValue))
                    {
                        list.Add(obj.treeValue);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 得到模型类型集合
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<string> GetModelType(this List<TreeNode> obj)
        {
            List<string> list = new List<string>();
            if (obj == null) return null;
            foreach (TreeNode item in obj)
            {
                if (!string.IsNullOrEmpty(item.treeModel))
                {
                    if (!list.Contains(item.treeModel.Trim()))
                    {
                        list.Add(item.treeModel.Trim());
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 判断GUID
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool ContainsGUID(this List<Guid> obj,Guid guid)
        {
            if (obj == null || obj.Count == 0)
            {
                return false;
            }
            else
            {
                return obj.Contains(guid);
            }
        }
        /// <summary>
        /// 判断GUID
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool ContainsGUID(this List<Guid?> obj, Guid guid)
        {
            if (obj == null || obj.Count == 0)
            {
                return false;
            }
            else
            {
                return obj.Contains(guid);
            }
        }
        /// <summary>
        /// GUID list 转换成Sql 中的 In 查询字符串 例如：'1','2','3',..
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertSQLInGUID(this List<Guid> obj)
        {
            string str = string.Empty;
            if (obj == null || obj.Count == 0)
            {
                return str ;
            }
            foreach (Guid item in obj)
            {
                if (item == obj[obj.Count - 1])
                {
                    str += "'" + item + "'";
                }
                else
                {
                    str += "'" + item + "',";
                }
            }
            return str;
        }

       /// <summary>
       /// 用符号连接字符串
       /// </summary>
       /// <param name="obj"></param>
       /// <param name="sign">字符串连接符号</param>
       /// <returns></returns>
        public static string SQLStrGUID(this List<Guid> obj,string sign)
        {
            string str = string.Empty;
            if (obj == null || obj.Count == 0)
            {
                return str;
            }
            str = string.Join(sign, obj.ToArray());            
            return str;
        }
    
    }

   


}
