using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Infrastructure.Expression
{
    /// <summary>
    /// 元数据单元

    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MetaUnit
    {
        private bool _isOperator = false;
        /// <summary>
        /// 元数据单元内容是否为操作符

        /// </summary>
        public bool IsOperator
        {
            get { return _isOperator; }
        }

        private object _value;
        /// <summary>
        /// 元数据单元内容

        /// 如果为操作符则为Operator对象，其余皆为字符串
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        private int _expressionStartPosition = 0;
        /// <summary>
        /// 表达式中元数据单元所包含的

        /// 字符串在表达式中的起始位置

        /// </summary>
        public int StartPosition
        {
            get { return _expressionStartPosition; }
            set { _expressionStartPosition = value; }
        }
        /// <summary>
        /// 元数据单元

        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="expressionStartPosition">在表达式中的起始位置</param>
        /// <param name="isOperator">是否是操作符</param>
        public MetaUnit(object content, int expressionStartPosition, bool isOperator)
        {
            this._value = content;
            this._expressionStartPosition = expressionStartPosition;
            this._isOperator = isOperator;
        }
        /// <summary>
        /// 元数据单元

        /// (默认为非操作符)
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="expressionStartPosition">在表达式中的起始位置</param>
        public MetaUnit(object content, int expressionStartPosition) : this(content, expressionStartPosition, false) { }
        /// <summary>
        /// 元数据单元

        /// (默认为非操作符)
        /// (默认在表达式中的起始位置为0)
        /// </summary>
        /// <param name="content">内容</param>
        public MetaUnit(object content) : this(content, 0, false) { }
    }

    /// <summary>
    /// 实体链实体

    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DoubleChainEntry
    {
        private object _value=null;

        private DoubleChainEntry _prev=null;
        /// <summary>
        /// 设置上游链实体

        /// </summary>
        public DoubleChainEntry Prev
        {
            get { return _prev; }
            set 
            {
                if (!this.Equals(value) && this._prev != value)
                {
                    this._prev = value;
                    value.Next = this;
                }
            }
        }

        private DoubleChainEntry _next=null;
        /// <summary>
        /// 设置下游链实体

        /// </summary>
        public DoubleChainEntry Next
        {
            get { return _next; }
            set 
            {
                if (!this.Equals(value) && this._next != value)
                {
                    this._next = value;
                    value.Prev = this;
                }
            }
        }

        /// <summary>
        /// 判断当前实体是否为所在双向链条中的末尾实体

        /// true 是 false 否

        /// </summary>
        public bool IsTailInChian { get { return this.Next == null ? true : false; } }

        /// <summary>
        /// 判断当前实体是否为所在双向链条中的头实体
        /// true 是 false 否

        /// </summary>
        public bool IsHeaderInChian { get { return this.Prev == null ? true : false; } }

        /// <summary>
        /// 获取当前实体所在双向链条中的头实体
        /// </summary>
        public DoubleChainEntry HeaderInChian
        {
            get
            {
                if (this.IsHeaderInChian) return this;
                return this.Prev.HeaderInChian;
            }
        }

        /// <summary>
        /// 获取当前实体所在双向链条中的末尾实体

        /// </summary>
        public DoubleChainEntry TailInChian
        {
            get
            {
                if (this.IsTailInChian) return this;
                return this.Next.TailInChian;
            }
        }

        /// <summary>
        /// 获取链实体所包含的内容

        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetValue<T>() where T:class
        {
            return _value as T;
        }
        /// <summary>
        /// 拷贝
        /// （拷贝的链实体上下游都为null)
        /// </summary>
        /// <returns></returns>
        public DoubleChainEntry Clone()
        {
            DoubleChainEntry result = new DoubleChainEntry(_value);
            result._next = null;
            result._prev = null;
            return result;
        }

        public DoubleChainEntry(object value) { this._value = value; }
    }

    /// <summary>
    /// 实体链生成器
    /// </summary>
    public sealed class DoubleChainBuilder
    {
        private DoubleChainEntry _header = null;
        private DoubleChainEntry _tail = null;
        /// <summary>
        /// 在末尾添加链实体
        /// </summary>
        /// <param name="value">链实体</param>
        public void Append(DoubleChainEntry value)
        {
            if (value == null) return;
            value = value.Clone();
            if (_header == null)
            {
                _header = value;
                _tail = value;
            }
            else
            {
                _tail.Next = value;
                _tail = value;
            }
        }
        /// <summary>
        /// 获得实体链头
        /// </summary>
        public DoubleChainEntry Header { get { return _header; } }
        /// <summary>
        /// 获得实体链尾
        /// </summary>
        public DoubleChainEntry Tail { get { return _tail; } }

        public override string ToString()
        {
            string ret = string.Empty;
            DoubleChainEntry temp = this.Header;
            while (temp != null)
            {
                MetaUnit meta = temp.GetValue<MetaUnit>();
                ret += meta.Value.ToString() + ",";
                temp = temp.Next;
            }
            if (ret != string.Empty) ret = ret.Substring(0, ret.Length - 1);
            return ret;
        }
    }
    /// <summary>
    /// 运行表达式

    /// </summary>
    public class ExpressionParser
    {
        /// <summary>
        /// 操作符创建器
        /// </summary>
        private OperatorBuilder operatorBuilder = new OperatorBuilder();
        /// <summary>
        /// 运行表达式并返回运行结果
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>结果</returns>
        [Obsolete("已被新方法ValidateResult Parser(string expression, out object result)替代")]
        protected object Parser(string expression)
        {
            if (this.ValidateParen(expression) && this.ValidateQuotation(expression))
            {
                DoubleChainBuilder chain = this.CompileInfixexpression(expression);
                DoubleChainBuilder post = PostfixExpressionConverter(chain);
                ValidateResult vr = new ValidateResult();
                return Run(post,out vr);
            }
            return null;
        }
        /// <summary>
        /// 运行表达式并返回运行结果
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="result">运行结果</param>
        /// <returns>
        /// 返回错误消息
        /// 如果表达式正常则返回ValidateResult.SUCCESS
        /// </returns>
        public ValidateResult Parser(string expression, out object result)
        {
            result = null;
            ValidateResult Err=new ValidateResult();
            if (!this.ValidateParen(expression))
            {
                Err.Errors.Add("表达式中的小括号不完整");
            }
            if (!this.ValidateQuotation(expression))
            {
                Err.Errors.Add("表达式中字符串缺少\"符号");
            }
            if (Err.Errors.Count > 0) return Err;
            DoubleChainBuilder chain = this.CompileInfixexpression(expression);
            DoubleChainBuilder post = PostfixExpressionConverter(chain);
            result = Run(post, out Err);
            return Err;
        }

        /// <summary>
        /// 运行表达式并返回结果
        /// </summary>
        /// <param name="PostfixExpression">后缀表达式链</param>
        /// <returns>结果</returns>
        public object Run(DoubleChainBuilder PostfixExpression,out ValidateResult validation)
        {
            validation = new ValidateResult();
            DoubleChainEntry dEntry = PostfixExpression.Header;
            Stack<object> Operand = new Stack<object>();//操作数栈
            while (dEntry != null)
            {
                MetaUnit meta = dEntry.GetValue<MetaUnit>();
                if (!meta.IsOperator)
                {
                    Operand.Push(meta.Value);
                }
                else
                {
                    Operator oper = meta.Value as Operator;
                    List<object> pars = new List<object>();
                    switch (oper.Dimension)
                    {
                        case OperatorDimensionEnum.One:
                            pars.Add(Operand.Pop());
                            break;
                        case OperatorDimensionEnum.Two:
                            pars.Add(Operand.Pop());
                            pars.Add(Operand.Pop());
                            break;
                    }
                    pars.Reverse();
                    validation = oper.ValidateParameters(pars);
                    if (validation != ValidateResult.Success) return null;
                    object result =  oper.Run(pars);
                    Operand.Push(result);
                }
                dEntry = dEntry.Next;
            }
            validation = ValidateResult.Success;
            if (Operand.Count == 0) return null;
            return Operand.Pop();
        }
        /// <summary>
        /// 编译表达式为中缀表达式元数据单元链

        /// (使用前表达式必须ValidateQuotation()为有效)
        /// (使用前表达式必须ValidateParen()为有效)
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>元数据单元链</returns>
        public DoubleChainBuilder CompileInfixexpression(string expression)
        {
            string value = Regex.Replace(expression, "[\\n\\t\\r\\f]*", "");
            expression = this.EliminateWhiteSpace(expression);
            char[] values = expression.ToCharArray();
            int length = values.Length;
            DoubleChainBuilder chainBuilder = new DoubleChainBuilder();
            int prevOperatorEndPos = -1; //上一操作符结束位置            int curOperatorEndPos=0;//当前操作符结束位置            int cindex = 0;
            while (cindex < length)
            {
                char current = values[cindex];
                //if (current == ',') { cindex++; continue; }//逗号继续
                //当前字符为操作符或操作符的一部分并且操作符不是文本串内的内容
                if ((this.operatorBuilder.IsMatch(current.ToString()) &&
                    this.FetchQuotationCount(values, 0, cindex - 1) % 2 == 0)||current==',')
                {
                    curOperatorEndPos = this.FetchOperatorEndPos(values, cindex);
                    //如果上一个操作符到当前操作符之间存在内容,
                    //则保存内容到元数据单元链表                    if (prevOperatorEndPos + 1 < cindex)
                    {
                        string upCurStr = expression.Substring(prevOperatorEndPos + 1, cindex - prevOperatorEndPos - 1);
                        bool isMatch=this.operatorBuilder.IsMatch(upCurStr);
                        MetaUnit me = null;
                        if (isMatch)
                        {
                            Operator oper = operatorBuilder.Match(upCurStr);
                            me = new MetaUnit(oper, prevOperatorEndPos + 1, isMatch);
                        }
                        else {
                            me = new MetaUnit(upCurStr, prevOperatorEndPos + 1, isMatch);
                        }
                        DoubleChainEntry contentEntry = new DoubleChainEntry(me);
                        chainBuilder.Append(contentEntry);
                    }
                    //保存当前操作符到元数据单元链表
                    if (current != ',')
                    {
                        Operator oper = operatorBuilder.Match(expression.Substring(cindex, curOperatorEndPos - cindex + 1));
                        //如果操作符为+或-,看是否是负号或正号操作符还是加或减操作符
                        //(正负号的判断依据:看之前的元数据是否是null或是操作符(除'+','-',')'外))
                        if (oper.Label == "+" || oper.Label == "-")
                        {
                            DoubleChainEntry prevEntry = chainBuilder.Tail;
                            MetaUnit prevmeta = prevEntry == null ? null : prevEntry.GetValue<MetaUnit>();
                            if (prevmeta == null ||
                                (prevmeta.IsOperator &&
                                 ((Operator)prevmeta.Value).Label != ")" &&
                                 ((Operator)prevmeta.Value).Label != "-" &&
                                 ((Operator)prevmeta.Value).Label != "+"))
                            {
                                if (oper.Label == "+")
                                {
                                    oper = new Positive();
                                }
                                else
                                {
                                    oper = new Negative();
                                }
                            }
                        }
                        DoubleChainEntry operEntry = new DoubleChainEntry(new MetaUnit(
                            oper, cindex, true));
                        chainBuilder.Append(operEntry);
                    }
                    //保护现场环境参数
                    cindex = curOperatorEndPos + 1; //跳过当前操作符,指向操作符的下一个字符                    prevOperatorEndPos = curOperatorEndPos;//保存当前操作符的结束位置到prevOperatorEndPos中                }
                else
                {
                    cindex++;
                }

            }
            //获取末尾到上一个操作符之间的内容            if (prevOperatorEndPos + 1 < length)
            {
                DoubleChainEntry contentEntry = new DoubleChainEntry(new MetaUnit(
                            expression.Substring(prevOperatorEndPos + 1, length - prevOperatorEndPos - 1),
                            prevOperatorEndPos + 1, false));
                chainBuilder.Append(contentEntry);
            }
            return chainBuilder;
        }
        /// <summary>
        /// 中缀表达式转换为后缀表达式        /// (中缀表达式a + b*c + (d * e + f) * g，其转换成后缀表达式则为a b c * + d e * f  + g * +)
        /// </summary>
        /// <param name="Infixexpression">中缀表达式数据链</param>
        /// <returns></returns>
        public DoubleChainBuilder PostfixExpressionConverter(DoubleChainBuilder Infixexpression)
        {
            DoubleChainBuilder PostFix = new DoubleChainBuilder();
            Stack<DoubleChainEntry> tempOperStack = new Stack<DoubleChainEntry>();
            DoubleChainEntry curEntry = Infixexpression.Header;
            while (curEntry!= null)
            {
                MetaUnit metaValue = curEntry.GetValue<MetaUnit>();
                if (metaValue.IsOperator) //操作符                {
                    if (tempOperStack.Count==0) 
                        tempOperStack.Push(curEntry);
                    else
                    {
                        Operator Oper = metaValue.Value as Operator;
                        if (operatorBuilder.IsRightParen(Oper)) //如果是右括号
                        {
                            while (tempOperStack.Count > 0)
                            {
                                DoubleChainEntry dce = tempOperStack.Pop();
                                Operator dceOper = dce.GetValue<MetaUnit>().Value as Operator;
                                while (operatorBuilder.IsLeftParen(dceOper)) {
                                    dce = tempOperStack.Pop();
                                    dceOper = dce.GetValue<MetaUnit>().Value as Operator;
                                }
                                PostFix.Append(dce);
                                break;
                            }
                        }
                        else
                        {
                            if (operatorBuilder.IsLeftParen(Oper)) //如果是左括号
                            {
                                tempOperStack.Push(curEntry);
                            }
                            else
                            {
                                while (tempOperStack.Count > 0)
                                {
                                    Operator prev = tempOperStack.Peek().GetValue<MetaUnit>().Value as Operator;
                                    if (prev.Pri >= Oper.Pri && prev.Pri!=99)
                                    {
                                        DoubleChainEntry dce = tempOperStack.Pop();
                                        
                                        PostFix.Append(dce);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                tempOperStack.Push(curEntry);
                            }
                        }
                    }

                }
                else //非操作符
                {
                    PostFix.Append(curEntry);
                }
                curEntry = curEntry.Next;
            }
            while (tempOperStack.Count > 0)
            {
                DoubleChainEntry dce = tempOperStack.Pop();
                PostFix.Append(dce);
            }
            return PostFix;
        }

        /// <summary>
        /// 获取文本内容结束位置
        /// </summary>
        /// <param name="values">字符数组</param>
        /// <param name="TextStartPos">文本在字符数组中开始位置</param>
        /// <returns>获取到则返回位置，否则返回-1</returns>
        private int FetchTextEndPos(char[] values,int TextStartPos)
        {
            for (int i = TextStartPos + 1; i < values.Length; i++)
            {
                if (this.IsQuotation(values, i))
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// 获取操作符结束位置

        /// </summary>
        /// <param name="values">字符数组</param>
        /// <param name="OperatorStartPos">操作符在字符数组中开始位置</param>
        /// <returns>获取到则返回位置，否则返回-1</returns>
        private int FetchOperatorEndPos(char[] values, int OperatorStartPos)
        {
            string Oper = values[OperatorStartPos].ToString();
            for (int i = OperatorStartPos + 1; i < values.Length; i++)
            {
                if (values[i] != ' ')
                {
                    Oper += values[i].ToString();
                    if (!operatorBuilder.IsMatch(Oper))
                    {
                        return i - 1;
                    }
                }
            }
            return OperatorStartPos;
        }

        /// <summary>
        /// 检查表达式中的双引号文本是否都闭合
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>true 正确 false 不正确</returns>
        public bool ValidateQuotation(string expression)
        {
            char[] chArray = expression.Trim().ToCharArray();
            bool result = true;
            for (int i = 0; i < chArray.Length; i++)
            {
                if (IsQuotation(chArray, i)) result = !result;
            }
            return result;
        }
        /// <summary>
        /// 检查表达式中的小括号是否都闭合
        /// </summary>
        /// <param name="expression">表达式</param>
        /// <returns>true 正确 false 不正确</returns>
        public bool ValidateParen(string expression)
        {
            expression=AuxiliaryBuilder.BuilderMark().MarkCharacteristic(expression);
            char[] chArray = expression.Trim().ToCharArray();
            int leftparen = 0;
            int rightparen = 0;
            for (int i = 0; i < chArray.Length; i++)
            {
                if (operatorBuilder.IsLeftParen(chArray[i].ToString())) leftparen++;
                else if (operatorBuilder.IsRightParen(chArray[i].ToString())) rightparen++;
            }
            return (leftparen == rightparen);
        }

        /// <summary>
        /// 判断字符数组中指定索引的值是否是双引号        /// </summary>
        /// <param name="values">字符数组</param>
        /// <param name="index">索引 从0开始</param>
        /// <returns>true 是 false 不是</returns>
        private bool IsQuotation(char[] values, int index)
        {
            if (index < 0 || index >= values.Length) return false;
            char current=values[index];
            if (current == '"' && !(index > 0 && values[index - 1] == '\\')) return true;
            return false;
        }
        /// <summary>
        /// 判断表达式中指定索引的值是否是双引号        /// </summary>
        /// <param name="expression">表达式</param>
        /// <param name="index">索引 从0开始</param>
        /// <returns>true 是 false 不是</returns>
        private bool IsQuotation(string expression, int index)
        {
            return IsQuotation(expression.ToCharArray(), index);
        }
        /// <summary>
        /// 剔除字符数组中非文本内的空格
        /// (使用前表达式必须ValidateQuotation为有效)
        /// </summary>
        /// <param name="values">字符数组</param>
        /// <returns>表达式</returns>
        private string EliminateWhiteSpace(char[] values)
        {
            string result = string.Empty;
            int length=values.Length;
            int cindex=0;
            char current;
            while (cindex < length)
            {
                if (this.IsQuotation(values, cindex))
                {
                    int endpos = this.FetchTextEndPos(values, cindex);
                    for (int i = cindex; i <= endpos; i++)
                    {
                        result += values[i].ToString();
                    }
                    cindex = endpos + 1;
                }
                else
                {
                    current = values[cindex];
                    if (current != ' ') { result += current.ToString(); }
                    cindex++;
                }
            }
            return result;
        }
        /// <summary>
        /// 剔除表达式中非文本内的空格        /// (使用前表达式必须ValidateQuotation为有效)
        /// </summary>
        /// <param name="values">表达式</param>
        /// <returns>表达式</returns>
        private string EliminateWhiteSpace(string expression)
        {
            return EliminateWhiteSpace(expression.ToCharArray());
        }
        /// <summary>
        /// 获取字符数组中双引号的个数（\"除外)
        /// </summary>
        /// <param name="values">字符数组</param>
        /// <param name="startPos">扫描开始位置</param>
        /// <param name="endPos">扫描结束位置</param>
        /// <returns>个数</returns>
        private int FetchQuotationCount(char[] values, int startPos, int endPos)
        {
            int result = 0;
            for (int i = startPos; i <= endPos; i++)
            {
                if (this.IsQuotation(values, i)) result++;
            }
            return result;
        }
    }
    //函数变为字符
    public class FuntionChangeChar 
    {
        private List<string> FunListLToR = null;//函数按位置从左到右的集合
        private List<string> FunList = new List<string>() { "@Sub" };
        public FuntionChangeChar() 
        {
            FunListLToR = new List<string>();
        }
        public string GetFunsChangeExp(string exp) 
        {
           return GetFunsChangeExp(FunList, exp);
        }
        private  string GetFunsChangeExp(List<string> funList, string exp)
        {
            string strExpprision = exp;
            Dictionary<int, string> funPosDic = new Dictionary<int, string>();
            List<int> sortList = new List<int>();
            foreach (var fun in funList)
            {
                GetFunDic(fun, exp, ref funPosDic, ref sortList);
                strExpprision = strExpprision.Replace(fun, "@");
            }
            SetFunsDic(funPosDic, sortList);
            return strExpprision;
        }
        /// <summary>
        /// 设置函数字典 //先后顺序排序
        /// </summary>
        /// <param name="funList"></param>
        /// <param name="exp"></param>
        private void SetFunsDic(Dictionary<int, string> funPosDic,List<int> sortPos) 
        {
            sortPos.Sort();
            foreach (var item in sortPos)
            {
                string temp = "";
                if (funPosDic.TryGetValue(item, out temp)) {
                    this.FunListLToR.Add(temp);
                }
            }
        }
        /// <summary>
        /// 通过函数名匹配函数的位置
        /// </summary>
        /// <param name="funName"></param>
        /// <param name="exp"></param>
        /// <param name="funPosDic"></param>
        private  void GetFunDic(string funName, string exp, ref Dictionary<int, string> funPosDic,ref  List<int> sortList)
        {
            string funexp = string.Format("(?<group1>^*{0})", funName);
            Regex reg = new Regex(funexp, RegexOptions.IgnoreCase);
            var matcher = reg.Matches(exp);
            foreach (Match item in matcher)
            {
                Group gp = item.Groups["group1"];
                if (gp != null)
                {
                    sortList.Add(gp.Index);
                    funPosDic.Add(gp.Index, funName);
                }
            }

        }
    }
}
