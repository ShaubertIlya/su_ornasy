using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Devir.DMS.DL.Models.References;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.DocumentTemplates
{
    public class RouteType:ModelBase
    {
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Введи наименование")]
        public string Name { get; set; }
        [DisplayName("Комментарий")]
        public string Comment { get; set; }
        public List<RouteAction> Actions { get; set; }

        public RouteType()
        {
            Actions = new List<RouteAction>();
        }
    }
}
