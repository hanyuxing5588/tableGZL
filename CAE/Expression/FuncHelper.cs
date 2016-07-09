using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace CAE.Expression
{
    public interface IFunctionHelper
    {
        /// <summary>
        /// 运行函数并返回运行结果
        /// </summary>
        /// <returns>运行结果</returns>
        object Run();
    }

    public class FunctionException:Exception
    {
        private List<string> errs = new List<string>();
        public List<string> Errors { get { return errs; } }
        public FunctionException() : base() { }
        public FunctionException(string message) : base(message) { }
        public FunctionException(string message, Exception innerException)
            : base(string.Empty, innerException)
        {
            this.errs.Add(message);
        }
        public FunctionException(ValidateResult Validation, Exception innerException)
            : base(string.Empty, innerException)
        {
            this.errs.Add(innerException.Message);
            this.errs.AddRange(Validation.Errors);
        }
        public FunctionException(ValidateResult Validation)
            : base()
        {
            this.errs.AddRange(Validation.Errors);
        }
        public FunctionException(Exception innerException)
            : base(string.Empty, innerException)
        {
            this.errs.Add(innerException.Message);
        }
    }

    class SqlScalar : IFunctionHelper
    {
        private string providerName = string.Empty;
        private string connectionString = string.Empty;
        private string sqlStatement = string.Empty;
        public SqlScalar(string providerName, string connectionString, string sqlStatement)
        {
            this.providerName = providerName;
            this.connectionString = connectionString;
            this.sqlStatement = sqlStatement;
        }

        #region IFunctionHelper 成员

        public object Run()
        {
            DbConnection cnn = null;
            try
            {
                var provider = DbProviderFactories.GetFactory(providerName);
                cnn = provider.CreateConnection();
                cnn.ConnectionString = connectionString;
                var command = cnn.CreateCommand();
                command.CommandText = sqlStatement;
                cnn.Open();
                object result = command.ExecuteScalar();
                cnn.Close();
                return result;
            }
            catch (Exception e) { throw e; }
            finally 
            {
                if (cnn != null && cnn.State!= System.Data.ConnectionState.Closed)
                {
                    cnn.Close();
                }
            }
        }

        #endregion
    }

    public class SqlScalarException : FunctionException
    {
        public SqlScalarException() : base() { }
        public SqlScalarException(string message) : base(message) { }
        public SqlScalarException(string message, Exception innerException) : base(message, innerException) { }
        public SqlScalarException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public SqlScalarException(ValidateResult Validation) : base(Validation) { }
        public SqlScalarException(Exception innerException) : base(innerException) { }
    }

    /// <summary>
    /// 字符串长度 Len()
    /// </summary>
    public class Len : IFunctionHelper
    {
        private string strValue = string.Empty;
        public Len(string strValue)
        {
            this.strValue = strValue;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strValue.Length;
        }

        #endregion
    }

    public class LenException : FunctionException
    {
        public LenException() : base() { }
        public LenException(string message) : base(message) { }
        public LenException(string message, Exception innerException) : base(message, innerException) { }
        public LenException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public LenException(ValidateResult Validation) : base(Validation) { }
        public LenException(Exception innerException) : base(innerException) { }
    }

    /// <summary>
    /// 截取字符串 Sub(string,1,3)
    /// </summary>
    public class Sub : IFunctionHelper
    {
        private string strValue = string.Empty;
        private int startPos = 0;
        private int lenth = 0;
        public Sub(string strValue, int startPos, int lenth)
        {
            this.strValue = strValue;
            this.startPos = startPos;
            this.lenth = lenth;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strValue.Substring(startPos, lenth);
        }

        #endregion
    }

    public class SubException : FunctionException
    {
        public SubException() : base() { }
        public SubException(string message) : base(message) { }
        public SubException(string message, Exception innerException) : base(message, innerException) { }
        public SubException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public SubException(ValidateResult Validation) : base(Validation) { }
        public SubException(Exception innerException) : base(innerException) { }
    }

    /// <summary>
    /// 求和 Sum(1,3)
    /// </summary>
    public class Sum : IFunctionHelper
    {
        private double addDouble = 0;
        private double addedDouble = 0;
        public Sum(double add,double added)
        {
            this.addDouble = add;
            this.addedDouble = added;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return addDouble+addedDouble;
        }

        #endregion
    }

    public class SumException : FunctionException
    {
        public SumException() : base() { }
        public SumException(string message) : base(message) { }
        public SumException(string message, Exception innerException) : base(message, innerException) { }
        public SumException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public SumException(ValidateResult Validation) : base(Validation) { }
        public SumException(Exception innerException) : base(innerException) { }
    }
    /// <summary>
    /// 替换 Repalce("||","$")
    /// </summary>
    public class Repalce : IFunctionHelper
    {
        private string strValue = "";
        private string oldStr = "";
        private string newStr = "";
        public Repalce(string strValue,string oldStr, string newStr)
        {
            this.oldStr = oldStr;
            this.newStr = newStr;
            this.strValue = strValue;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strValue.Replace(oldStr, newStr);
        }

        #endregion
    }

    public class RepalceException : FunctionException
    {
        public RepalceException() : base() { }
        public RepalceException(string message) : base(message) { }
        public RepalceException(string message, Exception innerException) : base(message, innerException) { }
        public RepalceException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public RepalceException(ValidateResult Validation) : base(Validation) { }
        public RepalceException(Exception innerException) : base(innerException) { }
    }

    /// <summary>
    ///  在某一字符串的里面 IN("3","1,2,3,4")
    /// </summary>
    public class In : IFunctionHelper
    {
        private string strValue = "";
        private string strParams = "";
        public In(string strValue, string strParams)
        {
            this.strValue = strValue;
            this.strParams = strParams;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            string[] strArr = strParams.Split(',');
            return strArr.Contains(strValue);
        }

        #endregion
    }

    public class InException : FunctionException
    {
        public InException() : base() { }
        public InException(string message) : base(message) { }
        public InException(string message, Exception innerException) : base(message, innerException) { }
        public InException(ValidateResult Validation, Exception innerException) : base(Validation, innerException) { }
        public InException(ValidateResult Validation) : base(Validation) { }
        public InException(Exception innerException) : base(innerException) { }
    }
    /// <summary>
    /// 和某一个字符串的相似 Like("ab","abc")
    /// </summary>
    public class Like : IFunctionHelper
    {
        private string strValue = "";
        private string strLike = "";
        public Like(string strValue, string strLike)
        {
            this.strValue = strValue;
            this.strLike = strLike;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strLike.Contains(strValue);
        }

        #endregion
    }

    public class LikeException : FunctionException
    {
        public LikeException() : base() { }
        public LikeException(string message) : base(message) { }
        public LikeException(string message, Exception LikenerException) : base(message, LikenerException) { }
        public LikeException(ValidateResult Validation, Exception LikenerException) : base(Validation, LikenerException) { }
        public LikeException(ValidateResult Validation) : base(Validation) { }
        public LikeException(Exception LikenerException) : base(LikenerException) { }
    }
    /// <summary>
    /// 和某一个字符串开始字母匹配("ab","abc")
    /// </summary>
    public class StartWith: IFunctionHelper
    {
        private string strValue = "";
        private string strLike = "";
        public StartWith(string strValue, string strLike)
        {
            this.strValue = strValue;
            this.strLike = strLike;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strLike.StartsWith(strValue);
        }

        #endregion
    }

    public class StartWithException : FunctionException
    {
        public StartWithException() : base() { }
        public StartWithException(string message) : base(message) { }
        public StartWithException(string message, Exception StartWithnerException) : base(message, StartWithnerException) { }
        public StartWithException(ValidateResult Validation, Exception StartWithnerException) : base(Validation, StartWithnerException) { }
        public StartWithException(ValidateResult Validation) : base(Validation) { }
        public StartWithException(Exception StartWithnerException) : base(StartWithnerException) { }
    }

    /// <summary>
    /// 和某一个字符串结束字母匹配("bc","abc")
    /// </summary>
    public class EndWith : IFunctionHelper
    {
        private string strValue = "";
        private string strLike = "";
        public EndWith(string strValue, string strLike)
        {
            this.strValue = strValue;
            this.strLike = strLike;
        }
        #region IFunctionHelper 成员

        public object Run()
        {
            return strLike.EndsWith(strValue);
        }

        #endregion
    }

    public class EndWithException : FunctionException
    {
        public EndWithException() : base() { }
        public EndWithException(string message) : base(message) { }
        public EndWithException(string message, Exception EndWithnerException) : base(message, EndWithnerException) { }
        public EndWithException(ValidateResult Validation, Exception EndWithnerException) : base(Validation, EndWithnerException) { }
        public EndWithException(ValidateResult Validation) : base(Validation) { }
        public EndWithException(Exception EndWithnerException) : base(EndWithnerException) { }
    }
  
}
