using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Objects.DataClasses;

namespace Business.Common
{
    public class ConvertFunction
    {
        /// <summary>
        /// 转换字符串到对应正确的数据格式值
        /// </summary>
        /// <param name="PropertyType">属性类型</param>
        /// <param name="s">字符串值</param>
        /// <param name="result">返回值</param>
        /// <returns>true:转换成功 false:转换失败</returns>
        public static bool TryParse(Type PropertyType, string s, out object result)
        {
            result = null;
            bool returnbool = false;
            if (PropertyType == typeof(string))
            {
                result = s;
                returnbool = true;
            }
            if (PropertyType == typeof(int))
            {
                int temp;
                returnbool = int.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(decimal))
            {
                decimal temp;
                returnbool = decimal.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(float))
            {
                float temp;
                returnbool = float.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(bool))
            {
                bool temp;
                returnbool = bool.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(DateTime))
            {
                DateTime temp;
                returnbool = DateTime.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(Guid))
            {
                Guid temp;
                returnbool = Guid.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(double))
            {
                double temp;
                returnbool = double.TryParse(s, out temp);
                result = temp;
            }
            if (PropertyType == typeof(byte[]))
            {
                returnbool = true;
              
            }
            return returnbool;
        }      
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
            MethodCallExpression whereCallExpression = Expression.Call(typeof(Queryable), "Where", new Type[] { typeof(T) },Expression.Constant(q),pred);          
            //排序
            MethodCallExpression orderByCallExpression = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), typeof(string) }, whereCallExpression,Expression.Lambda(Expression.Property(param,proPertyName),param));
          
            var model = q.Provider.CreateQuery<T>(orderByCallExpression).FirstOrDefault();
            object docNum=null;
            T type=Activator.CreateInstance<T>();
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
       
    }

    public static class CommonExtension
    {
        public static bool IsNullOrEmpty(this Guid obj)
        {
            if (obj == null || obj == Guid.Empty)
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this Guid? obj)
        {
            if (obj == null || obj == Guid.Empty)
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this DateTime obj)
        {
            if (obj == null || obj == DateTime.MinValue || (obj > DateTime.MaxValue || obj < DateTime.MinValue))
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this DateTime? obj)
        {
            if (obj == null || obj == DateTime.MinValue || (obj > DateTime.MaxValue || obj < DateTime.MinValue))
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this Double? obj)
        {
            if (obj == null || obj == Double.MinValue || (obj > Double.MaxValue || obj < Double.MinValue))
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this Double obj)
        {
            if (obj == null || obj == Double.MinValue || (obj > Double.MaxValue || obj < Double.MinValue))
            {
                return true;
            }
            return false;
        }
        public static bool IsNullOrEmpty(this bool? obj)
        {
            if (obj == null)
            {
                return true;
            }
            return false;
        }
    }
   
    
}
