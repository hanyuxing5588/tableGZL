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
namespace Business.Budget
{
    public class ValuePair
    {
        public string strKey;
        public string strValue;
        public ValuePair()
        {
            strKey = string.Empty;
            strValue = string.Empty;
        }
        public ValuePair(string strKey, string strValue)
        {
            this.strKey = strKey;
            this.strValue = strValue;
        }
    }
    public class 预算编制 : BaseDocument
    {
        public bool YSTZ { get; set; }//预算调整调用修改调用此保存
        private Guid defaultGUID_YWType;
        private Guid defaultGUID_DocType;
        private Guid defaultGUID_UIType;
        private static readonly object obj=new object();
        public 预算编制()
            : base()
        {
            defaultGUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
            defaultGUID_DocType = new Guid("91FF4EDE-6569-4A17-A8B6-9C675AF1E110");
            defaultGUID_UIType = new Guid("2726487D-5CE7-456B-89EE-87064DC94FCA");
            
        }
        public 预算编制(Guid OperatorId, string ModelUrl)
            : base(OperatorId, ModelUrl)
        {
            defaultGUID_YWType = new Guid("D0169070-F2CB-49D4-819F-FBF372B5C916");
            defaultGUID_DocType = new Guid("91FF4EDE-6569-4A17-A8B6-9C675AF1E110");
            defaultGUID_UIType = new Guid("2726487D-5CE7-456B-89EE-87064DC94FCA");
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

                BG_MainView model = new BG_MainView();
                model.FillDefault(this, this.OperatorId);
                InitBG_Main(ref model);
                //model.GUID = new Guid("42D05DF4-6A50-49E1-A4B4-AE43855B2034");  
                jmodel.m = model.Pick();
                BG_DetailView dModel = new BG_DetailView();
                dModel.FillDetailDefault<BG_DetailView>(this, this.OperatorId);
                string strTime = DateTime.Now.ToString();
                string strBGYear = strTime.Substring(0, 4);
                dModel.BGYear = Int32.Parse(strBGYear) + 1;

                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                List<JsonAttributeModel> picker = dModel.Pick();
                fjgm.r.Add(picker);

                jmodel.f.Add(fjgm);

                // 加入BG_Detail的 BGYear
                JsonAttributeModel jam = new JsonAttributeModel();
                jam.m = "BG_Detail";
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

        public JsonModel New(string guid, string strScope)
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                BG_MainView objBGMain = null;
                Guid Id;
                Guid.TryParse(guid.ToString(),out Id);
                int bgYear = DateTime.Now.Year;
                if("ysfp"==strScope||string.IsNullOrEmpty(strScope))
                {                  
                    BG_AssignView objAssign = this.BusinessContext.BG_AssignView.FirstOrDefault(e=>e.GUID==Id);
                    if (objAssign == null)
                    {
                        return jmodel;
                    }
                    BGAssignToBGMain objTransfer = new BGAssignToBGMain(Id);
                    objBGMain = objTransfer.DocTransferBGMain(this.BusinessContext, this.InfrastructureContext, this.OperatorId,objAssign);
                    bgYear = objAssign.BGYear;
                }
                else if("yscszsz"==strScope)
                {
                    BGDefaultToBGMain objTransfer = new BGDefaultToBGMain(Id);
                    objBGMain = objTransfer.DocTransferBGMain(this.BusinessContext,this.InfrastructureContext,this.OperatorId);
                    var detailDefault = this.BusinessContext.BG_DefaultDetailView.FirstOrDefault(e=>e.GUID_BG_Main==Id);
                    if (detailDefault != null)
                    {
                        bgYear = detailDefault.BGYear==null?bgYear:(int)detailDefault.BGYear;
                    }

                }
                jmodel.m = objBGMain.Pick();

                // 加入BG_Detail的 BGYear
                JsonAttributeModel jam = new JsonAttributeModel();
                jam.m = "BG_Detail";
                jam.n = "BGYear";
                //string strTime = DateTime.Now.ToString();
                //string strBGYear = strTime.Substring(0, 4);
                //jam.v = (Int32.Parse(strBGYear) + 1).ToString();
                jam.v = bgYear.ToString();
                jmodel.m.Add(jam);
                //转换明细信息

                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        ///     函数功能:   我们获得了一个预算设置的所对应的listBG_SetupBGCode，经此函数分析后，获得listSortBG_SetupBGCode，这个是
        ///                 用于界面上显示时的顺序，界面上只对最小级别的预算科目进行编制，高级科目的值是自己的低级别科目汇总而来，因此

        ///                 界面上需要知道如何汇总，dicSumFormula记录了每个科目所对应的子科目所在行，用逗号分隔，如果没有，设置为NO，

        ///                 dicAddFormula 记录了每个科目的父级科目，如果没有，设置NO
        ///      author:    dongsheng.zhang
        ///        日期:    2014-4-23    
        /// </summary>
        /// <param name="listBG_SetupBGCode"></param>
        /// <param name="listSortBG_SetupBGCode"></param>
        /// <param name="dicSumFormula"></param>
        /// <param name="dicAddFormula"></param>
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

        // 根据搜索条件，listBG_SetupBGCode已经是有序的了，因此该算法是在listBG_SetupBGCode 有序的基础上进行的
        bool AnalyseBGCodeEx(ref List<BG_SetupBGCodeView> listBG_SetupBGCode, 
            ref Dictionary<Guid, string> dicSumFormula, ref Dictionary<Guid, string> dicAddFormula,  ref Dictionary<Guid, int> dicLevel)
        {
            int iLine = 0;
            Dictionary<Guid,int> DicLine = new Dictionary<Guid,int>();
            foreach (BG_SetupBGCodeView item in listBG_SetupBGCode)
            {
                DicLine.Add(item.GUID_BGCode,iLine);
                if (item.PSS_BGCodeGUID == null)
                {
                    dicAddFormula.Add(item.GUID_BGCode,"NO");
                    dicLevel.Add(item.GUID_BGCode,0);
                }
                else
                {
                    // 计算dicAddFormula
                    if (!DicLine.ContainsKey((Guid)item.PSS_BGCodeGUID))
                    {
                        return false;
                    }
                    else
                    {
                        dicAddFormula.Add(item.GUID_BGCode, DicLine[(Guid)item.PSS_BGCodeGUID].ToString());
                    }

                    // 计算dicSumFormula,计算上级的
                    if (dicSumFormula.ContainsKey((Guid)item.PSS_BGCodeGUID))
                    {
                        string strFormula = dicSumFormula[(Guid)item.PSS_BGCodeGUID];
                        if ("NO" == strFormula)
                        {
                            strFormula = iLine.ToString();
                        }
                        else
                        {
                            strFormula = strFormula + "," + iLine.ToString();
                        }
                        dicSumFormula[(Guid)item.PSS_BGCodeGUID] = strFormula;
                    }
                    else
                    {
                        dicSumFormula.Add((Guid)item.PSS_BGCodeGUID,iLine.ToString());
                    }

                    // 计算自身
                    dicSumFormula.Add(item.GUID_BGCode,"NO");

                    if (!dicLevel.ContainsKey((Guid)item.PSS_BGCodeGUID))
                    {
                        return false;
                    }
                    else
                    {
                        int tmpLevel = dicLevel[(Guid)item.PSS_BGCodeGUID] + 1;
                        dicLevel.Add(item.GUID_BGCode, tmpLevel);
                    }
                }
                iLine++;
            }
            return true;
        }
        /*
         * 默认的，detail里不存储非末级科目所对应的detail，但是老库里却存储了，且部分预算编制既存储了非末级，又部分只存储了末级
         * 因此需要根据末级查询父级
         * 
         */
        private void GetSortBG_SetupBGCodeView(ref List<BG_DetailView> listBG_DetailView, ref List<BG_SetupBGCodeView> listBG_SetupBGCodeView)
        {
            HashSet<string> hsBGCodeKey = new HashSet<string>();
            HashSet<string> hsGuid = new HashSet<string>();
            foreach (BG_DetailView item in listBG_DetailView)
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
        private void GetSortBG_SetupDetailView(ref List<BG_DetailView> listBG_DetailView, ref List<BusinessModel.BG_SetupDetailView> listBG_SDView, Guid guidBG_Setup)
        {
            HashSet<string> hsBGCodeKey = new HashSet<string>();
            foreach (BG_DetailView item in listBG_DetailView)
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
           listBG_SDView = listBG_SDView.OrderBy(e=> e.ItemOrder).ToList();
        }
        /*
         *      函数功能:   获得预算编制表的行数据
         *       author:   dongsheng.zhang
         *         日期:   2014-4-15   
         * 
         */
        private string GetBG_DetailDataEx(string strBG_SetupGUID, BG_MainView viewBG_Main,ref List<BG_DetailView> ListDetail,ref List<BG_SetupBGCodeView> listBG_SetupBGCode, ref List<BusinessModel.BG_SetupDetailView> listBG_SDViewEx,string strPreGuid,string strPreScope)
        {
            try
            {

           
            BusinessModel.SS_MoneyUnit objMoneyUnit = null;
            string strData = string.Empty;
            Guid guidBG_SetupGUID = new Guid(strBG_SetupGUID);
            Dictionary<Guid, Dictionary<Guid, BG_DetailView>> SortDic = new Dictionary<Guid, Dictionary<Guid, BG_DetailView>>();
            // 如果 main已经存在
            if(null!=viewBG_Main)
            {
                foreach (BG_DetailView tmpDetail in ListDetail)
                {
                    if (!SortDic.ContainsKey((Guid)tmpDetail.GUID_BGCode))
                    {
                        Dictionary<Guid, BG_DetailView> tmpDic = new Dictionary<Guid, BG_DetailView>();
                        tmpDic.Add((Guid)tmpDetail.GUID_Item, tmpDetail);
                        SortDic.Add((Guid)tmpDetail.GUID_BGCode, tmpDic);
                    }
                    else
                    {
                        Dictionary<Guid, BG_DetailView> tmpDic = SortDic[(Guid)tmpDetail.GUID_BGCode];
                        tmpDic.Add((Guid)tmpDetail.GUID_Item, tmpDetail);
                    }
                }
            }
            else       // 如果main不存在，那么就是新增，第一列预算科目根据预算设置获取
            {
                IQueryable<BG_SetupBGCodeView> viewBG_SetupBGCode = this.BusinessContext.BG_SetupBGCodeView.Where(e => e.GUID_BGSetup == guidBG_SetupGUID && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey);
                
                if (null == viewBG_SetupBGCode)
                {
                    return string.Empty;
                }
                // 获得到BG_Setup所关联的BG_Code
                listBG_SetupBGCode = viewBG_SetupBGCode.ToList();
                if (listBG_SetupBGCode.Count==0)
                {
                    return string.Empty;
                }
            }


            List<BG_SetupBGCodeView> listSortBG_SetupBGCode = new List<BG_SetupBGCodeView>();
            Dictionary<Guid, string> dicSumFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, string> dicAddFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, int> dicLevel = new Dictionary<Guid, int>();
            // 获得每一行的数值计算信息，包括自己由哪些行汇总，自己汇总到哪一行
            if (AnalyseBGCodeEx(ref listBG_SetupBGCode, ref dicSumFormula, ref dicAddFormula, ref dicLevel))
            {
                listSortBG_SetupBGCode.AddRange(listBG_SetupBGCode);
            }
            else
            {
                AnalyseBGCode(Guid.Empty, 0, ref listBG_SetupBGCode, ref listSortBG_SetupBGCode, ref dicSumFormula, ref dicAddFormula, 0, ref dicLevel);
            }
            


            string strTotal = string.Empty;
            string strRows = string.Empty;
            int iRowCount = listSortBG_SetupBGCode.Count;
            string strSumFormula = string.Empty;        // 需要进行汇总的行号
            string strAddFormula = string.Empty;        // 父级所在行号
            string strRow = string.Empty;               // 一行数据
            string strBgCodeName = string.Empty;        // 界面上显示科目
            string strBGItem = string.Empty;
            int iAddTotal = 0;                          // 最后需要汇总到合计的行号，也就是最高级的科目所在行号
            // 拼行数据
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

                strRow = "{\\\"Sum\\\":\\\"" + strSumFormula + "\\\",\\\"Add\\\":\\\"" + strAddFormula + "\\\",\\\"GUID_BGCode\\\":\\\"" + item.GUID_BGCode + "\\\",\\\"BGCodeName\\\":\\\"" + strBgCodeName + "\\\"";
                strBGItem = string.Empty;

                if (null != viewBG_Main && objMoneyUnit==null)
                {
                    objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == viewBG_Main.GUID_MoneyUnit);
                }

                foreach (BusinessModel.BG_SetupDetailView tmp in listBG_SDViewEx)
                {
                    //var BG_Detail = ListDetail.Find(e => e.GUID_BGCode == item.GUID_BGCode && e.GUID_Item == tmp.GUID_Item);
                    BG_DetailView objBG_Detail = null;
                    if (null != viewBG_Main)
                    {
                        if (SortDic.ContainsKey((Guid)item.GUID_BGCode))
                        {
                            objBG_Detail = SortDic[(Guid)item.GUID_BGCode][(Guid)tmp.GUID_Item];
                        }
                    }
                    else if(strPreGuid!="")     // 如果预算编制这张单据是通过流程进来的，且还没有新增入库
                    { 
                        if(strPreScope=="yscszsz")  // 如果流程的上一个节点是预算初始值设置，那么应该通过预算初始值里的detail进行赋值
                        {
                            List<BG_DefaultDetail> defaultList = this.BusinessContext.BG_DefaultDetail.Where(e => e.GUID_BG_Main == new Guid(strPreGuid)).ToList();
                            BG_DefaultDetail defaultDetail = defaultList.FirstOrDefault(e => e.GUID_Item == tmp.GUID_Item && e.GUID_BGCode == item.GUID_BGCode);
                            if(null!=defaultDetail)
                            {
                                objBG_Detail = new BG_DetailView();
                                objBG_Detail.Total_BG = defaultDetail.Total_BG;
                                objBG_Detail.BGMemo = defaultDetail.BGMemo;
                            }

                        }
                    }
                   
                    string strTotalBG = "";
                    string strBGMemoShow = "";
                    string strGUID_BG_Detail = "";
                    //判断是否是预算编制初始值 传值数据 2014-8-4 sxb
                    if (null == viewBG_Main && null != objBG_Detail && strPreScope == "yscszsz")
                    {
                        double dblTotal = (double)objBG_Detail.Total_BG;
                        strTotalBG = string.Format("{0:F2}", dblTotal);
                        strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        strBGMemoShow = objBG_Detail.BGMemo == null ? "" : objBG_Detail.BGMemo;                       

                    }
                    if (null != viewBG_Main && null != objBG_Detail && guidBG_SetupGUID == viewBG_Main.GUID_BGSetup)
                    {
                        double dblTotal = (double)objBG_Detail.Total_BG;
                        strTotalBG = string.Format("{0:F2}", dblTotal);
                        strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        strBGMemoShow = objBG_Detail.BGMemo == null ? "" : objBG_Detail.BGMemo;
                        // objBG_Detail 不为空，且strPreGuid为空，那么就说明objBG_Detail是数据库中存在的bg_detail，如果strPreGuid 不为空，那么
                        // objBG_Detail 只是临时生成的一个detail，为的是通过预算初始值的detail进行初始化赋值
                        //if(strPreGuid=="")//?为什么这么判断？这样有问题如果 是在流程中并且修改保存，修改数据会出问题 2014-8-4 sxb
                        //{
                            strGUID_BG_Detail = objBG_Detail.GUID.ToString();
                        //}
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
                //break;
            }
            strBGItem = "";
            // 合计 是最后一行，最后一行的数据并不存储，因此在前台进行统计即可                
            foreach (BusinessModel.BG_SetupDetailView tmp in listBG_SDViewEx)
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

            strRow = "{\\\"Sum\\\":\\\"" + strTotal + "\\\",\\\"Add\\\":\\\"NO\\\",\\\"GUID_BGCode\\\":\\\"\\\",\\\"BGCodeName\\\":\\\"合计\\\"";
            //strBGItem = string.Empty;
            strRow = strRow + strBGItem + "}";
            strRows = strRows + "," + strRow;
            string strCount = (iRowCount + 1).ToString();
            strData = "{\\\"total\\\":" + strCount + ",\\\"rows\\\":[" + strRows + "]}";

            return strData;
            }
            catch (Exception ex) 
            {
                return  "{\\\"total\\\":" + 0 + ",\\\"rows\\\":[]}";
            }
        }

        private void DeleteLineBreak(ref string str)
        {
            if(null==str)
            {
                str="";
                return;
            }
            str = str.Replace("\r\n", "");
            str = str.Replace("\r","");
            str = str.Replace("\n", "");
            str = str.Replace("\b", "");
            str = str.Replace("\f", "");
            str = str.Replace("\t", "");
            str = str.Replace("\'","\\'");
            str = str.Replace("\"","\\\"");
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


        /*
         *      函数功能:   根据BG_Main的GUID和预算设置的GUID，获得预算编制表
         *                  预算编制表根据预算设置动态变化，因此，需要动态的加载行和列的数据在  GetBG_DetailData 进行设置
         *                  行的数据在GetBG_DetailDataEx 获取
         *        author:   dongsheng.zhang
         *          日期:   2014-4-15
         * 
         */
        public string GetBG_DetailData(string strBG_MainGUID, string strBG_SetupGUID,string strPreGuid,string strPreScope)
        {
            // 获得预算设置所关联的BG_Item
            Guid guidBG_SetupGUID = new Guid(strBG_SetupGUID);
            Guid BG_Main_GUID = new Guid(strBG_MainGUID);
            BG_MainView viewBG_Main = this.BusinessContext.BG_MainView.FirstOrDefault(e => e.GUID == BG_Main_GUID);
            List<BG_DetailView> ListDetail = new List<BG_DetailView>();
            List<BusinessModel.BG_SetupDetailView> listBG_SetupDetail = new List<BusinessModel.BG_SetupDetailView>();
            List<BG_SetupBGCodeView> listBG_SetupBGCodeView = new List<BG_SetupBGCodeView>();

            if (null == viewBG_Main)
            {
                IQueryable<BusinessModel.BG_SetupDetailView> viewBG_SetupDetail = this.BusinessContext.BG_SetupDetailView.Where(e => e.GUID_BGSetup == guidBG_SetupGUID).OrderBy(e => e.ItemOrder);
                if (null == viewBG_SetupDetail)
                {
                    return string.Empty;
                }
                listBG_SetupDetail = viewBG_SetupDetail.ToList();       // 获得预算项
            }
            else
            {
                guidBG_SetupGUID = viewBG_Main.GUID_BGSetup;
                IQueryable<BG_DetailView> Details = this.BusinessContext.BG_DetailView.Where(e => e.GUID_BG_Main == BG_Main_GUID);
                ListDetail = Details.ToList();
                if (ListDetail.Count == 0)
                {
                    return string.Empty;
                }
                GetSortBG_SetupDetailView(ref ListDetail, ref listBG_SetupDetail, guidBG_SetupGUID);    // 获得预算项
                GetSortBG_SetupBGCodeView(ref ListDetail, ref listBG_SetupBGCodeView);                  // 获得预算科目
            }

            if (listBG_SetupDetail.Count <= 0)
            {
                return string.Empty;
            }

            // 循环获取对象，拼接列   每行有多个BGItem，因此要区分列的名字    strColumns 存储的是预算编制表的列          
            string strColumns = string.Empty;
            string strShareColumn = "NO";

            strColumns = "{width:100,field:'Sum',title:'Sum',hidden:'true'},{width:100,field:'Add',title:'Add',hidden:'true'},{width:100,field:'GUID_BGCode',title:'GUID_BGCode',hidden:'true'}," +
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

                if (item.BGItemKey == "06")
                {
                    strShareColumn = strFieldBGItemGUID.ToLower();
                }
                // 
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
            string strData = string.Empty;
            try
            {

           
            strData = GetBG_DetailDataEx(guidBG_SetupGUID.ToString(), viewBG_Main, ref ListDetail, ref listBG_SetupBGCodeView, ref listBG_SetupDetail,strPreGuid,strPreScope);
            }
            catch (Exception ex)
            {

                throw;
            }

            if (string.Empty == strData)
            {
                return string.Empty;
            }

            //查找这个预算设置 获取默认显示的货币单位
            string strMultiple = "";
            BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.GUID == guidBG_SetupGUID);
            if (null != objBG_Setup)
            {
                Infrastructure.SS_MoneyUnit objSS_MoneyUnit = this.InfrastructureContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == objBG_Setup.GUID_MoneyUnit);
                if (null != objSS_MoneyUnit)
                {
                    strMultiple = objSS_MoneyUnit.UnitMultiple.ToString();
                }
            }
            string strCurrMoney = "";
            string strPreMoney = "";
            string strMoney = "";
            if (viewBG_Main != null)
            {
                strCurrMoney = string.Format("{0:F2}", viewBG_Main.Total_BG_CurYear);
                strPreMoney = string.Format("{0:F2}", viewBG_Main.Total_BG_PreYear);
                strMoney = ",\"CurrMoney\":\"" + strCurrMoney + "\",\"PreMoney\":\"" + strPreMoney + "\"";
            }
            string strJson = "{\"success\": true, " + strColumns + ",\"data\":\"" + strData + "\",\"strShareColumn\":\"" +
                strShareColumn + "\",\"Multiple\":\"" + strMultiple + "\"" + strMoney + "}";
            return strJson;

        }
        /*
         *      函数功能:   为BG_Main 的属性赋默认值
         *       author:    dongsheng.zhang
         *         日期:    2014-4-14
         * 
         */

        private void InitBG_Main(ref BG_MainView viewBG_Main)
        {
            // 预算编号
            //string strDocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)viewBG_Main.GUID_DW, defaultGUID_YWType, viewBG_Main.DocDate.ToString());
            //viewBG_Main.DocNum = strDocNum;



            // 设置预算设置 默认值为 内部项目支出 
            BG_Setup bg_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == "08");
            viewBG_Main.GUID_BGSetup = bg_Setup.GUID;
            viewBG_Main.BGSetupKey = bg_Setup.BGSetupKey;
            viewBG_Main.BGSetupName = bg_Setup.BGSetupName;

            // 设置货币单位 默认值为 千元  这个是不是在页面设置初始值会更好
            var ss_MoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == bg_Setup.GUID_MoneyUnit);
            if (null == ss_MoneyUnit)
            {
                return;
            }
            viewBG_Main.GUID_MoneyUnit = ss_MoneyUnit.GUID;
            viewBG_Main.MoneyUnitKey = ss_MoneyUnit.MoneyUnitKey;
            viewBG_Main.MoneyUnitName = ss_MoneyUnit.MoneyUnitName;



