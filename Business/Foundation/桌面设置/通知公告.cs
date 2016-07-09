using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using OAModel;

namespace Business.Foundation.桌面设置
{
    public class 通知公告 : BaseDocument
    {   
        public 通知公告() : base() { }
        public 通知公告(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            OA_Notice notic = new OA_Notice();
            notic.Fill(jsonModel.m);
            notic.GUID = Guid.NewGuid();
            //OrderNum 是否是新增，新增的情况必须增加
            //var OrderNum = 0;
            //累加OrderNum
            var OrderNum = this.OAContext.OA_Notice.Max(e => e.OrderNum);
            notic.OrderNum = OrderNum + 1;
            this.OAContext.OA_Notice.AddObject(notic);

            //循环遍历 去OA_FileTemp attIds 附件
            //添加通知公告的附件数据
            //先取到临时表中的数据，在根据前台传过来的GUID去临时表中查找，然后循环，在将临时表的数据传给附件表
            //OA_FileTemp fileTemp = new OA_FileTemp();
            OA_FileTempModel fileTempModel = new OA_FileTempModel();
            fileTempModel.ClassFill(jsonModel.m);
            var fileTempGUID = fileTempModel.GUID_OA_FileTemp;
            //前台返回来可能有多个GUID，如果有多个，则进行分割
            string[] arrGuid;
            //如果有多个GUID，则加上分隔符,否则直接赋值
            if (fileTempGUID.IndexOf(',') > 0)
            {
                arrGuid = fileTempGUID.Split(',');
            }
            else
            {
                arrGuid = new string[] { fileTempGUID};
            }
            //将得到的GUID放在集合中，并转换类型
            List<Guid> GuidList = new List<Guid>();
            Guid g;
            for (int i = 0; i < arrGuid.Length; i++)
            { 
                if(!string.IsNullOrEmpty(arrGuid[i]))
                {
                    if (Guid.TryParse(arrGuid[i], out g))
                    {
                        GuidList.Add(g);
                    }
                }
            }
           
            //根据集合中查出来的GUID，在去临时表中匹配对应的数据 ,返回一个List
            var fileTempList = this.OAContext.OA_FileTemp.Where(e => GuidList.Contains(e.GUID));
            foreach (OA_FileTemp item in fileTempList)
            {
                OA_NoticeAnnex oan = new OA_NoticeAnnex();
                oan.GUID = item.GUID;   //附件GUID
                oan.GUID_Notice = notic.GUID;   //通告GUID
                oan.Annex = item.FileBody;
                oan.AnnexName = item.FileName;
                this.OAContext.OA_NoticeAnnex.AddObject(oan);
            }

            this.OAContext.SaveChanges();
            return notic.GUID;
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
            OA_Notice oaNotice = new OA_Notice();
            JsonAttributeModel id = jsonModel.m.IdAttribute(oaNotice.ModelName());
            if (id == null) return Guid.Empty;
            Guid g;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            oaNotice = this.OAContext.OA_Notice.FirstOrDefault(e => e.GUID == g);
            oaNotice.Fill(jsonModel.m);

            OA_FileTempModel fileTempModel = new OA_FileTempModel();
            fileTempModel.ClassFill(jsonModel.m);
            var fileTempGUID = fileTempModel.GUID_OA_FileTemp;


            var listAttGuid = CommonFuntion.ChangeStrArrToGuidList(fileTempGUID.Split(',').ToArray());
            var oanList = this.OAContext.OA_NoticeAnnex.Where(e => e.GUID_Notice == g).ToList();
            
            foreach (OA_NoticeAnnex item in oanList)
            {
                if (listAttGuid.Contains(item.GUID)) {
                    listAttGuid.Remove(item.GUID);
                    continue;
                }
                this.OAContext.OA_NoticeAnnex.DeleteObject(item);
            }
            //新增
            var fileTempList = this.OAContext.OA_FileTemp.Where(e => listAttGuid.Contains(e.GUID));
            foreach (OA_FileTemp item in fileTempList)
            {
                OA_NoticeAnnex oan = new OA_NoticeAnnex();
                oan.GUID = item.GUID;   //附件GUID
                oan.GUID_Notice = oaNotice.GUID;   //通告GUID
                oan.Annex = item.FileBody;
                oan.AnnexName = item.FileName;
                this.OAContext.OA_NoticeAnnex.AddObject(oan);
            }

            this.OAContext.ModifyConfirm(oaNotice);
            this.OAContext.SaveChanges();
            return oaNotice.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            
            if (guid != null)
            {
                //创建一个附件集合
                List<OA_NoticeAnnex> oanList;
                oanList = this.OAContext.OA_NoticeAnnex.Where(e => e.GUID_Notice == guid).ToList();
                //将附件表中通知公告对应的附件一一删除
                foreach (OA_NoticeAnnex item in oanList)
                {
                    OAContext.DeleteConfirm(item);
                }
            }
            //在将主表公告里的GUID 删除
            OA_Notice role = this.OAContext.OA_Notice.FirstOrDefault(e => e.GUID == guid);
            OAContext.DeleteConfirm(role);

            OAContext.SaveChanges();
        }
        /// <summary>
        /// 初始化时返回数据列表
        /// 返回一个空的格式数据
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override JsonModel Retrieve(Guid guid)
        {
            JsonModel jmodel = new JsonModel();
            return jmodel;
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
                Guid value = jsonModel.m.Id(new OA_Notice().ModelName());
                string strMsg = string.Empty;
                switch (status)
                {
                    case "1": //新建 
                       // strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Insert(jsonModel);
                        }
                        break;
                    case "2": //修改
                       // strMsg = DataVerify(jsonModel, status);
                        if (string.IsNullOrEmpty(strMsg))
                        {
                            value = this.Modify(jsonModel);
                        }
                        break;
                    case "3": //删除
                        //strMsg = DataVerify(jsonModel, status);
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
                        result = this.Retrieve(value);
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
        


    }

    public class OA_FileTempModel
    {
        public string GUID_OA_FileTemp { set; get; }
    }

}
