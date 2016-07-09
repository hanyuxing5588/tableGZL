using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;
using BusinessModel;
namespace Business.Common
{
    public class CreateDocNumber
    {
        
        /// <summary>
        /// 创建DocNum编号
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static string GetNextDocNum<T>(ObjectContext db, string proPertyName) where T : class
        {
            var q = db.CreateObjectSet<T>().AsQueryable();
            //查询表达式
            //创建一个参数
            //ParameterExpression selectparam = Expression.Parameter(typeof(T), "e");
            ////组建表达式树e.DocNum
            //Expression selector = Expression.Property(selectparam, typeof(T).GetProperty(proPertyName));
            //Expression selectpred = Expression.Lambda(selector, selectparam);
            ////组建表达式树 Select(e=>e.DocNum)
            //Expression selectexpr = Expression.Call(typeof(Queryable), "Select", new Type[] { typeof(T), typeof(string) }, Expression.Constant(q), selectpred);

            //创建一个参数
            ParameterExpression param = Expression.Parameter(typeof(T), "e");
            //组建表达式树e.DocNum
            Expression left = Expression.Property(param, typeof(T).GetProperty(proPertyName));
            Expression filter = Expression.Equal(left, left);
            Expression pred = Expression.Lambda(filter, param);

            //创建表达式
            MethodCallExpression whereCallExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(T) }, Expression.Constant(q), pred);
            //排序
            MethodCallExpression orderByCallExpression = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), typeof(string) }, whereCallExpression, Expression.Lambda(Expression.Property(param, proPertyName), param));

            var model = q.Provider.CreateQuery<T>(orderByCallExpression).FirstOrDefault();
            object docNum = null;
            T type = Activator.CreateInstance<T>();
            PropertyInfo[] infos = typeof(T).GetProperties();
            foreach (PropertyInfo item in infos)
            {
                if (item.Name.ToUpper() == proPertyName.ToUpper())
                {
                    docNum = item.GetValue(model, null);
                    break;
                }
            }

            string strDocNum = string.Empty;
            string nowdate = DateTime.Now.ToString("yyyyMM");
            string strnext_NO = string.Empty;
            string strdefaut = "0001";

            if (docNum != null)
            {
                string current_No = docNum.ToString().Substring(docNum.ToString().Length - 4);
                int next_NO = int.Parse(current_No) + 1;
                strnext_NO = string.Format("{0:D4}", next_NO, next_NO.ToString().Length);
                strDocNum = nowdate + strnext_NO;
            }
            else
            {
                strDocNum = nowdate + strdefaut;
            }
            return strDocNum;
        }
        /// <summary>
        /// 生成单据编号
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="db">数据库上下文</param>
        /// <param name="proPertyName">属性名称</param>
        /// <param name="dwGUID">单位GUID</param>
        /// <param name="ywGUID">业务GUID</param>
        /// <param name="docDate">日期</param>
        /// <returns></returns>
        public static string GetNextDocNum(BusinessEdmxEntities _bcontext,Guid dwGUID, Guid ywGUID, string date)
        {            
            //BusinessEdmxEntities _bcontext = new BusinessEdmxEntities();
            Infrastructure.BaseConfigEdmxEntities baseContext = new Infrastructure.BaseConfigEdmxEntities();
            string strDocNum = string.Empty;
            SS_DocNumber dnModel=_bcontext.SS_DocNumber.FirstOrDefault();
            string dwNum = string.Empty;
            string ywNum = string.Empty;
            string yNum = string.Empty;
            string mNum = string.Empty;
            int year=DateTime.Parse(date).Year;
            int month=DateTime.Parse(date).Month;
            int autoNumber = 0;
            bool isAdd = false;
            SS_DocNumberAutoNumber docNumberAutoNumberModel = new SS_DocNumberAutoNumber();
            var dnanModel = _bcontext.SS_DocNumberAutoNumber.FirstOrDefault(e => e.DocYear == year && e.DocMonth == month && e.GUID_DW == dwGUID && e.GUID_YWType == ywGUID);
            if (dnanModel != null)
            {
                docNumberAutoNumberModel = dnanModel;
                autoNumber = (int)dnanModel.AutoNumber+1;
            }
            else
            {//添加  
                isAdd = true;
                autoNumber=(int)dnModel.AutoNumberBegin;
                docNumberAutoNumberModel.GUID = Guid.NewGuid();
                docNumberAutoNumberModel.GUID_DocNumber =dnModel.GUID;
                docNumberAutoNumberModel.DocYear = year;
                docNumberAutoNumberModel.DocMonth = month;
                docNumberAutoNumberModel.GUID_DW = dwGUID;
                docNumberAutoNumberModel.GUID_YWType = ywGUID;
                docNumberAutoNumberModel.AutoNumber = dnModel.AutoNumberBegin;
                _bcontext.SS_DocNumberAutoNumber.AddObject(docNumberAutoNumberModel);
            }
            if ((bool)dnModel.IsDW)
            {
                if ((bool)dnModel.IsDWKey)
                {
                   var dwModel=baseContext.SS_DW.FirstOrDefault(e=>e.GUID==dwGUID);
                   dwNum = dwModel.DWKey;
                }
            }
            if ((bool)dnModel.IsYWType)
            {
                if ((bool)dnModel.IsYWTypeKey)
                {
                    var ywModel = baseContext.SS_YWType.FirstOrDefault(e=>e.GUID==ywGUID);
                    ywNum = ywModel.YWTypeKey;
                }
            }
            if ((bool)dnModel.IsYear)
            {
                if (dnModel.YearFormat >= 0 && dnModel.YearFormat <= 4)
                {
                    yNum = docNumberAutoNumberModel.DocYear.ToString().Substring((4 -(int)dnModel.YearFormat),(int)dnModel.YearFormat);
                   
                }
            }
            if ((bool)dnModel.IsMonth)
            {
                mNum = docNumberAutoNumberModel.DocMonth.ToString();
                if (docNumberAutoNumberModel.DocMonth < 10)
                {
                    mNum = "0" + mNum;
                }
            }

            string strAutoNumber = string.Empty;
            strAutoNumber = string.Format("{0:D" + dnModel.AutoNumberLong+ "}",autoNumber,autoNumber.ToString().Length);
            string orderbyDocNum = GetOrderByDocNum(dnModel,dwNum,ywNum,yNum,mNum);
            strDocNum = orderbyDocNum + strAutoNumber;
            if (isAdd == false)
            {
                //修改
                docNumberAutoNumberModel.AutoNumber = autoNumber;
            }

            return strDocNum;
        }       
       
        /// <summary>
        /// 根据排序进行组织编号
        /// </summary>
        /// <param name="dnModel">SS_DocNumber模型</param>
        /// <param name="dwNum">单位编号</param>
        /// <param name="ywNum">业务编号</param>
        /// <param name="yNum">年编号</param>
        /// <param name="mNum">月编号</param>
        /// <returns>string</returns>
        private static string GetOrderByDocNum(SS_DocNumber dnModel,string dwNum, string ywNum, string yNum, string mNum)
        {
            string str = string.Empty;
            //根据排序进行组织编号
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            dictionary.Add((int)dnModel.Order_DW, dwNum);
            dictionary.Add((int)dnModel.Order_YWType, ywNum);
            dictionary.Add((int)dnModel.Order_Year, yNum);
            dictionary.Add((int)dnModel.Order_Month, mNum);
            var dict = dictionary.OrderBy(e => e.Key);
            foreach (KeyValuePair<int, string> item in dict)
            {
                str += item.Value;
            }           
            return str;
        }
        /// <summary>
        /// 判断编号是否设置日期并且是否改变
        /// </summary>
        /// <param name="orgDateTime">原日期</param>
        /// <param name="currentDateTime">当前修改日期</param>
        /// <returns>Bool</returns>
        public static bool IsDateChange(BusinessEdmxEntities _bcontext,DateTime orgDateTime, DateTime currentDateTime)
        {
            bool returnValue = false;
            if (currentDateTime != null && currentDateTime != DateTime.MinValue)
            {

                SS_DocNumber dnModel = _bcontext.SS_DocNumber.FirstOrDefault();
                if ((bool)dnModel.IsYear || (bool)dnModel.IsMonth)//生成的编号设置了时间
                {
                    if (orgDateTime.Year != currentDateTime.Year || orgDateTime.Month != currentDateTime.Month)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }
       
       
    }
}
