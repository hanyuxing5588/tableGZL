using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Infrastructure;
using Business.CommonModule;

namespace BaothApp.Controllers.Infrastructure
{
    public class GridController : SpecificController //为什么继承基类
    {
        IntrastructureFun dbobj = new IntrastructureFun();
        //
        // GET: /Grid/

        public ActionResult Index()
        {
            return View();
        }
        #region 暂时无用
        /// <summary>
        /// 获取人Json List数据
        /// </summary>
        public ContentResult GetPersonList()
        {
            StringBuilder sb = new StringBuilder();
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            sb.Append("[");
            List<SS_Person> pesonlist = dbobj.GetPerson(true, operatorId);
            if (pesonlist != null)
            {
                for (int i = 0; i < pesonlist.Count; i++)
                {
                    sb.Append(JsonPerson(pesonlist[i]));
                    if (i == pesonlist.Count - 1)
                    {
                        sb.Remove(sb.ToString().LastIndexOf(","), 1);
                    }
                }
            }
            sb.Append("]");
           // this.WriteJsonMessage(sb.ToString());
            //return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 人员Json数据
        /// </summary>
        /// <param name="personModel">实体类</param>
        /// <returns>string</returns>
        private string JsonPerson(SS_Person personModel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", personModel.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "PersonName", CommonFuntion.StringToJson(personModel.PersonName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_DW", CommonFuntion.StringToJson(personModel.GUID_DW.ToString()));
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWName", CommonFuntion.StringToJson(personModel.SS_DW.DWName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWKey", CommonFuntion.StringToJson(personModel.SS_DW.DWKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_Department", CommonFuntion.StringToJson(personModel.GUID_Department.ToString()));
            sb.AppendFormat("\"{0}\":\"{1}\",", "DepartmentName", CommonFuntion.StringToJson(personModel.SS_Department.DepartmentName));
            sb.AppendFormat("\"{0}\":\"{1}\" ", "DepartmentKey", CommonFuntion.StringToJson(personModel.SS_Department.DepartmentKey));
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        ///获取单位
        /// </summary>
        public ContentResult GetDWList()
        {
            StringBuilder sb = new StringBuilder();
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            sb.Append("[");
            var dwList = dbobj.GetDW(true,operatorId);
            if (dwList != null)
            {
                for (int i = 0; i < dwList.Count; i++)
                {
                    sb.Append(JsonDW(dwList[i]));
                    if (i == dwList.Count - 1)
                    {
                        sb.Remove(sb.ToString().LastIndexOf(","), 1);
                    }
                }
            }
            sb.Append("]");
            //this.WriteJsonMessage(sb.ToString());
            //return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 获取单位Json
        /// </summary>
        /// <param name="dwPersonModel"></param>
        /// <returns></returns>
        private string JsonDW(SS_DW dwPersonModel)
        {
            
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", dwPersonModel.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWName", CommonFuntion.StringToJson(dwPersonModel.DWName));
            sb.AppendFormat("\"{0}\":\"{1}\" ", "DWKey", CommonFuntion.StringToJson(dwPersonModel.DWKey));
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        /// 获取部门Json
        /// </summary>
        public ContentResult GetDepartmentList()
        {
            StringBuilder sb = new StringBuilder();
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            sb.Append("[");
            var depList = dbobj.GetDepartment(true,operatorId);
            if (depList != null)
            {
                for (int i = 0; i < depList.Count; i++)
                {
                    sb.Append(JsonDepartment(depList[i]));
                    if (i == depList.Count - 1)
                    {
                        sb.Remove(sb.ToString().LastIndexOf(","), 1);
                    }
                }
            }
            sb.Append("]");
           // this.WriteJsonMessage(sb.ToString());
           // return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 转换部门Json数据
        /// </summary>
        /// <param name="dwPersonModel"></param>
        /// <returns></returns>
        private string JsonDepartment(SS_Department dwPersonModel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", dwPersonModel.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "DepartmentName", CommonFuntion.StringToJson(dwPersonModel.DepartmentName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "DepartmentKey", CommonFuntion.StringToJson(dwPersonModel.DepartmentKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_DW", dwPersonModel.GUID_DW);
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWName", CommonFuntion.StringToJson(dwPersonModel.SS_DW.DWName));
            sb.AppendFormat("\"{0}\":\"{1}\" ", "DWKey", CommonFuntion.StringToJson(dwPersonModel.SS_DW.DWKey));
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        /// 获取科目信息
        /// </summary>
        public ContentResult GetBgCodeTree()
        {
             if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var bgcodeList = dbobj.GetBgcode(true,operatorId);
            for (int i = 0; i < bgcodeList.Count; i++)
            {
                sb.Append(JsonBgCode(bgcodeList[i]));
                if (i == bgcodeList.Count - 1)
                {
                    sb.Remove(sb.ToString().LastIndexOf(","), 1);
                }
            }
            sb.Append("]");
            //this.WriteJsonMessage(sb.ToString());
            //return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 科目实体转换成Json数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string JsonBgCode(SS_BGCode model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", model.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "BGCodeKey", CommonFuntion.StringToJson(model.BGCodeKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "BGCodeName", CommonFuntion.StringToJson(model.BGCodeName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "PGUID", model.PGUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_EconomyClass", CommonFuntion.StringToJson(model.GUID_EconomyClass.ToString()));
            sb.AppendFormat("\"{0}\":\"{1}\",", "BeginYear", CommonFuntion.StringToJson(model.BeginYear.ToString()));
            sb.AppendFormat("\"{0}\":\"{1}\",", "IsStop", model.IsStop);
            sb.AppendFormat("\"{0}\":\"{1}\" ", "StopYear", model.StopYear);
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        /// 获取项目List数据
        /// </summary>
        public ContentResult GetProjectList()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            StringBuilder sb = new StringBuilder();
            var proList = dbobj.GetProject(true,operatorId);
            for (int i = 0; i < proList.Count; i++)
            {
                sb.Append(JsonProject(proList[i]));
                if (i == proList.Count - 1)
                {
                    sb.Remove(sb.ToString().LastIndexOf(","), 1);
                }
            }
           // this.WriteJsonMessage(sb.ToString());
           // return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 项目实体类转化成Json数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string JsonProject(SS_Project model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", model.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectKey", CommonFuntion.StringToJson(model.ProjectKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectName", CommonFuntion.StringToJson(model.ProjectName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "PGUID", model.PGUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_ProjectClass", model.GUID_ProjectClass);
            sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectClassName", CommonFuntion.StringToJson(model.SS_ProjectClass.ProjectClassName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectClassKey", CommonFuntion.StringToJson(model.SS_ProjectClass.ProjectClassKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_FunctionClass", model.GUID_FunctionClass);
            sb.AppendFormat("\"{0}\":\"{1}\",", "FinanceProjectKey", CommonFuntion.StringToJson(model.FinanceProjectKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "IsFinance", model.IsFinance);
            sb.AppendFormat("\"{0}\":\"{1}\",", "IsBalanced", model.IsBalanced);
            sb.AppendFormat("\"{0}\":\"{1}\",", "BeginYear", model.BeginYear);
            sb.AppendFormat("\"{0}\":\"{1}\",", "StopYear", model.StopYear);
            sb.AppendFormat("\"{0}\":\"{1}\",", "IsStop", model.IsStop);
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_DW", model.GUID_DW);
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWKey", CommonFuntion.StringToJson(model.SS_DW.DWKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "DWName", CommonFuntion.StringToJson(model.SS_DW.DWName));
            sb.AppendFormat("\"{0}\":\"{1}\"", "ExtraCode", CommonFuntion.StringToJson(model.ExtraCode));
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        /// 获取货品数据
        /// </summary>
        public ContentResult GetGoods()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var goodsList = dbobj.GetGoods(false, "", false);
            for (int i = 0; i < goodsList.Count; i++)
            {
                sb.Append(JsonGoods(goodsList[i]));
                if (i == goodsList.Count - 1)
                {
                    sb.Remove(sb.ToString().LastIndexOf(","), 1);
                }
            }
            sb.Append("]");
           // this.WriteJsonMessage(sb.ToString());
            //return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 货品实体类转化成Json数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string JsonGoods(SS_Goods model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", model.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "GoodsKey", CommonFuntion.StringToJson(model.GoodsKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GoodsName", CommonFuntion.StringToJson(model.GoodsName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_GoodsUnit", model.GUID_GoodsUnit);
            sb.AppendFormat("\"{0}\":\"{1}\"", "GUID_GoodsType", model.GUID_GoodsType);
            //sb.AppendFormat("\"{0}\":\"{1}\",", "GoodsSize", CommonFuntion.StringToJson(model.GoodsSize));
            //sb.AppendFormat("\"{0}\":\"{1}\" ", "IsStop", model.IsStop);
            sb.Append("},");
            return sb.ToString();
        }
        /// <summary>
        /// 获取往来单位

        /// </summary>
        public ContentResult GetCustomerList()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            var customerList = dbobj.GetCustomer(false, "");
            for (int i = 0; i < customerList.Count; i++)
            {
                if (i == customerList.Count - 1)
                {
                    sb.Remove(sb.ToString().LastIndexOf(","), 1);
                }
            }
            sb.Append("]");
            //this.WriteJsonMessage(sb.ToString());
            //return View(sb.ToString());
            //return new JsonResult() { Data = sb.ToString(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            return Content(sb.ToString());
        }
        /// <summary>
        /// 来往单位客户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string JonsCustomer(SS_Customer model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", model.GUID);
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerKey", CommonFuntion.StringToJson(model.CustomerKey));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerName", CommonFuntion.StringToJson(model.CustomerName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerAddress", CommonFuntion.StringToJson(model.CustomerAddress));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerBankName", CommonFuntion.StringToJson(model.CustomerBankName));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerBankNumber", CommonFuntion.StringToJson(model.CustomerBankNumber));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerTelephone", CommonFuntion.StringToJson(model.CustomerTelephone));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerFax", CommonFuntion.StringToJson(model.CustomerFax));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerPostcode", CommonFuntion.StringToJson(model.CustomerPostcode));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerEmail", CommonFuntion.StringToJson(model.CustomerEmail));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerWebsite", CommonFuntion.StringToJson(model.CustomerWebsite));
            sb.AppendFormat("\"{0}\":\"{1}\",", "CustomerLikeMan", CommonFuntion.StringToJson(model.CustomerLikeMan));
            sb.AppendFormat("\"{0}\":\"{1}\",", "IsCustomer", model.IsCustomer);
            sb.AppendFormat("\"{0}\":\"{1}\" ", "IsVendor", model.IsVendor);
            sb.Append("},");
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 支票列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCheckList()
        {
            List<object> list = new List<object>();
            var ordertype = Request["ordertype"];
            if (string.IsNullOrEmpty(ordertype)) ordertype = "asc";
            var Asc = ordertype == "asc" ? true : false;
            var guidBankAccount = Request["bankAccountID"];
            var DocTypeKey = Request["DocTypeKey"];
            var checkNumber = Request["checkNumber"] + "" ;
            var isInvalid = (Request["isInvalid"]+"").Trim()=="1"?true:false;
            var guid = Guid.Empty;
            if (!string.IsNullOrEmpty(guidBankAccount))
            {
                guid = ConvertGuid(guidBankAccount);
            }
            list = CommonBusinessSelect.GetCheckList(guid, checkNumber, DocTypeKey, isInvalid, Asc);
            var  list1 = list.Distinct();
            return Json(list1, JsonRequestBehavior.AllowGet);
        }
        //提现单和存现单的待选支票号只用到现金支票，不用显示转账支票号
        public ContentResult GetCheckListNotUseed()
        {
            var guidBankAccount = Request["bankAccountID"];
            var checkguid = Request["checkguid"];
            var guid = Guid.Empty;
            Guid.TryParse(guidBankAccount, out guid);
            var dt = CommonBusinessSelect.GetCheckTable(guid,checkguid, " and CheckType= 0 and IsInvalid<>1  ");
             var strContext=BaothApp.Utils.JsonHelper.DataTableToJson(dt);
            return Content(strContext);
        }
        //转账  支票领取
        public ContentResult GetCheckListNotUseed1()
        {
            var guidBankAccount = Request["bankAccountID"];
            var guid = Guid.Empty;
            Guid.TryParse(guidBankAccount, out guid);
            var dt = CommonBusinessSelect.GetCheckTable(guid, " and CheckType<> 0  and IsInvalid<>1 ");
            var strContext = BaothApp.Utils.JsonHelper.DataTableToJson(dt);
            return Content(strContext);
        }

        public JsonResult GetU8Project()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("select * from {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetU8Project1()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("Select cVenCode as citemcode ,cVenName as citemname  from   {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetU8Project2()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("Select cCusCode as citemcode ,cCusName as citemname  from   {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetU8Project3()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("Select cPersonCode as citemcode ,cPersonName as citemname  from   {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetU8Project4()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("Select cDepCode as citemcode ,cDepName as citemname  from   {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetU8Project5()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var prjtablename = Request["prj"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql = string.Format("Select ccode as citemcode ,ccode_name as citemname  from   {0}..{1}", dbname, prjtablename);
                var fitems = context.ExecuteStoreQuery<U8ProjectModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }

    public class U8ProjectModel
    {
        public int l_id { get; set; }
        public string citemcode { get; set; }
        public string citemname { get; set; }
        public bool? bclose { get; set; }
        public string citemccode { get; set; }
        public int? iotherused { get; set; }
        public DateTime? dEndDate { get; set; }
    }
}
