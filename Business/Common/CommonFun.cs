using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OAModel;
using Infrastructure;
namespace Business.Common
{
    public static class CommonFun
    {
        public static bool WriteCommonFun(string commonFunGuid, Guid operatorId)
        {
            Guid commonFunGuid1 = Guid.Empty;
            if (!Guid.TryParse(commonFunGuid, out commonFunGuid1)) return false;
            if (operatorId == Guid.Empty) return false;
            try
            {


                using (var context = new BaseConfigEdmxEntities())
                {
                    var ent = context.SS_CommonFunction.FirstOrDefault(e => e.GUID_Auth == commonFunGuid1 && e.GUID_Operator == operatorId);
                    if (ent != null)
                    {
                        ent.OperationTimes = ent.OperationTimes + 1;
                    }
                    else
                    {
                        var a = context.SS_CommonFunction.CreateObject();
                        a.GUID = Guid.NewGuid();
                        a.GUID_Auth = commonFunGuid1;
                        a.GUID_Operator = operatorId;
                        a.OperationTimes = 1;
                        context.SS_CommonFunction.AddObject(a);
                    }
                    context.SaveChanges();

                }
            }
            catch (Exception ex)
            {
            }
            return true;

        }
        public static string ChangeGuid(List<Guid> listGuid)
        {
            string strTemp = "({0})", strFormat = "'{0}',", strDeal = "";
            foreach (var item in listGuid)
            {
                strDeal += string.Format(strFormat, item);
            }
            return strTemp = string.Format(strTemp, strDeal.TrimEnd(','));
        }
        public static void RetrieveLeafModelIds(BaseConfigEdmxEntities context, string tableName, Guid parentId, ref List<Guid> result)
        {
            var children = RetrieveLeafChildModelIds(context, parentId, tableName);
            if (children.Count > 0)
            {
                foreach (var item in children)
                {
                    RetrieveLeafModelIds(context, tableName, item, ref result);
                }
            }
            else
            {
                result.Add(parentId);
            }
        }
        public static List<Guid> RetrieveLeafChildModelIds(BaseConfigEdmxEntities context, Guid parentId, string tablename)
        {
            var sql = string.Format(" select  Guid from {0} where Pguid='{1}'", tablename, parentId);
            return context.ExecuteStoreQuery<Guid>(sql).ToList();
        }

        public static void RetrieveLeafModelIds(BaseConfigEdmxEntities context, string tableName, Guid parentId, string whereField, ref List<Guid> result)
        {
            var children = RetrieveLeafChildModelIds(context, parentId, tableName, whereField);
            if (children.Count > 0)
            {
                foreach (var item in children)
                {
                    RetrieveLeafModelIds(context, tableName, item, whereField, ref result);
                }
            }
            else
            {
                result.Add(parentId);
            }
        }
        public static List<Guid> RetrieveLeafChildModelIds(BaseConfigEdmxEntities context, Guid parentId, string tablename, string whereField)
        {
            var sql = string.Format(" select  Guid from {0} where {1}='{2}'", tablename, whereField, parentId);
            return context.ExecuteStoreQuery<Guid>(sql).ToList();
        }

        public static List<OfficeFileType> GetFileType(OAEntities context = null)
        {
            if (context == null)
            {
                context = new OAEntities();
            }
            List<OfficeFileType> list = new List<OfficeFileType>();
            try
            {
                list = context.SS_OfficeFileType.Select(e => new OfficeFileType
                {
                    GUID = e.GUID,
                    PGUID = e.PGUID,
                    FileTypeKey = e.FileTypeKey,
                    FileTypeName = e.FileTypeName
                }).OrderBy(e => e.FileTypeKey).ToList();
            }
            catch { }
            return list;
        }

        public static string GetU8MatchDataBase(BusinessModel.BusinessEdmxEntities context, string AccountKey, string Year)
        {
            var tempSql = string.Format("select * from ufsystem..UA_AccountDatabase where cAcc_Id='{0}' and (iBeginYear<={1} or iBeginYear is null) and (iEndYear>={1} or iEndYear is null) order by ibeginyear desc", AccountKey
                ,Year);
            var result = context.ExecuteStoreQuery<UA_AccountDatabaseModel>(tempSql);
            var item = result.FirstOrDefault();
            return item==null ? string.Empty : item.cDatabase;
        }

        public static bool IsDataBaseExsist(BusinessModel.BusinessEdmxEntities context, string DataBaseName)
        {
            var tempSql = string.Format("select * From master.dbo.sysdatabases where name='{0}'", DataBaseName);
            var result = context.ExecuteStoreQuery<UA_AccountDatabaseModel>(tempSql);
            var item = result.FirstOrDefault();
            return item == null ? false : true;
        }
    }

    public class UA_AccountDatabaseModel
    {
        //GUID,FiscalYear,CWPeriod,DocNum,GUID_Maker,GUID_PZType,PZTypeName,DocDate,Maker,ExteriorDataBase,a.Ino_id as ino_id,MakeDate,YWTypeKey
        public string cDatabase { get; set; }
    }
}
