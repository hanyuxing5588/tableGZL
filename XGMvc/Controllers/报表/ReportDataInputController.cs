using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Infrastructure;
using BaothApp.Utils;
using Business.Common;
using System.Xml.Serialization;
using System.Data;

namespace BaothApp.Controllers.报表
{
    public class ReportDataInputController : Controller
    {
        #region 项目支出执行
        public ViewResult yskcx()
        {
            return View("yskcx");
        }
        public ViewResult excelImport()
        {
            return View("excelImport");
        }
        public ContentResult GetYskcxData()
        {
            var accountType = Request["accountType"];
            var isYE = Request["isYE"] + "" == "1";
            int i;
            int.TryParse(accountType, out i);

            using (BaseConfigEdmxEntities context = new BaseConfigEdmxEntities())
            {
                IEnumerable<RP_YSKView> list = null;
                if (isYE) {
                    list = context.RP_YSKView.Where(e => e.AccountType == i).AsEnumerable();
                }
                else
                {
                    list = context.RP_YSKView.Where(e => e.AccountType == i && e.Balance!=0).AsEnumerable();
                }

                var tempList = list.Select(e => new
                {
                    GUID = e.GUID,
                    DocDate = e.DocDate == null ? "" : ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                    DocMemo = e.DocMemo,
                    DocNum = e.DocNum,
                    Loan = e.Loan,
                    Repayment = e.Repayment,
                    Balance = e.Balance,
                    GUID_Person = e.GUID_Person,
                    PersonKey = e.PersonKey,
                    PersonName = e.PersonName,
                    GUID_Department = e.GUID_Department,
                    DepartmentKey = e.DepartmentKey,
                    DepartmentName = e.DepartmentName,
                    Remark = e.Remark
                }).OrderByDescending(e => e.DocDate).ToList();

                var yskWrite = new CAE.YSKMXWrite();
                var u8Value = yskWrite.GetU8账面支出(i);
                var Loan = tempList.Sum(e => e.Loan);
                var Repayment = tempList.Sum(e => e.Repayment);
                var Balance = tempList.Sum(e => e.Balance);
                var rowJson = BaothApp.Utils.JsonHelper.objectToJson(tempList);
                var rowTotal = "[{\"Loan\":" + Loan + ",\"Repayment\":" + Repayment + ",\"Balance\":" + Balance + ",\"PersonName\":\"账面合计:" + u8Value + "\"}]";
                string json = JsonHelper.PageTotalJsonFormat(rowJson, rowTotal, tempList.Count);
                return Content(json);
            }
        }
       
        public JsonResult Save()
        {
            var data = Request["data"];
            var accountType = Request["accountType"];
            var isYE = Request["isYE"] + "" == "1";
            int i;
            int.TryParse(accountType, out i);
            var list = JsonHelp.JsonToObject<List<RP_YSK>>(data);
            string msg = string.Empty;
            var b = SaveRelative(isYE,i, list, out msg);
            return Json(new { flag = b, msg = msg });
        }
        public JsonResult GetUploadData() 
        {
            var filePath = Request["filepath"];
            var message = "";
            var dtList=ImportExcel.ImportAll(filePath,out message);

            var msg= new ImpoartYSKData().ImportData(dtList, out message);
            if (message != "")
            {
                return Json(new { msg = message });
            }
            if(dtList==null){
                return Json(new {msg="导入错误"});
            }
             return Json(new {msg="导入成功"});
        }

