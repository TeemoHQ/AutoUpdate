using AutoUpdateServer.Model;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer
{
    public class UserIdentity : IUserIdentity
    {
        public UserIdentity(string userName, string roleName) :
            this(userName, roleName, new List<string>())
        {
        }
        public UserIdentity(string userName, string roleName, IEnumerable<string> claims)
        {
            this.UserName = userName;
            this.RoleName = roleName;
            this.Claims = claims;
        }
        public IEnumerable<string> Claims
        {
            get; private set;
        }
        public string UserName
        {
            get; private set;
        }
        public string RoleName
        {
            get; private set;
        }

    }
}