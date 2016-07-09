using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Expression
{
    /// <summary>
    /// 几元操作符枚举
    /// </summary>
    public enum OperatorDimensionEnum
    {
        None=0,
        One=1,
        Two=2,
        Three = 3
    }
    /// <summary>
    /// 操作符处理创建器
    /// </summary>
    public class OperatorBuilder
    {
        private Dictionary<string, Operator> Operators = new Dictionary<string, Operator>();

        public OperatorBuilder()
        {
            Operator op = new And();
            Operators.Add(op.Label, op);
            Operators.Add("&", op);
            op = new SqlLike();
            Operators.Add(op.Label, op);
            op = new Or();
            Operators.Add(op.Label, op);
            Operators.Add("|", op);
            op = new Equal();
            Operators.Add(op.Label, op);
            op = new NotEqual();
            Operators.Add(op.Label, op);
            op = new LessThan();
            Operators.Add(op.Label, op);
            op = new GreaterThan();
            Operators.Add(op.Label, op);
            op = new LessEqual();
            Operators.Add(op.Label, op);
            op = new GreaterEqual();
            Operators.Add(op.Label, op);
            op = new Plus();
            Operators.Add(op.Label, op);
            op = new Minus();
            Operators.Add(op.Label, op);
            op = new Multiply();
            Operators.Add(op.Label, op);
            op = new Divide();
            Operators.Add(op.Label, op);
            op = new Mod();
            Operators.Add(op.Label, op);
            //op = new Positive();
            //Operators.Add(op.Label, op);
            //op = new Negative();
            //Operators.Add(op.Label, op);
            op = new Non();
            Operators.Add(op.Label, op);
            op = new LeftParen();
            Operators.Add(op.Label, op);
            op = new RightParen();
            Operators.Add(op.Label, op);
            op = new CSub();
            Operators.Add(op.Label, op);
            op = new CLen();
            Operators.Add(op.Label, op);
            op = new CLink();
            Operators.Add(op.Label, op);
        }
        /// <summary>
        /// 根据操作符标签获取操作符
        /// </summary>
        /// <param name="Label">操作符标签</param>
        /// <returns>操作符</returns>
        public Operator Match(string Label)
        {
            return Operators.ContainsKey(Label) ? Operators[Label] : null;
        }
        /// <summary>
        /// 判断当前标签是否存在对应的操作符
        /// （换言之，可判断当前标签是否是操作符）
        /// </summary>
        /// <param name="Label">操作符标签</param>
        /// <returns>true 存在(是) false 不存在(否)</returns>
        public bool IsMatch(string Label)
        {
            return Operators.ContainsKey(Label);
        }

        /// <summary>
        /// 判断是否为左括弧
        /// </summary>
        /// <param name="value">操作符</param>
        /// <returns>true 是 false 不是</returns>
        public bool IsLeftParen(Operator value) { if (value == null) return false; return IsLeftParen(value.Label); }
        /// <summary>
        /// 判断是否为左括弧
        /// </summary>
        /// <param name="Label">字符</param>
        /// <returns>true 是 false 不是</returns>
        public bool IsLeftParen(string Label) { return Label == "(" ? true : false; }
        /// <summary>
        /// 判断是否为右括弧
        /// </summary>
        /// <param name="value">操作符</param>
        /// <returns>true 是 false 不是</returns>
        public bool IsRightParen(Operator value) { if (value == null) return false; return IsRightParen(value.Label); }
        /// <summary>
        /// 判断是否为右括弧
        /// </summary>
        /// <param name="Label">字符</param>
        /// <returns>true 是 false 不是</returns>
        public bool IsRightParen(string Label) { return Label == ")" ? true : false; }
    }
    /// <summary>
    /// 操作符
    /// </summary>
    public abstract class Operator
    {
        protected static ConvertorAuxiliary Convertor = AuxiliaryBuilder.BuilderConvertor();
        /// <summary>
        /// 优先级
        /// </summary>
        public abstract int Pri { get; }
        /// <summary>
        /// 操作符
        /// </summary>
        public abstract string Label { get; }
        /// <summary>
        /// 几元操作符
        /// </summary>
        public abstract OperatorDimensionEnum Dimension { get; }
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="pars">参数</param>
        /// <returns>执行结果</returns>
        public abstract object Run(List<object> pars);
        /// <summary>
        /// 执行操作
        /// </summary>
        /// <param name="pars">参数</param>
        /// <returns>执行结果</returns>
        public object Run(object[] pars)
        {
            List<object> result = new List<object>();
            if (pars != null && pars.Count() > 0)
            {
                foreach (object par in pars) { result.Add(par); }
            }
            return Run(result);
        }
        /// <summary>
        /// 验证操作参数是否正确
        /// </summary>
        /// <param name="pars">参数</param>
        /// <returns>验证结果 正确返回ValidateResult.SUCCESS </returns>
        public virtual ValidateResult ValidateParameters(List<object> pars)
        {
            ValidateResult result = new ValidateResult();
            int DimensionCount = (int)this.Dimension;
            if (DimensionCount==0) return ValidateResult.Success;
            if (pars == null || pars.Count != DimensionCount)
            {
                result.Errors.Add(string.Format("运算符“{0}”缺少操作数,运算符“{0}”为{1}元运算符.", this.Label, DimensionCount));
                return result;
            }
            List<Type> parTypes = new List<Type>();
            
            foreach (object par in pars)
            {
                parTypes.Add(Convertor.JudgeType(par.ToString()));
            }
            if (!ValidateParametersType(parTypes))
            {
                string err=" ";
                foreach(Type tt in parTypes)
                {
                    if (tt != null)
                    {
                        err += "“" + tt.Name + "”和";
                    }
                }
                err=err.Substring(0,err.Length-1);
                result.Errors.Add(string.Format(
                    "运算符“{0}”无法应用于" + err + "类型的操作数", this.Label));
                return result;
            }
            return ValidateResult.Success;
        }
        /// <summary>
        /// 验证操作参数是否正确
        /// </summary>
        /// <param name="pars">参数</param>
        /// <returns>验证结果 正确返回ValidateResult.SUCCESS </returns>
        public ValidateResult ValidateParameters(object[] pars)
        {
            List<object> result=new List<object>();
            if (pars != null && pars.Count() > 0)
            {
                foreach (object par in pars) { result.Add(par); }
            }
            return ValidateParameters(result);
        }
        /// <summary>
        ///  验证操作参数类型是否正确
        ///  (假设操作数个数正确)
        /// </summary>
        /// <param name="ParTypes">参数类型</param>
        /// <returns>true 匹配 false不匹配</returns>
        protected virtual bool ValidateParametersType(List<Type> ParTypes) { return false; }

        public override string ToString()
        {
            return this.Label;
        }
    }
    /// <summary>
    /// 与操作
    /// </summary>
    public class And : Operator
    {
        public override int Pri
        {
            get { return 1; }
        }

        public override string Label
        {
            get { return "&&"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return bool.Parse(val1.ToString()) && bool.Parse(val2.ToString());
        }

        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            Type bt = typeof(bool);
            if (bt.Equals(ParTypes[0]) && bt.Equals(ParTypes[1])) return true;
            return false;
        }
    }
    /// <summary>
    /// 或操作
    /// </summary>
    public class Or : Operator
    {
        public override int Pri
        {
            get { return 1; }
        }

        public override string Label
        {
            get { return "||"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return bool.Parse(val1.ToString()) || bool.Parse(val2.ToString());
        }

        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            Type bt = typeof(bool);
            if (bt.Equals(ParTypes[0]) && bt.Equals(ParTypes[1])) return true;
            return false;
        }
    }
    /// <summary>
    /// like
    /// </summary>
    public class SqlLike : Operator
    {
        public override int Pri
        {
            get { return 2; }
        }

        public override string Label
        {
            get { return "∩"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            if (val2 == null || val1 == null) return false;
            if (val2.ToString().IndexOf("%") == 0) {
                if (val2.ToString().IndexOf(val1.ToString()) == 0) return true;
            }
            if (val2.ToString().LastIndexOf("%") == 0)
            {
                if (val2.ToString().LastIndexOf(val1.ToString()) == 0) return true;
            }
            if (val1.ToString().Contains(val2.ToString())) return true;
            return false;
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            //只能用于 int 或者 double boolean dateTime
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            if (t1.Equals(t2)) return true;
            Type tString = typeof(string);
            if ((t1.Equals(tString) && t2.Equals(tString)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 等于
    /// </summary>
    public class Equal : Operator
    {

        public override int Pri
        {
            get { return 2; }
        }

        public override string Label
        {
            get { return "="; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
          
            object val1 = Convertor.ConvertorToObject((pars[0]+"").Trim());
            object val2 = Convertor.ConvertorToObject((pars[1] + "").Trim());

            if (val1 == null && val2 == null) { 
                return   (pars[0] + "") == (pars[1] + "");
            };
            if (val1 == null) return val2.Equals(val1);
            if (val2 == null) return val1.Equals(val2);
            if (val1.ToString() == val2.ToString()) return true;
            return val1.Equals(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            //只能用于 int 或者 double boolean dateTime
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            if (t1 == null && t2 == null) return true;
            if (t1.Equals(t2)) return true;
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) && t2.Equals(tDouble)) || (t2.Equals(tInt) || t1.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 不等于
    /// </summary>
    public class NotEqual : Operator
    {

        public override int Pri
        {
            get { return 2; }
        }

        public override string Label
        {
            get { return "!="; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject((pars[0]+"").Trim());
            object val2 = Convertor.ConvertorToObject((pars[1] + "").Trim());
            if (val1.ToString() != val2.ToString()) return true;
            if (val1 == null && val2 == null) return false;
            if (val1 == null) return !val2.Equals(val1);
            if (val2 == null) return !val1.Equals(val2);
            return !val1.Equals(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            if (t1 == t2) return true;
            if (t1 == null || t2 == null) return false;
            if (t1.Equals(t2)) return true;
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) && t2.Equals(tDouble)) || (t2.Equals(tInt) || t1.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 小于
    /// </summary>
    public class LessThan : Operator
    {

        public override int Pri
        {
            get { return 3; }
        }

        public override string Label
        {
            get { return "<"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) < Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 大于
    /// </summary>
    public class GreaterThan : Operator
    {

        public override int Pri
        {
            get { return 3; }
        }

        public override string Label
        {
            get { return ">"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) > Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 小于等于
    /// </summary>
    public class LessEqual : Operator
    {

        public override int Pri
        {
            get { return 3; }
        }

        public override string Label
        {
            get { return "<="; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) <= Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 大于等于
    /// </summary>
    public class GreaterEqual : Operator
    {

        public override int Pri
        {
            get { return 3; }
        }

        public override string Label
        {
            get { return ">="; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) >= Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            //只能用于 int 或者 double
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 加法操作
    /// </summary>
    public class Plus : Operator
    {

        public override int Pri
        {
            get { return 4; }
        }

        public override string Label
        {
            get { return "+"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            Type par1Type = Convertor.JudgeType(pars[0].ToString());
            Type par2Type = Convertor.JudgeType(pars[1].ToString());
            object val1=Convertor.ConvertorToObject(pars[0].ToString());
            object val2=Convertor.ConvertorToObject(pars[1].ToString());
            if (par1Type == typeof(string) || par2Type == typeof(string))
            {
                return "\"" + val1.ToString() + val2.ToString() + "\"";
            }
            else
            {
                return Convert.ToDouble(val1) + Convert.ToDouble(val2);
            }
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 减法操作
    /// </summary>
    public class Minus : Operator
    {
        public override int Pri
        {
            get { return 4; }
        }

        public override string Label
        {
            get { return "-"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) - Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            if (t1 == null || t2 == null) return false;
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 乘法操作
    /// </summary>
    public class Multiply : Operator
    {
        public override int Pri
        {
            get { return 5; }
        }

        public override string Label
        {
            get { return "*"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) * Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 除法操作
    /// </summary>
    public class Divide : Operator
    {
        public override int Pri
        {
            get { return 5; }
        }

        public override string Label
        {
            get { return "/"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) / Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            //只能用于 int 或者 double
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt=typeof(int);
            Type tDouble=typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 取模操作
    /// </summary>
    public class Mod : Operator
    {
        public override int Pri
        {
            get { return 5; }
        }

        public override string Label
        {
            get { return "%"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            object val2 = Convertor.ConvertorToObject(pars[1].ToString());
            return Convert.ToDouble(val1) % Convert.ToDouble(val2);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null Int32
            Type t1 = ParTypes[0];
            Type t2 = ParTypes[1];
            Type tInt = typeof(int);
            Type tDouble = typeof(double);
            if ((t1.Equals(tInt) || t1.Equals(tDouble)) && (t2.Equals(tInt) || t2.Equals(tDouble)))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 正号
    /// </summary>
    public class Positive : Operator
    {
        public override int Pri
        {
            get { return 6; }
        }

        public override string Label
        {
            get { return "+"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.One; }
        }

        public override object Run(List<object> pars)
        {
            throw new NotImplementedException();
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double Int32
            Type t1 = ParTypes[0];
            if ((t1.Equals(typeof(int)) || t1.Equals(typeof(double))))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 负号
    /// </summary>
    public class Negative : Operator
    {
        public override int Pri
        {
            get { return 6; }
        }

        public override string Label
        {
            get { return "-"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.One; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            return 0- Convert.ToDouble(val1);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
         {
            Type t1 = ParTypes[0];
            if ((t1.Equals(typeof(int)) || t1.Equals(typeof(double))))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 非    /// </summary>
    public class Non : Operator
    {
        public override int Pri
        {
            get { return 6; }
        }

        public override string Label
        {
            get { return "!"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.One; }
        }

        public override object Run(List<object> pars)
        {
            object val1 = Convertor.ConvertorToObject(pars[0].ToString());
            return !Convert.ToBoolean(val1);
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            //Double DateTime Boolean String null
            Type t1 = ParTypes[0];
            Type tBool = typeof(bool);
            if (t1.Equals(tBool))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 左括号
    /// </summary>
    public class LeftParen : Operator
    {
        public override int Pri
        {
            get { return 99; }
        }

        public override string Label
        {
            get { return "("; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.None; }
        }

        public override object Run(List<object> pars)
        {
            throw new NotImplementedException();
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            return true;
        }
    }
    /// <summary>
    /// 右括号
    /// </summary>
    public class RightParen : Operator
    {
        public override int Pri
        {
            get { return 99; }
        }

        public override string Label
        {
            get { return ")"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.None; }
        }

        public override object Run(List<object> pars)
        {
            throw new NotImplementedException();
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            return true;
        }
    }
    /// <summary>
    /// 求和
    /// </summary>
    public class CSub : Operator
    {
        public override int Pri
        {
            get { return 100; }
        }

        public override string Label
        {
            get { return "@Sub"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            return (double.Parse(pars[0].ToString()) + double.Parse(pars[1].ToString()));
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            Type t1 = ParTypes[0], t2 = ParTypes[1];;
            if ((t1.Equals(typeof(int)) || (t1.Equals(typeof(double)))) && (t2.Equals(typeof(int)) || (t2.Equals(typeof(double)))))
            {
                return true;
            }
            return false;
        }
    }
    /// <summary>
    /// 长度
    /// </summary>
    public class CLen : Operator
    {
        public override int Pri
        {
            get { return 100; }
        }

        public override string Label
        {
            get { return "@Len"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.One; }
        }

        public override object Run(List<object> pars)
        {
            if (pars[0] == null) return 0;
            return pars[0].ToString().Length;
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            return true;
        }
    }

    /// <summary>
    /// 连接
    /// </summary>
    public class CLink : Operator
    {
        public override int Pri
        {
            get { return 100; }
        }

        public override string Label
        {
            get { return "@Link"; }
        }

        public override OperatorDimensionEnum Dimension
        {
            get { return OperatorDimensionEnum.Two; }
        }

        public override object Run(List<object> pars)
        {
            if (pars[0] == null) return 0;
            return pars[0].ToString().TrimEnd('"').TrimStart('"') + pars[1].ToString().TrimEnd('"').TrimStart('"');
        }
        protected override bool ValidateParametersType(List<Type> ParTypes)
        {
            if (ParTypes[0] == null || ParTypes[1] == null) return false;
            return true;
        }
    }
}
