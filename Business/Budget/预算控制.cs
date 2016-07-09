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

namespace Business.Budget
{
    public class 预算控制 : BaseDocument
    {
        private HashSet<int> SkipSet;
        private Dictionary<Guid, int> DicIndex;
        public 预算控制()
            : base()
        {
            SkipSet = new HashSet<int>();
            DicIndex = new Dictionary<Guid, int>();
        }
        public 预算控制(Guid OperatorId, string ModelUrl)
            : base(OperatorId, ModelUrl)
        {
            SkipSet = new HashSet<int>();
            DicIndex = new Dictionary<Guid, int>();
        }
        public override JsonModel Retrieve(Guid guid)
        { 
            JsonModel jmodel = new JsonModel();
            try
            {
                BG_ControlMainView main = this.BusinessContext.BG_ControlMainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();
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
        private void InitMain(ref BG_ControlMainView viewBG_ControlMain) 
        {
            BG_Setup bg_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == "07");
            var ss_MoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == bg_Setup.GUID_MoneyUnit);
            if (null == ss_MoneyUnit)
            {
                return;
            }
            viewBG_ControlMain.GUID_MoneyUnit = ss_MoneyUnit.GUID;
            viewBG_ControlMain.MoneyUnitKey = ss_MoneyUnit.MoneyUnitKey;
            viewBG_ControlMain.MoneyUnitName = ss_MoneyUnit.MoneyUnitName;

            string strTime = DateTime.Now.ToString();
            string strBGYear = strTime.Substring(0, 4);
            int iBGYear = Int32.Parse(strBGYear)+1;
            viewBG_ControlMain.CMYear = iBGYear;
            Guid GuidDW = viewBG_ControlMain.GUID_DW;
            Guid GuidDepartment = viewBG_ControlMain.GUID_Department;
            BG_ControlMainView objBG_ControlMain = this.BusinessContext.BG_ControlMainView.FirstOrDefault(e => e.CMYear == iBGYear &&
    e.GUID_DW == GuidDW && e.GUID_Department == GuidDepartment);           
            viewBG_ControlMain.ControlWayKey = "01";
            if (null != objBG_ControlMain)
            {
                viewBG_ControlMain.ControlWayKey = objBG_ControlMain.ControlWayKey;
                viewBG_ControlMain.GUID_MoneyUnit = objBG_ControlMain.GUID_MoneyUnit;
                if (objBG_ControlMain.GUID_Project != null)
                {
                    viewBG_ControlMain.GUID_Project = objBG_ControlMain.GUID_Project;
                }
            }
        }
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();
                BG_ControlMainView model = new BG_ControlMainView();
                model.FillDefault(this, this.OperatorId);
                InitMain(ref model);
                jmodel.m = model.Pick();
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetRowData(ref List<ValuePair> listValue)
        {
            string strRow = "{";
            string strPair = "";
            foreach(ValuePair item in listValue)
            {
                strPair = "";
                strPair = "\\\"" + item.strKey + "\\\":" + "\\\"" + item.strValue + "\\\"";
                if (strRow == "{")
                {
                    strRow += strPair;
                }
                else 
                {
                    strRow = strRow + "," + strPair;
                }
            }
            strRow += "}";
            return strRow;
        }

