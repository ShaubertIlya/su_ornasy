using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Devir.DMS.DL.Models.DocumentTemplates;
using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Repositories;

namespace Devir.DMS.Web.Models.Reference
{
    public class DynamicFieldsTemplateViewModel
    {
        public Guid DynamicReferenceId { get; set; }
        public List<FieldType> FieldTypes { get; set; }
        public List<DynamicReferenceFieldTemplate> FieldTemplates { get; set; }


        public static void SaveToDynamicReference(DynamicFieldsTemplateViewModel data)
        {
            var docTypeRepo = RepositoryFactory.GetRepository<DynamicReference>();
            var tmpDocType = docTypeRepo.Single(m => m.Id == data.DynamicReferenceId);
            tmpDocType.FieldTemplates = new List<DynamicReferenceFieldTemplate>();

            data.FieldTemplates.ForEach(m =>
            {
                if (m.Id == Guid.Empty)
                {
                    m.Id = Guid.NewGuid();
                }

                m.TypeOfTheField = RepositoryFactory.GetRepository<FieldType>().Single(n => n.Id == m.TypeOfTheFieldId);
                tmpDocType.FieldTemplates.Add(m);

            });

            docTypeRepo.update(
                       tmpDocType
                       );
        }

    }
}