using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using System.Reflection;
using System.Data.Objects;
using BusinessModel;
namespace Business.Casher
{
      public class CDocTrans
    {

      public List<Bill> ListBill { get; set;}
      public List<BillDetail> ListBillDetail { get;set; }
      public Guid OperatorId { set; get; }
      private Infrastructure.BaseConfigEdmxEntities dbBaseEntity;
      private BusinessEdmxEntities dbBusinessEntity;
      public CDocTrans() 
      {
          this.dbBaseEntity = new BaseConfigEdmxEntities();
          this.dbBusinessEntity = new BusinessEdmxEntities();
      }
      public CDocTrans(BaseConfigEdmxEntities dbBaseEntity, BusinessEdmxEntities dbBusinessEntity,Guid operatorId)
      {
          this.dbBaseEntity = dbBaseEntity;
          this.dbBusinessEntity = dbBusinessEntity;
          this.OperatorId = operatorId;
      }
      public CDocTrans(BaseConfigEdmxEntities dbBaseEntity,BusinessEdmxEntities dbBusinessEntity, Guid OperatorId, List<Bill> listBill, List<BillDetail> listBillDetail)          
      {
          this.dbBaseEntity = dbBaseEntity ;
          this.dbBusinessEntity = dbBusinessEntity;
          this.OperatorId = OperatorId;
          this.ListBill = listBill;
          this.ListBillDetail = listBillDetail;
      }
      #region 现金存储转换成出纳
      /// <summary>
      /// 现金存储转换成出纳
      /// </summary>
      /// <param name="listBill"></param>
      /// <param name="listBillDetail"></param>
      public void CashTransferCN(DateTime hxDateTime,EnumType.EnumCNType cnType, out List<Bill> listBill, out List<BillDetail> listBillDetail, out string message)

