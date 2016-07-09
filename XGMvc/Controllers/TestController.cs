using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.IBusiness;

namespace BaothApp.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/
       
        public ActionResult Index()
        {
            var context = new BusinessModel.BusinessEdmxEntities();
            for (int i = 1; i <= 45; i++)
            {
                context.ExecuteStoreCommand(GetSql(i));
            }
        
            return View("Test");
        }

        public string GetSql(int i) {
            string sql =string.Format( @"
            INSERT INTO dbo.SA_PlanActionDetail
        ( GUID ,
          GUID_PlanAction ,
          GUID_Person ,
          GUID_Department ,
          GUID_Bank ,
          BankCardNo ,
          GUID_Item ,
          ItemValue ,
          ItemOrderNum
        )
SELECT NEWID() AS guid,
'32EE1A8C-2487-42FA-9265-5A49F9C38C0F' AS GUID_PlanAction,
a.GUID AS GUID_Person,
b.GUID_Department,
'260294AE-226D-4635-862D-0C9025C0B01A' AS GUID_Bank,
b.BankCardNo,
(SELECT GUID_Item FROM dbo.SA_PlanItemView WHERE GUID_Plan='E0622D6E-8AAC-9A4E-9C2F-6C04BD30C705' 
AND ItemOrder={0}) AS guid_Item,
			 t{0} AS ItemValue,
			  {0} AS ItemOrderNum
FROM Table_工资导入 a
LEFT JOIN dbo.SS_Person b ON a.guid=b.GUID 
            ",i);
            return sql;
        
        }
        public JsonResult UploadFile() 
        {
            var hfc = Request.Files;
            List<string> guids = new List<string>();
            for (int i = 0; i < hfc.Count; i++)
            {
                HttpPostedFileBase file = hfc[i];
                byte[] buffer = new byte[hfc[i].InputStream.Length];
                hfc[i].InputStream.Read(buffer, 0, buffer.Length);
            }
            var context=new  BusinessModel.BusinessEdmxEntities();
            //var ent =context.CreateObjectSet<A
            return Json(new {msg="上传文件错误"});
        }
    }
    public interface IRepository 
    {
        string GetName();
    }
    public class UserRepository : IRepository 
    {

        #region IRepository 成员

        public string GetName()
        {
            return "hanyxuing";
        }

        #endregion
    }
}
