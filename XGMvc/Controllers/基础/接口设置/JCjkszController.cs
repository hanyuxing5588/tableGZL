using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Foundation;
using BaothApp.Utils;

namespace BaothApp.Controllers.基础.接口设置
{
    public class JCjkszController : SpecificController
    {

        private Business.Foundation.接口设置.接口设置 JKSZ = new Business.Foundation.接口设置.接口设置();

        #region 项目对应关系设置
        public ActionResult xmdygxsz()
        {
            this.ModelUrl = "xmdygxsz";
            return View("xmdygxsz");
        }
        public JsonResult Savexmdygxsz()
        {
            var id = Request["id"];
            var toformKey = Request["toformKey"];
            var compasrMainID = Request["compasrMainID"];
            var type = Request["type"];
            var classID = Request["ClassID"];
            var Ctype = Request["Ctype"];
            var sql = string.Format(@"INSERT INTO dbo.SS_ComparisonDetail
        ( GUID ,
          GUID_ComparisonMain ,
          ClassID ,
          GUID_Self ,
          ExteriorKey ,
          Comparisontype
        )
VALUES  ( NEWID() , -- GUID - uniqueidentifier
          '{0}' , -- GUID_ComparisonMain - uniqueidentifier
          {3}, -- ClassID - int
          '{1}' , -- GUID_Self - uniqueidentifier
          N'{2}' , -- ExteriorKey - nvarchar(50)
          N'{4}'  -- Comparisontype - nvarchar(50)
        )", compasrMainID,id, toformKey,classID,Ctype );
            if (type == "1") {
                sql =string.Format( "delete from SS_ComparisonDetail where GUID_Self='{0}' and GUID_ComparisonMain='{1}'",id,compasrMainID);
            }
            var msg= JKSZ.Save( sql);
            if (!string.IsNullOrEmpty(msg)) {
                return Json(new { sucess = "0",msg=msg }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { sucess = "1" }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProject()
        {
            var comparimainID = Request["id"]+"";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID,out guid);
            var result = JKSZ.GetProjects(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }

       
        #endregion

        public ActionResult gys()
        {
            return View("gys");
        }
        public JsonResult GetGYS()
        {
            var comparimainID = Request["id"] + "";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID, out guid);
            var result = JKSZ.GetGYSs(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult kh() { 
            return View("kh");
        }
        public JsonResult GetKH()
        {
            var comparimainID = Request["id"] + "";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID, out guid);
            var result = JKSZ.GetKH(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BB()
        {
            return View("bb");
        }
        public JsonResult GetBB()
        {
            var comparimainID = Request["id"] + "";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID, out guid);
            var result = JKSZ.GetBB(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Person()
        {
            return View("person");
        }
        public JsonResult GetPerson()
        {
            var comparimainID = Request["id"] + "";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID, out guid);
            var result = JKSZ.GetPerson(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult KM() 
        {
            return View("km");
        }
        public ActionResult GetKM() 
        {
            var comparimainID = Request["id"] + "";
            var guid = Guid.Empty;
            Guid.TryParse(comparimainID, out guid);
            var result = JKSZ.GetKM(guid);
            return Json(new { rows = result }, JsonRequestBehavior.AllowGet);
        }
    }
}
