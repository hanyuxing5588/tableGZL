using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace BaothApp.Utils
{
    public class RequestCommon
    {
        public RequestCommon() { }

        #region "获取用户表单提交字段值"

        public static T RequestValue<T>(string ValueName)
        {
            HttpContext rq = HttpContext.Current;
            T TempValue;
            if (rq.Request.QueryString[ValueName] != null)
            {
                TempValue = (T)Convert.ChangeType(rq.Request.QueryString[ValueName], typeof(T));
            }
            else
            {
                TempValue = default(T);
            }

            return TempValue;
        }

        #endregion

        public static string SESSION_USERINFO = "SESSION_USERINFO";

        public static void AddSessionUserInfo(SessionUser userInfo) 
        {
            HttpContext rq = HttpContext.Current;
            rq.Session[SESSION_USERINFO] = userInfo;
            //Session["UserInfo"] = userInfoShow;
        }

        public static SessionUser GetSessionUserInfo() 
        {
            HttpContext rq = HttpContext.Current;
            return (SessionUser)rq.Session[SESSION_USERINFO];
        }
    }
}