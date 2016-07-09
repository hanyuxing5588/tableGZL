using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Infrastructure;

using OAModel;
using Business.Common;
using Business.CommonModule;

namespace BaothApp.Controllers.Infrastructure
{
    public class TreeController : SpecificController
    {
        IntrastructureFun dbobj = new IntrastructureFun();
        
        CommonTree commTree = new CommonTree();
        //
        // GET: /Tree/

        public ContentResult GetBGCodeTreeFX()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var year = Request["year"];
            var proList = new List<SS_BGCode>();
            var proClassList = new List<SS_BGCode>();
            if (!string.IsNullOrEmpty(year))
            {
                int outYear;
                if (int.TryParse(year, out outYear))
                {
                    using ( var context = new BaseConfigEdmxEntities())
                    {
                        proList = context.ExecuteStoreQuery<SS_BGCode>(@"SELECT DISTINCT
        b.*
FROM    dbo.BG_Detail a
        LEFT JOIN dbo.SS_BGCode b ON a.GUID_BGCode = b.GUID
WHERE   BGYear = " + year + " AND LEN(BGCodeKey)=4 ORDER BY BGCodeKey").ToList();
                        //报表
                        proClassList = context.ExecuteStoreQuery<SS_BGCode>(@"SELECT DISTINCT
        b.*
FROM    dbo.BG_Detail a
        LEFT JOIN dbo.SS_BGCode b ON a.GUID_BGCode = b.GUID
WHERE   BGYear = " + year + " AND LEN(BGCodeKey)=2 ORDER BY BGCodeKey").ToList();
                     
                    }

                }
            }
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int i = 0; i < proClassList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = proClassList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(proClassList[i].BGCodeName + "(" + proClassList[i].BGCodeKey + ")");
                 Dictionary<string, string> dic = new Dictionary<string, string>();
                 dic["m"] = "ss_code";
                dic["BGCodeKey"] = CommonFuntion.StringToJson(proClassList[i].BGCodeKey.ToString());
                dic["BGCodeName"] = CommonFuntion.StringToJson(proClassList[i].BGCodeName);
                treeModel.attributes = dic;
                treeModel.state = "open";
                var childList = proList.FindAll(e => e.PGUID == proClassList[i].GUID);
                if (childList.Count > 0) {
                    treeModel.children = new List<TreeNodeModel>();
                }
                foreach (var item in childList)
	            {
		 
                    TreeNodeModel treeModel1 = new TreeNodeModel();
                    treeModel1.id = item.GUID.ToString();
                    treeModel1.text = CommonFuntion.StringToJson(item.BGCodeName + "(" + item.BGCodeKey + ")");
                 Dictionary<string, string> dic1 = new Dictionary<string, string>();
                 dic1["m"] = "ss_code";
                 dic1["BGCodeKey"] = CommonFuntion.StringToJson(item.BGCodeKey.ToString());
                 dic1["BGCodeName"] = CommonFuntion.StringToJson(item.BGCodeName);
                    treeModel1.attributes = dic1;
                    treeModel1.state = "open";
                    treeModel.children.Add(treeModel1);
               }
                treeModelList.Add(treeModel);
            }
           var jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        public override ViewResult Index()
        {
            return View();
        }
        /// <summary>
        /// 单位的单位子项        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>Penson
        private List<TreeNodeModel> GetDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_Department> orgDepList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "[" + currentList[i].DWKey + "]");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    treeModel.attributes = dic;
                    treeModel.isCheck = true;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childNodeList = new List<TreeNodeModel>();
                        //单位下的单位子节点






