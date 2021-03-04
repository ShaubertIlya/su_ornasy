using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class DynamicRecordViewModel
    {
        public Guid RecordId { get; set; }
        public Guid ReferenceId { get; set; }        
        public List<DynamicRecordFieldViewModel> Fields { get; set; }

        public DynamicRecordViewModel()
        {
            Fields=new List<DynamicRecordFieldViewModel>();
        }

        public void InsertToDynamicReference()
        {
            var recordId = Guid.NewGuid();
            
            List<DynamicRecord> Records = Fields.Select(m =>            
                new DynamicRecord()
                {
                    RecordId = recordId,
                    DynamicReferenceId = this.ReferenceId,
                    DynamicReferenceFieldTemplateId = m.DynamicFieldTemplateId,
                    Value = new DynamicValue()
                    {
                        Id = Guid.NewGuid(),
                        Value = new DL.Models.Document.DocumentFieldValues()
                        {          
                            StringValue = m.Value,
                            Id = Guid.NewGuid(),
                            FieldTypeId = m.TypeOfTheFieldId  ,
                            DynamicReferenceId = m.DynamicReferenceId ,
                            DynamicReferenceFieldTemplateId = m.DynamicFieldTemplateId
                        }                        
                    }
                }
            ).ToList();

            Records.ForEach(m => BL.DynamicRecords.DataTypeHelper.AddDynamicFieldValue(m.Value.Value, this.ReferenceId, m.DynamicReferenceFieldTemplateId));

            Records.ForEach(m => RepositoryFactory.GetRepository<DynamicRecord>().Insert(m));

        }



        public void UpdateInDynamicReference()
        {
            var recordId = this.RecordId;
            
            List<DynamicRecord> Records = Fields.Select(m =>            
                new DynamicRecord()
                {
                    Id = m.Id,
                    RecordId = recordId,
                    DynamicReferenceId = this.ReferenceId,
                    DynamicReferenceFieldTemplateId = m.DynamicFieldTemplateId,
                    Value = new DynamicValue()
                    {
                        Id = Guid.NewGuid(),
                        Value = new DL.Models.Document.DocumentFieldValues()
                        {          
                            StringValue = m.Value,
                            Id = Guid.NewGuid(),
                            FieldTypeId = m.TypeOfTheFieldId  ,
                            DynamicReferenceId = m.DynamicReferenceId ,
                            DynamicReferenceFieldTemplateId = m.DynamicFieldTemplateId
                        }                        
                    }
                }
            ).ToList();

            Records.ForEach(m => BL.DynamicRecords.DataTypeHelper.AddDynamicFieldValue(m.Value.Value, this.ReferenceId, m.DynamicReferenceFieldTemplateId));
            Records.ForEach(m => RepositoryFactory.GetRepository<DynamicRecord>().update(m));

            Task RecalculateAllReferenceValues = new Task(() =>
            {
                RepositoryFactory.GetDynamicRecordRepository().RecalculateAllReferenceValues(this.RecordId, this.ReferenceId);
            });

            RecalculateAllReferenceValues.Start();
        }
    }

}