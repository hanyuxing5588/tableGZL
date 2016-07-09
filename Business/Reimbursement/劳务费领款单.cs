using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using Platform.Flow.Run;
using BusinessModel;
namespace Business.Reimbursement
{
   
    public class 劳务费领款单:BXDocument
    {
         public 劳务费领款单() : base() { }
         public 劳务费领款单(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
         /// <summary>
         /// 更改单据状态


         /// </summary>
         /// <param name="guid">GUID</param>
         /// <param name="docState">单据状态</param>
         /// <returns>Bool</returns>
         public override bool UpdateDocState(Guid guid, EnumType.EnumDocState docState)
         {
             BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
             if (main != null)
             {
                 if (main.DocState != "999")
                 {
                     main.DocState = ((int)docState).ToString();
                 }
                 var dt=main.MakeDate ;
                 var error = "";
                 var curNode = WorkFlowAPI.GetCurNodeByDocId(main.GUID, out error);
                 if (string.IsNullOrEmpty(error))
                 {
                     //为工程院
                     if (curNode.NodeType ==5) {
                         main.DocState = ((int)EnumType.EnumDocState.NotApprove).ToString();
                     }
                     main.SubmitDate = DateTime.Now;
                 }
                 this.BusinessContext.SaveChanges();
                 return true;
             }
             return false;
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

                 BX_MainView model = new BX_MainView();
                 model.FillDefault(this, this.OperatorId);
                 jmodel.m = model.Pick();

                 BX_DetailView dModel = new BX_DetailView();
                 dModel.FillDetailDefault<BX_DetailView>(this, this.OperatorId, this.ModelUrl);//"lwfbxd"
                 jmodel.m.AddRange(dModel.Pick());   

                 CN_PaymentNumberView payment = new CN_PaymentNumberView();
                 payment.FillCN_PaymentNumberDefault(this, this.ModelUrl);//"lwfbxd"
                 jmodel.m.AddRange(payment.Pick());   
                //明细添加默认一行数据

                 //string[] item = { "Total_BX" };
                 //BX_InviteFeeView fModel = new BX_InviteFeeView();               
                 //fModel.FillDetailDefault<BX_InviteFeeView>(this, this.OperatorId, this.ModelUrl);//"lwfbxd"
                 //JsonGridModel fjgm = new JsonGridModel(fModel.ModelName());
                 //jmodel.d.Add(fjgm);
                 //List<JsonAttributeModel> picker = fModel.Pick(item.ToList());
                 //fjgm.r.Add(picker);


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
         public override JsonModel  Retrieve(Guid guid)
         {
             JsonModel jmodel = new JsonModel();
             try
             {
                 BX_MainView main = this.BusinessContext.BX_MainView.FirstOrDefault(e => e.GUID == guid);

                 if (main != null)
                 {
                     GetBXDetail(jmodel, main);
                     GetBXInviteFeeDetail(jmodel, main);
                     GetRetrieveDefault(jmodel);
                     jmodel.s = new JsonMessage("", "", "");
                 }
                 else
                 {
                     jmodel.result = JsonModelConstant.Info;
                     jmodel.s = new JsonMessage("提示", "无数据！", JsonModelConstant.Info);
                 }
                
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
        /// 报销明细信息(主页面信息)
        /// </summary>
        /// <param name="jmodel"></param>
        /// <param name="main"></param>
         private void GetBXDetail(JsonModel jmodel,BX_MainView main)
         {
             if(main==null) return;
             jmodel.m = main.Pick();//主单信息
             BX_DetailView detailView = this.BusinessContext.BX_DetailView.FirstOrDefault(e => e.GUID_BX_Main == main.GUID);
             if (detailView != null)
             {
                 jmodel.m.AddRange(detailView.Pick());
                 CN_PaymentNumberView payment = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e => e.GUID == detailView.GUID_PaymentNumber);
                 if (payment != null)
                 {
                     jmodel.m.AddRange(payment.Pick());
                 }
             }
            
         }
         /// <summary>
         /// 报销劳务费明细信息

         /// </summary>
         /// <param name="jmodel"></param>
         /// <param name="main"></param>
         private void GetBXInviteFeeDetail(JsonModel jmodel, BX_MainView main)
         {
             //劳务费明细

             var invitefeeList = this.BusinessContext.BX_InviteFeeView.Where(e => e.GUID_BX_Main == main.GUID).OrderBy(e => e.OrderNum).ToList();
             if (invitefeeList != null && invitefeeList.Count > 0)
             {
                 JsonGridModel feeJgm = new JsonGridModel(invitefeeList[0].ModelName());
                 jmodel.d.Add(feeJgm);
                 foreach (BX_InviteFeeView item in invitefeeList)
                 {
                     List<JsonAttributeModel> feeList = item.Pick();
                     feeList.Add(new JsonAttributeModel() { m = "BX_InviteFee", v = item.CredentialTypekey, n = "CredentialTypeKey" });
                     feeJgm.r.Add(feeList);
                 }
             }
         }
        /// <summary>
        /// 获取返回值的默认值

        /// </summary>
        /// <param name="jmodel"></param>
         private void GetRetrieveDefault(JsonModel jmodel)
         {
             //明细中f 填充默认值

             //BX_DetailView dModel = new BX_DetailView();
             //dModel.FillDetailDefault<BX_DetailView>(this, this.OperatorId, this.ModelUrl);
             //JsonGridModel fjgm = new JsonGridModel(dModel.ModelName());
             //jmodel.f.Add(fjgm);

             //List<JsonAttributeModel> fpicker = dModel.Pick();

             //CN_PaymentNumberView fpayment = new CN_PaymentNumberView();
             //fpayment.FillCN_PaymentNumberDefault(this, this.ModelUrl);
             //fpicker.AddRange(fpayment.Pick());

             //fjgm.r.Add(fpicker);

             //劳务费填充默认值

             // Bx_InviteFeeView feeviewModel=new  Bx_InviteFeeView();
         }
         public bool GetPersonRepeat(Guid bxMainId,List<string> sfzhNum,bool isAdd) {
             var sqlformat = "SELECT * FROM  dbo.BX_InviteFeeview WHERE InvitePersonIDCard='{0}' AND GUID_BX_Main='{1}'";
             var sql = string.Format(sqlformat, sfzhNum, bxMainId);
             if (isAdd) {
                 sql = string.Format("SELECT * FROM  dbo.BX_InviteFeeview WHERE InvitePersonIDCard='{0}'", sfzhNum, false) ;
             }
             try
             {
                 var dt = Business.Common.DataSource.ExecuteQuery(sql);
                 if (dt != null && dt.Rows.Count > 0) {
                     return true;
                 }
             }
             catch (Exception)
             {
                 
             }
             return false;
         }
         /// <summary>
         /// 添加
         /// </summary>
         /// <param name="jsonModel">JsonModel名称</param>
         /// <returns>GUID</returns>
         protected override Guid Insert(JsonModel jsonModel)
         {
             if (jsonModel.m == null) return Guid.Empty;
             BX_Main main = new BX_Main();            
             main.FillDefault(this, this.OperatorId);
             main.Fill(jsonModel.m);
             main.GUID = Guid.NewGuid();       
             main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString()); //ConvertFunction.GetNextDocNum<BX_Main>(this.BusinessContext,"DocNum");

             if (jsonModel.d != null && jsonModel.d.Count > 0)
             {
                AddBXDetail(jsonModel, main);
                AddBXInviteFee(jsonModel,main);                 
             }
             this.BusinessContext.BX_Main.AddObject(main);
             this.BusinessContext.SaveChanges();
             return main.GUID;
         }
        /// <summary>
        /// 添加报销明细信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
         private void AddBXDetail(JsonModel jsonModel,BX_Main main)
         {                
             //明细信息
             BX_Detail detail = new BX_Detail();
             detail.GUID = Guid.Empty;
             detail.FillDefault(this, this.OperatorId);
             detail.Fill(jsonModel.m);
             detail.GUID = Guid.NewGuid();
             detail.GUID_Person = main.GUID_Person;
             detail.GUID_BX_Main = main.GUID;
             detail.GUID_Department = main.GUID_Department;

             detail.CN_PaymentNumber = new CN_PaymentNumber();
             detail.CN_PaymentNumber.FillDefault(this, Guid.Empty);
             detail.CN_PaymentNumber.Fill(jsonModel.m);
             detail.CN_PaymentNumber.GUID = Guid.NewGuid();
             detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
             detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;
            
             detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
             detail.Total_BX = detail.Total_Real;
             //算税
             
             main.BX_Detail.Add(detail);            
            
         }
         /// <summary>
         /// 获取InvitePerson实体类

         /// </summary>
         /// <param name="row"></param>
         /// <returns></returns>
         private SS_InvitePerson GetInvitePersonModel(List<JsonAttributeModel> row)
         {
             SS_InvitePerson model = new SS_InvitePerson();
             if (row == null) return null;
             foreach (JsonAttributeModel att in row)
             {
                 if (att.m != null && att.m.Trim().ToLower() == "BX_InviteFee".ToLower() && att.n != null && att.n.Trim().ToLower() == "GUID_InvitePerson".ToLower())
                 {
                     model.InvitePersonName = att.v;
                 }
                 if (att.m != null && att.m.Trim().ToLower() == "BX_InviteFee".ToLower() && att.n != null && att.n.Trim().ToLower() == "InvitePersonIDCard".ToLower())
                 {
                     model.InvitePersonIDCard = att.v;
                 }
                 if (att.m != null && att.m.Trim().ToLower() == "BX_InviteFee".ToLower() && att.n != null && att.n.Trim().ToLower() == "CredentialTypekey".ToLower())
                 {
                     model.CredentialTypekey = att.v;
                 }
                 if (att.m != null && att.m.Trim().ToLower() == "SS_InvitePerson".ToLower() && att.n != null && att.n.Trim().ToLower() == "InvitePersonName".ToLower())
                 {
                     Guid g;
                     if (!string.IsNullOrEmpty(att.v))
                     {
                         model.InvitePersonName = att.v;
                     }
                 }

                 
             }
             return model;
         }
        /// <summary>
        /// 添加劳务费明细

        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
        /// <returns></returns>
         private void AddBXInviteFee(JsonModel jsonModel, BX_Main main)
         {
             //是否需要更新基础信息
             bool isSaveChange = false;
             //添加劳务费明细


             string feeModelName = new BX_InviteFee().ModelName();
             JsonGridModel feeGrid = jsonModel.d.Find(feeModelName);
             if (feeGrid != null)
             {
                 int orderNum = 0;
                 if (feeGrid.r.Count > 0)
                 {
                     AddBxInviteFeeDetail(feeGrid.r, main,orderNum);
                 }
                
             }
             
         }

         public bool IsExistSFZ(string zjlx,string zjNum) {
             var ent = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.CredentialTypekey == zjlx && e.InvitePersonIDCard == zjNum);
             if (ent != null) return true;
             return false;
         
         }
        /// <summary>
        /// 添加外聘人员
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
         private Guid AddInvitePerson(List<JsonAttributeModel> row)
         {
     
             Guid id=Guid.Empty;
             SS_InvitePerson invitePerson1 = GetInvitePersonModel(row);//特殊处理 手工录入时 new SS_InvitePerson();//
             //invitePerson.Fill(row);
             if (IsExist(invitePerson1, out id))
             {
                 return id;
             }
             else
             {

                 SS_InvitePerson invitePerson = this.InfrastructureContext.SS_InvitePerson.CreateObject();
                 invitePerson.InvitePersonName = invitePerson1.InvitePersonName;
                 Guid oldIP;
                 if (Guid.TryParse(invitePerson1.InvitePersonName, out oldIP))
                 {
                     var oldiviteperson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == oldIP);
                     if (oldiviteperson != null) invitePerson.InvitePersonName = oldiviteperson.InvitePersonName;
                 }



                 invitePerson.InvitePersonIDCard = invitePerson1.InvitePersonIDCard;
                 invitePerson.GUID = Guid.NewGuid();
                 //SS_CredentialType credentialtype = new SS_CredentialType();
                 //credentialtype.Fill(row);
                 //if (credentialtype != null && credentialtype.CredentialTypekey != null)
                 //{
                 //    invitePerson.CredentialTypekey =credentialtype.CredentialTypekey;
                 //}
                 invitePerson.CredentialTypekey = invitePerson1.CredentialTypekey;
                 if (IsExistSFZ(invitePerson.CredentialTypekey, invitePerson.InvitePersonIDCard)) {
                     throw new Exception("同一证件类型的证件号码相同");
                 }
                 this.InfrastructureContext.SS_InvitePerson.AddObject(invitePerson);
                 this.InfrastructureContext.SaveChanges();
                 id = invitePerson.GUID;
             }
             return id;

         }
        /// <summary>
        /// 判断是否存在，存在返回True 并且传出ID，否则返回False
        /// </summary>
         /// <param name="model">SS_InvitePerson名称</param>
        /// <param name="id">ID名称</param>
        /// <returns>Bool</returns>
         private bool IsExist(SS_InvitePerson model,out Guid id)
         {
             id = Guid.Empty;
             if(model==null)return false;
             var obj = this.InfrastructureContext.SS_InvitePersonView.FirstOrDefault(e=>e.InvitePersonName==model.InvitePersonName && e.InvitePersonIDCard==model.InvitePersonIDCard && e.CredentialTypekey==model.CredentialTypekey);
             if (obj != null)
             {
                 id = obj.GUID;
                 return true;
             }
             return false;
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
             BX_Main main = new BX_Main(); 
             JsonAttributeModel id = jsonModel.m.IdAttribute(main.ModelName());
             if (id == null) return Guid.Empty;
             Guid g;
             if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
             main = this.BusinessContext.BX_Main.Include("BX_Detail").Include("BX_Detail.CN_PaymentNumber").FirstOrDefault(e => e.GUID == g);
             if (main != null)
             {
                 orgDateTime = main.DocDate;
             }
             main.FillDefault(this, this.OperatorId);
             main.Fill(jsonModel.m);
              main.ResetDefault(this,this.OperatorId);
             //判断日期是否已经改变，如果编号带有启动日期，日期改变时，编号也要改变 待确定？
             //if (CreateDocNumber.IsDateChange(this.BusinessContext,orgDateTime, main.DocDate))
             //{
             //    main.DocNum = CreateDocNumber.GetNextDocNum(this.BusinessContext, main.GUID_DW, main.GUID_YWType, main.DocDate.ToString());
             //}
             ModifyBxDetail(jsonModel,main);
             ModifyBxInvitefeeDetail(jsonModel,main);
             this.BusinessContext.ModifyConfirm(main);
             this.BusinessContext.SaveChanges();
             return main.GUID;
         }
        /// <summary>
        /// 修改报销明细信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <param name="main"></param>
         private void ModifyBxDetail(JsonModel jsonModel,BX_Main main)
         {            
             BX_Detail detailModel = new BX_Detail();
             JsonAttributeModel id = jsonModel.m.IdAttribute(detailModel.ModelName());
             if (id == null)
             {
                 foreach (BX_Detail item in main.BX_Detail) { this.BusinessContext.DeleteConfirm(item); }
             }
             else
             {
               
                 List<BX_Detail> detailList = new List<BX_Detail>();
                 foreach (BX_Detail detail in main.BX_Detail)
                 {
                     detailList.Add(detail);
                 }
                 var orderNum = 0;
                 foreach (BX_Detail detail in detailList)
                 {
                     JsonAttributeModel row = jsonModel.m.IdAttribute(detail.ModelName());
                    
                     if (row == null) this.BusinessContext.DeleteConfirm(detail);
                     else
                     {
                         orderNum++;
                         detail.OrderNum = orderNum;
                         detail.FillDefault(this, this.OperatorId);
                         detail.Fill(jsonModel.m);
                         detail.ResetDefault(this, this.OperatorId);
                         detail.CN_PaymentNumber.Fill(jsonModel.m);
                         detail.CN_PaymentNumber.GUID_Project = detail.GUID_Project;
                         detail.CN_PaymentNumber.GUID_BGCode = detail.GUID_BGCode;
                         detail.GUID_PaymentNumber = detail.CN_PaymentNumber.GUID;
                         detail.Total_BX = detail.Total_Real;
                         //算税

                         this.BusinessContext.ModifyConfirm(detail);

                     }
                 }
                
             }
             
         }       
         /// <summary>
         /// 修改报销明细信息
         /// </summary>
         /// <param name="jsonModel"></param>
         /// <param name="main"></param>
         private void ModifyBxInvitefeeDetail(JsonModel jsonModel, BX_Main main)
         {
             BX_InviteFee tempFee = new BX_InviteFee();
             string detailModelName = tempFee.ModelName();
             JsonGridModel Grid = jsonModel.d == null ? null : jsonModel.d.Find(detailModelName);
             if (Grid == null)
             {
                 foreach (BX_InviteFee detail in main.BX_InviteFee) { this.BusinessContext.DeleteConfirm(detail); }
             }
             else
             {
                 List<BX_InviteFee> detailList = new List<BX_InviteFee>();
                 foreach (BX_InviteFee detail in main.BX_InviteFee)
                 {
                     detailList.Add(detail);
                 }
                 var orderNum = 0;
                 List<Guid> ListGuidInviteFee = new List<Guid>();
                 foreach (List<JsonAttributeModel> row in Grid.r)
                 {
                     orderNum++;
                     
                     SS_InvitePerson invitePerson = GetInvitePersonModel(row);//特殊处理
                     Guid tp; SS_InvitePerson entinvitePerson = null;
                     if (Guid.TryParse(invitePerson.InvitePersonIDCard, out tp))
                     {
                         entinvitePerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == tp);
                     }
                     else
                     {
                         entinvitePerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.InvitePersonIDCard == invitePerson.InvitePersonIDCard);
                     }
                     
                     Guid ipg;
                     Guid.TryParse(invitePerson.InvitePersonName, out ipg);
                     var ssperson = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == ipg && e.IDCard == invitePerson.InvitePersonIDCard);


