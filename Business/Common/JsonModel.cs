using System.Collections.Generic;
using System.Data.Entity;
using System.Runtime.Serialization;
using System;
using System.Text;
using System.Web.Script.Serialization;

namespace Business.Common
{

    /// <summary>
    /// JsonModel类设计的常量
    /// </summary>
    public static class JsonModelConstant
    {
        public const string Success = "success";
        public const string Error = "error";
        public const string Info = "info";
        public const string Question = "question";
        public const string Waring = "warning";
    }

    /// <summary>
    /// 数据属性


    /// </summary>
    public class JsonAttributeModel
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string n { get; set; }
        /// <summary>
        /// 属性值


        /// </summary>
        public string v { get; set; }
        /// <summary>
        /// 模型名


        /// </summary>
        public string m { get; set; }

        public JsonAttributeModel() { }
        public JsonAttributeModel(string name, string value,string model)
        {
            this.n = name;
            this.v = value;
            this.m = model;
        }
    }
    /// <summary>
    /// 列表
    /// </summary>
    public class JsonGridModel
    {
        /// <summary>
        /// 列表主模型名称


        /// </summary>
        public string m { get; set; }
        /// <summary>
        /// 列表行数据集合


        /// </summary>
        public List<List<JsonAttributeModel>> r { get; set; }

        public JsonGridModel() { r = new List<List<JsonAttributeModel>>(); }
        public JsonGridModel(string model) : base() { m = model; r = new List<List<JsonAttributeModel>>(); }
    }
    /// <summary>
    /// Json模型
    /// </summary>
    public class JsonModel
    {
        public string result { get; set; }
        /// <summary>
        /// 主Model信息
        /// </summary>
        public List<JsonAttributeModel> m { get; set; }
        /// <summary>
        /// 明显Model信息
        /// </summary>
        public List<JsonGridModel> d { get; set; }
        /// <summary>
        /// 明细Model的默认信息

        /// </summary>
        public List<JsonGridModel> f { get; set; }
        /// <summary>
        /// 消息信息
        /// </summary>
        public JsonMessage s { get; set; }
        public JsonModel()
        {
            m = new List<JsonAttributeModel>();
            d = new List<JsonGridModel>();
            f = new List<JsonGridModel>();
            result = JsonModelConstant.Success;
        }
    }
    /// <summary>
    /// 消息
    /// </summary>
    public class JsonMessage
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string t { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string m { get; set; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public string i { get; set; }


        public JsonMessage() { }
        public JsonMessage(string title, string msg, string icon)
        {
            this.t = title;
            this.m = msg;
            this.i = icon;
        }
    }
    /// <summary>
    /// 返回信息
    /// </summary>
    public class ResponseJsonMessage
    {
        /// <summary>
        /// 结果类型
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// 消息信息
        /// </summary>
        public JsonMessage s { get;set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object data { get;set; }

        public Guid Id { get; set; }
    }
    public static class JsonModelExtension
    {

        public static string GetValue(this List<JsonAttributeModel> objs, string modelName, string attributeName)
        {
            JsonAttributeModel result = objs.Find(e => e.n != null && e.m != null && e.m.ToLower() == modelName.ToLower()
                && e.n.ToLower() == attributeName.ToLower());
            return result == null ? string.Empty : result.v;
        }

        public static JsonAttributeModel IdAttribute(this List<JsonAttributeModel> objs,string model)
        {
            return objs.Find(e => e.n != null && e.m != null && e.n.ToLower() == "guid" && e.m.ToLower() == model.ToLower());           
            //return objs.Find(e => e.n.ToLower() == "guid" && e.m.ToLower()==model.ToLower());
        }

        public static Guid Id(this List<JsonAttributeModel> objs, string model)
        {
            JsonAttributeModel idat = objs.IdAttribute(model);
            if (idat==null) return Guid.Empty;
            Guid result = Guid.Empty;
            Guid.TryParse(idat.v, out result);
            return result;
        }
        //根据属性获取属性值
        public static object GetValueByAttribute(this List<JsonAttributeModel> obj,string attributeName)
        {
            JsonAttributeModel model=obj.Find(e => e.n + "".ToLower() == attributeName+"".ToLower());
            if (model == null) return null;
            return model.v;
        }
        //根据属性属性赋值
        public static void SetValueByAttribute(this List<JsonAttributeModel> obj, string attributeName,string value)
        {
            JsonAttributeModel model = obj.Find(e => e.n + "".ToLower() == attributeName + "".ToLower());
            if (model == null) return ;
            model.v = value;
        }
        public static bool Exists(this List<JsonAttributeModel> objs, Guid value,string model)
        {
            JsonAttributeModel item = objs.IdAttribute(model);
            if (item == null)
            {
                return value == Guid.Empty;
            }


            string invalue = value == Guid.Empty ? "" : value.ToString();
            return item.v.ToLower() == invalue.ToLower() ? true : false;
        }
        public static bool ExistsByModel(this List<JsonAttributeModel> objs, string model)
        {
           var  obj=objs.Find(e=>e.m+""==model+"");
           return obj == null ? false : true;
        }
        public static JsonGridModel Find(this List<JsonGridModel> objs, string model)
        {
            foreach (JsonGridModel obj in objs)
            {
                if (obj!=null && obj.m.ToLower() == model.ToLower())
                {
                    return obj;
                }
            }
            return null;
            //return objs.Find(e => (e.m+"").ToLower() == model.ToLower());
        }
        public static List<List<JsonAttributeModel>> FindByModelName(this List<List<JsonAttributeModel>> objs,string model)
        {
            return objs.FindAll(e => e.ExistsByModel(model)); ;
        }
        public static List<JsonAttributeModel> Find(this List<List<JsonAttributeModel>> objs, Guid value,string model)
        {
            foreach (List<JsonAttributeModel> obj in objs)
            {
                if (obj!=null && obj.Exists(value, model)) return obj;
            }

            return null;
        }
        /// <summary>
        /// 行中的不同属性Model的筛选
        /// </summary>
        /// <param name="objs"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static List<JsonAttributeModel> Find(this List<JsonAttributeModel> objs,string model)
        {
            return objs.FindAll(e =>e.m+"".ToLower()==model+"".ToLower());
        }
            
        public static List<List<JsonAttributeModel>> FindNew(this List<List<JsonAttributeModel>> objs,string model)
        {
            List<List<JsonAttributeModel>> result = new List<List<JsonAttributeModel>>();
            foreach (List<JsonAttributeModel> obj in objs)
            {

                if (obj!=null && obj.Exists(Guid.Empty, model))
                {
                    result.Add(obj); 
                }
            }
            return result;
        }
        /// <summary>
        /// 获取主模型名
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string GetModelName(this List<JsonAttributeModel> objs)
        {
            string str=string.Empty;
            if (objs!= null && objs.Count>0)
            {
                str = objs[0].m;
            }
            return str;
        }
        /// <summary>
        ///扩展 JsonModel转换成Json
        /// </summary>
        /// <param name="jsonModel"></param>
        /// <returns></returns>
        public static string JsonModelToString(this JsonModel jsonModel)
        {
            return JsonHelp.ObjectToJson(jsonModel);
        }
        /// <summary>
        ///扩展 Json转换成JsonModel
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static JsonModel JsonToJsonModel(this string json)
        {
            return JsonHelp.JsonToObject<JsonModel>(json);
        }
        
    }

    public static class JsonHelp
    {
        /// <summary>
        ///对象反序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Serialize(obj);
        }
        /// <summary>
        /// Jons转换成实体



        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            return jss.Deserialize<T>(json);
        }
    }
}