      {
          message = string.Empty;
          listBill = new List<Bill>();
          listBillDetail = new List<BillDetail>();
          #region 校验信息
          if (this.ListBill == null || this.ListBill.Count==0 || this.ListBillDetail.Count == 0)
          {
              message = "有提现单不存在，无法转换!";
          }
          foreach (Bill item in this.ListBill)
          {
              if (item.ClassId !=(int)EnumType.EnumClass.CN_CashMain)
              {
                  message = "存在不为提现单的对象，无法转换!";
                  return;
              }
          }
          //'校验提现单Main的单位，部门和人员是否都相同
          var dw = this.ListBill[0].GUID_DW;
          var department = this.ListBill[0].GUID_Department;
          var person = this.ListBill[0].GUID_Person;
          foreach (Bill item in this.ListBill)
          {
              if (item.GUID_DW != dw)
              {
                  message = "提现单单位不一致，无法转换!";
                  return;
              }
              else if (item.GUID_Department != department)
              {
                  message = "提现单部门不一致，无法转换!";
                  return;
              }
              else if (item.GUID_Person != person)
              {
                  message = "提现单人员不一致，无法转换!";
                  return;
              }
          }

          #endregion
          #region 转换单元
          //结算方式
          var settleType = this.dbBaseEntity.SS_SettleType.FirstOrDefault(e => e.SettleTypeKey == "01");
          //单据类型
          var docKey=cnType== EnumType.EnumCNType.出纳付款单 ? 
              ((int)EnumType.EnumCNType.出纳付款单).ToString() : ((int)EnumType.EnumCNType.出纳收款单).ToString();
          var docType = this.dbBaseEntity.SS_DocTypeView.FirstOrDefault(e => e.DocTypeKey == docKey);
          var ywType = this.dbBaseEntity.SS_YWType.FirstOrDefault(e => e.YWTypeKey == "08");
          //生成CN_Mian数据列表
          foreach (Bill item1 in this.ListBill)
          {
              var item = new Bill();
              item.CopyCommField(item1);
              item.GUID_DW = dw;
              item.GUID_Department = department;
              item.GUID_Person = person;
              item.DocDate = hxDateTime.ToString("yyyy-MM-dd");
              if (ywType != null)
              {
                  item.GUID_YWType = ywType.GUID;
                  item.YWTypeKey = ywType.YWTypeKey;
              }
              if (docType != null)
              {

                  item.DocTypeKey = docType.DocTypeKey;
                  item.ClassId = 35;
                  item.DocTypeName = cnType == EnumType.EnumCNType.出纳付款单 ? "出纳付款单" : "出纳收款单";
                  item.GUID_DocType = docType.GUID;                  
              }
              listBill.Add(item);  
              //明细信息
              SetCNDetail(item,ref listBillDetail,settleType,cnType,out message);
          }
          
         
          #endregion
      }
      /// <summary>
      /// 设置明细信息
      /// </summary>
      /// <param name="bill"></param>
      /// <param name="listBillDetail"></param>
      /// <param name="cnType"></param>
      private void SetCNDetail(Bill bill, ref List<BillDetail> listBillDetail, SS_SettleType settleType, EnumType.EnumCNType cnType, out string message)
      {
          message = "";
          var list = this.ListBillDetail.FindAll(e=>e.GUID_Main==bill.GUID);
          if (list != null)
          {
              //创建出纳明细
              foreach (BillDetail item1 in list)
              {
                  var item = new BillDetail();
                  item.Memo = item1.Memo;
                  item.CopyCommField(item1);
                  //获取现金存储的财政支付令
                  //var paymentNumber = this.BusinessContext.CN_PaymentNumberView.FirstOrDefault(e=>e.GUID==item.GUID_PaymentNumber);
                  item.IsDC = cnType == EnumType.EnumCNType.出纳收款单;
                  item.DocTypeKey =((int)cnType).ToString();
                  //生成CN_Detail 对象
                  //item.IsDC = cnType == null ? false : true;
                  #region 结算方式
                  if (item.DocTypeKey == "21")//提现单 GUID_SettleType需要从数据库中查询
                  {
                      if (cnType == EnumType.EnumCNType.出纳付款单)
                      {
                         
                      }
                      else if (cnType == EnumType.EnumCNType.出纳收款单)
                      {
                          if (settleType != null)
                          {
                              item.GUID_SettleType = settleType.GUID;
                              item.SettleTypeName = settleType.SettleTypeName;
                              item.SettleTypeKey = settleType.SettleTypeKey;
                          }
                          else
                          {
                              message = "无法找到结算方式，无法转换!";
                              return;
                          }
                      }
                  }
                  else//存现单
                  {
                      if (cnType == EnumType.EnumCNType.出纳付款单)
                      {
                          if (settleType != null)
                          {
                              item.SettleTypeName = settleType.SettleTypeName;
                              item.SettleTypeKey = settleType.SettleTypeKey;
                              item.GUID_SettleType = settleType.GUID;
                          }
                          else
                          {
                              message = "无法找到结算方式，无法转换!";
                              return;
                          }
                      }
                  }
                  #endregion
                  //支票号
                  var checkDrawDeail = this.dbBusinessEntity.CN_CheckDrawDetailView.FirstOrDefault(e => e.GUID_DocDetail == item.GUID);
                  if (checkDrawDeail != null)
                  {
                      var checkDrawMian = this.dbBusinessEntity.CN_CheckDrawMainView.FirstOrDefault(e => e.GUID == checkDrawDeail.GUID_CheckDrawMain);
                      if (checkDrawMian != null)
                      {
                          item.CheckNumber = checkDrawMian.CheckNumber;
                      }
                  }
                  item.ClassId = 36;
                  item.ClassMainId = 35;
                  listBillDetail.Add(item);
              }

          }
      }

      #endregion

