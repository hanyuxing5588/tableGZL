using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using Business.Reimbursement;
using Business.Common;
using Business.Budget;
using CAE;
using System.Data;
namespace BaothApp.Controllers.预算.预算编制
{   
    public class ysbzController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "ysbz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //主界面
        public void GetSql()
        {
            try
            {

           
            var dt = Business.Common.DataSource.ExecuteQuery(@"SELECT guid  FROM dbo.BG_Mainview WHERE DocDate>='2015-12-25' ");
            if (dt.Rows.Count > 0)
            {
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    var mainid = dr["guid"].ToString();

                    var sqlformat = @"UPDATE  BG_Detail
SET     Total_BG = c.Total_BG
FROM    (
	 SELECT    a.Total_BG ,
                    a.GUID_BGCode ,
                    b.GUID_Item ,
                    a.GUID_BG_Main
          FROM      ( SELECT    *
                      FROM      dbo.BG_Detail
                      WHERE     GUID_BG_Main = '{0}'
                                AND GUID_Item = '11E535CB-394E-4113-9AC1-9E0364A3A80C'
                    ) a
                    LEFT JOIN ( SELECT  *
                                FROM    dbo.BG_Detail
                                WHERE   GUID_BG_Main = '{0}'
                                        AND GUID_Item = '80F44A02-E30E-4C13-9308-06A28499A442'
                              ) b ON a.GUID_BGCode = b.GUID_BGCode
        ) c
WHERE   BG_Detail.GUID_BG_Main = c.GUID_BG_Main
        AND dbo.BG_Detail.GUID_BGCode = c.GUID_BGCode
        AND BG_Detail.GUID_Item = '80F44A02-E30E-4C13-9308-06A28499A442'";
                    var sql = string.Format(sqlformat, mainid); ;
                    Business.Common.DataSource.ExecuteNonQuery(sql);
                }
            }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public override JsonResult New()
        {
            //GetSql();
            JsonModel result;
            var guid = Request["PreGuid"] + "";//如果在流程中 流程的Id
            var preScope = Request["Scope"]+"";      //单据转换前一个单据的作用域
            if (guid == "")
            {
                result = this.Actor.New();
            }
            else
            {
                Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                result = objYSBZ.New(guid, preScope);
            }
            return Json(result);
        }

        public override ViewResult Index()
        {
            var guid = Request["guid"]+"";//如果在流程中 流程的Id
            var preScope = Request["common"]+"";      //单据转换前一个单据的作用域
            ViewData["PreGuid"] = guid;
            ViewData["common"] = preScope;
            if (preScope == "3")//预算列表
            {
                var objYSBZ = new Business.Budget.预算编制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                ViewData["guid"] = objYSBZ.GetBGMain(new Guid(guid));
                ViewData["status"] = 4;
                return View("ysbz");
            }
            if (!string.IsNullOrEmpty(guid))
            {
                Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                Guid Id;
                Guid.TryParse(guid,out Id);
                if (objYSBZ.IsExistByID(Id))
                {
                    ViewData["status"] = 4;
                }else if (!objYSBZ.GetBGMainExist(Id,out Id)){
                    ViewData["guid"]=Id;
                    ViewData["status"] = 4;
                }
                else
                {
                    ViewData["status"] = 1;
                }
            }
            else
            {
                ViewData["status"] = 1;
            }
            return View("ysbz");
        }

        public JsonResult SaveBGMain()
        {
            var BG_Main = Request["BG_Main"];
            var BG_Detail = Request["BG_Detail"];
            var BGYear = Request["BGYear"];
            var MoneyUnit = Request["MoneyUnit"];
            var PreGuid = Request["PreGuid"];
            var PreScope = Request["PreScope"];
            string strState = this.Status;
            var YSTZ = Request["YSTZ"] + "";
            Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制(this.CurrentUserInfo.UserGuid,this.ModelUrl);
            objYSBZ.YSTZ = YSTZ == "1";
            VerifyResult vr = new VerifyResult();
            JsonModel json = objYSBZ.SaveBG_Main(BG_Main, BG_Detail, strState, BGYear, MoneyUnit,PreGuid,PreScope ,ref vr);

            //if (null == strJson || strJson == string.Empty)
            //{
            //    return Content("{\"success\": false ,\"errMsg\":'\"保存失败\"}");
            //}

            return Json(json);
        }
        // 获得预算编制的编号
        public ContentResult GetDocNum()
        { 
            var GUID_DW = Request["GUID_DW"];
            var DocDate = Request["DocDate"];
            Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制();
            string strDocNum = objYSBZ.GetDocNumber(GUID_DW, DocDate);
            if(string.Empty==strDocNum || ""==strDocNum)
            {
                return Content("{\"success\": false ,\"errMsg\":'\"获得预算编号失败!\"}");
            }
            string strJson = "{\"success\":true,\"DocNum\":\"" + strDocNum + "\"}";
            ContentResult result = Content(strJson);
            return Content(strJson);
        }
        // 获取预算编制表信息
        public ContentResult GetBGDetail()
        {
            var BG_MainGUID = Request["BG_MainGUID"];
            var BG_Setup = Request["BG_Setup"];
            var PreGuid = Request["PreGuid"];
            var PreScope = Request["PreScope"];

            Business.Budget.预算编制 objYSBZ =new Business.Budget.预算编制();
            string strJson = objYSBZ.GetBG_DetailData(BG_MainGUID, BG_Setup, PreGuid, PreScope);
            if (null == strJson || strJson == string.Empty)
            {
                return Content("{\"success\": false ,\"msg\":\"获得预算设置列失败!\"}");
            }            
            
            return Content(strJson);
        }
        //选单
        public ViewResult selectDoc()
        {
            return View("selectDoc");
        }
        //public JsonResult ChangeDocData()
        //{
        //    string guid = Request["guid"];
        //    string docType = Request["docType"];
        //    var o= this.Actor.ChangeDocData(guid,docType,false);
        //    return Json(o);
        //}
       
        // 历史
        public override ContentResult History()
        {
            string condition = Request["condition"];
            BillHistoryCondition conditionModel = new BillHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BillHistoryCondition>(condition);
            List<object> result = this.Actor.History(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        public  ContentResult Reference() 
        {
            var dwKey = Request["dwKey"];
            var depKey = Request["depKey"];
            var proKey = Request["proKey"];
            if(proKey==null)
            {
                proKey = "";
            }
            Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            List<object> result = objYSBZ.Reference(dwKey,depKey,proKey);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        public ContentResult GetReference()
        {
            var guid = Request["GUID"];
            Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制();
            string strJson = objYSBZ.GetBG_DetailData(guid, Guid.Empty.ToString(),"","");
            if (null == strJson || strJson == string.Empty)
            {
                return Content("{\"success\": false ,\"errMsg\":'\"获得预算设置列失败!\"}");
            }

            ContentResult result = Content(strJson);
            return Content(strJson);
        }
        public  ContentResult HistoryEx()
        {
            string condition = Request["condition"];
            BillHistoryCondition conditionModel = new BillHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BillHistoryCondition>(condition);
            Business.Budget.预算编制 objYSBZ = new Business.Budget.预算编制();
            List<object> result = objYSBZ.HistoryEx(conditionModel);
            string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
            string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
            return Content(json);
        }
        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            var listTasks = Platform.Flow.Run.TaskManager.GetAllTask().ToList();
            iRunWork.SetTasks(listTasks);
            iRunWork.SetOperatorId(this.CurrentUserInfo.UserGuid);
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 62);
            if (iRunWork.ProcessStatus != 1)
            {
                //审批中

                this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            }
            else
            {
                //审批完成 
                this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approved);
            }
            return Json(message);
        }

        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }

        public override JsonResult SendBackFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.RejectFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 62);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);
        }


    }
}