                        var dwChild = GetDWChildNode(childList, orgList, orgDepList);
                        if (dwChild != null && dwChild.Count > 0)
                        {
                            childNodeList.AddRange(dwChild);
                        }
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetDepChild(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                childNodeList.AddRange(depChild);
                            }
                        }
                        treeModel.children = childNodeList;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetDepChild(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 基础----单位的单位子项----tree
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>Penson
        private List<TreeNodeModel> GetJCDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_DepartmentView> orgDepList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "[" + currentList[i].DWKey + "]");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey.ToString());
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    dic["valid"] = "1";
                    treeModel.attributes = dic;
                    treeModel.isCheck = true;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childNodeList = new List<TreeNodeModel>();
                        //单位下的单位子节点






                        var dwChild = GetJCDWChildNode(childList, orgList, orgDepList);
                        if (dwChild != null && dwChild.Count > 0)
                        {
                            childNodeList.AddRange(dwChild);
                        }
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetJCDepChild(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                childNodeList.AddRange(depChild);
                            }
                        }
                        treeModel.children = childNodeList;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetJCDepChild(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 单位的单位子项        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>Penson
        private List<TreeNodeModel> GetDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_Department> orgDepList, List<PersonTreeModel> personList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            int personCount = 0;
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    personCount = 0;
                    var pList = personList.FindAll(e => e.GUID_DW == currentList[i].GUID);
                    if (pList != null)
                    {
                        personCount = pList.Count();
                    }
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "[" + currentList[i].DWKey + "]");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    dic["personCount"] = personCount.ToString();
                    treeModel.attributes = dic;
                    treeModel.isCheck = true;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childNodeList = new List<TreeNodeModel>();
                        //单位下的单位子节点






                        var dwChild = GetDWChildNode(childList, orgList, orgDepList, personList);
                        if (dwChild != null && dwChild.Count > 0)
                        {
                            childNodeList.AddRange(dwChild);
                        }
                        //不是末级单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetDepChild(depChildList, orgDepList, personList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                childNodeList.AddRange(depChild);
                            }
                        }
                        treeModel.children = childNodeList;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetDepChild(depChildList, orgDepList, personList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 单位下的人员信息
        /// </summary>
        /// <param name="currentList">当前部门信息</param>
        /// <param name="orgList">原始部门信息</param>
        /// <param name="orgDepList">原始单位</param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetPensonDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_Department> orgDepList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "(" + currentList[i].DWKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var childNode = GetPensonDWChildNode(childList, orgList, orgDepList);
                        if (childNode != null && childNode.Count > 0)
                        {
                            treeModelChild = childNode;
                        }
                        treeModel.children = treeModelChild;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetPensonDepChildNode(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        private List<TreeNodeModel> GetPensonDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_Department> orgDepList, List<PersonTreeModel> orgPersonList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "(" + currentList[i].DWKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var childNode = GetPensonDWChildNode(childList, orgList, orgDepList, orgPersonList);
                        if (childNode != null && childNode.Count > 0)
                        {
                            treeModelChild = childNode;
                        }
                        treeModel.children = treeModelChild;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetPensonDepChildNode(depChildList, orgDepList, orgPersonList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        /// <summary>
        /// 基础----单位下的人员信息----tree
        /// </summary>
        /// <param name="currentList">当前部门信息</param>
        /// <param name="orgList">原始部门信息</param>
        /// <param name="orgDepList">原始单位</param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetJCPensonDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList, List<SS_Department> orgDepList)
        {
            string DwModel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "(" + currentList[i].DWKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = DwModel;
                    dic["valid"] = "1";
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var childNode = GetJCPensonDWChildNode(childList, orgList, orgDepList);
                        if (childNode != null && childNode.Count > 0)
                        {
                            treeModelChild = childNode;
                        }
                        treeModel.children = treeModelChild;
                    }
                    else
                    {
                        //单位下的部门
                        var depChildList = orgDepList.FindAll(e => e.GUID_DW == currentList[i].GUID && e.PGUID == null);//部门子项并且是单位的父项
                        if (depChildList != null && depChildList.Count > 0)
                        {
                            var depChild = GetJCPensonDepChildNode(depChildList, orgDepList);// 部门子项;
                            if (depChild != null && depChild.Count > 0)
                            {
                                treeModel.children = depChild;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 单位的单位子项


        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetAttributesDWChildNode(List<SS_DW> currentList, List<SS_DW> orgList)
        {
            string dwmodel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeNodeList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "(" + currentList[i].DWKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = dwmodel;
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> dwChild = new List<TreeNodeModel>();
                        var list = GetAttributesDWChildNode(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            dwChild = list;
                        }
                        treeModel.children = dwChild;

                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = dwmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName.ToString());
                        dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    }
                    treeNodeList.Add(treeModel);
                }
            }
            return treeNodeList;
        }

        /// <summary>
        /// 部门的部门子项


        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetDepChild(List<SS_Department> currentList, List<SS_Department> orgList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var depChild = GetDepChild(childList, orgList);
                        if (depChild != null && depChild.Count > 0)
                        {
                            treeModel.children = depChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = depmodel;
                        dic["valid"] = "1";
                        dic["DepartmentKey"] = CommonFuntion.StringToJson(currentList[i].DepartmentKey.ToString());
                        dic["GUID_DW"] = currentList[i].GUID_DW.ToString();
                        dic["DWName"] =currentList[i].SS_DW==null?"": CommonFuntion.StringToJson(currentList[i].SS_DW.DWName);
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["DepartmentName"] = CommonFuntion.StringToJson(currentList[i].DepartmentName.ToString());
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 基础----部门的部门子项----tree
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetJCDepChild(List<SS_DepartmentView> currentList, List<SS_DepartmentView> orgList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    dic["valid"] = "1";
                    dic["GUID_DW"] = currentList[i].GUID_DW.ToString();
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["DepartmentName"] = CommonFuntion.StringToJson(currentList[i].DepartmentName.ToString());
                    dic["DepartmentKey"] = CommonFuntion.StringToJson(currentList[i].DepartmentKey.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var depChild = GetJCDepChild(childList, orgList);
                        if (depChild != null && depChild.Count > 0)
                        {
                            treeModel.children = depChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = depmodel;
                        dic["valid"] = "1";
                        dic["GUID_DW"] = currentList[i].GUID_DW.ToString();
                        dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                        dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                        dic["DepartmentName"] = CommonFuntion.StringToJson(currentList[i].DepartmentName.ToString());
                        dic["DepartmentKey"] = CommonFuntion.StringToJson(currentList[i].DepartmentKey.ToString());
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 部门的部门子项


        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetDepChild(List<SS_Department> currentList, List<SS_Department> orgList, List<PersonTreeModel> personList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            int personCount = 0;
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    personCount = 0;
                    //如果有多级时，人数要改


                    //currentList[i].RetrieveLeafs(orgList);
                    var pList = personList.FindAll(e => e.GUID_Department == currentList[i].GUID);
                    if (pList != null)
                    {
                        personCount = pList.Count();
                    }
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    dic["personCount"] = personCount.ToString();
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var depChild = GetDepChild(childList, orgList, personList);
                        if (depChild != null && depChild.Count > 0)
                        {
                            treeModel.children = depChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = depmodel;
                        dic["valid"] = "1";
                        dic["GUID_DW"] = currentList[i].GUID_DW.ToString();
                        dic["DWName"] = CommonFuntion.StringToJson(currentList[i].SS_DW.DWName);
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["DepartmentName"] = CommonFuntion.StringToJson(currentList[i].DepartmentName.ToString());
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 部门的部门子项





        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetPensonDepChildNode(List<SS_Department> currentList, List<SS_Department> orgList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var chilList = GetPensonDepChildNode(childList, orgList);
                        if (childList != null && childList.Count > 0)
                        {
                            treeModelChild = chilList;
                        }
                        treeModel.children = treeModelChild;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <param name="orgPersonList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetPensonDepChildNode(List<SS_Department> currentList, List<SS_Department> orgList, List<PersonTreeModel> orgPersonList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var chilList = GetPensonDepChildNode(childList, orgList, orgPersonList);
                        if (childList != null && childList.Count > 0)
                        {
                            treeModelChild = chilList;
                        }
                        treeModel.children = treeModelChild;
                    }
                    else
                    {
                        Guid gId;
                        Guid.TryParse(treeModel.id, out gId);
                        var pesonlist = orgPersonList.FindAll(e => e.GUID_Department == gId);
                        var list = GetDepPersonNode(pesonlist);
                        if (list != null && list.Count > 0)
                        {
                            treeModel.children = list;
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 基础----部门的部门子项----tree
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetJCPensonDepChildNode(List<SS_Department> currentList, List<SS_Department> orgList)
        {
            string depmodel = typeof(SS_Department).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DepartmentName + "(" + currentList[i].DepartmentKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = depmodel;
                    dic["valid"] = "1";
                    dic["DepartmentName"] = CommonFuntion.StringToJson(currentList[i].DepartmentName);
                    dic["DepartmentKey"] = CommonFuntion.StringToJson(currentList[i].DepartmentKey);
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        List<TreeNodeModel> treeModelChild = new List<TreeNodeModel>();
                        var chilList = GetJCPensonDepChildNode(childList, orgList);
                        if (childList != null && childList.Count > 0)
                        {
                            treeModelChild = chilList;
                        }
                        treeModel.children = treeModelChild;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }


        /// <summary>
        /// 项目分类中的子项
        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetChildProjectClassNode(List<SS_ProjectClass> currentList, List<SS_ProjectClass> orgList, List<ProjectTreeModel> orgProjectList, string isClosedProClass = "closed")
        {
            string pcmodel = typeof(SS_ProjectClass).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].ProjectClassName + "(" + currentList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = pcmodel;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = isClosedProClass;
                        List<TreeNodeModel> childClassOrProjectList = new List<TreeNodeModel>();
                        var childClassList = GetChildProjectClassNode(childList, orgList, orgProjectList);
                        //项目分类
                        if (childClassList != null && childClassList.Count > 0)
                        {
                            childClassOrProjectList.AddRange(childClassList);
                        }
                        //不是末级的项目分类下还可能有项目
                        var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目分类的子项目
                        if (childProjectList != null && childProjectList.Count > 0)
                        {
                            var childProList = GetChildProject(childProjectList, orgProjectList);//项目子项
                            if (childProList != null && childProList.Count > 0)
                            {
                                childClassOrProjectList.AddRange(childProList);
                            }
                        }
                        if (childClassOrProjectList != null && childClassOrProjectList.Count > 0)
                        {
                            treeModel.children = childClassOrProjectList;
                        }
                    }
                    else
                    {
                        var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目子项
                        if (childProjectList != null && childProjectList.Count > 0)
                        {
                            treeModel.state = isClosedProClass;
                            var childProList = GetChildProject(childProjectList, orgProjectList);//项目子项
                            if (childProList != null && childProList.Count > 0)
                            {
                                treeModel.children = childProList;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }



        /// <summary>
        /// 项目的子项





        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetChildProject(List<ProjectTreeModel> currentList, List<ProjectTreeModel> orgList)
        {
            string prjModel = typeof(SS_Project).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = string.Format("({0}){1}", CommonFuntion.StringToJson(currentList[i].ProjectKey), CommonFuntion.StringToJson(currentList[i].ProjectName));
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = prjModel;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectName);
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var list = GetChildProject(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            treeModel.children = list;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = prjModel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectKey.ToString());
                        dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectName);
                        dic["GUID_FunctionClass"] = CommonFuntion.StringToJson(currentList[i].GUID_FunctionClass.ToString());
                        dic["ExtraCode"] = currentList[i].ExtraCode == null ? "" : currentList[i].ExtraCode;
                        dic["FinanceCode"] = currentList[i].FinanceCode == null ? "" : currentList[i].FinanceCode;
                        dic["IsFinance"] = currentList[i].IsFinance == null ? "false" : currentList[i].IsFinance.ToString();
                        dic["IsLeaf"] = "1";
                        treeModel.attributes = dic;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 获取BGCode科目的子信息
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildBGCodeNode(List<BGCodeTreeModel> currentList, List<BGCodeTreeModel> orgList)
        {
            string bgmodel = typeof(SS_BGCode).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].BGCodeName + "(" + currentList[i].BGCodeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = bgmodel;
                    treeModel.attributes = dic;

                    List<BGCodeTreeModel> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetChildBGCodeNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = bgmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["BGCodeName"] = CommonFuntion.StringToJson(currentList[i].BGCodeName);
                        dic["BGCodeKey"] = CommonFuntion.StringToJson(currentList[i].BGCodeKey);
                        dic["GUID_EconomyClass"] = currentList[i].GUID_EconomyClass.ToString();
                        dic["EconomyClassKey"] = currentList[i].EconomyClassKey;
                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }

        /// <summary>
        /// 获取文件下载的子信息
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildFileTypeNode(List<OfficeFileType> currentList, List<OfficeFileType> orgList)
        {
            string model = typeof(SS_OfficeFileType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].FileTypeName + "(" + currentList[i].FileTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var temp = currentList[i];
                    temp = temp != null ? temp : null;
                    dic["m"] = model;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                    dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                    dic["FileTypeName"] = CommonFuntion.StringToJson(temp.FileTypeName != null ? temp.FileTypeName.ToString() : "");
                    dic["FileTypeKey"] = CommonFuntion.StringToJson(temp.FileTypeKey != null ? temp.FileTypeKey.ToString() : "");
                    treeModel.attributes = dic;

                    List<OfficeFileType> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "open";
                        var bgChild = GetChildFileTypeNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = model;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                        dic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }

        /// <summary>
        /// 获取功能分类信息
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildFunctionClassNode(List<SS_FunctionClassView> currentList, List<SS_FunctionClassView> orgList)
        {
            string funClassmodel = typeof(SS_FunctionClass).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].FunctionClassName + "(" + currentList[i].FunctionClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var temp = currentList[i];
                    temp = temp != null ? temp : null;
                    dic["m"] = funClassmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                    dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                    dic["FunctionClassName"] = CommonFuntion.StringToJson(temp.FunctionClassName != null ? temp.FunctionClassName.ToString() : "");
                    dic["FunctionClassKey"] = CommonFuntion.StringToJson(temp.FunctionClassKey != null ? temp.FunctionClassKey.ToString() : "");
                    dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                    dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                    dic["BeginYear"] = CommonFuntion.StringToJson(temp.BeginYear.ToString());
                    dic["FinanceCode"] = CommonFuntion.StringToJson(temp.FinanceCode != null ? temp.FinanceCode.ToString() : "");
                    dic["IsDefault"] = CommonFuntion.StringToJson(temp.IsDefault != null ? temp.IsDefault.ToString() : "");
                    dic["IsProject"] = CommonFuntion.StringToJson(temp.IsProject != null ? temp.IsProject.ToString() : "");
                    dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                    treeModel.attributes = dic;

                    List<SS_FunctionClassView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetChildFunctionClassNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = funClassmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["FunctionClassName"] = CommonFuntion.StringToJson(currentList[i].FunctionClassName.ToString());
                        dic["FunctionClassKey"] = CommonFuntion.StringToJson(currentList[i].FunctionClassKey.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey.ToString());
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName.ToString());
                        dic["BeginYear"] = CommonFuntion.StringToJson(currentList[i].BeginYear.ToString());
                        dic["FinanceCode"] = CommonFuntion.StringToJson(currentList[i].FinanceCode.ToString());
                        dic["IsDefault"] = CommonFuntion.StringToJson(currentList[i].IsDefault.ToString());
                        dic["IsProject"] = CommonFuntion.StringToJson(currentList[i].IsProject.ToString());
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());

                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }

        /// <summary>
        /// 获取收入类型信息
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildSRTypeNode(List<SS_SRTypeView> currentList, List<SS_SRTypeView> orgList)
        {
            string funClassmodel = typeof(SS_SRType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].SRTypeName + "(" + currentList[i].SRTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var temp = currentList[i];
                    temp = temp != null ? temp : null;
                    dic["m"] = funClassmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                    dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                    dic["SRTypeName"] = CommonFuntion.StringToJson(temp.SRTypeName != null ? temp.SRTypeName.ToString() : "");
                    dic["SRTypeKey"] = CommonFuntion.StringToJson(temp.SRTypeKey != null ? temp.SRTypeKey.ToString() : "");
                    dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                    dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                    dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                    treeModel.attributes = dic;

                    List<SS_SRTypeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetChildSRTypeNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = funClassmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["SRTypeKey"] = CommonFuntion.StringToJson(currentList[i].SRTypeName.ToString());
                        dic["SRTypeKey"] = CommonFuntion.StringToJson(currentList[i].SRTypeKey.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey.ToString());
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName.ToString());
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());

                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }
        /// <summary>
        /// 获取往来分类信息


        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildWLTypeNode(List<SS_WLTypeView> currentList, List<SS_WLTypeView> orgList)
        {
            string wlModel = typeof(SS_WLType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].WLTypeName + "(" + currentList[i].WLTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var temp = currentList[i];
                    temp = temp != null ? temp : null;
                    dic["m"] = wlModel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                    dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                    dic["WLTypeName"] = CommonFuntion.StringToJson(temp.WLTypeName != null ? temp.WLTypeName.ToString() : "");
                    dic["WLTypeKey"] = CommonFuntion.StringToJson(temp.WLTypeKey != null ? temp.WLTypeKey.ToString() : "");
                    dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                    dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                    dic["IsDC"] = CommonFuntion.StringToJson(temp.IsDC.ToString());
                    dic["IsCustomer"] = CommonFuntion.StringToJson(temp.IsCustomer.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop.ToString());
                    treeModel.attributes = dic;

                    List<SS_WLTypeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetChildWLTypeNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = wlModel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["WLTypeName"] = CommonFuntion.StringToJson(currentList[i].WLTypeName.ToString());
                        dic["WLTypeKey"] = CommonFuntion.StringToJson(currentList[i].WLTypeKey.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey.ToString());
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName.ToString());
                        dic["IsDC"] = CommonFuntion.StringToJson(currentList[i].IsDC.ToString());
                        dic["IsCustomer"] = CommonFuntion.StringToJson(currentList[i].IsCustomer.ToString());
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());

                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }

        /// <summary>
        /// 获取子菜单信息



        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetChildMenuNode(List<SS_MenuView> currentList, List<SS_MenuView> orgList)
        {
            string mModel = typeof(SS_Menu).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].MenuName + "(" + currentList[i].MenuKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    var temp = currentList[i];
                    temp = temp != null ? temp : null;
                    dic["m"] = mModel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                    dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                    dic["GUID_MenuClass"] = CommonFuntion.StringToJson(temp.GUID_MenuClass != null ? temp.GUID_MenuClass.ToString() : "");
                    dic["MenuName"] = CommonFuntion.StringToJson(temp.MenuName != null ? temp.MenuName.ToString() : "");
                    dic["MenuKey"] = CommonFuntion.StringToJson(temp.MenuKey != null ? temp.MenuKey.ToString() : "");
                    dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                    dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                    dic["scope"] = CommonFuntion.StringToJson(temp.Scope != null ? temp.Scope.ToString() : "");
                    treeModel.attributes = dic;

                    List<SS_MenuView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetChildMenuNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = mModel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["GUID_MenuClass"] = CommonFuntion.StringToJson(currentList[i].GUID_MenuClass.ToString());
                        dic["MenuName"] = CommonFuntion.StringToJson(currentList[i].MenuName.ToString());
                        dic["MenuKey"] = CommonFuntion.StringToJson(currentList[i].MenuKey.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey.ToString());
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName.ToString());
                        dic["scope"] = CommonFuntion.StringToJson((currentList[i].Scope == null || currentList[i].Scope == "") ? "" : currentList[i].Scope.ToString());

                    }
                    treeModelList.Add(treeModel);
                }

            }
            return treeModelList;

        }
        /// <summary>
        /// 人员Json数据
        /// </summary>
        /// <param name="personModelList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetPersonNode(List<PersonTreeModel> personModelList)
        {
            string permodel = typeof(SS_Person).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (personModelList != null)
            {
                for (int j = 0; j < personModelList.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = personModelList[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(personModelList[j].PersonName + "(" + personModelList[j].PersonKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID_DW"] = personModelList[j].GUID_DW.ToString();
                    dic["DWName"] = CommonFuntion.StringToJson(personModelList[j].DWName);
                    dic["GUID_Department"] = CommonFuntion.StringToJson(personModelList[j].GUID_Department.ToString());
                    dic["DepartmentName"] = CommonFuntion.StringToJson(personModelList[j].DepartmentName);
                    dic["GUID"] = CommonFuntion.StringToJson(personModelList[j].GUID.ToString());
                    dic["OfficialCard"] = CommonFuntion.StringToJson(personModelList[j].OfficialCard);
                    dic["PersonName"] = CommonFuntion.StringToJson(personModelList[j].PersonName);
                    dic["PersonKey"] = CommonFuntion.StringToJson(personModelList[j].PersonKey);
                    dic["BankCardNo"] = CommonFuntion.StringToJson(personModelList[j].BankCardNo);
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }


        #region 基础--预算公式设置
        /// <summary>
        /// 预算公式
        /// </summary>
        /// <returns></returns>
        public ContentResult GetSalarySetupTree()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getyusuanItem();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetSalarySetupNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 预算公式JSon数据
        /// </summary>
        /// <param name="SalaryItemModelList"></param>
        /// <returns></returns>
        public List<TreeNodeModel> GetSalarySetupNode(List<SalarySetupItemTreeModel> SalaryItemModelList)
        {
            string SetupModel = typeof(BG_Item).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < SalaryItemModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = SalaryItemModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(SalaryItemModelList[j].BGItemName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = SetupModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(SalaryItemModelList[j].GUID.ToString());
                dic["BGItemKey"] = CommonFuntion.StringToJson(SalaryItemModelList[j].BGItemKey);
                dic["BGItemName"] = CommonFuntion.StringToJson(SalaryItemModelList[j].BGItemName);
                dic["BGItemMemo"] = CommonFuntion.StringToJson(SalaryItemModelList[j].BGItemMemo);
                dic["IsStop"] = CommonFuntion.StringToJson(SalaryItemModelList[j].IsStop.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        /// <summary>
        /// 根据条件获取对应的预算项
        /// </summary>
        /// <returns></returns>
        public ContentResult GetSalarySetupByContions()
        {
            var tabName = Request["tabName"];
            string mModel = typeof(BG_Setup).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var q = dbobj.context.BG_Setup.Where(e => e.BGSetupName == tabName).Select(e => e.GUID);
            var list = dbobj.context.BG_SetupDetailView.Where(e => q.Contains((Guid)e.GUID_BGSetup)).OrderBy(e => e.ItemOrder).Select(e => new { e.GUID, e.GUID_Item, e.BGItemName, e.ItemFormula, e.ItemDefaultFormula }).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = list[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(list[i].BGItemName);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = list[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["GUID_Item"] = CommonFuntion.StringToJson(temp.GUID_Item != null ? temp.GUID_Item.ToString() : "");
                dic["BGItemName"] = CommonFuntion.StringToJson(temp.BGItemName != null ? temp.BGItemName : "");
                dic["ItemFormula"] = CommonFuntion.StringToJson(temp.ItemFormula != null ? temp.ItemFormula : "");
                dic["ItemDefaultFormula"] = CommonFuntion.StringToJson(temp.ItemDefaultFormula != null ? temp.ItemDefaultFormula : "");
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        #endregion

        //工资项目Json数据
        private List<TreeNodeModel> GetSalaryItemNode(List<SalaryItemTreeModel> SalaryItemModelList)
        {
            string ItemModel = typeof(SA_Item).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < SalaryItemModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = SalaryItemModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(SalaryItemModelList[j].ItemName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(SalaryItemModelList[j].GUID.ToString());
                dic["ItemKey"] = CommonFuntion.StringToJson(SalaryItemModelList[j].ItemKey.ToString());
                dic["ItemName"] = CommonFuntion.StringToJson(SalaryItemModelList[j].ItemName.ToString());
                dic["ItemType"] = CommonFuntion.StringToJson(SalaryItemModelList[j].ItemType.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(SalaryItemModelList[j].IsStop.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        //工资计划Json数据
        private List<TreeNodeModel> GetSalaryPlanNode(List<SalaryPlanTreeModel> SalaryPlanModelList)
        {
            string ItemModel = typeof(SA_PlanView).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < SalaryPlanModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = SalaryPlanModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(SalaryPlanModelList[j].PlanName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(SalaryPlanModelList[j].GUID.ToString());
                dic["ItemKey"] = CommonFuntion.StringToJson(SalaryPlanModelList[j].PlanKey.ToString());
                dic["ItemName"] = CommonFuntion.StringToJson(SalaryPlanModelList[j].PlanName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(SalaryPlanModelList[j].IsStop.ToString());
                dic["IsDefault"] = CommonFuntion.StringToJson(SalaryPlanModelList[j].IsDefault.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        //支出类型Json数据
        private List<TreeNodeModel> GetPayOutTypeNode(List<SS_ExpendType> PayOutTypeNodeModelList)
        {
            string ItemModel = typeof(SA_PlanView).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < PayOutTypeNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = PayOutTypeNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(PayOutTypeNodeModelList[j].ExpendTypeName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(PayOutTypeNodeModelList[j].GUID.ToString());
                dic["ExpendTypeKey"] = CommonFuntion.StringToJson(PayOutTypeNodeModelList[j].ExpendTypeKey.ToString());
                dic["ExpendTypeName"] = CommonFuntion.StringToJson(PayOutTypeNodeModelList[j].ExpendTypeName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(PayOutTypeNodeModelList[j].IsStop.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }

        //银行档案Json数据
        private List<TreeNodeModel> GetBankInfoNode(List<SS_Bank> BankInfoNodeModelList)
        {
            string ItemModel = typeof(SS_Bank).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < BankInfoNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = BankInfoNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(BankInfoNodeModelList[j].BankName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(BankInfoNodeModelList[j].GUID.ToString());
                dic["BankKey"] = CommonFuntion.StringToJson(BankInfoNodeModelList[j].BankKey.ToString());
                dic["BankName"] = CommonFuntion.StringToJson(BankInfoNodeModelList[j].BankName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(BankInfoNodeModelList[j].IsStop.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        //证件类型Json数据
        private List<TreeNodeModel> GetCredentialInfoNode(List<SS_CredentialType> CredentialInfoNodeModelList)
        {
            string ItemModel = typeof(SS_CredentialType).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < CredentialInfoNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = CredentialInfoNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(CredentialInfoNodeModelList[j].CredentialTypeName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(CredentialInfoNodeModelList[j].GUID.ToString());
                dic["CredentialTypekey"] = CommonFuntion.StringToJson(CredentialInfoNodeModelList[j].CredentialTypekey.ToString());
                dic["CredentialTypeName"] = CommonFuntion.StringToJson(CredentialInfoNodeModelList[j].CredentialTypeName.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        //人员类别Json数据
        private List<TreeNodeModel> GetPersonTypeNode(List<SS_PersonType> PersonTypeNodeModelList)
        {
            string ItemModel = typeof(SS_PersonType).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < PersonTypeNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = PersonTypeNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(PersonTypeNodeModelList[j].PersonTypeName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(PersonTypeNodeModelList[j].GUID.ToString());
                dic["PersonTypeKey"] = CommonFuntion.StringToJson(PersonTypeNodeModelList[j].PersonTypeKey.ToString());
                dic["PersonTypeName"] = CommonFuntion.StringToJson(PersonTypeNodeModelList[j].PersonTypeName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(PersonTypeNodeModelList[j].IsStop.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        /// <summary>
        /// 获取工资项目
        /// </summary>
        /// <returns></returns>
        public ContentResult GetSalaryItemTree()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getSalaryItem();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetSalaryItemNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取支出类型
        /// </summary>
        /// <returns></returns>
        public ContentResult GetPayOutType()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getPayOutType();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetPayOutTypeNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        /// <summary>
        /// 获取银行档案
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBankInfo()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getBankInfo();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetBankInfoNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取银行档案
        /// </summary>
        /// <returns></returns>
        public ContentResult GetCredentialInfo()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getCredentialInfo();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetCredentialInfoNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 人员类别设置
        /// </summary>
        /// <returns></returns>
        public ContentResult GetPersonType()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getPersonType();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetPersonTypeNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 根据条件获取对应的工资项
        /// </summary>
        /// <returns></returns>
        public ContentResult GetSalaryItemByContions()
        {
            var tabName = Request["tabName"];
            string mModel = typeof(SA_Item).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;

            var q = dbobj.context.SA_Plan.Where(e => e.PlanName == tabName).Select(e => e.GUID).ToList();
            var list = dbobj.context.SA_PlanItemView.Where(e => q.Contains(e.GUID_Plan)).OrderBy(e => e.ItemOrder).Select(e => new { e.GUID, e.GUID_Item, e.ItemName, e.ItemFormula }).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = list[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(list[i].ItemName);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = list[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["GUID_Item"] = CommonFuntion.StringToJson(temp.GUID_Item != null ? temp.GUID_Item.ToString() : "");
                dic["ItemName"] = CommonFuntion.StringToJson(temp.ItemName != null ? temp.ItemName.ToString() : "");
                dic["ItemFormula"] = CommonFuntion.StringToJson(temp.ItemFormula != null ? temp.ItemFormula.ToString() : "");
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取工资计划
        /// </summary>
        /// <returns></returns>
        public ContentResult GetSalaryPlanTree()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getSalaryPlan();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetSalaryPlanNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----人员Json数据tree
        /// </summary>
        /// <param name="personModelList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCPersonNode(List<SS_PersonView> personModelList)
        {
            string permodel = typeof(SS_Person).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (personModelList != null)
            {
                for (int j = 0; j < personModelList.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = personModelList[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(personModelList[j].PersonName + "(" + personModelList[j].PersonKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID_DW"] = personModelList[j].GUID_DW.ToString();
                    dic["DWName"] = CommonFuntion.StringToJson(personModelList[j].DWName);
                    dic["DWKey"] = CommonFuntion.StringToJson(personModelList[j].DWKey);
                    dic["GUID_Department"] = CommonFuntion.StringToJson(personModelList[j].GUID_Department.ToString());
                    dic["DepartmentName"] = CommonFuntion.StringToJson(personModelList[j].DepartmentName);
                    dic["DepartmentKey"] = CommonFuntion.StringToJson(personModelList[j].DepartmentKey);
                    dic["GUID"] = CommonFuntion.StringToJson(personModelList[j].GUID.ToString());
                    dic["PersonName"] = CommonFuntion.StringToJson(personModelList[j].PersonName);
                    dic["PersonKey"] = CommonFuntion.StringToJson(personModelList[j].PersonKey);
                    dic["GUID_PersonType"] = CommonFuntion.StringToJson(personModelList[j].GUID_PersonType.ToString());
                    dic["IDCardType"] = CommonFuntion.StringToJson(personModelList[j].IDCardType);
                    dic["PersonBirthday"] = CommonFuntion.StringToJson(personModelList[j].PersonBirthday == null ? "" : ((DateTime)personModelList[j].PersonBirthday).ToString("yyyy-MM-dd"));
                    dic["IDCard"] = CommonFuntion.StringToJson(personModelList[j].IDCard);
                    dic["BankCardNo"] = CommonFuntion.StringToJson(personModelList[j].BankCardNo);
                    dic["OfficialCard"] = CommonFuntion.StringToJson(personModelList[j].OfficialCard);
                    dic["IsTax"] =(personModelList[j].IsTax+"").ToLower();
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 人员Json数据
        /// </summary>
        /// <param name="personModelList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetInvitePersonNode(List<InvitePersonTreeModel> personModelList)
        {
            string permodel = typeof(SS_InvitePerson).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < personModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = personModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(personModelList[j].InvitePersonName + "(" + personModelList[j].InvitePersonIDCard + ")");
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = permodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(personModelList[j].GUID.ToString());
                dic["InvitePersonIDCard"] = CommonFuntion.StringToJson(personModelList[j].InvitePersonIDCard);
                dic["CredentialTypeName"] = CommonFuntion.StringToJson(personModelList[j].CredentialTypeName);
                dic["CredentialTypekey"] = CommonFuntion.StringToJson(personModelList[j].CredentialTypekey);
                dic["InvitePersonName"] = CommonFuntion.StringToJson(personModelList[j].InvitePersonName);
                dic["IsUnit"] = personModelList[j].IsUnit.ToString();
                treeModel.attributes = dic;

                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        /// <summary>
        /// 获取Person类型数据
        /// </summary>        
        public ContentResult GetPersonTree()
        {
            string DwModel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string strJson = string.Empty;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (Request["id"] != null && Request["id"] != "")
            {
                Guid depid = ConvertGuid(Request["id"]);
                List<PersonTreeModel> pesonlist = BaseTree.GetPerson(true, operatorId, depid);
                var list = GetDepPersonNode(pesonlist);
                if (list != null && list.Count > 0)
                {
                    treeModelList = list;
                }
            }
            else
            {
                List<SS_DW> dwList = dbobj.GetDW(true, operatorId);
                List<SS_Department> depList = dbobj.GetDepartment(true, operatorId);
                var parentDWList = dwList.FindAll(e => e.PGUID == null);
                var dwChild = GetPensonDWChildNode(parentDWList, dwList, depList);//人员 单位子项部门                    
                if (dwChild != null && dwChild.Count > 0)
                {
                    treeModelList = dwChild;
                }
            }
            strJson = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(strJson);
        }
        /// <summary>
        /// 获取所有Person类型数据
        /// </summary>        
        public ContentResult GetAllPersonTree()
        {
            string DwModel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string strJson = string.Empty;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            List<PersonTreeModel> pesonlist = BaseTree.GetPerson(true, operatorId);
            List<SS_DW> dwList = dbobj.GetDW(true, operatorId);
            List<SS_Department> depList = dbobj.GetDepartment(true, operatorId);
            var parentDWList = dwList.FindAll(e => e.PGUID == null);
            var dwChild = GetPensonDWChildNode(parentDWList, dwList, depList, pesonlist);//人员 单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            strJson = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(strJson);
        }

        /// <summary>
        /// 基础----获取Person类型数据----tree
        /// </summary>        
        public ContentResult GetJCPersonTree()
        {
            string DwModel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string strJson = string.Empty;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (Request["id"] != null && Request["id"] != "")
            {
                Guid depid = ConvertGuid(Request["id"]);
                List<SS_PersonView> pesonlist = dbobj.GetPersonView(false, operatorId, depid);
                var list = GetJCDepPersonNode(pesonlist);//部门人员
                if (list != null && list.Count > 0)
                {
                    treeModelList = list;
                }
            }
            else
            {//走的这个else，没有权限





                List<SS_DW> dwList = dbobj.GetDW(false, operatorId);
                List<SS_Department> depList = dbobj.GetDepartment(false, operatorId);
                var parentDWList = dwList.FindAll(e => e.PGUID == null);
                var dwChild = GetJCPensonDWChildNode(parentDWList, dwList, depList);//人员 单位子项部门                    
                if (dwChild != null && dwChild.Count > 0)
                {
                    treeModelList = dwChild;
                }
            }
            strJson = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(strJson);
        }
        /// <summary>
        /// 基础----部门人员----tree
        /// </summary>
        /// <param name="depID"></param>
        /// <returns></returns>
        public List<TreeNodeModel> GetJCDepPersonNode(List<SS_PersonView> pesonlist)//= "602FB515-9C10-4E82-8D79-0A865048A6E3"
        {
            return GetJCPersonNode(pesonlist);
        }
        /// <summary>
        /// 部门人员
        /// </summary>
        /// <param name="depID"></param>
        /// <returns></returns>
        public List<TreeNodeModel> GetDepPersonNode(List<PersonTreeModel> pesonlist)//= "602FB515-9C10-4E82-8D79-0A865048A6E3"
        {
            return GetPersonNode(pesonlist);
        }
        /// <summary>
        /// 基础----获取部门信息----tree
        /// </summary>
        public ContentResult GetJCDepartmentTree()
        {
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var depList = dbobj.GetJCDepartmentView(false, operatorId);//部门    修改权限  true:有  false:无





            List<SS_DW> dwList = dbobj.GetDW(false, operatorId);//单位     修改权限   true:有  false:无





            var parentList = dwList.FindAll(e => e.PGUID == null);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwChild = GetJCDWChildNode(parentList, dwList, depList);//单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取部门信息
        /// </summary>
        public ContentResult GetDepartmentTree()
        {
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var depList = dbobj.GetDepartment(true, operatorId);
            List<SS_DW> dwList = dbobj.GetDW(true, operatorId);//单位
            var parentList = dwList.FindAll(e => e.PGUID == null);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwChild = GetDWChildNode(parentList, dwList, depList);//单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        public ContentResult GetDepartmentTreeCheck()
        {
            var select = Request["select"];
            bool bSelect = true;
            if (select == "0")
            {
                bSelect = false;
            }
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var depList = dbobj.GetDepartment(true, operatorId);
            List<SS_DW> dwList = dbobj.GetDW(true, operatorId);//单位
            var parentList = dwList.FindAll(e => e.PGUID == null);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwChild = GetDWChildNode(parentList, dwList, depList);//单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            SetCheckBoxForTree(bSelect, ref treeModelList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        public List<SS_Department> GetDepartment(int year)
        {
            var sql =string.Format( @"SELECT * FROM SS_Department WHERE GUID IN (
 SELECT    a.GUID_Department 
          FROM      BG_MainView a
          WHERE     a.GUID_Project IN (
                    SELECT  GUID
                    FROM    SS_Project
                    WHERE   
                            a.guid IN ( SELECT  guid_bg_main
                                            FROM    bg_detail
                                            WHERE   bgyear = {0} )
                            AND
                             ISNULL(a.Invalid, 0) = 1
                            AND a.BGStepKey = '05' )
          GROUP BY  a.DepartmentKey,GUID_Department)",year);
           // List<SS_Department> list = new List<SS_Department>();
            var context=dbobj.context;
            return context.ExecuteStoreQuery<SS_Department>(sql).ToList();
        }
        public ContentResult GetDepartmentTreeCheck1()
        {
            var select = Request["select"];
            bool bSelect = true;
            if (select == "0")
            {
                bSelect = false;
            }
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var year = Request["year"];
            int year1 = DateTime.Now.Year;
            int.TryParse(year, out year1);
            if (year1 == 0) {
                year1 = DateTime.Now.Year;
            }
            var depList = GetDepartment(year1);
            List<SS_DW> dwList = dbobj.GetDW(false, operatorId);//单位
            var parentList = dwList.FindAll(e => e.PGUID == null);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwChild = GetDWChildNode(parentList, dwList, depList);//单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            SetCheckBoxForTree1(bSelect, ref treeModelList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取部门信息信息（应用于部门预算报表）






        /// </summary>
        public ContentResult GetReportDepartmentTree()
        {
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            var depList = dbobj.GetDepartment(true, operatorId);
            List<SS_DW> dwList = dbobj.GetDW(true, operatorId);//单位
            List<PersonTreeModel> pesonlist = BaseTree.GetPerson(true, operatorId);
            var parentList = dwList.FindAll(e => e.PGUID == null);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwChild = GetDWChildNode(parentList, dwList, depList, pesonlist);//单位子项部门                    
            if (dwChild != null && dwChild.Count > 0)
            {
                treeModelList = dwChild;
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取单位信息
        /// </summary>
        public ContentResult GetDWTree()
        {
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwList = dbobj.GetDW(true, operatorId);
            var dwParentList = dwList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < dwParentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = dwParentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(dwParentList[i].DWName + "(" + dwParentList[i].DWKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = dwmodel;
                treeModel.attributes = dic;
                var childList = dwList.FindAll(e => e.PGUID == dwParentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var childDw = GetAttributesDWChildNode(childList, dwList);
                    if (childDw != null && childDw.Count > 0)
                    {
                        treeModel.children = childDw;
                    }

                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        #region 基础--获取单位信息--tree

        /// <summary>
        /// 基础----获取单位信息----tree信息  --zzp--
        /// </summary>
        public ContentResult GetJCDWTree()
        {
            string dwmodel = typeof(SS_DW).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var dwList = dbobj.GetDWView(false, operatorId);    //是否有权限     true:有  false:无


            var dwParentList = dwList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < dwParentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = dwParentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(dwParentList[i].DWName + "(" + dwParentList[i].DWKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = dwmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(dwParentList[i].GUID.ToString());
                dic["DWKey"] = CommonFuntion.StringToJson(dwParentList[i].DWKey);
                dic["DWName"] = CommonFuntion.StringToJson(dwParentList[i].DWName);
                dic["IsStop"] = CommonFuntion.StringToJson(dwParentList[i].IsStop.ToString());
                treeModel.attributes = dic;
                var childList = dwList.FindAll(e => e.PGUID == dwParentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var childDw = GetAttributesJCDWChildNode(childList, dwList);
                    if (childDw != null && childDw.Count > 0)
                    {
                        treeModel.children = childDw;
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----获取单位的单位子项--tree信息
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetAttributesJCDWChildNode(List<SS_DWView> currentList, List<SS_DWView> orgList)
        {
            string dwmodel = typeof(SS_DW).Name;
            List<TreeNodeModel> treeNodeList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].DWName + "(" + currentList[i].DWKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = dwmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> dwChild = new List<TreeNodeModel>();
                        var list = GetAttributesJCDWChildNode(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            dwChild = list;
                        }
                        treeModel.children = dwChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = dwmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                        dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    }
                    treeNodeList.Add(treeModel);
                }
            }
            return treeNodeList;
        }

        #endregion

        #region 基础--获取帐套主表信息--tree

        /// <summary>
        /// -基础-- 获取帐套主表信息-- tree信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCZTTree()
        {
            string accmodel = typeof(AccountMain).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var acList = dbobj.GetAccountMainView(false, operatorId);    //是否有权限     true:有  false:无
            var acParentList = acList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < acParentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = acParentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(acParentList[i].Description + "(" + acParentList[i].AccountKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = accmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(acParentList[i].GUID.ToString());
                dic["Description"] = CommonFuntion.StringToJson(acParentList[i].Description);
                dic["AccountKey"] = CommonFuntion.StringToJson(acParentList[i].AccountKey);
                dic["AccountName"] = CommonFuntion.StringToJson(acParentList[i].AccountName);
                dic["GUID_DW"] = CommonFuntion.StringToJson(acParentList[i].GUID_DW.ToString());
                dic["DWKey"] = CommonFuntion.StringToJson(acParentList[i].DWKey);
                dic["DWName"] = CommonFuntion.StringToJson(acParentList[i].DWName);
                dic["PGUID"] = CommonFuntion.StringToJson(acParentList[i].PGUID.ToString());
                dic["PKey"] = CommonFuntion.StringToJson(acParentList[i].PKey);
                dic["PName"] = CommonFuntion.StringToJson(acParentList[i].PName);
                dic["FirstYear"] = CommonFuntion.StringToJson(acParentList[i].FirstYear.ToString());
                dic["LastYear"] = CommonFuntion.StringToJson(acParentList[i].LastYear.ToString());
                treeModel.attributes = dic;
                var childList = acList.FindAll(e => e.PGUID == acParentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var childDw = GetAttributesJCAccountChildNode(childList, acList);
                    if (childDw != null && childDw.Count > 0)
                    {
                        treeModel.children = childDw;
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----获取单位的单位子项--tree信息
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetAttributesJCAccountChildNode(List<AccountMainView> currentList, List<AccountMainView> orgList)
        {
            string acmodel = typeof(AccountMain).Name;
            List<TreeNodeModel> treeNodeList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].Description + "(" + currentList[i].AccountKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = acmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["AccountKey"] = CommonFuntion.StringToJson(currentList[i].AccountKey);
                    dic["AccountName"] = CommonFuntion.StringToJson(currentList[i].AccountName);
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["GUID_DW"] = CommonFuntion.StringToJson(currentList[i].GUID_DW.ToString());
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    dic["FirstYear"] = CommonFuntion.StringToJson(currentList[i].FirstYear.ToString());
                    dic["LastYear"] = CommonFuntion.StringToJson(currentList[i].LastYear.ToString());
                    dic["Description"] = CommonFuntion.StringToJson(currentList[i].Description);
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> dwChild = new List<TreeNodeModel>();
                        var list = GetAttributesJCAccountChildNode(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            dwChild = list;
                        }
                        treeModel.children = dwChild;
                    }
                    //else
                    //{
                        treeModel.state = "open";
                    //    dic["m"] = acmodel;
                    //    dic["valid"] = "1";
                    //    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    //    dic["AccountKey"] = CommonFuntion.StringToJson(currentList[i].AccountKey);
                    //    dic["AccountName"] = CommonFuntion.StringToJson(currentList[i].AccountName);
                    //    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    //    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    //    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    //    dic["GUID_DW"] = CommonFuntion.StringToJson(currentList[i].GUID_DW.ToString());
                    //    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    //    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    //    dic["FirstYear"] = CommonFuntion.StringToJson(currentList[i].FirstYear.ToString());
                    //    dic["LastYear"] = CommonFuntion.StringToJson(currentList[i].LastYear.ToString());
                    //    dic["Description"] = CommonFuntion.StringToJson(currentList[i].Description);
                    //}
                    treeNodeList.Add(treeModel);
                }
            }
            return treeNodeList;
        }

        #endregion

        #region 基础--获取帐套主表信息--tree

        /// <summary>
        /// -基础-- 获取帐套子表信息-- tree信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCZTZBTree()
        {
            string accmodel = typeof(AccountMain).Name;
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var acList = dbobj.GetAccountMainView(false, operatorId);    //是否有权限     true:有  false:无
            var acDetaList = dbobj.GetAccountDetailView(false, operatorId);
            var acParentList = acList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < acParentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = acParentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(acParentList[i].Description + "(" + acParentList[i].AccountKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = accmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(acParentList[i].GUID.ToString());
                dic["AccountKey"] = CommonFuntion.StringToJson(acParentList[i].AccountKey);
                dic["AccountName"] = CommonFuntion.StringToJson(acParentList[i].AccountName);
                treeModel.attributes = dic;

                var childList = acList.FindAll(e => e.PGUID == acParentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var childDw = GetJCAccountMainChildNode(childList, acList,acDetaList);
                    if (childDw != null && childDw.Count > 0)
                    {
                        treeModel.children = childDw;
                    }
                } 
                else
                {
                    var childDetailList = acDetaList.FindAll(e => e.GUID_AccountMain == acParentList[i].GUID);
                    if (childDetailList != null && childDetailList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var childDw = GetAttributesJCAccountDetailChildNode(childDetailList, acDetaList);
                        if (childDw != null && childDw.Count > 0)
                        {
                            treeModel.children = childDw;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----获取帐套的子项的子项--tree信息
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetJCAccountMainChildNode(List<AccountMainView> currentList, List<AccountMainView> orgList,List<AccountDetailView> orgDetailList)
        {
            string acmodel = typeof(AccountMain).Name;
            List<TreeNodeModel> treeNodeList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].Description + "(" + currentList[i].AccountKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = acmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["AccountKey"] = CommonFuntion.StringToJson(currentList[i].AccountKey);
                    dic["AccountName"] = CommonFuntion.StringToJson(currentList[i].AccountName);
                    treeModel.attributes = dic;

                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> dwChild = new List<TreeNodeModel>();
                        var list = GetJCAccountMainChildNode(childList, orgList,orgDetailList);
                        if (list != null && list.Count > 0)
                        {
                            dwChild = list;
                        }
                        treeModel.children = dwChild;
                    }
                    else
                    {
                        var childDetailList = orgDetailList.FindAll(e => e.GUID_AccountMain == currentList[i].GUID);
                        if (childDetailList != null && childDetailList.Count > 0)
                        {
                            treeModel.state = "closed";
                            var childDw = GetAttributesJCAccountDetailChildNode(childDetailList, orgDetailList);
                            if (childDw != null && childDw.Count > 0)
                            {
                                treeModel.children = childDw;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                                           
                    }
                    treeNodeList.Add(treeModel);
                }
            }
            return treeNodeList;
        }

        /// <summary>
        /// 基础----获取帐套的子项--tree信息
        /// </summary>
        /// <param name="currentList">当前项集合</param>
        /// <param name="orgList">原始数据集合</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>String</returns>
        private List<TreeNodeModel> GetAttributesJCAccountDetailChildNode(List<AccountDetailView> currentList, List<AccountDetailView> orgList)
        {
            string acmodel = typeof(AccountDetail).Name;
            List<TreeNodeModel> treeNodeList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].ExteriorYear + "(" + currentList[i].FiscalYear + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = acmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["GUID_AccountMain"] = CommonFuntion.StringToJson(currentList[i].GUID_AccountMain.ToString());
                    dic["AccountKey"] = CommonFuntion.StringToJson(currentList[i].accountkey);
                    dic["AccountName"] = CommonFuntion.StringToJson(currentList[i].accountname);
                    dic["FiscalYear"] = CommonFuntion.StringToJson(currentList[i].FiscalYear.ToString());
                    dic["ExteriorYear"] = CommonFuntion.StringToJson(currentList[i].ExteriorYear.ToString());
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.GUID_AccountMain == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> dwChild = new List<TreeNodeModel>();
                        var list = GetAttributesJCAccountDetailChildNode(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            dwChild = list;
                        }
                        treeModel.children = dwChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = acmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["GUID_AccountMain"] = CommonFuntion.StringToJson(currentList[i].GUID_AccountMain.ToString());
                        dic["AccountKey"] = CommonFuntion.StringToJson(currentList[i].accountkey);
                        dic["AccountName"] = CommonFuntion.StringToJson(currentList[i].accountname);
                        dic["FiscalYear"] = CommonFuntion.StringToJson(currentList[i].FiscalYear.ToString());
                        dic["ExteriorYear"] = CommonFuntion.StringToJson(currentList[i].ExteriorYear.ToString());
                    }
                    treeNodeList.Add(treeModel);
                }
            }
            return treeNodeList;
        }

        #endregion

        /// <summary>
        /// 获取科目信息
        /// </summary>
        public ContentResult GetBgCodeTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string bgmodel = typeof(SS_BGCode).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            //var bgcodeList = dbobj.GetBgcode(true,operatorId);
            var bgcodeList = BaseTree.GetBgcode(true, operatorId);
            var parentList = bgcodeList.FindAll(e => e.PGUID == null);//tes 测试用
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].BGCodeName + "(" + parentList[i].BGCodeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = bgmodel;
                treeModel.attributes = dic;
                var childList = bgcodeList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var bgChild = GetChildBGCodeNode(childList, bgcodeList);
                    if (bgChild != null && bgChild.Count > 0)
                    {
                        treeModel.children = bgChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取文件类型数据
        /// </summary>
        public ContentResult GetFileTypeData()
        {
            string model = typeof(SS_OfficeFileType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var List = CommonFun.GetFileType();
            var parentList = List.FindAll(e => e.PGUID == null);
            Guid guid = Guid.NewGuid();
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].FileTypeName + "(" + parentList[i].FileTypeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = parentList[i];
                temp = temp != null ? temp : null;
                dic["m"] = model;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["PGUID"] = CommonFuntion.StringToJson(guid.ToString());
                dic["FileTypeName"] = CommonFuntion.StringToJson(temp.FileTypeName != null ? temp.FileTypeName.ToString() : "");
                dic["FileTypeKey"] = CommonFuntion.StringToJson(temp.FileTypeKey != null ? temp.FileTypeKey.ToString() : "");
                treeModel.attributes = dic;
                var childList = List.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var Child = GetChildFileTypeNode(childList, List);
                    if (Child != null && Child.Count > 0)
                    {
                        treeModel.children = Child;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            List<TreeNodeModel> pModelList = new List<TreeNodeModel>();
            TreeNodeModel pModel = new TreeNodeModel();
            pModel.id = Guid.Empty.ToString();
            pModel.text = "全部文件";
            Dictionary<string, string> dic2 = new Dictionary<string, string>();
            dic2["m"] = model;
            dic2["valid"] = "1";
            dic2["GUID"] = CommonFuntion.StringToJson(guid.ToString());
            dic2["PGUID"] = CommonFuntion.StringToJson("");
            dic2["FileTypeName"] = CommonFuntion.StringToJson("全部文件");
            dic2["FileTypeKey"] = CommonFuntion.StringToJson("00");
            pModel.attributes = dic2;
            pModelList.Add(pModel);
            pModelList[0].children = treeModelList;
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(pModelList);
            return Content(jsonStr);
        }


        #region 基础  科目设置  科目摘要设置

        public ContentResult GetJCBgCodeMemoTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string bgmodel = typeof(SS_BGCode).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var bgcodeMemoList = dbobj.GetBgcodeMemoView(false, operatorId);

            var bgcodeList = dbobj.GetBgcodeView(false, operatorId);
            var parentList = bgcodeList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].BGCodeName + "(" + parentList[i].BGCodeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = bgmodel;
                dic["GUID"] = CommonFuntion.StringToJson(parentList[i].GUID.ToString());
                dic["PGUID"] = CommonFuntion.StringToJson(parentList[i].PGUID.ToString());
                treeModel.attributes = dic;
                var childList = bgcodeList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var bgChild = GetJCChildBGCodeMemoNode(childList, bgcodeList, bgcodeMemoList);
                    if (bgChild != null && bgChild.Count > 0)
                    {
                        treeModel.children = bgChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        private List<TreeNodeModel> GetJCChildBGCodeMemoNode(List<SS_BGCodeView> currentList, List<SS_BGCodeView> orgList, List<SS_BGCodeMemoView> memoList)
        {
            //string bgmodel = typeof(SS_BGCode).Name;
            string bgmodel = typeof(SS_BGCodeMemo).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].BGCodeName + "(" + currentList[i].BGCodeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = bgmodel;
                    treeModel.attributes = dic;

                    List<SS_BGCodeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetJCChildBGCodeMemoNode(childList, orgList, memoList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = bgmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["BGCodeName"] = CommonFuntion.StringToJson(currentList[i].BGCodeName);
                        dic["BGCodeKey"] = CommonFuntion.StringToJson(currentList[i].BGCodeKey);

                        var memoModel = memoList.FirstOrDefault(e => e.GUID_BGCode == currentList[i].GUID);
                        //摘要信息
                        if (memoModel != null)
                        {
                            dic["GUID_BGCode"] = CommonFuntion.StringToJson(memoModel.GUID_BGCode.ToString());
                            dic["BGCodeMemo"] = CommonFuntion.StringToJson(memoModel.BGCodeMemo);
                            dic["MemoType"] = memoModel.MemoType == null ? "0" : memoModel.MemoType.ToString();
                            dic["IsDefault"] = (memoModel.IsDefault == null || memoModel.IsDefault == false) ? "否" : "是";
                        }
                        treeModel.attributes = dic;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }


        #endregion


        #region 基础  科目设置 预算科目总表

        /// <summary>
        /// 获取科目信息
        /// </summary>
        public ContentResult GetJCBgCodeTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string bgmodel = typeof(SS_BGCode).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var bgcodeList = dbobj.GetBgcodeView(false, operatorId);
            var parentList = bgcodeList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].BGCodeName + "(" + parentList[i].BGCodeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = bgmodel;
                treeModel.attributes = dic;
                var childList = bgcodeList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var bgChild = GetJCChildBGCodeNode(childList, bgcodeList);
                    if (bgChild != null && bgChild.Count > 0)
                    {
                        treeModel.children = bgChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                    dic["m"] = bgmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(parentList[i].GUID.ToString());
                    dic["BGCodeName"] = CommonFuntion.StringToJson(parentList[i].BGCodeName);
                    dic["BGCodeKey"] = CommonFuntion.StringToJson(parentList[i].BGCodeKey);
                    dic["PGUID"] = CommonFuntion.StringToJson(parentList[i].PGUID.ToString());
                    dic["PKey"] = CommonFuntion.StringToJson(parentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(parentList[i].PName);
                    dic["GUID_EconomyClass"] = parentList[i].GUID_EconomyClass.ToString();
                    dic["EconomyClassName"] = CommonFuntion.StringToJson(parentList[i].EconomyClassName);
                    dic["EconomyClassKey"] = CommonFuntion.StringToJson(parentList[i].EconomyClassKey);
                    dic["BeginYear"] = parentList[i].BeginYear == null ? "" : parentList[i].BeginYear.ToString();
                    dic["StopYear"] = parentList[i].StopYear == null ? "" : parentList[i].StopYear.ToString();
                    dic["IsStop"] = CommonFuntion.StringToJson(parentList[i].IsStop.ToString());
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取BGCode科目的子信息
        /// </summary>
        /// <param name="currentList"></param>
        /// <param name="orgList"></param>
        /// <returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetJCChildBGCodeNode(List<SS_BGCodeView> currentList, List<SS_BGCodeView> orgList)
        {
            string bgmodel = typeof(SS_BGCode).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].BGCodeName + "(" + currentList[i].BGCodeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = bgmodel;
                    treeModel.attributes = dic;

                    List<SS_BGCodeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetJCChildBGCodeNode(childList, orgList);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = bgmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["BGCodeName"] = CommonFuntion.StringToJson(currentList[i].BGCodeName);
                        dic["BGCodeKey"] = CommonFuntion.StringToJson(currentList[i].BGCodeKey);
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                        dic["GUID_EconomyClass"] = currentList[i].GUID_EconomyClass.ToString();
                        dic["EconomyClassName"] = CommonFuntion.StringToJson(currentList[i].EconomyClassName);
                        dic["BeginYear"] = currentList[i].BeginYear == null ? "" : currentList[i].BeginYear.ToString();
                        dic["StopYear"] = currentList[i].StopYear == null ? "" : currentList[i].StopYear.ToString();
                        dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion

        public ContentResult GetProjectTreeFX()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;
            var year = Request["year"];
            var proList = new List<ProjectTreeModel>();
            List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();//项目分类
            if (!string.IsNullOrEmpty(year))
            {
                int outYear;
                if (int.TryParse(year, out outYear))
                {
                    IntrastructureFun dbobj = new IntrastructureFun();
                    proList = dbobj.context.ExecuteStoreQuery<ProjectTreeModel>(@"SELECT  GUID ,
        PGUID ,
        ProjectName ,
        ProjectKey ,
        GUID_FunctionClass ,
        GUID_ProjectClass ,
        IsFinance
FROM    SS_ProjcetExView
WHERE   PGUID IS NULL
        AND IsFinance = 1
        AND IsStop = 0
        AND StopYear IS NULL
ORDER BY ProjectKey").ToList();
                    //报表
                    proClassList = dbobj.GetProjectClass(true, operatorId, outYear);//操作员GUID要改 
                }
            }
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClass> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {
                if (parentClassList[i].GUID ==new Guid( "4997896E-24F0-4897-9253-1BD74969EA8D")) continue;
                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    treeModel.state = "open";



                    var projectClassChild = GetChildProjectClassNode(childList, proClassList, proList,"open");//项目分类子项
                    var childProjectList = proList.FindAll(e => e.GUID_ProjectClass == parentClassList[i].GUID && e.PGUID == null);
                    foreach (var item in childProjectList)
                    {
                        TreeNodeModel treeModel1 = new TreeNodeModel();
                        treeModel1.id = item.GUID.ToString();
                        treeModel1.text = CommonFuntion.StringToJson(item.ProjectName + "(" + item.ProjectKey + ")");
                        Dictionary<string, string> dic1 = new Dictionary<string, string>();
                        dic1["m"] = promodel;
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey);
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);

                        treeModel.state = "open";
                        dic1["valid"] = "1";
                        dic1["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey.ToString());
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);
                        dic1["GUID_FunctionClass"] = CommonFuntion.StringToJson(item.GUID_FunctionClass.ToString());
                        dic1["ExtraCode"] = item.ExtraCode == null ? "" : item.ExtraCode;
                        dic1["FinanceCode"] = item.FinanceCode == null ? "" : item.FinanceCode;
                        dic1["IsFinance"] = item.IsFinance == null ? "false" : item.IsFinance.ToString();
                        dic1["IsLeaf"] = "1";
                        treeModel1.attributes = dic1;
                        projectClassChild.Add(treeModel1);
                    }
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetChildProjectClassNode(new List<SS_ProjectClass> { parentClassList[i] }, proClassList, proList,"open");//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }


            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }


        public ContentResult GetProjectTreeFX1()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;
            var year = Request["year"];
            var proList = new List<ProjectTreeModel>();
            List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();//项目分类
            if (!string.IsNullOrEmpty(year))
            {
                int outYear;
                if (int.TryParse(year, out outYear))
                {
                    IntrastructureFun dbobj = new IntrastructureFun();
                    proList = dbobj.context.ExecuteStoreQuery<ProjectTreeModel>(@"SELECT  GUID ,
        PGUID ,
        ProjectName ,
        ProjectKey ,
        GUID_FunctionClass ,
        GUID_ProjectClass ,
        IsFinance
FROM    SS_ProjcetExView
WHERE   PGUID IS NULL
        AND IsFinance = 1
        AND IsStop = 0
        AND StopYear IS NULL
ORDER BY ProjectKey").ToList();
                    //报表
                    proClassList = dbobj.GetProjectClass(true, operatorId, outYear);//操作员GUID要改 
                }
            }
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClass> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {
                if (parentClassList[i].GUID == new Guid("4997896E-24F0-4897-9253-1BD74969EA8D")) continue;
                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    treeModel.state = "open";
                    treeModel.@checked = true;



                    var projectClassChild = GetChildProjectClassNode(childList, proClassList, proList, "open");//项目分类子项
                    var childProjectList = proList.FindAll(e => e.GUID_ProjectClass == parentClassList[i].GUID && e.PGUID == null);
                    foreach (var item in childProjectList)
                    {
                        TreeNodeModel treeModel1 = new TreeNodeModel();
                        treeModel1.id = item.GUID.ToString();
                        treeModel1.@checked = true;
                        treeModel1.text = CommonFuntion.StringToJson(item.ProjectName + "(" + item.ProjectKey + ")");
                        Dictionary<string, string> dic1 = new Dictionary<string, string>();
                        dic1["m"] = promodel;
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey);
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);

                        treeModel.state = "open";
                        treeModel.@checked = true;
                        dic1["valid"] = "1";
                        dic1["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey.ToString());
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);
                        dic1["GUID_FunctionClass"] = CommonFuntion.StringToJson(item.GUID_FunctionClass.ToString());
                        dic1["ExtraCode"] = item.ExtraCode == null ? "" : item.ExtraCode;
                        dic1["FinanceCode"] = item.FinanceCode == null ? "" : item.FinanceCode;
                        dic1["IsFinance"] = item.IsFinance == null ? "false" : item.IsFinance.ToString();
                        dic1["IsLeaf"] = "1";
                        treeModel1.attributes = dic1;
                        projectClassChild.Add(treeModel1);
                    }
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetChildProjectClassNode(new List<SS_ProjectClass> { parentClassList[i] }, proClassList, proList, "open");//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }


            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取项目信息
        /// </summary>
        public ContentResult GetProjectTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;
            var year = Request["year"];
            var proList = new List<ProjectTreeModel>();
            List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();//项目分类
            if (!string.IsNullOrEmpty(year))
            {
                int outYear;
                if (int.TryParse(year, out outYear))
                {
                    proList = BaseTree.GetProject(true, operatorId, outYear);
                    //报表
                    proClassList = dbobj.GetProjectClass(true, operatorId, outYear);//操作员GUID要改 
                }
            }
            else
            {
                proList = BaseTree.GetProject(false, operatorId);
                proClassList = dbobj.GetProjectClass(false, operatorId);//操作员GUID要改 
            }
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClass> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    treeModel.state = "closed";

                  

                    var projectClassChild = GetChildProjectClassNode(childList, proClassList, proList);//项目分类子项
                    var childProjectList = proList.FindAll(e => e.GUID_ProjectClass == parentClassList[i].GUID && e.PGUID == null);
                    foreach (var item in childProjectList)
                    {
                        TreeNodeModel treeModel1 = new TreeNodeModel();
                        treeModel1.id = item.GUID.ToString();
                        treeModel1.text = CommonFuntion.StringToJson(item.ProjectName + "(" + item.ProjectKey + ")");
                        Dictionary<string, string> dic1 = new Dictionary<string, string>();
                        dic1["m"] = promodel;
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey);
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);
                       
                        treeModel.state = "open";
                        dic1["valid"] = "1";
                        dic1["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                        dic1["ProjectKey"] = CommonFuntion.StringToJson(item.ProjectKey.ToString());
                        dic1["ProjectName"] = CommonFuntion.StringToJson(item.ProjectName);
                        dic1["GUID_FunctionClass"] = CommonFuntion.StringToJson(item.GUID_FunctionClass.ToString());
                        dic1["ExtraCode"] = item.ExtraCode == null ? "" : item.ExtraCode;
                        dic1["FinanceCode"] = item.FinanceCode == null ? "" : item.FinanceCode;
                        dic1["IsFinance"] = item.IsFinance == null ? "false" : item.IsFinance.ToString();
                        dic1["IsLeaf"] = "1";
                        treeModel1.attributes = dic1;
                        projectClassChild.Add(treeModel1);
                    }
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetChildProjectClassNode(new List<SS_ProjectClass> { parentClassList[i] }, proClassList, proList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }


            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        public ContentResult GetProjectTreeCheck()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;
            var year = Request["year"];
            var select = Request["select"];
            bool bSelect = true;
            if (select == "0")
            {
                bSelect = false;
            }
            var proList = new List<ProjectTreeModel>();
            List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();//项目分类
            if (!string.IsNullOrEmpty(year))
            {
                int outYear;
                if (int.TryParse(year, out outYear))
                {
                    proList = BaseTree.GetProject(true, operatorId, outYear);
                    proClassList = dbobj.GetProjectClass(true, operatorId, outYear);//操作员GUID要改 
                }
            }
            else
            {
                proList = BaseTree.GetProject(true, operatorId);
                proClassList = dbobj.GetProjectClass(true, operatorId);//操作员GUID要改 
            }
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClass> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var projectClassChild = GetChildProjectClassNode(childList, proClassList, proList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetChildProjectClassNode(new List<SS_ProjectClass> { parentClassList[i] }, proClassList, proList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }

            }

            SetCheckBoxForTree(bSelect, ref treeModelList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        private void SetCheckBoxForTree1(bool bCheck, ref List<TreeNodeModel> treeModelList)
        {
            var kes = Request["keys"];
            if (null == treeModelList)
            {
                return;
            }
            kes =string.IsNullOrEmpty(kes)?kes: "(" + kes + ")";
            foreach (TreeNodeModel item in treeModelList)
            {
                if (string.IsNullOrEmpty(kes))
                {
                    item.@checked = bCheck;
                    item.isCheck = bCheck;
                }
                else {
                    if (item.text.Contains( kes))
                    {
                        //<span class="tree-title">业务处（应急服务办公室、国家发地图集办公室）(04)</span>
                        item.@checked = true;
                        item.isCheck = true;
                    }
                    else {
                        item.@checked = false;
                        item.isCheck = false;
                    }
                }
                item.state = "open";
                List<TreeNodeModel> list = item.children;
                SetCheckBoxForTree1(bCheck, ref list);
            }
        }
     
        private void SetCheckBoxForTree(bool bCheck, ref List<TreeNodeModel> treeModelList)
        {
            if (null == treeModelList)
            {
                return;
            }
            foreach (TreeNodeModel item in treeModelList)
            {
                item.@checked = bCheck;
                item.isCheck = bCheck;
                item.state = "open";
                List<TreeNodeModel> list = item.children;
                SetCheckBoxForTree(bCheck, ref list);
            }
        }
        #region 基础  项目设置  项目档案

        /// <summary>
        /// 基----获取项目信息tree
        /// </summary>
        public ContentResult GetJCXMDAProjectTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;

            var proList = new List<SS_ProjectView>();
            List<SS_ProjectClass> proClassList = new List<SS_ProjectClass>();//项目分类
            var type = Request["type"];
            if (type == null)
            {
                type = "0";
            }
            switch (type)
            { 
                case"1"://停用
                    proList = dbobj.GetProjectView(false, operatorId,true);
                    proClassList = dbobj.GetProjectClass(false, operatorId,true);
                    break;
                case"2"://全部
                     proList = dbobj.GetProjectView(false, operatorId);
                     proClassList = dbobj.GetProjectClass(false, operatorId);//操作员GUID要改 
                    break;
                default://未停用
                     proList = dbobj.GetProjectView(false, operatorId,false);
                    proClassList = dbobj.GetProjectClass(false, operatorId,false);
                    break;
            }
           

            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClass> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {

                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["ProjectKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey.ToString());
                    dic["ProjectName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var projectClassChild = GetDAChildProjectClassNode(childList, proClassList, proList);//项目分类子项
                    //看有没有项目 
                    var childProjectList = proList.FindAll(e => e.GUID_ProjectClass == parentClassList[i].GUID && e.PGUID == null);//项目分类的子项目
                    if (childProjectList != null && childProjectList.Count > 0)
                    {
                        var childProList = GetDAChildProject(childProjectList, proList);//项目子项
                        if (childProList != null && childProList.Count > 0)
                        {
                            projectClassChild.AddRange(childProList);
                        }
                    }
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetDAChildProjectClassNode(new List<SS_ProjectClass> { parentClassList[i] }, proClassList, proList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }

            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        /// <summary>
        /// 基础----项目分类中的子项tree
        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetDAChildProjectClassNode(List<SS_ProjectClass> currentList, List<SS_ProjectClass> orgList, List<SS_ProjectView> orgProjectList)
        {
            
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].ProjectClassName + "(" + currentList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = "SS_ProjectClass";
                    dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey.ToString());
                    dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childClassOrProjectList = new List<TreeNodeModel>();
                        var childClassList = GetDAChildProjectClassNode(childList, orgList, orgProjectList);
                        //项目分类
                        if (childClassList != null && childClassList.Count > 0)
                        {
                            childClassOrProjectList.AddRange(childClassList);
                        }
                        //不是末级的项目分类下还可能有项目
                        var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目分类的子项目
                        if (childProjectList != null && childProjectList.Count > 0)
                        {
                            var childProList = GetDAChildProject(childProjectList, orgProjectList);//项目子项
                            if (childProList != null && childProList.Count > 0)
                            {
                                childClassOrProjectList.AddRange(childProList);
                            }
                        }
                        if (childClassOrProjectList != null && childClassOrProjectList.Count > 0)
                        {
                            treeModel.children = childClassOrProjectList;
                        }
                    }
                    else
                    {
                        var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目子项
                        if (childProjectList != null && childProjectList.Count > 0)
                        {
                            treeModel.state = "closed";
                            var childProList = GetDAChildProject(childProjectList, orgProjectList);//项目子项
                            if (childProList != null && childProList.Count > 0)
                            {
                                treeModel.children = childProList;
                            }
                        }
                        else
                        {
                            treeModel.state = "open";
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        /// <summary>
        /// 基础----项目的子项tree
        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetDAChildProject(List<SS_ProjectView> currentList, List<SS_ProjectView> orgList)
        {
            string prjModel = typeof(SS_Project).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = string.Format("({0}){1}", CommonFuntion.StringToJson(currentList[i].ProjectKey), CommonFuntion.StringToJson(currentList[i].ProjectName));
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    //dic["m"] = prjModel;
                    //dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectKey.ToString());
                    //dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectName);
                    dic["m"] = prjModel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectKey);
                    dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectName);
                    dic["GUID_ProjectClass"] = CommonFuntion.StringToJson(currentList[i].GUID_ProjectClass.ToString());
                    dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey);
                    dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                    dic["GUID_DW"] = CommonFuntion.StringToJson(currentList[i].GUID_DW.ToString());
                    dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                    dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["GUID_FunctionClass"] = CommonFuntion.StringToJson(currentList[i].GUID_FunctionClass.ToString());
                    dic["FuntionClassKey"] = CommonFuntion.StringToJson(currentList[i].FunctionClassKey);
                    dic["FunctionClassName"] = CommonFuntion.StringToJson(currentList[i].FunctionClassName);
                    dic["FinanceProjectKey"] = CommonFuntion.StringToJson(currentList[i].FinanceProjectKey);
                    dic["IsBalanced"] = CommonFuntion.StringToJson(currentList[i].IsBalanced.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    dic["ExtraCode"] = CommonFuntion.StringToJson(currentList[i].ExtraCode);
                    dic["IsFinance"] = currentList[i].IsFinance == null ? "false" : currentList[i].IsFinance.ToString();
                    dic["BeginYear"] = currentList[i].BeginYear == null ? "" : currentList[i].BeginYear.ToString();
                    dic["StopYear"] = currentList[i].StopYear == null ? "" : (currentList[i].StopYear.ToString());
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var list = GetDAChildProject(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            treeModel.children = list;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        //dic["m"] = prjModel;
                        //dic["valid"] = "1";
                        //dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        //dic["ProjectKey"] = CommonFuntion.StringToJson(currentList[i].ProjectKey);
                        //dic["ProjectName"] = CommonFuntion.StringToJson(currentList[i].ProjectName);
                        //dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey);
                        //dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                        //dic["GUID_DW"] = CommonFuntion.StringToJson(currentList[i].GUID_DW.ToString());
                        //dic["DWKey"] = CommonFuntion.StringToJson(currentList[i].DWKey);
                        //dic["DWName"] = CommonFuntion.StringToJson(currentList[i].DWName);
                        //dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        //dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        //dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                        //dic["GUID_FunctionClass"] = CommonFuntion.StringToJson(currentList[i].GUID_FunctionClass.ToString());
                        //dic["FuntionClassKey"] = CommonFuntion.StringToJson(currentList[i].FunctionClassKey);
                        //dic["FunctionClassName"] = CommonFuntion.StringToJson(currentList[i].FunctionClassName);
                        //dic["FinanceProjectKey"] = CommonFuntion.StringToJson(currentList[i].FinanceProjectKey);
                        //dic["IsBalanced"] = CommonFuntion.StringToJson(currentList[i].IsBalanced.ToString());
                        //dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                        //dic["ExtraCode"] = CommonFuntion.StringToJson(currentList[i].ExtraCode);
                        //dic["IsFinance"] = currentList[i].IsFinance == null ? "false" : currentList[i].IsFinance.ToString();
                        //dic["BeginYear"] = currentList[i].BeginYear == null ? "" : currentList[i].BeginYear.ToString();
                        //dic["StopYear"] = currentList[i].StopYear == null ? "" : (currentList[i].StopYear.ToString());
                        //treeModel.attributes = dic;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion



        #region 基础  项目设置  项目分类

        /// <summary>
        /// 基础----获取项目信息tree   --zzp--2014-05-14-15:12
        /// </summary>
        public ContentResult GetJCProjectTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string promodel = typeof(SS_Project).Name;
            string proClass = typeof(SS_ProjectClass).Name;
           // var year = Request["year"];
            var type = Request["type"];//0 表示未停用 1表示停用 2 表示全部
            if (type == null)//默认为未停用
            {
                type = "0";//未停用
            }
           // var proList = new List<ProjectTreeModel>();
            List<SS_ProjectClassView> proClassList = new List<SS_ProjectClassView>();//项目分类
            //if (!string.IsNullOrEmpty(year))
            //{
            //    int outYear;
            //    if (int.TryParse(year, out outYear))
            //    {
            //       // proList = BaseTree.GetProject(false, operatorId, outYear);
            //        proClassList = dbobj.GetJCProjectClass(false, operatorId, outYear);//操作员GUID要改 
            //    }
            //}
            //else
            //{
               // proList = BaseTree.GetProject(false, operatorId);
                switch (type)
                {                    
                    case "1"://true
                        proClassList = dbobj.GetJCProjectClass(false, operatorId, true);
                        break;
                    case "2": //全部
                        proClassList = dbobj.GetJCProjectClass(false, operatorId);
                        break;
                    default:  //false                       
                        proClassList = dbobj.GetJCProjectClass(false, operatorId, false);
                        break;
                }
              
            //}
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            List<SS_ProjectClassView> parentClassList = proClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentClassList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                var childList = proClassList.FindAll(e => e.PGUID == parentClassList[i].GUID);//项目分类               
                if (childList != null && childList.Count > 0)
                {
                    treeModel.id = parentClassList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName + "(" + parentClassList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = proClass;
                    dic["GUID"] = CommonFuntion.StringToJson(parentClassList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(parentClassList[i].PGUID.ToString());
                    dic["ProjectClassKey"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassKey);
                    dic["ProjectClassName"] = CommonFuntion.StringToJson(parentClassList[i].ProjectClassName);
                    dic["PKey"] = CommonFuntion.StringToJson(parentClassList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(parentClassList[i].PName);
                    dic["BeginYear"] = CommonFuntion.StringToJson(parentClassList[i].BeginYear.ToString());
                    dic["StopYear"] = CommonFuntion.StringToJson(parentClassList[i].StopYear.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(parentClassList[i].IsStop.ToString());
                    dic["valid"] = "1";
                    treeModel.attributes = dic;
                    treeModel.state = "closed";
                    var projectClassChild = GetJCChildProjectClassNode(childList, proClassList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModel.children = projectClassChild;
                    }
                    treeModelList.Add(treeModel);
                }
                else
                {
                    //childList.Add(parentClassList[i]);//如果项目分类没有子项，把本身条件进去                   
                    var projectClassChild = GetJCChildProjectClassNode(new List<SS_ProjectClassView> { parentClassList[i] }, proClassList);//项目分类子项
                    if (projectClassChild != null && projectClassChild.Count > 0)
                    {
                        treeModelList.AddRange(projectClassChild);
                    }
                }

            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----项目分类中的子项tree
        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetJCChildProjectClassNode(List<SS_ProjectClassView> currentList, List<SS_ProjectClassView> orgList)
        {
            string pcmodel = typeof(SS_ProjectClass).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].ProjectClassName + "(" + currentList[i].ProjectClassKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = pcmodel;
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey);
                    dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["BeginYear"] = CommonFuntion.StringToJson(currentList[i].BeginYear.ToString());
                    dic["StopYear"] = CommonFuntion.StringToJson(currentList[i].StopYear.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(currentList[i].IsStop.ToString());
                    dic["valid"] = "1";
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childClassOrProjectList = new List<TreeNodeModel>();
                        var childClassList = GetJCChildProjectClassNode(childList, orgList);
                        //项目分类
                        if (childClassList != null && childClassList.Count > 0)
                        {
                            childClassOrProjectList.AddRange(childClassList);
                        }
                        ////不是末级的项目分类下还可能有项目
                        //var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目分类的子项目
                        //if (childProjectList != null && childProjectList.Count > 0)
                        //{
                        //    //var childProList = GetChildProject(childProjectList, orgProjectList);//项目子项
                        //    var childProList = GetJCChildProject(childProjectList, orgProjectList);
                        //    if (childProList != null && childProList.Count > 0)
                        //    {
                        //        childClassOrProjectList.AddRange(childProList);
                        //    }
                        //}
                        if (childClassOrProjectList != null && childClassOrProjectList.Count > 0)
                        {
                            treeModel.children = childClassOrProjectList;
                        }
                    }
                    //else
                    //{
                    //    var childProjectList = orgProjectList.FindAll(e => e.GUID_ProjectClass == currentList[i].GUID && e.PGUID == null);//项目子项
                    //    if (childProjectList != null && childProjectList.Count > 0)
                    //    {
                    //        treeModel.state = "closed";
                    //        var childProList = GetChildProject(childProjectList, orgProjectList);//项目子项
                    //        if (childProList != null && childProList.Count > 0)
                    //        {
                    //            treeModel.children = childProList;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        treeModel.state = "open";
                    //    }
                    //}
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 基础----项目的子项tree
        /// </summary>
        /// <param name="currentList">当前List</param>
        /// <param name="orgList">原始数据集</param>
        /// <param name="sb">StringBuilder对象</param>
        /// <returns>string</returns>
        private List<TreeNodeModel> GetJCChildProject(List<SS_ProjectClassView> currentList, List<SS_ProjectClassView> orgList)
        {
            string prjModel = typeof(SS_ProjectClass).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null && currentList.Count > 0)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = string.Format("({0}){1}", CommonFuntion.StringToJson(currentList[i].ProjectClassKey), CommonFuntion.StringToJson(currentList[i].ProjectClassName));
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = prjModel;
                    dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey);
                    dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                    dic["valid"] = "1";
                    treeModel.attributes = dic;
                    var childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var list = GetJCChildProject(childList, orgList);
                        if (list != null && list.Count > 0)
                        {
                            treeModel.children = list;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = prjModel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["ProjectClassKey"] = CommonFuntion.StringToJson(currentList[i].ProjectClassKey);
                        dic["ProjectClassName"] = CommonFuntion.StringToJson(currentList[i].ProjectClassName);
                        treeModel.attributes = dic;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion

        #region 基础----桌面设置--政策法规tree
        /// <summary>
        /// 基础--桌面设置--政策法规tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCOfficeFileTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string bgmodel = typeof(SS_OfficeFileTypeView).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var bgcodeList = commTree.GetJCOfficeFileType(false, operatorId);
            var officeFiles = commTree.GetJCOfficeFile(false, operatorId);
            var parentList = bgcodeList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].FileTypeName + "(" + parentList[i].FileTypeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = bgmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(parentList[i].GUID.ToString());
                dic["PGUID"] = CommonFuntion.StringToJson(parentList[i].PGUID.ToString());
                dic["FileTypeKey"] = CommonFuntion.StringToJson(parentList[i].FileTypeKey);
                dic["FileTypeName"] = CommonFuntion.StringToJson(parentList[i].FileTypeName);
                dic["PKey"] = CommonFuntion.StringToJson(parentList[i].PKey);
                dic["PName"] = CommonFuntion.StringToJson(parentList[i].PName);
                treeModel.attributes = dic;
                var childList = bgcodeList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var bgChild = GetJCOfficeFileNode(childList, bgcodeList, officeFiles);
                    if (bgChild != null && bgChild.Count > 0)
                    {
                        treeModel.children = bgChild;
                    }
                    else
                    {
                        treeModel.state = "open";

                    }
                }
                else
                {

                    treeModel.state = "open";
                    var OfficeFileList = officeFiles.FindAll(e => e.GUID_OfficeFileType == parentList[i].GUID);
                    if (OfficeFileList != null && OfficeFileList.Count > 0)
                    {
                        treeModel.state = "closed";
                        List<TreeNodeModel> childOfficeList = new List<TreeNodeModel>();
                        foreach (OA_OfficeFile item in OfficeFileList)
                        {
                            TreeNodeModel treeChildModel = new TreeNodeModel();
                            treeChildModel.id = item.GUID.ToString();
                            treeChildModel.text = CommonFuntion.StringToJson(item.FileName);
                            Dictionary<string, string> officedic = new Dictionary<string, string>();
                            officedic["m"] = typeof(OA_OfficeFile).Name;
                            officedic["valid"] = "1";
                            officedic["GUID_OfficeFile"] = CommonFuntion.StringToJson(item.GUID.ToString());
                            officedic["OrderNum"] = CommonFuntion.StringToJson(item.OrderNum.ToString());
                            //dic["FileBody"] = CommonFuntion.StringToJson(item.FileBody.ToString());
                            officedic["FileKey"] = CommonFuntion.StringToJson(item.FileKey);
                            officedic["FileName"] = CommonFuntion.StringToJson(item.FileName);
                            officedic["Visible"] = CommonFuntion.StringToJson(item.Visible.ToString());

                            officedic["FileTypeName"] = CommonFuntion.StringToJson(parentList[i].FileTypeName);
                            officedic["FileTypeKey"] = CommonFuntion.StringToJson(parentList[i].FileTypeKey);
                            officedic["PKey"] = CommonFuntion.StringToJson(parentList[i].PKey);
                            officedic["PName"] = CommonFuntion.StringToJson(parentList[i].PName);
                            officedic["PGUID"] = CommonFuntion.StringToJson(parentList[i].PGUID.ToString());
                            officedic["GUID_OfficeFileType"] = CommonFuntion.StringToJson(parentList[i].GUID.ToString());
                            treeChildModel.attributes = officedic;
                            childOfficeList.Add(treeChildModel);
                        }
                        treeModel.children = childOfficeList;
                    }
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        ///<summary>
        ///获取政策法规附件信息
        ///</summary>
        ///<param name="currentList"></param>
        ///<param name="orgList"></param>
        ///<returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetJCOfficeFileNode(List<SS_OfficeFileTypeView> currentList, List<SS_OfficeFileTypeView> orgList, List<OA_OfficeFile> officeFile)
        {
            string bgmodel = typeof(SS_OfficeFileTypeView).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].FileTypeName + "(" + currentList[i].FileTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = bgmodel;
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                    dic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                    treeModel.attributes = dic;

                    List<SS_OfficeFileTypeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetJCOfficeFileNode(childList, orgList, officeFile);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = bgmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                        dic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);

                        //返回政策法规附件的所有字段



                        var OfficeFileList = officeFile.FindAll(e => e.GUID_OfficeFileType == currentList[i].GUID);
                        if (OfficeFileList != null && OfficeFileList.Count > 0)
                        {
                            treeModel.state = "closed";
                            List<TreeNodeModel> childOfficeList = new List<TreeNodeModel>();
                            foreach (OA_OfficeFile item in OfficeFileList)
                            {
                                TreeNodeModel treeChildModel = new TreeNodeModel();
                                treeChildModel.id = item.GUID.ToString();
                                treeChildModel.text = CommonFuntion.StringToJson(item.FileName);
                                Dictionary<string, string> officedic = new Dictionary<string, string>();
                                officedic["m"] = typeof(OA_OfficeFile).Name;
                                officedic["valid"] = "1";
                                officedic["GUID_OfficeFile"] = CommonFuntion.StringToJson(item.GUID.ToString());
                                officedic["OrderNum"] = CommonFuntion.StringToJson(item.OrderNum.ToString());
                                //dic["FileBody"] = CommonFuntion.StringToJson(item.FileBody.ToString());
                                officedic["FileKey"] = CommonFuntion.StringToJson(item.FileKey);
                                officedic["FileName"] = CommonFuntion.StringToJson(item.FileName);
                                officedic["Visible"] = CommonFuntion.StringToJson(item.Visible.ToString());

                                officedic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                                officedic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                                officedic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                                officedic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                                officedic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                                officedic["GUID_OfficeFileType"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                                treeChildModel.attributes = officedic;
                                childOfficeList.Add(treeChildModel);
                            }
                            treeModel.children = childOfficeList;
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion


        #region 基础----桌面设置--通知通告tree
        /// <summary>
        /// 基础--桌面设置--通知公告tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCNoticeTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<OA_Notice> list = commTree.GetJCNotice(false, operatorId);
            var noticeAnnexList = commTree.GetJCNoticeAnnex(false, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCNoticeNode(list, noticeAnnexList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----通知通告Json----tree
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCNoticeNode(List<OA_Notice> list, List<OA_NoticeAnnexEx> noticeList)
        {
            var permodel = typeof(OA_Notice).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].Title);
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["Title"] = CommonFuntion.StringToJson(list[j].Title);
                    dic["Notice"] = CommonFuntion.StringToJson(list[j].Notice);
                    dic["isPop"] = CommonFuntion.StringToJson(list[j].isPop.ToString());     //首页显示
                    dic["OrderNum"] = CommonFuntion.StringToJson(list[j].OrderNum.ToString());  //编号
                    dic["overdue"] = CommonFuntion.StringToJson(list[j].overdue.ToString());   //过期
                    dic["EndDate"] = CommonFuntion.StringToJson(list[j].EndDate == null ? "" : ((DateTime)list[j].EndDate).ToString("yyyy-MM-dd"));  //过期时间
                    dic["NoticeDate"] = CommonFuntion.StringToJson(list[j].NoticeDate == null ? "" : ((DateTime)list[j].NoticeDate).ToString("yyyy-MM-dd"));
                    //返回通知公告对应的附件的所有数据



                    var noticeAnnexList = noticeList.FindAll(e => e.GUID_Notice == list[j].GUID).ToList();
                    if (noticeAnnexList != null && noticeAnnexList.Count > 0)
                    {
                        var guidsList = noticeAnnexList.Select(e => e.GUID).ToList();
                        string guids = string.Empty;
                        //因为通知公告可能对应多个附件，所以要取多个GUID
                        for (int i = 0, k = guidsList.Count; i < k; i++)
                        {
                            if (i == k - 1)
                            {//一个GUID
                                guids += guidsList[i].ToString();
                            }
                            else
                            {//多个GUID的时候



                                guids += guidsList[i].ToString() + ",";
                            }

                        }
                        //得到通知公告下所对应的附件的字段值


                        dic["GUID_NoticeAnnex"] = guids;
                        dic["AnnexName"] = string.Join(",\n", noticeAnnexList.Select(e => e.AnnexName));
                     //   dic["Annex"] = string.Join(",", noticeAnnexList.Select(e => e.Annex));
                    }
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        #endregion

        #region 基础----桌面设置--文件类型tree
        /// <summary>
        /// 基础--桌面设置--政策法规tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCOfficeFileTypeTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string bgmodel = typeof(SS_OfficeFileTypeView).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var bgcodeList = commTree.GetJCOfficeFileType(false, operatorId);
            var officeFiles = commTree.GetJCOfficeFile(false, operatorId);
            var parentList = bgcodeList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                //这是最高一级节点



                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].FileTypeName + "(" + parentList[i].FileTypeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = bgmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(parentList[i].GUID.ToString());
                dic["PGUID"] = CommonFuntion.StringToJson(parentList[i].PGUID.ToString());
                dic["FileTypeKey"] = CommonFuntion.StringToJson(parentList[i].FileTypeKey);
                dic["FileTypeName"] = CommonFuntion.StringToJson(parentList[i].FileTypeName);
                dic["PKey"] = CommonFuntion.StringToJson(parentList[i].PKey);
                dic["PName"] = CommonFuntion.StringToJson(parentList[i].PName);
                treeModel.attributes = dic;
                var childList = bgcodeList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "closed";
                    var bgChild = GetJCOfficeFileTypeNode(childList, bgcodeList, officeFiles);
                    if (bgChild != null && bgChild.Count > 0)
                    {
                        treeModel.children = bgChild;
                    }
                    else
                    {
                        treeModel.state = "open";

                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        ///<summary>
        ///获取政策法规附件信息
        ///</summary>
        ///<param name="currentList"></param>
        ///<param name="orgList"></param>
        ///<returns>TreeNodeModel List</returns>
        private List<TreeNodeModel> GetJCOfficeFileTypeNode(List<SS_OfficeFileTypeView> currentList, List<SS_OfficeFileTypeView> orgList, List<OA_OfficeFile> officeFile)
        {
            string bgmodel = typeof(SS_OfficeFileTypeView).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (currentList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    //这是二级节点
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = currentList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(currentList[i].FileTypeName + "(" + currentList[i].FileTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = bgmodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                    dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                    dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                    dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    dic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                    dic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                    treeModel.attributes = dic;

                    List<SS_OfficeFileTypeView> childList = orgList.FindAll(e => e.PGUID == currentList[i].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var bgChild = GetJCOfficeFileTypeNode(childList, orgList, officeFile);
                        if (bgChild != null && bgChild.Count > 0)
                        {
                            treeModel.children = bgChild;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                        dic["m"] = bgmodel;
                        dic["valid"] = "1";
                        dic["GUID"] = CommonFuntion.StringToJson(currentList[i].GUID.ToString());
                        dic["FileTypeName"] = CommonFuntion.StringToJson(currentList[i].FileTypeName);
                        dic["FileTypeKey"] = CommonFuntion.StringToJson(currentList[i].FileTypeKey);
                        dic["PGUID"] = CommonFuntion.StringToJson(currentList[i].PGUID.ToString());
                        dic["PKey"] = CommonFuntion.StringToJson(currentList[i].PKey);
                        dic["PName"] = CommonFuntion.StringToJson(currentList[i].PName);
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion




        /// <summary>
        /// 外聘人员与人员档案
        /// </summary>
        /// <returns></returns>
        public ContentResult GetInvitePeson()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.GetPersonView(true, operatorId);
            if (list != null && list.Count > 0)
            {
                // ------------------------------
                var nodeList = GetInvitePersonNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 判断是否存在InvitePerson
        /// </summary>
        /// <param name="orgList"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool IsExistInvitePerson(List<SS_InvitePersonView> orgList, SS_PersonView model)
        {
            var list = orgList.FindAll(e => e.InvitePersonName == model.PersonName && (model.IDCard != null && e.CredentialTypekey == model.IDCardType.ToString()) && e.InvitePersonIDCard == model.IDCard);
            if (list != null && list.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 交通工具
        /// </summary>
        /// <returns></returns>
        public ContentResult GetTrafficTree()
        {
            StringBuilder sb = new StringBuilder();
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_TrafficView> list = dbobj.GetTraffic(true, operatorId);
            var str = GetTrafficJson(list);
            return Content(str);
        }


        /// <summary>
        /// Json格式工具
        /// </summary>
        /// <param name="model">SS_Allowance工具视图实体</param>
        /// <returns></returns>
        private string GetAllowanceJson(List<SS_Allowance> list)
        {
            StringBuilder sb = new StringBuilder();
            var permodel = typeof(SS_Allowance).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].AllowanceName + "(" + list[j].AllowanceKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["AllowanceKey"] = CommonFuntion.StringToJson(list[j].AllowanceKey.ToString());
                    dic["AllowanceName"] = CommonFuntion.StringToJson(list[j].AllowanceName.ToString());
                    dic["AllowanceType"] = CommonFuntion.StringToJson(list[j].AllowanceType.ToString());
                    dic["DefaultValue"] = CommonFuntion.StringToJson(list[j].DefaultValue.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    treeModel.attributes = dic;

                    treeModelList.Add(treeModel);
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return jsonStr;
        }
        /// <summary>
        /// Json格式工具
        /// </summary>
        /// <param name="model">SS_TrafficView工具视图实体</param>
        /// <returns></returns>
        private string GetTrafficJson(List<SS_TrafficView> list)
        {
            StringBuilder sb = new StringBuilder();
            var permodel = typeof(SS_Traffic).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].TrafficName + "(" + list[j].TrafficKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["TrafficKey"] = CommonFuntion.StringToJson(list[j].TrafficKey.ToString());
                    dic["TrafficName"] = CommonFuntion.StringToJson(list[j].TrafficName.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    treeModel.attributes = dic;

                    treeModelList.Add(treeModel);
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return jsonStr;
        }


        #region 基础----角色用户

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetRoleTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Role> list = dbobj.GetJCRole(true, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetRoleNode(list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----获取角色信息----tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCRoleTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Role> list = dbobj.GetJCRole(true, operatorId);
            List<RoleOperatorModel> RoleList = BaseTree.GetRoleOperatorView();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCRoleNode(list, RoleList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----角色Json----tree
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCRoleNode(List<SS_Role> list, List<RoleOperatorModel> roleOperatorList)
        {
            var permodel = typeof(SS_Role).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].RoleName + "<" + list[j].RoleKey + ">");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["RoleKey"] = CommonFuntion.StringToJson(list[j].RoleKey);
                    dic["RoleName"] = CommonFuntion.StringToJson(list[j].RoleName);
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    //返回角色下操作员的GUID
                    var OperatorGUIDList = roleOperatorList.FindAll(e => e.GUID_Role == list[j].GUID).Select(s => s.GUID_Operator).Distinct().ToList();
                    if (OperatorGUIDList != null && OperatorGUIDList.Count > 0)
                    {
                        dic["GUID_Operator"] = string.Join(",", OperatorGUIDList);
                    }
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        #endregion

        #region 基础----获取操作员信息----tree
        /// <summary>
        /// 基础----获取操作员信息----tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCOperatorTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Operator> list = dbobj.GetJCOperator(true, operatorId, false);
            List<RoleOperatorModel> RoleOperatorlist = BaseTree.GetRoleOperatorView();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCOperatorNode(list, RoleOperatorlist);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----操作员Json----tree
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCOperatorNode(List<SS_Operator> list, List<RoleOperatorModel> roleOperatorList)
        {
            var permodel = typeof(SS_Operator).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].OperatorName + "(" + list[j].OperatorKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["OperatorKey"] = CommonFuntion.StringToJson(list[j].OperatorKey);
                    dic["OperatorName"] = CommonFuntion.StringToJson(list[j].OperatorName);
                    dic["Password"] = CommonFuntion.StringToJson(list[j].Password);
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    dic["IsTimeLimited"] = CommonFuntion.StringToJson(list[j].IsTimeLimited.ToString());
                    dic["StartTime"] = CommonFuntion.StringToJson(list[j].StartTime == null ? "" : ((DateTime)list[j].StartTime).ToString("yyyy-MM-dd"));
                    dic["StopTime"] = CommonFuntion.StringToJson(list[j].StopTime == null ? "" : ((DateTime)list[j].StopTime).ToString("yyyy-MM-dd"));
                    //返回用户中角色的GUID
                    var roleGUIDlist = roleOperatorList.FindAll(e => e.GUID_Operator == list[j].GUID).Select(s => s.GUID_Role).Distinct().ToList();
                    if (roleGUIDlist != null && roleGUIDlist.Count > 0)
                    {
                        dic["GUID_Role"] = string.Join(",", roleGUIDlist);
                    }
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion


        #region 基础----薪酬设置--工资项目设置--tree
        /// <summary>
        ///基础--薪酬设置--工资项目设置--获取工资项目信息tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCItemTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SA_Item> list = dbobj.GetJCItem(false, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCItemNode(list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----薪酬设置--工资项目设置--工资项目Json--tree
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCItemNode(List<SA_Item> list)
        {
            var permodel = typeof(SA_Item).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].ItemName + "<" + list[j].ItemKey + ">");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["ItemKey"] = CommonFuntion.StringToJson(list[j].ItemKey);
                    dic["ItemName"] = CommonFuntion.StringToJson(list[j].ItemName);
                    dic["ItemType"] = CommonFuntion.StringToJson(list[j].ItemType.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion

        #region 基础----薪酬设置--工资计划设置--tree
        /// <summary>
        ///基础--薪酬设置--工资项目设置--获取工资项目信息tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCPlanTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SA_PlanView> list = dbobj.GetJCPlan(false, operatorId);
            List<SA_PlanPersonSetView> planpersonList = dbobj.GetJCPlanPersonSet(false, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCPlanNode(list, planpersonList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础----薪酬设置--工资项目设置--工资项目Json--tree
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCPlanNode(List<SA_PlanView> list, List<SA_PlanPersonSetView> PlanPersonSetList)
        {
            var permodel = typeof(SA_Plan).Name;
            var personmodel = typeof(SA_PlanPersonSet).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].PlanName + "<" + list[j].PlanKey + ">");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["GUID_SA_PlanArea"] = CommonFuntion.StringToJson(list[j].GUID_SA_PlanArea.ToString());
                    dic["PlanKey"] = CommonFuntion.StringToJson(list[j].PlanKey);
                    dic["PlanName"] = CommonFuntion.StringToJson(list[j].PlanName);
                    dic["PlanDate"] = CommonFuntion.StringToJson(list[j].PlanDate == null ? "" : ((DateTime)list[j].PlanDate).ToString("yyyy-MM-dd"));
                    dic["PlanAreaKey"] = CommonFuntion.StringToJson(list[j].PlanAreaKey.ToString());
                    dic["PlanAreaName"] = CommonFuntion.StringToJson(list[j].PlanAreaName.ToString());
                    dic["IsStop"] = CommonFuntion.StringToJson(list[j].IsStop.ToString());
                    dic["IsDefault"] = CommonFuntion.StringToJson(list[j].IsDefault.ToString());
                    var BankModel = PlanPersonSetList.FirstOrDefault(e => e.GUID_SA_Plan == list[j].GUID);
                    if (BankModel != null)
                    {
                        // dic["GUID"] = string.Join(",", PlanPersonSetList[j].GUID);
                        //dic["GUID_SA_Plan"] = string.Join(",", PlanPersonSetList[j].GUID_SA_Plan);
                        dic["GUID_SS_Bank"] = CommonFuntion.StringToJson(BankModel.GUID_SS_Bank.ToString());
                        dic["GUID_SS_Person"] = CommonFuntion.StringToJson(BankModel.GUID_SS_Person.ToString());
                        dic["GUID_SA_Plan"] = CommonFuntion.StringToJson(BankModel.GUID_SA_Plan.ToString());
                        dic["BankName"] = CommonFuntion.StringToJson(BankModel.BankName);
                        dic["BankKey"] = CommonFuntion.StringToJson(BankModel.BankKey);
                    }
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        #endregion


        /// <summary>
        /// 角色Json
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetRoleNode(List<SS_Role> list)
        {
            var permodel = typeof(SS_Role).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].RoleName + "<" + list[j].RoleKey + ">");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["RoleKey"] = CommonFuntion.StringToJson(list[j].RoleKey);
                    dic["RoleName"] = CommonFuntion.StringToJson(list[j].RoleName);
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #region   基础--其他设置--往来单位档案--tree
        /// <summary>
        /// 客户信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCCustomerTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Customer> list = dbobj.GetCustomer(true, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetJCCustomerNode(list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 客户Json信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetJCCustomerNode(List<SS_Customer> list)
        {
            var permodel = typeof(SS_Customer).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].CustomerName + "(" + list[j].CustomerKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["CustomerName"] = CommonFuntion.StringToJson(list[j].CustomerName);
                    dic["CustomerKey"] = CommonFuntion.StringToJson(list[j].CustomerKey);
                    dic["CustomerAddress"] = CommonFuntion.StringToJson(list[j].CustomerAddress);
                    dic["CustomerBankName"] = CommonFuntion.StringToJson(list[j].CustomerBankName);
                    dic["CustomerBankNumber"] = CommonFuntion.StringToJson(list[j].CustomerBankNumber);
                    dic["CustomerTelephone"] = CommonFuntion.StringToJson(list[j].CustomerTelephone);
                    dic["CustomerFax"] = CommonFuntion.StringToJson(list[j].CustomerFax);
                    dic["CustomerPostcode"] = CommonFuntion.StringToJson(list[j].CustomerPostcode);
                    dic["CustomerEmail"] = CommonFuntion.StringToJson(list[j].CustomerEmail);
                    dic["CustomerWebsite"] = CommonFuntion.StringToJson(list[j].CustomerWebsite);
                    dic["CustomerLikeMan"] = CommonFuntion.StringToJson(list[j].CustomerLikeMan);
                    dic["IsCustomer"] = list[j].IsCustomer == null ? "false" : list[j].IsCustomer.ToString();
                    dic["IsVendor"] = list[j].IsVendor == null ? "false" : list[j].IsVendor.ToString();
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #endregion


        /// <summary>
        /// 客户信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetCustomerTree()
        {
            var type = "0";
            if (Request["ctype"] != null)
            {
                type = Request["ctype"];
            }
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_Customer> list = dbobj.GetCustomer(true, operatorId, type);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            treeModelList = GetCustomerNode(list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 客户Json信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetCustomerNode(List<SS_Customer> list)
        {
            var permodel = typeof(SS_Customer).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].CustomerName + "(" + list[j].CustomerKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["CustomerName"] = CommonFuntion.StringToJson(list[j].CustomerName);
                    dic["CustomerKey"] = CommonFuntion.StringToJson(list[j].CustomerKey);
                    dic["CustomerAddress"] = CommonFuntion.StringToJson(list[j].CustomerAddress);
                    dic["CustomerBankName"] = CommonFuntion.StringToJson(list[j].CustomerBankName);
                    dic["CustomerBankNumber"] = CommonFuntion.StringToJson(list[j].CustomerBankNumber);
                    dic["CustomerTelephone"] = CommonFuntion.StringToJson(list[j].CustomerTelephone);
                    dic["CustomerFax"] = CommonFuntion.StringToJson(list[j].CustomerFax);
                    dic["CustomerPostcode"] = CommonFuntion.StringToJson(list[j].CustomerPostcode);
                    dic["CustomerEmail"] = CommonFuntion.StringToJson(list[j].CustomerEmail);
                    dic["CustomerWebsite"] = CommonFuntion.StringToJson(list[j].CustomerWebsite);
                    dic["CustomerLikeMan"] = CommonFuntion.StringToJson(list[j].CustomerLikeMan);
                    dic["IsCustomer"] = list[j].IsCustomer == null ? "false" : list[j].IsCustomer.ToString();
                    dic["IsVendor"] = list[j].IsVendor == null ? "false" : list[j].IsVendor.ToString();
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 银行账户信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBankAccountTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_BankAccountView> list = dbobj.GetBankAccountView(true, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = GetBankAccountNode(list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 银行账户信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> GetBankAccountNode(List<SS_BankAccountView> list)
        {
            string jsonStr = string.Empty;
            var permodel = typeof(SS_BankAccount).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].BankAccountName + "(" + list[j].BankAccountKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["BankAccountName"] = CommonFuntion.StringToJson(list[j].BankAccountName);
                    dic["BankAccountKey"] = CommonFuntion.StringToJson(list[j].BankAccountKey);
                    dic["BankAccountNo"] = list[j].BankAccountNo;
                    dic["DWName"] = list[j].DWName;
                    dic["BankName"] = list[j].BankName;
                    dic["IsGuoKu"] = list[j].IsGuoKu.ToString();
                    treeModel.attributes = dic;

                    treeModelList.Add(treeModel);

                }
            }
            return treeModelList;
        }

        public ContentResult GetFunctionClassTreeForCheck()
        {
            var select = Request["select"];
            bool bSelect = true;
            if (select == "0")
            {
                bSelect = false;
            }
            // 获得全部的功能分类
            List<SS_FunctionClassView> list = dbobj.GetFunctionClassView();
            var permodel = typeof(SS_FunctionClass).Name;
            var jsonStr = string.Empty;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].PGUID != null)
                    {
                        continue;       // 先获得高级functionclass
                    }
                    // 对节点属性进行赋值
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].FunctionClassName + "(" + list[j].FunctionClassKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["FunctionClassName"] = CommonFuntion.StringToJson(list[j].FunctionClassName);
                    dic["FunctionClassKey"] = CommonFuntion.StringToJson(list[j].FunctionClassKey);
                    treeModel.attributes = dic;

                    // 获得子节点
                    treeModel.children = GetChildFunctionClass(ref list, list[j]);
                    treeModelList.Add(treeModel);
                }
            }

            SetCheckBoxForTree(bSelect, ref treeModelList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        // objFC 是需要找自己的子节点的功能分类   list是所有的功能分类的集合
        private List<TreeNodeModel> GetChildFunctionClass(ref List<SS_FunctionClassView> list, SS_FunctionClassView objFC)
        {
            // 找到objFC所有的子节点
            List<SS_FunctionClassView> childList = list.FindAll(e => e.PGUID == objFC.GUID).ToList();
            List<TreeNodeModel> childTreeNode = new List<TreeNodeModel>();
            foreach (SS_FunctionClassView item in childList)
            {
                // 设置节点属性
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = item.GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(item.FunctionClassName + "(" + item.FunctionClassKey + ")");
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = typeof(SS_FunctionClass).Name;
                dic["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                dic["FunctionClassName"] = CommonFuntion.StringToJson(item.FunctionClassName);
                dic["FunctionClassKey"] = CommonFuntion.StringToJson(item.FunctionClassKey);
                treeModel.attributes = dic;

                // 继续获取节点
                treeModel.children = GetChildFunctionClass(ref list, item);
                childTreeNode.Add(treeModel);
            }

            return childTreeNode;
        }
        /// <summary>
        /// 功能分类
        /// </summary>
        /// <returns></returns>
        public ContentResult GetFunctionClassTree()
        {
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_FunctionClassView> list = dbobj.GetFunctionClassView();
            string json = FunctionClassJson(list);
            return Content(json);
        }
        /// <summary>
        /// 功能分类Json数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private string FunctionClassJson(List<SS_FunctionClassView> list)
        {
            var permodel = typeof(SS_FunctionClass).Name;
            var jsonStr = string.Empty;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].FunctionClassName + "(" + list[j].FunctionClassKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["FunctionClassName"] = CommonFuntion.StringToJson(list[j].FunctionClassName);
                    dic["FunctionClassKey"] = CommonFuntion.StringToJson(list[j].FunctionClassKey);
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return jsonStr;
        }
        /// <summary>
        /// 获取单据树
        /// </summary>
        /// <returns></returns>
        public ContentResult GetDocTypeTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DocType> list = dbobj.GetDocType(true, operatorId);
            string[] strArr = { "02", "03", "04", "05", "0501", "0502", "0801", "0802", "1101" };
            List<SS_YWType> ywList = dbobj.GetYWType(true, operatorId);
            List<string> filterList = strArr.ToList();
            ywList = ywList.Where(e => filterList.Contains(e.YWTypeKey)).ToList();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = YWTypeNode(ywList, list);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 业务类型Json数据
        /// </summary>
        /// <param name="ywList"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> YWTypeNode(List<SS_YWType> ywList, List<SS_DocType> list)
        {
            string jsonStr = string.Empty;
            var permodel = typeof(SS_YWType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (ywList != null && ywList.Count() > 0)
            {
                for (int j = 0; j < ywList.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(ywList[j].YWTypeName + "(" + ywList[j].YWTypeKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(ywList[j].GUID.ToString());
                    dic["YWTypeName"] = CommonFuntion.StringToJson(ywList[j].YWTypeName);
                    dic["YWTypeKey"] = CommonFuntion.StringToJson(ywList[j].YWTypeKey);
                    treeModel.attributes = dic;

                    var childList = list.FindAll(e => e.GUID_YWType == ywList[j].GUID);
                    if (childList != null && childList.Count > 0)
                    {
                        treeModel.state = "closed";
                        var nodeList = DocTypeNode(childList, permodel, ywList[j].YWTypeKey);
                        if (nodeList != null && nodeList.Count > 0)
                        {
                            treeModel.children = nodeList;
                        }
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 单据类型Json数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> DocTypeNode(List<SS_DocType> list, string pModel, string pModelGuid)
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            var permodel = typeof(SS_DocType).Name;
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].DocTypeName + "(" + list[j].DocTypeKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["pm"] = pModel + "#" + pModelGuid.ToString();
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["DocTypeName"] = CommonFuntion.StringToJson(list[j].DocTypeName);
                    dic["DocTypeKey"] = CommonFuntion.StringToJson(list[j].DocTypeKey);
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 报销单据类型
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBXDocTypeTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_DocType> list = dbobj.GetDocType(true, operatorId);
            List<SS_YWType> ywList = dbobj.GetYWType(true, operatorId);
            SS_YWType ywModel = ywList.FirstOrDefault(e => e.YWTypeKey == "02");//报销管理
            if (ywModel == null) return null;
            List<SS_DocType> docList = list.FindAll(e => e.GUID_YWType == ywModel.GUID);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = DocTypeNode(docList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 单据类型Json数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> DocTypeNode(List<SS_DocType> list)
        {
            string jsonStr = string.Empty;
            var permodel = typeof(SS_DocType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].DocTypeName + "(" + list[j].DocTypeKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["DocTypeName"] = CommonFuntion.StringToJson(list[j].DocTypeName);
                    dic["DocTypeKey"] = CommonFuntion.StringToJson(list[j].DocTypeKey);
                    treeModel.attributes = dic;

                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        #region   基础--预算设置--tree
        /// <summary>
        /// 基础----预算设置----tree
        /// </summary>
        /// <returns></returns>
        public ContentResult GetJCBGSetUpTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<BG_SetupView> bgsetupList = dbobj.GetBGSetUpView(false, operatorId);
            //List<BG_StepView> bgstepList = dbobj.GetBGStepView(false, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = BGSetUpNode(bgsetupList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        #endregion

        /// <summary>
        /// 预算设置
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBGSetUpTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<BG_SetupView> bgsetupList = dbobj.GetBGSetUpView(true, operatorId);

            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = BGSetUpNode(bgsetupList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 预算设置Json
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<TreeNodeModel> BGSetUpNode(List<BG_SetupView> list)
        {
            var permodel = typeof(BG_Setup).Name;
            BG_StepView pStepModel = new BG_StepView();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();

                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].BGSetupName + "(" + list[j].BGSetupKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["BGSetupName"] = CommonFuntion.StringToJson(list[j].BGSetupName);
                    dic["BGSetupKey"] = CommonFuntion.StringToJson(list[j].BGSetupKey);
                    dic["GUID_BGType"] = list[j].GUID_BGType == null ? "" : list[j].GUID_BGType.ToString();
                    dic["BGTypeKey"] = CommonFuntion.StringToJson(list[j].BGTypeKey);
                    dic["BGTypeName"] = CommonFuntion.StringToJson(list[j].BGTypeName);
                    
                    dic["GUID_BGStep"] = list[j].GUID_BGStep == null ? "" : list[j].GUID_BGStep.ToString();
                    dic["BGStepKey"] = CommonFuntion.StringToJson(list[j].BGSetupKey);
                    dic["BGStepName"] = CommonFuntion.StringToJson(list[j].BGStepName);


                    dic["PBG_StepGUID"] = list[j].PBG_StepGUID == null ? "" : list[j].PBG_StepGUID.ToString();
                    dic["PBGStepKey"] = CommonFuntion.StringToJson(list[j].PBGStepKey);
                    dic["PBGStepName"] = CommonFuntion.StringToJson(list[j].PBGStepName);


                    treeModel.attributes = dic;

                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }

        /// <summary>
        /// 预算步骤
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBGStepTree()
        {
            string jsonStr = string.Empty;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<BG_StepView> bgstepList = dbobj.GetBGStepView(true, operatorId);
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            treeModelList = BGSetStepNode(bgstepList);
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 预算步骤
        /// </summary>
        /// <param name="bgstepList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> BGSetStepNode(List<BG_StepView> list)
        {
            var permodel = typeof(BG_Setup).Name;
            BG_StepView pStepModel = new BG_StepView();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (list != null && list.Count() > 0)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.id = list[j].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(list[j].BGStepName + "(" + list[j].BGStepKey + ")");
                    treeModel.state = "open";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["valid"] = "1";
                    dic["GUID"] = CommonFuntion.StringToJson(list[j].GUID.ToString());
                    dic["BGStepName"] = CommonFuntion.StringToJson(list[j].BGStepName);
                    dic["BGStepKey"] = CommonFuntion.StringToJson(list[j].BGStepKey);
                    treeModel.attributes = dic;
                    if (list[j].BGStepKey == "05")
                    {
                        treeModel.isCheck = true;
                    }
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 会计凭证
        /// </summary>
        /// <returns></returns>
        public ContentResult GetCWAccountTitleTree()
        {
            string jsonStr = string.Empty;
            int year = 0;
            string strYear = Request["year"];
            if (!string.IsNullOrEmpty(strYear))
            {
                int.TryParse(strYear, out year);
            }
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<AccountMainView> acountMainList = dbobj.GetAccountMainView(true, operatorId);
            List<CW_AccountTitle> cwAccountTileList = dbobj.GetCWAcountTitle(true, operatorId, year);
            var permodel = typeof(CW_AccountTitle).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (acountMainList != null && acountMainList.Count > 0)
            {
                for (int i = 0; i < acountMainList.Count; i++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.state = "open";
                    treeModel.id = acountMainList[i].GUID.ToString();
                    treeModel.text = CommonFuntion.StringToJson(acountMainList[i].Description) + "(" + CommonFuntion.StringToJson(acountMainList[i].AccountKey) + ")";
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["GUID"] = CommonFuntion.StringToJson(acountMainList[i].GUID.ToString());
                    dic["AccountName"] = CommonFuntion.StringToJson(acountMainList[i].AccountName);
                    dic["AccountKey"] = CommonFuntion.StringToJson(acountMainList[i].AccountKey);
                    dic["Description"] = CommonFuntion.StringToJson(acountMainList[i].Description);
                    var parentList = cwAccountTileList.FindAll(e => e.PGUID == null && e.GUID_AccountMain == acountMainList[i].GUID);
                    if (parentList != null && parentList.Count > 0)
                    {
                        var childNode = CWAccountTitleNode(cwAccountTileList, parentList);
                        if (childNode != null && childNode.Count > 0)
                        {
                            treeModel.state = "closed";
                            treeModel.children = childNode;
                        }
                    }
                    treeModelList.Add(treeModel);
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 会计科目节点
        /// </summary>
        /// <param name="bgstepList"></param>
        /// <returns></returns>
        private List<TreeNodeModel> CWAccountTitleNode(List<CW_AccountTitle> orgList, List<CW_AccountTitle> currentList)
        {
            var permodel = typeof(CW_AccountTitle).Name;
            CW_AccountTitle pStepModel = new CW_AccountTitle();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            if (orgList == null || orgList.Count == 0) return null;
            if (currentList != null && currentList.Count() > 0)
            {
                for (int j = 0; j < currentList.Count; j++)
                {
                    TreeNodeModel treeModel = new TreeNodeModel();
                    treeModel.state = "open";
                    treeModel.id = CommonFuntion.StringToJson(currentList[j].GUID.ToString());
                    treeModel.text = CommonFuntion.StringToJson(currentList[j].AccountTitleName + "(" + currentList[j].AccountTitleKey + ")");
                    Dictionary<string, string> dic = new Dictionary<string, string>();
                    dic["m"] = permodel;
                    dic["GUID"] = CommonFuntion.StringToJson(currentList[j].GUID.ToString());
                    dic["AccountTitleName"] = CommonFuntion.StringToJson(currentList[j].AccountTitleName);
                    dic["AccountTitleKey"] = CommonFuntion.StringToJson(currentList[j].AccountTitleKey);
                    var childList = orgList.FindAll(e => e.PGUID == currentList[j].GUID);
                    if (childList != null && childList.Count != 0)
                    {
                        var childNode = CWAccountTitleNode(orgList, childList);
                        if (childNode != null && childNode.Count != 0)
                        {
                            treeModel.state = "closed";
                            treeModel.children = childNode;
                        }
                    }
                    else
                    {
                        dic["valid"] = "1";
                    }
                    treeModel.attributes = dic;
                    treeModelList.Add(treeModel);
                }
            }
            return treeModelList;
        }
        /// <summary>
        /// 银行信息
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBankTree()
        {
            List<SS_Bank> bankList = dbobj.GetBank();
            List<SS_BankAccountView> bankAccount = dbobj.GetBankAccount();
            var permodel = typeof(SS_Bank).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int i = 0; i < bankList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = bankList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(bankList[i].BankName) + "(" + CommonFuntion.StringToJson(bankList[i].BankKey) + ")";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = permodel;
                dic["GUID"] = CommonFuntion.StringToJson(bankList[i].GUID.ToString());
                dic["BankName"] = CommonFuntion.StringToJson(bankList[i].BankName);
                dic["BankKey"] = CommonFuntion.StringToJson(bankList[i].BankKey);
                treeModel.attributes = dic;

                var childList = bankAccount.FindAll(e => e.GUID_Bank == bankList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    var childNode = GetChildBank(childList);
                    if (childNode != null && childNode.Count > 0)
                    {
                        treeModel.state = "closed";
                        treeModel.children = childNode;
                    }
                }
                treeModelList.Add(treeModel);
            }

            var jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }

        private List<TreeNodeModel> GetChildBank(List<SS_BankAccountView> childList)
        {
            var modelName = typeof(SS_BankAccount).Name;
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            if (childList.Count == 0) return list;
            for (int i = 0; i < childList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = childList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(childList[i].BankAccountName) + "(" + CommonFuntion.StringToJson(childList[i].BankAccountKey) + ")";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = modelName;
                dic["GUID"] = CommonFuntion.StringToJson(childList[i].GUID.ToString());
                dic["BankAccountName"] = CommonFuntion.StringToJson(childList[i].BankAccountName);
                dic["BankAccountKey"] = CommonFuntion.StringToJson(childList[i].BankAccountKey);
                treeModel.attributes = dic;
                list.Add(treeModel);
            }
            return list;
        }
        //权限设置 角色
        public JsonResult GetRole()
        {
            var roleList = dbobj.GetRole();
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            foreach (var item in roleList)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = item.GUID.ToString();
                treeModel.@checked = false;
                treeModel.text = CommonFuntion.StringToJson(item.RoleName) + "(" + CommonFuntion.StringToJson(item.RoleKey) + ")";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                dic["RoleName"] = CommonFuntion.StringToJson(item.RoleName);
                dic["RoleKey"] = CommonFuntion.StringToJson(item.RoleKey);
                treeModel.attributes = dic;
                list.Add(treeModel);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        //权限设置 人员
        public JsonResult GetPerson()
        {
            var roleList = dbobj.GetOperator();
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            foreach (var item in roleList)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = item.GUID.ToString();
                treeModel.@checked = false;
                treeModel.text = CommonFuntion.StringToJson(item.OperatorName) + "(" + CommonFuntion.StringToJson(item.OperatorKey) + ")";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["GUID"] = CommonFuntion.StringToJson(item.GUID.ToString());
                dic["OperatorName"] = CommonFuntion.StringToJson(item.OperatorName);
                dic["OperatorKey"] = CommonFuntion.StringToJson(item.OperatorKey);
                treeModel.attributes = dic;
                list.Add(treeModel);
            }
            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetComparisonMain()
        {
            BusinessModel.BusinessEdmxEntities bussinescontext = new BusinessModel.BusinessEdmxEntities();
            var comps = bussinescontext.SS_ComparisonMainView.OrderBy(e=>e.ComparisonKey);
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            foreach (var comp in comps)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = comp.GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(comp.ComparisonName);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = "SS_ComparisonMain";
                dic["GUID"] = CommonFuntion.StringToJson(treeModel.id);
                dic["ComparisonKey"] = CommonFuntion.StringToJson(comp.ComparisonKey);
                dic["ComparisonName"] = CommonFuntion.StringToJson(comp.ComparisonName);
                dic["AccountName"] = CommonFuntion.StringToJson(comp.AccountName);
                dic["AccountKey"] = CommonFuntion.StringToJson(comp.AccountKey);
                dic["Description"] = CommonFuntion.StringToJson(comp.Description);
                dic["DWKey"] = CommonFuntion.StringToJson(comp.DWKey);
                dic["DWName"] = CommonFuntion.StringToJson(comp.DWName);
                dic["ExteriorDataBase"] = CommonFuntion.StringToJson(comp.ExteriorDataBase);
                dic["ExteriorType"] = CommonFuntion.StringToJson(comp.ExteriorType);
                //为了省事 不改变原来的规则基础上 用FiscalYear 拼写库名
                dic["ExteriorYear"] = CommonFuntion.StringToJson(comp.FiscalYear.ToString());
                dic["GUID_AccountDetail"] = CommonFuntion.StringToJson(comp.GUID_AccountDetail.ToString());
                dic["GUID_AccountMain"] = CommonFuntion.StringToJson(comp.GUID_AccountMain.ToString());
                dic["GUID_DW"] = CommonFuntion.StringToJson(comp.GUID_DW.ToString());
                dic["Verion"] = CommonFuntion.StringToJson(comp.Verion);
                treeModel.state = "open";
                treeModel.attributes = dic;
                list.Add(treeModel);

            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public ContentResult GetDocNum()
        {
            List<TreeNodeModel> list = new List<TreeNodeModel>();
            TreeNodeModel treeModel = new TreeNodeModel();
            treeModel.state = "open";
            treeModel.id = "1";
            treeModel.@checked = false;
            treeModel.text = "单据编号";
            Dictionary<string, string> dic = new Dictionary<string, string>();
            treeModel.attributes = dic;
            list.Add(treeModel);
            string strJson = BaothApp.Utils.JsonHelper.objectToJson(list);
            return Content(strJson);
        }

        #region 基础方法
        /// <summary>
        /// 获取基础功能分类tree
        /// </summary>
        public ContentResult GetBaseFunclassClassTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string funClassmodel = typeof(SS_FunctionClass).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var funClassList = BaseTree.GetFunctionClass(false, operatorId);
            var parentList = funClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].FunctionClassName + "(" + parentList[i].FunctionClassKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = parentList[i];
                temp = temp != null ? temp : null;
                dic["m"] = funClassmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                dic["FunctionClassName"] = CommonFuntion.StringToJson(temp.FunctionClassName != null ? temp.FunctionClassName.ToString() : "");
                dic["FunctionClassKey"] = CommonFuntion.StringToJson(temp.FunctionClassKey != null ? temp.FunctionClassKey.ToString() : "");
                dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                dic["BeginYear"] = CommonFuntion.StringToJson(temp.BeginYear.ToString());
                dic["FinanceCode"] = CommonFuntion.StringToJson(temp.FinanceCode != null ? temp.FinanceCode.ToString() : "");
                dic["IsDefault"] = CommonFuntion.StringToJson(temp.IsDefault != null ? temp.IsDefault.ToString() : "");
                dic["IsProject"] = CommonFuntion.StringToJson(temp.IsProject != null ? temp.IsProject.ToString() : "");
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                treeModel.attributes = dic;
                var childList = funClassList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var funClassChild = GetChildFunctionClassNode(childList, funClassList);
                    if (funClassChild != null && funClassChild.Count > 0)
                    {
                        treeModel.children = funClassChild;
                    }
                    else
                    {
                        treeModel.state = "open";

                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取往来类型tree
        /// </summary>
        public ContentResult GetBaseWLTypeTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string wlModel = typeof(SS_WLType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var WLList = BaseTree.GetSS_WLType(false, operatorId);
            var parentList = WLList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].WLTypeName + "(" + parentList[i].WLTypeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = parentList[i];
                temp = temp != null ? temp : null;
                dic["m"] = wlModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                dic["WLTypeName"] = CommonFuntion.StringToJson(temp.WLTypeName != null ? temp.WLTypeName.ToString() : "");
                dic["WLTypeKey"] = CommonFuntion.StringToJson(temp.WLTypeKey != null ? temp.WLTypeKey.ToString() : "");
                dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                dic["IsDC"] = CommonFuntion.StringToJson(temp.IsDC.ToString());
                dic["IsCustomer"] = CommonFuntion.StringToJson(temp.IsCustomer.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop.ToString());
                treeModel.attributes = dic;
                var childList = WLList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var wlChild = GetChildWLTypeNode(childList, WLList);
                    if (wlChild != null && wlChild.Count > 0)
                    {
                        treeModel.children = wlChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取菜单tree
        /// </summary>
        public ContentResult GetBaseMenuTree()
        {
            string mModel = typeof(SS_Menu).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var mList = BaseTree.GetMenus();
            var parentList = mList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].MenuName + "(" + parentList[i].MenuKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = parentList[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                dic["GUID_MenuClass"] = CommonFuntion.StringToJson(temp.GUID_MenuClass != null ? temp.GUID_MenuClass.ToString() : "");
                dic["MenuName"] = CommonFuntion.StringToJson(temp.MenuName != null ? temp.MenuName.ToString() : "");
                dic["MenuKey"] = CommonFuntion.StringToJson(temp.MenuKey != null ? temp.MenuKey.ToString() : "");
                dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                dic["scope"] = CommonFuntion.StringToJson(temp.Scope != null ? temp.Scope.ToString() : "");
                treeModel.attributes = dic;
                var childList = mList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var wlChild = GetChildMenuNode(childList, mList);
                    if (wlChild != null && wlChild.Count > 0)
                    {
                        treeModel.children = wlChild;
                    }
                    else
                    {
                        treeModel.state = "open";
                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取基础省份tree
        /// </summary>
        public ContentResult GetBaseProvinceTree()
        {
            string mModel = typeof(SS_Province).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = dbobj.context.SS_Province.Select(e => new { e.GUID, e.ProvinceKey, e.ProvinceName, e.IsStop }).OrderBy(e => e.ProvinceKey).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = list[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(list[i].ProvinceName + "(" + list[i].ProvinceKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = list[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["ProvinceName"] = CommonFuntion.StringToJson(temp.ProvinceName != null ? temp.ProvinceName.ToString() : "");
                dic["ProvinceKey"] = CommonFuntion.StringToJson(temp.ProvinceKey != null ? temp.ProvinceKey.ToString() : "");
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取基础银行tree
        /// </summary>
        public ContentResult GetBaseBankTree()
        {
            string mModel = typeof(SS_Bank).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = dbobj.context.SS_Bank.Select(e => new { e.GUID, e.BankKey, e.BankName, e.IsStop }).OrderBy(e => e.BankKey).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = list[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(list[i].BankName + "(" + list[i].BankKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = list[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["ProvinceName"] = CommonFuntion.StringToJson(temp.BankName != null ? temp.BankName.ToString() : "");
                dic["ProvinceKey"] = CommonFuntion.StringToJson(temp.BankKey != null ? temp.BankKey.ToString() : "");
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取银行账号tree
        /// </summary>
        public ContentResult GetBaseBankAccountTree()
        {
            string mModel = typeof(SS_BankAccount).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = dbobj.context.SS_BankAccountView.Select(e => new
            {
                e.GUID,
                e.GUID_Bank,
                e.GUID_DW,
                e.GUID_Province,
                e.BankAccountKey,
                e.BankAccountName,
                e.BankAccountNo,
                e.BankKey,
                e.BankName,
                e.ProvinceKey,
                e.ProvinceName,
                e.ChildBankName,
                e.DWKey,
                e.DWName,
                e.IsBasic,
                e.IsCash,
                e.IsGuoKu,
                e.IsStop
            }).OrderBy(e => e.BankKey).ToList();

            for (int i = 0; i < list.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = list[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(list[i].BankAccountName + "(" + list[i].BankAccountKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = list[i];
                temp = temp != null ? temp : null;
                dic["m"] = mModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["GUID_Bank"] = CommonFuntion.StringToJson(temp.GUID_Bank != null ? temp.GUID_Bank.ToString() : "");
                dic["GUID_DW"] = CommonFuntion.StringToJson(temp.GUID_DW != null ? temp.GUID_DW.ToString() : "");
                dic["GUID_Province"] = CommonFuntion.StringToJson(temp.GUID_Province != null ? temp.GUID_Province.ToString() : "");
                dic["BankAccountKey"] = CommonFuntion.StringToJson(temp.BankAccountKey != null ? temp.BankAccountKey.Trim().ToString() : "");
                dic["BankAccountName"] = CommonFuntion.StringToJson(temp.BankAccountName != null ? temp.BankAccountName.Trim().ToString() : "");
                dic["BankAccountNo"] = CommonFuntion.StringToJson(temp.BankAccountNo != null ? temp.BankAccountNo.Trim().ToString() : "");
                dic["BankKey"] = CommonFuntion.StringToJson(temp.BankKey != null ? temp.BankKey.Trim().ToString() : "");
                dic["BankName"] = CommonFuntion.StringToJson(temp.BankName != null ? temp.BankName.Trim().ToString() : "");
                dic["ProvinceKey"] = CommonFuntion.StringToJson(temp.ProvinceKey != null ? temp.ProvinceKey.Trim().ToString() : "");
                dic["ProvinceName"] = CommonFuntion.StringToJson(temp.ProvinceName != null ? temp.ProvinceName.Trim().ToString() : "");
                dic["ChildBankName"] = CommonFuntion.StringToJson(temp.ChildBankName != null ? temp.ChildBankName.Trim().ToString() : "");
                dic["DWKey"] = CommonFuntion.StringToJson(temp.DWKey != null ? temp.DWKey.Trim().ToString() : "");
                dic["DWName"] = CommonFuntion.StringToJson(temp.DWName != null ? temp.DWName.Trim().ToString() : "");
                dic["IsBasic"] = CommonFuntion.StringToJson(temp.IsBasic != null ? temp.IsBasic.ToString() : "");
                dic["IsCash"] = CommonFuntion.StringToJson(temp.IsCash != null ? temp.IsCash.ToString() : "");
                dic["IsGuoKu"] = CommonFuntion.StringToJson(temp.IsGuoKu != null ? temp.IsGuoKu.ToString() : "");
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 基础交通工具


        /// </summary>
        /// <returns></returns>
        public ContentResult GetBaseTrafficTree()
        {
            StringBuilder sb = new StringBuilder();
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            List<SS_TrafficView> list = dbobj.GetTraffic(false, operatorId);
            var str = GetTrafficJson(list);
            return Content(str);
        }
        /// <summary>
        /// 基础补助项目
        /// </summary>
        /// <returns></returns>
        public ContentResult GetBaseAllowanceTree()
        {
            List<SS_Allowance> list = dbobj.GetAllowance();
            var str = GetAllowanceJson(list);
            return Content(str);

        }
        /// <summary>
        /// 基础----单据流程配置--tree
        /// </summary>
        /// <returns></returns>
        public JsonResult GetBaseWorkFlowTree()
        {
            var list = Platform.Flow.Run.WorkFlowAPI.GetWorkFlowInfo();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 获取基础收入类型tree
        /// </summary>
        public ContentResult GetBaseSrTypeTree()
        {
            if (this.CurrentUserInfo.UserGuid == null) return null;
            string operatorId = this.CurrentUserInfo.UserGuid.ToString();
            string funClassmodel = typeof(SS_SRType).Name;
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var funClassList = BaseTree.GetSRType(false, operatorId);
            var parentList = funClassList.FindAll(e => e.PGUID == null);
            for (int i = 0; i < parentList.Count; i++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = parentList[i].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(parentList[i].SRTypeName + "(" + parentList[i].SRTypeKey + ")");
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var temp = parentList[i];
                temp = temp != null ? temp : null;
                dic["m"] = funClassmodel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(temp.GUID != null ? temp.GUID.ToString() : "");
                dic["PGUID"] = CommonFuntion.StringToJson(temp.PGUID != null ? temp.PGUID.ToString() : "");
                dic["SRTypeName"] = CommonFuntion.StringToJson(temp.SRTypeName != null ? temp.SRTypeName.ToString() : "");
                dic["SRTypeKey"] = CommonFuntion.StringToJson(temp.SRTypeKey != null ? temp.SRTypeKey.ToString() : "");
                dic["PKey"] = CommonFuntion.StringToJson(temp.PKey != null ? temp.PKey.ToString() : "");
                dic["PName"] = CommonFuntion.StringToJson(temp.PName != null ? temp.PName.ToString() : "");
                dic["IsStop"] = CommonFuntion.StringToJson(temp.IsStop != null ? temp.IsStop.ToString() : "");
                treeModel.attributes = dic;
                var childList = funClassList.FindAll(e => e.PGUID == parentList[i].GUID);
                if (childList != null && childList.Count > 0)
                {
                    treeModel.state = "open";
                    var funClassChild = GetChildSRTypeNode(childList, funClassList);
                    if (funClassChild != null && funClassChild.Count > 0)
                    {
                        treeModel.children = funClassChild;
                    }
                    else
                    {
                        treeModel.state = "open";

                    }
                }
                else
                {
                    treeModel.state = "open";
                }
                treeModelList.Add(treeModel);
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        /// <summary>
        /// 获取基础结算方式tree
        /// </summary>
        public ContentResult GetBaseSettleType()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getBaseSettleType();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetBaseSettleTypeNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        private List<TreeNodeModel> GetBaseSettleTypeNode(List<SS_SettleType> SettleTypNodeModelList)
        {
            string ItemModel = typeof(SS_SettleType).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < SettleTypNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = SettleTypNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(SettleTypNodeModelList[j].SettleTypeName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].GUID.ToString());
                dic["SettleTypeKey"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].SettleTypeKey.ToString());
                dic["SettleTypeName"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].SettleTypeName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].IsStop.ToString());
                dic["IsCash"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].IsCash.ToString());
                dic["IsCheck"] = CommonFuntion.StringToJson(SettleTypNodeModelList[j].IsCheck.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }
        /// <summary>
        /// 获取基础收款类型tree
        /// </summary>
        public ContentResult GetBaseSKType()
        {
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            string jsonStr = string.Empty;
            var list = BaseTree.getBaseSKType();
            if (list != null && list.Count > 0)
            {
                var nodeList = GetBaseSKTypeNode(list);
                if (nodeList != null && nodeList.Count > 0)
                {
                    treeModelList = nodeList;
                }
            }
            jsonStr = BaothApp.Utils.JsonHelper.objectToJson(treeModelList);
            return Content(jsonStr);
        }
        private List<TreeNodeModel> GetBaseSKTypeNode(List<SS_SKTypeView> SKTypNodeModelList)
        {
            string ItemModel = typeof(SS_SKType).Name;
            StringBuilder sb = new StringBuilder();
            List<TreeNodeModel> treeModelList = new List<TreeNodeModel>();
            for (int j = 0; j < SKTypNodeModelList.Count; j++)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.id = SKTypNodeModelList[j].GUID.ToString();
                treeModel.text = CommonFuntion.StringToJson(SKTypNodeModelList[j].SKTypeName);
                treeModel.state = "open";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic["m"] = ItemModel;
                dic["valid"] = "1";
                dic["GUID"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].GUID.ToString());
                dic["GUID_SRWLType"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].GUID_SRWLType.ToString());
                dic["SKTypeKey"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].SKTypeKey.ToString());
                dic["SKTypeName"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].SKTypeName.ToString());
                dic["WLTypeKey"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].WLTypeKey == null ? "" : SKTypNodeModelList[j].WLTypeKey.ToString());
                dic["WLTypeName"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].WLTypeName == null ? "" : SKTypNodeModelList[j].WLTypeName.ToString());
                dic["SRTypeKey"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].SRTypeKey == null ? "" : SKTypNodeModelList[j].SRTypeKey.ToString());
                dic["SRTypeName"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].SRTypeName == null ? "" : SKTypNodeModelList[j].SRTypeName.ToString());
                dic["IsStop"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].IsStop.ToString());
                dic["IsDefault"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].IsDefault.ToString());
                dic["IsWLType"] = CommonFuntion.StringToJson(SKTypNodeModelList[j].IsWLType.ToString());
                treeModel.attributes = dic;
                treeModelList.Add(treeModel);
            }
            return treeModelList;
        }

        #endregion

        public ContentResult GetSS_FunctionClassTreeCheck() 
        {
            var listFun =BaseTree.GetFunctionClassWithProject();
            var keys = Request["keys"];

            List<TreeNodeModel> list = new List<TreeNodeModel>();
            TreeNodeModel RootNode = new TreeNodeModel();
            RootNode.state = "open";
            RootNode.id = string.Empty;
            if (string.IsNullOrEmpty(keys))
            {
                RootNode.@checked = true;
            }
            else
            {
                RootNode.@checked = false;
            }
            RootNode.text = "全部";
            
            foreach (var item in listFun)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = item.GUID.ToString();
                if (string.IsNullOrEmpty(keys))
                {
                    treeModel.@checked = true;
                }
                else {
                    if (keys.Contains(treeModel.id))
                    {
                        treeModel.@checked = true;
                    }
                    else {
                        treeModel.@checked = false; 
                    }
                }
               
                treeModel.text =item.FunctionClassName;
                list.Add(treeModel);
            }
            RootNode.children = list;
            List<TreeNodeModel> results = new List<TreeNodeModel> { RootNode };
            string strJson = BaothApp.Utils.JsonHelper.objectToJson(results);
            return Content(strJson);
        }

        public ContentResult GetSS_FunctionClassTreeCheckJB()
        {
            var listFun = BaseTree.GetFunctionClass(false,"");
            var keys = Request["keys"];

            List<TreeNodeModel> list = new List<TreeNodeModel>();
            TreeNodeModel RootNode = new TreeNodeModel();
            RootNode.state = "open";
            RootNode.id = string.Empty;
            if (string.IsNullOrEmpty(keys))
            {
                RootNode.@checked = true;
            }
            else
            {
                RootNode.@checked = false;
            }
            RootNode.text = "全部";

            foreach (var item in listFun)
            {
                TreeNodeModel treeModel = new TreeNodeModel();
                treeModel.state = "open";
                treeModel.id = item.GUID.ToString();
                if (string.IsNullOrEmpty(keys))
                {
                    treeModel.@checked = true;
                }
                else
                {
                    if (keys.Contains(treeModel.id))
                    {
                        treeModel.@checked = true;
                    }
                    else
                    {
                        treeModel.@checked = false;
                    }
                }

                treeModel.text = item.FunctionClassName;
                list.Add(treeModel);
            }
            RootNode.children = list;
            List<TreeNodeModel> results = new List<TreeNodeModel> { RootNode };
            string strJson = BaothApp.Utils.JsonHelper.objectToJson(results);
            return Content(strJson);
        }

    }
    /// <summary>
    /// 树节点模型
    /// </summary>
    public class TreeNodeModel
    {
        public string id { set; get; }
        public string text { set; get; }
        public string Departkey { get; set; }
        public string state { set; get; }
        public bool isCheck { set; get; }//checked
        public bool @checked { get; set; }
        public object attributes { set; get; }
        public List<TreeNodeModel> children { get; set; }
    }


}
