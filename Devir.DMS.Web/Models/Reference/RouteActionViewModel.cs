using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Devir.DMS.Web.Models.Reference
{
    public class RouteActionViewModel
    {
        [Required]
        public Guid RouteActionId { get; set; }
    }
}