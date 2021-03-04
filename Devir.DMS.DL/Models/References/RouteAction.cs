using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.References
{
    public class RouteAction:ModelBase
    {
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Введи наименование")]
        public string Name { get; set; }
        [DisplayName("Описание")]
        [Required(ErrorMessage = "Введи описание")]
        public string Comment { get; set; }        
    }
}
