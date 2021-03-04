using Devir.DMS.DL.Models.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Devir.DMS.DL.Models.References.OrganizationStructure;

namespace Devir.DMS.DL.Repositories
{
    public class DocumentTypeCountRepository : RepositoryBase<DocumentTypeCount>
    {

        public DocumentTypeCountRepository(MongoDatabase database, string collectionName, Guid userId)
            : base(database, collectionName, userId)
        {
        }

        public int GetDocumentNextNumber(Guid documentTypeId)
        {
            var countObject = this.GetCollection().AsQueryable().Where(m => m.DocumentTypeId == documentTypeId && m.Year == DateTime.Now.Year).FirstOrDefault();

            if (countObject != null)
            {
                countObject.Count++;
                this.update(countObject);
                return countObject.Count;
            }
            else
            {
                var newCountObject = new DocumentTypeCount() { DocumentTypeId = documentTypeId, Count = 1, Year = DateTime.Now.Year };
                this.Insert(newCountObject);
                return 1;
            }
            

        }


        public string GetDocumentNextNumberByUser(Guid documentTypeId, Guid UserId)
        {
            var countObject = this.GetCollection().AsQueryable().Where(m => m.DocumentTypeId == documentTypeId && m.Year == DateTime.Now.Year).FirstOrDefault();

            if (countObject != null)
            {
                countObject.Count++;
                this.update(countObject);
                return String.Format("{0}/{1}", RepositoryFactory.GetRepository<User>().Single(m => m.UserId == UserId).Nomenclature, countObject.Count.ToString());
            }
            else
            {
                var newCountObject = new DocumentTypeCount() { DocumentTypeId = documentTypeId, Count = 1, Year = DateTime.Now.Year};
                this.Insert(newCountObject);
                return String.Format("{0}/{1}", RepositoryFactory.GetRepository<User>().Single(m => m.UserId == UserId).Nomenclature,1);
            }


        }
    }
}
