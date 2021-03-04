using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devir.DMS.DL.Models.Document.DocumentNotifications;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Devir.DMS.DL.Repositories
{
    public class NotificationRepository : RepositoryBase<Notifications>
    {
        public NotificationRepository(MongoDatabase database, string collectionName, Guid userId)
            : base(database, collectionName, userId)
        {
        }

        public void SetViewDateTimeForNotification(Guid userId)
        {
            GetCollection().Update(
                Query.And(Query<Notifications>.EQ(x => x.ViewDateTime, null),
                    Query<Notifications>.EQ(m => m.ForWho.UserId, userId)),
                Update.Set("ViewDateTime", DateTime.Now), UpdateFlags.Multi);
            
        }
    }
}
