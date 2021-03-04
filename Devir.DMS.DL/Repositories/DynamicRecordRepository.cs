using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References.DynamicReferences;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Repositories
{
    public class DynamicRecordRepository: RepositoryBase<DynamicRecord>
    {
        public DynamicRecordRepository(MongoDatabase database, string collectionName, Guid userId)
            : base(database, collectionName, userId)
        {          
            
        }


        public void RecalculateAllReferenceValues(Guid RecordId, Guid DynamicReferenceId)
        {
            var DisplayFieldsArray = RepositoryFactory.GetAnonymousRepository<DynamicReference>().Single(m => m.Id == DynamicReferenceId).FieldTemplates.Where(m => m.isDisplay).Select(m => m.Id).ToList();



            var sss = DL.Repositories.RepositoryFactory.GetAnonymousRepository<DynamicRecord>().List(m => m.RecordId == RecordId);
            StringBuilder sb = new StringBuilder();
            sss.ToList().ForEach(m =>
            {
                if (DisplayFieldsArray.Contains(m.DynamicReferenceFieldTemplateId))
                    sb.Append(String.Format("{0} ", m.Value.Value.ValueToDisplay));
            });

            //var s = GetCollection().Find(Query<DynamicRecord>.EQ(ex => ex.Value.Value.DynamicRecordId, RecordId)).Count();
            

            GetCollection().Update(Query<DynamicRecord>.EQ(ex=>ex.Value.Value.DynamicRecordId, RecordId), new UpdateBuilder<DynamicRecord>().Set(ex=>ex.Value.Value.ValueToDisplay, sb.ToString()), UpdateFlags.Multi);
        }
    }
}
