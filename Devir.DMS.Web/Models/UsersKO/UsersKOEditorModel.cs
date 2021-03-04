using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.UsersKO
{
    public class UserKO
    {
        public string UserName { get; set; }
        public Guid UserGuid { get; set; }
        public int Order { get; set; }
        public string FieldPath { get; set; }
        public string FieldName { get; set; }
    }

    public class UsersKOEditorModel
    {
       public string FieldName { get; set; }
        public List<UserKO> UsersKO { get; set; }
        public string FieldPath { get; set; }

        public UsersKOEditorModel()
        {
            UsersKO = new List<UserKO>();
        }

        private void recalculateOrders()
        {
            var tmpUserOrders = this.UsersKO.OrderBy(m => m.Order).ToList();

            var i = 0;
            tmpUserOrders.ForEach(m =>
            {
                m.Order = i++;
            });
            this.UsersKO = tmpUserOrders;
        }       

        public void AddUserToList()
        {
            UsersKO.Add(new UserKO() { Order = UsersKO.Count() > 0 ? UsersKO.Max(m => m.Order) + 1 : 0 , FieldName = this.FieldName, FieldPath = this.FieldPath});
        }

        public void DeleteUser(int userIndex)
        {
            if (userIndex >= 0 && userIndex < UsersKO.Count)
                UsersKO.RemoveAt(userIndex);
            recalculateOrders();
        }
    }
}