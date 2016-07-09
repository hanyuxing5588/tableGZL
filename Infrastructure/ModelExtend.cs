using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public static class ModelExtend
    {
        public static SS_Department DefaultDepartment(this SS_Operator obj)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            List<Guid> roleoper = new List<Guid>();
            List<SS_Role> roles = obj.Roles();
            if (roles != null) roleoper = roles.ToGuidList();
            roleoper.Add(obj.GUID);
            if (roleoper.Count == 0) return null;
            List<SS_DataAuthSet> dass = CommonFuntion.RetrieveDefaultDataAuthSet(roleoper, classid);
            if (dass.Count == 0) return null;
            SS_DataAuthSet das = dass.Find(e => e.GUID_RoleOrOperator == obj.GUID);
            if (das != null) return context.SS_Department.FirstOrDefault(e => e.GUID == das.GUID_Data);
            das = dass.Find(e => e.GUID_RoleOrOperator != obj.GUID);
            if (das != null) return context.SS_Department.FirstOrDefault(e => e.GUID == das.GUID_Data);
            return null;
        }
        public static SS_DW DefaultDW(this SS_Operator obj)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            int classid = CommonFuntion.GetClassId(typeof(SS_DW).Name);
            List<Guid> roleoper = new List<Guid>();
            List<SS_Role> roles = obj.Roles();
            if (roles != null) roleoper = roles.ToGuidList();
            roleoper.Add(obj.GUID);
            if (roleoper.Count == 0) return null;
            List<SS_DataAuthSet> dass = CommonFuntion.RetrieveDefaultDataAuthSet(roleoper, classid);
            if (dass.Count == 0) return null;
            SS_DataAuthSet das = dass.Find(e => e.GUID_RoleOrOperator == obj.GUID);
            if (das != null) return context.SS_DW.FirstOrDefault(e => e.GUID == das.GUID_Data);
            das = dass.Find(e => e.GUID_RoleOrOperator != obj.GUID);
            if (das != null) return context.SS_DW.FirstOrDefault(e => e.GUID == das.GUID_Data);
            return null;
        }
        public static SS_Person DefaultPerson(this SS_Operator obj)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            List<Guid> roleoper = new List<Guid>();
            List<SS_Role> roles = obj.Roles();
            if (roles != null) roleoper = roles.ToGuidList();
            roleoper.Add(obj.GUID);
            if (roleoper.Count == 0) return null;
            List<SS_DataAuthSet> dass = CommonFuntion.RetrieveDefaultDataAuthSet(roleoper, classid);
            if (dass.Count == 0) return null;
            SS_DataAuthSet das = dass.Find(e => e.GUID_RoleOrOperator == obj.GUID);
            if (das != null) return context.SS_Person.FirstOrDefault(e => e.GUID == das.GUID_Data);
            das = dass.Find(e => e.GUID_RoleOrOperator != obj.GUID);
            if (das != null) return context.SS_Person.FirstOrDefault(e => e.GUID == das.GUID_Data);
            return null;
        }
        public static SS_PersonView DefaultPersonView(this SS_Operator obj)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            int classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            List<Guid> roleoper = new List<Guid>();
            List<SS_Role> roles = obj.Roles();
            if (roles != null) roleoper = roles.ToGuidList();
            roleoper.Add(obj.GUID);
            if (roleoper.Count == 0) return null;
            List<SS_DataAuthSet> dass = CommonFuntion.RetrieveDefaultDataAuthSet(roleoper, classid);
            if (dass.Count == 0) return null;
            SS_DataAuthSet das = dass.Find(e => e.GUID_RoleOrOperator == obj.GUID);
            if (das != null) return context.SS_PersonView.FirstOrDefault(e => e.GUID == das.GUID_Data);
            das = dass.Find(e => e.GUID_RoleOrOperator != obj.GUID);
            if (das != null) return context.SS_PersonView.FirstOrDefault(e => e.GUID == das.GUID_Data);
            return null;
        }
        public static List<SS_Role> Roles(this SS_Operator obj)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            IQueryable<SS_RoleOperator> q = context.SS_RoleOperator.Where(e => e.GUID_Operator == obj.GUID);
            List<SS_Role> result = new List<SS_Role>();
            if (q == null) { return result; }
            List<SS_RoleOperator> ros = q.ToList();
            List<Guid> roleguids = new List<Guid>();
            foreach (SS_RoleOperator item in ros) { roleguids.Add(item.GUID_Role); }
            IQueryable<SS_Role> qr = context.SS_Role.Where(e => roleguids.Contains(e.GUID));
            if (qr == null) return result;
            return qr.ToList();
        }
       
        public static List<Guid> ToGuidList(this List<SS_Role> objs)
        {
            List<Guid> result = new List<Guid>();
            foreach (SS_Role obj in objs)
            {
                result.Add(obj.GUID);
            }
            return result;
        }
        public static List<SS_DataAuthSet> RetrieveDefaultDataAuthSet(this SS_Operator obj, int ClassId)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => e.IsDefault == true && e.GUID == obj.GUID && e.ClassID == ClassId.ToString());
            List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
            if (q != null)
            {
                result = q.ToList();
            }
            DateTime dt = DateTime.Now;
            result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
            return result;
        }
        public static List<SS_DataAuthSet> RetrieveDataAuthSet(this SS_Operator obj, int ClassId)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => e.GUID == obj.GUID && e.ClassID == ClassId.ToString());
            List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
            if (q != null)
            {
                result = q.ToList();
            }
            DateTime dt = DateTime.Now;
            result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
            return result;
        }
        public static List<SS_DataAuthSet> RetrieveDefaultDataAuthSet(this SS_Role obj, int ClassId)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => e.IsDefault == true && e.GUID == obj.GUID && e.ClassID == ClassId.ToString());
            List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
            if (q != null)
            {
                result = q.ToList();
            }
            DateTime dt = DateTime.Now;
            result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
            return result;
        }
        public static List<SS_DataAuthSet> RetrieveDataAuthSet(this SS_Role obj, int ClassId)
        {
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            IQueryable<SS_DataAuthSet> q = context.SS_DataAuthSet.Where(e => e.GUID == obj.GUID && e.ClassID == ClassId.ToString());
            List<SS_DataAuthSet> result = new List<SS_DataAuthSet>();
            if (q != null)
            {
                result = q.ToList();
            }
            DateTime dt = DateTime.Now;
            result.RemoveAll(e => e.IsTimeLimited == true && (e.StartTime > dt || e.StopTime < dt));
            return result;
        }
        public static List<Guid> ToDataGuidList(this List<SS_DataAuthSet> objs)
        {
            List<Guid> result = new List<Guid>();
            foreach (SS_DataAuthSet obj in objs)
            {
                result.Add(obj.GUID_Data);
            }
            return result;
        }


        public static List<SS_Department> RetrieveChildren(this SS_Department obj, BaseConfigEdmxEntities context)
        {
            return context.SS_Department.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_Department obj, BaseConfigEdmxEntities context, ref List<SS_Department> result)
        {
            List<SS_Department> children = obj.RetrieveChildren(context);
            if (children != null && children.Count > 0)
            {
                foreach (SS_Department item in children)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
            else
            {
                result.Add(obj);
            }
        }

        #region 部门临时数据集中查询
        public static List<SS_Department> RetrieveChildren(this SS_Department obj, List<SS_Department> depAllList)
        {
            return depAllList.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_Department obj, List<SS_Department> depAllList, ref List<SS_Department> result)
        {
            List<SS_Department> children = obj.RetrieveChildren(depAllList);
            if (children != null && children.Count > 0)
            {
                foreach (SS_Department item in children)
                {
                    item.RetrieveLeafs(depAllList, ref result);
                }
            }
            else
            {
                var model = depAllList.FirstOrDefault(e=>e.GUID==obj.GUID);
                if (model != null && !result.Contains(model))
                {
                    result.Add(model);
                }
            }
        }
        #endregion

        public static List<SS_DW> RetrieveChildren(this SS_DW obj, BaseConfigEdmxEntities context)
        {
            return context.SS_DW.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_DW obj, BaseConfigEdmxEntities context, ref List<SS_DW> result)
        {
            List<SS_DW> children = obj.RetrieveChildren(context);
            if (children != null && children.Count > 0)
            {
                foreach (SS_DW item in children)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
            else
            {
                result.Add(obj);
            }
        }

        #region 临时数据集中查找
        public static List<SS_DW> RetrieveChildren(this SS_DW obj, List<SS_DW> dwAllList)
        {
            return dwAllList.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_DW obj, List<SS_DW> dwAllList, ref List<SS_DW> result)
        {
            List<SS_DW> children = obj.RetrieveChildren(dwAllList);
            if (children != null && children.Count > 0)
            {
                foreach (SS_DW item in children)
                {
                    item.RetrieveLeafs(dwAllList, ref result);
                }
            }
            else
            {
                var model = dwAllList.FirstOrDefault(e => e.GUID == obj.GUID);
                if (model != null && !result.Contains(model))
                {
                    result.Add(model);
                }
            }
        }
        #endregion

        public static List<SS_BGCode> RetrieveChildren(this SS_BGCode obj, BaseConfigEdmxEntities context)
        {
            return context.SS_BGCode.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_BGCode obj, BaseConfigEdmxEntities context, ref List<SS_BGCode> result)
        {
            List<SS_BGCode> children = obj.RetrieveChildren(context);
            if (children != null && children.Count > 0)
            {
                foreach (SS_BGCode item in children)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
            else
            {
                result.Add(obj);
            }
        }

        public static List<SS_Project> RetrieveChildren(this SS_Project obj, BaseConfigEdmxEntities context)
        {
            return context.SS_Project.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_Project obj, BaseConfigEdmxEntities context, ref List<SS_Project> result)
        {
            List<SS_Project> children = obj.RetrieveChildren(context);
            if (children != null && children.Count > 0)
            {
                foreach (SS_Project item in children)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
            else
            {
                result.Add(obj);
            }
        }
        #region 临时表中查找
        public static List<SS_Project> RetrieveChildren(this SS_Project obj, List<SS_Project> projectList)
        {
            return projectList.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_Project obj, List<SS_Project> projectList, ref List<SS_Project> result)
        {
            List<SS_Project> children = obj.RetrieveChildren(projectList);
            if (children != null && children.Count > 0)
            {
                foreach (SS_Project item in children)
                {
                    item.RetrieveLeafs(projectList, ref result);
                }
            }
            else
            {
                var model = projectList.FirstOrDefault(e => e.GUID == obj.GUID);
                if (model != null && !result.Contains(model))
                {
                    result.Add(model);
                }
            }
        }
        #endregion
        /// <summary>
        /// 功能分类返回SS_Project
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public static void RetrieveLeafs(this SS_FunctionClass obj, BaseConfigEdmxEntities context, ref List<SS_Project> result)
        {                     
                List<SS_Project> projects = context.SS_Project.Where(e => e.GUID_FunctionClass ==obj.GUID).ToList();
                foreach (SS_Project item in projects)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            
        }

        public static List<SS_ProjectClass> RetrieveChildren(this SS_ProjectClass obj, BaseConfigEdmxEntities context)
        {
            return context.SS_ProjectClass.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_ProjectClass obj, BaseConfigEdmxEntities context, ref List<SS_ProjectClass> result)
        {
            List<SS_ProjectClass> children = obj.RetrieveChildren(context);
            if (children != null && children.Count > 0)
            {
                foreach (SS_ProjectClass item in children)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
            else
            {
                result.Add(obj);
            }
        }

        public static void RetrieveLeafs(this SS_ProjectClass obj, BaseConfigEdmxEntities context, ref List<SS_Project> result)
        {
            List<SS_ProjectClass> projectclasses = new List<SS_ProjectClass>();
            obj.RetrieveLeafs(context, ref projectclasses);
            foreach (SS_ProjectClass ProjectClass in projectclasses)
            {
                List<SS_Project> projects = context.SS_Project.Where(e => e.GUID_ProjectClass == ProjectClass.GUID).ToList();
                foreach (SS_Project item in projects)
                {
                    item.RetrieveLeafs(context, ref result);
                }
            }
        }
        #region 临时列表中查找末级节点 （备注：不是项目分类末级节点也可能有项目）获取项目分类下的所有节点
        public static List<SS_ProjectClass> RetrieveChildren(this SS_ProjectClass obj, List<SS_ProjectClass> projecClassList)
        {
            return projecClassList.Where(e => e.PGUID == obj.GUID).ToList();
        }

        public static void RetrieveLeafs(this SS_ProjectClass obj, List<SS_ProjectClass> projecClassList, ref List<SS_ProjectClass> result)
        {
            List<SS_ProjectClass> children = obj.RetrieveChildren(projecClassList);
            if (children != null && children.Count > 0)
            {
                foreach (SS_ProjectClass item in children)
                {
                    #region 添加属于节点下子节点
                    var model = projecClassList.FirstOrDefault(e => e.GUID == obj.GUID);
                    if (model != null && !result.Contains(model))
                    {
                        result.Add(model);
                    }
                    #endregion
                    item.RetrieveLeafs(projecClassList, ref result);
                }
            }
            else
            {
                var model = projecClassList.FirstOrDefault(e => e.GUID == obj.GUID);
                if (model != null && !result.Contains(model))
                {
                    result.Add(model);
                }
            }
        }
        /// <summary>
        /// 获取项目分类的末级节点
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="classList">项目分类集合</param>
        /// <param name="projectList">项目集合</param>
        /// <param name="result"></param>
        public static void RetrieveLeafs(this SS_ProjectClass obj, List<SS_ProjectClass> classList,List<SS_Project> projectList, ref List<SS_Project> result)
        {
            List<SS_ProjectClass> projectclasses = new List<SS_ProjectClass>();
            obj.RetrieveLeafs(classList, ref projectclasses);
            foreach (SS_ProjectClass ProjectClass in projectclasses)
            {
                List<SS_Project> projects = projectList.Where(e => e.GUID_ProjectClass == ProjectClass.GUID).ToList();
                foreach (SS_Project item in projects)
                {
                    item.RetrieveLeafs(projectList, ref result);
                }
            }
        }

        #endregion 

        public static List<MenuFun> GetMenu(this MenuFun m, BaseConfigEdmxEntities context, Guid OperGuid)
        {
            //根据当前操作员获得到其所对应的角色集合
            var ListRoleGuid = context.SS_RoleOperator.Where(e => e.GUID_Operator == OperGuid).Select(e => e.GUID_Role);

            //得到menu菜单的guid
            var temp = (from s_m in context.SS_Menu
                        join s_as in context.SS_AuthSet on s_m.GUID_Auth equals s_as.GUID_Auth into joinCm
                        from a in joinCm.DefaultIfEmpty()
                        where a.GUID_RoleOrOperator == OperGuid || ListRoleGuid.Contains(a.GUID_RoleOrOperator)
                        select new
                        {
                            GUID = s_m.GUID
                        }).Select(e => e.GUID);
            //没有功能权限
            var NoFunPriMenu = (from nmu in context.SS_Menu
                             where !temp.Contains(nmu.GUID)
                             select new MenuFun
                             {
                                 GUID = nmu.GUID,
                                 MenuKey = nmu.MenuKey,
                                 MenuName = nmu.MenuName,
                                 GUID_MenuClass = nmu.GUID_MenuClass,
                                 PGUID = nmu.PGUID,
                                 GUID_Auth = nmu.GUID_Auth,
                                 scope =nmu.scope
                             });
            //有功能权限
            var HaveFunPriMenu = (from hmu in context.SS_Menu
                               join s_as in context.SS_AuthSet on hmu.GUID_Auth equals s_as.GUID_Auth into joinCm
                               from a in joinCm.DefaultIfEmpty()
                               where a.GUID_RoleOrOperator == OperGuid || ListRoleGuid.Contains(a.GUID_RoleOrOperator)
                               select new MenuFun
                              {
                                  GUID = hmu.GUID,
                                  MenuKey = hmu.MenuKey,
                                  MenuName = hmu.MenuName,
                                  GUID_MenuClass = hmu.GUID_MenuClass,
                                  PGUID = hmu.PGUID,
                                  GUID_Auth = hmu.GUID_Auth,
                                  scope = hmu.scope == null ? "" : hmu.scope
                              });
            //功能权限菜单
            var FunPriMenu = NoFunPriMenu.Union(HaveFunPriMenu).OrderBy(e => e.MenuKey);

            //有权限的菜单和有功能菜单集合
            List<MenuFun> MenuFunPriMenu = (from fpm in FunPriMenu
                                 join s_ms in context.SS_MenuSet on fpm.GUID equals s_ms.GUID_Menu
                                 join s_mc in context.SS_MenuClass on fpm.GUID_MenuClass equals s_mc.GUID
                                  where s_ms.GUID_RoleOrOperator == OperGuid || ListRoleGuid.Contains(s_ms.GUID_RoleOrOperator)
                                  select fpm).Distinct().OrderBy(e=>e.MenuKey).ToList();

            IntrastructureFun ifn = new IntrastructureFun();
            //根据操作员和操作员对应角色得到相应的功能列表项
            List<SS_MenuClass> SMenuClass = ifn.GetMenuClass(true,OperGuid.ToString());
            //实例化自定义模型对象
            List<MenuFun> MenuClassList = new List<MenuFun>();
            //将功能列表转成自定义模型集合
            MenuClassList = SMenuClass.OrderBy(e => e.MenuClassKey).Select(e => new MenuFun
            {
                GUID = e.GUID,
                MenuKey = e.MenuClassKey,
                MenuName = e.MenuClassName
            }).ToList();
            //循环遍历功能列表项
          
            foreach (var MenuClass in MenuClassList)
            {
              
                List<MenuFun> SonMenuList = MenuClass.LoadFirstMenus(context, OperGuid, MenuFunPriMenu);
                if (SonMenuList == null) continue;

                if (MenuClass.MenuKey == "00")
                {
                    MenuClass.Child = SonMenuList;
                }
                else
                {
                    //循环遍历功能列表子项
                    foreach (var item in SonMenuList)
                    {
                        List<MenuFun> ChildList=new List<MenuFun> ();
                        //除一级节点以外查找出所有的末级节点循环赋值
                        //item.Child = item.LoadSenondMenus(context, MenuFunPriMenu);
                        item.RetrieveLeaf(context, ref ChildList, MenuFunPriMenu);
                        if (ChildList.Count == 1) {
                            if (ChildList[0].GUID == item.GUID) {
                                ChildList = new List<MenuFun>();
                            }
                        }
                        item.Child = ChildList;
                    }
                    MenuClass.Child = SonMenuList;
                }
               
            }
            return MenuClassList;
        }
        public static List<MenuFun> LoadFirstMenus(this MenuFun obj, BaseConfigEdmxEntities context, Guid OperGuid, List<MenuFun> MenuFunPriMenu)
        {
            List<MenuFun> MenuFirstList = new List<MenuFun>();
            if (obj.MenuKey == "00")
            {
                var menu = (from ss_m in MenuFunPriMenu
                            select new
                            {
                                GUID = ss_m.GUID,
                                MenuKey = ss_m.MenuKey,
                                MenuName = ss_m.MenuName,
                                GUID_MenuClass = ss_m.GUID_MenuClass,
                                PGUID = ss_m.PGUID,
                                GUID_Auth = ss_m.GUID_Auth,
                                scope = ss_m.scope
                            });

                var temp = (from s_m in menu
                            join s_cfv in context.SS_CommonFunctionView on s_m.GUID_Auth equals s_cfv.GUID_Auth 
                            where s_cfv.GUID_Operator == OperGuid && s_cfv.IsDefault == true
                            orderby s_cfv.OperationTimes descending
                            select new MenuFun
                            {
                                GUID = s_m.GUID,
                                MenuKey = s_m.MenuKey,
                                MenuName = s_m.MenuName,
                                GUID_MenuClass = s_m.GUID_MenuClass,
                                PGUID = s_m.PGUID,
                                GUID_Auth = s_m.GUID_Auth,
                                scope = s_m.scope
                            }).Take(15);
                MenuFirstList = temp.ToList();
                return MenuFirstList;
            }
            else if ((obj.MenuKey == "08"))
            {
                MenuFirstList = MenuFunPriMenu.Where(e => e.GUID_MenuClass == obj.GUID && e.MenuKey.Length == 4).Select(e => new MenuFun
                {
                    GUID = e.GUID,
                    MenuKey = e.MenuKey,
                    MenuName = e.MenuName,
                    GUID_MenuClass = e.GUID_MenuClass,
                    PGUID = e.PGUID,
                    GUID_Auth = e.GUID_Auth,
                    scope = e.scope
                }).OrderBy(e => e.MenuKey).ToList<MenuFun>();
            }
            else
            {
                MenuFirstList = MenuFunPriMenu.Where(e => e.GUID_MenuClass == obj.GUID && e.MenuKey.Length == 6).Select(e => new MenuFun
                {
                    GUID = e.GUID,
                    MenuKey = e.MenuKey,
                    MenuName = e.MenuName,
                    GUID_MenuClass = e.GUID_MenuClass,
                    PGUID = e.PGUID,
                    GUID_Auth = e.GUID_Auth,
                    scope = e.scope
                }).OrderBy(e => e.MenuKey).ToList<MenuFun>();

            }
            return MenuFirstList;
        }

        public static List<MenuFun> LoadSenondMenus(this MenuFun obj, BaseConfigEdmxEntities context, List<MenuFun> MenuFunPriMenu)
        {
            var sysMenuSecondList = MenuFunPriMenu.Where(e => e.PGUID == obj.GUID).Select(e => new MenuFun
            {
                GUID = e.GUID,
                MenuKey = e.MenuKey,
                MenuName = e.MenuName,
                GUID_MenuClass = e.GUID_MenuClass,
                PGUID = e.PGUID,
                GUID_Auth = e.GUID_Auth,
                scope = e.scope
            }).OrderBy(e => e.MenuKey).ToList<MenuFun>();

            
            return sysMenuSecondList.ToList();
        }

        public static void RetrieveLeaf(this MenuFun obj, BaseConfigEdmxEntities context, ref List<MenuFun> result, List<MenuFun> MenuFunPriMenu)
        {
            var  child = obj.LoadSenondMenus(context, MenuFunPriMenu);//23
            if (child != null && child.Count > 0)
            {

                foreach (var item in child)
                {
                    RetrieveLeaf(item, context, ref result, MenuFunPriMenu);//45
                }
            }
            else {
                result.Add(obj);

            }
            
        }
        /// <summary>
        /// 获取string GUID 例如：'guid','guid','guid',...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetStrGUIDS(this List<Guid> obj)
        {
            string str = string.Empty;
            if (obj == null || obj.Count==0)
                return "'"+Guid.Empty.ToString()+"'";
            foreach (Guid item in obj)
            {
                if (item == obj[obj.Count - 1])
                {
                    str = str + "'" + item + "'";
                }
                else
                {
                    str = str + "'" + item + "',";
                }
            }
            return str;
        }
        /// <summary>
        /// 获取string GUID 例如：'guid','guid','guid',...
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetStrGUIDS(this List<string> obj)
        {
            string str = string.Empty;
            if (obj == null || obj.Count==0)
                return "''";
            foreach (string item in obj)
            {
                if (item == obj[obj.Count - 1])
                {
                    str = str + "'" + item + "'";
                }
                else
                {
                    str = str + "'" + item + "',";
                }
            }
            return str;
        }
        public static bool WriteCommonFun(string commonFunGuid,Guid operatorId) {
           Guid commonFunGuid1 = Guid.Empty;
           if (!Guid.TryParse(commonFunGuid, out commonFunGuid1)) return false;
           if (operatorId==Guid.Empty) return false;
           try
           {

           
           using (var context=new BaseConfigEdmxEntities())
           {
               var ent = context.SS_CommonFunctionView.FirstOrDefault(e => e.GUID_Auth == commonFunGuid1);
               if (ent != null)
               {
                   ent.OperationTimes = ent.OperationTimes + 1;
               }
               else {
                   var a = context.SS_CommonFunction.CreateObject();
                   a.GUID = Guid.NewGuid();
                   a.GUID_Auth = commonFunGuid1;
                   a.GUID_Operator = operatorId;
                   a.OperationTimes = 1;
                   context.SS_CommonFunction.AddObject(a);
               }
               context.SaveChanges();

           }
           }
           catch (Exception ex)
           {
           }
           return true;
       
       }
    }

    /// <summary>
    /// 前台功能菜单
    /// </summary>
    public class MenuFun
    {
        public Guid? GUID { get; set; }
        public string MenuKey { get; set; }
        public string MenuName { get; set; }
        public Guid? GUID_MenuClass { get; set; }
        public Guid? PGUID { get; set; }
        public Guid? GUID_Auth { get; set; }
        public string scope { get; set; }
        public List<MenuFun> Child { get; set; }
    }
}
