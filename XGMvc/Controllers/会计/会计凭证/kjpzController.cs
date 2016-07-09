using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Common;
using Business.CommonModule;

namespace BaothApp.Controllers.会计.会计凭证

{   
    public class kjpzController : SpecificController
    {
        public override string ModelUrl
        {
            get { return "kjpz"; }
        }

        public ViewResult Print()
        {
            var template = Request["pturl"];
            return View(template);
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <returns></returns>
        public ViewResult kjpzHistory()
        {
            ViewData["ModelUrl"] = "kjpz";
            return View("kjpzHistory");
        }

        /// <summary>
        /// 主页
        /// </summary>
        /// <returns></returns>
        public override ViewResult Index()
        {
            //1为正常 0为从核销打开
            ViewData["btnControl"] = string.IsNullOrEmpty(Request["Flag"] + "") ? 1 : 0;
            return View("kjpz");
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public override JsonResult New()
        {
            JsonModel result = this.Actor.New();
            return Json(result);
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public override JsonResult Save()
        {
            JsonModel result = this.Actor.Save(this.Status, this.jsonModel);
            return Json(result);
        }
        /// <summary>
        /// 明细
        /// </summary>
        /// <returns></returns>
        public override JsonResult Retrieve()
        {
            JsonModel result = this.Actor.Retrieve(this.Guid);
            return Json(result);
        }
        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public override JsonResult CommitFlow()
        {
            Platform.Flow.Run.IRunWorkFlow iRunWork = new Platform.Flow.Run.RunWorkFlow();
            Platform.Flow.Run.ResultMessager message = iRunWork.CommitFlow(this.Scope, this.CurrentUserInfo.UserGuid, this.Guid, 23);
            this.Actor.UpdateDocState(Guid, EnumType.EnumDocState.Approving);
            return Json(message);

        }

        public ViewResult viewProcess()
        {
            return View("process");
        }
        public JsonResult process()
        {
            Platform.Flow.Run.ResultMessager message = new Platform.Flow.Run.ResultMessager();
            message.Icon = Platform.Flow.Run.MessagerIconEnum.error;
            message.Title = "提示";
            message.Msg = "操作错误";
            Guid g = new System.Guid();
            var dataId = Request["Guid"];
            if (System.Guid.TryParse(dataId, out g))
            {
                var s = Platform.Flow.Run.WorkFlowAPI.GetProccessDoed(g, out message);
                if (message.Msg == null)
                {
                    return Json(s);
                }
                return Json(message);
            }
            return Json(message);
        }


        /// <summary>
        /// 历史
        /// </summary>
        /// <returns></returns>
        public override ContentResult History()
        {
            string condition = Request["condition"];
            PZHistoryCondition conditionModel = new PZHistoryCondition();
            conditionModel = BaothApp.Utils.JsonHelper.JsonToObject<PZHistoryCondition>(condition);
            var instance = this.Actor;
            List<object> result = instance.History(conditionModel);
            if (result != null)
            {
                string rowJson = BaothApp.Utils.JsonHelper.objectToJson(result);
                string json = BaothApp.Utils.JsonHelper.PageJsonFormat(rowJson, result.Count);
                return Content(json);
            }
            else
            {
                string json1 = "{\"errorcode\":\"" + instance.ErrorCode + "\"}";
                return Content(json1);
            }
            
        }

        /// <summary>
        /// 获取首条，上条，下条，末条
        /// </summary>
        /// <returns></returns>
        public JsonResult FetchItem()
        {
            var mactor = new Business.Accountant.会计凭证();
            Business.Accountant.CWPeriodModel model = new Business.Accountant.CWPeriodModel();
            var fetchtype=Request["fetchtype"];
            var curguid = Request["guid"];
            model.FiscalYear = Request["fiscalyear"];
            model.CWPeriod = Request["cwperiod"];
            model.AccountKey = Request["accountkey"];
            var pzmains = mactor.FetchItem(model);

            var sg=Guid.Parse(curguid);

            if (pzmains == null || pzmains.Count == 0)
            {
                return Json(new { id = string.Empty, errcode = "-1" });
            }
            
            switch (fetchtype)
            {
                case "first":
                    var ff = pzmains[0];
                    if (ff.GUID != sg)
                    {
                        return Json(new { id = ff.GUID.ToString(), errcode = string.Empty });
                    }
                    else
                    {
                        return Json(new { id = string.Empty, errcode = "已是首条" });
                    }
                case "pre":
                    for (int i = 0; i < pzmains.Count; i++)
                    {
                        var cobject = pzmains[i];
                        if (cobject.GUID == sg)
                        {
                            if (i == 0)
                            {
                                return Json(new { id = string.Empty, errcode = "已是首条" });
                            }
                            else
                            {
                                return Json(new { id = pzmains[i-1].GUID.ToString(), errcode = string.Empty });
                            }
                        }
                    }
                    return Json(new { id = string.Empty, errcode = string.Empty });
                    
                case "next":
                    for (int i = 0; i < pzmains.Count; i++)
                    {
                        var cobject = pzmains[i];
                        if (cobject.GUID == sg)
                        {
                            if (i == pzmains.Count-1)
                            {
                                return Json(new { id = string.Empty, errcode = "已是末条" });
                            }
                            else
                            {
                                return Json(new { id = pzmains[i+1].GUID.ToString(), errcode = string.Empty });
                            }
                        }
                    }
                    return Json(new { id = string.Empty, errcode = string.Empty });
                case "last":
                    var lf = pzmains[pzmains.Count-1];
                    if (lf.GUID != sg)
                    {
                        return Json(new { id = lf.GUID.ToString(), errcode = string.Empty });
                    }
                    else
                    {
                        return Json(new { id = string.Empty, errcode = "已是末条" });
                    }
            }
            return null;
        }


    }
}
