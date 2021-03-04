using Devir.DMS.Web.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Devir.DMS.Web.Helpers
{
    public class SignalRWebNotifierHelper
    {
        public static void SendToRefreshMainMenu(string message, string userName)
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<WebNotificationHub>();
                MvcApplication.SignalRUsrListNotifierWeb.GetUserByName(userName).ForEach(m =>
                {
                    hubContext.Clients.Client(m.SessionId.ToString()).receiveChat(message);
                });
            }
            catch(Exception ex) { }
        }

        public static void SendNotifyToClient(string userName, Guid NotificationId)
        {

            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                MvcApplication.SignalRUsrListNotifier.GetUserByName(userName).ForEach(m =>
                {
                    hubContext.Clients.Client(m.SessionId.ToString()).receiveChat(NotificationId);
                });

                SendToRefreshMainMenu("refreshNotificationDiv:" + NotificationId.ToString(), userName);
            }
            catch (Exception ex) { }
        }

        public static void UpdateNotifyAtClient(string userName, Guid NotificationId)
        {
            try
            {
                var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                MvcApplication.SignalRUsrListNotifier.GetUserByName(userName).ForEach(m =>
                {
                    hubContext.Clients.Client(m.SessionId.ToString()).updateNotifyStatus(NotificationId);
                });

            }
            catch (Exception ex) { }
        }
         
    }
}