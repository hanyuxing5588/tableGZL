using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BaothApp.Controllers
{
    public class gldocController : Controller
    {
        //
        // GET: /gldoc/

        public ActionResult Index()
        {
            ViewData["currentDate"] = DateTime.Now.ToShortDateString();
            ViewData["startDate"] = DateTime.Now.Year + "-" + DateTime.Now.Month + "-01";
            ViewData["ModelUrl"] = "filter";
            ViewData["GUID"] = Request["guid"];
            return View();
        }
        public JsonResult Save() {
            var guid = Request["guid"];
            var docids = Request["docids"]+"";
            var docidsArr = docids.Split(',');
            var listSql = new List<string>();
            var sqlformat = @"
            
    INSERT INTO [dbo].[ss_guanlian]
           ([mainid]
           ,[docid])
     VALUES
           ('{0}'
           ,'{1}')            
";          var id=Guid.Empty;
            foreach (var item in docidsArr)
            {
                if (Guid.TryParse(item, out id))
                {
                    var sql = string.Format(sqlformat, guid, item);
                    listSql.Add(sql);
                }
            }
            try
            {
                Business.Common.DataSource.ExecuteNonQueryInner(string.Format("delete from ss_guanlian where mainid='{0}'", guid));
                if (listSql.Count>0) Business.Common.DataSource.ExecuteNonQueryLst(listSql);
                return Json(new {IsSuccess=true},JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { IsSuccess = true }, JsonRequestBehavior.AllowGet);
            }
         

        }

    }
}
