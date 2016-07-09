using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Infrastructure;
using System.Web.UI.WebControls;
using System.Data.Common;
using BaothApp.Utils;

namespace BaothApp.Controllers.Logon
{
    public class LogonController : Controller
    {
        //实例化Model模型对象
        IntrastructureFun dbobj = new IntrastructureFun();
        Login _loginuserName = new Login();

        #region return Logon Page
        /// <summary>
        /// return Logon Page
        /// </summary>
        /// <returns></returns>

        public ActionResult Index()
        {
            //var name = RequestCommon.GetSessionUserInfo();
            //ViewBag.Name = name;
            //ViewBag.Name = name;
            return View();
        }
        #endregion


        #region return UserInfo
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="UserName">用户名</param>
        /// <param name="Password">用户密码</param>
        /// <returns></returns>
        //[Authorize]
        //[BaothApp.ActionFilter.ActionLog(Description = "用户留言")]
        public ActionResult LogonUserInfo(string Name, string PassWord)
        {
            #region 声明变量
            string loginname = Name;
            string loginpwd = PassWord;
            string UserInfoError = "no";
            #endregion
            #region 实例化SessionUser
            SessionUser userInfo = new SessionUser();
            #endregion
            SS_Operator result = dbobj.GetOperatorInfo(Name, PassWord);
            if (result != null)
            {
                #region 将用户信息添加到session中                userInfo.UserGuid = result.GUID;
                userInfo.UserKey = result.OperatorKey;
                userInfo.UserName = result.OperatorName;
                userInfo.UserPwd = result.Password;
                Session[RequestCommon.SESSION_USERINFO] = userInfo;
                //RequestCommon.AddSessionUserInfo(userInfo);
                #endregion
                UserInfoError = "ok";
            }
            else
            {
                //如果result==null，后台自动匹配用户名和密码,判断错误类别。                //1、用户不存在2、密码错误等
                UserInfoError = dbobj.Login(loginname, loginpwd, true);
            }
            return Content(UserInfoError);
        }

        /// <summary>
        /// 退出系统，清空session
        /// </summary>
        /// <returns></returns>
        public ActionResult LogonOut()
        {
            Session.Abandon();
            return Content("ok");
        }

        /// <summary>
        /// 注销系统，清空session
        /// </summary>
        /// <returns></returns>
        public ActionResult logonCancle() 
        {
            Session.Abandon();
            return Content("ok");
        }

        #endregion
    }
}