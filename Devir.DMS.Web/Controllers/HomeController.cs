using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.DocumentNotifications;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Devir.DMS.Web.Hubs;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.Web.Helpers;

namespace Devir.DMS.Web.Controllers
{
    public class HomeController : Base.BaseController
    {
        public ActionResult Index()
        {
            var s = RepositoryFactory.GetRepository<DocumentType>().List(m => m.isDeleted == false).ToList();
            var user = User.Identity.Name;
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";          
            return View();            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult TestImageScaner()
        {
            return View();
        }

        [OutputCache(Duration = 300, VaryByParam = "UserId")]
        public ActionResult RenderMainMenu(Guid UserId)
        {
            Models.MainMenu.MainMenuViewModel model = new Models.MainMenu.MainMenuViewModel();
                       

            model.newNotificationsCount =  RepositoryFactory.GetRepository<Notifications>().GetListCount(m => m.ForWho.UserId == RepositoryFactory.GetCurrentUser() && m.ViewDateTime == null);

             model.workingdocumentsCount = RepositoryFactory.GetDocumentRepository().getworkingDocumentsForUser();

             model.workingMyTasksCount = RepositoryFactory.GetInstructionRepository().GetInstructionsCount("all");

             model.workingTaskCount = RepositoryFactory.GetDocumentRepository().getAllTasksCount() + RepositoryFactory.GetDocumentRepository().getAllTasksForConfirmingPerformCount() +
                 RepositoryFactory.GetInstructionRepository().getAllTasksCount() + RepositoryFactory.GetInstructionRepository().getAllTasksForConfirmingPerformCount();

            model.newDocumentsCount = RepositoryFactory.GetDocumentRepository().getnewDocumentsForUser();

            model.newTasksCount = RepositoryFactory.GetDocumentRepository().getAllNewTasksCount() + RepositoryFactory.GetDocumentRepository().getAllNewTasksForConfirmingPerformCount() +
                RepositoryFactory.GetInstructionRepository().getAllNewTasksCount() + RepositoryFactory.GetInstructionRepository().getAllNewTasksForConfirmingPerformCount();

            model.badMyTasksCount = RepositoryFactory.GetInstructionRepository().GetInstructionsCount("outOfDate"); 

            model.badTasksCount = RepositoryFactory.GetDocumentRepository().getAllBadTasksCount() + RepositoryFactory.GetDocumentRepository().getAllBadTasksForConfirmingPerformCount() +
                RepositoryFactory.GetInstructionRepository().getAllBadTasksCount() + RepositoryFactory.GetInstructionRepository().getAllBadTasksForConfirmingPerformCount();


            return View(model);
        }


        public ActionResult GetNotifications()
        {
            var res = RepositoryFactory.GetRepository<Notifications>().List(x =>
                                           x.ForWho.UserId == RepositoryFactory.GetCurrentUser() &&
                                           x.ViewDateTime == null).ToList();
            return View(res);
        }
       

        public ActionResult SetViewDateTime()
        {
            RepositoryFactory.GetNotificationRepository().SetViewDateTimeForNotification(RepositoryFactory.GetCurrentUser());

            Helpers.SignalRWebNotifierHelper.SendToRefreshMainMenu("refreshMain", RepositoryFactory.GetRepository<User>().Single(m=>m.UserId==RepositoryFactory.GetCurrentUser()).Name.ToLower());

            return Json("1", JsonRequestBehavior.AllowGet);
        }

        public ActionResult Login()
        {
            return View(RepositoryFactory.GetRepository<User>().Single(u => !u.isDeleted && u.UserId == RepositoryFactory.GetCurrentUser()));
        }
    }
}
