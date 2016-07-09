using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.CommonModule;
using System.Text;
using Business.Common;

namespace BaothApp.Controllers.出纳.核销处理
{   
    public class hxclController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "hxcl"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        //核销主界面
        public override ViewResult Index()
        {
            ViewData["currentDate"] = DateTime.Now.ToString("yyyy-MM-dd");
            ViewData["curPerson"] = this.CurrentUserInfo.UserName;
            ViewData["isProcess"] = (Request["isProcess"] + "");
            ViewData["Guid"] = (Request["guid"] + "");
            ViewData["DocType"] = (Request["common"] + "");
            return View("hxcl");
        }
        //选单
        public ViewResult selectDoc()
        {
            return View("selectDoc");
        }
        //借款
        public ViewResult jk()
        {
            var userborrowid = Request["UserBorrowID"];
            ViewData["ModelUrl"] = "jk";
            ViewData["GridUrl"] = "/hxcl/BorrowList?borrowUserId="+userborrowid;
            return View("jk");
        }
        public JsonResult ChangeDocData()
        {
            //单据guid
            string guid = Request["guid"];
            //单据类型
            string docType = Request["docType"];
            //标示 用来区分选单:0 借款:1
            var isDC = Request["mark"]=="1"?true:false;
            //核销日期
            DateTime dt=DateTime.Now;
            DateTime.TryParse(Request["hxDate"]+"",out dt);
            var o= this.Actor.ChangeDocData(guid,docType,isDC,dt);
            return Json(o);
        }
        public override JsonResult Save()
        {
            //单据guid
            string guid = Request["guid"];
            //单据类型
            string docType = Request["docType"];
            //标示 用来区分选单:0 借款:1
            string isDC = Request["mark"];
            //核销日期
            string date=Request["hxDate"] + "";
            //凭证号
            string pzNum = Request["pzNum"];
            //保存方式
            string isSave = Request["isSave"];
            var strTemp = string.Format("{0}#{1}#{2}#{3}#{4}#{5}", guid, docType, isDC, date, pzNum, isSave);
            var o = this.Actor.SaveWithReturnObj(strTemp, this.jsonModel);//Json(new {error:''});

            //验证是否核销
            Guid guid1=Guid.Empty;
            Guid.TryParse(guid,out guid1);
            var entHX = this.Actor.BusinessContext.HX_Detail.FirstOrDefault(e => e.GUID_Main == guid1);
            if (entHX == null)
            {
                return Json(o);
            }
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow("hxcl", this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approved);
            return Json(o);
        }
        // 选单列表查询
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

        /// <summary>
        /// 借款选单列表
        /// </summary>
        /// <returns></returns>
        public  JsonResult BorrowList()
        {
            var borrowUserId = Request["borrowUserId"];
            return Json(this.Actor.BorrowList(borrowUserId), JsonRequestBehavior.AllowGet);
        }
        //提交
        public override JsonResult CommitFlow()
        {
            //验证是否核销
            var entHX = this.Actor.BusinessContext.HX_Detail.FirstOrDefault(e => e.GUID_Main == this.Guid);
            if (entHX == null) {
                var msg = new Platform.Flow.Run.ResultMessager() { Msg="没有核销单据不能进行提交",Title="提示" };
                return Json(msg);
            }
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
           //this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
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
        //获得借款总金额
        public JsonResult GetBorrowMenoy() 
        {
            var jsonModel=this.Actor.Retrieve("");
            return Json(new {sumMenoy=jsonModel.result},JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取打印数据
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPrintData()
        {
            var guid=Request["guid"];
            var doctypeKey=Request["doctypekey"];
            var paymentnumber=Request["paymentnumber"];
            //guid = "1B8D436A-A251-42F7-8E2E-2802FFC7E25E";
            //doctypeKey = "07";
           // paymentnumber = "1234567812";

            Business.Casher.核销处理 obj = new Business.Casher.核销处理();
            var model = obj.GetPrintData(guid, doctypeKey,paymentnumber);
            return Json(model,JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetTreePrintTemp()
        {
            //GetTreehkspdpz
            var guid = Request["guid"];
            Business.Casher.核销处理 obj = new Business.Casher.核销处理();
            var model = obj.GetTreehkspdpz(guid);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
