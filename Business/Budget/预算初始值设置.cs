using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Platform.Flow.Run;
using Infrastructure;
using Business.CommonModule;
using System.Data;
using System.Text.RegularExpressions;
using BusinessModel;
using Business.DocTrans;
using Platform.Flow.Run;
namespace Business.Budget
{
    public class 预算初始值设置 : BaseDocument
    {
        private Guid defaultGUID_YWType;
        private Guid defaultGUID_DocType;
        private Guid defaultGUID_UIType;
        public 预算初始值设置()
            : base()
        {
            defaultGUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
            defaultGUID_DocType = new Guid("471DEAB3-AC63-43A9-9041-B43BF912FA26");
            defaultGUID_UIType = new Guid("B2639101-7D4F-47B2-8ADF-AB24694E1828");
        }
        public 预算初始值设置(Guid OperatorId, string ModelUrl)
            : base(OperatorId, ModelUrl)
        {
            defaultGUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
            defaultGUID_DocType = new Guid("471DEAB3-AC63-43A9-9041-B43BF912FA26");
            defaultGUID_UIType = new Guid("B2639101-7D4F-47B2-8ADF-AB24694E1828");
        }

        /// <summary>
        /// 创建默认值


        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                BG_DefaultMainView model = new BG_DefaultMainView();
                model.FillDefault(this, this.OperatorId);
                InitBG_DefalutMain(ref model);
                jmodel.m = model.Pick();
                BG_DefaultDetailView dModel = new BG_DefaultDetailView();
                dModel.FillDetailDefault<BG_DefaultDetailView>(this, this.OperatorId);
                string strTime = DateTime.Now.ToString();
                string strBGYear = strTime.Substring(0, 4);
                dModel.BGYear = Int32.Parse(strBGYear) + 1;

                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                List<JsonAttributeModel> picker = dModel.Pick();
                fjgm.r.Add(picker);

                jmodel.f.Add(fjgm);

                // 加入BG_DefaultDetail的 BGYear
                JsonAttributeModel jam = new JsonAttributeModel();
                jam.m = "BG_DefaultDetail";
                jam.n = "BGYear";
                jam.v = (Int32.Parse(strBGYear) + 1).ToString();
                jmodel.m.Add(jam);

                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public JsonModel New(string strGuid,string strScope)
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                Guid Id;
                Guid.TryParse(strGuid,out Id);
                var bgassignModel = this.BusinessContext.BG_AssignView.FirstOrDefault(e=>e.GUID==Id);
                if (bgassignModel == null) {
                    return jmodel;
                }
                BGAssignToBGDefault objTransfer = new BGAssignToBGDefault(Id);
                BG_DefaultMainView objDefaultMain = objTransfer.DocTransferBGDefault(this.BusinessContext, this.InfrastructureContext, this.OperatorId,bgassignModel);
                jmodel.m = objDefaultMain.Pick();

                // 加入BG_Detail的 BGYear
                JsonAttributeModel jam = new JsonAttributeModel();
                jam.m = "BG_DefaultDetail";
                jam.n = "BGYear";
                //string strTime = DateTime.Now.ToString();
                //string strBGYear = strTime.Substring(0, 4);
                jam.v = bgassignModel.BGYear.ToString(); //(Int32.Parse(strBGYear) + 1).ToString();
                jmodel.m.Add(jam);
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
        }

        /*
         *      函数功能:   为BG_Main 的属性赋默认值
         *       author:    dongsheng.zhang
         *         日期:    2014-4-14
         * 
         */

