using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Models.Settings;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Devir.DMS.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {

        public static SignalRConnectedUserList SignalRUsrListNotifier { get; set; }
        public static SignalRConnectedUserList SignalRUsrListNotifierWeb { get; set; }
        public static Dictionary<String, Guid?> UserList { get; set; }

        public static string GetUserName { get { return HttpContext.Current.User.Identity.Name; } }

        public User CurrentUser { get; set; }

        protected void Application_Start()
        {
            ModelBinders.Binders.DefaultBinder = new PerpetuumSoft.Knockout.KnockoutModelBinder();

            MvcApplication.UserList = new Dictionary<string, Guid?>(StringComparer.OrdinalIgnoreCase);
            MvcApplication.SignalRUsrListNotifier = new SignalRConnectedUserList();
            MvcApplication.SignalRUsrListNotifierWeb = new SignalRConnectedUserList();
            //RouteTable.Routes.MapHubs();
            //RouteTable.Routes.MapHubs();
            AreaRegistration.RegisterAllAreas();



            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

            if (HttpContext.Current != null && HttpContext.Current.User != null && MvcApplication.UserList.ContainsKey(GetUserName))
            {
                var user = RepositoryFactory.GetAuthenticationRepository().Single(u => !u.isDeleted && u.Name.ToLower() == GetUserName.ToLower());

                if (user != null)
                {
                    if (CurrentUser == null)
                        CurrentUser = user;
                    string[] roles = RepositoryFactory.GetRepository<Role>().List(r => r.UsersInRoles.Contains(user.UserId)).Select(r2 => r2.Name).ToArray();
                    GenericPrincipal principal = new GenericPrincipal(HttpContext.Current.User.Identity, roles);
                    Thread.CurrentPrincipal = HttpContext.Current.User = principal;
                }
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {


        }

        protected void Session_Start()
        {
            BL.DocumentRouting.DocumentRouting.settings = RepositoryFactory.GetAnonymousRepository<Settings>().Single(m => !m.isDeleted);
        }



        //protected void PreRequestHandlerExecute(object sender, EventArgs e)
        //{

        //}
    }
}