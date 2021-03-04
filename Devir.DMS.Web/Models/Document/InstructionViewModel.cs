using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Models.Document
{
    public class InstructionViewModel
    {
        [Required]
        [Display(Name = "Резолюция")]
        public string Header { get; set; }
        [AllowHtml]
        [Display(Name = "Содержимое")]
        public string Body { get; set; }
        [Display(Name = "Исполнитель")]
        [Required]
        public Guid UserForWho { get; set; }
        [Required]
        [Display(Name = "Дата исполнения")]
        public DateTime DateBefore { get; set; }
        public List<Guid> attachment { get; set; }

        public DateTime maxDate { get; set; }
       
        public Guid RootDocumentId { get;set; }
        public Guid DocumentId { get; set; }
        public Guid RouteStageId { get; set; }
        public Guid RouteStageUserId { get; set; }
    }
}