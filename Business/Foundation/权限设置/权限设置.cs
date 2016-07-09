using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.IBusiness;
using Business.Common;
using System.Linq.Expressions;
using Infrastructure;
using BusinessModel;
namespace Business.Foundation.权限设置
{
   public  class 权限设置:IBusiness.IAuthSet
    {

       #region 单位
       public IEnumerable<AuthSetModel> GetDWAuth(Guid operatorId,bool isRole)
        {
            var  classId = "1";
            var IContext=DBFactory.GetCurInfrastructureDbContext();

            var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
            var a1 = IContext.SS_DataAuthSet.Where(e => e.ClassID == classId && e.GUID_RoleOrOperator == operatorId).ToList();
            var p1 = IContext.SS_DW.Where(e => e.IsStop!=true);
            var q = from dw in p1
                    join auth in p on dw.GUID equals auth.GUID_Data into temp
                    from t in temp.DefaultIfEmpty()
                    select new AuthSetModel
                    {
                        Guid = dw.GUID,
                        @checked=t.GUID_Data==null?false:true,
                        PGUID=dw.PGUID,
                        IsBrowser = t.IsBrowser,
                        IsDelete = t.IsDelete,
                        IsDefault = t.IsDefault,
                        IsModify = t.IsModify,
                        IsTimeLimited = t.IsTimeLimited,
                       StartTTime=t.StartTime,
                       StopTTime=t.StopTime,
                       ModelName = dw.DWName,
                        IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                    };
            var rq=RemoveRepeat(q.AsEnumerable()
                .Select(t => new AuthSetModel
            {
                Guid = t.Guid,
                _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                IsBrowser = t.IsBrowser,
                ClassId = "1",
                IsDelete = t.IsDelete,
                IsDefault = t.IsDefault,
                IsModify = t.IsModify,
                IsTimeLimited = t.IsTimeLimited,
                StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                @checked = t.@checked,
                ModelName = t.ModelName,
                IsAble = t.IsAble
            }));
            return rq;
        }
      
