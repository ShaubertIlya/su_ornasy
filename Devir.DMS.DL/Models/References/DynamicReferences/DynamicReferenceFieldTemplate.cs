using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References.DynamicReferences
{
    public class DynamicReferenceFieldTemplate
    {
        public Guid Id { get; set; }
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Введи наименование")]
        public string Header { get; set; }
        public bool isDisplay { get; set; }
        public bool isRequired { get; set; }
        public int FieldOrder { get; set; }

        [BsonIgnore]
        [Required(ErrorMessage = "Выберите тип поля")]
        public Guid TypeOfTheFieldId { get; set; }
        public FieldType TypeOfTheField { get; set; }
    }
}
