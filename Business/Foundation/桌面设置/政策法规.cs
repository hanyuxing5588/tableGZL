using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Common;
using Infrastructure;
using OAModel;
namespace Business.Foundation.桌面设置
{
    public class 政策法规 : BaseDocument
    {
        public 政策法规() : base() { }
        public 政策法规(Guid OperatorId, string ModelUrl) : base(OperatorId, ModelUrl) { }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="jsonModel">JsonModel名称</param>
        /// <returns>GUID</returns>
        protected override Guid Insert(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return Guid.Empty;
            SS_OfficeFileType sso = new SS_OfficeFileType();
            sso.Fill(jsonModel.m);
            if (sso.GUID == Guid.Empty) {
                sso = this.OAContext.SS_OfficeFileType.FirstOrDefault(e => e.FileTypeKey == sso.FileTypeKey);
            }
            //JsonAttributeModel id = jsonModel.m.IdAttribute(sso.ModelName());
            //if (id == null)
            //{
            //    sso.GUID = Guid.NewGuid();
            //}
            //this.OAContext.SS_OfficeFileType.AddObject(sso);

            //循环遍历OA_FileTemp  attIds
            //先添加政策法规的附件数据
            //先取到临时表中的数据，在根据前台传过来的GUID去临时表中查找，然后循环遍历，在讲GUID所对应的附件数据添加到真表中
            OA_FileTempModel fileTempModel = new OA_FileTempModel();
            fileTempModel.ClassFill(jsonModel.m);
            var fileTempGUID = fileTempModel.GUID_OA_FileTemp;
            //前台返回来的可能有多个GUID，如果有多个，则进行分割，放在一个数组里。
            string[] arrGuid;
            //如果有多个GUID,则加上分隔符，否则直接赋值
            if (fileTempGUID.IndexOf('.') > 0)
            {
                arrGuid = fileTempGUID.Split(',');
            }
            else
            {
                arrGuid = new string[] { fileTempGUID };
            }
            //讲得到的GUID放在集合中，并转换类型
            List<Guid> GuidList = new List<Guid>();
            Guid g;
            for (int i = 0; i < arrGuid.Length; i++)
            {
                if (!string.IsNullOrEmpty(arrGuid[i]))
                {
                    if (Guid.TryParse(arrGuid[i], out g))
                    {
                        GuidList.Add(g);
                    }
                }
            }
            //根据集合中查出来的GUID，在去临时表中查找对应的匹配的数据，返回一个List
            var fileTempList = this.OAContext.OA_FileTemp.Where(e => GuidList.Contains(e.GUID));
            OA_OfficeFile oao = new OA_OfficeFile();
            oao.Fill(jsonModel.m);
            //int orderNum = 0;
            foreach (OA_FileTemp item in fileTempList)
            {
                //orderNum++;
                OA_OfficeFile tempoao = this.OAContext.OA_OfficeFile.CreateObject();
                tempoao.GUID = item.GUID;
                tempoao.GUID_OfficeFileType = sso.GUID;
                tempoao.FileName = item.FileName;
                tempoao.FileBody = item.FileBody;

                tempoao.FileKey = oao.FileKey;
                tempoao.Visible = oao.Visible;
                tempoao.OrderNum = oao.OrderNum;

                this.OAContext.OA_OfficeFile.AddObject(tempoao);
                //将数据添加到真表后就直接清空临时表(根据你添加的GUID去清空，而不是将数据全部清空)
                this.OAContext.DeleteConfirm(item);
            }
            this.OAContext.SaveChanges();
            return sso.GUID;



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
            SS_OfficeFileType officelFileType = new SS_OfficeFileType();
            JsonAttributeModel id = jsonModel.m.IdAttribute(officelFileType.ModelName());
            JsonAttributeModel officeFileId = jsonModel.m.IdAttribute(new OA_OfficeFile().ModelName());
            if (id == null) return Guid.Empty;
            Guid g,fileGuid;
            if (Guid.TryParse(id.v, out g) == false) return Guid.Empty;
            if (Guid.TryParse(officeFileId.v, out fileGuid) == false) return Guid.Empty;
            officelFileType = this.OAContext.SS_OfficeFileType.FirstOrDefault(e => e.GUID == g);
            officelFileType.Fill(jsonModel.m);

            var fileTempModel = new OA_FileTempModel();
            fileTempModel.ClassFill(jsonModel.m);
            var fileTempGUID = fileTempModel.GUID_OA_FileTemp;
            Guid attId=Guid.Empty;
         
            if (!Guid.TryParse(fileTempGUID, out attId))
            {//Id不是guid 删除原来的
                var oaoFile = this.OAContext.OA_OfficeFile.FirstOrDefault(e => e.GUID == fileGuid);
                if (oaoFile != null)
                {
                    this.OAContext.OA_OfficeFile.DeleteObject(oaoFile);
                }
                this.OAContext.ModifyConfirm(officelFileType);
            }
            else {
                var oaoFile = this.OAContext.OA_OfficeFile.FirstOrDefault(e => e.GUID == fileGuid);
                if (oaoFile != null&&attId != oaoFile.GUID)
                {
                    this.OAContext.OA_OfficeFile.DeleteObject(oaoFile);
                }

                if (oaoFile==null||attId != oaoFile.GUID)
                {
                    var item = this.OAContext.OA_FileTemp.FirstOrDefault(e => e.GUID == attId);
                    OA_OfficeFile tempoao = new OA_OfficeFile();
                    tempoao.GUID = item.GUID;
                    tempoao.GUID_OfficeFileType = officelFileType.GUID;
                    tempoao.FileName = item.FileName;
                    tempoao.FileBody = item.FileBody;
                    this.OAContext.OA_OfficeFile.AddObject(tempoao);
                    this.OAContext.OA_FileTemp.DeleteObject(item);
                }
            }
            this.OAContext.SaveChanges();
            return officelFileType.GUID;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid">GUID</param>
        protected override void Delete(Guid guid)
        {
            if (guid != null)
            {
                OA_OfficeFile oaoList;
                oaoList = this.OAContext.OA_OfficeFile.FirstOrDefault(e => e.GUID == guid);
                //将附件中政策法规所对应的附件一一删除
                OAContext.DeleteConfirm(oaoList);
            }
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
                Guid value = jsonModel.m.Id(new SS_OfficeFileType().ModelName());
                Guid oaValue = jsonModel.m.Id(new OA_OfficeFile().ModelName());//返回临时表中附件的GUID
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
                            this.Delete(oaValue);
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

        #region 政策法规字段验证

        /// <summary>
        /// 数据验证
        /// </summary>
        /// <param name="jsonModel">JsonModel</param>
        /// <param name="status">状态</param>
        /// <returns>string</returns>
        /// 
        public string DataVerify(JsonModel jsonModel, string status)
        {
            string strMsg = string.Empty;
            VerifyResult vResult = null;
            SS_OfficeFileType main = null;
            OA_OfficeFile oaMain = null;
            switch (status)
            {
                case "1":
                    main = LoadMain(jsonModel);
                    var file = LoaOfficeFile(jsonModel);
                    vResult = InsertVerify(main, file);
                    strMsg = DataVerifyMessage(vResult);
                    break;
                case "2":
                    break;
                case "3":
                    Guid value = jsonModel.m.Id(new SS_OfficeFileType().ModelName());
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
        /// 数据从数据库删除前验证
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        protected override VerifyResult DeleteVerify(Guid guid)
        {
            //验证结果
            VerifyResult result = new VerifyResult();
            string str = string.Empty;
            //验证信息
            List<ValidationResult> resultList = new List<ValidationResult>();
            //得到往来单位本身的GUID在往来Detail中是否有数据
            var list = this.OAContext.SS_OfficeFileType.Where(e => e.GUID == guid).ToList();
            if (list.Count > 0)
            {
                str = "该政策法规不能删除！";
                resultList.Add(new ValidationResult("", str));
                result._validation = resultList;
            }
            return result;
        }

        /// <summary>
        /// 主表验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<ValidationResult> VerifyResultMain(SS_OfficeFileType data, OA_OfficeFile oaData)
        {
            string str = string.Empty;
            List<ValidationResult> resultList = new List<ValidationResult>();
            //List<OA_OfficeFile> con = this.OAContext.OA_OfficeFile.ToList();
            //返回一条指定的数据
            //OrderNum
            OA_OfficeFile oaOrd = this.OAContext.OA_OfficeFile.FirstOrDefault(e => e.OrderNum == oaData.OrderNum);
            //FileKey
            OA_OfficeFile oakey = this.OAContext.OA_OfficeFile.FirstOrDefault(e => e.FileKey == oaData.FileKey);
            SS_OfficeFileType mModel = data;
            OA_OfficeFile oaModel = oaData;
            object g;

            #region   主表字段验证

            //主表字段验证
            if (mModel.FileTypeKey == "")
            {
                str = "类型编号字段为必输项！";
                resultList.Add(new ValidationResult("", str));

            }
            if (mModel.FileTypeName == "")
            {
                str = "类型名称字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            //附件表字段验证
            if (oaModel.FileName == "")
            {
                str = "文件编号字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            if (oaModel.FileKey == "")
            {
                str = "文件名称字段为必输项！";
                resultList.Add(new ValidationResult("", str));
            }
            //if (oakey != null)
            //{
            //    if (oaModel.FileKey == oakey.FileKey)
            //    {
            //        str = "文件编号不能重复！";
            //        resultList.Add(new ValidationResult("", str));
            //    }
            //}
            //if (oaOrd != null)
            //{
            //    if (oaModel.OrderNum == oaOrd.OrderNum)
            //    {
            //        str = "顺序号不能重复！";
            //        resultList.Add(new ValidationResult("", str));
            //    }
            //}
            
            return resultList;

            #endregion
        }

        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private SS_OfficeFileType LoadMain(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            SS_OfficeFileType main = new SS_OfficeFileType();
            main.Fill(jsonModel.m);
            return main;
        }

        /// <summary>
        /// 加载主Model信息
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        private OA_OfficeFile LoaOfficeFile(JsonModel jsonModel)
        {
            if (jsonModel.m == null) return null;
            OA_OfficeFile main = new OA_OfficeFile();
            main.Fill(jsonModel.m);
            return main;
        }
        /// <summary>
        /// 数据插入数据库前验证
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected VerifyResult InsertVerify(SS_OfficeFileType data, OA_OfficeFile oaData)
        {
            VerifyResult result = new VerifyResult();
            //主Model验证
            var vf_main = VerifyResultMain(data, oaData);
            if (vf_main != null && vf_main.Count > 0)
            {
                result._validation.AddRange(vf_main);
            }
            return result;

        }



        #endregion

    }
}
