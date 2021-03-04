using Devir.DMS.DL.Models.DocumentTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Filter
{
    public class DocumentFilterVM
    {
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public bool IsSearchByDepartment { get; set; }

        public string StartDate { get; set; }
        public bool IsSearchByStartDate { get; set; }

        public string EndDate { get; set; }
        public bool IsSearchByEndDate { get; set; }

        public string Header { get; set; }
        public bool IsSearchByHeader { get; set; }
        public string MethodOfSearchForHeader { get; set; }

        //public bool IsExactMatchByHeader { get; set; } // точное совпадение
        //public bool IsInclusionByHeader { get; set; } // на вхождение


        public Guid DocTypeId { get; set; }
        public List<DynamicFields> DynamicFields { get; set; }


        public DocumentFilterVM()
        {
            DynamicFields = new List<DynamicFields>();
        }
    }


    public class DynamicFields
    {
        public FieldTemplate FieldTemplate { get; set; }
        public Object Value { get; set; }
        public string TextForReference { get; set; }
        public bool IsSearchEnabled { get; set; }
        public string MethodOfSearchForDynamicValue { get; set; }

        //public bool IsExactMatch { get; set; } // точное совпадение
        //public bool IsInclusion { get; set; } // на вхождение

        public DynamicFields()
        {
            FieldTemplate = new FieldTemplate();
        }
    }
}
