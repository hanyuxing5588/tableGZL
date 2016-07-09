using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure;
namespace Business.IBusiness
{
    public class AuthSetModel
    {
        public Guid? PGUID { get; set; }
        public Guid Guid { get; set; }
        public string ModelName { get; set; }
        public bool? IsBrowser { get; set; }
        public bool? IsModify { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsTimeLimited { get; set;}
        public string StartTime { get; set; }
        public string StopTime { get; set; }
        public DateTime? StartTTime { get; set; }
        public DateTime? StopTTime { get; set; }
        public bool? IsDefault { get; set; }
        public string _parentId { get; set; }
        public string ClassId { get; set; }
        public Guid? RGUID { get; set; }
        public string Key { get; set; }
        public bool @checked { get; set; }
        /// <summary>
        /// 是否可用狀態
        /// </summary>
        public bool? IsAble { set; get; }
    }

  public  interface IAuthSet
    {
        /// <summary>
        /// 单位权限加载
        /// </summary>
        /// <param name="operatorId"></param>
        /// <returns></returns>
      IEnumerable<AuthSetModel> GetDWAuth(Guid operatorId,bool isRole);
      IEnumerable<AuthSetModel> GetBMAuth(Guid operatorId, bool isRole);
      IEnumerable<AuthSetModel> GetProjectAuth(Guid operatorId, bool isRole);
      IEnumerable<AuthSetModel> GetBGCodeAuth(Guid operatorId, bool isRole);
      IEnumerable<AuthSetModel> GetGZAuth(Guid operatorId, bool isRole);
      IEnumerable<AuthSetModel> GetActionAuth(Guid operatorId);
      IEnumerable<AuthSetModel> GetMenuAuth(Guid operatorId, bool isRole);
      IEnumerable<AuthSetModel> GetPersonAuth(Guid operatorId, bool isRole);
      bool SaveAuth(string userOrRoleIds,string classId,List<AuthSetModel> listAuthsetModel);
    }
}
