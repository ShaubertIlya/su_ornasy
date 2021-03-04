using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.DL.Models.Settings
{
    public class Settings:ModelBase
    {
        public Guid NegotiationStage { get; set; }
        public Guid SignStage { get; set; }
        public Guid PerformStage { get; set; }
        public Guid ControlPerformStage {get;set;}

        public Guid OkNegotiationAction { get; set;  }
        public Guid NotOkNegotiationAction { get; set; }
        public Guid AdditionalNegotiationRequired { get; set; }

        public Guid OkSignAction { get; set; }
        public Guid NotOkSigntAction { get; set; }

        public Guid OkPerformAction { get; set; }
        public Guid NotOkPerformAction { get; set; }
        public Guid AdditionalPerformRequired { get; set; }

        public Guid OkControlPerformStage { get; set; }
        public Guid NotOkControlPerformStage { get; set; }

        public Guid PerformInstructionWaiting { get; set; }


        public Guid AddViewersStage { get; set; }
        

        /* Field Types */

        public Guid UserFieldType { get; set; }
        public Guid ListUsersFieldType { get; set; }


    }
}
