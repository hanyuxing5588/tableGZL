using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BaothApp.Utils;
using Business.Common;

namespace BaothApp.Controllers
{
    public class SpecificController : Controller
    {
        public JsonModel jsonModel = new JsonModel();

        public virtual string Scope { get { return this.Request["scope"]; } }

        public virtual string Status { get { return this.Request["status"]; } }

        public virtual Guid Guid { get { return this.ConvertGuid(this.Request["guid"]); } }

        public SessionUser CurrentUserInfo { get; set; }

        public virtual string ModelUrl { get; set; }


        public BaseDocument Actor
        {
            get
            {
                return BaseDocument.CreatInstance(this.ModelUrl, this.CurrentUserInfo.UserGuid);
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            //得到用户登录的信息

            CurrentUserInfo = Session[RequestCommon.SESSION_USERINFO] as SessionUser;
            //CurrentUserInfo = null;
            //判断用户是否为空
            if (CurrentUserInfo == null)
            {
                Response.Redirect("/Logon/Index");
                return;
            }
            this.SetViewData();
        }
        protected override void OnException(ExceptionContext filterContext)
        {
            CurrentUserInfo = Session[RequestCommon.SESSION_USERINFO] as SessionUser;
            //判断用户是否为空
            if (CurrentUserInfo == null)
            {
                filterContext.ExceptionHandled = true;
                filterContext.Result = new RedirectResult(Url.Action("Index", "Logon"));
            }
            //this.SetViewData();
            //filterContext.
            //base.OnException(filterContext);
        }
        protected override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            //CurrentUserInfo = Session[RequestCommon.SESSION_USERINFO] as SessionUser;
            ////判断用户是否为空
            //if (CurrentUserInfo == null)
            //{
            //    Response.Redirect("/Logon/Index");
            //    return;
            //}
            //this.SetViewData();
            base.OnResultExecuted(filterContext);
        }
        public SpecificController()
            : base() { }

        public virtual ViewResult Index()
        {
            return null;
        }
        public virtual ViewResult Index3()
        {
            return null;
        }
        public virtual ViewResult FirstPage()
        {
            return null;
        }

        public virtual JsonResult New()
        {
            return null;
        }

        public virtual JsonResult Retrieve()
        {
            return null;
        }

        public virtual JsonResult Save()
        {
            return null;
        }

        public virtual JsonResult CommitFlow()
        {
            return null;
        }
        public virtual JsonResult SendBackFlow() 
        {
            //Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            //Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            //this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            //return Json(message);
            return null;
        }

        public virtual ContentResult History()//JsonResult
        {
            return null;
        }

        protected Guid ConvertGuid(string value)
        {
            Guid result = Guid.Empty;
            if (Guid.TryParse(value, out result))
            {
                return result;
            }
            return Guid.Empty;
        }

        protected void SetViewData()
        {
            ViewData["scope"] = this.Scope;
            ViewData["status"] = this.Request["status"];
            ViewData["guid"] = this.Request["guid"];

            string m = this.Request["m"];
            string d = this.Request["d"];
            //System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
            //st.Start();
            var commonFunGuid = Request["CommonFunGuid"];
            if (!string.IsNullOrEmpty(commonFunGuid)&&CurrentUserInfo!=null) {

                CommonFun.WriteCommonFun(commonFunGuid, CurrentUserInfo.UserGuid);
            }
            if (m != null && m != string.Empty)
            {
                jsonModel.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m);
            }
            if (d != null && d != string.Empty)
            {
                jsonModel.d = JsonHelper.JsonToObject<List<JsonGridModel>>(d);
            }
            
           // st.Stop();
           // long l = st.ElapsedMilliseconds;
            
        }
    }



}