using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure
{
    public class BaseCombo
    {
        /// <summary>
        /// 人员数据
        /// </summary>
        /// <param name="filterAuth"></param>
        /// <param name="operatorGuid"></param>
        /// <returns></returns>
        public static List<object> GetPersonCombo(bool filterAuth, string operatorGuid, BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<object> list = null;
            if (filterAuth)
            {
                int classid = CommonFuntion.GetClassId(context, typeof(SS_Person).Name);
                var qdataset = IntrastructureCommonFun.GetDataSet(context, classid.ToString(), operatorGuid); //人员
                if (qdataset == null) return null;
                //获取人员权限信息
                var q = from a in context.SS_PersonView
                        where qdataset.Contains(a.GUID)
                        orderby a.PersonKey
                        select new
                        {
                            a.GUID,
                            a.GUID_Department,
                            a.GUID_DW,
                            a.PersonKey,
                            a.PersonName,
                            a.DepartmentName,
                            a.DWName,
                            OfficialCard = a.OfficialCard == "Null" ? "" : a.OfficialCard
                        };
                list = q.ToList<object>();
            }
            else
            {
                var q = from a in context.SS_PersonView
                        orderby a.PersonKey
                        select new
                        {
                            a.GUID,
                            a.GUID_Department,
                            a.GUID_DW,
                            a.PersonKey,
                            a.PersonName,
                            a.DepartmentName,
                            a.DWName,
                            OfficialCard = a.OfficialCard == "Null" ? "" : a.OfficialCard
                        };
                list = q.ToList<object>();
            }

            return list;
        }
        /// <summary>
        /// 功能分类加载
        /// </summary>
        /// <returns></returns>
        public static List<object> GetFunctionClass(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            var list = context.SS_FunctionClassView.OrderBy(e => e.FunctionClassKey).Select(e => new { 
                e.GUID,
                e.FunctionClassKey,
                e.FunctionClassName,
                e.IsDefault,
                e.IsProject,
                e.IsStop,
                e.FinanceCode,
                e.BeginYear
            }).ToList<object>();
            return list;

        }
        /// <summary>
        /// 工资数据加载类型
        /// </summary>
        /// <returns></returns>
        public static List<object> GetSalarySetUp(BaseConfigEdmxEntities context = null)
        {
            if (context == null)
            {
                context = new BaseConfigEdmxEntities();
            }
            List<object> list = null;
            var q = context.SA_SetUpView.OrderBy(e => e.SetUpKey);
            list = q.ToList<object>();
            return list;
        }
    }
}