      #region 收款凭单转换收入单据或者往来单据
      public void SkDocTransferDoc(out List<Bill> listBill, out List<BillDetail> listBillDetail, out string message)
      {
          message = string.Empty;
          listBillDetail = new List<BillDetail>();
          listBill = new List<Bill>();
          if (this.ListBill == null || this.ListBill.Count == 0)
          {
              message = "收款单据无数据！";
              return;
          }
          
          if (this.ListBillDetail == null || this.ListBillDetail.Count == 0)
          {
              message = "收款单据明细信息无数据！";
              return;
          }
          int classId=0, classMainId=0;
          foreach (Bill item in this.ListBill)
          {
              if (item.SRWLTypeClassID == 34)//收入类型
              {
                  var doctype = this.dbBaseEntity.SS_DocTypeView.FirstOrDefault(e => e.DocTypeKey == "14");
                  var uiType = this.dbBaseEntity.SS_UIType.FirstOrDefault(e => e.UITypeKey == "0301");
                  if (doctype != null)
                  {
                      classId = 33;
                      classMainId = 32;
                      item.YWTypeKey = doctype.YWTypeKey;
                      item.GUID_YWType = doctype.GUID_YWType==null?Guid.Empty:(Guid)doctype.GUID_YWType;
                      item.DocTypeKey=doctype.DocTypeKey;
                      item.ClassId = 32;
                      item.DocTypeName = "收入单";
                      item.GUID_DocType=doctype.GUID;
                  
                  }
                  if(uiType!=null)
                  {
                   // item
                  }
              }
              else if (item.SRWLTypeClassID == 20)//往来类型
              {
                  classId = 31;
                  classMainId = 30;
                  var doctype = this.dbBaseEntity.SS_DocTypeView.FirstOrDefault(e => e.DocTypeKey == "18");
                  var uiType = this.dbBaseEntity.SS_UIType.FirstOrDefault(e => e.UITypeKey == "050101");
                if (doctype != null)
                  {
                      item.YWTypeKey = doctype.YWTypeKey;
                      item.GUID_YWType = doctype.GUID_YWType==null?Guid.Empty:(Guid)doctype.GUID_YWType;
                      item.DocTypeKey=doctype.DocTypeKey;
                      item.ClassId = 30;
                      item.DocTypeName = "应付单";
                      item.GUID_DocType=doctype.GUID;
                }
                if(uiType!=null)
                {
                // item
                }
              }
              listBill.Add(item);
          }
          foreach (var item in ListBillDetail)
          {
              item.ClassId = classId;
              item.ClassMainId = classMainId;
              item.IsDC = true;
              item.Guid_SRType = item.GUID_SRWLType;
              item.IsCustomer = true;
              listBillDetail.Add(item);
          }
      }
      #endregion
    }

      public class OAOObject
      {

          private int _clsId = 0;

          public int ClassId
          {
              get { return _clsId; }
          }
          private Dictionary<string, string> Values = new Dictionary<string, string>();

          protected OAOObject(Dictionary<string, string> Values, int classid)
          {
              this._clsId = classid;

              this.Values = Values;
          }

          public string GetValue(string AttributeName)
          {
              AttributeName = AttributeName.ToLower();
              return Values.ContainsKey(AttributeName) ? Values[AttributeName] : string.Empty;
          }

          public static OAOObject Create<T>(T obj, int classid)
          {

              Dictionary<string, string> Values = new Dictionary<string, string>();
              Type mType = typeof(T);
              PropertyInfo[] infos = mType.GetProperties();
              for (int i = 0; i < infos.Length; i++)
              {
                  string proname = infos[i].Name.ToLower();
                  if (Values.ContainsKey(proname))
                  {
                      Values[proname] = infos[i].GetValue(obj);

                  }
                  else
                  {
                      Values.Add(proname, infos[i].GetValue(obj));
                  }
              }

              return new OAOObject(Values, classid);
          }

          public static List<OAOObject> Create<T>(List<T> objs, int classid)
          {
              List<OAOObject> results = new List<OAOObject>();
              foreach (T obj in objs)
              {
                  OAOObject item = OAOObject.Create<T>(obj, classid);
                  if (item != null) results.Add(item);
              }
              return results;
          }
      }

      public class DocTransferPz
      {
         
