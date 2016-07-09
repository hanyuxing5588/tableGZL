using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Reflection;
namespace Platform.Flow.Run
{
    public  static class TaskManager
    {
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public static ITask CreateTask(string taskId, List<Type> listTask)
        {
            //先判断内置任务有没有在执行 外置任务
            ITask itask = CreateInternalTask(taskId);
            if (itask != null) return itask;
            //执行外置任务
            var listTaskAll = listTask;
            foreach (var item in listTaskAll)
            {
                var task = Activator.CreateInstance(item) as ITask;
                if (task.GetTaskId() == taskId)
                {
                    return task;
                }
            }
            return null;
        }
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        public static ITask CreateTask(string taskId)
        {
            //先判断内置任务有没有在执行 外置任务
            ITask itask = CreateInternalTask(taskId);
            if (itask != null) return itask;
            //执行外置任务
            var  listTaskAll = GetAllTask() as List<Type>;
            foreach (var item in listTaskAll)
            {
                ITask task = Activator.CreateInstance(item) as ITask;
                if (task.GetTaskId() == taskId)
                {
                    return task;
                }
            }
            return null;
        }
        public static ITask CreateInternalTask(string taskId)
        {
            return null;// FlowTaskImplement.TaskFactory.CreateTask(taskId);
            
        }
        /// <summary>
        /// 给所有任务
        /// </summary>
        /// <returns></returns>
        public static ICollection<Type> GetAllTask()
        {
            string CurrentTaskPath = string.Empty; ;
            string strTaskPath = System.Configuration.ConfigurationManager.AppSettings["TaskPath"];
            CurrentTaskPath = AppDomain.CurrentDomain.BaseDirectory + strTaskPath;
            return TaskManager.GetAllTask(CurrentTaskPath);
        }

        public static ICollection<Type> GetAllTask(string TaskPath)
        {
            try
            {
                var files = new DirectoryInfo(TaskPath);
                    FileInfo[] fif = files.GetFiles("*.dll");
                    List<Type> tasks = new List<Type>();
                    foreach (var item in fif)
                    {
                        string strPath = item.FullName;
                        List<Type> taskTemp = GetTask(strPath, typeof(ITask));
                        foreach (var tt in taskTemp)
                        {
                            tasks.Add(tt);
                        }
                    }
                    return tasks;
                    }
            catch (Exception ex)
            {
                return new Collection<Type>();
            }
        }
        /// <summary>
        /// 给所有的任务ID 和 名称
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetTaskNameAndID()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            var listTask = GetAllTask();
            foreach (var type1 in listTask)
            {
                ITask task = Activator.CreateInstance(type1) as ITask;
                string strName = task.GetTaskName();
                string strID = task.GetTaskId();
                dic.Add(strID, strName);
            }
            return dic;
        }
        /// <summary>
        /// 获取某一个dll实现 itask的所有任务 linq
        /// </summary>
        /// <param name="path"></param>
        private static List<Type> GetTask(string path, Type type)
        {
            Assembly ass = Assembly.LoadFile(path);
            return ass.GetTypes().Where(e => e.GetInterfaces().Contains(type)).ToList();
        }
        
    }

}
