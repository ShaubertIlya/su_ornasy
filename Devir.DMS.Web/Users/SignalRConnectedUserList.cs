using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Users
{
    public class SignalRConnectedUserList
    {
        List<SignalRUser> signalRUsers = null;       

        public SignalRConnectedUserList()
        {
            signalRUsers = new List<SignalRUser>();
        }

        public void AddUser(string userName, Guid sessionId)
        {
            var user = RepositoryFactory.GetAnonymousRepository<User>().Single(u => !u.isDeleted && u.Name.ToLower() ==  userName.ToLower());

            signalRUsers.RemoveAll(m => m.UserName == userName.ToLower() && m.SessionId == sessionId);
            signalRUsers.Add(new SignalRUser() { UserName = userName.ToLower(), UserId = user.UserId, SessionId = sessionId });
        }

        public void DeleteUser(Guid sessionId)
        {
            signalRUsers.RemoveAll(m=>m.SessionId == sessionId);                
        }

        public List<SignalRUser> GetUserByName(string userName)
        {
            return signalRUsers.Where(m => m.UserName.ToLower() == userName.ToLower()).ToList();
        }

        public List<SignalRUser> GetUserById(Guid userId)
        {
            return signalRUsers.Where(m => m.UserId == userId).ToList();
        }

        public List<SignalRUser> GetAllUsers()
        {
            return signalRUsers.ToList();
        }

        
    }
}