            // 设置单据state BGPeriod  Invalid verson
            viewBG_Main.DocState = 0;
            viewBG_Main.BGPeriod = 0;
            viewBG_Main.Invalid = true;
            viewBG_Main.DocVerson = "1";

            // 设置金额
            viewBG_Main.Total_BG = 0;
            viewBG_Main.Total_BG_CurYear = 0;
            viewBG_Main.Total_BG_PreYear = 0;
        }

        // 有些属性无需根据前台获得，在后台赋值即可

        private void SetBG_MainFeild(ref BG_Main objBG_Main)
        {
            // 设置 业务，单据，UI 的type ，为了和老平台兼容，这些属性可以直接赋值

            objBG_Main.GUID_YWType = defaultGUID_YWType;
            objBG_Main.GUID_DocType = defaultGUID_DocType;
            objBG_Main.GUID_UIType = defaultGUID_UIType;

            // 任何时候，修改时间都可以用当前时间,不考虑凌晨前打开页面，凌晨后保存的极端情况

            objBG_Main.ModifyDate = DateTime.Now;
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
        // 修改一个BG_Main
        private void ModifyBG_Main( BG_Main objBG_Main)
        {
            JsonModel jsonModel = new JsonModel();
            jsonModel.m = objBG_Main.Pick();

            BG_Main main = new BG_Main();
            BG_Detail tempDetail = new BG_Detail();
            JsonGridModel jgm = new JsonGridModel(tempDetail.ModelName());
            jsonModel.d.Add(jgm);
            foreach (BG_Detail detail in objBG_Main.BG_Detail)
            {
                List<JsonAttributeModel> picker = detail.Pick();
                jgm.r.Add(picker);
            }

            // 一定要从数据库中获得数据对象，否则修改将出现异常
            main = this.BusinessContext.BG_Main.Include("BG_Detail").FirstOrDefault(e => e.GUID == objBG_Main.GUID);
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this, this.OperatorId);

            string detailModelName = tempDetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BG_Detail detail in main.BG_Detail)
                {
                    this.BusinessContext.DeleteConfirm(detail);
                }
            }
            else
            {
               
                List<BG_Detail> detailList = new List<BG_Detail>();
                foreach (BG_Detail detail in main.BG_Detail)
                {
                    detailList.Add(detail);
                }

                foreach (BG_Detail detail in detailList)
                {
                    if (detail.GUID == Guid.Parse("7F82AA3F-D0D2-45DB-984A-E74B56765EFB"))
                    {
                        int i = 0;
                    }
                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {                                              
                        detail.Fill(row);
                        this.BusinessContext.ModifyConfirm(detail);
                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    AddDetail(main, row, Grid.r.IndexOf(row));
                }
                
            }
            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
        }
        /*
         *      函数功能:   从前台获取BG_Main和detail数据，并保存
         *       author:    dongsheng.zhang
         *         日期:    2014-4-21
         * 
         */
        public JsonModel SaveBG_Main(string strBG_MainData, string strDetailData, string strState, string strBGYear, string strMoneyUnitGUID, string strPreGuid,string strPreScope,ref VerifyResult vr)
        {
            JsonModel result = new JsonModel();
            try
            {
                string strMsg = string.Empty;
                VerifyResult vResult = null;
                Guid Guid_BG_Main = Guid.Empty;
                Guid guidNew = Guid.Empty;
                // 获得BG_Main信息
                List<ValuePair> BG_MainInfo = SplitJsonObject(strBG_MainData);
                if ("3" == strState)    //进行删除操作
                {
                    string strGUID_BG_Main = string.Empty;
                    var vp = BG_MainInfo.Find(e => e.strKey == "BG_Main_GUID");
                    if(vp!=null)
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
                else if ("1" == strState || "2" == strState || "22" == strState)
                {
                    // 获得BG_Detail信息
                    int indexFirst = strDetailData.IndexOf('[');
                    int indexLast = strDetailData.LastIndexOf(']');
                    string strData = strDetailData.Substring(indexFirst, indexLast - indexFirst + 1);
                    List<string> ListDetail = SplitJsonArray(strData);

                    // 最后一行是合计，不用解析
                    List<List<ValuePair>> ListDetailInfo = new List<List<ValuePair>>();
                    int iCount = ListDetail.Count;
                    for (int i = 0; i < iCount - 1; i++)
                    {
                        List<ValuePair> BG_Detail = SplitJsonObject(ListDetail[i]);
                        ListDetailInfo.Add(BG_Detail);
                    }

                    // 组装 BG_Main ，组装好的包含BG_Detail
                    BG_Main objBG_Main = AssemblingBG_Main(BG_MainInfo, ListDetailInfo, strBGYear);
                    Guid_BG_Main = objBG_Main.GUID;
                    Guid GUID_MoneyUnit = new Guid(strMoneyUnitGUID);
                    var objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == GUID_MoneyUnit);
                    // 转换货币单位
                    //ConvertMoneyUnit(ref objBG_Main, objMoneyUnit);
                    SetBG_MainFeild(ref objBG_Main);
                    if ("1" == strState)
                    {
                        vResult = InsertVerify(objBG_Main);

                        objBG_Main.Invalid = true;
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            lock (obj)
                            {
                                objBG_Main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)objBG_Main.GUID_DW, defaultGUID_YWType, objBG_Main.DocDate.ToString());
                                foreach (var item in objBG_Main.BG_Detail) item.GUID = Guid.NewGuid();
                                this.BusinessContext.BG_Main.AddObject(objBG_Main);
                                this.BusinessContext.SaveChanges();
                                if(strPreGuid!="")
                                {
                                    if(strPreScope=="ysfp")
                                    {
                                        WorkFlowAPI.SaveBGDocTransToProcess(new Guid(strPreGuid), objBG_Main.GUID, BGTransStatus.预算分配到预算编制);
                                    }
                                    else if (strPreScope == "yscszsz")
                                    {
                                        WorkFlowAPI.SaveBGDocTransToProcess(new Guid(strPreGuid), objBG_Main.GUID, BGTransStatus.预算初始值到预算编制);//BGTransStatus.预算分配到预算初始值
                                    }
                                }
                            }
                        }
                    }
                    else if ("2" == strState)
                    {
                        vResult = ModifyVerify(objBG_Main);//修改验证
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            ModifyBG_Main(objBG_Main);
                        }
                    }
                    else if ("22" == strState)
                    {
                        // 预算调整，新生成一个预算，主表的制单人要改变，制单日期要改变,版本号加1,先将原来的预算编制设置为无效
                        BG_Main modifyMain = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID == Guid_BG_Main);
                        if (null != modifyMain)
                        {
                            modifyMain.Invalid = false;
                            this.BusinessContext.ModifyConfirm(modifyMain);
                        }
                        this.BusinessContext.SaveChanges();
                        objBG_Main.GUID = Guid.NewGuid();
                        guidNew = objBG_Main.GUID;
                        objBG_Main.MakeDate = DateTime.Now;
                        objBG_Main.DocDate = DateTime.Now;
                        foreach (BG_Detail item in objBG_Main.BG_Detail)
                        {
                            item.GUID = Guid.NewGuid();
                            item.GUID_BG_Main = objBG_Main.GUID;
                        }
                        vResult = InsertVerify(objBG_Main);
                        objBG_Main.Invalid = true;
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {

                            SS_Operator Operator = OperatorId == Guid.Empty ? null : this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == this.OperatorId);
                            if(null!=Operator)
                            {
                                objBG_Main.GUID_Maker = Operator.GUID;
                            }
                            objBG_Main.DocVerson = (Int32.Parse(objBG_Main.DocVerson) + 1).ToString();

                            lock (obj)
                            {
                                objBG_Main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)objBG_Main.GUID_DW, defaultGUID_YWType, objBG_Main.DocDate.ToString());
                                this.BusinessContext.BG_Main.AddObject(objBG_Main);
                                this.BusinessContext.SaveChanges();
                            }
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
                    else if (strState == "1" || strState=="2") 
                    {
                        result = this.Retrieve(Guid_BG_Main);
                        strMsg = "保存成功！";
                    }
                    else if("22"==strState)
                    {
                        if (guidNew == Guid.Empty)
                        {
                            result.result = JsonModelConstant.Error;
                            result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                        }
                        else
                        {
                            result = this.Retrieve(guidNew);
                            strMsg = "保存成功！";
                        }
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
        // 根据货币单位，将bg_detail的 total转换成相应的值

        private void ConvertMoneyUnit(ref BG_Main objBG_Main, BusinessModel.SS_MoneyUnit objMoneyUnit)
        {
            objBG_Main.Total_BG_PreYear = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG_PreYear, objMoneyUnit.UnitMultiple);
            objBG_Main.Total_BG_CurYear = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG_CurYear, objMoneyUnit.UnitMultiple);
            objBG_Main.Total_BG = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN((double)objBG_Main.Total_BG, objMoneyUnit.UnitMultiple);
            List<BG_Detail> ListBG_Detail = objBG_Main.BG_Detail.ToList();
            foreach (BG_Detail item in ListBG_Detail)
            {
                double dblTotal = (double)item.Total_BG;
                dblTotal = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN(dblTotal, objMoneyUnit.UnitMultiple);
                item.Total_BG = dblTotal;
            }
        }
        // 根据前台返回的数据组装BG_Main
        private BG_Main AssemblingBG_MainEx(List<ValuePair> BG_MainInfo)
        {
            BG_Main objBG_Main = new BG_Main();
            int iBG_MainCount = BG_MainInfo.Count;
            foreach (ValuePair item in BG_MainInfo)
            {
                switch (item.strKey)
                {
                    case "BG_Main_GUID":
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
                            if (!string.IsNullOrEmpty(item.strValue))
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

        // 根据前台返回的数据组装BG_Main，同时将BG_Detail加载
        private BG_Main AssemblingBG_Main(List<ValuePair> BG_MainInfo, List<List<ValuePair>> ListDetailInfo, string strBGYear)
        {
            BG_Main objBG_Main = AssemblingBG_MainEx(BG_MainInfo);
            if (null == objBG_Main)
            {
                return null;
            }

            foreach (List<ValuePair> item in ListDetailInfo)
            {
                List<BG_Detail> ListDetail = AssemblingBG_Detail(item);
                if(null==ListDetail)
                {
                    continue;
                }
                foreach (BG_Detail detail in ListDetail)
                {
                    detail.GUID_BG_Main = objBG_Main.GUID;
                    detail.BGYear = Int32.Parse(strBGYear);
                    objBG_Main.BG_Detail.Add(detail);
                }
            }
            return objBG_Main;
        }

        // 组装一行BG_Detail，每一行，前三个数据对是固定的，从第四行开始，就是BG_Item的GUID,BG_Detail的total_bg，如果有BGMemo，则夹带一个BGMemo
        private List<BG_Detail> AssemblingBG_Detail(List<ValuePair> ListDetailInfo)
        {
            List<BG_Detail> ListDetail = new List<BG_Detail>();
            BG_Detail detail = null;
            Guid GUID_BGCoe = Guid.Empty;
            foreach (ValuePair item in ListDetailInfo)
            {
                switch (item.strKey)
                {
                    case "Sum":
                        {
                            // 如果是非末级，那么一定不是NO，非末级的数据是不需要保存的
                            //if(item.strValue!="NO"){
                            //    return null;
                            //}
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
                                detail = new BG_Detail();
                                detail.GUID_BGCode = GUID_BGCoe;
                                if ("" == item.strValue)        //新增
                                {
                                    //detail.GUID = Guid.NewGuid();
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

        public string GetDocNumber(string strGUID_DW, string strDocDate)
        {
            Guid guidDW = new Guid(strGUID_DW);
            return CreateDocNumber.GetNextDocNum(this.BusinessContext, guidDW, defaultGUID_YWType, strDocDate);
        }

        
        /// <summary>
        /// 返回实体
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            JsonModel jmodel = new JsonModel();
            try
            {
                BG_MainView main = this.BusinessContext.BG_MainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    BusinessModel.SS_MoneyUnit moneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == main.GUID_MoneyUnit);
                    if(null == moneyUnit){
                        jmodel.result = JsonModelConstant.Error;
                        jmodel.s = new JsonMessage("提示", "获取数据错误！", JsonModelConstant.Error);
                        return jmodel;
                    }
                    //main.Total_BG = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)main.Total_BG,moneyUnit.UnitMultiple);
                    //main.Total_BG_PreYear = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)(main.Total_BG_PreYear),moneyUnit.UnitMultiple);
                    //main.Total_BG_CurYear = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit((double)(main.Total_BG_CurYear),moneyUnit.UnitMultiple);

                    jmodel.m = main.Pick();

                    BG_Detail BG_Detail = this.BusinessContext.BG_Detail.FirstOrDefault(e => e.GUID_BG_Main == guid);
                    // 加入BG_Detail的 BGYear
                    JsonAttributeModel jam = new JsonAttributeModel();
                    jam.m = "BG_Detail";
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


                    BG_DetailView dModel = new BG_DetailView();
                    dModel.FillDetailDefault<BG_DetailView>(this, this.OperatorId);
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
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            BG_Main main = new BG_Main();
            // main.GUID = Guid.Empty;  
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();

            main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                BG_Detail temp = new BG_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 0;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        AddDetail(main, row, orderNum);
                    }
                }
            }
            this.BusinessContext.BG_Main.AddObject(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 添加明细信息
        /// </summary>
        /// <param name="main"></param>
        private void AddDetail(BG_Main main, List<JsonAttributeModel> row, int orderNum)
        {


            BG_Detail temp = new BG_Detail();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            // 科目
            SS_BGCode bgcode = new SS_BGCode();
            bgcode.Fill(row);
            if (bgcode != null)
            {
                if (!bgcode.GUID.IsNullOrEmpty())
                {
                    temp.GUID_BGCode = bgcode.GUID;
                }
            }
            //预算条目
            BG_Item bgitem = new BG_Item();
            bgitem.Fill(row);
            if (bgitem != null && !bgitem.GUID.IsNullOrEmpty())
            {
                temp.GUID_Item = bgitem.GUID;
            }
            temp.GUID = Guid.NewGuid();
            main.BG_Detail.Add(temp);

        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>GUID</returns>
        protected override Guid Modify(JsonModel jsonModel)
        {
            DateTime orgDateTime = DateTime.MinValue;
            if (jsonModel.m == null) return Guid.Empty;
            BG_Main main = new BG_Main(); BG_Detail tempdetail = new BG_Detail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.BG_Main.Include("BG_Detail").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = (DateTime)main.DocDate;
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this, this.OperatorId);
            //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
            if (IsDateChange(orgDateTime, (DateTime)main.DocDate))
            {
                main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, (Guid)main.GUID_DW, main.GUID_YWType, main.DocDate.ToString());
            }

            string detailModelName = tempdetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BG_Detail detail in main.BG_Detail) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {
                List<BG_Detail> detailList = new List<BG_Detail>();
                foreach (BG_Detail detail in main.BG_Detail)
                {
                    detailList.Add(detail);
                }
                var orderNum = 0;
                foreach (BG_Detail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {

                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);

                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    orderNum++;
                    AddDetail(main, row, orderNum);
                }
            }
            this.BusinessContext.ModifyConfirm(main);
            this.BusinessContext.SaveChanges();
            return main.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            BG_Main main = this.BusinessContext.BG_Main.Include("BG_Detail").FirstOrDefault(e => e.GUID == guid);

            List<BG_Detail> details = new List<BG_Detail>();

            foreach (BG_Detail item in main.BG_Detail)
            {
                details.Add(item);
            }

            foreach (BG_Detail item in details) { BusinessContext.DeleteConfirm(item); }

            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="status">状态1表示新建 2表示修改 3表示删除</param>
        /// <param name="jsonModel">Json Model</param>
        /// <returns>JsonModel</returns>
        public override JsonModel Save(string status, JsonModel jsonModel)
        {
            JsonModel result = new JsonModel();
            try
            {
                Guid value = jsonModel.m.Id(new BG_Main().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Insert(jsonModel);
                        }
                        break;
                    case "2": //修改
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Modify(jsonModel);
                        }
                        break;
                    case "3": //删除
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            this.Delete(value);
                        }
                        break;

                }
                if (string.IsNullOrEmpty(strMsg))
                {
                    if (status == "3")//删除时返回默认值
                    {
                        result = this.New();
                        strMsg = "删除成功！";
                    }
                    else
                    {
                        result = this.Retrieve(value);
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
        /// <summary>
        /// 判断编号是否设置日期并且是否改变
        /// </summary>
        /// <param name="orgDateTime">原日期</param>
        /// <param name="currentDateTime">当前修改日期</param>
        /// <returns>Bool</returns>
        private bool IsDateChange(DateTime orgDateTime, DateTime currentDateTime)
        {
            bool returnValue = false;
            if (currentDateTime != null && currentDateTime != DateTime.MinValue)
            {

                SS_DocNumber dnModel = this.BusinessContext.SS_DocNumber.FirstOrDefault();
                if ((bool)dnModel.IsYear || (bool)dnModel.IsMonth)//生成的编号设置了时间
                {
                    if (orgDateTime.Year != currentDateTime.Year || orgDateTime.Month != currentDateTime.Month)
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// 更改单据状态

        /// </summary>
        /// <param name="guid">GUID</param>
        /// <param name="docState">单据状态</param>
        /// <returns>Bool</returns>
        public override bool UpdateDocState(Guid guid, EnumType.EnumDocState docState)
        {
            BG_Main main = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                main.DocState = (int)docState;
                this.BusinessContext.SaveChanges();
                return true;
            }
            return false;
        }
        public List<object> Reference(string strDWKey,string strDepKey,string strProKey)
        {
            
            List<object> list = new List<object>();
            IQueryable<BG_MainView> main = null;
            if (strProKey=="")
            {
                main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true 
                && e.DWKey == strDWKey && e.DepartmentKey == strDepKey && e.ProjectKey == null);
            }
            else
            {
                main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true
                && e.DWKey == strDWKey && e.DepartmentKey == strDepKey && e.ProjectKey == strProKey);
            }
            
            List<BG_MainView> s = main.ToList();
            // 获得权限
            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            IntrastructureFun objIF = new Infrastructure.IntrastructureFun();
            var DepartmentAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            var PersonAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            var ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            IQueryable<BG_MainView> mainSetWithOutProject = main.Where(e => e.GUID_Project == null && PersonAuth.Contains((Guid)e.GUID_Person)
                && DepartmentAuth.Contains((Guid)e.GUID_Department));
            //List<BG_MainView> s1 = mainSetWithOutProject.ToList();
            IQueryable<BG_MainView> mainSetWithProject = main.Where(e => e.GUID_Project != null && PersonAuth.Contains((Guid)e.GUID_Person)
                && ProjectAuth.Contains((Guid)e.GUID_Project));
            //List<BG_MainView> s2 = mainSetWithProject.ToList();
            IQueryable<BG_MainView> allBG_Main = mainSetWithOutProject.Union(mainSetWithProject);
            list = allBG_Main.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                BGSetupName = e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.Maker,
                e.MakeDate
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();
            return list;
        }
        //历史
        /// <summary>
        ///  如果是项目预算，则当前操作员需要拥有项目的权限，和项目负责人的权限
        ///  如果是基本支出，则当前操作员需要拥有预算负责人和部门的权限
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            List<object> list = new List<object>();
            //IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.DocTypeUrl == null);
            IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true && e.DocTypeUrl == "ysbz");
            List<BG_MainView> s = main.ToList();
            // 获得权限
            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            IntrastructureFun objIF = new Infrastructure.IntrastructureFun();
            var DepartmentAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            var PersonAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            var ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            IQueryable<BG_MainView> mainSetWithOutProject = main.Where(e => e.GUID_Project==null && PersonAuth.Contains((Guid)e.GUID_Person)
                && DepartmentAuth.Contains((Guid)e.GUID_Department));

            IQueryable<BG_MainView> mainSetWithProject = main.Where(e => e.GUID_Project!=null && PersonAuth.Contains((Guid)e.GUID_Person)
                && ProjectAuth.Contains((Guid)e.GUID_Project));

            IQueryable<BG_MainView> allBG_Main = mainSetWithOutProject.Union(mainSetWithProject);
            list = allBG_Main.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToShortDateString(),
                BGSetupName = e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.Maker,
                MakeDate = ((DateTime)e.MakeDate).ToShortDateString()
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();
            return list;

        }

        public List<object> HistoryEx(SearchCondition conditions)
        {
            IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.Invalid == true );
            List<object> list = new List<object>();
            list = main.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                e.DocVerson,
                e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.PersonName

            }).OrderByDescending(e => e.DocNum).ToList<object>();

            return list;
        }
        /// <summary>
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<object> BG_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单

            IQueryable<BG_DetailView> detail = this.BusinessContext.BG_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                main = main.Where(e => e.GUID_Modifier == this.OperatorId);
                
            }
            if (historyconditions != null)
            {

                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件


                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                    {
                        main = main.Where(e => e.DocState == null || e.DocState == 0);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                    {
                        main = main.Where(e => e.DocState == 999 || e.DocState == -1);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//审批中
                    {
                        main = main.Where(e => e.DocState != 999 && e.DocState != -1 && e.DocState != null && e.DocState != 0);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
                    {
                        //在凭证主表中 存在核销明细表中的主表信息


                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//未生成凭证
                    {
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != 9);

                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == 9);

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_BG_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_BG) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     //join s in this.InfrastructureContext
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.BGSetupName, m.ProjectName, m.ProjectKey, m.BGStepName, m.BGTypeName, m.DepartmentName, m.Maker, m.DocDate, m.PersonName, d.Total_BX, m.MakeDate });

            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                BGSetupName = e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.Maker,
                e.MakeDate
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;

        }
        /// <summary>
        /// 参照
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public List<object> CZ_History(SearchCondition conditions)
        {
            JsonModel jsonmodel = new JsonModel();
            HistoryCondition historyconditions = (HistoryCondition)conditions;
            IQueryable<BG_MainView> main = this.BusinessContext.BG_MainView.Where(e => e.DocTypeUrl == historyconditions.ModelUrl);//或者用ModelUrl 02指现金报销单

            IQueryable<BG_DetailView> detail = this.BusinessContext.BG_DetailView;
            List<SS_Department> depList = new List<SS_Department>();
            List<SS_DW> dwList = new List<SS_DW>();
            List<SS_Project> projectList = new List<SS_Project>();
            List<SS_BGCode> bgcodeList = new List<SS_BGCode>();
            List<SS_ProjectClass> projectclassList = new List<SS_ProjectClass>();
            if (this.OperatorId.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                main = main.Where(e => e.GUID_Modifier == this.OperatorId);
            }
            if (historyconditions != null)
            {

                if (!string.IsNullOrEmpty(historyconditions.Year) && historyconditions.Year != "0")
                {
                    int y;
                    if (int.TryParse(historyconditions.Year, out y))
                    {

                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Year == y);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.Month) && historyconditions.Month != "0")
                {
                    int m;
                    if (int.TryParse(historyconditions.Month, out m))
                    {
                        main = main.Where(e => !e.DocDate.IsNullOrEmpty() && ((DateTime)e.DocDate).Month == m);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.DocNum))
                {
                    main = main.Where(e => e.DocNum.Contains(historyconditions.DocNum));
                }
                #region 审批状态条件


                if (!string.IsNullOrEmpty(historyconditions.ApproveStatus))//审批状态
                {
                    if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.NotApprove).ToString())//未审批
                    {
                        main = main.Where(e => e.DocState == null || e.DocState == 0);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//已审批
                    {
                        main = main.Where(e => e.DocState == 999 || e.DocState == -1);
                    }
                    else if (historyconditions.ApproveStatus == ((int)Common.EnumType.EnumApproveStatus.Approved).ToString())//审批中
                    {
                        main = main.Where(e => e.DocState != 999 && e.DocState != -1 && e.DocState != null && e.DocState != 0);
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CheckStatus))//支票状态
                {
                    if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未领取
                    {
                        //main.GUID in (select GUID_Doc from cn_checkdrawmain)
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CheckStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已领取
                    {
                        var guidList = this.BusinessContext.CN_CheckDrawMain.Select(e => e.GUID_Doc);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.WithdrawStatus))//提现状态
                {
                    if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.NotWithdraw).ToString())//未提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.WithdrawStatus == ((int)Common.EnumType.EnumWithdrawStatus.Withdrawed).ToString())//已提现
                    {
                        var guidList = this.BusinessContext.CN_CashRequirements.Select(e => e.GUID_DocMain);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.PayStatus))//付款状态
                {
                    if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.NotPay).ToString())//未付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.PayStatus == ((int)Common.EnumType.EnumPayStatus.Payed).ToString())//已付款
                    {
                        var guidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => guidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CertificateStatus))//凭证状态
                {
                    if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.NotCertificate).ToString())//未生成凭证
                    {
                        //在凭证主表中 存在核销明细表中的主表信息


                        var guidList = this.BusinessContext.HX_Detail.Where(e => this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        main = main.Where(e => !guidList.Contains(e.GUID));
                    }
                    else if (historyconditions.CertificateStatus == ((int)Common.EnumType.EnumCertificateStatus.Certificated).ToString())//未生成凭证
                    {
                        var pzguidList = this.BusinessContext.HX_Detail.Where(e => !this.BusinessContext.CW_PZMain.Select(m => m.GUID).Contains(e.GUID_HX_Main)).Select(e => e.GUID_Main);
                        var detailGuidList = this.BusinessContext.HX_Detail.Select(e => e.GUID_Main);
                        main = main.Where(e => pzguidList.Contains(e.GUID) || !detailGuidList.Contains(e.GUID));
                    }
                }
                if (!string.IsNullOrEmpty(historyconditions.CancelStatus))//作废
                {
                    if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.NotCancel).ToString())//未作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState != 9);

                    }
                    else if (historyconditions.CancelStatus == ((int)Common.EnumType.EnumCancelStatus.Canceled).ToString())//已作废
                    {
                        //作废 DocState 为9
                        main = main.Where(e => e.DocState == 9);

                    }
                }
                #endregion

                if (!string.IsNullOrEmpty(historyconditions.treeModel) && (historyconditions.treeValue != null && historyconditions.treeValue != Guid.Empty))
                {
                    switch (historyconditions.treeModel.ToLower())
                    {
                        case "ss_department":
                            SS_Department dep = new SS_Department();
                            dep.GUID = historyconditions.treeValue;
                            dep.RetrieveLeafs(this.InfrastructureContext, ref depList);
                            var depguid = depList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Department != null && depguid.Contains((Guid)e.GUID_Department));
                            //detail = detail.Where(e => depguid.Contains(e.GUID_Department));
                            break;
                        case "ss_dw":
                            SS_DW dw = new SS_DW();
                            dw.GUID = historyconditions.treeValue;
                            dw.RetrieveLeafs(this.InfrastructureContext, ref dwList);
                            var dwguid = dwList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_DW != null && dwguid.Contains((Guid)e.GUID_DW));
                            break;
                        case "ss_project":
                            SS_Project projectModel = new SS_Project();
                            projectModel.GUID = historyconditions.treeValue;
                            projectModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectGUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectGUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_projectclass":
                            SS_ProjectClass projectclassModel = new SS_ProjectClass();
                            projectclassModel.GUID = historyconditions.treeValue;
                            projectclassModel.RetrieveLeafs(this.InfrastructureContext, ref projectList);
                            var projectUID = projectList.Select(e => e.GUID);
                            main = main.Where(e => e.GUID_Project != null && projectUID.Contains((Guid)e.GUID_Project));
                            break;
                        case "ss_bgcode":
                            SS_BGCode bgcodeModel = new SS_BGCode();
                            bgcodeModel.GUID = historyconditions.treeValue;
                            bgcodeModel.RetrieveLeafs(this.InfrastructureContext, ref bgcodeList);
                            var bgcodeGUID = bgcodeList.Select(e => e.GUID);
                            detail = detail.Where(e => bgcodeGUID.Contains(e.GUID_BGCode));
                            break;
                        case "ss_person":
                            main = main.Where(e => e.GUID_Person == historyconditions.treeValue);
                            break;
                    }
                }


            }
            //明细信息
            var dbdetai = from a in detail
                          group a by a.GUID_BG_Main into g
                          select new { GUID_BX_Main = g.Key, Total_BX = g.Sum(a => a.Total_BG) };
            var o = (from d in dbdetai
                     join m in main on d.GUID_BX_Main equals m.GUID //into temp
                     //join s in this.InfrastructureContext
                     where d.GUID_BX_Main != null && m.GUID != null
                     select new { m.GUID, m.DocNum, m.BGSetupName, m.ProjectName, m.ProjectKey, m.BGStepName, m.BGTypeName, m.DepartmentName, m.Maker, m.DocDate, m.PersonName, d.Total_BX, m.MakeDate });
            
            var mainList = o.AsEnumerable().Select(e => new
            {
                e.GUID,
                DocNum = e.DocNum == null ? "" : e.DocNum,
                DocDate = ((DateTime)e.DocDate).ToString("yyyy-MM-dd"),
                BGSetupName = e.BGSetupName,
                e.ProjectName,
                e.ProjectKey,
                e.BGStepName,
                e.BGTypeName,
                e.DepartmentName,
                e.Maker,
                e.MakeDate
            }).OrderByDescending(e => e.MakeDate).OrderByDescending(e => e.DocNum).ToList<object>();

            return mainList;

        }

        /// <summary>
        /// 根据ID判断是否存在
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool IsExistByID(Guid guid)
        {
            var model = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID == guid);
            return model == null ? false : true;

        }
        public Guid GetBGMain(Guid bg_assignGUID)
        {
            var model = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID_BG_Assign == bg_assignGUID);
            return model == null ? Guid.NewGuid() : model.GUID;

        }
        public bool GetBGMainExist(Guid bg_assignGUID,out Guid Id)
        {
            var model = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID_BG_Assign == bg_assignGUID);
            if (model != null)
            {
                Id = model.GUID;
            }
            else {
                Id = Guid.Empty;
            }
            return model == null;

        }

        #region 验证
        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="jsonModel">JsonModel</param>
        /// <param name="status">状态</param>
        /// <returns>string</returns>
        private string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            BG_Main main = null; ; //new BX_Main();
            switch (status)
            {
                case "1": //新建
                    main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(main);//
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    main = LoadMain(jsonModel);
                    vResult = ModifyVerify(main);//修改验证
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new BG_Main().ModelName());
                    vResult = DeleteVerify(value);
                    strMsg = DataVerifyMessage(vResult);
                    break;

            }
            return strMsg;
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
        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private BG_Main LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            BG_Main main = new BG_Main();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                BG_Detail temp = new BG_Detail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new BG_Detail();
                        temp.Fill(row);

                        main.BG_Detail.Add(temp);
                    }
                }
            }

            return main;
        }
        /// <summary>
        /// 明显表验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetail(BG_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<BG_Detail> detailList = new List<BG_Detail>();
            foreach (BG_Detail item in data.BG_Detail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BG_Detail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_BX_Detail(item, rowIndex);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            else
            {
                resultList.Add(new ValidationResult("", "请添加明细科目信息！"));

            }

            return resultList;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(BG_Main data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BG_Main mModel = data;
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

            if(mModel.GUID_BGSetup.IsNullOrEmpty())
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
                        str = "当前预算设置是基本支出预算，请不要选择项目!";
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
            SS_ProjectView viewProject = this.InfrastructureContext.SS_ProjectView.FirstOrDefault(e=> e.GUID == mModel.GUID_Project);
            if (null == viewProject)
            {
                //if (!mModel.GUID_FunctionClass.IsNullOrEmpty())
                //{
                //    // 没有选择项目，那么就不要选择功能分类
                //    str = "没有选择项目，功能分类也不能选择!";
                //    resultList.Add(new ValidationResult("", str));
                //}
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
        private List<ValidationResult> VerifyResult_BX_Detail(BG_Detail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BG_Detail item = data;
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

        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            BG_Main model = (BG_Main)data;
            

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

            if (result._validation.Count > 0) return result;

            var ret = 预算编制单据唯一性验证(data);
            if (ret != null) result._validation.Add(ret);

            return result;

        }

        /// <summary>
        /// 数据从数据库删除前验证

        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BG_Main bxMain = new BG_Main();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
                str = "请选择删除项！";
                resultList.Add(new ValidationResult("", str));
                
            }
            else
            {
                object g;
                if (!Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
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


            BG_Main main = this.BusinessContext.BG_Main.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                if (main.DocState == int.Parse("9") || main.DocState == int.Parse("999"))
                {
                    str = "此单已经作废！不能删除！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            result._validation = resultList;
            return result;
        }

        /// <summary>
        /// 数据更新到数据库验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult ModifyVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BG_Main model = (BG_Main)data;
            BG_Main orgModel = this.BusinessContext.BG_Main.Include("BG_Detail").FirstOrDefault(e => e.GUID == model.GUID);
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
                if (orgModel.GUID_BG_Assign == null)//2014-7-30 改
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "此单据没有保存对应的预算分配信息！"));
                    result._validation = resultList;
                    return result;
                }

                FlowNodeModel objFnm = WorkFlowAPI.GetCurNodeByDocId((Guid)orgModel.GUID_BG_Assign, out strErr);
                if (null == objFnm)
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "此单据虽然在流程中，但找不到所在节点"));
                    result._validation = resultList;
                    return result;
                }
                //else if (objFnm.WorkFlowNodeName != "预算编制")
                //{
                //    List<ValidationResult> resultList = new List<ValidationResult>();
                //    resultList.Add(new ValidationResult("", "此单据不在预算编制节点上，因此不能修改"));
                //    result._validation = resultList;
                //    return result;
                //}
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
                        if (YSTZ) return result;//hanyx 2016 1 4 预算调整不用验证是不是该节点的操作员
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


        private ValidationResult 预算编制单据唯一性验证(object data)
        {
            string sql = string.Empty;
            ValidationResult result = null;
            BG_Main model = (BG_Main)data;
            if (model.BG_Detail.Count == 0) return null;
            //获得预算年
            var BGYear = model.BG_Detail.ElementAt(0).BGYear;
            if (BGYear == null) return null;
            //获得部门
            var DepartmentId = model.GUID_Department;
            //获得单位
            var DwId = model.GUID_DW;
            //获取项目
            var ProjectId = model.GUID_Project;
            //获取预算设置
            var BgSetupId = model.GUID_BGSetup;
            //获取预算设置对象
            var BGSetup = this.InfrastructureContext.BG_SetupView.FirstOrDefault(e => e.GUID == BgSetupId);
            if (BGSetup.BGTypeKey == "01") //基本支出
            {
                sql = string.Format("select top 1 DocNum from BG_Main a left join BG_Detail b on a.GUID=b.GUID_BG_Main where DocState!=9 and Invalid=1 " +
                                  "and GUID_DW='{0}' and GUID_Department='{1}' and GUID_BGSetup='{2}' and BGYear={3}",
                                  DwId, DepartmentId, BgSetupId, BGYear);
            }
            else //项目支出
            {
                sql = string.Format("select top 1 DocNum from BG_Main a left join BG_Detail b on a.GUID=b.GUID_BG_Main where DocState!=9 and Invalid=1 " +
                                      "and GUID_DW='{0}' and GUID_Department='{1}' and GUID_BGSetup='{2}' and GUID_Project='{3}' and BGYear={4}",
                                      DwId, DepartmentId, BgSetupId, ProjectId, BGYear);
            }

            var docnums = this.BusinessContext.ExecuteStoreQuery<string>(sql).ToList();
            if (docnums.Count > 0)
            {
                string str = "这个预算编制已经存在，预算编号为: " + docnums[0];
                result = new ValidationResult("", str);
            }
            return result;
        }

        private ValidationResult 预算编制明细金额存在性验证(object data)
        {
            return null;
        }
        #endregion

    }

    public static class kkk
    {
        public static bool IsNullOrEmptyEx(this string obj)
        {
            if (string.IsNullOrEmpty(obj) == true) return true;
            if (obj.ToLower() == "null") return true;
            return false;
        }
    }
}
