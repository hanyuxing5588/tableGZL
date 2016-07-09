using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OAModel;
using Infrastructure;

namespace Business.CommonModule
{
  public  class CommonTree
    {
       public OAEntities context = null;

        public CommonTree() { context = new OAEntities(); }
        public CommonTree(string connectstring) { context = new OAEntities(connectstring); }

        #region 基础----桌面设置--政策法规tree
        /// <summary>
        /// 政策法规
        /// </summary>
        /// <param name="filterAuth">是否权限过过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SS_OfficeFileTypeView> GetJCOfficeFileType(bool filterAuth, string operatorGuid)
        {
            List<SS_OfficeFileTypeView> list = new List<SS_OfficeFileTypeView>();
            
            if (filterAuth)
            {
                list = context.SS_OfficeFileTypeView.OrderBy(e => e.FileTypeKey).ToList();
            }
            else
            {
                list = context.SS_OfficeFileTypeView.OrderBy(e => e.FileTypeKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 政策法规--附件
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<OA_OfficeFile> GetJCOfficeFile(bool filterAuth, string operatorGuid)
        {
            return context.OA_OfficeFile.OrderBy(e => e.OrderNum).ToList();
        }

        #endregion


        #region 基础----桌面设置--通知公告tree
        /// <summary>
        /// 通知公告
        /// </summary>
        /// <param name="filterAuth">是否权限过过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<OA_Notice> GetJCNotice(bool filterAuth, string operatorGuid)
        {
            List<OA_Notice> list = new List<OA_Notice>();
            if (filterAuth)
            {
                list = context.OA_Notice.Where(e => e.overdue == false).OrderBy(e => e.OrderNum).ToList();
            }
            else
            {
                list = context.OA_Notice.OrderBy(e => e.OrderNum).ToList();
            }
            return list;
        }
        /// <summary>
        /// 通知公告--附件
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<OA_NoticeAnnexEx> GetJCNoticeAnnex(bool filterAuth, string operatorGuid)
        {
          var  noticeAnnexList = context.OA_NoticeAnnex.Take(1000).Select(e =>new OA_NoticeAnnexEx { 
                GUID=e.GUID,
                GUID_Notice=e.GUID_Notice,
                AnnexName=e.AnnexName
            }).ToList();
          return noticeAnnexList;
        }

        #endregion

        

    }
  public class OA_NoticeAnnexEx {
      public Guid GUID { get; set; }
      public Guid? GUID_Notice { get; set; }
      public string AnnexName { get; set; }
  }
  /// <summary>
  /// 项目执行进度类
  /// </summary>
  public static class ProjectProgress
  {
      #region 首界面-项目执行进度

      public static List<ProjectProgressModel> LoadSenondProjects(this ProjectProgressModel obj,BusinessModel.BusinessEdmxEntities context, List<ProjectProgressModel> data)
      {
          var ProjectsSecondList = data.Where(e => e._parentId == obj.GUID).Select(e => new ProjectProgressModel
          {
              GUID = e.GUID,
              _parentId = e._parentId,
              ProjectName = e.ProjectName,
              ProjectKey = e.ProjectKey,
              PersonName = e.PersonName,
              iBGTotalSum = e.iBGTotalSum,
              iBGTotalXD = e.iBGTotalXD,
              iBXTotal = e.iBXTotal,
              iBalance = e.iBalance,
              iRatio = e.iRatio

          }).OrderBy(e => e.ProjectKey).ToList<ProjectProgressModel>();


          return ProjectsSecondList.ToList();
      }
      public static void RetrieveLeaf(this ProjectProgressModel obj, BusinessModel.BusinessEdmxEntities context, ref List<ProjectProgressModel> result, List<ProjectProgressModel> data)
      {

          var child = obj.LoadSenondProjects(context, data);
          if (child != null && child.Count > 0)
          {

              foreach (var item in child)
              {
                  var q = data.Where(e => e.GUID == item._parentId).ToList();
                  q[0].iBGTotalSum += item.iBGTotalSum;
                  q[0].iBGTotalXD += item.iBGTotalXD;
                  q[0].iBXTotal += item.iBXTotal;
                  q[0].iBalance += item.iBalance;
                  q[0].iRatio += item.iRatio;
                  RetrieveLeaf(item, context, ref result, data);
              }
          }
          else
          {
              result.Add(obj);

          }

      }
      #endregion

  }
  public class ProjectProgressModel 
  {
      public Guid GUID { get; set; }
      public Guid? _parentId { get; set; }
      public string ProjectName { get; set; }
      public string ProjectKey { get; set; }
      public string PersonName { get; set; }
      public double? iBGTotalSum { get; set; }
      public double? iBGTotalXD { get; set; }
      public double? iBXTotal { get; set; }
      public string iBalance { get; set; }
      public string iRatio { get; set; }
  }

}
