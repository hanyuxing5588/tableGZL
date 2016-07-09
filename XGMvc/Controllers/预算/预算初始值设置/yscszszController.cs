using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using Business.Common;
using CAE;
using Business.Budget;
using Platform.Flow.Run;
namespace BaothApp.Controllers.预算.预算初始值设置
{   
    public class yscszszController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "yscszsz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //主界面
        public void GetSql() 
        {
            var dt = Business.Common.DataSource.ExecuteQuery(@"SELECT guid  FROM dbo.BG_DefaultMain WHERE DocDate='2015-12-25' ORDER BY DocNum
");
            if (dt.Rows.Count > 0)
            {
                foreach (System.Data.DataRow dr  in dt.Rows)
                {
                    var mainid = dr["guid"].ToString();
             
                var sqlformat = @"UPDATE  BG_DefaultDetail
SET     Total_BG = c.Total_BG
FROM    (
	 SELECT    a.Total_BG ,
                    a.GUID_BGCode ,
                    b.GUID_Item ,
                    a.GUID_BG_Main
          FROM      ( SELECT    *
                      FROM      dbo.BG_DefaultDetail
                      WHERE     GUID_BG_Main = '{0}'
                                AND GUID_Item = '11E535CB-394E-4113-9AC1-9E0364A3A80C'
                    ) a
                    LEFT JOIN ( SELECT  *
                                FROM    dbo.BG_DefaultDetail
                                WHERE   GUID_BG_Main = '{0}'
                                        AND GUID_Item = '80F44A02-E30E-4C13-9308-06A28499A442'
                              ) b ON a.GUID_BGCode = b.GUID_BGCode
        ) c
WHERE   BG_DefaultDetail.GUID_BG_Main = c.GUID_BG_Main
        AND dbo.BG_DefaultDetail.GUID_BGCode = c.GUID_BGCode
        AND BG_DefaultDetail.GUID_Item = '80F44A02-E30E-4C13-9308-06A28499A442'";
                var sql = string.Format(sqlformat, mainid); ;
                Business.Common.DataSource.ExecuteNonQuery(sql);
                }
            }
        }
        public override JsonResult New()
        {
            var guid = Request["PreGuid"]+"";//如果在流程中 流程的Id
            var preScope = Request["Scope"] + "";      //单据转换前一个单据的作用域
            JsonModel result;
            if(guid=="")
            {
                result = this.Actor.New();
            }
            else
            {
                Business.Budget.预算初始值设置 objYSCSZSZ = new Business.Budget.预算初始值设置(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                result = objYSCSZSZ.New(guid, preScope);
            }
             
            return Json(result);
        }
        // 获取预算编制表信息
        public ContentResult GetBGDetail()
        {
            var BG_DefaultMain_GUID = Request["BG_DefaultMain_GUID"];
            var BG_Setup = Request["BG_Setup"];

            Business.Budget.预算初始值设置 objYSCSZSZ = new Business.Budget.预算初始值设置();
            string strJson = objYSCSZSZ.GetBG_DetailData(BG_DefaultMain_GUID, BG_Setup);

            if (null == strJson || strJson == string.Empty)
            {
                return Content("{\"success\": false ,\"errMsg\":'\"获得预算设置列失败!\"}");
            }

            ContentResult result = Content(strJson);
            return Content(strJson);
        }

        public JsonResult SaveBGDefaultMain()
        {
            var BG_Main = Request["BG_Main"];
            var BG_Detail = Request["BG_Detail"];
            var BGYear = Request["BGYear"];
            var MoneyUnit = Request["MoneyUnit"];
            var PreGuid = Request["PreGuid"];
            var PreScope = Request["PreScope"];
            string strState = this.Status;

            Business.Budget.预算初始值设置 objYSCSZSZ = new Business.Budget.预算初始值设置(this.CurrentUserInfo.UserGuid, this.ModelUrl);
            VerifyResult vr = new VerifyResult();
            JsonModel json = objYSCSZSZ.SaveBG_Main(BG_Main, BG_Detail, strState, BGYear, MoneyUnit, PreGuid, PreScope, ref vr);

            //if (null == strJson || strJson == string.Empty)
            //{
            //    return Content("{\"success\": false ,\"errMsg\":'\"保存失败\"}");
            //}

            return Json(json);
        }
       
        public override ViewResult Index()
        {
            var guid = Request["guid"]+"";//如果在流程中 流程的Id
            var preScope = Request["common"]+"";      //单据转换前一个单据的作用域           
            ViewData["PreGuid"] = guid;
            ViewData["common"] = preScope;
            if (!string.IsNullOrEmpty(guid))
            {
                Guid Id;
                Guid.TryParse(guid,out Id);
                Business.Budget.预算初始值设置 objYSCSZSZ = new Business.Budget.预算初始值设置(this.CurrentUserInfo.UserGuid, this.ModelUrl);
                if (objYSCSZSZ.IsExistByID(Id))
                {
                    ViewData["status"] = "4";//状态为4 浏览状态
                }
                else
                {
                    ViewData["status"] = "1";//状态为1 新增状态
                }
            }
            else
            {
                ViewData["status"] = "1";
            }
            return View("yscszsz");
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
            //string condition = Request["condition"];
            //BillHistoryCondition conditionModel = new BillHistoryCondition();
            //conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<BillHistoryCondition>(condition);
            this.Actor.OperatorId = this.CurrentUserInfo.UserGuid;
            List<object> result = this.Actor.History(null);
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
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 90);
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
            //WorkFlowAPI.SaveProcessData(Guid, "yscszsz","ysbz", iRunWork.ProcessId);
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
            Platform.Flow.Run.ResultMessager message = iRunWork.SendBackFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 9);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);
        }
    }
}
