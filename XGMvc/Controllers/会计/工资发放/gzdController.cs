using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Reimbursement;
using Business.Common;
using BaothApp.Utils;
using System.IO;
using System.Data;
using Business;

namespace BaothApp.Controllers.现金报销单

{
    public class gzdController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "gzd"; }
        }

        public ViewResult Print() 
        {
            var template=Request["pturl"];
            return View(template);
        }
        public override ViewResult Index()
        {
            return View("gzd");
        }
        public ViewResult selectPerson()
        {
            return View("selectPerson");
        }
        public ViewResult SAemail()
        {
            return View("SAemail");
        }
        public ViewResult gzdItemSet()
        {
            return View("gzdItemSet");
        }
        public ViewResult gzd_particulars()
        {
            return View("gzd_particulars");
        }
        public ViewResult itemDataSet()
        {
            return View("itemDataSet");
        }
        /// <summary>
        /// 导入页面
        /// </summary>
        /// <returns></returns>
        public ViewResult excelImport()
        {
            return View("excelImport");
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadFile()
        {
            var file = Request.Files;
            if (file == null || file.Count == 0)
            {
                return Json(new { msg = "没有选择要导入的文件！" });
            }
            if (file.Count >1)
            {
                return Json(new { msg = "只能选择一个文件进行导入" });
            }
           
            HttpPostedFileBase hpfb = file[0];
            var path = Request.MapPath("~/Upload");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fileName = Path.Combine(path, Path.GetFileName(hpfb.FileName));
            hpfb.SaveAs(fileName);

            return Json(new { msg = "", path = fileName});
            //var user = this.CurrentUserInfo;
            //Guid userGuid;
            //if (user != null)
            //{
            //    userGuid = user.UserGuid;
            //}
            //else
            //{
            //    userGuid = Guid.Empty;
            //}
            //JsonModel jsonModel1 = new JsonModel();
            //string m1 = Request.Params["m"];
            //string d1 = Request.Params["d"];
            //if (m1 != null && m1 != string.Empty)
            //{
            //    this.jsonModel.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m1);
            //}
            //if (d1 != null && d1 != string.Empty)
            //{
            //    this.jsonModel.d = JsonHelper.JsonToObject<List<JsonGridModel>>(d1);
            //}         

            //var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            //JsonModel result = obj.GetUploadFileData(fileName,this.jsonModel);
          
        }
        /// <summary>
        /// 获取上传数据
        /// </summary>
        /// <returns></returns>
        public ContentResult GetUploadData()
        {
            var filePath=Request["filepath"];
            var user = this.CurrentUserInfo;
            Guid userGuid;
            if (user != null)
            {
                userGuid = user.UserGuid;
            }
            else
            {
                userGuid = Guid.Empty;
            }
            JsonModel jsonModel1 = new JsonModel();
            string m1 = Request.Params["m"];
            string d1 = Request.Params["d"];
            if (m1 != null && m1 != string.Empty)
            {
                this.jsonModel.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m1);
            }
            if (d1 != null && d1 != string.Empty)
            {
                this.jsonModel.d = JsonHelper.JsonToObject<List<JsonGridModel>>(d1);
            }

            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.GetUploadFileData(filePath, this.jsonModel);

            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Content(rowJson);
            //return Json(result);
        }
        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public ActionResult Export()
        {
            JsonModel jsonModel = new JsonModel();
            string m1 = Request.Params["m"];
            string d1 = Request.Params["d"];
            string colName = Request.Params["colName"];
            if (m1 != null && m1 != string.Empty)
            {
                jsonModel.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m1);
            }
            if (d1 != null && d1 != string.Empty)
            {
                jsonModel.d = JsonHelper.JsonToObject<List<JsonGridModel>>(d1);
            }
            var user = this.CurrentUserInfo;
            Guid userGuid;
            if (user != null)
            {
                userGuid = user.UserGuid;
            }
            else
            {
                userGuid = Guid.Empty;
            }
            List<Business.Accountant.ColModel> colNameList = JsonHelper.JsonToObject<List<Business.Accountant.ColModel>>(colName);
            var gzdobj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);

            string errorMsg = string.Empty;
            string fileName = string.Empty;
            DataTable dt = gzdobj.CreateExportData(colNameList, jsonModel);
            string pathFlie = GetExportPath(dt, out fileName, out errorMsg);

            object obj = new { msg = "", data = pathFlie };
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg == "1")
                {
                    obj = new { msg = "没有可导出的数据！", data = pathFlie };
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
                else if (errorMsg == "-1")
                {                   
                    obj = new { msg = "数据已经过期,请查询后再导出！", data = pathFlie };
                    return Json(obj, JsonRequestBehavior.AllowGet);
                }
                
            }
            return Json(obj, JsonRequestBehavior.AllowGet);

                   
        }
        public ActionResult DataExport()
        {
            var path=Request["path"];
            var fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + "工资单.xls";           
            return File(path, "application/msexcel", fileName);     
        }

        //导出报表
        private string GetExportPath(DataTable data, out string fileName, out string message)
        {
            fileName = "";
            message = "";
            CAE.Report.BaseReport basereport = new CAE.Report.BaseReport();
            var template = basereport.tempalte;
            template = Path.Combine(template, "gzd.xls");
            try
            {
                if (data != null && data.Rows.Count <= 0)
                {
                    message = "1";
                    return "";
                }
                string filePath = CAE.ExportExcel.Export(data, template, 0, 0, new List<CAE.ExcelCell>() { });
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }

        public override JsonResult New()
        {

            JsonModel result = this.Actor.New();
            return Json(result);
        }
        public JsonResult NewByPlan()
        {
            var strGuid=Request["guid"];
            Guid g;
            Guid.TryParse(strGuid,out g);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.NewByPlan(g);
            return Json(result);
        }
        /// <summary>
        /// 工资项数据保存
        /// </summary>
        /// <returns></returns>
        public JsonResult SavePlanItemSetup()
        {
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.SaveItemSet(this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <returns></returns>
        public JsonResult RetrievePlanItemSetUp()
        {
            var strGuid = Request["guid"];
            Guid g;
            Guid.TryParse(strGuid, out g);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.RetrievePlanItemSetup(g);
            return Json(result); 
        }
        /// <summary>
        /// 发放 更改状态
        /// </summary>
        /// <returns></returns>
        public JsonResult UpdateActionState()
        {
            var strGuid = Request["guid"];
            Guid g;
            Guid.TryParse(strGuid, out g);
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonMessage result = obj.UpdateActionState(g);
            return Json(result);
        }
        /// <summary>
        /// 计算获取数据
        /// </summary>
        /// <returns></returns>
        public ContentResult JSGetData()
        {         
            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.JSGetData(this.jsonModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Content(rowJson);
        }
        /// <summary>
        /// 设值        /// </summary>
        /// <returns></returns>
        public JsonResult SetValueData()
        {
            JsonModel jsonModel1 = new JsonModel();
            string m1 = this.Request["m1"];
            string d1 = this.Request["d1"];
            if (m1 != null && m1 != string.Empty)
            {
                jsonModel1.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m1);
            }
            if (d1 != null && d1 != string.Empty)
            {
                jsonModel1.d = JsonHelper.JsonToObject<List<JsonGridModel>>(d1);
            }

            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.SetValueData(this.jsonModel, jsonModel1);
            return Json(result);
        }
        /// <summary>
        /// 自动设值
        /// </summary>
        /// <returns></returns>
        public JsonResult AutoSetValueData()
        {
            JsonModel jsonModel1 = new JsonModel();
            string m1 = this.Request["m1"];
            if (m1 != null && m1 != string.Empty)
            {
                jsonModel1.m = JsonHelper.JsonToObject<List<JsonAttributeModel>>(m1);
            }
            jsonModel1.d =null;

            var user = this.CurrentUserInfo;
            Guid userGuid = user.UserGuid;
            var obj = new Business.Accountant.工资发放(userGuid, this.ModelUrl);
            JsonModel result = obj.AutoSetValueData(this.jsonModel, jsonModel1);
            return Json(result);
        }

        /// <summary>
        /// 发送工资条到邮件
        /// </summary>
        /// <returns></returns>
        public JsonResult SendEmail()
        {
            var cyear = Request["year"];
            var cmonth = Request["month"];
            var actionstate = Request["actionstate"];
            SADrive sadriver = new SADrive();
            //获取邮件配置信息
            string host = System.Configuration.ConfigurationManager.AppSettings["EmailHost"];
            string hostaddress = System.Configuration.ConfigurationManager.AppSettings["HostAddress"];
            string hostdisplayname = System.Configuration.ConfigurationManager.AppSettings["HostDisplayName"];
            string hostpassword = System.Configuration.ConfigurationManager.AppSettings["HostPassword"];
            string subjectname = System.Configuration.ConfigurationManager.AppSettings["SubjectName"];
            string enablesslstr = System.Configuration.ConfigurationManager.AppSettings["EnableSsl"];
            string hostportstr = System.Configuration.ConfigurationManager.AppSettings["EmailHostPort"];
            bool EnableSsl = false;
            if (!bool.TryParse(enablesslstr,out EnableSsl)) EnableSsl = false;
            int HostPort = 25;
            if (!int.TryParse(hostportstr, out HostPort)) HostPort = 25;
            subjectname = subjectname.Replace("@year", cyear);
            subjectname = subjectname.Replace("@month", cmonth);
            EmailDrive emailDriver = new EmailDrive(host, hostaddress, hostpassword);
            var results = sadriver.RetrieveSA(int.Parse(cyear), int.Parse(cmonth), actionstate);
            foreach (SAPerson item in results)
            {
                if (!string.IsNullOrEmpty(item.Email))
                {
                    try
                    {
                        emailDriver.SendMail(subjectname, hostaddress, hostdisplayname,
                            item.Email, item.PersonName, item.ConvertToHtml(), EnableSsl, HostPort);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return Json(new {result=1});
        }

        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);

        }
        public ContentResult Save1()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Content(rowJson);
        }
        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Json(result);
        }


        public ContentResult Retrieve1()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            return Content(rowJson);
        }
        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);

            //拿到任务
            //根据任务去找到具体执行的对象
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            int i = iRunWork.ProcessStatus == 1 ? 1 : 0;
            this.Actor.UpdateDocState(Guid,(EnumType.EnumDocState)i);
            return Json(message);
            
        }
        public override JsonResult SendBackFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, 0);
            return Json(message);
        }
        
        
    }
    
   
}