          public static List<OAOObject> GetRelations(ObjectContext context,Guid RightId, EnumType.EnumClass ClassId)
          {
              switch (ClassId)
              {
                  case EnumType.EnumClass.SS_BXDetail:
                      List<BX_DetailView> bxdetails= context.CreateObjectSet<BX_DetailView>().Where(e => e.GUID_BX_Main == RightId).ToList();
                      return OAOObject.Create<BX_DetailView>(bxdetails, (int)ClassId);
                  case EnumType.EnumClass.PaymentNumber:
                      List<CN_PaymentNumberView> pays = context.CreateObjectSet<CN_PaymentNumberView>().Where(e => e.GUID == RightId).ToList();
                      return OAOObject.Create<CN_PaymentNumberView>(pays, (int)ClassId);
                  case EnumType.EnumClass.BX_Travel:
                      List<BX_TravelView> travels = context.CreateObjectSet<BX_TravelView>().Where(e => e.GUID_BX_Main == RightId).ToList();
                      return OAOObject.Create<BX_TravelView>(travels, (int)ClassId);
                  case EnumType.EnumClass.BX_InvitFee:
                      List<BX_InviteFeeView> Fees = context.CreateObjectSet<BX_InviteFeeView>().Where(e => e.GUID_BX_Main == RightId).ToList();
                      return OAOObject.Create<BX_InviteFeeView>(Fees, (int)ClassId);
                  case EnumType.EnumClass.BX_TravelAllowance:
                      List<BX_TravelAllowanceView> TravelAllowances = context.CreateObjectSet<BX_TravelAllowanceView>().Where(e => e.GUID_BX_Main == RightId).ToList();
                      return OAOObject.Create<BX_TravelAllowanceView>(TravelAllowances, (int)ClassId);
                  case EnumType.EnumClass.CN_CheckDrawDetail:
                      List<CN_CheckDrawDetailView> checkdrawdetails = context.CreateObjectSet<CN_CheckDrawDetailView>().Where(e => e.GUID_DocDetail == RightId).ToList();
                      return OAOObject.Create<CN_CheckDrawDetailView>(checkdrawdetails, (int)ClassId);
                  case EnumType.EnumClass.HX_Detail:
                      List<HX_Detail> hxdetails = context.CreateObjectSet<HX_Detail>().Where(e => e.GUID_Detail == RightId).ToList();
                      return OAOObject.Create<HX_Detail>(hxdetails, (int)ClassId);
                  default:
                      return new List<OAOObject>();
                  
              }
                  
          }

          public static List<OAOObject> GetCheckDrawMains(ObjectContext context, List<CN_CheckDrawDetailView> checkdrawdetails)
          {
              List<Guid> guids=new List<Guid>();
              foreach (CN_CheckDrawDetailView item in checkdrawdetails)
              {
                  if (!guids.Contains((Guid)item.GUID_CheckDrawMain))
                  {
                      guids.Add((Guid)item.GUID_CheckDrawMain);
                  }
              }
              List<CN_CheckDrawMainView> cdms = context.CreateObjectSet<CN_CheckDrawMainView>().Where(e => guids.Contains(e.GUID)).ToList();
              return OAOObject.Create<CN_CheckDrawMainView>(cdms, 42);
          }

          public static List<OAOObject> GetCheckDrawMains(ObjectContext context, List<OAOObject> checkdrawdetails)
          {
              List<Guid> guids = new List<Guid>();
              foreach (OAOObject item in checkdrawdetails)
              {
                  Guid iguid = Guid.Parse(item.GetValue("GUID_CheckDrawMain"));
                  if (!guids.Contains(iguid))
                  {
                      guids.Add(iguid);
                  }
              }
              List<CN_CheckDrawMainView> cdms = context.CreateObjectSet<CN_CheckDrawMainView>().Where(e => guids.Contains(e.GUID)).ToList();
              return OAOObject.Create<CN_CheckDrawMainView>(cdms, 42);
          }

          public static List<OAOObject> GetHxMains(ObjectContext context, List<HX_Detail> hxdetails)
          {
              List<Guid> guids = new List<Guid>();
              foreach (HX_Detail item in hxdetails)
              {
                  if (!guids.Contains(item.GUID_HX_Main))
                  {
                      guids.Add(item.GUID_HX_Main);
                  }
              }
              List<HX_MainView> cdms = context.CreateObjectSet<HX_MainView>().Where(e => guids.Contains(e.GUID)).ToList();
              return OAOObject.Create<HX_MainView>(cdms, (int)EnumType.EnumClass.HX_Main);
          }

