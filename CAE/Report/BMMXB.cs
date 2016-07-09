using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace CAE.Report
{
  public  class BMMXB:BaseReport
    {
      public int Year { get; set; }
      public int EMonth { get; set; }
      public int SMonth { get; set; }
      public Guid PlanGUID { get; set; }
      public string DepartmentGUID { get; set; }
      public string sqlAction = @"SELECT  *
FROM    dbo.SA_PlanActionDetailView
WHERE   GUID_PlanAction IN ( SELECT GUID
                             FROM   SA_PlanAction
                             WHERE  ActionYear = {0} AND GUID_Plan='{3}'
                                    AND ActionMouth >= {1} AND ActionMouth<={2} and ActionState=1 )
         AND DepartmentKey in('{4}') ORDER BY DepartmentKey,personkey,ItemOrderNum";
      public string sqlHead = @" 
         SELECT a.*
         FROM   ( SELECT DISTINCT
                            GUID_Item ,
                            ItemKey ,
                            ItemName
                  FROM      dbo.SA_PlanActionDetailView
                  WHERE     GUID_PlanAction IN (
                            SELECT  GUID
                            FROM    SA_PlanAction
                            WHERE   ActionYear = {0}
                                    AND GUID_Plan = '{3}'
                                    AND ActionMouth >= {1} AND ActionMouth<={2}  and ActionState=1 )
                ) a
                LEFT JOIN ( SELECT  GUID_Item ,
                                    ItemKey ,
                                    ItemName,
                                    ItemOrder
                            FROM    dbo.SA_PlanItemView
                            WHERE   GUID_Plan = '{3}'
                                    AND IsStop <> 1
                        
                          ) b ON a.GUID_Item = b.GUID_Item
                          
                ORDER BY  b.ItemOrder";
      public BMMXB(int year,int smonth, int emonth,Guid planGUID,string departmentGUID) 
      {
          Year = year;
          EMonth = emonth;
          SMonth = smonth;
          PlanGUID = planGUID;
          DepartmentGUID = departmentGUID;
      }
      public BMMXB() { }
      public Dictionary<Guid, string> DicItemKey = new Dictionary<Guid, string>();
      public DataTable dtHead = null;
      public DataTable dtData = null;
      public string Msg { get; set; }
      public bool GetHead() 
      {
            try 
	        {
                dtHead = DataSource.ExecuteQuery(string.Format(sqlHead, Year, SMonth,EMonth, PlanGUID));
                if (dtHead == null) { Msg = "查询工资项数据失败。"; return false; }
                dtData = DataSource.ExecuteQuery(string.Format(sqlAction, Year, SMonth,EMonth, PlanGUID, DepartmentGUID.Replace(",","','")));

                if (dtData == null) { 
                    Msg = "查询工资数据失败。"; 
                    return false;
                }
                return true;
	        }
	        catch (Exception ex)
	        {
                  Msg = ex.Message.ToString();
		         return false;
	        }
      }
      public DataTable CreateResultTable() 
      {
          DataTable dt = new DataTable();
          dt.Columns.Add(new DataColumn("DepartmentName"));
          dt.Columns.Add(new DataColumn("PersonName"));
          //,DepartmentName
          foreach (var item in DicItemKey.Keys)
	      {
              var v = DicItemKey[item];
              dt.Columns.Add(new DataColumn("T" +v));
          }
          return dt;
      }
      public DataTable GetReport() 
      {
          if (!GetHead()) {
              return null;
          }
          foreach (DataRow item in dtHead.Rows)
          {
              Guid itemGuid ;
              if(Guid.TryParse(item["GUID_Item"]+"",out itemGuid)){
                  DicItemKey.Add(itemGuid, item["ItemKey"]+"");
              }
          }
          Dictionary<Guid,PersonItem> dicRow = new Dictionary<Guid,PersonItem>();
          foreach (DataRow dr in dtData.Rows)
          {
              Guid personGUID=Guid.Empty;
              if(!Guid.TryParse(dr["GUID_Person"]+"",out personGUID)){
                 continue;
              }
               Guid GUID_Item=Guid.Empty;
               if (!Guid.TryParse(dr["GUID_Item"] + "", out GUID_Item))
               {
                 continue;
              }
              var itemkey=dr["ItemKey"]+"";
              var value=dr["ItemValue"]+"";
              var department=dr["DepartmentName"]+"";
              var personName=dr["PersonName"]+"";
              if (dicRow.ContainsKey(personGUID))
              {
                  var ent = dicRow[personGUID];
                  if (ent.dicItem.ContainsKey(itemkey))
                  {
                      var v1 = ent.dicItem[itemkey];
                      var v2 = value;
                      double vv1 = 0;
                      double vv2 = 0;
                      double.TryParse(v1, out vv1);
                      double.TryParse(v2, out vv2);


                      ent.dicItem[itemkey] = (vv1 + vv2).ToString();
                  }
                  else
                  {
                      ent.dicItem.Add(itemkey, value);
                  }
              }
              else {
                   var ent = new PersonItem();
                   ent.Department=department;
                   ent.PersonName=personName;
                   ent.dicItem.Add(itemkey,value);
                   dicRow.Add(personGUID, ent);
              }
          }
          var ResultTable= this.CreateResultTable();
          foreach (var item in dicRow.Keys)
          {
              try
              {

             
              var ent = dicRow[item];
              var drnew = ResultTable.NewRow();
              drnew["DepartmentName"] = ent.Department;
              drnew["PersonName"] = ent.PersonName;
              var dic = ent.dicItem;
              foreach (var key in DicItemKey.Keys)
              {
                  var itemKey = DicItemKey[key];
                  drnew["T" + itemKey] = ent.dicItem[itemKey];
              }
              ResultTable.Rows.Add(drnew);
              }
              catch (Exception ex)
              {
              }
          }
          return ResultTable;
      }

      public string GetColums()
      {
          string strLink = "";
          foreach (DataRow item in dtHead.Rows)
          {
              string strColum = "{width:100,field:'T" + item["ItemKey"] + "" + "',title:'" + item["ItemName"] + "" + "', halign:'center',align: 'right',formatter:function(v){return formatNum(v)},styler: function(value,row,index){return 'color:blue';}}";
              if (strLink == "")
              {
                  strLink = strColum;
              }
              else
              {
                  strLink = strLink + "," + strColum;
              }
          }
          strLink = "{field:'DepartmentName',title:'部门',width:100,halign:'center',align: 'left'},{field:'PersonName',title:'姓名',width:100,halign:'center',align: 'left'}," + strLink;
          strLink = "[[" + strLink + "]]";
          return strLink;
      }

      //导出报表
      public override string GetExportPath(DataTable data, out string fileName, out string message)
      {
          foreach (DataRow ddd in data.Rows)
          {
              for (int i = 0; i < data.Columns.Count; i++)
              {
                  var d=0.0;
                  var v=ddd[i] + "";
                  if (string.IsNullOrEmpty(v)) {
                      ddd[i] = "0.00";
                  }
                  if (double.TryParse(v,out d)) {
                      ddd[i] = d.ToString("N");
                  }
                  
              }
          }
          var dtHead1 = DataSource.ExecuteQuery(string.Format(sqlHead, Year, SMonth, EMonth, PlanGUID));
          var row = data.NewRow();
          row[0] = "部门";
          row[1] = "姓名";
          for (int i = 0; i < dtHead1.Rows.Count; i++)
          {
              row[i+2] = dtHead1.Rows[i]["ItemName"]+"";
          }
          data.Rows.InsertAt(row, 0);
          var tempalte1 = Path.Combine(this.tempalte, "bmgzDetal.xls");   
          fileName = "";
          message = "";
          try
          {
              if (data != null && data.Rows.Count <= 0)
              {
                  message = "1";
                  return "";
              }
              string filePath = ExportExcel.Export(data, tempalte1, 1, 0);
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
public class PersonItem 
{
    public PersonItem() {
        dicItem = new Dictionary<string, string>();
    }
    public string Department { get; set; }
    public string PersonName { get; set; }
    public Dictionary<string, string> dicItem { get; set; }
}
