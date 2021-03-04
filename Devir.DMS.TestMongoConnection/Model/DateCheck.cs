using Devir.DMS.DL.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.TestMongoConnection.Model
{
    public class DateCheck:ModelBase
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime check { get; set; }
    }
}
