using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Devir.DMS.DL.Models.References;
using System.ComponentModel.DataAnnotations;
using Devir.DMS.Web.Helpers.Validation;

namespace Devir.DMS.Web.Models.Reference
{
    public class RouteTypeActionsViewModel
    {
        public Guid RouteTypeId { get; set; }

        [EnsureOneElementAttribute(ErrorMessage = "Ыыыыы")]
        public List<RouteActionViewModel> RouteActions { get; set; }   
        
        public List<RouteAction> RouteActionsList { get; set; }
    }
}