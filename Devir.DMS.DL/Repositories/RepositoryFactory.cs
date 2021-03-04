using Devir.DMS.DL.Models;
using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.DocumentNotifications;
using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Repositories
{
    public static class RepositoryFactory
    { 

        public delegate Guid GetUserDelegate();

        public static GetUserDelegate GetCurrentUser { get; set; }

        private static Guid CurrentUser()
        {
            if (GetCurrentUser == null)
                throw new Exception("Не задан алгоритм получения текущего пользователя. Для задания используйте делегат GetCurrentUser.");
            return GetCurrentUser.Invoke();
        }       

        private static void connectToDB()
        {               
            if (!MongoHelpers.MongoHelper.IsConnected)
                MongoHelpers.MongoHelper.connect(Settings.Default.Server, Settings.Default.DBName);
        }

        public static RepositoryBase<T> GetRepository<T>() where T : ModelBase
        {            
            connectToDB();
            return new RepositoryBase<T>(MongoHelpers.MongoHelper.Database, typeof(T).Name, CurrentUser());
        }

        public static RepositoryBaseNoAudit<T> GetNoAuditRepository<T>() where T : ModelBase
        {
            connectToDB();
            return new RepositoryBaseNoAudit<T>(MongoHelpers.MongoHelper.Database, typeof(T).Name, CurrentUser());
        }

        public static RepositoryBase<T> GetAnonymousRepository<T>() where T : ModelBase
        {
            connectToDB();
            return new RepositoryBase<T>(MongoHelpers.MongoHelper.Database, typeof(T).Name, Guid.Empty);
        }


        public static RepositoryBase<User> GetAuthenticationRepository()
        {
            return new RepositoryBase<User>(MongoHelpers.MongoHelper.Database, "User", Guid.Empty);
        }

        public static DocumentRepository GetDocumentRepository()
        {
            connectToDB();
            return new DocumentRepository(MongoHelpers.MongoHelper.Database, typeof(Document).Name, CurrentUser());
        }

        public static DynamicRecordRepository GetDynamicRecordRepository()
        {
            connectToDB();
            return new DynamicRecordRepository(MongoHelpers.MongoHelper.Database, typeof(DynamicRecord).Name, Guid.Empty);
        }

        public static NotificationRepository GetNotificationRepository()
        {
            connectToDB();
            return new NotificationRepository(MongoHelpers.MongoHelper.Database, typeof(Notifications).Name, CurrentUser());
        }

        public static InstructionRepository GetInstructionRepository()
        {
            connectToDB();
            return new InstructionRepository(MongoHelpers.MongoHelper.Database, typeof(Instruction).Name, CurrentUser());
        }


        public static DocumentTypeCountRepository GetDocumentTypeCountRepository()
        {
            connectToDB();
            return new DocumentTypeCountRepository(MongoHelpers.MongoHelper.Database, typeof(DocumentTypeCount).Name, CurrentUser());
        }
    }
}
