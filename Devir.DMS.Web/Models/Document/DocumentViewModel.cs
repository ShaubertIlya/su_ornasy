using Devir.DMS.DL.Models.Document;
using Devir.DMS.DL.Models.Document.NegotiatorsRoutes;
using Devir.DMS.DL.Models.References.OrganizationStructure;
using Devir.DMS.DL.Repositories;
using Devir.DMS.Web.Models.Reference;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Devir.DMS.Web.Models.Document
{
    public class DocumentViewModel
    {
        public Guid? ForRootInstructionId { get; set; }

        public Guid? ForRootDocumentId { get; set; }
        public Guid? ForUserForRouteId { get; set; }
        

        [Display(Name = "Заголовок")]
        [Required(ErrorMessage = "Необходимо заполнить поле заголовок")]
        public string Header { get; set; }

        public bool isNewVersion { get; set; }

        public DocumentType DocumenType { get; set; }

        public int AutoShiftDays { get; set; }

        public Guid DocumentTypeId { get; set; }
        [Display(Name = "Содержимое")]
        [AllowHtml]
        public string Body { get; set; }
        public List<Models.NegotiatorsKO.NegotiationStageEditorModel> NegotiatorsStage { get; set; }
        //Динамические поля
        public List<DynamicRecordFieldViewModel> Fields { get; set; }
        public List<Guid> attachment { get; set; }

        public List<InstructionsKO.InstructionKO> instructions { get; set; }
        
        public int Version { get; set; }

        public bool isUrgent { get; set; }

        public string DocumentNumber { get; set; }

        public List<Block> VisualFieldsTemplate { get; set; }

        public string RealDocNumber { get; set; }

        public Guid ParentDoc { get; set; }

        //Сюда остальные поля документа
        [Display(Name = "Дата исполнения")]
        [Required(ErrorMessage = "Необходимо заполнить дату исполнения документа")]
        public DateTime FinishDate { get; set; }

    }

    public class TaskViewModel
    {
        public Guid Id { get; set; }
        public string CalculatedId { get; set; }
        public string Header { get; set; }
        public string Name { get; set; }
        public string FinishDate { get; set; }
        public string Number { get; set; }
        public string Author { get; set; }
        public int Group { get; set; }
        public string Classes { get; set; }
        public string hrefAddress { get; set; }

        public string gDate { get; set; }
        public string sDate { get; set; }
        public bool isNew { get; set; }
        public bool isUrgent { get; set; }
    }
}