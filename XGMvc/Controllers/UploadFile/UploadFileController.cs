using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace BaothApp.Controllers
{
    public class UploadFileController : Controller
    {
        //
        // GET: /Test/

        //public ActionResult Index()
        //{
        //    return View("Test");
        //}

        /// <summary>
        /// 通知公告
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadFile() 
        {
            try
            {

          
            var hfc = Request.Files;
            List<string> guids = new List<string>();
            var context = new OAModel.OAEntities();
            for (int i = 0; i < hfc.Count; i++)
            {
                //点击上传附件时，将公告所对应的附件数据添加到临时表中(还没有保存)
                HttpPostedFileBase file = hfc[i];
                byte[] buffer = new byte[hfc[i].InputStream.Length];
                hfc[i].InputStream.Read(buffer, 0, buffer.Length);
                var tempFile= context.OA_FileTemp.CreateObject();
                tempFile.FileBody = buffer;
                tempFile.FileName = file.FileName;
                tempFile.GUID = Guid.NewGuid();
                //tempFile.GUID_OfficeFileType = new Guid();  保存的时候赋值                context.OA_FileTemp.AddObject(tempFile);

                guids.Add(tempFile.GUID.ToString());
                guids.Add(tempFile.FileName);

            }
            string ids = string.Join(",",guids);
            context.SaveChanges();
            return Json(new {fileIds=ids});
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public JsonResult UploadOneFile()
        {
            var hfc = Request.Files;
            if (hfc == null || hfc.Count == 0)
            {
                return Json(new { msg = "没有选择要导入的文件！" });
            }
            if (hfc.Count > 1)
            {
                return Json(new { msg = "只能选择一个文件进行导入" });
            }
              
            HttpPostedFileBase file = hfc[0];
            byte[] buffer = new byte[hfc[0].InputStream.Length];
            hfc[0].InputStream.Read(buffer, 0, buffer.Length);
          
            var path = Request.MapPath("~/Upload");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var strfileName = file.FileName;
            var fileType = strfileName.Substring(strfileName.IndexOf('.') + 1);
            if (fileType == "xls" || fileType == "xlsx")
            {
                var fileName = Path.Combine(path, Path.GetFileName(strfileName));
                file.SaveAs(fileName);
                return Json(new { msg = "", filepath = fileName });
            }
            else
            {
                return Json(new { msg = "只能导入xls或者xlsx文件！" });
            }           

        }

        public ActionResult Index()
        {
            return View("Upload");
        }


    }
}
