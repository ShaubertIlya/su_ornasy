using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers.Base
{
    [Authorize]
    public class BaseController : Controller
    {
        //
        // GET: /Base/

        object thislock = new object();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            if (DL.Repositories.RepositoryFactory.GetCurrentUser == null)
                DL.Repositories.RepositoryFactory.GetCurrentUser = () =>
                {

                    lock (thislock)
                    {
                        if (MvcApplication.UserList.ContainsKey(MvcApplication.GetUserName))
                            return (Guid)MvcApplication.UserList[MvcApplication.GetUserName];
                        else
                        {
                            var user = DL.Repositories.RepositoryFactory.GetAuthenticationRepository().List(m => m.Name.ToLower() == MvcApplication.GetUserName.ToLower() && m.isDeleted == false).FirstOrDefault();

                            if (user != null)
                            {
                                MvcApplication.UserList.Add(user.Name, user.UserId);
                                return user.UserId;
                            }
                            else
                            {
                                
                            }
                            

                            return /*user.Id*/ Guid.Empty;
                        }
                    }
                };

            if (CurrentUser == null)
                filterContext.Result = new RedirectResult(Url.Action("UserNotFoundError", "Error"));
            
            base.OnActionExecuting(filterContext);
        }

        public User CurrentUser
        {
            get
            {
                return DL.Repositories.RepositoryFactory.GetRepository<User>().Single(u => u.Name.ToLower() == MvcApplication.GetUserName.ToLower());
            }
        }

    }
}
