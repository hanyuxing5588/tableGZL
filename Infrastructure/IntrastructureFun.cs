using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.Objects;
using Infrastructure;
using System.Text.RegularExpressions;
using System.Data.Objects.DataClasses;


namespace Infrastructure
{
    public enum IntrastructureModelEnum
    {
        单位,
        人员,
        操作员,
        部门,
    }

    public class IntrastructureFun
    {
        public BaseConfigEdmxEntities context = null;

        public IntrastructureFun() { context = new BaseConfigEdmxEntities(); }
        public IntrastructureFun(string connectstring) { context = new BaseConfigEdmxEntities(connectstring); }

        #region 登陆信息
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="LoginName">用户名</param>
        /// <param name="PassWd">密码</param>
        /// <returns>Bool</returns>
        public bool Login(string LoginName, string Password)
        {
            Infrastructure.Encryption encryption = new Infrastructure.Encryption();
            Password = encryption.DigestStrToHexStr(Password);
            var oper = context.SS_Operator.FirstOrDefault(a => a.OperatorKey == LoginName && a.Password == Password && a.IsStop == false);
            return oper == null ? false : true;
        }
        /// <summary>
        /// 登陆验证
        /// </summary>
        /// <param name="LoginName">用户名</param>
        /// <param name="Password">密码</param>
        /// <param name="isCheck">是否验证</param>
        /// <returns>string</returns>
        public string Login(string LoginName, string Password, bool isCheck)
        {
            Infrastructure.Encryption encryption = new Infrastructure.Encryption();
            Password = encryption.DigestStrToHexStr(Password);
            string message = string.Empty;
            var oper = context.SS_Operator.FirstOrDefault(a => a.OperatorKey == LoginName);
            if (oper == null)
            {
                message = "用户名不存在！";
            }
            else
            {
                if (oper.Password != Password)
                {
                    message = "密码错误！";
                }
                else if (oper.IsStop == true)
                {
                    message = "用户名已经停用！";
                }
                else
                {
                    message = "系统错误！";
                }
            }
            return message;
        }
        /// <summary>
        /// 操作员信息



        /// </summary>
        /// <param name="LoginName"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public SS_Operator GetOperatorInfo(string LoginName, string Password)
        {
            Infrastructure.Encryption encryption = new Infrastructure.Encryption();
            Password = encryption.DigestStrToHexStr(Password);
            return context.SS_Operator.Where(a => a.OperatorKey == LoginName && a.Password == Password && a.IsStop == false).FirstOrDefault();

        }
        #endregion
        public List<T> RetrieveModels<T>(Expression<Func<T, bool>> whereLambda) where T : EntityObject
        {
            return context.CreateObjectSet<T>().Where(whereLambda).ToList();
        }
        /// <summary>
        /// 获取基础档案
        /// </summary>       
        /// <param name="FilterAuth">是否需要权限过滤</param>
        /// <param name="OperatorKey">操作员</param>
        /// <param name="Cascade">是否需要获得关联档案</param>
        /// <returns>基础档案集合</returns>
        public List<T> RetrieveModels<T>(Expression<Func<T, bool>> whereLambda, bool FilterAuth, string OperatorKey, bool Cascade) where T : class
        {
            List<T> list = new List<T>();
            list = context.CreateObjectSet<T>().Where(whereLambda).ToList();
            return list;
        }
        /// <summary>
        /// 调用存储过程返回实体集合
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="procName">存储</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public List<T> RetrieveModels<T>(string procName, params ObjectParameter[] obj) where T : class
        {
            return CommonFuntion.GetList<T>(context, procName, obj);
        }

