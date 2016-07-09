using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CAE.Expression
{
    /// <summary>
    /// 辅助函数标识器验证消息
    /// </summary>
    public class ValidateResult
    {
        public static readonly ValidateResult Success;
        public List<string> Errors = new List<string>();
    }

    /// <summary>
    /// 创建器
    /// </summary>
    public class AuxiliaryBuilder
    {
        public static MarkAuxiliary BuilderMark()
        {
            BackslashQuotationMarkAuxiliary bqa = new BackslashQuotationMarkAuxiliary();
            bqa.SetResponsibility(new QuotationMarkAuxiliary());
            return bqa;
        }
        public static ConvertorAuxiliary BuilderConvertor()
        {
            ConvertorAuxiliary strca = new StringConvertorAuxiliary();
            ConvertorAuxiliary intca = new IntegerConvertorAuxiliary();
            ConvertorAuxiliary dobca = new DoubleConvertorAuxiliary();
            ConvertorAuxiliary booca = new BooleanConvertorAuxiliary();
            ConvertorAuxiliary datca = new DateTimeConvertorAuxiliary();
            datca.SetResponsibility(strca);
            booca.SetResponsibility(datca);
            dobca.SetResponsibility(booca);
            intca.SetResponsibility(dobca);
            return intca;
        }
    }

    #region 特殊字符转换
    /// <summary>
    /// 文本转换处理
    /// </summary>
    public abstract class TextAuxiliary
    {
        protected virtual string pattern { get { return string.Empty; } }
        protected Dictionary<string, string> _markValues = new Dictionary<string, string>();
        public virtual string MarkCharacteristic(string input) { return input; }
        public virtual string UnMarkCharacteristic(string input) { return input; }
    }
    /// <summary>
    /// 标记处理
    /// </summary>
    public abstract class MarkAuxiliary : TextAuxiliary
    {
        protected MarkAuxiliary Responsibility;

        public void SetResponsibility(MarkAuxiliary Responsibility)
        {
            this.Responsibility = Responsibility;
        }

        /// <summary>
        /// 标记
        /// 利用标记替换字符串中的值
        /// 添加标记及标记对应的值到MarkValues属性中
        /// </summary>
        /// <param name="input">字符串</param>
        /// <returns>字符串</returns>
        public override string MarkCharacteristic(string input)
        {
            if (this.Responsibility != null) input = this.Responsibility.MarkCharacteristic(input);
            return input;
        }
        /// <summary>
        /// 反标记
        /// 利用MarkValues属性中的标记及值还原字符串
        /// </summary>
        /// <param name="input">字符串</param>
        /// <returns>字符串</returns>
        public override string UnMarkCharacteristic(string input)
        {
            if (this.Responsibility != null) input = this.Responsibility.UnMarkCharacteristic(input);
            foreach (string key in _markValues.Keys)
            {
                input = input.Replace(key, _markValues[key]);
            }
            return input;
        }
    }
    /// <summary>
    /// 双引号内的字符串处理
    /// </summary>
    public sealed class QuotationMarkAuxiliary : MarkAuxiliary
    {
        public static readonly string Label = "#xf02{0}#";

        private readonly string GroupName = "content";

        //private readonly string QuotationLabel = "#xf02{0}#";

        private static int index = 0;

        protected override string pattern
        {
            get
            {
                return "[\\s=+\\-\\*/)(;]?(?<" + GroupName + ">\"[^\"]*\")[\\s=+\\-\\*/)(;]?";
            }
        }
        
        public override string MarkCharacteristic(string input)
        {
            MatchCollection mcs = Regex.Matches(input, pattern);
            SetMarks(mcs);
            foreach (string key in _markValues.Keys)
            {
                string value = _markValues[key];
                input = input.Replace(value, key);
            }
            return base.MarkCharacteristic(input);
        }

        private void SetMarks(MatchCollection mcs)
        {
            if (mcs==null || mcs.Count==0) return;
            foreach (Match mc in mcs)
            {
                Group gp = mc.Groups[GroupName];
                if (gp != null)
                {
                    _markValues.Add(string.Format(QuotationMarkAuxiliary.Label, index++), gp.Value);
                }
            }
        }

    }
    /// <summary>
    /// 反斜杠双引号的处理
    /// </summary>
    public sealed class BackslashQuotationMarkAuxiliary : MarkAuxiliary
    {
        private readonly string BackslashQuotationLabel = "#xf01#";

        protected override string pattern
        {
            get
            {
                return "\\\\\\\"";
            }
        }

        public override string MarkCharacteristic(string input)
        {
            input = Regex.Replace(input, pattern, BackslashQuotationLabel);
            this.SetMarks();
            return base.MarkCharacteristic(input);
        }

        private void SetMarks()
        {
            if (!_markValues.ContainsKey(BackslashQuotationLabel))
            {
                _markValues.Add(BackslashQuotationLabel, "\\\"");
            }
            else
            {
                _markValues[BackslashQuotationLabel] = "\\\"";
            }
        }
    }
    #endregion

    #region 字面值与真实值转换
    /// <summary>
    /// 数据类型值与字符序列串转换器
    /// </summary>
    public abstract class ConvertorAuxiliary
    {
        protected ConvertorAuxiliary Responsibility;
        public void SetResponsibility(ConvertorAuxiliary Responsibility)
        {
            this.Responsibility = Responsibility;
        }

        protected abstract Type type { get; }
        /// <summary>
        /// 字符序列串转成运算值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object ConvertorToObject(string value)
        {
            if (this.MatchNull(value)) return null;
            if (this.Match(value)) return this.ToValue(value);
            if (this.Responsibility != null) return this.Responsibility.ConvertorToObject(value);
            return null;
        }
        /// <summary>
        /// 运算值转字符序列串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ConvertorToText(object value)
        {
            if (value == null) return "null";
            if (value.GetType() == this.type) return this.ToText(value);
            if (this.Responsibility != null) return this.Responsibility.ConvertorToText(value);
            return "null";
        }
        /// <summary>
        /// 判断文本序列串的类型
        /// </summary>
        /// <param name="value">文本序列串</param>
        /// <returns>类型</returns>
        public Type JudgeType(string value)
        {
            if (this.Match(value)) return this.type;
            if (this.Responsibility != null) return this.Responsibility.JudgeType(value);
            return null;
        }
        /// <summary>
        /// 判断文本序列穿是否匹配当前类型
        /// </summary>
        /// <param name="value">文本序列串</param>
        /// <returns>true 匹配 false 不匹配</returns>
        protected abstract bool Match(string value);
        /// <summary>
        /// 转换文本值到真实值
        /// </summary>
        /// <param name="value">文本值</param>
        /// <returns>真实值</returns>
        protected abstract object ToValue(string value);
        /// <summary>
        /// 转换真实值到文本值
        /// </summary>
        /// <param name="value">真实值</param>
        /// <returns>文本值</returns>
        protected abstract string ToText(object value);

        /// <summary>
        /// null值处理
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool MatchNull(string value)
        {
            return Regex.IsMatch(value, "^\\s*null\\s*$");
        }

        /// <summary>
        /// 预处理字符串
        /// 去掉字符串中的\n\t\r\f特殊符号,并去掉首尾的空格
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string PretreatmentValue(string value)
        {
            value = Regex.Replace(value, "[\\n\\t\\r\\f]*", "");
            value = value.Trim();
            return value;
        }
    }

    /// <summary>
    /// 字符串数据类型
    /// string
    /// </summary>
    public sealed class StringConvertorAuxiliary : ConvertorAuxiliary
    {
        protected override bool Match(string value)
        {
            value = value.Replace("\\\"", "");
            return Regex.IsMatch(value, "^[\\s]*\"[^\"]*\"[\\s]*$");
            
        }

        protected override Type type
        {
            get { return typeof(string); }
        }

        protected override object ToValue(string value)
        {
            value = this.PretreatmentValue(value);
            return Regex.Replace(value, "(^\\s*\")|(\"\\s*$)", "");
        }

        protected override string ToText(object value)
        {
            return "\"" + value.ToString() + "\"";
        }

    }
    /// <summary>
    /// 整数数据类型
    /// int
    /// </summary>
    public sealed class IntegerConvertorAuxiliary : ConvertorAuxiliary
    {
        protected override Type type
        {
            get { return typeof(int); }
        }

        protected override bool Match(string value)
        {
            value = this.PretreatmentValue(value);
            string pattern = "^\\s*-?\\d+\\s*$";
            return Regex.IsMatch(value, pattern);
        }

        protected override object ToValue(string value)
        {
            value = this.PretreatmentValue(value);
            return int.Parse(value);
        }

        protected override string ToText(object value)
        {
            return value.ToString();
        }
    }
    /// <summary>
    /// 浮点数数据类型
    /// double
    /// </summary>
    public sealed class DoubleConvertorAuxiliary : ConvertorAuxiliary
    {

        protected override Type type
        {
            get { return typeof(double); }
        }

        protected override bool Match(string value)
        {
            value = this.PretreatmentValue(value);
            string pattern = "^\\s*[+\\-]?[0-9]*\\.?[0-9]+\\s*$";
            return Regex.IsMatch(value, pattern);
        }

        protected override object ToValue(string value)
        {
            value = this.PretreatmentValue(value);
            return double.Parse(value);
        }

        protected override string ToText(object value)
        {
            return value.ToString();
        }
    }
    /// <summary>
    /// 布尔数据类型
    /// bool
    /// </summary>
    public sealed class BooleanConvertorAuxiliary : ConvertorAuxiliary
    {

        protected override Type type
        {
            get { return typeof(bool); }
        }

        protected override bool Match(string value)
        {
            value = this.PretreatmentValue(value);
            value = value.ToLower();
            return Regex.IsMatch(value, "(?:^\\s*false\\s*$)|(?:^\\s*true\\s*$)");
        }

        protected override object ToValue(string value)
        {
            value = this.PretreatmentValue(value);
            return bool.Parse(value.ToLower());
        }

        protected override string ToText(object value)
        {
            return value.ToString();
        }
    }
    /// <summary>
    /// 日期数据类型
    /// DateTime
    /// </summary>
    public sealed class DateTimeConvertorAuxiliary : ConvertorAuxiliary
    {
        protected override Type type
        {
            get { return typeof(DateTime); }
        }

        protected override bool Match(string value)
        {
            value = this.PretreatmentValue(value);
            DateTime temp;
            return DateTime.TryParse(value, out temp);
        }

        protected override object ToValue(string value)
        {
            value = this.PretreatmentValue(value);
            return DateTime.Parse(value);
        }

        protected override string ToText(object value)
        {
            return value.ToString();
        }
    }

    #endregion

    #region 文本表达式函数运行器

    /// <summary>
    /// 运行文本表达式函数
    /// </summary>
    public abstract class FunctionAuxiliary
    {
        protected MarkAuxiliary _markAuxiliary = null;

        protected readonly string FuncGroupName = "func";

        protected readonly string ParsGroupName = "pars";

        protected string prealter = "[\\s=+\\-\\*/(;,%]?";

        protected string fixalter = "[\\s=+\\-\\*/)(;,%]?";

        protected string parsPattern
        {
            get
            {
                return "\\((?<" + ParsGroupName + ">[^@]*)\\)";
            }
        }

        protected string funcPattern
        {
            get
            {
                return "(?<" + FuncGroupName + ">" + EditLabel + "\\s*" + parsPattern +
                    ")";
            }
        }

        protected string pattern 
        { 
            get 
            {
                return prealter + funcPattern + fixalter;
            } 
        }

        /// <summary>
        /// 函数标识
        /// </summary>
        public abstract string EditLabel { get; }
        /// <summary>
        /// 函数执行提供类的全限定名
        /// </summary>
        protected abstract string functionProviderName { get; }
        /// <summary>
        /// 异常类全限定名
        /// </summary>
        protected abstract string exceptionProviderName { get; }
        /// <summary>
        /// 参数名称及类型
        /// （参数顺序从左到右）
        /// </summary>
        protected abstract Dictionary<string, Type> ParameterNameAndType { get; }
        /// <summary>
        /// 根据索引返回参数名称
        /// </summary>
        /// <param name="index">
        /// 参数在参数声明中的索引
        /// 从0开始的参数索引
        /// </param>
        /// <returns>参数名称</returns>
        protected string GetParameterName(int index)
        {
            if (index < 0 || index >= ParameterNameAndType.Count) return string.Empty;
            int temp = 0;
            foreach (string name in ParameterNameAndType.Keys)
            {
                if (temp == index) return name;
                temp++;
            }
            return string.Empty;
        }
        /// <summary>
        /// 根据索引返回参数类型
        /// </summary>
        /// <param name="index">
        /// 参数在参数声明中的索引 
        /// 从0开始的参数索引
        /// </param>
        /// <returns>类型</returns>
        protected Type GetParameterType(int index)
        {
            string name = this.GetParameterName(index);
            if (string.IsNullOrEmpty(name)) return null;
            return this.ParameterNameAndType[name];
        }
        /// <summary>
        /// 根据索引验证参数值是否匹配参数类型
        /// </summary>
        /// <param name="value">
        /// 参数在参数声明中的索引 
        /// 从0开始的参数索引
        /// </param>
        /// <param name="index">参数值</param>
        /// <returns>true 匹配 false 不匹配</returns>
        protected bool ValidateParameterType(string value, int index)
        {
            if (this._markAuxiliary != null) value = _markAuxiliary.UnMarkCharacteristic(value);
            Type mustType = this.GetParameterType(index);
            if (mustType == null) return true;
            Type beType = AuxiliaryBuilder.BuilderConvertor().JudgeType(value);
            return mustType.Equals(beType);
        }

        /// <summary>
        /// 验证参数序列串
        /// （例如"par1",par2,"par3"）
        /// </summary>
        /// <param name="parameterString">参数序列串</param>
        /// <param name="outParameters">
        /// 如果验证通过 则outParameters中存储真实的参数值
        /// outParameters在传入前必须实例化
        /// </param>
        /// <param name="functionString">原表达式文本</param>
        /// <returns>ValidateResult</returns>
        protected ValidateResult ParametersValidate(string parameterString,List<object> outParameters
            ,string functionString="")
        {
            outParameters.Clear();
            ValidateResult result = new ValidateResult();
            string pat = "\\s*,\\s*";
            string[] pars = Regex.Split(parameterString, pat);
            if (pars.Length != this.ParameterNameAndType.Count)
            {
                result.Errors.Add(functionString + " 参数个数不匹配");
                return result;
            }
            for (int i = 0; i < pars.Length; i++)
            {
                string par = pars[i];
                if (!this.ValidateParameterType(par, i))
                {
                    string parName = this.GetParameterName(i);
                    result.Errors.Add(string.Format(functionString + " 第{0}个参数{1}类型不正确，应为{2}型",
                        i + 1, parName, this.ParameterNameAndType[parName].Name));
                }
                else
                {
                    outParameters.Add(this.GetParameter(par));
                }
            }
            if (result.Errors.Count == 0) result = ValidateResult.Success;
            return result;
        }

        /// <summary>
        /// 返回真实的参数值
        /// </summary>
        /// <param name="value">参数序列串</param>
        /// <param name="index">
        /// 参数在参数声明中的索引 
        /// 从0开始
        /// </param>
        /// <returns>真实参数值</returns>
        protected object GetParameter(string value)
        {
            if (this._markAuxiliary != null) value = _markAuxiliary.UnMarkCharacteristic(value);
            
            return AuxiliaryBuilder.BuilderConvertor().ConvertorToObject(value);
        }

        /// <summary>
        /// 函数声明
        /// </summary>
        public string FunctionDeclare
        {
            get
            {
                string ret = EditLabel + "(";
                foreach (string par in ParameterNameAndType.Keys)
                {
                    ret += ParameterNameAndType[par].Name + " " + par + ",";
                }
                if (ParameterNameAndType.Count > 0) ret = ret.Substring(0, ret.Length - 1);
                return ret + ")";
            }
        }

        /// <summary>
        /// 执行输入字符串中的函数并返回执行完毕后的字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>结果字符串</returns>
        public string Perform(string input)
        {
            _markAuxiliary = AuxiliaryBuilder.BuilderMark();
            input = _markAuxiliary.MarkCharacteristic(input);

            MatchCollection mcs = Regex.Matches(input, pattern, RegexOptions.IgnoreCase);
            foreach (Match mc in mcs)
            {
                Group funcgp = mc.Groups[FuncGroupName];
                Group parsgp = mc.Groups[ParsGroupName];
                if (funcgp != null)
                {
                    List<object> truepars = new List<object>();
                    ValidateResult vr = ParametersValidate(parsgp.Value, 
                        truepars,_markAuxiliary.UnMarkCharacteristic(funcgp.Value));

                    if (vr != ValidateResult.Success)
                    {
                        object[] eps = new object[] { vr };
                        throw this.CreateException(eps);
                    }
                    else
                    {
                        try
                        {
                            string xx = AuxiliaryBuilder.BuilderConvertor().ConvertorToText(this.Run(truepars.ToArray()));
                            input = input.Replace(funcgp.Value, xx);
                        }
                        catch (Exception e)
                        {
                            if (e is FunctionException) throw e;
                            string message = _markAuxiliary.UnMarkCharacteristic(funcgp.Value) + " " + e.Message;
                            object[] eps = new object[] { message, e };
                            throw this.CreateException(eps);
                        }

                    }

                }
            }
            return _markAuxiliary.UnMarkCharacteristic(input);
        }

        /// <summary>
        /// 运行函数
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected object Run(object[] parameters)
        {
            IFunctionHelper proxy = (IFunctionHelper)Activator.CreateInstance(Type.GetType(functionProviderName), parameters);
            return proxy.Run();
        }
        /// <summary>
        /// 是否与文本函数匹配
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Match(string input)
        {
            _markAuxiliary = AuxiliaryBuilder.BuilderMark();
            input = _markAuxiliary.MarkCharacteristic(input);
            return Regex.IsMatch(input, pattern);
        }
        /// <summary>
        /// 创建特定异常
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected FunctionException CreateException(object[] parameters)
        {
            return Activator.CreateInstance(Type.GetType(exceptionProviderName), parameters) as FunctionException;
        }
    }

    /// <summary>
    /// 运行数据库ExecuteScalar操作
    /// </summary>
    public sealed class SqlScalarAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get
            {
                return "@SqlScalar";
            }
        }

        protected override string functionProviderName
        {
            get
            {
                return "Infrastructure.Expression.SqlScalar";
            }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.SqlScalarException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("providerName", typeof(string));
                    _parameterNameAndType.Add("connectionString", typeof(string));
                    _parameterNameAndType.Add("sqlStatement", typeof(string));
                }
                return _parameterNameAndType;
            }
        }

    }


    /// <summary>
    /// Len函数操作 
    /// </summary>
    public sealed class LenAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@Len"; }
        }

        protected override string functionProviderName
        {
            get{return "Infrastructure.Expression.Len";}
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.LenException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("stringValue", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }

    /// <summary>
    /// Sub函数操作 
    /// </summary>
    public sealed class SubAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@Sub"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.Sub"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.SubException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("stringValue", typeof(string));
                    _parameterNameAndType.Add("startPos", typeof(int));
                    _parameterNameAndType.Add("lenth", typeof(int));
                }
                return _parameterNameAndType;
            }
        }
    }
    /// <summary>
    /// Sum函数操作 
    /// </summary>
    public sealed class SumAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@Sum"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.Sum"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.SumException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("addDouble", typeof(double));
                    _parameterNameAndType.Add("addedDouble", typeof(double));
                }
                return _parameterNameAndType;
            }
        }
    }
    /// <summary>
    /// Replace函数操作 
    /// </summary>
    public sealed class ReplaceAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@Replace"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.Replace"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.ReplaceException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("strValue", typeof(string));
                    _parameterNameAndType.Add("oldStr", typeof(string));
                    _parameterNameAndType.Add("newStr", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }
    /// <summary>
    /// IN函数操作 
    /// </summary>
    public sealed class InAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@In"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.In"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.InException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("strValue", typeof(string));
                    _parameterNameAndType.Add("strParams", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }
    /// <summary>
    /// Like函数操作 
    /// </summary>
    public sealed class LikeAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@Like"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.Like"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.LikeException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("strValue", typeof(string));
                    _parameterNameAndType.Add("strLike", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }

    /// <summary>
    /// StartWith函数操作 
    /// </summary>
    public sealed class StartWithAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@StartWith"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.StartWith"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.StartWithException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("strValue", typeof(string));
                    _parameterNameAndType.Add("strLike", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }

    /// <summary>
    /// EndWith函数操作 
    /// </summary>
    public sealed class EndWithAuxiliary : FunctionAuxiliary
    {
        public override string EditLabel
        {
            get { return "@EndWith"; }
        }

        protected override string functionProviderName
        {
            get { return "Infrastructure.Expression.EndWith"; }
        }

        protected override string exceptionProviderName
        {
            get { return "Infrastructure.Expression.EndWithException"; }
        }

        private Dictionary<string, Type> _parameterNameAndType = null;

        protected override Dictionary<string, Type> ParameterNameAndType
        {
            get
            {
                if (_parameterNameAndType == null)
                {
                    _parameterNameAndType = new Dictionary<string, Type>();
                    _parameterNameAndType.Add("strValue", typeof(string));
                    _parameterNameAndType.Add("strLike", typeof(string));
                }
                return _parameterNameAndType;
            }
        }
    }
    #endregion

}
