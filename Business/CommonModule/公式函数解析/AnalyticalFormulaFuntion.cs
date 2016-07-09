using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Business.CommonModule
{
    /// <summary>
    ///  公式函数 Formula
    /// </summary>
    public class FormulaFunction
    {
        public FormulaFunction() { }
        /// <summary>
        /// 测试
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string GetValue(string a)
        {
            int i;
            int.TryParse(a, out i);
            return (4 + i).ToString();
        }
    }
    /// <summary>
    /// 解析函数算法(公式中函数格式 #[函数名称]#)
    /// </summary>
    public class AnalyticalAlgorithm
    {
        //公式中函数格式 #[函数名称]#
        /// <summary>
        /// 得到函数名称
        /// </summary>
        /// <param name="formulaValue"></param>
        /// <returns></returns>
        private static string GetFormulaFunctionName(string formulaValue)
        {
            string formulaName = string.Empty;
            int startIndex = formulaValue.IndexOf("#[");
            int endIndex = formulaValue.IndexOf("]#");
            if (startIndex >= 0 && endIndex >= 0)
            {
                formulaName = formulaValue.Substring(startIndex + 2, endIndex - (startIndex + 2));
                //如果有参数把参数赋值
            }
            return formulaName;
        }
        /// <summary>
        /// 得到函数的值
        /// </summary>
        /// <param name="formulaFunction"></param>
        /// <returns></returns>
        private static string GetFormulaFunctionValue(string formulaFunctionEx)
        {
            //先解析带一个参数或者不带参数的函数 
            //注意：参数用{}中的值
            var rValue = string.Empty;
            var functionMothod = GetFormulaFunctionName(formulaFunctionEx);
            //函数名称
            var functionName = functionMothod.Substring(0, functionMothod.IndexOf("("));
            int startIndex = functionMothod.IndexOf("(");
            int endIndex = functionMothod.IndexOf(")");
            string argValue = string.Empty;
            if (startIndex >= 0 && endIndex >= 0)
            {
                var strArg = functionMothod.Substring(startIndex + 1, endIndex - (startIndex + 1));
                argValue = strArg.Substring(1, strArg.Length - 2);//减去大括号的长度
            }
            MethodInfo method = typeof(FormulaFunction).GetMethod(functionName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method != null)
            {
                var argObject = new object[] { argValue };
                object obj = method.Invoke(null, argObject);
                //如果调用的函数带Ref 参数 
                //用 var ref1=(string)argValue[0];...
                if (obj != null)
                {
                    rValue = obj.ToString();
                }
            }
            return rValue;
        }
        /// <summary>
        /// 函数字典
        /// </summary>
        /// <param name="strFormula"></param>
        /// <returns></returns>
        private static Dictionary<string, string> DicFunction(string strFormula)
        {
            //#[函数名称]#
            var dic = new Dictionary<string, string>();
            var arrF = strFormula.Split('#');
            var dicKey = string.Empty;
            var dicValue = string.Empty;
            foreach (string item in arrF)
            {
                if (item.Contains("[") && item.Contains("]"))
                {
                    dicKey = "#" + item + "#";
                    dicValue = GetFormulaFunctionValue(dicKey);
                    dic.Add(dicKey, dicValue);
                }
            }
            return dic;
        }
        /// <summary>
        /// 获取转换函数后的公式
        /// </summary>
        /// <param name="strFormula"></param>
        /// <returns></returns>
        public static string GetTransferFunctionFormula(string strFormula)
        {
            var dic = DicFunction(strFormula);
            if (dic != null)
            {
                foreach (KeyValuePair<string, string> item in dic)
                {
                    strFormula = strFormula.Replace(item.Key, item.Value);
                }
            }
            return strFormula;
        }
        /// <summary>
        /// 获取公式类中的所有函数名称（应用于下拉选择函数）
        /// </summary>
        /// <returns></returns>
        public static List<string> GetFormulaFuntionList()
        {
            List<string> list = new List<string>();
            string[] arrList = new[] { "ToString", "Equals", "GetHashCode", "GetType" };
            string typeName = string.Empty;
            string parameterName = string.Empty;
            MethodInfo[] mothod = typeof(FormulaFunction).GetMethods();
            foreach (MethodInfo item in mothod)
            {
                if (!arrList.Contains(item.Name))
                {
                    ParameterInfo[] parames = item.GetParameters();

                    string parameterStr = string.Empty;
                    for (int i = 0, j = parames.Length; i < j; i++)
                    {
                        typeName = parames[0].ParameterType.Name;
                        parameterName = parames[0].Name;
                        if (i == j - 1)
                        {
                            parameterStr += typeName.ToLower() + " " + parameterName;
                        }
                        else
                        {
                            parameterStr += typeName.ToLower() + " " + parameterName + ",";
                        }
                    }
                    var mothodName = "#[" + item.Name + "(" + parameterStr + ")]#";
                    list.Add(mothodName);
                }
            }
            return list;
        }

    }

}
