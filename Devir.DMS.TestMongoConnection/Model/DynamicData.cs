using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.TestMongoConnection.Model
{
    class DynamicData : DL.Models.ModelBase
    {
        public Guid ReferenceId { get; set; }
        Dictionary<Guid, DL.Models.Document.DocumentFieldValues> FieldValues { get; set; }     
    }
}
