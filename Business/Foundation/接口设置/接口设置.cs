using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.接口设置
{
    public class 接口设置
    {
        public string Save(string sql) {
            try
            {
                Infrastructure.DataSource.ExecuteNonQuery(sql);
                return "";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        public IEnumerable<DYGXModel> GetKM(Guid comparnMainID)
        {
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var ents = IContext.ExecuteStoreQuery<DYGXModel>(@"SELECT  GUID AS id ,
        GUID AS Guid ,
        PGUID ,
        AccountTitleKey AS ModelKey ,
        AccountTitleName AS ModelName
FROM    dbo.CW_AccountTitle
WHERE   GUID_AccountMain IN (
        SELECT  GUID_AccountMain
        FROM    dbo.AccountDetail
        WHERE   GUID IN (
                SELECT  GUID_AccountDetail
                FROM    dbo.SS_ComparisonMain
                WHERE   GUID = '" + comparnMainID + "' ) )").ToList();
            var listGuid = new List<Guid>();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='AccountTitle' AND ClassID=65 and GUID_ComparisonMain='" + comparnMainID + "'").ToList(); ;
            List<DYGXModel> listAuthTemp = new List<DYGXModel>();
            foreach (var item in ents)
            {
                if (!listGuid.Contains(item.Guid))
                {
                    var ent = projects.FirstOrDefault(E => E.Guid == item.Guid);
                    if (ent != null)
                    {
                        item.Color = 1;
                        item.ExtKey = ent.ExtKey;
                    }
                    item._parentId =item.PTGUID + "" == "" ? item.PGUID.ToString() : item.PTGUID.ToString();
                    item.ClassId = "65";
                    item.ModelName = "(" + item.ModelKey + ")" + item.ModelName;
                    item.ModelKey = item.ModelKey;
                    listAuthTemp.Add(item);
                    listGuid.Add(item.Guid);
                }
            }

            return listAuthTemp;
        }

        public IEnumerable<DYGXModel> GetProjectClass()
        {
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var p1 = IContext.SS_ProjectClass.Where(e=>e.StopYear == null || e.StopYear >= DateTime.Now.Year);
            var q = from bm in p1
                    orderby bm.ProjectClassKey
                    select new DYGXModel
                    {
                        Guid = bm.GUID,
                        PGUID = bm.PGUID,
                        id = bm.GUID,
                        ModelName = bm.ProjectClassName,
                        ModelKey = bm.ProjectClassKey
                    };
            return this.RemoveRepeat(q.AsEnumerable()
                .Select(t => new DYGXModel
                {
                    Guid = t.Guid,
                    id = t.Guid,
                    _parentId = t.PGUID == null ? "" : t.PGUID.ToString(),
                    ClassId = "4",
                    ModelName = t.ModelName,
                    ModelKey = t.ModelKey
                }));
        }
        public IEnumerable<DYGXModel> GetBB(Guid comparnMainID)
        {
            IntrastructureFun dbobj = new IntrastructureFun();
            var list = dbobj.GetDepartment(false, "");
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='Department' AND ClassID=2 and GUID_ComparisonMain='" + comparnMainID + "'").ToList();
            List<DYGXModel> dx = new List<DYGXModel>();
            foreach (var item in list)
            {
                DYGXModel ent = new DYGXModel();
                ent.id = item.GUID;
                ent.Guid = item.GUID;
                ent.ModelName = "(" + item.DepartmentKey + ")" + item.DepartmentName;
                ent.ModelKey = item.DepartmentKey;

                var ent1 = projects.FirstOrDefault(E => E.Guid == item.GUID);
                if (ent1 != null)
                {
                    ent.Color = 1;
                    ent.ExtKey = ent1.ExtKey;
                }
                dx.Add(ent);

            } return dx;
        }
        public IEnumerable<DYGXModel> GetPerson(Guid comparnMainID)
        {
            IntrastructureFun dbobj = new IntrastructureFun();
            List<SS_PersonView> list = dbobj.GetPersonView(false, "");
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='Person' AND ClassID=3 and GUID_ComparisonMain='" + comparnMainID + "'").ToList();
            List<DYGXModel> dx = new List<DYGXModel>();
            foreach (var item in list)
            {
                DYGXModel ent = new DYGXModel();
                ent.id = item.GUID;
                ent.Guid = item.GUID;
                ent.ModelName = "(" + item.PersonKey + ")" + item.PersonName;
                ent.ModelKey = item.PersonKey;

                var ent1 = projects.FirstOrDefault(E => E.Guid == item.GUID);
                if (ent1 != null)
                {
                    ent.Color = 1;
                    ent.ExtKey = ent1.ExtKey;
                }
                dx.Add(ent);

            } return dx;
        }
        public IEnumerable<DYGXModel> GetKH(Guid comparnMainID)
        {
            IntrastructureFun dbobj = new IntrastructureFun();
            List<SS_Customer> list = dbobj.GetCustomer(true, "", "1");
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='Customer' AND ClassID=17 and GUID_ComparisonMain='" + comparnMainID + "'").ToList();
            List<DYGXModel> dx = new List<DYGXModel>();
            foreach (var item in list)
            {
                DYGXModel ent = new DYGXModel();
                ent.id = item.GUID;
                ent.Guid = item.GUID;
                ent.ModelName = "(" + item.CustomerKey + ")" + item.CustomerName;
                ent.ModelKey = item.CustomerKey;

                var ent1 = projects.FirstOrDefault(E => E.Guid == item.GUID);
                if (ent1 != null)
                {
                    ent.Color = 1;
                    ent.ExtKey = ent1.ExtKey;
                }
                dx.Add(ent);

            } return dx;
        }
        public IEnumerable<DYGXModel> GetGYSs(Guid comparnMainID)
        {
            IntrastructureFun dbobj = new IntrastructureFun();
            List<SS_Customer> list = dbobj.GetCustomer(true, "", "2");
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='Vendor' AND ClassID=17 and GUID_ComparisonMain='" + comparnMainID + "'").ToList();
            List<DYGXModel> dx = new List<DYGXModel>();
            foreach (var item in list)
            {
                DYGXModel ent = new DYGXModel();
                ent.id = item.GUID;
                ent.Guid = item.GUID;
                ent.ModelName = "(" + item.CustomerKey + ")" + item.CustomerName;
                ent.ModelKey = item.CustomerKey;

                var ent1 = projects.FirstOrDefault(E => E.Guid == item.GUID);
                if (ent1 != null)
                {
                    ent.Color = 1;
                    ent.ExtKey = ent1.ExtKey;
                }
                dx.Add(ent);

            } return dx;
        }
        public IEnumerable<DYGXModel> GetProjects(Guid comparnMainID)
        {
            var pc = GetProjectClass();
            var IContext = DBFactory.GetCurInfrastructureDbContext();
            var p1 = IContext.SS_Project.Where(e => e.StopYear == null || e.StopYear >= DateTime.Now.Year);
            var q = from bm in p1
                    orderby bm.ProjectKey
                    select new DYGXModel
                    {
                        id = bm.GUID,
                        Guid = bm.GUID,
                        PGUID = bm.GUID_ProjectClass,
                        PTGUID=bm.PGUID,
                        ModelName = bm.ProjectName,
                        ModelKey = bm.ProjectKey
                    };
            var entList=q.AsEnumerable()
                .Select(t => new DYGXModel
                {
                    Guid = t.Guid,
                    id = t.Guid,
                    _parentId =t.PTGUID+""==""? t.PGUID.ToString():t.PTGUID.ToString(),

                    ClassId = "5",
                    ModelName = "(" + t.ModelKey + ")" + t.ModelName,
                    ModelKey = t.ModelKey
                }).ToList();
            var b = this.RemoveRepeat1(IContext, entList, comparnMainID);
            return pc.Concat(b);
        }
        public IEnumerable<DYGXModel> RemoveRepeat1(BaseConfigEdmxEntities IContext, IList<DYGXModel> listAuthSetModel, Guid comparnMainID)
        {
            var listGuid = new List<Guid>();
            var projects = IContext.ExecuteStoreQuery<DYGXModel>("SELECT GUID_Self AS Guid,ExteriorKey as ExtKey FROM dbo.SS_ComparisonDetail WHERE Comparisontype='Project' AND ClassID=5 and GUID_ComparisonMain='" + comparnMainID + "'").ToList(); ;
            List<DYGXModel> listAuthTemp = new List<DYGXModel>();
            foreach (var item in listAuthSetModel)
            {
                if (!listGuid.Contains(item.Guid))
                {
                    var ent=projects.FirstOrDefault(E=>E.Guid==item.Guid);
                    if (ent != null) { 
                        item.Color = 1;
                        item.ExtKey = ent.ExtKey;
                    }
                    listAuthTemp.Add(item);
                    listGuid.Add(item.Guid);
                }
            }
            return listAuthTemp;
        }  
        public IEnumerable<DYGXModel> RemoveRepeat(IEnumerable<DYGXModel> listAuthSetModel, bool IsNullGuid = true)
        {
            var listGuid = new List<Guid>();
            List<DYGXModel> listAuthTemp = new List<DYGXModel>();
            foreach (var item in listAuthSetModel)
            {
                if (!listGuid.Contains(item.Guid))
                {
                    listAuthTemp.Add(item);
                    listGuid.Add(item.Guid);
                }
            }
            return listAuthTemp;
        }  
    }

    public class DYGXModel
    {
        public Guid? PGUID { get; set; }
        public Guid? PTGUID { get; set; }
        public Guid id { get; set; }
        public Guid Guid { get; set; }
        public string ModelName { get; set; }
        public string ModelKey { get; set; }
        public string _parentId { get; set; }
        public string ClassId { get; set; }
        public string ExtKey { get; set; }
        public int Color { get; set; }
    }
}
