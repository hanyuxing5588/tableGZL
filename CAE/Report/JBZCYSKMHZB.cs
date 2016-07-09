using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Business.Common;
using Business.CommonModule;
using Infrastructure;
using System.IO;

namespace CAE.Report
{   
    //基本支出预算科目汇总表		
    public class JBZCYSKMHZB : BaseReport
    {
       
        public string DepName { get; set; }//报表导出用
        public string Date { get; set; }//报表导出用
        public string Unit { get; set; }//报表导出用
        public JBZCYSKMHZB(string key)
            : base(key)
        {

        }
        public override void Init()
        {   
            //0 部门编号 1预算 2 年度
            this.SqlFormat = " select bgcode.guid,bgcode.pguid, bgcode.BGCodeKey,bgcode.BGCodeName,item1.totalbg1 from ss_bgcode bgcode left join(select guid_BGcode,sum(total_BG) as totalbg1 from bg_detailview where guid_item in(select guid from bg_item where bgitemkey='02') " +
            "and guid_bg_main in(select guid from bg_mainview where isnull(invalid,0)=1 and GUID_Department in" +
            "({0}) and bgtypekey='01' and bgstepkey in ({1})) and bgyear='{2}' group by guid_bgcode) item1 on bgcode.guid=item1.guid_bgcode"+
            " where bgcode.BGCodeKey in(select DISTINCT BGCodeKey from BG_SetupBGCodeView WHERE  BGSetupKey IN ({1}))" +/*根据预算步骤一样 可以从这里过滤*/
            " order by BGCodeKey";
            this.tempalte = Path.Combine(this.tempalte, "jbzcyskmhzb.xls");
        }
        
