using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References
{
    public class FieldType: ModelBase
    {      
        [DisplayName("Наименование")]
        [Required(ErrorMessage="Введи наименование")]
        public String Name { get; set; }
        [DisplayName("Комментарий")]
        public string Comment { get; set; }
        [DisplayName("Тип данных")]
        [Required(ErrorMessage = "Введи тип данных")]
        public string SystemName { get; set; }

        public Guid DynamicReferenceId { get; set; } 
    }
}