       #endregion
       #region 部门
       public IEnumerable<AuthSetModel> GetBMAuth(Guid operatorId, bool isRole)
       {
           var dw = GetDWAuth(operatorId,isRole);
           var classId = "2";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_Department.Where(e => e.IsStop != true);
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       PGUID = bm.PGUID,
                       @checked = t.GUID_Data == null ? false : true,
                       RGUID = bm.GUID_DW,
                       IsBrowser = t.IsBrowser,
                       IsDelete = t.IsDelete,
                       IsDefault = t.IsDefault,
                       IsModify = t.IsModify,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.DepartmentName,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           var b = RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID == null ? t.RGUID.ToString() : t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   IsDelete = t.IsDelete,
                   IsDefault = t.IsDefault,
                   ClassId = "2",
                   @checked = t.@checked,
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                   IsAble=t.IsAble
               }));
           return dw.Concat(b);
       }

       #endregion
       #region 人员
       public IEnumerable<AuthSetModel> GetPersonAuth(Guid operatorId, bool isRole)
       {
           var bm1 = GetBMAuth(operatorId,isRole);
           var classId = "3";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_Person;
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       @checked = t.GUID_Data == null ? false : true,
                       PGUID = bm.GUID_Department,
                       RGUID = bm.GUID_DW,
                       IsBrowser = t.IsBrowser,
                       IsDelete = t.IsDelete,
                       IsDefault = t.IsDefault,
                       IsModify = t.IsModify,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.PersonName,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           var b =this.RemoveRepeat( q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   @checked=t.@checked,
                   _parentId =t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   IsDelete = t.IsDelete,
                   IsDefault = t.IsDefault,
                   ClassId = "3",
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                   IsAble=t.IsAble
               }));
           return bm1.Concat(b);
       }

       #endregion
       #region 项目

       public IEnumerable<AuthSetModel> GetProjectClass(Guid operatorId, bool isRole)
       {
           var classId = "4";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_ProjectClass.Where(e=>e.IsStop!=true && (e.StopYear==null || e.StopYear>=DateTime.Now.Year));
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty() orderby bm.ProjectClassKey
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       PGUID = bm.PGUID,
                       IsBrowser = t.IsBrowser,
                       @checked = t.GUID_Data == null ? false : true,
                       IsDelete = t.IsDelete,
                       IsDefault = t.IsDefault,
                       IsModify = t.IsModify,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.ProjectClassName,
                       Key = bm.ProjectClassKey,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           return this.RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   IsDelete = t.IsDelete,
                   IsDefault = t.IsDefault,
                   @checked=t.@checked,
                   ClassId = "4",
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName + "(" + t.Key + ")",
                   IsAble=t.IsAble
               }));
       }
       public IEnumerable<AuthSetModel> GetProjectAuth(Guid operatorId, bool isRole)
       {
           var pc = GetProjectClass(operatorId,isRole);
           var classId = "5";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_Project.Where(e => e.IsStop != true && (e.StopYear == null || e.StopYear >= DateTime.Now.Year));
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty() orderby bm.ProjectKey
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       PGUID =bm.PGUID==null? bm.GUID_ProjectClass:bm.PGUID,
                       IsBrowser = t.IsBrowser,
                       IsDelete = t.IsDelete,
                       IsDefault = t.IsDefault,
                       @checked = t.GUID_Data == null ? false : true,
                       IsModify = t.IsModify,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.ProjectName,
                       Key=bm.ProjectKey,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           var b =this.RemoveRepeat( q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   IsDelete = t.IsDelete,
                   IsDefault = t.IsDefault,
                   @checked=t.@checked,
                   ClassId = "5",
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName + "(" + t.Key + ")",
                   IsAble=t.IsAble
               }));
           return pc.Concat(b);
       }

       #endregion
       #region 科目 成员


       public IEnumerable<AuthSetModel> GetBGCodeAuth(Guid operatorId, bool isRole)
       {

           var classId = "6";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_BGCode.Where(e => e.IsStop != true);
           
           var q = from dw in p1
                   join auth in p on dw.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = dw.GUID,
                       PGUID = dw.PGUID,
                       IsBrowser = t.IsBrowser,
                       IsDelete = t.IsDelete,
                       IsDefault = t.IsDefault,
                       IsModify = t.IsModify,
                       @checked = t.GUID_Data == null ? false : true,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = dw.BGCodeName,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           return this.RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   ClassId = "6",
                   IsDelete = t.IsDelete,
                   @checked=t.@checked,
                   IsDefault = t.IsDefault,
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                   IsAble=t.IsAble
               }));
       }

       #endregion
       #region IAuthSet 工资


       public IEnumerable<AuthSetModel> GetGZAuth(Guid operatorId, bool isRole)
       {
           var classId = "50";
           var IContext = DBFactory.GetCurInfrastructureDbContext();

           var p = this.GetDataAuthDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SA_Item.Where(e => e.IsStop != true);
           var q = from dw in p1
                   join auth in p on dw.GUID equals auth.GUID_Data into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = dw.GUID,
                       PGUID = null,
                       IsBrowser = t.IsBrowser,
                       IsDelete = t.IsDelete,
                       @checked = t.GUID_Data == null ? false : true,
                       IsDefault = t.IsDefault,
                       IsModify = t.IsModify,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       Key=dw.ItemKey,
                       ModelName = dw.ItemName,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   } ;
           return this.RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                   IsBrowser = t.IsBrowser,
                   ClassId = "50",
                   IsDelete = t.IsDelete,
                   @checked=t.@checked,
                   IsDefault = t.IsDefault,
                   Key=t.Key,
                   IsModify = t.IsModify,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                   IsAble=t.IsAble
               }).OrderBy(e=>e.Key));
       }

       #endregion
       #region 操作权限


       public IEnumerable<AuthSetModel> GetActionAuth(Guid operatorId)
       {
           throw new NotImplementedException();
       }

       #endregion
       #region 菜单权限设置

       public IEnumerable<AuthSetModel> GetMenuClass(Guid operatorId, bool isRole)
       {
           var classId = "45";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetMenuSetDataByOperatorIdOrRoleId(operatorId, isRole, classId, IContext);
           var p1 = IContext.SS_MenuClass;
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Menu into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       PGUID = null,
                       IsDefault = t.GUID== null ? false : true,
                       @checked = t.GUID == null ? false : true,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.MenuClassName,
                        Key=bm.MenuClassKey,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           return this.RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                   IsDefault = t.IsDefault,
                   @checked=t.@checked,
                   ClassId = "45",
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                    Key=t.Key,
                    IsAble=t.IsAble
               }));
       }
       public IEnumerable<AuthSetModel> GetMenuAuth(Guid operatorId, bool isRole)
       {
           var pc = GetMenuClass(operatorId,isRole);
           var classId = "46";
           var IContext = DBFactory.GetCurInfrastructureDbContext();
           var p = GetMenuSetDataByOperatorIdOrRoleId(operatorId,isRole,classId,IContext);
           var p1 = IContext.SS_Menu;
           var q = from bm in p1
                   join auth in p on bm.GUID equals auth.GUID_Menu into temp
                   from t in temp.DefaultIfEmpty()
                   select new AuthSetModel
                   {
                       Guid = bm.GUID,
                       PGUID = bm.PGUID,
                       @checked = t.GUID == null ? false : true,
                       IsDefault = t.GUID==null?false:true,
                       RGUID=bm.GUID_MenuClass,
                       IsTimeLimited = t.IsTimeLimited,
                       StartTTime = t.StartTime,
                       StopTTime = t.StopTime,
                       ModelName = bm.MenuName,
                       Key=bm.MenuKey,
                       IsAble = isRole == true ? true : (t.GUID_RoleOrOperator == operatorId ? true : false)
                   };
           var b =this.RemoveRepeat(q.AsEnumerable()
               .Select(t => new AuthSetModel
               {
                   Guid = t.Guid,
                   _parentId =t.PGUID==null?t.RGUID.ToString(): t.PGUID.ToString(),
                   IsDefault = t.IsDefault,
                   ClassId = "46",
                   @checked=t.@checked,
                   IsTimeLimited = t.IsTimeLimited,
                   StartTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StartTTime) : "",
                   StopTime = t.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(t.StopTTime) : "",
                   ModelName = t.ModelName,
                   IsAble=t.IsAble
               }));
           return pc.Concat(b).OrderBy(e=>e.Key).Distinct();
       }

       #endregion
       #region 保存数据权限
       public bool SaveAuthMenu(string userOrRoleIds, string classId, List<AuthSetModel> listAuthsetModel)
       {
           try
           {
               var IContext = DBFactory.GetCurInfrastructureDbContext();
               var listStrGuid = userOrRoleIds.Split(',').ToArray();
               var listGuid = CommonFuntion.ChangeStrArrToGuidList(listStrGuid);
               this.DeleteAuthMenuSetData(IContext, listGuid, classId);
               foreach (var userOrRoleId in listGuid)
               {
                   foreach (var item in listAuthsetModel)
                   {
                       AddDataAuthMenuSet(IContext, item, userOrRoleId, classId);
                   }
               }
               IContext.SaveChanges();
               return true;
           }
           catch (Exception ex)
           {
               return false;
           }
       }
       private void AddDataAuthMenuSet(BaseConfigEdmxEntities context, AuthSetModel authModel, Guid userOrRole, string classId)
       {
           var ent = context.SS_MenuSet.CreateObject();
           ent.GUID = Guid.NewGuid();
           ent.GUID_Menu = authModel.Guid;
           ent.GUID_RoleOrOperator = userOrRole;
           ent.MenuType = authModel.ClassId == "45" ? 1 : 0;
           ent.IsTimeLimited = authModel.IsTimeLimited;
           if (ent.IsTimeLimited == true)
           {
               DateTime dt = DateTime.Now;
               ent.StartTime = dt;
               if (DateTime.TryParse(authModel.StartTime, out dt))
               {
                   ent.StartTime = dt;
               }
               if (DateTime.TryParse(authModel.StopTime, out dt))
               {
                   ent.StopTime = dt;
               }
           }
           else
           {
               ent.StartTime = DateTime.Now;
           }
           context.SS_MenuSet.AddObject(ent);
       }
       private void DeleteAuthMenuSetData(BaseConfigEdmxEntities context, List<Guid> listUserOrRoleGuid, string classId)
       {
           var listInt = classId.Split(',').ToArray();
           var authDatas = context.SS_MenuSet.Where(e => listUserOrRoleGuid.Contains(e.GUID_RoleOrOperator)).ToList();
           foreach (var item in authDatas)
           {
               context.SS_MenuSet.DeleteObject(item);
           }
       }
       //保存方法
       public bool SaveAuth(string userOrRoleIds, string classId, List<AuthSetModel> listAuthsetModel)
       {
           try
           {
               if (classId.Contains("45"))
               {
                   return SaveAuthMenu(userOrRoleIds, classId, listAuthsetModel);
               }
               var IContext = DBFactory.GetCurInfrastructureDbContext();
               var listStrGuid = userOrRoleIds.Split(',').ToArray();
               var listGuid = CommonFuntion.ChangeStrArrToGuidList(listStrGuid);
               this.DeleteAuthSetData(IContext, listGuid, classId);
               foreach (var userOrRoleId in listGuid)
               {
                   foreach (var item in listAuthsetModel)
                   {
                       AddDataAuthSet(IContext, item, userOrRoleId, classId);
                   }
               }
               IContext.SaveChanges();
               return true;
           }
           catch (Exception ex)
           {
               return false;
           }
       }
       private void AddDataAuthSet(BaseConfigEdmxEntities context, AuthSetModel authModel, Guid userOrRole, string classId)
       {
           var ent = context.SS_DataAuthSet.CreateObject();
           ent.ClassID = authModel.ClassId;
           ent.GUID = Guid.NewGuid();
           ent.GUID_Data = authModel.Guid;
           ent.GUID_RoleOrOperator = userOrRole;
           ent.IsBrowser = authModel.IsBrowser;
           ent.IsDefault = authModel.IsDefault;
           ent.IsDelete = authModel.IsDelete;
           ent.IsModify = authModel.IsModify;
           ent.IsTimeLimited = authModel.IsTimeLimited;
           if (ent.IsTimeLimited == true)
           {
               DateTime dt = DateTime.Now;
               ent.StartTime = dt;
               if (DateTime.TryParse(authModel.StartTime, out dt))
               {
                   ent.StartTime = dt;
               }
               if (DateTime.TryParse(authModel.StopTime, out dt))
               {
                   ent.StopTime = dt;
               }
           }
           else
           {
               ent.StartTime = DateTime.Now;
           }
           context.SS_DataAuthSet.AddObject(ent);
       }
       private void DeleteAuthSetData(BaseConfigEdmxEntities context, List<Guid> listUserOrRoleGuid, string classId)
       {
           var listInt = classId.Split(',').ToArray();
           var authDatas = context.SS_DataAuthSet.Where(e => listUserOrRoleGuid.Contains(e.GUID_RoleOrOperator) && listInt.Contains(e.ClassID)).ToList();
           foreach (var item in authDatas)
           {
               context.SS_DataAuthSet.DeleteObject(item);
           }
       }
       #endregion
       private IQueryable<SS_DataAuthSet> GetDataAuthDataByOperatorIdOrRoleId(Guid operatorId,bool isRole,string classId,BaseConfigEdmxEntities context) 
       {
           IQueryable<SS_DataAuthSet> p = null;
         if (!isRole)
         {
             var roleQa = context.SS_RoleOperator.Where(e => e.GUID_Operator == operatorId).Select(e => e.GUID_Role);
             p = context.SS_DataAuthSet.Where(e =>e.ClassID == classId&&( e.GUID_RoleOrOperator == operatorId || roleQa.Contains(e.GUID_RoleOrOperator)));
         }
         else
         {
             p =  context.SS_DataAuthSet.Where(e => e.ClassID == classId && e.GUID_RoleOrOperator == operatorId);
         }
         return p;
       }
       private IQueryable<SS_MenuSet> GetMenuSetDataByOperatorIdOrRoleId(Guid operatorId, bool isRole, string classId, BaseConfigEdmxEntities context)
       {
           IQueryable<SS_MenuSet> p = null;
           if (!isRole)
           {
               var roleQa = context.SS_RoleOperator.Where(e => e.GUID_Operator == operatorId).Select(e => e.GUID_Role);
               p = context.SS_MenuSet.Where(e =>(e.GUID_RoleOrOperator == operatorId || roleQa.Contains(e.GUID_RoleOrOperator)));
           }
           else
           {
               p = context.SS_MenuSet.Where(e =>  e.GUID_RoleOrOperator == operatorId);
           }
           return p;
       }
       public IEnumerable<AuthSetModel> RemoveRepeat(IEnumerable<AuthSetModel> listAuthSetModel, bool IsNullGuid = true) 
       {
           var listGuid = new List<Guid>();
           List<AuthSetModel> listAuthTemp=new List<AuthSetModel> ();
           foreach (var item in listAuthSetModel)
           {
               if (!listGuid.Contains(item.Guid))
               {
                   listAuthTemp.Add(item);
                   listGuid.Add(item.Guid);
               }
               else 
               {
                   var tempItem = listAuthTemp.First(e => e.Guid == item.Guid);
                   tempItem.@checked = GetBoolOrValue(tempItem.@checked, item.@checked);
                   tempItem.IsBrowser = GetBoolOrValue(tempItem.IsBrowser, item.IsBrowser);
                   tempItem.IsDefault = GetBoolOrValue(tempItem.IsDefault, item.IsDefault);
                   tempItem.IsDelete = GetBoolOrValue(tempItem.IsDelete, item.IsDelete);
                   tempItem.IsModify = GetBoolOrValue(tempItem.IsModify, item.IsModify);
                   tempItem.IsTimeLimited = GetBoolOrValue(tempItem.IsTimeLimited, item.IsTimeLimited);
                   tempItem.StartTTime = GetDateTimeOrValue(tempItem.StartTTime, item.StartTTime);
                   tempItem.StopTTime = GetDateTimeOrValue(tempItem.StopTTime, item.StopTTime);
                   tempItem.IsAble = GetConvertBool(tempItem.IsAble, item.IsAble);

                   tempItem.StartTime = tempItem.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(tempItem.StartTTime) : "";
                   tempItem.StopTime = tempItem.IsTimeLimited == true ? Infrastructure.CommonFuntion.ConvertDateTimeToStr(tempItem.StopTTime) : "";
               }
               
           }
           return listAuthTemp;
       }  
    
       public DateTime? GetDateTimeOrValue(DateTime? b1, DateTime? b2,bool IsMax=true)
       {
           if (b2 ==null)
           {
               return b1;
           }
           if (b1 == null) {
               return b2;
           }
           if (b1 < b2)
           {
               return IsMax?b2:b1;
           }
           else {
               return IsMax ? b1 : b2;
           }
       }
       public bool GetBoolOrValue(bool? b1, bool? b2) 
       {
           if (b1==true || b2==true) {
               return true;
           }
           return false;
       }
       /// <summary>
       /// 是否可用 只要有一個不可用 都不可用用
       /// </summary>
       /// <param name="b1"></param>
       /// <param name="b2"></param>
       /// <returns></returns>
       public bool GetConvertBool(bool? b1, bool? b2)
       {
           if (b1 == false || b2 == false)
           {
               return false;
           }
           return true;
       }
    }
}
