using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.References.DynamicReferences;
using Devir.DMS.DL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.BL.DynamicRecords
{
    public class DataTypeHelper
    {
        public static bool CheckFieldForDataType(Guid DynamicReferenceId, Guid DynamicFieldTemplateId, string Value)
        {
            var tmpDynamicFieldTemplate = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == DynamicReferenceId).FieldTemplates.SingleOrDefault(m => m.Id == DynamicFieldTemplateId);

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "e3224442-d53a-47e9-b1bb-495c034b10d8")
                return true;

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "a427dbfb-9cb7-4f52-9d5e-c7d0677e8103" || tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "f23165db-7c3d-49d5-bbc0-127eef90de36")
                return true;

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "2490becb-3476-43ab-8717-0f0b138a6ab2")
            {
                bool tmpRes = false;
                return Boolean.TryParse(Value, out tmpRes);
            }

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "8a37142c-0e29-4b40-b4a3-0a3a7d4f21d9")
            {
                int tmpRes = 0;
                return Int32.TryParse(Value, out tmpRes);
            }

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "944388a1-b1e3-4a4d-910d-7ad9df107e20")
            {
                decimal tmpRes = 0;
                return decimal.TryParse(Value, out tmpRes);
            }

            if (tmpDynamicFieldTemplate.TypeOfTheField.Id.ToString() == "d88f464a-ca95-4c41-ad7d-7df5adfd90d8")
            {
                DateTime tmpRes;
                return DateTime.TryParse(Value, out tmpRes);
            }



            Guid tmpGuid;
            return Guid.TryParse(Value, out tmpGuid);

        }

        public static void AddDynamicFieldValue(DocumentFieldValues DynamicValue, Guid TypeOfTheField)
        {
            var Value = DynamicValue.StringValue;

            DynamicValue.StringValue = String.Empty;
            DynamicValue.ValueToDisplay = Value;
            DynamicValue.FieldTypeId = TypeOfTheField;            

            if (TypeOfTheField.ToString() == "398877ee-49f3-46b6-bc2e-f567ecd75410")
            {
                if (String.IsNullOrEmpty(Value))
                {
                    DynamicValue.ValueToDisplay = "Не связан";
                    return;
                }
                var doc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == new Guid(Value));
                if (doc != null)
                    DynamicValue.ValueToDisplay = "№ " + doc.DocumentNumber + "  " + doc.Header;
                else
                    DynamicValue.ValueToDisplay = "Не связан";
                return;
            }


            if (TypeOfTheField.ToString() == "a427dbfb-9cb7-4f52-9d5e-c7d0677e8103" || TypeOfTheField.ToString() == "f23165db-7c3d-49d5-bbc0-127eef90de36")
            {
                DynamicValue.StringValue = Value;
                return;
            }

            if (TypeOfTheField.ToString() == "2490becb-3476-43ab-8717-0f0b138a6ab2")
            {
                bool tmpRes = false;
                Boolean.TryParse(Value, out tmpRes);
                DynamicValue.BooleanValue = tmpRes;
                return;
            }

            if (TypeOfTheField.ToString() == "8a37142c-0e29-4b40-b4a3-0a3a7d4f21d9")
            {
                int tmpRes = 0;
                Int32.TryParse(Value, out tmpRes);
                DynamicValue.IntValue = tmpRes;
                return;
            }

            if (TypeOfTheField.ToString() == "944388a1-b1e3-4a4d-910d-7ad9df107e20")
            {
                DynamicValue.DecimalValue = Value;
                return;
            }

            if (TypeOfTheField.ToString() == "d88f464a-ca95-4c41-ad7d-7df5adfd90d8")
            {
                DateTime tmpRes;
                DateTime.TryParse(Value, out tmpRes);
                DynamicValue.DateTimeValue = tmpRes;
                return;
            }

            if (TypeOfTheField.ToString() == "2c308153-04a9-4bf6-b021-cc28b82a7ab5")
            {
                return;
            }

            if (TypeOfTheField.ToString() == "9b7be5b0-3e69-466a-a876-eae0402ebbe7")
            {
                return;
            }

            if (TypeOfTheField.ToString() == "e3224442-d53a-47e9-b1bb-495c034b10d8")
            { return; }


            Guid tmpGuid;
            Guid.TryParse(Value, out tmpGuid);
            DynamicValue.DynamicRecordId = tmpGuid;


            //Если ниодно из вышеперечисленных, то динамический справочник естьжи:) 
            var DisplayFieldsArray = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == DynamicValue.DynamicReferenceId).FieldTemplates.Where(m => m.isDisplay).Select(m => m.Id).ToList();

            var sss = DL.Repositories.RepositoryFactory.GetRepository<DynamicRecord>().List(m => m.RecordId == DynamicValue.DynamicRecordId);
            StringBuilder sb = new StringBuilder();
            sss.ToList().ForEach(m =>
            {
                if (DisplayFieldsArray.Contains(m.DynamicReferenceFieldTemplateId))
                    sb.Append(String.Format("{0} ", m.Value.Value.ValueToDisplay));
            });
            DynamicValue.ValueToDisplay = sb.ToString();
        }



        public static void AddDynamicFieldValue(DocumentFieldValues DynamicValue, Guid DynamicReferenceId, Guid DynamicFieldTemplateId)
        {
            var tmpDynamicFieldTemplate = RepositoryFactory.GetRepository<DynamicReference>().Single(m => m.Id == DynamicReferenceId).FieldTemplates.SingleOrDefault(m => m.Id == DynamicFieldTemplateId);
            AddDynamicFieldValue(DynamicValue, tmpDynamicFieldTemplate.TypeOfTheField.Id);
            //sss.Where(DisplayFieldsArray).FirstOrDefault().Value.Value.ValueToDisplay;
        }

    }


}
