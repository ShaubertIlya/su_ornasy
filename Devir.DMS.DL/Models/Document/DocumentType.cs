using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Devir.DMS.DL.Models.DocumentTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Devir.DMS.DL.Models.References.OrganizationStructure;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentType : ModelBase
    {
        [DisplayName("Наименование")]
        [Required(ErrorMessage = "Введите наименование")]
        public string Name { get; set; }
        [DisplayName("Описание")]
        [Required(ErrorMessage = "Введите описание")]
        public string Comment { get; set; }

        public int StageOrderForNumbering { get; set; }
        public Guid StageiDForNumbering { get; set; }
        public Guid FiledTypeForNumbering { get; set; }

        public Guid StageAfterSendInstructions { get; set; }

        public bool NumberingDependsOnUser { get; set; }

        public bool SiplifiedInstructionField { get; set; }

        public Guid FieldForAdditionalMenuFiltering { get; set; }

        public Guid PrimaryFiltrationField { get; set; }

        private int? _autoShiftDate;

        //[Range(0, Int32.MaxValue, ErrorMessage = "Введите число")]
        [DisplayName("Автоматический cдвиг даты")]
        public int? AutoShiftDate
        {
            get
            {
                if (_autoShiftDate == null) return 0;
                return _autoShiftDate;
            }
            set { _autoShiftDate = value; }
        }

        public List<FieldTemplate> FieldTemplates { get; set; }
        public List<RouteTemplate> RouteTemplates { get; set; }
        public List<Role> Roles { get; set; }
        public List<Block> DocumentVisualTemplate { get; set; }


        public DocumentType()
        {
            FieldTemplates = new List<FieldTemplate>();
            RouteTemplates = new List<RouteTemplate>();
            Roles = new List<Role>();
        }

        
    }
}
