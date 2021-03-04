using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class RouteTypeViewModel
    {
        [Required]
        public Guid RouteTypeId { get; set; }
    }
}