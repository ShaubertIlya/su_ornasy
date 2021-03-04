using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document
{
    public class DocumentVisualTemplate
    {
        public List<Block> Blocks { get; set; }
        public Guid DocumentTypeId { get; set; }
    }

    public class Block
    {
        public int Id { get; set; }
        public int Height { get; set; }
        public string Type { get; set; }
        public List<Control> Controls { get; set; }
    }

    public class Control
    {
        public int Width { get; set; }
        public int Top {get;set;}
        public int Left{get;set;}
        public string Type {get;set;}
        public Guid? FieldId{get;set;}
        public bool islabel {get;set;}
        public int LabelId{get;set;}
        public string LabelText { get; set; }
    }
}