        private string GetMoneyMultiple(string strBGSetupKey)
        {
            BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == strBGSetupKey);
            if (null != objBG_Setup)
            {
                Infrastructure.SS_MoneyUnit objSS_MoneyUnit = this.InfrastructureContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == objBG_Setup.GUID_MoneyUnit);
                if (null != objSS_MoneyUnit)
                {
                    return objSS_MoneyUnit.UnitMultiple.ToString();
                }
            }
            return "";
        }

        // 根据搜索条件，listBG_SetupBGCode已经是有序的了，因此该算法是在listBG_SetupBGCode 有序的基础上进行的
        bool AnalyseBGCodeEx(ref List<BG_SetupBGCodeView> listBG_SetupBGCode,
            ref Dictionary<Guid, string> dicSumFormula, ref Dictionary<Guid, string> dicAddFormula, ref Dictionary<Guid, int> dicLevel)
        {
            int iLine = 0;
            Dictionary<Guid, int> DicLine = new Dictionary<Guid, int>();
            foreach (BG_SetupBGCodeView item in listBG_SetupBGCode)
            {
                DicLine.Add(item.GUID_BGCode, iLine);
                if (item.PSS_BGCodeGUID == null)
                {
                    dicAddFormula.Add(item.GUID_BGCode, "NO");
                    dicLevel.Add(item.GUID_BGCode, 0);
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
                        dicSumFormula.Add((Guid)item.PSS_BGCodeGUID, iLine.ToString());
                    }

                    // 计算自身
                    dicSumFormula.Add(item.GUID_BGCode, "NO");

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

        private void GetSortBG_SetupBGCodeView(ref List<BG_ControlDetailView> listBG_DetailView, ref List<BG_SetupBGCodeView> listBG_SetupBGCodeView)
        {
            HashSet<string> hsBGCodeKey = new HashSet<string>();
            HashSet<string> hsGuid = new HashSet<string>();
            foreach (BG_ControlDetailView item in listBG_DetailView)
            {
                if (!hsBGCodeKey.Contains(item.BGCodeKey))
                {
                    hsBGCodeKey.Add(item.BGCodeKey);
                    BG_SetupBGCodeView objBGCode = new BG_SetupBGCodeView();
                    objBGCode.GUID_BGCode = (Guid)item.GUID_BGCode;
                    objBGCode.BGCodeKey = item.BGCodeKey;
                    objBGCode.BGCodeName = item.BGCodeName;
                    objBGCode.PSS_BGCodeGUID = item.bgCodePGUID;
                    listBG_SetupBGCodeView.Add(objBGCode);
                    Guid? tmp = objBGCode.PSS_BGCodeGUID;
                    // 数据库中只存储末级科目，因此，要通过末级科目将父级科目加载进来
                    while (null != tmp && !hsGuid.Contains(tmp.ToString()))
                    {
                        SS_BGCode objTmpBGCode = this.InfrastructureContext.SS_BGCode.FirstOrDefault(e=> e.GUID == tmp);
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

        private string LoadDetailEx(ref List<BG_SetupBGCodeView> listBG_SetupBGCodeView, ref BG_ControlMainView objMain, ref BG_Main objBG_Main)
        {
            Infrastructure.BG_HandleMethod objHandleMethod = GetDefaultHandleMethod("01");
            BusinessModel.SS_MoneyUnit objMoneyUnit = null;
               
            Dictionary<Guid, string> dicSumFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, string> dicAddFormula = new Dictionary<Guid, string>();
            Dictionary<Guid, int> dicLevel = new Dictionary<Guid, int>();
            // 如果是从数据库中获取要展示的数据，那么预算科目要从detail中获取并加载
            List<BG_ControlDetailView> listDetail = null;
            if (null != objMain)
            {
                Guid guidMoneyUnit = (Guid)objMain.GUID_MoneyUnit;
                objMoneyUnit = objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == guidMoneyUnit);
                Guid guid_ControlMain = objMain.GUID;
                listBG_SetupBGCodeView.Clear();
                listDetail = this.BusinessContext.BG_ControlDetailView.Where(e => e.GUID_ControlMain == guid_ControlMain).ToList();
                listDetail = listDetail.OrderBy(e=> e.BGCodeKey).ToList();
                GetSortBG_SetupBGCodeView(ref listDetail, ref listBG_SetupBGCodeView);
            }
            // 获得每一行的数值计算信息，包括自己由哪些行汇总，自己汇总到哪一行
            AnalyseBGCodeEx(ref listBG_SetupBGCodeView, ref dicSumFormula, ref dicAddFormula, ref dicLevel);
            string strSumFormula = string.Empty;        // 需要进行汇总的行号
            string strAddFormula = string.Empty;        // 父级所在行号
            string strBgCodeName = string.Empty;
            int iCount = listBG_SetupBGCodeView.Count;
            string strRows = "";
            for (int i = 0; i < iCount;i++ )
            {
                BG_SetupBGCodeView item = listBG_SetupBGCodeView[i];
                BG_ControlDetailView viewControlDetail = null;
                if(null != objMain)
                {
                    viewControlDetail = listDetail.Find(e => e.GUID_BGCode == item.GUID_BGCode);
                }
                
                strSumFormula = dicSumFormula[item.GUID_BGCode];
                strAddFormula = dicAddFormula[item.GUID_BGCode];
                int iLevel = dicLevel[item.GUID_BGCode];
                strBgCodeName = GetShowBGCodeName(iLevel, item.BGCodeName);
                string strHandleMethod = "";
                string strGUID_ControlDetail = "";
                string strGUID_BGItem = "";
                string strIsControl = "√";
                string strTotalBG = "";
                string strIsScaleOrNot = "";
                string strControlEDScale = "";
                string strControlEDLimit = "";
                string strGroupID = "";
                string strGUID_HandleMethod = "";
                string strBGItemName = "";
                if (null != viewControlDetail)
                {
                    strGUID_ControlDetail = viewControlDetail.GUID.ToString();
                    strGUID_BGItem = viewControlDetail.GUID_BGItem.ToString();
                    strHandleMethod = viewControlDetail.GUID_HandleMethod.ToString();
                    strBGItemName = viewControlDetail.BGItemName;
                    if (!viewControlDetail.IsControl)
                    {
                        strIsControl = "";
                    }
                    double dblTotal = 0;
                    if (null != objBG_Main)
                    {
                        BG_Detail viewDetail = objBG_Main.BG_Detail.FirstOrDefault(e => e.GUID_BGCode == viewControlDetail.GUID_BGCode && e.GUID_Item == viewControlDetail.GUID_BGItem);
                        if (null != viewDetail)
                        {
                            dblTotal = (double)viewDetail.Total_BG;
                            strTotalBG = string.Format("{0:F2}", dblTotal);
                            strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        }
                    }
                    if ((bool)viewControlDetail.IsScaleOrNot)
                    {
                        strIsScaleOrNot = "√";
                        strControlEDScale = string.Format("{0:F2}", viewControlDetail.ControlED);
                        strControlEDScale = strControlEDScale == "0.00" ? "" : strControlEDScale;
                        double dblEd = (double)viewControlDetail.ControlED * dblTotal;
                        strControlEDLimit = string.Format("{0:F2}", dblEd);
                        strControlEDLimit = strControlEDLimit == "0.00" ? "" : strControlEDLimit;
                    }
                    else
                    {
                        double dblEd = (double)viewControlDetail.ControlED;
                        dblEd = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN(dblEd, objMoneyUnit.UnitMultiple);
                        strControlEDLimit = string.Format("{0:F2}", dblEd);
                        strControlEDLimit = strControlEDLimit == "0.00" ? "" : strControlEDLimit;
                    }
                    strGroupID = viewControlDetail.GroupID.ToString();
                    strGUID_HandleMethod = viewControlDetail.HandleName;
                }
                else if (item.PSS_BGCodeGUID!=null)
                {
                    strHandleMethod = objHandleMethod.GUID.ToString();
                    strGUID_HandleMethod = objHandleMethod.HandleName;
                }

                List<ValuePair> listPair = new List<ValuePair>();
                listPair.Add(new ValuePair("HandleMethod", strHandleMethod));
                listPair.Add(new ValuePair("GUID_ControlDetail", strGUID_ControlDetail));
                listPair.Add(new ValuePair("GUID_BGCode", item.GUID_BGCode.ToString()));
                listPair.Add(new ValuePair("GUID_BGItem", strGUID_BGItem));
                listPair.Add(new ValuePair("Sum", strSumFormula));
                listPair.Add(new ValuePair("Add", strAddFormula));
                listPair.Add(new ValuePair("IsControl", strIsControl));
                listPair.Add(new ValuePair("BGCodeName", strBgCodeName));
                listPair.Add(new ValuePair("BGItemName", strBGItemName));
                listPair.Add(new ValuePair("BGED", strTotalBG));
                listPair.Add(new ValuePair("IsScaleOrNot", strIsScaleOrNot));
                listPair.Add(new ValuePair("ControlEDScale", strControlEDScale));
                listPair.Add(new ValuePair("ControlEDLimit", strControlEDLimit));
                listPair.Add(new ValuePair("GroupID", strGroupID));
                listPair.Add(new ValuePair("GUID_HandleMethod", strGUID_HandleMethod));

                string strRow = GetRowData(ref listPair);
                if ("" == strRows)
                {
                    strRows = strRow;
                }
                else
                {
                    strRows = strRows + "," +strRow;
                }
            }
            string strData = "{\\\"total\\\":" + iCount.ToString() +",\\\"rows\\\":[" + strRows + "]}";
            return strData;
        }
        public JsonModel SaveControlMain(string strCMYear ,string strDw,string strDepartment,string strControlWayKey,
            string strProject,string strDetail,string strMaker,string strMakeDate,string strState,string strMoneyUnit) 
        {
            int iCMYear = Int32.Parse(strCMYear);
            Guid guid_Dw = new Guid(strDw);
            Guid guidDepartment = new Guid(strDepartment);
            Guid? guidProject = null;
            if(""!=strProject){
                guidProject = new Guid(strProject);
            }
            Guid guid_Maker = new Guid(strMaker);
            DateTime makeDate = DateTime.Parse(strMakeDate);
            Guid guidMoneyUnit = new Guid(strMoneyUnit);
            bool bExist = true;
            string strControlKey = strControlWayKey;
            BG_ControlMain objBG_ControlMain = null;
            if (strProject == "")
            {
                objBG_ControlMain = objBG_ControlMain = this.BusinessContext.BG_ControlMain.FirstOrDefault(e => e.CMYear == iCMYear &&
                    e.GUID_DW == guid_Dw && e.GUID_Department == guidDepartment && e.GUID_Project == null);
            }
            else
            {
                objBG_ControlMain = objBG_ControlMain = this.BusinessContext.BG_ControlMain.FirstOrDefault(e => e.CMYear == iCMYear &&
                   e.GUID_DW == guid_Dw && e.GUID_Department == guidDepartment && e.GUID_Project == guidProject);               
            }

            if (null == objBG_ControlMain)
            {
                if (strState=="3")
                {
                    JsonModel jResult = new JsonModel();
                    VerifyResult result = new VerifyResult();
                    string str = "该预算控制不存在，不能删除！";
                    result._validation.Add(new ValidationResult("", str));
                    string strMsg = DataVerifyMessage(result);
                    jResult.result = JsonModelConstant.Error;
                    jResult.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                    return jResult;
                }
                bExist = false;
                objBG_ControlMain = new BG_ControlMain();
                objBG_ControlMain.GUID = Guid.NewGuid();
                objBG_ControlMain.GUID_Department = guidDepartment;
                objBG_ControlMain.GUID_DW = guid_Dw;
                objBG_ControlMain.CMYear = iCMYear;
                objBG_ControlMain.GUID_Maker = guid_Maker;
                objBG_ControlMain.MakeDate = makeDate;
                SS_Operator Operator = OperatorId == Guid.Empty ? null : this.InfrastructureContext.SS_Operator.FirstOrDefault(e => e.GUID == OperatorId);
                objBG_ControlMain.GUID_Modifier = Operator.GUID;
                objBG_ControlMain.ModifyDate = DateTime.Now;
                objBG_ControlMain.ControlWayKey = strControlKey;
                objBG_ControlMain.GUID_Project = guidProject;
            }
            else   // 如果是已经存在的，那么要检查一下控制方式是否一致
            {
                // 这种情况下是不能保存的
                if (strControlKey != objBG_ControlMain.ControlWayKey)
                {
                    JsonModel jResult = new JsonModel();
                    VerifyResult result = new VerifyResult();
                    string str = "";
                    string strMsg = "";
                    if (strState == "3")
                    {
                        if (objBG_ControlMain.ControlWayKey == "01")
                        {
                            str = "该预算已经存在“按总额控制”方式,删除操作无效";
                        }
                        else if (objBG_ControlMain.ControlWayKey == "02")
                        {
                            str = "该预算已经存在“按明细控制”方式,删除操作无效";
                        }
                        else
                        {
                            str = "该预算已经存在“按分组控制”方式,删除操作无效";
                        }
                        result._validation.Add(new ValidationResult("", str));
                        strMsg = DataVerifyMessage(result);
                        jResult.result = JsonModelConstant.Error;
                        jResult.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                        return jResult;
                    }
                    if (objBG_ControlMain.ControlWayKey == "01")
                    {
                        str = "该预算已经存在“按总额控制”方式,不能保存新的预算控制";
                    }
                    else if (objBG_ControlMain.ControlWayKey == "02")
                    {
                        str = "该预算已经存在“按明细控制”方式,不能保存新的预算控制";
                    }
                    else
                    {
                        str = "该预算已经存在“按分组控制”方式,不能保存新的预算控制";
                    }
                    result._validation.Add(new ValidationResult("", str));
                    strMsg = DataVerifyMessage(result);
                    jResult.result = JsonModelConstant.Error;
                    jResult.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                    return jResult;
                }
            }
            objBG_ControlMain.GUID_MoneyUnit = guidMoneyUnit;
            objBG_ControlMain.ResetDefault(this, this.OperatorId);
            BusinessModel.SS_MoneyUnit objMoneyUnit = objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == guidMoneyUnit);                
            // 获得BG_Detail信息
            int indexFirst = strDetail.IndexOf('[');
            int indexLast = strDetail.LastIndexOf(']');
            string strData = strDetail.Substring(indexFirst, indexLast - indexFirst + 1);
            List<string> ListDetail = SplitJsonArray(strData);
            int iCount = ListDetail.Count;
            for (int i = 0; i < iCount; i++)
            {
                List<ValuePair> Control_Detail = SplitJsonObject(ListDetail[i]);
                ValuePair vp = Control_Detail.Find(e=>e.strKey == "Sum");
                if(vp.strValue!="NO")
                {
                    SkipSet.Add(i);
                    continue;
                }
                
                BG_ControlDetail detail = AssemblingBG_ControlDetail(Control_Detail, objMoneyUnit.UnitMultiple);
                if(null==detail){
                    continue;
                }
                DicIndex.Add(detail.GUID,i+1);
                detail.GUID_ControlMain = objBG_ControlMain.GUID;
                detail.GUID_Project = objBG_ControlMain.GUID_Project;
                if (bExist)
                {
                    BG_ControlDetail existDetail = objBG_ControlMain.BG_ControlDetail.FirstOrDefault(e => e.GUID == detail.GUID);
                    CopyObject(ref existDetail, ref detail);
                }
                else
                {
                    objBG_ControlMain.BG_ControlDetail.Add(detail);
                }
            }

            return SaveControlMainEx(ref objBG_ControlMain,bExist, strState);
        }
        private void CopyObject(ref BG_ControlDetail src,ref BG_ControlDetail target)
        {
            src.GUID_BGItem = target.GUID_BGItem;
            src.GUID_HandleMethod = target.GUID_HandleMethod;
            src.IsControl = target.IsControl;
            src.IsScaleOrNot = target.IsScaleOrNot;
            src.ControlED = target.ControlED;
            src.GroupID = target.GroupID;
        }
        protected override VerifyResult InsertVerify(object data)
        {
            VerifyResult result = new VerifyResult();
            BG_ControlMain model = (BG_ControlMain)data;
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
        private List<ValidationResult> VerifyResultDetail(BG_ControlMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<BG_ControlDetail> detailList = new List<BG_ControlDetail>();
            foreach (BG_ControlDetail item in data.BG_ControlDetail)
            {
                detailList.Add(item);
            }

            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (BG_ControlDetail item in detailList)
                {
                    rowIndex = DicIndex[item.GUID];
                    var vf_detail = VerifyResult_BGControl_Detail(item, rowIndex, data.ControlWayKey);
                    if (vf_detail != null && vf_detail.Count > 0)
                    {
                        resultList.AddRange(vf_detail);
                    }
                }
            }
            return resultList;
        }
        private List<ValidationResult> VerifyResult_BGControl_Detail(BG_ControlDetail data, int rowIndex,string strControlWayKey)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            BG_ControlDetail item = data;
            if (item.IsControl && !SkipSet.Contains(rowIndex-1))
            {
                if(item.GUID_BGItem==null && strControlWayKey!="01")
                {
                    str = "预算项 字段为必输项!";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }

                if (item.GUID_HandleMethod == null)
                {
                    str = "控制方式 字段为必输项!";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }

            return resultList;
        }
        private List<ValidationResult> VerifyResultMain(BG_ControlMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            BG_ControlMain mModel = data;
            object g;
            if (mModel.MakeDate.IsNullOrEmpty())
            {
                str = "制单日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.MakeDate.GetType(), mModel.MakeDate.ToString(), out g) == false)
                {
                    str = "制单日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));

                }
            }

            if (mModel.ModifyDate.IsNullOrEmpty())
            {
                str = "修改日期 字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            else
            {
                if (Common.ConvertFunction.TryParse(mModel.ModifyDate.GetType(), mModel.ModifyDate.ToString(), out g) == false)
                {
                    str = "修改日期 格式不正确！";
                    resultList.Add(new ValidationResult("", str));
                }
            }

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

            return resultList;
        }
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
        protected override void Delete(Guid guid)
        {
            BG_ControlMain main = this.BusinessContext.BG_ControlMain.Include("BG_ControlDetail").FirstOrDefault(e => e.GUID == guid);

            List<BG_ControlDetail> details = new List<BG_ControlDetail>();

            foreach (BG_ControlDetail item in main.BG_ControlDetail)
            {
                details.Add(item);
            }

            foreach (BG_ControlDetail item in details) 
            {
                BusinessContext.Detach(item);
                BusinessContext.DeleteConfirm(item);
            }
            BusinessContext.Detach(main);
            BusinessContext.DeleteConfirm(main);
            BusinessContext.SaveChanges();
        }
        private JsonModel SaveControlMainEx(ref  BG_ControlMain objBG_ControlMain,bool bExist, string strState)
        {
            JsonModel result = new JsonModel();
            try
            {
                string strMsg = string.Empty;
                VerifyResult vResult = null;
                if ("3" == strState)
                {
                    vResult = DeleteVerify(objBG_ControlMain.GUID);
                    strMsg = DataVerifyMessage(vResult);
                    if (string.IsNullOrEmpty(strMsg))
                    {
                        Guid tmp = objBG_ControlMain.GUID;
                        objBG_ControlMain = null;
                        this.Delete(tmp);
                    }
                }
                else
                {
                    if (!bExist)
                    {
                        vResult = InsertVerify(objBG_ControlMain);
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            this.BusinessContext.BG_ControlMain.AddObject(objBG_ControlMain);
                            this.BusinessContext.SaveChanges();
                        }
                    }
                    else if ("3" != strState)
                    {
                        vResult = ModifyVerify(objBG_ControlMain);//修改验证
                        strMsg = DataVerifyMessage(vResult);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            List<BG_ControlDetail> listDetail = new List<BG_ControlDetail>();
                            foreach(BG_ControlDetail item in objBG_ControlMain.BG_ControlDetail)
                            {
                                listDetail.Add(item);
                            }

                            foreach (BG_ControlDetail item in listDetail)
                            {
                                this.BusinessContext.ModifyConfirm(item);
                            }
                            this.BusinessContext.ModifyConfirm(objBG_ControlMain);
                            this.BusinessContext.SaveChanges();
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
                        result = this.Retrieve(objBG_ControlMain.GUID);
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
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

            BG_ControlMain main = this.BusinessContext.BG_ControlMain.FirstOrDefault(e=> e.GUID==guid); 
            if(null==main)
            {
                str = "该预算控制不存在，删除无效！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            return result;
        }
        protected override VerifyResult ModifyVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            BG_ControlMain model = (BG_ControlMain)data;
            BG_ControlMain orgModel = this.BusinessContext.BG_ControlMain.Include("BG_ControlDetail").FirstOrDefault(e => e.GUID == model.GUID);

            //
            //if (WorkFlowAPI.ExistProcess(model.GUID))
            //{
            //    List<ValidationResult> resultList = new List<ValidationResult>();
            //    resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
            //    result._validation = resultList;
            //    return result;
            //}
            ////作废           
            //if (orgModel != null && orgModel.DocState == int.Parse("9") && model.DocState != (int)Business.Common.EnumType.EnumDocState.RcoverState)
            //{
            //    List<ValidationResult> resultList = new List<ValidationResult>();
            //    resultList.Add(new ValidationResult("", "此单已经作废，不能进行修改！"));
            //    result._validation = resultList;
            //    return result;
            //}

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
        private void ModifyControlMainEx(BG_ControlMain objBG_ControlMain)
        {
            JsonModel jsonModel = new JsonModel();
            jsonModel.m = objBG_ControlMain.Pick();

            BG_ControlMain main = new BG_ControlMain();
            BG_ControlDetail tempDetail = new BG_ControlDetail();
            JsonGridModel jgm = new JsonGridModel(tempDetail.ModelName());
            jsonModel.d.Add(jgm);
            foreach (BG_ControlDetail detail in objBG_ControlMain.BG_ControlDetail)
            {
                List<JsonAttributeModel> picker = detail.Pick();
                jgm.r.Add(picker);
            }

            // 一定要从数据库中获得数据对象，否则修改将出现异常
            main = this.BusinessContext.BG_ControlMain.Include("BG_ControlDetail").FirstOrDefault(e => e.GUID == objBG_ControlMain.GUID);
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this, this.OperatorId);

            string detailModelName = tempDetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (BG_ControlDetail detail in main.BG_ControlDetail)
                {
                    this.BusinessContext.DeleteConfirm(detail);
                }
            }
            else
            {
                List<BG_ControlDetail> detailList = this.BusinessContext.BG_ControlDetail.Where(e => e.GUID_ControlMain == objBG_ControlMain.GUID).ToList();
                foreach (BG_ControlDetail detail in detailList)
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
        private BG_ControlDetail AssemblingBG_ControlDetail(List<ValuePair> ListDetailInfo, int objTargetMoneyUnit)
        {
            BG_ControlDetail detail = new BG_ControlDetail();
            ValuePair isScaleItem = ListDetailInfo.Find(e => e.strKey == "IsScaleOrNot");
            detail.IsScaleOrNot = isScaleItem.strValue == "" ? false : true;
            foreach (ValuePair item in ListDetailInfo)
            {
                switch (item.strKey)
                {
                    case "Sum":
                        { 
                            if(item.strValue!="NO"){
                                return null;
                            }
                            break;
                        }
                    case "GUID_ControlDetail":
                        {
                            if (item.strValue != "")
                            {
                                detail.GUID = new Guid(item.strValue);
                            }
                            else
                            {
                                detail.GUID = Guid.NewGuid();
                            }
                            break;
                        }
                    case "GUID_BGCode":
                        {
                            if (item.strValue == "")
                            {
                                detail.GUID_BGCode = null;
                            }
                            else
                            {
                                detail.GUID_BGCode = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "GUID_BGItem":
                        {
                            if (item.strValue == "")
                            {
                                detail.GUID_BGItem = null;
                            }
                            else
                            {
                                detail.GUID_BGItem = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "HandleMethod":
                        {
                            if (item.strValue == "")
                            {
                                detail.GUID_HandleMethod = null;
                            }
                            else
                            {
                                detail.GUID_HandleMethod = new Guid(item.strValue);
                            }
                            break;
                        }
                    case "IsControl":
                        {
                            detail.IsControl = item.strValue==""?false:true;
                            break;
                        }
                    case "ControlEDScale":
                        {

                            double dblValue = item.strValue==""?0:Double.Parse(item.strValue);
                            if((bool)detail.IsScaleOrNot)
                            {
                                detail.ControlED = dblValue;
                            }
                            break;
                        }
                    case "ControlEDLimit":
                        {
                            double dblValue = item.strValue == "" ? 0 : Double.Parse(item.strValue);
                            dblValue = Infrastructure.CommonFuntion.ConvertYUANtoOtherMoneyUnit(dblValue, objTargetMoneyUnit);
                            if (!(bool)detail.IsScaleOrNot)
                            {
                                detail.ControlED = dblValue;
                            }
                            break;
                        }
                    case "GroupID":
                        {
                            detail.GroupID = item.strValue;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                
            }
            return detail;
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
        private Infrastructure.BG_HandleMethod GetDefaultHandleMethod(string strHandleKey)
        {
            Infrastructure.BG_HandleMethod obj = this.InfrastructureContext.BG_HandleMethod.FirstOrDefault(e => e.HandleKey == strHandleKey);
            return obj;
        }
        public string LoadDetail(string strCMYear, string strControlKey, string strGUID_DW, string strGUID_Department, string strGUID_Project,string strGUID_Control_Main)
        {   
            if(strCMYear == "" || strControlKey == "" ||strGUID_DW == "" ||strGUID_Department == ""){
                return string.Empty;
            }

            Infrastructure.BG_HandleMethod objHandleMethod = GetDefaultHandleMethod("01");
            int iCMYear = Int32.Parse(strCMYear);
            Guid GuidDW = new Guid(strGUID_DW);
            Guid GuidDepartment = new Guid(strGUID_Department);
            Guid? GuidProject = null;
            Guid GuidControl_Main = new Guid(strGUID_Control_Main);
            if (strGUID_Project!="")
            {
                GuidProject = new Guid(strGUID_Project);
            }
            // 先判断这个预算是否存在，如果不存在，那么detail是不可以编辑的
            var guid_bg_mainSet = from detail in this.BusinessContext.BG_Detail
                                  where detail.BGYear == iCMYear
                                  select  detail.GUID_BG_Main ;
            guid_bg_mainSet = guid_bg_mainSet.Distinct();
            List<BG_Main> BG_MainList = new List<BG_Main>();
            if (strGUID_Project == "")
            {
                BG_MainList = this.BusinessContext.BG_Main.Where(e => guid_bg_mainSet.Contains(e.GUID) && e.GUID_DW == GuidDW &&
                e.GUID_Department == GuidDepartment && e.GUID_Project == null && e.Invalid == true).ToList();
            }
            else
            {
                BG_MainList = this.BusinessContext.BG_Main.Where(e => guid_bg_mainSet.Contains(e.GUID) && e.GUID_DW == GuidDW &&
                e.GUID_Department == GuidDepartment && e.GUID_Project == GuidProject && e.Invalid == true).ToList();
            }

            // 有效的预算编制存在多个
            if (BG_MainList.Count>1)
            {
                string strJson = "{\"success\":false,\"errMsg\":\"系统错误，预算编制存在多个!\",\"iState\":\"1\"}";
                return strJson;
            }

            // 获取货币单位
            string strBG_SetupKey = "07";
            if (strGUID_Project != "")
            {
                strBG_SetupKey = "08";
            }

            string strMultiple = GetMoneyMultiple(strBG_SetupKey);
            if (strMultiple == "")
            {
                string strJson = "{\"success\":false,\"errMsg\":\"货币单位获取失败!\"}";
                return strJson;
            }

            BG_Main objBG_Main = null;
            if (BG_MainList != null && BG_MainList.Count==1)
            {
                objBG_Main = BG_MainList[0];
            }
            BG_ControlMainView objBG_ControlMain = null;
            if (strGUID_Project == "")
            {
                objBG_ControlMain = this.BusinessContext.BG_ControlMainView.FirstOrDefault(e => e.CMYear == iCMYear &&
                e.GUID_DW == GuidDW && e.GUID_Department == GuidDepartment && e.GUID_Project == null);
            }
            else
            {
                objBG_ControlMain = this.BusinessContext.BG_ControlMainView.FirstOrDefault(e => e.CMYear == iCMYear &&
                e.GUID_DW == GuidDW && e.GUID_Department == GuidDepartment && e.GUID_Project == GuidProject);
            }

            // 如果这个预算的控制还不存在
            if (objBG_ControlMain == null)
            {
                if (strControlKey == "01")    // 按总额控制
                {
                    List<ValuePair> listPair = new List<ValuePair>();
                    listPair.Add(new ValuePair("HandleMethod", objHandleMethod.GUID.ToString()));
                    listPair.Add(new ValuePair("GUID_ControlDetail", ""));
                    listPair.Add(new ValuePair("GUID_BGCode",""));
                    listPair.Add(new ValuePair("GUID_BGItem", ""));
                    listPair.Add(new ValuePair("Sum", "NO"));
                    listPair.Add(new ValuePair("Add", "NO"));
                    listPair.Add(new ValuePair("IsControl", "√"));
                    listPair.Add(new ValuePair("BGCodeName", ""));
                    listPair.Add(new ValuePair("BGItemName", ""));
                    string strTotalBG = "";
                    // 如果所对应的预算存在
                    if (objBG_Main != null)
                    {
                        double dblTotal = (double)objBG_Main.Total_BG;
                        strTotalBG = string.Format("{0:F2}", dblTotal);
                        strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                    }

                    listPair.Add(new ValuePair("BGED", strTotalBG));
                    listPair.Add(new ValuePair("IsScaleOrNot", ""));
                    listPair.Add(new ValuePair("ControlEDScale", ""));
                    listPair.Add(new ValuePair("ControlEDLimit", strTotalBG));
                    listPair.Add(new ValuePair("GroupID", ""));
                    listPair.Add(new ValuePair("GUID_HandleMethod", objHandleMethod.HandleName));
                    string strBG_Main = "";
                    if (objBG_Main!=null)
                    {
                        strBG_Main = objBG_Main.GUID.ToString();
                    }
                    string strRow = GetRowData(ref listPair);
                    string strData = "{\\\"total\\\":1,\\\"rows\\\":[" + strRow + "]}";
                    string strJson = "{\"success\": true,\"data\":\"" + strData + "\",\"Multiple\":\""
                        + strMultiple + "\",\"iState\":\"1\",\"Tip\":\"该预算的控制不存在，可新增\",\"GUID_BG_Main\":\"" + strBG_Main + "\"}";
                    return strJson;
                }
                else    // 按明细或分组控制
                {
                    BG_ControlMainView viewMainFun = null;
                    IQueryable<BG_SetupBGCodeView> viewBG_SetupBGCode = this.BusinessContext.BG_SetupBGCodeView.Where(e => e.BGSetupKey == strBG_SetupKey
                        && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey);
                    List<BG_SetupBGCodeView> listBG_SetupBGCodeView = viewBG_SetupBGCode.ToList();
                    string strData = LoadDetailEx(ref listBG_SetupBGCodeView, ref viewMainFun, ref objBG_Main);
                    string strBG_Main = "";
                    string strBGItemInfo = "";
                    // 如果预算编制存在，那么每条科目所对应的预算项的值也传向前台
                    if (objBG_Main != null)
                    {
                        strBG_Main = objBG_Main.GUID.ToString();
                        List<BG_Detail> listBG_Detail = objBG_Main.BG_Detail.ToList();
                        BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == strBG_SetupKey);
                        List<Infrastructure.BG_SetupDetailView> viewDetailList = this.InfrastructureContext.BG_SetupDetailView.Where(e => e.GUID_BGSetup == objBG_Setup.GUID).OrderBy(e => e.ItemOrder).ToList();
                        string strRows = "";
                        foreach (BG_SetupBGCodeView item in listBG_SetupBGCodeView)
                        {
                            string strRow = "{";
                            foreach (Infrastructure.BG_SetupDetailView BG_SetupDetail in viewDetailList)
                            {
                                BG_Detail tmpDetail = listBG_Detail.Find(e => e.GUID_Item == BG_SetupDetail.GUID_Item && e.GUID_BGCode == item.GUID_BGCode);
                                string strTotal_bg = "";
                                if (null != tmpDetail)
                                {
                                    double dblTotal_bg = (double)tmpDetail.Total_BG;
                                    strTotal_bg = string.Format("{0:F2}", dblTotal_bg);
                                    strTotal_bg = strTotal_bg == "0.00" ? "" : strTotal_bg;
                                }
                                if (strRow == "{")
                                {
                                    strRow = strRow + "\\\"" + BG_SetupDetail.GUID_Item.ToString() + "\\\":" + "\\\"" + strTotal_bg + "\\\"";
                                }
                                else
                                {
                                    strRow = strRow + ",\\\"" + BG_SetupDetail.GUID_Item.ToString() + "\\\":" + "\\\"" + strTotal_bg + "\\\"";
                                }
                            }
                            strRow = strRow + "}";
                            if (strRows == "")
                            {
                                strRows = strRow;
                            }
                            else
                            {
                                strRows = strRows + "," + strRow;
                            }
                        }

                        strBGItemInfo = ",\"BGItemInfo\":\"{\\\"info\\\":[" + strRows + "]}\"";
                        //strBGItemInfo = "\"BGItemInfo\":\"\"";
                    }
                    string strJson = "{\"success\": true,\"data\":\"" + strData + "\",\"Multiple\":\""
                                        + strMultiple + "\",\"iState\":\"1\",\"Tip\":\"该预算的控制尚不存在，可新增\",\"GUID_BG_Main\":\"" + strBG_Main + "\"" + strBGItemInfo + "}";
                    return strJson;
                }
            }
            else
            {
                BusinessModel.SS_MoneyUnit objMoneyUnit = objMoneyUnit = this.BusinessContext.SS_MoneyUnit.FirstOrDefault(e => e.GUID == objBG_ControlMain.GUID_MoneyUnit);
                strMultiple = objMoneyUnit.UnitMultiple.ToString();
                if (strControlKey == "01")    // 按总额控制
                {
                    // 总额控制，只有一条controldetail
                    List<BG_ControlDetailView> listDetail = this.BusinessContext.BG_ControlDetailView.Where(e=>e.GUID_ControlMain==objBG_ControlMain.GUID).ToList();
                    if (listDetail.Count != 1 && strControlKey==objBG_ControlMain.ControlWayKey)
                    {
                        string strJson = "{\"success\":false,\"errMsg\":\"系统错误，该预算无控制方式或包含多个控制方式!\"}";
                        return strJson;
                    }

                    BG_ControlDetailView detail = listDetail[0];;
                    List<ValuePair> listPair = new List<ValuePair>();
                    // 如果当前所选择控制方式和已存在的控制方式相同
                    if (strControlKey == objBG_ControlMain.ControlWayKey)
                    {
                        listPair.Add(new ValuePair("HandleMethod", detail.GUID_HandleMethod.ToString()));
                        listPair.Add(new ValuePair("GUID_ControlDetail", detail.GUID.ToString()));
                        listPair.Add(new ValuePair("GUID_BGCode", detail.GUID_BGCode.ToString()));
                        listPair.Add(new ValuePair("GUID_BGItem", detail.GUID_BGItem.ToString()));
                        listPair.Add(new ValuePair("Sum", "NO"));
                        listPair.Add(new ValuePair("Add", "NO"));
                        string strIsControl = detail.IsControl ? "√" : "";
                        listPair.Add(new ValuePair("IsControl", strIsControl));
                        listPair.Add(new ValuePair("BGCodeName", detail.BGCodeName));
                        listPair.Add(new ValuePair("BGItemName", detail.BGItemName));
                        string strTotalBG = "";
                        // 如果所对应的预算存在,且已经存在的预算控制的控制方式和当前所选择的相同，那么才有必要显示预算金额
                        double dblTotal = 0;
                        if (objBG_Main != null)
                        {
                            dblTotal = (double)objBG_Main.Total_BG;
                            strTotalBG = string.Format("{0:F2}", dblTotal);
                            strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        }
                        listPair.Add(new ValuePair("BGED", strTotalBG));
                        string strIsScaleOrNot = (bool)detail.IsScaleOrNot ? "√" : "";
                        double dblED = (double)detail.ControlED;

                        listPair.Add(new ValuePair("IsScaleOrNot", strIsScaleOrNot));

                        if ((bool)detail.IsScaleOrNot)
                        {
                            string strEd = "";
                            strEd = string.Format("{0:F2}", dblED);
                            double dblLimit = dblED * dblTotal;
                            string strLimit = string.Format("{0:F2}", dblLimit);
                            strLimit = string.Format("{0:F2}", strLimit);
                            listPair.Add(new ValuePair("ControlEDScale", strEd));
                            listPair.Add(new ValuePair("ControlEDLimit", strLimit));
                        }
                        else
                        {
                            dblED = Infrastructure.CommonFuntion.ConverOtherMoneyUnitToYUAN(dblED, objMoneyUnit.UnitMultiple);
                            string strEd = "";
                            strEd = string.Format("{0:F2}", dblED);
                            strEd = strEd == "0.00" ? "" : strEd;
                            listPair.Add(new ValuePair("ControlEDScale", ""));
                            listPair.Add(new ValuePair("ControlEDLimit", strEd));
                        }

                        listPair.Add(new ValuePair("GroupID", detail.GroupID));
                        listPair.Add(new ValuePair("GUID_HandleMethod", detail.HandleName));
                    }
                    else
                    {
                        string strTotalBG = "";
                        // 如果所对应的预算存在,且已经存在的预算控制的控制方式和当前所选择的相同，那么才有必要显示预算金额
                        if (objBG_Main != null)
                        {
                            double dblTotal = (double)objBG_Main.Total_BG;
                            strTotalBG = string.Format("{0:F2}", dblTotal);
                            strTotalBG = strTotalBG == "0.00" ? "" : strTotalBG;
                        }
                        listPair.Add(new ValuePair("HandleMethod", ""));
                        listPair.Add(new ValuePair("GUID_ControlDetail", ""));
                        listPair.Add(new ValuePair("GUID_BGCode", ""));
                        listPair.Add(new ValuePair("GUID_BGItem", ""));
                        listPair.Add(new ValuePair("Sum", "NO"));
                        listPair.Add(new ValuePair("Add", "NO"));
                        listPair.Add(new ValuePair("IsControl", "√"));
                        listPair.Add(new ValuePair("BGCodeName", ""));
                        listPair.Add(new ValuePair("BGItemName", ""));
                        listPair.Add(new ValuePair("BGED", strTotalBG));
                        listPair.Add(new ValuePair("IsScaleOrNot", ""));
                        listPair.Add(new ValuePair("ControlEDScale", ""));
                        listPair.Add(new ValuePair("ControlEDLimit", strTotalBG));
                        listPair.Add(new ValuePair("GroupID", ""));
                        listPair.Add(new ValuePair("GUID_HandleMethod", ""));
                    }

                    string strState = "1";      // 表示正常
                    string strTip = "预算控制存在，修改后可保存";         // 提示
                    if (strControlKey != objBG_ControlMain.ControlWayKey)
                    {
                        strState = "2";     // 表示已经存在一种控制方式
                        if (objBG_ControlMain.ControlWayKey == "02")
                        {
                            strTip = "已经存在按明细控制的方式";
                        }
                        else
                        {
                            strTip = "已经存在按分组控制的方式";
                        }
                    }

                    string strBG_Main = "";
                    if (objBG_Main != null)
                    {
                        strBG_Main = objBG_Main.GUID.ToString();
                    }
                    string strRow = GetRowData(ref listPair);
                    string strData = "{\\\"total\\\":1,\\\"rows\\\":[" + strRow + "]}";
                    string strJsonEx = "{\"success\": true,\"data\":\"" + strData + "\",\"Multiple\":\"" +
                        strMultiple + "\",\"iState\":\"" + strState + "\",\"Tip\":\"" + strTip + "\",\"GUID_BG_Main\":\"" + strBG_Main + "\"}";
                    return strJsonEx;

                }
                else    // 按明细或分组控制
                {
                    BG_ControlMainView viewMainFun = null;
                    if (strControlKey == objBG_ControlMain.ControlWayKey)
                    {
                        viewMainFun = objBG_ControlMain;
                    }
                    IQueryable<BG_SetupBGCodeView> viewBG_SetupBGCode = this.BusinessContext.BG_SetupBGCodeView.Where(e => e.BGSetupKey == strBG_SetupKey
                        && e.BGCodeIsStop == false).OrderBy(e => e.BGCodeKey);
                    List<BG_SetupBGCodeView> listBG_SetupBGCodeView = viewBG_SetupBGCode.ToList();
                    string strData = LoadDetailEx(ref listBG_SetupBGCodeView, ref viewMainFun, ref objBG_Main); ;

                    string strState = "1";      // 表示正常
                    string strTip = "预算控制存在，修改后可保存";         // 提示
                    if (strControlKey != objBG_ControlMain.ControlWayKey)
                    {
                        strState = "2";     // 表示已经存在一种控制方式
                        if (objBG_ControlMain.ControlWayKey == "02")
                        {
                            strTip = "已经存在按明细控制的方式";
                        }
                        else if (objBG_ControlMain.ControlWayKey == "03")
                        {
                            strTip = "已经存在按分组控制的方式";
                        }
                        else
                        {
                            strTip = "已经存在按总额控制的方式";
                        }
                    }

                    string strBG_Main = "";
                    string strBGItemInfo = "";
                    if (objBG_Main != null)
                    {
                        
                        strBG_Main = objBG_Main.GUID.ToString();
                        List<BG_Detail> listBG_Detail = objBG_Main.BG_Detail.ToList();
                        BG_Setup objBG_Setup = this.InfrastructureContext.BG_Setup.FirstOrDefault(e => e.BGSetupKey == strBG_SetupKey);
                        List<Infrastructure.BG_SetupDetailView> viewDetailList = this.InfrastructureContext.BG_SetupDetailView.Where(e => e.GUID_BGSetup == objBG_Setup.GUID).OrderBy(e => e.ItemOrder).ToList();
                        string strRows = "";
                        foreach (BG_SetupBGCodeView item in listBG_SetupBGCodeView)
                        {
                            string strRow = "{";
                            foreach (Infrastructure.BG_SetupDetailView BG_SetupDetail in viewDetailList)
                            {
                                BG_Detail tmpDetail = listBG_Detail.Find(e=> e.GUID_Item == BG_SetupDetail.GUID_Item && e.GUID_BGCode==item.GUID_BGCode);
                                string strTotal_bg = "";
                                if(null!=tmpDetail)
                                {
                                    double dblTotal_bg  = (double)tmpDetail.Total_BG;
                                    strTotal_bg = string.Format("{0:F2}", dblTotal_bg);
                                    strTotal_bg = strTotal_bg == "0.00" ? "" : strTotal_bg;
                                }
                                if (strRow == "{")
                                {
                                    strRow = strRow + "\\\"" + BG_SetupDetail.GUID_Item.ToString() + "\\\":" + "\\\"" + strTotal_bg + "\\\"";
                                }
                                else
                                {
                                    strRow = strRow + ",\\\"" + BG_SetupDetail.GUID_Item.ToString() + "\\\":" + "\\\"" + strTotal_bg + "\\\"";
                                }
                            }
                            strRow = strRow + "}";
                            if (strRows == "")
                            {
                                strRows = strRow;
                            }
                            else
                            {
                                strRows = strRows + "," + strRow;
                            }
                        }

                        strBGItemInfo = ",\"BGItemInfo\":\"{\\\"info\\\":[" + strRows + "]}\"";

                    }
                    string strJsonEx = "{\"success\": true,\"data\":\"" + strData + "\",\"Multiple\":\"" +
                                        strMultiple + "\",\"iState\":\"" + strState + "\",\"Tip\":\"" + strTip + "\",\"GUID_BG_Main\":\"" + strBG_Main 
                                        + "\"" + strBGItemInfo + "}";
                    return strJsonEx;
                }
            }
        }

        public override List<object> History(SearchCondition conditions)
        {
            List<object> list = new List<object>();
            IQueryable<BG_ControlMainView> main = this.BusinessContext.BG_ControlMainView;
            // 获得权限
            int classid = CommonFuntion.GetClassId(typeof(SS_Department).Name);
            IntrastructureFun objIF = new Infrastructure.IntrastructureFun();
            var DepartmentAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Person).Name);
            var PersonAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            classid = CommonFuntion.GetClassId(typeof(SS_Project).Name);
            var ProjectAuth = objIF.GetDataSet(classid.ToString(), this.OperatorId.ToString()).ToList();

            IQueryable<BG_ControlMainView> mainSetWithOutProject = main.Where(e => e.GUID_Project == null && DepartmentAuth.Contains((Guid)e.GUID_Department));

            IQueryable<BG_ControlMainView> mainSetWithProject = main.Where(e => e.GUID_Project != null && DepartmentAuth.Contains((Guid)e.GUID_Department));

            IQueryable<BG_ControlMainView> allBG_Main = mainSetWithOutProject.Union(mainSetWithProject);
            list = allBG_Main.AsEnumerable().Select(e => new
            {
                e.GUID,
                e.CMYear,
                e.DWName,
                e.DepartmentName,
                e.ProjectName,
                ControlWayKey = e.ControlWayKey == "01" ? "总额控制" : e.ControlWayKey == "02"?"明细控制":"分组控制",
                e.Maker
            }).OrderByDescending(e => e.ProjectName).OrderByDescending(e => e.CMYear).ToList<object>();

            return list;
        }
    }
}
