using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Flow.Run;

namespace Business.IBusiness
{
    public class AgencyDoc
    {
        public Guid DocId{get;set;}
        public string DocTypeName{get;set;}
        public string Scope { get; set; }
        public string DocNum{get;set;}
        public DateTime? DocDate { get; set; }
        public string StrDocDate { get; set; }
        public string CreatePerson{get;set;}
        public string DWName{get;set;}
        public string DeptmentName{get;set;}
        public string Remark{get;set;}
        public int ClassId { get; set; }

    }
    public class FlowDataModel : AgencyTaskEx
    {
        public FlowDataModel() : base() { }
        public Guid DocId { get; set; }
        public double? SumMoney { get; set; }
        public string Scope { get; set; }
        public int ClassId { get; set; }
        public string DocNum { get; set; }
        public string DeptmentName { get; set; }
        public string CreatePerson { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string BGStepName { get; set; }
        public string BGTypeName { get; set; }
        public string StrAcceptDate { get; set; }
        public string StrProcessDate { get; set; }

        
    }
  public  interface IAgencyTask
    {
        
        
        /// <summary>
        /// 启用 提交流程
        /// </summary>
        /// <param name="scope">作用域</param>
        /// <param name="userId">用户ID</param>
        /// <param name="docId">单据ID</param>
        /// <returns>返回结果</returns>
      ResultMessager DocSumitWorkFlow(string scope, Guid userId, Guid docId, int classId);
        
        /// <summary>
        /// 退回
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="userId"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
      ResultMessager DocSendBackWorkFlow(string scope, Guid userId, Guid docId, int classId);

        /// <summary>
        /// 打开单据
        /// </summary>
        /// <param name="processID"></param>
        /// <param name="processNodeID"></param>
        /// <param name="nodeLevel"></param>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        bool OpenDocForWorkFlow(Guid processID, Guid processNodeID, int nodeLevel, int nodeType);

        /// <summary>
        /// 给没有启动的 单据单据
        /// </summary>
        /// <returns></returns>
        IEnumerable<AgencyDoc> GetDocData(Guid userId);

        /// <summary>
        /// 给流程数据的 显示全部的项
        /// </summary>
        /// <returns></returns>
        IEnumerable<FlowDataModel> GetFlowDataModel(string YWKey, Guid DocTypeGuid,Guid UserId, string DocNum);
    }
}
