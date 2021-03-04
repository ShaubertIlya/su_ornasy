using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Controllers
{
    public class NotificationController : BaseController
    {
        //
        // GET: /Notification/

        public ActionResult GetNotifications(string userName, long lastNotifyDate)
        {
            var lastNotifyDateConverted = DateTime.FromBinary(lastNotifyDate);
            return Json(RepositoryFactory.GetNotificationRepository().List(m => m.ForWho.Name.ToLower().Contains(userName.ToLower()) && (m.ViewDateTime >= lastNotifyDateConverted || m.CreateDate >= lastNotifyDateConverted)).Select(
                m => new { Id = m.Id, CreateDate = m.CreateDate, ViewDateTime = m.ViewDateTime, Text = m.Text, LinkText = m.LinkText }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNotificationById(Guid Id)
        {
            return Json(RepositoryFactory.GetNotificationRepository().List(m => m.Id == Id).Select(
             m => new { Id = m.Id, CreateDate = m.CreateDate, ViewDateTime = m.ViewDateTime, Text = m.Text, LinkText = m.LinkText }).FirstOrDefault(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckCredentials()
        {
            return null;
        }

    }
}