        /// <summary>
        /// 获取数量列表
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="whereLambda">条件</param>
        /// <returns></returns>
        public List<T> GetList<T>(Expression<Func<T, bool>> whereLambda) where T : class
        {
            return CommonFuntion.RetrieveModels<T>(context, whereLambda);
        }
        /// <summary>
        /// 通过存储过程获取列表信息
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="procName">存储过程名称</param>
        /// <param name="obj">参数数组</param>
        /// <returns>实体List</returns>
        public List<T> GetListByProc<T>(string procName, params ObjectParameter[] obj) where T : class
        {
            return CommonFuntion.GetList<T>(context, procName, obj);
        }
        /// <summary>
        /// 获取单位列表信息
        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>   
        /// <returns></returns>
        public List<SS_DW> GetDWList(bool FilterAuth, string OperatorGuid)
        {
            List<SS_DW> list = new List<SS_DW>();
            if (FilterAuth)
            {
                if (CommonFuntion.IsGUID(OperatorGuid))
                {
                    Guid guid = CommonFuntion.ConvertGUID(OperatorGuid);
                    int classid = CommonFuntion.GetClassId(typeof(SS_DW).Name);
                    var qdataset = GetDataSet(classid.ToString(), OperatorGuid);//"1"
                    if (qdataset == null) return null;
                    //获取单位权限信息
                    var q = from dw in context.SS_DW
                            where dw.IsStop == false && qdataset.Contains(dw.GUID)
                            select dw;
                    list = q.ToList();

                }

            }
            else
            {
                list = context.SS_DW.ToList();
            }
            return list;
        }
        /// <summary>
        /// 基础----获取单位列表信息
        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>   
        /// <returns>zzp</returns>
        public List<SS_DWView> GetDWViewList(bool FilterAuth, string OperatorGuid)
        {
            List<SS_DWView> list = new List<SS_DWView>();
            if (FilterAuth)
            {
                if (CommonFuntion.IsGUID(OperatorGuid))
                {
                    Guid guid = CommonFuntion.ConvertGUID(OperatorGuid);
                    int classid = CommonFuntion.GetClassId(typeof(SS_DW).Name);
                    var qdataset = GetDataSet(classid.ToString(), OperatorGuid);//"1"
                    if (qdataset == null) return null;
                    //获取单位权限信息
                    var q = from dw in context.SS_DWView
                            where dw.IsStop == false && qdataset.Contains(dw.GUID)
                            select dw;
                    list = q.ToList();

                }

            }
            else
            {
                list = context.SS_DWView.ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取单Key位列表信息


        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>   
        /// <returns></returns>
        public List<string> GetDWKeyList(bool FilterAuth, string OperatorGuid)
        {
            List<string> list = new List<string>();
            if (FilterAuth)
            {
                if (CommonFuntion.IsGUID(OperatorGuid))
                {
                    Guid guid = CommonFuntion.ConvertGUID(OperatorGuid);
                    int classid = CommonFuntion.GetClassId(typeof(SS_DW).Name);
                    var qdataset = GetDataSet(classid.ToString(), OperatorGuid);//"1"
                    if (qdataset == null) return null;
                    //获取单位权限信息
                    var q = from dw in context.SS_DW
                            where dw.IsStop == false && qdataset.Contains(dw.GUID)
                            select dw.DWKey;
                    list = q.ToList();

                }

            }
            else
            {
                list = context.SS_DW.Select(e => e.DWKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取单GUID位列表信息


        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>   
        /// <returns></returns>
        public List<Guid> GetDWGUIDList(bool FilterAuth, string OperatorGuid)
        {
            List<Guid> list = new List<Guid>();
            if (FilterAuth)
            {
                if (CommonFuntion.IsGUID(OperatorGuid))
                {
                    Guid guid = CommonFuntion.ConvertGUID(OperatorGuid);
                    int classid = CommonFuntion.GetClassId(typeof(SS_DW).Name);
                    var qdataset = GetDataSet(classid.ToString(), OperatorGuid);//"1"
                    if (qdataset == null) return null;
                    //获取单位权限信息
                    var q = from dw in context.SS_DW
                            where dw.IsStop == false && qdataset.Contains(dw.GUID)
                            select dw.GUID;
                    list = q.ToList();

                }

            }
            else
            {
                list = context.SS_DW.Select(e => e.GUID).ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取单位列表信息
        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>      
        /// <returns></returns>
        public List<SS_DW> GetDW(bool filterAuth, string operatorGuid)
        {
            List<SS_DW> dwList = new List<SS_DW>();

            if (filterAuth)
            {
                dwList = GetDWList(true, operatorGuid);
            }
            else
            {
                dwList = GetDWList(false, operatorGuid);
            }

            return dwList;
        }
        /// <summary>
        /// 基础----获取单位列表信息
        /// </summary>
        /// <param name="FilterAuth">是否根据权限过滤</param>
        /// <param name="OperatorGuid">操作员GUID</param>      
        /// <returns>zzp</returns>
        public List<SS_DWView> GetDWView(bool filterAuth, string operatorGuid)
        {
            List<SS_DWView> dwList = new List<SS_DWView>();

            if (filterAuth)
            {
                dwList = GetDWViewList(true, operatorGuid);
            }
            else
            {
                dwList = GetDWViewList(false, operatorGuid);
            }

            return dwList;
        }
        /// <summary>
        /// 根据权限获取人员信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public List<SS_Person> GetPerson(bool filterAuth, string operatorGuid)
        {
            List<SS_Person> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //人员"3"
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_Person.Include("SS_Department").Include("SS_DW")
                        where qdataset.Contains(a.GUID)
                        select a;
                list = q.ToList();
            }
            else
            {
                var person = context.SS_Person.Include("SS_Department").Include("SS_DW").ToList();
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
        public List<SS_Person> GetPerson(bool filterAuth, string operatorGuid, Guid departmentID)
        {
            List<SS_Person> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //人员"3"
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_Person.Include("SS_Department").Include("SS_DW")
                        where a.GUID_Department == departmentID && qdataset.Contains(a.GUID)
                        select a;
                list = q.ToList();
            }
            else
            {
                var person = context.SS_Person.Include("SS_Department").Include("SS_DW").Where(e => e.GUID_Department == departmentID).ToList();
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
        public List<SS_PersonView> GetPersonView(bool filterAuth, string operatorGuid, Guid departmentID)
        {
            List<SS_PersonView> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //人员"3"
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_PersonView
                        where a.GUID_Department == departmentID && qdataset.Contains(a.GUID)
                        select a;
                list = q.ToList();
            }
            else
            {
                var person = context.SS_PersonView.Where(e => e.GUID_Department == departmentID).ToList();
                list = person;
            }

            return list;
        }

        /// <summary>
        /// 根据权限获取人员信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>    
        /// <returns></returns>
        public List<SS_PersonView> GetPersonView(bool filterAuth, string operatorGuid)
        {
            List<SS_PersonView> list = null;

            if (filterAuth)
            {
                BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
                int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //人员
                if (qdataset == null) return null;
                //获取人员权限信息

                var q = from a in context.SS_PersonView
                        where qdataset.Contains(a.GUID)
                        select a;
                list = q.OrderBy(e => e.PersonKey).ToList();
            }
            else
            {
                var person = context.SS_PersonView.OrderBy(e => e.PersonName).ToList();
                list = person;
            }

            return list;
        }
        /// <summary>
        /// 获取人员实体
        /// </summary>
        /// <param name="operatorKey">操作员Key值</param>
        /// <returns>SS_Person类</returns>
        public SS_Person GetPersonModel(string operatorKey)
        {
            return context.SS_Person.Where(e => e.PersonKey == operatorKey).FirstOrDefault();
        }
        /// <summary>
        /// 根据权限获取操作员信息

        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_Operator> GetOperator(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            List<SS_Operator> list = null;
            if (isLoadRelateData)
            {
                if (filterAuth)
                {
                    var qdataset = GetDataSet("3", operatorGuid); //人员
                    //获取人员权限信息
                    var q = from a in context.SS_Operator.Include("SS_Department").Include("SS_DW")
                            where qdataset.Contains(a.GUID)
                            select a;
                    list = q.ToList();
                }
                else
                {
                    var operatorlist = context.SS_Operator.Include("SS_Department").Include("SS_DW").ToList();
                    list = operatorlist;
                }
            }
            else
            {
                if (filterAuth)
                {
                    var qdataset = GetDataSet("3", operatorGuid); //人员
                    //获取人员权限信息
                    var q = from a in context.SS_Operator
                            where qdataset.Contains(a.GUID)
                            select a;
                    list = q.ToList();
                }
                else
                {
                    var person = context.SS_Operator.ToList();
                    list = person;
                }
            }
            return list;
        }

        /// <summary>
        /// 根据权限获取操作员信息


        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_Operator> GetJCOperator(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            List<SS_Operator> list = null;
            if (isLoadRelateData)
            {
                if (filterAuth)
                {
                    var qdataset = GetDataSet("98", operatorGuid); //人员
                    //获取人员权限信息
                    var q = from a in context.SS_Operator.Include("SS_Department").Include("SS_DW")
                            //where qdataset.Contains(a.GUID)
                            select a;
                    list = q.OrderBy(e => e.OperatorKey).ToList();
                }
                else
                {
                    var operatorlist = context.SS_Operator.Include("SS_Department").Include("SS_DW").OrderBy(e => e.OperatorKey).ToList();
                    list = operatorlist;
                }
            }
            else
            {
                if (filterAuth)
                {
                    var qdataset = GetDataSet("98", operatorGuid); //人员
                    //获取人员权限信息
                    var q = from a in context.SS_Operator
                            // where qdataset.Contains(a.GUID)
                            select a;
                    list = q.OrderBy(e => e.OperatorKey).ToList();
                }
                else
                {
                    var person = context.SS_Operator.OrderBy(e => e.OperatorKey).ToList();
                    list = person;
                }
            }
            return list;
        }

        /// <summary>
        /// 返回Operator实体类


        /// </summary>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <returns>SS_Operator实体类</returns>
        public SS_Operator GetOperatorModel(string operatorGuid)
        {
            if (!CommonFuntion.IsGUID(operatorGuid))
            {
                return null;
            }
            Guid g = CommonFuntion.ConvertGUID(operatorGuid);
            return context.SS_Operator.Where(e => e.GUID == g).FirstOrDefault();
        }
        /// <summary>
        /// 获取部门信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>       
        /// <returns></returns>
        public List<SS_Department> GetDepartment(bool filterAuth, string operatorGuid)
        {
            List<SS_Department> list = new List<SS_Department>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //部门   "2"     
                if (qdataset == null) return null;
                //获取单位权限信息
                var q = from a in context.SS_Department
                        where a.IsStop == false && qdataset.Contains(a.GUID)
                        select a;
                list = q.OrderBy(e => e.DepartmentKey).ToList();
                //list = context.SS_Department.Include("SS_DW").OrderBy(e => e.DepartmentKey).ToList();   
            }
            else
            {
                list = context.SS_Department.OrderBy(e => e.DepartmentKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取有权限的部门GUID
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<Guid> GetDepartmentGUID(bool filterAuth, string operatorGuid)
        {
            List<Guid> list = new List<Guid>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //部门   "2"     
                if (qdataset == null) return null;
                //获取单位权限信息
                var q = from a in context.SS_Department
                        where a.IsStop == false && qdataset.Contains(a.GUID)
                        select a.GUID;
                list = q.ToList();
                //list = context.SS_Department.Include("SS_DW").OrderBy(e => e.DepartmentKey).ToList();   
            }
            else
            {
                list = context.SS_Department.Where(e => e.IsStop == false).Select(e => e.GUID).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取有权限的部门Key
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<string> GetDepartmentKey(bool filterAuth, string operatorGuid)
        {
            List<string> list = new List<string>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //部门   "2"     
                if (qdataset == null) return null;
                //获取单位权限信息
                var q = from a in context.SS_Department
                        where a.IsStop == false && qdataset.Contains(a.GUID)
                        select a.DepartmentKey;
                list = q.ToList();
            }
            else
            {
                list = context.SS_Department.Where(e => e.IsStop == false).Select(e => e.DepartmentKey).ToList();
            }

            return list;
        }

        public List<SS_DepartmentView> GetDepartmentView(bool filterAuth, string operatorGuid)
        {
            List<SS_DepartmentView> list = new List<SS_DepartmentView>();
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //部门"2"
                //获取单位权限信息
                var q = from a in context.SS_DepartmentView
                        where a.IsStop == false && qdataset.Contains(a.GUID)
                        select a;
                list = q.OrderBy(e => e.DepartmentKey).ToList(); ;
            }
            else
            {
                list = context.SS_DepartmentView.Where(e => e.IsStop == false).OrderBy(e => e.DepartmentKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 基础--部门信息
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<SS_DepartmentView> GetJCDepartmentView(bool filterAuth, string operatorGuid)
        {
            List<SS_DepartmentView> list = new List<SS_DepartmentView>();
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid); //部门"2"
                //获取单位权限信息
                var q = from a in context.SS_DepartmentView
                        where a.IsStop == false && qdataset.Contains(a.GUID)
                        select a;
                list = q.OrderBy(e => e.DepartmentKey).ToList(); ;
            }
            else
            {
                list = context.SS_DepartmentView.OrderBy(e => e.DepartmentKey).ToList();
            }
            return list;
        }

        /// <summary>
        /// 获取科目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_BGCode> GetBgcode(bool filterAuth, string operatorGuid)
        {
            List<SS_BGCode> list = new List<SS_BGCode>();

            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_BGCode).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_BGCode.Where(e => qdataset.Contains(e.GUID) && e.IsStop == false).OrderBy(e => e.BGCodeKey).ToList();
            }
            else
            {
                list = context.SS_BGCode.Where(e => e.IsStop == false).OrderBy(e => e.BGCodeKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取科目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_BGCodeView> GetBgcodeView(bool filterAuth, string operatorGuid)
        {
            List<SS_BGCodeView> list = new List<SS_BGCodeView>();
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_BGCodeView).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = context.SS_BGCodeView.Where(e => qdataset.Contains(e.GUID) && e.IsStop == false).OrderBy(e => e.BGCodeKey).ToList();
            }
            else
            {
                list = context.SS_BGCodeView.Distinct().OrderBy(e => e.BGCodeKey).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获取科目摘要信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<object> GetBgcodeMemo(bool filterAuth, string operatorGuid)
        {
            List<object> list = new List<object>();

            if (filterAuth)
            {
                var listGuid = GetBgcode(filterAuth, operatorGuid).Select(e => e.GUID);
                list = context.SS_BGCodeMemo.Where(e => listGuid.Contains(e.GUID_BGCode)).Select(e => new
                {
                    GUID = e.GUID,
                    GUID_BGCode = e.GUID_BGCode,
                    FeeMemo = e.BGCodeMemo,
                    ActionMemo = e.BGCodeMemo,
                    PZMemo = e.BGCodeMemo
                }).ToList<object>();
            }
            else
            {
                list = context.SS_BGCodeMemo.Select(e => new
                {
                    GUID = e.GUID,
                    GUID_BGCode = e.GUID_BGCode,
                    FeeMemo = e.BGCodeMemo,
                    ActionMemo = e.BGCodeMemo,
                    PZMemo = e.BGCodeMemo
                }).ToList<object>();
            }

            return list;
        }
        /// <summary>
        /// 获取科目摘要信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_BGCodeMemoView> GetBgcodeMemoView(bool filterAuth, string operatorGuid)
        {
            List<SS_BGCodeMemoView> list = new List<SS_BGCodeMemoView>();

            if (filterAuth)
            {
                var listGuid = GetBgcode(filterAuth, operatorGuid).Select(e => e.GUID);
                list = context.SS_BGCodeMemoView.Where(e => listGuid.Contains((Guid)e.GUID_BGCode)).ToList();

            }
            else
            {

                list = context.SS_BGCodeMemoView.OrderBy(e => e.BGCodeKey).ToList();
            }

            return list;
        }


        /// <summary>
        /// 获取项目GUID信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<Guid> GetProjectGUID(bool filterAuth, string operatorGuid)
        {
            List<Guid> list = new List<Guid>();
            int year = DateTime.Now.Year;
            var q = context.SS_Project.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).Select(e => e.GUID).ToList();
            }
            else
            {
                list = q.Select(e => e.GUID).ToList();
            }

            return list;
        }
        /// <summary>
        /// 获取项目GUID信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<Guid> GetProjectGUID(bool filterAuth, string operatorGuid, int year)
        {
            //--都有值时 开始年<=年 并且 结束年>=年


            //--结束年为NULL值 ：开始年<=年并且 年==NULL 并且 isStop为false
            List<Guid> list = new List<Guid>();
            if (year == 0)
            {
                list = GetProjectGUID(filterAuth, operatorGuid);
            }
            else
            {
                var q = context.SS_Project.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
                if (filterAuth)
                {
                    //备注 停用条件用在还没有停用年的项目中，StopYear已经有值的不再使用IsStop 
                    int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                    var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                    if (qdataset == null) return null;
                    list = q.Where(e => qdataset.Contains(e.GUID)).Select(e => e.GUID).ToList();
                }
                else
                {
                    list = q.Select(e => e.GUID).ToList();
                }
            }

            return list;
        }
        /// <summary>
        /// 获取项目Key信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<string> GetProjectKey(bool filterAuth, string operatorGuid, int year)
        {
            List<string> list = new List<string>();
            var q = context.SS_Project.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                //&& ((e.BeginYear<=year && e.StopYear>=year) ||(e.StopYear==null && e.IsStop == false))
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).Select(e => e.ProjectKey).ToList();
            }
            else
            {
                list = q.Select(e => e.ProjectKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<SS_Project> GetProject(bool filterAuth, string operatorGuid)
        {
            List<SS_Project> list = new List<SS_Project>();
            int year = DateTime.Now.Year;
            var q = context.SS_Project.Include("SS_ProjectClass").Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectKey).ToList();
            }
            else
            {
                list = context.SS_Project.OrderBy(e => e.ProjectKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<SS_ProjcetExView> GetProjectEx(bool filterAuth, string operatorGuid)
        {
            List<SS_ProjcetExView> list = new List<SS_ProjcetExView>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjcetExView.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectKey).ToList();
            }
            else
            {
                list = q.OrderBy(e => e.ProjectKey).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获取项目数据
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <param name="year">年度</param>
        /// <returns></returns>
        public List<SS_Project> GetProject(bool filterAuth, string operatorGuid, int year)
        {
            List<SS_Project> list = new List<SS_Project>();
            if (year == 0)
            {
                list = GetProject(filterAuth, operatorGuid);
            }
            else
            {
                var q = context.SS_Project.Include("SS_ProjectClass").Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
                if (filterAuth)
                {
                    int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                    var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                    if (qdataset == null) return null;
                    list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectKey).ToList();
                }
                else
                {
                    list = q.OrderBy(e => e.ProjectKey).ToList();
                }
            }

            return list;
        }
        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectView> GetProjectView(bool filterAuth, string operatorGuid)
        {
            List<SS_ProjectView> list = new List<SS_ProjectView>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectView.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectKey).ToList();
            }
            else
            {
                list = context.SS_ProjectView.OrderBy(e => e.ProjectKey).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获取项目信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectView> GetProjectView(bool filterAuth, string operatorGuid,bool isStop)
        {
            List<SS_ProjectView> list = new List<SS_ProjectView>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectView.Where(e => e.IsStop == isStop && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectKey).ToList();
            }
            else
            {
                list = context.SS_ProjectView.Where(e => e.IsStop == isStop).OrderBy(e => e.ProjectKey).ToList();
            }

            return list;
        }

        /// <summary>
        /// 获取预算来源信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_BGSource> GetBGSource(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            List<SS_BGSource> list = new List<SS_BGSource>();
            if (isLoadRelateData)
            {
                if (filterAuth)
                {
                    list = context.SS_BGSource.Where(e => e.IsStop == false).OrderBy(e => e.BGSourceKey).ToList();

                }
                else
                {
                    list = context.SS_BGSource.Where(e => e.IsStop == false).OrderBy(e => e.BGSourceKey).ToList();
                }
            }
            else
            {
                if (filterAuth)
                {
                    list = context.SS_BGSource.Where(e => e.IsStop == false).OrderBy(e => e.BGSourceKey).ToList();
                }
                else
                {
                    list = context.SS_BGSource.Where(e => e.IsStop == false).OrderBy(e => e.BGSourceKey).ToList();
                }
            }
            return list;
        }
        /// <summary>
        /// 获取项目大类信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectClass> GetProjectClass(bool filterAuth, string operatorGuid)
        {
            List<SS_ProjectClass> list = new List<SS_ProjectClass>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectClass.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

            }
            else
            {
                list = q.OrderBy(e => e.ProjectClassKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取项目大类信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectClass> GetProjectClass(bool filterAuth, string operatorGuid,bool isStop)
        {
            List<SS_ProjectClass> list = new List<SS_ProjectClass>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectClass.Where(e => e.IsStop == isStop && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

            }
            else
            {
                list = q.Where(e=>e.IsStop==isStop).OrderBy(e => e.ProjectClassKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 获取项目大类信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <param name="year">年度</param>
        /// <returns></returns>
        public List<SS_ProjectClass> GetProjectClass(bool filterAuth, string operatorGuid, int year)
        {
            List<SS_ProjectClass> list = new List<SS_ProjectClass>();
            // && ((e.BeginYear <= year && e.StopYear >= year) || (e.StopYear == null && e.IsStop == false))

            var q = context.SS_ProjectClass.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (year == 0)
            {
                list = GetProjectClass(filterAuth, operatorGuid);
            }
            else
            {
                if (filterAuth)
                {
                    int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                    var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                    if (qdataset == null) return null;
                    list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

                }
                else
                {
                    list = q.OrderBy(e => e.ProjectClassKey).ToList();
                }
            }


            return list;
        }

        #region 基础  项目设置  项目分类

        /// <summary>
        /// 基础----获取项目大类信息tree
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <param name="year">年度</param>
        /// <returns></returns>
        public List<SS_ProjectClassView> GetJCProjectClass(bool filterAuth, string operatorGuid, int year)
        {
            List<SS_ProjectClassView> list = new List<SS_ProjectClassView>();
            var q = context.SS_ProjectClassView.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)));
            if (year == 0)
            {
                list = GetJCProjectClass(filterAuth, operatorGuid);
            }
            else
            {
                if (filterAuth)
                {
                    int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                    var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                    if (qdataset == null) return null;
                    list = q.Where(e => qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

                }
                else
                {
                    list = q.OrderBy(e => e.ProjectClassKey).ToList();
                }
            }
            return list;
        }
        /// <summary>
        /// 基础----获取项目大类信息tree
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectClassView> GetJCProjectClass(bool filterAuth, string operatorGuid)
        {
            List<SS_ProjectClassView> list = new List<SS_ProjectClassView>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectClassView;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => e.IsStop == false && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)) && qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

            }
            else
            {
                list = q.OrderBy(e => e.ProjectClassKey).ToList();
            }
            return list;
        }

        /// <summary>
        /// 基础----获取项目大类信息tree
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_ProjectClassView> GetJCProjectClass(bool filterAuth, string operatorGuid,bool isStop)
        {
            List<SS_ProjectClassView> list = new List<SS_ProjectClassView>();
            int year = DateTime.Now.Year;
            var q = context.SS_ProjectClassView;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                if (qdataset == null) return null;
                list = q.Where(e => e.IsStop == isStop && ((e.BeginYear <= year && e.StopYear >= year) || (e.BeginYear <= year && e.StopYear == null)) && qdataset.Contains(e.GUID)).OrderBy(e => e.ProjectClassKey).ToList();

            }
            else
            {
                list = q.Where(e=>e.IsStop==isStop).OrderBy(e => e.ProjectClassKey).ToList();
            }
            return list;
        }
        #endregion

        /// <summary>
        /// 获取货品信息
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_Goods> GetGoods(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            List<SS_Goods> list = new List<SS_Goods>();
            if (isLoadRelateData)
            {
                if (filterAuth)
                {
                    //list = context.SS_Goods.Where(e=>e.IsStop==false).OrderBy(e => e.GoodsKey).ToList();
                    list = context.SS_Goods.OrderBy(e => e.GoodsKey).ToList();

                }
                else
                {
                    //list = context.SS_Goods.Where(e=>e.IsStop==false).OrderBy(e => e.GoodsKey).ToList();
                    list = context.SS_Goods.OrderBy(e => e.GoodsKey).ToList();
                }
            }
            else
            {
                if (filterAuth)
                {
                    //list = context.SS_Goods.Where(e=>e.IsStop==false).OrderBy(e => e.GoodsKey).ToList();
                    list = context.SS_Goods.OrderBy(e => e.GoodsKey).ToList();
                }
                else
                {
                    //list = context.SS_Goods.Where(e=>e.IsStop==false).OrderBy(e => e.GoodsKey).ToList();
                    list = context.SS_Goods.OrderBy(e => e.GoodsKey).ToList();
                }
            }
            return list;
        }

        /// <summary>
        /// 获取菜单分类
        /// </summary>
        /// <returns></returns>
        public List<SS_MenuClass> GetMenuClass(bool filterAuth, string operatorGuid)
        {
            Guid g;
            List<SS_MenuClass> list = new List<SS_MenuClass>();
            if (filterAuth)
            {
                if (CommonFuntion.IsGUID(operatorGuid))
                {
                    g = CommonFuntion.ConvertGUID(operatorGuid);

                    var h = from o in context.SS_MenuClass
                            where o.MenuClassKey == "00"
                            select o;

                    var w = from c in context.SS_Role
                            join d in context.SS_RoleOperator on c.GUID equals d.GUID_Role
                            where d.GUID_Operator == g
                            select c.GUID;
                    var q = from a in context.SS_MenuSet
                            join b in context.SS_MenuClass on a.GUID_Menu equals b.GUID
                            where a.MenuType == 1 && (a.GUID_RoleOrOperator == g || w.Contains(a.GUID_RoleOrOperator))
                            orderby b.MenuClassKey
                            select b;


                    var k = h.ToList();

                    list = q.Union(h).OrderBy(e => e.MenuClassKey).ToList();
                }

            }
            else
            {
                list = context.SS_MenuClass.OrderBy(e => e.MenuClassKey).ToList();
            }
            return list;
        }
        /// <summary>
        /// 得到菜单数据
        /// </summary>
        /// <param name="filterAuth">是否根据权限获取菜单</param>
        /// <param name="operatorGuid">操作人GUID</param>
        /// <returns></returns>
        public List<SS_Menu> GetMenu(bool filterAuth, string operatorGuid)
        {
            List<SS_Menu> menulist = new List<SS_Menu>();
            if (filterAuth)
            {
                Guid g;
                if (CommonFuntion.IsGUID(operatorGuid))
                {
                    g = CommonFuntion.ConvertGUID(operatorGuid);
                    List<SS_MenuClass> menuClassList = GetMenuClass(true, operatorGuid);
                    var mcg = from a in menuClassList
                              select (Guid?)a.GUID;
                    var q = from a in context.SS_MenuSet
                            join b in context.SS_Menu on a.GUID_Menu equals b.GUID
                            join c in context.SS_Role on a.GUID_RoleOrOperator equals c.GUID
                            where a.MenuType == 0
                            select b.MenuKey;
                    menulist = (from a in context.SS_Menu
                                where q.Contains(a.MenuKey) && mcg.Contains(a.GUID_MenuClass)
                                orderby a.MenuKey
                                select a).ToList();

                }

            }
            else
            {
                menulist = context.SS_Menu.OrderBy(e => e.MenuKey).ToList();
            }
            return menulist;
        }

        /// <summary>
        /// 结算方式
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<SS_SettleType> GetSettleType(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            List<SS_SettleType> list = new List<SS_SettleType>();
            if (isLoadRelateData)
            {
                if (filterAuth)
                {
                    list = context.SS_SettleType.Where(e => e.IsStop == false).OrderBy(e => e.SettleTypeKey).ToList();

                }
                else
                {
                    list = context.SS_SettleType.Where(e => e.IsStop == false).OrderBy(e => e.SettleTypeKey).ToList();
                }
            }
            else
            {
                if (filterAuth)
                {
                    list = context.SS_SettleType.Where(e => e.IsStop == false).OrderBy(e => e.SettleTypeKey).ToList();
                }
                else
                {
                    list = context.SS_SettleType.Where(e => e.IsStop == false).OrderBy(e => e.SettleTypeKey).ToList();
                }
            }
            return list;
        }
        /// <summary>
        /// 预算类型
        /// </summary>
        /// <param name="filterAuth">是否根据权限过滤</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <param name="isLoadRelateData">是否加载关联数据</param>
        /// <returns></returns>
        public List<BG_Type> GetBGType(bool filterAuth, string operatorGuid, bool isLoadRelateData)
        {
            var list = context.BG_Type.Where(e => e.IsStop == false).ToList();

            return list;
        }
        /// <summary>
        /// 数据权限集合
        /// </summary>
        /// <param name="classid">表对应的SS_Class中的ClassID</param>
        /// <param name="operatorGuid">操作员GUID</param>
        /// <returns>对应的数据权限GUID List</returns>
        public IQueryable<Guid> GetDataSet(string classid, string operatorGuid)
        {
            IQueryable<Guid> list = null;
            if (CommonFuntion.IsGUID(operatorGuid))
            {
                Guid g;
                g = CommonFuntion.ConvertGUID(operatorGuid);

                //List<Guid> roleoper = new List<Guid>();
                //SS_Operator obj = new SS_Operator();
                //obj.GUID = g;

                //List<SS_Role> roles = obj.Roles();
                //if (roles != null) roleoper = roles.ToGuidList();
                //roleoper.Add(obj.GUID);
                //if (roleoper.Count == 0) return null;
                //IQueryable<SS_DataAuthSet> dasList = context.SS_DataAuthSet.Where(e => roleoper.Contains(e.GUID_RoleOrOperator) && e.ClassID == classid);
                //list = dasList.Select(e=>e.GUID_Data);

                ////获取数据权限               
                //角色GUID
                var guidlist = from role in context.SS_Role
                               join rp in context.SS_RoleOperator on role.GUID equals rp.GUID_Role
                               where rp.GUID_Operator == g
                               select role.GUID;
                //数据权限
                var qss_dataauthset = from dset in context.SS_DataAuthSet
                                      where (dset.GUID_RoleOrOperator == g || guidlist.Contains(dset.GUID_RoleOrOperator)) && dset.ClassID == classid
                                      select dset.GUID_Data;
                list = qss_dataauthset;
            }
            return list;
        }
        /// <summary>
        /// 外聘人员
        /// </summary>
        /// <param name="filterAuth">是否过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns>SS_InvitePerson List</returns>
        public List<SS_InvitePersonView> GetInvitePerson(bool filterAuth, string operatorGuid)
        {

            List<SS_InvitePersonView> list = new List<SS_InvitePersonView>();
            if (filterAuth)
            {
                //int classid = CommonFuntion.GetClassId(typeof(SS_ProjectClass).Name);
                //var qdataset = GetDataSet(classid.ToString(), operatorGuid);
                //if (qdataset == null) return null;
                list = context.SS_InvitePersonView.OrderBy(e => e.CredentialTypekey).ToList();

            }
            else
            {
                list = context.SS_InvitePersonView.OrderBy(e => e.CredentialTypekey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 交通工具

        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns>SS_InvitePerson List</returns>
        public List<SS_TrafficView> GetTraffic(bool filterAuth, string operatorGuid)
        {

            List<SS_TrafficView> list = new List<SS_TrafficView>();
            if (filterAuth)
            {
                list = context.SS_TrafficView.Where(e => e.IsStop == false).OrderBy(e => e.TrafficKey).ToList();
            }
            else
            {
                list = context.SS_TrafficView.OrderBy(e => e.TrafficKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 补助项目
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns>SS_InvitePerson List</returns>
        public List<SS_Allowance> GetAllowance()
        {
            List<SS_Allowance> list = new List<SS_Allowance>();
            list = context.SS_Allowance.OrderBy(e => e.AllowanceKey).ToList();
            return list;
        }
        /// <summary>
        /// 客户信息
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns>SS_InvitePerson List</returns>
        public List<SS_Customer> GetCustomer(bool filterAuth, string operatorGuid)
        {

            List<SS_Customer> list = new List<SS_Customer>();
            if (filterAuth)
            {
                list = context.SS_Customer.OrderBy(e => e.CustomerKey).ToList();
            }
            else
            {
                list = context.SS_Customer.OrderBy(e => e.CustomerKey).ToList();
            }

            return list;
        }

        /// </summary>
        /// 角色信息
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SS_Role> GetRole(bool filterAuth, string operatorGuid)
        {
            List<SS_Role> list = new List<SS_Role>();
            if (filterAuth)
            {
                list = context.SS_Role.OrderBy(e => e.RoleKey).ToList();
            }
            else
            {
                list = context.SS_Role.OrderBy(e => e.RoleKey).ToList();
            }

            return list;
        }


        #region 基础----角色用户--tree

        /// </summary>
        /// 基础----角色信息----tree
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SS_Role> GetJCRole(bool filterAuth, string operatorGuid)
        {
            List<SS_Role> list = new List<SS_Role>();
            if (filterAuth)
            {
                list = context.SS_Role.OrderBy(e => e.RoleKey).ToList();
            }
            else
            {
                list = context.SS_Role.OrderBy(e => e.RoleKey).ToList();
            }

            return list;
        }

        #endregion

        #region 基础----薪酬设置--工资项目设置--tree
        /// </summary>
        /// 工资项目
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SA_Item> GetJCItem(bool filterAuth, string operatorGuid)
        {
            List<SA_Item> list = new List<SA_Item>();
            if (filterAuth)
            {
                list = context.SA_Item.Where(e => e.IsStop == false).OrderBy(e => e.ItemKey).ToList();
            }
            else
            {
                list = context.SA_Item.OrderBy(e => e.ItemKey).ToList();
            }
            return list;
        }

        #endregion

        #region 基础----薪酬设置--工资计划设置--tree
        /// </summary>
        /// 工资计划
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SA_PlanView> GetJCPlan(bool filterAuth, string operatorGuid)
        {
            List<SA_PlanView> list = new List<SA_PlanView>();
            if (filterAuth)
            {
                list = context.SA_PlanView.Where(e => e.IsStop == false).OrderBy(e => e.PlanKey).ToList();
            }
            else
            {
                list = context.SA_PlanView.OrderBy(e => e.PlanKey).ToList();
            }
            return list;
        }
        #endregion

        #region 基础----薪酬设置--工资计划人员设置--tree
        /// </summary>
        /// 工资计划人员设置
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        /// <returns></returns>
        public List<SA_PlanPersonSetView> GetJCPlanPersonSet(bool filterAuth, string operatorGuid)
        {
            List<SA_PlanPersonSetView> list = new List<SA_PlanPersonSetView>();
            if (filterAuth)
            {
                list = context.SA_PlanPersonSetView.OrderBy(e => e.PlanKey).ToList();
            }
            else
            {
                list = context.SA_PlanPersonSetView.OrderBy(e => e.PlanKey).ToList();
            }
            return list;
        }

        #endregion

        /// <summary>
        /// 客户信息
        /// </summary>
        /// <param name="filterAuth">是否权限过滤条件</param>
        /// <param name="operatorGuid">操作人ID</param>
        ///<param name="cType">类型</param>
        /// <returns>SS_InvitePerson List</returns>
        public List<SS_Customer> GetCustomer(bool filterAuth, string operatorGuid, string cType)
        {

            List<SS_Customer> list = new List<SS_Customer>();
            if (filterAuth)
            {
                if (cType == "1")//客户
                {
                    list = context.SS_Customer.Where(e => e.IsCustomer == true).OrderBy(e => e.CustomerKey).ToList();
                }
                else if (cType == "2")//供应商

                {
                    list = context.SS_Customer.Where(e => e.IsVendor == true).OrderBy(e => e.CustomerKey).ToList();
                }
                else
                {
                    list = context.SS_Customer.OrderBy(e => e.CustomerKey).ToList();
                }
            }
            else
            {
                if (cType == "1")//客户
                {
                    list = context.SS_Customer.Where(e => e.IsCustomer == true).OrderBy(e => e.CustomerKey).ToList();
                }
                else if (cType == "2")//供应商

                {
                    list = context.SS_Customer.Where(e => e.IsVendor == true).OrderBy(e => e.CustomerKey).ToList();
                }
                else
                {
                    list = context.SS_Customer.OrderBy(e => e.CustomerKey).ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// 支票信息
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<CN_CheckView> GetCheckView(bool filterAuth, string operatorGuid)
        {
            List<CN_CheckView> list = new List<CN_CheckView>();
            if (filterAuth)
            {
                list = context.CN_CheckView.OrderBy(e => e.BankKey).ToList();
            }
            else
            {
                list = context.CN_CheckView.OrderBy(e => e.BankKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 银行账号
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<SS_BankAccountView> GetBankAccountView(bool filterAuth, string operatorGuid)
        {
            List<SS_BankAccountView> list = new List<SS_BankAccountView>();
            if (filterAuth)
            {
                list = context.SS_BankAccountView.Where(e => e.IsStop == false).OrderBy(e => e.BankAccountKey).ToList();
            }
            else
            {
                list = context.SS_BankAccountView.Where(e => e.IsStop == false).OrderBy(e => e.BankAccountKey).ToList();
            }

            return list;
        }
        /// <summary>
        /// 根据银行编号得到支票列表
        /// </summary>
        /// <param name="guidBankaccount">银行GUID</param>
        /// <param name="checkType">类型 0表示现金支票 1表示转账支票</param>
        /// <returns>object List</returns>
        public List<object> GetCheckList(Guid guidBankaccount, int checkType)
        {
            var guidList = context.SS_BankAccountView.Where(e => e.IsStop == false).OrderBy(e => e.BankAccountKey).Select(e => e.GUID);
            List<object> list = new List<object>();
            if (guidBankaccount != null && guidBankaccount != Guid.Empty && !string.IsNullOrEmpty(guidBankaccount.ToString()))
            {
                list = this.context.CN_CheckView.Where(e => (e.IsInvalid == false || e.IsInvalid == null) && e.CheckType == checkType && e.GUID_BankAccount == guidBankaccount && guidList.Contains(e.GUID_BankAccount))
                    .Select(e =>
                    new
                    {
                        e.GUID,
                        e.CheckNumber,
                        CheckType = e.CheckType == 0 ? "现金支票" : "转账支票",
                        e.GUID_BankAccount,
                        e.BankAccountNo,
                        e.BankAccountName,
                        e.BankName,
                        e.DWName,
                        IsInvalid = e.IsInvalid == true ? "是" : "否"

                    }).ToList<object>();

            }
            else
            {
                list = this.context.CN_CheckView.Where(e => (e.IsInvalid == false || e.IsInvalid == null) && e.CheckType == checkType && guidList.Contains(e.GUID_BankAccount)).Select(e =>
                   new
                   {
                       e.GUID,
                       e.CheckNumber,
                       CheckType = e.CheckType == 0 ? "现金支票" : "转账支票",
                       e.GUID_BankAccount,
                       e.BankAccountNo,
                       e.BankAccountName,
                       e.BankName,
                       e.DWName,
                       IsInvalid = e.IsInvalid == true ? "是" : "否"

                   }).ToList<object>();
            }
            return list;
        }

        /// <summary>
        /// 功能分类
        /// </summary>
        /// <returns></returns>
        public List<SS_FunctionClassView> GetFunctionClassView()
        {
            return this.context.SS_FunctionClassView.OrderBy(e => e.FunctionClassKey).ToList();
        }
        /// <summary>
        /// 单据类型
        /// </summary>
        /// <returns></returns>
        public List<SS_DocType> GetDocType(bool filterAuth, string operatorGuid)
        {
            if (filterAuth)
            {
                return this.context.SS_DocType.Where(e => e.IsStop == false).OrderBy(e => e.DocTypeKey).ToList();
            }
            else
            {
                return this.context.SS_DocType.Where(e => e.IsStop == false).OrderBy(e => e.DocTypeKey).ToList();
            }
        }
        /// <summary>
        /// 业务类型
        /// </summary>
        /// <returns></returns>
        public List<SS_YWType> GetYWType(bool filterAuth, string operatorGuid)
        {
            if (filterAuth)
            {
                return this.context.SS_YWType.Where(e => e.IsStop == false).OrderBy(e => e.YWTypeKey).ToList();
            }
            else
            {
                return this.context.SS_YWType.Where(e => e.IsStop == false).OrderBy(e => e.YWTypeKey).ToList();
            }
        }
        /// <summary>
        /// 预算设置
        /// </summary>
        /// <returns></returns>
        public List<BG_SetupView> GetBGSetUpView(bool filterAuth, string operatorGuid)
        {
            if (filterAuth)
            {
                return this.context.BG_SetupView.Where(e => e.IsStop == false).OrderBy(e => e.BGSetupKey).ToList();
            }
            else
            {
                return this.context.BG_SetupView.Where(e => e.IsStop == false).OrderBy(e => e.BGSetupKey).ToList();
            }
        }
        /// <summary>
        /// 预算步骤
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<BG_StepView> GetBGStepView(bool filterAuth, string operatorGuid)
        {
            if (filterAuth)
            {
                return this.context.BG_StepView.Where(e => e.IsStop == false).OrderBy(e => e.BGStepKey).ToList();
            }
            else
            {
                return this.context.BG_StepView.Where(e => e.IsStop == false).OrderBy(e => e.BGStepKey).ToList();
            }
        }
        /// <summary>
        /// 会计科目
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<CW_AccountTitle> GetCWAcountTitle(bool filterAuth, string operatorGuid)
        {
            int year = DateTime.Now.Year;
            var q = this.context.CW_AccountTitle.Where(e => (e.IsStop == false || e.IsStop == null) && (e.StopTime != null && e.StartTime != null && (((DateTime)e.StartTime).Year <= year && ((DateTime)e.StopTime).Year >= year) || (e.StartTime != null && ((DateTime)e.StartTime).Year <= year && e.StopTime == null)));

            if (filterAuth)
            {
                return q.OrderBy(e => e.AccountTitleKey).ToList();
            }
            else
            {
                return q.OrderBy(e => e.AccountTitleKey).ToList();
            }
        }
        /// <summary>
        /// 会计科目
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<CW_AccountTitle> GetCWAcountTitle(bool filterAuth, string operatorGuid, int year)
        {
            if (year == 0)
            {
                return GetCWAcountTitle(filterAuth, operatorGuid);
            }
            else
            {
                var q = this.context.CW_AccountTitle.Where(e => (e.IsStop == false || e.IsStop == null) && (e.StopTime != null && e.StartTime != null && (((DateTime)e.StartTime).Year <= year && ((DateTime)e.StopTime).Year >= year) || (e.StartTime != null && ((DateTime)e.StartTime).Year <= year && e.StopTime == null)));

                if (filterAuth)
                {
                    return q.OrderBy(e => e.AccountTitleKey).ToList();
                }
                else
                {
                    return q.OrderBy(e => e.AccountTitleKey).ToList();
                }
            }
        }
        /// <summary>
        /// 帐套
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<AccountMainView> GetAccountMainView(bool filterAuth, string operatorGuid)
        {
            var q = this.context.AccountMainViews;

            if (filterAuth)
            {
                return q.OrderBy(e => e.AccountKey).ToList();
            }
            else
            {
                return q.OrderBy(e => e.AccountKey).ToList();
            }
        }

        /// <summary>
        /// 帐套
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<AccountDetailView> GetAccountDetailView(bool filterAuth, string operatorGuid)
        {
            var q = this.context.AccountDetailViews;

            if (filterAuth)
            {
                return q.OrderBy(e => e.accountkey).ToList();
            }
            else
            {
                return q.OrderBy(e => e.accountkey).ToList();
            }
        }


        /// <summary>
        /// 角色操作员视图
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public List<SS_RoleOperatorView> GetRoleOperatorView(bool filterAuth, string operatorGuid)
        {
            List<SS_RoleOperatorView> list = new List<SS_RoleOperatorView>();
            var q = this.context.SS_RoleOperatorView;
            if (filterAuth)
            {

                list = this.context.SS_RoleOperatorView.Where(e => e.GUID_Operator == e.GUID_Operator).OrderBy(e => e.OperatorKey).ToList();
                return list;
            }
            else
            {
                list = q.OrderBy(e => e.OperatorKey).ToList();
                return list;

            }
        }
        /// <summary>
        /// 银行信息
        /// </summary>
        /// <returns></returns>
        public List<SS_Bank> GetBank()
        {
            return this.context.SS_Bank.OrderBy(e => e.BankKey).ToList();
        }
        /// <summary>
        /// 角色信息
        /// </summary>
        /// <returns></returns>
        public List<SS_Role> GetRole()
        {
            return this.context.SS_Role.OrderBy(e => e.RoleKey).ToList();
        }
        /// <summary>
        /// 人员信息
        /// </summary>
        /// <returns></returns>
        public List<SS_Operator> GetOperator()
        {
            return this.context.SS_Operator.Where(e => e.IsStop != true).OrderBy(e => e.OperatorKey).ToList();
        }
        /// <summary>
        /// 银行账户信息
        /// </summary>
        /// <returns></returns>
        public List<SS_BankAccountView> GetBankAccount()
        {
            return this.context.SS_BankAccountView.Where(e => e.IsStop == false || e.IsStop == null).ToList();
        }
        /// <summary>
        /// 获取模型属性


        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public List<SS_ClassAttribute> GetClassAttributeList(string tableName)
        {
            List<SS_ClassAttribute> list = new List<SS_ClassAttribute>();
            SS_Class model = context.SS_Class.Include("SS_ClassAttribute").FirstOrDefault(e => e.TableName == tableName);
            foreach (SS_ClassAttribute item in model.SS_ClassAttribute)
            {
                list.Add(item);
            }
            return list;
        }
        /*
         *      函数功能:   返回moneyUnit列表
         *       author:   dongsheng.zhang
         *         日期:    2014-4-14
         */

        public List<SS_MoneyUnit> GetSS_MoneyUnit()
        {
            var ss_MoneyUnitSet = this.context.SS_MoneyUnit;
            return ss_MoneyUnitSet.OrderBy(e => e.MoneyUnitKey).ToList();
        }

    }


    public static class IntrastructureCommonFun
    {
        public static IQueryable<Guid> GetDataSet(BaseConfigEdmxEntities context, string classid, string operatorGuid)
        {
            IQueryable<Guid> list = null;
            if (CommonFuntion.IsGUID(operatorGuid))
            {
                Guid g;
                g = CommonFuntion.ConvertGUID(operatorGuid);
                ////获取数据权限               
                //角色GUID
                var guidlist = from role in context.SS_Role
                               join rp in context.SS_RoleOperator on role.GUID equals rp.GUID_Role
                               where rp.GUID_Operator == g
                               select role.GUID;
                //数据权限
                var qss_dataauthset = from dset in context.SS_DataAuthSet
                                      where (dset.GUID_RoleOrOperator == g || guidlist.Contains(dset.GUID_RoleOrOperator)) && dset.ClassID == classid
                                      select dset.GUID_Data;
                list = qss_dataauthset;
            }
            return list;
        }
    }
}
