using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public class TaxMoney
    {
        public double SumTotalReal { get; set; }
        public double SumTotalTax { get; set; }
        public double SumTotalBx { get; set; }
        public Guid guid_bx_main { get; set; }
    }
    public static class BaseTree
    {
        public static List<SS_FunctionClassView> GetFunctionClassWithProject() 
        {
            var  context = new BaseConfigEdmxEntities();
            return context.SS_FunctionClassView.Where(e => e.IsProject == true).OrderBy(e=>e.FunctionClassKey).ToList();
        }
        public static List<InvitePersonTreeModelEx> GetPersonView(string personName, Guid operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            int classid = CommonFuntion.GetClassId(context, typeof(SS_Person).Name);
            //获取人员权限信息
            var Persons = context.SS_PersonView.Where(e => e.PersonName == personName).Select(e => new InvitePersonTreeModelEx
            {
                GUID = e.GUID,
                InvitePersonName = e.PersonName,
                InvitePersonIDCard = e.IDCard,
                CredentialTypeName = e.CredentialTypeName,
                CredentialTypekey = e.IDCardType,
                IsUnit = true,
                GUID_Department = e.GUID_Department,
                SumTotalReal = 0,
                SumTotalTax = 0,
                DepartmentName = e.DepartmentName
            }).ToList();
            var listGuid=Persons.Select(e=>e.GUID);
            var InvitePersons = context.SS_InvitePersonView.Where(e => !listGuid.Contains(e.GUID) && e.InvitePersonName == personName).Select(e => new InvitePersonTreeModelEx
            {
                GUID = e.GUID,
                InvitePersonName = e.InvitePersonName,
                InvitePersonIDCard = e.InvitePersonIDCard,
                CredentialTypeName = e.CredentialTypeName,
                CredentialTypekey = e.CredentialTypekey,
                IsUnit = false,
                GUID_Department = null,
                SumTotalReal = 0,
                SumTotalTax=0,
                DepartmentName = ""
            }).ToList();
            /*算税和，总计*/
            Persons.AddRange(InvitePersons);
            //foreach (var item in Persons)
            //{
            //    if (item.IsUnit)
            //    {
            //        string sql = GetInviteSql(item.GUID);
            //        var tm = context.ExecuteStoreQuery<TaxMoney>(sql).FirstOrDefault();
            //        item.SumTotalReal = item.SumTotalReal;
            //        item.SumTotalTax = item.SumTotalTax;
            //    }
            //}
            return Persons;
        }
       
        /// <summary>
        /// 给外聘人员的树 提供的人员的列表
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>    
        /// <returns></returns>
        public static List<InvitePersonTreeModel> GetPersonView(bool filterAuth, string operatorGuid,BaseConfigEdmxEntities context=null)
        {
            if (context == null) {
                context = new BaseConfigEdmxEntities();
            }
            List<InvitePersonTreeModel> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(context, typeof(SS_Person).Name);
                //获取人员权限信息
                var c = from q1 in context.SS_Person select q1.GUID;
                var q = context.SS_PersonView.Select(e => new InvitePersonTreeModel
                {
                    GUID = e.GUID,
                    InvitePersonName = e.PersonName,
                    InvitePersonIDCard = e.IDCard,
                    CredentialTypeName = e.CredentialTypeName,
                    CredentialTypekey = e.IDCardType,
                    IsUnit = true,
                    GUID_Department=e.GUID_Department,
                    DepartmentName=e.DepartmentName
                }).Union(context.SS_InvitePersonView.Where(e => !c.Contains(e.GUID)).Select(e => new InvitePersonTreeModel
                {
                    GUID = e.GUID,
                    InvitePersonName = e.InvitePersonName,
                    InvitePersonIDCard = e.InvitePersonIDCard,
                    CredentialTypeName = e.CredentialTypeName,
                    CredentialTypekey = e.CredentialTypekey,
                    IsUnit = false,
                    GUID_Department=null,
                    DepartmentName =""
                }));
             
                list = q.OrderBy(e => e.InvitePersonName).ToList();
                list = FilterPerson(list);
                return  list;
            }
            else
            {
                list = context.SS_PersonView.Select(e => new InvitePersonTreeModel
                {
                    GUID = e.GUID,
                    InvitePersonName = e.PersonName,
                    InvitePersonIDCard = e.IDCard,
                    CredentialTypeName = e.CredentialTypeName,
                    CredentialTypekey = e.IDCardType,
                    IsUnit = true,
                    GUID_Department=e.GUID_Department,
                    DepartmentName =""
                }).OrderBy(e => e.InvitePersonName).ToList();

            }

            return list;
        }
        public static List<InvitePersonTreeModel> FilterPerson(List<InvitePersonTreeModel> list) 
        {
            if (list == null) return null;
            List<InvitePersonTreeModel> listTemp = new List<InvitePersonTreeModel>();
            foreach (var item in list)
            {
                if (listTemp.FirstOrDefault(e=>e.GUID==item.GUID)==null) {
                    listTemp.Add(item);
                }
            }
            return listTemp;
        }
        /// <summary>
        /// 根据权限获取人员信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>    
        /// <param name="departmentID">根据部门ID</param>
        /// <returns>SS_Person List</returns>
        public static List<PersonTreeModel> GetPerson(bool filterAuth, string operatorGuid, Guid departmentID, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<PersonTreeModel> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid); //人员"3"
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_Person.Include("SS_Department").Include("SS_DW")
                        where a.GUID_Department == departmentID && qdataset.Contains(a.GUID)
                        orderby a.PersonName
                        select new PersonTreeModel
                        {
                            GUID = a.GUID,
                            GUID_DW = a.GUID_DW,
                            DWName = a.SS_DW.DWName,
                            GUID_Department = a.GUID_Department,
                            DepartmentName = a.SS_Department.DepartmentName,
                            OfficialCard = a.OfficialCard,
                            PersonName = a.PersonName,
                            PersonKey = a.PersonKey,
                            BankCardNo=a.BankCardNo


                        };
                list = q.ToList();
            }
            else
            {
                var person = context.SS_Person.Include("SS_Department").Include("SS_DW").Where(e => e.GUID_Department == departmentID)
                    .OrderBy(a => a.PersonName).Select(a => new PersonTreeModel
                    {
                        GUID = a.GUID,
                        GUID_DW = a.GUID_DW,
                        DWName = a.SS_DW.DWName,
                        GUID_Department = a.GUID_Department,
                        DepartmentName = a.SS_Department.DepartmentName,
                        OfficialCard = a.OfficialCard,
                        PersonName = a.PersonName,
                        PersonKey = a.PersonKey,
                        BankCardNo = a.BankCardNo
                    }).ToList();
                list = person;
            }

            return list;
        }
        /// <summary>
        /// 根据权限获取人员信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>    
        /// <param name="departmentID">根据部门ID</param>
        /// <returns>SS_Person List</returns>
        public static List<PersonTreeModel> GetPerson(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<PersonTreeModel> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid); //人员"3"
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_Person.Include("SS_Department").Include("SS_DW")
                        where  qdataset.Contains(a.GUID)
                        orderby a.PersonName
                        select new PersonTreeModel
                        {
                            GUID = a.GUID,
                            GUID_DW = a.GUID_DW,
                            DWName = a.SS_DW.DWName,
                            GUID_Department = a.GUID_Department,
                            DepartmentName = a.SS_Department.DepartmentName,
                            OfficialCard = a.OfficialCard,
                            PersonName = a.PersonName,
                            PersonKey = a.PersonKey,
                            BankCardNo = a.BankCardNo

                        };
                list = q.ToList();
            }
            else
            {
                var person = context.SS_Person.Include("SS_Department").Include("SS_DW")
                    .OrderBy(a => a.PersonName).Select(a => new PersonTreeModel
                    {
                        GUID = a.GUID,
                        GUID_DW = a.GUID_DW,
                        DWName = a.SS_DW.DWName,
                        GUID_Department = a.GUID_Department,
                        DepartmentName = a.SS_Department.DepartmentName,
                        OfficialCard = a.OfficialCard,
                        PersonName = a.PersonName,
                        PersonKey = a.PersonKey,
                        BankCardNo = a.BankCardNo
                    }).ToList();
                list = person;
            }

            return list;
        }
        /// <summary>
        /// 获取科目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public static List<BGCodeTreeModel> GetBgcode(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<BGCodeTreeModel> list = new List<BGCodeTreeModel>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_BGCode).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_BGCodeView.Where(e => qdataset.Contains(e.GUID) && e.IsStop == false).Select(e => new BGCodeTreeModel
                {
                    GUID = e.GUID,
                    PGUID = e.PGUID,
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    GUID_EconomyClass = e.GUID_EconomyClass,
                    EconomyClassKey=e.EconomyClassKey
                }).OrderBy(e => e.BGCodeKey).ToList();
            }
            else
            {
                list = context.SS_BGCodeView.Where(e => e.IsStop == false).Select(e => new BGCodeTreeModel
                {
                    GUID = e.GUID,
                    PGUID = e.PGUID,
                    BGCodeName = e.BGCodeName,
                    BGCodeKey = e.BGCodeKey,
                    GUID_EconomyClass = e.GUID_EconomyClass,
                    EconomyClassKey = e.EconomyClassKey
                }).OrderBy(e => e.BGCodeKey).ToList();
            }

            return list;
        }
     
        /// <summary>
        /// 获取功能分类信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public static List<SS_FunctionClassView> GetFunctionClass(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<SS_FunctionClassView> list = new List<SS_FunctionClassView>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_FunctionClassView).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_FunctionClassView.Where(e => qdataset.Contains(e.GUID)).OrderBy(e=>e.FunctionClassKey).ToList();
            }
            else
            {

                list = context.SS_FunctionClassView.OrderBy(e => e.FunctionClassKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取收入类型信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public static List<SS_SRTypeView> GetSRType(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<SS_SRTypeView> list = new List<SS_SRTypeView>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_SRTypeView).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_SRTypeView.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.SRTypeKey).ToList();
            }
            else
            {

                list = context.SS_SRTypeView.OrderBy(e => e.SRTypeKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取往来类型信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public static List<SS_WLTypeView> GetSS_WLType(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<SS_WLTypeView> list = new List<SS_WLTypeView>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_WLTypeView).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_WLTypeView.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.WLTypeKey).ToList();
            }
            else
            {

                list = context.SS_WLTypeView.OrderBy(e => e.WLTypeKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取所有菜单项
        /// </summary>
        /// <param name="context">数据库上下文对象</param>
        /// <returns>返回所有菜单的集合</returns>
        public static List<SS_MenuView> GetMenus(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_MenuView.OrderBy(e => e.MenuKey).ToList();
            return list;
        }
        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>        
        /// <returns></returns>
        public static List<ProjectTreeModel> GetProject(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<ProjectTreeModel> list = new List<ProjectTreeModel>();
            //如果要过滤年 加年条件限制
            //select * from ss_projectclass where BeginYear<=Year(getdate()) and isnull(StopYear,Year(getdate()))>=Year(getdate())
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectView.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));  
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                if (qdataset == null) return null;               
                list = q.Where(e => qdataset.Contains(e.GUID)).Select(e => new ProjectTreeModel
                {
                    GUID = e.GUID,
                    PGUID = e.PGUID,
                    ProjectName = e.ProjectName,
                    ProjectKey = e.ProjectKey,
                    GUID_FunctionClass = e.GUID_FunctionClass,
                    GUID_ProjectClass = e.GUID_ProjectClass,
                    IsFinance = e.IsFinance,
                    ExtraCode=e.ExtraCode,
                    FinanceCode=e.FinanceCode
                }).OrderBy(e => e.ProjectKey).ToList();

            }
            else
            {
                list = q.Select(e => new ProjectTreeModel
                {
                    GUID = e.GUID,
                    PGUID = e.PGUID,
                    ProjectName = e.ProjectName,
                    ProjectKey = e.ProjectKey,
                    GUID_FunctionClass = e.GUID_FunctionClass,
                    GUID_ProjectClass = e.GUID_ProjectClass,
                    IsFinance = e.IsFinance,
                    ExtraCode = e.ExtraCode,
                    FinanceCode = e.FinanceCode
                }).OrderBy(e => e.ProjectKey).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param> 
       /// <param name="year">年份</param>
       /// <returns></returns>
        public static List<ProjectTreeModel> GetProject(bool filterAuth, string operatorGuid, int year, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<ProjectTreeModel> list = new List<ProjectTreeModel>();
            //如果要过滤年 加年条件限制
            //select * from ss_projectclass where BeginYear<=Year(getdate()) and isnull(StopYear,Year(getdate()))>=Year(getdate())
            if (year != 0)
            {
                var q = context.SS_Project.Include("SS_ProjectClass").Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));                
                    if (filterAuth)
                    {
                        int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                        var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid);
                        if (qdataset == null) return null;
                        list = q.Where(e => qdataset.Contains(e.GUID)).Select(e => new ProjectTreeModel
                        {
                            GUID = e.GUID,
                            PGUID = e.PGUID,
                            ProjectName = e.ProjectName,
                            ProjectKey = e.ProjectKey,
                            GUID_FunctionClass = e.GUID_FunctionClass,
                            GUID_ProjectClass = e.GUID_ProjectClass,
                            IsFinance = e.IsFinance
                        }).OrderBy(e => e.ProjectKey).ToList();

                    }
                    else
                    {
                        list = q.Select(e => new ProjectTreeModel
                        {
                            GUID = e.GUID,
                            PGUID = e.PGUID,
                            ProjectName = e.ProjectName,
                            ProjectKey = e.ProjectKey,
                            GUID_FunctionClass = e.GUID_FunctionClass,
                            GUID_ProjectClass = e.GUID_ProjectClass,
                            IsFinance = e.IsFinance
                        }).OrderBy(e => e.ProjectKey).ToList();
                    }
            }
            else
            {
                list = GetProject(filterAuth,operatorGuid);
            }

            return list;
        }
        /// <summary>
        /// 操作员角色
        /// </summary>
        /// <returns></returns>
        public static List<RoleOperatorModel> GetRoleOperatorView(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var q = context.SS_RoleOperatorView;
            var list = q.Select(e => new RoleOperatorModel
            {
                OperatorKey=e.OperatorKey,
                OperatorName=e.OperatorName,
                GUID_Operator=e.GUID_Operator==null?Guid.Empty:(Guid)e.GUID_Operator,
                GUID_Role = e.GUID_Role == null ? Guid.Empty : (Guid)e.GUID_Role,
                RoleKey=e.RoleKey,
                RoleName=e.RoleName
            }).OrderBy(s => s.OperatorKey).ToList();
            return list;
        }
        /// <summary>
        /// 预算条目
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static List<SalarySetupItemTreeModel> getyusuanItem(BaseConfigEdmxEntities context = null)
        {
            if (context == null) {
                context = new BaseConfigEdmxEntities();
            }
            var q = context.BG_Item;
            var list = q.Select(e => new SalarySetupItemTreeModel
            {
                GUID = e.GUID,
                BGItemKey = e.BGItemKey,
                BGItemName = e.BGItemName,
                BGItemMemo = e.BGItemMemo,
                IsStop = e.IsStop
            }).OrderBy(e => e.BGItemKey).ToList();
            return list;
        }


        /// <summary>
        /// 工资项目
        /// </summary>
        /// <returns></returns>
        public static List<SalaryItemTreeModel> getSalaryItem(BaseConfigEdmxEntities context = null) 
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var q = context.SA_Item;
            var list = q.Select(e => new SalaryItemTreeModel
            {
                GUID = e.GUID,
                ItemKey = e.ItemKey,
                ItemName = e.ItemName,
                ItemType = e.ItemType,
                IsStop = e.IsStop
            }).OrderBy(e => e.ItemKey).ToList();
            return list;
        }

        /// <summary>
        /// 工资计划
        /// </summary>
        /// <returns></returns>
        public static List<SalaryPlanTreeModel> getSalaryPlan(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var q = context.SA_PlanView;
            var list = q.Select(e => new SalaryPlanTreeModel
            {
                GUID = e.GUID,
                PlanKey = e.PlanKey,
                PlanName = e.PlanName,
                IsStop = e.IsStop,
                IsDefault = e.IsDefault,
            }).OrderBy(e => e.PlanKey).ToList();
            return list;
        }

        /// <summary>
        /// 支出类型
        /// </summary>
        /// <returns></returns>
        public static List<SS_ExpendType> getPayOutType(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_ExpendType.OrderBy(e => e.ExpendTypeKey).ToList();
            return list;
        }
        /// <summary>
        /// 银行档案
        /// </summary>
        /// <returns></returns>
        public static List<SS_Bank> getBankInfo(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_Bank.OrderBy(e => e.BankKey).ToList();
            return list;
        }
        /// <summary>
        /// 证件类型
        /// </summary>
        /// <returns></returns>
        public static List<SS_CredentialType> getCredentialInfo(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_CredentialType.OrderBy(e => e.CredentialTypekey).ToList();
            return list;
        }
        /// <summary>
        /// 人员类别
        /// </summary>
        /// <returns></returns>
        public static List<SS_PersonType> getPersonType(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_PersonType.OrderBy(e => e.PersonTypeKey).ToList();
            return list;
        }
        /// <summary>
        /// 结算方式
        /// </summary>
        /// <returns></returns>
        public static List<SS_SettleType> getBaseSettleType(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_SettleType.OrderBy(e => e.SettleTypeKey).ToList();
            return list;
        }
        /// <summary>
        /// 收款类型
        /// </summary>
        /// <returns></returns>
        public static List<SS_SKTypeView> getBaseSKType(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_SKTypeView.OrderBy(e => e.SKTypeKey).ToList();
            return list;
        }

    }
    /// <summary>
    /// 外聘人员Tree模型
    /// </summary>
    public class InvitePersonTreeModel
    {
        public Guid GUID { get; set; }
        public string InvitePersonName { get; set; }
        public string InvitePersonIDCard { get; set; }
        public string CredentialTypeName { get; set; }
        public string CredentialTypekey { get; set; }
        public bool IsUnit { get; set; }
        public Guid? GUID_Department { set; get; }
        public string DepartmentName { set; get; }
        
    }
    public class InvitePersonTreeModelEx : InvitePersonTreeModel
    {
        public double SumTotalTax { get; set; }
        public double SumTotalReal { get; set; }
    }
    /// <summary>
    /// 人员Tree模型
    /// </summary>
    public class PersonTreeModel
    {   ///GUID
        public Guid GUID { get; set; }
        /// <summary>
        /// 单位编号
        /// </summary>
        public Guid GUID_DW { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string DWName { get; set; }
        /// <summary>
        /// 部门编号
        /// </summary>
        public Guid GUID_Department { set; get; }
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartmentName { set; get; }
        /// <summary>
        /// 公务卡

        /// </summary>
        public string OfficialCard { set; get; }
        /// <summary>
        /// 人员名称
        /// </summary>
        public string PersonName { set; get; }
        /// <summary>
        /// 人员Key
        /// </summary>
        public string PersonKey { set; get; }
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string BankCardNo { set; get; }

    }
    /// <summary>
    /// 科目Tree模型
    /// </summary>
    public class BGCodeTreeModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Guid GUID { set; get; }
        /// <summary>
        /// 父编号

        /// </summary>
        public Guid? PGUID { set; get; }
        /// <summary>
        /// 科目名称
        /// </summary>
        public string BGCodeName { set; get; }
        /// <summary>
        /// 科目Key值

        /// </summary>
        public string BGCodeKey { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public Guid GUID_EconomyClass { set; get; }

        public string EconomyClassKey { get; set; }

    }
    /// <summary>
    /// 项目Tree模型
    /// </summary>
    public class ProjectTreeModel
    {
        /// <summary>
        /// 项目ＧＵＩＤ
        /// </summary>
        public Guid GUID { set; get; }
        /// <summary>
        /// 父项目编号
        /// </summary>
        public Guid? PGUID{set;get;}
        /// <summary>
        /// 项目Key
        /// </summary>
        public string ProjectKey{set;get;}
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName{set;get;}
        /// <summary>
        /// 功能分类GUID
        /// </summary>
        public Guid? GUID_FunctionClass{set;get;}
        /// <summary>
        /// 项目分类
        /// </summary>
        public Guid GUID_ProjectClass { set; get; }
        /// <summary>
        /// 是否财政项目
        /// </summary>
        public bool?IsFinance{set;get;}
        /// <summary>
        /// 附加码
        /// </summary>
        public string ExtraCode {set;get;}
        /// <summary>
        /// 财政码
        /// </summary>
        public string FinanceCode {set;get;}

    }
    public class RoleOperatorModel
    { 
         public string OperatorKey{set;get;}
         public string OperatorName{set;get;}
         public Guid GUID_Operator{set;get;}
         public Guid GUID_Role{set;get;}
         public string RoleKey{set;get;}
         public string RoleName { set; get; }
    }

    public class SalaryItemTreeModel 
    {
        /// <summary>
        /// 工资项目GUID
        /// </summary>
        public Guid? GUID { set; get; }
        /// <summary>
        /// 工资项目编号
        /// </summary>
        public string ItemKey { get; set; }
        /// <summary>
        /// 工资项目名称
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// 工资项目类型
        /// </summary>
        public int? ItemType { get; set; }
        /// <summary>
        /// 是否停用
        /// </summary>
        public bool? IsStop { get; set; }
    }

    public class SalaryPlanTreeModel
    {
        /// <summary>
        /// 工资计划GUID
        /// </summary>
        public Guid? GUID { set; get; }
        /// <summary>
        /// 工资计划编号
        /// </summary>
        public string PlanKey { get; set; }
        /// <summary>
        /// 工资计划名称
        /// </summary>
        public string PlanName { get; set; }
        /// 是否停用
        /// </summary>
        public bool? IsStop { get; set; }
        /// 是否默认
        /// </summary>
        public bool? IsDefault { get; set; }
    }

    /// <summary>
    /// 文件类型Tree模型
    /// </summary>
    public class OfficeFileType
    {
        /// <summary>
        /// GUID
        /// </summary>
        public Guid GUID { set; get; }
        /// <summary>
        /// PGUID
        /// </summary>
        public Guid? PGUID { set; get; }
        /// <summary>
        /// 文件编码
        /// </summary>
        public string FileTypeKey { set; get; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileTypeName { set; get; }

    }

    #region 预算
    /// <summary>
    /// 预算类型
    /// </summary>
    public class SalarySetupTreeModel
    {
        /// <summary>
        /// 预算类型GUID
        /// </summary>
        public Guid? GUID { set; get; }
        /// <summary>
        /// 预算类型编号
        /// </summary>
        public string BGTypeKey { set; get; }
        /// <summary>
        /// 预算类型名称
        /// </summary>
        public string BGTypeName { set; get; }
        /// <summary>
        /// 是否停用
        /// </summary>
        public bool? IsStop { set; get; }
    }
    /// <summary>
    /// 预算条目
    /// </summary>
    public class SalarySetupItemTreeModel{
        /// <summary>
        /// 预算条目GUID
        /// </summary>
        public Guid? GUID { set; get; }
        /// <summary>
        /// 预算条目编号
        /// </summary>
        public string BGItemKey{set;get;}
        /// <summary>
        /// 预算条目名称
        /// </summary>
        public string BGItemName { set; get; }
        /// <summary>
        /// 是否停用
        /// </summary>
        public bool? IsStop { set; get; }
        /// <summary>
        /// 预算条目扩展
        /// </summary>
        public string BGItemMemo { set; get; }
    }
   
    #endregion
    
}






