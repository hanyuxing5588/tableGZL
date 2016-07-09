using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using BusinessModel;
namespace Business.Common
{
    internal abstract class Log
    {
        public Guid OperatorId { get; set; }
        public string Data { get; set; }
        public Guid DocId { get; set;}
        public string DocTypeName { get; set; }
        public bool IsSuccess { get; set; }
        public string Action { get; set; }
        public string Remark { get; set; }
        public void Init(Guid operatorId, Guid docId, string status, string docTypename, string data,string remark, bool isSuccess = true) 
        {
            OperatorId = operatorId;
            Data = data;
            DocId = docId;
            IsSuccess = isSuccess;
            DocTypeName = docTypename;
            Action = status == "1" ? "新增" : status == "2" ? "修改" : "删除";
            Remark = remark;
        }
        public abstract void WriteLog();

    }
    internal class DBLog : Log
    {
        public override void WriteLog()
        {
            using (var context = new BusinessEdmxEntities()) {
                var ent = context.CreateObject<OperLog>();
                ent.Guid = Guid.NewGuid();
                ent.OperatorGuid = this.OperatorId;
                ent.OperatorData = Data;
                ent.DocGuid = this.DocId;
                ent.DocTypeName = this.DocTypeName;
                ent.Action = this.Action;
                ent.IsSuccess = this.IsSuccess;
                ent.OperatorDate = DateTime.Now;
                ent.Remark = this.Remark;
                context.OperLogs.AddObject(ent);
                context.SaveChanges();
            }
        }
    }
    internal class FileLog : Log 
    {
        public string logPath { get; set; }
        public override void WriteLog()
        {
            try
            {
                logPath = System.Configuration.ConfigurationManager.AppSettings["logPath"];

                //首先拿到根日志存放的根目录
                string rootPath = HttpContext.Current.Server.MapPath("~/");
                string filePath = rootPath + logPath + "\\" + DateTime.Now.Year.ToString() + "年" + DateTime.Now.Month.ToString() + "月";
                string fileName = "操作记录.txt";
                string filePathAll = filePath + "\\" + fileName;
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (!File.Exists(filePathAll))
                {
                    FileStream filestream = new FileStream(filePathAll, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    StreamWriter writer = new StreamWriter(filestream, System.Text.Encoding.UTF8);
                    writer.BaseStream.Seek(0, SeekOrigin.End);
                    writer.WriteLine(DateTime.Now + " 对 《" + this.DocTypeName + "》 进行了 《" + this.Action + "》 操作,对应的操作数据是:" + this.Data + "\r\n");
                    writer.Flush();
                    writer.Close();
                    filestream.Close();
                }
                else {
                    var writer = File.AppendText(filePathAll);
                   writer.WriteLine(DateTime.Now + " 对 《" + this.DocTypeName + "》 进行了 《" + this.Action + "》 操作,对应的操作数据是:" + this.Data + "\r\n");
                   writer.Flush();
                   writer.Close();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
    public class OperatorLog 
    {
        public static void WriteLog(Guid operatorId, Guid docId, string status, string docTypename, string data, string remark, bool isSuccess = true)
        {
            var log = CreateLog(operatorId, docId, status, docTypename, data, remark, isSuccess);
            log.WriteLog();
        }
        public static void WriteLog(Guid operatorId, Guid docId, string status, string docTypename, string data, bool isSuccess = true)
        {
            var log = CreateLog(operatorId, docId, status, docTypename, data, "", isSuccess);
            log.WriteLog();
        }
        public static void WriteLog(Guid operatorId, Guid docId, string docTypename,string remark, bool isSuccess = true)
        {
            var log = CreateLog(operatorId, docId, "", docTypename, "", remark, isSuccess);
            log.WriteLog();
        }
        public static void WriteLog(Guid operatorId, string docTypename, string data, bool isSuccess = true)
        {
            var log = CreateLog(operatorId, new Guid(), "", docTypename, data, "", isSuccess);
            log.WriteLog();
        }
        public static void WriteLog(Guid operatorId, string docTypename, string remark,string data, bool isSuccess = true)
        {
            var log = CreateLog(operatorId, new Guid(),"", docTypename, data,remark, isSuccess);
            log.WriteLog();
        }
        private static Log CreateLog(Guid operatorId, Guid docId, string status, string docTypename, string data,string remark, bool isSuccess = true)
        {
            Log log;
            var IsDbLog = System.Configuration.ConfigurationManager.AppSettings["Log"] == "true";
            if (IsDbLog)
            {
                log = new DBLog();
            }
            else
            {
                log = new FileLog();
            }
            log.Init(operatorId, docId, status, docTypename, data, remark, isSuccess);
            return log;
        }
    }
}