        public bool SaveRelative(bool isYE,int type, List<RP_YSK> item, out string msg)
        {
            msg = string.Empty;
            try
            {
                using (BaseConfigEdmxEntities context = new BaseConfigEdmxEntities())
                {
                    for (int i = 0, j = item.Count; i < j; i++)
                    {
                        msg += VerifyData(item[i], i);
                    }
                    if (msg.Length <= 0)
                    {
                        List<RP_YSK> list = new List<RP_YSK>();
                        if (isYE)
                        {
                            list = context.RP_YSK.Where(e => e.AccountType == type).ToList();
                        }
                        else 
                        {
                            list = context.RP_YSK.Where(e => e.AccountType == type&&e.Balance!=0).ToList();
                        }
                        foreach (var it in list)
                        {
                            context.DeleteObject(it);
                        }
                        if (item != null)
                        {
                            foreach (var it in item)
                            {
                                var model = context.RP_YSK.CreateObject();
                                model.GUID = Guid.NewGuid();
                                model.GUID_Person = it.GUID_Person;
                                model.GUID_Department = it.GUID_Department;
                                model.Loan = it.Loan;
                                model.Repayment = it.Repayment;
                                model.Balance = it.Balance;
                                model.DocDate = it.DocDate;
                                model.DocMemo = it.DocMemo;
                                model.DocNum = it.DocNum;
                                model.Remark = it.Remark;
                                model.AccountType = type;
                                context.RP_YSK.AddObject(model);
                            }
                        }
                        context.SaveChanges();
                        msg = "保存成功！";
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                msg = "保存失败！";
                return false;
            }
        }

        private string VerifyData(RP_YSK item, int i)
        {
            string retStr = string.Empty;
            if (item.DocDate.IsNullOrEmpty())
            {
                retStr += "录入日期不能为空！</br>";
            }
            if (item.DocNum == null)
            {
                retStr += "凭证号不能为空！</br>";

            }
            if (item.GUID_Department.IsNullOrEmpty())
            {
                retStr += "所属部门不能为空！</br>";

            }
            if (retStr != "")
            {
                retStr = "第" + (i + 1) + "行:" + retStr;
            }
            return retStr;
        }

        #endregion

    }
    public class ImpoartYSKData 
    {

        public bool  ImportData(List<DataTable> dtList,out string msg) 
        {
            msg = "";
            try
            {

           
            BaseConfigEdmxEntities context = new BaseConfigEdmxEntities();
            var listPerson = context.SS_Person.Select(e => new { 
                e.GUID,
                e.PersonName
            }).ToList();
            var listDep = context.SS_Department.Select(e => new { 
                e.GUID,
                e.DepartmentName
            }).ToList();
            for (int i = 0; i < dtList.Count; i++)
            {

                var dt = dtList[i];
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    DataRow dr = dt.Rows[j];
                    var model = context.RP_YSK.CreateObject();
                    model.GUID = Guid.NewGuid();
                    var strName= dr[5].ToString();
                    var person=listPerson.FirstOrDefault(e => e.PersonName ==strName);
                    if (person == null) {
                        msg = "第"+(i+1).ToString()+"的sheet页中,第"+(j+2).ToString()+"行的借款人在系统查找不到";
                        return false;
                    }
                    model.GUID_Person = person.GUID;
                    var depName = dr[6].ToString();
                    var dep = listDep.FirstOrDefault(e => e.DepartmentName == depName);
                    if (dep == null)
                    {
                        msg = "第" + (i + 1).ToString() + "的sheet页中,第" + (j + 2).ToString() + "行的部门名称在系统查找不到";
                        return false;
                    }
                    model.GUID_Department = dep.GUID;

                    double d;
                    if (!double.TryParse(dr[3]+"",out d))
                    {

                    }
                    model.Loan = d;
                    double d1;
                    if (!double.TryParse(dr[4] + "", out d1))
                    {

                    }
                    model.Repayment = d1;
                    model.Balance =d-d1;
                    DateTime dt11 = DateTime.Now ;
                    if (!DateTime.TryParse(dr[0]+"", out dt11))
                    {
                        dt11 = DateTime.Now;
                    }
                    model.DocDate = dt11;
                    model.DocMemo =dr[2]+"";
                    model.DocNum = dr[1] + "";
                    model.Remark = dr[7] + "";
                    model.AccountType = i;
                    context.RP_YSK.AddObject(model);
                }
            }
            var list= context.RP_YSK.ToList();
            foreach (var it in list)
            {
                context.RP_YSK.DeleteObject(it);
            }
            context.SaveChanges();
            return true;
            }
            catch (Exception ex)
            {
                msg = "导入格式错误";
                return false;
            }
        }
    }
   
}
