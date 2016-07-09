using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;

namespace Business.Foundation.科目设置
{
    public class 预算科目设置 : BaseDocument
    {
        public 预算科目设置() : base() { }
        public 预算科目设置(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }
        /// <summary>
        /// 初始化时返回数据列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<BGCodeSetUpModel> Retrieve()
        {
            var details = this.InfrastructureContext.SS_BGCode.OrderBy(e => e.BGCodeKey).AsEnumerable();
            return details.Select(bg => new BGCodeSetUpModel
             {
                 GUID = bg.GUID,
                 PGUID = bg.PGUID,
                 BGCodeName = bg.BGCodeName,
                 GUID_BG_SetupBGCode = null, //目前没有用到，删除的时候可能用到，BG_SetupBGCode表中的GUID
                 RateNum = null,
                 _parentId = bg.PGUID
             }).ToList();
        }
        /// <summary>
        /// 按条件查询
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<BGCodeSetUpModel> Retrievekm(Guid guid)
        {
            var bglist = this.InfrastructureContext.BG_SetupBGCode.Where(e => e.GUID_BGSetup == guid);
            var bgGUIDLIst = bglist.Select(e => e.GUID_BGCode).ToList();
            var bgCodeList = (from a in this.InfrastructureContext.SS_BGCode
                              join b in bglist on a.GUID equals b.GUID_BGCode into temp
                              from b in temp.DefaultIfEmpty()
                              orderby a.BGCodeKey
                              select new BGCodeSetUpModel
                              {
                                  GUID = a.GUID,
                                  PGUID = a.PGUID,
                                  BGCodeName = a.BGCodeName,
                                  GUID_BG_SetupBGCode = b.GUID,
                                  RateNum = b.RateNum*10000,
                                  _parentId = a.PGUID,
                                  @checked = bgGUIDLIst.Contains(a.GUID) ? true : false
                              }).ToList();

            return bgCodeList;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ssSetupId">科目设置的GUID</param>
        /// <param name="listAuthsetModel">模型</param>
        /// <returns></returns>
        public bool SaveJC(string ssSetupId, List<BGCodeSetUpModel> listAuthsetModel)
        {
            try
            {
                //先删除在添加
                var IContext = DBFactory.GetCurInfrastructureDbContext();
                Guid gId;
                Guid.TryParse(ssSetupId, out gId);
                this.DeleteJCMenuSetData(IContext, gId);
                foreach (var item in listAuthsetModel)
                {
                    BG_SetupBGCode model = new BG_SetupBGCode();
                    model.GUID_BGCode = item.GUID;
                    model.GUID_BGSetup = gId;
                    model.RateNum =item.RateNum==0?0: item.RateNum / 10000;
                    AddDataJCMenuSet(IContext, model, gId);
                }
                IContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="context">公共</param>
        /// <param name="authModel">模型</param>
        /// <param name="userOrRole">科目设置GUID</param>
        private void AddDataJCMenuSet(BaseConfigEdmxEntities context, BG_SetupBGCode authModel, Guid bgSetupId)
        {
            var ent = context.BG_SetupBGCode.CreateObject();
            ent.GUID = Guid.NewGuid();
            ent.GUID_BGSetup = bgSetupId;
            ent.GUID_BGCode = authModel.GUID_BGCode;
            ent.RateNum = authModel.RateNum;
            context.BG_SetupBGCode.AddObject(ent);
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="context">公共</param>
        /// <param name="bgSetupGuid">科目设置GUID</param>
        private void DeleteJCMenuSetData(BaseConfigEdmxEntities context, Guid bgSetupGuid)
        {
            var datas = context.BG_SetupBGCode.Where(e => e.GUID_BGSetup == bgSetupGuid).ToList();
            foreach (var item in datas)
            {
                context.BG_SetupBGCode.DeleteObject(item);
            }
        }
    }
    //重新定义类型 传输 
    public class BGCodeSetUpModel
    {
        public Guid GUID { set; get; }
        public Guid? PGUID { set; get; }
        public string BGCodeName { set; get; }
        public Guid? GUID_BG_SetupBGCode { set; get; }
        public double? RateNum { set; get; }
        public Guid? _parentId { get; set; }
        public bool @checked { get; set; }

    }
}