          public static List<OAOObject> GetHxMains(ObjectContext context, List<OAOObject> hxdetails)
          {
              List<Guid> guids = new List<Guid>();
              foreach (OAOObject item in hxdetails)
              {
                  Guid iguid = Guid.Parse(item.GetValue("GUID_HX_Main"));
                  if (!guids.Contains(iguid))
                  {
                      guids.Add(iguid);
                  }
              }
              List<HX_MainView> cdms = context.CreateObjectSet<HX_MainView>().Where(e => guids.Contains(e.GUID)).ToList();
              return OAOObject.Create<HX_MainView>(cdms, (int)EnumType.EnumClass.HX_Main);
          }
          /// <summary>
          /// 获取关联单据集合
          /// </summary>
          /// <param name="context">上下文</param>
          /// <param name="DocMain">主单</param>
          /// <param name="DocDetail">明细单</param>
          /// <param name="hxdetails">未保存的核销明细</param>
          /// <param name="hxmain">未保存的核销单</param>
          /// <returns></returns>
          public static List<OAOObject> GetDocRelationDocs(ObjectContext context, Bill DocMain, BillDetail DocDetail, List<HX_Detail> hxdetails, Bill hxmain)
          {
              CN_PaymentNumberView pay = null; List<OAOObject> temps = new List<OAOObject>();
              List<OAOObject> results = new List<OAOObject>();
              switch ((EnumType.EnumClass)DocDetail.ClassId)
              {
                  case EnumType.EnumClass.SS_BXDetail: /**报销单明细**/
                      BX_DetailView bxdetail = context.CreateObjectSet<BX_DetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      results.Add(OAOObject.Create<BX_DetailView>(bxdetail,DocDetail.ClassId));
                      results.AddRange(DocTransferPz.GetRelations(context, DocMain.GUID, EnumType.EnumClass.BX_Travel)); //差旅
                      results.AddRange(DocTransferPz.GetRelations(context, DocMain.GUID, EnumType.EnumClass.BX_InvitFee)); //劳务费
                      results.AddRange(DocTransferPz.GetRelations(context, DocMain.GUID, EnumType.EnumClass.BX_TravelAllowance)); //出差补助明细
                      pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == bxdetail.GUID_PaymentNumber);
                      results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令
                      temps=DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context,temps)); //核销单
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.CN_CheckDrawDetail); //支票领取明细
                      results.AddRange(temps); //支票领取明细
                      results.AddRange(DocTransferPz.GetCheckDrawMains(context,temps)); //支票领取单
                      if (DocMain.ClassId == (int)EnumType.EnumClass.BX_CollectMain) //如果是公务卡汇总卡
                      {
                          BX_CollectMainView main = context.CreateObjectSet<BX_CollectMainView>().FirstOrDefault(e => e.GUID == DocMain.GUID);
                          BX_MainView bxmain = context.CreateObjectSet<BX_MainView>().FirstOrDefault(e => e.GUID == bxdetail.GUID_BX_Main);
                          results.Add(OAOObject.Create<BX_CollectMainView>(main, (int)EnumType.EnumClass.BX_CollectMain));
                          results.Add(OAOObject.Create<BX_MainView>(bxmain, (int)EnumType.EnumClass.SS_BXMain));
                      }
                      else
                      {
                          BX_MainView bxmain = context.CreateObjectSet<BX_MainView>().FirstOrDefault(e => e.GUID == bxdetail.GUID_BX_Main);
                          results.Add(OAOObject.Create<BX_MainView>(bxmain, (int)EnumType.EnumClass.SS_BXMain));
                      }
                      break;
                  case EnumType.EnumClass.JJ_Detail: /**基金明细**/
                      JJ_DetailView jjdetail = context.CreateObjectSet<JJ_DetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == jjdetail.GUID_PaymentNumber);
                      results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context, temps)); //核销单
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.CN_CheckDrawDetail); //支票领取明细
                      results.AddRange(temps); //支票领取明细
                      results.AddRange(DocTransferPz.GetCheckDrawMains(context, temps)); //支票领取单
                      JJ_MainView jjmain = context.CreateObjectSet<JJ_MainView>().FirstOrDefault(e => e.GUID == jjdetail.GUID_JJ_Main);
                      results.Add(OAOObject.Create<JJ_MainView>(jjmain, (int)EnumType.EnumClass.JJ_Main));
                      break;
                  case EnumType.EnumClass.WL_Detail: /**往来明细**/
                      WL_DetailView wldetail = context.CreateObjectSet<WL_DetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == wldetail.GUID_PaymentNumber);
                      results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context, temps)); //核销单
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.CN_CheckDrawDetail); //支票领取明细
                      results.AddRange(temps); //支票领取明细
                      results.AddRange(DocTransferPz.GetCheckDrawMains(context, temps)); //支票领取单
                      WL_MainView wlmain = context.CreateObjectSet<WL_MainView>().FirstOrDefault(e => e.GUID == wldetail.GUID_WL_Main);
                      results.Add(OAOObject.Create<WL_MainView>(wlmain, (int)EnumType.EnumClass.WL_Main));
                      break;
                  case EnumType.EnumClass.SR_Detail: /**收入明细**/
                      SR_DetailView srdetail = context.CreateObjectSet<SR_DetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == srdetail.GUID_PaymentNumber);
                      results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context, temps)); //核销单
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.CN_CheckDrawDetail); //支票领取明细
                      results.AddRange(temps); //支票领取明细
                      results.AddRange(DocTransferPz.GetCheckDrawMains(context, temps)); //支票领取单
                      SR_MainView srmain = context.CreateObjectSet<SR_MainView>().FirstOrDefault(e => e.GUID == srdetail.GUID_SR_Main);
                      results.Add(OAOObject.Create<SR_MainView>(srmain, (int)EnumType.EnumClass.SR_Main));
                      break;
                  case EnumType.EnumClass.CN_CashDetail: /**提现单明细**/
                      CN_CashDetailView cashdetail = context.CreateObjectSet<CN_CashDetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == cashdetail.GUID_PaymentNumber);
                      results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context, temps)); //核销单
                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.CN_CheckDrawDetail); //支票领取明细
                      results.AddRange(temps); //支票领取明细
                      results.AddRange(DocTransferPz.GetCheckDrawMains(context, temps)); //支票领取单
                      CN_CashMainView cashmain = context.CreateObjectSet<CN_CashMainView>().FirstOrDefault(e => e.GUID == cashdetail.GUID_CN_CashMain);
                      results.Add(OAOObject.Create<CN_CashMainView>(cashmain, (int)EnumType.EnumClass.CN_CashMain));
                      break;
                  case EnumType.EnumClass.CN_Detail: /**出纳单明细**/
                      CN_DetailView cndetail = context.CreateObjectSet<CN_DetailView>().FirstOrDefault(e => e.GUID == DocDetail.GUID);
                      if(cndetail!=null){
                        pay = context.CreateObjectSet<CN_PaymentNumberView>().FirstOrDefault(e => e.GUID == cndetail.GUID_PaymentNumber);
                        results.Add(OAOObject.Create<CN_PaymentNumberView>(pay, (int)EnumType.EnumClass.PaymentNumber)); //财政支付令                      }

                      temps = DocTransferPz.GetRelations(context, DocDetail.GUID, EnumType.EnumClass.HX_Detail); //核销明细
                      results.AddRange(temps); //核销明细
                      results.AddRange(DocTransferPz.GetHxMains(context, temps)); //核销单
                      if (cndetail != null)
                      {
                          CN_MainView cnmain = context.CreateObjectSet<CN_MainView>().FirstOrDefault(e => e.GUID == cndetail.GUID_CN_Main);
                          results.Add(OAOObject.Create<CN_MainView>(cnmain, (int)EnumType.EnumClass.CN_Main));
                      }
                      break;
              }
              //获得当前没有保存的核销单和核销明细
              if (hxdetails != null && hxdetails.Count > 0)
              {
                  results.AddRange(OAOObject.Create<HX_Detail>(hxdetails, (int)EnumType.EnumClass.HX_Detail));
              }
              if (hxmain != null)
              {
                  results.Add(OAOObject.Create<Bill>(hxmain, (int)EnumType.EnumClass.HX_Main));
              }
              return results;
          }
      }
}
