using Devir.DMS.DL.Models.References;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Devir.DMS.DL.Models.Document.Route
{
    [Bind(Exclude = "Action")]
    public class UserSignResult
    {   
        public User User { get; set; }  
        [Display(Name="Комментарий к действию")]
        [AllowHtml]
        public string Comment { get; set; }
        public List<Guid> attachment { get; set; }
        public RouteAction Action { get; set; }
        public Guid ActionId { get; set; }
        public DateTime Date { get; set; }
      
    }
}
