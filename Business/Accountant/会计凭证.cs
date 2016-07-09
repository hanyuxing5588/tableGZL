using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Business.CommonModule;
using Platform.Flow.Run;
using BusinessModel;
namespace Business.Accountant
{   
    public class 会计凭证 : BaseDocument
    {

        public 会计凭证() : base() { }
        public 会计凭证(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        
        /// <summary>
        /// 创建默认值

        /// </summary>
        /// <returns></returns>
        public override JsonModel New()
        {
            try
            {
                JsonModel jmodel = new JsonModel();

                CW_PZMainView model = new CW_PZMainView();
                model.FillDefault(this, this.OperatorId, this.ModelUrl);
                jmodel.m = model.Pick();

                CW_PZDetailView dModel = new CW_PZDetailView();
                dModel.FillDetailDefault<CW_PZDetailView>(this, this.OperatorId);
                dModel.GUID_Department = null;
                dModel.DepartmentName = string.Empty;
                dModel.DepartmentKey = string.Empty;
                dModel.GUID_Person = null;
                dModel.PersonName = string.Empty;
                dModel.PersonKey = string.Empty;
                JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
                jmodel.f.Add(fjgm);

                List<JsonAttributeModel> picker = dModel.Pick();
                
                fjgm.r.Add(picker);
                return jmodel;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                CW_PZMainView main = this.BusinessContext.CW_PZMainView.FirstOrDefault(e => e.GUID == guid);

                if (main != null)
                {
                    jmodel.m = main.Pick();
                    //取用友编号

                    if (!string.IsNullOrEmpty(main.ExteriorDataBase) && main.FiscalYear != null)
                    {
                        U8Certificate u8Obj = new U8Certificate(this.BusinessContext);
                       System.Data.DataTable dt=u8Obj.GetGL_accvouchByCoutNO_ID(main.ExteriorDataBase, (int)main.FiscalYear, main.GUID.ToString());
                       if (dt != null && dt.Rows.Count > 0)
                       {
                           GL_accvouch lg = new GL_accvouch();
                           lg.Ino_id = dt.Rows[0]["ino_id"].ToString();
                           jmodel.m.AddRange(lg.ClassPick());
                       }
                    }
                    IQueryable<CW_PZDetailView> q = this.BusinessContext.CW_PZDetailView.Where(e => e.GUID_PZMAIN == guid).OrderBy(e => e.OrderNum);
                    List<CW_PZDetailView> details = q == null ? new List<CW_PZDetailView>() : q.ToList();
                    if (details.Count > 0)
                    {
                        JsonGridModel jgm = new JsonGridModel(details[0].ModelName());
                        jmodel.d.Add(jgm);
                        foreach (CW_PZDetailView detail in details)
                        {
                            List<JsonAttributeModel> picker = detail.Pick();                            
                            jgm.r.Add(picker);
                        }
                    }
                    //明细中f 填充默认值


                    CW_PZDetailView dModel = new CW_PZDetailView();
                    dModel.FillDetailDefault<CW_PZDetailView>(this, this.OperatorId);
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

        public List<CW_PZMainView> FetchItem(CWPeriodModel model)
        {
            int CWPeriod=int.Parse(model.CWPeriod);
            int FiscalYear=int.Parse(model.FiscalYear);
            var q = this.BusinessContext.CW_PZMainView.Where(e => e.AccountKey == model.AccountKey && e.CWPeriod == CWPeriod && e.FiscalYear == FiscalYear).OrderBy(e => e.DocNum);
            return q.ToList<CW_PZMainView>();
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        public override Guid Insert(JsonModel jsonModel,bool isSave=true)
        {
            if (jsonModel.m == null) return Guid.Empty;
            CW_PZMain main = new CW_PZMain();
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.GUID = Guid.NewGuid();


            var periodModel=GetCWPeriodModel(jsonModel);
            main.GUID_AccountDetail = GetAccountDetailGUID(periodModel);
            main.GUID_CWPeriod = GetCWPeriodGUID(periodModel);
            var docNum=this.BusinessContext.CW_PZMain.Where(e=>e.GUID_PZType==main.GUID_PZType && e.GUID_CWPeriod==main.GUID_CWPeriod).ToList().Count();
            main.DocNum = docNum + 1;

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CW_PZDetail temp = new CW_PZDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    int orderNum = 1;
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        AddDetail(main, row, orderNum); orderNum++;
                    }
                }
            }
            this.BusinessContext.CW_PZMain.AddObject(main);
            if (isSave)
            {
                this.BusinessContext.SaveChanges();
            }
            return main.GUID;
        }
        /// <summary>
        /// 获取期间Model（特殊处理）
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public CWPeriodModel GetCWPeriodModel(JsonModel jsonModel)
        {
            CWPeriodModel model = new CWPeriodModel();
            foreach (JsonAttributeModel item in jsonModel.m)
            {
                if ((item.n+"").ToLower() == "cwperiod")
                {
                    model.CWPeriod =item.v;
                }
                if ((item.n + "").ToLower() == "fiscalyear")
                {
                    model.FiscalYear = item.v;
                }
                if ((item.n + "").ToLower() == "accountkey")
                {
                    model.AccountKey = item.v;
                }
            }
            return model;
        }
        /// <summary>
        /// 获取期间GUID
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Guid GetCWPeriodGUID(CWPeriodModel model)
        {
            //CWPeriodModel model = GetCWPeriodModel(jsonModel);
            if (model == null) return Guid.Empty;
            int fiscalyear = 0;
            if (!string.IsNullOrEmpty(model.FiscalYear))
            {
                int.TryParse(model.FiscalYear,out fiscalyear);
            }
            int cwPeriod = 0;
            if (!string.IsNullOrEmpty(model.CWPeriod))
            {
                int.TryParse(model.CWPeriod,out cwPeriod);
            }
            var period = this.BusinessContext.CW_PeriodView.FirstOrDefault(e => e.FiscalYear == fiscalyear && e.AccountKey == model.AccountKey && e.CWPeriod == cwPeriod);
            if (period != null)
            {
                return period.GUID;
            }
            return Guid.Empty;
        }
        /// <summary>
        /// 获取帐套明细GUID
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Guid GetAccountDetailGUID(CWPeriodModel model)
        {
            if (model == null) return Guid.Empty;
            int fiscalyear = 0;
            if (!string.IsNullOrEmpty(model.FiscalYear))
            {
                int.TryParse(model.FiscalYear, out fiscalyear);
            }
            int cwPeriod = 0;
            if (!string.IsNullOrEmpty(model.CWPeriod))
            {
                int.TryParse(model.CWPeriod, out cwPeriod);
            }
            var acountdetail = this.InfrastructureContext.AccountDetailViews.FirstOrDefault(e=>e.FiscalYear==fiscalyear && e.accountkey==model.AccountKey);
            if (acountdetail != null)
            {
                return acountdetail.GUID;
            }
            return Guid.Empty;
        }
        /// <summary>
        /// 添加明显信息
        /// </summary>
        /// <param name="main"></param>
        public void AddDetail(CW_PZMain main, List<JsonAttributeModel> row, int orderNum)
        {
            AddDetailEx(main, row, orderNum);
        }

        public CW_PZDetail AddDetailEx(CW_PZMain main, List<JsonAttributeModel> row, int orderNum)
        {
            CW_PZDetail temp = new CW_PZDetail();
            temp.FillDefault(this, this.OperatorId);
            temp.Fill(row);
            temp.OrderNum = orderNum;
            temp.GUID_PZMAIN = main.GUID;
            temp.GUID = Guid.NewGuid();
            
            

            SetPzDetailSpecialAttribute(temp, row);
            SetTotalPZ(temp, row);
            main.CW_PZDetail.Add(temp);
            return temp;
        }

        private void SetPzDetailSpecialAttribute(CW_PZDetail detail, List<JsonAttributeModel> row)
        {
            if (detail == null || row == null) return;
            //部门特殊处理
            var attriitem = row.Find(e => e.n.ToLower() == "GUID_Department".ToLower());
            if (attriitem == null)
            {
                detail.GUID_Department = null;
            }
            else
            {
                Guid mg;
                if (Guid.TryParse(attriitem.v, out mg))
                {
                    detail.GUID_Department = mg;
                }
                else
                {
                    detail.GUID_Department = null;
                }
            }
            //项目特殊处理
            attriitem = row.Find(e => e.n.ToLower() == "GUID_Project".ToLower());
            if (attriitem == null)
            {
                detail.GUID_Project = null;
            }
            else
            {
                Guid mg;
                if (Guid.TryParse(attriitem.v, out mg))
                {
                    detail.GUID_Project = mg;
                }
                else
                {
                    detail.GUID_Project = null;
                }
            }
            //customer特殊处理
            attriitem = row.Find(e => e.n.ToLower() == "GUID_Customer".ToLower());
            if (attriitem == null)
            {
                detail.GUID_Customer = null;
            }
            else
            {
                Guid mg;
                if (Guid.TryParse(attriitem.v, out mg))
                {
                    detail.GUID_Customer = mg;
                }
                else
                {
                    detail.GUID_Customer = null;
                }
            }
            //person特殊处理
            attriitem = row.Find(e => e.n.ToLower() == "GUID_Person".ToLower());
            if (attriitem == null)
            {
                detail.GUID_Person = null;
            }
            else
            {
                Guid mg;
                if (Guid.TryParse(attriitem.v, out mg))
                {
                    detail.GUID_Person = mg;
                }
                else
                {
                    detail.GUID_Person = null;
                }
            }
            //SettleType特殊处理
            attriitem = row.Find(e => e.n.ToLower() == "GUID_SettleType".ToLower());
            if (attriitem == null)
            {
                detail.GUID_SettleType = null;
            }
            else
            {
                Guid mg;
                if (Guid.TryParse(attriitem.v, out mg))
                {
                    detail.GUID_SettleType = mg;
                }
                else
                {
                    detail.GUID_SettleType = null;
                }
            }
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
            CW_PZMain main = new CW_PZMain(); CW_PZDetail tempdetail = new CW_PZDetail();
            JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            main = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == g);
            if (main != null)
            {
                orgDateTime = main.DocDate;
            }
            main.FillDefault(this, this.OperatorId);
            main.Fill(jsonModel.m);
            main.ResetDefault(this, this.OperatorId);
            //main.GUID_CWPeriod = Guid.Parse("4CC43365-30A3-49A8-9B70-1E47D08D7101");//测试用


            var periodModel = GetCWPeriodModel(jsonModel);
            main.GUID_AccountDetail = GetAccountDetailGUID(periodModel);
            main.GUID_CWPeriod = GetCWPeriodGUID(periodModel);
            string detailModelName = tempdetail.ModelName();
            JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
            if (Grid == null)
            {
                foreach (CW_PZDetail detail in main.CW_PZDetail) { this.BusinessContext.DeleteConfirm(detail); }
            }
            else
            {
                List<CW_PZDetail> detailList = new List<CW_PZDetail>();
                foreach (CW_PZDetail detail in main.CW_PZDetail)
                {
                    detailList.Add(detail);
                }
                foreach (CW_PZDetail detail in detailList)
                {

                    List<JsonAttributeModel> row = Grid.r.Find(detail.GUID, detailModelName);
                    if (row == null) this.BusinessContext.DeleteConfirm(detail);
                    else
                    {
                        detail.OrderNum = Grid.r.IndexOf(row)+1;
                        detail.FillDefault(this, this.OperatorId);
                        detail.Fill(row);
                        detail.ResetDefault(this, this.OperatorId);
                        SetPzDetailSpecialAttribute(detail, row);
                        SetTotalPZ(detail, row);
                        this.BusinessContext.ModifyConfirm(detail);

                    }
                }
                List<List<JsonAttributeModel>> newRows = Grid.r.FindNew(detailModelName);//查找GUID为空的行，并且为新增
                foreach (List<JsonAttributeModel> row in newRows)
                {
                    AddDetail(main, row, Grid.r.IndexOf(row)+1);
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
            CW_PZMain main = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == guid);

            List<CW_PZDetail> details = new List<CW_PZDetail>();

            foreach (CW_PZDetail item in main.CW_PZDetail)
            {
                details.Add(item);
            }

            foreach (CW_PZDetail item in details) { BusinessContext.DeleteConfirm(item); }

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
            CW_PZMain oldPzMain = new CW_PZMain();
            List<CW_PZDetail> oldPzDetails = new List<CW_PZDetail>();
            JsonModel result = new JsonModel();
            var data = JsonHelp.ObjectToJson(jsonModel);
            try
            {
                Guid value = jsonModel.m.Id(new CW_PZMain().ModelName());
                var inoidvalue = jsonModel.m.GetValue("gl_accvouch", "Ino_id");
                int Orgin_Ino_id = inoidvalue == string.Empty ? 0 : int.Parse(inoidvalue);
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            try
                            {
                                value = this.Insert(jsonModel);
                            }
                            catch(Exception ex)
                            {
                                OperatorLog.WriteLog(this.OperatorId, "会计凭证", ex.Message, data, false);
                                result.result = JsonModelConstant.Error;
                                result.s = new JsonMessage("提示", "新建时,系统错误！", JsonModelConstant.Error);
                                return result;
                            }
                        }
                        break;
                    case "2": //修改
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            JsonAttributeModel id = jsonModel.m.IdAttribute(oldPzMain.ModelName());
                            
                            Guid g;
                            Guid.TryParse(id.v, out g);
                            oldPzMain = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == g);
                            oldPzDetails = this.BusinessContext.CW_PZDetail.Where(e => e.GUID_PZMAIN == oldPzMain.GUID).ToList<CW_PZDetail>();
                            try
                            {
                                value = this.Modify(jsonModel);
                            }
                            catch (Exception ex)
                            {
                                OperatorLog.WriteLog(this.OperatorId, "会计凭证", ex.Message, data, false);
                                result.result = JsonModelConstant.Error;
                                result.s = new JsonMessage("提示", "修改时,系统错误！", JsonModelConstant.Error);
                                return result;
                            }
                        }
                        break;
                    case "3": //删除
                        strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            JsonAttributeModel id = jsonModel.m.IdAttribute(oldPzMain.ModelName());

                            Guid g;
                            Guid.TryParse(id.v, out g);
                            oldPzMain = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == g);
                            oldPzDetails = this.BusinessContext.CW_PZDetail.Where(e => e.GUID_PZMAIN == oldPzMain.GUID).ToList<CW_PZDetail>();
                            try
                            {
                                this.Delete(value);
                            }
                            catch (Exception ex)
                            {
                                OperatorLog.WriteLog(this.OperatorId, "会计凭证", ex.Message, data, false);
                                result.result = JsonModelConstant.Error;
                                result.s = new JsonMessage("提示", "删除时,系统错误！", JsonModelConstant.Error);
                                return result;
                            }
                        }
                        break;

                }               
                if (string.IsNullOrEmpty(strMsg))
                {
                    
                    try
                    {
                        strMsg = SaveU8(value, status, Orgin_Ino_id);
                    }
                    catch
                    {
                        strMsg = "U8保存时出错！";
                    }
                    if (string.IsNullOrEmpty(strMsg))
                    {
                            
                        if (status == "3")//删除时返回默认值                        {
                            result = this.New();
                            strMsg = "删除成功！";
                        }
                        else
                        {
                            result = this.Retrieve(value);
                            strMsg = "保存成功！";
                        }
                        OperatorLog.WriteLog(this.OperatorId, value, status, "会计凭证", data);
                        result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Info);
                    }
                    else
                    {
                        //回滚数据
                        switch (status)
                        {
                            case "1"://新建 
                                this.Delete(value);
                                break;
                            case "2": //修改
                                this.Delete(value);
                                this.BusinessContext.CW_PZMain.AddObject(oldPzMain);
                                foreach (var detailitem in oldPzDetails)
                                {
                                    this.BusinessContext.CW_PZDetail.AddObject(detailitem);
                                }

                                this.BusinessContext.SaveChanges();
                                break;
                            case "3"://删除
                                this.BusinessContext.CW_PZMain.AddObject(oldPzMain);
                                foreach (var detailitem in oldPzDetails)
                                {
                                    this.BusinessContext.CW_PZDetail.AddObject(detailitem);
                                }
                                this.BusinessContext.SaveChanges();
                                break;
                        }
                            
                        result.result = JsonModelConstant.Error;
                        result.s = new JsonMessage("提示", strMsg, JsonModelConstant.Error);
                    }
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
                OperatorLog.WriteLog(this.OperatorId, "会计凭证", ex.Message, data, false);
                result.result = JsonModelConstant.Error;
                result.s = new JsonMessage("提示", "系统错误！", JsonModelConstant.Error);
                return result;
            }
        }
        /// <summary>
        /// U8保存
        /// </summary>
        /// <param name="mainGUID"></param>
        /// <returns></returns>
        public string SaveU8(Guid mainGUID, string status, int OrignIno_id = 0)        
        {           
            string strMsg = string.Empty;
            U8Result result = new U8Result();
            if (mainGUID == Guid.Empty) return null;
            U8Certificate u8obj = new U8Certificate(this.BusinessContext);
            CW_PZMainView pzMain = this.BusinessContext.CW_PZMainView.FirstOrDefault(e=>e.GUID==mainGUID);          
            if (pzMain == null) return null;
            switch (status)
            {
                case "1": //新建 
                    u8obj.Insert(pzMain,ref result);
                    break;
                case "2": //修改
                    u8obj.Update(pzMain, ref result, OrignIno_id);
                    break;
                case "3": //删除
                    u8obj.Delete(pzMain,ref result);
                    break;

            }
            if (result != null && !string.IsNullOrEmpty(result.ResultMessage))
            {
                strMsg = result.ResultMessage;
            }
            return strMsg;
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
        /// 历史
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public override List<object> History(SearchCondition conditions)
        {
            var lishijilu=new 历史记录(this.OperatorId,this.ModelUrl);
            var results = lishijilu.History(conditions);
            this.ErrorCode = lishijilu.ErrorCode;
            return results;
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
            CW_PZMain main = null;
            switch (status)
            {
                case "1": //新建
                    //main = LoadMain(jsonModel);//.Fill(jsonModel.m);
                    vResult = InsertVerify(jsonModel);//
                    var vf_u8 = VerifyResultU8(jsonModel);//U8验证
                    if (vf_u8 != null && vf_u8.Count > 0)
                    {
                        vResult._validation.AddRange(vf_u8);
                    }
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2": //修改
                    //main = LoadMain(jsonModel);
                    vResult = ModifyVerify(jsonModel);//修改验证
                     var vf_u8M = VerifyResultU8(jsonModel);//U8验证
                     if (vf_u8M != null && vf_u8M.Count > 0)
                    {
                        vResult._validation.AddRange(vf_u8M);
                    }
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "3": //删除
                    Guid value = jsonModel.m.Id(new BX_Main().ModelName());
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
        private CW_PZMain LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            CW_PZMain main = new CW_PZMain();
            main.Fill(jsonModel.m);

            if (jsonModel.d != null && jsonModel.d.Count > 0)
            {
                CW_PZDetail temp = new CW_PZDetail();
                string detailModelName = temp.ModelName();
                JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                if (Grid != null)
                {
                    foreach (List<JsonAttributeModel> row in Grid.r)
                    {
                        temp = new CW_PZDetail();                        
                        temp.Fill(row);                       
                        temp.GUID_PZMAIN = main.GUID;
                        SetTotalPZ(temp, row);
                        main.CW_PZDetail.Add(temp);
                    }
                }
            }

            return main;
        }
        private List<ValidationResult> VerifyResultDetailInsert(CW_PZMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<CW_PZDetail> detailList = new List<CW_PZDetail>();
            foreach (CW_PZDetail item in data.CW_PZDetail)
            {
                detailList.Add(item);
            }

            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CW_PZDetail item in detailList)
                {
                    rowIndex++;
                    var accounttitle = this.InfrastructureContext.CW_AccountTitleView.FirstOrDefault(e => e.GUID == item.GUID_AccountTitle);
                    if (accounttitle == null)
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 没有会计科目！"));
                        continue;
                    }

                    if (accounttitle.IsPerson == true && item.GUID_Person.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为人员核算,人员不能为空！"));
                    }
                    else if (accounttitle.IsPerson != true && !item.GUID_Person.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非人员核算,人员应为空！"));
                    }
                    else if (accounttitle.IsDepartment == true && item.GUID_Department.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为部门核算,部门不能为空！"));
                    }
                    else if (accounttitle.IsDepartment != true && !item.GUID_Department.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非部门核算,部门应为空！"));
                    }
                    else if (accounttitle.IsProject == true && item.GUID_Project.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为项目核算,项目不能为空！"));
                    }
                    else if (accounttitle.IsProject != true && !item.GUID_Project.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非项目核算,项目应为空！"));
                    }
                    else if (accounttitle.IsCustomer == true && item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为客户核算,客户不能为空！"));
                    }
                    else if (accounttitle.IsVCustomer == true && item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为供应商核算,供应商不能为空！"));
                    }
                    else if (accounttitle.IsCustomer != true && accounttitle.IsVCustomer != true && !item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非客户或供应商核算,客户或供应商应为空！"));
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
        /// 明显表验证


        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetailModify(CW_PZMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<CW_PZDetail> detailList = new List<CW_PZDetail>();
            foreach (CW_PZDetail item in data.CW_PZDetail)
            {
                detailList.Add(item);
            }
            
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CW_PZDetail item in detailList)
                {
                    rowIndex++;
                    var accounttitle = this.InfrastructureContext.CW_AccountTitleView.FirstOrDefault(e => e.GUID == item.GUID_AccountTitle);
                    if (accounttitle == null)
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 没有会计科目！"));
                        continue;
                    }

                    if (accounttitle.IsPerson == true && item.GUID_Person.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为人员核算,人员不能为空！"));
                    }
                    else if (accounttitle.IsPerson != true && !item.GUID_Person.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非人员核算,人员应为空！"));
                    }
                    else if (accounttitle.IsDepartment == true && item.GUID_Department.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为部门核算,部门不能为空！"));
                    }
                    else if (accounttitle.IsDepartment != true && !item.GUID_Department.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非部门核算,部门应为空！"));
                    }
                    else if (accounttitle.IsProject == true && item.GUID_Project.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为项目核算,项目不能为空！"));
                    }
                    else if (accounttitle.IsProject != true && !item.GUID_Project.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非项目核算,项目应为空！"));
                    }
                    else if (accounttitle.IsCustomer == true && item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为客户核算,客户不能为空！"));
                    }
                    else if (accounttitle.IsVCustomer == true && item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为供应商核算,供应商不能为空！"));
                    }
                    else if (accounttitle.IsCustomer != true && accounttitle.IsVCustomer != true && !item.GUID_Customer.IsNullOrEmpty())
                    {
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", "明细 为非客户或供应商核算,客户或供应商应为空！"));
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
        /// 明显表验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultDetailAbsolute(CW_PZMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //明细验证
            List<CW_PZDetail> detailList = new List<CW_PZDetail>();
            foreach (CW_PZDetail item in data.CW_PZDetail)
            {
                detailList.Add(item);
            }
            if (detailList != null && detailList.Count > 0)
            {
                var rowIndex = 0;
                foreach (CW_PZDetail item in detailList)
                {
                    rowIndex++;
                    var vf_detail = VerifyResult_CN_Detail(item, rowIndex);
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
        /// 验证U8数据
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultU8(JsonModel jsonModel)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            var model=GetCWPeriodModel(jsonModel);
            if (model != null)
            {
                if (string.IsNullOrEmpty(model.FiscalYear))
                {
                    str = "年度 不能为空！";
                    resultList.Add(new ValidationResult("", str));                    
                }
                if (string.IsNullOrEmpty(model.AccountKey))
                {
                    str = "凭证帐套 不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }//CWPeriod
                if (string.IsNullOrEmpty(model.CWPeriod))
                {
                    str = "会计期间 不能为空！";
                    resultList.Add(new ValidationResult("", str));
                }
            }
            else
            {
                str = "会计期间，年度，凭证帐套不能为空！";
                resultList.Add(new ValidationResult("", str));
            }
            return resultList;
        }
        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(CW_PZMain data)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            CW_PZMain mModel = data;
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
            //附单据数量




            if (mModel.BillCount != null && Common.ConvertFunction.TryParse(mModel.BillCount.GetType(), mModel.BillCount.ToString(), out g) == false)
            {
                str = "附单据数量 格式不正确！";
                resultList.Add(new ValidationResult("", str));

            }
            //摘要
            ////if (string.IsNullOrEmpty(mModel))
            ////{
            ////    str = "摘要 字段为必输入项！";
            ////    resultList.Add(new ValidationResult("", str));

            ////}

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

            #endregion
        }
       
        /// <summary>
        /// 明显表验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResult_CN_Detail(CW_PZDetail data, int rowIndex)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            object g;
            CW_PZDetail item = data;
            /// <summary>
            /// 明细表字段验证

            /// </summary>
            #region 明细表字段验证


            //报销金额
            if (item.Total_PZ.ToString() == "")
            {
                if (item.IsDC == true)
                {
                    str = "明细 借方金额字段为必输项！";
                }
                else
                {
                    str = "明细 贷方字段为必输项！";
                }               
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.Total_PZ.GetType(), item.Total_PZ.ToString(), out g) == false)
                {
                    if (item.IsDC == true)
                    {
                        str = "明细 借方金额格式不正确！";
                    }
                    else
                    {
                        str = "明细 贷方金额格式不正确！";
                    }
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
                else
                {
                    if (double.Parse(g.ToString()) == 0F)
                    {
                        if (item.IsDC == true)
                        {
                            str = "明细 借方金额不能为零！";
                        }
                        else
                        {
                            str = "明细 贷方金额不能为零！";
                        }
                       
                        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                    }
                }
            }
            //摘要

            if (string.IsNullOrEmpty(item.PZMemo))
            {
                str = "明细摘要 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.PZMemo.GetType(), item.PZMemo, out g) == false)
                {
                    str = "明细摘要格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                }
            }
            //项目GUID
            if (item.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_Project.GetType(), item.GUID_Project.ToString(), out g) == false)
            {
                str = "明细项目格式不正确！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            //部门GUID
            if (item.GUID_Department.IsNullOrEmpty())
            {
                str = "明细部门 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                {
                    str = "明细部门格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }

            //人员GUID
            if (item.GUID_Department.IsNullOrEmpty())
            {
                str = "明细人员 字段为必输项！";
                resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            }
            else
            {
                if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                {
                    str = "明细人员 格式不正确！";
                    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

                }
            }
            ////结算方式GUID
            //if (item.GUID_SettleType.IsNullOrEmpty())
            //{
            //    str = "明细结算方式 字段为必输项！";
            //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //}
            //else
            //{
            //    if (Common.ConvertFunction.TryParse(item.GUID_SettleType.GetType(), item.GUID_SettleType.ToString(), out g) == false)
            //    {
            //        str = "明细结算方式格式不正确！";
            //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));

            //    }
            //}

           

            #endregion

            return resultList;
        }
        /// <summary>
        /// u8校验
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        protected VerifyResult U8Verity(JsonModel jsonModel)
        {
            VerifyResult result = new VerifyResult();
            var vf_u8 = VerifyResultU8(jsonModel);
            if (vf_u8 != null && vf_u8.Count>0)
            {
                result._validation.AddRange(vf_u8);
            }
            return result;
        }

        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override VerifyResult InsertVerify(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            JsonModel jsonModel = (JsonModel)data;
            CW_PZMain model = new CW_PZMain();
            model = LoadMain(jsonModel);
            CW_PZMainView modelView = new CW_PZMainView();
            modelView.Fill(jsonModel.m);
            //主Model验证
            var vf_main = VerifyResultMain(model);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            //先判断借款和贷款金额是否相同
            double total_jf = 0, total_df = 0;
            foreach (CW_PZDetail item in model.CW_PZDetail)
            {
                if (item.IsDC)
                {
                    total_jf += item.Total_PZ;
                }
                else
                {
                    total_df += item.Total_PZ;
                }
            }
            if (Math.Abs(total_jf - total_df) > 0.001)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "借贷双方金额不相等，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //判断会计期间和日期是否相符
            var cwperiod = this.BusinessContext.CW_PeriodView.FirstOrDefault(e => e.FiscalYear == modelView.FiscalYear && e.AccountKey == modelView.AccountKey && e.CWPeriod == modelView.CWPeriod);
            if (cwperiod == null || cwperiod.FiscalYear != modelView.FiscalYear || cwperiod.CWPeriod != modelView.CWPeriod)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "会计期间填写错误，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            if (model.DocDate.Year != modelView.FiscalYear || model.DocDate.Month != modelView.CWPeriod)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "会计期间和日期不相符，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            //明细验证 
            var vf_detail = VerifyResultDetailInsert(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
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
            CW_PZMain bxMain = new CW_PZMain();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //报销单GUID

                str = "请选择删除项！";
            if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
            {
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


            CW_PZMain main = this.BusinessContext.CW_PZMain.FirstOrDefault(e => e.GUID == guid);
            if (main != null)
            {
                //if (main.DocState == "9")
                //{
                //    str = "此单已经作废！不能删除！";
                //    resultList.Add(new ValidationResult("", str));
                //}
            }
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
            JsonModel jsonModel = (JsonModel)data;
            CW_PZMain model = new CW_PZMain();
            model = LoadMain(jsonModel);
            CW_PZMainView modelView = new CW_PZMainView();
            modelView.Fill(jsonModel.m);
            
            CW_PZMain orgModel = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == model.GUID);           
            if (orgModel != null)
            {
                if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
                    result._validation = resultList;
                    return result;
                }
            }
            //流程验证
            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            ////作废           
            //if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
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
                return result;
            }
            //先判断借款和贷款金额是否相同
            double total_jf = 0, total_df = 0;
            foreach (CW_PZDetail item in model.CW_PZDetail)
            {
                if (item.IsDC)
                {
                    total_jf += item.Total_PZ;
                }
                else
                {
                    total_df += item.Total_PZ;
                }
            }
            if (Math.Abs(total_jf - total_df) > 0.001)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "借贷双方金额不相等，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //判断会计期间和日期是否相符
            var cwperiod = this.BusinessContext.CW_PeriodView.FirstOrDefault(e => e.FiscalYear == modelView.FiscalYear && e.AccountKey == modelView.AccountKey && e.CWPeriod == modelView.CWPeriod);
            if (cwperiod == null || cwperiod.FiscalYear!=modelView.FiscalYear || cwperiod.CWPeriod!=modelView.CWPeriod)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "会计期间填写错误，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            if (model.DocDate.Year != modelView.FiscalYear || model.DocDate.Month != modelView.CWPeriod)
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "会计期间和日期不相符，不能进行修改！"));
                result._validation = resultList;
                return result;
            }

            //明细验证 
            var vf_detail = VerifyResultDetailModify(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;
        }

        protected  VerifyResult ModifyVerifyAbsolute(object data)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            CW_PZMain model = (CW_PZMain)data;
            CW_PZMain orgModel = this.BusinessContext.CW_PZMain.Include("CW_PZDetail").FirstOrDefault(e => e.GUID == model.GUID);
            if (orgModel != null)
            {
                if (model.OAOTS.ArrayToString() != orgModel.OAOTS.ArrayToString())
                {
                    List<ValidationResult> resultList = new List<ValidationResult>();
                    resultList.Add(new ValidationResult("", "时间戳不一致，不能进行修改！"));
                    result._validation = resultList;
                    return result;
                }
            }
            //流程验证
            if (WorkFlowAPI.ExistProcess(model.GUID))
            {
                List<ValidationResult> resultList = new List<ValidationResult>();
                resultList.Add(new ValidationResult("", "此单正在流程审核中，不能进行修改！"));
                result._validation = resultList;
                return result;
            }
            ////作废           
            //if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
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
                return result;
            }


            //明细验证 
            var vf_detail = VerifyResultDetailModify(model);
            if (vf_detail != null && vf_detail.Count > 0)
            {
                result._validation.AddRange(vf_detail);
            }
            return result;
        }
        #endregion


        public void SetTotalPZ(CW_PZDetail Detail, List<JsonAttributeModel> row)
        {
            
            var LoanItem = row.Find(e => e.n.ToLower() == "Total_Loan".ToLower());
            var BorrowItem = row.Find(e => e.n.ToLower() == "Total_Borrow".ToLower());
            double Loan = 0, Borrow = 0;
            if (LoanItem!=null) double.TryParse(LoanItem.v,out Loan);
            if (BorrowItem != null) double.TryParse(BorrowItem.v, out Borrow);
            if (Math.Abs(Loan) > Math.Abs(Borrow))
            {
                Detail.IsDC = false;
                Detail.Total_PZ = Loan;
            }
            else //if (Borrow!=0 && Detail.Total_PZ!=Borrow)
            {
                Detail.IsDC = true;
                Detail.Total_PZ = Borrow;
            }
        }
    }
    /// <summary>
    /// 期间Model
    /// </summary>
    public class CWPeriodModel
    {
        /// <summary>
        /// 期间
        /// </summary>
        public string CWPeriod { set; get; }
        /// <summary>
        /// 年度
        /// </summary>
        public string FiscalYear { set; get; }
        /// <summary>
        /// 帐套
        /// </summary>
        public string AccountKey { set; get; }
    }

    
}
