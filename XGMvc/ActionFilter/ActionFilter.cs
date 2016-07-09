using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp
{
    public class ActionFilter
    {
        public class ActionLogAttribute : ActionFilterAttribute
        {
            public string Description { get; set; }

            public ActionLogAttribute()
            {
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                //using (MvcGuestbookEntities db = new MvcGuestbookEntities())
                //{
                    //ActionLog log = new ActionLog()
                    //{
                        //Member = db.Member.Where(p => p.Account == filterContext.HttpContext.User.Identity.Name).First(),
                        string s1 = filterContext.RouteData.Values["controller"] + "." +filterContext.RouteData.Values["action"];
                        string ClientIP = filterContext.HttpContext.Request.UserHostAddress;
                       string Description = this.Description;
                       string Account=filterContext.HttpContext.User.Identity.Name;
                    //};

                    //db.AddToActionLog(log);
                    //db.SaveChanges();
            }
        }
    }
}