        private void InitBG_DefalutMain(ref BG_DefaultMainView viewDefaultMain)
        {
            // 设置预算设置 默认值为 内部项目支出 
            BG_Setup bg_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == "08");
            viewDefaultMain.GUID_BGSetup = bg_Setup.GUID;
            viewDefaultMain.BGSetupKey = bg_Setup.BGSetupKey;
            viewDefaultMain.BGSetupName = bg_Setup.BGSetupName;
            // 设置货币单位 默认值为 千元  这个是不是在页面设置初始值会更好
            var ss_MoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == bg_Setup.GUID_MoneyUnit);
            if (null == ss_MoneyUnit)
            {
                return;
            }
            viewDefaultMain.GUID_MoneyUnit = ss_MoneyUnit.GUID;
            viewDefaultMain.MoneyUnitKey = ss_MoneyUnit.MoneyUnitKey;
            viewDefaultMain.MoneyUnitName = ss_MoneyUnit.MoneyUnitName;

            // 设置单据state BGPeriod  Invalid verson
            viewDefaultMain.DocState = 0;
            viewDefaultMain.BGPeriod = 0;
            viewDefaultMain.DocVerson = "1";

            // 设置金额
            viewDefaultMain.Total_BG = 0;
            viewDefaultMain.Total_BG_CurYear = 0;
            viewDefaultMain.Total_BG_PreYear = 0;
        }

        private void DeleteLineBreak(ref string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                str = string.Empty;
                return;
            }
            
            str = str.Replace("\r\n", "");
            str = str.Replace("\r", "");
            str = str.Replace("\n", "");
            str = str.Replace("\b", "");
            str = str.Replace("\f", "");
            str = str.Replace("\t", "");
            str = str.Replace("\'", "\\'");
            str = str.Replace("\"", "\\\"");
            str = str.Replace("\\", "\\\\");
        }
        private string GetGUIDLink(string strFormula)
        {
            string strGUIDLink = string.Empty;
            char[] chArray = strFormula.ToCharArray();
            int iLen = chArray.Length;
            List<char> chList = new List<char>();
            char[] newArray = new char[500];
            bool bAdd = false;
            int j = 0;
            for (int i = 0; i < iLen; i++)
            {
                char ch = chArray[i];
                if ('{' == ch)
                {
                    bAdd = true;
                }
                if ('}' == ch)
                {
                    newArray[j++] = ch;
                    newArray[j++] = ',';
                    bAdd = false;
                }
                if (bAdd)
                {
                    newArray[j++] = ch;
                }
            }
            return new string(newArray, 0, j - 1);
        }

        public string GetBG_DetailData(string strBG_MainGUID, string strBG_SetupGUID)
        {
            // 获得预算设置所关联的BG_Item
            Guid guidBG_SetupGUID = new Guid(strBG_SetupGUID);
            Guid BG_Main_GUID = new Guid(strBG_MainGUID);
            BG_DefaultMainView viewBG_Main = this.BusinessContext.BG_DefaultMainView.FirstOrDefault(e => e.GUID == BG_Main_GUID);
            List<BusinessModel.BG_SetupDetailView> listBG_SetupDetail = new List<BusinessModel.BG_SetupDetailView>();
            List<BG_DefaultDetailView> ListDetail = new List<BG_DefaultDetailView>();
            // 如果主表不存在，则按新增设置表头，如果存在，则根据实际存储的数据设置表头
            if (viewBG_Main == null)
            {
                IQueryable<BusinessModel.BG_SetupDetailView> viewBG_SetupDetail = this.BusinessContext.BG_SetupDetailView.Where(e => e.GUID_BGSetup == guidBG_SetupGUID).OrderBy(e => e.ItemOrder);
                if (null == viewBG_SetupDetail)
                {
                    return string.Empty;
                }

                listBG_SetupDetail = viewBG_SetupDetail.ToList();
            }
            else
            {
                IQueryable<BG_DefaultDetailView> Details = this.BusinessContext.BG_DefaultDetailView.Where(e => e.GUID_BG_Main == BG_Main_GUID);
                if (null == Details)
                {
                    return string.Empty;
                }

                ListDetail = Details.ToList();
                if (ListDetail.Count == 0)
                {
                    return string.Empty;
                }
                GetSortBG_SetupDetailView(ref ListDetail, ref listBG_SetupDetail, guidBG_SetupGUID);
            }

            if (listBG_SetupDetail.Count <= 0)
            {
                return string.Empty;
            }

            // 循环获取对象，拼接列   每行有多个BGItem，因此要区分列的名字    strColumns 存储的是预算编制表的列          
            string strColumns = string.Empty;
            string strShareColumn = "NO";
            strColumns = "{width:100,field:'RateNum',title:'RateNum',hidden:'true'},{width:100,field:'Sum',title:'Sum',hidden:'true'}," +
                "{width:100,field:'Add',title:'Add',hidden:'true'},{width:100,field:'GUID_BGCode',title:'GUID_BGCode',hidden:'true'}," +
                "{field:'BGCodeName',title:'预算科目',width:100,halign:'center',align: 'left'},";

            foreach (BusinessModel.BG_SetupDetailView item in listBG_SetupDetail)
            {
                // 按顺序存储bgitem
                string strBGMemo = (null == item.BGItemMemo ? null : item.BGItemMemo.Trim());

                // BGItem的 GUID作为field  BGItemName作为title, 两个计算公式放在editor的options中


                string strFieldBGItemGUID = item.GUID_Item.ToString();
                string strFormula = item.ItemFormula;
                string strDefaultFormula = item.ItemDefaultFormula;
                string strFormulaGUIDLink = "NO";
                string strDefaultFromulaGUIDLink = "NO";
                DeleteLineBreak(ref strFormula);
                DeleteLineBreak(ref strDefaultFormula);
                if (strFormula == null || strFormula == string.Empty)
                {
                    strFormula = "NO";
                }
                else
                {
                    strFormulaGUIDLink = GetGUIDLink(strFormula);
                }

                if (null == strDefaultFormula || strDefaultFormula == string.Empty)
                {
                    strDefaultFormula = "NO";
                }
                else
                {
                    strDefaultFromulaGUIDLink = GetGUIDLink(strDefaultFormula);
                }

                // 
                if(item.BGItemKey == "06")
                {
                    strShareColumn = strFieldBGItemGUID.ToLower();
                }
                string strDetailGUID = "DetailGUID" + item.BGItemKey;
                string strColumn = "{width:100,field:'" + strDetailGUID + "',title:'DetailGUID',hidden:'true'},{width:100,field:'" + strFieldBGItemGUID + "',title:'" + item.BGItemName +
                    "',halign:'center',align:'right',editor:{type:'numberbox',options:{precision:2,\\\"ItemFormula\\\":\\\"" + strFormula +
                    "\\\",\\\"ItemDefaulFormula\\\":\\\"" + strDefaultFormula + "\\\",\\\"FormulaLink\\\":\\\"" + strFormulaGUIDLink +
                    "\\\",\\\"DefaultFormulaLink\\\":\\\"" + strDefaultFromulaGUIDLink + "\\\"} },styler: function(value,row,index){return 'color:blue';}},";

                // 一个Item可能有BGMemo，这一列不是固定的，因此要根据实际情况加载
                if (null != strBGMemo && strBGMemo != string.Empty)
                {
                    string[] strSplitArray;
                    strSplitArray = strBGMemo.Split(',');
                    string strMemoName = strSplitArray[0];
                    string strMemoKey = strSplitArray[1];
                    strMemoKey = strMemoKey + "_" + item.BGItemKey;
                    strColumn += "{field:'" + strMemoKey + "',title: '" + strMemoName + "',width:100,halign:'center',align:'left',editor:{type:'text'}},";
                }
                strColumns = strColumns + strColumn;

            }
            // 去掉最后面的逗号
            strColumns = strColumns.Substring(0, strColumns.Length - 1);
            strColumns = "[[" + strColumns + "]]";
            strColumns = "\"column\":" + "\"" + strColumns + "\"";

            string strData = GetBG_DetailDataEx(strBG_SetupGUID, strBG_MainGUID, listBG_SetupDetail);
            if (string.Empty == strData)
            {
                return string.Empty;
            }

             //查找这个预算设置 获取默认显示的货币单位
            string strMultiple = "";
            BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.GUID == guidBG_SetupGUID);
            if(null!=objBG_Setup)
            {
                Infrastructure.SS_MoneyUnit objSS_MoneyUnit = this.InfrastructureContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == objBG_Setup.GUID_MoneyUnit);
                if (null != objSS_MoneyUnit)
                {
                    strMultiple = objSS_MoneyUnit.UnitMultiple.ToString();
                }
            }
            string strJson = "{\"success\": true, " + strColumns + ",\"data\":\"" + strData + "\",\"strShareColumn\":\"" +
                strShareColumn + "\",\"Multiple\":\"" + strMultiple + "\"}";
            return strJson;
        }

        private string GetShowBGCodeName(int iLevel, string strBGCodeName)
        {
            string strShow = strBGCodeName;
            string strBlank = "&nbsp&nbsp";
            for (int i = 0; i < iLevel; i++)
            {
                strShow = strBlank + strShow;
            }
            return strShow;
        }

        private void AnalyseBGCode(Guid parent, int iParentLine, ref List<BG_SetupBGCodeView> listBG_SetupBGCode, ref List<BG_SetupBGCodeView> listSortBG_SetupBGCode,
            ref Dictionary<Guid, string> dicSumFormula, ref Dictionary<Guid, string> dicAddFormula, int iLevel, ref Dictionary<Guid, int> dicLevel)
        {
            Guid? parentGUID = null;
            if (parent != Guid.Empty)
            {
                parentGUID = parent;
            }

            var tmpList = listBG_SetupBGCode.FindAll(e => e.PSS_BGCodeGUID == parentGUID).OrderBy(e => e.BGCodeKey);

            if (0 == tmpList.Count())
            {
                if (Guid.Empty == parent)
                {
                    return;
                }
                dicSumFormula.Add(parent, "NO");             // 已经没有子级科目了，设置为NO
                return;
            }

            string strSum = string.Empty;
            foreach (BG_SetupBGCodeView item in tmpList)
            {
                int iLine = listSortBG_SetupBGCode.Count;                   // 这是item所在的行号
                if (strSum == string.Empty)
                {
                    strSum = iLine.ToString();
                }
                else
                {
                    strSum = strSum + "," + iLine.ToString();
                }
                dicLevel.Add(item.GUID_BGCode, iLevel);
                listSortBG_SetupBGCode.Add(item);                           // 顺序存储
                if (null != parentGUID)
                {
                    dicAddFormula.Add(item.GUID_BGCode, iParentLine.ToString());      // 这个科目所对应的父级科目所在行号已经传入


                }
                else
                {
                    dicAddFormula.Add(item.GUID_BGCode, "NO");                  //如果没有父级科目，那么设置为NO
                }
                // 分析自己的子级科目


                AnalyseBGCode(item.GUID_BGCode, iLine, ref listBG_SetupBGCode, ref listSortBG_SetupBGCode, ref dicSumFormula, ref dicAddFormula, iLevel + 1, ref dicLevel);
            }

            if (null != parentGUID)
            {
                dicSumFormula.Add(parent, strSum);
            }
        }

        private void GetSortBG_SetupBGCodeView(ref List<BG_DefaultDetailView> listBG_DetailView, ref List<BG_SetupBGCodeView> listBG_SetupBGCodeView)
        {
            HashSet<string> hsBGCodeKey = new HashSet<string>();
            HashSet<string> hsGuid = new HashSet<string>();
            foreach (BG_DefaultDetailView item in listBG_DetailView)
            {
                if (!hsBGCodeKey.Contains(item.BGCodeKey))
                {
                    hsBGCodeKey.Add(item.BGCodeKey);
                    BG_SetupBGCodeView objBGCode = new BG_SetupBGCodeView();
                    objBGCode.GUID_BGCode = item.GUID_BGCode;
                    objBGCode.BGCodeKey = item.BGCodeKey;
                    objBGCode.BGCodeName = item.BGCodeName;
                    objBGCode.PSS_BGCodeGUID = item.PGUID;
                    listBG_SetupBGCodeView.Add(objBGCode);

                    Guid? tmp = objBGCode.PSS_BGCodeGUID;
                    while (null != tmp && !hsGuid.Contains(tmp.ToString()))
                    {
                        SS_BGCode objTmpBGCode = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e => e.GUID == tmp);
                        if (null != objTmpBGCode && !hsBGCodeKey.Contains(objTmpBGCode.BGCodeKey))
                        {
                            hsBGCodeKey.Add(objTmpBGCode.BGCodeKey);
                            hsGuid.Add(objTmpBGCode.GUID.ToString());
                            BG_SetupBGCodeView obj = new BG_SetupBGCodeView();
                            obj.GUID_BGCode = objTmpBGCode.GUID;
                            obj.BGCodeKey = objTmpBGCode.BGCodeKey;
                            obj.BGCodeName = objTmpBGCode.BGCodeName;
                            obj.PSS_BGCodeGUID = objTmpBGCode.PGUID;
                            listBG_SetupBGCodeView.Add(obj);
                            tmp = obj.PSS_BGCodeGUID;
                        }
                        else
                        {
                            tmp = null;
                        }

                    }
                }
            }
            listBG_SetupBGCodeView = listBG_SetupBGCodeView.OrderBy(e => e.BGCodeKey).ToList();
        }
        private void GetSortBG_SetupDetailView(ref List<BG_DefaultDetailView> listBG_DetailView, ref List<BusinessModel.BG_SetupDetailView> listBG_SDView, Guid guidBG_Setup)
        {
            HashSet<string> hsBGCodeKey = new HashSet<string>();
            foreach (BG_DefaultDetailView item in listBG_DetailView)
            {
                if (!hsBGCodeKey.Contains(item.BGItemKey))
                {
                    hsBGCodeKey.Add(item.BGItemKey);
                    BusinessModel.BG_SetupDetailView objBG_SetupDetail = this.BusinessContext.BG_SetupDetailView.FirstOrDefault(e => e.GUID_Item == item.GUID_Item && guidBG_Setup == e.GUID_BGSetup);
                    if (null != objBG_SetupDetail)
                    {
                        listBG_SDView.Add(objBG_SetupDetail);
                    }
                }
            }
            listBG_SDView = listBG_SDView.OrderBy(e => e.ItemOrder).ToList();
        }


        private string GetBG_DetailDataEx(string strBG_SetupGUID, string strBG_MainGUID, List<BusinessModel.BG_SetupDetailView> listBG_SDViewEx)
        {
            string strData = string.Empty;
            Guid guidBG_SetupGUID = new Guid(strBG_SetupGUID);
            Guid BG_Main_GUID = new Guid(strBG_MainGUID);
            BG_DefaultMainView viewBG_Main = this.BusinessContext.BG_DefaultMainView.FirstOrDefault(e => e.GUID == BG_Main_GUID);
            IQueryable<BG_DefaultDetailView> Details = this.BusinessContext.BG_DefaultDetailView.Where(e => e.GUID_BG_Main == BG_Main_GUID);
            List<BG_DefaultDetailView> ListDetail = new List<BG_DefaultDetailView>();
            List<BG_SetupBGCodeView> listBG_SetupBGCode = new List<BG_SetupBGCodeView>();
            List<BusinessModel.BG_SetupDetailView> listBG_SDView = new List<BusinessModel.BG_SetupDetailView>();
            IQueryable<BG_SetupBGCodeView> viewBG_SetupBGCode = this.BusinessContext.BG_SetupBGCodeView.Where(e => e.GUID_BGSetup == guidBG_SetupGUID && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey);
            if (null == viewBG_SetupBGCode)
            {
                return strData;
            }
            var tempBGSetupBGCode = viewBG_SetupBGCode.ToList();
            if (null != viewBG_Main)
            {
                if(null == Details)
                {
                    return string.Empty;
                }

                ListDetail = Details.ToList();
                if (ListDetail.Count==0)
                {
                    return string.Empty;
                }

                GetSortBG_SetupBGCodeView(ref ListDetail, ref listBG_SetupBGCode);
                GetSortBG_SetupDetailView(ref ListDetail, ref listBG_SDView, guidBG_SetupGUID);
            }
            else
            {

                
                // 获得到BG_Setup所关联的BG_Code
                listBG_SetupBGCode = tempBGSetupBGCode;
                listBG_SDView = listBG_SDViewEx;
                if (listBG_SetupBGCode.Count == 0)
                {
                    return string.Empty;
                }
            }
            

            List<BG_SetupBGCodeView> listSortBG_SetupBGCode = new List<BG_SetupBGCodeView>();
            Dictionary<Guid, string> dicSumFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, string> dicAddFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, int> dicLevel = new Dictionary<Guid, int>();

            AnalyseBGCode(Guid.Empty, 0, ref listBG_SetupBGCode, ref listSortBG_SetupBGCode, ref dicSumFormula, ref dicAddFormula, 0, ref dicLevel);



            string strTotal = string.Empty;
            string strRows = string.Empty;
            int iRowCount = listSortBG_SetupBGCode.Count;
            string strSumFormula = string.Empty;        // 需要进行汇总的行号
            string strAddFormula = string.Empty;        // 父级所在行号
            string strRow = string.Empty;               // 一行数据
            string strBgCodeName = string.Empty;        // 界面上显示科目
            string strBGItem = string.Empty;
            int iAddTotal = 0;                          // 最后需要汇总到合计的行号，也就是最高级的科目所在行号


            foreach (BG_SetupBGCodeView item in listSortBG_SetupBGCode)
            {
                if (!dicSumFormula.ContainsKey(item.GUID_BGCode) || !dicAddFormula.ContainsKey(item.GUID_BGCode) || !dicLevel.ContainsKey(item.GUID_BGCode))
                {
                    return string.Empty;
                }

                strSumFormula = dicSumFormula[item.GUID_BGCode];
                strAddFormula = dicAddFormula[item.GUID_BGCode];

                if (strAddFormula == "NO")
                {
                    if (strTotal == string.Empty)
                    {
                        strTotal = iAddTotal.ToString();
                    }
                    else
                    {
                        strTotal = strTotal + "," + iAddTotal.ToString();
                    }
                    strAddFormula = iRowCount.ToString();
                }
                iAddTotal++;
                int iLevel = dicLevel[item.GUID_BGCode];
                strBgCodeName = GetShowBGCodeName(iLevel, item.BGCodeName);
                double? mRateNum;
                if (viewBG_Main != null)
                {
                    var tempitem = tempBGSetupBGCode.Find(e => e.GUID_BGCode == item.GUID_BGCode && e.GUID_BGSetup == viewBG_Main.GUID_BGSetup);
                    mRateNum = tempitem == null ? null : tempitem.RateNum;
                }
                else
                {
                    mRateNum = item.RateNum;
                }
                string strRateNum = mRateNum == null ? "0" : mRateNum.ToString();
                strRow = "{\\\"RateNum\\\":\\\"" + strRateNum + "\\\",\\\"Sum\\\":\\\"" + strSumFormula + "\\\",\\\"Add\\\":\\\"" + strAddFormula + "\\\",\\\"GUID_BGCode\\\":\\\"" + item.GUID_BGCode + "\\\",\\\"BGCodeName\\\":\\\"" + strBgCodeName + "\\\"";
                strBGItem = string.Empty;
                BusinessModel.SS_MoneyUnit objMoneyUnit = null;
                if (null != viewBG_Main)
                {
                    objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == viewBG_Main.GUID_MoneyUnit);
                }

                foreach (BusinessModel.BG_SetupDetailView tmp in listBG_SDView)
                {
                    var BG_Detail = ListDetail.Find(e => e.GUID_BGCode == item.GUID_BGCode && e.GUID_Item == tmp.GUID_Item);
                    string strTotalBG = "";
                    string strBGMemoShow = "";
                    string strGUID_BG_Detail = "";
                    // 预算编制表存在，detail存在，且当前的预算设置和已经存储的预算设置一致，如果在修改的时候，修改了预算设置。。。。。。


                    if (null != viewBG_Main && null != BG_Detail && guidBG_SetupGUID == viewBG_Main.GUID_BGSetup)
                    {
                        double dblTotal = (double)BG_Detail.Total_BG;
                        strTotalBG = string.Format("{0:F2}", dblTotal);
                        strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        strBGMemoShow = BG_Detail.BGMemo == null ? "" : BG_Detail.BGMemo;
                        strGUID_BG_Detail = BG_Detail.GUID.ToString();
                    }
                    string strTmp = ",\\\"" + "DetailGUID" + tmp.BGItemKey + "\\\":\\\"" + strGUID_BG_Detail + "\\\",\\\"" + tmp.GUID_Item.ToString() + "\\\":\\\"" + strTotalBG + "\\\"";
                    string strBGMemo = tmp.BGItemMemo;
                    if (null != strBGMemo && strBGMemo != string.Empty)
                    {
                        string strMemoKey = "BGMemo" + "_" + tmp.BGItemKey;
                        DeleteLineBreak(ref strBGMemoShow);
                        strTmp = strTmp + ",\\\"" + strMemoKey + "\\\":\\\"" + strBGMemoShow + "\\\"";
                    }
                    strBGItem += strTmp;
                }

                strRow = strRow + strBGItem + "}";
                if (strRows == string.Empty)
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," + strRow;
                }
            }

            // 合计 是最后一行，最后一行的数据并不存储，因此在前台进行统计即可                
            foreach (BusinessModel.BG_SetupDetailView tmp in listBG_SDView)
            {
                string strTmp = ",\\\"" + "DetailGUID" + tmp.BGItemKey + "\\\":\\\"\\\",\\\"" + tmp.GUID_Item.ToString() + "\\\":\\\"\\\"";
                string strBGMemo = tmp.BGItemMemo;
                if (null != strBGMemo && strBGMemo != string.Empty)
                {
                    string strMemoKey = "BGMemo" + "_" + tmp.BGItemKey;
                    strTmp = strTmp + ",\\\"" + strMemoKey + "\\\":\\\"\\\"";
                }
                strBGItem += strTmp;
            }

            strRow = "{\\\"RateNum\\\":\\\"0\\\",\\\"Sum\\\":\\\"" + strTotal + "\\\",\\\"Add\\\":\\\"NO\\\",\\\"GUID_BGCode\\\":\\\"\\\",\\\"BGCodeName\\\":\\\"合计\\\"";
            //strBGItem = string.Empty;
            strRow = strRow + strBGItem + "}";
            strRows = strRows + "," + strRow;
            string strCount = (iRowCount + 1).ToString();
            strData = "{\\\"total\\\":" + strCount + ",\\\"rows\\\":[" + strRows + "]}";

            return strData;
        }
        /*
 *      函数功能:  将一个json对象解析成一个list，list中的元素记录一个键值对。实例：{"name":"dongsheng","city":"fog"}
 *                 解析后，生成两个ValuePair 
 *       author:   dongsheng.zhang
 *         日期:    2014-4-21
 */

        private List<ValuePair> SplitJsonObject(string strJson)
        {
            // 先去掉两边的大括号


            string strPairs = strJson.Substring(1, strJson.Length - 2);
            string[] strArray = Regex.Split(strPairs, "\",\"", RegexOptions.None);
            List<ValuePair> ListReturn = new List<ValuePair>();

            int iLen = strArray.Length;
            for (int i = 0; i < iLen; i++)
            {
                string strTmp = strArray[i];
                string[] strTmpArray = Regex.Split(strTmp, "\":\"", RegexOptions.None);
                string strKey = strTmpArray[0];
                string strValue = strTmpArray[1];
                if (0 == i)    //第一个要去左面的 "
                {
                    strKey = strKey.Substring(1, strKey.Length - 1);
                }
                else if (iLen - 1 == i)  //最后一个要去右面的 "
                {
                    strValue = strValue.Substring(0, strValue.Length - 1);
                }

                ValuePair vp = new ValuePair(strKey, strValue);
                ListReturn.Add(vp);
            }

            return ListReturn;
        }

        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BG_DefaultMain bxMain = new BG_DefaultMain();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            else
            {
                object g;
                if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                {
                    str = "此单GUID格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }

            }
            //流程验证
            if (WorkFlowAPI.ExistProcess(guid))
            {
                str = "此单正在流程审核中！不能删除！";
                resultList.Add(new ValidationResult("", str));
            }
            //作废的不能删除



            BG_DefaultMain main = this.BusinessContext.BG_DefaultMain.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == int.Parse("9") || main.DocState == int.Parse("999"))
                {
                    str = "此单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            return result;
        }
        /// <summary>
        /// 验证信息
        /// </summary>
        /// <param name="vResult"></param>
        /// <returns></returns>
        private string DataVerifyMessage(VerifyResult vResult)
        {
            string strMsg = string.Empty;
            if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
            {
                for (int i = 0; i < vResult.Validation.Count; i++)
                {
                    strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                }
            }
            return strMsg;
        }
        /*
         *      函数功能:   将json对象数组解析分割，实例：[{"test":"1"},{"test":"2"}]  解析分割后得到两个字符串{"test":"1"} 和 {"test":"2"}
         *       author:   dongsheng.zhang
         *         日期:    2014-4-21
         *      函数bug：   函数先找到   },{   所在位置，然后进行分割，如果一个键值对里的单个值本身就包含这个字符串，那么该函数将返回一个错误的结果
         *                 考虑到目前的应用场景不会出现这个问题，该问题暂时不解决，待我想到比较好的算法后解决
         */

        private List<string> SplitJsonArray(string strJsonArray)
        {
            List<string> ListReturn = new List<string>();
            string strJsonObjects = strJsonArray.Substring(1, strJsonArray.Length - 2);
            List<int> ListIndex = new List<int>();

            // 找到分割的位置


            int index = strJsonObjects.IndexOf("},{", 0);
            while (-1 != index)
            {
                ListIndex.Add(index + 1);   // 注意，真实的分割位置是逗号
                index = strJsonObjects.IndexOf("},{", index + 3);
            }

            int iCount = ListIndex.Count;
            int iStart = 0;
            string strJsonObject = string.Empty;
            for (int i = 0; i < iCount; i++)
            {
                int iEnd = ListIndex[i] - 1;
                strJsonObject = strJsonObjects.Substring(iStart, iEnd - iStart + 1);
                ListReturn.Add(strJsonObject);
                iStart = iEnd + 2;
            }

            // 不要忘记最后一个


            strJsonObject = strJsonObjects.Substring(iStart, strJsonObjects.Length - iStart);
            ListReturn.Add(strJsonObject);
            return ListReturn;
        }

        private void ConvertMoneyUnit(ref BG_DefaultMain objBG_Main, BusinessModel.SS_MoneyUnit objMoneyUnit)
        {
            objBG_Main.Total_BG_PreYear = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG_PreYear, objMoneyUnit.UnitMultiple);
            objBG_Main.Total_BG_CurYear = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG_CurYear, objMoneyUnit.UnitMultiple);
            objBG_Main.Total_BG = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG, objMoneyUnit.UnitMultiple);
            List<BG_DefaultDetail> ListBG_Detail = objBG_Main.BG_DefaultDetail.ToList();
            foreach (BG_DefaultDetail item in ListBG_Detail)
            {
                double dblTotal = (double)item.Total_BG;
                dblTotal = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN(dblTotal, objMoneyUnit.UnitMultiple);
                item.Total_BG = dblTotal;
            }
        }

        private void SetBG_MainFeild(ref BG_DefaultMain objBG_Main)
        {
            // 设置 业务，单据，UI 的type ，为了和老平台兼容，这些属性可以直接赋值


            objBG_Main.GUID_YWType = defaultGUID_YWType;
            objBG_Main.GUID_DocType = defaultGUID_DocType;
            objBG_Main.GUID_UIType = defaultGUID_UIType;

            // 任何时候，修改时间都可以用当前时间,不考虑凌晨前打开页面，凌晨后保存的极端情况


            objBG_Main.ModifyDate = DateTime.Now;
        }
        protected  VerifyResult InsertVerify(object data,string bgYear)
        {
            VerifyResult result = new VerifyResult();
            BG_DefaultMain model = (BG_DefaultMain)data;

            int bgyear = 0;
            int.TryParse(bgYear,out bgyear);

            // 先检查一下是不是已经有一个等价的预算编制了，单位，部门，项目，预算步骤，预算年可以唯一的决定一个预算编制
            // 要注意，所查到的预算编制是有效的且不是作废的
            IQueryable<BG_DefaultMain> BG_MainSet = null;
            if (null != model.GUID_Project)
            {               
                BG_MainSet = this.BusinessContext.BG_DefaultMain.Where(e => e.GUID_DW == model.GUID_DW && e.GUID_Department == model.GUID_Department &&
                e.GUID_Project == model.GUID_Project && e.DocState != 9 );
                //预算年在明细中，在明细中取年份判断
                var q = (from a in BG_MainSet
                         join b in this.BusinessContext.BG_DefaultDetail on a.GUID equals b.GUID_BG_Main
                         where b.BGYear!=null && (int)b.BGYear==bgyear
                         select a).Distinct().AsQueryable();
                BG_MainSet = q;

            }
            else
            {
                BG_MainSet = this.BusinessContext.BG_DefaultMain.Where(e => e.GUID_DW == model.GUID_DW && e.GUID_Department == model.GUID_Department &&
                e.GUID_Project == null && e.DocState != 9 );
                //预算年在明细中，在明细中取年份判断
                var q = (from a in BG_MainSet
                         join b in this.BusinessContext.BG_DefaultDetail on a.GUID equals b.GUID_BG_Main
                         where b.BGYear != null && (int)b.BGYear == bgyear
                         select a).Distinct().AsQueryable();
                BG_MainSet = q;
            }
            // 先找到 单位，部门，项目预算步骤完全相同的对象
            List<BG_DefaultMain> ListBG_Main = new List<BG_DefaultMain>();
            int iCount = model.BG_DefaultDetail.Count;
            if (null != BG_MainSet && iCount != 0)
            {
                ListBG_Main = BG_MainSet.ToList();
                BG_DefaultDetail tmpDetail = model.BG_DefaultDetail.ElementAt(0);
                BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.GUID == model.GUID_BGSetup);
                foreach (BG_DefaultMain item in ListBG_Main)
                {
                    // 判断预算编制的预算年是否相同,预算类型和预算步骤是否相同

                    BG_DefaultDetail targetDetail = this.BusinessContext.BG_DefaultDetail.FirstOrDefault(e => e.GUID_BG_Main == item.GUID && e.BGYear == tmpDetail.BGYear);
                    if (null != targetDetail)
                    {
                        BG_Setup objTmpBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.GUID == item.GUID_BGSetup);
                        if (null != objBG_Setup && null != objTmpBG_Setup)
                        {
                            // 这里面不使用预算设置，避免预算设置的修改导致判断出错，一个预算设置被删除，又新添加，新旧预算设置所关联的预算类型和预算步骤完全一致
                            if (objBG_Setup.BG_Type == objTmpBG_Setup.BG_Type && objBG_Setup.BG_Step == objTmpBG_Setup.BG_Step)
                            {
                                List<ValidationResult> resultList = new List<ValidationResult>();
                                string str = "这个预算编制已经存在，预算编号为: " + item.DocNum;
                                resultList.Add(new ValidationResult("", str));
                                result._validation.AddRange(resultList);
                                break;
                            }
                        }

                    }
                }
            }
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            var vf_detail = VerifyResultDetail(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;

        }
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(BG_DefaultMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BG_DefaultMain mModel = data;
            object g;

            #region   主表字段验证

            //报销日期
            if (mModel.DocDate.IsNullOrEmpty())
            {
                str = "单据日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                {
                    str = "单据日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //制单人



            if (mModel.GUID_Maker.IsNullOrEmpty())
            {
                str = "制单人 不能为空!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Maker.GetType(), mModel.GUID_Maker.ToString(), out g) == false)
                {
                    str = "制单人格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //最后修改人
            if (mModel.GUID_Modifier.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.GUID_Modifier.GetType(), mModel.GUID_Modifier.ToString(), out g) == false)
            {
                str = "最后修改人格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //制单日期
            if (mModel.MakeDate.IsNullOrEmpty())
            {
                str = "制单日期 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.MakeDate.GetType(), mModel.MakeDate.ToString(), out g) == false)
                {
                    str = "制单日期格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //最后修改日期

            if (mModel.ModifyDate.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.ModifyDate.GetType(), mModel.ModifyDate.ToString(), out g) == false)
            {
                str = "最后修改日期格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //提交日期
            if (mModel.SubmitDate.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(mModel.SubmitDate.GetType(), mModel.SubmitDate.ToString(), out g) == false)
            {
                str = "提交日期格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }

            //预算人GUID
            if (mModel.GUID_Person.IsNullOrEmpty())
            {
                str = "预算人 字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                {
                    str = "预算人格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //预算部门
            if (mModel.GUID_Department.IsNullOrEmpty())
            {
                str = "预算部门 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                {
                    str = "预算部门格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }
            //单位GUID
            if (mModel.GUID_DW.IsNullOrEmpty())
            {
                str = "单位 字段为必输项!";
                resultList.Add(new ValidationResult("", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.GUID_DW.GetType(), mModel.GUID_DW.ToString(), out g) == false)
                {
                    str = "单位格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            if (mModel.GUID_BGSetup.IsNullOrEmpty())
            {
                str = "预算设置 字段为必输项!";
                resultList.Add(new ValidationResult("", str));
            }

            // 检查预算设置和项目之间的关系
            BG_SetupView viewBG_SetupView = this.InfrastructureContext.BG_SetupView.FirstOrDefault(e => e.GUID == mModel.GUID_BGSetup);
            if (mModel.GUID_Project != null)
            {
                if (null != viewBG_SetupView)
                {
                    //如果选择了项目，那么预算设置不能是关于基础支出的
                    if (viewBG_SetupView.BGTypeKey == "01")
                    {
                        str = "当前预算设置为基本支出预算，请不要选择项目!";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
            }
            else
            {
                if (null != viewBG_SetupView)
                {
                    //如果选择了项目，那么预算设置不能是关于基础支出的
                    if (viewBG_SetupView.BGTypeKey == "02")
                    {
                        str = "当前预算设置为项目预算，请选择项目后再保存!";
                        resultList.Add(new ValidationResult("", str));
                    }
                }
            }

            // 检查功能分类和项目之间的对应关系
            SS_ProjectView viewProject = this.InfrastructureContext.SS_ProjectView.FirstOrDefault(e => e.GUID == mModel.GUID_Project);
            if (null == viewProject)
            {
                if (!mModel.GUID_FunctionClass.IsNullOrEmpty())
                {
                    // 没有选择项目，那么就不要选择功能分类
                    str = "没有选择项目，功能分类也不能选择!";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            else
            {
                // 功能分类和项目不对应
                if (viewProject.GUID_FunctionClass != mModel.GUID_FunctionClass)
                {
                    str = "项目与功能分类不对应，功能分类应选择: " + viewProject.FunctionClassName;
                    resultList.Add(new ValidationResult("", str));
                }
            }

            return resultList;

            #endregion
        }
        /// <summary>
        /// 明显表验证


        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetail(BG_DefaultMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<BG_DefaultDetail> detailList = new List<BG_DefaultDetail>();
            foreach (BG_DefaultDetail item in data.BG_DefaultDetail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BG_DefaultDetail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_BG_DefaultDetail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                resultList.Add(new ValidationResult("", "请添加预算信息！"));

            }

            return resultList;
        }
        private List<ValidationResult> VerifyResult_BG_DefaultDetail(BG_DefaultDetail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BG_DefaultDetail item = data;
            /// <summary>
            /// 明细表字段验证


            /// </summary>
            //#region 明细表字段验证




            ////预算科目的GUID
            //if (item.GUID_BGCode.IsNullOrEmpty())
            //{
            //    str = "明细预算科目 字段为必输项!";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_BGCode.GetType(), item.GUID_BGCode.ToString(), out g) == false)
            //    {
            //        str = "明细预算科目格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}
            ////金额
            //if (item.Total_BG.ToString() == "")
            //{
            //    str = "明细金额 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.Total_BG.GetType(), item.Total_BG.ToString(), out g) == false)
            //    {
            //        str = "明细金额格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //    else
            //    {
            //        if (double.Parse(g.ToString()) == 0F)
            //        {
            //            str = "明细金额不能为零！";
            //            resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
            //        }
            //    }
            //}
            //#endregion


            return resultList;
        }

        protected override void Delete(Guid guid)
        {
            BG_DefaultMain main = this.BusinessContext.BG_DefaultMain.Include("BG_DefaultDetail").FirstOrDefault(e => e.GUID == guid);

            List<BG_DefaultDetail> details = new List<BG_DefaultDetail>();

            foreach (BG_DefaultDetail item in main.BG_DefaultDetail)
            {
                details.Add(item);
            }

            foreach (BG_DefaultDetail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
        }
        /// <summary>
        /// 预算保存
        /// </summary>
        /// <param name="strBG_MainData"></param>
        /// <param name="strDetailData"></param>
        /// <param name="strState"></param>
        /// <param name="strBGYear"></param>
        /// <param name="strMoneyUnitGUID"></param>
        /// <param name="strPreGuid"></param>
        /// <param name="strPreScope"></param>
        /// <param name="vr"></param>
        /// <returns></returns>
        public JsonModel SaveBG_Main(string strBG_MainData, string strDetailData, string strState, string strBGYear, string strMoneyUnitGUID, string strPreGuid,string strPreScope,ref VerifyResult vr)
        {
            JsonModel result = new JsonModel();
            try
            {
                string strMsg = string.Empty;
                VerifyResult vResult = null;
                Guid Guid_BG_Main = Guid.Empty;
                // 获得BG_Main信息
                List<ValuePair> BG_MainInfo = SplitJsonObject(strBG_MainData);
                if ("3" == strState)    //进行删除操作
                {
                    string strGUID_BG_Main = string.Empty;
                    var vp = BG_MainInfo.Find(e => e.strKey == "BG_DefaultMain_GUID");
                    if (vp != null)
                    {
                        strGUID_BG_Main = vp.strValue;
                    }

                    if (strGUID_BG_Main == string.Empty)
                    {

                    }
                    else
                    {
                        Guid_BG_Main = new Guid(strGUID_BG_Main);
                        vResult = DeleteVerify(Guid_BG_Main);
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            this.Delete(Guid_BG_Main);
                        }
                    }

                }
                else if ("1" == strState || "2" == strState)
                {
                    // 获得BG_Detail信息
                    int indexFirst = strDetailData.IndexOf('[');
                    int indexLast = strDetailData.LastIndexOf(']');
                    string strData = strDetailData.Substring(indexFirst, indexLast - indexFirst + 1);
                    List<string> ListDetail = SplitJsonArray(strData);

                    // 最后一行是合计，不用解析
                    List<List<ValuePair>> ListDetailInfo = new List<List<ValuePair>>();
                    int iCount = ListDetail.Count;
                    for (int i = 0; i < iCount-1; i++)
                    {
                        List<ValuePair> BG_Detail = SplitJsonObject(ListDetail[i]);
                        ListDetailInfo.Add(BG_Detail);
                    }

                    // 组装 BG_Main ，组装好的包含BG_Detail
                    BG_DefaultMain objBG_Main = AssemblingBG_Main(BG_MainInfo, ListDetailInfo, strBGYear);
                    Guid_BG_Main = objBG_Main.GUID;
                    Guid GUID_MoneyUnit = new Guid(strMoneyUnitGUID);
                    var objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == GUID_MoneyUnit);
                    // 转换货币单位
                    //ConvertMoneyUnit(ref objBG_Main, objMoneyUnit);
                    SetBG_MainFeild(ref objBG_Main);
                    if ("1" == strState)
                    {
                        vResult = InsertVerify(objBG_Main,strBGYear);
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            objBG_Main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)objBG_Main.GUID_DW, defaultGUID_YWType, objBG_Main.DocDate.ToString());
                            this.BusinessContext.BG_DefaultMain.AddObject(objBG_Main);
                            this.BusinessContext.SaveChanges();
                            if(strPreGuid!="")
                            {
                                WorkFlowAPI.SaveBGDocTransToProcess(new Guid(strPreGuid), objBG_Main.GUID, BGTransStatus.预算分配到预算初始值);
                            }
                        }
                    }
                    else
                    {
                        vResult = ModifyVerify(objBG_Main);//修改验证
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            ModifyBG_Main(objBG_Main);
                        }
                    }

                }

                if (string.IsNullOrEmpty(strMsg))
                {
                    if (strState == "3")//删除时返回默认值
                    {
                        result = this.New();
                        strMsg = "删除成功！";
                    }
                    else
                    {
                        result = this.Retrieve(Guid_BG_Main);
                        strMsg = "保存成功！";
                    }
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                }
                else
                {
                    result.result = JsonModelConstant.Error;
                    result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                }
                return result;
            }
            catch (Exception ex)
            {
                //throw ex;
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
        public override List<object> History(SearchCondition conditions)
        {
            
            List<object> list = new List<object>();
            //IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.DocTypeUrl == null);
            IQueryable<BG_DefaultMainView> main = this.BusinessContext.BG_DefaultMainView;
            // 获得权限
            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            IntrastructureFun objIF = new Infrastructure.IntrastructureFun();
            var DepartmentAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            var PersonAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            var ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            IQueryable<BG_DefaultMainView> mainSetWithOutProject = main.Where(e => e.GUID_Project == null && PersonAuth.Contains((Guid)e.GUID_Person)
                && DepartmentAuth.Contains((Guid)e.GUID_Department));
            IQueryable<BG_DefaultMainView> mainSetWithProject = main.Where(e => e.GUID_Project != null && PersonAuth.Contains((Guid)e.GUID_Person)
                && ProjectAuth.Contains((Guid)e.GUID_Project));

            IQueryable<BG_DefaultMainView> allBG_Main = mainSetWithOutProject.Union(mainSetWithProject);
            allBG_Main = allBG_Main.Where(e => e.GUID_Maker == this.OperatorId);
            list = allBG_Main.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                BGSetupName = e.BGSetupName,
                ProjectName=e.ProjectName,
                ProjectKey=e.ProjectKey,
                BGStepName=e.BGStepName,
                BGTypeName=e.BGTypeName,
                DepartmentName=e.DepartmentName,
                Maker=e.Maker,
                e.MakeDate
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();
            return list;

        }
        public override JsonModel Retrieve(Guid guid)
        {
            JsonModel jmodel = new JsonModel();
            try
            {
                BG_DefaultMainView main = this.BusinessContext.BG_DefaultMainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    BusinessModel.SS_MoneyUnit moneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == main.GUID_MoneyUnit);
                    if (null == moneyUnit)
                    {
                        jmodel.result = JsonModelConstant.Error;
                        jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                        return jmodel;
                    }
                    //main.Total_BG = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)main.Total_BG, moneyUnit.UnitMultiple);
                    //main.Total_BG_PreYear = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)(main.Total_BG_PreYear), moneyUnit.UnitMultiple);
                    //main.Total_BG_CurYear = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)(main.Total_BG_CurYear), moneyUnit.UnitMultiple);

                    jmodel.m = main.Pick();

                    BG_DefaultDetail BG_Detail = this.BusinessContext.BG_DefaultDetail.FirstOrDefault(e => e.GUID_BG_Main == guid);
                    // 加入BG_Detail的 BGYear
                    JsonAttributeModel jam = new JsonAttributeModel();
                    jam.m = "BG_DefaultDetail";
                    jam.n = "BGYear";
                    jam.v = BG_Detail.BGYear.ToString();
                    jmodel.m.Add(jam);
                    //IQueryable<BG_DetailView> q = this.BusinessContext.BG_DetailView.Where(e => e.GUID_BG_Main == guid).OrderBy(e => e.BGCodeKey);
                    //List<BG_DetailView> details = q == null ? new List<BG_DetailView>() : q.ToList();
                    //if (details.Count > 0)
                    //{
                    //    JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                    //    jmodel.d.Add(jgm);
                    //    foreach (BG_DetailView detail in details)
                    //    {
                    //        List<JsonAttributeModel> picker = detail.Pick();

                    //        jgm.r.Add(picker);
                    //    }
                    //}
                    //明细中f 填充默认值



                    BG_DefaultDetailView dModel = new BG_DefaultDetailView();
                    dModel.FillDetailDefault<BG_DefaultDetailView>(this, this.OperatorId);
                    JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                    jmodel.f.Add(fjgm);

                    List<JsonAttributeModel> fpicker = dModel.Pick();
                    fjgm.r.Add(fpicker);
                }
                jmodel.s = new JsonMessage("", "", "");
                return jmodel;
            }
            catch (Exception ex)
            {
                // throw ex;
                jmodel.result = JsonModelConstant.Error;
                jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                return jmodel;
            }
        }
        // 修改一个BG_Main
        private void ModifyBG_Main(BG_DefaultMain objBG_Main)
        {
            JsonModel jsonModel = new JsonModel();
            jsonModel.m = objBG_Main.Pick();

            BG_DefaultMain main = new BG_DefaultMain();
            BG_DefaultDetail tempDetail = new BG_DefaultDetail();
            JsonGridModel jgm = new JsonGridModel(tempDetail.ModelName());
            jsonModel.d.Add(jgm);
            foreach (BG_DefaultDetail detail in objBG_Main.BG_DefaultDetail)
            {
                List<JsonAttributeModel> picker = detail.Pick();
                jgm.r.Add(picker);
            }

            // 一定要从数据库中获得数据对象，否则修改将出现异常
            main = this.BusinessContext.BG_DefaultMain.Include("BG_DefaultDetail").FirstOrDefault(e => e.GUID == objBG_Main.GUID);
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this, this.OperatorId);

            string detailModelName = tempDetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BG_DefaultDetail detail in main.BG_DefaultDetail)
                {
                    this.BusinessContext.DeleteConfirm(detail);
                }
            }
            else
            {
                List<BG_DefaultDetail> detailList = new List<BG_DefaultDetail>();
                foreach (BG_DefaultDetail detail in main.BG_DefaultDetail)
                {
                    detailList.Add(detail);
                }

                foreach (BG_DefaultDetail detail in detailList)
                {
                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        detail.Fill(row);
                        this.BusinessContext.ModifyConfirm(detail);
                    }
                }
                this.BusinessContext.ModifyConfirm(main);
                this.BusinessContext.SaveChanges();
            }
        }

        protected override VerifyResult ModifyVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BG_DefaultMain model = (BG_DefaultMain)data;
            BG_DefaultMain orgModel = this.BusinessContext.BG_DefaultMain.Include("BG_DefaultDetail").FirstOrDefault(e => e.GUID == model.GUID);
            //if (orgModel != null)
            //{
            //    if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
            //    {
            //        List<ValidationResult> resultList = new List<ValidationResult>();
            //        resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
            //        result._validation = resultList;
            //        return result;
            //    }
            //}


            //流程验证
            string strErr = "";
            

            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                if (model.GUID_BG_Assign == null)//2014-7-30 改
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "此单据没有保存对应的预算分配信息！"));
                    result._validation = resultList;
                    return result;
                }

                FlowNodeModel objFnm = WorkFlowAPI.GetCurNodeByDocId(model.GUID, out strErr);
                
                if (null == objFnm)
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "此单据虽然在流程中，但找不到所在节点"));
                    result._validation = resultList;
                    return result;
                }
                else if (objFnm.WorkFlowNodeName != "预算初始值")
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "此单据不在预算初始值节点上，因此不能修改"));
                    result._validation = resultList;
                    return result;
                }
                else 
                {
                    var excutorId = WorkFlowAPI.GetExcutorByProcessNodeId(objFnm.ProcessNodeId);
                    if (null == excutorId)
                    {
                        List<ValidationResult> resultList = new List<ValidationResult>();
                        resultList.Add(new ValidationResult("", "当前流程节点找不到操作员"));
                        result._validation = resultList;
                        return result;
                    }
                    else if (excutorId != this.OperatorId)
                    {
                        List<ValidationResult> resultList = new List<ValidationResult>();
                        resultList.Add(new ValidationResult("", "你无权修改这张单据"));
                        result._validation = resultList;
                        return result;
                    }
                }
            }
            
            //作废           
            if (orgModel != null && orgModel.DocState == int.Parse("9") && model.DocState != (int)Business.Common.EnumType.EnumDocState.RcoverState)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //明细验证
            var vf_detail = VerifyResultDetail(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;
        }


        // 组装一行BG_Detail，每一行，前三个数据对是固定的，从第四行开始，就是BG_Item的GUID,BG_Detail的total_bg，如果有BGMemo，则夹带一个BGMemo
        private List<BG_DefaultDetail> AssemblingBG_Detail(List<ValuePair> ListDetailInfo)
        {
            List<BG_DefaultDetail> ListDetail = new List<BG_DefaultDetail>();
            BG_DefaultDetail detail = null;
            Guid GUID_BGCoe = Guid.Empty;
            foreach (ValuePair item in ListDetailInfo)
            {
                switch (item.strKey)
                {
                    case "RateNum":
                        {
                            break;
                        }
                    case "Sum":
                        {
                            if(item.strValue!="NO")
                            {
                                return null;
                            }
                            break;
                        }
                    case "Add":
                        {
                            break;
                        }
                    case "GUID_BGCode":
                        {
                            GUID_BGCoe = new Guid(item.strValue);
                            break;
                        }
                    case "BGCodeName":
                        {
                            break;
                        }
                    default:
                        {
                            if (item.strKey.StartsWith("DetailGUID"))
                            {
                                if (null != detail)
                                {
                                    //将上一个detail放入list中
                                    ListDetail.Add(detail);
                                }
                                detail = new BG_DefaultDetail();
                                detail.GUID_BGCode = GUID_BGCoe;
                                if ("" == item.strValue)        //新增
                                {
                                    detail.GUID = Guid.NewGuid();
                                }
                                else                            // 修改或删除
                                {
                                    detail.GUID = new Guid(item.strValue);
                                }
                            }
                            else if (item.strKey.StartsWith("BGMemo"))
                            {
                                detail.BGMemo = item.strValue;
                            }
                            else
                            {
                                detail.GUID_Item = new Guid(item.strKey);
                                if ("" == item.strValue)
                                {
                                    detail.Total_BG = 0.00;
                                }
                                else
                                {
                                    detail.Total_BG = Double.Parse(item.strValue);
                                }
                            }
                            break;
                        }
                }
            }
            ListDetail.Add(detail);
            return ListDetail;
        }
        // 根据前台返回的数据组装BG_Main，同时将BG_Detail加载
        private BG_DefaultMain AssemblingBG_Main(List<ValuePair> BG_MainInfo, List<List<ValuePair>> ListDetailInfo, string strBGYear)
        {
            BG_DefaultMain objBG_Main = AssemblingBG_MainEx(BG_MainInfo);
            if (null == objBG_Main)
            {
                return null;
            }

            foreach (List<ValuePair> item in ListDetailInfo)
            {
                List<BG_DefaultDetail> ListDetail = AssemblingBG_Detail(item);
                if(null==ListDetail)
                {
                    continue;
                }

                foreach (BG_DefaultDetail detail in ListDetail)
                {
                    detail.GUID_BG_Main = objBG_Main.GUID;
                    detail.BGYear = Int32.Parse(strBGYear);
                    objBG_Main.BG_DefaultDetail.Add(detail);
                }
            }
            return objBG_Main;
        }
        // 根据前台返回的数据组装BG_Main
        private BG_DefaultMain AssemblingBG_MainEx(List<ValuePair> BG_MainInfo)
        {
            BG_DefaultMain objBG_Main = new BG_DefaultMain();
            int iBG_MainCount = BG_MainInfo.Count;
            foreach (ValuePair item in BG_MainInfo)
            {
                switch (item.strKey)
                {
                    case "BG_DefaultMain_GUID":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "DocNum":
                        {
                            objBG_Main.DocNum = item.strValue;
                            break;
                        }
                    case "DocDate":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.DocDate = DateTime.Parse(item.strValue);
                            }
                            break;
                        }
                    case "GUID_BG_Setup":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_BGSetup = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "DocVerson":
                        {
                            objBG_Main.DocVerson = item.strValue;
                            break;
                        }
                    case "GUID_DW":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_DW = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_Department":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_Department = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_Person":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_Person = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_Project":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_Project = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_Maker":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_Maker = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_Modifier":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_Modifier = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "DocState":
                        {
                            objBG_Main.DocState = Int32.Parse(item.strValue);
                            break;
                        }
                    case "GUID_BG_Assign":
                        {
                            if(!string.IsNullOrEmpty(item.strValue))
                            {
                                objBG_Main.GUID_BG_Assign = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "Total_BG":
                        {
                            if ("" == item.strValue)
                            {
                                objBG_Main.Total_BG = 0.0;
                            }
                            else
                            {
                                objBG_Main.Total_BG = Double.Parse(item.strValue);
                            }
                            break;
                        }
                    case "Total_BG_PreYear":
                        {
                            if ("" == item.strValue)
                            {
                                objBG_Main.Total_BG_PreYear = 0.0;
                            }
                            else
                            {
                                objBG_Main.Total_BG_PreYear = Double.Parse(item.strValue);
                            }
                            break;
                        }
                    case "Total_BG_CurYear":
                        {
                            if ("" == item.strValue)
                            {
                                objBG_Main.Total_BG_CurYear = 0.0;
                            }
                            else
                            {
                                objBG_Main.Total_BG_CurYear = Double.Parse(item.strValue);
                            }
                            break;
                        }
                    case "GUID_FunctionClass":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.GUID_FunctionClass = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "MakeDate":
                        {
                            if ("" != item.strValue)
                            {
                                objBG_Main.MakeDate = DateTime.Parse(item.strValue);
                            }
                            break;
                        }
                    //case "ModifyDate":
                    //    {
                    //        objBG_Main.ModifyDate = DateTime.Parse(item.strValue);
                    //        break;
                    //    }
                    //case "SubmitDate":
                    //    {
                    //        objBG_Main.SubmitDate = DateTime.Parse(item.strValue);
                    //        break;
                    //    }
                    default:
                        {
                            return null;
                        }
                }
            }

            return objBG_Main;
        }
        /// <summary>
        /// 根据ID判断是否存在
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsExistByID(Guid guid)
        {
           var model= this.BusinessContext.BG_DefaultMain.FirstOrDefault(e=>e.GUID==guid);
           return model == null ? false : true;

        }

    }
}
