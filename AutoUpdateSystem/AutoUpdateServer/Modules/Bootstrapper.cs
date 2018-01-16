using Autofac;
using AutoUpdateServer.Common;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Autofac;
using Nancy.Conventions;
using Nancy.Security;
using Nancy.Session;
using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using AutoUpdateServer.DAL;

namespace AutoUpdateServer.Modules
{
    public class Bootstrapper : AutofacNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("assets"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Js"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Images"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("front"));
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("Css"));
        }

        protected override void ConfigureApplicationContainer(ILifetimeScope container)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<UserMapper>().As<IUserMapper>().InstancePerLifetimeScope();
            builder.Update(container.ComponentRegistry);
        }

        protected override void ApplicationStartup(ILifetimeScope container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            //数据库
           // MYSQLHelper.ConnectionString = ConfigurationManager.ConnectionStrings["conn"].ToString();
            DataAccessCenter.Load();
            //请求跟踪
            //StaticConfiguration.EnableRequestTracing = true;暂时不打开，性能消耗很大
            //显示详细错误信息
            StaticConfiguration.DisableErrorTraces = false;
            //启用AntiToken
            Csrf.Enable(pipelines);
        }

        protected override void RequestStartup(ILifetimeScope container, IPipelines pipelines, NancyContext context)
        {
            base.RequestStartup(container, pipelines, context);
            var formsAuthConfiguration = new FormsAuthenticationConfiguration()
            {
                RedirectUrl = "~/login",
                UserMapper = container.Resolve<IUserMapper>(),
            };
            FormsAuthentication.Enable(pipelines, formsAuthConfiguration);
            CookieBasedSessions.Enable(pipelines);
        }
    }
}