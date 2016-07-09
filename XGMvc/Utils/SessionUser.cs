using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaothApp.Utils
{
    public class SessionUser
    {
        public Guid UserGuid { get; set; }
        public string UserKey { get; set; }
        public string UserName { get; set; }
        public string UserPwd { get; set; }
        public DateTime LogonDate { get; set; }
        public Guid RoleGuid { get; set; }

        public SessionUser() 
        {
            this.LogonDate = DateTime.Now;
        }

        public SessionUser(Guid UserGuid, string UserKey, string UserName, string UserPwd, Guid RoleGuid)
        {
            this.UserGuid = UserGuid;
            this.UserKey = UserKey;
            this.UserName = UserName;
            this.UserPwd = UserPwd;
            this.RoleGuid = RoleGuid;
        }
    }
}