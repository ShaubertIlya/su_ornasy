using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Security;
using System.Threading.Tasks;

namespace Devir.DMS.Web.Hubs
{
    public class NotificationHub : Hub
    {

        private string _userId
        {
            get { return this.Context.Request.User.Identity.Name; }
        }

        private Guid _connectionId
        {
            get { return Guid.Parse(Context.ConnectionId); }
        }

        public override Task OnConnected()
        {
            MvcApplication.SignalRUsrListNotifier.AddUser(_userId, _connectionId);

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        public override Task OnDisconnected()
        {
            MvcApplication.SignalRUsrListNotifier.DeleteUser(_connectionId);

            return base.OnDisconnected();
        }

        public void Send(string title, string message, string link)
        {

            // Call the addNewMessageToPage method to update clients.
            Clients.All.receiveChat(title, message, link);
        }

        public void SendToUser(string title, string message, string link, string userName)
        {
            MvcApplication.SignalRUsrListNotifier.GetUserByName(userName).ForEach(m =>
            {
                Clients.Client(m.SessionId.ToString()).receiveChat(title, message, link);
            });
        }

        public void SendToUser(string title, string message, string link, Guid userId)
        {
            MvcApplication.SignalRUsrListNotifier.GetUserById(userId).ForEach(m =>
            {
                Clients.Client(m.SessionId.ToString()).receiveChat(title, message, link);
            });
        }


    }
}