using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Reflection;
using Infrastructure;
using BaothApp.Utils;
using Business.Common;
using Business.CommonModule;

namespace BaothApp.Controllers.Infrastructure
{
    public class CombogridController : SpecificController
    {
        IntrastructureFun dbobj = new IntrastructureFun();
        //改财政支付令 复选项目 2016 1 22
        public ContentResult ProjectViewFX(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            var sql = "  SELECT * FROM  SS_ProjcetExView  WHERE PGUID IS NULL AND IsFinance=1 AND IsStop=0 AND StopYear IS NULL ORDER BY ProjectKey";
            var leftprojects = dbobj.context.ExecuteStoreQuery<SS_ProjcetExView>(sql).ToList();
            var helper = new JsonGridHelper<SS_ProjcetExView>(leftprojects);
            return Content(helper.stringify(filter));
        }

        //根据人名 拿个人信息
        public JsonResult GetInfoByPersonName()
        {
            var name = Request["name"];
            using (var context = new BaseConfigEdmxEntities()) 
            {
                var persons = BaseTree.GetPersonView(name, this.CurrentUserInfo.UserGuid, context);
                return Json(persons, JsonRequestBehavior.AllowGet);
            }
           
        }
        public JsonResult GetTaxMoney() 
        {
            Guid g;
            Guid.TryParse(Request["Guid"]+"",out g);
            double d;
            double.TryParse(Request["Money"]+"",out d);
            using (var context = new BaseConfigEdmxEntities())
            {
                if (d == 0)
                {
                    return Json(new { tax = "0.00", sumReal = "0.00" }, JsonRequestBehavior.AllowGet);
                }
                var person = context.SS_Person.FirstOrDefault(e => e.GUID == g);
                if (person!=null&&person.IsTax == false)
                {
                    return Json(new { tax = "0.00", sumReal = "0.00" }, JsonRequestBehavior.AllowGet);
                }
                var tax = new Business.Casher.BillDoFax().GetPersonTax(context, d, g);
                return Json(new { tax = tax.ToString("0.00"), sumReal = (d + tax).ToString("0.00") }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetTaxMoneyY()
        {
            Guid g;
            Guid.TryParse(Request["Guid"] + "", out g);
            double d;
            double.TryParse(Request["Money"] + "", out d);
            using (var context = new BaseConfigEdmxEntities())
            {
                if (d == 0)
                {
                    return Json(new { tax = "0.00", sumReal = "0.00" }, JsonRequestBehavior.AllowGet);
                }
                var person = context.SS_Person.FirstOrDefault(e => e.GUID == g);
                if (person != null && person.IsTax == false)
                {
                    return Json(new {tax = "0.00", sumReal = "0.00" }, JsonRequestBehavior.AllowGet);
                }
                var tax = 0.00;//new Business.Casher.BillDoFax().GetPersonTax(context, d, g);
                return Json(new { isWP = true, tax = tax.ToString("0.00"), sumReal = (d + tax).ToString("0.00") }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetTravelAllowance()
        {
            var o = dbobj.RetrieveModels<SS_Allowance>(e => e.IsStop == false).OrderBy(e => e.AllowanceKey).Select(e => new
            {
                e.GUID,
                e.AllowanceKey,
                e.AllowanceName
            });
            return Json(o, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Person(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var o = BaseCombo.GetPersonCombo(true, operatorId);
            return Json(o, JsonRequestBehavior.AllowGet);
        }
        public JsonResult PersonGWK(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var o = BaseCombo.GetPersonCombo(false, operatorId);
            return Json(o, JsonRequestBehavior.AllowGet);
        }
        
        public ContentResult Operator(List<string> filter)
        {
            List<SS_Operator> ss_Operator = dbobj.GetJCOperator(false, "", false);
            JsonGridHelper<SS_Operator> helper = new JsonGridHelper<SS_Operator>(ss_Operator);
            return Content(helper.stringify(filter));
        }
        public JsonResult Department(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DepartmentView> deps = dbobj.GetDepartmentView(false, operatorId);
            //获得叶子节点
            List<SS_DepartmentView> leafDeps = new List<SS_DepartmentView>();
            foreach (SS_DepartmentView dep in deps)
            {
                if (!deps.Exists(e => e.PGUID == dep.GUID))
                {
                    leafDeps.Add(dep);
                }
            }
            return Json(leafDeps, JsonRequestBehavior.AllowGet);
        }
        public JsonResult Department2ForGCY()
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            using (var context= new BaseConfigEdmxEntities())
            {
               var deps= context.SS_DepartmentView.Where(e => e.IsStop == false && e.PGUID == null).OrderBy(e=>e.DepartmentKey).ToList();
               return Json(deps, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 基础--部门档案--Commbogrid
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult JCDepartment(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DepartmentView> deps = dbobj.GetJCDepartmentView(false, operatorId);   //ComboboxGrid下拉列表权限控制  true:有  false:无   --zzp--
            //获得叶子节点
            //List<SS_DepartmentView> leafDeps = new List<SS_DepartmentView>();
            //foreach (SS_DepartmentView dep in deps)
            //{
            //    if (!deps.Exists(e => e.PGUID == dep.GUID))
            //    {
            //        leafDeps.Add(dep);
            //    }
            //}

            return Json(deps, JsonRequestBehavior.AllowGet);

            //筛选

            //JsonGridHelper<SS_DepartmentView> helper = new JsonGridHelper<SS_DepartmentView>(deps);
            //return Json(helper.stringify(filter), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础--部门档案--Commbogrid 部门的叶子节点
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult JCLeafDepartment(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DepartmentView> deps = dbobj.GetJCDepartmentView(false, operatorId);   //ComboboxGrid下拉列表权限控制  true:有  false:无   --zzp--
           // 获得叶子节点
            List<SS_DepartmentView> leafDeps = new List<SS_DepartmentView>();
            foreach (SS_DepartmentView dep in deps)
            {
                if (!deps.Exists(e => e.PGUID == dep.GUID))
                {
                    leafDeps.Add(dep);
                }
            }
            return Json(leafDeps, JsonRequestBehavior.AllowGet);
            //JsonGridHelper<SS_DepartmentView> helper = new JsonGridHelper<SS_DepartmentView>(leafDeps);
           // return Json(helper.stringify(filter), JsonRequestBehavior.AllowGet);
        }

        public JsonResult FuncClass()
        {
            var o = BaseCombo.GetFunctionClass();
            return Json(o, JsonRequestBehavior.AllowGet);
        }
        public ContentResult DW(List<string> filter)
        {

            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DW> DWS = dbobj.GetDW(true, operatorId);
            ////获得叶子节点
            //List<SS_DW> leafDWs = new List<SS_DW>();

            //foreach (SS_DW dw in DWS)
            //{
            //    if (!DWS.Exists(e => e.PGUID == dw.GUID))
            //    {
            //        leafDWs.Add(dw);
            //    }
            //}
            JsonGridHelper<SS_DW> helper = new JsonGridHelper<SS_DW>(DWS);
            return Content(helper.stringify(filter));
        }
        /// <summary>
        /// 基础--上级单位的编码和名称--Combogrid
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp</returns>
        public ContentResult DWView(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DWView> DWS = dbobj.GetDWView(false, operatorId);    //ComboboxGrid列表权限控制  true:有  false:无  --zzp--
            ////获得叶子节点
            //List<SS_DW> leafDWs = new List<SS_DW>();

            //foreach (SS_DW dw in DWS)
            //{
            //    if (!DWS.Exists(e => e.PGUID == dw.GUID))
            //    {
            //        leafDWs.Add(dw);
            //    }
            //}
            JsonGridHelper<SS_DWView> helper = new JsonGridHelper<SS_DWView>(DWS);
            return Content(helper.stringify(filter));
        }

        /// <summary>
        /// 基础--上级单位的编码和名称--Combogrid 获取单位末级节点
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp</returns>
        public ContentResult DWLeafView(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DWView> DWS = dbobj.GetDWView(false, operatorId);    //ComboboxGrid列表权限控制  true:有  false:无  --zzp--
            //获得叶子节点
            List<SS_DWView> leafDWs = new List<SS_DWView>();

            foreach (SS_DWView dw in DWS)
            {
                if (!DWS.Exists(e => e.PGUID == dw.GUID))
                {
                    leafDWs.Add(dw);
                }
            }
            JsonGridHelper<SS_DWView> helper = new JsonGridHelper<SS_DWView>(leafDWs);
            return Content(helper.stringify(filter));
        }
        /// <summary>
        /// 工资数据加载类型
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult SalarySetUp(List<string> filter)
        {
            var o = BaseCombo.GetSalarySetUp();
            return Json(o, JsonRequestBehavior.AllowGet);
        }
        public ContentResult BGCode(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_BGCode> codes = dbobj.GetBgcode(true, operatorId);
            //获得叶子节点
            List<SS_BGCode> leafcodes = new List<SS_BGCode>();
            foreach (SS_BGCode code in codes)
            {
                if (!codes.Exists(e => e.PGUID == code.GUID))
                {
                    leafcodes.Add(code);
                }
            }
            JsonGridHelper<SS_BGCode> helper = new JsonGridHelper<SS_BGCode>(leafcodes);

            return Content(helper.stringify(filter));
        }

        /// <summary>
        /// 基础--上级科目的编码和名称--Combogrid
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp</returns>
        public ContentResult BGCodeView(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_BGCodeView> codes = dbobj.GetBgcodeView(true, operatorId);
            //获得叶子节点
            List<SS_BGCodeView> leafcodes = new List<SS_BGCodeView>();
            foreach (SS_BGCodeView code in codes)
            {
                if (!codes.Exists(e => e.PGUID == code.GUID))
                {
                    leafcodes.Add(code);
                }
            }
            JsonGridHelper<SS_BGCodeView> helper = new JsonGridHelper<SS_BGCodeView>(leafcodes);

            return Content(helper.stringify(filter));
        }



        public JsonResult BGCodeMemo()
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var codeMemos = dbobj.GetBgcodeMemo(true, operatorId);

            return Json(codeMemos, JsonRequestBehavior.AllowGet);
        }

        #region 基础    项目分类

        public ContentResult ProjectClass(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_ProjectClass> projects = dbobj.GetProjectClass(false, operatorId);
            ////获得叶子节点
            //List<SS_ProjectClass> leftprojects = new List<SS_ProjectClass>();
            //foreach (SS_ProjectClass code in projects)
            //{
            //    if (!projects.Exists(e => e.PGUID == code.GUID))
            //    {
            //        leftprojects.Add(code);
            //    }
            //}
            JsonGridHelper<SS_ProjectClass> helper = new JsonGridHelper<SS_ProjectClass>(projects);

            return Content(helper.stringify(filter));
        }

        #endregion

        #region 基础    项目档案

        public ContentResult ProjectJC(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Project> project = dbobj.GetProject(false, operatorId);
            JsonGridHelper<SS_Project> helper = new JsonGridHelper<SS_Project>(project);

            return Content(helper.stringify(filter));
        }

        public JsonResult ProjectJCEx(List<string> filter)
        {
            var projects = dbobj.context.SS_ProjectView.OrderBy(e => e.ProjectKey).ToList();


            return Json(projects, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 基础----薪酬设置--工资项目设置--combogrid
        /// <summary>
        /// 工资计划区间
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp  2014/05/25  15:40</returns>
        public JsonResult PlanArea(List<string> filter)
        {
            var result = dbobj.context.SA_PlanArea.Select(e => new { e.GUID, e.PlanAreaName, e.PlanAreaKey, }).OrderBy(e => e.PlanAreaKey).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ContentResult Project(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Project> projects = dbobj.GetProject(true, operatorId);


            //获得叶子节点
            List<SS_Project> leftprojects = new List<SS_Project>();
            foreach (SS_Project code in projects)
            {
                if (!projects.Exists(e => e.PGUID == code.GUID))
                {
                    leftprojects.Add(code);
                }
            }
            JsonGridHelper<SS_Project> helper = new JsonGridHelper<SS_Project>(leftprojects);

            return Content(helper.stringify(filter));
        }
        public ContentResult ProjectEx(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_ProjcetExView> projects = dbobj.GetProjectEx(true, operatorId);
            //获得叶子节点
            List<SS_ProjcetExView> leftprojects = new List<SS_ProjcetExView>();
            foreach (SS_ProjcetExView code in projects)
            {
                if (!projects.Exists(e => e.PGUID == code.GUID))
                {

                    leftprojects.Add(code);
                }
            }
            JsonGridHelper<SS_ProjcetExView> helper = new JsonGridHelper<SS_ProjcetExView>(leftprojects);

            return Content(helper.stringify(filter));
        }
        public ContentResult ProjectView(List<string> filter)
        {
            if (this.CurrentUserInfo == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_ProjcetExView> projects = dbobj.GetProjectEx(false, operatorId);
            //获得叶子节点
            List<SS_ProjcetExView> leftprojects = new List<SS_ProjcetExView>();
            foreach (SS_ProjcetExView code in projects)
            {
                if (!projects.Exists(e => e.PGUID == code.GUID))
                {

                    leftprojects.Add(code);
                }
            }
            JsonGridHelper<SS_ProjcetExView> helper = new JsonGridHelper<SS_ProjcetExView>(leftprojects);

            return Content(helper.stringify(filter));
        }
        private string ProjectJson(List<SS_ProjectView> list)
        {
            StringBuilder sb = new StringBuilder();
            var permodel = typeof(SS_DocType).Name;
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    sb.Append("{");
                    sb.AppendFormat("\"{0}\":\"{1}\",", "GUID", list[j].GUID);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectKey", CommonFuntion.StringToJson(list[j].ProjectKey));
                    sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectName", CommonFuntion.StringToJson(list[j].ProjectName));
                    sb.AppendFormat("\"{0}\":\"{1}\",", "ProjectClassName", CommonFuntion.StringToJson(list[j].ProjectClassName));
                    sb.AppendFormat("\"{0}\":\"{1}\",", "IsFinance", list[j].IsFinance);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "FinanceCode", list[j].FinanceCode);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "ExtraCode", list[j].ExtraCode);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "GUID_FunctionClass", list[j].GUID_FunctionClass);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "FunctionClassName", list[j].FunctionClassName);
                    sb.AppendFormat("\"{0}\":\"{1}\",", "IsProject", list[j].IsProject);
                    sb.AppendFormat("\"{0}\":\"{1}\" ", "Year", list[j].BeginYear + "-" + (list[j].StopYear == null ? "至今" : list[j].StopYear.ToString()));
                    sb.Append("},");
                    if (j == list.Count - 1)
                    {
                        sb.Remove(sb.ToString().LastIndexOf(","), 1);
                    }
                }
            }
            return sb.ToString();
        }
        public ContentResult SettleType(List<string> filter)
        {
            List<SS_SettleType> SettleTypes = dbobj.GetSettleType(false, "", false);

            JsonGridHelper<SS_SettleType> helper = new JsonGridHelper<SS_SettleType>(SettleTypes);

            return Content(helper.stringify(filter));
        }
        public ContentResult BgSource(List<string> filter)
        {
            List<SS_BGSource> SettleTypes = dbobj.GetBGSource(true, "", false);

            JsonGridHelper<SS_BGSource> helper = new JsonGridHelper<SS_BGSource>(SettleTypes);

            return Content(helper.stringify(filter));
        }

        public ContentResult BGType(List<string> filter)
        {
            List<object> ipView = dbobj.context.BG_Type.Select(e => new { e.GUID, e.BGTypeKey, e.BGTypeName, }).OrderBy(e => e.BGTypeKey).ToList<object>();
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(ipView);
            string json = JsonHelper.PageJsonFormat(rowJson, ipView.Count);
            return Content(json);
        }

        // 东升添加 2014-4-9
        public ContentResult BGStepView(List<string> filter)
        {
            List<object> ipView = dbobj.context.BG_Step.Select(e => new { e.GUID, e.BGStepKey, e.BGStepName, }).OrderBy(e => e.BGStepKey).ToList<object>();
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(ipView);
            string json = JsonHelper.PageJsonFormat(rowJson, ipView.Count);
            return Content(json);
        }

        // 东升添加 201-4-10
        public ContentResult BGStepUpView(List<string> filter)
        {
            List<BG_SetupView> bgSetUps = dbobj.GetBGSetUpView(false, "");
            JsonGridHelper<BG_SetupView> helper = new JsonGridHelper<BG_SetupView>(bgSetUps);
            return Content(helper.stringify(filter));
        }

        public JsonResult FunctionClass()
        {
            var fcs = dbobj.context.SS_FunctionClass.OrderBy(e => e.FunctionClassKey).Select(e => new
            {
                e.FunctionClassKey,
                e.FunctionClassName,
                GUID = e.GUID,
                IsProject = e.IsProject,
                e.FinanceCode,
                IsFinance = true
            }).ToList();
            return Json(fcs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ExpendType(List<string> filter)
        {
            List<object> fcs = dbobj.context.SS_ExpendType.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.ExpendTypeName, e.ExpendTypeKey }).OrderBy(e => e.ExpendTypeKey).ToList<object>();

            return Json(fcs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EconomyClass(List<string> filter)
        {
            List<object> fcs = dbobj.context.SS_EconomyClass.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.EconomyClassKey, e.EconomyClassName }).OrderBy(e => e.EconomyClassKey).ToList<object>();
            return Json(fcs, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 证件类型
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult CredentialType(List<string> filter)
        {
            List<object> fcs = dbobj.context.SS_CredentialType.Select(e => new { e.GUID, e.CredentialTypekey, e.CredentialTypeName }).OrderBy(e => e.CredentialTypekey).ToList<object>();
            return Json(fcs, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 外聘人员
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ContentResult InvitePerson(List<string> filter)
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<InvitePersonTreeModel> ipView = BaseTree.GetPersonView(true, operatorId);
            //List<object> ipView = dbobj.context.SS_InvitePersonView.Select(e => new { e.GUID, e.InvitePersonName, e.InvitePersonIDCard, e.CredentialTypekey, e.CredentialTypeName }).OrderBy(e => e.CredentialTypekey).ToList<object>();
            // JsonGridHelper<object> helper = new JsonGridHelper<object>(ipView);                  
            //var rowJson = helper.stringify(filter);  
            var rowJson = BaothApp.Utils.JsonHelper.objectToJson(ipView);
            string json = JsonHelper.PageJsonFormat(rowJson, ipView.Count);
            return Content(json);
        }
        /// <summary>
        /// 人员和外聘人员


        /// </summary>
        /// <returns></returns>
        public ContentResult _PersonInvitePerson()
        {
            var json = "";
            return Content(json);
        }
        /// <summary>
        /// 客户信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult Customer(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_Customer.Where(e => e.IsCustomer != null).Select(e => new { e.GUID, e.CustomerName, e.CustomerKey, e.CustomerAddress, e.CustomerBankName, e.CustomerBankNumber }).OrderBy(e => e.CustomerKey).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 收款类型
        /// </summary>
        /// <param name="filter">条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult SKType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_SKType.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.SKTypeName, e.SKTypeKey }).OrderBy(e => e.SKTypeKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 收入类型
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult SRType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_SRType.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.SRTypeName, e.SRTypeKey }).OrderBy(e => e.SRTypeKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础收入类型-包含所有节点
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseSRType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_SRTypeView.Select(e => new { e.GUID, e.SRTypeName, e.SRTypeKey,e.PKey,e.PName,e.IsStop }).OrderBy(e => e.SRTypeKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础收入类型-只包含所有末级节点
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseSRTatialType(List<string> filter)
        {
            var listGuids = (from s1 in dbobj.context.SS_SRType
                             join s2 in dbobj.context.SS_SRType on s1.GUID equals s2.PGUID
                             select new
                             {
                                 GUID = s1.GUID
                             }).Select(e => e.GUID).Distinct();
            var q = (from s in dbobj.context.SS_SRType
                     where !listGuids.Contains(s.GUID) && s.IsStop == false
                     select new
                     {
                         GUID = s.GUID,
                         SRTypeKey = s.SRTypeKey,
                         SRTypeName = s.SRTypeName
                     }).OrderBy(e => e.SRTypeKey).ToList();

            return Json(q, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 往来类型

        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult WLType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_WLType.Where(e => e.IsStop == false || e.IsStop == null).Select(e => new { e.GUID, e.WLTypeName, e.WLTypeKey }).OrderBy(e => e.WLTypeKey).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 基础往来类型
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseWLType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_WLTypeView.Select(e => new { e.GUID, e.WLTypeName, e.WLTypeKey, e.PKey, e.PName, e.IsDC, e.IsCustomer, e.IsStop }).OrderBy(e => e.WLTypeKey).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 基础收款类型设置
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseSKWLType(List<string> filter)
        {
            var listGuids = (from s1 in dbobj.context.SS_WLType
                             join s2 in dbobj.context.SS_WLType on s1.GUID equals s2.PGUID
                             select new
                             {
                                 GUID = s1.GUID
                             }).Select(e => e.GUID).Distinct();
            var q = (from s in dbobj.context.SS_WLType
                     where !listGuids.Contains(s.GUID) && (s.IsStop == false ||s.IsStop==null)
                     select new
                     {
                         GUID = s.GUID,
                         WLTypeKey = s.WLTypeKey,
                         WLTypeName = s.WLTypeName
                     }).OrderBy(e => e.WLTypeKey).ToList();

            return Json(q, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础菜单
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseMenu(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_MenuView.Select(e => new { e.GUID, e.MenuName, e.MenuKey, e.PKey, e.PName }).OrderBy(e => e.MenuKey).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BaseDocType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_DocTypeView.Select(e => new
            {
                e.GUID,
                e.DocTypeKey,
                e.DocTypeName,
                DocTypeUrl = (e.DocTypeUrl).ToLower() == "null" ? "" : e.DocTypeUrl
            }).OrderBy(e => e.DocTypeKey
                ).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 基础菜单分类
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BaseClassMenu(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_MenuClass.Select(e => new { e.GUID, e.MenuClassKey, e.MenuClassName, }).OrderBy(e => e.MenuClassKey).ToList<object>();

            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        
        /// <summary>
        /// 交通工具




        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult Traffic(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_TrafficView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.TrafficName, e.TrafficKey }).OrderBy(e => e.TrafficKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 银行账户信息
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult BankAccount(List<string> filter)
        {
            var ipView = dbobj.context.SS_BankAccountView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.BankAccountName, e.BankAccountNo, e.BankAccountKey }).OrderBy(e => e.BankAccountKey).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 银行名称
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult Bank(List<string> filter)
        {
            var ipView = dbobj.context.SS_Bank.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.BankKey, e.BankName }).OrderBy(e => e.BankKey).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Check(List<string> filter)
        {
            var ipView = dbobj.context.CN_CheckView.Where(e => e.IsInvalid == false).Select(e => new { e.GUID, e.BankAccountNo, e.GUID_BankAccount, e.BankAccountName, e.CheckNumber }).OrderBy(e => e.CheckNumber).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 基础银行名称
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult BaseBank(List<string> filter)
        {
            var ipView = dbobj.context.SS_Bank.Select(e => new { e.GUID, e.BankKey, e.BankName }).OrderBy(e => e.BankKey).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 省份
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult Province(List<string> filter)
        {
            var ipView = dbobj.context.SS_Province.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.ProvinceKey, e.ProvinceName }).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础省份
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult BaseProvince(List<string> filter)
        {
            var ipView = dbobj.context.SS_Province.Select(e => new { e.GUID, e.ProvinceKey, e.ProvinceName }).ToList();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }

        // 货币单位 东升添加 2014-4-11
        public ContentResult MoneyUnit(List<string> filter)
        {
            List<SS_MoneyUnit> listMoneyUnit = dbobj.GetSS_MoneyUnit();
            JsonGridHelper<SS_MoneyUnit> jHelper = new JsonGridHelper<SS_MoneyUnit>(listMoneyUnit);
            return Content(jHelper.stringify(filter));
        }

        // 预算控制-处理方式  东升添加 2014-5-19
        public ContentResult HandleMethod(List<string> filter)
        {
            List<BG_HandleMethod> listBG_HandleMethod = dbobj.context.BG_HandleMethod.OrderBy(e => e.HandleKey).ToList();
            JsonGridHelper<BG_HandleMethod> jHelper = new JsonGridHelper<BG_HandleMethod>(listBG_HandleMethod);
            return Content(jHelper.stringify(filter));
        }

        // 节点的状态可以修改，但是StateKey 的编号不要修改，程序根据这个值进行条件筛选，如果增加 StateKey ，那么要程序
        // 目前，预算分配的历史在进行查询时会使用到这个函数
        public ContentResult FlowNodeState()
        {
            List<object> result = new List<object>();

            string str4 = "{\"StateKey\":\"0\",\"StateName\":\"全部\"}";
            object obj4 = JsonHelp.JsonToObject<object>(str4);

            string str = "{\"StateKey\":\"1\",\"StateName\":\"未到达\"}";
            object obj1 = JsonHelp.JsonToObject<object>(str);

            str = "{\"StateKey\":\"2\",\"StateName\":\"处理中\"}";
            object obj2 = JsonHelp.JsonToObject<object>(str);

            str = "{\"StateKey\":\"3\",\"StateName\":\"已通过\"}";
            object obj3 = JsonHelp.JsonToObject<object>(str);
            result.Add(obj4);
            result.Add(obj1);
            result.Add(obj2);
            result.Add(obj3);

            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }

        // 预算控制界面加载预算项


        public ContentResult BGItemForYSKZ(List<string> filter)
        {
            var BG_SetupKey = Request["BG_SetupKey"];
            BG_Setup objBG_Setup = dbobj.context.BG_Setup.FirstOrDefault(e => e.BGSetupKey == BG_SetupKey);
            List<BG_SetupDetailView> viewDetailList = dbobj.context.BG_SetupDetailView.Where(e => e.GUID_BGSetup == objBG_Setup.GUID).OrderBy(e => e.ItemOrder).ToList();
            JsonGridHelper<BG_SetupDetailView> jHelper = new JsonGridHelper<BG_SetupDetailView>(viewDetailList);
            return Content(jHelper.stringify(filter));
        }
        /// <summary>
        /// 预算设置
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult BGSetUp(List<string> filter)
        {
            List<object> ipView = dbobj.context.BG_SetupView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.BGSetupName, e.BGSetupKey }).OrderBy(e => e.BGSetupKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 专用基金类型
        /// </summary>
        /// <param name="filter"></param>
        /// /// <returns></returns>
        public JsonResult JJType(List<string> filter)
        {
            List<object> ipView = dbobj.context.SS_JJTypeView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.JJTypeName, e.JJTypeKey }).OrderBy(e => e.JJTypeKey).ToList<object>();
            return Json(ipView, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 会计科目
        /// </summary>
        /// <returns></returns>
        public JsonResult CWAcountTitle()
        {
            var guid = Request["guid"];
            Guid g = Guid.Empty;
            if (CommonFuntion.IsGUID(guid))
            {
                g = CommonFuntion.ConvertGUID(guid);
            }
            else
            {
                g = Guid.Empty;
            }

            var key = Request["key"];



            if (!string.IsNullOrEmpty(key))
            {
                var temp = dbobj.context.CW_AccountTitleView.Where(e => (e.IsStop == false || e.IsStop == null) && e.AccountKey == key).Select(e => new { e.GUID, e.GUID_AccountMain, e.AccountTitleName, e.AccountTitleKey });
                return Json(temp.ToList(), JsonRequestBehavior.AllowGet);
            }
            else if (!g.IsNullOrEmpty())
            {
                var temp = dbobj.context.CW_AccountTitleView.Where(e => (e.IsStop == false || e.IsStop == null) && e.GUID_AccountMain == g).Select(e => new { e.GUID, e.GUID_AccountMain, e.AccountTitleName, e.AccountTitleKey });
                return Json(temp.ToList(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var temp = dbobj.context.CW_AccountTitleView.Where(e => e.IsStop == false || e.IsStop == null).Select(e => new { e.GUID, e.GUID_AccountMain, e.AccountTitleName, e.AccountTitleKey });
                return Json(temp.ToList(), JsonRequestBehavior.AllowGet);
            }
            
            //if (!string.IsNullOrEmpty(guid))
            //{
            //    var result = temp.AsEnumerable().Where(e => e.GUID_AccountMain == g).Select(e => new { e.GUID, e.GUID_AccountMain, e.AccountTitleName, e.AccountTitleKey });
            //    return Json(result.ToList(), JsonRequestBehavior.AllowGet);
            //}
            //if (!string.IsNullOrEmpty(key))
            //{
            //    var result = temp.AsEnumerable().Where(e => e == key).Select(e => new { e.GUID, e.GUID_AccountMain, e.AccountTitleName, e.AccountTitleKey });
            //}

            //return Json(temp.ToList(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 工资计划
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult SAPlan()
        {
            var ipView = dbobj.context.SA_PlanView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.PlanKey, e.PlanName, e.PlanAreaName }).OrderBy(e => e.PlanKey).ToList();
            //var rowJson = BaothApp.Utils.JsonHelper.objectToJson(ipView);
            //string json = JsonHelper.PageJsonFormat(rowJson, ipView.Count);
            return Json(ipView,JsonRequestBehavior.AllowGet);
        }
        public JsonResult SAPlanEx()
        {

            var ipView = dbobj.context.SA_PlanView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.PlanKey, e.PlanName, e.PlanAreaName }).OrderBy(e => e.PlanKey).ToList();
            //var rowJson = BaothApp.Utils.JsonHelper.objectToJson(ipView);
            //string json = JsonHelper.PageJsonFormat(rowJson, ipView.Count);
            return Json(ipView, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 工资数据加载类型
        /// </summary>
        /// <returns></returns>
        public JsonResult SA_SetUp()
        {
            var obj = dbobj.context.SA_SetUp.Select(e => new { e.GUID, e.SetUpKey, e.SetUpName }).OrderBy(e => e.SetUpKey).ToList();
            return Json(obj);
        }


        public JsonResult GetAccountMain()
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var objs = context.AccountMainViews.OrderBy(e=>e.AccountKey);
            return Json(objs, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAccountDetail()
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var objs = context.AccountDetailViews.OrderBy(e => e.FiscalYear).ThenBy(e=>e.accountkey);
            return Json(objs, JsonRequestBehavior.AllowGet);
        }

        public class mo
        {
            public string GUID { get; set; }

        }
    }
    public class ComboController : Controller
    {
        BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();

        public JsonResult Operator()
        {
            var result = context.SS_Operator.Select(e => new { e.GUID, e.OperatorName, e.OperatorKey }).OrderBy(e => e.OperatorKey).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 出差事由
        /// </summary>
        /// <param name="filter">筛选字段条件</param>
        /// <returns>ContentResult</returns>
        public JsonResult BgcodeMemo(List<string> filter)
        {
            var rId = Request["bgId"];
            Guid bgId = new Guid();
            if (rId != null)
            {
                if (rId.Length != 0 || rId != "")
                {
                    bgId = new Guid(rId);
                    var result2 = context.SS_BGCodeMemoView.Where(e => e.GUID_BGCode == bgId).Select(e => new { e.GUID, e.BGCodeMemo, e.BGCodeKey }).OrderBy(e => e.BGCodeKey).ToList();
                    return Json(result2, JsonRequestBehavior.AllowGet);
                }
            }

            Guid bgcodeId = context.SS_BGCode.Where(e => e.BGCodeName == "差旅费").FirstOrDefault().GUID;
            if (bgcodeId == null || bgcodeId == Guid.Empty)
            {
                return null;
            }

            var result = context.SS_BGCodeMemoView.Where(e => e.GUID_BGCode == bgcodeId).Select(e => new { e.GUID, e.BGCodeMemo, e.BGCodeKey }).OrderBy(e => e.BGCodeKey).ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        ///单据类型或根据业务类型查找单据类型




        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult DocType()
        {
            var guid = Request["ywguid"];
            var isHaveAll = Request["isNotAll"] + ""; ;
            Guid g;
            List<object> List = new List<object>();
            List<object> result = new List<object>();
            SS_DocTypeView model = new SS_DocTypeView();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                model.GUID = Guid.Empty;
                model.DocTypeName = "全部";
                model.DocTypeKey = "00";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            if (guid != null && Guid.TryParse(guid, out g))
            {
                result = context.SS_DocTypeView.Where(e => e.GUID_YWType == g && e.IsStop == false).Select(e => new { e.GUID, e.DocTypeName, e.YWTypeKey, e.DocTypeKey, e.GUID_YWType }).Distinct().OrderBy(e => e.DocTypeKey).Distinct().ToList<object>();
            }
            else
            {
                List<string> filterList = new List<string>();
                filterList.Add(Constant.YWTwo);//“02”报销管理
                filterList.Add(Constant.YWThree);//“03”收入管理




                filterList.Add(Constant.YWFour);//"04"专用基金
                //filterList.Add(Constant.YWFive);//"05"往来管理



                filterList.Add(Constant.YWFiveO);
                filterList.Add(Constant.YWFiveT);

                filterList.Add(Constant.YWEightO);//"0801"收付款管理




                filterList.Add(Constant.YWEightT);//"0802"提存现管理




                filterList.Add(Constant.YWElevenO);//"1101"收入信息流转

                result = context.SS_DocTypeView.OrderBy(e => e.DocTypeKey).Where(e => e.IsStop == false && filterList.Contains(e.YWTypeKey)).Select(e => new { e.GUID, e.DocTypeName, e.DocTypeKey, e.YWTypeKey, e.GUID_YWType }).ToList<object>().Distinct<object>().ToList();
            }
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 单据类型或根据业务类型查找单据类型(专为审批->待办任务)
        /// </summary>
        /// <returns></returns>
        public JsonResult DocTypeDBRW()
        {
            var guid = Request["ywguid"];
            var isHaveAll = Request["isNotAll"] + ""; ;
            Guid g;
            List<object> List = new List<object>();
            List<object> result = new List<object>();
            SS_DocTypeView model = new SS_DocTypeView();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                model.GUID = Guid.Empty;
                model.DocTypeName = "全部";
                model.DocTypeKey = "00";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            if (guid != null && Guid.TryParse(guid, out g))
            {
                result = context.SS_DocTypeView.Where(e => e.GUID_YWType == g && e.IsStop == false).Select(e => new { e.GUID, e.DocTypeName, e.YWTypeKey, e.DocTypeKey, e.GUID_YWType }).Distinct().OrderBy(e => e.DocTypeKey).Distinct().ToList<object>();
            }
            else
            {
                List<string> filterList = new List<string>();
                filterList.Add(Constant.YWOne);//"01" 预算管理
                filterList.Add(Constant.YWTwo);//“02”报销管理
                filterList.Add(Constant.YWThree);//“03”收入管理





                filterList.Add(Constant.YWFour);//"04"专用基金
                //filterList.Add(Constant.YWFive);//"05"往来管理




                filterList.Add(Constant.YWFiveO);
                filterList.Add(Constant.YWFiveT);

                filterList.Add(Constant.YWEightO);//"0801"收付款管理





                filterList.Add(Constant.YWEightT);//"0802"提存现管理





                filterList.Add(Constant.YWElevenO);//"1101"收入信息流转

                result = context.SS_DocTypeView.OrderBy(e => e.DocTypeKey).Where(e => e.IsStop == false && filterList.Contains(e.YWTypeKey)).Select(e => new { e.GUID, e.DocTypeName, e.DocTypeKey, e.YWTypeKey, e.GUID_YWType }).ToList<object>().Distinct<object>().ToList();
            }
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
       

        /// <summary>
        /// 报销单据类型
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult BXDocType(List<string> filter)
        {
            // string[] strArr = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "24", "35", "39" };
            //List<string> filterList = strArr.ToList();
            var ywKey = Constant.YWTwo;//报销管理
            List<object> List = new List<object>();
            SS_DocTypeView model = new SS_DocTypeView();
            model.GUID = Guid.Empty;
            model.DocTypeName = "全部";
            model.DocTypeKey = "00";
            model.YWTypeKey = "00";
            List.Add(model);
            var result = context.SS_DocTypeView.Where(e => e.YWTypeKey == ywKey && e.IsStop == false).Select(e => new { e.GUID, e.DocTypeName, e.YWTypeKey, e.GUID_YWType,e.DocTypeKey }).OrderBy(e => e.DocTypeKey).ToList<object>();
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 业务类型
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult YWType(List<string> filter)
        {
            var isHaveAll = Request["isNotAll"] + ""; ;
            List<string> filterList = new List<string>();
            filterList.Add(Constant.YWTwo);//“02”报销管理
            filterList.Add(Constant.YWThree);//“03”收入管理




            filterList.Add(Constant.YWFour);//"04"专用基金
            filterList.Add(Constant.YWFive);//"05"往来管理




            filterList.Add(Constant.YWEightO);//"0801"收付款管理




            filterList.Add(Constant.YWEightT);//"0802"提存现管理




            filterList.Add(Constant.YWElevenO);//"1101"收入信息流转
            List<object> List = new List<object>();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                SS_YWType model = new SS_YWType();
                model.GUID = Guid.Empty;
                model.YWTypeName = "全部";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            var result = context.SS_YWType.Where(e => filterList.Contains(e.YWTypeKey) && e.IsStop == false).Select(e => new { e.GUID, e.YWTypeName, e.YWTypeKey }).OrderBy(e => e.YWTypeKey).ToList<object>();
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 业务类型(专为审批->待办任务)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult YWTypeDBRW(List<string> filter)
        {
            var isHaveAll = Request["isNotAll"] + ""; ;
            List<string> filterList = new List<string>();
            filterList.Add(Constant.YWOne);//"01" 预算管理
            filterList.Add(Constant.YWTwo);//“02”报销管理
            filterList.Add(Constant.YWThree);//“03”收入管理





            filterList.Add(Constant.YWFour);//"04"专用基金
            filterList.Add(Constant.YWFive);//"05"往来管理





            filterList.Add(Constant.YWEightO);//"0801"收付款管理





            filterList.Add(Constant.YWEightT);//"0802"提存现管理





            filterList.Add(Constant.YWElevenO);//"1101"收入信息流转
            List<object> List = new List<object>();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                SS_YWType model = new SS_YWType();
                model.GUID = Guid.Empty;
                model.YWTypeName = "全部";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            var result = context.SS_YWType.Where(e => filterList.Contains(e.YWTypeKey) && e.IsStop == false).Select(e => new { e.GUID, e.YWTypeName, e.YWTypeKey }).OrderBy(e => e.YWTypeKey).ToList<object>();
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 业务类型
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public JsonResult YWTypeAll(List<string> filter)
        {
            List<object> List = new List<object>();
            SS_YWType model = new SS_YWType();
            model.GUID = Guid.Empty;
            model.YWTypeName = "全部";
            model.YWTypeKey = "00";
            List.Add(model);
            var result = context.SS_YWType.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.YWTypeName, e.YWTypeKey }).OrderBy(e => e.YWTypeKey).ToList<object>();
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }

        #region 基础----薪酬设置--工资项目设置--combobox
        /// <summary>
        /// 工资项目类型--Combobox
        /// 暂且没有用到
        /// </summary>
        /// <returns></returns>
        public JsonResult ItemArea()
        {
            List<object> List = new List<object>();
            SA_Item model = new SA_Item();
            model.GUID = Guid.Empty;
            List.Add(model);
            var result = context.SA_Item.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.ItemType }).OrderBy(e => e.ItemType).ToList<object>();
            List.AddRange(result);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        #endregion



        /// <summary>
        /// 凭证类型
        /// </summary>
        /// <returns></returns>
        public JsonResult CWPZType()
        {
            List<object> list = new List<object>();
            CW_PZType model = new CW_PZType();
            model.GUID = Guid.Empty;
            model.PZTypeName = "全部";
            model.PZTypeKey = "00";
            list.Add(model);

            var result = context.CW_PZTypeView.Where(e => e.IsStop == false).Select(e => new { e.GUID, e.PZTypeName, e.PZTypeKey }).ToList<object>();
            list.AddRange(result);
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 凭证帐套
        /// </summary>
        /// <returns></returns>
        public JsonResult AccountMain()
        {
            string year = Request["year"];
            int iyear;
            if (!string.IsNullOrEmpty(year))
            {
                if (int.TryParse(year, out iyear))
                {
                    var detail = context.AccountDetailViews.Where(e => e.FiscalYear == iyear).Select(e => e.GUID_AccountMain);
                    var result1 = context.AccountMainViews.Where(e => detail.Contains(e.GUID)).Select(e => new { e.GUID, e.AccountKey }).ToList<object>();
                    return Json(result1, JsonRequestBehavior.AllowGet);
                }
            }
            var result = context.AccountMains.Select(e => new { e.GUID, e.AccountKey }).ToList<object>();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础----帐套设置--Combogrid
        /// </summary>
        /// <returns></returns>
        public JsonResult JCAccountMain()
        {
            //string year = Request["year"];
            //int iyear;
            //if (!string.IsNullOrEmpty(year))
            //{
            //    if (int.TryParse(year, out iyear))
            //    {
            //        var detail = context.AccountDetailViews.Where(e => e.FiscalYear == iyear).Select(e => e.GUID_AccountMain);
            //        var result1 = context.AccountMainViews.Where(e => detail.Contains(e.GUID)).Select(e => new { e.GUID, e.AccountKey }).ToList<object>();
            //        return Json(result1, JsonRequestBehavior.AllowGet);
            //    }
            //}
            var result = context.AccountMains.Select(e => new { e.GUID, e.AccountKey,e.AccountName,e.Description }).OrderBy(e=>e.AccountKey).ToList<object>();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 基础----人员类别----Combobox
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp    04-24</returns>
        public JsonResult JCPersonType(List<string> filter)
        {
            List<string> filterList = new List<string>();
            filterList.Add(Constant.PersonTypeOne);     //"01"在编
            filterList.Add(Constant.PersonTypeTwo);     //"02"离职
            filterList.Add(Constant.PersonTypeThree);   //"03"离退休


            filterList.Add(Constant.PersonTypeFour);    //"04"外聘
            filterList.Add(Constant.PersonTypeFive);    //"05"其他

            List<object> ObjectList = new List<object>();
            var result = context.SS_PersonType.Where(e => filterList.Contains(e.PersonTypeKey) && e.IsStop == 0).Select(e => new { e.GUID, e.PersonTypeName, e.PersonTypeKey }).OrderBy(e => e.PersonTypeKey).ToList<object>();
            ObjectList.AddRange(result);
            return Json(ObjectList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 基础----证件类型----Combobox
        /// </summary>
        /// <param name="filter"></param>
        /// <returns>zzp  2014/04/24  11:58</returns>
        public JsonResult JCIDCardType(List<string> filter)
        {
            List<object> objectList = new List<object>();
            var result = context.SS_CredentialType.Select(e => new { e.GUID, e.CredentialTypeName, e.CredentialTypekey }).OrderBy(e => e.CredentialTypekey).ToList<object>();
            objectList.AddRange(result);
            return Json(objectList, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据计划获取期间个数
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPeriodByPlanID()
        { 
            var strId=Request["guid"];
            List<ComboModel> comboList = new List<ComboModel>();
            Guid gId;
            Guid.TryParse(strId,out gId);
            var strYear=Request["year"];
            int year = DateTime.Now.Year;
            if (!string.IsNullOrEmpty(strYear))
            {
                int.TryParse(strYear, out year);
            }
            var strMonth=Request["month"];
            int month = DateTime.Now.Month;
            if (!string.IsNullOrEmpty(strMonth))
            {
                int.TryParse(strMonth, out month);
            }

            var result = context.SA_PlanView.FirstOrDefault(e => e.GUID == gId);
            if (result != null)
            {
                switch (result.PlanAreaName)
                { 
                    case"年":
                    case"季":
                    case"月":
                        comboList = GetPeriodCount(1);
                        break;
                    case"旬":
                        comboList = GetPeriodCount(3);
                        break;
                    case "周":
                        comboList = GetPeriodCount(5);
                        break;
                    case "日":
                        int days = DateTime.DaysInMonth(year,month);
                        comboList = GetPeriodCount(days);
                        break;
                }
            }
            return Json(comboList,JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 期间的个数

        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private List<ComboModel> GetPeriodCount(int count)
        {
            List<ComboModel> list = new List<ComboModel>();
            for (int i =1; i <=count; i++)
            {
                ComboModel model = new ComboModel();
                model.ID = i.ToString();
                model.Text = i.ToString();
                list.Add(model);
            }
            return list;
        }
        public ContentResult GetCheckListNotUseed()
        {
            var guidBankAccount = Request["bankAccountID"];
            var guid = Guid.Empty;
            Guid.TryParse(guidBankAccount, out guid);
            var dt = CommonBusinessSelect.GetCheckTable1(guid, " and CheckType= 0 ");
            var strContext = BaothApp.Utils.JsonHelper.DataTableToJson(dt);
            return Content(strContext);
        }
        //转账  支票领取
        public ContentResult GetCheckListNotUseed1()
        {
            var guidBankAccount = Request["bankAccountID"];
            var guid = Guid.Empty;
            Guid.TryParse(guidBankAccount, out guid);
            var dt = CommonBusinessSelect.GetCheckTable1(guid, " and CheckType<> 0 ");
            var strContext = BaothApp.Utils.JsonHelper.DataTableToJson(dt);
            return Content(strContext);
        }
        //支票领取选单
       // /Combo/DocType
        public JsonResult DocTypeZP() {
            var isHaveAll = Request["isNotAll"] + ""; ;
            Guid g;
            List<object> List = new List<object>();
            List<object> result = new List<object>();
            SS_DocTypeView model = new SS_DocTypeView();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                model.GUID = Guid.Empty;
                model.DocTypeName = "全部";
                model.DocTypeKey = "00";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            List<string> filterList = new List<string>();
            filterList.Add(Constant.YWTwo);//“02”报销管理
            filterList.Add(Constant.YWThree);//“03”收入管理
            filterList.Add(Constant.YWFour);//"04"专用基金
            //filterList.Add(Constant.YWFive);//"05"往来管理
            filterList.Add(Constant.YWFiveO);
            filterList.Add(Constant.YWFiveT);
            filterList.Add(Constant.YWEightO);//"0801"收付款管理
            filterList.Add(Constant.YWEightT);//"0802"提存现管理
            filterList.Add(Constant.YWElevenO);//"1101"收入信息流转
            result = context.SS_DocTypeView.OrderBy(e => e.DocTypeKey).Where(e => e.IsStop == false && e.DocTypeKey != "24" && filterList.Contains(e.YWTypeKey)).Select(e => new { e.GUID, e.DocTypeName, e.DocTypeKey, e.YWTypeKey, e.GUID_YWType }).ToList<object>().Distinct<object>().ToList();
            List.AddRange(result);
            model = new SS_DocTypeView();
            model.GUID =new Guid("2DB5261F-F817-4B8A-8B4D-AD70A8199952");
            model.DocTypeName = "公务卡汇总报销单";
            model.DocTypeKey = "24";
            model.YWTypeKey = "76";
            List.Add(model);
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        public JsonResult YWTypeZP(List<string> filter)
        {
            var isHaveAll = Request["isNotAll"] + ""; ;
            List<string> filterList = new List<string>();
            filterList.Add(Constant.YWTwo);//“02”报销管理
            filterList.Add(Constant.YWThree);//“03”收入管理





            filterList.Add(Constant.YWFour);//"04"专用基金
            filterList.Add(Constant.YWFive);//"05"往来管理





            filterList.Add(Constant.YWEightO);//"0801"收付款管理





            filterList.Add(Constant.YWEightT);//"0802"提存现管理





            filterList.Add(Constant.YWElevenO);//"1101"收入信息流转
            List<object> List = new List<object>();
            if (string.IsNullOrEmpty(isHaveAll))
            {
                SS_YWType model = new SS_YWType();
                model.GUID = Guid.Empty;
                model.YWTypeName = "全部";
                model.YWTypeKey = "00";
                List.Add(model);
            }
            var result = context.SS_YWType.Where(e => filterList.Contains(e.YWTypeKey) && e.IsStop == false).Select(e => new { e.GUID, e.YWTypeName, e.YWTypeKey }).OrderBy(e => e.YWTypeKey).ToList<object>();
            List.AddRange(result);

            SS_YWType model1 = new SS_YWType();
            model1.GUID =Guid.NewGuid ();
            model1.YWTypeName = "公务卡汇总";
            model1.YWTypeKey = "76";
            List.Add(model1);
            return Json(List, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UfFitem()
        {
            var extdb = Request["extdb"];
            var cyear = Request["year"];
            var context = new BusinessModel.BusinessEdmxEntities();
            var sql = string.Format("select cAcc_Id as ID,cDatabase as Text from UFSystem..ua_accountdatabase where cAcc_Id='{0}' and (iBeginYear>={1} and (iEndYear is null or iEndYear<={1}))"
                , extdb, cyear);

            var db = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
            if (db != null && db.Count > 0)
            {
                var dbname = db[0].Text;
                sql=string.Format("select ctable as ID,citem_name as Text from {0}..fitem",dbname);
                var fitems = context.ExecuteStoreQuery<ComboModel>(sql).ToList();
                return Json(fitems, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
    }
    public class ComboModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { set; get; }
        /// <summary>
        /// Text
        /// </summary>
        public string Text { set; get; }
    }

    
}