                     BX_InviteFee newitem = new BX_InviteFee();
                     newitem.FillDefault(this, this.OperatorId);
                     newitem.Fill(row);
                     var detail = detailList.FirstOrDefault(e => e.GUID == newitem.GUID);
                     if (detail!=null)
                     {
                         ListGuidInviteFee.Add(detail.GUID);
                         detail.OrderNum = orderNum;
                         detail.FillDefault(this, this.OperatorId);
                         detail.Fill(row);
                         if (entinvitePerson == null)
                         {
                             if (ssperson != null)
                             {
                                 //是本单位人  添加外聘人员
                                 detail.GUID_InvitePerson = ssperson.GUID;
                                 var entNew = this.InfrastructureContext.SS_InvitePerson.CreateObject();
                                 entNew.GUID = ssperson.GUID;
                                 entNew.InvitePersonName = ssperson.PersonName;
                                 entNew.InvitePersonIDCard = ssperson.IDCard;
                                 entNew.CredentialTypekey = ssperson.IDCardType;
                                 this.InfrastructureContext.SS_InvitePerson.AddObject(entNew);
                             }
                             else
                             {
                                 detail.GUID_InvitePerson = AddInvitePerson(row);
                             }
                         }
                         else
                         {
                             detail.GUID_InvitePerson = entinvitePerson.GUID;
                         }
                         this.BusinessContext.ModifyConfirm(detail);
                     }
                     else {
                         newitem.GUID = Guid.NewGuid();
                         newitem.OrderNum = orderNum;
                         newitem.GUID_BX_Main = main.GUID;
                         main.BX_InviteFee.Add(newitem);
                         
                         ListGuidInviteFee.Add(newitem.GUID);
                         if (entinvitePerson == null)
                         {
                             if (ssperson != null)
                             {
                                 //是本单位人  添加外聘人员
                                 newitem.GUID_InvitePerson = ssperson.GUID;
                                 var entNew = this.InfrastructureContext.SS_InvitePerson.CreateObject();
                                 entNew.GUID = ssperson.GUID;
                                 entNew.InvitePersonName = ssperson.PersonName;
                                 entNew.InvitePersonIDCard = ssperson.IDCard;
                                 entNew.CredentialTypekey = ssperson.IDCardType;
                                 this.InfrastructureContext.SS_InvitePerson.AddObject(entNew);
                             }
                             else
                             {
                                 newitem.GUID_InvitePerson = AddInvitePerson(row);
                             }
                         }
                         else
                         {
                             newitem.GUID_InvitePerson = entinvitePerson.GUID;
                         }

                     }
                 }
                 foreach (BX_InviteFee detail in detailList)
                 {
                     if (!ListGuidInviteFee.Contains(detail.GUID))
                     {
                         this.BusinessContext.DeleteConfirm(detail);
                     }
                 }


             }
         }

         /// <summary>
         /// 添加劳务费信息

         /// </summary>
         /// <param name="newRows"></param>
         /// <param name="main"></param>
         /// <param name="orderNum"></param>
         private void AddBxInviteFeeDetail(List<List<JsonAttributeModel>> newRows, BX_Main main, int orderNum)
         {
             bool isSaveChange = false;            
            //List<SS_InvitePersonView> invitePersonList=this.InfrastructureContext.SS_InvitePersonView.ToList();
             foreach (List<JsonAttributeModel> row in newRows)
             {
                 orderNum++;
                 isSaveChange = NewMethod(main, orderNum, isSaveChange, row);          
                
             }
             if (isSaveChange)
             {
                 this.InfrastructureContext.SaveChanges();
             }
         }

         private bool NewMethod(BX_Main main, int orderNum, bool isSaveChange, List<JsonAttributeModel> row)
         {
             BX_InviteFee newitem = new BX_InviteFee();
             newitem.FillDefault(this, this.OperatorId);
             newitem.Fill(row);
             newitem.GUID = Guid.NewGuid();
             newitem.OrderNum = orderNum;
             newitem.GUID_BX_Main = main.GUID;
             main.BX_InviteFee.Add(newitem);


             SS_InvitePerson invitePerson = GetInvitePersonModel(row);//特殊处理
             Guid tp; SS_InvitePerson entinvitePerson = null;
             if (Guid.TryParse(invitePerson.InvitePersonIDCard, out tp))
             {
                 entinvitePerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == tp);
             }
             else
             {
                 entinvitePerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.InvitePersonIDCard == invitePerson.InvitePersonIDCard);
             }

             Guid ipg;
             Guid.TryParse(invitePerson.InvitePersonName, out ipg);
             var ssperson = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == ipg && e.IDCard == invitePerson.InvitePersonIDCard);

             if (newitem.GUID_InvitePerson.IsNullOrEmpty() || entinvitePerson == null)
             {
                 if (ssperson != null)
                 {
                     //是本单位人  添加外聘人员
                     newitem.GUID_InvitePerson = ssperson.GUID;
                     var entNew = this.InfrastructureContext.SS_InvitePerson.CreateObject();
                     entNew.GUID = ssperson.GUID;
                     entNew.InvitePersonName = ssperson.PersonName;
                     entNew.InvitePersonIDCard = ssperson.IDCard;
                     entNew.CredentialTypekey = ssperson.IDCardType;
                     this.InfrastructureContext.SS_InvitePerson.AddObject(entNew);
                 }
                 else
                 {
                     newitem.GUID_InvitePerson = AddInvitePerson(row);
                 }
                 isSaveChange = true;
             }
             else
             {
                 newitem.GUID_InvitePerson = entinvitePerson.GUID;
                 isSaveChange = true;
             }

             //SS_InvitePerson invitePerson = GetInvitePersonModel(row);//特殊处理
             //var entinvitePerson = this.InfrastructureContext.SS_InvitePerson.FirstOrDefault(e => e.GUID == newitem.GUID_InvitePerson);
             //var ssperson = this.InfrastructureContext.SS_Person.FirstOrDefault(e => e.GUID == newitem.GUID_InvitePerson);
             //if (newitem.GUID_InvitePerson.IsNullOrEmpty() || (entinvitePerson == null))//添加外聘人员 如果是人员档案中的人也要添加到外聘人员中
             //{
             //    if (ssperson == null)
             //    {
             //        newitem.GUID_InvitePerson = AddInvitePerson(row);
             //    }
             //    else
             //    {//是本单位人  添加外聘人员
             //        newitem.GUID_InvitePerson = ssperson.GUID;
             //        var entNew = this.InfrastructureContext.SS_InvitePerson.CreateObject();
             //        entNew.GUID = ssperson.GUID;
             //        entNew.InvitePersonName = ssperson.PersonName;
             //        entNew.InvitePersonIDCard = ssperson.IDCard;
             //        entNew.CredentialTypekey = ssperson.IDCardType;
             //        this.InfrastructureContext.SS_InvitePerson.AddObject(entNew);
             //    }
             //    isSaveChange = true;
             //}

             return isSaveChange;
         }
        /// <summary>
        /// 判断是否存在外聘人员
        /// </summary>
        /// <param name="invitePerson"></param>
        /// <returns></returns>
         private bool IsExistInvitePerson( List<SS_InvitePersonView> invitePersonList,SS_InvitePerson invitePerson)
         {
             if (invitePersonList != null && invitePersonList.Count > 0)//人员档案中是否在外聘人员中存在，如果在外聘人员中不存在，添加一条数据

             {
                 var list = invitePersonList.FindAll(e => e.InvitePersonName == invitePerson.InvitePersonName && e.InvitePersonIDCard == invitePerson.InvitePersonIDCard && e.CredentialTypekey == invitePerson.CredentialTypekey);
                 if (list != null && list.Count > 0)
                 {
                     return true;
                 }
             }
             return false;
         }
         /// <summary>
         /// 删除
         /// </summary>
         /// <param name="guid">GUID</param>
         protected override void Delete(Guid guid)
         {            
             BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
             List<BX_Detail> details = new List<BX_Detail>();

             foreach (BX_Detail item in main.BX_Detail)
             {
                 details.Add(item);
             }

             foreach (BX_Detail item in details) { BusinessContext.DeleteConfirm(item); }

             //删除劳务费


             List<BX_InviteFee> feeList = new List<BX_InviteFee>();
             foreach (BX_InviteFee item in main.BX_InviteFee)
             {
                 feeList.Add(item);
             }
             if (feeList != null && feeList.Count > 0)
             {
                 foreach (BX_InviteFee item in feeList)
                 {
                     BusinessContext.DeleteConfirm(item);
                 }
             }
             //删除单据同时 删除单据在流程的信息（确定过（张龙））

             WorkFlowAPI.DeleteProcessInfoByDocId(guid);
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
             var data = JsonHelp.ObjectToJson(jsonModel);
             try
             {
                 Guid value = jsonModel.m.Id(new BX_Main().ModelName());
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
                         ////算税
                         //var doFax = new Business.Casher.GCYDoFax(value);
                         //doFax.DoTaxCaculte(this.BusinessContext, true);
                         result = this.Retrieve(value);
                         strMsg = "保存成功！";
                     }
                     //var time1 = DateTime.Now;
                     OperatorLog.WriteLog(this.OperatorId, value, status, "劳务费领款单", data);
                     //var time2 = DateTime.Now;
                     //var sub = time2.Subtract(time1).TotalSeconds;
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
                 //OperatorLog.WriteLog(this.OperatorId, "劳务费领款单", ex.Message, data, false);
                 result.result = JsonModelConstant.Error;
                 result.s = new JsonMessage("提示", ex.Message, JsonModelConstant.Error);
                 return result;
             }
         }
       
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
             BX_Main main = null; ; //new BX_Main();

             switch (status)
             {
                 case "1": //新建
                     main = LoadBX_Main(jsonModel,out strMsg);//.Fill(jsonModel.m);
                     if (!string.IsNullOrEmpty(strMsg)) break;
                     vResult = InsertVerify(main);//
                     if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                     {
                         for (int i = 0; i < vResult.Validation.Count; i++)
                         {
                             strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                         }
                     }
                     break;
                 case "2": //修改
                     main = LoadBX_Main(jsonModel, out strMsg);
                     if (!string.IsNullOrEmpty(strMsg)) break;
                     vResult = ModifyVerify(main);//修改验证
                     if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                     {
                         for (int i = 0; i < vResult.Validation.Count; i++)
                         {
                             strMsg += vResult.Validation[i].MemberName + vResult.Validation[i].Message + "<br>";//"\n";
                         }
                     }
                     break;
                 case "3": //删除
                     Guid value = jsonModel.m.Id(new BX_Main().ModelName());
                     vResult = DeleteVerify(value);
                     if (vResult != null && vResult.Validation != null && vResult.Validation.Count > 0)
                     {
                         strMsg = vResult.Validation[0].Message + "<br>";//"\n";
                     }
                     break;

             }
             return strMsg;
         }
         /// <summary>
         /// 加载主Model信息
         /// </summary>
         /// <param name="jsonModel"></param>
         /// <returns></returns>
         private BX_Main LoadBX_Main(JsonModel jsonModel,out string msg)
         {
             msg = "";
             if (jsonModel.m == null) return null;
             BX_Main main = new BX_Main();
             main.Fill(jsonModel.m);             
             //明细信息
             BX_Detail detail = new BX_Detail();
             detail.Fill(jsonModel.m);
             detail.GUID_Department = main.GUID_Department;

             //支付码

             CN_PaymentNumber payment = new CN_PaymentNumber();
             payment.Fill(jsonModel.m);
             detail.CN_PaymentNumber = payment;
             main.BX_Detail.Add(detail);
             //劳务费明细

             if (jsonModel.d != null && jsonModel.d.Count > 0)
             {
                 BX_InviteFee temp = new BX_InviteFee();
                 string detailModelName = temp.ModelName();
                 JsonGridModel Grid = jsonModel.d.Find(detailModelName);
                 if (Grid != null)
                 {
                     foreach (List<JsonAttributeModel> row in Grid.r)
                     {
                         msg = GetVaildSFZ(row);
                         if (!string.IsNullOrEmpty(msg)) break;
                         temp = new BX_InviteFee();                        
                         temp.Fill(row);                        
                         main.BX_InviteFee.Add(temp);
                     }
                 }
             }

             return main;
         }

         private string GetVaildSFZ(List<JsonAttributeModel> row) 
         {
             try
             {

            
             var personId = row.FirstOrDefault(e => e.n == "GUID_InvitePerson");
             if (personId == null && personId.v == Guid.Empty.ToString())
             {
                 var card = row.First(e => e.n == "InvitePersonIDCard");
                 var cardType = row.First(e => e.n == "CredentialTypeKey");
                 var ent = this.InfrastructureContext.SS_InvitePersonView.FirstOrDefault(e => e.InvitePersonIDCard == card.v && e.CredentialTypekey == cardType.v);
                 if (ent != null) {
                     return ent.CredentialTypeName+"类型的号码为"+card.v+",重复不能保存。";
                 }
             }
             return "";
             }
             catch (Exception)
             {

                 return "";
             }
         }
         /// <summary>
         /// 明显表验证

         /// </summary>
         /// <param name="data"></param>
         /// <returns></returns>
         private List<ValidationResult> VerifyResult_Bx_Detail(BX_Detail data, int rowIndex)
         {
             string str = string.Empty;
             List<ValidationResult> resultList = new List<ValidationResult>();
             object g;
             BX_Detail item = data;

             /// <summary>
             /// 明细表字段验证

             /// </summary>
             #region 明细表字段验证


             //预算科目的GUID
             if (item.GUID_BGCode.IsNullOrEmpty())
             {
                 str = "预算科目 字段为必输项!";
                // resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(item.GUID_BGCode.GetType(), item.GUID_BGCode.ToString(), out g) == false)
                 {
                     str = "预算科目格式不正确！";
                     // resultList.Add(new ValidationResult("", str));

                 }
             }
                                      
             //项目GUID
             if (item.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_Project.GetType(), item.GUID_Project.ToString(), out g) == false)
             {
                 str = "项目格式不正确！";
                 //resultList.Add(new ValidationResult("", str));

             }
             //部门GUID
             if (item.GUID_Department.IsNullOrEmpty())
             {
                 str = "部门 字段为必输项！";
                //  resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(item.GUID_Department.GetType(), item.GUID_Department.ToString(), out g) == false)
                 {
                     str = "部门格式不正确！";
                     resultList.Add(new ValidationResult("", str));

                 }
             }
             //结算方式GUID
             if (item.GUID_SettleType.IsNullOrEmpty())
             {
                 str = "结算方式 字段为必输项！";
                 // resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(item.GUID_SettleType.GetType(), item.GUID_SettleType.ToString(), out g) == false)
                 {
                     str = "结算方式格式不正确！";
                     // resultList.Add(new ValidationResult("", str));

                 }
             }
            
             //财政支付码GUID
             if (item.GUID_PaymentNumber.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.GUID_PaymentNumber.GetType(), item.GUID_PaymentNumber.ToString(), out g) == false)
             {
                 str = "财政支付码格式不正确！";
                 //  resultList.Add(new ValidationResult("", str));

             }


             #endregion

             #region 支付码验证


             if (item.CN_PaymentNumber != null)
             {
                 //var vf_pn = VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex);
                 var vf_pn = base.VerifyResult_CN_PaymentNumber(item.CN_PaymentNumber, rowIndex, item.BX_Main.GUID_DW);
                 if (vf_pn != null && vf_pn.Count > 0)
                 {
                     resultList.AddRange(vf_pn);
                 }
             }
             #endregion
             return resultList;
         }
         /// <summary>
         /// 支付码验证


         /// </summary>
         /// <param name="data"></param>
         /// <returns></returns>
         private List<ValidationResult> VerifyResult_CN_PaymentNumber(CN_PaymentNumber data, int rowIndex)
         {
             string str = string.Empty;
             List<ValidationResult> resultList = new List<ValidationResult>();
             object g;
             /// <summary>
             /// 财富支付码表字段验证
             /// </summary>
             #region 财富支付码表字段验证


             if (!string.IsNullOrEmpty(data.PaymentNumber) && Common.ConvertFunction.TryParse(data.PaymentNumber.GetType(), data.PaymentNumber, out g) == false)
             {
                 str = "财政支付码格式不正确！";
                 resultList.Add(new ValidationResult("", str));

             }
             //是否国库
             if (data.IsGuoKu.ToString() == "")
             {
                 str = "是否国库 字段为必输项！";
                 resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(data.IsGuoKu.GetType(), data.IsGuoKu.ToString(), out g) == false)
                 {
                     str = "是否国库格式不能为空！";
                     resultList.Add(new ValidationResult("", str));
                 }
                 else
                 {

                     //如果不为空则,则支付码不能为空
                     if (data.IsGuoKu == true && string.IsNullOrEmpty(data.PaymentNumber))
                     {
                         str = "财政支付令不能为空！";
                         resultList.Add(new ValidationResult("", str));
                     }
                 }
             }
             //是否项目
             if (data.IsProject.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.IsProject.GetType(), data.IsProject.ToString(), out g) == false)
             {
                 str = "项目格式不正确！";
                 resultList.Add(new ValidationResult("", str));

             }
             //功能分类GUID
             if (data.GUID_FunctionClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_FunctionClass.GetType(), data.GUID_FunctionClass.ToString(), out g) == false)
             {
                 str = "功能分类格式不正确！";
                 resultList.Add(new ValidationResult("", str));

             }
             //预算科目GUID
             if (data.GUID_BGCode.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGCode.GetType(), data.GUID_BGCode.ToString(), out g) == false)
             {
                 str = "预算科目格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }
             //经济分类GUID
             if (data.GUID_EconomyClass.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_EconomyClass.GetType(), data.GUID_EconomyClass.ToString(), out g) == false)
             {
                 str = "经济分类格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }
             //支出类型GUID
             if (data.GUID_ExpendType.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_ExpendType.GetType(), data.GUID_ExpendType.ToString(), out g) == false)
             {
                 str = "支出类型格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }
             //项目GUID
             if (data.GUID_Project.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_Project.GetType(), data.GUID_Project.ToString(), out g) == false)
             {
                 str = "项目格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }
             //项目财政编号
             if (!string.IsNullOrEmpty(data.FinanceProjectKey) && Common.ConvertFunction.TryParse(data.FinanceProjectKey.GetType(), data.FinanceProjectKey.ToString(), out g) == false)
             {
                 str = "项目财政编号格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }
             //预算来源GUID
             if (data.GUID_BGResource.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(data.GUID_BGResource.GetType(), data.GUID_BGResource.ToString(), out g) == false)
             {
                 str = "预算来源格式不能为空！";
                 resultList.Add(new ValidationResult("", str));

             }

             #endregion

             return resultList;
         }
         /// <summary>
         /// 主表验证
         /// </summary>
         /// <param name="data"></param>
         /// <returns></returns>
         private List<ValidationResult> VerifyResult_BX_Main(BX_Main data)
         {
             string str = string.Empty;
             List<ValidationResult> resultList = new List<ValidationResult>();
             BX_Main mModel = data;
             object g;

             #region   主表字段验证

             //报销日期
             if (mModel.DocDate.IsNullOrEmpty())
             {
                 str = "报销日期 字段为必输项！";
                 resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(mModel.DocDate.GetType(), mModel.DocDate.ToString(), out g) == false)
                 {
                     str = "报销日期 格式不正确！";
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
             if (mModel.DocMemo != null && Common.ConvertFunction.TryParse(mModel.DocMemo.GetType(), mModel.DocMemo, out g) == false)
             {
                 str = "摘要 格式不正确！";
                 resultList.Add(new ValidationResult("", str));

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

             //报销人GUID
             if (mModel.GUID_Person.IsNullOrEmpty())
             {
                 str = "报销人 字段为必输项！";
                 resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(mModel.GUID_Person.GetType(), mModel.GUID_Person.ToString(), out g) == false)
                 {
                     str = "报销人格式不正确！";
                     resultList.Add(new ValidationResult("", str));

                 }
             }
             //报销人部门




             if (mModel.GUID_Department.IsNullOrEmpty())
             {
                 str = "报销部门 字段为必输项!";
                 resultList.Add(new ValidationResult("", str));

             }
             else
             {
                 if (Common.ConvertFunction.TryParse(mModel.GUID_Department.GetType(), mModel.GUID_Department.ToString(), out g) == false)
                 {
                     str = "报销部门格式不正确！";
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

             #endregion
         }
        /// <summary>
        /// 劳务费明细验证

        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
         private List<ValidationResult> VerifyResult_InviteFee(BX_Main data)
         {
             List<ValidationResult> resultList = new List<ValidationResult>();
             object g;
             string str = string.Empty;
             BX_Main mModel = data;
             if (mModel == null) return null;
             List<BX_InviteFee> invitefeeList = new List<BX_InviteFee>();
             foreach (BX_InviteFee item in mModel.BX_InviteFee)
             {
                 invitefeeList.Add(item);
             }
             if (invitefeeList.Count == 0)
             {
                 str = "尚未添加劳务费明细项！";
                 resultList.Add(new ValidationResult("", str));
             }
             int rowIndex = 0;            
             foreach (BX_InviteFee item in invitefeeList)
             {
                 rowIndex++;


                 if (item.IsTotalTax.IsNullOrEmpty() == false && Common.ConvertFunction.TryParse(item.IsTotalTax.GetType(),item.IsTotalTax.ToString(), out g) == false)
                 {
                     str = "是否合并计税 字段格式不正确！";
                     resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 }
                 //if (item.GUID_InvitePerson.IsNullOrEmpty())
                 //{
                 //    str = "外聘人员 字段为必输入项！";
                 //    resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 //}
                 //else
                 //{
                 //    if (Common.ConvertFunction.TryParse(item.GUID_InvitePerson.GetType(),item.GUID_InvitePerson.ToString(),out g) == false)
                 //    {
                 //        str = "外聘人员 字段数据格式不正确！";
                 //        resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 //    }
                 //}
                 if (item.Total_BX.ToString()=="")
                 {
                     str = "实领金额 字段为必输入项！";
                     resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 }
                 else
                 {
                     if (Common.ConvertFunction.TryParse(item.Total_BX.GetType(), item.Total_BX.ToString(), out g) == false)
                     {
                         str = "实领金额 字段数据格式不正确！";
                         resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                     }
                     else
                     {
                         if (double.Parse(g.ToString()) == 0F)
                         {
                             str = "实领报销金额不能为零！";
                             resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                         }
                     }
                 }

                 if (item.Total_Tax.ToString() == "")
                 {
                     str = "税额 字段为必输入项！";
                     resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 }
                 else
                 {
                     if (Common.ConvertFunction.TryParse(item.Total_Tax.GetType(), item.Total_Tax.ToString(), out g) == false)
                     {
                         str = "税额 字段数据格式不正确！";
                         resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                     }                     
                 }

                
                 if (item.OrderNum.ToString() != "" && Common.ConvertFunction.TryParse(item.OrderNum.GetType(), item.OrderNum.ToString(), out g) == false)
                 {
                     str = "明细排序 字段数据格式不正确！";
                     resultList.Add(new ValidationResult("第" + rowIndex.ToString() + "行", str));
                 }
                
             }

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
             BX_Main model = (BX_Main)data;
             //主Model验证
             var vf_main = VerifyResult_BX_Main(model);
             if (vf_main != null && vf_main.Count > 0)
             {
                 result._validation.AddRange(vf_main);
             }
             //明细验证
             List<BX_Detail> bx_DetailList = new List<BX_Detail>();
             foreach (BX_Detail item in model.BX_Detail)
             {
                 bx_DetailList.Add(item);
             }
             if (bx_DetailList != null && bx_DetailList.Count > 0)
             {
                 //验证人名和证件号及证件类型不能有重复

                 var rowIndex = 0;
                 foreach (BX_Detail item in bx_DetailList)
                 {
                     rowIndex++;
                     var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
                     if (vf_detail != null && vf_detail.Count > 0)
                     {
                         result._validation.AddRange(vf_detail);
                     }
                 }
             }
             //劳务费验证

             var vfInvitefee=VerifyResult_InviteFee(model);
             if (vfInvitefee != null && vfInvitefee.Count > 0)
             {
                 result._validation.AddRange(vfInvitefee);
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
             BX_Main bxMain = new BX_Main();
             string str = string.Empty;
             //验证信息
             List<ValidationResult> resultList = new List<ValidationResult>();
             //报销单GUID

             if (bxMain.GUID == null || bxMain.GUID.ToString() == "")
             {
                 str = "请选择删除项！";
                 resultList.Add(new ValidationResult("", str));
                 result._validation = resultList;
                 return result;
             }
             else
             {
                 //object g;
                 //if (Common.ConvertFunction.TryParse(guid.GetType(), guid.ToString(), out g))
                 //{
                 //    str = "报销单GUID格式不正确！";
                 //    resultList.Add(new ValidationResult("", str));
                 //}

             }
             //流程验证
             //作废时也要判断 不能删除？待定

             if (WorkFlowAPI.ExistProcess(guid))
             {
                 str = "此单据已经进入审批流程，不能进行删除!";
                 resultList.Add(new ValidationResult("", str));
                 result._validation = resultList;
                 return result;
             }
             //作废的不能删除

             BX_Main main = this.BusinessContext.BX_Main.FirstOrDefault(e => e.GUID == guid);
             if (main != null)
             {
                 if (main.DocState == "9")
                 {
                     str = "此报销单已经作废！不能删除！";
                     resultList.Add(new ValidationResult("", str));
                     result._validation = resultList;
                     return result;
                 }
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
             BX_Main model = (BX_Main)data;
             //if (model.GUID_Maker != this.OperatorId)
             //{
             //    List<ValidationResult> resultList = new List<ValidationResult>();
             //    resultList.Add(new ValidationResult("", "制单人与修改人不是同一个人，不能修改！"));
             //    result._validation = resultList;
             //    return result;
             //}
             BX_Main orgModel = this.BusinessContext.BX_Main.Include("BX_Detail").FirstOrDefault(e => e.GUID == model.GUID);
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
             if (WorkFlowAPI.ExistProcessCurPerson(model.GUID,OperatorId))
             {
                 List<ValidationResult> resultList = new List<ValidationResult>();
                 resultList.Add(new ValidationResult("", "此劳务费领款单正在流程审核中，不能进行修改！"));
                 result._validation = resultList;
                 return result;
             }
             //作废
             if (orgModel != null && orgModel.DocState == "9" && model.DocState != ((int)Business.Common.EnumType.EnumDocState.RcoverState).ToString())
             {
                 List<ValidationResult> resultList = new List<ValidationResult>();
                 resultList.Add(new ValidationResult("", "此报销单已经作废，不能进行修改！"));
                 result._validation = resultList;
                 return result;
             }
             //主Model验证
             var vf_main = VerifyResult_BX_Main(model);
             if (vf_main != null && vf_main.Count > 0)
             {
                 result._validation.AddRange(vf_main);
             }
             //明细验证
             List<BX_Detail> bx_DetailList = new List<BX_Detail>();
             foreach (BX_Detail item in model.BX_Detail)
             {
                 bx_DetailList.Add(item);
             }
             if (bx_DetailList != null && bx_DetailList.Count > 0)
             {
                 var rowIndex = 0;
                 foreach (BX_Detail item in bx_DetailList)
                 {
                     rowIndex++;
                     var vf_detail = VerifyResult_Bx_Detail(item, rowIndex);
                     if (vf_detail != null && vf_detail.Count > 0)
                     {
                         result._validation.AddRange(vf_detail);
                     }
                 }
             }
             //劳务费验证

             var vfInvitefee = VerifyResult_InviteFee(model);
             if (vfInvitefee != null && vfInvitefee.Count > 0)
             {
                 result._validation.AddRange(vfInvitefee);
             }

             return result;
         }


    }
}
