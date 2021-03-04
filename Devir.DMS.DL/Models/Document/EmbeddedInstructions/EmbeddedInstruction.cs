using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Document.EmbeddedInstructions
{
    public class EmbeddedInstruction
    {
        public int Order { get; set; }
        public Guid UserFor { get; set; }
        public DateTime DateBefore { get; set; }
        public String Resolutions { get; set; }
        public string Body { get; set; }
        public Guid UserController { get; set; }
    }
}
