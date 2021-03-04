using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class FieldTemplateViewModel
    {
        public Guid DocumentTypeId { get; set; }
        public List<FieldTemplate> FieldTemplates { get; set; }
        public List<FieldType> FieldTypes { get; set; }

        public static void SaveToDocumentType(FieldTemplateViewModel data)
        {
            var docTypeRepo = RepositoryFactory.GetRepository<DocumentType>();
            var tmpDocType = docTypeRepo.Single(m => m.Id == data.DocumentTypeId);
            tmpDocType.FieldTemplates = new List<FieldTemplate>();

            data.FieldTemplates.ForEach(m =>
            {
                if (m.Id == Guid.Empty)
                {
                    m.Id = Guid.NewGuid();
                }

                    m.FieldType = RepositoryFactory.GetRepository<FieldType>().Single(n => n.Id == m.FieldTypeId);
                    tmpDocType.FieldTemplates.Add(m);
               
            });

            docTypeRepo.update(
                       tmpDocType
                       );
        }
    }
}