        /// <summary>
        /// 获取拼接的Sql
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string GetSql(BBSearchCondition conditionModel)
        {

            string strsql = this.SqlFormat;
            string departId = string.Empty;
            string bgSetpId = string.Empty;
            if (conditionModel.Year == "0")
            {
                conditionModel.Year = "";
            }
            if (!string.IsNullOrEmpty(conditionModel.treeModel) && (conditionModel.treeValue != null && conditionModel.treeValue != Guid.Empty))
            {
                switch (conditionModel.treeModel.ToLower())
                {
                    case "ss_department":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        departId = GetByDepartmentId(conditionModel.treeValue.ToString());
                        break;
                    case "bg_setup":
                        if (conditionModel.treeValue == null)
                        {
                            conditionModel.treeValue = Guid.Empty;
                        }
                        bgSetpId = GetStepKey(conditionModel.treeValue.ToString());
                        break;
                }
            }
            //树节点条件

            if (conditionModel.TreeNodeList != null && conditionModel.TreeNodeList.Count > 0)
            {
                foreach (TreeNode item in conditionModel.TreeNodeList)
                {
                    switch (item.treeModel.ToLower())
                    {
                        case "ss_department":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            departId = GetByDepartmentId(item.treeValue);
                            break;
                        case "bg_setup":
                            if (item.treeValue == null)
                            {
                                item.treeValue = Guid.Empty.ToString();
                            }
                            bgSetpId = GetStepKey(item.treeValue);
                            break;
                    }
                }
            }
            if (string.IsNullOrEmpty(departId))
            {
                departId = GetByDepartmentId(Guid.Empty.ToString());
            }
            if (string.IsNullOrEmpty(bgSetpId))
            {
                bgSetpId = GetStepKey(Guid.Empty.ToString());
            }
            return strsql = string.Format(strsql, departId, bgSetpId, conditionModel.Year);
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        public DataTable GetReport(SearchCondition conditions, out string msgError)
        {
            msgError = "";
            string sqlStr = string.Empty;
            BBSearchCondition conditionModel = (BBSearchCondition)conditions;
            sqlStr = GetSql(conditionModel);
            DataTable orgdt = CreateDataTable(sqlStr, ref msgError);
            DataTable newdt = CreateNewDataTable();
            ReSetData(orgdt, ref newdt, conditionModel.RMBUnit);
            return newdt;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="orgdt"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        public void ReSetData(DataTable orgdt, ref DataTable newdt, string rmbUnit)
        {
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }
            double totalbg = 0.00;
            double dhj = 0F;
            string pguid = string.Empty;
            DataRow[] pdr = orgdt.Select(" pguid is null");
            foreach (DataRow row in pdr)
            {
                pguid = row["guid"]==DBNull.Value?"":row["guid"].ToString();
                DataRow newRow = newdt.NewRow();
                var bk = row["bgcodekey"].ToString();
                newRow["bgcodekey"] =bk;
                newRow["BGCodeName"] = row["BGCodeName"].ToString();

                var sumTotalbg = orgdt.Compute("Sum(totalbg1)", "pguid='" + pguid + "'");
                sumTotalbg = sumTotalbg == DBNull.Value ? 0F : sumTotalbg;              
               
                totalbg = orgdt == null ? 0F : double.Parse(sumTotalbg.ToString());
                newRow["totalbg1"] = totalbg == 0F ?"": FormatMoney(1, totalbg / unit);
                newdt.Rows.Add(newRow);

                 DataRow[] childDr = orgdt.Select(" pguid='" + pguid + "'", " bgcodekey asc "); ;
                 if (childDr.Length > 0)
                 {
                     ReSetChildData(childDr,ref newdt,rmbUnit);
                 }
                 dhj += totalbg;
            }

            DataRow hjRow = newdt.NewRow();
            hjRow["bgcodekey"] = "合计";
            hjRow["BGCodeName"] = "";
            hjRow["totalbg1"] = FormatMoney(1, dhj / unit);
            newdt.Rows.Add(hjRow);
        }
        /// <summary>
        /// 设置子项数据
        /// </summary>
        /// <param name="drs"></param>
        /// <param name="newdt"></param>
        /// <param name="rmbUnit"></param>
        public void ReSetChildData(DataRow[] drs, ref DataTable newdt, string rmbUnit)
        {
            int unit = 1;
            if (int.TryParse(rmbUnit, out unit) == false)
            {
                unit = 1;
            }                 
            foreach (DataRow row in drs)
            {
                DataRow newRow = newdt.NewRow();
                var bk = row["bgcodekey"].ToString();
                newRow["bgcodekey"] = this.colChar+bk;
                newRow["BGCodeName"] = row["BGCodeName"].ToString();
                double d = 0;
                newRow["totalbg1"] = double.TryParse((row["totalbg1"] + "").ToString(), out d) ? FormatMoney(1, d / unit) : "";              
                newdt.Rows.Add(newRow);
            }
        }
        /// <summary>
        /// 创建临时新表
        /// </summary>
        /// <returns></returns>
        private DataTable CreateNewDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("BGCodeKey");//科目编号
            dt.Columns.Add("BGCodeName");//科目编号
            dt.Columns.Add("totalbg1");//科目编号
            return dt;
        }
        /// 加载数据
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="msgError"></param>
        /// <returns></returns>
        private DataTable CreateDataTable(string sql, ref string msgerror)
        {
            var dt = DataSource.ExecuteQuery(sql);
            if (dt == null || dt.Rows.Count == 0)
            {
                msgerror = "服务器连接失败。";
                return null;
            }
            return dt;
        }
        //导出报表
        public override string GetExportPath(DataTable data, out string fileName, out string message)
        {
            fileName = "";
            message = "";
            try
            {
                if (data != null && data.Rows.Count <= 0)
                {
                    message = "1";
                    return "";
                }
                string filePath = ExportExcel.Export(data, this.tempalte, 5, 0, new List<ExcelCell>() { new ExcelCell(1, 1, this.DepName), new ExcelCell(1, 2, this.Date), new ExcelCell(1, 3, this.Unit) });
                fileName = Path.GetFileName(filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return "";
            }

        }
    }
}
