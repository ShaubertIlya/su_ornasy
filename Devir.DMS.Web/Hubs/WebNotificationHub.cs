using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Web.Security;
using System.Threading.Tasks;

namespace Devir.DMS.Web.Hubs
{
    
    public class WebNotificationHub: Hub
    {       

        private string _userId
        {
            get
            {
                try { return this.Context.Request.User.Identity.Name; }
                catch (Exception ex)
                {
                    return String.Empty;
                }
                }
        }

        private Guid _connectionId
        {
            get { return Guid.Parse(Context.ConnectionId); }
        }

        public override Task OnConnected()
        {
            MvcApplication.SignalRUsrListNotifierWeb.AddUser(_userId, _connectionId);
            //var userConnectionRepository = new UserConnectionRepository();
            //userConnectionRepository.Create(_userId, _connectionId);
            //userConnectionRepository.Submit();

            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            MvcApplication.SignalRUsrListNotifierWeb.DeleteUser(_connectionId);
            //var userConnectionRepository = new UserConnectionRepository();
            //userConnectionRepository.Delete(_userId, _connectionId);
            //userConnectionRepository.Submit();

            return base.OnDisconnected();
        }

        public void Send(string message)
        {
            
            // Call the addNewMessageToPage method to update clients.
            Clients.All.receiveChat(message); 
        }

        public void SendToUser(string message, string userName)
        {
            MvcApplication.SignalRUsrListNotifierWeb.GetUserByName(userName).ForEach(m =>
            {
                Clients.Client(m.SessionId.ToString()).receiveChat(message);
            });
        }

        public void SendToUser(string message, Guid userId)
        {
            MvcApplication.SignalRUsrListNotifierWeb.GetUserById(userId).ForEach(m =>
            {
                Clients.Client(m.SessionId.ToString()).receiveChat(message);
            });
        }

        
    }
}