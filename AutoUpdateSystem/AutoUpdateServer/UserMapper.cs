using AutoUpdateServer.Model;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;
using System;

namespace AutoUpdateServer
{
    public class UserMapper : IUserMapper
    {
        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            UserModel user = context.Request.Session[identifier.ToString()] as UserModel;
            return user == null ? null : new UserIdentity(user.Name, user.RoleName);
        }
    }
}