using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Data;

namespace CAE.Report
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string Msg { get; set; }

    }
    public static class ToolResult
    {
        public static Result GetSuccess(string msg)
        {
            return new Result() { IsSuccess = true, Msg = msg };
        }
        public static Result GetFailure(string msg)
        {
            return new Result() { IsSuccess = false, Msg = msg };
        }
    }
    public class CZYSZXQK : BaseReport
    {
        public string msg { get; set; }
        public DateTime EndDate { get; set;}
        public DateTime StartDate { get; set; }

        public formulaCZYSZXQK Create1(int endYear)
        {
           
            string basepath = System.AppDomain.CurrentDomain.BaseDirectory;
            string path = basepath.EndsWith("\\") ? basepath + "bin\\Common\\CZYSZXQKFormula" + endYear + ".xml" : basepath + "\\bin\\Common\\CZYSZXQKFormula" + endYear + ".xml";
            XmlSerializer xs = new XmlSerializer(typeof(formulaCZYSZXQK));

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            var mFormula = xs.Deserialize(fs) as formulaCZYSZXQK;
            fs.Close();
            return mFormula;
        }
        public DataTable GetDataTable() {
            var sql =string.Format(@"SELECT [Id]
      ,[CodeKey]
      ,[CodeName]
      ,[SNJZ]
      ,[BNYS]
      ,[HJ]
      ,[LJZC]
      ,[ZCBL]
      ,[JSJF]
      ,[BZ]
      ,[JZZC]
      ,[Year]
      ,[Iorder]
      ,[IsEdit]
      ,[Level] FROM dbo.RP_CZYSZXQK WHERE Year='{0}' order by CodeKey",EndDate.Year);
            var dt = DataSource.ExecuteQuery(sql);
            return dt;

        }

        public DataTable GetExprotDataTable() 
        {
            var sql = string.Format(@"SELECT CodeName
      ,[SNJZ]
      ,[BNYS]
      ,[HJ]
      ,[LJZC]
      ,[ZCBL]
      ,[JSJF]
      ,[BZ]
      ,[JZZC]
      FROM dbo.RP_CZYSZXQK WHERE Year='{0}' order by CodeKey", EndDate.Year);
            var dt = DataSource.ExecuteQuery(sql);
            foreach (DataRow item in dt.Rows)
            {
                item["SNJZ"] =string.IsNullOrEmpty(item["SNJZ"] + "")?"": ConventDoubleRStr(item["SNJZ"] + "").ToString("0.00");
                item["BNYS"] = string.IsNullOrEmpty(item["BNYS"] + "") ? "" : ConventDoubleRStr(item["BNYS"] + "").ToString("0.00");
                item["HJ"] = string.IsNullOrEmpty(item["HJ"] + "") ? "" : ConventDoubleRStr(item["HJ"] + "").ToString("0.00");
                item["JZZC"] = string.IsNullOrEmpty(item["JZZC"] + "") ? "" : ConventDoubleRStr(item["JZZC"] + "").ToString("0.00");
            }
            return dt;
        }
        public DataTable GetReport() 
        {
            try
            {

            
            var dtData = GetDataTable();
            var dic = GetDataByTemplate();
            foreach (DataRow dr in dtData.Rows)
            {
                ////SNJZ+BNYS=HJ  //LJZC/HJ=ZCBL JSJF= HJ-LJZC
                var key = dr["CodeKey"]+"";
                RowInfo r=new RowInfo();
                if(dic.TryGetValue(key,out r)){
                    if (r.level <= 4)
                    {
                        dr["LJZC"] = r.value;
                    }

                }
                //   row1["HJ"] = rNumFun(row1["SNJZ"]) + rNumFun(row1["BNYS"]);
                //var c = rNumFun(row1["SNJZ"]) + rNumFun(row1["BNYS"]) - rNumFun(row1["LJZC"]);
                //row1["JSJF"] = c < 0 ? 0.001 : c;
                //if (c < 0)
                //{
                //    row1["LJZC"] = rNumFun(row1["HJ"]);
                //}
                var dHJ=ConventDoubleRStr(dr["HJ"]+"");
                dr["CodeName"] = GetKG(r.level, dr["CodeName"]+"") + dr["CodeName"] + "";
                dr["Level"] = r.level;
                var c= (dHJ == 0 ? 0 : ConventDoubleRStr(r.value + "") / dHJ);
                dr["ZCBL"] =c>1?"100%":c.ToString("0.00%");
                if (c > 1) {
                    dr["LJZC"] = dHJ.ToString("0.00");
                }
                dr["JSJF"] = c > 1?"0.00":(dHJ - ConventDoubleRStr(r.value + "")).ToString("0.00");
                dr["IsEdit"] = r.isEdit=="1";
            }
            return dtData;
            }
            catch (Exception ex)
            {
                msg = ex.Message.ToString();
                return null;
            }
        }
        public string GetKG(int level,string codeName) 
        {
            var s = "";
            for (int i = 0; i <3- level; i++)
            {
                s += "&nbsp;&nbsp;&nbsp;&nbsp;";
            }
            if (codeName == "其中：收支两条线") {
                return "&nbsp;&nbsp;";
            }
            return s;
        }

        //t通过模版转换数据
        public Dictionary<string, RowInfo> GetDataByTemplate()
        {
            var czyzxqkTemplate = this.Create1(EndDate.Year);
            var dic = new Dictionary<string, RowInfo>();
            var hjCount = 0.0;
            foreach (var item in czyzxqkTemplate.sqlf.cols)
            {
                var rowInfo = new RowInfo();
                rowInfo.level = item.level;
                rowInfo.isEdit = item.isEdit;
                if (rowInfo.level == 4)
                {
                    rowInfo.value =( hjCount/10000).ToString();
                    //if (EndDate.Year == 2015) { 
                        
                    //}
                }
                else
                {
                    var dData = DataSource.ExecuteQuery(item.value.Replace("@SDate", StartDate.ToShortDateString()).Replace("@EDate", EndDate.ToShortDateString()));

                    if (dData != null)
                    {
                        var temp = 0.0;
                        double.TryParse(dData.Rows[0][0] + "", out temp);
                        if (rowInfo.level == 3)
                        {
                            hjCount += temp;
                        }
                        rowInfo.value = (temp/10000).ToString(); ;
                    }
                    else
                    {
                        rowInfo.value = "";
                    }
                }
                dic.Add(item.key, rowInfo);
            }
            return dic;
        }

        public Result Save(IList<CZYSZXQKModel> ents)
        {
            var sqlInsert = @"
          INSERT INTO [dbo].[RP_CZYSZXQK]
           ([CodeKey]
           ,[CodeName]
           ,[SNJZ]
           ,[BNYS]
           ,[HJ]
           ,[LJZC]
           ,[ZCBL]
           ,[JSJF]
           ,[BZ]
           ,[JZZC]
           ,[Year]
           ,[isEdit]
           ,[level])
     VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}',{12})
            ";
            List<string> listSql = new List<string> { "delete from RP_CZYSZXQK where Year='" + EndDate.Year + "'" };
            foreach (var ent in ents)
            {
                try
                {
                    var sql = string.Format(sqlInsert, ent.CodeKey, ent.CodeName.Replace("&nbsp;",""), ent.SNJZ, ent.BNYS, ent.HJ,ConventDoubleRStr( ent.LJZC).ToString("0.00"), ent.ZCBL, ent.JSJF, ent.BZ, ent.JZZC, EndDate.Year,
                        ent.IsEdit=="1"||ent.IsEdit.ToLower()=="true"?1:0,
                        ent.Level);
                    listSql.Add(sql);
                }
                catch (Exception ex)
                {

                }

            }
            if (listSql.Count == 1)
            {
                ToolResult.GetFailure("没有要保存的数据！");
            }
            return Save(listSql);

        }
        public  Result Save(List<string> ilistSql)
        {
            try
            {

                DataSource.ExecuteNonQueryLst(ilistSql);
            }
            catch (Exception ex)
            {
                return ToolResult.GetFailure("保存失败,错误信息:" + ex.Message);
            }
            return ToolResult.GetSuccess("保存成功");
        }

        public  string GetExportPath(DataTable dt, int startRowindex, out string fileName)
        {
            var template = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.Configuration.ConfigurationManager.AppSettings["TemplatePath"], "CZYSZXQK"+EndDate.Year+".xls");
            fileName = "";
            string filePath = ExportExcel.Export(dt, template, startRowindex, 1, new List<ExcelCell>() { new ExcelCell(){
                Col=3,
                Row=1,
                Value=EndDate.ToLongDateString()
            }});
            fileName = Path.GetFileName(filePath);
            return filePath;
        }
    }

    public class CZYSZXQKModel 
    {
        public string IsEdit { get; set; }
        public string Level { get; set; }
        public string CodeKey { get; set; }
        public string CodeName { get; set; }
        public string LJZC { get; set; }
        public string ZCBL { get; set; }
        public string JSJF { get; set; }
        public string BZ { get; set; }
        public string JZZC { get; set; }
        public string SNJZ { get; set; }
        public string BNYS { get; set; }
        public string HJ { get; set; }
    }

     [Serializable]
    [XmlRoot("formula")]
    public class formulaCZYSZXQK
     {
         [XmlElement("sqlformula")]
         public sqlformulaCol sqlf { get; set; }
     }
     public class sqlformulaCol
     {
         [XmlElement("ccol")]
         public List<RowInfo> cols { get; set; }
     }
     public class RowInfo 
     {
         [XmlAttribute]
         public string key { get; set; }
         [XmlAttribute]
         public string descript { get; set; }
         [XmlAttribute]
         public string value { get; set; }
         [XmlAttribute]
         public int level { get; set; }
         [XmlAttribute]
         public string isEdit { get; set; }
         public string conn { get; set; }
     }
}
