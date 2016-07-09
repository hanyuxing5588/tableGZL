using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Infrastructure;
using Platform.Flow.Run;
using Business;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using BaothApp.Utils;
using System.Data.Objects;
using BusinessModel;
using System.Data;
using OAModel;
using CAE;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Reflection;
using Business.CommonModule;

namespace BaothApp.Controllers.Home
{
    public class HomeController:SpecificController
    {
        BaseConfigEdmxEntities OtherContext = new BaseConfigEdmxEntities();
        //OAEntities context = new OAEntities();
        #region return Home Page
        /// <summary>
        /// return Home Page
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            if (this.CurrentUserInfo == null) { return View("Index"); }
            ViewData["LogonUser"] = this.CurrentUserInfo.UserName;
           // return View("Index3");//工程院是Index3
            //Response.Redirect("/gldoc/Index");
            return View("Index3"); ;
        }
        public ActionResult IndexTest()
        {

            return RedirectToActionPermanent("Index", "gldoc");
        }
        //public override ViewResult Index3()
        //{
        //    if (this.CurrentUserInfo == null) { return View("Index3"); }
        //    ViewData["LogonUser"] = this.CurrentUserInfo.UserName;

        //    return View("Index3");
        //}

        public override ViewResult FirstPage()
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var dept = getDepartMentKeyList(operatorId);
            var deptList = OtherContext.SS_Department.Where(e => dept.Contains(e.DepartmentKey)).Select(e => e.DepartmentName).ToList();
            if (deptList != null && deptList.Count > 0)
            {
                ViewData["DeptName"] = deptList[0];
            }
            else
            {
                ViewData["DeptName"] = "";
            }
            ViewData["Year"] = DateTime.Now.Year;
            ViewData["Month"] = DateTime.Now.Month;
            return View("FirstPage1");
        }

        #endregion
        #region return AgencyTask data
        public JsonResult GetAcencyTaskData()
        {
            var atList = Platform.Flow.Run.WorkFlowAPI.GetFlowData(this.CurrentUserInfo.UserGuid);
            if (atList.Count > 100)
            {
                atList.RemoveRange(100, atList.Count - 100);
            }
            List<FlowDataShowModel> showlist = new List<FlowDataShowModel>();
            var businesContext = new BusinessEdmxEntities();
            foreach (AgencyTask item in atList)
            {
                FlowDataShowModel value = new FlowDataShowModel(item.ProcessID, item.ProcessNodeID, item.NodeLevel, item.NodeName, item.AcceptDate, item.NodeType);
                value.UserName = this.CurrentUserInfo.UserName;
                value.LoadAppendInformations(OtherContext, businesContext);
                if (value.BXUserName == null && value.DocNum == null)
                {

                }
                else {
                    showlist.Add(value);
                }
            }
            return Json(showlist, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Notice data
        public ContentResult GetNoticeData()
        {
            OAEntities context = new OAEntities();
            try
            {
                var list = context.OA_Notice.OrderBy(e => e.OrderNum).Select(e =>
                    new
                    {
                        GUID = e.GUID,
                        Notice = e.Notice,
                        Title = e.Title,
                        NoticeDate = e.NoticeDate,
                        EndDate = e.EndDate
                    }).AsEnumerable();
                var tempList = list.Select(e => new
                {
                    GUID = e.GUID,
                    Notice = e.Notice,
                    Title = e.Title,
                    NoticeDate = e.NoticeDate == null ? "" : ((DateTime)e.NoticeDate).ToString("yyyy-MM-dd"),
                    EndDate = e.EndDate == null ? "" : ((DateTime)e.EndDate).ToString("yyyy-MM-dd"),
                    IsNew = e.NoticeDate == null ? false : (Math.Abs(DateTime.Now.Subtract((DateTime)e.NoticeDate).TotalHours) < 2)
                }).OrderByDescending(e => e.NoticeDate).ToList();
                var rowJson = BaothApp.Utils.JsonHelper.objectToJson(tempList);
                string json = JsonHelper.PageJsonFormat(rowJson, tempList.Count);
                return Content(json);
            }
            catch
            {
                return Content(null);
            }

        }
        #endregion
        #region 根据tree id 返回相应数据集合
        public ContentResult GetDataByTreeId()
        {
            OAEntities context = new OAEntities();
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var b = Guid.TryParse(guid, out g) == true ? id = g : id = Guid.Empty;
            var q = context.OA_OfficeFile.OrderBy(e => e.OrderNum);
            List<object> list = new List<object>();
            if (id.Equals(Guid.Empty))
            {
                list = q.Select(e => new { e.GUID, e.FileName, e.FileKey, e.GUID_OfficeFileType }).ToList<object>();
            }
            else
            {
                list = q.Where(e => e.GUID_OfficeFileType == id).Select(e => new { e.GUID, e.FileName, e.FileKey, e.GUID_OfficeFileType }).ToList<object>();
            }
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        #endregion
        #region 根据datagrid id返回相应数据集合
        public JsonResult GetDataByGridId()
        {
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var b = Guid.TryParse(guid, out g) == true ? id = g : id = Guid.Empty;
            try
            {
                using (OAEntities context = new OAEntities()) 
                {
                    var list = context.OA_NoticeAnnex.Where(e => e.GUID_Notice == g).Select(e => new { e.GUID, e.GUID_Notice, e.AnnexName }).ToList();
                    return Json(new { total = list.Count, rows = list });
                }
            }
            catch (Exception)
            {

                return Json(new { total = 0, rows = new List<object>() });
            }
        }
        #endregion
        #region 根据查询关键字返回相应数据集合


        public ContentResult GetDataByKeyWord()
        {
            OAEntities context = new OAEntities();
            var KeyWords = Request["KeyWord"];

            var q = context.OA_OfficeFile.OrderBy(e => e.OrderNum);

            List<object> list = new List<object>();

            if (string.IsNullOrEmpty(KeyWords))
            {
                list = q.Select(e => new { e.GUID, e.FileName, e.FileKey, e.GUID_OfficeFileType }).ToList<object>();
            }
            else
            {
                list = q.Where(e => e.FileName.Contains(KeyWords)).Select(e => new { e.GUID, e.FileName, e.FileKey, e.GUID_OfficeFileType }).ToList<object>();
            }
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            string json = JsonHelper.PageJsonFormat(rowJson, list.Count);
            return Content(json);
        }
        #endregion
        #region OfficeFileDown
        public ActionResult GetOfficeDownFile()
        {
            OAEntities context = new OAEntities();
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var flag = Guid.TryParse(guid, out g);
            if (flag)
            {
                id = g;
            }
            else
            {
                id = Guid.Empty;
            }
            var q = context.OA_OfficeFile.FirstOrDefault(e => e.GUID == id);
            byte[] data = q.FileBody;
            var filetype = CommonFuntion.GetFileType(q.FileName);
            return File(data, filetype, q.FileName);
        }
        #endregion
        #region NoticeFileDown
        public ActionResult GetNoticeDownFile()
        {
            OAEntities context = new OAEntities();
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var b = Guid.TryParse(guid, out g) == true ? id = g : id = Guid.Empty;
            var q = context.OA_NoticeAnnex.FirstOrDefault(e => e.GUID == id);
            byte[] data = q.Annex;

            var filetype = CommonFuntion.GetFileType(q.AnnexName);

            return File(data, filetype, q.AnnexName);
        }
        #endregion
        #region 校验政策法规附件是否存在
        public JsonResult IsOfficeExist()
        {
            OAEntities context = new OAEntities();
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var bo = Guid.TryParse(guid, out g);
            if (bo)
            {
                id = g;
            }
            else
            {
                id = Guid.Empty;
            }
            var q = context.OA_OfficeFile.FirstOrDefault(e => e.GUID == id);
            bool flag = false;
            if (q.FileBody != null)
            {
                flag = true;
            }
            return Json(new { flag });
        }
        #endregion
        #region 校验通知公告附件是否存在
        public JsonResult IsNoticeExist()
        {
            OAEntities context = new OAEntities();
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            Guid id = Guid.Empty;
            var b = Guid.TryParse(guid, out g) == true ? id = g : id = Guid.Empty;
            var q = context.OA_NoticeAnnex.FirstOrDefault(e => e.GUID == id);
            bool flag = false;
            if (q.Annex != null)
            {
                flag = true;
            }
            return Json(new { flag = flag });
        }
        #endregion
        #region Progress data
        public ContentResult GetProgressData()
        {
            var context = new BusinessEdmxEntities();
            //获取当前操作员guid
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var year = DateTime.Now.Year;
            var dept = getDepartMentKeyList(operatorId);

            var departKey = dept.GetStrGUIDS();
            var project = getProjectKeyList(operatorId);
            var projectKey = project.GetStrGUIDS();
            var SqlFormat = initSql();
            var sql = string.Format(SqlFormat, year, departKey, projectKey);
            //返回所有符合条件的数据集合
            var data = context.ExecuteStoreQuery<ProjectProgressModel>(sql).ToList();
            foreach (var item in data)
            {
                item.iBalance = (doubleConvent(item.iBGTotalXD) - doubleConvent(item.iBXTotal)).ToString();
                item.iRatio = doubleConvent(item.iBGTotalXD) == 0.0 ? "" : (doubleConvent(item.iBXTotal) / doubleConvent(item.iBGTotalXD) * 100).ToString();

            }
            foreach (var item in data)
            {
                List<ProjectProgressModel> ChildList = new List<ProjectProgressModel>();
                item.RetrieveLeaf(context, ref ChildList, data);
            }
            double g = 0.0;
            var iBGTotalSum = data.Where(e => e._parentId == null).Sum(e => e.iBGTotalSum);
            var iBGTotalXD = data.Where(e => e._parentId == null).Sum(e => e.iBGTotalXD);
            var iBXTotal = data.Where(e => e._parentId == null).Sum(e => e.iBXTotal);
            var iBalance = data.Where(e => e._parentId == null).Sum(e => double.TryParse(e.iBalance, out g) ? g : 0.00);
            var iRatio = (iBXTotal / iBGTotalXD) * 100;
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(data);
            var rowTotal = "[{\"ProjectName\":\"合计\",\"ProjectKey\":\"\",\"PersonName\":\"\",\"iBGTotalSum\":" + iBGTotalSum + ",\"iBGTotalXD\":" + iBGTotalXD + ",\"iBXTotal\":" + iBXTotal + ",\"iBalance\":" + iBalance + ",\"iRatio\":" + iRatio + "}]";
            string json = JsonHelper.PageTotalJsonFormat(rowJson, rowTotal, data.Count);
            return Content(json);
        }


        public static DataTable ListToDataTable<T>(List<T> entitys)
        {
            if (entitys == null || entitys.Count < 1)
            {
                throw new Exception("需转换的集合为空");
            }
            Type entityType = entitys[0].GetType();
            PropertyInfo[] entityProperties = entityType.GetProperties();
            DataTable dt = new DataTable();
            for (int i = 0; i < entityProperties.Length; i++)
            {
                dt.Columns.Add(entityProperties[i].Name);
            }
            foreach (object entity in entitys)
            {
                if (entity.GetType() != entityType)
                {
                    throw new Exception("要转换的集合元素类型不一致");
                }
                object[] entityValues = new object[entityProperties.Length];
                for (int i = 0; i < entityProperties.Length; i++)
                {
                    entityValues[i] = entityProperties[i].GetValue(entity, null);
                }
                dt.Rows.Add(entityValues);
            }
            return dt;
        }
        protected double doubleConvent(double? dTemp)
        {
            return dTemp == null ? 0.0 : (double)dTemp;
        }
        public IQueryable<Guid> GetDataSet(string classid, string operatorGuid)
        {
            IQueryable<Guid> list = null;
            if (CommonFuntion.IsGUID(operatorGuid))
            {
                Guid g;
                g = CommonFuntion.ConvertGUID(operatorGuid);
                ////获取数据权限    

                //角色GUID
                var guidlist = from role in OtherContext.SS_Role
                               join rp in OtherContext.SS_RoleOperator on role.GUID equals rp.GUID_Role
                               where rp.GUID_Operator == g
                               select role.GUID;
                //数据权限
                var qss_dataauthset = from dset in OtherContext.SS_DataAuthSet
                                      where (dset.GUID_RoleOrOperator == g || guidlist.Contains(dset.GUID_RoleOrOperator)) && dset.ClassID == classid && dset.IsDefault == true
                                      select dset.GUID_Data;
                list = qss_dataauthset;
            }
            return list;
        }

        public List<string> getDepartMentKeyList(string OperatorGuid)
        {
            List<string> list = new List<string>();
            //部门权限-部门权限去设置了默认值的部门
            var deptDataAuth = GetDataSet("2", OperatorGuid);
            //获取单位权限信息
            var dept = from a in OtherContext.SS_DepartmentView
                       where a.IsStop == false && deptDataAuth.Contains(a.GUID)
                       select a.DepartmentKey;
            list = dept.ToList();
            return list;
        }
        public List<string> getProjectKeyList(string OperatorGuid)
        {
            List<string> list = new List<string>();
            //项目权限-项目权限去设置了默认值的部门
            var protDataAuth = new IntrastructureFun().GetDataSet("5", OperatorGuid).ToList();
            //获取项目权限信息
            var project = from b in OtherContext.SS_ProjectView
                          where b.IsStop == false && protDataAuth.Contains(b.GUID)
                          select b.ProjectKey;

            list = project.ToList();
            return list;
        }
        #endregion
        #region initSql
        public string initSql()
        {
            var SqlFormat = "Select Project.GUID,Project.PGUID as '_parentId',Project.ProjectName,Project.ProjectKey,"
                     + "isnull(BG.PersonName,'') as PersonName,isnull(BG.iBGTotalSum,0) as iBGTotalSum,"
                     + "isnull(BG.iBGTotalXD,0) as iBGTotalXD,isnull(BX.iBXTotal,0) as iBXTotal,'' as 'Child' From SS_Project Project "
                     + "Left Join (Select BGMain.GUID_Project,BGMain.ProjectName,BGMain.ProjectKey,BGMain.PersonName,BGTotal.iBGTotalSum,BGXD.iBGTotalXD From BG_MainView BGMain "
                     + "Left Join (Select GUID_BG_Main,Sum(Total_BG) As iBGTotalSum From BG_DetailView "
                     + "Where len(bgcodekey)=2 and BGItemKey in ('03','04','07','08') And BGYear ='{0}'  "
                     + "Group By GUID_BG_Main) BGTotal On BGMain.GUID=BGTotal.GUID_BG_Main "
                     + "Left Join (Select GUID_BG_Main,Sum(Total_BG) As iBGTotalXD From BG_DetailView "
                     + "Where len(bgcodekey)=2 and BGItemKey in ('07','08') And BGYear ='{0}'  "
                     + "Group By GUID_BG_Main) BGXD On BGMain.GUID=BGXD.GUID_BG_Main "
                     + "Where BGMain.DepartMentKey in({1}) And BGMain.ProjectKey in({2}) "
                     + "And  BGSetupKey in('08') and guid in (select guid_bg_main from bg_detailview where len(bgcodekey)=2 and bgyear='{0}')) "
                     + "BG On Project.GUID=BG.GUID_Project Left Join (Select GUID_Project,Sum(Total_BX) As iBXTotal  From BX_DetailView "
                     + "Where DepartmentKey In({1}) And ProjectKey In({2}) And GUID_PaymentNumber In(Select GUID from CN_PaymentNumberView "
                     + "Where isnull(BGSourceKey,1) In('1','2')) And GUID_BX_Main In(Select GUID From BX_MainView "
                     + "Where  1=1  and  1=1  and  1=1  and Year(DocDate)='{0}' And Month(DocDate)>='1'  And Month(DocDate)<='12') Group By GUID_Project) "
                     + "BX On Project.GUID=BX.GUID_Project Where Isnull(Project.IsStop,0)=0 and Isnull(Project.StopYear,0)<3000 And Project.ProjectKey In({2}) "
                     + "and (project.guid in(select distinct guid_project from bg_mainview "
                     + "where guid in(select distinct guid_bg_main from bg_detail where BGYear='{0}') and DepartMentKey in({1}) And  BGSetupKey in('08')) "
                     + "or project.guid in(select distinct pguid from ss_project where guid in (select guid_project from bg_mainview "
                     + "where guid in(select distinct guid_bg_main from bg_detail where BGYear='{0}') and DepartMentKey in({1}) And  BGSetupKey in('08')  ) )) order by Project.ProjectKey";
            return SqlFormat;
        }
        #endregion
        #region File data
        public JsonResult GetFileData()
        {
            OAEntities context = new OAEntities();
            try
            {
                var q = context.OA_OfficeFile.OrderBy(e => e.OrderNum).Select(e => new { e.GUID, e.FileName, e.FileKey, e.GUID_OfficeFileType });
                var list = q.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(null);
            }
        }
        #endregion
        #region 系统菜单
        /// <summary>
        /// 不通过权限过滤所有菜单项(Menu)
        /// </summary>
        /// <returns>返回菜单项list集合</returns>
        public JsonResult GetSysMenu()
        {
            #region 加载功能列表项

            //List<object> sysMenuFirstList = new List<object>();
            //List<object> sysMenuSecondList = new List<object>();
            ////返回所有菜单大类(MenuClass)list集合
            //List<SS_MenuClass> sysMenuClassList = this.GetMenuClass();
            //StringBuilder JsonBulider = new StringBuilder();
            //JsonBulider.Append("[");
            ////循环遍历返回的(MenuClass)list集合对象
            //foreach (var smcList in sysMenuClassList)
            //{
            //    JsonBulider.Append("{");
            //    JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "GUID", smcList.GUID);
            //    JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "MenuKey", smcList.MenuClassKey);
            //    JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "MenuName", smcList.MenuClassName);
            //    #region 加载第一级菜单




            //    JsonBulider.Append("\"son\":[");

            //    sysMenuFirstList = ModelExtend.LoadFirstMenus(smcList,context);
            //    if (smcList.MenuClassKey == "00") { 


            //    }
            //    foreach (var smlf in sysMenuFirstList)
            //    {
            //        JsonBulider.Append("{");
            //        JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "GUID", smlf.GUID);
            //        JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "MenuKey", smlf.MenuKey);
            //        JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "MenuName", smlf.MenuName);
            //        #region 加载第二级菜单




            //        JsonBulider.Append("\"grandson\":[");
            //        sysMenuSecondList = ModelExtend.LoadSenondMenus(smlf, context);
            //        foreach (var smls in sysMenuSecondList)
            //        {
            //            JsonBulider.Append("{");
            //            JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "GUID", smls.GUID);
            //            JsonBulider.AppendFormat("\"{0}\":\"{1}\",", "MenuKey", smls.MenuKey);
            //            JsonBulider.AppendFormat("\"{0}\":\"{1}\"", "MenuName", smls.MenuName);
            //            JsonBulider.Append("},");
            //            if (smls == sysMenuSecondList[sysMenuSecondList.Count - 1])
            //            {
            //                JsonBulider.Remove(JsonBulider.ToString().LastIndexOf(","), 1);
            //            }
            //        }
            //        JsonBulider.Append("]");
            //        JsonBulider.Append("},");
            //        if (smlf == sysMenuFirstList[sysMenuFirstList.Count - 1])
            //        {
            //            JsonBulider.Remove(JsonBulider.ToString().LastIndexOf(","), 1);
            //        }
            //        #endregion
            //    }
            //    JsonBulider.Append("]");
            //    JsonBulider.Append("},");
            //    if (smcList == sysMenuClassList[sysMenuClassList.Count - 1])
            //    {
            //        JsonBulider.Remove(JsonBulider.ToString().LastIndexOf(","), 1);
            //    }
            //    #endregion
            //}
            //JsonBulider.Append("]");
            //return Content(JsonBulider.ToString());
            #endregion
            MenuFun mf = new MenuFun();
            object result = new object();

            var OperGuid = CurrentUserInfo.UserGuid;
            if (OperGuid != null)
            {
                result = mf.GetMenu(OtherContext, OperGuid);

            }

            return Json(result);

        }
        #endregion
        #region 待办任务
        //点击待办执行的方法

        public JsonResult GetViewParameters()
        {
            var processID = Request["processID"];
            var processNodeID = Request["processNodeID"];
            var nodeLevel = Request["nodeLevel"];
            var nodeType = Request["nodeType"];
            var docId = Request["docId"];
            var docScope = Request["docScope"];
            Guid gid1, gid2; int nodelevel, iNodeType;
            Guid docguid;
            var falg = Guid.TryParse(processID, out gid1);
            var falg1 = Guid.TryParse(processNodeID, out gid2);
            var falg2 = int.TryParse(nodeLevel, out nodelevel);
            var falg3 = int.TryParse(nodeType, out iNodeType);
            var falg4 = Guid.TryParse(docId, out docguid);
            if (!falg || !falg1 || !falg2 || !falg3)
            {
                if (!falg4 || string.IsNullOrEmpty(docScope))
                {
                    return Json(new { Msg = "参数类型转换失败" });
                }
                else
                {
                    ResultFlowBoxShowModel rshowModel1 = new ResultFlowBoxShowModel();
                    rshowModel1.Url = docScope;
                    rshowModel1.DataId = docId;
                    rshowModel1.Msg = "";

                    rshowModel1.Common = "";
                    rshowModel1.LoadAppendInformations(OtherContext);
                    rshowModel1.IsProcess = false;
                    return Json(rshowModel1);
                }
            }
            Platform.Flow.Run.IRunWorkFlow irun = new Platform.Flow.Run.RunWorkFlow();
            var result = irun.GetViewFlowBox(new AgencyTask() { NodeLevel = nodelevel, ProcessNodeID = gid2, ProcessID = gid1, NodeType = iNodeType });
            //var context = new BusinessEdmxEntities();
            ResultFlowBoxShowModel rshowModel = new ResultFlowBoxShowModel();
            rshowModel.Url = result.Url;
            rshowModel.DataId = result.DataId;
            rshowModel.Msg = result.Msg;
            rshowModel.ProcessID = gid1;
            rshowModel.Common = result.Common;
            rshowModel.LoadAppendInformations(OtherContext);
            rshowModel.IsProcess = true;
            return Json(rshowModel);
        }
        #endregion
        #region 自动弹出页签权限验证
        public JsonResult TCAuth()
        {
            if (this.CurrentUserInfo == null) { return Json(new {sucess="0"}); }
            var operatorId = this.CurrentUserInfo.UserGuid;
            BusinessEdmxEntities bcontext = new BusinessEdmxEntities();
            var sql = string.Format(" select COUNT(*) from SS_RoleOperatorView where RoleKey='999' and GUID_Operator='{0}'", operatorId);
            var count = bcontext.ExecuteStoreQuery<int>(sql).FirstOrDefault();

             sql = string.Format(" select COUNT(*) from SS_RoleOperatorView where RoleKey='998' and GUID_Operator='{0}'", operatorId);
            var count1 = bcontext.ExecuteStoreQuery<int>(sql).FirstOrDefault();
            if (count > 0 && count1 > 0)
            {
                return Json(new { sucess = "3" });
            }
            if (count > 0 )
            {
                return Json(new { sucess = "1" });
            }
            if (count1 > 0)
            {
                return Json(new { sucess = "2" });
            }
          
            return Json(new { sucess = "0" });
        }
        #endregion


        /*检查单据是否能作废*/
        public JsonResult AbandonedDoc() { 
             var guid=Request["guid"];
            var id=Guid.Empty;
             if(!Guid.TryParse(guid,out id)){
                return Json(new {IsSuccess=true});
             }
             var bIsTrue=Platform.Flow.Run.WorkFlowAPI.ExistProcess(id);
             return Json(new { IsSuccess = bIsTrue });
        }
    }
    //增加待办所需字段
    public class FlowDataShowModel : AgencyTask
    {
        public string StrAcceptDate { get; set; }
        public string UserName { get; set; }//未预算

        public FlowDataShowModel(Guid processId, Guid processNodeId, int? nodeLevel, string nodeName, DateTime? acceptDate, int nodeType) :
            base(processId, processNodeId, nodeLevel, nodeName, acceptDate, nodeType)
        {
            StrAcceptDate = acceptDate != null ? acceptDate.Value.ToShortDateString() : "";
        }
        //todo
        public string BXUserName { get; set; }
        public string DocName { get; set; }
        public double? SumMoney { get; set; }
        public string DocNum { get; set; }

        //public 
        public void LoadAppendInformations(BaseConfigEdmxEntities IContext, BusinessEdmxEntities BusinessContext)
        {
            string url; Guid docId;
            var dataId = WorkFlowAPI.GetDocIdAndUrl(ProcessID, out docId, out url);
            var ent = GetDataFlowByDoc(docId, url, UserName, BusinessContext, IContext);
            if (ent != null)
            {
                BXUserName = ent.BXUserName;
                DocName = ent.DocName;
                DocNum = ent.DocNum;
                SumMoney = ent.SumMoney;
            }
        }
        private FlowData GetDataFlowByDoc(Guid docId, string url, string userName, BusinessEdmxEntities bContext, BaseConfigEdmxEntities iContext)
        {

            string YWTypeKey = string.Empty;
            string docTypeName = string.Empty;
            if (url == "hxcl")
            {
                YWTypeKey = "hxcl";
                docTypeName = "核销处理";
            }
            else if (url == "skpd")
            {
                YWTypeKey = "1112";
                docTypeName = "收款凭单";
            }
            else if (url == "gwkhzbxd")
            {
                YWTypeKey = "1111";
                docTypeName = "公务卡汇总报销单";
            }
            else if (url == "zplq")
            {
                url = "zpsld";
                var ssDoc = iContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl == url);
                if (ssDoc == null) return null;
                YWTypeKey = ssDoc.YWTypeKey;
                docTypeName = ssDoc.DocTypeName;
            }
            else if (url == "gzd") {
                YWTypeKey = "gzd";
                docTypeName ="工资单";
            }
            else
            {
                var ssDoc = iContext.SS_DocTypeView.FirstOrDefault(e => e.DocTypeUrl == url);
                if (ssDoc == null) return null;
                YWTypeKey = ssDoc.YWTypeKey;
                docTypeName = ssDoc.DocTypeName;
            }


            var flowData = new FlowData();
            switch (YWTypeKey)
            {
                case "hxcl":
                    flowData = GetDocInfoWithHXCL(bContext, docId);
                    break;
                case "gzd":
                    var dt = Business.Common.DataSource.ExecuteQuery("SELECT  SUM(ItemValue) FROM    dbo.SA_PlanActionDetail WHERE   GUID_PlanAction = '" + docId + "' AND GUID_Item IN ( SELECT   GUID FROM     dbo.SA_Item WHERE    ItemName = '实发合计' )");
                    var d = dt.Rows[0][0].ToString();
                    double dd = 0;
                    double.TryParse(d, out dd);
                    var m = bContext.SA_PlanActionView.FirstOrDefault(e => e.GUID == docId);
                    flowData.DocNum = m.DocNum;
                    flowData.BXUserName = m.Maker;
                    flowData.SumMoney = dd;
                    flowData.DocName = "工资单";

                    break;
                case "1111"://公务卡汇总报销单                    var entBxC = bContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == docId);
                    if (entBxC == null) return null;
                    var detailIds = bContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == docId).Select(e => e.GUID_BXDetail);
                    var jeBxC = bContext.BX_Detail.Where(e => detailIds.Contains(e.GUID)).Sum(e => e.Total_BX);
                    flowData.DocNum = entBxC.DocNum;
                    flowData.BXUserName = entBxC.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeBxC;
                    break;
                case "1112"://收款
                    var entSk = bContext.SK_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entSk == null) return null;
                    var jeSk = bContext.SK_MainView.Where(e => e.GUID == docId).Sum(e => e.Total_SK);
                    flowData.DocNum = entSk.DocNum;
                    flowData.BXUserName = entSk.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeSk;
                    break;
                case "05"://往来管理

                case "0501"://单位往来

                case "0502"://个人往来
                    var ent = bContext.WL_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (ent == null) return null;
                    var je = bContext.WL_Detail.Where(e => e.GUID_WL_Main == docId).Sum(e => e.Total_WL);
                    flowData.DocNum = ent.DocNum;
                    flowData.BXUserName = ent.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = je;
                    break;
                case "02"://报销
                    var entBx = bContext.BX_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entBx == null) return null;
                    var jeBx = bContext.BX_Detail.Where(e => e.GUID_BX_Main == docId).Sum(e => e.Total_BX);
                    flowData.DocNum = entBx.DocNum;
                    flowData.BXUserName = entBx.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeBx;
                    break;
                case "03"://收入
                    var entSr = bContext.SR_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entSr == null) return null;
                    var jeSr = bContext.SR_Detail.Where(e => e.GUID_SR_Main == docId).Sum(e => e.Total_SR);
                    flowData.DocNum = entSr.DocNum;
                    flowData.BXUserName = entSr.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeSr;
                    break;
                case "04"://基金
                    var entJj = bContext.JJ_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entJj == null) return null;
                    var jeJj = bContext.JJ_Detail.Where(e => e.GUID_JJ_Main == docId).Sum(e => e.Total_JJ);
                    flowData.DocNum = entJj.DocNum;
                    flowData.BXUserName = entJj.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeJj;
                    break;
                case "0801"://出纳
                    var entCn = bContext.CN_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entCn == null) return null;
                    var jeCn = bContext.CN_Detail.Where(e => e.GUID_CN_Main == docId).Sum(e => e.Total_CN);
                    flowData.DocNum = entCn.DocNum;
                    flowData.BXUserName = entCn.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeCn;
                    break;
                case "0802"://出纳
                    var entCnC = bContext.CN_CashMainView.Where(e => e.GUID == docId).FirstOrDefault();
                    if (entCnC == null) return null;
                    var jeCnC = bContext.CN_CashDetail.Where(e => e.GUID_CN_CashMain == docId).Sum(e => e.Total_Cash);
                    flowData.DocNum = entCnC.DocNum;
                    flowData.BXUserName = entCnC.PersonName;
                    flowData.DocName = docTypeName;
                    flowData.SumMoney = jeCnC;
                    break;

                case "01"://预算
                    try
                    {
                        if (url == "ysfp")
                        {
                            var entBG = bContext.BG_AssignView.Where(e => e.GUID == docId).FirstOrDefault();
                            if (entBG != null)
                            {
                                flowData.DocNum = entBG.DocNum;
                                flowData.BXUserName = userName;
                                flowData.DocName = docTypeName;
                                flowData.SumMoney = 0;
                                break;
                            }
                        }
                        else if (url == "ysbz")
                        {
                            var entBG1 = bContext.BG_MainView.Where(e => e.GUID == docId).FirstOrDefault();
                            if (entBG1 != null)
                            {
                                var jeBGDetail = bContext.BG_Main.Where(e => e.GUID == docId).Sum(e => e.Total_BG);
                                flowData.DocNum = entBG1.DocNum;
                                flowData.BXUserName = entBG1.PersonName;
                                flowData.DocName = docTypeName;
                                flowData.SumMoney = jeBGDetail;
                                break;
                            }
                            return flowData;
                        }
                        var entBG2 = bContext.BG_DefaultMainView.Where(e => e.GUID == docId).FirstOrDefault();
                        if (entBG2 != null)
                        {
                            var jeBGDetail = entBG2.Total_BG;
                            flowData.DocNum = entBG2.DocNum;
                            flowData.BXUserName = entBG2.PersonName;
                            flowData.DocName = docTypeName;
                            flowData.SumMoney = jeBGDetail;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    break;
                default:
                    break;
            }
            return flowData;


        }
        public FlowData GetDocInfoWithHXCL(BusinessEdmxEntities bContext, Guid docId)
        {
            var flowData = new FlowData();
            var entBxC = bContext.BX_CollectMainView.FirstOrDefault(e => e.GUID == docId);
            if (entBxC != null)
            {
                var detailIds = bContext.BX_CollectDetail.Where(e => e.GUID_BXCOLLECTMain == docId).Select(e => e.GUID_BXDetail);
                var jeBxC = bContext.BX_Detail.Where(e => detailIds.Contains(e.GUID)).Sum(e => e.Total_BX);
                flowData.DocNum = entBxC.DocNum;
                flowData.BXUserName = entBxC.PersonName;
                flowData.DocName = entBxC.DocTypeName;
                flowData.SumMoney = jeBxC;
                return flowData;
            }
            //收款
            var entSk = bContext.SK_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entSk != null)
            {
                var jeSk = bContext.SK_MainView.Where(e => e.GUID == docId).Sum(e => e.Total_SK);
                flowData.DocNum = entSk.DocNum;
                flowData.BXUserName = entSk.PersonName;
                flowData.DocName = entSk.DocTypeName;
                flowData.SumMoney = jeSk;
                return flowData;
            }

            var ent = bContext.WL_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (ent != null)
            {
                var je = bContext.WL_Detail.Where(e => e.GUID_WL_Main == docId).Sum(e => e.Total_WL);
                flowData.DocNum = ent.DocNum;
                flowData.BXUserName = ent.PersonName;
                flowData.DocName = ent.DocTypeName;
                flowData.SumMoney = je;
                return flowData;
            }

            var entBx = bContext.BX_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entBx != null)
            {
                var jeBx = bContext.BX_Detail.Where(e => e.GUID_BX_Main == docId).Sum(e => e.Total_BX);
                flowData.DocNum = entBx.DocNum;
                flowData.BXUserName = entBx.PersonName;
                flowData.DocName = entBx.DocTypeName;
                flowData.SumMoney = jeBx;
                return flowData;
            }
            var entSr = bContext.SR_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entSr == null) return null;
            var jeSr = bContext.SR_Detail.Where(e => e.GUID_SR_Main == docId).Sum(e => e.Total_SR);
            flowData.DocNum = entSr.DocNum;
            flowData.BXUserName = entSr.PersonName;
            flowData.DocName = entSr.DocTypeName;
            flowData.SumMoney = jeSr;

            var entJj = bContext.JJ_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entJj != null)
            {
                var jeJj = bContext.JJ_Detail.Where(e => e.GUID_JJ_Main == docId).Sum(e => e.Total_JJ);
                flowData.DocNum = entJj.DocNum;
                flowData.BXUserName = entJj.PersonName;
                flowData.DocName = entJj.DocTypeName; ;
                flowData.SumMoney = jeJj;
                return flowData;
            }
            var entCn = bContext.CN_MainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entCn != null)
            {
                var jeCn = bContext.CN_Detail.Where(e => e.GUID_CN_Main == docId).Sum(e => e.Total_CN);
                flowData.DocNum = entCn.DocNum;
                flowData.BXUserName = entCn.PersonName;
                flowData.DocName = entCn.DocTypeName;
                flowData.SumMoney = jeCn;
                return flowData;
            }

            var entCnC = bContext.CN_CashMainView.Where(e => e.GUID == docId).FirstOrDefault();
            if (entCnC != null)
            {
                var jeCnC = bContext.CN_CashDetail.Where(e => e.GUID_CN_CashMain == docId).Sum(e => e.Total_Cash);
                flowData.DocNum = entCnC.DocNum;
                flowData.BXUserName = entCnC.PersonName;
                flowData.DocName = entCnC.DocTypeName;
                flowData.SumMoney = jeCnC;
                return flowData;
            }
            return flowData;
        }
    }
    //点击待办返回的信息


    public class FlowData
    {
        public FlowData() { }
        public FlowData(string bxUserName, string docName, double sumMoney, string docNum)
        {
            this.BXUserName = bxUserName;
            this.DocName = docName;
            this.SumMoney = sumMoney;
            this.DocNum = docNum;
        }
        public string BXUserName { get; set; }
        public string DocName { get; set; }
        public double? SumMoney { get; set; }
        public string DocNum { get; set; }
    }

    public class ResultFlowBoxShowModel : ResultFlowBox
    {
        public ResultFlowBoxShowModel()
        {
            IsProcess = true;
        }
        public Guid ProcessID { get; set; }
        //todo
        public string MenuName { get; set; }
        public bool IsProcess { get; set; }
        //todo
        //单据的信息


        public string DepartmentName { get; set; }

        public virtual void LoadAppendInformations(ObjectContext BusinessContext)
        {
            //var bxMain = BusinessContext.CreateObjectSet<BX_Main>().FirstOrDefault(e => e.GUID == this.DataId);

            SS_Menu menuitem = BusinessContext.CreateObjectSet<SS_Menu>().FirstOrDefault(e => e.scope.ToLower() == this.Url.ToLower());
            if (menuitem != null) this.MenuName = menuitem.MenuName;
        }
    }
}
